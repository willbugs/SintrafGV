using Microsoft.EntityFrameworkCore;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Associado> Associados => Set<Associado>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Eleicao> Eleicoes => Set<Eleicao>();
    public DbSet<Pergunta> Perguntas => Set<Pergunta>();
    public DbSet<Opcao> Opcoes => Set<Opcao>();
    public DbSet<Voto> Votos => Set<Voto>();
    public DbSet<VotoDetalhe> VotosDetalhes => Set<VotoDetalhe>();
    public DbSet<ConfiguracaoSindicato> ConfiguracoesSindicato => Set<ConfiguracaoSindicato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Associado>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cpf).HasMaxLength(14).IsRequired();
            e.Property(x => x.MatriculaSindicato).HasMaxLength(50);
            e.Property(x => x.MatriculaBancaria).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Celular).HasMaxLength(20);
            e.HasIndex(x => x.Cpf);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.SenhaHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50);
            e.Property(x => x.Ativo);
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Eleicao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(2000);
            e.Property(x => x.ArquivoAnexo).HasMaxLength(500);
            e.Property(x => x.Tipo).HasConversion<int>();
            e.Property(x => x.Status).HasConversion<int>();
            e.HasMany(x => x.Perguntas).WithOne(x => x.Eleicao).HasForeignKey(x => x.EleicaoId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Votos).WithOne(x => x.Eleicao).HasForeignKey(x => x.EleicaoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pergunta>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Texto).HasMaxLength(500).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(1000);
            e.Property(x => x.Tipo).HasConversion<int>();
            e.HasMany(x => x.Opcoes).WithOne(x => x.Pergunta).HasForeignKey(x => x.PerguntaId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.VotosDetalhes).WithOne(x => x.Pergunta).HasForeignKey(x => x.PerguntaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Opcao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Texto).HasMaxLength(300).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(1000);
            e.Property(x => x.Foto).HasMaxLength(500);
            e.HasOne(x => x.Associado).WithMany().HasForeignKey(x => x.AssociadoId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.VotosDetalhes).WithOne(x => x.Opcao).HasForeignKey(x => x.OpcaoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Voto>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.IpOrigem).HasMaxLength(50);
            e.Property(x => x.UserAgent).HasMaxLength(500);
            e.Property(x => x.CodigoComprovante).HasMaxLength(100);
            
            // Novos campos para autenticação cartorial
            e.Property(x => x.TimestampPreciso).HasMaxLength(50).IsRequired();
            e.Property(x => x.HashVoto).HasMaxLength(100).IsRequired();
            e.Property(x => x.AssinaturaDigital).HasMaxLength(500);
            e.Property(x => x.RespostaCriptografada).HasMaxLength(200).IsRequired();
            e.Property(x => x.ChaveCriptografia).HasMaxLength(100).IsRequired();
            e.Property(x => x.NumeroSequencial).HasMaxLength(50);
            e.Property(x => x.HashAnterior).HasMaxLength(100);
            e.Property(x => x.DadosDispositivo).HasMaxLength(200);
            e.Property(x => x.Latitude).HasMaxLength(50);
            e.Property(x => x.Longitude).HasMaxLength(50);
            e.Property(x => x.NumeroProtocoloCartorio).HasMaxLength(100);
            e.Property(x => x.ObservacoesValidacao).HasMaxLength(500);
            
            e.HasOne(x => x.Associado).WithMany().HasForeignKey(x => x.AssociadoId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EleicaoId, x.AssociadoId }).IsUnique();
            e.HasIndex(x => x.HashVoto).IsUnique();
            e.HasIndex(x => x.DataHoraVoto);
        });

        modelBuilder.Entity<VotoDetalhe>(e =>
        {
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ConfiguracaoSindicato>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RazaoSocial).HasMaxLength(200).IsRequired();
            e.Property(x => x.NomeFantasia).HasMaxLength(200).IsRequired();
            e.Property(x => x.CNPJ).HasMaxLength(18).IsRequired();
            e.Property(x => x.InscricaoEstadual).HasMaxLength(20);
            e.Property(x => x.Endereco).HasMaxLength(300).IsRequired();
            e.Property(x => x.Numero).HasMaxLength(10).IsRequired();
            e.Property(x => x.Complemento).HasMaxLength(100);
            e.Property(x => x.Bairro).HasMaxLength(100).IsRequired();
            e.Property(x => x.Cidade).HasMaxLength(100).IsRequired();
            e.Property(x => x.UF).HasMaxLength(2).IsRequired();
            e.Property(x => x.CEP).HasMaxLength(9).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20);
            e.Property(x => x.Celular).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Website).HasMaxLength(200);
            e.Property(x => x.Presidente).HasMaxLength(200).IsRequired();
            e.Property(x => x.CPFPresidente).HasMaxLength(14).IsRequired();
            e.Property(x => x.Secretario).HasMaxLength(200);
            e.Property(x => x.CPFSecretario).HasMaxLength(14);
            e.Property(x => x.TextoAutenticacao).HasMaxLength(500);
            e.Property(x => x.CartorioResponsavel).HasMaxLength(200);
            e.Property(x => x.EnderecoCartorio).HasMaxLength(300);
            e.Property(x => x.CriadoPor).HasMaxLength(100);
            e.Property(x => x.AtualizadoPor).HasMaxLength(100);
            
            e.HasIndex(x => x.CNPJ).IsUnique();
        });
    }
}
