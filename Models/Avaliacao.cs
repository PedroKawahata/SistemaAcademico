using System.Text.Json.Serialization;

namespace SistemaAvaliacao.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipo")]
[JsonDerivedType(typeof(Prova), "prova")]
[JsonDerivedType(typeof(Trabalho), "trabalho")]
[JsonDerivedType(typeof(Participacao), "participacao")]
public abstract record Avaliacao(int Id, string Titulo)
{
    public abstract double CalcularNota();

    public virtual string Descricao() => $"Avaliação: {Titulo}";
}
