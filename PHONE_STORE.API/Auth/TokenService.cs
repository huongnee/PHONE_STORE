using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Application.Options;

namespace PHONE_STORE.API.Auth;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opt;
    public TokenService(Microsoft.Extensions.Options.IOptions<JwtOptions> opt) => _opt = opt.Value;

    public string CreateAccessToken(long userId, string? email, IEnumerable<string> roles)
    {
        var active = _opt.Keys.SingleOrDefault(k => k.Kid == _opt.ActiveKid);
        if (active is null)
            throw new InvalidOperationException($"Jwt:ActiveKid '{_opt.ActiveKid}' không khớp key nào trong Jwt:Keys.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(active.Key)) { KeyId = active.Kid };
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // Chuẩn OpenID
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            // Dự phòng cho middleware/lib khác
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new(JwtRegisteredClaimNames.Email, email));
            claims.Add(new(ClaimTypes.Name, email));
        }

        if (roles != null)
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
