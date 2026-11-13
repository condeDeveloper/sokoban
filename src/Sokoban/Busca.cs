namespace Conde.Sokoban;

/// <summary>
/// A busca por EMPURRÕES. Entre dois empurrões o jogador só caminha, e para
/// onde ele caminha não muda nada: o que importa é de qual lado da caixa ele
/// consegue chegar. Então o estado vira (região alcançável, caixas), e a
/// região é representada pela sua menor casa.
///
/// Isso junta vários estados do <see cref="Juiz"/> num só, e é a única
/// diferença entre os dois. A poda de canto morto é opcional e fica desligada
/// por padrão, para que dê para medir quanto ela vale.
/// </summary>
public static class Busca
{
    /// <summary>Quantos estados a ultima busca visitou.</summary>
    public static long UltimoNumeroDeEstados { get; private set; }

    /// <summary>
    /// O menor numero de empurroes ate resolver, ou -1 se nao der.
    /// </summary>
    public static int Empurroes(Nivel nivel, bool comPoda = false)
    {
        long estados = 0;
        var visto = new HashSet<long>();
        var fila = new Queue<(int Regiao, int Caixas, int Passos)>();

        if (comPoda && CantoMorto(nivel, nivel.Caixas))
        {
            UltimoNumeroDeEstados = 0;
            return -1;
        }

        int regiaoInicial = Canonica(nivel, nivel.Jogador, nivel.Caixas);
        visto.Add((long)regiaoInicial * 65536 + nivel.Caixas);
        fila.Enqueue((regiaoInicial, nivel.Caixas, 0));

        while (fila.Count > 0)
        {
            var (regiao, caixas, passos) = fila.Dequeue();
            estados++;
            if (nivel.Pronto(caixas))
            {
                UltimoNumeroDeEstados = estados;
                return passos;
            }

            int alcance = Alcance(nivel, regiao, caixas);

            for (int casa = 0; casa < nivel.Casas; casa++)
            {
                if ((caixas & (1 << casa)) == 0) continue;
                for (int d = 0; d < 4; d++)
                {
                    int atras = nivel.Vizinha(casa, Oposta(d));
                    int frente = nivel.Vizinha(casa, d);
                    if (atras < 0 || frente < 0) continue;
                    if ((caixas & (1 << frente)) != 0) continue;
                    if ((alcance & (1 << atras)) == 0) continue;

                    int novas = caixas & ~(1 << casa) | (1 << frente);
                    if (comPoda && CantoMorto(nivel, novas)) continue;

                    int novaRegiao = Canonica(nivel, casa, novas);
                    long chave = (long)novaRegiao * 65536 + novas;
                    if (visto.Add(chave)) fila.Enqueue((novaRegiao, novas, passos + 1));
                }
            }
        }

        UltimoNumeroDeEstados = estados;
        return -1;
    }

    /// <summary>O nivel tem solucao?</summary>
    public static bool TemSolucao(Nivel nivel, bool comPoda = false)
        => Empurroes(nivel, comPoda) >= 0;

    /// <summary>A direcao oposta.</summary>
    public static int Oposta(int d) => d switch
    {
        0 => 1,
        1 => 0,
        2 => 3,
        _ => 2,
    };

    /// <summary>As casas que o jogador alcanca sem empurrar nada.</summary>
    public static int Alcance(Nivel nivel, int de, int caixas)
    {
        int visto = 1 << de;
        var pilha = new Stack<int>();
        pilha.Push(de);
        while (pilha.Count > 0)
        {
            int casa = pilha.Pop();
            for (int d = 0; d < 4; d++)
            {
                int v = nivel.Vizinha(casa, d);
                if (v < 0) continue;
                if ((caixas & (1 << v)) != 0) continue;
                if ((visto & (1 << v)) != 0) continue;
                visto |= 1 << v;
                pilha.Push(v);
            }
        }
        return visto;
    }

    /// <summary>A menor casa da regiao alcancavel: o nome canonico dela.</summary>
    public static int Canonica(Nivel nivel, int de, int caixas)
    {
        int regiao = Alcance(nivel, de, caixas);
        return System.Numerics.BitOperations.TrailingZeroCount((uint)regiao);
    }

    /// <summary>
    /// A poda: uma caixa num canto do retangulo que nao e alvo nunca mais sai
    /// de la. Nao precisa de busca nenhuma para saber que o nivel morreu.
    /// </summary>
    public static bool CantoMorto(Nivel nivel, int caixas)
    {
        for (int casa = 0; casa < nivel.Casas; casa++)
        {
            if ((caixas & (1 << casa)) == 0) continue;
            if (nivel.EhCanto(casa) && (nivel.Alvos & (1 << casa)) == 0) return true;
        }
        return false;
    }
}
