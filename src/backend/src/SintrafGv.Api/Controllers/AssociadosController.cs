using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SintrafGv.Application.DTOs;
using SintrafGv.Application.Exceptions;
using SintrafGv.Application.Services;
using SintrafGv.Domain.Entities;

namespace SintrafGv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssociadosController : ControllerBase
{
    private readonly IAssociadoService _service;

    public AssociadosController(IAssociadoService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssociadoDto>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var associado = await _service.ObterPorIdAsync(id, cancellationToken);
        if (associado is null)
            return NotFound();
        return Ok(ToDto(associado));
    }

    [HttpGet]
    public async Task<ActionResult<object>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20,
        [FromQuery] bool apenasAtivos = false,
        CancellationToken cancellationToken = default)
    {
        // Leitura explícita da query (ToString() em StringValues retorna "" quando ausente)
        var buscaTrim = string.IsNullOrWhiteSpace(Request.Query["busca"].ToString()) ? null : Request.Query["busca"].ToString()!.Trim();
        var statusTrim = string.IsNullOrWhiteSpace(Request.Query["status"].ToString()) ? null : Request.Query["status"].ToString()!.Trim();
        if (!string.IsNullOrEmpty(buscaTrim) || !string.IsNullOrEmpty(statusTrim))
        {
            var (itens, total) = await _service.ListarAsync(pagina, porPagina, buscaTrim, statusTrim, cancellationToken);
            return Ok(new { itens = itens.Select(ToDto), total });
        }
        var (itensLegado, totalLegado) = await _service.ListarAsync(pagina, porPagina, apenasAtivos, cancellationToken);
        return Ok(new { itens = itensLegado.Select(ToDto), total = totalLegado });
    }

    [HttpPost]
    public async Task<ActionResult<AssociadoDto>> Criar([FromBody] CreateAssociadoRequest request, CancellationToken cancellationToken)
    {
        var associado = new Associado
        {
            Nome = request.Nome,
            Cpf = request.Cpf,
            MatriculaSindicato = request.MatriculaSindicato,
            MatriculaBancaria = request.MatriculaBancaria,
            Sexo = request.Sexo,
            EstadoCivil = request.EstadoCivil,
            DataNascimento = request.DataNascimento,
            Naturalidade = request.Naturalidade,
            Cep = request.Cep,
            Endereco = request.Endereco,
            Complemento = request.Complemento,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            Estado = request.Estado,
            Banco = request.Banco,
            Agencia = request.Agencia,
            CodAgencia = request.CodAgencia,
            Conta = request.Conta,
            Funcao = request.Funcao,
            Ctps = request.Ctps,
            Serie = request.Serie,
            DataAdmissao = request.DataAdmissao,
            DataFiliacao = request.DataFiliacao,
            DataDesligamento = request.DataDesligamento,
            Telefone = request.Telefone,
            Celular = request.Celular,
            Email = request.Email,
            Ativo = request.Ativo,
            Aposentado = request.Aposentado
        };
        try
        {
            var criado = await _service.CriarAsync(associado, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, ToDto(criado));
        }
        catch (CpfDuplicadoException)
        {
            return Conflict(new { message = "Já existe um associado cadastrado com este CPF." });
        }
        catch (ArgumentException ex) when (ex.ParamName == "associado")
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return Conflict(new { message = "Já existe um associado cadastrado com este CPF." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Atualizar(Guid id, [FromBody] UpdateAssociadoRequest request, CancellationToken cancellationToken)
    {
        var associado = await _service.ObterPorIdAsync(id, cancellationToken);
        if (associado is null)
            return NotFound();

        associado.Nome = request.Nome;
        associado.Cpf = request.Cpf;
        associado.MatriculaSindicato = request.MatriculaSindicato;
        associado.MatriculaBancaria = request.MatriculaBancaria;
        associado.Sexo = request.Sexo;
        associado.EstadoCivil = request.EstadoCivil;
        associado.DataNascimento = request.DataNascimento;
        associado.Naturalidade = request.Naturalidade;
        associado.Cep = request.Cep;
        associado.Endereco = request.Endereco;
        associado.Complemento = request.Complemento;
        associado.Bairro = request.Bairro;
        associado.Cidade = request.Cidade;
        associado.Estado = request.Estado;
        associado.Banco = request.Banco;
        associado.Agencia = request.Agencia;
        associado.CodAgencia = request.CodAgencia;
        associado.Conta = request.Conta;
        associado.Funcao = request.Funcao;
        associado.Ctps = request.Ctps;
        associado.Serie = request.Serie;
        associado.DataAdmissao = request.DataAdmissao;
        associado.DataFiliacao = request.DataFiliacao;
        associado.DataDesligamento = request.DataDesligamento;
        associado.Telefone = request.Telefone;
        associado.Celular = request.Celular;
        associado.Email = request.Email;
        associado.Ativo = request.Ativo;
        associado.Aposentado = request.Aposentado;

        try
        {
            await _service.AtualizarAsync(associado, cancellationToken);
            return NoContent();
        }
        catch (CpfDuplicadoException)
        {
            return Conflict(new { message = "Já existe um associado cadastrado com este CPF." });
        }
        catch (ArgumentException ex) when (ex.ParamName == "associado")
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return Conflict(new { message = "Já existe um associado cadastrado com este CPF." });
        }
    }

    /// <summary>Detecta violação de índice/constraint único no SQL Server (2627, 2601).</summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        for (var e = ex.InnerException; e != null; e = e.InnerException)
        {
            if (e is Microsoft.Data.SqlClient.SqlException sqlEx &&
                (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                return true;
        }
        return false;
    }

    private static AssociadoDto ToDto(Associado a) => new(
        a.Id,
        a.Nome,
        a.Cpf,
        a.MatriculaSindicato,
        a.MatriculaBancaria,
        a.Sexo,
        a.EstadoCivil,
        a.DataNascimento,
        a.Naturalidade,
        a.Cep,
        a.Endereco,
        a.Complemento,
        a.Bairro,
        a.Cidade,
        a.Estado,
        a.Banco,
        a.Agencia,
        a.CodAgencia,
        a.Conta,
        a.Funcao,
        a.Ctps,
        a.Serie,
        a.DataAdmissao,
        a.DataFiliacao,
        a.DataDesligamento,
        a.Telefone,
        a.Celular,
        a.Email,
        a.Ativo,
        a.Aposentado,
        a.DataUltimaAtualizacao,
        a.CriadoEm);
}
