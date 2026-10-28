using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[Controller]")]

public class AvaliacoesController : ControllerBase
{
    private IAvaliacaoRepository _repository;

    public AvaliacoesController(IAvaliacaoRepository repository)
    {
        _repository = repository;
    }
    private static Dictionary<int, Avaliacao> _avaliacoes = new Dictionary<int, Avaliacao>()
    {
        {1, new Prova{ Id = 1, Titulo = "Prova 1", Acertos=5, Total = 10}},
        {2, new Trabalho{ Id = 2, Titulo = "Projeto Final", conteudo = 5, Apresentacao = 6}},
        {3, new Participacao{Id = 3, Titulo = "Presenca em aula", Presencas = 18, Aulas = 20}}
    };
    [HttpGet]
    public IActionResult Get()
    {
        var resultado = _repository.ObterTodos().Select(avaliacao =>
        {
            return new
            {
              avaliacao.Id,
              Tipo = avaliacao.GetType().Name,
              Descricao = avaliacao.Descricao(),
              Nota = Math.Round(avaliacao.CalcularNota(), 2)
            };
        });
        return Ok(resultado);
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        bool removida = _repository.Remover(id);
        if (!removida)
        {
            return NotFound("Avaliação não encontrada.");
        }
        return Ok("Avaliação removida com sucesso.");
    }
}