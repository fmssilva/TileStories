
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 06a (21/out/2026)
A importância dos testes na programação

Matrizes em Python.

A importância dos testes na programação
Quando terminamos a escrita dum programa, muitas vezes ficamos com a ilusão de que o programa está finalizado e se comporta da maneira pretendida em todas as circunstâncias.
A verdade é que a primeira versão do nosso programa costuma ter defeitos (bugs), ou seja, exibe comportamentos errados para certos valores de entrada.

Seria bom que existisse um oráculo, conhecedor de todas as verdades do universo, a quem pudéssemos perguntar se o nosso programa está certo. Como não conhecemos tal oráculo, a única possibilidade é sermos desconfiados e começarmos a testar o programa com valores de entrada que tenham uma boa probabilidade de expor problemas.

Repare que criar bons testes é uma parte essencial da programação. Saber como criar bons testes é algo que se pode estudar, mas também é algo que se aprende com a experiência. Os testes ajudam a descobrir as falhas do programa. Se o programa passar todos os testes que conseguimos imaginar, então ganhamos confiança no programa.

Os testes a um programa devem sempre incluir:

Pelo menos um caso normal;
Todos os casos especiais que seja possível imaginar;
Todos os casos limite que seja possível imaginar, tanto relativamente aos dados de entrada, como relativamente aos dados de saída.
Exemplo: O problema dos azulejos
Enunciado
Desenvolva um programa que determine o número de azulejos necessários para cobrir uma superfície retangular que tem um dado comprimento e uma dada largura (dados em cm). Os azulejos são quadrados com 20 cm de lado e não podem ser partidos. O programa escreve o número de azulejos completos que cabem no retângulo e qual a área que fica por cobrir.
Solução
TILE_SIDE = 20    # cm
TILE_AREA = TILE_SIDE * TILE_SIDE

def how_many_full_tiles(length: int, width: int) -> int:
    """ How many full tiles fit in the rectangle? """
    return (length // TILE_SIDE) * (width // TILE_SIDE)

def uncovered_area(length: int, width: int, full_tiles: int) -> int:
    """ Uncovered area, where no full tile fits """
    total_area = length * width
    covered_area = full_tiles * TILE_AREA
    return total_area - covered_area

def main() -> None:
    # Input data
    length = int(input("Comprimento em cm da área a cobrir: "))
    width = int(input("Largura em cm da área a cobrir: "))
    
    # Calculate the number of complete tiles
    how_many = how_many_full_tiles(length, width)
    
    # Calculate the uncovered area
    uncovered = uncovered_area(length, width, how_many)
    
    # Output the results
    print(f"Azulejos = {how_many}, Área descoberta = {uncovered} cm2")

main()
Testando o programa
A seguinte tabela ilustra alguns testes a que o programa anterior deve ser submetido para começarmos a ganhar confiança nele.
A tabela inclui um caso normal, dois casos especiais e dois casos limite. Os dois valores da esquerda são os valores de entrada. Os dois valores da direita são os valores de saída.

Teste	Comprimento	Largura	 	Azulejos	Área descoberta
Caso normal	35	25	 	1	475
Pouca largura	10	20	 	0	200
Pouca altura	35	10	 	0	350
Comprimento e largura mínimos	0	0	 	0	0
Cobertura total	400	500	 	500	0
Exercício: Conceba testes para o problema do triângulo das aulas práticas (exercício 17).

Depuração dum programa
Se os testes nos levam a descobrir um erro no programa, então relemos o programa, concentrando-nos nas partes relacionadas com o erro:
Idealmente, descobrimos a origem do problema lendo o código e fazemos a correção.
Caso contrário, temos de colocar mensagens em zonas críticas do código para confirmar se o que está a acontecer dentro do programa corresponde realmente ao que pensamos.
Matrizes
Na computação científica com Python, o uso de matrizes é essencial para a viabilidade dos cálculos. Por isso é inevitável falarmos do tema das matrizes em Python. Imagine um programa que precise de lidar...
Com as notas dos alunos dos sete turnos práticos da cadeira de IPCE?
Com um tabuleiro de xadrez?
No primeiro caso, podemos pensar numa matriz (tabela) com tamanho 7 * 30.
No segundo caso, pensamos logo numa matriz com tamanho 8 * 8.

Em ambas as situações, estamos a considerar usar matrizes com duas dimensões.

Para representar uma matriz com 2 dimensões, em Python básico (sem recurso a uma biblioteca), usa-se uma lista de listas. Para representar uma matriz com 3 dimensões, em Python usa-se uma lista de listas de listas. Etc. Não há limite para o número de dimensões.

Para exemplificar, o seguinte literal mostra uma possível representação para um tabuleiro de xadrez:

chess_board = [
    ["R", "N", "B", "Q", "K", "B", "N", "R"],
    ["P", "P", "P", "P", "P", "P", "P", "P"],
    [".", ".", ".", ".", ".", ".", ".", "."],
    [".", ".", ".", ".", ".", ".", ".", "."],
    [".", ".", ".", ".", ".", ".", ".", "."],
    [".", ".", ".", ".", ".", ".", ".", "."],
    ["p", "p", "p", "p", "p", "p", "p", "p"],
    ["r", "n", "b", "q", "k", "b", "n", "r"]
]
Confirma-se que se trata duma lista de listas. É assim que vamos representar as nossas matrizes, como já foi dito.

Numa lista de listas, nada obriga a que as linhas tenham todas o mesmo tamanho. Mas, se estivermos a pensar numa matriz verdadeira, então interessa-nos que todas as linhas tenham todas o mesmo tamanho.

Programação com matrizes
Vamos programar duas funções sobre matrizes de inteiros.
Começamos por definir um tipo novo, o tipo das matrizes de inteiros:

type IMatrix = list[list[int]]
Contar o número de ocorrências dum valor
Aqui usamos a técnica de percorrer diretamente os elementos da matriz, sem recorrer a índices.
def count(m: IMatrix, v: int) -> int:
    """ Count the ocurrences of some integer value. """
    sum = 0
    for l in m:               # percorre as linhas da matriz 
        for x in l:           # percorre qualquer linha da matriz
            if x == v:
                sum += 1
    return sum

>>> m = [list(range(4))]*4
>>> print(m)
[[0, 1, 2, 3], [0, 1, 2, 3], [0, 1, 2, 3], [0, 1, 2, 3]]
>>> count(m, 3)
4
>>> count(m, 99)
0
Ler matriz de inteiros
Aqui consideramos que o tamanho m * n da matriz é conhecido. Pedimos o valor de cada posição da matriz, indicando os índices dessa posição ao utilizador.
def input_int_matrix(nr: int, nc: int) -> IMatrix:
    """ Input a sequence of integers to a matrix.
        Parameters:
           nr - number of rows
           nc - number of columns
        Precondition:
           nr > 0 and nc > 0
    """
    m = []
    for i in range(nr):
        l = []
        for j in range(nc):
            x = int(input(f"m[{i}][{j}] = "))
            l.append(x)
        m.append(l)
    return m

>>> m = input_int_matrix(2, 5)
m[0][0] = 1
m[0][1] = 2
m[0][2] = 3
m[0][3] = 4
m[0][4] = 5
m[1][0] = 6
m[1][1] = 7
m[1][2] = 8
m[1][3] = 9
m[1][4] = 10
>>> print(m)
[[1, 2, 3, 4, 5], [6, 7, 8, 9, 10]]
>>> 
Desenvolvimento dum conjunto de operações de apoio ao uso de matrizes
A linguagem Python de base não oferece operações de suporte de matrizes, ao contrário do que faz, por exemplo, para as listas.
Por isso, precisamos de desenvolver um conjunto de funções auxiliares de apoio à utilização de matrizes.

Antes de começar, convém observar que há uma diferença importante no processamento de matrizes, quando comparado com o processamento de listas:

Com as listas é frequente ir fazendo crescer as listas em função das nossas necessidades;
Com matrizes, pelo contrário, trabalhamos quase sempre com matrizes de dimensões fixas - em parte devido à natureza dos problemas que queremos resolver, mas também devido ao facto de, por exemplo, ao fazer crescer uma linha, a matriz deixa de ser matriz.
As operações
As operações básicas aqui desenvolvidas são orientadas para o tratamento de matrizes com dimensões fixas. Aplicam-se a matrizes genéricas (que registam valores de qualquer tipo).
create - cria uma matriz com as dimensões indicadas, toda preenchida com um dado valor, que é indicado.
is_valid - testa se uma lista de lista é uma matriz válida (i.e. todas as linhas têm o mesmo comprimento)
nrows - produz o número de linha duma matriz
is_empty - testa se a matriz é vazia (i.e. tem zero linha e zero colunas)
ncols - produz o número de colunas duma matriz
row - produz uma cópia duma linha duma matriz
col - produz uma cópia duma coluna duma matriz
input_int_matrix - lê uma matriz de inteiros
Vamos analisar e discutir o seguinte código. Há imensos detalhes interessantes para observar:
type GList = list                # lista genérica
type GMatrix = list[GList]       # matriz genérica

type IList = list[int]          # lista de inteiros
type IMatrix = list[IList]      # matriz de inteiros

def create(nr: int, nc: int, value: any) -> GMatrix:
    """ Create and initialize a matrix with nr rows and nc columns.
        Precondition: nr >= 0 and nc >= 0
    """
    m = [None]*nr         # cria espaço para as linhas
    for i in range(nr):   # preenche as várias linhas
        m[i] = [value]*nc
    return m

def is_valid(m: GMatrix) -> bool:
    """ Check if all the lines have the same length. """
    nr = len(m)
    for i in range(nr-1):  # verifica se todas as linhas têm igual comprimento
        if len(m[i]) != len(m[i+1]):
            return False
    return True

def nrows(m: GMatrix) -> int:
    """ Number of rows of the matrix.
        Precondition: is_valid(m)
    """
    return len(m)             # número de linhas

def is_empty(m: GMatrix) -> int:
    """ Is the matrix empty? """
    return nrows(m) == 0       # testa se matriz vazia 

def ncols(m: GMatrix) -> int:
    """ Number of columns of the matrix.
        Precondition: is_valid(m)
    """
    if is_empty(m):           # a matriz vazia tem 0 linhas
        return 0
    else:
        return len(m[0])      # número de colunas é o comprimento duma linha

def row(m: GMatrix, i: int) -> GList:
    """ Get the row i of the matrix.
        Precondition: is_valid(m) and i in range(nrows(m))
    """
    return m[i].copy()      # faz cópia porque col também faz cópia

def col(m: GMatrix, j: int) -> GList:
    """ Get the column j of the matrix.
        Precondition: is_valid(m) and j in range(ncols(m))
    """
    col = []
    for r in m:             # acumula os vários elementos da coluna j
        col.append(r[j])
    return col

def input_int_matrix() -> IMatrix:
    """ Interactive matrix reading. """
    l = input("ROWS COLS: ").split()
    nr, rc = int(l[0]), int(l[1])
    m = []
    for i in range(nr):
        l = []
        for j in range(rc):
            x = int(input(f"m[{i}][{j}] = "))
            l.append(x)
        m.append(l)
    return m
Exemplos de utilização:
>>> m = create(3, 5, 0)
>>> print(m)
[[0, 0, 0, 0, 0], [0, 0, 0, 0, 0], [0, 0, 0, 0, 0]]
>>> is_valid(m)
True
>>> is_empty(m)
False
>>> nrows(m)
3
>>> ncols(m)
5

>>> row(m,1)
[0, 0, 0, 0, 0]
>>> col(m2,4)
[5, 5, 5]
 
>>> m3 = input_int_matrix()
ROWS COLS: 2 3
m[0][0] = 1
m[0][1] = 2
m[0][2] = 3
m[1][0] = 4
m[1][1] = 5
m[1][2] = 6
>>> print(m3)
[[1, 2, 3], [4, 5, 6]]
>>> 
Processamento de matrizes
Vamos agora usar as operações básicas para resolver alguns problemas sobre matrizes.
Há muitas funções sobre matrizes que percorrem a matriz para calcular uma estatística simples. Por exemplo determinar o valor máximo que ocorre na matriz. Neste caso a matriz dada não é alterada.

Há outras funções que envolvem tomar uma ou mais matrizes e produzir a partir delas uma matriz completa. Neste caso, há duas abordagens possíveis:

programar usando imutabilidade - a função cria uma matriz nova que preenche e retorna; as matrizes iniciais não são alteradas;
programar usando mutabilidade - a função altera diretamente uma das matrizes originais e não produz resultado (o resultado é None).
Exemplo 1
Problema: Função para somar uma unidade a cada elemento duma matriz de inteiros.
Programar usando imutabilidade:

def inc(m: IMatrix) -> IMatrix:
    """ New matrix with all the elements of m incremented.
        Precondition: is_valid(m)
    """
    nr, nc = nrows(m), ncols(m)
    m2 = create(nr, nc, 0)
    for i in range(nr):
        for j in range(nc):
            m2[i][j] = m[i][j] + 1
    return m2

>>> m = create(3, 5, 7)
>>> print(m)
[[7, 7, 7, 7, 7], [7, 7, 7, 7, 7], [7, 7, 7, 7, 7]]
>>> m2 = inc(m)
>>> print(m)
[[7, 7, 7, 7, 7], [7, 7, 7, 7, 7], [7, 7, 7, 7, 7]]
>>> print(m2)
[[8, 8, 8, 8, 8], [8, 8, 8, 8, 8], [8, 8, 8, 8, 8]]
>>> 
Programar usando mutabilidade:

def inc2(m: IMatrix) -> None:
    """ Increment all the elements of m.
        Precondition: is_valid(m)
    """
    nr, nc = nrows(m), ncols(m)
    for i in range(nr):
        for j in range(nc):
            m[i][j] += 1

>>> m = create(3, 5, 7)
>>> print(m)
[[7, 7, 7, 7, 7], [7, 7, 7, 7, 7], [7, 7, 7, 7, 7]]
>>> inc2(m)
>>> print(m)
[[8, 8, 8, 8, 8], [8, 8, 8, 8, 8], [8, 8, 8, 8, 8]]
>>> 
Exemplo 2
Problema: Somar duas matrizes de inteiros com as mesmas dimensões.
Programar usando imutabilidade:

def sum(m1: IMatrix, m2: IMatrix) -> IMatrix:
    """ New matrix with all the corresponding elements of m1 and m2 added.
        Precondition: is_valid(m1) and is_valid(m2)
           and nrows(m1) == nrows(m2) and ncols(m1) == ncols(m2)
    """
    nr, nc = nrows(m1), ncols(m1)
    m3 = create(nr, nc, 0)
    for i in range(nr):
        for j in range(nc):
            m3[i][j] = m1[i][j] + m2[i][j]
    return m3

>>> m1 = create(3, 5, 7)
>>> print(m1)
[[7, 7, 7, 7, 7], [7, 7, 7, 7, 7], [7, 7, 7, 7, 7]]
>>> m2 = create(3, 5, 11)
>>> print(m2)
[[11, 11, 11, 11, 11], [11, 11, 11, 11, 11], [11, 11, 11, 11, 11]]
>>> m3 = sum(m1, m2)
>>> print(m1)
[[7, 7, 7, 7, 7], [7, 7, 7, 7, 7], [7, 7, 7, 7, 7]]
>>> print(m2)
[[11, 11, 11, 11, 11], [11, 11, 11, 11, 11], [11, 11, 11, 11, 11]]
>>> print(m3)
[[18, 18, 18, 18, 18], [18, 18, 18, 18, 18], [18, 18, 18, 18, 18]]
>>> 
Programar usando mutabilidade:

def sum2(m1: IMatrix, m2: IMatrix) -> None:
    """ Add the elements of m2 to the corresponding elements of m1.
        Precondition: is_valid(m1) and is_valid(m2)
                  and nrows(m1) == nrows(m2) and ncols(m1) == ncols(m2)
    """
    nr, nc = nrows(m1), ncols(m1)
    for i in range(nr):
        for j in range(nc):
            m1[i][j] += m2[i][j]

>>> m1 = create(3, 5, 7)
>>> print(m1)
[[7, 7, 7, 7, 7], [7, 7, 7, 7, 7], [7, 7, 7, 7, 7]]
>>> m2 = create(3, 5, 11)
>>> print(m2)
[[11, 11, 11, 11, 11], [11, 11, 11, 11, 11], [11, 11, 11, 11, 11]]
>>> sum2(m1, m2)
>>> print(m1)
[[18, 18, 18, 18, 18], [18, 18, 18, 18, 18], [18, 18, 18, 18, 18]]
>>> print(m2)
[[11, 11, 11, 11, 11], [11, 11, 11, 11, 11], [11, 11, 11, 11, 11]]
>>> 
Quando o cabeçalho duma função indica que é retornada uma matriz, isso significa que a função foi escrita com a intenção de usar a técnica da imutabilidade.

#











Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 06b (21/out/2026)
Tuplos em Python.
Ranges em Python.
Sets em Python.
Dicionários em Python.

Tuplos
Eis um exemplo dum tuplo literal, com a sequência dos nomes dos meses do ano em português. Note o uso de parêntesis curvos.

("janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro")
Comparação dos tuplos com as listas
Para guardar e processar numerosos valores, as listas são normalmente adequadas.
Recordemos que as listas têm as seguintes características:

Representam uma sequência de valores, onde a ordem em que os valores aparecem é relevante.
Podemos percorrer os valores duma lista sequencialmente usando um for.
Permitem valores duplicados.
São mutáveis, no sentido em que podemos modificar o valor de qualquer posição duma lista, e também podemos fazer a lista crescer ou encolher.
A melhor mais simples de apresentar os tuplos é compará-los com as listas:

Tal como um lista, um tuplo representa uma sequência de valores. Também permitem valores duplicados. Também suportam indexação.
A única diferença é que os tuplos são imutáveis. Não podemos modificar o valor duma posição dum tuplo, e também não o podemos fazer crescer ou encolher.
As operações das listas estão disponíveis para tuplos, exceto as que envolvem modificação (por exemplo insert, append, pop, remove e reverse).
Usos dos tuplos
Representar sequências constantes
Olhe novamente para o tuplo dos meses do ano:
("janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro")
O facto de se usarem parêntesis curvos em vez de parêntesis retos, constitui uma mensagem importante para o leitor do código. Os parêntesis curvos indicam que o objeto que representa a sequência dos meses não se destina a ser alterado.

Além disso, se por engano o programador tentar alterar o tuplo, ocorre um erro de execução.

Implementar funções com mais do que um resultado
Uma funções pode usar um tuplo para retornar múltiplos resultados.
A seguinte função calcula o mínimo e o máximo duma lista ao mesmo tempo. Note o tipo do resultado.

def minmax(l: list[int]) -> tuple[int, int]:
    """ Min and max of integer list.
        Precondition: l != []
    """
    min = max = l[0]
    for v in l:
        if v > max:
            max = v
        if v < min:
            min = v
    return (min, max)
    
>>> minmax([2, 7, 5, -4])
(-4, 7)
Definir tipos simples
Para implementar datas de calendário, é sempre melhor definir uma dataclasse, como veremos na próxima aula.
Mas aqui vai uma alternativa interessante, usando tuplos:

type Date = tuple[int, int, int]   # (day, month, year)

def date_create(day: int, month: int, year: int) -> Date:
    return (day, month, year)

def date_is_valid(date: Date) -> bool:
    """ Is the data valid? """
    day, month, year = date  # acesso às componentes
    return (year > 0
        and 1 <= month <= 12
        and 1 <= day <= month_length(month, year))
Definição
Para descrever completamente o tipo dos tuplos temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas às listas
Valores matemáticos
Esta parte é igual à das listas. Um tuplo pode ser visto como uma sequência matemática finita:
a = a0 a1 a2 ... an 
Literais
Os literais dos tuplos são escritos entre parêntesis curvos (), com os diversos valores separados por vírgulas.
O tuplo vazio escreve-se simplesmente:

()                           # tuplo vazio
O seguinte tuplo regista as cores do arco-íris:
("vermelho", "laranja", "amarelo", "verde", "azul", "anil", "violeta")    # tuplo de strings
Agora surge um problema. Como se escreve um tuplo de comprimento 1? Usando esta notação para escrever um tuplo de comprimento 1, os parêntesis curvos confundem-se com os parêntesis normais das expressões. O truque/convenção do Python consiste em acrescentar uma vírgula para indicar que se trata dum tuplo. Exemplo:
("vermelho",)               # tuplo de tamanho 1
Operações
Os tuplos suportam as operações das listas que não envolvem modificação. Aqui está a maioria delas:
Operação	Descrição	Tipo
len(t)	comprimento	tuplo -> int
t[i]	acesso indexado	tuple * int -> any
in, not in	Pertença a tuplo	any * tuple -> bool
== != < > <= >=	comparações	tuple * tuple -> bool
tuple(seq)	a partir de sequência	seq -> tuple
t1+t2	concatenação	tuple * tuple -> tuple
t*i	concatenação múltipla	tuple * int -> tuple
enumerate(t)	cria sequência de pares com os índices explicitados	tuple -> tuple[int * any]
t.index(v)	índice da 1ª ocorrência	tuple * any -> int
t.count(v)	número de ocorrências	tuple * any -> int
Exemplos de uso das operações:
>>> rainbow = ("vermelho", "laranja", "amarelo", "verde", "azul", "anil", "violeta")
>>> len(rainbow)
7
>>> type(("vermelho"))            # não é um tuplo!
<class 'str'>
>>> type(("vermelho",))           # é um tuplo!
<class 'tuple'>

>>> rainbow[0]
'vermelho'
>>> rainbow[1]
'laranja'
>>> rainbow[len(rainbow)-1]
'violeta'
>>> rainbow[-1]
'violeta'
>>> rainbow[3]
'verde'
>>> rainbow[-4]
'verde'
>>> rainbow[100]
Traceback (most recent call last):
  File "", line 1, in 
IndexError: tuple index out of range

>>> 5 in (1,8,4,5)
True
>>> (1,2,3) < (2,3)
True

>>> tuple(range(10))
(0, 1, 2, 3, 4, 5, 6, 7, 8, 9)
>>> tuple("Python")
('P', 'y', 't', 'h', 'o', 'n')
>>> tuple({1,3,2,3,2,3})
(1, 2, 3)

>>> (1,2,3) + (3,4,5)
(1, 2, 3, 3, 4, 5)
>>> () + ()
()
>>> (0,) + (0,) + (0,) + (0,) + (0,) + (0,) + (0,) + (0,) + (0,) + (0,)
(0, 0, 0, 0, 0, 0, 0, 0, 0, 0)

>>> (0,)*10
(0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
>>> (0,1,2)*10
(0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2)
>>> (None,)*10
(None, None, None, None, None, None, None, None, None, None)

>>> tuple(enumerate(rainbow))
((0, 'vermelho'), (1, 'laranja'), (2, 'amarelo'), (3, 'verde'), (4, 'azul'), (5, 'anil'), (6, 'violeta'))

Ranges
O tipo dos ranges (intervalos) descreve sequências de inteiros que são efetivamente sucessões aritméticas finitas de inteiros. Por exemplo, o seguinte range:
range(4, 20, 3)
descreve a sucessão aritmética que começa no valor 4, não ultrapassa o valor 20, e usa o incremento (step) 3:
4, 7, 10, 13, 16, 19
Vamos confirmar, fazendo uma conversão para lista:
>>> list(range(4, 20, 3))
[4, 7, 10, 13, 16, 19]
Eis os principais usos práticos dos ranges:
No contexto do ciclo for, servem para percorrer uma sucessão de valores (e.g. for i in range(20)).
São muito úteis para criar certas listas ou tuplos de forma prática e expedita (e.g. list(range(20))).
Definição
Para descrever completamente o tipo dos ranges temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas às listas
Valores matemáticos
Um range pode ser modelado por uma sucessão aritmética finita de inteiros:
a = a0 a1 a2 ... an 
A sucessão tem um valor inicial inteiro a0, um valor limite inteiro e um incremento inteiro. Cada elemento da sucessão, exceto o primeiro, obtém-se somando o incremento ao elemento anterior.
Literais
Os literais têm a forma:
range(start, limit, step)
Note que quaisquer dos valores, incluindo o step, podem ser negativos. A única coisa que é proibida é o step ser zero.
Por omissão, o valor de start é 0. Por omissão o valor de step é 1. Desta forma:

range(5) == range(0,5,1)
range(0,5) == range(0,5,1)
Operações
Os ranges suportam apenas algumas das operações das listas, concretamente as seguintes:
Operação	Descrição	Tipo
len(r)	Comprimento	range -> int
r[i]	acesso indexado	range * int -> any
== !=	igualdade	range * range -> bool
r.index(v)	índice da 1ª ocorrência	range * any -> int
r.count(v)	número de ocorrências	range * any -> int
enumerate(r)	cria sequência com os índices explicitados	range -> int * range
Repare que não estão disponíveis as operações +, *, <, <=, >, >=.

Exemplos de uso das operações:

>>> r = range(0,30,3)
>>> print(r)
range(0, 30, 3)
>>> len(r)
10
>>> list(r)
[0, 3, 6, 9, 12, 15, 18, 21, 24, 27]

>>> r[5]
15
>>> r[-1]
27
>>> r[20]
Traceback (most recent call last):
  File "", line 1, in 
IndexError: range object index out of range

>>> range(0,30,3) == range(0,29,3)
True
>>> range(0,500,1000) == range(0,30,1000)
True
>>> range(0,500,10) == range(0,30,10)
False

>>> 15 in r
True
>>> 16 in r
False

>>> r.index(15)
5
>>> r.index(16)
Traceback (most recent call last):
  File "", line 1, in 
ValueError: 16 is not in range

>>> r.count(15)
1
>>> r.count(16)
0

Conjuntos
Eis um exemplo dum conjunto literal, com a sequência dos nomes dos meses do ano em português. Note o uso de chavetas.

{"vermelho", "laranja", "amarelo", "verde", "azul", "anil", "violeta"}
Comparação dos conjuntos com as listas
Tal como as listas, os conjuntos permitem representar e manipular coleções de valores. Além disso, tal como as listas, os conjuntos são mutáveis, sendo possível adicionar e remover elementos.
As três diferenças entre as listas e os conjuntos são as seguinte:

Um conjunto não tem elementos repetidos.
Os valores são guardados sem qualquer ordem particular (por exemplo, não faz sentido falar no primeiro valor dum conjunto).
Só guardam valores imutáveis (valores de tipos básicos, tuplos e ranges). Não podem guardar listas, sets e dicionários.
Os conjuntos capturam de forma direta a noção de conjunto matemático. Claro que podíamos usar uma lista para representar conjuntos, tentando não colocar elementos repetidos. Mas, estando o conceito de conjunto disponível de forma direta, é muito melhor assim. Conseguimos exprimir as nossas ideias de forma mais direta.
Uma particularidade importante dos conjuntos é que estão otimizados para que a operação in seja muito eficiente. Enquanto que a implementação da operação in nas listas percorre a lista desde o início à procura dum valor, no caso dos conjuntos a implementação determina a presença ou ausência do valor duma forma quase imediata, mediante o uso duma técnica chamada de "tabela de dispersão" (cujo estudo sai do âmbito da nossa disciplina).

Definição
Para descrever completamente o tipo dos conjuntos temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas às listas
Valores matemáticos
Os conjuntos matemáticos formalizam os conjuntos do Python.
Literais
Os literais dos conjuntos são escritos entre chavetas {}, com os diversos valores separados por vírgulas.
O conjunto vazio escreve-se assim:

set()                           # conjunto vazio
A notação {} está reservada para os dicionários vazios.
Mais exemplos de conjuntos literais:

{"vermelho", "laranja", "amarelo", "verde",
                "azul", "anil", "violeta"}  # conjunto de strings
{0, 1, 4, 9, 16, 25, 36, 49, 64, 81}        # conjunto de inteiros
{True}                                      # conjunto de booleanos
{56.78, 34e-45, 12.12, 0.0000234}           # conjunto de reais
{math.sqrt, math.sin, math.floor, round}    # conjunto de funções
{range(5), range(7), range(1000)}           # conjunto de ranges
Um conjunto-literal pode ter valores repetidos, mas as repetições são ignoradas no conjunto-valor.
Operações
Aqui está a maioria das operações sobre conjuntos:
Operação	Descrição	Tipo
len(s)	cardinalidade	set -> int
in, not in	Pertença a tuplo	any * set -> bool
== != < > <= >=	comparações e inclusão	set * set -> bool
set(container)	a partir de contentor	seq -> set
s1|s2 ou s1.union(s2)	união	set * set -> set
s1&s2 ou s1.intersection(s2)	intersecção	set * set -> set
s1-s2 ou s1.difference(s2)	diferença	set * set -> set
s.add(v)	acrescentar valor	set * any -> None
s1.update(s2)	acrescentar valores	set * set -> None
s.discard(v)	eliminar valor	set * any -> None
s.pop()	elimina elemento arbitrário	set -> any
s.clear()	eliminar todos os valores	set -> None
enumerate(t)	cria sequência com índices ad hoc explicitados	set -> list[int * any]
Exemplos de uso das operações:
>>> rainbow = {"vermelho", "laranja", "amarelo", "verde",
                 "azul", "anil", "violeta"}
>>> len(rainbow)
7
>>> len({"vermelho"})
1

>>> "vermelho" in rainbow
True
>>> "castanho" in rainbow
False

>>> rainbow == rainbow
True
>>> rainbow == set()
False
>>> {1,2,3} == {3,2,1,3}
True

>>> rainbow < rainbow                       # inclusão estrita
False 
>>> {"laranja", "azul", "anil"} < rainbow   # inclusão estrita
True
>>> rainbow <= rainbow                      # inclusão lata
True

>>> set("Python")
{'t', 'y', 'o', 'P', 'h', 'n'}

>>> {1,2,3,4} | {3,4,5}                    # união
{1, 2, 3, 4, 5}
>>> {1,2,3,4} & {3,4,5}                    # interseção
{3, 4}
>>> {1,2,3,4} - {3,4,5}                    # diferença
{1, 2}

>>> rainbow.discard("laranja")
>>> print(rainbow)
{'verde', 'amarelo', 'vermelho', 'azul', 'violeta', 'anil'}
>>> rainbow.add(123)
>>> print(rainbow)
{'verde', 'amarelo', 'vermelho', 123, 'azul', 'violeta', 'anil'}

Programação com sets
O uso de conjuntos simplifica enormemente a resolução de certos problemas.
Exemplo, que se resolve numa linha de código: Contar o número de valores distintos que ocorrem numa lista de valores básicos.

def count_distinct(l: list[any]) -> int:
    """ How many distinct elements? """
    return len(set(l))

>>> count_distinct([1,5,3,6,3,4,2,5,4,3,6,5,4,1,2,3])
6
>>> count_distinct("The quick brown fox jumps over the lazy dog")
28
Exercício: Resolva este problema sem usar conjuntos, usando apenas listas.

Dicionários
Um dicionário é um conjunto de pares chave:valor, que estabelece uma relação funcional entre um conjunto de chaves e um conjunto dos valores.
Por exemplo, o dicionário abaixo estabelece uma relação funcional entre o conjunto dos países e o conjunto das capitais:

capitals = {"Portugal":"Lisboa", "Espanha":"Madrid", "França":"Paris"}
Ser uma relação funcional significa que para cada chave, o valor associado é único.

Eis as propriedades mais importantes dos dicionários:

São sequências, porque guardam os pares pela ordem em que foram armazenados (nota-se isso quando se faz print).
São mutáveis, tal como as listas e os conjuntos.
Não podem existir dois pares com a mesma chave. [Caso contrário, a relação deixaria de ser funcional.]
A chave tem de ser dum tipo imutável: por exemplo, pode ser inteiro, string ou tuplo - mas não pode ser lista ou conjunto.
Os dicionários são muito úteis na programação, pois ajudam a lidar com relações funcionais entre um conjunto de chaves num conjunto de valores. Simplificam enormemente a resolução de certos problemas.

Definição
Para descrever completamente o tipo dos dicionários temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas às listas
Valores matemáticos
Cada dicionário representa uma função que mapeia um conjunto de chaves num conjunto de valores.
O conjunto de todas essas funções, podemos representá-las assim:

Maps = {K->V : K e V são conjuntos finitos}
Literais
Os literais das listas são escritos entre chavetas {}, com cada par chave:valor escrito com um : no meio.
Um exemplo de dicionário literal:

{"Portugal":"Lisboa", "Espanha":"Madrid", "França":"Paris"}
Mais exemplos de dicionários literais:
greek = {"α": "alpha", "ω": "omega", "𝛾": "gamma"}
numbers = {0: "zero", 1: "one", 2: "two"}
numbers_reverse = {"zero": 0, "one": 1, "two": 2}
O dicionário vazio pode escrever-se das duas formas seguintes:
{}                           # dicionário vazio
dict()                       # dicionário vazio
Operações
Aqui está a maioria das operações que envolvem dicionários:
Operação	Descrição	Tipo
len(d)	cardinalidade	dict -> int
in, not in	Pertença ao conjunto das chaves existentes	any * dict -> bool
== !=	igualdade	dict * dict -> bool
d[k]	acesso indexado	dict * key -> value
d[k] = v	atribuição indexada	dict -> key -> value -> None
d.keys()	sequência das chaves	dict -> seq[key]
d.values()	sequência dos valores	dict -> seq[value]
d.items()	sequência dos pares	dict -> seq[(key, value)]
d.pop(k)	eliminação de chave	dict -> key -> value
d1.update(d2)	acrescentar valores	dict * dict -> None
d.clear()	apaga o conteúdo	dict -> None
Para verificar se uma chave está presente num dicionário, usa-se a operação de pertença in:

>>> capitals = {"Portugal":"Lisboa", "Espanha":"Madrid", "França":"Paris"}
>>> "Portugal" in capitals
True
>>> "Burkina Faso" in capitals
False
Para obter o valor associado a uma chave usa-se indexação:

>>> capitals["Portugal"]     # índice pode ser de qualquer tipo imutável
'Lisboa'
Note que se a chave não existir, ocorre um erro de execução (por isso, podemos ter de testar primeiro se a chave existe).
>>> capitals["Burkina Faso"]
Traceback (most recent call last):
  File "", line 1, in 
KeyError: 'Burkina Faso'

>>> if "Burkina Faso" in capitals:
        print(capitals["Burkina Faso"])
else:
    print("Não")
Não
Para alterar o valor associado a uma chave usa-se indexação com atribuição:

>>> capitals["Portugal"] = "Porto"
>>> print(capitals)
{'Portugal': 'Porto', 'Espanha': 'Madrid', 'França': 'Paris'}
>>> capitals["Portugal"] = "Lisboa"
Para acrescentar uma chave nova, também se usa atribuição:
>>> capitals["Burkina Faso"] = "Ouagadougou"
>>> print(capitals)
{'Portugal': 'Lisboa', 'Espanha': 'Madrid', 'França': 'Paris', 'Burkina Faso': 'Ouagadougou'}
Para eliminar uma chave, usa-se a operação pop:
>>> capitals.pop("Burkina Faso")
'Ouagadougou'
>>> print(capitals)
{'Portugal': 'Lisboa', 'Espanha': 'Madrid', 'França': 'Paris'} 
Pode iterar-se sobre dicionários assim:
>>> for k in capitals:
...     print(f"{k} - {capitals[k]}")
... 
Portugal - Lisboa
Espanha - Madrid
França - Paris
ou assim:
>>> for (k,v) in capitals.items():
...     print(f"{k} - {v}")
... 
Portugal - Lisboa
Espanha - Madrid
França - Paris
Programação com dicionários
Exemplo: Tradutor de código morse.
MORSE_CODE = {
          'A':'.-', 'B':'-...',
          'C':'-.-.', 'D':'-..', 'E':'.',
          'F':'..-.', 'G':'--.', 'H':'....',
          'I':'..', 'J':'.---', 'K':'-.-',
          'L':'.-..', 'M':'--', 'N':'-.',
          'O':'---', 'P':'.--.', 'Q':'--.-',
          'R':'.-.', 'S':'...', 'T':'-',
          'U':'..-', 'V':'...-', 'W':'.--',
          'X':'-..-', 'Y':'-.--', 'Z':'--..',
          '1':'.----', '2':'..---', '3':'...--',
          '4':'....-', '5':'.....', '6':'-....',
          '7':'--...', '8':'---..', '9':'----.',
          '0':'-----', ', ':'--..--', '.':'.-.-.-',
          '?':'..--..', '/':'-..-.', '-':'-....-',
          '(':'-.--.', ')':'-.--.-' }
 
def morse(msg: str) -> str:
    """ Text to Morse encoder """
    s = ''
    for c in msg:
        if c in MORSE_CODE:
            s += MORSE_CODE[c]
        s += ' '
    return s

>>> m = morse("SOS")
>>> print(m)
'... --- ... '
>>>
Exemplo: Inverter dicionário:
capitals = {"Portugal":"Lisboa", "Espanha":"Madrid", "França":"Paris"}
 
def invert(d: dict) -> dict:
    d2 = {}
    for (a, b) in d.items():
    	d2[b] = a
    return d2

>>> d = invert(capitals)
>>> print(d)
{'Lisboa': 'Portugal', 'Madrid': 'Espanha', 'Paris': 'França'}
>>>
