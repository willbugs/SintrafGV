using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SintrafGv.Infrastructure.Repositories
{
    public class VotoRepository : IVotoRepository
    {
        private readonly AppDbContext _context;

        public VotoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Voto>> ObterVotosPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .Include(v => v.Associado)
                .Include(v => v.Eleicao)
                .Where(v => v.EleicaoId == eleicaoId)
                .OrderBy(v => v.DataHoraVoto)
                .ToListAsync(cancellationToken);
        }

        public async Task<Voto?> ObterVotoPorIdAsync(Guid votoId, CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .Include(v => v.Associado)
                .Include(v => v.Eleicao)
                .FirstOrDefaultAsync(v => v.Id == votoId, cancellationToken);
        }

        public async Task<bool> VerificarVotoExistenteAsync(Guid eleicaoId, Guid associadoId, CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .AnyAsync(v => v.EleicaoId == eleicaoId && v.AssociadoId == associadoId, cancellationToken);
        }

        public async Task<Voto> SalvarVotoAsync(Voto voto, CancellationToken cancellationToken = default)
        {
            if (voto.Id == Guid.Empty)
            {
                voto.Id = Guid.NewGuid();
                _context.Votos.Add(voto);
            }
            else
            {
                _context.Votos.Update(voto);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return voto;
        }

        public async Task<int> ContarVotosPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .CountAsync(v => v.EleicaoId == eleicaoId, cancellationToken);
        }

        public async Task<List<Voto>> ObterVotosPorAssociadoAsync(Guid associadoId, CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .Include(v => v.Eleicao)
                .Where(v => v.AssociadoId == associadoId)
                .OrderByDescending(v => v.DataHoraVoto)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Voto>> ListarTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .Include(v => v.Associado)
                .Include(v => v.Eleicao)
                .OrderByDescending(v => v.DataHoraVoto)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Voto>> ListarPorEleicaoAsync(Guid eleicaoId, CancellationToken cancellationToken = default)
        {
            return await _context.Votos
                .Include(v => v.Associado)
                .Include(v => v.Eleicao)
                .Where(v => v.EleicaoId == eleicaoId)
                .OrderBy(v => v.DataHoraVoto)
                .ToListAsync(cancellationToken);
        }
    }
}