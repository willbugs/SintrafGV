using Microsoft.Extensions.DependencyInjection;
using SintrafGv.Domain.Interfaces;
using SintrafGv.Application.Interfaces;
using SintrafGv.Application.Services;

namespace SintrafGv.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAssociadoService, AssociadoService>();
        services.AddScoped<IEleicaoService, EleicaoService>();
        services.AddScoped<IRelatorioService, RelatorioService>();
        services.AddScoped<IExportacaoService, ExportacaoService>();
        services.AddScoped<IRelatorioVotacaoCartorialService, RelatorioVotacaoCartorialService>();
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        return services;
    }
}
