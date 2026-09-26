# %%
"""
===========================================================================
GUIA DE SESSÃO — Prática 03 — IPCE 2026/2027
===========================================================================

Cobre os guiões 03a (ex. 19-21) e 03b (ex. 22-24).
https://ipce-184ea7.gitlab.io/

Tema de hoje: o ciclo FOR.
    A primeira vez que um programa nosso faz a mesma coisa MUITAS vezes
    sem nós termos de escrever o código muitas vezes.

Como usar este ficheiro no Spyder:
    - cada bloco que começa com "# %%" é uma célula
    - Ctrl + Enter  -> corre só a célula onde está o cursor
    - F5            -> corre o ficheiro todo (vai pedir vários inputs!)

"""



# %%
"""
===========================================================================
LOGÍSTICA / Revisões [8 min -> 10:18]
===========================================================================

    - Baixar este guia da drive e abrir no Spyder
        (guia na drive mais vazio para eles fazerem na aula)

    - Confirmar bom horario de dúvidas para o turno

    - Folha de Presenças (50% presenças para quem faz cadeira 1ª vez)
    
    - Mooshak: já conseguem entrar? (precisa de Eduroam ou VPN)

    - Sobre VPN sempre a abrir - façam "shut down client" duas vezes e depis deve parar 

REVIÕES/DÚVIDAS ----------------------------------------------------
    - Dúvidas da aula passada (if / elif / else, and / or / not)

    - EXERCICIO PARA CONSOLIDAR:

        a) Escreve uma função que dados 2 números inteiros a e b, 
            devolve o valor maior deles 
"""
def maximo(a: int, b: int) -> int:
    """ Devolve o maior de dois números inteiros. """
    if a > b:
        return a
    else:
        return b

print(maximo(3, 5))   # 5
print(maximo(5, 3))   # 5



#%%
"""
    b) Escreve uma função que dados 2 números inteiros a e b, 
        devolve True se o primeiro for múltiplo do segundo, e False caso contrário
"""
def is_multiple(a: int, b: int) -> bool:
    """ Devolve True se a for múltiplo de b, False caso contrário. """
    if a % b == 0: 
        return True
    else:
        return False

print(is_multiple(10, 2))   # True
print(is_multiple(10, 3))   # False

# %%
# O else final muitas vezes pode ser omitido.
# Porque se chega aqui, é porque todas as condições anteriores de if/elif não se verificaram 
def is_multiple(a: int, b: int) -> bool:
    """ Devolve True se a for múltiplo de b, False caso contrário. """
    if a % b == 0: 
        return True
    return False

print(is_multiple(10, 2))   # True
print(is_multiple(10, 3))   # False

# %%
# Se a função retorna só True ou False, podemos retornar logo a expressão booleana
def is_multiple(a: int, b: int) -> bool:
    """ Devolve True se a for múltiplo de b, False caso contrário. """
    return a % b == 0

print(is_multiple(10, 2))   # True
print(is_multiple(10, 3))   # False








# %%
"""
===========================================================================
CICLO FOR — primeiros passos + Guião 03a, ex. 19a, 19b [34 min -> 10:52]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) Ciclo for -> repetir uma tarefa muitas vezes [7 min -> 10:25] ----------

    Escrevam e corram cada um destes pequenos exemplos (um por célula).
    Antes de correr, tentem adivinhar que números vão aparecer.
"""
# forma só com FIM (vai de 0 até FIM, não incluído)
for numero in range(5):
    print("numero =", numero)

# tudo o que está INDENTADO debaixo do for repete-se, uma vez por cada número.
# a variável numero vai tendo um valor diferente em cada volta.



# %%
# forma com INICIO e FIM (vai de INICIO até FIM, não incluído)
for numero in range(2,5):
    print("numero =", numero)



# %%
# forma com INICIO, FIM e PASSO (vai de INICIO até FIM, não incluído, de PASSO em PASSO)
for numero in range(0, 20, 5):
    print("numero =", numero)



# %%
# podemos ter início negativo...
for numero in range(-5, 5, 2):
    print("numero =", numero)



# %%
# com PASSO negativo vai a descer (e o FIM também pode ser negativo)
for numero in range(7, -5, -2):
    print("numero =", numero)



# %%
# vazio - o ciclo não corre nenhuma vez
for numero in range(5, 5):
    print("numero =", numero)



# %%
"""
b) EXERCICIO (EXTRA) — Tabuada do 7 [6 min -> 10:31] ----------------------

    Escrever a tabuada do 7 no ecrã:
        7 x 1 = 7
        7 x 2 = 14
        ...
        7 x 10 = 70
"""

for i in range(1, 11):
    print(f"7 x {i} = {7 * i}")

# O que acabou de acontecer:
# o código indentado debaixo do "for" correu 10 vezes.
# em cada vez, a variável i tinha um valor diferente: 1, 2, 3, ..., 10.
# repara que o 11 NÃO aparece -> o range pára ANTES do 11.

# Sem este ciclo for íamos ter de escrever 10 prints...
# Se fosse a tabuada até 1000? 1000 prints? Nem pensar.



# %%
"""
c) EXERCICIO (GUIÃO, exemplo do início do 03a) — Fatorial [6 min -> 10:37] --

    O fatorial de n é o produto de todos os inteiros de 1 até n:
    5! = 1 * 2 * 3 * 4 * 5 = 120

    Corram e vejam com o print dentro do for o que está a acontecer
"""

def factorial(n: int) -> int:
    """ Fatorial dum número natural.
        Precondition: n >= 0
    """
    fact = 1
    for i in range(1, n + 1, 1):
        fact = fact * i
        print(f"   volta com i={i}  ->  fact passou a valer {fact}")   # o raio-X
    return fact

print("resultado:", factorial(5))

# Lê o output de cima para baixo:
# o fact começa em 1 e vai sendo multiplicado por 1, depois 2, depois 3...
# o fact "acumula" o resultado das voltas anteriores.
# a isto chama-se um ACUMULADOR -> é O padrão mais importante de hoje.
#
# repara no "return fact": está FORA do ciclo (mesma indentação que o for).
# só devolvemos o resultado depois de o ciclo acabar todas as voltas.
# (no ex 22 vamos ver o que acontece se o pusermos lá dentro...)



# %%
"""
d) EXERCICIO 19a (GUIÃO) — Soma dos n primeiros naturais [10 min -> 10:47] ---

    19a - Escreva um programa que receba do utilizador um número natural n
    e que calcule a soma dos n primeiros números naturais.
    Por exemplo, para n igual a 4, o resultado deve ser 6 (= 0 + 1 + 2 + 3).

    Escreva uma função com um ciclo for. Use uma variável para acumular a
    soma dos vários valores. A estrutura e lógica da função são parecidas
    com a da função factorial.

"""

def sum_naturals(n: int) -> int:
    """ Soma dos n primeiros naturais: 0 + 1 + ... + (n-1).
        Precondition: n >= 0
    """
    total = 0                       # uma soma começa em 0 (o fatorial começava em 1)
    for i in range(0, n, 1):        # i = 0, 1, 2, ..., n-1  (n números!)
        total = total + i
    return total

def main() -> None:
    x = int(input("Introduza um número natural: "))
    print(sum_naturals(x))

main()

# Exemplo do guião: 4 -> 6
# O range(0, n, 1) dá exatamente n números: 0, 1, ..., n-1.
# Para n = 4: 0, 1, 2, 3  -> 4 números, que é o que o enunciado pede.



# %%
"""
e) EXERCICIO 19b (GUIÃO) — Soma dos n primeiros quadrados [5 min -> 10:52] ---

    19b - Escreva um programa que receba do utilizador um número natural n
    e que calcule a soma dos primeiros n quadrados perfeitos.
    Por exemplo, para n igual a 4, o resultado deve ser 14 (= 0 + 1 + 4 + 9).
"""

def sum_squares(n: int) -> int:
    """ Soma dos n primeiros quadrados perfeitos: 0 + 1 + 4 + ... + (n-1)^2.
        Precondition: n >= 0
    """
    total = 0
    for i in range(0, n, 1):
        total = total + i * i       # a ÚNICA diferença para o 19a
        # total = total + i ** 2    # também dá
    return total

def main() -> None:
    x = int(input("Introduza um número natural: "))
    print(sum_squares(x))

main()

# Exemplo do guião: 4 -> 14
# Reparem: mudou UMA linha em relação ao 19a.
# A "máquina" (acumulador + for) é sempre igual.
# Só muda O QUE acumulamos em cada volta.



# %%
"""
===========================================================================
CICLO FOR — CONCEITOS [22 min -> 11:14]
===========================================================================

a) Sintaxe do for com range [2 min -> 10:54] ------------------------------

    for VARIAVEL in range(INICIO, FIM, PASSO):
        corpo do ciclo (indentado!)

    - cada execução do corpo chama-se uma ITERAÇÃO (ou "volta")
    - range(INICIO, FIM, PASSO) gera números
        a começar em INICIO,
        a andar de PASSO em PASSO,
        e PÁRA ANTES de chegar a FIM.
    - o FIM nunca está incluído!

    Formas do range:
        range(FIM)          == range(0, FIM, 1)
        range(INICIO, FIM)  == range(INICIO, FIM, 1)
        range(INICIO, FIM, PASSO)  -> PASSO pode ser negativo (a descer)
"""



# %%
"""
b) O "for" pode correr em qualquer sequência, não só range [3 min -> 10:57] --

    Noutras linguagens (C, Java) o for é quase sempre "conta de X até Y".
    Em Python o for é mais geral: "para cada coisa DENTRO de uma sequência".
    O range é só UMA das sequências possíveis.
"""

for letra in "IPCE":               # uma string é uma sequência de caracteres
    print(letra)

for x in [0, 10, 1]:               # uma lista (vamos ver listas com calma mais à frente)
    print(x)


# PERGUNTA DE TESTE (Teste 1 2024/25, 1b) — sintaticamente correto ou incorreto?
#
#     for i in [0,10,1]:
#         a[i] = 3*i
#
# Resposta: CORRETO!
# Não dá erro de sintaxe, só que NÃO faz o mesmo que range(0,10,1):
#   range(0, 10, 1) -> 0, 1, 2, ..., 9   (10 voltas)
#   [0, 10, 1]      -> 0, 10, 1          (3 voltas, com esses 3 valores)
# É uma armadilha clássica de teste: parece igual, mas não é.



# %%
"""
c) O padrão ACUMULADOR [3 min -> 11:00] -----------------------------------

    Já usámos o acumulador 3 vezes (fatorial, 19a, 19b). A receita é sempre:

        1. ANTES do ciclo:  criar a variável com o valor inicial
        2. DENTRO do ciclo: juntar-lhe alguma coisa em cada volta
        3. DEPOIS do ciclo: usar/devolver o resultado

    Valor inicial = o "elemento neutro" da operação:
        soma    -> começa em 0   (x + 0 = x, não estraga nada)
        produto -> começa em 1   (x * 1 = x, não estraga nada)
        se o produto começasse em 0... tudo daria 0!

"""
def sum_naturals_v2(n: int) -> int:
    acc = 0
    for i in range(n):
        acc += i
    return acc

print(sum_naturals_v2(4))



# %%
"""
d) Atribuição aumentada += [3 min -> 11:03] --------------------------------

    Quando usamos uma variável como "acumulador",
    podemos escrever normalmente:

        total = total + i

    ou escrever de forma mais curta com a atribuição aumentada:
        total += i

    e isto funciona para qualquer operador binário: +, -, *, /, //, %, **, ...
        fact  = fact * i     ->   fact *= i
        x     = x - 1        ->   x -= 1
        x     = x / 2        ->   x /= 2
"""

def factorial_v2(n: int) -> int:
    fact = 1
    for i in range(1, n + 1):
        # fact = fact * i
        fact *= i
    return fact

print(factorial_v2(5))

# Cuidado: é "+=" e não "=+".
#   x += 3   -> soma 3 ao x
#   x =+ 3   -> atribui +3 ao x (o "+" é só o sinal do número!) -> bug silencioso



# %%
"""
e) Off-by-one — o erro mais comum [4 min -> 11:07] ------------------------

    "Off-by-one error" (erro de um-a-mais ou um-a-menos)
    é quando um ciclo dá uma volta a mais ou uma volta a menos.

    Exemplo: queremos somar 1 + 2 + ... + 10
    » resposta: 55
"""

# Erro típico: range(1, 10) pára no 9, não inclui o 10.
total = 0
for i in range(1, 10):
    total = total + i
print("total: ", total)   # 45  (falta o 10)

# Correto: range(1, 11) pára ANTES do 11, por isso já inclui o 10.
total = 0
for i in range(1, 10 + 1):
    total = total + i
print("total: ", total)   # 55

# Regra prática para não falhar no teste:
#   "de A até B, com o B incluído"  ->  range(A, B + 1)
#   "n vezes a começar no 0"        ->  range(0, n)   (ou só range(n))
#
# Dica: testem SEMPRE o vosso ciclo com um caso pequeno à mão
# (ex: n = 1 ou n = 2) e confirmem a primeira e a última volta.



# %%
"""
f) Curiosidade - Não chamem "sum" às vossas variáveis ou funções [3 min -> 11:10] ---------

    As soluções oficiais usam muitas vezes "sum" como nome de variável.
    Funciona, mas é má ideia:
    o Python JÁ TEM uma função chamada sum, que soma uma sequência.
"""

print(sum([1, 2, 3]))              # 6 -> a sum do Python
print(sum(range(4)))               # 6 -> o 19a numa linha só!

# Se criarmos uma variável com o mesmo nome, ela "tapa" a função do Python
# (em inglês diz-se shadowing — a nossa variável faz sombra à original).
# Dentro desta função, "sum" deixou de ser a função e passou a ser um int:

def demo_shadowing() -> int:
    sum = 10
    return sum([1, 2, 3])          # TypeError: 'int' object is not callable

# print(demo_shadowing())          # <- descomenta e corre para veres o erro

# Moral: usem nomes vossos -> total, soma, acc, resultado...
# O mesmo vale para: max, min, list, str, int, input, print...
# (sim, dá para fazer "print = 5" e depois nunca mais conseguem imprimir nada,
#  até fazerem "del print" ou reiniciarem a consola)



# %%
"""
g) Curiosidade — para esta soma nem precisamos de for: o truque de Gauss [4 min -> 11:14]

    Diz a lenda que um professor, para ter a turma entretida, mandou
    somar 1 + 2 + ... + 100. O pequeno Gauss (que viria a ser um dos
    maiores matemáticos de sempre) respondeu em segundos: 5050.

    Truque: juntar o primeiro com o último, o segundo com o penúltimo...
        1 + 100 = 101
        2 +  99 = 101
        ...
        50 pares de 101 -> 50 * 101 = 5050

    Fórmula geral: 0 + 1 + ... + (n-1)  =  n * (n - 1) / 2
    Ou seja, o 19a pode ser feito SEM ciclo nenhum.

    Qual a diferença em termos de tempo?
"""

import time

def sum_naturals_loop(n: int) -> int:
    """ O 19a com ciclo (igual ao sum_naturals_v2 de cima).
        Precondition: n >= 0
    """
    total = 0
    for i in range(n):
        total += i
    return total

def sum_naturals_gauss(n: int) -> int:
    """ O 19a com a fórmula de Gauss: 0 + 1 + ... + (n-1).
        Precondition: n >= 0
    """
    return n * (n - 1) // 2
    # se fizéssemos return n * (n - 1) / 2 » a divisão dava resultado em float
    # então podíamos fazer cast para converter para int: return int(n * (n - 1) / 2)
    # ou então podemos usar logo a divisão inteira diretamente
    # e podemos usar à vontade porque (n*(n-1) é sempre par, a divisão é exata)

n = 10_000_000                     # 10 milhões (o "_" é só para ler melhor)

t0 = time.perf_counter()
r1 = sum_naturals_loop(n)          # com ciclo: 10 milhões de voltas
t1 = time.perf_counter()
r2 = sum_naturals_gauss(n)         # com fórmula: 3 contas
t2 = time.perf_counter()

print(f"ciclo:   {r1}  em {t1 - t0:.4f} segundos")
print(f"fórmula: {r2}  em {t2 - t1:.6f} segundos")
print("dão o mesmo?", r1 == r2)

# Os dois dão o mesmo resultado, mas um faz 10 milhões de voltas e o outro 3 contas.
# Nos enunciados dos testes aparece sempre esta frase:
#   "Quanto mais simples for a função e quanto menos trabalho
#    desnecessário ela fizer, melhor."
# É isto. Pensar ANTES de programar pode poupar muito trabalho ao computador.
# (no teste, se pedirem "use um ciclo", usem um ciclo! mas fica a ideia)



# %%
"""
===========================================================================
Guião 03a, exercícios 20, 21 [21 min -> 11:35]
https://ipce-184ea7.gitlab.io/
===========================================================================

EXERCICIO 20 (GUIÃO, Mooshak F) — Tabela de anos [10 min -> 11:24] ---------

    20 - Escreva um programa que, dado um intervalo de anos, por exemplo
    de 2000 a 2020, apresente uma tabela com a duração de cada ano em dias,
    nesse intervalo.
    Use um ciclo for dentro da função main.
    Aproveite a função year_length, que aparece no início desta aula.

    Exemplo de execução
        A: 2000
        B: 2010
        2000 366
        2001 365
        ...
        2010 365

"""

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if is_leap_year(year):
        return 366
    else:
        return 365

def main() -> None:
    a = int(input("A: "))
    b = int(input("B: "))
    for year in range(a, b + 1):   # b + 1 -> o 2010 TEM de aparecer (off-by-one!)
        print(f"{year} {year_length(year)}")

main()

# Aqui o ciclo não acumula nada: em cada volta só ESCREVE uma linha.
# Nem todos os ciclos são acumuladores.
#
# Reparem que o ciclo está no main (como o guião pede):
# escrever a tabela é "interação com o utilizador" -> é trabalho do main.
# A lógica (quantos dias tem um ano) está nas funções.
#
# Mooshak: o output tem de ser IGUAL ao do exemplo, caractere a caractere.
# "2000 366" e não "2000: 366" nem "2000  366".



# %%
"""
EXERCICIO 21 (GUIÃO, Mooshak G) — Ler e somar [11 min -> 11:35] ------------

    21 - Escreva um programa para somar uma sequência de números inteiros.
    O programa começa por perguntar qual o número de valores a somar;
    depois vai lendo os sucessivos valores enquanto faz as contas;
    e no final escreve o resultado.

    Exemplo de execução
        Introduza a quantidade de números a somar: 5
        1> 45
        2> 44
        3> 20
        4> 10
        5> 10
        129

"""

def read_and_sum(n: int) -> int:
    """ The sum of a sequence of integers gathered from the input.
        Precondition: n >= 0
    """
    total = 0
    for i in range(1, n + 1):              # 1, 2, ..., n -> para o prompt dizer "1>", "2>", ...
        x = int(input(f"{i}> "))           # o input pode estar DENTRO do ciclo!
        total += x
    return total

    # Alternativa: o ciclo começa em 0, mas temos de somar 1 no texto do prompt
    # for i in range(n):
    #     x = int(input(f"{i + 1}> "))
    #     total += x

def main() -> None:
    n = int(input("Introduza a quantidade de números a somar: "))
    print(read_and_sum(n))

main()

# Exemplo do guião: 5 números (45, 44, 20, 10, 10) -> 129
#
# Reparem que nunca guardámos os 5 números!
# Cada número entra, é somado ao total, e a variável x é reaproveitada
# na volta seguinte para o próximo número.
# Somar "enquanto se lê" poupa memória: dava para somar 1 milhão de números
# usando só 2 variáveis.



# %%
"""
===========================================================================
INTERVALO [20 min -> 11:55]
===========================================================================
"""



# %%
"""
===========================================================================
Guião 03b, exercício 22 — 4 funções quase iguais [21 min -> 12:16]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) EXERCICIO 22 (GUIÃO) — Prever e correr [7 min -> 12:02] ------------------

    22 - Considere as seguintes quatro funções.
    Explique com detalhe o que cada função está a fazer e porque produz
    o resultado observado.

    ANTES de correr: escrevam num papel quanto acham que dá f1(5), f2(5),
    f3(5) e f4(5). Só depois corram e comparem.
"""

def f1(n: int) -> int:
    total = 0
    for i in range(0, n, 1):
        total += i
    return total

def f2(n: int) -> int:
    total = 0
    for i in range(0, n, 1):
        total += i
        return total              # <- mais indentado que no f1

def f3(n: int) -> int:
    for i in range(0, n, 1):
        total = 0                 # <- dentro do ciclo
        total += i
    return total

def f4(n: int) -> int:
    for i in range(0, n, 1):      # <- falta o "total = 0"
        total += i
    return total

print("f1(5) =", f1(5))           # 10
print("f2(5) =", f2(5))           # 0
print("f3(5) =", f3(5))           # 4
# print("f4(5) =", f4(5))        # <- descomenta e corre: UnboundLocalError

# f1 é a versão correta: 0 + 1 + 2 + 3 + 4 = 10.
# As outras 3 são os 3 erros mais comuns com acumuladores.
# Vamos ver cada uma:



# %%
"""
b) f2 — o return dentro do ciclo [3 min -> 12:05] --------------------------

    O return faz DUAS coisas:
        1. devolve o valor
        2. TERMINA a função imediatamente (o ciclo morre ali mesmo)

    No f2 o return está dentro do ciclo (mais indentado).
    Logo na 1ª volta (i = 0): total = 0 + 0 = 0 -> return 0 -> acabou.
    As voltas 1, 2, 3, 4 nunca acontecem.

    Em Python, a indentação NÃO é só estética: muda o que o programa faz.
"""
def f2_(n: int) -> int:
    total = 0
    for i in range(0, n, 1):
        total += i
        return total

print("f2_(5) =", f2_(5))           # 0
print("f2_(0) =", f2_(0))           # None!

# E o f2(0)? range(0, 0) é vazio -> o ciclo não dá nenhuma volta
# -> nunca chega ao return -> a função acaba "pelo fundo"
# -> uma função que acaba sem return devolve None.



# %%
"""
b.1) Um return dentro do ciclo nem sempre é bug [4 min -> 12:09] -----------
"""
# MAS: um return dentro do ciclo nem sempre é um bug!
# É muito útil quando já sabemos a resposta e não vale a pena continuar:

def has_multiple_of_7(a: int, b: int) -> bool:
    """ Verifica se há algum múltiplo de 7 entre a e b (inclusive). """
    for i in range(a, b + 1):
        if i % 7 == 0:
            return True           # encontrei um! não preciso de ver o resto
    return False                  # FORA do ciclo: só chego aqui se vi tudo e não encontrei

print(has_multiple_of_7(1, 10))   # True  (o 7)
print(has_multiple_of_7(8, 13))   # False

# O return False fora do ciclo é essencial:
# só podemos dizer "não há" depois de ver TODOS.
# Este padrão ("procurar e sair mais cedo") vai aparecer MUITO nos testes.



# %%
"""
c) f3 — o acumulador dentro do ciclo [3 min -> 12:12] ----------------------

    No f3 o "total = 0" está DENTRO do ciclo.
    Então em CADA volta o total volta a zero antes de somar:

        i = 0:  total = 0,  total = 0 + 0 = 0
        i = 1:  total = 0,  total = 0 + 1 = 1
        i = 2:  total = 0,  total = 0 + 2 = 2
        i = 3:  total = 0,  total = 0 + 3 = 3
        i = 4:  total = 0,  total = 0 + 4 = 4    <- é este que sobra

    O acumulador "esquece" tudo o que tinha. Só sobrevive a última volta.
    Receita: inicializar SEMPRE o acumulador ANTES do ciclo.
"""

def f3_(n: int) -> int:
    for i in range(0, n, 1):
        total = 0                 # <- dentro do ciclo
        total += i
        print(f"   i={i}  total={total}")
    return total

print("f3(5) =", f3_(5))





# %%
"""
d) f4 — variável sem valor: UnboundLocalError [2 min -> 12:14] -------------

    No f4 ninguém disse com que valor o total começa.
    Na 1ª volta, "total += i" significa "total = total + i"
    -> o Python tenta LER o total para lhe somar o i
    -> mas o total ainda não tem valor nenhum -> erro.

    "Unbound Local" = "variável local sem valor associado".

"""
def f4_corrigida(n: int) -> int:
    total = 0                     # a linha que faltava
    for i in range(0, n, 1):
        total += i
    return total

print("f4 corrigida (5) =", f4_corrigida(5))


# Resumo do ex 22 — os 3 bugs clássicos de um acumulador:
#   f2 -> return dentro do ciclo   -> sai logo na 1ª volta
#   f3 -> inicialização no ciclo   -> reset a cada volta, só fica a última
#   f4 -> sem inicialização        -> UnboundLocalError
# Os 3 aparecem em testes, muitas vezes disfarçados.






# %%
"""
===========================================================================
CICLO FOR + IF/ELSE — executar à mão, como no teste [6 min -> 12:22]
===========================================================================

    No teste não há computador.
    Há perguntas do tipo "o que dá este código?".
    A técnica é fazer uma TABELA com uma coluna por variável
    e uma linha por volta do ciclo. Devagar, sem saltar passos.

    (No Spyder também dá para ver isto passo a passo com o DEBUGGER:
     clicam ao lado do número da linha para pôr um "breakpoint" (ponto vermelho),
     correm com Ctrl+F5, e avançam linha a linha com Ctrl+F10.
     O "Variable Explorer" mostra os valores das variáveis em cada momento.)

a) PERGUNTA DE TESTE (Teste 1 2024/25, 1c) [6 min -> 12:22] ----------------

    No código abaixo, qual o valor que fica em x quando o ciclo termina?
        A. 195     B. 26     C. 28     D. 13

    Resolvam primeiro à mão, sem correr!
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
print("x =", x)

# Resposta: D. 13
#
# O truque: aqui é "x = ...", não é "x += ..."!
# Em cada volta o x é SUBSTITUÍDO, não acumulado.
# Então só interessa a ÚLTIMA volta. Não é preciso fazer as 14!
#
# A última volta é i = 13 (o range pára ANTES do 14):
#   13 % 3 == 1          -> não
#   13 % 5 == 3, 13 % 4 == 1  -> não
#   13 % 7 == 6          -> não
#   else                 -> x = 13
#
# E as outras respostas são armadilhas:
#   C. 28  = 14 * 2  -> quem pensou que o 14 estava incluído (off-by-one!)
#   B. 26  = 13 * 2  -> quem se enganou no elif do 7
#   A. 195 = 15 * 13 -> quem se enganou no 13 % 3



# %%
"""
===========================================================================
Guião 03b, exercícios 23, 24 + o operador "in" [32 min -> 12:54]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) O operador "in" (e "not in") [4 min -> 12:26] ---------------------------

    VALOR in COLEÇÃO  -> True se o valor estiver lá dentro, False se não.
    Funciona com várias coisas.
    Escrevam e corram: antes de cada linha, adivinhem se dá True ou False.
"""

print(7 in range(1, 7))            # False  -> o range não inclui o 7!
print("a" in "banana")             # True   -> uma letra dentro de uma string
print("nan" in "banana")           # True   -> até pedaços de texto
print(3 in [1, 3, 5])              # True   -> list (lista), com parênteses retos []
print(3 in (1, 3, 5))              # True   -> tuple (tuplo), com parênteses ()
print(3 in {1, 3, 5})              # True   -> set (conjunto), com chavetas {}
print(3 in {1: "a", 2: "b", 3: "c"})  # True   -> dicionário, procura nas CHAVES
print(4 not in {1, 3, 5})          # True   -> "not in" é o contrário

# Set, Lista, Tuplo, Dicionário:
#   - lista: preserva a ordem (1º, 2º, 3º...) e pode ter repetidos
#   - tuplo: preserva a ordem (1º, 2º, 3º...) e pode ter repetidos, mas é imutável
#   - set: não preserva a ordem e sem repetidos, e a pesquisa é super rápida
#   - dicionário: pares chave -> valor; chaves sem repetidos; o "in" procura nas CHAVES (não nos valores)
# (tuplos e dicionários vêm mais para a frente — por agora basta saber que o "in" funciona com todos)
#
# Atenção: o "in" do "for i in range(...)" e o "in" de "3 in {1,3,5}"
# escrevem-se igual mas são coisas diferentes:
#   for x in coleção:     -> PERCORRE a coleção, um valor por volta
#   x in coleção          -> PERGUNTA se x está lá (dá True/False)



# %%
"""
b) EXERCICIO 23 (GUIÃO) — Duração de um mês [9 min -> 12:35] ------------------

    23 - Escreva um programa que receba do utilizador um mês e um ano,
    ambos valores inteiros, e escreva a duração desse mês em dias.
    Repare que a duração do mês de fevereiro depende do facto do ano
    ser ou não bissexto.

"""

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def month_length(month: int, year: int) -> int:
    """ Number of days of a given month (in a given year).
        Precondition: 1 <= month <= 12
    """
    if month in {1, 3, 5, 7, 8, 10, 12}:
        return 31
    elif month in {4, 6, 9, 11}:
        return 30
    elif is_leap_year(year):          # se chegámos aqui, só pode ser fevereiro
        return 29
    else:
        return 28

    # Sem o "in", teríamos de escrever:
    # if month == 1 or month == 3 or month == 5 or month == 7 or month == 8 or month == 10 or month == 12:
    #     return 31

# Testes rápidos
print(month_length(4, 2023), month_length(12, 2023))  # 30 31
print(month_length(2, 2024), month_length(2, 2023))   # 29 28
print(month_length(2, 1900), month_length(2, 2000))   # 28 29

# Reparem no terceiro ramo: "elif is_leap_year(year)" não pergunta pelo mês!
# Não precisa: se os dois primeiros ramos falharam, e a precondição
# garante 1 <= month <= 12, então o mês só pode ser o 2.
# O elif aproveita a informação dos ramos de cima (a ORDEM importa, aula 2).



# %%
# b.1) main() para o problema 23 [2 min -> 12:37]
def main() -> None:
    month = int(input("Mês: "))
    year = int(input("Ano: "))
    print(month_length(month, year))

main()



# %%
"""
c) EXERCICIO 24 (GUIÃO, Mooshak H) — Posição de uma data no ano [14 min -> 12:51]

    24 - Escreva um programa que receba do utilizador uma data completa
    - usando três inteiros: dia (1..31), mês (1..12) e ano - e responda
    a posição dessa data dentro do respetivo ano.
    Por exemplo, a data 1/1/2008 tem a posição 1 e a data 31/12/2008
    tem a posição 366. Se a data não existir, o programa escreve
    "DATA INVÁLIDA".

    Pensar primeiro no papel: qual é a posição de 10 de março de 2024?
        janeiro inteiro (31) + fevereiro inteiro (29) + 10 dias de março
        = 31 + 29 + 10 = 70

    Ou seja: somar os meses COMPLETOS antes do mês da data, e depois o dia.
"""

# correr células acima para ativar estas funções:
# def is_leap_year(year: int) -> bool:
# def month_length(month: int, year: int) -> int:

def is_date_valid(day: int, month: int, year: int) -> bool:
    """ Validate a calendar date. """
    return 1 <= month <= 12 and 1 <= day <= month_length(month, year)

    # A versão "comprida" (funciona, mas é o que se chama escrever código a mais):
    # if 1 <= month <= 12 and 1 <= day <= month_length(month, year):
    #     return True
    # else:
    #     return False
    # A condição JÁ É True ou False -> devolvemo-la diretamente.

def day_order(day: int, month: int, year: int) -> int:
    """ Position of a date within the respective year, starting with 1.
        Precondition: is_date_valid(day, month, year)
    """
    total = day                           # o acumulador começa já com os dias do mês atual
    for m in range(1, month):             # meses COMPLETOS antes: 1, 2, ..., month-1
        total += month_length(m, year)
    return total

# Exemplos para testar:
print(day_order(1, 1, 2008))              # 1    -> janeiro: range(1, 1) é vazio, só soma o dia
print(day_order(31, 12, 2008))            # 366
print(day_order(10, 3, 2024))             # 70   -> o do exemplo lá em cima
print(is_date_valid(29, 2, 2023))         # False (2023 não é bissexto)
print(is_date_valid(29, 2, 2024))         # True
print(is_date_valid(5, 13, 2024))         # False (não há mês 13)
print(is_date_valid(0, 5, 2024))          # False (não há dia 0)



# %%
# c.1) main() para o problema 24 [3 min -> 12:54]
def main() -> None:
    day = int(input("Dia: "))
    month = int(input("Mês: "))
    year = int(input("Ano: "))
    if is_date_valid(day, month, year):
        print(day_order(day, month, year))
    else:
        print("DATA INVÁLIDA")

main()

# Pela primeira vez o programa VALIDA os dados do utilizador.
# Lembram-se da aula 2? Precondição = contrato, validação = verificar mesmo.
#   - day_order TEM precondição: confia que a data é válida, não verifica nada.
#   - o main é o "cliente": valida com is_date_valid ANTES de chamar day_order.
# Cada função faz uma coisa só -> fica tudo simples.
#
# Aqui o FIM excluído do range dá-nos jeito: range(1, month) pára no mês anterior.
# NÃO queremos o próprio mês (esse ainda não está completo, contamos só "day" dias dele).



# %%
"""
===========================================================================
FECHO — dúvidas e Mooshak [6 min -> 13:00]
===========================================================================

    - Dúvidas do dia
    - Submeter no Mooshak os exercícios 20 (F), 21 (G) e 24 (H)
    - Logo à tarde, na teórica, vão rever tudo isto com mais formalidade
"""



# %%
"""
===========================================================================
CONSOLIDAR — exercícios de testes anteriores
(se sobrar tempo na aula; senão, ficam para casa — todos com solução)
===========================================================================

    Tentem primeiro sem olhar para a solução: 
    é exatamente este o nível do teste.
"""



# %%
"""
PERGUNTA DE TESTE (Teste 1 2025/26, 1b) — executar à mão ------------------

    Escreva o resultado da seguinte chamada: add([1,2,3,4]) = ____

    Aqui o for percorre diretamente os valores de uma lista (como no "IPCE"
    e no [0, 10, 1] de há bocado). Em cada volta, v é um dos valores.
    Façam a tabela à mão: v | par? | total
"""

def add(l: list[int]) -> int:
    total = 0
    for v in l:
        if v % 2 == 0:
            total += v
        else:
            total -= 1
    return total

print("add([1,2,3,4]) =", add([1, 2, 3, 4]))

# Tabela:
#   v | par? | total
#   - |  -   |   0    (antes do ciclo)
#   1 | não  |  -1
#   2 | sim  |   1
#   3 | não  |   0
#   4 | sim  |   4    <- resposta
#
# Aqui sim, é um acumulador (+= e -=) -> tem de se fazer todas as voltas.
# Pergunta a fazer sempre: "a variável é SUBSTITUÍDA ou ACUMULADA em cada volta?"



# %%
"""
EXERCICIO (TESTE 1 2024/25, pergunta 3) — Soma de múltiplos ---------------

    Escreva uma função inteira para somar todos os múltiplos dum número
    inteiro positivo m que sejam menores ou iguais que outro número inteiro
    positivo lim. Exemplos:
        sum_multiples(10, 10) == 0+10 == 10
        sum_multiples(5, 10)  == 0+5+10 == 15
        sum_multiples(2, 10)  == 0+2+4+6+8+10 == 30
        sum_multiples(1, 10)  == 0+1+2+...+10 == 55

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def sum_multiples(m: int, lim: int) -> int:
            ''' Sum all the multiples of m until lim inclusive.
                Precondition: m > 0 and lim > 0
            '''
"""

def sum_multiples_v1(m: int, lim: int) -> int:
    """ Sum all the multiples of m until lim inclusive.
        Precondition: m > 0 and lim > 0
    """
    total = 0
    for i in range(0, lim + 1):           # passa por TODOS os números de 0 a lim
        if i % m == 0:                    # e só soma os múltiplos
            total += i
    return total

def sum_multiples(m: int, lim: int) -> int:
    """ Sum all the multiples of m until lim inclusive.
        Precondition: m > 0 and lim > 0
    """
    total = 0
    for i in range(0, lim + 1, m):        # salta diretamente de múltiplo em múltiplo!
        total += i
    return total

    # Versão Gauss, sem ciclo nenhum (há k = lim // m múltiplos, fora o 0):
    # k = lim // m
    # return m * k * (k + 1) // 2

print(sum_multiples(10, 10), sum_multiples(5, 10), sum_multiples(2, 10), sum_multiples(1, 10))
print(sum_multiples_v1(10, 10), sum_multiples_v1(5, 10), sum_multiples_v1(2, 10), sum_multiples_v1(1, 10))
# 10 15 30 55 (as duas linhas)

# Os dois dão o mesmo, mas:
#   v1:             para m=10, lim=10 dá 11 voltas e faz 11 testes com %
#   sum_multiples:  para m=10, lim=10 dá 2 voltas (0 e 10) e zero testes
# É exatamente o "menos trabalho desnecessário" que o enunciado pede.
# Aqui o 3º argumento do range (o PASSO, que vimos no range(0, 20, 5) do início) faz o trabalho todo.
#
# E o lim + 1? Off-by-one outra vez: "menores OU IGUAIS a lim" -> lim incluído.



# %%
"""
EXERCICIO (TESTE 1 2025/26, pergunta 3) — Série de Zeno -----------------

    Considere a famosa série do paradoxo de Zeno. A soma desta série vale 1.

        1/2 + 1/4 + 1/8 + 1/16 + ...     (o termo n é 1 / 2**n, n = 1, 2, 3...)

    Escreva uma função real para calcular a soma dos primeiros k termos
    desta série. Exemplos:
        zeno(0) == 0.0      zeno(2) == 0.75
        zeno(1) == 0.5      zeno(3) == 0.875

    Programe a função usando um ciclo. Tente não usar a biblioteca math.

        def zeno(k: int) -> float:
            ''' Sum of the k first terms of the Zeno series '''
"""

def zeno_v1(k: int) -> float:
    """ Sum of the k first terms of the Zeno series.
        Precondition: k >= 0
    """
    total = 0.0                           # float, porque o resultado é real
    for n in range(1, k + 1):             # n = 1, 2, ..., k -> k termos
        total += 1 / 2 ** n               # ** tem prioridade: 1 / (2 ** n)
    return total

def zeno(k: int) -> float:
    """ Sum of the k first terms of the Zeno series.
        Precondition: k >= 0
    """
    total = 0.0
    term = 0.5                            # o 1º termo
    for _ in range(k):                    # k voltas; o "_" diz "não uso esta variável"
        total += term
        term /= 2                         # o termo seguinte é metade do atual
    return total

print(zeno(0), zeno(1), zeno(2), zeno(3))              # 0.0 0.5 0.75 0.875
print(zeno_v1(0), zeno_v1(1), zeno_v1(2), zeno_v1(3))  # 0.0 0.5 0.75 0.875
print(zeno(50))                                        # quase 1 (a flecha lá chega!)

# O zeno_v1 calcula 2 ** n do zero em cada volta.
# O zeno reaproveita o termo anterior: dividir por 2 é mais barato.
# Truque muito usado em séries: "o próximo termo a partir do anterior".
#
# Aqui há DOIS "acumuladores": total (soma) e term (vai sendo dividido).
#
# Paradoxo de Zeno: para atravessar a sala, primeiro andas metade,
# depois metade do que falta, depois metade do que falta...
# São infinitos passos, mas a soma dá 1 -> chegas lá na mesma.



# %%
"""
EXERCICIO (EXAME RECURSO 2023/24, pergunta 2) — Série de ln(2) ------------

    Esta famosa série alternada converge para o logaritmo natural de 2:

        1 - 1/2 + 1/3 - 1/4 + 1/5 - ...

    Escreva uma função real para calcular a soma dos n primeiros termos
    desta série. Exemplos:
        ln2(0) == 0.0     ln2(2) == 0.5
        ln2(1) == 1.0     ln2(3) == 0.8333

    Escreva a função usando um ciclo. Não precisa da biblioteca math.

    Dica: é o zeno com um sinal que vai trocando.
"""

def ln2(n: int) -> float:
    """ Soma dos n primeiros termos da série 1 - 1/2 + 1/3 - ...
        Precondition: n >= 0
    """
    total = 0.0
    sign = 1                              # +1, -1, +1, -1, ...
    for k in range(1, n + 1):
        total += sign / k
        sign = -sign                      # troca o sinal para a volta seguinte
    return total

    # Alternativa: escolher o sinal com um if, pela paridade de k
    # for k in range(1, n + 1):
    #     if k % 2 == 1:
    #         total += 1 / k
    #     else:
    #         total -= 1 / k

print(ln2(0), ln2(1), ln2(2), ln2(3))     # 0.0 1.0 0.5 0.8333333333333333
print(ln2(100_000))                       # perto de 0.693147... (= ln 2)



# %%
"""
EXERCICIO (TESTE 1 2025/26, pergunta 4) — O mês de um dia do ano ----------

    É o ex 24 AO CONTRÁRIO: dado o número de ordem de um dia no ano,
    dizer qual o mês. A função recebe o número de ordem e se o ano é
    bissexto. Exemplos:
        get_month(5, False)   == 1     # janeiro
        get_month(31, False)  == 1     # janeiro
        get_month(60, True)   == 2     # fevereiro
        get_month(60, False)  == 3     # março
        get_month(366, True)  == 12    # dezembro

    Para saber o número de dias de cada mês, a função usa a constante
    local DURATIONS (uma lista — spoiler: listas vêm aí nas próximas aulas).
    DURATIONS[0] é a duração de janeiro, DURATIONS[1] de fevereiro, etc.

        def get_month(order: int, leap_year: bool) -> int:
            ''' Calculate the month corresponding to some day order in a year
                Precondition: 1 <= order <= 366 '''
            DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]

    Ideia: ir "gastando" os dias mês a mês.
    Enquanto o order não couber no mês atual, tira-se esse mês e avança-se.
"""

def get_month(order: int, leap_year: bool) -> int:
    """ Calculate the month corresponding to some day order in a year
        Precondition: 1 <= order <= 366 (e 366 só se leap_year)
    """
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    for month in range(1, 13):
        days = DURATIONS[month - 1]       # -1 porque a lista começa na posição 0
        if month == 2 and leap_year:
            days += 1
        if order <= days:                 # o dia cabe neste mês -> encontrado!
            return month                  # return dentro do ciclo: "procurar e sair"
        order -= days                     # não cabe -> gastar este mês e passar ao seguinte
    return 12                             # nunca chega aqui se a precondição for cumprida

print(get_month(5, False), get_month(31, False), get_month(60, True),
      get_month(60, False), get_month(366, True))       # 1 1 2 3 12



# %%
"""
EXERCICIO (TESTE 1 2024/25, pergunta 5, "Difícil") — Pinheiro de Natal -----

    Escreva uma função para desenhar um pinheiro de Natal com copa de
    tamanho n. A copa é um triângulo isósceles com n linhas:
    a 1ª linha é uma estrela '*', as restantes n-1 linhas são agulhas '^'.
    Por baixo, um tronco 3x3 de madeira '#'.
    Para n=10: a 1ª linha tem 9 espaços à esquerda; a 2ª tem 8;
    a 10ª tem 0; as três linhas do tronco têm 8, tal como a 2ª linha.

    Código de partida (dado no teste, uso opcional):
        def draw_segment(x: str, n: int):
            for i in range(n):
                print(x, end='')

    Peça nova: print(x, end='') escreve x SEM mudar de linha no fim.
    (normalmente o print acaba com um "Enter" escondido; o end='' tira-o)
    Para mudar de linha quando quisermos: print() sozinho.

    Ideia: para cada linha, contar quantos espaços e quantos símbolos.
    Linha k da copa (k = 0, 1, ..., n-1):
        espaços  = n - 1 - k
        símbolos = 2 * k + 1   (1, 3, 5, 7, ...)
"""

def draw_segment(x: str, n: int) -> None:
    """ Draw a partial line with length n using the char x.
        Precondition: len(x) == 1 and n >= 0
    """
    for i in range(n):
        print(x, end='')

def draw_line(spaces: int, x: str, count: int) -> None:
    """ Desenha uma linha completa: 'spaces' espaços, 'count' vezes o x, e muda de linha.
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
    draw_line(n - 1, a, 1)                    # a estrela no topo
    for k in range(1, n):                     # as n-1 linhas de agulhas
        draw_line(n - 1 - k, b, 2 * k + 1)
    for _ in range(3):                        # o tronco
        draw_line(n - 2, c, 3)

draw_pine_tree('*', '^', '#', 10)

# Um ciclo (em draw_pine_tree) que chama uma função que tem outro ciclo
# (draw_segment) -> um "ciclo dentro de um ciclo", mas arrumado em funções.
# A função auxiliar draw_line com comentário e precondição é exatamente
# o que o enunciado sugeria ("Talvez deseje definir uma função auxiliar").








# %%
"""
===========================================================================
Contexto / Escopo de variáveis
===========================================================================
    Conceito 1: 
    Antes de ler ou usarmos uma variável, temos de lhe dar um valor.
"""

# Descomenta e corre este código:
# estamos a tentar usar uma variável que nunca recebeu valor nenhum.

# print(total_que_nao_existe)



# %%
"""
    Conceito 2: 
    Não dá para aceder às variáveis dentro de uma função a partir de fora.

    Cada função tem o seu próprio "espaço de nomes" (namespace).
    Se criamos variáveis dentro de uma função, elas só existem lá dentro.

"""
# Este código dá erro - o print exterior não consegue ver a variavel dentro da função
def fun():
    a = 2
# print(a)



# %%
"""
    Conceito 3:
    Quando criamos variavel dentro de função com mesmo nome,
    ela "tapa" a variável de fora (shadowing).

    Ou seja, dá para LER variáveis de fora a partir de dentro duma função,
    mas só enquanto a função não lhes ATRIBUIR nenhum valor.

     A regra é: 
        - se a variável é atribuída dentro da função, 
            o Python considera-a local e já não consegue ver a global
        - se a variável é apenas lida, 
            o Python procura primeiro uma local, depois uma global

    O Python decide isto lendo a função INTEIRA antes de a correr, 
    não linha a linha:
        - se em QUALQUER linha da função houver uma atribuição a esse nome
          (=, +=, -=, ...), o Python já a marca como LOCAL na função TODA,
          mesmo nas linhas ANTES dessa atribuição
        - senão, quando o nome é lido, o Python vai procurá-lo fora
"""

# só leitura -> o print dentro da função consegue ver o total fora da funçao 
total = 0
def so_le() -> None:
    print("só leitura:", total)
so_le()

# mudar variavel -> essa variável passa a ser local à função TODA
# (mesmo que a atribuição só apareça numa linha a seguir ao print)
def le_e_muda() -> None:
    print("total local (ainda sem valor):", total)   # <- dá erro AQUI, não na linha de baixo!
    total = 5          

# le_e_muda()   # <- descomenta e corre: UnboundLocalError, já na linha do print
#               # prova que o Python decidiu ANTES de correr que "total" era local

def le_e_muda_certo() -> None:
    total = 5
    print("criado total local à funçao:", total)
    
le_e_muda_certo()   
print("total global não mudou:", total)  # <- imprime 0, a variável global não mudou



# %%
"""
    Conceito 4:
    Estes limites só se aplicam a funções, 
    não se aplicam a ciclos for/while ou if/else.

    Em termos de variáveis, mesmo que criadas dentro de um if ou for
    é como se estivesse tudo ao mesmo nível. 
"""

# Mesmo tendo vários níveis na logica do cdigo:
#       - global, função, for, if,
# em termos de variáveis só temos 2 níveis:
#       - global, funçao
a = 5
def fun2() -> None:
    a = 10
    print("a local no início:", a)
    for i in range(1):
        a = 20
        print("a local dentro do for:", a)
        if True:
            a = 30
            print("a local dentro do if:", a)
        print("a local depois do if:", a)
    print("a local no fim:", a)

print("a global no início:", a)
fun2()
print("a global no fim:", a)




# %%
"""
EXERCICIO A — Prever o output -----------------------------------------

    Sem correr, escrevam no papel o que os dois prints vão mostrar.
    Depois corram e confirmem.
"""

x = 100

def misterio(x):
    x = x + 1          # este x é o PARÂMETRO, uma caixa só da função
    y = x * 2
    for i in range(3):
        y += i          # y e i sobrevivem ao for, mas são locais à função
    return y

r = misterio(x)
print(r, x)

# Solução: 205 100
# Dentro da função, x começa como CÓPIA do valor de fora (100), passa a 101,
# e y = 202. O for depois soma 0+1+2 = 3, logo y fica 205 -> é o que é devolvido.
# O x de fora nunca é tocado: o x da função é outra caixa, só com o mesmo nome
# (tal como vimos no f do início desta secção).



# %%
"""
EXERCICIO B — Encontrar e corrigir o bug ------------------------------

    Este código tenta classificar uma nota, mas tem um bug escondido
    de contexto de variáveis. Corram e vejam o que acontece com
    classifica(10). Depois corrijam.
"""

def classifica(nota):
    if nota > 10:
        resultado = "Aprovado"
    elif nota < 10:
        resultado = "Reprovado"
    return resultado

print(classifica(15))   # Aprovado
print(classifica(5))    # Reprovado
# print(classifica(10))   # <- descomenta: UnboundLocalError!

# O que se passa: nota > 10 e nota < 10 NÃO cobrem o caso nota == 10.
# Nesse caso nenhum ramo do if corre, "resultado" nunca é criado,
# e o return tenta ler uma variável local que não tem valor -> erro.
# É a mesma armadilha do Conceito 3: o if/elif não é uma "caixa" 
# separada, mas se nenhum ramo correr, a variável simplesmente não existe.

# Solução: garantir que HÁ SEMPRE um ramo que cria "resultado"
def classifica_certo(nota):
    if nota > 10:
        resultado = "Aprovado"
    elif nota < 10:
        resultado = "Reprovado"
    else:
        resultado = "Aprovado"   # 10 valores é nota de aprovação
    return resultado

print(classifica_certo(15), classifica_certo(5), classifica_certo(10))
# Aprovado Reprovado Aprovado



# %%
"""
EXERCICIO C — Encontrar e corrigir o bug ------------------------------

    Esta função devia contar quantos números pares há numa lista.
    Tem o mesmo tipo de bug do f4 (ex. 22). Encontrem-no antes de correr.
"""

def conta_pares(lista):
    for v in lista:
        if v % 2 == 0:
            count += 1
    return count

# print(conta_pares([1, 2, 3, 4, 5, 6]))  # <- descomenta: UnboundLocalError

# O bug: falta inicializar o acumulador ANTES do ciclo.
# Como há "count += 1" lá dentro, o Python trata count como local à função
# toda, mas nunca lhe deu um valor inicial -> rebenta logo no primeiro par.

def conta_pares_certo(lista):
    count = 0                     # a linha que faltava
    for v in lista:
        if v % 2 == 0:
            count += 1
    return count

print(conta_pares_certo([1, 2, 3, 4, 5, 6]))   # 3