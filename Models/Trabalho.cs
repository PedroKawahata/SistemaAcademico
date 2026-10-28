public class Trabalho : Avaliacao
{
    public double conteudo {get; set;}
    public double Apresentacao {get; set;}

    public override double CalcularNota()
    {
        return (conteudo + Apresentacao) /2;
    }

    public override string Descricao()
    {
        return $"Trabalho Prático {Titulo}";
    }
}