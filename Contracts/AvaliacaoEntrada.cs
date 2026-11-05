using System.ComponentModel.DataAnnotations;
using SistemaAvaliacao.Models;

namespace SistemaAvaliacao.Contracts;

public sealed class AvaliacaoEntrada : IValidatableObject
{
    [Required, StringLength(120)]
    public string Titulo { get; init; } = "";

    [Required, RegularExpression("^(Prova|Trabalho|Participacao)$",
        ErrorMessage = "Use Prova, Trabalho ou Participacao.")]
    public string Tipo { get; init; } = "";

    [Range(0, int.MaxValue)]
    public int? Acertos { get; init; }

    [Range(1, int.MaxValue)]
    public int? Total { get; init; }

    [Range(0, 10)]
    public double? Conteudo { get; init; }

    [Range(0, 10)]
    public double? Apresentacao { get; init; }

    [Range(0, int.MaxValue)]
    public int? Presencas { get; init; }

    [Range(1, int.MaxValue)]
    public int? Aulas { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        switch (Tipo)
        {
            case nameof(Prova) when Acertos is null || Total is null || Acertos > Total:
                yield return new("Informe acertos e total, com acertos menores ou iguais ao total.",
                    [nameof(Acertos), nameof(Total)]);
                break;
            case nameof(Trabalho) when Conteudo is null || Apresentacao is null:
                yield return new("Informe as notas de conteúdo e apresentação.",
                    [nameof(Conteudo), nameof(Apresentacao)]);
                break;
            case nameof(Participacao) when Presencas is null || Aulas is null || Presencas > Aulas:
                yield return new("Informe presenças e aulas, com presenças menores ou iguais às aulas.",
                    [nameof(Presencas), nameof(Aulas)]);
                break;
        }
    }

    public Avaliacao Criar(int id, Avaliacao? anterior = null) => Tipo switch
    {
        nameof(Prova) => new Prova(id, Titulo.Trim(), Acertos!.Value, Total!.Value,
            (anterior as Prova)?.NotaRecuperacao),
        nameof(Trabalho) => new Trabalho(id, Titulo.Trim(), Conteudo!.Value, Apresentacao!.Value),
        nameof(Participacao) => new Participacao(id, Titulo.Trim(), Presencas!.Value, Aulas!.Value),
        _ => throw new InvalidOperationException("Tipo de avaliação inválido.")
    };
}

public sealed class RecuperacaoEntrada
{
    [Required, Range(0, 10)]
    public double? Nota { get; init; }
}
