using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Payments.Services;

public sealed class PaymentService(
    IPaymentRepository paymentRepository,
    IBookingRepository bookingRepository,
    IPaymentProvider paymentProvider,
    ISeatTicketFulfillmentService seatTicketFulfillmentService,
    IReceiptService receiptService,
    INotificationService notificationService,
    IPaymentTransactionRepository? transactionRepository = null,
    ISandboxPaymentCallbackVerifier? callbackVerifier = null,
    IAuditLogService? auditLogService = null,
    ISeatService? seatService = null,
    ITicketService? ticketService = null,
    IPayHereGatewayService? payHereGatewayService = null)
    : IPaymentService
{
    public async Task<IReadOnlyList<PaymentResponse>> GetMineAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
        => (await paymentRepository.GetByCustomerAsync(customerUserId, cancellationToken))
            .Select(Map)
            .ToList();

    public async Task<PaymentResponse> StartAsync(
        Guid customerUserId,
        StartPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.BookingId == Guid.Empty)
            throw new ArgumentException("Booking is required.");

        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking was not found.");

        if (booking.CustomerUserId != customerUserId)
            throw new ForbiddenAccessException("You do not own this booking.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be paid.");

        var existing = await paymentRepository.GetByBookingIdAsync(booking.Id, cancellationToken);

        if (existing is not null && existing.Status != PaymentStatus.Failed)
            return Map(existing);

        var checkoutReference = await paymentProvider.CreateCheckoutAsync(
            booking.Id,
            booking.TotalAmount,
            cancellationToken);

        var payment = new Payment
        {
            BookingId = booking.Id,
            CustomerUserId = customerUserId,
            Amount = booking.TotalAmount,
            Currency = "LKR",
            Provider = "Sandbox",
            CheckoutReference = checkoutReference,
            Status = PaymentStatus.Pending
        };

        await paymentRepository.AddAsync(payment, cancellationToken);
        await paymentRepository.SaveChangesAsync(cancellationToken);

        await RecordTransactionAsync(
            payment,
            "CheckoutCreated",
            checkoutReference,
            PaymentStatus.Pending,
            null,
            cancellationToken);

        return Map(payment);
    }

    public async Task<PaymentResponse> CompleteMockAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment was not found.");

        return await CompleteVerifiedAsync(
            payment,
            $"MOCK-COMPLETE-{payment.Id:N}",
            null,
            cancellationToken);
    }

    public async Task<PaymentResponse> FailMockAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment was not found.");

        if (payment.Status == PaymentStatus.Failed)
            return Map(payment);

        if (payment.Status is PaymentStatus.Successful or PaymentStatus.Refunded)
            throw new InvalidOperationException(
                "Successful/refunded payments cannot be marked failed.");

        payment.Status = PaymentStatus.Failed;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync(cancellationToken);

        await RecordTransactionAsync(
            payment,
            "PaymentFailed",
            payment.CheckoutReference,
            PaymentStatus.Failed,
            null,
            cancellationToken);

        await notificationService.CreateAsync(
            payment.CustomerUserId,
            "Payment failed",
            "Your payment could not be completed. Please try again.",
            "Payment",
            cancellationToken);

        return Map(payment);
    }

    public async Task<PaymentResponse> ProcessSandboxCallbackAsync(
        SandboxPaymentCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (callbackVerifier is null || !callbackVerifier.Verify(request))
            throw new UnauthorizedAccessException("Payment callback signature is invalid.");

        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment was not found.");

        if (payment.Amount != request.Amount ||
            !string.Equals(payment.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Payment callback amount or currency does not match the checkout.");
        }

        var status = request.Status.Trim().ToUpperInvariant();

        if (status is "SUCCESS" or "SUCCESSFUL" or "PAID")
        {
            return await CompleteVerifiedAsync(
                payment,
                request.ProviderReference,
                callbackVerifier.HashPayload(request),
                cancellationToken);
        }

        if (status is "FAILED" or "CANCELLED" or "CANCELED")
        {
            if (payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAtUtc = DateTime.UtcNow;
                await paymentRepository.SaveChangesAsync(cancellationToken);
            }

            await RecordTransactionAsync(
                payment,
                $"Callback{status}",
                request.ProviderReference,
                PaymentStatus.Failed,
                callbackVerifier.HashPayload(request),
                cancellationToken);

            return Map(payment);
        }

        throw new ArgumentException("Unsupported payment callback status.");
    }


    public async Task<PayHereCheckoutResponse> GetPayHereCheckoutAsync(
        Guid customerUserId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        if (payHereGatewayService is null)
            throw new InvalidOperationException("PayHere gateway is not configured.");

        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment was not found.");

        if (payment.CustomerUserId != customerUserId)
            throw new ForbiddenAccessException("You do not own this payment.");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can open a PayHere checkout.");

        payment.Provider = "PayHere";
        payment.UpdatedAtUtc = DateTime.UtcNow;
        await paymentRepository.SaveChangesAsync(cancellationToken);

        return payHereGatewayService.CreateCheckout(payment);
    }

    public async Task<PaymentResponse> ProcessPayHereNotificationAsync(
        PayHereNotifyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (payHereGatewayService is null ||
            !payHereGatewayService.VerifyNotification(request))
        {
            throw new UnauthorizedAccessException(
                "PayHere notification signature is invalid.");
        }

        var payment = await paymentRepository.GetByCheckoutReferenceAsync(
            request.OrderId,
            cancellationToken)
            ?? throw new KeyNotFoundException("Payment was not found.");

        if (!decimal.TryParse(
                request.PayHereAmount,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var amount))
        {
            throw new ArgumentException("PayHere amount is invalid.");
        }

        if (payment.Amount != amount ||
            !string.Equals(
                payment.Currency,
                request.PayHereCurrency,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "PayHere amount or currency does not match the checkout.");
        }

        if (request.StatusCode == "2")
        {
            return await CompleteVerifiedAsync(
                payment,
                string.IsNullOrWhiteSpace(request.PaymentId)
                    ? request.OrderId
                    : request.PaymentId,
                payHereGatewayService.HashNotificationPayload(request),
                cancellationToken);
        }

        if (payment.Status == PaymentStatus.Pending &&
            request.StatusCode is "-1" or "-2" or "-3")
        {
            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAtUtc = DateTime.UtcNow;
            await paymentRepository.SaveChangesAsync(cancellationToken);

            await RecordTransactionAsync(
                payment,
                $"PayHereStatus{request.StatusCode}",
                request.PaymentId,
                PaymentStatus.Failed,
                payHereGatewayService.HashNotificationPayload(request),
                cancellationToken);
        }

        return Map(payment);
    }

    public async Task<IReadOnlyList<PaymentTransactionResponse>> GetTransactionsAsync(
        Guid customerUserId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(paymentId, cancellationToken)
            ?? throw new KeyNotFoundException("Payment was not found.");

        if (payment.CustomerUserId != customerUserId)
            throw new ForbiddenAccessException("You do not own this payment.");

        return (await transactionRepository.GetByPaymentAsync(paymentId, cancellationToken))
            .Select(x => new PaymentTransactionResponse
            {
                PaymentTransactionId = x.Id,
                PaymentId = x.PaymentId,
                Type = x.Type,
                ProviderReference = x.ProviderReference,
                Status = x.Status,
                Amount = x.Amount,
                Currency = x.Currency,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToList();
    }

    private async Task<PaymentResponse> CompleteVerifiedAsync(
        Payment payment,
        string providerReference,
        string? payloadHash,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(payment.BookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking was not found.");

        if (payment.Status == PaymentStatus.Successful)
        {
            await EnsureArtifactsAsync(payment, booking, cancellationToken);
            return Map(payment);
        }

        if (payment.Status == PaymentStatus.Refunded)
            return Map(payment);

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be completed.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Booking is no longer pending.");

        var seatIds = await bookingRepository.GetSeatIdsAsync(booking.Id, cancellationToken);

        if (seatService is not null && ticketService is not null)
        {
            var committed = await seatService.CommitHoldAsync(
                booking.HoldToken,
                booking.CustomerUserId,
                booking.Id,
                cancellationToken);

            if (!committed)
            {
                var existingTickets = await ticketService.GetForBookingAsync(
                    booking.Id,
                    cancellationToken);

                if (existingTickets.Count == 0)
                    throw new InvalidOperationException(
                        "Seat hold could not be converted to a booking.");
            }
        }

        // Persist payment/booking state before generating tickets and receipts.
        payment.Status = PaymentStatus.Successful;
        payment.PaidAtUtc = DateTime.UtcNow;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAtUtc = DateTime.UtcNow;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync(cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        await RecordTransactionAsync(
            payment,
            "PaymentSuccessful",
            providerReference,
            PaymentStatus.Successful,
            payloadHash,
            cancellationToken);

        if (auditLogService is not null)
        {
            await auditLogService.WriteAsync(
                payment.CustomerUserId,
                "Payment confirmed",
                "Payment",
                payment.Id.ToString(),
                "Pending",
                "Successful",
                null,
                null,
                cancellationToken);
        }

        await EnsureArtifactsAsync(payment, booking, cancellationToken);

        await notificationService.CreateAsync(
            booking.CustomerUserId,
            "Booking confirmed",
            $"Booking {booking.BookingNumber} has been confirmed.",
            "Booking",
            cancellationToken);

        return Map(payment);
    }

    private async Task EnsureArtifactsAsync(
        Payment payment,
        SEVPMS.Domain.Entities.Bookings.Booking booking,
        CancellationToken cancellationToken)
    {
        var seatIds = await bookingRepository.GetSeatIdsAsync(booking.Id, cancellationToken);

        if (ticketService is not null)
        {
            await ticketService.IssueAsync(
                booking.Id,
                new IssueTicketsRequest(
                    booking.EventId,
                    seatIds.Select(x => (Guid?)x).ToArray()),
                cancellationToken);
        }
        else
        {
            await seatTicketFulfillmentService.CompletePaidBookingAsync(
                booking.Id,
                booking.EventId,
                booking.CustomerUserId,
                booking.HoldToken,
                seatIds,
                cancellationToken);
        }

        await receiptService.IssueAsync(
            payment.Id,
            booking.Id,
            booking.CustomerUserId,
            payment.Amount,
            payment.Currency,
            cancellationToken);
    }

    private async Task RecordTransactionAsync(
        Payment payment,
        string type,
        string providerReference,
        PaymentStatus status,
        string? payloadHash,
        CancellationToken cancellationToken)
    {
        if (transactionRepository is null)
        {
            throw new InvalidOperationException(
                "Payment transaction repository is not configured.");
        }
            return;

        await transactionRepository.AddAsync(
            new PaymentTransaction
            {
                PaymentId = payment.Id,
                BookingId = payment.BookingId,
                CustomerUserId = payment.CustomerUserId,
                Type = type,
                ProviderReference = providerReference,
                Status = status,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PayloadHash = payloadHash,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        await transactionRepository.SaveChangesAsync(cancellationToken);
    }

    private static PaymentResponse Map(Payment x) => new()
    {
        PaymentId = x.Id,
        BookingId = x.BookingId,
        CustomerUserId = x.CustomerUserId,
        Amount = x.Amount,
        Currency = x.Currency,
        Provider = x.Provider,
        CheckoutReference = x.CheckoutReference,
        QrPayload = $"sevpms://payment/{x.Id:N}?ref={Uri.EscapeDataString(x.CheckoutReference)}",
        Status = x.Status,
        PaidAtUtc = x.PaidAtUtc,
        CreatedAtUtc = x.CreatedAtUtc
    };
}
