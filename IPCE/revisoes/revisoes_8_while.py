# %%
"""
===========================================================================
REVISÕES 8 - O CICLO WHILE
Repetir ENQUANTO uma condição for verdadeira.
Quando é que se usa for, e quando é que se usa while?
E como escrever um while que acaba sempre?
IPCE 2026/2027
===========================================================================


Porquê um ficheiro só para isto?
  O for já o conheces bem (ficheiro 4): percorre um range, uma lista, uma string.
  Mas há problemas em que NÃO há nada para percorrer:
  só se sabe QUANDO PARAR. Um jogo acaba quando se acerta.
  Um programa pede valores até o utilizador escrever "fim".
  Um número vai sendo dividido até chegar a 1.
  Para esses, o for não serve. É o trabalho do while.
  E o while tem um perigo novo: se a condição nunca ficar falsa,
  o programa nunca acaba. Aqui aprendes uma receita para isso não acontecer.

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 27. for ou while? E a receita do while ...................... [~40 min]
       Porquê: primeiro, tudo o que já fazes com for, agora com while.
       Depois, escolher bem o ciclo (poupa trabalho e erros), e escrever
       SEMPRE o while pela mesma ordem (evita os ciclos infinitos).

 28. Quando só se sabe quando parar ........................... [~25 min]
       Porquê: é aqui que o while é mesmo preciso
       (dividir até 1, juntar até chegar a um valor, procurar o próximo...).

 29. Ler e validar o input ..................................... [~30 min]
       Porquê: pedir valores "até ao fim", repetir a pergunta até a resposta
       fazer sentido, e menus de comandos. Aparecem em quase todos os programas.

 30. Erros, executar à mão e desafios .......................... [~45 min]
       Porquê: saber prever o que um ciclo faz (à mão, numa tabela)
       e encontrar os erros típicos antes de correr.

  Resumo + soluções dos desafios

  Total: ~2h20. Não é para fazer de uma vez.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Alguns exercícios fazem input: escreve as respostas na consola.
  - Se um programa não acabar: o quadrado vermelho da consola, ou Ctrl + C
    com o cursor na consola.
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
    contar algarismos, Euclides, juros a duplicar, interpretador de comandos,
    ler até -1, jogo Hi-Lo (ex 46 e 47), somar algarismos, Collatz,
    média de notas com validação, get_month com while.
  Este ficheiro junta outros exercícios, à volta das mesmas ideias.

  A numeração continua a dos ficheiros anteriores: este começa na secção 27.
"""






# %%
"""
===========================================================================
27. FOR OU WHILE? E A RECEITA DO WHILE [~40 min]
===========================================================================

    O while repete um bloco de código ENQUANTO uma condição for verdadeira.

        inicialização
        while condição:
            corpo               # inclui o avanço

    Tem sempre 3 peças:
        1. inicialização   antes do ciclo        (ex: i = 0)
        2. condição        na linha do while     (ex: i < 5)
        3. avanço          dentro do corpo       (ex: i += 1)

    O for faz as 3 sozinho: range(5) já diz onde começa, onde acaba e o passo.
    No while, somos nós que escrevemos cada uma.

    Primeiro (Ex 27.1 a 27.4): tudo o que já fazes com for, agora com while.
    Depois (Ex 27.5 em diante): quando escolher cada um,
    e uma receita para escrever um while que acaba sempre.

Ex 27.1 [EXEMPLO] O mesmo ciclo, com for e com while -----------------------

    Corre as duas versões. Escrevem o mesmo.
"""

# Com for: o range diz tudo numa linha (começa em 0, acaba antes de 5, passo 1)
for i in range(5):
    print(i, end=" ")
print()                         # 0 1 2 3 4

# Com while: as 3 peças em 3 sítios diferentes
i = 0                           # 1. inicialização
while i < 5:                    # 2. condição: enquanto for True, repete
    print(i, end=" ")
    i += 1                      # 3. avanço
print()                         # 0 1 2 3 4

# O while testa a condição ANTES de cada volta.
# Na volta em que i = 5, a condição dá False e o ciclo acaba.
# Se a condição for logo False, o corpo não corre nenhuma vez (zero voltas).






# %%
"""
Ex 27.2 [EXEMPLO] Mais três, lado a lado -------------------------------------

    Em cada um, encontra as 3 peças do while e compara com o for.
"""

# a) Os pares de 0 a 8
for i in range(0, 10, 2):
    print(i, end=" ")
print()                         # 0 2 4 6 8

i = 0
while i < 10:
    print(i, end=" ")
    i += 2                      # o passo do range virou o avanço
print()                         # 0 2 4 6 8

# b) Somar 1 + 2 + ... + n
def sum_to_n_for(n: int) -> int:
    """ 1 + 2 + ... + n. """
    total = 0
    for i in range(1, n + 1):
        total += i
    return total

def sum_to_n_while(n: int) -> int:
    """ 1 + 2 + ... + n. """
    total = 0
    i = 1                       # inicialização: o início do range
    while i <= n:               # condição: o fim do range (n + 1 não entra -> <= n)
        total += i
        i += 1                  # avanço: o passo do range
    return total

print(sum_to_n_for(5), sum_to_n_while(5))      # 15 15

# c) Escrever cada nome com a sua posição
names = ["Ana", "Rui", "Eva"]

for i in range(len(names)):
    print(f"{i}: {names[i]}")   # 0: Ana   1: Rui   2: Eva

i = 0
while i < len(names):
    print(f"{i}: {names[i]}")
    i += 1

# A tradução é sempre a mesma:
#   range(início, fim, passo)  ->  i = início / while i < fim / i += passo
# O que estava dentro do for fica igual, dentro do while.






# %%
"""
Ex 27.3 [FAZ] Do for para o while -------------------------------------------

    Cada alínea já está feita com for. Faz a versão com while.
    Começa pelas 3 peças (inicialização, condição, avanço), e só depois o resto.
    As duas versões têm de escrever o mesmo.

    a) Escrever 10 9 8 ... 1.
    b) Escrever os múltiplos de 3 até 30: 0 3 6 ... 30.
    c) Somar os elementos de uma lista.
    d) Contar as vogais de uma string.
"""

# a)
for i in range(10, 0, -1):
    print(i, end=" ")
print()                         # 10 9 8 7 6 5 4 3 2 1
# TODO: com while

# b)
for i in range(0, 31, 3):
    print(i, end=" ")
print()                         # 0 3 6 9 12 15 18 21 24 27 30
# TODO: com while

# c)
def sum_list_for(l: list[int]) -> int:
    """ Sum of the elements of a list. """
    total = 0
    for v in l:
        total += v
    return total

def sum_list_while(l: list[int]) -> int:
    """ Sum of the elements of a list. """
    pass        # TODO

print(sum_list_for([4, 8, 15, 16]), sum_list_while([4, 8, 15, 16]))      # 43 43

# d)
def count_vowels_for(s: str) -> int:
    """ Number of vowels (a, e, i, o, u) in s. """
    count = 0
    for c in s:
        if c in "aeiou":
            count += 1
    return count

def count_vowels_while(s: str) -> int:
    """ Number of vowels (a, e, i, o, u) in s. """
    pass        # TODO

print(count_vowels_for("programar"), count_vowels_while("programar"))    # 3 3
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 27.3

# a)
i = 10
while i >= 1:
    print(i, end=" ")
    i -= 1
print()                         # 10 9 8 7 6 5 4 3 2 1

# b)
i = 0
while i <= 30:
    print(i, end=" ")
    i += 3
print()                         # 0 3 6 9 12 15 18 21 24 27 30

# c)
def sum_list_while(l: list[int]) -> int:
    """ Sum of the elements of a list. """
    total = 0
    i = 0
    while i < len(l):
        total += l[i]
        i += 1
    return total

print(sum_list_while([4, 8, 15, 16]))      # 43

# d)
def count_vowels_while(s: str) -> int:
    """ Number of vowels (a, e, i, o, u) in s. """
    count = 0
    i = 0
    while i < len(s):
        if s[i] in "aeiou":
            count += 1
        i += 1                  # FORA do if: avança sempre (ver Ex 30.4)
    return count

print(count_vowels_while("programar"))     # 3

# Repara:
#   o range(10, 0, -1) virou i = 10 / i >= 1 / i -= 1
#   o range(0, 31, 3) virou i = 0 / i <= 30 / i += 3   (o 31 "não entra" vira <= 30)
#   o for v in l virou um índice i, de 0 até len(l) - 1
#
# Todas funcionam, mas as versões for leem-se melhor:
# aqui o intervalo está bem definido, por isso o certo é o for.






# %%
"""
Ex 27.4 [FAZ] Do while para o for -------------------------------------------

    Ao contrário: estas funções usam while, mas o intervalo está bem definido.
    Reescreve-as com for (ficam mais curtas e mais claras).
"""

def count_multiples(a: int, b: int, k: int) -> int:
    """ How many multiples of k in the closed interval [a, b].
        Precondition: k > 0
    """
    count = 0
    i = a
    while i <= b:
        if i % k == 0:
            count += 1
        i += 1
    return count

def first_index_of(l: list[int], v: int) -> int:
    """ Index of the first v in l, or -1 if v is not there. """
    i = 0
    while i < len(l):
        if l[i] == v:
            return i
        i += 1
    return -1

print(count_multiples(1, 20, 3))           # 6   (3, 6, 9, 12, 15, 18)
print(first_index_of([5, 7, 9, 7], 7))     # 1
print(first_index_of([5, 7, 9, 7], 8))     # -1

# TODO: count_multiples_for e first_index_of_for
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 27.4

def count_multiples_for(a: int, b: int, k: int) -> int:
    """ How many multiples of k in the closed interval [a, b].
        Precondition: k > 0
    """
    count = 0
    for i in range(a, b + 1):
        if i % k == 0:
            count += 1
    return count

def first_index_of_for(l: list[int], v: int) -> int:
    """ Index of the first v in l, or -1 if v is not there. """
    for i in range(len(l)):
        if l[i] == v:
            return i
    return -1

print(count_multiples_for(1, 20, 3))           # 6
print(first_index_of_for([5, 7, 9, 7], 7))     # 1
print(first_index_of_for([5, 7, 9, 7], 8))     # -1

# Com for, desaparecem duas linhas (a inicialização e o avanço)
# e o intervalo lê-se logo: range(a, b + 1), range(len(l)).
# Menos linhas para escrever, e menos sítios onde errar.






# %%
"""
A RECEITA DO WHILE -----------------------------------------------------------

    Já sabes escrever um while. Agora: quando o usar, e como o escrever sem erros.

    A primeira pergunta, perante qualquer problema com repetição (a pergunta 0):

        O que vou percorrer está BEM DEFINIDO desde o início?
        (um intervalo de números, uma lista, uma string)

            sim -> for        (o for trata do início, do fim e do passo)
            não -> while      (só sei a condição para continuar)

    Se a escolha for while, escreve-se sempre pela mesma ordem (a RECEITA):

        1. PENSAR AS 3 PEÇAS, antes de qualquer outra coisa, e escrevê-las logo:
               inicialização                  i = 0
               while condição:                while i < n:
                   ...                            ...
                   avanço                         i += 1
           O avanço fica logo escrito, 3 ou 4 linhas abaixo do while.
           Assim nunca te esqueces dele.

        2. PENSAR A LÓGICA do problema: o que o ciclo faz em cada volta.
           Escreve-se no meio, entre o while e o avanço.

        3. CONFIRMAR as 3 peças, agora com a lógica escrita:
               inicialização: os valores de partida estão certos? (0 ou 1? soma a 0?)
               condição:      é a variável certa? < ou <=?
               avanço:        está no sítio certo, antes ou depois das contas?
               e a pergunta da paragem: o avanço aproxima-me MESMO de a condição
               ficar falsa? Há algum caso em que nunca chega a ficar falsa?
           Por fim, testar à mão um caso pequeno, e o caso de zero voltas.
           Se alguma coisa "pode não acabar" ou dá um a mais/a menos, corrige-se aqui.

Ex 27.5 [EXEMPLO] A receita com uma só variável ----------------------------

    Problema: escrever os quadrados (1, 4, 9, 16, ...) menores do que um limite.
        print_squares_below(50)  ->  1 4 9 16 25 36 49

    Pergunta 0: quantos quadrados vão ser? Depende do limite, não se sabe à partida
    -> while.
"""

# Passo 1: pensar as 3 peças, e escrevê-las (ainda não faz nada de útil)
#
#     i = 1
#     while i * i < limit:      # continua enquanto o quadrado ainda é pequeno
#
#
#         i += 1
#
# Passo 2: a lógica, no meio
#
#     i = 1
#     while i * i < limit:
#         print(i * i, end=" ")
#         i += 1
#
# Passo 3: confirmar
#   inicialização: i = 1, porque o primeiro quadrado é 1 (com i = 0 escrevia o 0).
#   condição: < porque o limite não entra (com 50: o 49 entra, o 64 não).
#   avanço: depois do print (se viesse antes, o 1 nunca era escrito).
#   paragem: o i cresce 1 em cada volta, o i * i também, e um dia chega ao limite.
#   à mão com limit = 10: escreve 1, 4, 9; com limit = 1: zero voltas. Certo.

def print_squares_below(limit: int) -> None:
    """ Print the squares 1, 4, 9, ... that are smaller than limit. """
    i = 1
    while i * i < limit:
        print(i * i, end=" ")
        i += 1
    print()

print_squares_below(50)         # 1 4 9 16 25 36 49
print_squares_below(1)          # (linha vazia: 1 * 1 < 1 é logo falso, zero voltas)

# Aqui UMA variável faz tudo:
#   o i é inicializado (i = 1), está na condição (i * i < limit),
#   avança (i += 1) e entra nas contas (print(i * i)).
# É o caso mais simples, igual ao dos exercícios anteriores.
# No próximo exemplo, os papéis ficam repartidos por duas variáveis.






# %%
"""
Ex 27.6 [EXEMPLO] A receita com duas variáveis -----------------------------

    Problema: qual é o MENOR n tal que 1 + 2 + ... + n é maior ou igual a um limite?
        smallest_n(10) == 4       (1+2+3 = 6 não chega; 1+2+3+4 = 10 chega)
        smallest_n(100) == 14

    Pergunta 0: o intervalo está bem definido? Não: não sei até onde vai o n.
    Só sei quando parar (quando a soma chegar ao limite). -> while.
"""

# Passo 1: pensar as 3 peças, e escrevê-las
#
#     n = 0
#     total = 0
#     while total < limit:      # continua ENQUANTO a soma ainda não chegou
#
#
#         n += 1
#
# Passo 2: a lógica, no meio: somar o número
#
#     n = 0
#     total = 0
#     while total < limit:
#         total += n
#         n += 1
#
# Passo 3: confirmar (e aqui apanha-se um erro!)
#   à mão com limit = 10:
#       total: 0 -> 0 -> 1 -> 3 -> 6 -> 10   e n: 1, 2, 3, 4, 5
#   Devolve 5, mas a resposta é 4. A 1ª volta soma o 0, e o n fica um a mais.
#   O avanço está no sítio errado: primeiro passa-se ao número seguinte,
#   e só depois se soma. Trocar as duas linhas:
#
#         n += 1                # o próximo número...
#         total += n            # ... entra na soma
#
#   à mão outra vez: n: 1, 2, 3, 4 e total: 1, 3, 6, 10 -> devolve 4. Certo.
#   paragem: em cada volta o total cresce pelo menos 1 (n >= 1), por isso
#   um dia total >= limit. Acaba sempre.
#   zero voltas: limit = 0 -> 0 < 0 é falso, devolve 0. Certo.
#
#   A receita não obriga a pôr o avanço no fim: obriga a escrevê-lo LOGO
#   (para não ficar esquecido) e a confirmar o sítio dele no passo 3.

def smallest_n(limit: int) -> int:
    """ Smallest n such that 1 + 2 + ... + n >= limit. """
    n = 0
    total = 0
    while total < limit:
        n += 1
        total += n
    return n

print(smallest_n(10))           # 4
print(smallest_n(100))          # 14
print(smallest_n(0))            # 0   (zero voltas: a soma vazia já chega)

# Aqui há DUAS variáveis, com papéis diferentes:
#
#   variável   inicialização   está na condição?   como muda?      é a resposta?
#   n          n = 0           não                 n += 1          sim (return n)
#   total      total = 0       sim (total < limit) total += n      não
#
# A condição olha para o total, mas é o n que "anda" de volta para volta,
# e o total cresce por causa dele.
#
# Na pergunta da paragem, olha sempre para a variável da CONDIÇÃO:
# é ela que tem de mudar em cada volta, e na direção certa.
# Se nenhuma variável da condição muda dentro do ciclo, é um ciclo infinito.






# %%
"""
Ex 27.7 [EXEMPLO] Bem definido ou não? E que peças? -------------------------

    Para cada problema, os dois primeiros passos de pensar, ainda sem código:
      - a pergunta 0: for ou while?
      - se for while, as peças: que variável começa com que valor,
        o que é testado na condição, o que muda em cada volta (o avanço),
        e o que é a conta (o que se quer no fim).
"""

# 1. Escrever os números de 1 a 100.
#      bem definido (1 a 100)                         -> for i in range(1, 101)
#
# 2. Contar as vogais de uma frase.
#      bem definido (os carateres da frase)            -> for c in phrase
#
# 3. Pedir números até o utilizador escrever 0, e somá-los.
#      não se sabe quantos vão ser                     -> while
#        inicialização: total = 0, e ler o primeiro número v
#        condição:      v != 0
#        avanço:        ler o número seguinte (v = int(input(...)))
#        conta:         total += v
#
# 4. Quantos anos até um depósito de 1000 euros duplicar, a uma taxa rate?
#      não se sabe quantos anos                        -> while
#        inicialização: money = 1000, years = 0
#        condição:      money < 2000
#        avanço:        money *= (1 + rate)   (o dinheiro cresce)
#        conta:         years += 1            (a resposta é o years)
#      O avanço é o money, que está na condição; o years só conta voltas.
#
# 5. Procurar se há um negativo numa lista.
#      bem definido (a lista); sai-se mais cedo com return  -> for
#
# 6. Dividir um número por 2 até ficar ímpar.
#      não se sabe quantas divisões                    -> while
#        inicialização: o próprio n (vem como parâmetro)
#        condição:      n % 2 == 0          (enquanto é par)
#        avanço:        n //= 2
#        conta:         o n final (ou quantas divisões, com um count += 1)
#      Pergunta da paragem: e se n = 0? 0 é par, 0 // 2 = 0, e fica par
#      para sempre: ciclo infinito. Precisa de precondição n > 0.
#
# Repara no 5: sair a meio NÃO obriga a while.
# Um for com return (ou break) lá dentro também pára mais cedo.
# A pergunta é "sei o que percorrer?", não "vou até ao fim?".






# %%
"""
Ex 27.8 [PENSA] for ou while? -----------------------------------------------

    Para cada problema: for ou while? E se for while, quais são as peças
    (inicialização, condição, avanço)?

    a) Escrever a tabuada do 7 (7 x 1 até 7 x 10).
    b) Pedir uma password até estar certa.
    c) Somar os números de uma lista até aparecer o primeiro negativo.
    d) Quantas vezes se pode tirar 7 a um número antes de ficar negativo?
    e) Escrever cada letra de uma palavra numa linha.
    f) Lançar um dado até sair um 6.
"""

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 27.8 (sem código):
#
#   a) for: o intervalo é 1 a 10.                     for i in range(1, 11)
#   b) while: não se sabe quantas tentativas.         while password != CORRECT
#   c) for com break (ou return): percorre-se a lista, e sai-se no negativo.
#      Também dá com while: while i < len(l) and l[i] >= 0
#   d) while: depende do número.                      while n >= 7: n -= 7
#      (ou, sem ciclo nenhum: n // 7)
#   e) for: as letras da palavra.                     for c in word
#   f) while: não se sabe quantos lançamentos.        while dice != 6
#
# Na c) as duas servem. Com while, repara na condição dupla:
# primeiro "ainda há elementos?" (i < len(l)), depois "o elemento serve?".
# A ordem importa: se i já for len(l), o l[i] dava IndexError,
# mas o and pára logo no primeiro False e nem chega a avaliar o l[i]
# (teórica 3b: o and e o or avaliam da esquerda para a direita e param mais cedo).






# %%
"""
===========================================================================
28. QUANDO SÓ SE SABE QUANDO PARAR [~25 min]
===========================================================================

    Nestes problemas não há nada para percorrer.
    Há um valor que vai mudando, e uma condição para parar.
    A receita é a mesma: as 3 peças, a lógica, e no fim confirmar tudo
    (com a pergunta da paragem).

Ex 28.1 [EXEMPLO] Quantas vezes se pode dividir por 2? -----------------------

    halvings(n): quantas divisões inteiras por 2 até o n chegar a 1.
        halvings(1) == 0
        halvings(8) == 3          (8 -> 4 -> 2 -> 1)
        halvings(100) == 6        (100 -> 50 -> 25 -> 12 -> 6 -> 3 -> 1)
"""

def halvings(n: int) -> int:
    """ How many integer divisions by 2 until n reaches 1.
        Precondition: n >= 1
    """
    count = 0                   # inicialização
    while n > 1:                # condição: enquanto ainda não chegou a 1
        n //= 2                 # avanço: o próprio n encolhe
        count += 1
    return count

print(halvings(1), halvings(8), halvings(100))     # 0 3 6

# Aqui não há um i: o avanço é o n //= 2.
# A pergunta da paragem: o n encolhe para metade em cada volta,
# por isso chega a 1 (se começar >= 1). Acaba sempre.
#
# E se alguém chamar halvings(0)? 0 > 1 é logo False: zero voltas, devolve 0.
# Não fica infinito, mas o resultado não faz sentido: daí a precondição.
#
# (é o pior caso do Hi-Lo do ex 47: cortar a meio, de cada vez)






# %%
"""
Ex 28.2 [EXEMPLO] A primeira potência de 2 que chega -------------------------

    first_power_of_2(n): a menor potência de 2 (1, 2, 4, 8, ...) que é >= n.
        first_power_of_2(1) == 1
        first_power_of_2(5) == 8
        first_power_of_2(64) == 64
"""

def first_power_of_2(n: int) -> int:
    """ Smallest power of 2 greater than or equal to n. """
    power = 1
    while power < n:
        power *= 2
    return power

print(first_power_of_2(1), first_power_of_2(5), first_power_of_2(64))   # 1 8 64

# Inicialização: power = 1. Condição: ainda não chega. Avanço: duplica.
# A pergunta da paragem: o power cresce sempre (duplica), por isso um dia passa o n.
#
# Atenção ao "<" e não "<=": com 64, queremos o próprio 64.
# Com "power <= n" o ciclo dava mais uma volta e devolvia 128.






# %%
"""
Ex 28.3 [FAZ] Inverter um número --------------------------------------------

    reverse_number(1234) == 4321
    reverse_number(7) == 7
    reverse_number(1200) == 21       (os zeros do fim desaparecem)

    Pista: n % 10 dá o último algarismo; n // 10 tira-o.
    E para "colar" um algarismo d à direita de um número r: r * 10 + d.
"""

def reverse_number(n: int) -> int:
    """ The number with the digits of n in reverse order.
        Precondition: n >= 0
    """
    pass        # TODO

print(reverse_number(1234), reverse_number(7), reverse_number(1200))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 28.3

def reverse_number(n: int) -> int:
    """ The number with the digits of n in reverse order.
        Precondition: n >= 0
    """
    result = 0
    while n > 0:
        result = result * 10 + n % 10   # cola o último algarismo de n à direita
        n //= 10                        # e tira-o de n
    return result

print(reverse_number(1234), reverse_number(7), reverse_number(1200))   # 4321 7 21

# À mão, com 1234:
#   n      result
#   1234   0
#   123    4
#   12     43
#   1      432
#   0      4321    -> 0 > 0 é False, acaba
#
# É o mesmo ciclo do count_digits da aula 7: o avanço é n //= 10.






# %%
"""
Ex 28.4 [FAZ] A bola que salta ----------------------------------------------

    Uma bola cai de uma altura h (em metros). Em cada salto, sobe só até
    uma fração ratio da altura anterior (ex: 0.6 = 60%).
    Quantos saltos até a altura do salto ficar abaixo de min_height?

        bounces(2.0, 0.6, 0.1) == 6
            (2.0 -> 1.2 -> 0.72 -> 0.432 -> 0.2592 -> 0.15552 -> 0.093312)

    Depois responde: e se o ratio for 1.0? E 1.5?
"""

def bounces(h: float, ratio: float, min_height: float) -> int:
    """ Number of bounces until the height is below min_height.
        Precondition: 0 < ratio < 1 and min_height > 0
    """
    pass        # TODO

print(bounces(2.0, 0.6, 0.1))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 28.4

def bounces(h: float, ratio: float, min_height: float) -> int:
    """ Number of bounces until the height is below min_height.
        Precondition: 0 < ratio < 1 and min_height > 0
    """
    count = 0
    while h >= min_height:      # enquanto o salto ainda é alto
        h *= ratio              # o salto seguinte
        count += 1
    return count

print(bounces(2.0, 0.6, 0.1))   # 6
print(bounces(2.0, 0.9, 0.1))   # 29   (bola mais saltitona)

# A pergunta da paragem: com 0 < ratio < 1, o h diminui sempre,
# por isso um dia fica abaixo de min_height.
#
# Com ratio = 1.0, o h nunca muda: ciclo infinito.
# Com ratio = 1.5, o h cresce: ciclo infinito também.
# É por isso que a precondição diz 0 < ratio < 1.
# A precondição faz parte da resposta à pergunta da paragem.






# %%
"""
Ex 28.5 [FAZ] O próximo primo (teórica 4a) -----------------------------------

    next_prime(n): o primeiro número primo maior ou igual a n.
        next_prime(14) == 17
        next_prime(17) == 17
        next_prime(1) == 2

    A função is_prime já está feita (é a da teórica 3b, com uma melhoria:
    o 1 não é primo).
"""

def is_prime(n: int) -> bool:
    """ Is n a prime number? """
    if n < 2:
        return False
    for d in range(2, n):
        if n % d == 0:
            return False
    return True

def next_prime(n: int) -> int:
    """ The first prime number >= n. """
    pass        # TODO

print(next_prime(14), next_prime(17), next_prime(1))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 28.5

def is_prime(n: int) -> bool:
    """ Is n a prime number? """
    if n < 2:
        return False
    for d in range(2, n):
        if n % d == 0:
            return False
    return True

def next_prime(n: int) -> int:
    """ The first prime number >= n. """
    while not is_prime(n):
        n += 1
    return n

print(next_prime(14), next_prime(17), next_prime(1))     # 17 17 2

# Um while de duas linhas. As 3 peças:
#   inicialização: o próprio parâmetro n (já vem com valor)
#   condição:      "ainda não é primo"
#   avanço:        n += 1
#
# A pergunta da paragem: acaba sempre? Sim, porque há infinitos primos
# (a teórica 4a diz isto mesmo). É um caso em que a garantia vem da matemática.
#
# Repara nos dois ciclos: o is_prime usa for (os divisores de 2 a n-1 estão
# bem definidos), o next_prime usa while (não se sabe onde está o próximo primo).
# Cada um no seu sítio.






# %%
"""
===========================================================================
29. LER E VALIDAR O INPUT [~30 min]
===========================================================================

    Ler valores do utilizador é o caso mais comum do while:
    nunca se sabe quantos valores o utilizador vai escrever,
    nem se vai escrever um disparate.

    Aqui o "avanço" é ler o valor seguinte.
    A receita fica:
        while True:
            ler                      (o avanço)
            se for para parar: break (a condição, testada logo a seguir a ler)
            a lógica

    (há a outra forma, ler antes do ciclo e outra vez no fim do corpo:
     está na aula 7, secção d), ponto 5. As duas são aceites.)

Ex 29.1 [EXEMPLO] Ler até uma linha vazia (teórica 4b) ------------------------

    Ler temperaturas, uma por linha, até o utilizador carregar ENTER sem escrever nada.
    No fim, escrever a mais alta.

        Temperatura: 18.5
        Temperatura: 22
        Temperatura: 19.0
        Temperatura:
        Máxima: 22.0
"""

END_MARK = ""                   # a linha vazia

def read_temperatures() -> list[float]:
    """ Read temperatures, one per line, until an empty line. """
    temps = []
    while True:
        s = input("Temperatura: ")      # ler (o avanço)
        if s == END_MARK:               # testar logo a seguir
            break
        temps.append(float(s))          # a lógica
    return temps

def maximum(l: list[float]) -> float:
    """ Largest value of l.
        Precondition: len(l) > 0
    """
    best = l[0]
    for v in l:
        if v > best:
            best = v
    return best

def main() -> None:
    temps = read_temperatures()
    if len(temps) == 0:
        print("Não há temperaturas")
    else:
        print(f"Máxima: {maximum(temps)}")

main()

# Porquê ler como string e só depois converter?
# Porque a marca de fim (a linha vazia) não é um número:
# float("") dava ValueError. Primeiro testa-se, depois converte-se.
#
# Os dois ciclos, cada um no seu sítio:
#   ler: while (não se sabe quantas temperaturas vêm)
#   máximo: for (a lista já existe, está bem definida)






# %%
"""
Ex 29.2 [EXEMPLO] Repetir a pergunta até a resposta fazer sentido ------------

    Pedir uma idade até ela estar entre 0 e 120.

        Idade: 150
        Idade: -3
        Idade: 25
        Tem 25 anos.
"""

def read_age() -> int:
    """ Read an age until it is between 0 and 120. """
    while True:
        age = int(input("Idade: "))
        if 0 <= age <= 120:
            return age          # o return sai do ciclo E da função

def main() -> None:
    age = read_age()
    print(f"Tem {age} anos.")

main()

# Validar o input: o programa não confia no utilizador.
# Repete até a resposta ser boa, e só depois a usa.
#
# Aqui sai-se com return em vez de break: a função só tem este trabalho,
# por isso quando a idade é boa, já acabou.
#
# (se o utilizador escrever "abc", o int dá erro: isso já não é connosco.
#  Nesta cadeira valida-se o VALOR, não se o texto é um número)






# %%
"""
Ex 29.3 [FAZ] Ler um número positivo ----------------------------------------

    a) read_positive(prompt): pede um inteiro com o prompt dado,
       até ele ser maior do que 0. Se não for, escreve "Tem de ser positivo".
    b) main: lê a largura e a altura de um retângulo com a read_positive,
       e escreve a área.

        Largura: 0
        Tem de ser positivo
        Largura: 4
        Altura: -2
        Tem de ser positivo
        Altura: 3
        Área: 12
"""

def read_positive(prompt: str) -> int:
    """ Read an integer until it is positive. """
    pass        # TODO

def main() -> None:
    pass        # TODO

main()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 29.3

def read_positive(prompt: str) -> int:
    """ Read an integer until it is positive. """
    while True:
        n = int(input(prompt))
        if n > 0:
            return n
        print("Tem de ser positivo")

def main() -> None:
    width = read_positive("Largura: ")
    height = read_positive("Altura: ")
    print(f"Área: {width * height}")

main()

# O prompt é um parâmetro: a mesma função serve para a largura e para a altura.
# Uma função de leitura com validação, reutilizada duas vezes.
#
# Repara onde fica o print do erro: DEPOIS do if com return.
# Só lá chega quando o número não serviu.






# %%
"""
Ex 29.4 [FAZ] Ler uma lista de inteiros (teórica 4b) ------------------------

    a) read_int_list(): lê inteiros com o prompt ": " até uma linha vazia,
       e devolve a lista.
    b) main: lê a lista e escreve quantos valores são pares e qual a soma.
       Se a lista vier vazia, escreve "Lista vazia".

        : 4
        : 7
        : 10
        :
        Pares: 2
        Soma: 21
"""

def read_int_list() -> list[int]:
    """ Read integers until an empty line. """
    pass        # TODO

def main() -> None:
    pass        # TODO

main()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 29.4

END_MARK = ""

def read_int_list() -> list[int]:
    """ Read integers until an empty line. """
    l = []
    while True:
        s = input(": ")
        if s == END_MARK:
            break
        l.append(int(s))
    return l

def count_even(l: list[int]) -> int:
    """ How many even values in l. """
    count = 0
    for v in l:
        if v % 2 == 0:
            count += 1
    return count

def main() -> None:
    l = read_int_list()
    if len(l) == 0:
        print("Lista vazia")
    else:
        total = 0
        for v in l:
            total += v
        print(f"Pares: {count_even(l)}")
        print(f"Soma: {total}")

main()

# A leitura (while) separada das contas (for).
# Primeiro tenho a lista toda; depois faço com ela o que quiser.
# É o padrão de "ler N valores" do ficheiro 7, agora sem saber o N.






# %%
"""
Ex 29.5 [FAZ] Um menu de comandos -------------------------------------------

    Uma calculadora que fica a pedir comandos até ao "q":
        s -> pede dois inteiros e escreve a soma
        m -> pede dois inteiros e escreve o produto
        q -> escreve "Adeus!" e acaba
        qualquer outro -> "Comando inválido"

        Comando (s, m, q)? s
        a: 3
        b: 4
        3 + 4 = 7
        Comando (s, m, q)? x
        Comando inválido
        Comando (s, m, q)? m
        a: 5
        b: 6
        5 * 6 = 30
        Comando (s, m, q)? q
        Adeus!

    Faz uma função para cada comando, e a função run com o ciclo.
"""

def run() -> None:
    """ Read and execute commands until "q". """
    pass        # TODO

run()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 29.5

def read_two() -> tuple[int, int]:
    """ Read the two operands. """
    a = int(input("a: "))
    b = int(input("b: "))
    return (a, b)

def command_sum() -> None:
    """ Command s: read two integers and print their sum. """
    a, b = read_two()
    print(f"{a} + {b} = {a + b}")

def command_product() -> None:
    """ Command m: read two integers and print their product. """
    a, b = read_two()
    print(f"{a} * {b} = {a * b}")

def run() -> None:
    """ Read and execute commands until "q". """
    while True:
        cmd = input("Comando (s, m, q)? ")
        if cmd == "s":
            command_sum()
        elif cmd == "m":
            command_product()
        elif cmd == "q":
            print("Adeus!")
            break
        else:
            print("Comando inválido")

run()

# É o esqueleto de qualquer interpretador de comandos (o projeto da cadeira
# costuma ser um destes): um while True, um if/elif por comando,
# e uma função por comando, para o ciclo ficar curto e fácil de ler.
#
# A read_two devolve dois valores de uma vez, num tuplo (teórica 6b):
#   a, b = read_two()   desempacota o tuplo nas duas variáveis.
# (podiam ser dois input em cada comando; assim não se repete código)






# %%
"""
===========================================================================
30. ERROS, EXECUTAR À MÃO E DESAFIOS [~45 min]
===========================================================================

Ex 30.1 [EXEMPLO] Executar à mão: a tabela de valores -----------------------

    Para prever o que um while faz, faz-se uma tabela: uma coluna por variável,
    uma linha por cada vez que se testa a condição.
"""

def mystery(n: int) -> int:
    """ A mystery function. """
    a = 0
    b = 1
    while b < n:
        a, b = b, a + b
    return a

print(mystery(20))              # 13

# À mão, com n = 20:
#
#   a    b    b < 20 ?
#   0    1    sim
#   1    1    sim
#   1    2    sim
#   2    3    sim
#   3    5    sim
#   5    8    sim
#   8    13   sim
#   13   21   não  -> acaba, devolve a = 13
#
# São os números de Fibonacci (0, 1, 1, 2, 3, 5, 8, 13, 21, ...):
# cada um é a soma dos dois anteriores.
# O mystery(n) devolve o maior deles que é menor do que n.
#
# Uma tabela destas responde a qualquer "qual o output?" sem correr nada.






# %%
"""
Ex 30.2 [FAZ] Executar à mão -------------------------------------------------

    Sem correr: o que escreve cada bloco? Faz a tabela numa folha.
    Depois corre para confirmar.
"""

# a)
x = 100
count = 0
while x > 10:
    x = x // 3
    count += 1
print(x, count)

# b)
i = 1
s = ""
while i <= 16:
    s += str(i) + " "
    i *= 2
print(s)

# c)
n = 5
while n != 0:
    if n % 2 == 1:
        n -= 1
    else:
        n //= 2
    print(n, end=" ")
print()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 30.2

# a) 3 3
#   x     count   x > 10 ?
#   100   0       sim
#   33    1       sim
#   11    2       sim
#   3     3       não  -> escreve 3 3
#
# b) 1 2 4 8 16
#   i:  1, 2, 4, 8, 16, 32 (32 <= 16 é falso: acaba)
#   o s fica "1 2 4 8 16 " (com um espaço no fim)
#
# c) 4 2 1 0
#   5 é ímpar -> 4;  4 é par -> 2;  2 é par -> 1;  1 é ímpar -> 0;  0 != 0 é falso
#
# Pergunta da paragem no c): ímpar tira 1, par divide por 2.
# O n diminui sempre, e nunca salta o 0 (de 1 vai para 0). Acaba sempre.






# %%
"""
Ex 30.3 [PENSA] Acaba ou não acaba? -------------------------------------------

    NÃO corras estes (alguns nunca acabam). Responde só a pensar:
    acaba? Se acaba, com que valor? Se não, porquê?
"""

def loop_a() -> int:
    i = 0
    while i < 10:
        i += 2
    return i

def loop_b() -> int:
    i = 1
    while i != 10:
        i += 2
    return i

def loop_c() -> int:
    i = 10
    while i > 0:
        i += 1
    return i

def loop_d() -> float:
    x = 0.0
    while x != 1.0:
        x += 0.1
    return x

def loop_e(n: int) -> int:
    while n > 0:
        n -= 3
    return n

# TODO: responde em comentário (para cada função: acaba? valor? porquê?)
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 30.3 (sem código):
#
#   a) acaba, devolve 10.  i: 0, 2, 4, 6, 8, 10 e 10 < 10 é falso.
#
#   b) NÃO acaba. i: 1, 3, 5, 7, 9, 11, ... só ímpares, nunca é 10.
#      Salta por cima do fim. Com "while i < 10" já parava (em 11).
#
#   c) NÃO acaba. O i começa em 10 e cresce: i > 0 é sempre verdade.
#      O avanço vai para o lado errado.
#
#   d) NÃO acaba, e é o mais traiçoeiro. 0.1 não é exato num computador
#      (teórica 3b: 0.1 + 0.2 == 0.3 dá False). Depois de 10 somas,
#      x fica 0.9999999999999999, não 1.0, e passa por cima.
#      Com "while x < 1.0" acaba, mas dá 11 voltas (por causa do mesmo 0.9999...).
#      Regra: nunca usar == ou != com floats numa condição de paragem.
#
#   e) acaba (se n for um inteiro qualquer), mas o resultado pode ser negativo:
#      loop_e(10) -> 10, 7, 4, 1, -2: devolve -2.  loop_e(9) -> 0.
#      Com "n > 0" e passo -3, o n passa o 0 sem problema: o < e o > aguentam
#      "saltar por cima"; o != não.






# %%
"""
Ex 30.4 [FAZ] Encontra o bug -------------------------------------------------

    Cada função tem UM erro. Encontra-o sem correr, e corrige.
    (o b) não acaba: só o corras depois de corrigido!)
"""

# a) Devia devolver a soma de 1 até n
def sum_to(n: int) -> int:
    """ 1 + 2 + ... + n. """
    total = 0
    i = 1
    while i < n:
        total += i
        i += 1
    return total

print(sum_to(4))                # devia dar 10

# b) Devia contar os pares de uma lista
def count_even(l: list[int]) -> int:
    """ How many even values in l. """
    count = 0
    i = 0
    while i < len(l):
        if l[i] % 2 == 0:
            count += 1
            i += 1
    return count

# print(count_even([2, 4, 5, 6]))   # devia dar 3

# c) Devia escrever as somas de cada linha: 6, 15
def print_row_sums(m: list[list[int]]) -> None:
    """ Print the sum of each row of m. """
    i = 0
    total = 0
    while i < len(m):
        j = 0
        while j < len(m[i]):
            total += m[i][j]
            j += 1
        print(total)
        i += 1

print_row_sums([[1, 2, 3], [4, 5, 6]])
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 30.4

# a) "i < n" deixa o n de fora: dava 1 + 2 + 3 = 6. Corrigido: i <= n.
def sum_to(n: int) -> int:
    """ 1 + 2 + ... + n. """
    total = 0
    i = 1
    while i <= n:
        total += i
        i += 1
    return total

print(sum_to(4))                # 10

# b) O i += 1 está DENTRO do if: no primeiro ímpar (o 5), o i nunca mais avança
#    e o ciclo fica infinito. Corrigido: o avanço fora do if.
def count_even(l: list[int]) -> int:
    """ How many even values in l. """
    count = 0
    i = 0
    while i < len(l):
        if l[i] % 2 == 0:
            count += 1
        i += 1
    return count

print(count_even([2, 4, 5, 6]))   # 3

# c) O total = 0 está FORA do while de fora: as somas acumulam (6, 21).
#    Corrigido: total = 0 dentro, antes de cada linha.
def print_row_sums(m: list[list[int]]) -> None:
    """ Print the sum of each row of m. """
    i = 0
    while i < len(m):
        total = 0
        j = 0
        while j < len(m[i]):
            total += m[i][j]
            j += 1
        print(total)
        i += 1

print_row_sums([[1, 2, 3], [4, 5, 6]])     # 6, 15

# Três erros clássicos:
#   a) a condição: < ou <=? Testa com um n pequeno e faz a conta à mão.
#   b) o avanço dentro de um if: só avança às vezes. A receita evita isto:
#      o avanço é escrito logo, ao nível do while, antes da lógica.
#   c) o sítio da inicialização: o que recomeça em cada linha fica DENTRO
#      do ciclo de fora (o mesmo do "j = 0" do ex 45).
#   (e no c) os dois while eram for: o intervalo está bem definido!)






# %%
"""
DESAFIOS ------------------------------------------------------------------

    Mais perto do nível dos testes. Soluções no fim do ficheiro.
    Antes de correr, a receita: 3 peças, lógica, confirmar (e a pergunta da paragem).

Ex 30.5 [DESAFIO] Adivinhar com tentativas limitadas ---------------------------

    O jogo Hi-Lo, mas com um máximo de tentativas. O segredo vem por parâmetro
    (para se poder testar sempre igual).

        play(secret, max_attempts)

    Em cada tentativa:
        "Tentativa: "  -> se for menor, "O número é maior"
                          se for maior, "O número é menor"
                          se acertar,   "Acertou em N tentativas!" e acaba
    Se esgotar as tentativas sem acertar: "Perdeu! O número era X."

        play(42, 3):
            Tentativa: 50
            O número é menor
            Tentativa: 40
            O número é maior
            Tentativa: 42
            Acertou em 3 tentativas!

        play(42, 2):
            Tentativa: 10
            O número é maior
            Tentativa: 20
            O número é maior
            Perdeu! O número era 42.

    Pista: o ciclo pára por DUAS razões diferentes.
"""

def play(secret: int, max_attempts: int) -> None:
    """ Hi-Lo game with a limited number of attempts. """
    pass        # TODO

play(42, 3)






# %%
"""
Ex 30.6 [DESAFIO] Raiz digital -----------------------------------------------

    Somam-se os algarismos de um número; se o resultado tiver mais de um
    algarismo, somam-se outra vez; e assim até ficar só um.
        digital_root(9875) == 2      (9+8+7+5 = 29 -> 2+9 = 11 -> 1+1 = 2)
        digital_root(7) == 7
        digital_root(0) == 0

    Pista: um while dentro de outro (ou um while que chama uma função com outro).
"""

def digital_root(n: int) -> int:
    """ Repeated sum of digits, until one digit is left.
        Precondition: n >= 0
    """
    pass        # TODO

print(digital_root(9875), digital_root(7), digital_root(0))






# %%
"""
Ex 30.7 [DESAFIO] Raiz quadrada por bisseção ---------------------------------

    Sem usar math.sqrt: procura-se a raiz de x num intervalo [lo, hi],
    cortando-o a meio em cada volta (como o computador no Hi-Lo do ex 47):
        mid = (lo + hi) / 2
        se mid * mid < x, a raiz está na metade de cima: lo = mid
        senão, está na metade de baixo: hi = mid
    Pára quando o intervalo ficar mais pequeno do que 1e-12,
    e devolve o meio desse intervalo.
    Começa com lo = 0 e hi = x (ou 1, se x < 1).

        my_sqrt(2.0)  -> 1.414213562...
        my_sqrt(9.0)  -> 3.000000000...
        my_sqrt(0.25) -> 0.5000000...

    Compara com math.sqrt usando math.isclose (teórica 3b).
"""

def my_sqrt(x: float) -> float:
    """ Square root of x, by bisection.
        Precondition: x >= 0
    """
    pass        # TODO

print(my_sqrt(2.0))






# %%
"""
Ex 30.8 [DESAFIO] Paradoxo de Zeno, ao contrário (Teste 1 2025/26, pergunta 3) --

    No teste pedia-se a soma dos k primeiros termos de 1/2 + 1/4 + 1/8 + ...
    (que se aproxima de 1). Agora ao contrário:
    quantos termos são precisos para a soma ficar a menos de eps de 1?

        zeno_terms(0.1) == 4         (1/2 + 1/4 + 1/8 + 1/16 = 0.9375)
        zeno_terms(0.01) == 7        (soma 0.9921875)
        zeno_terms(0.5) == 2         (0.75: com 1 termo, 0.5, ainda falta 0.5)

    No teste o k vinha dado (for). Aqui não se sabe quantos termos vão ser...
"""

def zeno_terms(eps: float) -> int:
    """ Number of terms of 1/2 + 1/4 + ... so that 1 - sum < eps.
        Precondition: eps > 0
    """
    pass        # TODO

print(zeno_terms(0.1), zeno_terms(0.01), zeno_terms(0.5))






# %%
"""
Ex 30.9 [SOZINHA] O multibanco -----------------------------------------------

    Um multibanco tem notas de 50, 20, 10 e 5 euros e dá sempre
    as notas maiores primeiro. Para uma quantia (múltipla de 5),
    escreve quantas notas de cada.

        Quantia: 185
        50: 3
        20: 1
        10: 1
        5: 1

    Pista: para cada nota (um for sobre [50, 20, 10, 5]),
    um while que tira essa nota enquanto ainda couber.
    (e depois pensa: dava para fazer sem while, com // e %?)
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  FOR OU WHILE?
  [ ] o que vou percorrer está bem definido (range, lista, string)? -> for
  [ ] só sei a condição para continuar?                             -> while
  [ ] sair a meio não obriga a while: for com return ou break também pára

  A RECEITA DO WHILE
  [ ] 1. pensar as 3 peças primeiro (inicialização, while condição, avanço)
         e escrevê-las logo
  [ ] 2. pensar a lógica do problema, e escrevê-la no meio
  [ ] 3. confirmar as 3 peças com a lógica: valores de partida, < ou <=,
         o sítio do avanço, e a pergunta da paragem (pode não acabar?)
  [ ] testar à mão um caso pequeno e o caso de zero voltas
  [ ] o avanço pode ser i += 1, n //= 10, power *= 2, ler o próximo input...
  [ ] o while pode dar zero voltas (condição logo falsa)

  LER INPUT
  [ ] while True: ler; se for a marca: break; a lógica
  [ ] marca de fim que não é número (a linha vazia): testar ANTES de converter
  [ ] validar: repetir a pergunta até a resposta servir (return quando serve)
  [ ] menu de comandos: while True + if/elif + uma função por comando

  OS ERROS
  [ ] avanço esquecido, ou dentro de um if            -> ciclo infinito
  [ ] avanço para o lado errado                       -> ciclo infinito
  [ ] != que o passo salta por cima                   -> ciclo infinito (preferir < e <=)
  [ ] == ou != com floats                             -> nunca usar numa paragem
  [ ] < ou <=?                                        -> testar com um caso pequeno
  [ ] o que recomeça em cada volta de fora            -> inicializar DENTRO do ciclo de fora

  EXECUTAR À MÃO
  [ ] uma coluna por variável, uma linha por cada teste da condição

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_9_matrizes.py (listas de listas, com for e com while).
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
# Solução do Ex 30.5

def play(secret: int, max_attempts: int) -> None:
    """ Hi-Lo game with a limited number of attempts. """
    attempts = 0
    won = False
    while attempts < max_attempts and not won:
        guess = int(input("Tentativa: "))
        attempts += 1
        if guess < secret:
            print("O número é maior")
        elif guess > secret:
            print("O número é menor")
        else:
            won = True
    if won:
        print(f"Acertou em {attempts} tentativas!")
    else:
        print(f"Perdeu! O número era {secret}.")

play(42, 3)                     # com 50, 40, 42: acerta à 3ª
play(42, 2)                     # com 10, 20: perde

# As duas razões para parar estão as duas na condição do while:
#   attempts < max_attempts   (ainda há tentativas)
#   not won                   (ainda não acertou)
# O won é um booleano "bandeira" (aula 5): começa False, passa a True ao acertar.
#
# Depois do ciclo, para saber PORQUÊ parou, olha-se para a bandeira.
# (com while True + break também dava: o break ao acertar, e um if no fim
#  de cada volta para as tentativas esgotadas)
#
# A pergunta da paragem: o attempts sobe 1 em cada volta,
# por isso o ciclo acaba no máximo ao fim de max_attempts voltas.






# %%
# Solução do Ex 30.6

def sum_digits(n: int) -> int:
    """ Sum of the digits of n.
        Precondition: n >= 0
    """
    total = 0
    while n > 0:
        total += n % 10
        n //= 10
    return total

def digital_root(n: int) -> int:
    """ Repeated sum of digits, until one digit is left.
        Precondition: n >= 0
    """
    while n >= 10:              # enquanto tiver mais de um algarismo
        n = sum_digits(n)
    return n

print(digital_root(9875), digital_root(7), digital_root(0))     # 2 7 0

# Partir em peças: a sum_digits (CONSOLIDAR 1 da aula 7) faz uma soma;
# a digital_root repete-a enquanto for preciso.
# Fica um while por função, cada um fácil de ler.
#
# A pergunta da paragem: para n >= 10, a soma dos algarismos é sempre
# menor do que n. O n diminui sempre, e um dia fica < 10.






# %%
# Solução do Ex 30.7

import math

PRECISION = 1e-12

def my_sqrt(x: float) -> float:
    """ Square root of x, by bisection.
        Precondition: x >= 0
    """
    lo = 0.0
    hi = x
    if x < 1:
        hi = 1.0                # para x < 1, a raiz é maior do que x
    while hi - lo > PRECISION:
        mid = (lo + hi) / 2
        if mid * mid < x:
            lo = mid            # a raiz está na metade de cima
        else:
            hi = mid            # a raiz está na metade de baixo
    return (lo + hi) / 2        # o meio do intervalo final

print(my_sqrt(2.0))                                  # 1.414213562...
print(my_sqrt(9.0))                                  # 3.0000000000...
print(math.isclose(my_sqrt(2.0), math.sqrt(2.0)))    # True
print(math.isclose(my_sqrt(0.25), 0.5))              # True

# É a pesquisa binária do Hi-Lo (ex 47), com reais:
# em vez de "maior/menor" do utilizador, a pergunta é mid * mid < x.
#
# A pergunta da paragem: o intervalo fica com metade do tamanho em cada volta,
# por isso um dia fica mais pequeno do que 1e-12.
# Repara que a condição é "hi - lo > PRECISION", e não um == entre floats.
#
# Porquê o caso x < 1? A raiz de 0.25 é 0.5, que é MAIOR do que 0.25:
# com hi = x, a raiz ficava fora do intervalo.






# %%
# Solução do Ex 30.8

def zeno_terms(eps: float) -> int:
    """ Number of terms of 1/2 + 1/4 + ... so that 1 - sum < eps.
        Precondition: eps > 0
    """
    total = 0.0
    term = 0.5
    count = 0
    while 1 - total >= eps:     # ainda falta mais do que eps
        total += term
        term /= 2
        count += 1
    return count

print(zeno_terms(0.1), zeno_terms(0.01), zeno_terms(0.5))     # 4 7 2

# Com o k dado (o teste), o intervalo está bem definido: for i in range(k).
# Sem o k, só se sabe quando parar: while.
# O mesmo problema, os dois ciclos, cada um no seu caso.
#
# A pergunta da paragem: o que falta para 1 fica a metade em cada volta
# (0.5, 0.25, 0.125, ...), por isso um dia fica abaixo de eps (se eps > 0).
# Com eps = 0 nunca acabava: daí a precondição.