namespace Conde.Sokoban;

/// <summary>As tabelas do README. Todo numero de la sai daqui.</summary>
public static class Medidas
{
    private static readonly Dictionary<string, object> Cache = [];

    private static T Guardado<T>(string chave, Func<T> faz)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(chave, out var pronto)) return (T)pronto;
            var feito = faz()!;
            Cache[chave] = feito;
            return feito;
        }
    }

    /// <summary>Os tamanhos varridos por inteiro.</summary>
    public static readonly (int Largura, int Altura, int Caixas)[] Tamanhos =
    [
        (3, 3, 1), (3, 3, 2), (3, 3, 3),
        (3, 4, 1), (3, 4, 2),
        (4, 4, 1), (4, 4, 2),
    ];

    /// <summary>
    /// Os tamanhos da tabela CRUZADA. O 4 por 4 com duas caixas fica de fora:
    /// ele sozinho tem 201.600 niveis e teria de ser varrido duas vezes, com
    /// e sem poda, so para encher uma linha.
    /// </summary>
    public static readonly (int Largura, int Altura, int Caixas)[] TamanhosCruzados =
    [
        (3, 3, 1), (3, 3, 2), (3, 3, 3), (3, 4, 1), (3, 4, 2), (4, 4, 1),
    ];

    /// <summary>A varredura de um tamanho, guardada.</summary>
    public static Medida Do(int largura, int altura, int k)
        => Guardado($"{largura}x{altura}x{k}", () => Varredura.Mede(largura, altura, k));

    /// <summary>A tabela principal: um tamanho por linha.</summary>
    public static IReadOnlyList<Medida> Tabela()
        => Guardado("Tabela", () => Tamanhos.Select(t => Do(t.Largura, t.Altura, t.Caixas)).ToList());

    /// <summary>
    /// O juiz, que conta MOVIMENTOS sem normalizar o jogador, contra a busca,
    /// que conta EMPURROES. Os numeros sao diferentes de proposito: o que tem
    /// de bater e QUEM TEM SOLUCAO.
    /// </summary>
    public static (long Niveis, long Divergem) JuizContraBusca(int largura, int altura, int k)
        => Guardado($"Juiz{largura}x{altura}x{k}", () =>
        {
            long niveis = 0, divergem = 0;
            foreach (var nivel in Varredura.Niveis(largura, altura, k))
            {
                niveis++;
                if (Juiz.TemSolucao(nivel) != Busca.TemSolucao(nivel)) divergem++;
            }
            return (niveis, divergem);
        });

    /// <summary>
    /// A poda de canto morto: quanto ela decide sozinha e se ela erra.
    /// </summary>
    public static IReadOnlyList<Medida> Poda() => Tabela();

    /// <summary>
    /// Com a poda ligada e desligada: quantos estados a varredura visita, e
    /// se a resposta muda. Tabela cruzada, nao duas medidas soltas.
    /// </summary>
    public static IReadOnlyList<(int Largura, int Altura, int Caixas, long SemPoda,
                                 long ComPoda, double Razao, bool MesmaResposta)> Cruzada()
        => Guardado("Cruzada", () =>
        {
            var saida = new List<(int, int, int, long, long, double, bool)>();
            foreach (var (l, a, k) in TamanhosCruzados)
            {
                var sem = Do(l, a, k);
                var com = Varredura.Mede(l, a, k, comPoda: true);
                bool igual = sem.Resolvem == com.Resolvem
                          && sem.MaiorEmpurroes == com.MaiorEmpurroes;
                saida.Add((l, a, k, sem.Estados, com.Estados,
                           (double)sem.Estados / com.Estados, igual));
            }
            return (IReadOnlyList<(int, int, int, long, long, double, bool)>)saida;
        });

    /// <summary>O desenho de um nivel, em texto.</summary>
    public static string Desenha(Nivel nivel)
    {
        var linhas = new List<string>();
        for (int l = 0; l < nivel.Altura; l++)
        {
            var linha = new System.Text.StringBuilder();
            for (int c = 0; c < nivel.Largura; c++)
            {
                int casa = l * nivel.Largura + c;
                bool caixa = (nivel.Caixas & (1 << casa)) != 0;
                bool alvo = (nivel.Alvos & (1 << casa)) != 0;
                bool jog = nivel.Jogador == casa;
                linha.Append(caixa && alvo ? '*' : caixa ? 'O' : jog && alvo ? '+'
                             : jog ? '@' : alvo ? '.' : ' ');
            }
            linhas.Add("|" + linha + "|");
        }
        return string.Join("\n", linhas);
    }
}
