public interface IAvaliacaoRepository
{
    IEnumerable<Avaliacao> ObterTodos();
    Avaliacao? ObterPorId(int id);
    bool Remover(int id);
}
