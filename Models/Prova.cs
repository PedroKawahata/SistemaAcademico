using Microsoft.Net.Http.Headers;

public class Prova : Avaliacao, IRecuperavel
{
    public int Acertos {get; set;}
    public int Total {get; set;}

    public double? NotaRecuperacao{get; set;}

    public override double CalcularNota()
    {
     double notaOriginal = (double)Acertos / Total *10;
        if (NotaRecuperacao.HasValue)
        {
            return Math.Max(notaOriginal, NotaRecuperacao.Value);    
        }
        return notaOriginal;
    }
        public double Recuperar(double novaNota)
    {
        NotaRecuperacao = novaNota;
        return CalcularNota();
    }
    }


