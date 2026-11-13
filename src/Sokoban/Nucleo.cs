namespace Conde.Sokoban;

/// <summary>
/// Sokoban num retangulo aberto: sem parede nenhuma por dentro, so a borda.
/// As unicas coisas que atrapalham sao as proprias caixas.
///
/// O jogador anda nas quatro direcoes para casa vazia. Para EMPURRAR uma
/// caixa, ele tem de estar do lado oposto e a casa seguinte tem de estar
/// vazia e dentro do tabuleiro. Ganha quando toda caixa esta num alvo.
///
/// Um nivel e: as medidas do retangulo, as caixas, os alvos e onde o jogador
/// comeca. Tudo cabe em mascara de bits porque o maior tabuleiro varrido tem
/// 16 casas.
/// </summary>
public sealed class Nivel(int largura, int altura, int caixas, int alvos, int jogador)
{
    /// <summary>Quantas colunas.</summary>
    public int Largura { get; } = largura;

    /// <summary>Quantas linhas.</summary>
    public int Altura { get; } = altura;

    /// <summary>As casas com caixa, em mascara.</summary>
    public int Caixas { get; } = caixas;

    /// <summary>As casas que sao alvo, em mascara.</summary>
    public int Alvos { get; } = alvos;

    /// <summary>Onde o jogador comeca.</summary>
    public int Jogador { get; } = jogador;

    /// <summary>Quantas casas o tabuleiro tem.</summary>
    public int Casas => Largura * Altura;

    /// <summary>A coluna e a linha de uma casa.</summary>
    public (int Coluna, int Linha) Onde(int casa) => (casa % Largura, casa / Largura);

    /// <summary>
    /// A casa vizinha na direcao dada (0 esquerda, 1 direita, 2 cima,
    /// 3 baixo), ou -1 quando sai do tabuleiro.
    /// </summary>
    public int Vizinha(int casa, int direcao)
    {
        var (c, l) = Onde(casa);
        switch (direcao)
        {
            case 0: c--; break;
            case 1: c++; break;
            case 2: l--; break;
            case 3: l++; break;
            default: throw new ArgumentOutOfRangeException(nameof(direcao));
        }
        if (c < 0 || c >= Largura || l < 0 || l >= Altura) return -1;
        return l * Largura + c;
    }

    /// <summary>A casa e um canto do retangulo?</summary>
    public bool EhCanto(int casa)
    {
        var (c, l) = Onde(casa);
        return (c == 0 || c == Largura - 1) && (l == 0 || l == Altura - 1);
    }

    /// <summary>Quantas casas tem caixa.</summary>
    public int QuantasCaixas => System.Numerics.BitOperations.PopCount((uint)Caixas);

    /// <summary>O nivel ja esta resolvido?</summary>
    public bool Pronto(int caixas) => caixas == Alvos;
}
