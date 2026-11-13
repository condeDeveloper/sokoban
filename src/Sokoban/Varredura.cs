namespace Conde.Sokoban;

/// <summary>
/// A varredura: TODOS os niveis que cabem num retangulo aberto de um tamanho
/// dado, com um numero fixo de caixas.
///
/// Um nivel e uma escolha de caixas, uma escolha de alvos do mesmo tamanho, e
/// uma casa livre para o jogador. Nada e sorteado e nada e desenhado a mao:
/// o conjunto inteiro de niveis daquele tamanho passa pela busca.
/// </summary>
public static class Varredura
{
    /// <summary>Todos os niveis de um retangulo com k caixas.</summary>
    public static IEnumerable<Nivel> Niveis(int largura, int altura, int k)
    {
        int casas = largura * altura;
        foreach (int caixas in Subconjuntos(casas, k))
            foreach (int alvos in Subconjuntos(casas, k))
                for (int jogador = 0; jogador < casas; jogador++)
                {
                    if ((caixas & (1 << jogador)) != 0) continue;
                    yield return new Nivel(largura, altura, caixas, alvos, jogador);
                }
    }

    /// <summary>Todas as mascaras de k bits entre n casas.</summary>
    public static IEnumerable<int> Subconjuntos(int n, int k)
    {
        if (k == 0) { yield return 0; yield break; }
        var escolha = new int[k];

        IEnumerable<int> Anda(int inicio, int quantos, int mascara)
        {
            if (quantos == k) { yield return mascara; yield break; }
            for (int i = inicio; i < n; i++)
            {
                escolha[quantos] = i;
                foreach (var r in Anda(i + 1, quantos + 1, mascara | (1 << i)))
                    yield return r;
            }
        }

        foreach (var m in Anda(0, 0, 0)) yield return m;
    }

    /// <summary>O que a varredura de um tamanho devolve.</summary>
    public static Medida Mede(int largura, int altura, int k, bool comPoda = false)
    {
        long total = 0, resolvem = 0, estados = 0, podaMata = 0, erros = 0;
        int maior = -1;
        Nivel? recordista = null;
        var histograma = new Dictionary<int, long>();

        foreach (var nivel in Niveis(largura, altura, k))
        {
            total++;
            // a poda de canto morto, medida na MESMA passada: sem isso a
            // varredura inteira rodava duas vezes so para contar isto aqui
            bool morta = Busca.CantoMorto(nivel, nivel.Caixas);
            if (morta) podaMata++;

            int e = Busca.Empurroes(nivel, comPoda);
            estados += Busca.UltimoNumeroDeEstados;
            if (e < 0) continue;
            resolvem++;
            if (morta) erros++;
            histograma[e] = histograma.GetValueOrDefault(e) + 1;
            if (e > maior)
            {
                maior = e;
                recordista = nivel;
            }
        }

        return new Medida(largura, altura, k, total, resolvem, maior, estados,
                          recordista, histograma, podaMata, erros);
    }

}

/// <summary>O que a varredura de um tamanho devolve.</summary>
public sealed class Medida(int largura, int altura, int k, long total, long resolvem,
                           int maiorEmpurroes, long estados, Nivel? recordista,
                           Dictionary<int, long> histograma, long podaMata, long errosDaPoda)
{
    /// <summary>Quantos niveis a poda de canto morto mata na largada.</summary>
    public long PodaMata { get; } = podaMata;

    /// <summary>Quantas vezes a poda matou um nivel que TINHA solucao.</summary>
    public long ErrosDaPoda { get; } = errosDaPoda;

    /// <summary>Quantos niveis nao tem solucao.</summary>
    public long SemSolucao => Total - Resolvem;

    /// <summary>Quantas colunas.</summary>
    public int Largura { get; } = largura;

    /// <summary>Quantas linhas.</summary>
    public int Altura { get; } = altura;

    /// <summary>Quantas caixas.</summary>
    public int Caixas { get; } = k;

    /// <summary>Quantos niveis existem desse tamanho.</summary>
    public long Total { get; } = total;

    /// <summary>Quantos tem solucao.</summary>
    public long Resolvem { get; } = resolvem;

    /// <summary>O maior numero minimo de empurroes, entre os que tem solucao.</summary>
    public int MaiorEmpurroes { get; } = maiorEmpurroes;

    /// <summary>Quantos estados a varredura visitou ao todo.</summary>
    public long Estados { get; } = estados;

    /// <summary>Um nivel que realiza o maximo.</summary>
    public Nivel? Recordista { get; } = recordista;

    /// <summary>Quantos niveis tem cada numero minimo de empurroes.</summary>
    public IReadOnlyDictionary<int, long> Histograma { get; } = histograma;

    /// <summary>A fracao com solucao, em por cento.</summary>
    public double PorCento => Total == 0 ? 0 : 100.0 * Resolvem / Total;
}
