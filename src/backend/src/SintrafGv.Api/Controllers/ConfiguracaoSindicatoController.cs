using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SintrafGv.Application.DTOs;
using SintrafGv.Domain.Entities;
using SintrafGv.Domain.Interfaces;

namespace SintrafGv.Api.Controllers
{
    [ApiController]
    [Route("api/configuracao-sindicato")]
    [Authorize]
    public class ConfiguracaoSindicatoController : ControllerBase
    {
        private readonly IConfiguracaoSindicatoRepository _repository;

        public ConfiguracaoSindicatoController(IConfiguracaoSindicatoRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Obtém a configuração atual do sindicato
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ConfiguracaoSindicatoDto>> ObterConfiguracao()
        {
            var configuracao = await _repository.ObterConfiguracaoAsync();

            if (configuracao == null)
            {
                return Ok(new ConfiguracaoSindicatoDto());
            }

            var dto = new ConfiguracaoSindicatoDto
            {
                Id = configuracao.Id,
                RazaoSocial = configuracao.RazaoSocial,
                NomeFantasia = configuracao.NomeFantasia,
                CNPJ = configuracao.CNPJ,
                InscricaoEstadual = configuracao.InscricaoEstadual,
                Endereco = configuracao.Endereco,
                Numero = configuracao.Numero,
                Complemento = configuracao.Complemento,
                Bairro = configuracao.Bairro,
                Cidade = configuracao.Cidade,
                UF = configuracao.UF,
                CEP = configuracao.CEP,
                Telefone = configuracao.Telefone,
                Celular = configuracao.Celular,
                Email = configuracao.Email,
                Website = configuracao.Website,
                Presidente = configuracao.Presidente,
                CPFPresidente = configuracao.CPFPresidente,
                Secretario = configuracao.Secretario,
                CPFSecretario = configuracao.CPFSecretario,
                TextoAutenticacao = configuracao.TextoAutenticacao,
                CartorioResponsavel = configuracao.CartorioResponsavel,
                EnderecoCartorio = configuracao.EnderecoCartorio
            };

            return Ok(dto);
        }

        /// <summary>
        /// Salva ou atualiza a configuração do sindicato
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ConfiguracaoSindicatoDto>> SalvarConfiguracao(
            [FromBody] ConfiguracaoSindicatoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var configuracao = new ConfiguracaoSindicato
            {
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                CNPJ = dto.CNPJ,
                InscricaoEstadual = dto.InscricaoEstadual,
                Endereco = dto.Endereco,
                Numero = dto.Numero,
                Complemento = dto.Complemento,
                Bairro = dto.Bairro,
                Cidade = dto.Cidade,
                UF = dto.UF,
                CEP = dto.CEP,
                Telefone = dto.Telefone,
                Celular = dto.Celular,
                Email = dto.Email,
                Website = dto.Website,
                Presidente = dto.Presidente,
                CPFPresidente = dto.CPFPresidente,
                Secretario = dto.Secretario,
                CPFSecretario = dto.CPFSecretario,
                TextoAutenticacao = dto.TextoAutenticacao,
                CartorioResponsavel = dto.CartorioResponsavel,
                EnderecoCartorio = dto.EnderecoCartorio,
                AtualizadoPor = User.Identity?.Name ?? "Sistema"
            };

            var resultado = await _repository.SalvarConfiguracaoAsync(configuracao);

            var resultadoDto = new ConfiguracaoSindicatoDto
            {
                Id = resultado.Id,
                RazaoSocial = resultado.RazaoSocial,
                NomeFantasia = resultado.NomeFantasia,
                CNPJ = resultado.CNPJ,
                InscricaoEstadual = resultado.InscricaoEstadual,
                Endereco = resultado.Endereco,
                Numero = resultado.Numero,
                Complemento = resultado.Complemento,
                Bairro = resultado.Bairro,
                Cidade = resultado.Cidade,
                UF = resultado.UF,
                CEP = resultado.CEP,
                Telefone = resultado.Telefone,
                Celular = resultado.Celular,
                Email = resultado.Email,
                Website = resultado.Website,
                Presidente = resultado.Presidente,
                CPFPresidente = resultado.CPFPresidente,
                Secretario = resultado.Secretario,
                CPFSecretario = resultado.CPFSecretario,
                TextoAutenticacao = resultado.TextoAutenticacao,
                CartorioResponsavel = resultado.CartorioResponsavel,
                EnderecoCartorio = resultado.EnderecoCartorio
            };

            return Ok(resultadoDto);
        }

        /// <summary>
        /// Verifica se existe configuração do sindicato
        /// </summary>
        [HttpGet("existe")]
        public async Task<ActionResult<bool>> ExisteConfiguracao()
        {
            var existe = await _repository.ExisteConfiguracaoAsync();
            return Ok(existe);
        }
    }

    public class ConfiguracaoSindicatoDto
    {
        public Guid Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;
        public string? InscricaoEstadual { get; set; }
        public string Endereco { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Celular { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string Presidente { get; set; } = string.Empty;
        public string CPFPresidente { get; set; } = string.Empty;
        public string? Secretario { get; set; }
        public string? CPFSecretario { get; set; }
        public string? TextoAutenticacao { get; set; }
        public string? CartorioResponsavel { get; set; }
        public string? EnderecoCartorio { get; set; }
    }
}