namespace SistemaAvaliacao.Models;

public sealed record Prova(int Id, string Titulo, int Acertos, int Total, double? NotaRecuperacao = null)
    : Avaliacao(Id, Titulo), IRecuperavel
{
    public override double CalcularNota()
    {
        var notaOriginal = 10.0 * Acertos / Total;
        return Math.Max(notaOriginal, NotaRecuperacao ?? 0);
    }

    public Avaliacao Recuperar(double novaNota) => this with { NotaRecuperacao = novaNota };
}
