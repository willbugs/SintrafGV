using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SintrafGv.Application.Interfaces;
using SintrafGv.Infrastructure.Data;
using SintrafGv.Infrastructure.Repositories;

namespace SintrafGv.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=SintrafGv;User Id=sa;Password=SintrafGv_Dev2025!;TrustServerCertificate=True;";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAssociadoRepository, AssociadoRepository>();
        services.AddScoped<IEleicaoRepository, EleicaoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}
