# %%
"""
===========================================================================
REVISÕES 6 - TESTE DE EXEMPLO 2 - SOLUÇÕES
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
1. [3 valores] Execução à mão
===========================================================================

a) Escreve: 3 4 4
   A chamada é f(y, x) = f(4, 3): dentro do f, x = 4 e y = 3.
       x = g(3)  -> no g: y = 6, return 7   -> o x do f passa a 7
       return 7 - 3 = 4
   O x e o y de fora nunca mudam: 3 e 4. O z recebe 4.
   (há 3 "x" e 3 "y" diferentes: os de fora, os do f e os do g)

b) Escreve: [5] [10, 2, 3] [1, 20, 3, 4]
       a = [1, 2, 3]   a -> L1
       b = a           b -> L1 (a mesma lista)
       c = a + [4]     c -> L2 = [1, 2, 3, 4] (o + cria uma lista NOVA)
       b[0] = 10       altera L1: [10, 2, 3]
       c[1] = 20       altera L2: [1, 20, 3, 4]
       a = [5]         só a seta do a muda: a -> [5]. O b continua em L1.
   (ficheiro 5, secção 18)

c) Resposta: A. 6
       i = 1: j = 1, 2, 3, 4 -> somas pares: 1+1, 1+3      -> 2
       i = 2: j = 2, 3, 4    -> 2+2, 2+4                   -> 2
       i = 3: j = 3, 4       -> 3+3                        -> 1
       i = 4: j = 4          -> 4+4                        -> 1
   Total: 6.
   As outras opções:
       B. 10 -> contou todas as voltas, esquecendo o if
       C. 8  -> fez o j de 1 a 4 em todas as linhas (ignorou o "range(i, 5)")
       D. 4  -> fez o i e o j só até 3, como se fosse range(1, 4) (off-by-one)

Cotação sugerida: 1 valor por alínea. Na a) e na b), 0.5 se só um dos
valores estiver errado.
"""






# %%
"""
===========================================================================
2. [3 valores] Função booleana sobre listas
===========================================================================
"""

def is_sorted(l: list[int]) -> bool:
    """ Check if the list is in increasing order (equal neighbours allowed). """
    for i in range(len(l) - 1):
        if l[i] > l[i + 1]:
            return False
    return True

# Padrão "todos?" (ficheiro 4, Ex 14.7): procurar um par de vizinhos FORA de ordem.
# Se encontrar um, já se sabe a resposta: False.
# Só depois de ver todos os pares se pode dizer True.
#
# range(len(l) - 1): comparamos l[i] com l[i + 1], por isso o i para no penúltimo.
# Com [] e [7] o range é vazio: devolve True sem nenhum if extra.
#
# ">" e não ">=": elementos iguais seguidos são permitidos ([1, 2, 2, 5] é True).
#
# Erros comuns:
#   - range(len(l)) -> l[i + 1] dá IndexError no último i;
#   - return True dentro do ciclo (decide logo no primeiro par);
#   - "if l[i] < l[i + 1]: return True" -> só olha para o primeiro par;
#   - ">=" em vez de ">" (rejeita elementos iguais seguidos).
#
# Cotação sugerida:
#   3.0 -> correta
#   2.0 -> correta para listas com 2 ou mais elementos, mas rebenta ou erra com [] ou [7]
#   1.5 -> erro de fronteira (>= em vez de >), resto bem
#   1.0 -> return True/False dentro do ciclo (só olha para o primeiro par)






# %%
"""
===========================================================================
3. [4 valores] Série com um ciclo
===========================================================================
"""

def euler(n: int) -> float:
    """ Sum of the first n terms of the series 1/0! + 1/1! + 1/2! + ...
        Precondition: n >= 0
    """
    total = 0.0
    term = 1.0                  # o 1º termo: 1/0! = 1
    for k in range(n):
        total += term
        term /= k + 1           # 1/(k+1)! = (1/k!) / (k+1)
    return total

# O truque é calcular cada termo a partir do ANTERIOR (como no zeno do
# Teste 1 2025/26 e no Taylor da aula 4):
#   termo k     = 1 / k!
#   termo k + 1 = 1 / (k+1)!  =  (1 / k!) / (k+1)
# Basta dividir o termo atual por (k + 1) para ter o seguinte.
#
# Executar à mão euler(4):
#   k = 0: total = 1.0,    term = 1.0 / 1 = 1.0
#   k = 1: total = 2.0,    term = 1.0 / 2 = 0.5
#   k = 2: total = 2.5,    term = 0.5 / 3 = 0.1666...
#   k = 3: total = 2.6666, term = ...
#
# Alternativa, igualmente boa: guardar o fatorial num acumulador
#     fact = 1
#     for k in range(n):
#         total += 1 / fact
#         fact *= k + 1
#
# Solução que perde pontos ("trabalho desnecessário"):
#     for k in range(n):
#         total += 1 / factorial(k)      # recalcula o fatorial do zero em cada volta
#
# Erros comuns:
#   - começar o 1º termo em 1/1! (esquece o 1/0! = 1): euler(1) daria 1.0
#     por acaso, mas euler(2) daria 1.5;
#   - atualizar o termo ANTES de o somar (salta o primeiro termo);
#   - dividir por k em vez de k + 1 (na 1ª volta, k = 0: ZeroDivisionError).
#
# Cotação sugerida:
#   4.0 -> correta, termo a partir do anterior (ou fatorial acumulado)
#   3.0 -> correta, mas recalcula o fatorial em cada termo
#   2.0 -> ideia certa, com um termo a mais ou a menos
#   1.0 -> acumulador certo, termo errado






# %%
"""
===========================================================================
4. [3 valores] Função que cria uma lista
===========================================================================
"""

def running_max(l: list[int]) -> list[int]:
    """ New list with the partial maximums of l. """
    result = []
    for v in l:
        if len(result) == 0 or v > result[-1]:
            result.append(v)
        else:
            result.append(result[-1])
    return result

# Ideia: o máximo parcial até i é o maior entre o máximo parcial até i-1
# (que é o último elemento já posto no result) e o l[i].
# Não é preciso voltar a percorrer o início da lista em cada posição.
#
# O "len(result) == 0" trata o primeiro elemento (ainda não há máximo anterior).
# O curto-circuito do "or" protege o result[-1]: com o result vazio,
# a parte da direita nem é avaliada (ficheiro 3, Ex 7.5).
#
# Alternativa, igualmente boa (guardar o máximo numa variável):
#     result = []
#     if len(l) == 0:
#         return result
#     biggest = l[0]
#     for v in l:
#         if v > biggest:
#             biggest = v
#         result.append(biggest)
#     return result
#
# Trabalho desnecessário a evitar: para cada i, um ciclo interior que
# procura o máximo de l[0..i] do zero (funciona, mas faz n²/2 comparações).
#
# Erros comuns:
#   - começar o máximo em 0: [-2, -5, -1] daria [0, 0, 0];
#   - alterar a lista l em vez de criar uma nova (l[i] = ...);
#   - l[0] sem verificar a lista vazia -> IndexError com [].
#
# Cotação sugerida:
#   3.0 -> correta, uma só passagem, lista original intacta
#   2.5 -> correta, mas com ciclo interior (recalcula o máximo)
#   2.0 -> correta exceto a lista vazia (IndexError)
#   1.0 -> máximo inicial em 0, ou altera a lista original






# %%
"""
===========================================================================
5. [3 valores] [Difícil] Desenho com caracteres
===========================================================================
"""

def draw_x(c: str, n: int) -> None:
    """ Draw an X with the char c in a square of side n.
        Precondition: len(c) == 1 and n >= 1
    """
    for i in range(n):                          # linha i
        for j in range(n):                      # coluna j
            if j == i or j == n - 1 - i:        # diagonal principal ou secundária
                print(c, end='')
            else:
                print(' ', end='')
        print()

draw_x('*', 5)
draw_x('#', 4)

# Pensar no quadrado como uma grelha de linhas (i) e colunas (j), as duas de 0 a n-1.
#   diagonal principal  (de cima à esquerda para baixo à direita): j == i
#   diagonal secundária (de cima à direita para baixo à esquerda): j == n - 1 - i
# Para cada posição: se está numa das diagonais, escreve c; senão, um espaço.
#
# Para n = 5, linha i = 1: j = 1 (principal) e j = 3 (secundária) -> " * * "
#
# Com n ímpar, as duas diagonais cruzam-se no centro (i = j = n // 2):
# o "or" faz com que o c seja escrito UMA vez. Com n par não se cruzam.
#
# Erros comuns:
#   - j == n - i (sem o -1): a diagonal secundária fica deslocada uma coluna;
#   - não escrever os espaços da direita (só importa no Mooshak, mas o enunciado
#     diz "exatamente n caracteres");
#   - print() dentro do ciclo do j (muda de linha a cada carácter);
#   - tentar calcular espaços "antes" e "entre" (como no pinheiro): funciona,
#     mas é bem mais difícil acertar nas contas, sobretudo na linha do meio.
#
# Cotação sugerida:
#   3.0 -> correta para n par e ímpar
#   2.0 -> só uma das diagonais certa, ou falha só no centro com n ímpar
#   1.0 -> estrutura de ciclos encaixados certa, condições erradas






# %%
"""
===========================================================================
6. [4 valores] Programa completo
===========================================================================
"""

def are_valid(capital: float, rate: float, years: int) -> bool:
    """ Check if the deposit values are valid. """
    return capital > 0 and rate >= 0 and years >= 0

def capital_after(capital: float, rate: float, year: int) -> float:
    """ Capital after some years at a yearly rate (in %), with compound interest.
        Precondition: are_valid(capital, rate, year)
    """
    return capital * (1 + rate / 100) ** year

def print_table(capital: float, rate: float, years: int) -> None:
    """ Print the capital at the end of each year, and the total interest.
        Precondition: are_valid(capital, rate, years)
    """
    for year in range(years + 1):               # do ano 0 ao último, inclusive
        print(f"Ano {year}: {capital_after(capital, rate, year):.2f}")
    interest = capital_after(capital, rate, years) - capital
    print(f"Juros: {interest:.2f}")

def main() -> None:
    capital = float(input("Capital: "))
    rate = float(input("Taxa (%): "))
    years = int(input("Anos: "))
    if not are_valid(capital, rate, years):
        print("Valores inválidos")
    else:
        print_table(capital, rate, years)

main()

# range(years + 1): a tabela começa no ano 0 e inclui o último (off-by-one!).
#
# Duas formas de calcular o capital de cada ano, ambas certas:
#   1. fórmula direta (a de cima): capital * (1 + rate/100) ** year
#   2. acumulador: começar com o capital e multiplicar por (1 + rate/100) em cada volta
#        value = capital
#        for year in range(years + 1):
#            print(f"Ano {year}: {value:.2f}")
#            value *= 1 + rate / 100
#      (cuidado: aqui o print vem ANTES da multiplicação, senão o ano 0 sai errado,
#       e no fim o value já tem um ano a mais: os juros calculam-se com o anterior)
#
# Os juros são o capital final menos o inicial.
#
# Erros comuns:
#   - range(years) -> falta o último ano;
#   - rate em vez de rate / 100 (taxa 4 multiplicava por 5!);
#   - capital * (1 + rate / 100) * year (multiplicar pelo ano em vez de potência:
#     isso seria juro simples, e mesmo assim errado);
#   - validação incompleta (esquecer anos negativos);
#   - escrever 1124.864 em vez de 1124.86 (falta o :.2f).
#
# Cotação sugerida:
#   4.0 -> correto, bem organizado, validação completa, formato exato
#   3.0 -> correto, mas tudo na main
#   2.5 -> organização boa, um erro de fronteira (falta o ano 0 ou o último)
#   1.5 -> lê, valida e escreve, mas a conta dos juros está errada






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

# Pergunta 2
assert is_sorted([1, 2, 2, 5]) and is_sorted([]) and is_sorted([7])
assert not is_sorted([3, 1, 2]) and not is_sorted([1, 2, 3, 0])

# Pergunta 3
assert (euler(0), euler(1), euler(2), euler(3)) == (0.0, 1.0, 2.0, 2.5)
assert abs(euler(4) - 2.6667) < 1e-4
assert abs(euler(20) - 2.718281828459045) < 1e-12

# Pergunta 4
original = [3, 1, 4, 1, 5]
assert running_max(original) == [3, 3, 4, 4, 5] and original == [3, 1, 4, 1, 5]
assert running_max([-2, -5, -1]) == [-2, -2, -1] and running_max([]) == []

# Pergunta 5
assert captured(draw_x, '*', 5) == "*   *\n * * \n  *  \n * * \n*   *\n"
assert captured(draw_x, '#', 4) == "#  #\n ## \n ## \n#  #\n"
assert captured(draw_x, 'o', 1) == "o\n"

# Pergunta 6
assert captured(print_table, 1000.0, 4.0, 3) == (
    "Ano 0: 1000.00\nAno 1: 1040.00\nAno 2: 1081.60\nAno 3: 1124.86\nJuros: 124.86\n")
assert not are_valid(1000.0, 4.0, -1) and not are_valid(0.0, 4.0, 3)
assert not are_valid(1000.0, -1.0, 3) and are_valid(1000.0, 0.0, 0)

print("Todas as soluções passam nos testes.")
