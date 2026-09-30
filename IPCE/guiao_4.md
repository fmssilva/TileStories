
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Lista de exercícios
Artur Miguel Dias
Prática 04a (Semana de 05-09/out/2026)
Sumário: Exercícios de 25 a 27.

25 - Problema para ser resolvido no quadro, com o docente a orientar a discussão.
Se a linguagem Python não tivesse os operadores lógicos and, or e not, que funções booleanas escreveria para os substituir. Complete o código abaixo da forma mais compacta possível. Não pode usar os três operadores lógicos, mas pode usar o if.

def and_(a: bool, b: bool) -> bool:
    """ AND - Boolean operation. """
    ...

def or_(a: bool, b: bool) -> bool:
    """ OR - Boolean operation. """
    ...

def not_(a: bool) -> bool:
    """ NOT - Boolean operation. """
    ...
I : 26 - Escreva um programa para tabelar o polinómio quadrático ax2+bx+c num dado intervalo [limInf, limSup] para um determinado número de pontos numPontos (superior a 1). O programa pede ao utilizador os valores reais a, b, c, limInf, limSup e o valor inteiro numPontos.
A tabela produzida deve ter o seguinte aspeto (se a=0.0, b=1.0, c=0.0, limInf=0.0, limSup=1.0, numPontos=5):

0.000000 0.000000
0.250000 0.250000
0.500000 0.500000
0.750000 0.750000
1.000000 1.000000
Para escrever um número real r com 6 casas decimais, faça assim: print(f"{r:.6f}").

Organização do programa: (1) uma função real para avaliar um polinómio quadrático num ponto; (2) uma função sem resultado mas com 6 parâmetros para calcular e escrever a tabela; (3) função main que pede os dados e manda escrever a tabela.

Nota: O Python só tem ranges de inteiros. Não tem ranges de reais. Isso limita um pouco as opções sobre a forma de resolver este problema. O ciclo for vai ter de usar uma variável inteira.

Complete o programa:

def quadratic_polynomial_value(a: float, b: float, c: float, x: float) -> float:
    """ Eval a quadratic polynomial for the given value x """
    return ...

def print_table(a: float, b: float, c: float,
                lower_bound: float, upper_bound: float,
                n: int) -> None:                # no result
    """ Print table of values for a quadratic polynomial in a interval.
        Precondition: lower_bound < upper_bound and n > 1
    """
    ...

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    lower_bound = float(input("LOWER: "))
    upper_bound = float(input("UPPER: "))
    n_points = int(input("N: "))
    if not (lower_bound < upper_bound and n > 1):
        print("Valores inválidos")
    else
        print_table(a, b, c, lower_bound, upper_bound, n_points)

main()
Nota: Como escrever cada linha da tabela usando sempre 6 casas decimais? Isto ainda não foi ensinado nas aulas teóricas. Se as variáveis a escrever tiverem o nome x e y, faz-se assim: print(f"{x:.6f} {y:.6f}").
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
J : 27 - Escreva um programa para achar as raízes da equação quadrática ax2+bx+c=0 (a ≠ 0).
O programa pede ao utilizador os valores reais a, b, c e responde dizendo qual o número de raízes (0, 1 ou 2) e quais os valores dessas raízes. No caso de haver 2 raízes, elas devem ser escritas por ordem crescente.

Relembre a fórmula resolvente da equação quadrática: aqui.

Escreva um programa bem organizado, com várias funções:

uma função para a fórmula do discriminante;
outra função que determina quantas são as raízes;
outra função que calcula a raiz da esquerda;
outra função que calcula a raiz da direita;
a função main.
Oferecemos-lhe a função main:

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
Exemplo de execução
A: 1.0
B: -4.0
C: 4.0
1 2.0






Introdução à Programação para a Ciência e Engenharia (2026/2027)
Lista de exercícios
Artur Miguel Dias
Prática 04b (Semana de 05-09/out/2026)
Sumário: Números reais e funções reais. Exercícios de 28 a 31.

28 - Os ângulos são medidos usando uma de duas unidades de medida: graus ou radianos. Os graus dum círculo variam entre 0 e 360. Os radianos dum círculo variam entre 0 e 2*PI.
Escreva um programa para converter graus em radianos. O programa deve aceitar um número real entre 0 e 360 e produzir um número real entre 0 e 2*PI. (Por favor, não use a função math.radians, para podermos treinar um raciocínio matemático simples.)

Complete o programa:

import math

def degrees_to_radians(degrees: float) -> float:
    """ Convert degrees (0..360) to radians (0..2*pi).
        Precondition: 0 <= degrees <= 360
    """
    ....

def main() -> None:
    degrees = float(input("Introduza graus: "))
    if not (0 <= degrees <= 360):
        print("Argumento inválido")
    else:
        radians = degrees_to_radians(degrees)
        print(f"{radians} radianos")

main()
K : 29 - Consulte aqui quais as operações disponíveis para números reais no módulo math. Escreva um programa que, dado um real positivo, calcule e mostre, usando sempre 8 casas decimais, os três seguintes valores:
O seu logaritmo de base 16 (por favor, chamando o logaritmo da biblioteca com apenas um argumento, ou seja o logaritmo natural - isto é um pretexto para usar logb(x) = ln(x)/ln(b).)
A sua parte decimal (por favor, com a ajuda da função math.floor) - por exemplo: 34.5678 → 0.5678;
O seu valor arredondado às centésimas (por favor, com a ajuda da função math.floor e sem usar a função round; não é trivial!) - por exemplo: 34.5678 → 34.5700.

Complete o programa:

import math

def log16(x: float) -> float:
    """ Precondition: x > 0 """
    ...

def decimal_part(x: float) -> float:
    """ Precondition: x > 0 """
    ...

def round_hundredths(x: float) -> float:
    """ Precondition: x > 0 """
    ...

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
Exemplo de execução
Introduza um real positivo: 1.2345678
log16(1.23456780) = 0.07600152
decimal_part(1.23456780) = 0.23456780
round_hundredths(1.23456780) = 1.23000000

30 - Eis o desenvolvimento da função sin em série de Taylor:

Escreva um programa para calcular uma aproximação da função sin num ponto x, usando os primeiros n termos da série de Taylor.
O programa pede os valores de x e n, e escreve dois valores: o valor "exato" de sin(x) e o valor aproximado calculado com base na série de Taylor.

Agora um pedido diferente do habitual. Cada termo da série podia ser calculado individualmente, mas repare que isso implica muita repetição nos cálculos. Nós conseguimos, por exemplo, calcular o valor de 7! a partir de 5!, e o valor de x7 a partir de x5. Tente usar esta ideia. Se não estiver a ver como implementar a ideia, então programe da maneira menos eficiente, porque o mais importante é ter uma solução.

Nota: Depois de escrever o programa, se os testes derem resultados estranhos, saiba que a aproximação de Taylor só é adequada num determinado intervalo centrado em zero.

Complete o programa:

import math

def taylor_sin(x: float, n: int) -> float:
    """ Taylor series aproximation of the sin function.
        Precondition: n > 0
    """
    ...

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
L : 31 - Eis uma conhecida propriedade da função seno: O quociente sin(h)/h converge para 1 quando h converge para 0. Ou seja: .
O objetivo deste problema é gerar dados experimentais que confirmem esse facto.

Escreva um programa para tabelar valores da função f(h) = sin(h)/h.

O programa pede o valor inicial de h (um valor real) e o número de linhas da tabela a produzir (um inteiro positivo). Seguidamente produz a tabela, dividindo sempre por 2 o valor de h, por forma a fazer h convergir para 0.

Veja o exemplo de execução, mais abaixo.

Para o seu programa ficar bem organizado, deve definir uma função que escreva a tabela. Esta função não retorna nenhum valor. A missão dela é executar ações de escrita; não calcular um resultado.

Complete o programa:

import math

def f(x: float) -> float:
    """ Some function. """
    ...

def print_table_f(h: float, n: int) -> None:
    """ Print n values of f with the argument h converging to zero.
        Precondition: h != 0 and n >= 0
    """
    print("    h      f(h)")    # prints the header of the table
    ...

def main() -> None:
    ...    # por favor, valide os argumentos em conformidade com a precondição.

main()
Sugestão: no ciclo for você pode ir sucessivamente dividindo o h por 2.
Exemplo de execução
H: 3
N: 10
    h      f(h)
3.000000 0.047040
1.500000 0.664997
0.750000 0.908852
0.375000 0.976727
0.187500 0.994151
0.093750 0.998536
0.046875 0.999634
0.023438 0.999908
0.011719 0.999977
0.005859 0.999994
