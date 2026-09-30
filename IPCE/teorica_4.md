
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 04a (07/out/2026)
A função print.
Ciclos for e ciclos while.

Início
Vamos começar com um problema que usa a instrução for duma forma nova.
Escrever números decrescentemente
Enunciado do problema
Desenvolva um programa que escreva todos os valores dum intervalo fechado [a, b] pela ordem inversa.
Solução
def print_decreasing1(a: int, b: int) -> None:
    """ Print decreasingly, integers in the closed interval [a, b]
        Precondition: a <= b
    """
    for i in range(0, b-a+1, 1):
    	print(b - i)
    	
def print_decreasing2(a: int, b: int) -> None:
    """ Print decreasingly, integers in the closed interval [a, b]
        Precondition: a <= b
    """
    for i in range(b, a-1, -1):    # podem usar-se valores pela ordem inversa!
    	print(i)

def main() -> None:
    a = int(input("A: "))
    b = int(input("B: "))
    if not (a <= b):
        print("Argumentos inválidos")
    else:
        print('-------------')
        print_decreasing1(a, b)
        print('-------------')
        print_decreasing2(a, b)
        print('-------------')

main()
A função print
Convém conhecer relativamente bem a função print, para a conseguirmos usar da forma mais simples e conveniente em cada situação.
Vamos apresentar algumas possibilidades, usando exemplos.

- Ausência de argumento:

print()                                    # escreve uma simples mudança de linha
- Um argumento simples:

print(123)                                 # 123
print(1/3)                                 # 0.3333333333333333
print("Hello")                             # Hello
print({1,2,3})                             # {1, 2, 3}
- Múltiplos argumentos simples, e neste caso é inserido automaticamente um espaço entre as representações escritas:

print(123, 456)                            # 123 456
print("Hello", "World", 1/3)               # Hello World 0.3333333333333333
Esses espaços geralmente interessam-nos. Mas se os quisermos omitir, podemos usar a concatenação de strings:

print(str(123) + str(456))                 # 123456
print("Hello" + "World" + str(1/3))        # HelloWorld0.3333333333333333
- A mudança de linha que o print coloca no final geralmente interessa-nos. Mas se a quisermos omitir:
print("Hello", end='')
print("World")                             # HelloWorld
- A forma mais poderosa do print usa strings de formatação (f-strings):
x = 5
print(f"fatorial({x}) = {factorial(x)}")   # fatorial(5) = 120
As f-strings foram introduzidas na versão 3.6 do Python e muitos programadores adoraram isso.

Ciclos
Os ciclos permitem ao programador descrever tarefas repetitivas duma forma prática. As instruções relacionadas com ciclos são as seguintes:

for
while
break
return
Cada repetição da tarefa repetitiva chama-se uma iteração. Por esse motivo as instruções que implementam ciclos são também conhecidas por instruções iterativas.
Na aula teórica 5 falámos pela primeira vez em instruções de controlo. As quatro instruções indicadas acima são todas de controlo.

Já vimos, na aula teórica 2, que em Python também se pode exprimir repetição usando funções recursivas. Mas quando se usa o Python para escrever programas no chamado estilo procedimental (que é o nosso objetivo, de momento), o uso de ciclos é o que está previsto.

Há dois tipos de ciclos em Python:

ciclos for - são escolhidos quando queremos processar contentores.
ciclos while - são escolhidos para lidar com outras situações.
Usar um ou outro tipo de ciclo não é uma questão de preferência:

Devemos escolher, em cada situação, a solução que seja entendível com menor esforço (ou seja, mais legível).
Quando se trata de processar um contentor, a solução que usa um for costuma ser a mais legível.
Ciclos for
O ciclo for do Python é muito flexível e prático porque permite iterar sobre os valores de qualquer contentor suportado pela linguagem.
Um contentor é um objeto que pode conter outros objetos: conjunto, lista, tuplo, range (intervalo), string, etc. Os contentores cujo conteúdo siga uma dada ordem, têm a designação adicional de sequências.

Vamos agora analisar muitos exemplos de utilização do for usando diversos tipos de contentores.

Nestes exemplos, vamos apresentar apenas situações que envolvem escrita de dados, porque assim conseguimos observar imediatamente os efeitos do que está a acontecer.

for aplicado a intervalo (range)
O seguinte exemplo escreve os números inteiros de 20 até 49, com um passo de 3:
>>> n = 50
>>> for i in range(20,n,3):
...     print(i)
... 
20
23
26
29
32
35
38
41
44
47
Eis detalhe prático sobre a construção range: Se a sequência de inteiros começar em 0,então pode omitir-se a indicação do início da sequência. Além disso, quando o passo é 1, pode omitir-se o passo. Por exemplo, range(n) é equivalente a range(0,n,1).
Usar ou não a forma abreviada nos ranges é uma questão de preferência pessoal.

O seguinte exemplo escreve os números inteiros de 0 até 9, com um passo de 1:

>>> n = 10
>>> for i in range(n):
...     print(i)

... 
0
1
2
3
4
5
6
7
8
9
for aplicado a lista
O seguinte exemplo escreve o nome de algumas cores:
>>> colors = ["vermelho", "amarelo", "azul", "vermelho"]
>>> for i in colors:
...     print(i)
... 
vermelho
amarelo
azul
vermelho
for aplicado a um tuplo
O seguinte exemplo também escreve o nome de algumas cores:
>>> colors = ("vermelho", "amarelo", "azul", "vermelho")
>>> for i in colors:
...     print(i)
... 
vermelho
amarelo
azul
vermelho
for aplicado a conjunto
O seguinte exemplo escreve o nome de algumas cores, mas agora através dum conjunto.
Um conjunto guarda os elementos sem repetição. Assim se explica o que está a ser escrito (o "vermelho" não aparece repetido):

>>> colors = {"vermelho", "amarelo", "azul", "vermelho"}
>>> print(colors)
{'vermelho', 'amarelo', 'azul'}
>>> for c in colors:
...     print(c)
... 
amarelo
azul
vermelho
for aplicado a string
O seguinte exemplo escreve os carateres duma string. Faz sentido, porque uma string é uma sequência:
>>> s = "Python"
>>> for i in s:
...     print(i)
... 
P
y
t
h
o
n
for aplicado a dicionário
Este exemplo só será completamente percebido quando dermos atenção aos dicionários, numa aula futura.
>>> mounts = {"janeiro":31, "fevereiro":28, "março": 31}
>>> for m in mounts:
...     print(f"{m} -> {mounts[m]}")
... 
janeiro -> 31
fevereiro -> 28
março -> 31
Ciclos for encaixados (imbricados)
O que acontece se escrevermos um ciclo for dentro de outro ciclo for?
Neste caso ocorre um efeito multiplicativo: para cada valor do ciclo exterior, o ciclo interior é executado.

O seguinte exemplo escreve todas as permutações de comprimento 2 que se conseguem obter combinando os primeiros m naturais e os primeiros n naturais.

A função é testada usando os valores 4 e 3. Repare que m * n = 4 * 3 = 12, sendo esse o número de linhas escritas.

def print_permutations_2(m: int, n: int) -> None:
    """ Print all the permutations of i in range(m), j in range(n). """
    for i in range(m):
        for j in range(n):
            print(f"({i}, {j})")

def main() -> None:
    print_permutations_2(4, 3)

>>> main()
(0, 0)
(0, 1)
(0, 2)
(1, 0)
(1, 1)
(1, 2)
(2, 0)
(2, 1)
(2, 2)
(3, 0)
(3, 1)
(3, 2)
Repare que são primeiro escritos todos os pares com 0 na esquerda; a seguir com 1 na esquerda, etc. Assim, confirma-se que para cada valor do ciclo exterior, o ciclo interior corre completamente.
Ciclos for que acumulam
Muitas das utilizações do ciclo for envolvem problemas de acumulação. Já vimos diversos problemas desta natureza nas aulas práticas, incluindo a função factorial.
O seguinte exemplo calcula a média dos valores inteiros entre a e b, com a<=b. A função soma os valores e divide pela quantidade de valores.

def average_of_range(a: int, b: int) -> float:
    """ Average of the values in a range of integers.
        Precondition: a <= b
    """
    sum = 0
    for i in range(a, b+1, 1):
        sum += i
    return sum / (b - a + 1)

>>> average_of_range(0, 10)
5.0
>>> average_of_range(1, 10)
5.5
>>> average_of_range(10, 20)
15.0
>>> average_of_range(10, 10)
10.0
Neste exemplo, a variável sum costuma ser designada como variável de acumulação. Ela é inicializada com o elemento neutro da operação em causa. Depois, dentro do ciclo, acumula-se nela, a pouco e pouco, aquele que será o valor final.
Ciclo for que efetua uma pesquisa
Para programar uma pesquisa usando um ciclo for, vamos usar adicionalmente as instruções return ou break.
Exemplo de pesquisa usando return
A função abaixo testa se uma dada string s contém um dado caráter c:

def search(s: str, c: str) -> bool:
    """ Check if the char c occurs in the string s
        Precondition: len(c) == 1
    """ 
    for i in s:
        if i == c:
            return True
    return False

>>> search("Python", 't')
True
>>> search("Python", 'x')
False
O ciclo for começa com a intenção de percorrer a string completa:

Mas, se durante o percurso, o caráter procurado for encontrado, o ciclo desiste de ir até ao fim e a função retorna imediatamente o valor True.
Se o ciclo chegar ao final, é porque o caráter não foi encontrado, e nesse caso o resultado é False.
Note que seria um grave erro escrever o código abaixo, que não faz o que se pretende:

def search_wrong(s: str, c: str) -> bool:
    for i in s:
        if i == c:
            return True
        else:
            return False
Se você desejar mesmo explicitar o else correspondente ao if que está dentro do ciclo, não fica muito elegante, mas seria assim:
def search(s: str, c: str) -> bool:
    """ Check if the char c occurs in the string s
        Precondition: len(c) == 1
    """ 
    for i in s:
        if i == c:
            return True
        else:
            pass
    return False
Se você não quiser usar um return dentro do ciclo, isso é possível com a ajuda duma variável auxiliar (found). A legibilidade é equivalente, mas perde-se eficiência, porque o ciclo vai sempre até ao final.
def search(s: str, c: str) -> bool:
    """ Check if the char c occurs in the string s
        Precondition: len(c) == 1
    """ 
    found = False
    for v in s:
        if v == c:
            found = True
    return found
Exemplo de pesquisa usando break
Vamos resolver o mesmo problema de pesquisa de mais uma forma ainda: quando o caráter procurado é encontrado, quebra-se (termina-se) o ciclo for, mas desta vez sem abandonar a função corrente.

Para quebrar um ciclo, usa-se a instrução break, que faz a execução do for terminar imediatamente.

Tomando o exemplo anterior, agora acrescentamos um simples break que será executado quando o valor procurado for descoberto. A ineficiência que foi referida antes, ficou resolvida.

def search(s: str, c: str) -> bool:
    """ Check if the char c occurs in the string s
        Precondition: len(c) == 1
    """ 
    found = False
    for v in s:
        if v == c:
            found = True
            break
    return found

>>> search("Python", 't')
True
>>> search("Python", 'x')
False
Nos nossos programas, quando quisermos interromper um ciclo a meio, é provável que usemos mais a técnica do return do que a técnica do break. Será frequente implementarmos um ciclo numa função independente, como fizemos no caso da pesquisa.

Ciclo for para intervalo de reais?
Os intervalos (ranges) do Python só suportam números inteiros. Seria interessante poder escrever expressões como range(0.0, 2.1, 0.1), mas tal não é suportado em Python.
No entanto, não é difícil fazer uma redução a intervalos de inteiros. Para começar, é possível escrever um intervalo de inteiros que faça o mesmo número de iterações que faria o indisponível intervalo de reais.

A função abaixo, escreve uma tabela para a função f(x) = x2 num dado intervalo real [a, b], com um dado passo step. Observe os detalhes associados às variáveis x e y:

import math

def f(x: float) -> float:
    return x * x

def table_square(a: float, b: float, step: float) -> None:
    """ Print table for the function f with x varying between a and b inclusive """ 
    n = math.floor((b - a)/step) + 1  # número de pontos de avaliação
    for i in range(0,n,1):
        x = a + step * i
        y = f(x)
        print(f"f({x:0.6f}) = {y:0.6f}")

>>> table_square(0.0, 2.1, 0.1)
f(0.000000) = 0.000000
f(0.100000) = 0.010000
f(0.200000) = 0.040000
f(0.300000) = 0.090000
f(0.400000) = 0.160000
f(0.500000) = 0.250000
f(0.600000) = 0.360000
f(0.700000) = 0.490000
f(0.800000) = 0.640000
f(0.900000) = 0.810000
f(1.000000) = 1.000000
f(1.100000) = 1.210000
f(1.200000) = 1.440000
f(1.300000) = 1.690000
f(1.400000) = 1.960000
f(1.500000) = 2.250000
f(1.600000) = 2.560000
f(1.700000) = 2.890000
f(1.800000) = 3.240000
f(1.900000) = 3.610000
f(2.000000) = 4.000000
f(2.100000) = 4.410000
Ciclos while
O ciclo while é mais geral do que o for e permite tratar qualquer situação que necessite dum ciclo. Recorde que o for só serve para iterar sobre contentores.

A instrução while é necessária quando a repetição de dada ação depende duma condição lógica requerida pelo problema: por exemplo, podemos querer repetir algo enquanto o valor duma variável for diferente de zero.

Na prática, um ciclo while é praticamente sempre usado de acordo com o seguinte esquema:

inicialização
while condição:
    corpo              # inclui o "avanço"
O significado as linhas anteriores é o seguinte: Após a inicialização, um ciclo while executa uma sequência de instruções (o corpo do while) enquanto uma dada condição (a condição do while) for verdadeira.

O corpo do while tem de especificar dois aspetos:

A contribuição para o objetivo do ciclo: por exemplo, somar um elemento durante o cálculo dum somatório.
O avanço - que garante a progressão de variáveis (muitas vezes auxiliares) para que o ciclo percorra o espaço de valores correto.
Mais aspetos essenciais do while:

A condição do while é testada no início. Se for logo avaliada a False, o corpo do while não chega a ser executado nenhuma vez. Portanto, existe a possibilidade dum ciclo while efetuar zero iterações
Por outro lado, se a condição for sempre avaliada a True, o ciclo while nunca termina: o corpo do while é executado repetidamente, sem fim, e estamos perante um ciclo infinito.
Os nossos ciclos precisam de terminar! O avanço tem a responsabilidade de fazer evoluir as variáveis envolvidas na condição por forma a garantir terminação. Realmente, a condição precisa de se tornar falsa ao fim de algumas iterações.
Exemplo introdutório com while
Vamos considerar novamente o exemplo do cálculo da média dos valores inteiros entre a e b, com a<=b.

Seria mais simples programar usando um for, mas aqui vamos optar por um ciclo while, para ver como fica:

def average_of_range(a: int, b: int) -> float:
    """ Average of the integers between a and b inclusive """ 
        Precondition: a <= b
    """
    sum = 0
    i = a                   # inicialização
    while i <= b:           # condição
        sum +=i             # corpo
        i += 1              # corpo (avanço)
    return sum / (b - a + 1)
Entenda o código anterior e analise bem: a inicialização, a condição, o corpo, o avanço.
Comparando os ciclos for com os ciclos while
Algo que chama logo a atenção no código anterior é que, quando usamos um while, temos de ser nós a gerir a variável do ciclo i, algo que um ciclo for faz automaticamente.
Compare com a versão que usa um ciclo for.

def average_of_range(a: int, b: int) -> float:
    """ Average of the integers between a and b inclusive """ 
        Precondition: a <= b
    """
    sum = 0
    for i in range(a, b+1, 1):
        sum += i
    return sum / (b - a + 1)
Esta segunda versão é mais legível porque destaca logo na primeira linha do for os limites de variação.
Por outro lado, para perceber a versão com while, temos de procurar ativamente onde se encontra a inicialização e o avanço. Num ciclo while grande, a procura desses dois elementos pode demorar alguns segundos. Depois ainda temos de ver se os dois elementos fazem sentido e estão corretos.

Quando estiver em causa um contentor, preferimos usar um ciclo for. Quando não estiver em causa um contentor, usaremos um ciclo while sem qualquer hesitação.

Primeiro problema que exige um ciclo while
Considere o problema de procurar o primeiro número primo maior ou igual a um dado inteiro n.
Eis a nossa solução. O ciclo da função next_prime é rudimentar e evolui de 1 em 1 (a partir de n), testando se o valor corrente é primo:

def is_prime(n: int) -> bool:
    # está na teórica 6
    pass

def next_prime(n: int) -> int:
    while not is_prime(n):
        n += 1              # avanço
    return n

>>> next_prime(5555653)     # já é primo
5555653
>>> next_prime(5555654)     # não é primo
5555677
Pergunta: Como é que podemos justificar que este ciclo termina sempre?

Resposta: A Matemática diz-nos que a quantidade de números primos é infinita e portanto a procura irá terminar mais cedo ou mais tarde

Perceber melhor os ciclos while
A seguinte definição matemática, que tenta usar a sintaxe do Python, descreve matematicamente o funcionamento dum ciclo while. É uma definição recursiva, como acontece normalmente na matemática:
def execute(while C: B, state) -> None:
    if evaluate(C, state):
        state1 = execute(B, state)
        state2 = execute(while C: B, state1)
        return state2
    else:
        return state
Para perceber esta definição, temos de começar por aprender o seguinte: Num programa, o conjunto das variáveis mais os respetivos valores, designa-se por estado do programa. Quando um programa é executado, o estado do programa vai mudando ao longo do tempo (usando a instrução de atribuição, ou de outras formas).
O que a definição diz é o seguinte:

Para executar um while C: B num dado estado:
Primeiro avalia-se a condição C.
Se C for verdadeira, então executa-se o corpo B; e logo a seguir executa-se o while C: B completo original (é isto que causa o efeito de repetição). Estas execuções podem mudar o estado.
Se C for falsa, então não se executa mais nada (significa que o ciclo terminou). O estado não muda.
Segundo problema que requer um ciclo while
Consideremos agora um novo problema: ler uma sequência de inteiros positivos a partir do teclado e calcular a soma desses inteiros.
Se perguntarmos, logo no início, a quantidade de valores a somar, então podemos usar um ciclo for:

def read_and_sum(n: int) -> int:
    """ Input and add a sequence of integers. """
    sum = 0
    for i in range(n):
        v = int(input("> "))
        sum += v
    return sum

def main() -> None:
    n = int(input("Introduza a quantidade de números a somar: "))
    print(read_and_sum(n))

main()

>>> main()
Introduza a quantidade de números a somar: 5
> 10
> 11
> 10
> 11
> 10
52
Está feito!
Mas agora vamos assumir que não sabemos à partida o número de valores a somar. O utilizador introduzirá um valor convencional, por exemplo -1, para indicar o final da sequência.

Eis a primeira tentativa de solução:

END_MARK = -1

def read_and_sum() -> int:
    """ Input and add a sequence of integers. """
    sum = 0
    v = int(input("> "))       # leitura antes do ciclo
    while v != END_MARK:
        sum += v
        v = int(input("> "))   # leitura dentro do ciclo (avanço)
    return sum

def main() -> None:
    print(f"Introduza uma sequência de números para somar, terminada por {END_MARK}")
    print(read_and_sum())

main()

>>> main()
Introduza uma sequência de números para somar, terminada por -1
> 10
> 11
> 10
> 11
> 10
> -1
52
Esta solução está correta e já é razoavelmente boa e é perfeitamente aceite em IPCE!
Mas há dois aspetos que complicam um pouco a legibilidade e a manutenção futura do programa:

A operação de leitura está escrita em dois lugares: antes do ciclo, para pedir o primeiro valor; e dentro do ciclo para pedir os valores seguintes.
Dentro do ciclo estranha-se o uso da ordem "somar e depois ler" - seria mais intuitivo "ler e depois somar".
Haverá forma de evitar a duplicação e mudar a ordem das instruções no corpo?

Eis uma solução "revolucionária". Consiste em fazer a leitura exclusivamente dentro do ciclo e usar um break para terminar o ciclo.

Estamos perante um exemplo de ciclo com saída pelo meio, onde a condição que determina o final do ciclo ocorre a meio:

END_MARK = -1

def read_and_sum() -> int:
    """ Input and add a sequence of integers. """
    sum = 0
    while True:
        v = int(input("> "))    # avanço
        if v == END_MARK:       # condição dum ciclo com saída pelo meio
            break
        sum += v
    return sum

def main() -> None:
    print(f"Introduza uma sequência de números para somar, terminada por {END_MARK}")
    print(read_and_sum())

main()
Quem observa esta solução pela primeira vez, normalmente estranha a condição do ciclo ser True, mas a verdade é que não há nada que necessite de ser testado nesse ponto.

Esta solução pode dizer-se "revolucionária" porque ignora a forma normal de usar o while: a condição do while é trivializada (fica um simples True) e inventa-se uma condição no interior do corpo que acaba por ser a condição decisiva para a lógica do ciclo.

Este tipo de ciclos com saída pelo meio foram discutidos por Dijkstra nos anos 1960. Ele chamou a atenção para a conveniência de suportar este tipo de ciclos que designou de ciclos Loop-and-a-Half.

Ciclos com saída pelo início, pelo meio e pelo final
Saída pelo início
Como a condição do while é testada no início, costuma dizer-se que um ciclo while normal é um ciclo com saída pelo início.
Um ciclo for também é um ciclo com saída pelo início, porque o for testa uma condição interna antes de cada iteração, incluindo a primeira iteração.

Há muitos problemas em que usar um ciclo com saída pelo início é o que convém para a lógica da solução.

Como já foi dito, os ciclos com saída pelo início podem executar zero iterações.

O Python suporta diretamente ciclos com saída pelo início, através das instruções while e for.

Saída pelo meio
Se a lógica da solução convidar a testar a condição do ciclo a meio do corpo, podemos usar a técnica do exercício anterior. Ou então tentar reformular a solução noutros moldes (com algum risco de ficar mais complicado e menos legível).
Algumas linguagens de programação oferecem uma instrução específica para ciclos com saída pelo meio. Por exemplo, em Ada, a instrução loop/exit when.

Em Python, através da instrução break, conseguimos imitar este tipo de instruções de forma bastante aproximada. Podemos afirmar que o Python suporta ciclos com saída pelo meio através da instrução break.

Saída pelo final
Ao contrário do Python, muitas linguagens de programação dispõem dum tipo de ciclo com saída pelo final, por exemplo, repeat/until em Pascal e do/while em Java.
Este tipo de ciclo tem a particularidade de executar pelo menos uma iteração, visto a condição ser testada no final.

Discussão final sobre o uso de return ou break nos ciclos
Só se deve usar return ou break nos ciclos com boa justificação:
Apenas se ajudar a tornar a expressão das ideias do programador mais direta e mais clara, no sentido de se obter código mais legível, mais fácil de entender depressa (para depois ser mais fácil de alterar e também para se poupar tempo).
A maioria dos ciclos dispensa o seu uso. Não faz sentido usar essas instruções de forma injustificada.

Por exemplo, os dois ciclos abaixo são equivalentes. Mas o segundo é um exemplo de mau estilo, porque está a implementar um ciclo de forma desnecessariamente complicada, portanto menos legível:

sum = 0
while i < size:
    sum += l[i]
    i += f(i)

sum = 0
while True:
    if i >= size:     # mau estilo, porque está a complicar e até a confundir
        break
    sum += l[i]
    i += f(i)
#




Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 04b (07/out/2026)
Listas em Python.

Listas
Muitas vezes precisamos de guardar e processar múltiplos valores. Por exemplo, processar:
As notas finais dos 210 alunos da cadeira de IPCE;
As temperaturas médias de todos os dias ao longo dum ano.
A linguagem Python oferece um mecanismo específico que responde a este tipo de necessidade: o mecanismo das listas.
Eis um exemplo: a lista de cores do arco-íris apresentadas pela sua ordem natural (que estamos a associar a uma variável chamada rainbow):

rainbow = ["vermelho", "laranja", "amarelo",
             "verde", "azul", "anil", "violeta"]
Representação em memória
A representação duma lista na memória do computador requer o uso dum bloco contíguo. No caso da lista das cores do arco-íris, a organização em memória é a seguinte:

rainbow[0]   →   "vermelho"
rainbow[1]   →   "laranja"
rainbow[2]   →   "amarelo"
rainbow[3]   →   "verde"
rainbow[4]   →   "azul"
rainbow[5]   →   "anil"
rainbow[6]   →   "violeta"
Alguns pontos importantes:
Cada célula da lista é identificada por um índice inteiro não negativo.
Os índices começam em zero e continuam sequencialmente de um em um.
Cada célula da lista tem um valor associado.
O número de elementos duma lista l obtém-se escrevendo len(l).
Acesso a partir do final
O Python suporta outra forma de identificar as células, usando índices negativos, contados a partir do final da lista:
rainbow[-7]   →   "vermelho"
rainbow[-6]   →   "laranja"
rainbow[-5]   →   "amarelo"
rainbow[-4]   →   "verde"
rainbow[-3]   →   "azul"
rainbow[-2]   →   "anil"
rainbow[-1]   →   "violeta"
Isto é muito prático em algumas situações. Por exemplo, permite referir o último elemento da lista, sem ser preciso levar em conta o comprimento da lista.
As listas são sequências
O Python suporta vários tipos de sequências. As listas constituem um tipo particular de sequência, muito usado. Para além das listas, os outros tipos de sequências mais importantes são: tuplo, range (intervalo), string.
A propriedade que caracteriza qualquer sequência é o facto de cada elemento ser identificado por um índice! Um contentor que não seja sequência não tem essa propriedade.

Definição
Para descrever completamente o tipo das listas temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas às listas
Valores matemáticos
Em matemática uma sequência finita é uma enumeração finita de valores. Nessa enumeração, é permitida a ocorrência de repetições e a ordem dos elementos é relevante.
Em matemática, uma sequência finita genérica a de comprimento n costuma ser representada usando a seguinte notação:

a = a0 a1 a2 ... an-1 
Por exemplo, eis a sequência finita dos primeiro 10 quadrados perfeitos:
square10 = 0 1 4 9 16 25 36 49 64 81
O conjunto de todas as sequências finitas é infinito enumerável e podemos tentar representá-lo assim:
𝕊f = {seq0, seq1, seq2, ... seqi, ... : i ∈ ℕ}
Literais
Os literais das listas são escritos entre parêntesis retos [], com os diversos valores separados por vírgulas.
A lista vazia escreve-se simplesmente:

[]                           # lista vazia
Eis mais exemplos de listas literais:
["vermelho", "laranja", "amarelo",
       "verde", "azul", "anil", "violeta"]              # lista de strings
[0, 1, 4, 9, 16, 25, 36, 49, 64, 81]                    # lista de inteiros
[False, True, True, True, False, True]                  # lista de booleanos
[56.78, 34e-45, 12.12, 0.0000234]                       # lista de reais
[math.sqrt, math.sin, math.floor, round]                # lista de funções
[[1,2,3], [], [], [56,55,43,21], [0]]                   # lista de lista de inteiros
[0, "vermelho", 0, True, math.sin, [3,[4],"zz"], 23, 0] # lista heterogénea (as anteriores são homogéneas)
Operações de acesso
Operação	Descrição	Tipo
len(l)	comprimento	list -> int
l[i]	acesso indexado	list * int -> any
in, not in	pertença a lista	any * list -> bool
== != < <= > >=	operadores relacionais
A função len produz o comprimento duma lista. Trata-se duma função geral especial que pode ser aplicada a qualquer tipo de contentor, por exemplo a um conjunto, uma string, uma lista, etc.
A operação de acesso indexado [] permite aceder a qualquer elemento duma lista (e, em geral, duma sequência). Os índices começam em 0 mas, como já vimos, também podem ser usados índices negativos para acesso a partir do final.

As operações in, not in para testar a ocorrência dum valor numa lista.

Temos ainda diversas operações booleanas de comparação, para testar se duas listas são iguais ou se uma lista é menor do que outra lista. Note que existe uma ordenação definida nas listas - a chamada ordem lexicográfica (basicamente, compara-se o primeiro elemento da primeira lista com o primeiro elemento da segunda lista; se forem diferentes, isso já decide o resultado; se forem iguais, então repete-se a ideia para os segundos elementos, e por ai fora...)

Exemplos de uso das operações:

>>> rainbow = ["vermelho", "laranja", "amarelo",
                   "verde", "azul", "anil", "violeta"]
>>> len(rainbow)
7

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
IndexError: list index out of range

>>> "laranja" in rainbow
True

>>> [1,2,3] < [2,3]
True
>>> [1,2,3] < [1,2,3]
False
<>>> [1,2,3] < [1,2,3,4]
True
Operações de criação
Operação	Descrição	Tipo
list(cont)	a partir de contentor	container -> list
l1+l2	concatenação	list * list -> list
l*i	concatenação múltipla	list * int -> list
zip(l1,l2)	emparelhamento	list * list -> list[any*any]
enumerate(l)	cria sequência de pares
com os índices explicitados	list -> list[int*any]
Estas operações oferecem formas práticas de criar novas listas (sem modificar listas existentes).
Exemplos:

>>> list(range(10))
[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
>>> list("Python")
['P', 'y', 't', 'h', 'o', 'n']
>>> list({6,3,5,8})
[8, 3, 5, 6]           # sim, poder ficar desordenado

>>> [1,2,3] + [3,4,5]
[1, 2, 3, 3, 4, 5]
>>> [] + []
[]
>>> [0] + [0] + [0] + [0] + [0] + [0] + [0] + [0] + [0] + [0]
[0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

>>> [0]*10
[0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
>>> [0,1,2]*10
[0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1,
         2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2]
>>> [None]*10
[None, None, None, None, None, None, None, None, None, None]

>>> list(zip(["Portugal", "Espanha", "França"],
               ["Lisboa", "Madrid", "Paris", "Berlin"]))
[('Portugal', 'Lisboa'), ('Espanha', 'Madrid'), ('França', 'Paris')]

>>> list(enumerate(rainbow))
[(0, 'vermelho'), (1, 'laranja'), (2, 'amarelo'),
        (3, 'verde'), (4, 'azul'), (5, 'anil'), (6, 'violeta')]
Operações de modificação
Operação	Descrição	Tipo
l[i] = v	atribuição indexada	list -> int -> any -> None
l.insert(i, v)	inserção	list * int * any -> None
l.append(v)	acrescenta valor no final	list * any -> None
l.pop(i)	remoção	list * int -> any
l1.extend(l2)	acrescenta lista no final	list * list -> None
O tipo das listas, é o primeiro tipo que encontramos que suporta operações de modificação!
Numa lista, os valores guardados na lista podem ser trocados por outros, usando atribuição indexada.

Também há operação específicas que permitem a uma lista crescer ou encolher.

As operações insert, append e extend não produzem resultado. A operação pop o valor removido da lista.

Podem ser indicados índices negativos, com o significado de que a posição a modificar é contada a partir do final da lista.

Qual a diferença entre l[i] = v e l.insert(i,v)?

l[i] = v - altera o conteúdo da posição i da lista. O tamanho da lista não muda.
l.insert(i,v) - insere um novo elemento na posição i da lista, fazendo a lista crescer uma posição. Tudo o que se encontra a começar no índice i, avança uma posição para a frente.
l.insert(-1,v) - (caso enganador, apesar da regra ser a mesma) insere na última posição da lista. Tudo o que se encontra a começar no índice -1, avança uma posição para a frente... ou seja, o valor inserido fica na 2ª posição a partir do final.
Exemplos:

>>> l = list(range(10))
>>> print(l)
[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]

>>> l[0] = "zero"
>>> l[5] = "cinco"
>>> print(l)
['zero', 1, 2, 3, 4, 'cinco', 6, 7, 8, 9]
>>> l[-1] = "último"
>>> print(l)
['zero', 1, 2, 3, 4, 'cinco', 6, 7, 8, 'último']
>>> l.insert(7, 6.5)
>>> print(l)
['zero', 1, 2, 3, 4, 'cinco', 6, 6.5, 7, 8, 'último']
>>> l.insert(-1, "estranho")
>>> print(l)
['zero', 1, 2, 3, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 'último']
>>> l.insert(len(l), "final")
>>> print(l)
['zero', 1, 2, 3, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 'último', 'final']

>>> l.append("finalíssimo")
>>> print(l)
['zero', 1, 2, 3, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 'último', 'final', 'finalíssimo']

>>> l.pop(0)
'zero'
>>> print(l)
[1, 2, 3, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 'último', 'final', 'finalíssimo']
>>> l.pop(-1)
'finalíssimo'
>>> print(l)
[1, 2, 3, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 'último', 'final']
>>> l.pop(2)
3
>>> print(l)
[1, 2, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 'último', 'final']
>>> l.pop()
'final'
>>> l.pop()
'último'
>>> print(l)
[1, 2, 4, 'cinco', 6, 6.5, 7, 8, 'estranho']

>>> l.extend([99, 88, 77])
>>> print(l)
[1, 2, 4, 'cinco', 6, 6.5, 7, 8, 'estranho', 99, 88, 77]
Métodos
Agora apareceu esta nova notação, que envolve usar um operador ponto. Por exemplo:

l.pop(5)
As funções especiais que são chamadas com esta sintaxe chamam-se métodos.
Em Python, cada objeto tem os seus métodos, que fazem parte integrante do próprio objeto. Em contraste, uma função é uma entidade com autonomia, sem ligação a qualquer objeto.

Curiosidade: Será que os objetos inteiros também têm métodos? Têm sim: i1 + i2 é apenas "notação açucarada" para uma expressão que também pode ser escrita assim: i1.__add__(i2).

>>> 5 .__add__(6)
11
>>> i1 = 5
>>> i2 = 6
>>> i1 + i2
11
>>> i1.__add__(i2)
11
Operações adicionais sobre listas
As operações sobre listas que apareceram até agora são consideradas operações essenciais. Algumas vezes você será chamado a resolver alguns problemas usando apenas as funções essenciais, como forma de treinar a programação.
Mas o Python oferece mais algumas operações sobre listas que oferecem soluções prontas a usar:

Operação	Descrição	Tipo
l.clear()	elimina todo o conteúdo	list -> None
l.copy()	duplica	list -> list
l.remove(v)	remoção da 1ª ocorrência	list * any -> None
l.index(v)	índice da 1ª ocorrência	list * any -> int
l.count(v)	número de ocorrências	list * any -> int
l.sort()	ordenação	list -> None
l.reverse()	inversão	list -> None
l[i:j:k]	cria slice (fatia)	list * int * int * int -> list
max(l)	máximo	list -> any
min(l)	mínimo	list -> any
sum(l)	soma	list -> num
Exercício: Tente programar algumas destas operações recorrendo apenas às operações essenciais. Por exemplo pode implementar a operação clear por aplicação sucessiva da operação pop.

Programação com listas
Vamos programar algumas funções sobre listas de inteiros. Pela primeira vez, vamos definir um tipo novo, o tipo das listas de inteiros:
type IList = list[int]
Duas formas de percorrer uma lista
Muitas vezes, fica mais simples usar a primeira técnica. Mas há muitos casos em que precisamos de recorrer à segunda técnica.
1ª técnica: Percorrer diretamente os elementos usando um for deste género:

for v in l:
Por exemplo:
def sum(l: IList) -> int:
    """ Add the elements of a list. """
    sum = 0
    for v in l:            # 1ª técnica   
        sum += v
    return sum

>>> sum([1,2,3])
6
2ª técnica: Percorrer com base no índice, assim:

n = len(l)
for i in range(n):
Por exemplo:
def has_two_consecutive_equal(l: IList) -> bool:
    """ Does it contain two consecutive repeating elements? """
    n = len(l)
    for i in range(n-1):            # 2ª técnica 
        if l[i] == l[i+1]:
            return True
    return False

>>> has_two_consecutive_equal([1,2,3,4,5])
False
>>> has_two_consecutive_equal([1,2,3,3,4,5])
True
Pergunta: Por qual razão, na solução deste problema particular, queremos que o ciclo percorra a lista só até ao índice n-2?
Ler sequência de inteiros
Produz lista nova.
A marca de fim é uma linha vazia.

END_MARK = ''

IList = list[int]

def input_int_list() -> IList:
    """ Input sequence of integers to a list. """
    l = []
    s = input(": ")        # 1ª linha
    while s != END_MARK:
        l.append(int(s))
        s = input(": ")    # linhas seguintes
    return l

>>> ls = input_int_list()
: 1
: 2
: 3
: 4
: 
>>> print(ls)
[1, 2, 3, 4]
Ler sequência de inteiros (outra maneira)
Produz lista nova.
A marca de fim é uma linha vazia.

END_MARK = ''

IList = list[int]

def input_int_list() -> IList:
    """ Input sequence of integers to a list. """
    l = []
    while True:
        s = input(": ")
        if s == END_MARK:
            break;
        l.append(int(s))
    return l

>>> ls = input_int_list()
: 1
: 2
: 3
: 4
: 
>>> print(ls)
[1, 2, 3, 4]
Contar o número de ocorrências dum valor
def count(l: IList, v: int) -> int:
    """ Count the ocurrences of some integer value. """
    sum = 0
    for x in l:
        if x == v:
            sum += 1
    return sum

>>> l = list(range(4))*4
>>> print(l)
[0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3]
>>> count(l, 3)
4
>>> count(l, 99)
0
Trocar entre si, os conteúdos de duas posições
Altera a lista original e por isso não precisa de produzir resultado.
def swap(l: IList, i: int, j: int) -> None:
    """ Swap the contents of two positions.
        Precondition: 0 <= i < len(l) and 0 <= j < len(l)
    """
    l[i], l[j] = l[j], l[i]

>>> l = list(range(10))
>>> print(l)
[0, 1, 2, 3, 4, 5, 6, 7, 8, 9]
>>> swap(l,3,7)
>>> print(l)
[0, 1, 2, 7, 4, 5, 6, 3, 8, 9]
>>> swap(l,2,2)
>>> print(l)
[0, 1, 2, 7, 4, 5, 6, 3, 8, 9]
Inverter lista
Ao contrário do reverse predefinido no Python, este reverse2 produz uma lista nova, respeitando a lista original.
def reverse2(l: IList) -> IList:
    """ Create a reversed copy of a list. """
    a = []
    for x in l:
        a.insert(0, x)
    return a

>>> l = list(range(4))*4
>>> print(l)
[0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3]
>>> reverse2(l)
[3, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0]
Apagar conteúdo de lista
Altera a lista original e por isso não produz resultado.
def clear(l: IList) -> None:
    """ Remove all the elements from list. """
    n = len(l)
    for _ in range(n):        # podemos usar uma variável anónima   
        l.pop()

>>> l = list(range(4))*4
>>> print(l)
[0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3]
>>> clear(l)
[3, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0]
Criar sublista
Produz lista nova.
def sublist(l: IList, a: int, b: int) -> IList:
    """ Sublist of the elements with index in the range(a, b).
        Precondition: 0 <= a <= len(l) and 0 <= b <= len(l)
    """
    res = []
    for x in range(a, b):
        res.append(l[x])
    return res

>>> l = list(range(20))
>>> print(l)
[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]
>>> sublist(l, 5, 12)
[5, 6, 7, 8, 9, 10, 11]
Outra maneira, usando slices:
def sublist(l: IList, a: int, b: int) -> IList:
    """ Sublist of the elements with index in the range(a, b).
        Precondition: 0 <= a <= len(l) and 0 <= b <= len(l)
    """
    return l[a:b]

>>> l = list(range(20))
>>> print(l)
[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]
>>> sublist(l, 5, 12)
[5, 6, 7, 8, 9, 10, 11]
#
