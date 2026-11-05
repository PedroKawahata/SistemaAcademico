using SistemaAvaliacao.Models;

namespace SistemaAvaliacao.Contracts;

public sealed record AvaliacaoResposta(
    int Id, string Titulo, string Tipo, string Descricao, double Nota,
    int? Acertos = null, int? Total = null, double? NotaRecuperacao = null,
    double? Conteudo = null, double? Apresentacao = null, int? Presencas = null, int? Aulas = null)
{
    public static AvaliacaoResposta De(Avaliacao avaliacao)
    {
        var resposta = new AvaliacaoResposta(avaliacao.Id, avaliacao.Titulo, avaliacao.GetType().Name,
            avaliacao.Descricao(), Math.Round(avaliacao.CalcularNota(), 2));

        return avaliacao switch
        {
            Prova prova => resposta with
            {
                Acertos = prova.Acertos, Total = prova.Total, NotaRecuperacao = prova.NotaRecuperacao
            },
            Trabalho trabalho => resposta with { Conteudo = trabalho.Conteudo, Apresentacao = trabalho.Apresentacao },
            Participacao participacao => resposta with { Presencas = participacao.Presencas, Aulas = participacao.Aulas },
            _ => resposta
        };
    }
}
