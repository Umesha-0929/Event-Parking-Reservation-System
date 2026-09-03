using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Features.Seats.Interfaces;
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
    INotificationService notificationService)
    : IPaymentService
{
    public async Task<IReadOnlyList<PaymentResponse>> GetMineAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
        => (await paymentRepository.GetByCustomerAsync(
                customerUserId,
                cancellationToken))
            .Select(Map)
            .ToList();

    public async Task<PaymentResponse> StartAsync(
        Guid customerUserId,
        StartPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.BookingId == Guid.Empty)
            throw new ArgumentException("Booking is required.");

        var booking =
            await bookingRepository.GetByIdAsync(
                request.BookingId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Booking was not found.");

        if (booking.CustomerUserId != customerUserId)
        {
            throw new ForbiddenAccessException(
                "You do not own this booking.");
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending bookings can be paid.");
        }

        var existing =
            await paymentRepository.GetByBookingIdAsync(
                booking.Id,
                cancellationToken);

        if (existing is not null &&
            existing.Status != PaymentStatus.Failed)
        {
            return Map(existing);
        }

        var checkoutReference =
            await paymentProvider.CreateCheckoutAsync(
                booking.Id,
                booking.TotalAmount,
                cancellationToken);

        var payment = new Payment
        {
            BookingId = booking.Id,
            CustomerUserId = customerUserId,
            Amount = booking.TotalAmount,
            Currency = "LKR",
            Provider = "Mock",
            CheckoutReference = checkoutReference,
            Status = PaymentStatus.Pending
        };

        await paymentRepository.AddAsync(
            payment,
            cancellationToken);

        await paymentRepository.SaveChangesAsync(
            cancellationToken);

        return Map(payment);
    }

    public async Task<PaymentResponse> CompleteMockAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment =
            await paymentRepository.GetByIdAsync(
                paymentId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Payment was not found.");

        // Idempotent completion: repeating the same callback/action
        // does not create duplicate tickets, receipts or notifications.
        if (payment.Status == PaymentStatus.Successful)
            return Map(payment);

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending payments can be completed.");
        }

        var booking =
            await bookingRepository.GetByIdAsync(
                payment.BookingId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Booking was not found.");

        if (booking.Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Booking is no longer pending.");
        }

        var seatIds =
            await bookingRepository.GetSeatIdsAsync(
                booking.Id,
                cancellationToken);

        await seatTicketFulfillmentService
            .CompletePaidBookingAsync(
                booking.Id,
                booking.EventId,
                booking.CustomerUserId,
                booking.HoldToken,
                seatIds,
                cancellationToken);

        payment.Status = PaymentStatus.Successful;
        payment.PaidAtUtc = DateTime.UtcNow;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAtUtc = DateTime.UtcNow;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync(
            cancellationToken);

        await bookingRepository.SaveChangesAsync(
            cancellationToken);

        await receiptService.IssueAsync(
            payment.Id,
            booking.Id,
            booking.CustomerUserId,
            payment.Amount,
            payment.Currency,
            cancellationToken);

        await notificationService.CreateAsync(
            booking.CustomerUserId,
            "Booking confirmed",
            $"Booking {booking.BookingNumber} has been confirmed.",
            "Booking",
            cancellationToken);

        return Map(payment);
    }

    public async Task<PaymentResponse> FailMockAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment =
            await paymentRepository.GetByIdAsync(
                paymentId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Payment was not found.");

        // Idempotent failure: a duplicate failure callback/action
        // does not create duplicate notifications.
        if (payment.Status == PaymentStatus.Failed)
            return Map(payment);

        if (payment.Status == PaymentStatus.Successful)
        {
            throw new InvalidOperationException(
                "Successful payments cannot be marked failed.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending payments can be marked failed.");
        }

        payment.Status = PaymentStatus.Failed;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        await paymentRepository.SaveChangesAsync(
            cancellationToken);

        await notificationService.CreateAsync(
            payment.CustomerUserId,
            "Payment failed",
            "Your payment could not be completed. Please try again.",
            "Payment",
            cancellationToken);

        return Map(payment);
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
        Status = x.Status,
        PaidAtUtc = x.PaidAtUtc,
        CreatedAtUtc = x.CreatedAtUtc
    };
}
