namespace SintrafGv.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, UserDto User);

public record UserDto(Guid Id, string Nome, string Email, string Role);
