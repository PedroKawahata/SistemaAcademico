using Microsoft.AspNetCore.Mvc;
using SistemaAvaliacao.Contracts;
using SistemaAvaliacao.Models;
using SistemaAvaliacao.Repositories;

namespace SistemaAvaliacao.Controllers;

[ApiController]
[Route("api/avaliacoes")]
public sealed class AvaliacoesController(IAvaliacaoRepository repository) : ControllerBase
{
    [HttpGet]
    public IEnumerable<AvaliacaoResposta> Listar() => repository.ObterTodos().Select(AvaliacaoResposta.De);

    [HttpGet("{id:int}")]
    public ActionResult<AvaliacaoResposta> Obter(int id)
    {
        var avaliacao = repository.ObterPorId(id);
        return avaliacao is null ? NotFound() : AvaliacaoResposta.De(avaliacao);
    }

    [HttpPost]
    public ActionResult<AvaliacaoResposta> Criar(AvaliacaoEntrada entrada)
    {
        var avaliacao = repository.Adicionar(id => entrada.Criar(id));
        return CreatedAtAction(nameof(Obter), new { avaliacao.Id }, AvaliacaoResposta.De(avaliacao));
    }

    [HttpPut("{id:int}")]
    public ActionResult<AvaliacaoResposta> Atualizar(int id, AvaliacaoEntrada entrada)
    {
        var avaliacao = repository.Atualizar(id, anterior => entrada.Criar(id, anterior));
        return avaliacao is null ? NotFound() : AvaliacaoResposta.De(avaliacao);
    }

    [HttpPut("{id:int}/recuperacao")]
    public ActionResult<AvaliacaoResposta> RegistrarRecuperacao(int id, RecuperacaoEntrada entrada)
    {
        var avaliacao = repository.Atualizar(id, anterior =>
            anterior is IRecuperavel recuperavel ? recuperavel.Recuperar(entrada.Nota!.Value) : anterior);

        return avaliacao switch
        {
            null => NotFound(),
            IRecuperavel => AvaliacaoResposta.De(avaliacao),
            _ => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Este tipo de avaliação não aceita recuperação.")
        };
    }

    [HttpDelete("{id:int}")]
    public IActionResult Remover(int id) => repository.Remover(id) ? NoContent() : NotFound();
}
