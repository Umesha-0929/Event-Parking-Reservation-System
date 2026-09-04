using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Services;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Application.Features.Receipts.DTOs;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Payments;

public sealed class PaymentTransactionRecordingTests
{
    [Fact]
    public async Task StartAsync_records_checkout_created_transaction()
    {
        var customerId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerUserId = customerId,
            EventId = Guid.NewGuid(),
            BookingNumber = "TEST-BOOKING",
            HoldToken = "TEST-HOLD",
            Status = BookingStatus.Pending,
            TotalAmount = 2500m
        };

        var paymentRepository = new FakePaymentRepository();
        var bookingRepository = new FakeBookingRepository(booking);
        var transactionRepository =
            new FakePaymentTransactionRepository();

        var service = new PaymentService(
            paymentRepository,
            bookingRepository,
            new FakePaymentProvider(),
            new FakeSeatTicketFulfillmentService(),
            new FakeReceiptService(),
            new FakeNotificationService(),
            transactionRepository);

        var result = await service.StartAsync(
            customerId,
            new StartPaymentRequest
            {
                BookingId = booking.Id
            });

        Assert.NotEqual(Guid.Empty, result.PaymentId);

        var transaction =
            Assert.Single(transactionRepository.Items);

        Assert.Equal(
            result.PaymentId,
            transaction.PaymentId);

        Assert.Equal(
            booking.Id,
            transaction.BookingId);

        Assert.Equal(
            customerId,
            transaction.CustomerUserId);

        Assert.Equal(
            "CheckoutCreated",
            transaction.Type);

        Assert.Equal(
            PaymentStatus.Pending,
            transaction.Status);

        Assert.Equal(
            booking.TotalAmount,
            transaction.Amount);

        Assert.Equal(
            "LKR",
            transaction.Currency);
    }

    [Fact]
public async Task Successful_callback_is_idempotent_and_records_success_once()
{
    var customerId = Guid.NewGuid();

    var booking = new Booking
    {
        Id = Guid.NewGuid(),
        CustomerUserId = customerId,
        EventId = Guid.NewGuid(),
        BookingNumber = "TEST-CALLBACK-BOOKING",
        HoldToken = "TEST-CALLBACK-HOLD",
        Status = BookingStatus.Pending,
        TotalAmount = 3500m
    };

    var paymentRepository = new FakePaymentRepository();
    var bookingRepository = new FakeBookingRepository(booking);
    var transactionRepository =
        new FakePaymentTransactionRepository();

    var service = new PaymentService(
        paymentRepository,
        bookingRepository,
        new FakePaymentProvider(),
        new FakeSeatTicketFulfillmentService(),
        new FakeReceiptService(),
        new FakeNotificationService(),
        transactionRepository,
        new FakeSandboxPaymentCallbackVerifier());

    var started = await service.StartAsync(
        customerId,
        new StartPaymentRequest
        {
            BookingId = booking.Id
        });

    var callback = new SandboxPaymentCallbackRequest
    {
        PaymentId = started.PaymentId,
        ProviderReference = "CALLBACK-REF-001",
        Status = "SUCCESS",
        Amount = booking.TotalAmount,
        Currency = "LKR",
        TimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Signature = "TEST-SIGNATURE"
    };

    var first =
        await service.ProcessSandboxCallbackAsync(callback);

    var second =
        await service.ProcessSandboxCallbackAsync(callback);

    Assert.Equal(
        PaymentStatus.Successful,
        first.Status);

    Assert.Equal(
        PaymentStatus.Successful,
        second.Status);

    Assert.Equal(
        BookingStatus.Confirmed,
        booking.Status);

    Assert.Single(
        transactionRepository.Items.Where(
            x => x.Type == "PaymentSuccessful"));

    Assert.Single(
        transactionRepository.Items.Where(
            x => x.Type == "CheckoutCreated"));

    Assert.Equal(
        2,
        transactionRepository.Items.Count);
}

    private sealed class FakePaymentRepository
        : IPaymentRepository
    {
        private readonly List<Payment> items = new();

        public Task<Payment?> GetByIdAsync(
            Guid paymentId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                items.FirstOrDefault(
                    x => x.Id == paymentId));

        public Task<Payment?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                items
                    .Where(
                        x => x.BookingId == bookingId)
                    .OrderByDescending(
                        x => x.CreatedAtUtc)
                    .FirstOrDefault());

        public Task<Payment?> GetByCheckoutReferenceAsync(
            string checkoutReference,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                items.FirstOrDefault(
                    x =>
                        x.CheckoutReference ==
                        checkoutReference));

        public Task<IReadOnlyList<Payment>>
            GetByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Payment>>(
                items
                    .Where(
                        x =>
                            x.CustomerUserId ==
                            customerUserId)
                    .ToList());

        public Task AddAsync(
            Payment payment,
            CancellationToken cancellationToken = default)
        {
            items.Add(payment);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeBookingRepository
        : IBookingRepository
    {
        private readonly Booking booking;

        public FakeBookingRepository(
            Booking booking)
        {
            this.booking = booking;
        }

        public Task<Booking?> GetByIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Booking?>(
                booking.Id == bookingId
                    ? booking
                    : null);

        public Task<IReadOnlyList<Booking>>
            GetByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Booking>>(
                booking.CustomerUserId ==
                customerUserId
                    ? new[] { booking }
                    : Array.Empty<Booking>());

        public Task<IReadOnlyList<Guid>>
            GetSeatIdsAsync(
                Guid bookingId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Guid>>(
                Array.Empty<Guid>());

        public Task AddAsync(
            Booking booking,
            IReadOnlyCollection<BookingSeat> bookingSeats,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class
        FakePaymentTransactionRepository
        : IPaymentTransactionRepository
    {
        public List<PaymentTransaction> Items
        {
            get;
        } = new();

        public Task<IReadOnlyList<PaymentTransaction>>
            GetByPaymentAsync(
                Guid paymentId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<PaymentTransaction>>(
                    Items
                        .Where(
                            x =>
                                x.PaymentId ==
                                paymentId)
                        .ToList());

        public Task AddAsync(
            PaymentTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            Items.Add(transaction);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakePaymentProvider
        : IPaymentProvider
    {
        public Task<string> CreateCheckoutAsync(
            Guid bookingId,
            decimal amount,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                $"TEST-{bookingId:N}");
    }

    private sealed class
        FakeSeatTicketFulfillmentService
        : ISeatTicketFulfillmentService
    {
        public Task<IReadOnlyCollection<TicketDto>>
            CompletePaidBookingAsync(
                Guid bookingId,
                Guid eventId,
                Guid customerUserId,
                string holdToken,
                IReadOnlyCollection<Guid> seatIds,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyCollection<TicketDto>>(
                    Array.Empty<TicketDto>());
    }

    private sealed class FakeReceiptService
        : IReceiptService
    {
        public Task<IReadOnlyList<ReceiptResponse>>
            GetMineAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<ReceiptResponse>>(
                    Array.Empty<ReceiptResponse>());

        public Task<ReceiptResponse>
            GetByIdAsync(
                Guid customerUserId,
                Guid receiptId,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                new ReceiptResponse
                {
                    ReceiptId = receiptId,
                    CustomerUserId =
                        customerUserId
                });

        public Task<ReceiptResponse>
            IssueAsync(
                Guid paymentId,
                Guid bookingId,
                Guid customerUserId,
                decimal amount,
                string currency,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                new ReceiptResponse
                {
                    ReceiptId = Guid.NewGuid(),
                    PaymentId = paymentId,
                    BookingId = bookingId,
                    CustomerUserId =
                        customerUserId,
                    Amount = amount,
                    Currency = currency,
                    ReceiptNumber =
                        "TEST-RECEIPT",
                    IssuedAtUtc =
                        DateTime.UtcNow
                });
    }

    private sealed class FakeNotificationService
        : INotificationService
    {
        public Task<IReadOnlyList<NotificationResponse>>
            GetMineAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<NotificationResponse>>(
                    Array.Empty<NotificationResponse>());

        public Task<NotificationResponse>
            MarkReadAsync(
                Guid userId,
                Guid notificationId,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                new NotificationResponse
                {
                    NotificationId =
                        notificationId,
                    UserId = userId,
                    IsRead = true,
                    ReadAtUtc =
                        DateTime.UtcNow
                });

        public Task<NotificationResponse>
            CreateAsync(
                Guid userId,
                string title,
                string message,
                string type,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                new NotificationResponse
                {
                    NotificationId =
                        Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    CreatedAtUtc =
                        DateTime.UtcNow
                });
    }

    private sealed class FakeSandboxPaymentCallbackVerifier
        : ISandboxPaymentCallbackVerifier
    {
        public bool Verify(
            SandboxPaymentCallbackRequest request)
            => true;

        public string HashPayload(
            SandboxPaymentCallbackRequest request)
            => "TEST-PAYLOAD-HASH";
    }
}