namespace Nkolex.Propman.Server.Abstractions
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);

        bool VerifyPassword(string hashedPassword, string providedPassword);

        bool IsHashed(string value);
    }
}
