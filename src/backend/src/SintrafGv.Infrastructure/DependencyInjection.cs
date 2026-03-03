using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Infrastructure.Data;
using SintrafGv.Infrastructure.Repositories;

namespace SintrafGv.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=127.0.0.1;Initial Catalog=Sintraf_GV;User Id=Durval;Password=Lspxmw01oz;TrustServerCertificate=True;Connect Timeout=30;";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAssociadoRepository, AssociadoRepository>();
        services.AddScoped<IEleicaoRepository, EleicaoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IVotoRepository, VotoRepository>();
        services.AddScoped<IConfiguracaoSindicatoRepository, ConfiguracaoSindicatoRepository>();
        services.AddScoped<IConfiguracaoEmailRepository, ConfiguracaoEmailRepository>();

        return services;
    }
}
