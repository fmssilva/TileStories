
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 02a (23/set/2026)
Finalização da aula anterior.
Computação. Algoritmo. Programas e Linguagens de Programação.
Computadores eletrónicos.
Spyder - Um ambiente integrado de desenvolvimento de programas.
A Linguagem de Programação Python.

Conceitos iniciais: Computação, Algoritmos, Programas
Nesta cadeira vamos aprender a escrever programas que se destinam a ser executados num computador.
Para perceber o que é um programa, temos de compreender que se trata da concretização dum algoritmo que descreve computações.

Programar envolve a invenção de algoritmos que depois têm de ser convertidos em programas que correm no computador.

Tentemos clarificar estes conceitos:


Computação
Computação
Computação significa: processamento automático de informação.
É a atividade realizada pelos computadores.
A informática estuda a computação, incluindo formas úteis de tirar partido dela para resolver problemas importantes.
Facetas da computação
Faceta interna: a informação é codificada usando símbolos (e.g. bytes), sendo a computação uma atividade de manipulação e transformação automática de símbolos.
Faceta externa: ... mas, geralmente, as computações também estabelecem um diálogo interativo com o ambiente exterior (constituído por humanos e por outras máquinas).
Processamento automático e sua especificação
Qualquer computação é realizada de acordo com regras estabelecidas antes desse processamento se iniciar. É isso que significa o processamento ser automático. Depois de especificado, o processamento realiza-se com pouca ou nenhuma intervenção humana.
É necessário especificar as computações de forma rigorosa e exaustiva, prevendo todas as possibilidades.
Algoritmos

Muhammad ibn Musa al-Khwarizmi
Algoritmo
Um algoritmo descreve uma maneira de resolver um problema. Corresponde a um conjunto de regras que determinam completamente como uma computação vai decorrer. Precisamos de imaginação para inventar algoritmos.
Por outras palavras, um algoritmo é uma "receita" para resolver um problema.
Em geral um mesmo problema pode ser resolvido usando diversos algoritmos, ou seja, de diversas maneiras. Depende da imaginação de quem o inventa.
Um algoritmo é independente de qualquer linguagem de programação particular. Um dado algoritmo pode ser implementado em C, em Java, etc.
A palavra "algoritmo" deriva da palavra Algoritmi que por sua vez corresponde à latinização do nome do matemático Persa Muhammad ibn Musa al-Khwarizmi, nascido por volta de 780 DC. Este cientista, que trabalhou quase toda a sua vida em Bagdade, deu importantes contribuições para a Álgebra, Trigonometria e Aritmética. Veja aqui um vídeo de 5 minutos sobre este génio da antiguidade.
Exemplo de algoritmo
Eis o algoritmo de Euclides (300 BC), que serve para achar o máximo divisor comum (MDC) entre dois números - MDC(m,n).
Usar dois contadores x e y.
Inicializar x com m e inicializar y com n.
Se x > y então o valor de x muda para x - y
Se x < y então o valor de y muda para y - x
Repetir os dois passos anteriores até os valores de x e y ficarem iguais. Quando isso acontecer, esse é o resultado final.
Euclides demonstrou que este algoritmo calcula efetivamente o valor MDC(m,n) e não qualquer outro valor.
Exemplo - Cálculo de MDC(252, 105):

x	y
Inicialização	252	105
Passo 1	147	105
Passo 2	42	105
Passo 3	42	63
Passo 4	42	21
Passo 5	21	21
Características de um algoritmo
É exato e rigoroso: cada passo do algoritmo deve ser definido de forma precisa.
É efetivo: Cada passo do algoritmo é essencial.
Termina: Um algoritmo deve terminar após um número finito de passos.
Algoritmos, logo desde a escola primária
Logo na escola primária (1º ciclo), nós mesmos aprendemos diversos algoritmos: somar, subtrair, multiplicar e dividir.

Antiguidade dos Algoritmos
Os algoritmos mais antigos que se conhecem foram inventados pelos Babilónios (3000AC-1500AC). Eles criaram algoritmos para fatorizar números, achar raízes quadradas, e muito mais. Os seus livros de Matemática eram acima de tudo receitas sobre como efetuar os cálculos para resolver determinados problemas. Esses algoritmos eram para executar "à mão".
Antigamente, "computador" era o nome duma profissão humana (bastante monótona, diga-se). Quando apareceram os primeiros computadores eletrónicos, estes dispositivos receberam esse nome porque faziam trabalho que previamente era feito por computadores humanos.
A principal motivação para executar algoritmos em máquinas é a grande velocidade de execução.
Programas e Linguagens de Programação
Programa
Um programa é a expressão concreta dum algoritmo. Trata-se dum texto que descreve o algoritmo usando um sistema de regras de bem definido.
Diz-se que: um programa implementa um algoritmo numa linguagem de programação concreta.
Um algoritmo não pode ser executado diretamente. Mas depois de convertido para um programa, já pode ser executado por uma máquina.
Linguagem de programação
É uma notação para escrever programas.
Cada linguagem de programação é caraterizada por conjunto de regras sintáticas e por conjunto semânticas, descritas no documento de referência da linguagem.
As regras sintáticas definem a estrutura das construções de texto que podem ser usadas nos programas. Por exemplo: "para cada parêntesis a abrir deve existir um parêntesis a fechar correspondente".
As regras semânticas definem o efeito da execução dessas construções. Por exemplo, o operador "-" representa a operação matemática de subtração de dois números.
Elaboração dum programa
A elaboração dum programa consiste em três passos sucessivos:
Problema - Análise do problema para o percebermos bem.
Algoritmo - Conceção dum algoritmo para resolver o problema, o que requer imaginação e alguma experiência.
Programa - Implementação concreta do algoritmo usando uma linguagem como Python, Java, etc.
Elaborar programas em Python é o que nós fazemos na disciplina de IPCE. Tipicamente: (1) lemos o enunciado do problema; (2) começamos a imaginar um algoritmo; (3) vamos logo escrevendo o programa para o algoritmo que estamos a imaginar.
Implementação em Python do algoritmo de Euclides
Este programa corresponde ao algoritmo de Euclides que foi apresentado atrás.
def euclides(m: int, n: int) -> int:
    """ Greatest common divisor of two natural numbers.
        Precondition: m > 0 and n > 0
    """
    while m != n:
        if m > n:
            m -= n
        else:
            n -= m
    return m

def main() -> None:
    a = int(input("Introduza o primeiro inteiro: "))
    b = int(input("Introduza o segundo inteiro: "))
    print(f"euclides({a}, {b}) = {euclides(a,b)}")

main()
Eis um exemplo de execução do programa:
Introduza o primeiro inteiro: 123
Introduza o segundo inteiro: 456
euclides(123, 456) = 3
Computadores eletrónicos
A imagem abaixo mostra a arquitetura dum computador eletrónico moderno. Repare nos dispositivos de entrada e saída, na memória onde residem os dados e o programa, e no processador (CPU) que serve para executar o programa.


O Python é uma linguagem de alto nível, concebida para ser usada por humanos, mas não pelo processador da máquina. De facto, os circuitos eletrónicos do processador só "entendem" uma linguagem de baixo nível que é designada linguagem máquina.

Há duas técnicas de implementação de linguagens de alto nível:

Compilação: usa-se um programa especial, chamado de compilador, que traduz a linguagem de alto nível para linguagem máquina:


Interpretação: usa-se um programa especial, chamado de interpretador, que conhece e executa diretamente o código da linguagem de alto nível.
Para exemplificar, geralmente, as implementações da linguagem C usam a técnica de compilação e as implementações da linguagem Python compilam para uma linguagem intermédia que depois é interpretada.

Spyder - Um ambiente integrado de desenvolvimento de programas
Normalmente os programas são desenvolvidos dentro dum ambiente integrado de desenvolvimento, dentro do qual podemos escrever e executar os nossos programas. Um bom sistema de desenvolvimento aumenta a produtividade dos programadores e contribui indiretamente para aumentar a qualidade dos programas. Se não tivéssemos um ambiente integrado, restar-nos-ia usar um editor de texto normal para escrever os programas (o que também não seria o fim do mundo...)
Na nossa a disciplina usaremos o ambiente Spyder que funciona igualmente bem em Windows, Macintosh e Linux. O Spyder é um sistema gratuito, fácil de instalar, intuitivo e fácil de usar.

Veja na aula prática 1 um pequeno guia sobre a utilização do Spyder.



A Linguagem de Programação Python

Um livro antigo	
Guido van Rossum	
Matrícula do carro de Guido
Resumo
Concebida e implementada por Guido van Rossum entre 1988 e 1991 no Centrum Wiskunde & Informatica da Holanda. O Python 0.9.0 foi divulgado em 1991; o Python 2.0 em 2000; e o Python 3.0 em 2008.
Em 2026 é a linguagem mais popular de acordo com diversas estatísticas, por exemplo o TIOBE
É uma linguagem menos difícil de aprender do que outras e por isso foi adotada por muitos não programadores, como contabilistas e cientistas, para uma variedade de tarefas quotidianas.
Atualmente um elemento básico na Ciência de Dados, permitindo que analistas de dados e outros profissionais usem a linguagem para realizar cálculos estatísticos complexos, criar visualizações de dados, etc.
Mas a linguagem tem sido também usada em muitos outros domínios, como aprendizagem automática, programação para a Web, escrita de jogos, etc. .
Ao contrário da maioria das linguagens importantes, o Python não tem uma norma oficial, controlada por um organismo oficial. Mas tem uma especificação e implementação de referência, a mesma que nós usamos nas aulas e que é conhecida pelo nome técnico de CPython ("C" porque a implementação está escrita usando a linguagem C).
Para lá da implementação de referência, há muitas outras implementações que cumprem objetivos particulares (por exemplo o MicroPython está otimizado para correr em microcontroladores com pouca memória)
É uma linguagem de alto nível, que se diz multiparadigma por suportar diversos estilos de programação, nomeadamente os estilos imperativo, procedimental, funcional, orientado pelos objetos e concorrente. Nós iremos usar principalmente os estilos imperativo e procedimental.
Tem um gestor de memória automático.
Tem um sistema de tipos forte, que deteta todos os erros de tipo. Mas o sistema de tipos é dinâmico, o que significa que os erros de tipo são detetados em tempo de execução.
Tradicionalmente a linguagem não é muito rápida, mas nos últimos anos têm sido usadas técnicas de implementação que fazem com que a linguagem tenha atualmente uma velocidade satisfatória.
#80 .




Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 02b (23/set/2026)
Expressões booleanas e funções booleanas.
Início do estudo sistemático da linguagem Python.
Elementos Básicos da linguagem Python: Estrutura dum programa. Identificadores. Palavras reservadas. Comentários. Constantes.

Expressões booleanas e funções booleanas
A lógica tem sido estudada desde a antiguidade, notavelmente por Aristóteles e muitos outros.

O matemático George Boole, em meados do sec. XIX, definiu um sistema algébrico com operações para manipular os valores de verdade e falsidade.

O Python possui um tipo de dados para representar a verdade e a falsidade:

bool
Uma expressão que produza um resultado booleano chama-se uma expressão booleana ou condição. As condições são tipicamente usadas quando um programa tem de tomar uma decisão.
Por exemplo, na função abaixo (copiada da Teórica 2), ocorre uma condição complexa que é usada para se tomar uma decisão numa instrução if:

Avalia-se a condição year % 4 == 0 and year % 100 != 0) or year % 400 == 0.
Se a condição for verdadeira, é executado o 1º ramo do if (o ramo then do if);
se a condição for falsa, é executado o 2º ramo do if (o ramo else do if).
def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if (year % 4 == 0 and year % 100 != 0) or year % 400 == 0:
        return 366
    else:
        return 365
Em Python podemos escrever funções que se destinam a calcular um valor lógico! Quando uma condição representa um conceito importante dum programa, é conveniente escrever essa condição numa função separada e escolher um bom nome para a função. Na maioria das vezes, o nome das funções booleanas começa por "is_".
Para melhorar a qualidade do código anterior, vamos introduzir a função is_leap_year. Repare que o resultado é de tipo bool.

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0
Agora vamos reescrever a função year_length usando a função anterior:
def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if is_leap_year(year):
        return 366
    else:
        return 365
Em breve, noutras aulas será dada mais informação sobre o tipo dos booleanos.
As funções booleanas são importantes e já as começámos a usar nas aulas práticas.

Início do estudo sistemático da linguagem Python
O principal objetivo desta cadeira é aprender a programar, mas para programar temos de conhecer algumas partes selecionadas da nossa ferramenta de trabalho, que é a linguagem Python.
Em IPCE não se assume o conhecimento prévio de qualquer linguagem de programação. Vamos agora introduzir os elementos mais essenciais da linguagem Python, para você começar a conhecer a linguagem e começar a usá-la com confiança.

No que se segue, presume-se que vamos usar a linguagem Python recorrendo ao estilo procedimental.

Legibilidade dos programas
Quem escreve um programa tem a responsabilidade de escrever código legível, ou seja código compreensível, muito claro, fácil de entender e ser modificado por outras pessoas.
A legibilidade é um dos critérios mais importantes para aferir a qualidade dum programa. (Outro critério é, obviamente, a correção do programa.)

A linguagem Python dá muita liberdade ao programador. Contudo, um programa em Python de qualidade deve ser sujeito a algumas restrições:

Um bom programa é constituído por múltiplas funções. Cada função deve desempenhar uma tarefa bem definida, deve ter um nome bem escolhido que sugira essa tarefa, e deve ser a única responsável por essa tarefa. Este é o principal ponto que se quer enfatizar neste momento.
Há dois aspetos que precisam de ser tratados separadamente: (1) interação com o utilizador e (2) lógica da resolução dos problemas. A função main é especializada na interação com o utilizador (e poderão existir funções de interação adicionais se a interação for complexa). As restantes funções são especializadas na lógica da resolução dos problemas.
[Contudo, há casos raros em que o enunciado descreve uma interação com utilizador completamente misturada com a lógica. Nesse caso, podemos não conseguir separar os dois aspetos. Mesmo assim, tentamos reduzir ao mínimo a quantidade de funções que misturam os dois aspetos. Esta questão levanta-se no exercício 21 da prática 03a].
A boa indentação dos programas também contribui para a sua legibilidade. Neste aspeto, o Python já não dá realmente qualquer liberdade ao programador. A linguagem possui regras rígidas relacionadas com a indentação e qualquer imperfeição a este nível é considerada um erro sintático, ou então o programa fica com um significado diferente do pretendido.
Outros aspetos relevantes: uso de constantes, comentários, precondições, etc.
Estas regras não são arbitrárias. Fazem parte do património da Engenharia de Software. São questões identificadas pelos melhores engenheiros e investigadores na década de 1960.

Elementos Básicos da linguagem Python
Um programa em Python pode estar repartido por vários ficheiros, mas nós vamos dedicar-nos principalmente a escrever programas que estão guardados num único ficheiro.
Estrutura dum programa
Um programa em Python bem organizado deve ser constituído por diversas zonas, seguindo a ordem indicada abaixo. Vão ser listados aqui alguns conceitos novos, que só irão aparecer em programas futuros. Mas outros conceitos já apareceram nos programas da aula teórica 2.
Zona das instruções de import
É habitual colocar as instruções de import no início do programa, para as entidades importadas dum módulo externo ficarem disponíveis em todo o programa.
Contudo, se o objetivo é importar uma entidade para usar apenas dentro duma determinada função, então também se pode colocar a instrução de importação no início do corpo da função.
Exemplo:
import math
Zona das definições de tipos (type aliases)
Em programas mais elaborados, convém definir novos nomes de tipos, para os conceitos do programa ficarem mais claros.
Depois de definidos, estes tipos são usados da mesma forma dos tipos predefinidos: no cabeçalho das funções, para indicar os tipos dos parâmetros e dos resultados.
Exemplo:
type Date = tuple[int, int, int]     # (day, month, year)
Zona das definições de constantes
Numa linguagem de programação, uma constante é um nome que representa um valor fixo. Dar nome a um valor com significado especial, explicita o significado desse valor.
Colocam-se as constantes perto do início do programa para ficarem disponíveis em todo o programa.
Mas se uma constante for precisa apenas dentro duma determinada função, também não há mal em definir a constante apenas dentro da função.
O nome duma constante deve ser constituído apenas por letras maiúsculas (e ainda pode conter "_" e algarismos).
Exemplos:
HOURS_IN_DAY = 24
MINUTES_IN_HOUR = 60
SECONDS_IN_MINUTE = 60
SECONDS_IN_HOUR = MINUTES_IN_HOUR * SECONDS_IN_MINUTE
PI = math.pi
GRAVITY = 9.8
TABLE_CAPACITY = 256
SCHOOL_NAME = "FCT/UNL"
Zona das definições de variáveis globais
É um princípio clássico recomendar que um programa não deve ter variáveis globais. Em IPCE concordamos com este princípio.
Para não seguir este princípio numa situação particular, é preciso que exista uma razão bem justificada.
Uma variável global fica acessível em todas as funções e pode ser alterada por todas elas.
O nome duma variável global deve ser constituído apenas por letras minúsculas (e ainda pode conter "_" e algarismos).
Exemplo - uma variável global que contém o tabuleiro num jogo de xadrez:
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
Zona das definições de funções
A parte mais importante do programa corresponde a uma sequência de definições de funções.
Cada função deve desempenhar uma tarefa bem definida e deve ter um nome bem escolhido que sugira a sua tarefa.
O cabeçalho da função deve indicar o tipo dos parâmetros (quando existirem) e o tipo do resultado.
Se uma função for parcial ou se existirem condições especiais para o seu uso, deve ser escrita uma precondição no comentário do início da função.
Todas as variáveis e parâmetros definidos dentro duma função são locais à função. Essas variáveis e parâmetros não são conhecidos nem ficam acessíveis fora da função.
O nome duma função deve ser constituído apenas por letras minúsculas (e ainda pode conter "_" e algarismos).
def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1
    for i in range(1,n+1,1):
        fact = fact * i
    return fact
Zona final (ou zona da função main)
O programa termina com a definição duma função sem parâmetros e com resultado None chamada main.
Na última linha do programa aparece a chamada main().
Exercício
O seguinte programa contém três das zonas atrás referidas. Quais são essas zonas?
HOURS_IN_DAY = 24
MINUTES_IN_HOUR = 60
SECONDS_IN_MINUTE = 60
SECONDS_IN_HOUR = MINUTES_IN_HOUR * SECONDS_IN_MINUTE

def seconds(h: int, m: int, s: int) -> int:
    """ Convert the time h:m:s to seconds.
        Precondition:
               0 <= h < HOURS_IN_DAY
           and 0 <= m <= MINUTES_IN_HOUR
           and 0 <= s <= SECONDS_IN_HOUR
    """
    return h * SECONDS_IN_HOUR + m * SECONDS_IN_MINUTE + s   

def main() -> None:
    h = int(input("Horas: "))
    m = int(input("Minutos: "))
    s = int(input("Segundos: "))
    if not (0 <= h < HOURS_IN_DAY
           and 0 <= m <= MINUTES_IN_HOUR
           and 0 <= s <= SECONDS_IN_HOUR):
        print("Argumentos inválidos")
    else:
        print(seconds(h, m, s))

main()
Identificadores
Quando se olha para um programa em Python, percebe-se que há muitos identificadores (nomes) ocorrendo no programa. Os identificadores servem para dar nome às entidades dos programas.
Chama-se entidade à concretização dum conceito num programa. As entidades com que vamos trabalhar são as seguintes:

tipos
constantes
variáveis
parâmetros
funções
Exercício: No programa atrás, procure todos os identificadores e diga qual a entidade que cada um deles representa!

Os identificadores são palavras cuidadosamente inventadas pelo programador, para ajudar a tornar o programa mais fácil de perceber.

Em Python, um identificador começa por uma letra ou sublinhado '_' e continua com algarismos, letras e sublinhados. Exemplos:

print_table
x
x123
_1
_
Faz-se distinção entre maiúsculas e minúsculas. Por exemplo, os seguintes identificadores são considerados distintos:

Sum
SUM
sUM
SuM
É proibido usar como identificadores, as palavras reservadas da linguagem, tais como if, else ou return.

Também não convém usar como identificadores palavras que, não sendo palavras reservadas, já tenham uma utilização habitual, como por exemplo int, float, print ou type.

Devemos escrever todos os identificadores em inglês, se desejarmos que os nossos programas sejam compreendidos em todo o mundo, visto que a língua franca da atualidade é o inglês. Em IPCE, recomendamos fortemente que os identificadores sejam escritos em inglês (mas não queremos tornar isso estritamente obrigatório para os alunos).

Palavras reservadas
Palavras reservadas (keywords) são palavras que têm um significado especial na linguagem Python e cujo significado não pode ser alterado.
Por exemplo, se usarmos uma palavra reservada para dar nome a uma variável, obtemos um erro sintático. Veja:

>>> def = 5
  File "", line 1
    def = 5
        ^
SyntaxError: invalid syntax
Todas as palavras reservadas são em minúsculas, exceto as seguintes três: False, True, None.

Eis as 35 palavras reservadas do Python 3.13:

False	await	else	import	pass	None	break
except	in	raise	True	class	finally	is
return	and	continue	for	lambda	try	as
def	from	nonlocal	while	assert	del	global
not	with	async	elif	if	or	yield
Comentários
Os comentários são ignorados pelo Python e não contribuem para a execução dos programas. Os comentários servem para documentar o programa. Destinam-se ao ser humano que está a ler e a tentar entender o programa.
Há dois tipos de comentários em Python:

# Comentários de linha.

""" Comentários multilinha.
    Podem ocupar várias linhas.
    São escritos entre aspas triplas e a sua indentação é relevante.
"""
Convém evitar usar comentários para exprimir ideias óbvias. O seguinte exemplo é negativo e é considerado poluição do código:
wb = wb + 1                   # increments w
Mas o seguinte exemplo já faz sentido, pois tem a intenção de ajudar o leitor do código com informação nova:
wb = wb + 1                   # adjusts the windows border
Convém não exagerar na quantidade de comentários - usam-se só quando é preciso clarificar algum aspeto menos óbvio.
Mas também não ter receio de os usar nos casos em que fazem falta.

A função abaixo exagera nos comentários, À partida, é um exemplo negativo. Contudo ela pode fazer sentido num contexto de ensino da programação, para explicar ao aluno o que cada linha do código faz.

def factorial(n: int) -> int:
    """ Fatorial dum número natural.
        Precondition: n >= 0
    """

    # A implementação usa um ciclo 'for' que percorre
    # os valores de 1 até n. Com a ajuda da variável
    # de acumulação 'fact' todos estes valores são multiplicados
    # entre si.

    fact = 1                  # inicializa o acumulador
    for i in range(1,n+1,1):  # ciclo que faz o 'i' variar de 1 até n
        fact = fact * i       # acumula o valor corrente de 'i'
        # print(f"{i:2d} -> {fact}")
    return fact               # retorna o valor acumulado
Neste exemplo, também se usa um comentário para desativar um pedaço de código, que preferimos não apagar: a linha do print.

Docstrings
O comentário especial que se coloca logo por baixo do cabeçalho duma função chama-se uma docstring. Por convenção, escreve-se sempre entre aspas triplas.
Uma docstring descreve o objetivo duma função e também pode dar detalhes sobre a forma dela ser usada.

É muito importante documentar cada função:

Aumentamos a clareza da função e temos oportunidade de explicitar algum aspeto que se arriscaria a ficar implícito;
Uma função que coloquemos num programa deve ser vista como um elemento que pode ser reutilizado noutros programas, por nós próprios ou por outras pessoas; assim é essencial que a função fique bem documentada.
Uma docstring deve descrever a função do ponto de vista lógico e, normalmente, abstém-se de referir qualquer aspeto relacionado com a implementação interna da função. Para dar informação sobre aspetos de implementação usam-se comentários normais (veja o exemplo anterior, da função factorial).

Há dois tipos de docstrings - curtas e extensas:

Num programa normal, a maioria das funções são pequenas e têm uma missão muito específica. Nestas funções deve usar-se uma docstring curta, geralmente com apenas uma linha; ou duas linhas no caso de haver precondição.
Num programa normal, é normal ocorrerem algumas funções que têm uma missão mais complicada e que precisam de fornecer ao utilizador informação extensa. Nesse caso usa-se uma docstring extensa.
Este exemplo usa uma docstring extensa:
import math

AVERAGE_EARTH_RADIUS_KM = 6371.0
AVERAGE_EARTH_RADIUS_MILES = 3959.0

def haversine(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    """
    Calculate the great circle distance in kilometers between two points 
    on the earth (specified in decimal degrees)
    
    Parameters:
      lat1, lon1 - geographic coordinates of the first point
      lat2, lon2 - geographic coordinates of the second point

    Precondition:
          -90.0 <= lat1 <= 90.0
      and -180.0 <= lon1 <= 180.0
      and -90.0 <= lat2 <= 90.0
      and -180.0 <= lon2 <= 180.0

    Returns:
      Distance in km between the two points
    """
    # convert decimal degrees to radians
    dLat = math.radians(lat2 - lat1)
    dLon = math.radians(lon2 - lon1)
    lat1 = math.radians(lat1)
    lat2 = math.radians(lat2)
    
    # haversine formula
    a = (math.sin(dLat/2)**2
           + math.cos(lat1) * math.cos(lat2) * math.sin(dLon / 2)**2)
    c = 2 * math.asin(math.sqrt(a))
    return AVERAGE_EARTH_RADIUS_KM * c

Constantes
Em programação, uma constante é um nome que representa um valor fixo. (Já, em matemática, uma constante designa um valor fixo, mesmo que não tenha nome).
Exemplo de definição de constante em Python:

VOTING_AGE = 18
Depois de definir a constante, usamos o nome VOTING_AGE em vez do valor 18, sempre que nos referirmos à idade mínima para votar em Portugal.
Porquê definir constantes? Há duas razões:

O código fica mais legível, mais fácil de perceber. Normalmente o leitor não adivinharia o que significa o número 18 escrito diretamente no meio do programa. A escrita direta do 18 torna-se uma barreira ao entendimento do programa.
O código fica mais fácil de modificar. Se a legislação mudar e for modificada a idade mínima para votar em Portugal, muda-se o valor da constante e o programa fica novamente correto.
Usar constantes é uma excelente ideia! Mas será que devemos dar um nome a todos os valores fixos usados nos programas?
Não! Muitas vezes um valor representa-se a si próprio e não tem nenhum significado adicional que mereça ser registado num nome. Por exemplo, na atribuição seguinte, o número 1 representa-se a si próprio, sendo usado para incrementar uma variável. Seria disparatado dar um nome a este 1.

x = x + 1
Valores mágicos
Quando se usa diretamente num programa um valor fixo com significado especial, sem lhe dar um nome, diz-se que se trata dum valor mágico. O leitor olha para o valor, não faz a mínima ideia do que esse valor significa, mas a verdade é que o uso daquele valor particular faz o programa funcionar bem.
No código abaixo, o 35 é um valor mágico. Se mudarmos o valor, o programa passa a dar resultados incorretos. Mas não percebemos o que significa o 35... Será a idade mínima para ser Presidente da República em Portugal? Ou será o peso em kg das botijas grandes de gás propano?

if x >= 35:
    ...
Convenção
A seguinte convenção é usada em praticamente todas as linguagens de programação:
As letras que ocorrerem no nome duma constante estão todas em maiúsculas. Também podem ocorrer "_" e algarismos.
Quando vemos um nome todo em maiúsculas, sabemos que se trata duma constante.
Limitação do Python
Na maioria das linguagens de programação, há uma notação especial para definir constantes.
Por exemplo, em C/C++:

const VOTING_AGE = 18
Em Java:
final VOTING_AGE = 18
Estas linguagens não permitem alterar o valor duma constante. É isso o que se deseja, claro!
Infelizmente, o Python não tem suporte direto para a definição de constantes. Os programadores de Python têm de usar variáveis (com nomes em maiúsculas) para representar constantes. Os programadores de Python têm a responsabilidade de nunca mudar o valor das suas constantes, apesar da linguagem o permitir.

