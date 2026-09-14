using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Users.DTOs;

public sealed class UpdateUserStatusRequest
{
    public AccountStatus Status { get; set; }
}