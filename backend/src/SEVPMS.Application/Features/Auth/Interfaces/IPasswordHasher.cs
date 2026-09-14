namespace SEVPMS.Application.Features.Auth.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(
        string passwordHash,
        string providedPassword);
}