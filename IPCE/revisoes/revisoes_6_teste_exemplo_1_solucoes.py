# %%
"""
===========================================================================
REVISÕES 6 - TESTE DE EXEMPLO 1 - SOLUÇÕES
IPCE 2026/2027
===========================================================================

Só para abrires DEPOIS de fazeres o teste (1h30, sem consulta).

Para cada pergunta:
  - a solução recomendada ("quanto mais simples, melhor");
  - alternativas que também estão certas;
  - erros comuns;
  - critérios de cotação SUGERIDOS para te corrigires.
    (não são os critérios oficiais da cadeira: são uma estimativa
     razoável, para teres uma ideia da tua nota)

A última célula testa todas as soluções automaticamente.
"""






# %%
"""
===========================================================================
1. [3 valores] Escolha múltipla e execução à mão
===========================================================================

a) Resposta: A. 8
   17 // 5 * 2 + 17 % 5
   = 3 * 2 + 2         (// e % têm a mesma prioridade que *, da esquerda para a direita)
   = 6 + 2
   = 8
   As outras opções: 4.4 e 5.4 são de quem usou "/" em vez de "//"
   (17 / 5 = 3.4). O 7 é de quem fez 17 % 5 = 1.

b) Respostas:
   1) CORRETO.    range com passo negativo é válido.
   2) INCORRETO.  O operador é ">=". "=>" não existe em Python.
   3) CORRETO.    Sintaxe válida: quer dizer total = +1 (o "+" é o sinal do 1).
                  Não é erro de sintaxe, é um bug silencioso
                  (quem escreveu queria total += 1). Ficheiro 1, Ex 1.7.
   4) INCORRETO.  Faltam os ":" no fim do cabeçalho.
   5) CORRETO.    Atribuição paralela.
   6) INCORRETO.  Em Python escreve-se "elif", não "else if".

c) Resposta: A. 15
   O ciclo passa por i = 1, 3, 5, 7, 9 (o 10 não entra).
       i = 1: nada (1 % 3 != 0, 1 > 5 é False)
       i = 3: 3 % 3 == 0 -> x = 0 + 3 = 3
       i = 5: nada (5 > 5 é False!)
       i = 7: 7 % 3 != 0, 7 > 5 -> x = 3 * 2 = 6
       i = 9: 9 % 3 == 0 -> x = 6 + 9 = 15  (o if ganha: o elif nem é testado)
   As outras opções são armadilhas:
       B. 12 -> quem testou "i > 5" antes do "i % 3 == 0" (a ordem dos ramos importa)
       C. 6  -> quem esqueceu o i = 9 (off-by-one: achou que o range parava antes)
       D. 30 -> quem aplicou os dois ramos no i = 9 (o elif só corre se o if falhar)

Cotação sugerida: a) 1 valor; b) 1 valor (1/6 por cada caso certo);
c) 1 valor (tudo ou nada).
"""






# %%
"""
===========================================================================
2. [3 valores] Função booleana
===========================================================================
"""

def is_summer(day: int, month: int) -> bool:
    """ Check if the date is in summer (21/6 to 22/9, inclusive). """
    return ((month == 6 and day >= 21)
            or month == 7 or month == 8
            or (month == 9 and day <= 22))

# Alternativa (igualmente boa):
#     return (month == 6 and day >= 21) or month in {7, 8} or (month == 9 and day <= 22)
#
# A ideia: o verão são 4 meses, e só os das pontas (junho e setembro)
# precisam de olhar para o dia. Julho e agosto são verão inteiros.
#
# Os parêntesis à volta da 1ª e da última parte não são obrigatórios
# (o and vem antes do or), mas deixam a intenção clara.
# Os parêntesis exteriores servem só para partir a expressão em várias linhas.
#
# Erros comuns:
#   - "6 <= month <= 9 and 21 <= day <= 22": só aceita os dias 21 e 22!
#   - esquecer julho e agosto, ou usar "day > 21" (o dia 21 fica de fora);
#   - if/return True/return False: funciona, mas perde pontos pela simplicidade.
#
# Cotação sugerida:
#   3.0 -> correta e com um único return (ou equivalente simples)
#   2.0 -> correta, mas com if/else desnecessários
#   1.0 -> ideia certa, com 1 erro de fronteira (> em vez de >=, etc.)
#   0.5 -> só trata parte dos casos (ex: esquece julho e agosto)






# %%
"""
===========================================================================
3. [4 valores] Série com um ciclo
===========================================================================
"""

def basel(k: int) -> float:
    """ Sum of the first k terms of the series 1/1² + 1/2² + 1/3² + ...
        Precondition: k >= 0
    """
    total = 0.0
    for i in range(1, k + 1):
        total += 1 / (i * i)
    return total

# O ciclo vai de 1 a k (inclusive): range(1, k + 1).
# Começar em 0 daria 1 / 0 -> ZeroDivisionError.
# Com k = 0 o range é vazio e devolve 0.0, como pede o exemplo.
#
# Os parêntesis em 1 / (i * i) são essenciais: 1 / i * i dava 1.0 sempre
# (da esquerda para a direita: (1 / i) * i).
# 1 / i ** 2 também está certo (a potência vem antes da divisão).
#
# Erros comuns:
#   - range(k) -> começa no 0 e rebenta (ou faz um termo a menos);
#   - range(1, k) -> off-by-one: um termo a menos;
#   - total = 0 em vez de 0.0: funciona (o / dá float), mas 0.0 diz logo que é real;
#   - return dentro do ciclo.
#
# Cotação sugerida:
#   4.0 -> correta
#   3.0 -> off-by-one no range (um termo a mais ou a menos)
#   2.0 -> erro na fórmula do termo, mas acumulador bem feito
#   1.0 -> estrutura de acumulador com vários erros






# %%
"""
===========================================================================
4. [3 valores] Função sobre listas
===========================================================================
"""

def average(l: list[int]) -> float:
    """ Average of the elements.
        Precondition: len(l) > 0
    """
    total = 0
    for v in l:
        total += v
    return total / len(l)

def count_above_average(l: list[int]) -> int:
    """ How many elements are greater than the average of the list.
        Precondition: len(l) > 0
    """
    avg = average(l)            # calculada UMA vez, fora do ciclo
    count = 0
    for v in l:
        if v > avg:
            count += 1
    return count

# São precisas DUAS passagens pela lista:
# primeiro para saber a média, depois para contar.
# Não dá para contar "à medida que se soma": a média só se conhece no fim.
#
# A função auxiliar average deixa o código mais limpo, e cada função faz uma coisa.
# Também se aceita tudo dentro de count_above_average, com dois ciclos seguidos.
#
# Trabalho desnecessário a evitar:
#     for v in l:
#         if v > average(l):    # recalcula a média em CADA volta!
# Está certo, mas para uma lista de 1000 elementos percorre a lista 1000 vezes.
#
# Erros comuns:
#   - ">=" em vez de ">" ([5, 5, 5] daria 3 em vez de 0);
#   - divisão inteira (//) na média: a média de [1, 2] é 1.5, não 1.
#     Curiosidade: AQUI a contagem até dá sempre igual (com elementos inteiros,
#     "v > 1.5" e "v > 1" apanham os mesmos valores), mas é por coincidência.
#     Uma média é um número real: usa sempre "/";
#   - usar sum(l): o enunciado pede para programar diretamente.
#
# Cotação sugerida:
#   3.0 -> correta, média calculada uma só vez
#   2.5 -> correta, mas recalcula a média dentro do ciclo
#   1.5 -> uma das duas partes (média ou contagem) correta
#   0.5 -> ideia certa, código com vários erros






# %%
"""
===========================================================================
5. [3 valores] [Difícil] Desenho com caracteres
===========================================================================
"""

def draw_segment(x: str, n: int) -> None:
    """ Draw a partial line with length n using the char x.
        Precondition: len(x) == 1 and n >= 0
    """
    for i in range(n):
        print(x, end='')

def draw_diamond_line(c: str, n: int, k: int) -> None:
    """ Draw line k of the diamond (k = 0 is the top line) and change line.
        Precondition: len(c) == 1 and 0 <= k < n
    """
    draw_segment(' ', n - 1 - k)
    draw_segment(c, 2 * k + 1)
    print()

def draw_diamond(c: str, n: int) -> None:
    """ Draw a diamond whose top half has n lines.
        Precondition: len(c) == 1 and n >= 1
    """
    for k in range(n):                  # metade de cima: k = 0, 1, ..., n-1
        draw_diamond_line(c, n, k)
    for k in range(n - 2, -1, -1):      # metade de baixo: k = n-2, ..., 0
        draw_diamond_line(c, n, k)

draw_diamond('*', 4)

# A tabela para n = 4 (k é o "nível" da linha):
#   k | espaços | caracteres
#   0 |    3    |     1
#   1 |    2    |     3
#   2 |    1    |     5
#   3 |    0    |     7
# espaços = n - 1 - k, caracteres = 2 * k + 1 (é a copa do pinheiro do Teste 1 2024/25)
#
# A metade de baixo usa as MESMAS linhas, pela ordem inversa,
# sem a linha do meio (k = n-1): range(n - 2, -1, -1).
# O -1 do FIM do range é para o k = 0 entrar.
#
# A função auxiliar draw_diamond_line evita escrever o mesmo código duas vezes.
#
# Erros comuns:
#   - repetir a linha do meio (range(n - 1, -1, -1));
#   - range(n - 2, 0, -1): falta a última linha (a ponta de baixo);
#   - espaços à DIREITA dos caracteres: não se veem, mas não são pedidos;
#   - esquecer o print() no fim de cada linha (fica tudo numa linha só).
#
# Cotação sugerida:
#   3.0 -> correta
#   2.0 -> metade de cima correta e de baixo com um erro (linha do meio repetida, etc.)
#   1.5 -> metade de cima correta, sem a de baixo
#   0.5 -> estrutura de ciclos certa, contas de espaços/caracteres erradas






# %%
"""
===========================================================================
6. [4 valores] Programa completo
===========================================================================
"""

FREE_MINUTES = 30           # até aqui é grátis
PRICE_PER_HOUR = 1.50       # euros por hora ou fração
MAX_PRICE = 12.00           # valor máximo a pagar
MAX_MINUTES = 1440          # 24 horas
MINUTES_PER_HOUR = 60

def is_valid_minutes(minutes: int) -> bool:
    """ Check if the parking time is within the allowed range. """
    return 0 <= minutes <= MAX_MINUTES

def hours_to_charge(minutes: int) -> int:
    """ Number of hours to charge (each fraction of an hour counts as an hour).
        Precondition: minutes >= 0
    """
    return (minutes + MINUTES_PER_HOUR - 1) // MINUTES_PER_HOUR

def parking_price(minutes: int) -> float:
    """ Price to pay for a parking time.
        Precondition: is_valid_minutes(minutes)
    """
    if minutes <= FREE_MINUTES:
        return 0.0
    price = hours_to_charge(minutes) * PRICE_PER_HOUR
    if price > MAX_PRICE:
        return MAX_PRICE
    return price

def main() -> None:
    minutes = int(input("Minutos: "))
    if not is_valid_minutes(minutes):
        print("Valor inválido")
    else:
        print(f"A pagar: {parking_price(minutes):.2f} euros")

main()

# hours_to_charge: "hora ou fração" é arredondar a divisão PARA CIMA.
#   (minutes + 59) // 60:   60 -> 119 // 60 = 1;   61 -> 120 // 60 = 2
#   Alternativa igualmente boa: math.ceil(minutes / 60) (com import math).
#   Errado: minutes // 60 + 1 -> 60 minutos dava 2 horas!
#
# A ordem no parking_price: primeiro o caso especial (grátis), depois a conta,
# e no fim o limite máximo. O limite também se pode fazer com min(price, MAX_PRICE).
#
# O main valida o input (com a precondição) e escreve com :.2f.
# "A pagar: 3.00 euros" tem de ser IGUAL ao enunciado, letra a letra.
#
# Erros comuns:
#   - minutes // 60 * 1.5 -> esquece a fração (61 minutos dava 1.50);
#   - testar o máximo ANTES de calcular, ou esquecê-lo;
#   - "< 30" em vez de "<= 30" (os 30 minutos certos seriam pagos);
#   - escrever 3.0 em vez de 3.00 (falta o :.2f);
#   - valores mágicos (30, 1.5, 12, 1440) espalhados no código, sem constantes.
#
# Cotação sugerida:
#   4.0 -> correto, bem organizado em funções, com constantes e validação
#   3.0 -> correto, mas tudo dentro da main (ou sem constantes)
#   2.0 -> organização boa, mas erra a "fração de hora" ou o máximo
#   1.0 -> só lê, valida e escreve, com a conta errada






# %%
"""
===========================================================================
TESTES AUTOMÁTICOS DAS SOLUÇÕES
===========================================================================

    Corre esta célula DEPOIS de correres todas as células de cima
    (usa as funções que lá estão definidas). Se não aparecer nenhum erro,
    todas as soluções dão os resultados dos exemplos do enunciado.
    Podes também usá-la para testar AS TUAS funções: copia-as para cima
    desta célula (com os mesmos nomes) e corre.
"""

import io, contextlib

def captured(fn, *args) -> str:
    """ What fn(*args) prints. """
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        fn(*args)
    return buf.getvalue()

# Pergunta 1
assert 17 // 5 * 2 + 17 % 5 == 8
x = 0
for i in range(1, 10, 2):
    if i % 3 == 0:
        x += i
    elif i > 5:
        x = x * 2
assert x == 15

# Pergunta 2
assert is_summer(21, 6) and not is_summer(20, 6)
assert is_summer(15, 8) and is_summer(22, 9) and not is_summer(23, 9)
assert not is_summer(1, 1) and is_summer(1, 7) and not is_summer(31, 5)

# Pergunta 3
assert basel(0) == 0.0 and basel(1) == 1.0 and basel(2) == 1.25
assert abs(basel(3) - 1.3611) < 1e-4

# Pergunta 4
assert count_above_average([1, 2, 3, 4, 10]) == 1
assert count_above_average([5, 5, 5]) == 0
assert count_above_average([1, 2]) == 1
assert count_above_average([7]) == 0

# Pergunta 5
assert captured(draw_diamond, '*', 4) == (
    "   *\n"
    "  ***\n"
    " *****\n"
    "*******\n"
    " *****\n"
    "  ***\n"
    "   *\n")
assert captured(draw_diamond, '#', 1) == "#\n"

# Pergunta 6
assert [f"{parking_price(m):.2f}" for m in (0, 20, 30, 31, 60, 61, 95, 600, 1440)] \
    == ["0.00", "0.00", "0.00", "1.50", "1.50", "3.00", "3.00", "12.00", "12.00"]
assert is_valid_minutes(0) and is_valid_minutes(1440)
assert not is_valid_minutes(-1) and not is_valid_minutes(2000)

print("Todas as soluções passam nos testes.")
