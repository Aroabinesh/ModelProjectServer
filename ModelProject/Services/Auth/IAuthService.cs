using ModelProject.Dtos.Auth;

namespace ModelProject.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken);
}
