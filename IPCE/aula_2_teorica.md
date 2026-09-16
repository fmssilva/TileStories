
Introdução à Programação para a Ciência e Engenharia (2025/2026)
Aulas teóricas
Artur Miguel Dias
Teórica 02 (15/set/2025)
Antevisão da programação em Python através da análise de vários miniprogramas usando: funções, tipos int e float, input e output, instrução condicional if, variáveis, biblioteca matemática, operadores de comparação e lógicos, recursividade, ciclo for.

Prólogo
Nesta aula apresentam-se alguns miniprogramas desenvolvidos usando uma parte limitada da linguagem Python.
Na próximas aulas teóricas, todos os mecanismos aqui usados serão esclarecidos devidamente. Mas, neste primeiro contacto, já deverá ser possível começar a entender diversas ideias importantes.

As primeiras aulas práticas de IPCE envolverão a escrita de pequenos programas em Python, e a programação será feita por imitação dos exemplos desta aula.

Programa das boas-vindas
Enunciado do problema
Desenvolva um programa que escreva simplesmente Bem-vindos!.
Solução
def main() -> None:
    """ Say Welcome!  (this is a commentary) """
    print("Bem-vindos!")

main()
Execução
Bem-vindos!
Explicações
Os nossos programas em Python consistirão numa sequência de unidades chamadas funções. Este primeiro programa contém apenas uma função chamada main.
Nos nossos programas existirá sempre uma função main e na última linha do programa existirá sempre uma invocação dessa função, para indicar que o o programa começa a correr na função main.
O conceito de função tem em Python um sentido técnico específico, próximo do sentido que tem na Matemática, mas não exatamente igual. Esta questão será discutida em aulas posteriores.
Olhando para o cabeçalho da função, neste caso vê-se que ela não tem parâmetros nem resultado (perceberá isso comparando com os exemplos mais abaixo).
No corpo da função, neste caso ocorre apenas uma instrução, que consiste numa chamada da função predefinida print para escrever uma string particular (chamamos "string" a uma cadeia de carateres, ou seja um pedaço de texto).
O cabeçalho duma função tem de começar no início duma linha (sem espaços atrás), mas o corpo duma função tem de ficar indentado, normalmente usando 4 espaços.
Um programa com duas funções
Enunciado do problema
Desenvolva um programa que calcule o cubo dum valor inteiro.
Solução
def cube(x: int) -> int:
    """ Cube of integer value. """
    return x * x * x

def main() -> None:
    i = int(input("Introduza um valor inteiro: "))
    print(f"O cubo de {i} é {cube(i)}.")

main()
Execução
Introduza um valor inteiro: 5
O cubo de 5 é 125.
Explicações
Quem escreve um programa tem a responsabilidade de escrever código claro, fácil de entender por outras pessoas. Os nossos programas em Python serão constituído por várias funções. Cada função deve desempenhar uma tarefa bem definida, deve ter um nome bem escolhido que sugira a sua tarefa, e não deve haver mais nenhuma função que se intrometa nessa tarefa.
A função cube tem um parâmetro inteiro e produz um resultado inteiro (veja o cabeçalho da função). A função calcula e retorna o cubo dum número, o que resolve o o problema proposto. O uso da palavra return é essencial, porque sem usar return, uma função não produz resultado nenhum.
A função main trata da interação com o utilizador: pede os dados, escreve o resultado. Para saber o que escrever, ela recorre a quem sabe resolver o problema ou seja à função cube. A função main chama a função cube usando como argumento o valor lido.
Uma novidade! Dentro da função main definimos uma variável inteira, chamada i. Uma variável tem um valor associado e neste caso queremos associar à variável o valor inteiro introduzido pelo utilizador. Para associar um valor à variável i, usámos o operador de atribuição, que se escreve = (cuidado, não se trata da igualdade matemática).
A leitura dos dados é feita pela função predefinida input, que obtém a linha de texto introduzida pelo utilizador. A função int converte essa string para um valor inteiro.
Outra novidade! A string usada na função print é precedida pelo caráter f, que indica formatação. Numa string de formatação podem ocorrer expressões entre chavetas: as expressões são avaliadas e os resultados inseridos na string. Para perceber bem este mecanismo, quando correr o programa, examine a relação entre a string de formatação e o output produzido.
Num programa bem organizado, para evitar uma grande confusão, há dois aspetos que precisam de tratados separadamente: (1) interação com o utilizador e (2) lógica da resolução dos problemas. A função main é especializada na interação com o utilizador (e poderão existir funções de interação adicionais se a interação for complexa). As restantes funções são especializadas na lógica da resolução dos problemas.
Um programa que usa números reais e a instrução condicional if
Enunciado do problema
Desenvolva um programa que calcule o valor absoluto dum valor real.
Solução
def absolute(x: float) -> float:
    """ Absolute value of float. """
    if x >= 0:
        return x
    else:
        return -x

def main() -> None:
    r = float(input("Introduza um valor real: "))
    print(f"O valor absoluto de {r} é {absolute(r)}.")

main()
Execução
Introduza um valor real: -45.79
O valor absoluto de -45.79 é 45.79.
Explicações
A função absolute tem um parâmetro real e produz um resultado real. Em Python, o nome float designa o tipo dos valores reais. Tal como na Matemática, na linguagem Python existe distinção entre números inteiros e números reais. Em cada exercício, devemos escolher o tipo de números mais adequado, mas por vezes o enunciado já diz qual o tipo de números a usar.
Num programa, é possível poder tomar decisões em função do estado corrente do programa. Para isso usa-se a instrução condicional if. Uma instrução if envolve: uma condição e dois ramos alternativos. Só um dos ramos será executado, consoante a condição seja verdadeira ou falsa.
Há uma terceira pequena novidade neste programa: o uso da função float para converter o input do utilizador num número real.
Um programa que contém uma função com dois parâmetros
Enunciado do problema
Desenvolva um programa que calcule o comprimento da hipotenusa de um triângulo retângulo a partir dos comprimentos dos respetivos catetos.
Solução
import math

def hypotenuse(cathetus1: float, cathetus2: float) -> float:
    """ Length of the hypotenuse of a right triangle.
        Precondition: cathetus1 > 0 and cathetus2 > 0
    """
    return math.sqrt(cathetus1 ** 2 + cathetus2 ** 2)

def main() -> None:
    cat1 = float(input("Primeiro cateto: "))
    cat2 = float(input("Segundo cateto: "))
    if not cat1 > 0 and cat2 > 0:
        print("Argumentos inválidos")
    else:
        print(f"Hipotenusa({cat1},{cat2}) = {hypotenuse(cat1, cat2)}")

main()
Execução
Primeiro cateto: 1.0
Segundo cateto: 1.0
Hipotenusa(1.0,1.0) = 1.4142135623730951
Explicações
A função hypotenuse tem dois parâmetros reais e um resultado também real. Pela primeira vez, vemos uma função com dois parâmetros.
Dentro da função main definem-se duas variáveis, às quais ficam associados os valores reais introduzidos pelo utilizador. Usamos duas variáveis porque há dois valores desconhecidos a guardar.
A diretiva import, na primeira linha permite que este programa use os serviços do módulo de biblioteca math. Por uma questão de organização da linguagem, há diversos serviços que não fazem parte do núcleo do Python, mas estão guardados numa biblioteca de módulos. Só devemos importar um módulo se precisarmos do que ele oferece. Neste programa, importamos o módulo math por precisamos da função sqrt, que calcula a raiz quadrada dum número.
Um programa que usa operadores de comparação e operadores lógicos
Enunciado do problema
Desenvolva um programa que determine o número de dias dum ano dado.
Solução
def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if (year % 4 == 0 and year % 100 != 0) or year % 400 == 0:
        return 366
    else:
        return 365

def main() -> None:
    y = int(input("Introduza o ano: "))
    print(f"O ano {y} tem {year_length(y)} dias.")

main()
Execução
Introduza o ano: 2025
O ano 2025 tem 365 dias.
Explicações
Dentro da função year_length são usados os operadores de comparação: == (igualdade) e != (diferença).
Também são usados os operadores lógicos: and (conjunção) e or (disjunção).
O operador matemático % representa a operação inteira módulo, também conhecida por resto da divisão.
Quando o nome das nossas funções e variáveis tiver mais do que uma palavra, deve ser escrito assim, year_length, usando o caráter sublinhado como separador.
Um programa com uma função recursiva
Enunciado do problema
Desenvolva um programa que calcule o fatorial dum número inteiro não negativo. Use a seguinte definição recursiva, que conhecemos da Matemática:

Solução
def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    if n == 0:
        return 1
    else:
        return n * factorial(n - 1)

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not x > 0:
        print("Argumentos inválidos")
    else:
        print(f"fatorial({x}) = {factorial(x)}")

main()
Execução
Introduza um número natural: 10
fatorial(10) = 3628800
Explicações
A função factorial chama-se a ela própria. É uma técnica por vezes útil. O corpo desta função consiste na tradução direta para Python da fórmula matemática dada.
A função factorial é parcial, ou seja não está definida em todo o seu domínio Sabemos que esta função só está definida para valores não negativos. A maioria das funções que iremos programar serão totais, mas ocasionalmente aparecerá uma função parcial, como esta. A terceira linha da função é uma precondição. Neste exemplo, a precondição informa que a função presume que os parâmetros são valores não negativos e que a função não se responsabiliza pelo que aconteça nos outros casos. As precondições são uma parte muito importante das nossas funções.
Um programa que usa um ciclo
Enunciado do problema
Desenvolva um programa que calcule o fatorial dum número inteiro não negativo, através da multiplicação direta de inteiros consecutivos, usando a ideia do seguinte piatório:

Solução
def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1     # accumulator (this is also a commentary)
    for i in range(1,n+1,1):
        fact = fact * i
    return fact

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not x > 0:
        print("Argumentos inválidos")
    else:
        print(f"fatorial({x}) = {factorial(x)}")

main()
Execução
Introduza um número natural: 10
fatorial(10) = 3628800
Explicações
Os ciclos são uma das partes mais complicadas da programação. Este programa usa um ciclo for que faz a variável i variar de 1 até n. O corpo do ciclo é executado n vezes, neste caso. Cada execução do corpo dum ciclo chama-se uma iteração.
Usa-se aqui uma estratégia de acumulação. A variável fact é inicializada com o elemento neutro da multiplicação. Depois, ao longo das várias iterações do ciclo, os valores sucessivos de i vão sendo multiplicados a fact. No final, ficamos com o valor pretendido em fact.
Uma programa igual ao anterior, mas que escreve os sucessivos valores das variáveis para percebermos melhor como o ciclo funciona
Enunciado do problema
Desenvolva um programa que calcule o fatorial dum número inteiro não negativo, através da multiplicação direta de inteiros consecutivos. Durante os cálculos, o programa deve mostrar a evolução do valor das variáveis usadas nas contas.
Solução
def factorial(n: int) -> int:
    """ Factorial of a natural number (showing the steps.)
        Precondition: n >= 0
    """
    fact = 1     # accumulator
    for i in range(1,n+1,1):
        fact = fact * i
        print(f"{i:2d} -> {fact}")
    return fact

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not x > 0:
        print("Argumentos inválidos")
    else
        print(f"fatorial({x}) = {factorial(x)}")

main()
Execução
Introduza um número natural: 10
 1 -> 1
 2 -> 2
 3 -> 6
 4 -> 24
 5 -> 120
 6 -> 720
 7 -> 5040
 8 -> 40320
 9 -> 362880
10 -> 3628800
fatorial(10) = 3628800
Explicações
Foi só adicionado um print dentro do ciclo for.
#80