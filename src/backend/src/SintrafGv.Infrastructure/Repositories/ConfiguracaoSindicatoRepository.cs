using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SintrafGv.Infrastructure.Repositories
{
    public class ConfiguracaoSindicatoRepository : IConfiguracaoSindicatoRepository
    {
        private readonly AppDbContext _context;

        public ConfiguracaoSindicatoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConfiguracaoSindicato?> ObterConfiguracaoAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ConfiguracoesSindicato
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ConfiguracaoSindicato> SalvarConfiguracaoAsync(ConfiguracaoSindicato configuracao, CancellationToken cancellationToken = default)
        {
            var existente = await ObterConfiguracaoAsync(cancellationToken);
            
            if (existente != null)
            {
                // Atualizar configuração existente
                existente.RazaoSocial = configuracao.RazaoSocial;
                existente.NomeFantasia = configuracao.NomeFantasia;
                existente.CNPJ = configuracao.CNPJ;
                existente.InscricaoEstadual = configuracao.InscricaoEstadual;
                existente.Endereco = configuracao.Endereco;
                existente.Numero = configuracao.Numero;
                existente.Complemento = configuracao.Complemento;
                existente.Bairro = configuracao.Bairro;
                existente.Cidade = configuracao.Cidade;
                existente.UF = configuracao.UF;
                existente.CEP = configuracao.CEP;
                existente.Telefone = configuracao.Telefone;
                existente.Celular = configuracao.Celular;
                existente.Email = configuracao.Email;
                existente.Website = configuracao.Website;
                existente.Presidente = configuracao.Presidente;
                existente.CPFPresidente = configuracao.CPFPresidente;
                existente.Secretario = configuracao.Secretario;
                existente.CPFSecretario = configuracao.CPFSecretario;
                existente.TextoAutenticacao = configuracao.TextoAutenticacao;
                existente.CartorioResponsavel = configuracao.CartorioResponsavel;
                existente.EnderecoCartorio = configuracao.EnderecoCartorio;
                existente.AtualizadoEm = DateTime.UtcNow;
                existente.AtualizadoPor = configuracao.AtualizadoPor;
                
                _context.ConfiguracoesSindicato.Update(existente);
                await _context.SaveChangesAsync(cancellationToken);
                return existente;
            }
            else
            {
                // Criar nova configuração
                configuracao.Id = Guid.NewGuid();
                configuracao.CriadoEm = DateTime.UtcNow;
                
                _context.ConfiguracoesSindicato.Add(configuracao);
                await _context.SaveChangesAsync(cancellationToken);
                return configuracao;
            }
        }

        public async Task<bool> ExisteConfiguracaoAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ConfiguracoesSindicato.AnyAsync(cancellationToken);
        }
    }
}