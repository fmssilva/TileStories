
Introdução à Programação para a Ciência e Engenharia (2025/2026)
Lista de exercícios
Artur Miguel Dias
Prática 02b (Semana de 16-22/set/2025)
Sumário: Primeiros programas com instruções condicionais. Exercícios de 16 a 18.

Em todos os exercícios desta aula, os programas têm de tomar decisões usando a instrução if. Uma função das primeiras aula teóricas foi copiada para aqui, para servir de inspiração:
Solução do problema do ano bissexto (versão com função booleana)
def is_leap_year(year: int) -> bool:  # novidade: bool é o tipo dos valores booleanos
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if is_leap_year(year):
        return 366
    else:
        return 365

def main() -> None:
    y = int(input("Introduza o ano: "))
    print(f"O ano {y} tem {year_length(y)} dias.")

main()
16 - Escreva um programa que permita achar o máximo entre dois valores inteiros.
A escolha do valor maior deve ser efetuada numa função com dois argumentos chamada maximum. Dentro desta função, use a instrução if do Python.

E : 17 - Sejam a,b,c, valores reais, supostamente comprimentos dos lados dum triângulo. Escreva uma função com três argumentos chamada triangle_kind, que receba os tamanhos a, b, c dos lados dum triângulo e produza:
0 se a,b,c não definirem um triângulo próprio;
1 se a,b,c definirem um triângulo equilátero;
2 se a,b,c definirem um triângulo isósceles;
3 se a,b,c definirem um triângulo escaleno.
Um triângulo próprio é um triângulo com área superior a zero! O comprimento de todos os lados tem de ser superior a zero e o comprimento de cada lado tem de ser estritamente inferior à soma do comprimento dos outros dois - só assim se garante que os lados definem um polígono fechado com área superior a zero.
Depois de escrita a função triangle_kind, integre-a num programa que peça ao utilizador os três valores e mostre o resultado no ecrã.

Oferecemos-lhe este código de partida. Complete-o.

def is_proper_triangle(a: float, b: float, c: float) -> bool:
    """ Check if the lengths a, b, c define a proper triangle. """
    return ...

def triangle_kind(a: float, b: float, c: float) -> int:
    """ The kind of a possible triangle.
        Parameters:
           a, b, c - the side lengths
        Result:
           0 if improper triangle
           1 if  equilateral
           2 if isosceles
           3 if scalene
    """
    if is_proper_triangle(a, b, c):
        ...
    else:
        return 0  # Invalid triangle

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    print(triangle_kind(a, b, c))

main()
Exemplo de execução
A: 0.5
B: 0.5
C: 0.5
1
Ajuda: Em Python um if complexo pode ter múltiplos ramos. Analise a forma abaixo. A parte do elif é novidade.
O else final só é executado se todas as condições forem falsas e seria mau estilo trocá-lo por um elif forçado, que negasse as condições anteriores.

if condição1:
    ....
elif condição2:
    ....
elif condição3:
    ....
else:
    ....
18 - A nota final não arredondada da disciplina IPCE é um número real. No caso dos alunos que fazem a cadeira por testes, essa nota é calculada com base nas seguintes notas parcelares, todas valores reais entre 0 e 20 valores:
1º Teste - 40% da nota final
2º Teste - 40% da nota final
Projeto - 20% da nota final
Para um aluno ser aprovado por avaliação contínua, há dois requisitos:
A nota do projeto tem de ser igual ou superior a 9.5 valores.
A média dos dois testes tem de ser igual ou superior a 9.5 valores.
Escreva um programa que leia as três notas parcelares dum aluno e que:
No caso do aluno ficar aprovado, mostra a nota final não arredondada;
No caso do aluno não ficar aprovado, escreve a palavra "REPROVADO".
Recomendações:

Para calcular a média dos dois testes, escreva uma função com dois argumentos reais e resultado real.
def average(grade1: float, grade2: float) -> float:
Para testar se o aluno foi aprovado, escreva uma função com três argumentos reais e resultado booleano.
def passed(test1: float, test2: float, proj: float) -> bool:
Para calcular a nota final não arredondada escreva uma função com três argumentos reais e resultado real. O resultado é calculado presumindo que o aluno foi aprovado.
def final_grade(test1: float, test2: float, proj: float) -> float:
    """ Precondition: passed(test1, test2, proj) """
Oferecemos-lhe a função main já escrita:
def main() -> None:
    t1 = float(input("T1: "))
    t2 = float(input("T2: "))
    pr = float(input("PR: "))
    if passed(t1, t2, pr):
        print(final_grade(t1, t2, pr))
    else:
        print("REPROVADO")
