using Conde.Sokoban;

if (args.Length == 0)
{
    Console.WriteLine("medidor <tabela|recordistas|histograma|juiz|poda|cruzada|tudo>");
    return 1;
}

switch (args[0])
{
    case "tabela": Tabela(); break;
    case "recordistas": Recordistas(); break;
    case "histograma": Histograma(); break;
    case "juiz": JuizContraBusca(); break;
    case "poda": Poda(); break;
    case "cruzada": Cruzada(); break;
    case "tudo":
        Tabela(); Recordistas(); Histograma(); JuizContraBusca(); Poda(); Cruzada();
        break;
    default:
        Console.WriteLine($"nao conheco '{args[0]}'");
        return 1;
}
return 0;

void Tabela()
{
    Console.WriteLine("tabuleiro   caixas      niveis   com solucao   por cento   maior empurroes");
    foreach (var m in Medidas.Tabela())
        Console.WriteLine($"{m.Largura} por {m.Altura}    {m.Caixas,6} {m.Total,11:n0} " +
                          $"{m.Resolvem,13:n0}   {m.PorCento,9:n1}   {m.MaiorEmpurroes,15}");
}

void Recordistas()
{
    foreach (var m in Medidas.Tabela())
    {
        Console.WriteLine($"{m.Largura} por {m.Altura}, {m.Caixas} caixas: " +
                          $"{m.MaiorEmpurroes} empurroes");
        if (m.Recordista is not null) Console.WriteLine(Medidas.Desenha(m.Recordista));
        Console.WriteLine();
    }
    Console.WriteLine("@ jogador   O caixa   . alvo   * caixa no alvo   + jogador no alvo");
}

void Histograma()
{
    foreach (var m in Medidas.Tabela())
    {
        Console.Write($"{m.Largura}x{m.Altura} com {m.Caixas}: ");
        foreach (var (empurroes, quantos) in m.Histograma.OrderBy(x => x.Key))
            Console.Write($"{empurroes}:{quantos:n0}  ");
        Console.WriteLine();
    }
}

void JuizContraBusca()
{
    foreach (var (l, a, k) in new[] { (3, 3, 1), (3, 3, 2), (3, 4, 1), (4, 4, 1) })
    {
        var (niveis, div) = Medidas.JuizContraBusca(l, a, k);
        Console.WriteLine($"{l} por {a} com {k} caixa(s): {niveis:n0} niveis, {div} divergem");
    }
}

void Poda()
{
    Console.WriteLine("tabuleiro   caixas      niveis   poda mata   sem solucao   erros da poda");
    foreach (var m in Medidas.Poda())
        Console.WriteLine($"{m.Largura} por {m.Altura}    {m.Caixas,6} {m.Total,11:n0} " +
                          $"{m.PodaMata,11:n0} {m.SemSolucao,13:n0} {m.ErrosDaPoda,15:n0}");
}

void Cruzada()
{
    Console.WriteLine("tabuleiro   caixas   estados sem poda   com poda   razao   mesma resposta");
    foreach (var (l, a, k, sem, com, razao, igual) in Medidas.Cruzada())
        Console.WriteLine($"{l} por {a}    {k,6} {sem,18:n0} {com,10:n0} {razao,7:n2}   " +
                          $"{(igual ? "sim" : "NAO"),14}");
}
