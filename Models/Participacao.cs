public class Participacao : Avaliacao
{
    public int Presencas {get; set;}
    public int Aulas {get; set;}

    public override double CalcularNota()
    {
        return (double) Presencas / Aulas * 10;
    }
}