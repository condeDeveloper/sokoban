using Conde.Sokoban;
using Xunit;

namespace Conde.Sokoban.Testes;

/// <summary>Niveis pequenos, com a resposta sabida na mao.</summary>
public class NucleoTestes
{
    private static int Casa(int c, int l, int largura) => l * largura + c;

    [Fact]
    public void Um_nivel_que_ja_comeca_resolvido_custa_zero()
    {
        // caixa em cima do alvo
        var nivel = new Nivel(3, 3, 1 << 4, 1 << 4, 0);
        Assert.Equal(0, Busca.Empurroes(nivel));
        Assert.Equal(0, Juiz.Movimentos(nivel));
    }

    [Fact]
    public void Empurrar_uma_casa_custa_um_empurrao_e_dois_movimentos()
    {
        // 3 por 3: jogador em (0,1), caixa em (1,1), alvo em (2,1)
        int jog = Casa(0, 1, 3), caixa = Casa(1, 1, 3), alvo = Casa(2, 1, 3);
        var nivel = new Nivel(3, 3, 1 << caixa, 1 << alvo, jog);
        Assert.Equal(1, Busca.Empurroes(nivel));
        Assert.Equal(1, Juiz.Movimentos(nivel));
    }

    [Fact]
    public void Caixa_num_canto_que_nao_e_alvo_nunca_sai_de_la()
    {
        var nivel = new Nivel(3, 3, 1 << 0, 1 << 4, 8);
        Assert.True(Busca.CantoMorto(nivel, nivel.Caixas));
        Assert.Equal(-1, Busca.Empurroes(nivel));
        Assert.Equal(-1, Juiz.Movimentos(nivel));
    }

    [Fact]
    public void Caixa_num_canto_que_e_alvo_esta_pronta()
    {
        var nivel = new Nivel(3, 3, 1 << 0, 1 << 0, 8);
        Assert.False(Busca.CantoMorto(nivel, nivel.Caixas));
        Assert.Equal(0, Busca.Empurroes(nivel));
    }

    [Fact]
    public void No_tabuleiro_aberto_o_jogador_da_a_volta_para_empurrar_do_outro_lado()
    {
        // Jogador a DIREITA da caixa e alvo a direita: ele nao atravessa a
        // caixa, mas o retangulo e aberto, entao ele contorna por cima e
        // empurra. Eu tinha escrito este teste esperando -1, e a varredura
        // disse 1: quem nao tem parede interna sempre pode contornar.
        int jog = Casa(2, 1, 3), caixa = Casa(1, 1, 3), alvo = Casa(2, 1, 3);
        var nivel = new Nivel(3, 3, 1 << caixa, 1 << alvo, jog);
        Assert.Equal(1, Busca.Empurroes(nivel));
        // quatro passos de caminhada mais o empurrao
        Assert.Equal(5, Juiz.Movimentos(nivel));
    }

    [Fact]
    public void As_vizinhas_param_na_borda()
    {
        var nivel = new Nivel(3, 3, 0, 0, 0);
        Assert.Equal(-1, nivel.Vizinha(0, 0));
        Assert.Equal(-1, nivel.Vizinha(0, 2));
        Assert.Equal(1, nivel.Vizinha(0, 1));
        Assert.Equal(3, nivel.Vizinha(0, 3));
    }

    [Fact]
    public void Os_quatro_cantos_do_retangulo_sao_cantos_e_o_resto_nao()
    {
        var nivel = new Nivel(4, 3, 0, 0, 0);
        var cantos = Enumerable.Range(0, 12).Where(nivel.EhCanto).ToArray();
        Assert.Equal(new[] { 0, 3, 8, 11 }, cantos);
    }

    [Fact]
    public void O_alcance_do_jogador_para_na_caixa()
    {
        // caixa atravessada na coluna do meio de um 3 por 1 nao existe;
        // num 3 por 3, caixas em (1,0),(1,1),(1,2) partem o tabuleiro
        int caixas = (1 << 1) | (1 << 4) | (1 << 7);
        var nivel = new Nivel(3, 3, caixas, 0, 0);
        int alcance = Busca.Alcance(nivel, 0, caixas);
        Assert.Equal((1 << 0) | (1 << 3) | (1 << 6), alcance);
        Assert.Equal(0, Busca.Canonica(nivel, 0, caixas));
        Assert.Equal(2, Busca.Canonica(nivel, 2, caixas));
    }

    [Fact]
    public void A_direcao_oposta_e_mesmo_oposta()
    {
        for (int d = 0; d < 4; d++)
            Assert.Equal(d, Busca.Oposta(Busca.Oposta(d)));
        Assert.Equal(1, Busca.Oposta(0));
        Assert.Equal(3, Busca.Oposta(2));
    }

    [Fact]
    public void A_contagem_de_niveis_e_a_conta_de_escolher_caixas_alvos_e_jogador()
    {
        foreach (var (largura, altura, k) in Medidas.Tamanhos)
        {
            long casas = largura * altura;
            long escolhas = Binomial((int)casas, k);
            Assert.Equal(escolhas * escolhas * (casas - k),
                         Varredura.Niveis(largura, altura, k).LongCount());
        }
    }

    private static long Binomial(int n, int k)
    {
        long r = 1;
        for (int i = 1; i <= k; i++) r = r * (n - k + i) / i;
        return r;
    }
}
