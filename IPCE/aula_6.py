# %%
"""
===========================================================================
GUIA DE SESSÃO - Prática 06 - IPCE 2026/2027
===========================================================================

Cobre os guiões 06a (ex. 34 a 39) e 06b (ex. 40 a 44).

Site da Cadeira: https://ipce-184ea7.gitlab.io/
Este Guião na drive: https://drive.google.com/drive/folders/1twlkB4M0yg0tBxOt2c1diqJU6xtUsoay?usp=sharing
»» Guardem estes links nos favoritos no browser para fácil acesso no futuro

Tema de hoje: DESENHOS e CARATERES.
    1ª parte: desenhar figuras com asteriscos.
              Cada desenho é um for dentro de outro for:
              um for para as LINHAS, outro para o que vai em cada linha.
    2ª parte: cada caráter tem um NÚMERO escondido (ord e chr).
              Com isso dá para percorrer o alfabeto e até cifrar mensagens.

Como usar este ficheiro no Spyder:
    - cada bloco que começa com "# %%" é uma célula
    - Ctrl + Enter  -> corre só a célula onde está o cursor
    - Shift + Enter -> corre a célula e salta para a seguinte
    - F5            -> corre o ficheiro todo (os programas pedem input!)

Mooshak de hoje: N (ex 35), O (ex 38), P (ex 42), Q (ex 44)
"""






# %%
"""
===========================================================================
LOGÍSTICA / Dúvidas [8 min -> 10:18]
===========================================================================

    - Folha de Presenças

    - Baixar este guia da drive e abrir no Spyder

    - Dúvidas da aula passada (listas, o interpretador do ex 33)
"""






# %%
"""
===========================================================================
REVISÃO - pelos valores ou pelos índices? [7 min -> 10:25]
===========================================================================

    A mesma lista, percorrida das duas formas, lado a lado.
    Antes de correr, prevejam cada print.
"""

grades = [12, 15, 9, 18]

# 1. Somar: só preciso dos VALORES
total = 0
for v in grades:
    total += v
print(total)                    

# 2. Somar outra vez, agora pelos ÍNDICES: dá o mesmo
total = 0
for i in range(len(grades)):
    total += grades[i]
print(total)                    

# %%
# 3. Print "posição" + "valor" »» preciso dos DOIS -> logo uso os índices
for i in range(len(grades)):
    print(f"{i}: {grades[i]}")  # 0: 12   1: 15   2: 9   3: 18

# %%
# 4. Aumentar todas as notas em 1: vou ALTERAR a lista -> índices
for i in range(len(grades)):
    grades[i] = grades[i] + 1
print(grades)                   # [13, 16, 10, 19]

# A regra (aula 5):
#   só USAR os valores (somar, contar, procurar)        -> for v in l
#   ALTERAR a lista, ou precisar da posição / do vizinho -> for i in range(len(l))
#
# Hoje o range vai ter outro trabalho: contar LINHAS e COLUNAS de um desenho.






# %%
"""
===========================================================================
Guião 06a, exercícios 34 a 37 - Desenhos 2D [55 min -> 11:20]
https://ipce-184ea7.gitlab.io/
===========================================================================



a) Duas formas novas de usar o print [4 min -> 10:29] ---------------------

    Corram e vejam onde muda a linha.
"""

print("ola", end="")
print("ole")                    # olaole   (o 1º print não mudou de linha)

print("A", end=" ")
print("B")                      # A B

print()                         # uma linha vazia: o print() sozinho só muda de linha




# %%
# Mini: 5 asteriscos na MESMA linha, com um for
for i in range(5):
    print("*", end="")
print()                         # *****   e agora sim, muda de linha

# O print escreve o que lhe damos e, no fim, muda de linha.
# Com end="" diz-se: "no fim, não escrevas nada". A linha continua aberta.
# O print() sozinho escreve só a mudança de linha.
#
# Por isso, para desenhar uma linha com um for:
#   dentro do for: print(..., end="")   -> os carateres ficam lado a lado
#   depois do for: print()              -> fecha a linha


# %%
# 5 asteriscos na MESMA linha, com um simples print e uma string repetida
print("*" * 5)                    # *****




# %%
"""
b) EXERCICIO 34 (GUIÃO) - QUADRADO (n == rows == cols) ------------------------------------------

    Desenhar um quadrado de lado n (aqui n = 4):
        ****
        ****
        ****
        ****

b.1) Forma 1: repetir uma string [4 min -> 10:33]
"""

n = 4
for i in range(n):              # uma volta por LINHA
    print("*" * n)                # cada linha é uma string com n asteriscos

# "*" * n é uma string com n asteriscos.
# (o próprio guião usa isto no ex 42: '-' * len(name))
# Para o quadrado, chega: n linhas iguais.
#
# Mas hoje o objetivo é treinar o for.
# E há desenhos em que cada caráter depende de onde está
# (o ex 39, ou um X): aí o "*" * n já não resolve.
# Por isso, a seguir: o mesmo quadrado, caráter a caráter.






# %%
"""
b.2) Forma 2: um for dentro de outro for [6 min -> 10:39]
"""

n = 4
for i in range(n):              # para cada LINHA i ...
    for j in range(n):          # ... escreve as n COLUNAS j dessa linha
        print("*", end="")
    print()                     # acabou a linha i: muda de linha

# Ciclos ENCAIXADOS: para cada volta do for de fora,
# o for de dentro dá TODAS as suas voltas.
#
#   i = 0:  j = 0, 1, 2, 3   -> ****   e muda de linha
#   i = 1:  j = 0, 1, 2, 3   -> ****   e muda de linha
#   ...
#
# São n * n = 16 asteriscos no total.
#
# Atenção à indentação do print():
#   está DENTRO do for de fora, mas FORA do for de dentro.
#   Corre uma vez por linha, depois de a linha estar escrita.






# %%
"""
b.3) Forma 3: cada parte com o seu nome - uma solução mais limpa  [8 min -> 10:47]
"""

def draw_line(c: str, n: int) -> None:
    """ Draw a line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()

    # ou
    # print(c * n)                 # uma linha cheia de c, e muda de linha

def draw_square(c: str, n: int) -> None:
    """ Draw a square with side n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        draw_line(c, n)

def main() -> None:
    n = int(input("Introduza o tamanho do lado: "))
    if not (n >= 0):
        print("Argumento inválido")
    else:
        draw_square('*', n)

main()

# É a forma 2, partida em duas funções:
#   o for de dentro passou a ser a draw_line  (desenha UMA linha);
#   o for de fora passou a ser a draw_square  (repete a linha n vezes).
#
# Porquê partir?
#   - cada função faz uma coisa e tem um nome que diz qual;
#   - a draw_line vai servir para os próximos desenhos todos.
#
# Porquê o parâmetro c, e não "*" fixo dentro da draw_line?
# Não dá mais trabalho, e a função fica pronta para outros carateres:
#   draw_square('#', 3)
#
# Funções que só desenham não devolvem nada: o tipo é -> None.
# Chamam-se como uma instrução: draw_square('*', n), sem print à volta.
#
# Lado 0: range(0) é vazio. Não se desenha nada, e está certo.


#%%
"""
    Diagonais 
"""

# Diagonal where i == j: 
def draw_diagonal(c: str, n: int) -> None:
    """ Draw a diagonal with side n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        for j in range(n):
            if j == i:
                print(c, end='')
            else:
                print(' ', end='')
        print()
draw_diagonal('*', 6)


# %%
# Diagonal where i + j == n - 1:
def draw_diagonal_rev(c: str, n: int) -> None:
    """ Draw a diagonal with side n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        for j in range(n):
            if j + i == n - 1:
                print(c, end='')
            else:
                print(' ', end='')
        print()

draw_diagonal_rev('*', 6)






# %%
"""
c) EXERCICIO 35 (GUIÃO, Mooshak N) - Triângulo [8 min -> 10:55] -----------

    Desenhar um triângulo de lado n:
        *
        **
        ***
        ****
        *****
        ******
"""


# Forma compacta
def draw_triangle_compact(c: str, n: int) -> None:
    for i in range(n):
        print(c * (i + 1))

draw_triangle_compact('*', 6)



# %%
# Forma com nested for
def draw_triangle_nested(c: str, n: int) -> None:
    for i in range(n):
        for j in range(i + 1):
            print(c, end='')
        print()

draw_triangle_nested('*', 6)



#%%
# Forma organizada por funções (Programa bem estruturado)
# Reparem como conseguimos "ler" o programa normalmente quando usamos funções:
def draw_line(c: str, n: int) -> None:
    """ Draw a line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()

def draw_triangle(c: str, n: int) -> None:
    """ Draw a upright right triangle with side n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        draw_line(c, i + 1)

    # Outra forma: pôr o range a contar já de 1 a n
    # for size in range(1, n + 1):
    #     draw_line(c, size)

def main() -> None:
    n = int(input("Introduza o tamanho do lado: "))
    if not (n >= 0):
        print("Argumento inválido")
    else:
        draw_triangle('a', n)

main()

# O truque de todos os desenhos: uma TABELA "linha -> quantos".
#
#   linha i:      0  1  2  3  4  5
#   asteriscos:   1  2  3  4  5  6     -> sempre i + 1
#
# Descoberta a fórmula, o código escreve-se sozinho: draw_line(c, i + 1).
#
# Comparado com o quadrado, só mudou o tamanho de cada linha:
# no quadrado é sempre n, aqui depende da linha i.








# %%
"""
d) EXERCICIO 36 (GUIÃO) - Triângulo invertido [7 min -> 11:02] -------------

    Desenhar um triângulo invertido de lado n:
        ******
        *****
        ****
        ***
        **
        *
"""

# Formma compacta
def draw_triangle_rev_compact(c: str, n: int) -> None:
    for i in range(n):
        print(c * (n - i))

draw_triangle_rev_compact('*', 6)


# %%
# Forma com nested for
def draw_triangle_rev_nested(c: str, n: int) -> None:
    for i in range(n):
        for j in range(n - i):
            print(c, end='')
        print()

draw_triangle_rev_nested('*', 6)


#%%
# Forma organizada por funções (Programa bem estruturado)
def draw_line(c: str, n: int) -> None:
    """ Draw a line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end=" ")
    print()
def draw_triangle_rev(c: str, n: int) -> None:
    """ Draw a upside down right triangle with side n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        draw_line(c, n - i)
    # Outra forma: um range que conta para trás, de n até 1
    # for size in range(n, 0, -1):
    #     draw_line(c, size)

def main() -> None:
    n = int(input("Introduza o tamanho do lado: "))
    if not (n >= 0):
        print("Argumento inválido")
    else:
        draw_triangle_rev('*', n)

main()

# A tabela, para n = 6:
#
#   linha i:      0  1  2  3  4  5
#   asteriscos:   6  5  4  3  2  1     -> sempre n - i
#
# Na forma alternativa, range(n, 0, -1) dá n, n-1, ..., 1.
# O FIM (0) não entra, como sempre. Por isso o último é o 1.






# %%
"""
e) Os erros clássicos dos desenhos [6 min -> 11:08] -------------------------

    Três triângulos com um erro cada. Antes de correr: o que desenham?
"""

n = 3

# Erro 1: o print() ficou DENTRO do for de dentro
for i in range(n):
    for j in range(i + 1):
        print("*", end="")
        print()

print("-----")



# %%
# Erro 2: esqueceu-se o print() no fim de cada linha
for i in range(n):
    for j in range(i + 1):
        print("*", end="")
    # print()
print()

print("-----")



# %%
# Erro 3: o for de dentro não depende da linha i
for i in range(n):
    for j in range(n):
        print("*", end="")
    print()

# Erro 1: muda de linha depois de CADA asterisco -> uma coluna de 6 asteriscos.
# Erro 2: nunca muda de linha -> ****** tudo seguido (1 + 2 + 3 = 6).
# Erro 3: todas as linhas têm n -> saiu um quadrado, não um triângulo.
#
# Quando um desenho sai mal, pergunta:
#   - onde está o print()? (indentação)
#   - quantas voltas dá o for de dentro, em cada linha i?







# %%
"""
f) EXERCICIO 37 (GUIÃO) - Histograma [12 min -> 11:20] ----------------------

    Desenhar o histograma de f(x) = x²/6, com x de -lim até lim.
    Cada linha tem round(f(x)) asteriscos.
    Experimentem com lim = 20 (o exemplo do guião) e com lim = 10.
"""

def f(x: float) -> float:
    """ Some real function. """
    return (x * x) / 6


# Forma compacta
def draw_histogram_f_compact(c: str, lim: int) -> None:
    for x in range(-lim, lim + 1):
        print(c * round(f(x)))

draw_histogram_f_compact('*', 10)


# %%
# Forma com nested for
def draw_histogram_f_nested(c: str, lim: int) -> None:
    for x in range(-lim, lim + 1):
        for j in range(round(f(x))):
            print(c, end='')
        print()

draw_histogram_f_nested('*', 10)


# %%
# Forma organizada por funções (Programa bem estruturado)
def draw_line(c: str, n: int) -> None:
    """ Draw a line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()


def draw_histogram_f(c: str, lim: int) -> None:
    """ Draw an histogram for f over [-lim, lim] using the char c.
        Precondition: len(c) == 1 and lim >= 0
    """
    for x in range(-lim, lim + 1):
        draw_line(c, round(f(x)))

def main() -> None:
    lim = int(input("Introduza o valor do limite: "))
    if not (lim >= 0):
        print("Argumento inválido")
    else:
        draw_histogram_f('*', lim)

main()

# O for percorre os x: range(-lim, lim + 1) dá -lim, ..., 0, ..., lim.
# O "+ 1" é para o lim entrar (o fim do range nunca entra).
#
# Cada linha: calcular f(x), arredondar, e desenhar esse número de asteriscos.
# A draw_line é a mesma de sempre: reaproveitar é isto.
#
# Para lim = 20 aparecem 3 linhas vazias a meio: x = -1, 0 e 1.
# f(1) = 1/6 = 0.17, que arredonda para 0. E draw_line(c, 0) só muda de linha.
#
# Se não fosse o round, draw_line(c, 0.17) dava erro:
# o range só aceita inteiros.






# %%
"""
===========================================================================
INTERVALO [20 min -> 11:40]
===========================================================================
"""






# %%
"""
===========================================================================
Guião 06a, exercícios 38 e 39 - Desenhos com várias partes [30 min -> 12:10]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
g) EXERCICIO 38 (GUIÃO, Mooshak O) - Moldura [8 min -> 11:48] ---------------

    Desenhar uma moldura de lado n:
        ******
        *    *
        *    *
        *    *
        *    *
        ******
    Agora há dois tipos de linha: a cheia e a oca.
"""

# %%
# Forma compacta (1)
def draw_frame_compact(c: str, n: int) -> None:
    print(c * n)                   # linha cheia
    for i in range(n - 2):
        print(c + ' ' * (n - 2) + c)  # linha oca
    print(c * n)                   # linha cheia

draw_frame_compact('*', 6)

# %%
# Forma compacta (2)
def draw_frame_compact(c: str, n: int) -> None:
    for i in range(n):
        if i == 0 or i == n - 1:       # linha cheia
            print(c * n)               # linha cheia
        else:                          # linha oca
            print(c + ' ' * (n - 2) + c)

draw_frame_compact('*', 6)


# %%
# Forma com nested for
def draw_frame_nested(c: str, n: int) -> None:
    for i in range(n):
        if i == 0 or i == n - 1:   # linha cheia
            for j in range(n):
                print(c, end='')
        else:                      # linha oca
            print(c, end='')
            for j in range(n - 2):
                print(' ', end='')
            print(c, end='')
        print()                    # muda de linha

draw_frame_nested('*', 6)

"""
    Como podem ver este é um caso em que o código começa realemente a ficar dificil de perceber
    É nestes casos mais complexos que a decomposição em funções ajuda a organizar o código e a perceber o que se passa 
"""


# %%
# Forma organizada por funções (Programa bem estruturado)
def draw_full_line(c: str, n: int) -> None:
    """ Draw a full line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()

def draw_hollow_line(c: str, n: int) -> None:
    """ Draw a hollow line with length n using the char c.
        Precondition: len(c) == 1 and n >= 2
    """
    print(c, end='')
    for i in range(n - 2):
        print(' ', end='')
    print(c)


# Reparem como nós conseguimos "ler" o programa normalmente quando usamos funções:
def draw_frame(c: str, n: int) -> None:
    """ Draw a frame with side n using the char c.
        Precondition: len(c) == 1 and n >= 2
    """
    draw_full_line(c, n)
    for i in range(n - 2):
        draw_hollow_line(c, n)
    draw_full_line(c, n)

draw_frame('*', 6)

# A moldura, de cima para baixo:
#   1 linha cheia, n - 2 linhas ocas, 1 linha cheia.  Total: n linhas.
#
# Cada linha oca, da esquerda para a direita:
#   1 asterisco, n - 2 espaços, 1 asterisco.  Total: n carateres.
#
# Os "- 2" são os dois cantos, que já têm asterisco.
#
# Por agora só funciona para n >= 2 (vejam a precondição).
# O enunciado também permite n = 0 e n = 1. É o próximo passo.






# %%
"""
h) Moldura: os casos pequenos [7 min -> 11:55] -------------------------------

    O enunciado diz: "Permite-se que o tamanho do lado seja zero."
    Antes de correr: o que desenha a versão de cima para n = 1? E para n = 0?
"""

def draw_full_line(c: str, n: int) -> None:
    """ Draw a full line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()

def draw_hollow_line(c: str, n: int) -> None:
    """ Draw a hollow line with length n using the char c.
        Precondition: len(c) == 1 and n >= 2
    """
    print(c, end='')
    for i in range(n - 2):
        print(' ', end='')
    print(c)

def draw_frame(c: str, n: int) -> None:
    """ Draw a frame with side n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    if n == 1:
        print(c)                # um só asterisco
    elif n >= 2:
        draw_full_line(c, n)
        for i in range(n - 2):
            draw_hollow_line(c, n)
        draw_full_line(c, n)
    # n == 0: não se desenha nada
    # podiamo tb desenhar um print() vazio... 

def main() -> None:
    n = int(input("Introduza o tamanho do lado: "))
    if not (n >= 0):
        print("Argumento inválido")
    else:
        draw_frame('*', n)

main()

# Com a versão de cima (sem os if):
#   n = 0: duas linhas cheias de tamanho 0, ou seja, duas linhas vazias.
#          Devia não sair nada.
#   n = 1: linha cheia "*", 0 linhas ocas (range(-1) é vazio), linha cheia "*".
#          Saíam DUAS linhas, mas a moldura de lado 1 é um só asterisco.
#   n == 2: linha cheia "**", 0 linhas ocas, linha cheia "**" -> certo.
#   n == 3: linha cheia "***", 1 linha oca "* *", linha cheia "***" -> certo.
# A regra: antes de dar um programa por acabado, experimentar os casos
# pequenos que o enunciado permite (0, 1, 2...). É aí que se escondem os erros.
# (e é exatamente isso que o Mooshak testa)
#
# draw_frame('*', 2) dá duas linhas "**": está certo (só há cantos).






# %%
"""
i) EXERCICIO 39 (GUIÃO) - Quadrado estranho [15 min -> 12:10] ----------------

    Desenhar, para n = 6:
        *****0
        ****0#
        ***0##
        **0###
        *0####
        0#####
    Agora cada linha tem três segmentos diferentes.
"""

# Forma compacta
def draw_funny_square_compact(a: str, b: str, c: str, n: int) -> None:
    for i in range(n):
        print(a * (n - i - 1) + b + c * i)

draw_funny_square_compact('*', '0', '#', 6)


# %%
# Forma organizada por funções (Programa bem estruturado)
def draw_segment(x: str, n: int) -> None:
    """ Draw a partial line with length n using the char x.
        Precondition: len(x) == 1 and n >= 0
    """
    for i in range(n):
        print(x, end=' ')

def draw_funny_line(a: str, b: str, c: str, n: int, i: int) -> None:
    """ Draw a funny line with length n using the chars a, b, c.
        The position of b in the line is given by i.
        Precondition: len(a) == 1 and len(b) == 1 and len(c) == 1 and n > 0
    """
    draw_segment(a, n - i - 1)
    
    draw_segment(b, 1)
    # print(b, end=' ')
    
    draw_segment(c, i)
    print()

def draw_funny_square(a: str, b: str, c: str, n: int) -> None:
    """ Draw a funny square with side n using the chars a, b, c.
        Precondition: len(a) == 1 and len(b) == 1 and len(c) == 1 and n > 0
    """
    for i in range(n):
        draw_funny_line(a, b, c, n, i)

def main() -> None:
    n = int(input("Introduza o tamanho do lado: "))
    if not (n > 0):
        print("Argumento inválido")
    else:
        draw_funny_square('*', '0', '#', n)

main()

# A tabela, agora com uma coluna por segmento (n = 6):
#
#   linha i:   0  1  2  3  4  5
#   '*':       5  4  3  2  1  0     -> n - i - 1
#   '0':       1  1  1  1  1  1     -> sempre 1
#   '#':       0  1  2  3  4  5     -> i
#
# Confirmação: 5 + 1 + 0 = 6, 4 + 1 + 1 = 6, ... cada linha tem n carateres.
#
# A draw_segment é a draw_line SEM o print() final:
# escreve um bocado da linha e deixa-a aberta para o segmento seguinte.
# Só a draw_funny_line fecha a linha, no fim dos três segmentos.
#
# É a receita dos desenhos difíceis dos testes (o pinheiro de Natal do
# Teste 1 2024/25, o losango, o X): para cada linha, que segmentos tem,
# e quanto mede cada um, em função de i e de n.






# %%
"""
===========================================================================
Guião 06b, exercícios 40 a 44 - Carateres [50 min -> 13:00]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
a) Cada caráter tem um número: ord e chr [6 min -> 12:16] --------------------

    Vejam a tabela gobal do Unicode (ou ASCII) e os códigos dos carateres.
    https://commons.wikimedia.org/wiki/File:ASCII-Table.svg
    
    Antes de correr, tentem adivinhar.
"""

print(ord('!'), ord('0'), ord('9'), ord('A'), ord('Z'), ord('a'), ord('z'))     # 48 57 65 90 97 122
print(chr(33), chr(48), chr(57), chr(65), chr(90), chr(97), chr(122))                  # ! 0 9 A Z a z
print(chr(ord('A') + 1))                # B
print(ord('a') - ord('A'))              # 32
print()



# %%
# Mini 1: que caráter vem 3 depois do 'x'?
print(chr(ord('x') + 3))                # {



# Mini 2: em que posição do alfabeto está o 'D'? (contando o 'A' como 0)
print(ord('D') - ord('A'))              # 3



# Mini 3: comparar carateres
print('A' < 'B')    # True
print('Z' < 'a')    # True
print('b' < 'B')    # False

# Cada caráter tem um código numérico (o Unicode; para as letras simples é o ASCII).
#   ord(c) -> o número do caráter c
#   chr(n) -> o caráter com o número n
# São inversas: chr(ord(c)) == c.
#
# As maiúsculas estão todas seguidas (A=65 ... Z=90),
# as minúsculas também (a=97 ... z=122), e os algarismos também.
# Por isso "a letra seguinte" é só somar 1 ao código.
#
# Comparar carateres é comparar os códigos:
# todas as maiúsculas vêm antes das minúsculas ('Z' < 'a').
#
# Em Python não há um tipo "caráter": um caráter é uma string de tamanho 1.






# %%
"""
b) EXERCICIO 40 (GUIÃO) - O caráter seguinte [5 min -> 12:21] ----------------

        Introduza um caráter: α
        Seguinte = β
"""

def is_char(c: str) -> bool:
    """ Check if a string is a single char. """
    return len(c) == 1

def next_char(c: str) -> str:
    """ Generate the following char.
        Precondition: is_char(c)
    """
    return chr(ord(c) + 1)

def main() -> None:
    c = input("Introduza um caráter: ")
    if not is_char(c):
        print("Caráter inválido")
    else:
        print(f"Seguinte = {next_char(c)}")

main()

# Caráter -> número (ord), somar 1, número -> caráter (chr).
#
# Funciona com qualquer alfabeto: 'a' -> 'b', 'α' -> 'β', '7' -> '8'.
# E 'z' -> '{' : o caráter a seguir ao 'z' na tabela não é uma letra.
#
# Se o utilizador escrever "ab" (2 carateres), o ord rebenta.
# Para validar, era a receita de sempre na main:
#     if not is_char(c): print("Caráter inválido")






# %%
"""
c) EXERCICIO 41 (GUIÃO) - Os carateres de um intervalo [6 min -> 12:27] -------

    Escrever numa linha todos os carateres de start a end,
    cada um com um espaço antes:
         ! " # $ % ... x y z { | } ~
"""

def is_char(c: str) -> bool:
    """ Check if a string is a single char. """
    return len(c) == 1

def print_chars(start: str, end: str) -> None:
    """ Print a range of contiguous characters.
        Precondition: is_char(start) and is_char(end)
    """
    if not is_char(start) or not is_char(end):
        print("Argumentos inválidos")
    else:
        for code in range(ord(start), ord(end) + 1):
            print(' ' + chr(code), end='')
        print()

def main() -> None:
    print_chars('!', '~')

main()

# O range não sabe contar letras, mas sabe contar os CÓDIGOS:
#   range(ord(start), ord(end) + 1) dá os códigos todos, do primeiro ao último.
# O "+ 1" é outra vez para o último entrar.
#
# Em cada volta: transformar o código em caráter (chr) e escrevê-lo.
# É o desenho da draw_line, com um caráter diferente em cada posição.
#
# Experimentem: print_chars('a', 'z'), print_chars('α', 'ω').






# %%
"""
d) EXERCICIO 43 (GUIÃO) - Construir uma string [5 min -> 12:32] --------------

    (o 43 vem antes do 42: é mais simples)
    Em vez de escrever os carateres, juntá-los numa string e devolvê-la.
"""

def is_char(c: str) -> bool:
    """ Check if a string is a single char. """
    return len(c) == 1

def make_str_range(start: str, end: str) -> str:
    """ Sequence of contiguous chars in a range.
        Precondition: is_char(start) and is_char(end)
    """
    if not is_char(start) or not is_char(end):
        print("Argumentos inválidos")
        return ""
    else:
        s = ""        
        for code in range(ord(start), ord(end) + 1):
            s += chr(code)
        return s

def main() -> None:
    s = make_str_range(' ', '~')
    print(s)

main()

print()
print(make_str_range('a', 'e'))         # abcde

# É o acumulador de sempre, com strings:
#   começar com a string vazia "" (o "zero" das strings),
#   e em cada volta juntar mais um caráter com +=.
#
# Diferença para o ex 41: o 41 ESCREVE (-> None), o 43 DEVOLVE (-> str).
# Uma função que devolve a string serve para mais coisas:
# escrevê-la, guardá-la, compará-la, passá-la a outra função.






# %%
"""
e) EXERCICIO 44 (GUIÃO, Mooshak Q) - ENCRYPT - Mensagem secreta [12 min -> 12:44] -------

    Dada uma string, encriptá-la da seguinte forma:  
        Avançar cada maiúscula sem acento para a letra seguinte ('Z' roda para 'A').
        Tudo o resto fica igual.
        Exemplo:

            ZZZ ATACAMOS AO AMANHECER ZZZ
            AAA BUBDBNPT BP BNBOIFDFS AAA
"""

def is_char(c: str) -> bool:
    """ Check if a string is a single char. """
    return len(c) == 1

def is_upper(c: str) -> bool:
    """ Is c a letter in upper case (without accent)? """
    return is_char(c) and 'A' <= c and c <= 'Z'

def next_char(c: str) -> str:
    """ Generate the following char.
        Precondition: is_char(c)
    """
    return chr(ord(c) + 1)

def next_upper(c: str) -> str:
    """ Generate the next upper case letter; but 'Z' rotates to 'A'.
        Precondition: is_upper(c)
    """
    if c == 'Z':
        return 'A'
    return next_char(c)

def translate(s: str) -> str:
    """ Advance all the upper case letters of s; 'Z' rotates to 'A'. """
    s2 = ""
    for c in s:
        if is_upper(c):
            s2 += next_upper(c)
        else:
            s2 += c
    return s2

def main() -> None:
    s = input(": ")
    print(translate(s))

main()

# Uma função por cada ideia, e cada uma usa a anterior:
#   is_upper    -> "é uma maiúscula?"        ('A' <= c <= 'Z': os códigos estão seguidos)
#   next_char   -> o ex 40, reaproveitado
#   next_upper  -> o caso especial do 'Z', e o resto é o next_char
#   translate   -> percorrer a mensagem e construir a nova (o acumulador do ex 43)
#
# Testar cada uma sozinha, antes da main:
#   next_upper('A') -> 'B'    next_upper('Z') -> 'A'
#   translate("OLA, ZE!") -> "PMB, AF!"
#
# for c in s percorre a string caráter a caráter (pelos valores: só precisamos de os ler).
# A string s não muda (as strings são imutáveis): construímos uma NOVA, a s2.
#
# É uma versão simples da cifra de César (Júlio César usava 3 letras de avanço).






# %%
"""
f) EXERCICIO 42 (GUIÃO, Mooshak P) - Tabela de carateres [14 min -> 12:58] ----

    Escrever os carateres de start a end numa tabela com ncols colunas,
    com um cabeçalho e um rodapé:

        TESTE: 4
        -----
        grego (25 carateres)
         α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ ς σ τ υ φ χ ψ ω
        -----
"""

def is_char(c: str) -> bool:
    """ Check if a string is a single char. """
    return len(c) == 1

def header(name: str, start: str, end: str) -> None:
    """ Header of the table.
        Precondition: is_char(start) and is_char(end)
    """
    how_many = ord(end) - ord(start) + 1
    print('-' * len(name))
    print(f"{name} ({how_many} carateres)")

def body(ncols: int, start: str, end: str) -> None:
    """ Body of the table.
        Precondition: ncols > 0 and is_char(start) and is_char(end)
    """
    count = 0                               # quantos carateres já escrevi
    for code in range(ord(start), ord(end) + 1):
        print(' ' + chr(code), end='')
        count += 1
        if count % ncols == 0:              # a linha encheu: muda de linha
            print()
    if count % ncols != 0:                  # a última linha ficou a meio
        print()

def foot(name: str) -> None:
    """ Foot of the table. """
    print('-' * len(name))

def print_char_table(name: str, ncols: int, start: str, end: str) -> None:
    """ Print a table of chars.
        Precondition: ncols > 0 and is_char(start) and is_char(end)
    """
    header(name, start, end)
    body(ncols, start, end)
    foot(name)

def main() -> None:
    s = input("TESTE: ")
    if s == "1":
        print_char_table("ascii", 30, ' ', '~')
    elif s == "2":
        print_char_table("hiragana", 30, 'ぁ', 'ゖ')
    elif s == "3":
        print_char_table("katakana", 30, 'ァ', 'ヿ')
    elif s == "4":
        print_char_table("grego", 30, 'α', 'ω')
    elif s == "5":
        print_char_table("chineses", 35, '一', '龥')
    elif s == "6":
        print_char_table("emoji", 30, '😀', '😫')
    else:
        print("Wrong input")

main()

# O header e o foot já vinham feitos. O trabalho todo está no body.
#
# É o ex 41 com uma novidade: mudar de linha de ncols em ncols.
# Um contador diz quantos carateres já foram escritos;
# quando é múltiplo de ncols (count % ncols == 0), a linha encheu.
#
# O pormenor do fim:
#   se o total de carateres for múltiplo de ncols, a última linha
#   já mudou de linha dentro do for, e não pode haver outro print();
#   senão, a última linha ficou a meio e falta fechá-la.
# Experimentem print_char_table("abc", 3, 'a', 'f') (6 carateres, 2 linhas certinhas).
#
# Nota sobre o TESTE 1: neste guia usamos ' ' (espaço) até '~', que são 95
# carateres, como diz o exemplo do guião. Se começar em '!' são só 94.








# %%
"""
===========================================================================
CONSOLIDAR - para casa
===========================================================================

    Três exercícios para treinar o que fizemos hoje, um pouco mais à frente.
    Tentem primeiro sem olhar para a solução.
"""






# %%
"""
EXERCICIO (EXTRA) 1 - As duas diagonais ------------------------------------

    Num quadrado de lado n, cada posição tem uma linha i e uma coluna j
    (as duas de 0 a n - 1). Escrevam duas funções, com dois for encaixados
    e um if que decide, em cada posição (i, j), se escreve c ou um espaço:

    a) draw_diagonal_down(c, n): a diagonal que desce para a direita.
    b) draw_diagonal_up(c, n):   a diagonal que sobe para a direita.

        draw_diagonal_down('*', 4)      draw_diagonal_up('*', 4)
        *                                  *
         *                                *
          *                              *
           *                            *

    (cada linha tem n carateres, contando os espaços)
    Pista: para cada desenho, façam a tabela das posições (i, j) com asterisco.
"""

def draw_diagonal_down(c: str, n: int) -> None:
    """ Draw the diagonal from the top left to the bottom right corner.
        Precondition: len(c) == 1 and n >= 0
    """
    pass        # TODO

def draw_diagonal_up(c: str, n: int) -> None:
    """ Draw the diagonal from the bottom left to the top right corner.
        Precondition: len(c) == 1 and n >= 0
    """
    pass        # TODO

draw_diagonal_down('*', 4)
draw_diagonal_up('*', 4)

# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: as duas diagonais

def draw_diagonal_down(c: str, n: int) -> None:
    """ Draw the diagonal from the top left to the bottom right corner.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        for j in range(n):
            if j == i:
                print(c, end='')
            else:
                print(' ', end='')
        print()

def draw_diagonal_up(c: str, n: int) -> None:
    """ Draw the diagonal from the bottom left to the top right corner.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        for j in range(n):
            if j == n - 1 - i:
                print(c, end='')
            else:
                print(' ', end='')
        print()

draw_diagonal_down('*', 4)
draw_diagonal_up('*', 4)

# As tabelas, para n = 4:
#   a descer: (0,0) (1,1) (2,2) (3,3)   -> a coluna é igual à linha: j == i
#   a subir:  (0,3) (1,2) (2,1) (3,0)   -> linha + coluna dá sempre 3: j == n - 1 - i
#
# Aqui o "*" * n já não chega: em cada posição é preciso DECIDIR o que escrever.
# É para isto que serve o for dentro do for com um if lá dentro.
#
# E as duas diagonais ao mesmo tempo? if j == i or j == n - 1 - i
# É o X da pergunta 5 do teste de exemplo 2 (revisoes_6_teste_exemplo_2.py).






# %%
"""
EXERCICIO (EXTRA) 2 - Tabuleiro de xadrez ------------------------------------

    draw_checkers(n): um tabuleiro n x n, com '#' e '.' alternados,
    a começar com '#' no canto de cima à esquerda.

        draw_checkers(5)
        #.#.#
        .#.#.
        #.#.#
        .#.#.
        #.#.#

    Pista: olhem para i + j nas posições com '#'.
"""

def draw_checkers(n: int) -> None:
    """ Draw an n x n checkerboard with '#' and '.'.
        Precondition: n >= 0
    """
    pass        # TODO

draw_checkers(5)

# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: tabuleiro de xadrez

def draw_checkers(n: int) -> None:
    """ Draw an n x n checkerboard with '#' and '.'.
        Precondition: n >= 0
    """
    for i in range(n):
        for j in range(n):
            if (i + j) % 2 == 0:
                print('#', end='')
            else:
                print('.', end='')
        print()

draw_checkers(5)

# Nas posições com '#', a soma i + j é sempre par: (0,0) (0,2) (1,1) (2,0) ...
# Andar uma casa para o lado ou para baixo muda a paridade: daí o alternado.
#
# O padrão de sempre: dois for (linhas e colunas) e um if sobre (i, j).
# Mais tarde, com as matrizes, este (i, j) vai ser a posição m[i][j].






# %%
"""
EXERCICIO (EXTRA) 3 - Cifra de César -----------------------------------------

    Generalizar o ex 44: avançar cada maiúscula k letras, sempre a rodar
    no fim do alfabeto. O resto fica igual.

        caesar("ATACAR AO AMANHECER", 3) == "DWDFDU DR DPDQKHFHU"
        caesar("XYZ", 3) == "ABC"
        caesar("DWDFDU", -3) == "ATACAR"      (com -k, decifra)

    Pista: a posição da letra no alfabeto é ord(c) - ord('A') (de 0 a 25).
    Somar k e "rodar" no 26 é um trabalho para o %.
"""

def is_upper(c: str) -> bool:
    """ Is c a letter in upper case (without accent)? """
    return len(c) == 1 and 'A' <= c <= 'Z'

def shift_upper(c: str, k: int) -> str:
    """ Advance the upper case letter c by k letters, rotating after 'Z'.
        Precondition: is_upper(c)
    """
    pass        # TODO

def caesar(s: str, k: int) -> str:
    """ Advance all the upper case letters of s by k letters. """
    pass        # TODO

print(caesar("ATACAR AO AMANHECER", 3))

# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: cifra de César

def is_upper(c: str) -> bool:
    """ Is c a letter in upper case (without accent)? """
    return len(c) == 1 and 'A' <= c <= 'Z'

def shift_upper(c: str, k: int) -> str:
    """ Advance the upper case letter c by k letters, rotating after 'Z'.
        Precondition: is_upper(c)
    """
    position = ord(c) - ord('A')            # 'A' -> 0, ..., 'Z' -> 25
    new_position = (position + k) % 26      # o % faz a volta: 26 -> 0, 27 -> 1, ...
    return chr(ord('A') + new_position)

def caesar(s: str, k: int) -> str:
    """ Advance all the upper case letters of s by k letters. """
    result = ""
    for c in s:
        if is_upper(c):
            result += shift_upper(c, k)
        else:
            result += c
    return result

print(caesar("ATACAR AO AMANHECER", 3))     # DWDFDU DR DPDQKHFHU
print(caesar("XYZ", 3))                     # ABC
print(caesar("DWDFDU", -3))                 # ATACAR

# O truque: passar da letra para a POSIÇÃO no alfabeto (0 a 25),
# fazer a conta na posição, e voltar para letra.
#
# O % 26 faz a roda: 'X' (23) + 3 = 26, e 26 % 26 = 0, que é o 'A'.
# Com k negativo também funciona: (0 - 3) % 26 = 23 em Python, o 'X'.
#
# O ex 44 é o caso k = 1. Lá, o 'Z' foi tratado com um if;
# aqui o % trata de todos os casos de uma vez.
#
# A translate do ex 44 e esta caesar têm o mesmo esqueleto:
# percorrer a string e construir uma nova, caráter a caráter.





