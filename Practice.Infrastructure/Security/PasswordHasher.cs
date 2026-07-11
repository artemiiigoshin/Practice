using System.Security.Cryptography;
using System.Text;
using Practice.Application.Security;

namespace Practice.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        return Convert.ToHexString(bytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var actualHash = Hash(password);

        return string.Equals(
            actualHash,
            passwordHash,
            StringComparison.OrdinalIgnoreCase);
    }
}