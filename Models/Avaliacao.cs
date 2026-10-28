public abstract class Avaliacao
{
    public int Id {get; init;}
    public string? Titulo {get; set;}

    public abstract double CalcularNota();

    public virtual string Descricao()
    {
        return $"Avaliacao: {Titulo}";
    }

}