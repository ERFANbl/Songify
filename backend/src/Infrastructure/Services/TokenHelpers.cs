using System.Text;

namespace Infrastructure.Services
{
    public static class TokenHelpers
    {
        public static string NormalizeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return token;
            return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? token["Bearer ".Length..].Trim()
                : token.Trim();
        }

        public static string HashToken(string token)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static TimeSpan GetTtlFromToken(string token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var ttl = jwt.ValidTo - DateTime.UtcNow;
            return ttl > TimeSpan.Zero ? ttl : TimeSpan.Zero;
        }
    }

}
