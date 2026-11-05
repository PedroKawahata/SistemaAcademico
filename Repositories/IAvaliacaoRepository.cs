using SistemaAvaliacao.Models;

namespace SistemaAvaliacao.Repositories;

public interface IAvaliacaoRepository
{
    IReadOnlyList<Avaliacao> ObterTodos();
    Avaliacao? ObterPorId(int id);
    Avaliacao Adicionar(Func<int, Avaliacao> criar);
    Avaliacao? Atualizar(int id, Func<Avaliacao, Avaliacao> alterar);
    bool Remover(int id);
}
