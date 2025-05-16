using Truckero.Core.Interfaces;

namespace Truckero.Infrastructure.Services;

public class HashService : IHashService
{
    public string Hash(string input) => BCrypt.Net.BCrypt.HashPassword(input);
    public bool Verify(string input, string hash) => BCrypt.Net.BCrypt.Verify(input, hash);
}