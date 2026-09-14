namespace SEVPMS.Application.Features.Tickets.Interfaces;
public interface ITicketQrTokenService
{
    string CreatePayload(Guid ticketId);
    string HashPayload(string payload);
    bool TryValidatePayload(string payload, out Guid ticketId);
}
