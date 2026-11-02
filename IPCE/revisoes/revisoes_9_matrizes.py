# %%
"""
===========================================================================
REVISÕES 9 - MATRIZES (LISTAS DE LISTAS)
Uma tabela com linhas e colunas: m[i][j].
Como se lê, como se percorre, e como se cria uma matriz nova?
IPCE 2026/2027
===========================================================================


Porquê um ficheiro só para isto?
  Muitos dados são tabelas: as notas de vários alunos em vários testes,
  um tabuleiro de jogo, uma imagem (uma grelha de pontos), uma folha de cálculo.
  Em Python, uma tabela é uma lista de listas: uma lista de LINHAS,
  e cada linha é uma lista de valores.
  As peças já as conheces: listas (ficheiro 4) e for dentro de for (secção 11).
  Aqui juntas as duas, e aprendes a pergunta que decide quase tudo:
  "preciso só dos valores, ou também da posição?"

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 31. Listas de listas: criar e aceder ......................... [~25 min]
       Porquê: m[i] é uma linha, m[i][j] é uma célula.
       Tudo o resto parte daqui.

 32. Percorrer uma matriz ..................................... [~45 min]
       Porquê: somar, contar, procurar, a média de cada linha ou de cada coluna.
       É o que mais se faz com matrizes.

 33. Criar matrizes novas ..................................... [~30 min]
       Porquê: a receita de uma matriz nova (create + preencher),
       e a diferença entre devolver uma matriz nova e alterar a que se recebe.

 34. Desafios ................................................. [~45 min]
       Jogo do galo, campo de minas, quadrado mágico, rodar uma matriz.

  Resumo + soluções dos desafios

  Total: ~2h25. Não é para fazer de uma vez.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Cada célula corre sozinha: as funções de que precisa estão lá dentro.
  - Níveis:
      [EXEMPLO]  resolvido: lê, corre, experimenta outros valores.
      [FAZ]      contigo. Solução na célula a seguir, depois de muito espaço.
      [PENSA]    contigo. A "solução" é só a ideia, sem código.
      [DESAFIO]  mais difícil. Solução no fim do ficheiro.
      [SOZINHA]  sem solução.
  - Nos [DESAFIO], tenta primeiro numa folha de papel, e só depois escreve e corre aqui.
  - Se encravares: email ao professor das práticas com o número do exercício,
    o que já fizeste, onde encravaste e o ficheiro em anexo.

  Já resolvidos na aula 7 (aula_7.py), e por isso não repetidos aqui:
    maximum, multiply, diagonals, transpose, product (ex 48 do guião),
    contar os pares, a soma de cada linha, row_sums, is_symmetric, identity,
    e a matriz esparsa com um dicionário.
  Este ficheiro junta outros exercícios, à volta das mesmas ideias.

  A numeração continua a dos ficheiros anteriores: este começa na secção 31.
"""






# %%
"""
===========================================================================
31. LISTAS DE LISTAS: CRIAR E ACEDER [~25 min]
===========================================================================

    Uma matriz é uma lista de linhas. Cada linha é uma lista.

        grades = [[12, 15,  9, 14],      <- linha 0
                  [ 8, 10, 11,  7],      <- linha 1
                  [17, 16, 18, 19]]      <- linha 2

    Só há 4 coisas a saber:
        grades[i]        a linha i (uma lista normal)
        grades[i][j]     a célula da linha i, coluna j
        len(grades)      o número de linhas
        len(grades[i])   o tamanho da linha i

    Ler da esquerda para a direita: primeiro escolho a linha, depois a coluna.

Ex 31.1 [EXEMPLO] As notas de 3 alunos em 4 testes ---------------------------

    Cada linha é um aluno, cada coluna é um teste.
    Antes de correr, prevê cada print.
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

print(grades[1])                # [8, 10, 11, 7]     as notas do aluno 1
print(grades[2][3])             # 19                 o aluno 2 no teste 3
print(grades[0][2])             # 9                  o aluno 0 no teste 2
print(len(grades))              # 3                  3 alunos (linhas)
print(len(grades[0]))           # 4                  4 testes (o tamanho de uma linha)
print(grades[-1])               # [17, 16, 18, 19]   a última linha
print(grades[-1][-1])           # 19                 a última célula

# Partir em várias linhas é só para se ver a "tabela".
# É o mesmo que: grades = [[12, 15, 9, 14], [8, 10, 11, 7], [17, 16, 18, 19]]






# %%
"""
Ex 31.2 [EXEMPLO] Mudar uma célula, mudar uma linha ---------------------------
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

grades[1][3] = 10               # dois índices: muda UMA célula
print(grades[1])                # [8, 10, 11, 10]

grades[0] = [20, 20, 20, 20]    # um índice: troca a LINHA inteira
print(grades)                   # [[20, 20, 20, 20], [8, 10, 11, 10], [17, 16, 18, 19]]

grades[2] = [5, 6]              # também se pode trocar por uma linha mais curta...
print(grades)                   # [[20, 20, 20, 20], [8, 10, 11, 10], [5, 6]]

# ... e aí deixou de ser uma matriz "a sério": as linhas têm tamanhos diferentes.
# Uma lista de listas pode ter linhas de qualquer tamanho.
# Uma MATRIZ é uma lista de listas em que todas as linhas têm o mesmo tamanho.
# Nesse caso, e só nesse, len(m[0]) é o número de colunas.






# %%
"""
Ex 31.3 [FAZ] Aceder e mudar -------------------------------------------------

    Com a matriz das temperaturas de 3 cidades em 5 dias
    (cada linha é uma cidade, cada coluna é um dia), escreve:

    a) a temperatura da cidade 1 no dia 4
    b) todas as temperaturas da cidade 2
    c) o número de cidades e o número de dias
    d) a última temperatura da última cidade (com índices negativos)
    e) muda a temperatura da cidade 0 no dia 0 para 21, e escreve a cidade 0
"""

temps = [[18, 20, 22, 19, 17],
         [25, 27, 26, 28, 30],
         [10, 12, 11,  9,  8]]

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 31.3

temps = [[18, 20, 22, 19, 17],
         [25, 27, 26, 28, 30],
         [10, 12, 11,  9,  8]]

print(temps[1][4])              # a) 30
print(temps[2])                 # b) [10, 12, 11, 9, 8]
print(len(temps), len(temps[0]))   # c) 3 5
print(temps[-1][-1])            # d) 8
temps[0][0] = 21
print(temps[0])                 # e) [21, 20, 22, 19, 17]

# A ordem é sempre [linha][coluna]: aqui [cidade][dia].
# Trocar a ordem (temps[4][1]) dava IndexError: não há linha 4.






# %%
"""
Ex 31.4 [FAZ] Prever ---------------------------------------------------------

    Sem correr: o que escreve cada print? Algum dá erro? Qual e porquê?
"""

m = [[1, 2, 3],
     [4, 5],
     [6]]

print(len(m))                   # a)
print(len(m[1]))                # b)
print(m[1][1])                  # c)
print(m[2])                     # d)
# print(m[2][1])                # e)  (tirar o # depois de responder)
print(m[0][-1] + m[-1][0])      # f)
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 31.4

# a) 3          (3 linhas)
# b) 2          (a linha 1 é [4, 5])
# c) 5
# d) [6]        (uma linha com um só valor: continua a ser uma lista)
# e) IndexError: a linha 2 só tem a coluna 0
# f) 9          (m[0][-1] é 3, m[-1][0] é 6)
#
# Esta m é uma lista de listas, mas não é uma matriz:
# as linhas têm tamanhos diferentes (3, 2 e 1).






# %%
"""
===========================================================================
32. PERCORRER UMA MATRIZ [~45 min]
===========================================================================

    Para passar por todas as células, um for dentro de outro for:
    o de fora passa pelas linhas, o de dentro pelos valores de cada linha.
    Há duas formas, as mesmas das listas (secção 14):

        Pelos VALORES                     Pelos ÍNDICES
        for row in m:                     for i in range(len(m)):
            for v in row:                     for j in range(len(m[i])):
                ... v ...                         ... m[i][j] ...

    Repara no len(m[i]) do for de dentro: o tamanho da linha i.
    (funciona sempre, mesmo que as linhas tenham tamanhos diferentes)

Ex 32.1 [EXEMPLO] Escrever a matriz, linha a linha ---------------------------
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

# Pelos valores
for row in grades:
    for v in row:
        print(v, end=" ")
    print()                     # muda de linha no fim de cada linha da matriz
# 12 15 9 14
# 8 10 11 7
# 17 16 18 19

# Pelos índices (o mesmo resultado)
for i in range(len(grades)):
    for j in range(len(grades[i])):
        print(grades[i][j], end=" ")
    print()

# Com alinhamento (teórica 3b: {v:3} ocupa 3 espaços, encostado à direita)
for row in grades:
    for v in row:
        print(f"{v:3}", end="")
    print()
#  12 15  9 14
#   8 10 11  7
#  17 16 18 19

# É o desenho da aula 6, agora com os valores da matriz:
# o print(..., end="") dentro do for de dentro, e o print() depois dele.






# %%
"""
Ex 32.2 [EXEMPLO] Somar e contar tudo ----------------------------------------
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

# A soma de todas as notas
total = 0
for row in grades:
    for v in row:
        total += v
print(total)                    # 156

# Quantas notas positivas (>= 9.5)
count = 0
for row in grades:
    for v in row:
        if v >= 9.5:
            count += 1
print(count)                    # 9

# A média de todas as notas
n_grades = 0
for row in grades:
    n_grades += len(row)        # quantas notas há em cada linha
print(total / n_grades)         # 13.0

# É o acumulador das listas (secção 14), com um for a mais.
# O total = 0 fica ANTES dos dois for: é uma conta da matriz toda.






# %%
"""
Ex 32.3 [FAZ] Contas na matriz toda ------------------------------------------

    Com a matriz m de baixo:
    a) quantos valores negativos há
    b) qual o maior valor (cuidado: são todos negativos numa das linhas)
    c) a soma dos valores pares
"""

m = [[ 3, -2,  7],
     [-5, -1, -8],
     [ 4,  0,  6]]

def count_negatives(m: list[list[int]]) -> int:
    """ How many negative values in m. """
    pass        # TODO

def largest(m: list[list[int]]) -> int:
    """ Largest value in m.
        Precondition: m has at least one value, and m[0] is not empty
    """
    pass        # TODO

def sum_even(m: list[list[int]]) -> int:
    """ Sum of the even values in m. """
    pass        # TODO

print(count_negatives(m), largest(m), sum_even(m))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 32.3

m = [[ 3, -2,  7],
     [-5, -1, -8],
     [ 4,  0,  6]]

def count_negatives(m: list[list[int]]) -> int:
    """ How many negative values in m. """
    count = 0
    for row in m:
        for v in row:
            if v < 0:
                count += 1
    return count

def largest(m: list[list[int]]) -> int:
    """ Largest value in m.
        Precondition: m has at least one value, and m[0] is not empty
    """
    best = m[0][0]              # começar com um valor que existe mesmo
    for row in m:
        for v in row:
            if v > best:
                best = v
    return best

def sum_even(m: list[list[int]]) -> int:
    """ Sum of the even values in m. """
    total = 0
    for row in m:
        for v in row:
            if v % 2 == 0:
                total += v
    return total

print(count_negatives(m), largest(m), sum_even(m))   # 4 7 0

# Só precisámos de LER os valores: pelos valores chega, e lê-se melhor.
# sum_even: -2 + (-8) + 4 + 0 + 6 = 0. Sim, dá mesmo 0.
# largest: começar com 0 dava certo AQUI (há um 7), mas numa matriz
# só com negativos devolvia 0, que nem lá está. m[0][0] é sempre seguro.






# %%
"""
A RECEITA DAS MATRIZES ---------------------------------------------------------

    Até aqui, contas sobre a matriz TODA. Agora vêm contas por linha,
    por coluna, e com posições. Antes de escrever, três perguntas:

        1. VALORES ou ÍNDICES?
               só preciso de ler os valores        -> for row in m / for v in row
               preciso da posição (i, j), ou de
               escrever numa matriz                -> for i ... / for j ...

        2. ONDE fica cada coisa? (pensar no resultado)
               uma conta da matriz toda     -> inicializar ANTES dos dois for
               uma conta por linha          -> inicializar DENTRO do for de fora,
                                               antes do for de dentro;
                                               usar/guardar DEPOIS do for de dentro
               uma conta por coluna         -> trocar os for: j por fora, i por dentro

        3. CONFIRMAR, à mão, com uma matriz pequena (2x2 chega):
               a inicialização recomeça onde deve?
               as linhas podem ter tamanhos diferentes? (len(m[i]))
               e com uma matriz vazia, ou com uma só linha?

Ex 32.4 [EXEMPLO] A média de cada aluno (uma conta por linha) ----------------

    row_averages(grades) == [12.5, 9.0, 17.5]
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

# Pergunta 1: só preciso de ler as notas -> pelos valores.
# Pergunta 2: uma média POR LINHA:
#               o total recomeça em cada linha  -> total = 0 dentro do for de fora
#               a média só existe no fim da linha -> append depois do for de dentro
#             e o resultado é uma lista da matriz toda -> averages = [] antes de tudo

def row_averages(m: list[list[int]]) -> list[float]:
    """ The average of each row of m.
        Precondition: no row is empty
    """
    averages = []                       # resultado da matriz toda: antes de tudo
    for row in m:
        total = 0                       # conta por linha: recomeça aqui
        for v in row:
            total += v
        averages.append(total / len(row))   # a linha acabou: guardar
    return averages

print(row_averages(grades))     # [12.5, 9.0, 17.5]

# Pergunta 3: confirmar à mão com [[1, 3], [10, 20]]:
#   linha [1, 3]:   total 0 -> 1 -> 4,   média 2.0
#   linha [10, 20]: total 0 -> 10 -> 30, média 15.0   -> [2.0, 15.0]. Certo.
#
#   E se o "total = 0" estivesse antes do for de fora?
#   A 2ª linha começava com o total da 1ª (4) e dava (4 + 30) / 2 = 17.0. Errado.
#   E se o append estivesse dentro do for de dentro?
#   Guardava uma "média" a cada valor, e a lista ficava com 4 números. Errado.
#
#   Precondição: uma linha vazia dava divisão por zero (len(row) = 0).






# %%
"""
Ex 32.5 [EXEMPLO] A média de cada teste (uma conta por coluna) -----------------

    column_averages(grades) == [12.33..., 13.66..., 12.66..., 13.33...]
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

# Pergunta 1: os valores de uma COLUNA estão em linhas diferentes:
#             preciso de grades[i][j] -> pelos índices.
# Pergunta 2: uma conta POR COLUNA -> o for das colunas (j) passa para FORA,
#             e o das linhas (i) para dentro.

def column_averages(m: list[list[int]]) -> list[float]:
    """ The average of each column of m.
        Precondition: m is a matrix (all rows with the same length), not empty
    """
    n_rows = len(m)
    n_cols = len(m[0])                  # só porque todas as linhas têm o mesmo tamanho
    averages = []
    for j in range(n_cols):             # cada coluna...
        total = 0
        for i in range(n_rows):         # ... desce pelas linhas
            total += m[i][j]
        averages.append(total / n_rows)
    return averages

for avg in column_averages(grades):
    print(f"{avg:.1f}", end=" ")
print()                         # 12.3 13.7 12.7 13.3

# Pergunta 3: confirmar à mão com [[1, 3], [10, 20]]:
#   coluna 0: 1 + 10 = 11, média 5.5;  coluna 1: 3 + 20 = 23, média 11.5. Certo.
#
#   Aqui usa-se len(m[0]) como número de colunas, e não len(m[i]).
#   Só é seguro porque a precondição diz que é uma matriz "a sério".
#   Com linhas de tamanhos diferentes, uma coluna podia não existir numa linha
#   (IndexError). Somar por linhas funciona sempre; por colunas, só em matrizes.






# %%
"""
Ex 32.6 [EXEMPLO] Onde está o maior? (a posição) ------------------------------

    position_of_max(grades) == (2, 3)     (o 19: aluno 2, teste 3)
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

# Pergunta 1: quero a POSIÇÃO -> pelos índices.
# Pergunta 2: uma conta da matriz toda -> best_i, best_j antes dos dois for.

def position_of_max(m: list[list[int]]) -> tuple[int, int]:
    """ (row, column) of the largest value of m (the first, if repeated).
        Precondition: m is not empty and m[0] is not empty
    """
    best_i = 0
    best_j = 0
    for i in range(len(m)):
        for j in range(len(m[i])):
            if m[i][j] > m[best_i][best_j]:
                best_i = i
                best_j = j
    return (best_i, best_j)

print(position_of_max(grades))              # (2, 3)
i, j = position_of_max(grades)              # desempacotar o par (teórica 6b)
print(f"Melhor nota: {grades[i][j]}, aluno {i}, teste {j}")
# Melhor nota: 19, aluno 2, teste 3

# Guarda-se a POSIÇÃO do melhor até agora, não o valor:
# o valor está sempre em m[best_i][best_j].
#
# Pergunta 3: com repetidos, [[5, 9], [9, 1]]: o > (e não >=) fica com o
# PRIMEIRO 9, em (0, 1). Com >= ficava com o último, em (1, 0).






# %%
"""
Ex 32.7 [FAZ] Por linha e por coluna -----------------------------------------

    Com as notas de 3 alunos em 4 testes:
    a) row_minimums(m): a nota mais baixa de cada aluno
           row_minimums(grades) == [9, 7, 16]
    b) passed_per_test(m): quantos alunos tiveram positiva (>= 9.5) em cada teste
           passed_per_test(grades) == [2, 3, 2, 2]
    c) best_student(m): o índice do aluno com a melhor média
           best_student(grades) == 2

    Para cada uma, responde primeiro às 3 perguntas da receita.
"""

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

def row_minimums(m: list[list[int]]) -> list[int]:
    """ The smallest value of each row of m.
        Precondition: no row is empty
    """
    pass        # TODO

def passed_per_test(m: list[list[int]]) -> list[int]:
    """ For each column of m, how many values are >= 9.5.
        Precondition: m is a matrix, not empty
    """
    pass        # TODO

def best_student(m: list[list[int]]) -> int:
    """ Index of the row with the highest average (the first, if repeated).
        Precondition: m is not empty, no row is empty
    """
    pass        # TODO

print(row_minimums(grades), passed_per_test(grades), best_student(grades))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 32.7

grades = [[12, 15,  9, 14],
          [ 8, 10, 11,  7],
          [17, 16, 18, 19]]

# a) valores; por linha: o "mínimo até agora" recomeça em cada linha
def row_minimums(m: list[list[int]]) -> list[int]:
    """ The smallest value of each row of m.
        Precondition: no row is empty
    """
    minimums = []
    for row in m:
        smallest = row[0]               # recomeça em cada linha
        for v in row:
            if v < smallest:
                smallest = v
        minimums.append(smallest)       # depois do for de dentro
    return minimums

# b) índices; por coluna: j por fora, i por dentro
def passed_per_test(m: list[list[int]]) -> list[int]:
    """ For each column of m, how many values are >= 9.5.
        Precondition: m is a matrix, not empty
    """
    counts = []
    for j in range(len(m[0])):
        count = 0
        for i in range(len(m)):
            if m[i][j] >= 9.5:
                count += 1
        counts.append(count)
    return counts

# c) a média de cada linha já está feita (Ex 32.4): reaproveitar!
def row_averages(m: list[list[int]]) -> list[float]:
    """ The average of each row of m.
        Precondition: no row is empty
    """
    averages = []
    for row in m:
        total = 0
        for v in row:
            total += v
        averages.append(total / len(row))
    return averages

def best_student(m: list[list[int]]) -> int:
    """ Index of the row with the highest average (the first, if repeated).
        Precondition: m is not empty, no row is empty
    """
    averages = row_averages(m)
    best = 0
    for i in range(len(averages)):
        if averages[i] > averages[best]:
            best = i
    return best

print(row_minimums(grades), passed_per_test(grades), best_student(grades))
# [9, 7, 16] [2, 3, 2, 2] 2

# A c) mostra a melhor ideia deste ficheiro: partir em peças.
# Com a row_averages pronta, o best_student é só o "índice do maior" de uma lista
# (secção 14), sem for dentro de for.






# %%
"""
Ex 32.8 [EXEMPLO] Procurar e parar logo ----------------------------------------

    first_negative(m): a posição do primeiro negativo (linha a linha),
    ou (-1, -1) se não houver.
        first_negative([[3, 1], [4, -2], [-5, 0]]) == (1, 1)
"""

def first_negative(m: list[list[int]]) -> tuple[int, int]:
    """ Position of the first negative value of m, or (-1, -1). """
    for i in range(len(m)):
        for j in range(len(m[i])):
            if m[i][j] < 0:
                return (i, j)           # encontrado: sai dos DOIS for de uma vez
    return (-1, -1)                     # os for acabaram sem encontrar

print(first_negative([[3, 1], [4, -2], [-5, 0]]))     # (1, 1)
print(first_negative([[3, 1], [4, 2]]))               # (-1, -1)

# O return sai da função toda, e por isso dos dois ciclos ao mesmo tempo.
# (um break só sairia do for de dentro: o de fora continuava)
#
# for ou while (ficheiro 8)? A matriz está bem definida -> for.
# Com while também dava (dois while e uma condição "ainda não encontrei"),
# mas com mais linhas e mais sítios para errar.






# %%
"""
Ex 32.9 [PENSA] Valores ou índices? Linha, coluna ou tudo? --------------------

    Para cada problema, responde às perguntas 1 e 2 da receita
    (sem escrever o código).

    a) A soma de cada coluna.
    b) Quantas vezes aparece o valor 0 na matriz.
    c) Escrever a matriz com cada valor multiplicado por 2 (sem a mudar).
    d) Mudar todos os negativos da matriz para 0 (na própria matriz).
    e) A linha com a maior soma (o índice).
    f) As posições de todos os 0, numa lista de pares (i, j).
"""

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 32.9 (sem código):
#
#   a) índices (a coluna j está em linhas diferentes);
#      por coluna: for j por fora, total = 0 dentro dele, append depois do for de dentro.
#   b) valores; da matriz toda: count = 0 antes dos dois for.
#   c) valores (só se lê e escreve no ecrã); print(v * 2, end=" ") e print() por linha.
#   d) ÍNDICES: para ESCREVER na matriz é preciso m[i][j] = 0.
#      (com "for v in row: v = 0" a matriz não muda: o v é só uma cópia do valor)
#   e) por linha (a soma) e depois da matriz toda (o melhor): como o Ex 32.7 c),
#      de preferência em duas funções.
#   f) índices (quero as posições); da matriz toda: zeros = [] antes de tudo,
#      zeros.append((i, j)) dentro do if.






# %%
"""
===========================================================================
33. CRIAR MATRIZES NOVAS [~30 min]
===========================================================================

    Para fazer uma matriz NOVA com nr linhas e nc colunas, usa-se a create
    do guião (teórica 6a):

        def create(nr, nc, value):
            m = []
            for _ in range(nr):
                m.append([value] * nc)     # uma linha NOVA em cada volta
            return m

    e depois preenche-se posição a posição: m[i][j] = ...

    A receita de uma matriz nova:
        1. o tamanho do resultado: quantas linhas? quantas colunas?
        2. create com esse tamanho
        3. for i, for j: m2[i][j] = ...   (pelos índices: vou ESCREVER)
        4. return

Ex 33.1 [EXEMPLO] A tabuada numa matriz ----------------------------------------

    times_table(3) == [[1, 2, 3],
                       [2, 4, 6],
                       [3, 6, 9]]
"""

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def times_table(n: int) -> list[list[int]]:
    """ The n x n multiplication table. """
    t = create(n, n, 0)                     # 1 e 2: n linhas, n colunas
    for i in range(n):                      # 3: preencher
        for j in range(n):
            t[i][j] = (i + 1) * (j + 1)     # as linhas e colunas começam no 1
    return t                                # 4

for row in times_table(3):
    print(row)
# [1, 2, 3]
# [2, 4, 6]
# [3, 6, 9]

# O (i + 1) e o (j + 1): os índices começam em 0, a tabuada em 1.
#
# Porque não [[0] * 3] * 3? Porque cria UMA linha e 3 setas para ela
# (ficheiro 5, secção 18): mudar uma célula mudava as 3 linhas.
# A create faz uma lista nova em cada volta. Por isso é que existe.






# %%
"""
Ex 33.2 [FAZ] Matrizes novas ---------------------------------------------------

    a) checkers(n): um tabuleiro n x n com 1 e 0 alternados, a começar com 1.
           checkers(3) == [[1, 0, 1], [0, 1, 0], [1, 0, 1]]
    b) add(m1, m2): a soma de duas matrizes do mesmo tamanho, posição a posição.
           add([[1, 2], [3, 4]], [[10, 20], [30, 40]]) == [[11, 22], [33, 44]]
    c) distances(n): a matriz n x n com |i - j| em cada posição (abs(i - j)).
           distances(3) == [[0, 1, 2], [1, 0, 1], [2, 1, 0]]

    Segue a receita: tamanho, create, for i / for j, return.
"""

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def checkers(n: int) -> list[list[int]]:
    """ n x n board with alternating 1 and 0, starting with 1. """
    pass        # TODO

def add(m1: list[list[int]], m2: list[list[int]]) -> list[list[int]]:
    """ New matrix with m1[i][j] + m2[i][j] in each position.
        Precondition: m1 and m2 are matrices with the same size
    """
    pass        # TODO

def distances(n: int) -> list[list[int]]:
    """ n x n matrix with |i - j| in each position. """
    pass        # TODO

print(checkers(3), add([[1, 2], [3, 4]], [[10, 20], [30, 40]]), distances(3))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 33.2

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def checkers(n: int) -> list[list[int]]:
    """ n x n board with alternating 1 and 0, starting with 1. """
    board = create(n, n, 0)
    for i in range(n):
        for j in range(n):
            if (i + j) % 2 == 0:
                board[i][j] = 1
    return board

def add(m1: list[list[int]], m2: list[list[int]]) -> list[list[int]]:
    """ New matrix with m1[i][j] + m2[i][j] in each position.
        Precondition: m1 and m2 are matrices with the same size
    """
    nr = len(m1)
    nc = len(m1[0])
    result = create(nr, nc, 0)
    for i in range(nr):
        for j in range(nc):
            result[i][j] = m1[i][j] + m2[i][j]
    return result

def distances(n: int) -> list[list[int]]:
    """ n x n matrix with |i - j| in each position. """
    d = create(n, n, 0)
    for i in range(n):
        for j in range(n):
            d[i][j] = abs(i - j)
    return d

print(checkers(3))              # [[1, 0, 1], [0, 1, 0], [1, 0, 1]]
print(add([[1, 2], [3, 4]], [[10, 20], [30, 40]]))   # [[11, 22], [33, 44]]
print(distances(3))             # [[0, 1, 2], [1, 0, 1], [2, 1, 0]]

# O tabuleiro: o (i + j) % 2 é o xadrez do CONSOLIDAR da aula 6, agora numa matriz.
# A distances: na diagonal (i == j) dá 0, e cresce para os lados.
#
# add: com len(m1[0]) no lugar de nc, não há problema em matrizes vazias?
# Há: add([], []) dá IndexError no m1[0]. Com a ncols do guião
# (que devolve 0 para a matriz vazia) já não. Mais um caso para confirmar à mão.






# %%
"""
Ex 33.3 [EXEMPLO] Devolver uma matriz nova, ou alterar a recebida? (teórica 6a) --

    Duas maneiras de "somar 1 a todas as células".
    Olha para o tipo do resultado no cabeçalho.
"""

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def inc(m: list[list[int]]) -> list[list[int]]:
    """ NEW matrix with all the values of m plus 1. m does not change. """
    m2 = create(len(m), len(m[0]), 0)
    for i in range(len(m)):
        for j in range(len(m[i])):
            m2[i][j] = m[i][j] + 1
    return m2

def inc2(m: list[list[int]]) -> None:
    """ Adds 1 to all the values of m. m CHANGES. """
    for i in range(len(m)):
        for j in range(len(m[i])):
            m[i][j] += 1

a = [[1, 2], [3, 4]]
b = inc(a)
print(a, b)                     # [[1, 2], [3, 4]] [[2, 3], [4, 5]]   a não mudou

inc2(a)
print(a)                        # [[2, 3], [4, 5]]   a mudou, e não há resultado

# O cabeçalho avisa o que a função faz:
#   -> list[list[int]]   devolve uma matriz NOVA; a recebida fica igual
#   -> None              não devolve nada; MUDA a matriz que recebe
#
# Porque é que a inc2 consegue mudar o a de fora? Porque a função recebe
# uma seta para a MESMA matriz (ficheiro 5, secção 19).
#
# Erro clássico: escrever b = inc2(a). O b fica None (a inc2 não devolve nada).






# %%
"""
Ex 33.4 [FAZ] As duas maneiras -------------------------------------------------

    Com o mesmo efeito, "pôr a 0 todos os valores negativos":
    a) clamp(m) devolve uma matriz NOVA, e a m fica igual
    b) clamp_in_place(m) muda a própria m, e não devolve nada

        m = [[3, -1], [-4, 2]]
        clamp(m) == [[3, 0], [0, 2]]     e a m continua [[3, -1], [-4, 2]]
        clamp_in_place(m)                e a m passa a [[3, 0], [0, 2]]
"""

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def clamp(m: list[list[int]]) -> list[list[int]]:
    """ New matrix like m, with the negative values replaced by 0.
        Precondition: m is a matrix, not empty
    """
    pass        # TODO

def clamp_in_place(m: list[list[int]]) -> None:
    """ Replace the negative values of m by 0. """
    pass        # TODO

m = [[3, -1], [-4, 2]]
print(clamp(m), m)
clamp_in_place(m)
print(m)
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 33.4

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def clamp(m: list[list[int]]) -> list[list[int]]:
    """ New matrix like m, with the negative values replaced by 0.
        Precondition: m is a matrix, not empty
    """
    m2 = create(len(m), len(m[0]), 0)
    for i in range(len(m)):
        for j in range(len(m[i])):
            if m[i][j] > 0:
                m2[i][j] = m[i][j]      # os negativos ficam com o 0 do create
    return m2

def clamp_in_place(m: list[list[int]]) -> None:
    """ Replace the negative values of m by 0. """
    for i in range(len(m)):
        for j in range(len(m[i])):
            if m[i][j] < 0:
                m[i][j] = 0

m = [[3, -1], [-4, 2]]
print(clamp(m), m)              # [[3, 0], [0, 2]] [[3, -1], [-4, 2]]
clamp_in_place(m)
print(m)                        # [[3, 0], [0, 2]]

# Repara: as duas precisam dos ÍNDICES (escrevem numa matriz).
# Com "for row in m: for v in row: if v < 0: v = 0" nada mudava:
# o v é só uma cópia do número, não a célula da matriz.






# %%
"""
===========================================================================
34. DESAFIOS [~45 min]
===========================================================================

    Mais perto do nível dos testes. Soluções no fim do ficheiro.
    Antes de escrever: as 3 perguntas da receita (valores ou índices?
    onde fica cada coisa? confirmar com uma matriz pequena).
    E partir em funções: uma por cada pergunta "pequena".

Ex 34.1 [DESAFIO] Quem ganhou o jogo do galo? ---------------------------------

    O tabuleiro é uma matriz 3x3 de 'X', 'O' e '.' (vazio).
    winner(board) devolve 'X' ou 'O' se esse jogador tiver 3 seguidos
    (numa linha, numa coluna ou numa diagonal), ou '.' se ninguém tiver.

        winner([['X', 'O', 'O'],
                ['.', 'X', 'O'],
                ['.', '.', 'X']]) == 'X'       (diagonal)
        winner([['X', 'O', 'X'],
                ['.', 'O', '.'],
                ['X', 'O', '.']]) == 'O'       (coluna 1)
        winner([['X', 'O', 'X'],
                ['X', 'O', 'O'],
                ['O', 'X', 'X']]) == '.'       (empate)

    Pista: uma função three_equal(a, b, c) que diz se os 3 são iguais e não '.'.
"""

def winner(board: list[list[str]]) -> str:
    """ 'X' or 'O' if that player has 3 in a row, '.' otherwise. """
    pass        # TODO

print(winner([['X', 'O', 'O'], ['.', 'X', 'O'], ['.', '.', 'X']]))






# %%
"""
Ex 34.2 [DESAFIO] Campo de minas -----------------------------------------------

    Um campo é uma matriz de '*' (mina) e '.' (vazio).
    a) count_mines(field, i, j): quantas minas há nas 8 casas à volta de (i, j)
       (sem contar a própria; cuidado com as bordas!)
    b) hints(field): uma matriz NOVA de strings, com '*' nas minas
       e, nas outras casas, o número de minas à volta (como string).

        field = [['*', '.', '.'],
                 ['.', '.', '*'],
                 ['.', '.', '.']]

        count_mines(field, 1, 1) == 2
        count_mines(field, 2, 0) == 0

        hints(field) == [['*', '2', '1'],
                         ['1', '2', '*'],
                         ['0', '1', '1']]

    Pista: as 8 vizinhas de (i, j) são (i + di, j + dj), com di e dj de -1 a 1,
    menos (0, 0). Uma posição (r, c) existe se 0 <= r < len(field)
    e 0 <= c < len(field[r]).
"""

def count_mines(field: list[list[str]], i: int, j: int) -> int:
    """ Number of mines in the (up to 8) positions around (i, j). """
    pass        # TODO

def hints(field: list[list[str]]) -> list[list[str]]:
    """ New matrix with '*' on mines and the number of neighbour mines elsewhere. """
    pass        # TODO

field = [['*', '.', '.'],
         ['.', '.', '*'],
         ['.', '.', '.']]
print(count_mines(field, 1, 1), hints(field))






# %%
"""
Ex 34.3 [DESAFIO] Quadrado mágico ----------------------------------------------

    Uma matriz quadrada é um quadrado mágico se todas as linhas, todas as
    colunas e as duas diagonais tiverem a mesma soma.

        is_magic([[2, 7, 6],
                  [9, 5, 1],
                  [4, 3, 8]]) == True        (tudo soma 15)
        is_magic([[1, 2],
                  [3, 4]]) == False

    Pista: calcula a soma da linha 0 (o "alvo") e procura um contra-exemplo
    (uma linha, coluna ou diagonal com outra soma).
"""

def is_magic(m: list[list[int]]) -> bool:
    """ Is the square matrix m a magic square?
        Precondition: m is square, not empty
    """
    pass        # TODO

print(is_magic([[2, 7, 6], [9, 5, 1], [4, 3, 8]]), is_magic([[1, 2], [3, 4]]))






# %%
"""
Ex 34.4 [DESAFIO] Rodar uma matriz 90 graus ------------------------------------

    rotate(m): uma matriz NOVA, que é a m rodada 90 graus no sentido dos ponteiros
    do relógio. Uma matriz 2x3 dá uma 3x2.

        rotate([[1, 2, 3],
                [4, 5, 6]]) == [[4, 1],
                                [5, 2],
                                [6, 3]]

    Pista: segue a receita da matriz nova. Primeiro o tamanho do resultado.
    Depois descobre, à mão, para onde vai cada (i, j): o 1 está em (0, 0) e
    vai para (0, 1); o 4 está em (1, 0) e vai para (0, 0); o 3 está em (0, 2)
    e vai para (2, 1)...
"""

def rotate(m: list[list[int]]) -> list[list[int]]:
    """ New matrix: m rotated 90 degrees clockwise.
        Precondition: m is a matrix, not empty
    """
    pass        # TODO

print(rotate([[1, 2, 3], [4, 5, 6]]))






# %%
"""
Ex 34.5 [SOZINHA] O triângulo de Pascal ------------------------------------------

    Uma lista de listas em que as linhas têm tamanhos diferentes (de propósito):
    cada linha começa e acaba com 1, e cada valor do meio é a soma dos dois
    que estão por cima.

        pascal(5) == [[1],
                      [1, 1],
                      [1, 2, 1],
                      [1, 3, 3, 1],
                      [1, 4, 6, 4, 1]]

    Pista: cada linha nova constrói-se a partir da anterior (a última de cima).
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  ACEDER
  [ ] m[i] é a linha i; m[i][j] a célula; primeiro a linha, depois a coluna
  [ ] len(m) linhas; len(m[i]) o tamanho da linha i
  [ ] matriz = todas as linhas do mesmo tamanho; só aí len(m[0]) é o nº de colunas
  [ ] m[i][j] = v muda uma célula; m[i] = [...] troca a linha toda

  PERCORRER (a receita)
  [ ] 1. valores (for row / for v) ou índices (for i / for j in range(len(m[i])))?
         índices quando preciso da posição, ou de ESCREVER na matriz
  [ ] 2. onde fica cada coisa?
         conta da matriz toda  -> inicializar antes dos dois for
         conta por linha       -> inicializar dentro do for de fora;
                                  guardar depois do for de dentro
         conta por coluna      -> j por fora, i por dentro (só em matrizes)
  [ ] 3. confirmar à mão com uma matriz 2x2, e pensar na vazia
  [ ] procurar e parar: return dentro dos dois for (sai dos dois)

  MATRIZES NOVAS
  [ ] tamanho do resultado -> create -> for i / for j: m2[i][j] = ... -> return
  [ ] nunca [[0] * n] * n (uma só linha, partilhada)
  [ ] -> matriz: devolve uma nova e não mexe na recebida
      -> None: muda a recebida e não devolve nada
  [ ] "for v in row: v = 0" não muda a matriz: é preciso m[i][j] = 0

  E SEMPRE: partir em funções pequenas, e reaproveitá-las.

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_10_strings.py (strings e carateres).
"""






# %%
"""
===========================================================================
SOLUÇÕES DOS DESAFIOS
===========================================================================

    Só para veres DEPOIS de tentares.
"""
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 34.1

EMPTY = '.'

def three_equal(a: str, b: str, c: str) -> bool:
    """ Are a, b and c the same player (not empty)? """
    return a != EMPTY and a == b and b == c

def winner(board: list[list[str]]) -> str:
    """ 'X' or 'O' if that player has 3 in a row, '.' otherwise. """
    for i in range(3):                                          # as 3 linhas
        if three_equal(board[i][0], board[i][1], board[i][2]):
            return board[i][0]
    for j in range(3):                                          # as 3 colunas
        if three_equal(board[0][j], board[1][j], board[2][j]):
            return board[0][j]
    if three_equal(board[0][0], board[1][1], board[2][2]):      # diagonal a descer
        return board[0][0]
    if three_equal(board[0][2], board[1][1], board[2][0]):      # diagonal a subir
        return board[0][2]
    return EMPTY

print(winner([['X', 'O', 'O'], ['.', 'X', 'O'], ['.', '.', 'X']]))   # X
print(winner([['X', 'O', 'X'], ['.', 'O', '.'], ['X', 'O', '.']]))   # O
print(winner([['X', 'O', 'X'], ['X', 'O', 'O'], ['O', 'X', 'X']]))   # .

# A three_equal faz a pergunta pequena ("estes 3 são do mesmo jogador?"),
# e a winner só tem de a fazer para as 8 linhas possíveis.
# O "a != EMPTY" é o pormenor: 3 casas vazias seguidas não são uma vitória.
# As diagonais: (0,0) (1,1) (2,2) e (0,2) (1,1) (2,0), as da aula 6 (j == i e j == 2 - i).






# %%
# Solução do Ex 34.2

MINE = '*'

def count_mines(field: list[list[str]], i: int, j: int) -> int:
    """ Number of mines in the (up to 8) positions around (i, j). """
    count = 0
    for di in range(-1, 2):                 # -1, 0, 1
        for dj in range(-1, 2):
            r = i + di
            c = j + dj
            inside = 0 <= r < len(field) and 0 <= c < len(field[r])
            if (di, dj) != (0, 0) and inside and field[r][c] == MINE:
                count += 1
    return count

def hints(field: list[list[str]]) -> list[list[str]]:
    """ New matrix with '*' on mines and the number of neighbour mines elsewhere. """
    result = []
    for i in range(len(field)):
        row = []
        for j in range(len(field[i])):
            if field[i][j] == MINE:
                row.append(MINE)
            else:
                row.append(str(count_mines(field, i, j)))
        result.append(row)
    return result

field = [['*', '.', '.'],
         ['.', '.', '*'],
         ['.', '.', '.']]
print(count_mines(field, 1, 1), count_mines(field, 2, 0))   # 2 0
for row in hints(field):
    print(row)
# ['*', '2', '1']
# ['1', '2', '*']
# ['0', '1', '1']

# O "inside" é a parte difícil: nas bordas, algumas vizinhas não existem.
# A ordem dentro do if importa: o field[r][c] só é lido se "inside" for True
# (o and pára no primeiro False, teórica 3b). Senão, r = 3 dava IndexError.
#
# E porque não basta "r < len(field)"? Porque r = -1 NÃO dá erro:
# lia a ÚLTIMA linha (índices negativos!) e contava minas do outro lado
# do campo, sem aviso nenhum. Por isso o "0 <= r" (e o "0 <= c") também são precisos.
#
# A hints constrói as linhas com append (como a input_int_matrix do guião),
# em vez de create: também é uma matriz nova, linha a linha.






# %%
# Solução do Ex 34.3

def is_magic(m: list[list[int]]) -> bool:
    """ Is the square matrix m a magic square?
        Precondition: m is square, not empty
    """
    n = len(m)
    target = 0
    for v in m[0]:                          # a soma da linha 0 é o alvo
        target += v
    for i in range(n):                      # as linhas
        total = 0
        for j in range(n):
            total += m[i][j]
        if total != target:
            return False
    for j in range(n):                      # as colunas
        total = 0
        for i in range(n):
            total += m[i][j]
        if total != target:
            return False
    main_diag = 0
    other_diag = 0
    for i in range(n):                      # as duas diagonais
        main_diag += m[i][i]
        other_diag += m[i][n - 1 - i]
    return main_diag == target and other_diag == target

print(is_magic([[2, 7, 6], [9, 5, 1], [4, 3, 8]]), is_magic([[1, 2], [3, 4]]))   # True False

# É a pesquisa do contra-exemplo (aula 5): ao primeiro que falha, return False.
# Só depois de tudo verificado, True.
# Três contas diferentes (linhas, colunas, diagonais): podiam ser três funções,
# e o is_magic ficava com 4 linhas. Experimenta!






# %%
# Solução do Ex 34.4

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ New matrix with nr rows and nc columns, all with value.
        Precondition: nr >= 0 and nc >= 0
    """
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m

def rotate(m: list[list[int]]) -> list[list[int]]:
    """ New matrix: m rotated 90 degrees clockwise.
        Precondition: m is a matrix, not empty
    """
    nr = len(m)
    nc = len(m[0])
    r = create(nc, nr, 0)                   # o tamanho troca: 2x3 -> 3x2
    for i in range(nr):
        for j in range(nc):
            r[j][nr - 1 - i] = m[i][j]
    return r

print(rotate([[1, 2, 3], [4, 5, 6]]))      # [[4, 1], [5, 2], [6, 3]]
s
# Para descobrir a fórmula, a tabela à mão (nr = 2):
#   valor  de (i, j)  para
#   1      (0, 0)     (0, 1)
#   2      (0, 1)     (1, 1)
#   3      (0, 2)     (2, 1)
#   4      (1, 0)     (0, 0)
#   5      (1, 1)     (1, 0)
#   6      (1, 2)     (2, 0)
# A nova linha é o j. A nova coluna é 1 - i, ou seja nr - 1 - i.
# É a transposta (aula 7) com as colunas ao contrário.s