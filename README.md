# Sokoban mínimo

Sokoban é o jogo das caixas: você empurra, nunca puxa, e uma caixa encostada
num canto não sai mais de lá. Normalmente alguém desenha os níveis à mão.

Aqui não há nível desenhado. Este repositório varre **todos os níveis que
cabem** num retângulo aberto — sem parede nenhuma por dentro, só a borda — com
um número fixo de caixas, alvos e uma casa para o jogador. São **302.640
níveis**, cada um resolvido por busca em largura, com um juiz independente
conferindo quem tem solução.

## Como rodar

```bash
dotnet test testes/Sokoban.Testes/Sokoban.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tabela
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- recordistas
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- histograma
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- juiz
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- poda
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- cruzada
```

## Os dois caminhos

- **O juiz** faz busca em largura contando **movimentos** — andar e empurrar
  valem um cada — sobre o estado `(posição exata do jogador, caixas)`. Não
  normaliza nada, não conhece canto morto.
- **A busca** conta **empurrões**. Entre dois empurrões o jogador só caminha, e
  para onde ele caminha não muda nada: o que importa é de que lado da caixa ele
  consegue chegar. O estado vira `(região alcançável, caixas)`.

Os números que eles dão são diferentes de propósito. O que tem de bater é
**quem tem solução** — e bateu em 15.144 níveis, com zero divergências.

## A varredura

```
tabuleiro   caixas      niveis   com solucao   por cento   maior empurroes
3 por 3          1         648           200        30,9                 2
3 por 3          2       9.072         1.216        13,4                 3
3 por 3          3      42.336         1.608         3,8                 3
3 por 4          1       1.584           550        34,7                 3
3 por 4          2      43.560         7.652        17,6                 5
4 por 4          1       3.840         1.500        39,1                 4
4 por 4          2     201.600        43.644        21,6                 6
```

## O que isto mediu

**Quase todo nível possível é impossível.** Num 3 por 3 com três caixas, só
**3,8%** dos 42.336 níveis têm solução. A fração cai quando se põe mais caixa
no mesmo tabuleiro e sobe quando se dá mais espaço às mesmas caixas: 30,9% no
3 por 3 com uma caixa, 39,1% no 4 por 4 com uma.

**A regra do canto morto decide metade dos níveis sem solução de graça, e
nunca errou.** No 4 por 4 com duas caixas ela mata 80.556 dos 157.956 sem
solução — 51,0% — só olhando se alguma caixa já nasceu num canto que não é
alvo. Nos 302.640 níveis dos sete tamanhos, **zero** vezes ela matou um nível
que tinha solução.

**A poda rende mais no tabuleiro apertado, não no grande.**

```
tabuleiro   caixas   estados sem poda   com poda   razao
3 por 3          1              1.416        696    2,03
3 por 3          2             37.904      9.130    4,15
3 por 3          3            132.004     27.687    4,77
3 por 4          1              4.818      2.926    1,65
3 por 4          2            403.708    138.910    2,91
4 por 4          1             16.080     11.310    1,42
```

Quatro vírgula oito vezes num 3 por 3 com três caixas; **uma vírgula quatro**
num 4 por 4 com uma. O que manda é a densidade de caixas, não o tamanho do
tabuleiro, e a resposta final é idêntica em todas as seis linhas.

**O nível mais difícil que cabe num 4 por 4 com duas caixas precisa de 6
empurrões, e só 644 dos 43.644 chegam lá.**

```
|@O .|
| O  |
|    |
|   .|
```

O histograma inteiro é unimodal, e o pico fica bem antes do máximo: 1.680
níveis com zero empurrões, 5.712 com um, 10.800 com dois, **12.568 com três**,
8.880, 3.360, e 644 com seis.

## O que as medidas me corrigiram

**Escrevi um teste esperando que o jogador ficasse preso do lado errado da
caixa, e ele dá a volta.** Jogador à direita da caixa, alvo à direita: não dá
para empurrar daquele lado. Mas o retângulo é aberto, então ele contorna por
cima em quatro passos e empurra. Num tabuleiro sem parede interna isso vale
sempre, e eu tinha escrito a intuição de Sokoban de verdade, onde há paredes.

**Exigi subida estrita no histograma e um dos sete empatou no topo.** O 4 por 4
com uma caixa tem 480 níveis com um empurrão e 480 com dois, exatamente iguais.
É o único patamar dos sete tamanhos, e agora ele está preso num teste próprio,
com os dois números, em vez de ter virado um `>=` silencioso.

**Apostei que a poda renderia mais no tabuleiro maior.** Mais espaço, mais
canto ruim, mais corte — era o raciocínio. A tabela cruzada diz o contrário:
1,42 vezes no maior tabuleiro com uma caixa, 4,77 no menor com três. Medir as
duas pontas no mesmo lugar é o que mostra isso; duas medidas soltas teriam me
deixado escrever a frase errada.

**A varredura inteira rodava duas vezes.** Uma para a tabela principal e outra
só para contar quantos níveis a poda mata. As duas passavam pelos mesmos
302.640 níveis. Juntadas numa passada só, e a conta da poda saiu de graça.

**A tabela cruzada deixa o 4 por 4 com duas caixas de fora, e diz isso.** São
201.600 níveis que teriam de ser varridos duas vezes, com e sem poda, só para
encher uma linha. A linha que não foi medida não entra como se tivesse sido.

## Os arquivos

```
src/Sokoban/Nucleo.cs      o nivel em mascara de bits, e as quatro direcoes
src/Sokoban/Juiz.cs        largura por MOVIMENTOS, sem normalizar nada
src/Sokoban/Busca.cs       largura por EMPURROES, com a regiao canonica
src/Sokoban/Varredura.cs   todos os niveis de cada tamanho
src/Sokoban/Medidas.cs     as tabelas deste README
ferramentas/Medidor/       o executavel que imprime as tabelas e os niveis
testes/                    43 testes, com os numeros daqui presos um a um
```

43 testes.
