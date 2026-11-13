using Conde.Sokoban;
using Xunit;

namespace Conde.Sokoban.Testes;

/// <summary>Todo numero do README esta preso aqui.</summary>
public class MedidasTestes
{
    // largura, altura, caixas, niveis, com solucao, maior numero de empurroes
    public static TheoryData<int, int, int, long, long, int> ATabela() => new()
    {
        { 3, 3, 1, 648, 200, 2 },
        { 3, 3, 2, 9_072, 1_216, 3 },
        { 3, 3, 3, 42_336, 1_608, 3 },
        { 3, 4, 1, 1_584, 550, 3 },
        { 3, 4, 2, 43_560, 7_652, 5 },
        { 4, 4, 1, 3_840, 1_500, 4 },
        { 4, 4, 2, 201_600, 43_644, 6 },
    };

    [Theory]
    [MemberData(nameof(ATabela))]
    public void A_tabela_principal(int largura, int altura, int k, long total,
                                   long resolvem, int maior)
    {
        var m = Medidas.Do(largura, altura, k);
        Assert.Equal(total, m.Total);
        Assert.Equal(resolvem, m.Resolvem);
        Assert.Equal(maior, m.MaiorEmpurroes);
    }

    [Fact]
    public void Ao_todo_302640_niveis_foram_varridos()
    {
        Assert.Equal(302_640L, Medidas.Tabela().Sum(m => m.Total));
    }

    // largura, altura, caixas, quantos a poda mata, quantos nao tem solucao
    public static TheoryData<int, int, int, long, long> APoda() => new()
    {
        { 3, 3, 1, 256, 448 },
        { 3, 3, 2, 5_390, 7_856 },
        { 3, 3, 3, 29_292, 40_728 },
        { 3, 4, 1, 484, 1_034 },
        { 3, 4, 2, 21_500, 35_908 },
        { 4, 4, 1, 900, 2_340 },
        { 4, 4, 2, 80_556, 157_956 },
    };

    [Theory]
    [MemberData(nameof(APoda))]
    public void A_poda_de_canto_morto(int largura, int altura, int k, long mata, long sem)
    {
        var m = Medidas.Do(largura, altura, k);
        Assert.Equal(mata, m.PodaMata);
        Assert.Equal(sem, m.SemSolucao);
    }

    [Fact]
    public void A_poda_nunca_matou_um_nivel_que_tinha_solucao()
    {
        foreach (var m in Medidas.Tabela())
            Assert.Equal(0, m.ErrosDaPoda);
    }

    [Fact]
    public void A_poda_decide_metade_dos_sem_solucao_no_maior_tabuleiro()
    {
        var m = Medidas.Do(4, 4, 2);
        Assert.Equal(80_556L, m.PodaMata);
        Assert.Equal(157_956L, m.SemSolucao);
        double fracao = 100.0 * m.PodaMata / m.SemSolucao;
        Assert.True(fracao > 50.9);
        Assert.True(fracao < 51.1);
    }

    [Fact]
    public void A_fracao_com_solucao_cai_com_a_caixa_e_sobe_com_o_tabuleiro()
    {
        // mais caixas no mesmo tabuleiro: menos niveis com solucao
        Assert.True(Medidas.Do(3, 3, 1).PorCento > Medidas.Do(3, 3, 2).PorCento);
        Assert.True(Medidas.Do(3, 3, 2).PorCento > Medidas.Do(3, 3, 3).PorCento);
        // mesmo numero de caixas, tabuleiro maior: mais niveis com solucao
        Assert.True(Medidas.Do(4, 4, 1).PorCento > Medidas.Do(3, 4, 1).PorCento);
        Assert.True(Medidas.Do(3, 4, 1).PorCento > Medidas.Do(3, 3, 1).PorCento);
    }

    [Fact]
    public void Em_tres_por_tres_com_tres_caixas_so_tres_e_oito_por_cento_tem_solucao()
    {
        var m = Medidas.Do(3, 3, 3);
        Assert.True(m.PorCento > 3.7);
        Assert.True(m.PorCento < 3.9);
    }

    [Fact]
    public void O_histograma_do_maior_tabuleiro_e_este()
    {
        var h = Medidas.Do(4, 4, 2).Histograma;
        Assert.Equal(1_680L, h[0]);
        Assert.Equal(5_712L, h[1]);
        Assert.Equal(10_800L, h[2]);
        Assert.Equal(12_568L, h[3]);
        Assert.Equal(8_880L, h[4]);
        Assert.Equal(3_360L, h[5]);
        Assert.Equal(644L, h[6]);
        Assert.Equal(43_644L, h.Values.Sum());
    }

    [Fact]
    public void Todo_histograma_sobe_ate_um_pico_e_desce_sem_voltar()
    {
        foreach (var m in Medidas.Tabela())
        {
            var valores = m.Histograma.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            int pico = Array.LastIndexOf(valores, valores.Max());
            for (int i = 1; i <= pico; i++) Assert.True(valores[i] >= valores[i - 1]);
            for (int i = pico + 1; i < valores.Length; i++) Assert.True(valores[i] < valores[i - 1]);
        }
    }

    [Fact]
    public void So_um_dos_sete_histogramas_empata_no_topo()
    {
        // A primeira versao deste teste exigia subida ESTRITA ate o pico e
        // quebrou: o 4 por 4 com uma caixa tem 480 niveis com um empurrao e
        // 480 com dois, empatados. E o unico patamar dos sete.
        var empatam = new List<string>();
        foreach (var m in Medidas.Tabela())
        {
            var valores = m.Histograma.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            long topo = valores.Max();
            if (valores.Count(v => v == topo) > 1)
                empatam.Add($"{m.Largura}x{m.Altura}x{m.Caixas}");
        }
        Assert.Equal(new[] { "4x4x1" }, empatam);
        var quatro = Medidas.Do(4, 4, 1).Histograma;
        Assert.Equal(480L, quatro[1]);
        Assert.Equal(480L, quatro[2]);
    }

    [Fact]
    public void O_juiz_e_a_busca_concordam_em_quem_tem_solucao()
    {
        long niveis = 0;
        foreach (var (l, a, k) in new[] { (3, 3, 1), (3, 3, 2), (3, 4, 1), (4, 4, 1) })
        {
            var (quantos, divergem) = Medidas.JuizContraBusca(l, a, k);
            niveis += quantos;
            Assert.Equal(0, divergem);
        }
        Assert.Equal(15_144L, niveis);
    }

    // largura, altura, caixas, estados sem poda, estados com poda
    public static TheoryData<int, int, int, long, long> ACruzada() => new()
    {
        { 3, 3, 1, 1_416, 696 },
        { 3, 3, 2, 37_904, 9_130 },
        { 3, 3, 3, 132_004, 27_687 },
        { 3, 4, 1, 4_818, 2_926 },
        { 3, 4, 2, 403_708, 138_910 },
        { 4, 4, 1, 16_080, 11_310 },
    };

    [Theory]
    [MemberData(nameof(ACruzada))]
    public void A_tabela_cruzada_da_poda(int largura, int altura, int k, long sem, long com)
    {
        var linha = Medidas.Cruzada()
                           .Single(x => x.Largura == largura && x.Altura == altura
                                     && x.Caixas == k);
        Assert.Equal(sem, linha.SemPoda);
        Assert.Equal(com, linha.ComPoda);
        Assert.True(linha.MesmaResposta);
    }

    [Fact]
    public void A_poda_rende_mais_no_tabuleiro_apertado_e_nao_no_grande()
    {
        var apertado = Medidas.Cruzada().Single(x => x.Largura == 3 && x.Altura == 3 && x.Caixas == 3);
        var grande = Medidas.Cruzada().Single(x => x.Largura == 4 && x.Altura == 4 && x.Caixas == 1);
        // a aposta escrita antes de medir era o contrario: tabuleiro maior,
        // mais canto ruim, mais corte. O que manda e a densidade de caixas.
        Assert.True(apertado.Razao > 4.5);
        Assert.True(grande.Razao < 1.5);
        Assert.True(apertado.Razao > grande.Razao * 3);
    }

    [Fact]
    public void A_poda_nunca_muda_a_resposta_de_nenhum_tamanho_cruzado()
    {
        foreach (var linha in Medidas.Cruzada())
            Assert.True(linha.MesmaResposta);
    }

    [Fact]
    public void O_recordista_de_cada_tamanho_tem_mesmo_o_numero_de_empurroes_dito()
    {
        foreach (var m in Medidas.Tabela())
        {
            Assert.NotNull(m.Recordista);
            Assert.Equal(m.MaiorEmpurroes, Busca.Empurroes(m.Recordista!));
        }
    }

    [Fact]
    public void O_desenho_do_nivel_tem_as_medidas_do_tabuleiro()
    {
        var m = Medidas.Do(4, 4, 2);
        var linhas = Medidas.Desenha(m.Recordista!).Split('\n');
        Assert.Equal(4, linhas.Length);
        foreach (var linha in linhas) Assert.Equal(6, linha.Length);
    }
}
