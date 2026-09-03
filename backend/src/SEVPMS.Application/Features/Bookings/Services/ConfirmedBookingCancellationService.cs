using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Bookings.Interfaces;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Bookings.Services;

public sealed class ConfirmedBookingCancellationService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IPaymentRepository paymentRepository,
    IRefundRepository refundRepository,
    IPaymentTransactionRepository transactionRepository,
    ITicketService ticketService,
    INotificationService notificationService,
    IAuditLogService auditLogService)
    : IConfirmedBookingCancellationService
{
    public async Task<RefundResponse> CancelAndRefundAsync(
        Guid customerUserId,
        Guid bookingId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking was not found.");

        if (booking.CustomerUserId != customerUserId)
            throw new ForbiddenAccessException("You do not own this booking.");

        var existingRefund = await refundRepository.GetByBookingAsync(bookingId, cancellationToken);
        if (existingRefund is not null)
            return Map(existingRefund);

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException(
                "Only confirmed bookings use the confirmed cancellation/refund flow.");

        var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event was not found.");

        if (eventEntity.StartAtUtc <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "A confirmed booking cannot be refunded after the event has started.");

        var payment = await paymentRepository.GetByBookingIdAsync(booking.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                "No payment exists for this confirmed booking.");

        if (payment.Status != PaymentStatus.Successful)
            throw new InvalidOperationException("Only a successful payment can be refunded.");

        // Ticket state remains owned by Klegar; use the published service boundary.
        var tickets = await ticketService.GetForBookingAsync(booking.Id, cancellationToken);
        foreach (var ticket in tickets)
            await ticketService.CancelAsync(ticket.TicketNo, cancellationToken);

        var refund = new Refund
        {
            PaymentId = payment.Id,
            BookingId = booking.Id,
            CustomerUserId = customerUserId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Reason = string.IsNullOrWhiteSpace(reason) ? "Customer cancellation" : reason.Trim(),
            RefundReference =
                $"RFD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            Status = RefundStatus.Successful,
            RefundedAtUtc = DateTime.UtcNow
        };

        await refundRepository.AddAsync(refund, cancellationToken);

        payment.Status = PaymentStatus.Refunded;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await refundRepository.SaveChangesAsync(cancellationToken);
        await paymentRepository.SaveChangesAsync(cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        await transactionRepository.AddAsync(
            new PaymentTransaction
            {
                PaymentId = payment.Id,
                BookingId = booking.Id,
                CustomerUserId = customerUserId,
                Type = "RefundSuccessful",
                ProviderReference = refund.RefundReference,
                Status = PaymentStatus.Refunded,
                Amount = refund.Amount,
                Currency = refund.Currency,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        await transactionRepository.SaveChangesAsync(cancellationToken);

        await auditLogService.WriteAsync(
            customerUserId,
            "Confirmed booking cancelled and refunded",
            "Booking",
            booking.Id.ToString(),
            "Confirmed",
            "Cancelled / payment refunded",
            null,
            null,
            cancellationToken);

        await notificationService.CreateAsync(
            customerUserId,
            "Booking refunded",
            $"Booking {booking.BookingNumber} was cancelled and {refund.Amount:0.00} {refund.Currency} was refunded in the sandbox flow.",
            "Refund",
            cancellationToken);

        return Map(refund);
    }

    private static RefundResponse Map(Refund x) => new()
    {
        RefundId = x.Id,
        PaymentId = x.PaymentId,
        BookingId = x.BookingId,
        Amount = x.Amount,
        Currency = x.Currency,
        Reason = x.Reason,
        RefundReference = x.RefundReference,
        Status = x.Status,
        RefundedAtUtc = x.RefundedAtUtc
    };
}
