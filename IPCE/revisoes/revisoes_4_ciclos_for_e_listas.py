# %%
"""
===========================================================================
REVISÕES 4 - CICLOS FOR E LISTAS
for com range, listas/tuplos/conjuntos, o operador in, percorrer listas
IPCE 2026/2027
===========================================================================

Antes deste ficheiro: revisoes_3_condicoes_e_if.py

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 11. O ciclo for com range (e for dentro de for) .............. [~70 min]
       Porquê: repetir uma tarefa 10, 1000 ou n vezes sem escrever o código
       n vezes. Somar, contar, desenhar, procurar: quase todos os exercícios
       de teste têm um ciclo.

 12. Listas e outras coleções: [], (), {} ...................... [~40 min]
       Porquê: guardar muitos valores numa só variável
       (as notas de uma turma, os dias de cada mês).

 13. O operador in ............................................ [~20 min]
       Porquê: "este valor está lá dentro?" numa só expressão,
       em vez de uma fila de or.

 14. Percorrer listas e strings com for ....................... [~50 min]
       Porquê: é a combinação dos 3 anteriores, e é o que os testes
       mais pedem: somar, contar, procurar, encontrar o máximo, verificar.

  Resumo + soluções dos desafios

  Total: ~3h.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Corre as células POR ORDEM (partilham a memória).
  - Níveis:
      [EXEMPLO]  resolvido: lê, corre, muda valores.
      [FAZ]      contigo. Solução na célula a seguir, depois de muito espaço.
      [PENSA]    contigo. A "solução" é só a ideia, sem código.
      [DESAFIO]  nível de teste. Solução no fim do ficheiro.
      [SOZINHA]  sem solução.
  - Se encravares: email ao professor das práticas com o número do exercício,
    o que já fizeste, onde encravaste e o ficheiro em anexo.

  A numeração continua a dos ficheiros anteriores: este começa na secção 11.
"""






# %%
"""
===========================================================================
11. O CICLO FOR COM RANGE [~70 min]
===========================================================================

Ex 11.1 [EXEMPLO] Repetir uma tarefa ----------------------------------------
"""

for i in range(5):
    print("volta número", i)
print("acabou")

# Output:
#   volta número 0
#   volta número 1
#   volta número 2
#   volta número 3
#   volta número 4
#   acabou
#
# As linhas indentadas debaixo do for (o CORPO do ciclo) repetem-se.
# range(5) dá os números 0, 1, 2, 3, 4: são 5 voltas.
# Em cada volta, a variável i tem o número seguinte.
# Cada volta chama-se uma ITERAÇÃO.
#
# O print("acabou") não está indentado: corre uma vez, depois do ciclo.
# Os ":" no fim da linha do for são obrigatórios (tal como no if e no def).






# %%
"""
Ex 11.2 [EXEMPLO] As formas do range ---------------------------------------

    Antes de correr cada linha, tenta adivinhar os números.
"""

for i in range(2, 6):           # INÍCIO e FIM
    print(i, end=" ")           # 2 3 4 5
print()

for i in range(0, 20, 5):       # INÍCIO, FIM e PASSO
    print(i, end=" ")           # 0 5 10 15
print()

for i in range(10, 0, -2):      # PASSO negativo: a descer
    print(i, end=" ")           # 10 8 6 4 2
print()

for i in range(5, 5):           # vazio: o corpo não corre nenhuma vez
    print(i, end=" ")
print("(nada)")

# range(FIM)                 -> 0, 1, ..., FIM-1
# range(INÍCIO, FIM)         -> INÍCIO, INÍCIO+1, ..., FIM-1
# range(INÍCIO, FIM, PASSO)  -> de PASSO em PASSO
#
# O FIM NUNCA ESTÁ INCLUÍDO. O range pára ANTES de lá chegar.
# range(n) dá exatamente n números.
#
# O print(..., end=" ") escreve tudo na mesma linha (ficheiro 1, Ex 3.1).
# O print() vazio muda de linha no fim.






# %%
"""
Ex 11.3 [EXEMPLO] Tabuada do 7 ---------------------------------------------
"""

for i in range(1, 11):
    print(f"7 x {i} = {7 * i}")

# Sem o ciclo seriam 10 prints. Com a tabuada até 1000, 1000 prints.
# Com o ciclo, muda-se um número no range.






# %%
"""
Ex 11.4 [EXEMPLO] O padrão ACUMULADOR -------------------------------------

    Somar 1 + 2 + ... + n.
"""

def sum_up_to(n: int) -> int:
    """ Sum 1 + 2 + ... + n.
        Precondition: n >= 0
    """
    total = 0                           # 1. ANTES do ciclo: valor inicial
    for i in range(1, n + 1):           #    (n + 1 para o n estar incluído)
        total += i                      # 2. DENTRO: juntar alguma coisa
    return total                        # 3. DEPOIS: devolver

print(sum_up_to(4))         # 10
print(sum_up_to(100))       # 5050

# Executar à mão sum_up_to(4):
#   antes:  total = 0
#   i = 1:  total = 1
#   i = 2:  total = 3
#   i = 3:  total = 6
#   i = 4:  total = 10
#   return 10
#
# O valor inicial é o "elemento neutro" da operação:
#   somas    -> começa em 0  (x + 0 = x)
#   produtos -> começa em 1  (x * 1 = x). Se começasse em 0, dava sempre 0.

def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1
    for i in range(2, n + 1):
        fact *= i
    return fact

print(factorial(5))         # 120

# Compara com o factorial recursivo do ficheiro 3 (Ex 9.2):
# fazem o mesmo, de formas diferentes. A teórica usa as duas.






# %%
"""
Ex 11.5 [EXEMPLO] O padrão CONTADOR: for + if --------------------------------

    Quantos múltiplos de 3 há entre 1 e 100?
"""

def count_multiples_of_3(limit: int) -> int:
    """ How many multiples of 3 between 1 and limit (inclusive). """
    count = 0
    for i in range(1, limit + 1):
        if i % 3 == 0:
            count += 1
    return count

print(count_multiples_of_3(100))     # 33
print(count_multiples_of_3(10))      # 3   (3, 6, 9)

# O contador é um acumulador que soma sempre 1, mas só quando a condição é True.
# O if está DENTRO do for: é testado em cada volta.






# %%
"""
Ex 11.6 [EXEMPLO] Off-by-one: o erro mais comum dos ciclos ------------------
"""

total = 0
for i in range(1, 10):
    total += i
print(total)            # 45  -> queríamos 1 + ... + 10 = 55. Falta o 10!

total = 0
for i in range(1, 11):
    total += i
print(total)            # 55

# "Off-by-one" = o ciclo dá uma volta a mais ou a menos.
#
# Regras práticas:
#   "de A até B, com B incluído"  -> range(A, B + 1)
#   "n vezes"                     -> range(n)        (0, 1, ..., n-1)
#
# Truque para não falhar: testa SEMPRE com um caso pequeno à mão
# (n = 1, n = 2) e confirma a primeira e a última volta.






# %%
"""
Ex 11.7 [EXEMPLO] Os 3 bugs clássicos do acumulador (ex 22 dos guiões) ------

    As 4 funções deviam calcular 0 + 1 + ... + (n-1). Só a f1 está certa.
"""

def f1(n: int) -> int:
    total = 0
    for i in range(n):
        total += i
    return total

def f2(n: int) -> int:
    total = 0
    for i in range(n):
        total += i
        return total            # <- return DENTRO do ciclo

def f3(n: int) -> int:
    for i in range(n):
        total = 0               # <- inicialização DENTRO do ciclo
        total += i
    return total

def f4(n: int) -> int:
    for i in range(n):          # <- falta o "total = 0"
        total += i
    return total

print(f1(5))        # 10
print(f2(5))        # 0  -> o return termina a função logo na 1ª volta (i = 0)
print(f3(5))        # 4  -> o total volta a 0 em cada volta: só fica a última
# print(f4(5))      # <- descomenta: UnboundLocalError (total nunca teve valor)

# A indentação decide se uma linha está dentro ou fora do ciclo.
# Uma linha mal indentada muda completamente o resultado.
# Estes 3 erros aparecem em testes, muitas vezes disfarçados.






# %%
"""
Ex 11.8 [EXEMPLO] Um for que escreve: desenhar com caracteres (ex 32 dos guiões)
"""

def draw_line(c: str, n: int) -> None:
    """ Print the char c n times, then change line.
        Precondition: len(c) == 1 and n >= 0
    """
    for _ in range(n):
        print(c, end="")
    print()

def draw_square(c: str, n: int) -> None:
    """ Print a square of side n with the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for _ in range(n):
        draw_line(c, n)

draw_line("*", 5)
print()
draw_square("#", 3)

# Output:
#   *****
#
#   ###
#   ###
#   ###
#
# Aqui o for não acumula nada: em cada volta só ESCREVE.
#
# "for _ in range(n)": o "_" é um nome de variável que diz
# "não vou usar o número da volta, só quero repetir n vezes".
#
# draw_square tem um ciclo que chama uma função que tem outro ciclo:
# é um ciclo dentro de um ciclo, arrumado em duas funções.
# Para cada uma das n linhas, a draw_line faz as suas n voltas: n * n asteriscos.






# %%
"""
Ex 11.9 [EXEMPLO] Um for dentro de outro for ----------------------------------
"""

for i in range(3):
    for j in range(2):
        print(f"i={i} j={j}")
    print("--- fim da volta", i, "do ciclo de fora")

# Output:
#   i=0 j=0
#   i=0 j=1
#   --- fim da volta 0 do ciclo de fora
#   i=1 j=0
#   i=1 j=1
#   --- fim da volta 1 do ciclo de fora
#   i=2 j=0
#   i=2 j=1
#   --- fim da volta 2 do ciclo de fora
#
# Para CADA volta do ciclo de fora, o ciclo de dentro dá TODAS as suas voltas.
# O corpo de dentro corre 3 * 2 = 6 vezes.
# Chama-se ciclos ENCAIXADOS (em inglês "nested loops").
#
# É como um relógio: o j (minutos) dá a volta completa
# antes de o i (horas) avançar uma posição.
#
# A indentação diz a que ciclo pertence cada linha:
#   - o print com i e j está DENTRO do for j  -> corre 6 vezes;
#   - o print "--- fim" está dentro do for i, mas FORA do for j -> corre 3 vezes.
#
# Os nomes das variáveis TÊM de ser diferentes (i e j).
# Com o mesmo nome, o ciclo de dentro estragava a variável do de fora.

# A tabuada completa, de 1 a 5:
for i in range(1, 6):
    for j in range(1, 6):
        print(f"{i * j:3}", end="")     # :3 -> cada número ocupa 3 espaços
    print()                             # muda de linha no fim de cada linha da tabela






# %%
"""
Ex 11.10 [EXEMPLO] O ciclo de dentro pode depender do de fora ----------------
"""

def draw_triangle_nested(c: str, n: int) -> None:
    """ Print a triangle with n lines using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(1, n + 1):           # linha número i (1, 2, ..., n)
        for _ in range(i):              # a linha i tem i caracteres
            print(c, end="")
        print()                         # fim da linha

draw_triangle_nested("*", 4)

# Output:
#   *
#   **
#   ***
#   ****
#
# Aqui o ciclo de dentro usa o i do ciclo de fora: range(i).
# Na 1ª linha dá 1 volta, na 2ª dá 2 voltas, etc.
# Total de caracteres: 1 + 2 + 3 + 4 = 10.
#
# Compara com o draw_square do Ex 11.8:
#     for _ in range(n):
#         draw_line(c, n)
# É EXATAMENTE a mesma ideia: um ciclo que chama uma função que tem outro ciclo
# é um ciclo encaixado, só que arrumado em duas funções.
# As duas formas estão certas. Com funções, cada uma faz uma coisa só,
# e é mais fácil ler e testar (a draw_line pode ser testada sozinha).
# Nos desenhos complicados (Ex 11.22) as funções auxiliares ajudam muito.






# %%
"""
Ex 11.11 [EXEMPLO] Procurar e sair mais cedo: return dentro do for ------------

    Há algum múltiplo de 7 entre a e b?
"""

def has_multiple_of_7(a: int, b: int) -> bool:
    """ Check if there is a multiple of 7 in [a, b]. """
    for i in range(a, b + 1):
        if i % 7 == 0:
            return True         # encontrei um: não preciso de ver o resto
    return False                # FORA do ciclo: só chego aqui se vi TODOS

print(has_multiple_of_7(1, 10))     # True   (o 7)
print(has_multiple_of_7(8, 13))     # False

# O ERRO clássico (está na teórica 04a):
def has_multiple_of_7_wrong(a: int, b: int) -> bool:
    """ WRONG version. """
    for i in range(a, b + 1):
        if i % 7 == 0:
            return True
        else:
            return False        # <- decide logo na 1ª volta!

print(has_multiple_of_7_wrong(1, 10))   # False (!!) só olhou para o 1

# Só se pode dizer "NÃO há" depois de ver TODOS.
# Só se pode dizer "HÁ" assim que se encontra um.
# Por isso: return True dentro do ciclo, return False depois do ciclo.
#
# Este padrão ("procurar e sair") aparece em quase todos os testes.






# %%
"""
Ex 11.12 [FAZ] Somas (ex 19a e 19b dos guiões) ------------------------------

    a) sum_naturals(n): 0 + 1 + ... + (n-1)        sum_naturals(4) == 6
    b) sum_squares(n):  0 + 1 + 4 + ... + (n-1)²   sum_squares(4) == 14
"""

def sum_naturals(n: int) -> int:
    """ Sum of the first n naturals: 0 + 1 + ... + (n-1).
        Precondition: n >= 0
    """
    pass        # TODO

def sum_squares(n: int) -> int:
    """ Sum of the first n perfect squares: 0 + 1 + 4 + ... + (n-1)^2.
        Precondition: n >= 0
    """
    pass        # TODO

print(sum_naturals(4), sum_naturals(0))     # 6 0
print(sum_squares(4), sum_squares(1))       # 14 0
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 11.12

def sum_naturals(n: int) -> int:
    """ Sum of the first n naturals: 0 + 1 + ... + (n-1).
        Precondition: n >= 0
    """
    total = 0
    for i in range(n):
        total += i
    return total

def sum_squares(n: int) -> int:
    """ Sum of the first n perfect squares: 0 + 1 + 4 + ... + (n-1)^2.
        Precondition: n >= 0
    """
    total = 0
    for i in range(n):
        total += i * i
    return total

print(sum_naturals(4), sum_naturals(0))     # 6 0
print(sum_squares(4), sum_squares(1))       # 14 0

# As duas só diferem numa linha: O QUE se acumula.
# range(n) dá exatamente os n primeiros naturais: 0, 1, ..., n-1.
# Com n = 0 o range é vazio: o ciclo não corre e devolve 0. Certo!






# %%
"""
Ex 11.13 [FAZ] Divisores e números primos (teórica 03b) ------------------

    a) count_divisors(n): quantos divisores tem n (de 1 a n).
           count_divisors(12) == 6     (1, 2, 3, 4, 6, 12)
    b) is_prime(n): n é primo se tiver EXATAMENTE 2 divisores.
           is_prime(7) == True     is_prime(12) == False     is_prime(1) == False
       Usa a count_divisors, sem ciclo nenhum.
"""

def count_divisors(n: int) -> int:
    """ Number of divisors of n.
        Precondition: n > 0
    """
    pass        # TODO

def is_prime(n: int) -> bool:
    """ Check if n is a prime number.
        Precondition: n > 0
    """
    pass        # TODO

print(count_divisors(12), count_divisors(1), count_divisors(7))     # 6 1 2
print(is_prime(7), is_prime(12), is_prime(1), is_prime(2))          # True False False True
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 11.13

def count_divisors(n: int) -> int:
    """ Number of divisors of n.
        Precondition: n > 0
    """
    count = 0
    for d in range(1, n + 1):
        if n % d == 0:
            count += 1
    return count

def is_prime(n: int) -> bool:
    """ Check if n is a prime number.
        Precondition: n > 0
    """
    return count_divisors(n) == 2

print(count_divisors(12), count_divisors(1), count_divisors(7))     # 6 1 2
print(is_prime(7), is_prime(12), is_prime(1), is_prime(2))          # True False False True

# O 1 não é primo: só tem 1 divisor. Por isso "== 2" e não "<= 2".
# (a teórica 03b escreve "<= 2", que dá is_prime(1) == True. Cuidado!)
#
# Esta solução é simples mas pouco eficiente: para n = 1000003 dá um milhão de voltas.
# Para saber se é primo, bastava procurar UM divisor entre 2 e n-1
# e sair logo com return False (o padrão do Ex 11.11).






# %%
"""
Ex 11.14 [FAZ] Triângulo de asteriscos (ex 33 dos guiões) ------------------

    draw_triangle(c, n) desenha (para n = 4):
        *
        **
        ***
        ****
    Usa a draw_line do Ex 11.8 (corre essa célula antes, para ela existir).
    Não copies o Ex 11.10: o objetivo aqui é usar a função.
"""

def draw_triangle(c: str, n: int) -> None:
    """ Print a triangle with n lines using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    pass        # TODO

draw_triangle("*", 4)
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 11.14

def draw_line(c: str, n: int) -> None:
    """ Print the char c n times, then change line.
        Precondition: len(c) == 1 and n >= 0
    """
    for _ in range(n):
        print(c, end="")
    print()

def draw_triangle(c: str, n: int) -> None:
    """ Print a triangle with n lines using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(1, n + 1):
        draw_line(c, i)

draw_triangle("*", 4)

# A linha número i tem i asteriscos. Basta pôr o range a ir de 1 a n.
# Com range(n) teria de ser draw_line(c, i + 1).
#
# A draw_line está repetida aqui só para esta célula funcionar sozinha.
#
# A mesma coisa com dois for encaixados, sem a função auxiliar (Ex 11.10):
#     for i in range(1, n + 1):
#         for _ in range(i):
#             print(c, end="")
#         print()
#
# E o triângulo ao contrário (ex 34 dos guiões)?
# Experimenta: for i in range(n, 0, -1)






# %%
"""
Ex 11.15 [FAZ] Pares com soma dada (for encaixados) -----------------------

    count_pairs(n, target): quantos pares (i, j), com i e j entre 1 e n,
    têm soma igual a target. A ordem conta: (1, 3) e (3, 1) são pares diferentes.
        count_pairs(3, 4) == 3      -> (1, 3), (2, 2), (3, 1)
        count_pairs(3, 7) == 0
"""

def count_pairs(n: int, target: int) -> int:
    """ Number of pairs (i, j), 1 <= i, j <= n, with i + j == target.
        Precondition: n >= 1
    """
    pass        # TODO

print(count_pairs(3, 4), count_pairs(3, 7), count_pairs(6, 7))     # 3 0 6















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 11.15

def count_pairs(n: int, target: int) -> int:
    """ Number of pairs (i, j), 1 <= i, j <= n, with i + j == target.
        Precondition: n >= 1
    """
    count = 0
    for i in range(1, n + 1):
        for j in range(1, n + 1):
            if i + j == target:
                count += 1
    return count

print(count_pairs(3, 4), count_pairs(3, 7), count_pairs(6, 7))     # 3 0 6

# Dois ciclos encaixados geram TODOS os pares (n * n), e o if conta os que servem.
# O contador está ANTES dos dois ciclos: se estivesse entre o for i e o for j,
# voltava a 0 em cada linha (o bug do f3 do Ex 11.7).
#
# Curiosidade: dava para fazer com UM ciclo só. Para cada i, o j é obrigatoriamente
# target - i: basta verificar se 1 <= target - i <= n.
# Menos trabalho, mas mais difícil de pensar. Primeiro a versão que funciona.






# %%
"""
Ex 11.16 [PENSA] Número perfeito (teórica 03b) ------------------------

    Um número é PERFEITO se for igual à soma dos seus divisores
    (sem contar com ele próprio). 6 = 1 + 2 + 3 é perfeito. 28 também.
        is_perfect(6) == True     is_perfect(8) == False

        def is_perfect(n: int) -> bool:
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 11.16 (sem código):
#
#   acumulador "soma" começa em 0
#   para cada d de 1 até n-1 (o próprio n fica de fora):
#       se d divide n (resto 0): somar d
#   no fim: devolver a comparação "n é igual à soma?"
#
# É um ACUMULADOR com if (só soma alguns valores),
# e no fim devolve diretamente uma condição (sem if/return True/return False).






# %%
"""
Ex 11.17 [PENSA] Ler e somar (ex 21 dos guiões) -----------------------

    Programa que pergunta quantos números se vão somar,
    lê esses números um a um e no fim escreve a soma.
        Introduza a quantidade de números a somar: 3
        1> 10
        2> 20
        3> 5
        35
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 11.17 (sem código):
#
#   função read_and_sum(n) -> int:
#       acumulador começa em 0
#       repetir n vezes, com i de 1 a n (para o prompt mostrar 1>, 2>, 3>):
#           ler um inteiro com o prompt f"{i}> " e somá-lo ao acumulador
#       devolver o acumulador
#   main: ler n, escrever read_and_sum(n)
#
# Repara que os números nunca são guardados todos: cada um é somado
# e a variável é reaproveitada na volta seguinte.
# (este é um caso raro em que a função de lógica também faz input:
#  o próprio enunciado mistura as duas coisas)






# %%
"""
Ex 11.18 [DESAFIO] Soma de múltiplos (Teste 1 2024/25, pergunta 3, 3 valores)

    Escreva uma função inteira para somar todos os múltiplos dum número
    inteiro positivo m que sejam menores ou iguais que outro número inteiro
    positivo lim. Exemplos:
        sum_multiples(10, 10) == 0+10 == 10
        sum_multiples(5, 10)  == 0+5+10 == 15
        sum_multiples(2, 10)  == 0+2+4+6+8+10 == 30
        sum_multiples(1, 10)  == 0+1+2+3+4+5+6+7+8+9+10 == 55

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def sum_multiples(m: int, lim: int) -> int:
            ''' Sum all the multiples of m until lim inclusive.
                Precondition: m > 0 and lim > 0
            '''

    Pista: "quanto menos trabalho desnecessário, melhor". Há uma solução
    que não precisa de if nenhum (olha outra vez para o Ex 11.2).
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 11.19 [DESAFIO] Série de Zeno (Teste 1 2025/26, pergunta 3, 4 valores) ---

    Considere a famosa série do paradoxo de Zeno. A soma desta série vale 1.
        1/2 + 1/4 + 1/8 + 1/16 + ...
    Escreva uma função real para calcular a soma dos primeiros k termos
    desta série. Exemplos:
        zeno(0) == 0.0      zeno(2) == 0.75
        zeno(1) == 0.5      zeno(3) == 0.875

    Programe a função usando um ciclo. Tente não usar a biblioteca math.
    Quanto mais simples, melhor. Não programe main, nem use input ou print.

        def zeno(k: int) -> float:
            ''' Sum of the k first terms of the Zeno series '''

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 11.20 [DESAFIO] Executar à mão (Teste 1 2024/25, 1c) -----------------

    Qual o valor que fica em x quando o ciclo termina?
        A. 195      B. 26      C. 28      D. 13
    Resolve à mão, sem correr! Pensa bem antes de fazer as 14 voltas...
    Solução no fim do ficheiro.
"""

x = 0
for i in range(0, 14, 1):
    if i % 3 == 0 and i < 20:
        x = 15 * i
    elif i % 5 == 0 or i % 4 == 0:
        x = (10 * i) / 5
    elif i % 7 == 0:
        x = i * 2
    else:
        x = i






# %%
"""
Ex 11.21 [DESAFIO] Ciclos encaixados à mão -------------------------------

    a) Qual o valor final de count?
    b) O que escreve o segundo bloco? (atenção aos espaços e às mudanças de linha)
    Sem correr! Solução no fim do ficheiro.
"""

count = 0
for i in range(4):
    for j in range(i, 4):
        count += 1
print(count)

for i in range(1, 4):
    for j in range(i):
        print(i * j, end=" ")
    print()






# %%
"""
Ex 11.22 [DESAFIO] Pinheiro de Natal (Teste 1 2024/25, pergunta 5, "Difícil")

    Escreva uma função para desenhar um pinheiro de Natal com copa de
    tamanho n. A copa é um triângulo isósceles com n linhas.
    Por baixo da copa fica o tronco, que tem sempre 3 linhas.
    A 1ª linha da copa (no topo) é uma estrela isolada '*'.
    As restantes n-1 linhas são agulhas '^'.
    O tronco é um quadrado 3x3 de madeira '#'.
    Para n=10: a 1ª linha tem 9 espaços à esquerda; a 2ª tem 8;
    a 10ª tem 0; as três linhas do tronco têm 8, tal como a 2ª linha.

    Para n = 5:
            *
           ^^^
          ^^^^^
         ^^^^^^^
        ^^^^^^^^^
           ###
           ###
           ###

    Código de partida dado no teste (uso opcional):
        def draw_segment(x: str, n: int):
            for i in range(n):
                print(x, end='')

        def draw_pine_tree(a: str, b: str, c: str, n: int):
            ''' Draw a Christmas pine tree.
                Arguments: a - star; b - pine needle; c - wood
                Precondition: len(a)==1 and len(b)==1 and len(c)==1 and n >= 3
            '''

    Pista: para a linha k da copa (k = 0, 1, ..., n-1), quantos espaços
    e quantos símbolos? Faz uma tabela para n = 5.
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 11.23 [SOZINHA] Série de ln(2) e moldura ---------------------------

    a) (Exame de Recurso 2023/24, pergunta 2, 2 valores)
       Esta famosa série alternada converge para o logaritmo natural de 2:
           1 - 1/2 + 1/3 - 1/4 + 1/5 - ...
       Escreva uma função real para calcular a soma dos n primeiros termos.
           ln2(0) == 0.0      ln2(2) == 0.5
           ln2(1) == 1.0      ln2(3) == 0.8333
       Escreva a função usando um ciclo. Não precisa da biblioteca math.
           def ln2(n: int) -> float:
       Pista: é parecido com o zeno, mas com um sinal que vai trocando.

    b) (ex 36 dos guiões) draw_frame(c, n): uma moldura quadrada de lado n
       (para n = 4):
           ****
           *  *
           *  *
           ****

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
12. LISTAS E OUTRAS COLEÇÕES: [], (), {} [~40 min]
===========================================================================

Ex 12.1 [EXEMPLO] Criar uma lista e aceder aos elementos -------------------
"""

grades = [12, 15, 9, 18, 14]

print(grades)               # [12, 15, 9, 18, 14]
print(len(grades))          # 5      -> número de elementos
print(grades[0])            # 12     -> o PRIMEIRO está na posição 0
print(grades[1])            # 15
print(grades[4])            # 14     -> o último está na posição len - 1
print(grades[-1])           # 14     -> índices negativos contam do fim
print(grades[-2])           # 18

# print(grades[5])          # <- descomenta: IndexError: list index out of range

empty = []
print(len(empty))           # 0

# Uma lista guarda vários valores, por ordem, entre [ ] e separados por vírgulas.
# Cada valor tem uma posição (ÍNDICE), que começa em 0.
# Numa lista com 5 elementos, os índices válidos são 0, 1, 2, 3, 4.
#
# IndexError = "não há nenhum elemento nessa posição".






# %%
"""
Ex 12.2 [EXEMPLO] Modificar uma lista ---------------------------------------
"""

grades = [12, 15, 9]

grades[2] = 10              # troca o elemento da posição 2
print(grades)               # [12, 15, 10]

grades.append(17)           # acrescenta no FIM
print(grades)               # [12, 15, 10, 17]

last = grades.pop()         # tira o último e devolve-o
print(last, grades)         # 17 [12, 15, 10]

grades.insert(0, 20)        # insere na posição 0 (os outros andam uma casa)
print(grades)               # [20, 12, 15, 10]

# As listas podem mudar depois de criadas: diz-se que são MUTÁVEIS.
# (as strings e os números não: "olá" é sempre "olá")
#
# grades.append(17) é uma chamada com um ponto: chama-se MÉTODO.
# É uma função que pertence à lista e mexe nela.
#
# Cuidado: grades[2] = 10 TROCA. grades.insert(2, 10) ACRESCENTA (a lista cresce).






# %%
"""
Ex 12.3 [EXEMPLO] Outras operações úteis ----------------------------------
"""

print([1, 2] + [3, 4])          # [1, 2, 3, 4]   junta duas listas (lista nova)
print([0] * 5)                  # [0, 0, 0, 0, 0] repete
print(list(range(5)))           # [0, 1, 2, 3, 4] lista a partir de um range

numbers = [4, 8, 1]
print(sum(numbers), max(numbers), min(numbers))     # 13 8 1

# sum, max e min são funções prontas do Python.
# São úteis para confirmar resultados, mas nos testes muitas vezes pede-se
# para programar a lógica à mão, com um ciclo (secção 14).
#
# Lembra-te: não chames sum, max, min ou list às tuas variáveis
# (ficheiro 1, Ex 1.6). A função original deixa de funcionar.






# %%
"""
Ex 12.4 [EXEMPLO] A surpresa das listas: duas variáveis, a MESMA lista --------

    No ficheiro 1 (Ex 1.3) viste que depois de "b = a", mudar o a não muda o b.
    Com listas, há uma surpresa.
"""

a = [1, 2, 3]
b = a                   # b NÃO é uma cópia: é outro nome para a MESMA lista
b.append(4)
print(a)                # [1, 2, 3, 4]  (!!) o a também mudou
print(b)                # [1, 2, 3, 4]

c = a.copy()            # agora sim, uma lista nova com os mesmos valores
c.append(5)
print(a)                # [1, 2, 3, 4]  -> o a não mudou
print(c)                # [1, 2, 3, 4, 5]

# O que acontece por dentro (teórica 03a):
# uma variável não "contém" o valor. Aponta para ele (é uma REFERÊNCIA).
# "b = a" põe o b a apontar para o mesmo objeto que o a.
#
# Com números isto não se nota: x += 1 cria um número NOVO e põe o x a apontar
# para ele. Os números não mudam (são imutáveis).
# Com listas nota-se: b.append(4) MUDA o objeto, e o a aponta para o mesmo objeto.
#
# Se queres uma cópia independente: a.copy() (ou list(a)).






# %%
"""
Ex 12.5 [EXEMPLO] Uma função pode mudar a lista que recebe -------------------
"""

def add_zero(l: list[int]) -> None:
    """ Append a 0 to the list. """
    l.append(0)

def add_one(n: int) -> None:
    """ Try to add one to n (it does not work outside!). """
    n += 1

numbers = [5, 6]
add_zero(numbers)
print(numbers)          # [5, 6, 0]  (!!) a lista de fora mudou

x = 5
add_one(x)
print(x)                # 5          -> com um número não muda (ficheiro 2, Ex 6.2)

# O parâmetro l aponta para a MESMA lista que o numbers (como no Ex 12.4).
# Quando a função faz l.append(0), muda essa lista.
#
# Com o número, n += 1 cria um número novo dentro da função.
# O x de fora continua a apontar para o 5.
#
# Por isso há dois tipos de funções sobre listas:
#   - as que MUDAM a lista recebida: devolvem None (ex: add_zero, list.sort);
#   - as que CRIAM uma lista nova e a devolvem com return (secção 14).
# A docstring deve deixar claro qual é o caso.
#
# (a cadeira chama a "list[int]" do cabeçalho uma lista de inteiros)






# %%
"""
Ex 12.6 [EXEMPLO] Tuplos: listas que não mudam ---------------------------
"""

point = (3, 4)
print(point[0], point[1])       # 3 4
print(len(point))               # 2

# point[0] = 10                 # <- descomenta: TypeError: 'tuple' object does not support item assignment

DURATIONS = (31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31)
print(DURATIONS[1])             # 28 (fevereiro, que é o mês 2, está na posição 1)

# Um tuplo escreve-se com ( ) e funciona como uma lista para LER:
# índices, len, for (secção 14), in (secção 13).
# Mas é IMUTÁVEL: não tem append, pop, nem se pode trocar elementos.
#
# Usa-se para dados fixos (como os dias de cada mês) ou para juntar
# poucos valores que andam sempre juntos (um ponto (x, y), uma data).






# %%
"""
Ex 12.7 [EXEMPLO] Conjuntos e dicionários: { } --------------------------------
"""

colors = {"vermelho", "azul", "vermelho", "verde"}
print(colors)               # {'vermelho', 'azul', 'verde'} (ordem pode variar)
print(len(colors))          # 3   -> os repetidos desaparecem

# print(colors[0])          # <- descomenta: TypeError: 'set' object is not subscriptable

days = {"janeiro": 31, "fevereiro": 28}
print(days["janeiro"])      # 31

print(type({}))             # <class 'dict'> (!!) o {} vazio é um DICIONÁRIO
print(type(set()))          # <class 'set'>  -> conjunto vazio escreve-se set()

# CONJUNTO {1, 2, 3}: sem repetidos, sem ordem, sem índices.
#   Serve sobretudo para perguntar "está lá?" (secção 13), e é muito rápido nisso.
#
# DICIONÁRIO {chave: valor}: associa cada chave a um valor.
#   d["janeiro"] dá o valor associado à chave "janeiro".
#   Os dicionários vêm a sério mais à frente na cadeira.
#
# Resumo:
#   [1, 2, 3]       lista      ordem, índices, repetidos, MUTÁVEL
#   (1, 2, 3)       tuplo      ordem, índices, repetidos, IMUTÁVEL
#   {1, 2, 3}       conjunto   sem ordem, sem índices, sem repetidos
#   {"a": 1}        dicionário chave -> valor
#   {}              dicionário VAZIO (não é um conjunto!)






# %%
"""
Ex 12.8 [FAZ] Prever --------------------------------------------------

    Escreve o output de cada print, sem correr. Depois corre a solução.
"""

l = [10, 20, 30, 40]
print(len(l))               # a)
print(l[1] + l[-1])         # b)
l[0] = l[3] // 2
print(l)                    # c)
l.append(l[0])
print(len(l), l[-1])        # d)
x = l.pop()
print(x, l)                 # e)

# TODO: as tuas respostas
# a)
# b)
# c)
# d)
# e)
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 12.8
#
#   a) 4
#   b) 60               -> l[1] = 20, l[-1] = 40
#   c) [20, 20, 30, 40] -> l[0] = 40 // 2 = 20
#   d) 5 20             -> acrescentou o l[0] (20) no fim
#   e) 20 [20, 20, 30, 40]  -> o pop tira o último (20) e devolve-o






# %%
"""
Ex 12.9 [FAZ] Dias de um mês com uma lista ----------------------------

    Reescreve o month_length (ficheiro 3, Ex 8.13) usando uma lista com
    os dias de cada mês. Já não precisas de ifs para os meses de 30 e 31!
    Só fevereiro, nos anos bissextos, precisa de tratamento especial.
"""

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def month_length(month: int, year: int) -> int:
    """ Number of days of a given month.
        Precondition: 1 <= month <= 12
    """
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    pass        # TODO

print(month_length(1, 2023), month_length(4, 2023),
      month_length(2, 2024), month_length(2, 2023))       # 31 30 29 28
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 12.9

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def month_length(month: int, year: int) -> int:
    """ Number of days of a given month.
        Precondition: 1 <= month <= 12
    """
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    if month == 2 and is_leap_year(year):
        return 29
    return DURATIONS[month - 1]

print(month_length(1, 2023), month_length(4, 2023),
      month_length(2, 2024), month_length(2, 2023))       # 31 30 29 28

# DURATIONS[month - 1]: o mês 1 (janeiro) está na posição 0.
# Esquecer o "- 1" é um off-by-one: dava os dias do mês seguinte,
# e month_length(12, ...) dava IndexError.
#
# DURATIONS é uma constante LOCAL (só a função precisa dela).
# É exatamente o que aparece no Teste 1 2025/26, pergunta 4 (Ex 14.17).






# %%
"""
Ex 12.10 [PENSA] Trocar o primeiro com o último -----------------------

    swap_ends(l): troca o primeiro e o último elemento da lista,
    MUDANDO a própria lista (não devolve nada).
        l = [1, 2, 3, 4]
        swap_ends(l)
        print(l)            # [4, 2, 3, 1]
    Precondição: a lista não está vazia.
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 12.10 (sem código):
#
#   Cabeçalho: def swap_ends(l: list[int]) -> None:
#   Uma única linha, com atribuição paralela (ficheiro 1, Ex 1.8):
#       o elemento da posição 0 e o da posição -1 trocam de lugar.
#
#   Não há return: a função muda a lista que recebeu (Ex 12.5).
#   E se a lista tiver só 1 elemento? Troca-o com ele próprio: fica igual. Certo.






# %%
"""
Ex 12.11 [DESAFIO] Referências: o que escreve? ----------------------------

    Sem correr! Solução no fim do ficheiro.
"""

def change(l: list[int]) -> None:
    l.append(0)
    l = [9, 9]
    l.append(1)

a = [1, 2]
b = a
c = a.copy()
b.append(3)
c[0] = 7
change(a)
print(a)
print(b)
print(c)






# %%
"""
Ex 12.12 [SOZINHA] Rodar uma lista ------------------------------------

    rotate_left(l): passa o primeiro elemento para o fim, mudando a lista.
        l = [1, 2, 3, 4]
        rotate_left(l)
        print(l)            # [2, 3, 4, 1]
    Pista: o pop aceita uma posição (l.pop(0) tira o primeiro).

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
13. O OPERADOR IN [~20 min]
===========================================================================

Ex 13.1 [EXEMPLO] "Está lá dentro?" ----------------------------------------

    Antes de cada linha, adivinha se dá True ou False.
"""

print(3 in [1, 3, 5])                   # True    lista
print(3 in (1, 3, 5))                   # True    tuplo
print(3 in {1, 3, 5})                   # True    conjunto
print(4 not in {1, 3, 5})               # True    "not in" é o contrário

print(7 in range(1, 7))                 # False   o range não inclui o 7!
print(10 in range(0, 20, 5))            # True    0, 5, 10, 15

print("a" in "banana")                  # True    uma letra numa string
print("nan" in "banana")                # True    até um pedaço de texto
print("bn" in "banana")                 # False   tem de estar seguido

print("janeiro" in {"janeiro": 31})     # True    num dicionário, procura nas CHAVES
print(31 in {"janeiro": 31})            # False   ...e não nos valores

# VALOR in COLEÇÃO dá True ou False. Funciona com todas as coleções.






# %%
"""
Ex 13.2 [EXEMPLO] Dois "in" diferentes ------------------------------------
"""

for x in [10, 20]:              # in do for: PERCORRE (um valor por volta)
    print(x)

print(20 in [10, 20])           # in sozinho: PERGUNTA (dá True/False)

# Escrevem-se igual, mas são coisas diferentes:
#   for x in coleção:      -> repete o corpo, com x igual a cada valor
#   x in coleção           -> é uma condição: True se x estiver lá






# %%
"""
Ex 13.3 [EXEMPLO] in em vez de uma fila de or -----------------------------
"""

def has_30_days(month: int) -> bool:
    """ Check if a month has 30 days. """
    return month in {4, 6, 9, 11}

    # Sem o in, era assim (ficheiro 3, Ex 8.13):
    # return month == 4 or month == 6 or month == 9 or month == 11

def is_vowel(c: str) -> bool:
    """ Check if c is a lowercase vowel.
        Precondition: len(c) == 1
    """
    return c in "aeiou"

print(has_30_days(4), has_30_days(5))       # True False
print(is_vowel("e"), is_vowel("x"))         # True False

# Porquê a precondição len(c) == 1 no is_vowel?
print("ae" in "aeiou")      # True (!!) "ae" é um pedaço de "aeiou"
print("" in "aeiou")        # True (!!) a string vazia está em todas
# Sem a precondição, is_vowel("ae") diria True.






# %%
"""
Ex 13.4 [FAZ] Funções com in ------------------------------------------
"""

def is_weekend(day: str) -> bool:
    """ Check if day ("segunda", ..., "domingo") is a weekend day. """
    pass        # TODO

def is_yes_or_no(answer: str) -> bool:
    """ Check if answer is one of "s", "S", "n", "N". """
    pass        # TODO

print(is_weekend("sábado"), is_weekend("terça"))        # True False
print(is_yes_or_no("S"), is_yes_or_no("talvez"))        # True False
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 13.4

def is_weekend(day: str) -> bool:
    """ Check if day ("segunda", ..., "domingo") is a weekend day. """
    return day in {"sábado", "domingo"}

def is_yes_or_no(answer: str) -> bool:
    """ Check if answer is one of "s", "S", "n", "N". """
    return answer in {"s", "S", "n", "N"}

print(is_weekend("sábado"), is_weekend("terça"))        # True False
print(is_yes_or_no("S"), is_yes_or_no("talvez"))        # True False

# Também funciona com lista ou tuplo: ["sábado", "domingo"].
# O conjunto é a escolha natural para "é um destes?": a ordem não interessa.
#
# Cuidado com is_yes_or_no usando uma string: answer in "sSnN"
# dava True para "sS" e para "" (Ex 13.3).






# %%
"""
Ex 13.5 [DESAFIO] True ou False? -----------------------------------------

    Sem correr! Solução no fim do ficheiro.
      a) "" in "abc"                    e) [1] in [1, 2]
      b) "ac" in "abc"                  f) 2 in {1: "a", 2: "b"}
      c) 15 in range(0, 15, 5)          g) "a" in {1: "a"}
      d) 15 in range(0, 16, 5)          h) "Abc" in "abcAbc"
"""






# %%
"""
Ex 13.6 [SOZINHA] Tipo de carácter ------------------------------------

    char_kind(c) devolve:
        "vogal", "consoante", "dígito" ou "outro"
    (só letras minúsculas sem acentos; precondição: len(c) == 1)
        char_kind("a") == "vogal"       char_kind("b") == "consoante"
        char_kind("7") == "dígito"      char_kind("?") == "outro"

    Pista: guarda as letras e os dígitos em constantes do tipo string
    ("abcdefghijklmnopqrstuvwxyz", "0123456789") e usa in.
    Pensa na ORDEM dos ramos (ficheiro 3, Ex 8.4).

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
14. PERCORRER LISTAS E STRINGS COM FOR [~50 min]
===========================================================================

Ex 14.1 [EXEMPLO] for diretamente sobre uma coleção ------------------------
"""

grades = [12, 15, 9]
for g in grades:
    print(g)                # 12, 15, 9 (um por volta)

for c in "olá":
    print(c)                # o, l, á  (uma string é uma sequência de caracteres)

for v in (1, 2):
    print(v * 10)           # 10, 20

# O for não serve só para o range: percorre QUALQUER coleção.
# Em cada volta, a variável recebe o elemento seguinte.
# (o range é só mais uma coleção: uma sequência de inteiros)






# %%
"""
Ex 14.2 [EXEMPLO] Pelos valores ou pelos índices? ----------------------------
"""

grades = [12, 15, 9]

for g in grades:                    # 1ª forma: pelos VALORES
    print(g)

for i in range(len(grades)):        # 2ª forma: pelos ÍNDICES (0, 1, 2)
    print(i, grades[i])

# 1ª forma: mais simples. Usa-se quando só interessa o valor.
# 2ª forma: usa-se quando interessa a POSIÇÃO.
#   - "a posição onde está o máximo"
#   - "os elementos nas posições pares"
#   - comparar um elemento com o seguinte (l[i] e l[i + 1])

def has_two_consecutive_equal(l: list[int]) -> bool:
    """ Check if two consecutive elements are equal. """
    for i in range(len(l) - 1):         # porquê "- 1"? ver em baixo
        if l[i] == l[i + 1]:
            return True
    return False

print(has_two_consecutive_equal([1, 2, 3]))         # False
print(has_two_consecutive_equal([1, 2, 2, 3]))      # True

# range(len(l) - 1): o último i é len(l) - 2, e o l[i + 1] é o último elemento.
# Com range(len(l)), no último i o l[i + 1] dava IndexError.






# %%
"""
Ex 14.3 [EXEMPLO] Somar e contar numa lista --------------------------------
"""

def sum_list(l: list[int]) -> int:
    """ Sum of the elements. """
    total = 0
    for v in l:
        total += v
    return total

def count_even(l: list[int]) -> int:
    """ How many elements are even. """
    count = 0
    for v in l:
        if v % 2 == 0:
            count += 1
    return count

numbers = [3, 8, 10, 7, 2]
print(sum_list(numbers))        # 30
print(count_even(numbers))      # 3
print(sum_list([]))             # 0   (lista vazia: o ciclo não corre)

# São os mesmos acumulador e contador da secção 11,
# mas a percorrer uma lista em vez de um range.






# %%
"""
Ex 14.4 [EXEMPLO] O máximo de uma lista (e um bug clássico) ----------------
"""

def maximum_wrong(l: list[int]) -> int:
    """ Largest element (WITH A BUG). """
    largest = 0
    for v in l:
        if v > largest:
            largest = v
    return largest

def maximum(l: list[int]) -> int:
    """ Largest element.
        Precondition: len(l) > 0
    """
    largest = l[0]              # começar com um elemento da PRÓPRIA lista
    for v in l:
        if v > largest:
            largest = v
    return largest

print(maximum_wrong([3, 8, 2]), maximum([3, 8, 2]))         # 8 8
print(maximum_wrong([-5, -2, -9]), maximum([-5, -2, -9]))   # 0 -2  (!!)

# Na versão errada, o "maior até agora" começa em 0.
# Se todos forem negativos, nenhum é maior do que 0 e devolve 0,
# um valor que nem está na lista!
#
# A correção: começar com o primeiro elemento.
# Por isso a precondição: a lista não pode ser vazia (l[0] daria IndexError).
#
# Aqui o largest é SUBSTITUÍDO (=), não acumulado (+=).






# %%
"""
Ex 14.5 [EXEMPLO] Procurar: está lá? em que posição? -------------------------
"""

def contains(l: list[int], x: int) -> bool:
    """ Check if x is in l (without using "in"). """
    for v in l:
        if v == x:
            return True
    return False

def index_of(l: list[int], x: int) -> int:
    """ Position of the first x in l, or -1 if it is not there. """
    for i in range(len(l)):
        if l[i] == x:
            return i
    return -1

print(contains([4, 7, 1], 7), contains([4, 7, 1], 5))       # True False
print(index_of([4, 7, 1, 7], 7), index_of([4, 7, 1], 5))    # 1 -1

# É o padrão "procurar e sair" do Ex 11.11.
# O contains faz o mesmo que "x in l". Nos testes às vezes pedem para
# programar à mão algo que o Python já tem, para treinar.
#
# O -1 é um valor CONVENCIONAL para "não encontrei":
# nunca pode ser uma posição válida.






# %%
"""
Ex 14.6 [EXEMPLO] Construir uma lista nova --------------------------------
"""

def squares(n: int) -> list[int]:
    """ List with the squares of 0, 1, ..., n-1. """
    result = []
    for i in range(n):
        result.append(i * i)
    return result

def positives(l: list[int]) -> list[int]:
    """ New list with only the positive elements of l. """
    result = []
    for v in l:
        if v > 0:
            result.append(v)
    return result

print(squares(5))                   # [0, 1, 4, 9, 16]
print(positives([3, -1, 0, 5, -2])) # [3, 5]

# É um acumulador de LISTAS: começa vazio ([]) e cresce com append.
# A lista original não é mexida: a função devolve uma lista NOVA.






# %%
"""
Ex 14.7 [EXEMPLO] "Todos?" e "Algum?" -----------------------------------------
"""

def all_positive(l: list[int]) -> bool:
    """ Check if all elements are positive. """
    for v in l:
        if v <= 0:
            return False        # basta UM que falhe para ser False
    return True                 # vi todos e nenhum falhou

def any_negative(l: list[int]) -> bool:
    """ Check if some element is negative. """
    for v in l:
        if v < 0:
            return True         # basta UM para ser True
    return False                # vi todos e nenhum era

print(all_positive([1, 2, 3]), all_positive([1, -2, 3]))    # True False
print(any_negative([1, 2, 3]), any_negative([1, -2, 3]))    # False True
print(all_positive([]))                                     # True (!!)

# Os dois padrões são espelhos um do outro:
#   "todos"  -> procura um CONTRA-EXEMPLO; se o encontra, return False.
#   "algum"  -> procura um EXEMPLO;        se o encontra, return True.
# O valor de "não encontrei" vai SEMPRE no return depois do ciclo.
#
# Lista vazia: "todos os elementos são positivos" é verdade
# (não há nenhum que falhe). Parece estranho, mas é assim na lógica.
# (o Teste 1 2024/25 usa isto: all_the_same([]) == True, Ex 14.14)






# %%
"""
Ex 14.8 [FAZ] Contar e média ------------------------------------------

    a) count(l, x): quantas vezes x aparece em l (sem usar l.count).
    b) average(l): média dos elementos (precondição: lista não vazia).
"""

def count(l: list[int], x: int) -> int:
    """ How many times x occurs in l. """
    pass        # TODO

def average(l: list[float]) -> float:
    """ Average of the elements.
        Precondition: len(l) > 0
    """
    pass        # TODO

print(count([1, 2, 1, 3, 1], 1), count([1, 2], 5))      # 3 0
print(average([10, 12, 17]))                            # 13.0
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 14.8

def count(l: list[int], x: int) -> int:
    """ How many times x occurs in l. """
    c = 0
    for v in l:
        if v == x:
            c += 1
    return c

def average(l: list[float]) -> float:
    """ Average of the elements.
        Precondition: len(l) > 0
    """
    total = 0
    for v in l:
        total += v
    return total / len(l)

print(count([1, 2, 1, 3, 1], 1), count([1, 2], 5))      # 3 0
print(average([10, 12, 17]))                            # 13.0

# Precondição do average: com a lista vazia seria 0 / 0 -> ZeroDivisionError.
# Dentro do count não chamei "count" à variável: é o nome da função!






# %%
"""
Ex 14.9 [FAZ] Pares e vogais ------------------------------------------

    a) evens(l): lista NOVA só com os elementos pares de l.
    b) count_vowels(s): quantas vogais minúsculas (a, e, i, o, u) tem a string s.
"""

def evens(l: list[int]) -> list[int]:
    """ New list with the even elements of l. """
    pass        # TODO

def count_vowels(s: str) -> int:
    """ Number of lowercase vowels in s. """
    pass        # TODO

print(evens([1, 2, 3, 4, 6]))       # [2, 4, 6]
print(count_vowels("programar"))    # 3
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 14.9

def evens(l: list[int]) -> list[int]:
    """ New list with the even elements of l. """
    result = []
    for v in l:
        if v % 2 == 0:
            result.append(v)
    return result

def count_vowels(s: str) -> int:
    """ Number of lowercase vowels in s. """
    c = 0
    for ch in s:
        if ch in "aeiou":
            c += 1
    return c

print(evens([1, 2, 3, 4, 6]))       # [2, 4, 6]
print(count_vowels("programar"))    # 3   (o, a, a)

# No count_vowels juntam-se os dois "in" do Ex 13.2:
#   for ch in s        -> percorre a string
#   ch in "aeiou"      -> pergunta se é vogal
# Aqui o "ch in 'aeiou'" é seguro: o for dá sempre um carácter de cada vez.






# %%
"""
Ex 14.10 [FAZ] Valores pares vs posições pares (ex 38 dos guiões) ----------

    a) sum_even_values(l): soma dos elementos que SÃO pares.
    b) sum_values_at_even_positions(l): soma dos elementos nas POSIÇÕES pares
       (0, 2, 4, ...).
    Para l = [5, 2, 7, 4, 1]:  a) 6  (2 + 4)    b) 13  (5 + 7 + 1)
"""

def sum_even_values(l: list[int]) -> int:
    """ Sum of the even elements. """
    pass        # TODO

def sum_values_at_even_positions(l: list[int]) -> int:
    """ Sum of the elements at even positions. """
    pass        # TODO

print(sum_even_values([5, 2, 7, 4, 1]))                 # 6
print(sum_values_at_even_positions([5, 2, 7, 4, 1]))    # 13
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 14.10

def sum_even_values(l: list[int]) -> int:
    """ Sum of the even elements. """
    total = 0
    for v in l:
        if v % 2 == 0:
            total += v
    return total

def sum_values_at_even_positions(l: list[int]) -> int:
    """ Sum of the elements at even positions. """
    total = 0
    for i in range(0, len(l), 2):       # 0, 2, 4, ... direto às posições pares
        total += l[i]
    return total

print(sum_even_values([5, 2, 7, 4, 1]))                 # 6
print(sum_values_at_even_positions([5, 2, 7, 4, 1]))    # 13

# A diferença entre as duas é a diferença entre os dois tipos de for (Ex 14.2):
#   valores pares   -> interessa o VALOR    -> for v in l
#   posições pares  -> interessa a POSIÇÃO  -> for i in range(...)
#
# O range com passo 2 evita percorrer as posições ímpares
# e evita o "if i % 2 == 0". Menos trabalho desnecessário.






# %%
"""
Ex 14.11 [PENSA] Mínimo e a sua posição -------------------------------

    position_of_min(l): a POSIÇÃO do menor elemento (se houver empate,
    a primeira). Precondição: lista não vazia.
        position_of_min([5, 2, 7, 2]) == 1
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 14.11 (sem código):
#
#   Interessa a posição -> for pelos índices.
#   Guardar a posição do "mínimo até agora", a começar em 0 (o 1º elemento).
#   Para cada i a partir de 1:
#       se l[i] for MENOR do que l[posição guardada]: atualizar a posição.
#   Devolver a posição.
#
# Porquê "menor" e não "menor ou igual"? Com <=, no empate ficava a ÚLTIMA.
# Com <, fica a primeira (o 2 da posição 3 não substitui o da posição 1).






# %%
"""
Ex 14.12 [PENSA] Lista ao contrário -----------------------------------

    reversed_copy(l): lista NOVA com os elementos de l pela ordem inversa,
    sem mexer na l (e sem usar l.reverse(), reversed() nem [::-1]).
        reversed_copy([1, 2, 3]) == [3, 2, 1]
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 14.12 (sem código), duas formas:
#
#   1. Percorrer os índices de trás para a frente com um range de passo -1
#      (de len(l) - 1 até 0, inclusive: o FIM do range tem de ser -1)
#      e fazer append de cada elemento a uma lista nova.
#
#   2. Percorrer pelos valores, do início ao fim,
#      e inserir cada um na POSIÇÃO 0 da lista nova (insert(0, v)).
#      (é a versão da teórica 04b)






# %%
"""
Ex 14.13 [DESAFIO] Executar à mão (Teste 1 2025/26, pergunta 1b e 1c) -------

    Escreva o resultado das chamadas:
        b) add([1,2,3,4]) = ____
        c) accumulation([1,1,1,1,1,1,1,0]) = ____
    Sem correr! Faz a tabela das variáveis.
    Solução no fim do ficheiro.
"""

def add(l: list[int]) -> int:
    total = 0
    for v in l:
        if v % 2 == 0:
            total += v
        else:
            total -= 1
    return total

def accumulation(l: list[int]) -> list[int]:
    for i in range(1, len(l), 1):
        l[i] = l[i] + l[i-1]
    return l






# %%
"""
Ex 14.14 [DESAFIO] Todos iguais (Teste 1 2024/25, pergunta 4, 3 valores) ----

    Escreva uma função booleana para testar se todos os valores duma lista
    de inteiros são iguais entre si. Exemplos:
        all_the_same([5,5,5,5,5]) == True      all_the_same([]) == True
        all_the_same([5,5,6,5,5]) == False     all_the_same([10]) == True

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def all_the_same(l: list[int]) -> bool:
            ''' Check if all the elements are the same. '''

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 14.15 [DESAFIO] Há repetidos? (for encaixados sobre uma lista) ----------

    has_duplicates(l): True se algum valor aparecer pelo menos 2 vezes.
        has_duplicates([3, 1, 4, 1]) == True
        has_duplicates([3, 1, 4]) == False
        has_duplicates([]) == False
    Sem usar in, count, set, nem ordenar a lista.
    Pista: compara cada elemento com todos os que estão DEPOIS dele.
    Quantas comparações fazes para uma lista de 4 elementos?
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 14.16 [DESAFIO] Contar em posições pares (Exame de Recurso 2023/24, pergunta 3)

    Escreva uma função inteira para contar o número de vezes que um dado
    caráter ocorre em posições de índice par numa lista de carateres.
    Atenção, que o número zero também é par. Exemplos:
        how_many('a', ['a','o','l','a','e','o','a']) == 2
        how_many('a', ['z','a','o','a','e','a','x']) == 0

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def how_many(c: str, l: list[str]) -> int:

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 14.17 [DESAFIO] O mês de um dia do ano (Teste 1 2025/26, pergunta 4, 3 valores)

    Escreva uma função inteira que, dado o número de ordem de um dia dentro
    dum ano, diga qual o mês correspondente. A função tem dois argumentos:
    o número de ordem e a indicação se o ano é ou não bissexto. Exemplos:
        get_month(5, False) == 1        # janeiro
        get_month(31, False) == 1       # janeiro
        get_month(60, True) == 2        # fevereiro
        get_month(60, False) == 3       # março
        get_month(366, True) == 12      # dezembro

    Para saber o número de dias de cada mês, a função usa a constante
    local DURATIONS. Quanto mais simples, melhor.
    Não programe nenhuma função main, nem use input ou print.

        def get_month(order: int, leap_year: bool) -> int:
            ''' Calculate the month corresponding to some day order in a year
                Precondition: 1 <= order <= 366 '''
            DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]

    Pista: "gastar" os dias mês a mês. Enquanto o dia não couber no mês
    atual, tira-se esse mês e passa-se ao seguinte.
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 14.18 [DESAFIO] Progressão geométrica (Teste 1 2025/26, pergunta 5, 3 valores)

    Uma progressão geométrica é uma sequência de valores reais em que cada
    elemento (exceto o primeiro) é igual ao anterior multiplicado por um
    valor real fixo chamado razão. Ou seja, a[n+1] = a[n] * r.
    Por exemplo, 2, 4, 8, 16, 32, 64 é uma progressão geométrica de razão 2.0.

    Escreva uma função booleana para testar se uma lista de valores reais
    constitui uma progressão geométrica (finita). Exemplos:
        is_geometric([2,4,8,16,32,64]) == True
        is_geometric([2,4,8,16,32,65]) == False
        is_geometric([1.1,5.5]) == True
        is_geometric([0,5.5]) == False
        is_geometric([2.33,0,0,0,0,0,0,0,0,0]) == True

        def is_geometric(l: list[float]) -> bool:
            ''' Check whether a list represents a geometric progression.
                Precondition: len(l) >= 2 '''

    Pistas: qual é a razão? (olha para os 2 primeiros elementos)
    E cuidado com os zeros e com os floats.
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 14.19 [SOZINHA] Médias parciais e posição de uma data ----------------

    a) (Exame de Recurso 2023/24, pergunta 5, 2.5 valores)
       Escreva uma função que, dada uma lista de inteiros l, retorne uma nova
       lista m, constituída pelas médias parciais da lista original.
       Para cada i, o valor que fica em m[i] é a média de l[0], l[1], ..., l[i].
           partial_averages([1,2,6,9,2]) == [1.0,1.5,3.0,4.5,4.0]
           partial_averages([]) == []      partial_averages([17]) == [17.0]
       Quanto mais simples, melhor (não recalcules a soma do zero em cada posição).
           def partial_averages(l: list[int]) -> list[float]:

    b) (ex 24 dos guiões) day_order(day, month, year): a posição de uma data
       dentro do ano. 1/1 é o dia 1, 31/12/2024 é o dia 366.
           day_order(10, 3, 2024) == 70     (31 + 29 + 10)
       Usa o month_length do Ex 12.9.

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  [ ] for com range: as 3 formas, o FIM nunca está incluído, passo negativo
  [ ] for dentro de for: o de dentro dá todas as voltas por cada volta do de fora
  [ ] acumulador (soma, produto, lista), contador; o valor inicial certo
  [ ] off-by-one: range(A, B + 1) para incluir o B
  [ ] os 3 bugs do acumulador: return dentro, início dentro, sem início
  [ ] procurar e sair: return dentro do ciclo, "não encontrei" depois do ciclo
  [ ] "todos?" vs "algum?"
  [ ] listas: índices a partir de 0, índices negativos, len, append, pop
  [ ] b = a não copia a lista; uma função pode mudar a lista recebida
  [ ] tuplos (imutáveis), conjuntos (sem repetidos), {} é um dicionário
  [ ] in: em listas, tuplos, conjuntos, ranges, strings e dicionários (chaves)
  [ ] for pelos valores vs pelos índices; range(len(l) - 1) para vizinhos
  [ ] máximo: começar com l[0], não com 0

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_5_valores_referencias_escopo.py
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
# Solução do Ex 11.18 (Teste 1 2024/25, pergunta 3)

def sum_multiples(m: int, lim: int) -> int:
    """ Sum all the multiples of m until lim inclusive.
        Precondition: m > 0 and lim > 0
    """
    total = 0
    for i in range(0, lim + 1, m):      # salta de múltiplo em múltiplo
        total += i
    return total

print(sum_multiples(10, 10), sum_multiples(5, 10),
      sum_multiples(2, 10), sum_multiples(1, 10))       # 10 15 30 55

# A versão "óbvia" passa por TODOS os números de 0 a lim e testa cada um:
#     for i in range(lim + 1):
#         if i % m == 0:
#             total += i
# Está certa, mas faz trabalho desnecessário. O passo do range (m)
# vai direto aos múltiplos: menos voltas e nenhum if.
# O lim + 1 é porque o enunciado diz "menores OU IGUAIS".






# %%
# Solução do Ex 11.19 (Teste 1 2025/26, pergunta 3)

def zeno(k: int) -> float:
    """ Sum of the k first terms of the Zeno series """
    total = 0.0
    term = 0.5                  # o 1º termo
    for _ in range(k):
        total += term
        term /= 2               # o termo seguinte é metade do atual
    return total

print(zeno(0), zeno(1), zeno(2), zeno(3))       # 0.0 0.5 0.75 0.875

# Outra forma, mais direta mas com mais contas:
#     for n in range(1, k + 1):
#         total += 1 / 2 ** n
# A versão de cima calcula cada termo a partir do anterior (uma divisão),
# em vez de calcular a potência do zero em cada volta.
# "O próximo termo a partir do anterior" é um truque muito usado em séries.






# %%
# Solução do Ex 11.20 (Teste 1 2024/25, 1c)
#
# Resposta: D. 13
#
# O truque: é "x = ...", não "x += ...". Em cada volta o x é SUBSTITUÍDO.
# Só interessa a ÚLTIMA volta. Não é preciso fazer as 14!
#
# A última volta é i = 13 (o range pára ANTES do 14):
#   13 % 3 == 1                    -> não
#   13 % 5 == 3, 13 % 4 == 1       -> não
#   13 % 7 == 6                    -> não
#   else                           -> x = 13
#
# As outras respostas são armadilhas:
#   C. 28  = 14 * 2   -> quem achou que o 14 estava incluído (off-by-one)
#   B. 26  = 13 * 2   -> quem se enganou no elif do 7
#   A. 195 = 15 * 13  -> quem se enganou no 13 % 3

x = 0
for i in range(0, 14, 1):
    if i % 3 == 0 and i < 20:
        x = 15 * i
    elif i % 5 == 0 or i % 4 == 0:
        x = (10 * i) / 5
    elif i % 7 == 0:
        x = i * 2
    else:
        x = i
print(x)        # 13






# %%
# Solução do Ex 11.21
#
# a) count = 10
#    O ciclo de dentro começa no i, por isso dá cada vez menos voltas:
#      i = 0: j = 0, 1, 2, 3   -> 4 voltas
#      i = 1: j = 1, 2, 3      -> 3
#      i = 2: j = 2, 3         -> 2
#      i = 3: j = 3            -> 1
#    4 + 3 + 2 + 1 = 10   (e não 4 * 4 = 16)
#
# b) Escreve (cada linha termina com um espaço):
#      0
#      0 2
#      0 3 6
#    i = 1: j = 0          -> 1*0
#    i = 2: j = 0, 1       -> 2*0, 2*1
#    i = 3: j = 0, 1, 2    -> 3*0, 3*1, 3*2
#    O print() vazio está no for de fora: muda de linha 3 vezes.

count = 0
for i in range(4):
    for j in range(i, 4):
        count += 1
print(count)

for i in range(1, 4):
    for j in range(i):
        print(i * j, end=" ")
    print()






# %%
# Solução do Ex 11.22 (Teste 1 2024/25, pergunta 5)

def draw_segment(x: str, n: int) -> None:
    """ Draw a partial line with length n using the char x.
        Precondition: len(x) == 1 and n >= 0
    """
    for i in range(n):
        print(x, end='')

def draw_tree_line(spaces: int, x: str, count: int) -> None:
    """ Draw a full line: spaces, then count times x, then change line.
        Precondition: len(x) == 1 and spaces >= 0 and count >= 0
    """
    draw_segment(' ', spaces)
    draw_segment(x, count)
    print()

def draw_pine_tree(a: str, b: str, c: str, n: int) -> None:
    """ Draw a Christmas pine tree.
        Arguments: a - star; b - pine needle; c - wood
        Precondition: len(a)==1 and len(b)==1 and len(c)==1 and n >= 3
    """
    draw_tree_line(n - 1, a, 1)                 # a estrela no topo
    for k in range(1, n):                       # as n-1 linhas de agulhas
        draw_tree_line(n - 1 - k, b, 2 * k + 1)
    for _ in range(3):                          # o tronco
        draw_tree_line(n - 2, c, 3)

draw_pine_tree('*', '^', '#', 5)
draw_pine_tree('*', '^', '#', 10)

# A tabela para n = 5 (k é o número da linha da copa, a começar em 0):
#   k | espaços | símbolos
#   0 |    4    |    1      (estrela)
#   1 |    3    |    3
#   2 |    2    |    5
#   3 |    1    |    7
#   4 |    0    |    9
# espaços = n - 1 - k, símbolos = 2 * k + 1 (1, 3, 5, 7, ...)
# O tronco tem 3 símbolos e fica centrado: n - 2 espaços (tal como a linha k = 1).
#
# A função auxiliar draw_tree_line, com comentário e precondição, é o que
# o enunciado sugeria ("Talvez deseje definir uma função auxiliar").






# %%
# Solução do Ex 12.11 (referências)
#
# Escreve:
#   [1, 2, 3, 0]
#   [1, 2, 3, 0]
#   [7, 2]
#
#   a = [1, 2]; b = a        -> a e b são a MESMA lista
#   c = a.copy()             -> c é uma lista nova: [1, 2]
#   b.append(3)              -> a lista partilhada fica [1, 2, 3]
#   c[0] = 7                 -> só o c muda: [7, 2]
#   change(a):
#       l.append(0)          -> l aponta para a lista partilhada: [1, 2, 3, 0]
#       l = [9, 9]           -> o l passa a apontar para uma lista NOVA
#       l.append(1)          -> muda a lista nova, que ninguém cá fora vê
#
# A diferença decisiva: l.append(...) MUDA a lista para onde l aponta;
# l = [...] só põe o l a apontar para outra lista.






# %%
# Solução do Ex 13.5
#
#   a) "" in "abc"               True   (a string vazia está em qualquer string)
#   b) "ac" in "abc"             False  (tem de estar seguido)
#   c) 15 in range(0, 15, 5)     False  (0, 5, 10: o FIM não entra)
#   d) 15 in range(0, 16, 5)     True   (0, 5, 10, 15)
#   e) [1] in [1, 2]             False  (a lista [1] não é um elemento; o 1 é)
#   f) 2 in {1: "a", 2: "b"}     True   (2 é uma chave)
#   g) "a" in {1: "a"}           False  (o in procura nas chaves, não nos valores)
#   h) "Abc" in "abcAbc"         True   (está a partir da posição 3)

print("" in "abc", "ac" in "abc", 15 in range(0, 15, 5), 15 in range(0, 16, 5),
      [1] in [1, 2], 2 in {1: "a", 2: "b"}, "a" in {1: "a"}, "Abc" in "abcAbc")






# %%
# Solução do Ex 14.13 (Teste 1 2025/26, pergunta 1b e 1c)
#
# b) add([1,2,3,4]) = 4
#      v | par? | total
#      - |  -   |   0
#      1 | não  |  -1
#      2 | sim  |   1
#      3 | não  |   0
#      4 | sim  |   4
#
# c) accumulation([1,1,1,1,1,1,1,0]) = [1, 2, 3, 4, 5, 6, 7, 7]
#    Cada posição passa a ser ela própria mais a anterior JÁ ATUALIZADA:
#      i=1: l[1] = 1 + 1 = 2
#      i=2: l[2] = 1 + 2 = 3
#      ...
#      i=7: l[7] = 0 + 7 = 7
#    A armadilha: usar o valor ORIGINAL do vizinho (daria [1,2,2,2,2,2,2,1]).
#    A lista é modificada durante o ciclo, e o l[i-1] já foi mudado na volta anterior.

def add(l: list[int]) -> int:
    total = 0
    for v in l:
        if v % 2 == 0:
            total += v
        else:
            total -= 1
    return total

def accumulation(l: list[int]) -> list[int]:
    for i in range(1, len(l), 1):
        l[i] = l[i] + l[i-1]
    return l

print(add([1, 2, 3, 4]))                            # 4
print(accumulation([1, 1, 1, 1, 1, 1, 1, 0]))       # [1, 2, 3, 4, 5, 6, 7, 7]






# %%
# Solução do Ex 14.14 (Teste 1 2024/25, pergunta 4)

def all_the_same(l: list[int]) -> bool:
    """ Check if all the elements are the same. """
    for i in range(len(l) - 1):
        if l[i] != l[i + 1]:
            return False
    return True

print(all_the_same([5, 5, 5, 5, 5]), all_the_same([5, 5, 6, 5, 5]),
      all_the_same([]), all_the_same([10]))         # True False True True

# Padrão "todos?" (Ex 14.7): procurar um par de vizinhos DIFERENTES.
# Se todos os vizinhos são iguais, são todos iguais.
#
# Lista vazia e lista com 1 elemento: range(-1) e range(0) são vazios,
# o ciclo não corre e devolve True. Os dois casos especiais saem "de graça",
# sem nenhum if extra.
#
# Outra forma: comparar todos com o primeiro (for v in l: if v != l[0]...).
# Mas aí a lista vazia rebenta no l[0] e precisa de um if à parte.






# %%
# Solução do Ex 14.15

def has_duplicates(l: list[int]) -> bool:
    """ Check if some value occurs more than once. """
    for i in range(len(l)):
        for j in range(i + 1, len(l)):      # só os elementos DEPOIS do i
            if l[i] == l[j]:
                return True
    return False

print(has_duplicates([3, 1, 4, 1]), has_duplicates([3, 1, 4]), has_duplicates([]))
# True False False

# Para cada posição i, compara-se com as posições j > i.
# Começar o j em i + 1 tem duas vantagens:
#   - não compara um elemento com ele próprio (l[i] == l[i] seria sempre True!);
#   - não repete comparações (se já comparou 0 com 2, não compara 2 com 0).
# Para 4 elementos: 3 + 2 + 1 = 6 comparações (o mesmo "triângulo" do Ex 11.21).
#
# É o padrão "algum?" (Ex 14.7) com dois ciclos:
# o return True sai dos DOIS ciclos e da função de uma vez.
# O return False só depois de os dois ciclos acabarem.






# %%
# Solução do Ex 14.16 (Exame de Recurso 2023/24, pergunta 3)

def how_many(c: str, l: list[str]) -> int:
    """ Number of times c occurs at even positions of l. """
    count = 0
    for i in range(0, len(l), 2):       # só as posições pares: 0, 2, 4, ...
        if l[i] == c:
            count += 1
    return count

print(how_many('a', ['a', 'o', 'l', 'a', 'e', 'o', 'a']))   # 2  (posições 0 e 6)
print(how_many('a', ['z', 'a', 'o', 'a', 'e', 'a', 'x']))   # 0  (só nas ímpares)

# Interessa a POSIÇÃO -> for pelos índices.
# O range com passo 2 é o "menos trabalho desnecessário" (como no Ex 14.10).






# %%
# Solução do Ex 14.17 (Teste 1 2025/26, pergunta 4)

def get_month(order: int, leap_year: bool) -> int:
    """ Calculate the month corresponding to some day order in a year
        Precondition: 1 <= order <= 366 """
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    for month in range(1, 13):
        days = DURATIONS[month - 1]
        if month == 2 and leap_year:
            days += 1
        if order <= days:           # o dia cabe neste mês: encontrado!
            return month
        order -= days               # não cabe: gastar este mês e passar ao seguinte
    return 12                       # só chega aqui se a precondição não for cumprida

print(get_month(5, False), get_month(31, False), get_month(60, True),
      get_month(60, False), get_month(366, True))       # 1 1 2 3 12

# Executar à mão get_month(60, False):
#   mês 1: 60 <= 31? não -> order = 29
#   mês 2: 29 <= 28? não -> order = 1
#   mês 3: 1 <= 31? sim  -> 3
#
# É o padrão "procurar e sair" com um acumulador (order) que vai diminuindo.
# O último return é só para a função devolver sempre um int.






# %%
# Solução do Ex 14.18 (Teste 1 2025/26, pergunta 5)

def is_geometric(l: list[float]) -> bool:
    """ Check whether a list represents a geometric progression.
        Precondition: len(l) >= 2 """
    EPSILON = 1e-9
    if l[0] == 0:
        return False                # sem razão possível (ver nota em baixo)
    r = l[1] / l[0]
    for i in range(1, len(l) - 1):
        if abs(l[i + 1] - l[i] * r) > EPSILON:
            return False
    return True

print(is_geometric([2, 4, 8, 16, 32, 64]), is_geometric([2, 4, 8, 16, 32, 65]),
      is_geometric([1.1, 5.5]), is_geometric([0, 5.5]),
      is_geometric([2.33, 0, 0, 0, 0, 0, 0, 0, 0, 0]))
# True False True False True

# A razão sai dos 2 primeiros: r = l[1] / l[0].
# Depois, padrão "todos?": cada elemento tem de ser o anterior vezes r.
# Começa em i = 1 porque o par (0, 1) já foi usado para calcular o r.
#
# Os casos difíceis:
#   [0, 5.5]: com l[0] == 0 não há razão (0 * r é sempre 0, nunca 5.5)
#             e l[1] / l[0] dava ZeroDivisionError. Daí o if inicial.
#   [2.33, 0, 0, ...]: r = 0, e depois 0 * 0 = 0 sempre. True.
#   [0, 0]: o enunciado não diz. Esta solução devolve False.
#           No teste, decisões destas escrevem-se num comentário
#           ("se encontrar ambiguidades, explicite as decisões tomadas").
#
# Floats: compara-se com EPSILON e não com == (ficheiro 3, Ex 7.7).
