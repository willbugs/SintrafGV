namespace SintrafGv.Application.DTOs;

/// <summary>DTO de usuário (quem faz login). Sem matrícula nem dados de associado.</summary>
public record UsuarioListDto(Guid Id, string Nome, string Email, string Role, bool Ativo, DateTime CriadoEm);

public record CreateUsuarioRequest(string Nome, string Email, string Senha, string Role);

public record UpdateUsuarioRequest(string Nome, string Email, string Role, bool Ativo);
