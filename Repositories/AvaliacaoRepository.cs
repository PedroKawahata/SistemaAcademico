using System.Text.Json;
using SistemaAvaliacao.Models;

namespace SistemaAvaliacao.Repositories;

public sealed class AvaliacaoRepository : IAvaliacaoRepository
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly object _sincronizacao = new();
    private readonly string _arquivo;
    private Estado _estado;

    public AvaliacaoRepository(IConfiguration configuracao, IWebHostEnvironment ambiente)
    {
        var caminho = configuracao["Avaliacoes:Arquivo"] ?? "App_Data/avaliacoes.json";
        _arquivo = Path.GetFullPath(caminho, ambiente.ContentRootPath);
        _estado = File.Exists(_arquivo)
            ? JsonSerializer.Deserialize<Estado>(File.ReadAllText(_arquivo), OpcoesJson)
                ?? throw new InvalidDataException("O arquivo de avaliações está vazio.")
            : new Estado(4, [
                new Prova(1, "Prova 1", 5, 10),
                new Trabalho(2, "Projeto final", 5, 6),
                new Participacao(3, "Presença em aula", 18, 20)
            ]);
    }

    public IReadOnlyList<Avaliacao> ObterTodos()
    {
        lock (_sincronizacao)
        {
            return _estado.Avaliacoes.ToArray();
        }
    }

    public Avaliacao? ObterPorId(int id)
    {
        lock (_sincronizacao)
        {
            return _estado.Avaliacoes.Find(avaliacao => avaliacao.Id == id);
        }
    }

    public Avaliacao Adicionar(Func<int, Avaliacao> criar)
    {
        lock (_sincronizacao)
        {
            var avaliacao = criar(_estado.ProximoId);
            Salvar(new Estado(checked(avaliacao.Id + 1), [.. _estado.Avaliacoes, avaliacao]));
            return avaliacao;
        }
    }

    public Avaliacao? Atualizar(int id, Func<Avaliacao, Avaliacao> alterar)
    {
        lock (_sincronizacao)
        {
            var indice = _estado.Avaliacoes.FindIndex(avaliacao => avaliacao.Id == id);
            if (indice < 0)
            {
                return null;
            }

            var anterior = _estado.Avaliacoes[indice];
            var atualizada = alterar(anterior);
            if (atualizada != anterior)
            {
                List<Avaliacao> avaliacoes = [.. _estado.Avaliacoes];
                avaliacoes[indice] = atualizada;
                Salvar(_estado with { Avaliacoes = avaliacoes });
            }

            return atualizada;
        }
    }

    public bool Remover(int id)
    {
        lock (_sincronizacao)
        {
            var avaliacoes = _estado.Avaliacoes.Where(avaliacao => avaliacao.Id != id).ToList();
            if (avaliacoes.Count == _estado.Avaliacoes.Count)
            {
                return false;
            }

            Salvar(_estado with { Avaliacoes = avaliacoes });
            return true;
        }
    }

    private void Salvar(Estado novoEstado)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_arquivo)!);
        var temporario = _arquivo + ".tmp";
        try
        {
            File.WriteAllText(temporario, JsonSerializer.Serialize(novoEstado, OpcoesJson));
            File.Move(temporario, _arquivo, overwrite: true);
            _estado = novoEstado;
        }
        finally
        {
            File.Delete(temporario);
        }
    }

    private sealed record Estado(int ProximoId, List<Avaliacao> Avaliacoes);
}
