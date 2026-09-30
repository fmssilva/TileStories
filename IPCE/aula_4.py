# %%
"""
===========================================================================
GUIA DE SESSÃO - Prática 04 - IPCE 2026/2027
===========================================================================

Cobre os guiões 04a (ex. 25-27) e 04b (ex. 28-31).
https://ipce-184ea7.gitlab.io/

Temas de hoje:
    1. Booleanos a sério: construir o and, o or e o not "à mão".
    2. Números reais (float): tabelas, raízes, arredondamentos, séries.
       E a grande surpresa do dia: o computador NÃO sabe fazer 0.1 + 0.2.

Como usar este ficheiro no Spyder:
    - cada bloco que começa com "# %%" é uma célula
    - Ctrl + Enter  -> corre só a célula onde está o cursor
    - F5            -> corre o ficheiro todo (vai pedir vários inputs!)

Mooshak de hoje: I (ex 26), J (ex 27), K (ex 29), L (ex 31)
"""






# %%
"""
===========================================================================
LOGÍSTICA / Revisões [8 min -> 10:18]
===========================================================================

    - Façam download deste documento da drive

    - Folha de Presenças
    - Mooshak: problemas F, G, H da aula passada já submetidos?
    - Dúvidas da aula passada (ciclo for, range, acumuladores)

AQUECIMENTO - prever o output (sem correr!) --------------------------------

    Quanto vale total no fim?
"""

total = 0
for i in range(1, 10, 3):
    total += i
    print(i, total)
print(total)

# Solução: 12
# O range(1, 10, 3) dá 1, 4, 7 (o 10 já não entra, o FIM nunca entra).
# 1 + 4 + 7 = 12
# Receita do acumulador: começa ANTES do ciclo, cresce DENTRO, usa-se DEPOIS.





# %%
"""
    Ciclos dentro de ciclos

    EXERCICIO: Desenhe um quadrado de asteriscos com 5 linhas e 5 colunas.
"""
rows = 3
cols = 6
for i in range(rows):         
    for j in range(cols):  
        print(" * ", end="")  # end="" evita o salto de linha
    print()                 # salto de linha no fim da linha




# %%
"""
===========================================================================
Guião 04a, exercício 25 - Booleanos "à mão" [27 min -> 10:45]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) Aquecimento - tabela da verdade com dois for [6 min -> 10:24] -----------

    Exercício: print tabela de verdade AND e OR
    
    »» Um bool só tem 2 valores: False e True.
    Então podemos pôr um for a passar pelos dois!
    E um for DENTRO de outro for passa por todas as combinações (2 x 2 = 4).

    Corram e comparem com o que aprenderam no secundário.
"""

print("a   b  |  and")
for a in [0, 1]:
    for b in [0, 1]:
        print(f"{a}   {b}  |   {a and b:<5}")

print("\na   b  |  or")
for a in [0, 1]:
    for b in [0, 1]:
        print(f"{a}   {b}  |   {a or b:<5}")


# Para cada valor do "a" de fora, o for de dentro dá a volta completa ao "b".
# (ciclos dentro de ciclos: vamos ver mais disto nas próximas aulas)
#
# Resumo da tabela:
#   and -> só é True se os DOIS forem True
#   or  -> só é False se os DOIS forem False






# %%
"""
b) EXERCICIO 25 (GUIÃO) - and, or e not sem usar and, or e not [12 min -> 10:36]

    25 - Se a linguagem Python não tivesse os operadores lógicos and, or e
    not, que funções booleanas escreveria para os substituir? Complete o
    código abaixo da forma mais compacta possível. Não pode usar os três
    operadores lógicos, mas pode usar o if.

    Nota sobre os nomes: "and" é palavra reservada, não pode ser nome de função.
    Daí o "_" no fim: and_, or_, not_ (é uma convenção habitual em Python).

    Pista: olhem para a tabela da célula anterior e façam UMA pergunta só.
        and: "o a é True?"  Se não é, a resposta já está decidida...
"""

def and_(a: bool, b: bool) -> bool:
    """ AND - Boolean operation. """
    if a:
        return b          # a é True -> quem decide é o b
    return False      # a é False -> nem vale a pena olhar para o b

print(and_(True, True))
print(and_(True, False))
print(and_(False, True))
print(and_(False, False))

# Reparem que podemos diretamente "if a:" e não "if a == True:".
# (regra de estilo da cadeira: nada de "== True" nem "== False")





# %%
def or_(a: bool, b: bool) -> bool:
    """ OR - Boolean operation. """
    if a:
        return True       # a é True -> já está, é True
    return b          # a é False -> quem decide é o b



print(or_(True, True))
print(or_(True, False))
print(or_(False, True))
print(or_(False, False))




# %%
def not_(a: bool) -> bool:
    """ NOT - Boolean operation. """
    if a:
        return False
    return True

print(not_(True))
print(not_(False))








# %%
"""
===========================================================================
Guião 04a, exercício 26 - Números reais e tabelas [35 min -> 11:20]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) Tipo de dados float [6 min -> 10:51] -----------------------------
"""

print(7 / 2)            # 3.5  -> a divisão "/" dá SEMPRE float
print(6 / 2)            # 3.0  -> mesmo quando a conta é exata!
print(7 // 2)           # 3    -> divisão inteira (int com int dá int)
print(2 + 0.5)          # 2.5  -> int misturado com float dá float
print(2 ** 0.5)         # 1.4142135623730951 -> raiz quadrada sem math!




# %%
# print floats com um número fixo de casas decimais: f-string com ":.Nf"
r = 2 / 3
print(r)                # 0.6666666666666666  (o Python decide)
print(f"{r:.6f}")       # 0.666667            (6 casas, e arredonda)
print(f"{r:.2f}")       # 0.67
print(f"{5:.6f}")       # 5.000000 -> também funciona com int





# %%
# E o que acontece se misturarmos texto com números?
# dá erro:
# print("3" + 4)        # <- descomenta: TypeError

# O Python recusa: não adivinha se queríamos 7 ou "34".
# Chama-se TIPAGEM FORTE: o Python não converte tipos "às escondidas".
# (int + float é uma exceção aceite, porque aí não há dúvida nenhuma)
# Se quisermos mesmo juntar, dizemos explicitamente:
print("3" + str(4))     # 34
print(int("3") + 4)     # 7

# Em JavaScript usa-se Tipagem fraca. 
# Javascript tenta adivinhar os tipos. 
# Então "3" + 4 dá "34" e "3" - 4 dá -1






# %%
"""
b) EXERCICIO 26 (GUIÃO, Mooshak I) - Tabela de um polinómio [17 min -> 11:08]

    26 - Escreva um programa para tabelar o polinómio quadrático ax2+bx+c
    num dado intervalo [limInf, limSup] 
    para um determinado número de pontos numPontos (superior a 1). 
    O programa pede ao utilizador os valores reais a, b, c, limInf, limSup e o valor inteiro numPontos.
    
    Exemplo de execução
        A: 0.0
        B: 1.0
        C: 0.0
        LOWER: 0.0
        UPPER: 1.0
        N: 5
        0.000000 0.000000
        0.250000 0.250000
        0.500000 0.500000
        0.750000 0.750000
        1.000000 1.000000

    Organização do programa:
        (1) uma função real para avaliar um polinómio quadrático num ponto;
        (2) uma função sem resultado mas com 6 parâmetros para calcular
            e escrever a tabela;
        (3) função main que pede os dados e manda escrever a tabela.

    Nota: O Python só tem ranges de inteiros. 
        Não existe range(0.0, 1.0, 0.25). 
        O ciclo for vai ter de usar uma variável inteira.
        Podemos usar o for com um inteiro i = 0, 1, 2, ..., n-1
        e CALCULAR o x a partir do i.

    Pensar no papel com o exemplo: 5 pontos entre 0 e 1.
        0.00   0.25   0.50   0.75   1.00
          |------|------|------|------|
    5 pontos, mas só 4 espaços entre eles!
"""

def quadratic_polynomial_value(a: float, b: float, c: float, x: float) -> float:
    """ Eval a quadratic polynomial for the given value x """
    return a * x ** 2 + b * x + c   

def print_table(a: float, b: float, c: float,
                lower_bound: float, upper_bound: float,
                n: int) -> None:
    """ Print table of values for a quadratic polynomial in a interval.
        Precondition: lower_bound < upper_bound and n > 1
    """
    step = (upper_bound - lower_bound) / (n - 1)   # n pontos -> n-1 espaços
    for i in range(n):                             # i = 0, 1, ..., n-1 (n pontos)
        x = lower_bound + i * step
        y = quadratic_polynomial_value(a, b, c, x)
        print(f"{x:.6f} {y:.6f}")


def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    lower_bound = float(input("LOWER: "))
    upper_bound = float(input("UPPER: "))
    n_points = int(input("N: "))
    if not (lower_bound < upper_bound and n_points > 1):
        print("Valores inválidos")
    else:
        print_table(a, b, c, lower_bound, upper_bound, n_points)

main()

print_table(0.0, 1.0, 0.0, 0.0, 1.0, 5)     # o exemplo do guião
print()
print_table(1.0, 0.0, 0.0, -2.0, 2.0, 5)    # x ao quadrado: 4 1 0 1 4

# Porque é que a precondição diz n > 1?
#   n = 1 -> step = (...) / 0 -> ZeroDivisionError
#   (com 1 ponto só não faz sentido falar em "espaço entre pontos")
# A precondição avisa quem chama: "garante isto, que eu não verifico".
# Quem garante é o main, que valida os dados antes de chamar.
#
# Reparem na divisão de trabalho:
#   quadratic_polynomial_value -> só faz a conta (devolve um float)
#   print_table                -> ciclo + escrita (não devolve nada: None)
#   main                       -> fala com o utilizador


# Caça aos bugs: o main que vem no enunciado do guião tem 2 gralhas.
#   1. "and n > 1"  -> a variável chama-se n_points, não n (NameError)
#   2. "else"       -> falta o ":" no fim (SyntaxError, o Spyder pinta a vermelho)
# Copiar código "tal e qual" também exige olhar para ele!






# %%
"""
c) Porque calcular x = lower + i * step e não ir somando x += step? [5 min -> 11:13]

    A ideia "natural" seria começar em x = lower e ir somando o step.
    Matematicamente é igual. No computador... vejam:
"""

step = 0.1
x = 0.0
for i in range(11):
    print(f"{i:2}   x += step: {x:.20f}    i * step: {i * step:.20f}")
    x += step

# Com 20 casas decimais vê-se o "lixo" nos últimos dígitos.
# As DUAS colunas têm lixo (o 0.1 já vem "sujo" de origem, ver célula seguinte).
# A diferença:
#   x += step  -> cada volta junta o erro NOVO ao erro das voltas anteriores.
#                 No fim devia dar 1.0 e dá 0.99999999999999988898...
#   i * step   -> cada linha é UMA conta só, feita do zero. O erro não se acumula.
#                 No fim dá 1.0 certinho.
# Com :.6f ambas mostrariam 1.000000, mas o valor guardado é diferente.
# Numa tabela de 11 linhas não se nota. Com 1 milhão de passos, nota-se.
#
# Regra prática: quando dá para CALCULAR diretamente a partir do i,
# é melhor do que ir somando pequenos passos (que vão juntando erros).
#
# E porquê o "lixo"? Isso é a surpresa da célula seguinte.






# %%
"""
d) Curiosidade: o computador não sabe fazer 0.1 + 0.2 [7 min -> 11:20] -----
"""

print(0.1 + 0.2)                # 0.30000000000000004  (!!!)
print(0.1 + 0.2 == 0.3)         # False  (!!!)
print(f"{0.1:.20f}")            # 0.10000000000000000555

# O que se passa:
# Tentem escrever 1/3 em decimal: 0.3333333... nunca acaba.
# Com um número limitado de casas, 1/3 fica SEMPRE aproximado.
#
# O computador guarda os float em BINÁRIO (só 0 e 1) e com 64 bits.
# Em binário, o 0.1 é como o 1/3 em decimal: dízima infinita!
#     0.1 (decimal) = 0.000110011001100110011... (binário)
# Os 64 bits cortam a dízima -> o 0.1 guardado é "quase" 0.1.
#
# Isto não é um bug do Python. Todas as linguagens fazem igual:
# C, Java, JavaScript, Excel... todas seguem a norma IEEE 754.
# (experimentem =0.1+0.2-0.3 no Excel com muitas casas decimais)
#
# Os float têm cerca de 15-16 algarismos significativos certos.
# Para engenharia chega e sobra: nenhuma régua mede com 15 casas.
# O problema NÃO é a precisão. O problema é o == entre reais.
#
# Regra de ouro: NUNCA comparar floats com ==.
# Depois do intervalo vamos ver um programa que dá a resposta ERRADA por isto.

print(1 / 2 + 1 / 4 == 0.75)    # True -> metades, quartos, oitavos... são exatos em binário
                                # (tal como 1/10 e 1/100 são exatos em decimal)






# %%
"""
===========================================================================
INTERVALO [20 min -> 11:40]
===========================================================================
"""






# %%
"""
===========================================================================
Guião 04a, exercício 27 - Raízes da equação do 2º grau [28 min -> 12:08]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) EXERCICIO 27 (GUIÃO, Mooshak J) - Fórmula resolvente [15 min -> 11:55] --

    27 - Escreva um programa para achar as raízes da equação quadrática
    ax2+bx+c=0 (a != 0). O programa pede ao utilizador os valores reais
    a, b, c e responde dizendo qual o número de raízes (0, 1 ou 2) e quais
    os valores dessas raízes. No caso de haver 2 raízes, elas devem ser
    escritas por ordem crescente.

    Funções pedidas:
        - uma função para a fórmula do discriminante;
        - outra função que determina quantas são as raízes;
        - outra função que calcula a raiz da esquerda;
        - outra função que calcula a raiz da direita;
        - a função main (dada no guião).

    Relembrar (secundário):
        d = b*b - 4*a*c
        d < 0  -> 0 raízes
        d == 0 -> 1 raiz
        d > 0  -> 2 raízes: (-b - raiz(d)) / (2a)  e  (-b + raiz(d)) / (2a)

    Cabeçalhos:
        def discriminant(a: float, b: float, c: float) -> float:
        def how_many_roots(a: float, b: float, c: float) -> int:
        def root_left(a: float, b: float, c: float) -> float:
        def root_right(a: float, b: float, c: float) -> float:

    Armadilha "ordem crescente": o "-" dá SEMPRE a raiz mais pequena?
    Testem à mão com a = -1, b = 0, c = 4   (-x² + 4 = 0, raízes -2 e 2)
        (-0 - raiz(16)) / (2 * -1) = -4 / -2 = 2     <- esta é a da DIREITA!
    Dividir por um número negativo troca a ordem. Depende do sinal de a.
"""






# %%
# Solução do exercício 27 (funções)
import math

def discriminant(a: float, b: float, c: float) -> float:
    """ Discriminant of ax^2+bx+c. """
    return b * b - 4 * a * c

def how_many_roots(a: float, b: float, c: float) -> int:
    """ Number of real roots of ax^2+bx+c=0.
        Precondition: a != 0
    """
    d = discriminant(a, b, c)     # calculado UMA vez, usado duas
    if d < 0:
        return 0
    elif d == 0:                  # <- guardem esta linha na memória...
        return 1
    else:
        return 2

def root_left(a: float, b: float, c: float) -> float:
    """ Smallest root of ax^2+bx+c=0.
        Precondition: a != 0 and discriminant(a, b, c) >= 0
    """
    if a > 0:
        return (-b - math.sqrt(discriminant(a, b, c))) / (2 * a)
    else:
        return (-b + math.sqrt(discriminant(a, b, c))) / (2 * a)

def root_right(a: float, b: float, c: float) -> float:
    """ Largest root of ax^2+bx+c=0.
        Precondition: a != 0 and discriminant(a, b, c) >= 0
    """
    if a > 0:
        return (-b + math.sqrt(discriminant(a, b, c))) / (2 * a)
    else:
        return (-b - math.sqrt(discriminant(a, b, c))) / (2 * a)

print(how_many_roots(1.0, -4.0, 4.0), root_left(1.0, -4.0, 4.0))    # 1 2.0 (exemplo do guião)
print(root_left(1.0, 0.0, -4.0), root_right(1.0, 0.0, -4.0))        # -2.0 2.0
print(root_left(-1.0, 0.0, 4.0), root_right(-1.0, 0.0, 4.0))        # -2.0 2.0 (a negativo!)

# Alternativa sem olhar para o sinal de a: calcular as duas e escolher a menor.
#
#   def root_left(a: float, b: float, c: float) -> float:
#       r1 = (-b - math.sqrt(discriminant(a, b, c))) / (2 * a)
#       r2 = (-b + math.sqrt(discriminant(a, b, c))) / (2 * a)
#       if r1 < r2:
#           return r1
#       return r2
#
# Precondição do root_left: d >= 0. Se d < 0, o math.sqrt rebenta:
# print(math.sqrt(-1))   # <- descomenta: ValueError: math domain error






# %%
# Solução do exercício 27 (main, dado no guião)

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    n = how_many_roots(a, b, c)
    if n == 0:
        print("0")
    elif n == 1:
        print(f"1 {root_left(a,b,c)}")
    else:
        print(f"2 {root_left(a,b,c)} {root_right(a,b,c)}")

main()

# Exemplo do guião: A=1.0, B=-4.0, C=4.0 -> "1 2.0"
# O main é o "cliente" que respeita as precondições:
# só chama root_left quando já sabe que há pelo menos 1 raiz.






# %%
"""
b) O bug escondido: d == 0 com reais [6 min -> 12:01] ----------------------

    Testem a equação (x - 0.7)² = 0, ou seja  x² - 1.4x + 0.49 = 0.
    Matemática: d = 1.96 - 1.96 = 0 -> 1 raiz, x = 0.7.
    O nosso programa diz...
"""

print(discriminant(1.0, -1.4, 0.49))     # -2.220446049250313e-16  (devia ser 0!)
print(how_many_roots(1.0, -1.4, 0.49))   # 0  -> "não tem raízes". ERRADO!

print(discriminant(1.0, -0.2, 0.01))     # 6.938893903907228e-18   (devia ser 0!)
print(how_many_roots(1.0, -0.2, 0.01))   # 2  -> "duas raízes". ERRADO!

# O "lixo" dos floats (célula do 0.1 + 0.2) estragou a comparação d == 0.
# O d dá quase zero, mas não EXATAMENTE zero. E o == só aceita exatamente.
# (o exemplo do guião, 1 -4 4, funciona porque são inteiros disfarçados de float)
#
# A solução: em vez de "d é igual a 0?" perguntar "d está MUITO perto de 0?"
#     abs(d) < EPSILON        com EPSILON = 1e-9 (um número minúsculo)
# O abs é a função do Python para o valor absoluto: abs(-3.5) == 3.5






# %%
# Versão corrigida do exercício 27 (substitui as funções de cima)
import math

EPSILON = 1e-9            # "quase zero": abaixo disto consideramos que é zero

def is_almost_zero(v: float) -> bool:
    """ Check if a real is so close to zero that it should be zero. """
    return abs(v) < EPSILON

def how_many_roots(a: float, b: float, c: float) -> int:
    """ Number of real roots of ax^2+bx+c=0.
        Precondition: a != 0
    """
    d = discriminant(a, b, c)
    if is_almost_zero(d):         # 1º testar o "quase zero"...
        return 1
    elif d < 0:                   # ...só depois os sinais
        return 0
    else:
        return 2

def sqrt_discriminant(a: float, b: float, c: float) -> float:
    """ Square root of the discriminant (a "quase zero" counts as zero).
        Precondition: how_many_roots(a, b, c) > 0
    """
    d = discriminant(a, b, c)
    if is_almost_zero(d):
        return 0.0                # -2.2e-16 ia rebentar o math.sqrt!
    return math.sqrt(d)

def root_left(a: float, b: float, c: float) -> float:
    """ Smallest root of ax^2+bx+c=0.
        Precondition: a != 0 and how_many_roots(a, b, c) > 0
    """
    if a > 0:
        return (-b - sqrt_discriminant(a, b, c)) / (2 * a)
    else:
        return (-b + sqrt_discriminant(a, b, c)) / (2 * a)

def root_right(a: float, b: float, c: float) -> float:
    """ Largest root of ax^2+bx+c=0.
        Precondition: a != 0 and how_many_roots(a, b, c) > 0
    """
    if a > 0:
        return (-b + sqrt_discriminant(a, b, c)) / (2 * a)
    else:
        return (-b - sqrt_discriminant(a, b, c)) / (2 * a)

print(how_many_roots(1.0, -1.4, 0.49), root_left(1.0, -1.4, 0.49))   # 1 0.7
print(how_many_roots(1.0, -0.2, 0.01), root_left(1.0, -0.2, 0.01))   # 1 0.1

# Porque foi preciso o sqrt_discriminant?
# Agora how_many_roots diz "1 raiz" para d = -2.2e-16.
# Mas o root_left ia fazer math.sqrt(-2.2e-16) -> ValueError!
# Corrigir um sítio obriga a pensar em TODOS os sítios que usam o d.
#
# O main de cima não precisa de mudar: se o correrem outra vez,
# já usa estas funções novas (o Python usa sempre a versão mais recente).
#
# Para o Mooshak, a versão com d == 0 costuma passar
# (os testes usam números "bonitos"). Mas agora sabem que há casos em que falha.






# %%
"""
c) PERGUNTA DE TESTE (Teste 1 2025/26, pergunta 2, 3 valores) - is_root [7 min -> 12:08]

    Escreva uma função booleana para testar se um valor r é raiz da
    equação de 2º grau ax²+bx+c=0. Note que não é preciso usar a fórmula
    resolvente (não complique!). Exemplos:
        is_root(0.0, 1.0, -3.0, 2.0) == False
        is_root(1.0, 1.0, -3.0, 2.0) == True

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def is_root(r: float, a: float, b: float, c: float) -> bool:
            ''' Check if r is a root of ax²+bx+c=0 '''

    Ideia: r é raiz se, pondo r no lugar de x, a conta der 0.
    E já sabemos: "dar 0" com floats é "dar QUASE 0".
"""

def is_root(r: float, a: float, b: float, c: float) -> bool:
    """ Check if r is a root of ax^2+bx+c=0 """
    EPSILON = 1e-9                                 # constante local à função
    return abs(a * r * r + b * r + c) < EPSILON

print(is_root(0.0, 1.0, -3.0, 2.0))   # False
print(is_root(1.0, 1.0, -3.0, 2.0))   # True
print(is_root(0.7, 1.0, -1.4, 0.49))  # True  (com == 0 dava False!)

# Versão que o enunciado quer evitar ("não complique!"):
# calcular as raízes com a fórmula resolvente e comparar com r.
# Mais código, mais contas, e ainda mais comparações de floats.
#
# Devolvemos a condição DIRETAMENTE (é um bool), sem if/return True/return False.
#
# E o math.isclose (da aula teórica)?
#     math.isclose(x, y)  -> True se x e y estiverem perto um do outro
# Funciona bem para comparar dois números "normais".
# MAS atenção com o zero:
print(math.isclose(0.1 + 0.2, 0.3))          # True
print(math.isclose(1e-17, 0.0))              # False (!!) mede a distância RELATIVA
print(math.isclose(1e-17, 0.0, abs_tol=1e-9))  # True -> ao pé do zero é preciso isto
# Para comparar com zero, o abs(v) < EPSILON é o mais simples e seguro.






# %%
"""
===========================================================================
Guião 04b, exercícios 28, 29 - Funções reais e o módulo math [24 min -> 12:32]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) EXERCICIO 28 (GUIÃO) - Graus para radianos [6 min -> 12:14] -----------

    28 - Os ângulos são medidos usando uma de duas unidades de medida:
    graus ou radianos. Os graus dum círculo variam entre 0 e 360.
    Os radianos dum círculo variam entre 0 e 2*PI.
    Escreva um programa para converter graus em radianos.
    (Por favor, não use a função math.radians.)

        def degrees_to_radians(degrees: float) -> float:
            ''' Convert degrees (0..360) to radians (0..2*pi).
                Precondition: 0 <= degrees <= 360
            '''

    Regra de três simples:
        360 graus   ->  2 * pi radianos
        degrees     ->  ?
"""
import math

def degrees_to_radians(degrees: float) -> float:
    """ Convert degrees (0..360) to radians (0..2*pi).
        Precondition: 0 <= degrees <= 360
    """
    return degrees / 360 * 2 * math.pi
    # return degrees * math.pi / 180   # a mesma coisa, simplificada

print(degrees_to_radians(180))    # 3.141592653589793 (pi)
print(degrees_to_radians(90))     # 1.5707963267948966 (pi / 2)
print(math.radians(90))           # a função pronta dá o mesmo

# math.pi é uma CONSTANTE do módulo math: escreve-se sem parêntesis.
# math.sqrt(x) é uma FUNÇÃO: leva parêntesis e argumento.






# %%
# Exercício 28 (main, dado no guião)

def main() -> None:
    degrees = float(input("Introduza graus: "))
    if not (0 <= degrees <= 360):
        print("Argumento inválido")
    else:
        radians = degrees_to_radians(degrees)
        print(f"{radians} radianos")

main()

# Reparem no "0 <= degrees <= 360": comparações encadeadas, como na matemática.
# É o mesmo que "0 <= degrees and degrees <= 360".






# %%
"""
b) EXERCICIO 29 (GUIÃO, Mooshak K) - log16, parte decimal, centésimas [13 min -> 12:27]

    29 - Escreva um programa que, dado um real positivo, calcule e mostre,
    usando sempre 8 casas decimais, os três seguintes valores:
      - O seu logaritmo de base 16 (chamando o logaritmo da biblioteca com
        apenas um argumento, ou seja o logaritmo natural:
        log_b(x) = ln(x) / ln(b));
      - A sua parte decimal (com a ajuda da função math.floor)
        por exemplo: 34.5678 -> 0.5678;
      - O seu valor arredondado às centésimas (com a ajuda da função
        math.floor e sem usar a função round; não é trivial!)
        por exemplo: 34.5678 -> 34.5700.

    Exemplo de execução
        Introduza um real positivo: 1.2345678
        log16(1.23456780) = 0.07600152
        decimal_part(1.23456780) = 0.23456780
        round_hundredths(1.23456780) = 1.23000000

    Ferramentas (experimentem antes de resolver!):
"""
import math

print(math.log(math.e))     # 1.0  -> math.log com 1 argumento = logaritmo natural (ln)
print(math.floor(34.5678))  # 34   -> "chão": o maior inteiro <= x
print(math.floor(-3.7))     # -4   -> atenção nos negativos: o chão de -3.7 é -4
print(int(-3.7))            # -3   -> o int() corta, não arredonda para baixo

# Pistas:
#   parte decimal: o que sobra de x quando lhe tiramos o chão?
#   centésimas: e se "empurrarmos" a vírgula 2 casas para a direita,
#               arredondarmos às unidades, e voltarmos a pôr a vírgula no sítio?
#               34.5678 -> 3456.78 -> 3457 -> 34.57
#   arredondar às unidades só com floor: floor(x + 0.5)
#               3456.78 + 0.5 = 3457.28 -> floor -> 3457






# %%
# Solução do exercício 29 (funções)
import math

def log16(x: float) -> float:
    """ Logarithm in base 16.
        Precondition: x > 0
    """
    return math.log(x) / math.log(16)

def decimal_part(x: float) -> float:
    """ Decimal part of a real number.
        Precondition: x > 0
    """
    return x - math.floor(x)
    # return x - int(x)    # para x > 0 dá o mesmo (a precondição garante x > 0)
    # return x % 1         # também dá: o resto da divisão por 1

def round_hundredths(x: float) -> float:
    """ Round a real number to 2 decimal places.
        Precondition: x > 0
    """
    return math.floor(x * 100 + 0.5) / 100
    # return round(x, 2)   # proibido neste exercício (e ver a célula seguinte)

print(log16(16), log16(256), log16(4))   # 1.0 2.0 0.5
print(decimal_part(34.5678))             # 0.5677999999999983 (o lixo de sempre)
print(round_hundredths(34.5678))         # 34.57

# O decimal_part devolve 0.5677999999999983 e não 0.5678.
# Com :.8f arredonda para 0.56780000, por isso o output fica certo.
# O "lixo" só se vê quando pedimos mais casas do que as que interessam.






# %%
# Solução do exercício 29 (main, dado no guião)

def main() -> None:
    # ler
    x = float(input("Introduza um real positivo: "))
    if not (x > 0):
        print("Argumento inválido")
    else:
      # calcular
        log_result = log16(x)
        decimal_result = decimal_part(x)
        rounded_result = round_hundredths(x)
      # escrever
        print(f"log16({x:.8f}) = {log_result:.8f}")
        print(f"decimal_part({x:.8f}) = {decimal_result:.8f}")
        print(f"round_hundredths({x:.8f}) = {rounded_result:.8f}")

main()

# Ler, calcular, escrever: 3 fases separadas. Fica muito fácil de ler.






# %%
"""
c) Arredondar não é tão simples como parece [5 min -> 12:32] ---------------
"""

print(round(0.5), round(1.5), round(2.5), round(3.5))   # 0 2 2 4  (!!)

# O round do Python, nos empates exatos (.5), escolhe o número PAR.
# Chama-se "arredondamento bancário" (em inglês: round half to even).
# Porquê? Se arredondássemos sempre para cima, em milhões de contas
# (bancos, estatística) os erros iam todos para o mesmo lado e somavam.
# Assim metade vai para cima e metade para baixo: os erros anulam-se.
#
# As notas da escola usam outra regra: .5 vai sempre para cima.
# Para isso: math.floor(nota + 0.5) -> exatamente o truque do ex 29!

print(round(2.675, 2))           # 2.67  (!!) esperavam 2.68?
print(f"{2.675:.20f}")           # 2.67499999999999982236 -> o 2.675 guardado é "quase" 2.675
print(round_hundredths(2.675))   # 2.68

# Aqui nem é a regra do par: o 2.675 guardado já está ABAIXO de 2.675.
# Moral: com floats, ".5 exato" é coisa rara. Não contem com ele.






# %%
"""
===========================================================================
Guião 04b, exercícios 30, 31 - Séries e limites [23 min -> 12:55]
https://ipce-184ea7.gitlab.io/
===========================================================================

a) EXERCICIO 30 (GUIÃO) - O seno pela série de Taylor [14 min -> 12:46] ----

    30 - O seno pode ser escrito como uma soma infinita (série de Taylor):

        sin(x) = x - x³/3! + x⁵/5! - x⁷/7! + x⁹/9! - ...

    Escreva um programa para calcular uma aproximação da função sin num
    ponto x, usando os primeiros n termos da série de Taylor.
    O programa pede os valores de x e n, e escreve dois valores: o valor
    "exato" de sin(x) e o valor aproximado calculado com base na série.

    Pedido especial: não recalcular tudo do zero em cada termo
    (dá para calcular 7! a partir de 5!, e x⁷ a partir de x⁵).
    Se não conseguirem, façam primeiro da maneira simples.

        def taylor_sin(x: float, n: int) -> float:
            ''' Taylor series aproximation of the sin function.
                Precondition: n > 0
            '''

    Olhem para o termo número i (a contar do 0):
        i = 0:  + x¹ / 1!
        i = 1:  - x³ / 3!
        i = 2:  + x⁵ / 5!
    Expoente = 2*i + 1.  Sinal = (-1) elevado a i.
"""






# %%
# Solução do exercício 30 - versão 1, a mais direta (faz o pedido "especial"? não)
import math

def taylor_sin_v1(x: float, n: int) -> float:
    """ Taylor series aproximation of the sin function.
        Precondition: n > 0
    """
    total = 0.0
    for i in range(n):
        total += (-1) ** i * x ** (2 * i + 1) / math.factorial(2 * i + 1)
    return total

print(taylor_sin_v1(1.0, 5), math.sin(1.0))

# Funciona! Mas em cada volta calcula a potência e o fatorial DO ZERO.
# O fatorial de 19 recalcula o 1*2*3*...*17 que o termo anterior já tinha feito.






# %%
# Solução do exercício 30 - versão 2, reaproveitar o termo anterior
import math

def taylor_sin(x: float, n: int) -> float:
    """ Taylor series aproximation of the sin function.
        Precondition: n > 0
    """
    total = 0.0
    term = x                                            # o 1º termo: x / 1!
    for i in range(n):
        total += term
        term *= -x * x / ((2 * i + 2) * (2 * i + 3))    # termo seguinte
    return total

print(taylor_sin(1.0, 5), math.sin(1.0))

# De onde vem o "(2i+2) * (2i+3)"? Do termo i para o termo i+1:
#     x⁵/5!  ->  x⁷/7!
#     multiplicar por x² e dividir por 6 * 7 (porque 7! = 5! * 6 * 7)
#     com i = 2: 2i+2 = 6 e 2i+3 = 7. Confere!
# E o "-" troca o sinal em cada volta (tal como o sign = -sign da série ln(2)).
#
# É o mesmo truque da série de Zeno da aula passada (term /= 2):
# "o próximo termo calcula-se a partir do anterior".
# Isto aparece MUITO em testes com séries.






# %%
# Exercício 30 (main, dado no guião)
import math

def main() -> None:
  # ler
    x = float(input("X: "))
    n = int(input("N: "))
    if not (n > 0):
        print("Argumento inválido")
    else:
      # calcular
        sin_x = math.sin(x)
        taylor_x = taylor_sin(x, n)
      # escrever
        print(f"   sin({x}) = {sin_x:.15f}")
        print(f"taylor({x}) = {taylor_x:.15f}")

main()

# Experimentem: X = 1 com N = 1, 2, 3, 5, 10. Quantas casas ficam certas?
# Depois X = 10 com N = 5 e com N = 20. O que aconteceu?
#
#   x = 10, n = 5   -> taylor dá 1448.27... e o seno é -0.544 !!!
#   x = 10, n = 20  -> taylor dá -0.54402111...  (já bate certo)
#
# Longe do zero, os primeiros termos são ENORMES (10⁹/9! ≈ 2756)
# e só se cancelam uns aos outros quando há termos suficientes.
# A aproximação de Taylor é boa perto do 0. Longe do 0 precisa de muitos termos.






# %%
"""
b) EXERCICIO 31 (GUIÃO, Mooshak L) - sin(h)/h quando h vai para 0 [9 min -> 12:55]

    31 - O quociente sin(h)/h converge para 1 quando h converge para 0.
    O objetivo é gerar dados experimentais que confirmem esse facto.

    Escreva um programa para tabelar valores da função f(h) = sin(h)/h.
    O programa pede o valor inicial de h (real) e o número de linhas da
    tabela (inteiro positivo). A tabela divide sempre por 2 o valor de h.

        def f(x: float) -> float:
        def print_table_f(h: float, n: int) -> None:
            ''' Print n values of f with the argument h converging to zero.
                Precondition: h != 0 and n >= 0
            '''
            print("    h      f(h)")    # prints the header of the table
            ...

    Exemplo de execução
        H: 3
        N: 10
            h      f(h)
        3.000000 0.047040
        1.500000 0.664997
        ...
        0.005859 0.999994

    Pista: o parâmetro h é uma variável como as outras.
    Pode ser alterado dentro do ciclo: h /= 2 no fim de cada volta.
"""
import math

def f(x: float) -> float:
    """ The function sin(x)/x.
        Precondition: x != 0
    """
    return math.sin(x) / x

def print_table_f(h: float, n: int) -> None:
    """ Print n values of f with the argument h converging to zero.
        Precondition: h != 0 and n >= 0
    """
    print("    h      f(h)")    # prints the header of the table
    for _ in range(n):          # o "_": só queremos repetir n vezes, não usamos o número
        print(f"{h:.6f} {f(h):.6f}")
        h /= 2

def main() -> None:
    h = float(input("H: "))
    n = int(input("N: "))
    if not (h != 0 and n >= 0):
        print("Argumentos inválidos")
    else:
        print_table_f(h, n)

main()

# Aqui o h /= 2 é "acumular" por divisão. Tal como o term /= 2 do Zeno.
# Mudar o h dentro da função NÃO muda nada no main:
# o h da função é uma variável local (vimos isto na aula passada).
#
# Porque é que h != 0 está na precondição? f(0) seria sin(0)/0 -> ZeroDivisionError.
# Mas o h nunca chega a 0 ao dividir por 2... pois não?
# (com floats, ao fim de 1077 divisões o 3.0 fica tão pequeno que vira 0.0 mesmo!
#  os float têm limites: menos de ~1e-308 já é "quase impossível" de guardar)
#
# Isto que acabaram de fazer é Matemática Experimental:
# não provámos o limite, mas vimos os números a aproximarem-se de 1.






# %%
"""
===========================================================================
FECHO [5 min -> 13:00]
===========================================================================

    O que ficou de hoje:
        1. and / or / not: tabelas da verdade, De Morgan, e a avaliação
           "preguiçosa" (a esquerda pode proteger a direita).
        2. Não existe range de reais: faz-se o for com um int
           e CALCULA-SE o x (x = inicio + i * passo).
        3. Floats são aproximados (0.1 + 0.2 != 0.3).
           Nunca comparar reais com ==. Usar abs(a - b) < EPSILON.
        4. Séries: o próximo termo calcula-se a partir do anterior.

    Para casa:
        - Mooshak: I (26), J (27), K (29), L (31)
        - Exercícios de testes anteriores logo a seguir (com solução)
"""






# %%
"""
===========================================================================
CONSOLIDAR - exercícios de testes anteriores
(se sobrar tempo na aula; senão, ficam para casa - todos com solução)
===========================================================================

    Tentem primeiro sem olhar para a solução:
    é exatamente este o nível do teste.
"""






# %%
"""
PREVER O OUTPUT - booleanos (estilo pergunta 1 dos testes) -----------------

    Sem correr: o que escreve cada linha? (True, False, ou erro?)
"""

a, b = 5, 0
print(a > 3 and b > 3)                  # ?
print(a > 3 or b / 0 > 1)               # ?
print(not a > 3 or b == 0)              # ?
print(not (a > 3 or b == 0))            # ?
print(b != 0 and a / b > 1)             # ?
print(3 < a < 10)                       # ?

# Soluções:
#   False  -> 5 > 3 é True, 0 > 3 é False -> and dá False
#   True   -> a esquerda já é True -> o or nem calcula o b / 0 (senão rebentava!)
#   True   -> lê-se (not a > 3) or (b == 0) = False or True
#   False  -> not (True or True) = not True
#   False  -> b != 0 é False -> o and pára ali, o a / b nunca é feito
#   True   -> 3 < 5 and 5 < 10






# %%
"""
EXERCICIO (TESTE 1 2024/25, pergunta 2, 4 valores) - Período do Natal ------

    Escreva uma função booleana para testar se uma data se situa no período
    do Natal. Vamos convencionar que este período se inicia às zero horas
    de 23/Dez e termina às 24 horas de 1/Jan. Exemplos:
        christmas(23, 12) == True      christmas(1, 1) == True
        christmas(22, 12) == False     christmas(10, 2) == False

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def christmas(day: int, month: int) -> bool:
            ''' Check if it is a Christmas date. '''
"""

def christmas(day: int, month: int) -> bool:
    """ Check if it is a Christmas date. """
    return (month == 12 and day >= 23) or (month == 1 and day == 1)

print(christmas(23, 12), christmas(1, 1), christmas(22, 12), christmas(10, 2))
# True True False False

# Versão "comprida" que perde pontos por não ser simples:
#   if month == 12 and day >= 23:
#       return True
#   elif month == 1 and day == 1:
#       return True
#   else:
#       return False
# A condição JÁ É o resultado. Devolve-se diretamente.
#
# Não é preciso "day <= 31": assume-se que a data é válida.
# Os parêntesis nem são obrigatórios (o and "cola" mais que o or),
# mas deixam a intenção clara. Na dúvida, parêntesis.






# %%
"""
EXERCICIO (TESTE 1 2025/26, pergunta 6, 4 valores) - Celsius / Fahrenheit --

    A equação F = 1.8 * C + 32 converte graus Celsius para Fahrenheit.
    A equação de conversão em sentido contrário não é difícil de deduzir.

    Escreva um programa completo que escreva tabelas de conversão entre
    graus Celsius e graus Fahrenheit, nos dois sentidos.
    O programa pergunta qual o sentido da conversão pretendida:
      - a resposta 'c' indica a conversão Celsius -> Fahrenheit;
      - a resposta 'f' indica a conversão Fahrenheit -> Celsius;
      - qualquer outra resposta fará o programa não escrever qualquer tabela.
    Depois pergunta qual o valor inicial da tabela.
    Finalmente, escreve uma tabela com 10 correspondências.
    O incremento é sempre 1 grau.

    Exemplo de execução (imitar este formato de forma rigorosa):
        Conversão: c->f ou f->c? f
        Valor de partida? 35.7
        Fahrenheit ===> Celsius
        35.700000 ===> 2.055556
        36.700000 ===> 2.611111
        ...
        44.700000 ===> 7.055556

    Escreva um programa bem organizado, constituído por várias funções.
    Inclua um pequeno comentário inicial e ainda uma precondição, se necessária.

    É o ex 26 + ex 31 com outra roupa: um for com int e um x calculado.
"""

LINES = 10          # número de linhas da tabela

def celsius_to_fahrenheit(c: float) -> float:
    """ Convert Celsius to Fahrenheit. """
    return 1.8 * c + 32

def fahrenheit_to_celsius(f: float) -> float:
    """ Convert Fahrenheit to Celsius. """
    return (f - 32) / 1.8

def print_table_c_to_f(start: float) -> None:
    """ Print a conversion table Celsius -> Fahrenheit. """
    print("Celsius ===> Fahrenheit")
    for i in range(LINES):
        c = start + i
        print(f"{c:.6f} ===> {celsius_to_fahrenheit(c):.6f}")

def print_table_f_to_c(start: float) -> None:
    """ Print a conversion table Fahrenheit -> Celsius. """
    print("Fahrenheit ===> Celsius")
    for i in range(LINES):
        f = start + i
        print(f"{f:.6f} ===> {fahrenheit_to_celsius(f):.6f}")

def main() -> None:
    direction = input("Conversão: c->f ou f->c? ")
    start = float(input("Valor de partida? "))
    if direction == 'c':
        print_table_c_to_f(start)
    elif direction == 'f':
        print_table_f_to_c(start)

main()

# Dedução da inversa: F = 1.8 * C + 32  ->  F - 32 = 1.8 * C  ->  C = (F - 32) / 1.8
# "Qualquer outra resposta": não há else -> o programa simplesmente não escreve nada.
# (o enunciado pergunta o valor de partida SEMPRE, mesmo com resposta inválida)
#
# Reparem na variável local "f" em print_table_f_to_c:
# "tapa" a função f do ex 31 só lá dentro. Funciona, mas não é elegante.
# Num programa a sério, o ex 31 e este estariam em ficheiros diferentes.






# %%
"""
EXERCICIO (TESTE 1 2024/25, pergunta 6, 4 valores) - Conta da eletricidade -

    Escreva um programa completo que calcule a conta de eletricidade dum
    cliente. O preço de cada unidade depende do consumo total:

        Consumo          Custo
        [0, 200[         0.12 €/unidade
        [200, 400[       0.15 €/unidade
        [400, 600[       0.18 €/unidade
        [600, inf[       0.20 €/unidade

    Por exemplo, 300 unidades custam 300*0.15€.
    Há mais duas regras:
      - Se o custo das unidades for superior a 45.0€, adiciona-se uma
        sobretaxa de 15% à conta final.
      - A conta final mínima é de 10.0€.

    Exemplos de execução:
        Qual o consumo: 800        Qual o consumo: 300        Qual o consumo: 50
        Custo = 160.0              Custo = 45.0               Custo = 6.0
        Sobretaxa = 24.0           Sobretaxa = 0.0            Sobretaxa = 0.0
        Total = 184.0              Total = 45.0               Total = 10.0

    Recomenda-se: uma função para o custo, outra para a sobretaxa,
    outra para o total, e a main. Com comentário e precondição.
"""

SURCHARGE_LIMIT = 45.0      # acima disto paga sobretaxa
SURCHARGE_RATE = 0.15       # 15%
MINIMUM_BILL = 10.0         # conta mínima

def cost(units: int) -> float:
    """ Cost of the consumed units.
        Precondition: units >= 0
    """
    if units < 200:
        return units * 0.12
    elif units < 400:
        return units * 0.15
    elif units < 600:
        return units * 0.18
    else:
        return units * 0.20

def surcharge(c: float) -> float:
    """ Surcharge for a given cost.
        Precondition: c >= 0
    """
    if c > SURCHARGE_LIMIT:
        return c * SURCHARGE_RATE
    return 0.0

def total(c: float) -> float:
    """ Final bill for a given cost (never below the minimum).
        Precondition: c >= 0
    """
    t = c + surcharge(c)
    if t < MINIMUM_BILL:
        return MINIMUM_BILL
    return t

def main() -> None:
    units = int(input("Qual o consumo: "))
    c = cost(units)
    print(f"Custo = {c}")
    print(f"Sobretaxa = {surcharge(c)}")
    print(f"Total = {total(c)}")

main()

# Nos elif não é preciso "200 <= units < 400":
# se chegámos ao 1º elif, já sabemos que units >= 200 (o if falhou).
# A ORDEM dos ramos faz metade do trabalho.
#
# 300 unidades: 300 * 0.15 = 45.0, que NÃO é superior a 45.0 -> sem sobretaxa.
# ("superior" é >, não >=. Ler o enunciado com lupa!)
#
# Ainda dá 45.0 certinho? Neste caso sim, confirmem com print(300 * 0.15).
# Se desse 45.00000000000001, o > mudava o resultado... floats outra vez.
#
# Três constantes com nome: sem valores mágicos. Se a sobretaxa mudar,
# muda-se numa linha só.