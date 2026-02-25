using SintrafGv.Domain.Entities;

namespace SintrafGv.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuario usuario);
    string GenerateTokenAssociado(Associado associado);
}
