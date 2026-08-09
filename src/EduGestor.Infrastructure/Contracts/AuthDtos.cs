namespace EduGestor.Infrastructure.Contracts;

public record RegisterRequest(string Email, string Password, string Name, Guid TenantId);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
public record UserDto(Guid Id, string Email, string Name, string Role, Guid TenantId);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record RefreshRequest();
