namespace SistemaAvaliacao.Models;

public sealed record Trabalho(int Id, string Titulo, double Conteudo, double Apresentacao)
    : Avaliacao(Id, Titulo)
{
    public override double CalcularNota() => (Conteudo + Apresentacao) / 2;

    public override string Descricao() => $"Trabalho prático: {Titulo}";
}
