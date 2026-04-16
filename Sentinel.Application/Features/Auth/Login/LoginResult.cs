namespace Sentinel.Application.Features.Auth.Login;

public class LoginResult
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}