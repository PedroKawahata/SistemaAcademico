namespace SistemaAvaliacao.Models;

public sealed record Participacao(int Id, string Titulo, int Presencas, int Aulas)
    : Avaliacao(Id, Titulo)
{
    public override double CalcularNota() => 10.0 * Presencas / Aulas;
}
