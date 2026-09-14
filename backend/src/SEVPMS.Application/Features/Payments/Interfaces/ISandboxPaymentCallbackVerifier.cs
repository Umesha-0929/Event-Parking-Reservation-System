using SEVPMS.Application.Features.Payments.DTOs;

namespace SEVPMS.Application.Features.Payments.Interfaces;

public interface ISandboxPaymentCallbackVerifier
{
    bool Verify(SandboxPaymentCallbackRequest request);
    string HashPayload(SandboxPaymentCallbackRequest request);
}
