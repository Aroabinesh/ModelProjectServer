using Microsoft.EntityFrameworkCore;
using ModelProject.Common.Exceptions;
using ModelProject.Data;
using ModelProject.Dtos.Auth;

namespace ModelProject.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApplicationDbContext db, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var username = dto.Username.Trim();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        // Same message whether the username doesn't exist or the password is wrong, so the
        // response never reveals which part of the credential pair was incorrect.
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username '{Username}'.", username);
            throw new UnauthorizedException("Invalid username or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        _logger.LogInformation("User {UserId} logged in.", user.Id);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Username = user.Username
        };
    }
}
