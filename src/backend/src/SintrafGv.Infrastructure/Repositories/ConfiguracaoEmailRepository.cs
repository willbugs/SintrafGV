using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Infrastructure.Data;

namespace SintrafGv.Infrastructure.Repositories;

public class ConfiguracaoEmailRepository : IConfiguracaoEmailRepository
{
    private readonly AppDbContext _context;

    public ConfiguracaoEmailRepository(AppDbContext context) => _context = context;

    public async Task<ConfiguracaoEmail?> ObterAsync(CancellationToken cancellationToken = default) =>
        await _context.ConfiguracoesEmail.FirstOrDefaultAsync(cancellationToken);

    public async Task<ConfiguracaoEmail> SalvarAsync(ConfiguracaoEmail config, CancellationToken cancellationToken = default)
    {
        var existente = await ObterAsync(cancellationToken);
        if (existente != null)
        {
            existente.SmtpHost = config.SmtpHost;
            existente.SmtpPort = config.SmtpPort;
            existente.UsarSsl = config.UsarSsl;
            existente.SmtpUsuario = config.SmtpUsuario;
            existente.SmtpSenha = config.SmtpSenha;
            existente.EmailRemetente = config.EmailRemetente;
            existente.NomeRemetente = config.NomeRemetente;
            existente.Habilitado = config.Habilitado;
            existente.AtualizadoEm = DateTime.UtcNow;
            _context.ConfiguracoesEmail.Update(existente);
            await _context.SaveChangesAsync(cancellationToken);
            return existente;
        }
        config.Id = Guid.NewGuid();
        config.CriadoEm = DateTime.UtcNow;
        _context.ConfiguracoesEmail.Add(config);
        await _context.SaveChangesAsync(cancellationToken);
        return config;
    }
}
