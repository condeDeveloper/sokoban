namespace Conde.Sokoban;

/// <summary>
/// O juiz joga do jeito mais burro: largura primeiro no grafo de estados
/// (posicao exata do jogador, mascara das caixas), contando MOVIMENTOS —
/// andar e empurrar valem um cada.
///
/// Ele nao normaliza o jogador, nao conhece canto morto e nao sabe o que e
/// poda. E mais caro e e por isso que ele serve: se a <see cref="Busca"/>
/// disser que um nivel nao tem solucao e ele achar uma, a poda esta comendo
/// resposta.
/// </summary>
public static class Juiz
{
    /// <summary>
    /// O menor numero de movimentos ate resolver, ou -1 se nao der.
    /// </summary>
    public static int Movimentos(Nivel nivel)
    {
        var visto = new HashSet<int>();
        var fila = new Queue<(int Jogador, int Caixas, int Passos)>();

        int inicio = nivel.Jogador * 65536 + nivel.Caixas;
        visto.Add(inicio);
        fila.Enqueue((nivel.Jogador, nivel.Caixas, 0));

        while (fila.Count > 0)
        {
            var (jog, caixas, passos) = fila.Dequeue();
            if (nivel.Pronto(caixas)) return passos;

            for (int d = 0; d < 4; d++)
            {
                int frente = nivel.Vizinha(jog, d);
                if (frente < 0) continue;

                int novasCaixas = caixas;
                if ((caixas & (1 << frente)) != 0)
                {
                    int depois = nivel.Vizinha(frente, d);
                    if (depois < 0 || (caixas & (1 << depois)) != 0) continue;
                    novasCaixas = caixas & ~(1 << frente) | (1 << depois);
                }

                int chave = frente * 65536 + novasCaixas;
                if (visto.Add(chave)) fila.Enqueue((frente, novasCaixas, passos + 1));
            }
        }

        return -1;
    }

    /// <summary>O nivel tem solucao?</summary>
    public static bool TemSolucao(Nivel nivel) => Movimentos(nivel) >= 0;
}
