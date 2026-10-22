
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 03a (30/set/2026)
Elementos que se podem escrever nos programas: Definições de entidades; Expressões; Instruções.
Dados em Python.
Variáveis em Python.

Elementos que se podem escrever nos programas: Definições de entidades; Expressões; Instruções
Vimos na aula anterior a estrutura geral recomendada para um bom programa em Python. Um programa deve estar organizado de acordo com estas seis zonas, em sucessão.
Zona das instruções de import
Zona das definições de tipos (type aliases)
Zona das definições de constantes
Zona das definições de variáveis globais
Zona das definições de funções
Zona final
Um programa não é obrigado a possuir todas as zonas, como já vimos.
Agora queremos entrar nos detalhes. Que componentes elementares são realmente usadas nos programas em Python, dentro das funções, na definição das constantes, etc?

Há três categorias de componentes elementares que se escrevem nos programas:

Definições de entidades
Expressões
Instruções
Definições de entidades
Uma entidade é um conceito individualizado que o programador usa para exprimir as suas ideias e organizar um programa. Uma entidade tem sempre um nome, e o programador usa esse nome para se referir à entidade.

As categorias de entidades mais importantes suportadas pela linguagem Python são as seguintes:

funções
parâmetros
variáveis
constantes
tipos
módulos
classes
Cada entidade é definida usando sintaxe própria.
O seguinte extrato de código define cinco entidades:

MULT = 128
a = 5
def strange(a: int) -> int:
    b = a + a
    return a * b * MULT
São estas as cinco entidades:
Constante MULT
Variável global a
Função strange
Parâmetro a
Variável local b
Note que neste código há duas entidades distintas com o mesmo nome. Isso é permitido. Mas o facto da função definir uma entidade local chamada a, impede a função de aceder à variável global a. Dentro duma função, as entidades locais são prioritárias.
Muito importante: A entidades definidas dentro duma função são locais a essa função. Apenas a própria função pode aceder a elas. A partir do exterior, não há acesso às entidades locais.

Exemplo: A função strange tem duas entidades locais: o parâmetro a e a variável local b.

Expressões
Uma expressão é um pedaço de código com uma sintaxe específica que se destina a ser avaliado para produzir um resultado. O conceito de avaliação está exclusivamente ligado às expressões.

Exemplos de expressões:

1                                                    # Uma expressão, muito simples
x + 1                                                # Outra expressão relativamente simples
b * b - 4 * a * c                                    # Uma expressão mais complicada
(-b + math.sqrt(discriminant(a,b,c)) / (2 * a)       # Outra expressão complicada
É muito frequente o uso de operadores nas expressões. Eis a tabela dos operadores de expressão mais importantes do Python, por ordem decrescente de precedência, e onde também se indica a associatividade:
Operador	Descrição	Associatividade
( )	Parêntesis	esquerda
**	Expoente	direita
* / // %	Multiplicação / Divisões / Resto	esquerda
+ -	Adição&Concatenação / Subtração	esquerda
< <= > >=	Operadores relacionais	esquerda
== !=	Operadores relacionais	esquerda
in, not in	Pertença a coleção	esquerda
not	Operador lógico	direita
and	Operador lógico	esquerda
or	Operador lógico	esquerda
Exemplo: No código abaixo, qual o valor da expressão a + b * c? O Python avalia a expressão (isto é, "faz as contas") e o resultado é 7.

a = 1
b = 2
c = 3
d = a + b * c    # o valor da expressão é 7
print(d)
Instruções
Uma instrução (ou comando) é um pedaço de código que executa uma ação. Atenção, uma instrução não produz resultado.

Uma instrução executa uma ação, como por exemplo: mudar o valor duma variável, fazer uma escrita no ecrã, fazer uma leitura do teclado.

Exemplos de instruções:

days = 366          # instrução de atribuição.
print("Olá!")       # instrução de escrita no ecrã
Vamos rever as instruções do Python que usámos até agora. Em aulas futuras iremos estudar mais algumas.
Instrução vazia
Em Python, a instrução vazia não faz nada. Eis a instrução vazia:
pass
Esta instrução é usada quando queremos que o corpo dum ciclo ou o ramo dum if não faça nada. Temos de escrever o pass porque não é permitido deixar em branco o corpo dum ciclo ou o ramo dum if.
Exemplo - trata-se dum if estranho, mas está de acordo com as regras da linguagem:

if x >= 0:
    pass
else:
    print("É negativo")
Seria mais natural escrever desta maneira:
if x < 0:
    print("É negativo")
Instrução de atribuição
Serve para mudar o valor duma variável. Falaremos mais desta instrução quando estudarmos com pormenor as variáveis.
Exemplo:

days = 366          # instrução de atribuição
Instruções de chamada de função sem resultado
Qualquer chamada duma função que não produza resultado (tipo de retorno None) é considerada uma instrução. Se houvesse resultado, a chamada seria uma expressão.
Exemplo:

print("Olá!")       # instrução de chamada de função sem resultado
Instruções de controlo
As instruções de controlo permitem-nos controlar a ordem pela qual as coisas acontecem, por exemplo na tomada de decisões e na execução de ciclos.
Já usámos algumas instruções de controlo nos exemplos da aula teórica anterior, concretamente as instruções if, for e return.

Três exemplos:

if leap_year(year):         # instrução if
    days = 366
else:
    days = 365;

for i in range(0,10,1):     # instrução for
    print(i)
 
return x * x * x            # instrução return
Exercício
No programa abaixo, identifique as definições de entidades, as expressões e as instruções:
import math

def hypotenuse(cathetus1: float, cathetus2: float) -> float:
    """ Length of the hypotenuse of a right triangle.
        Precondition: cathetus1 > 0 and cathetus2 > 0
    """
    return math.sqrt(cathetus1 ** 2 + cathetus2 ** 2)

def main() -> None:
    cat1 = float(input("Primeiro cateto: "))
    cat2 = float(input("Segundo cateto: "))
    print(f"Hipotenusa({cat1},{cat2}) = {hypotenuse(cat1, cat2)}")

main()
Dados em Python
Valores
Cada elemento de dados que o Python manipula chama-se um valor.
Os valores do Python estão classificados em categorias naturais que se chamam tipos. Temos o tipo dos inteiros, o tipo das strings, etc.

Exemplos de alguns valores e respetivos tipos:

O número 1 é um valor e tem tipo int.
O número 12.6 é um valor e tem tipo float.
A string "olá" é um valor e tem tipo str.
Tecnicamente, todos os valores do Python são objetos. Mas não queremos aprofundar, neste momento, o que significa ser "objeto". O que é importante é saber que em Python podemos usar as palavras valor e objeto de forma intermutável.
Tipos predefinidos
A linguagem Python introduz diversos tipos predefinidos (built-in types).
Eis a lista dos tipos predefinidos que iremos usar em IPCE:

Tipo booleano: bool
Tipos numéricos: int, float, complex
Tipo string: str
Tipos de sequências: tuplo, range, list
Outros tipos predefinidos: dict, set
O Python disponibiliza um função chamada type, que permite saber o tipo dum valor. Eis algumas experiências com a função type:
Python 3.11.4 (main, Jul  5 2026, 14:15:25) [GCC 11.2.0] on linux
Type "help", "copyright", "credits" or "license" for more information.

>>> type(123)
<class 'int'>

>>> type(123.0)
<class 'float'>

>>> type("olá")
<class 'str'>

>>> type(4.6+6.7j)
<class 'complex'>

>>> a=42
>>> type(a)
<class 'int'>

>>> a="olá"
>>> type(a)
<class 'str'>

>>> type([1,2,3])
<class 'list'>

>>> 
A função type não vai ter qualquer papel na nossa aprendizagem da programação, mas é útil para tirar dúvidas.
Valores imutáveis e valores mutáveis
Em Python, a maioria dos tipos representa valores imutáveis. Depois de criado um valor imutável, esse valor já não pode ser modificado.
Por exemplo, a string "olá" não pode ser alterada. O tipo das strings não disponibiliza qualquer operação de modificação. (Note que há linguagens de programação em que as strings são mutáveis.)

Eis os tipos com valores imutáveis que iremos usar:

Tipo booleano: bool
Tipos numéricos: int, float, complex
Tipo string: str
Tipos de sequências: tuplo, range
Em Python, existe uma minoria de tipos predefinidos que representa valores mutáveis. Depois de criado um valor mutável, este pode ser modificado. Há operações disponíveis para fazer a modificação. Por exemplo, a lista ['a', 'b', 'c'] é mutável. Veremos noutra altura que é possível modificar esta lista.
Eis os tipos com valores mutáveis que iremos usar:

Tipos de sequências: list
Outros tipos predefinidos: dict, set
Mutabilidade é uma característica que influencia muito a forma de resolver os problemas. Estudaremos isso noutra altura.
Os valores imutáveis são intuitivos e conseguimos trabalhar com eles sem problema. Note que não os podemos modificar, mas podemos usá-los para criar novos valores.

Por exemplo, um inteiro 3 não pode ser modificado, e ainda bem... o 3 será sempre o 3. Mas ao avaliar 3+1 obtemos o número 4 que é um novo valor. Este 4 foi obtido sem modificar o 3 original.

Outro exemplo: As strings "olá" e "olé" não podem ser modificadas, mas a expressão "olá" + "olé" produz a string nova "oláolé".

Variáveis em Python
Trabalhar diretamente com valores fixos conhecidos não nos permite ir muito longe na programação. Nós precisamos a algum mecanismo que nos permita referir um valor que foi lido a partir do teclado, um valor que resultou da avaliação duma expressão, etc.
As variáveis constituem um mecanismo que foi inventado para resolver este problema.

Uma variável é um nome que tem um valor associado. Conseguimos manipular o valor indiretamente, através da variável.

Definição de variáveis e atribuição
A instrução de atribuição permite associar um valor a uma variável. Por exemplo, a instrução abaixo cria o valor 42 e associa-o à variável x.
x = 42    # atribuição
Note que se usa o sinal de igual da matemática para exprimir a atribuição em Python. É uma escolha infeliz porque a atribuição não tem nada a ver com a igualdade matemática (há linguagens que usam os símbolos := ou ←).
Para definir uma variável nova com um dado nome, basta fazer uma atribuição envolvendo esse nome. Neste caso, tecnicamente, a instrução de atribuição torna-se também uma definição.

No seguinte exemplo, a primeira linha do corpo da função define uma variável x nova, e a segunda linha efetua uma atribuição normal à mesma variável.

def f() -> None:
    x = 42       # atribuição/definição
    x = x + x    # atribuição normal
    ...
Acesso ao valor duma variável
Para aceder ao valor duma variável basta escrever o nome da variável. Por exemplo, o seguinte código acede três vezes ao valor da variável x:
def f() -> None:
    x = 42
    print(x)           # acesso ao valor de x
    y = x + 2          # acesso ao valor de x
    x = x + 1          # acesso ao valor de x
Variáveis e tipos
Uma variável não tem qualquer tipo diretamente associado. Em Python os tipos estão associados aos valores e não às variáveis.
Apesar de ser considerado mau estilo, em Python permite-se que uma variável possa referir valores de tipos diferentes, ao longo do tempo. Por exemplo, as seguintes três atribuições, em sucessão, são permitidas.

x = 42
x = "olá"
x = 45.6
Salvo algumas exceções que precisam de ser justificadas, é melhor usar cada variável com valores sempre do mesmo tipo.
Referências
O que é que acontece exatamente dentro do computador quando se faz uma atribuição em Python? Vamos falar um pouco deste assunto.
Em Python, cada objeto tem um endereço interno único que se chama uma referência. É através dessa referência que uma variável refere um objeto.

Considere esta atribuição:

x = 42
A execução deste código cria o objeto 42 e faz a variável x referir esse objeto. Observe o diagrama:



Considere agora uma segunda atribuição, feita a seguir à primeira atribuição:

y = x
Neste caso, o Python não cria um objeto novo. Ele simplesmente associa à variável y o mesmo objeto que está associado à variável x.


Nesta situação, as variáveis x e y ficam a partilhar o mesmo objeto.

Perceber que duas ou mais variáveis podem partilhar o mesmo objeto é extremamente importante se o objeto for mutável! As implicações para a lógica do programa são enormes. Se o objeto for alterado através da variável x, a alteração também fica visível através da variável y.

O que acontece se agora executarmos:

y = 43
Fica assim:


Variantes da instrução de atribuição
Eis um exemplo de atribuição básica:

x = 42
Agora um exemplo de atribuição múltipla - todas as variáveis são definidas ao mesmo tempo e ficam todas a referir o mesmo valor.

x = y = z = 42
Agora um exemplo de atribuição paralela - todas as variáveis são definidas ao mesmo tempo, mas cada uma delas recebe um valor separado.

x, y, z = 42, 55, -4
A atribuição paralela é muito útil para trocar entre si o valor de duas variáveis:
a, b = b, a
Na atribuição paralela, primeiro avaliam-se todas as expressões do lado direito e só depois se associam os valores às variáveis que são referidas dos lado esquerdo. As atribuições elementares são feitas da esquerda para a direita.
Explique o valor final que é escrito:

>>> x, y, x = 1, 2, 3
>>> print(x)
3
Exercício: como é que seria feita a troca a,b = b,a usando apenas atribuições básicas. Precisa de quantas atribuições básicas: duas, três ou mais?

Agora dois exemplos de atribuições aumentadas - tratam-se de abreviaturas que se podem usar com alguns operadores binários:

x += 123
y *= 44
O código anterior é equivalente a:
x = x + 123
y = y * 44
É considerado bom estilo usar atribuições aumentadas. O facto de se escrever o nome da variável apenas uma vez, facilita a leitura do código e também conduz a menos enganos.
Eis uma tabela com os operadores mais importantes das atribuições:

Operador	Descrição	Associatividade
= += -= *= /= //= %= **= 	Atribuição 	direita
#






Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 03b (30/set/2026)
Problema dos números perfeitos.
Tipos. O conceito de tipo. Anotações de tipo.
Tudo sobre o tipo booleano (bool).
Tudo sobre o tipo inteiro (int).
Tudo sobre o tipo dos reais (float).

Início
Vamos começar com um problema que envolve os tipos bool e int.
Problema dos números perfeitos
Enunciado do problema
Desenvolva um programa que diga se um número inteiro positivo é perfeito, ou seja se é igual à soma dos seus divisores próprios. Por exemplo, 6 é perfeito: 6 = 1 + 2 + 3.
Solução
Desenvolvida ao vivo. Eis a versão final:
def is_perfect(n: int) -> bool:
    """ Check for perfect number.
        Precondition: n > 0
    """
    sum = 0
    for d in range(1,n,1):
        if n % d == 0:      # d é divisor?
            sum += d
    return n == sum

def main() -> None:
    x = int(input("Introduza um número inteiro positivo: "))
    if not (x > 0):
        print("Argumento inválido")
    else:
        print(f"is_perfect({x}) = {is_perfect(x)}")

main()
Tipos
Nesta secção vamos clarificar a noção de tipo e introduzir algum vocabulário novo.
Revisão
A generalidade das linguagens de programação são tipificadas. O Python não é exceção.
Uma linguagem ser tipificada, significa que os valores da linguagem estão organizados em categorias lógicas chamadas tipos. Cada valor tem um tipo específico. Por exemplo, em Python, o valor 1 tem o tipo inteiro (int).

Existem linguagens em que cada variável (e cada parâmetro) tem um tipo específico e só pode referir valores desse tipo, mas tal não acontece em Python.

Os tipos predefinidos que iremos usar em IPCE são estes:

Tipo booleano: bool
Tipos numéricos: int, float, complex
Tipo string: str
Tipos de sequências: tuplo, range, list
Outros tipos: dict, set
O conceito de tipo
Quando pensamos no tipo dos inteiros em Python, pensamos essencialmente no conjunto de todos os valores inteiros, {..., -2, -1, 0, 1, 2, ...}. Concorda?
No entanto, esta caracterização do tipo dos inteiros está incompleta.

Em geral, um tipo caracteriza-se por três elementos:

Um conjunto de valores - esta parte já sabíamos;
Um conjunto de literais - notação para representar os valores;
Um conjunto de operações - operações que envolvem os valores desse tipo.
Por exemplo, o tipo dos inteiros caracteriza-se por:
Um conjunto de valores matemáticos: {..., -2, -1, 0, 1, 2, ...}
Um conjunto de literais: {..., -2, -1, 0, 1, 2, ...}
Um conjunto de operações: {+, -, *, //, %, int, ...}
Se as duas primeiras linhas da lista anterior lhe fizerem confusão, repare que um valor e um literal são coisas diferentes. Usando o zero como exemplo:
Podemos falar do conceito matemático de número inteiro 0, definido como o número de elementos do conjunto vazio;
E podemos falar da notação escrita 0 (um literal), que é um símbolo que escrevemos para representar o valor matemático 0.
O zero matemático é sempre o mesmo, mas veja como ele era escrito antigamente - usando diferentes literais, respetivamente na Babilónia, China, Índia e América Central:



Anotações de tipo
O Python suporta anotações de tipo (type hints) nos cabeçalhos de funções (e também na definição de variáveis).
As anotações de tipo são ignoradas pelo Python. Destinam-se a documentar os programas e a torná-los mais fáceis de perceber pelo leitor humano.

Em IPCE, usamos anotações de tipo no cabeçalho das funções, para indicar o tipo de cada parâmetro e o tipo do resultado. Não usaremos anotações de tipo na definição de variáveis.

Anotações de tipo no cabeçalho das funções, são muito importantes para percebermos os objetivos de cada função.

Eis um exemplo duma função com anotações de tipo no cabeçalho:

def hypotenuse(cathetus1: float, cathetus2: float) -> float:
    """ Length of the hypotenuse of a right triangle.
        Precondition: cathetus1 > 0 and cathetus2 > 0
    """
    return math.sqrt(cathetus1 ** 2 + cathetus2 ** 2)
No caso duma função não ter resultado (por não fazer return), usa-se a anotação None:

def hello(your_name: str) -> None:
    print(f"Hello {your_name}!")
Tipo booleano
O tipo booleano - bool - serve para trabalharmos com as noções de verdade e falsidade nos nossos programas. Precisamos desses conceitos para conseguir exprimir certos aspetos da lógica da resolução dos problemas.
As regras matemáticas dos booleanos são bem conhecidas. George Boole, em meados do séc. XIX, definiu uma álgebra para lidar com este assunto.

O tipo dos booleanos já tem aparecido nos nossos programas. Concretamente, em cada instrução if testamos sempre uma expressão booleana (condição) para escolher o ramo do if a seguir.

O seguinte exemplo, que tem aparecido repetidamente, inclui uma função booleana e inclui um if que testa uma condição:

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if is_leap_year(year):
        return 366
    else:
        return 365
Definição
Para descrever completamente o tipo dos booleanos temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos booleanos
Valores matemáticos
Em matemática o conjunto dos valores booleanos só tem dois valores e costuma ser assim escrito:
𝔹 = { F, T }
Literais
Em Python, os dois literais correspondentes aos dois valores lógicos escrevem-se assim:

False
True
Tratam-se de duas palavras reservadas na linguagem.
Estes dois nomes são verdadeiras constantes. Mas infelizmente o Python não segue aqui a convenção de as escrever usando só letras maiúsculas.

Para exemplificar, eis a definição de duas variáveis usando valores booleanos:

a = False
b = True
Operações booleanas
As operações mais importantes ligadas ao tipo bool são as seguintes:
Operador	Descrição	Associatividade
< <= > >=	Operadores relacionais	esquerda
== !=	Operadores relacionais	esquerda
in, not in	Pertença a contentor	esquerda
not	Operador lógico	direita
and	Operador lógico	esquerda
or	Operador lógico	esquerda
bool	Função de conversão	
As seis operações relacionais permitem comparar valores de vários tipos, sendo o resultado das operações booleano. Por exemplo, o valor da expressão 5 > 7 é False.

As duas operações de pertença a um contentor (container) aplicam-se a um valor e um contentor, sendo o resultado das operações booleano.
[Um contentor é um objeto que pode conter outros objetos. Exemplos de contentores: conjunto, lista, tuplo, range (intervalo), string, etc. Este é um tema para aulas futuras.]

As três operações lógicas aplicam-se a booleanos e produzem booleanos. Recorde a semântica destes operadores: aqui.

Eis exemplos de expressões que usam os operadores booleanos que foram referidos. Examine com especial atenção as expressões que usam o operador in.

a > b                  # testa se a é maior do que b
a != b                 # testa se a e b são diferentes
x in {1,2,3}           # testa se o valor de x pertence ao conjunto {1,2,3} 
"bc" in "abcd"         # testa se a 1ª string é uma substring da 2ª string 
1 in range(0,10,2)     # testa se o valor 1 ocorre na sequência de valores representada pelo range 
a > b and c > d        # testa se duas desigualdades são verdadeiras ao mesmo tempo
a > b or c > d         # testa pelo menos uma de duas desigualdades é verdadeira
is_leap_year(2000) and not is_leap_year(2001) 
                       # testa se 2000 é bissexto e 2001 não é bissexto
Avaliação de expressões booleanas
A avaliação dos operadores booleanos binários and e or é feita da esquerda para a direita.
Se a subexpressão da esquerda já permitir determinar o valor do resultado, então a subexpressão da direita não chega a ser avaliada!

Por exemplo, as duas expressões abaixo só avaliam a subexpressão da esquerda:

is_leap_year(1999) and is_leap_year(2000)

is_leap_year(2000) or is_leap_year(1999)
No caso do operador unário not a avaliação é feita da direita para a esquerda. Por exemplo, o valor da expressão abaixo (com quatro nots) é True:
not not not not is_leap_year(2000)
O Python olha para a expressão anterior da seguinte forma:
not (not (not (not is_leap_year(2000))))
Conversões booleanas
Está disponível a função predefinida bool, com um parâmetro. Esta função converte qualquer valor para booleano.
Exemplos:

bool(0)                 # False
bool(1)                 # True
bool("olá")             # True
bool("")                # False
bool(0.0)               # False
bool(0.1)               # True
bool([])                # False
bool([0])               # True
Basicamente, o resultado é False quando o argumento é: zero, a string vazia, um conjunto vazio, ou uma lista vazia. Caso contrário o resultado é True.
Num contexto em que o Python está à espera dum valor booleano, por exemplo no contexto da condição dum if, se a expressão usada não for booleana, o Python aplica automaticamente a conversão da função bool.

Regras de estilo
Existem duas regras de estilo ligada aos booleanos.
A primeira regra diz:

Não se devem usar as constantes False e True desnecessariamente nas expressões booleanas.
Violar esta regra significa complicar desnecessariamente, perdendo legibilidade.
Eis três exemplos de mau estilo:

if (a > b) == True:
    ....

if is_leap_year(y) == True:
    ....

def is_leap_year(year: int) -> bool:
    if (year % 4 == 0 and year % 100 != 0) or year % 400 == 0:
        return True
    else
        return False
Eis a correção:
if a > b:
    ....

if is_leap_year(y):
    ....

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0
A segunda regra diz:
Nunca aproveitar a conversão automática que o Python faz em alguns contextos usando implicitamente a função bool.
Nunca devemos aproveitar a conversão automática. Nos contextos onde se espera uma expressão booleana, devemos usar mesmo uma expressão booleana. Não gostamos da conversão automática porque confunde o leitor do código.

Por exemplo, nunca devemos usar o número 1 em vez de True e o 0 em vez de False.

Desenvolvimento duma função booleana
Enunciado do problema
Desenvolva uma função para testar se um valor inteiro positivo é primo.
Solução
Há várias formas de abordar este problema. Eis uma possibilidade (não muito eficiente, mas simples):
def count_divisors(n: int) -> int:
    """ How many divisors?
        Precondition: n > 0
    """
    count = 0
    for i in range(1,n+1,1):
        if n % i == 0:
            count += 1
    return count

def is_prime(n: int) -> bool:
    """ Check if a positive integer is a prime number.
        Precondition: n > 0
    """
    return count_divisors(n) <= 2
Estude o código e explique qual foi a ideia usada para resolver o problema.

Tipo inteiro
O tipo inteiro - int - serve para trabalharmos com valores inteiros. Há muitos problemas que se resolvem usando números inteiros.
Um aspeto marcante dos inteiros em Python é não estarem sujeitos a um valor máximo. Por exemplo, testando no interpretador:

>>> 2**1000
107150860718626732094842504906000181056140481170553360744375038837035105112493
612249319837881569585812759467291755314682518714528569231404359845775746985748
039345677748242309854210746050623711418779541821530464749835819412673987675591
65543946077062914571196477686542167660429831652624386837205668069376
A seguinte função, que já conhecemos bem, trabalha com valores inteiros:
def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1
    for i in range(1,n+1,1):
        fact = fact * i
    return fact
Definição
Para descrever completamente o tipo dos inteiros temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos inteiros
Valores matemáticos
Em matemática o conjunto dos valores inteiros tem infinitos valores e podemos tentar representá-lo da seguinte maneira:
ℤ = {..., -2, -1, 0, 1, 2, ...}
Literais
Em Python, os literais correspondentes aos valores inteiros usam o sistema numérico indo-arábico: são usados dez dígitos, e a posição dum dígito dentro dum número determina a contribuição do dígito para o valor representado.
Eis alguns exemplos de literais inteiros decimais:

12345
0
-444444
545234523523
Também há literais inteiros em hexadecimal (base 16), octal (base 8), binário (base 2). Exemplos:

0xA           # vale 10
0x10AF86      # vale 1093510
0x100         # vale 256

0o77          # vale 63
0o100         # vale 64

0b11111111    # vale 255
0b100         # vale 4
Operações inteiras
As operações mais importantes ligadas ao tipo int são as seguintes:
Operador	Descrição	Associatividade
**	Expoente	direita
* // %	Multiplicação / Divisão / Resto	esquerda
+ -	Adição / Subtração	esquerda
int	Conversão	
Todas estas operações podem ser usadas com argumentos inteiros para produzir resultados inteiros.
Mas há outras operações que podem ser aplicadas a inteiros, embora não retornem inteiros: (1) as operações relacionais (já vistas) permitem comparar inteiros e retornam um booleano; (2) a divisão real (/) pode ser aplicada a dois inteiros e o resultado é um número real.

Numa expressão, a avaliação de cada operação inteira binária é feita da esquerda para a direita, mas conhecer este facto não tem implicações práticas para nós.

Conversões inteiras
Está disponível a função predefinida int. Esta função sabe converter alguns valores do Python para inteiro, mas é relativamente limitada nos tipos de argumentos que aceita.
Note que, quando aplicada a um número real, a função int arredonda no sentido do zero.

Exemplos:

int(123)               # 123
int(False)             # 0
int(True)              # 1
int("1234")            # 1234
int("olá")             # erro
int({1,2,3})           # erro
int(0.0)               # 0
int(0.5)               # 0
int(0.99)              # 0
int(-0.99)             # 0
int(-1.99)             # -1
int(1e20)              # 100000000000000000000
Leitura de inteiros
Eis um exemplo que mostra como se faz a leitura dum inteiro a partir do teclado:
x = int(input("Por favor introduza um valor inteiro: "))
input é uma função predefinida que lê uma string a partir do teclado. Esta função tem como argumento um prompt, ou seja uma string que pede o valor ao utilizador. O prompt é opcional, mas a função raramente é usada sem ele.
int é a função de conversão descrita na secção anterior. Neste caso, converte a string lida para inteiro. Se a string lida não for um literal inteiro válido, ocorre um erro de execução.

Escrita de inteiros
Para escrever um valor inteiro basta usar a função predefinida print. Por exemplo:
>>> print(123)
123
>>> x = 456
>>> print(x)
456
Mas isto não chega. Controlar o alinhamento é necessário para produzir tabelas e isso faz falta aos engenheiros e cientistas.
Para controlar a escrita dum inteiro precisamos de usar f-strings, também conhecidas como strings de formatação (já as começamos a usar na aula teórica 02). Numa string de formatação podem ocorrer expressões entre chavetas - e essas expressões são avaliadas e os seus resultados inseridos na string. Por exemplo:

>>> x = 456
>>> print(f"ola {x} olé {(x+1)/x} oli")
ola 456 olé 1.0021929824561404 oli
Para controlar o alinhamento e outros aspetos da apresentação dos inteiros, precisamos de acrescentar no final das expressões o símbolo ":", seguido dum especificador de formato.

Alinhar à direita é a situação mais frequente e a mais simples de concretizar: basta usar como especificador de formato um número inteiro que indica a largura do campo usado para o alinhamento.

Exemplo: mostramos as primeiras 21 potências de 10, alinhadas à direita num campo de largura 16:

>>> for i in range(0,21,1):
...     pow = i**10
...     print(f"{i:2} {pow:16}")
... 
 0                0
 1                1
 2             1024
 3            59049
 4          1048576
 5          9765625
 6         60466176
 7        282475249
 8       1073741824
 9       3486784401
10      10000000000
11      25937424601
12      61917364224
13     137858491849
14     289254654976
15     576650390625
16    1099511627776
17    2015993900449
18    3570467226624
19    6131066257801
20   10240000000000
O programa abaixo ilustra mais algumas possibilidades oferecidas pelos especificadores de formato:
def print_int(n: int) -> None:
    """ Testing some integer format specifiers """
    print(n)              # alinhado à esquerda em decimal
     
    print(f"{n: >20d}")    # alinhado à direita em decimal num campo de 20 carateres
    print(f"{n:_>20d}")    # alinhado à direita em decimal num campo de 20 carateres
    print(f"{n:#>20d}")    # alinhado à direita em decimal num campo de 20 carateres
    print(f"{n:#>20b}")    # alinhado à direita em binário num campo de 20 carateres
    print(f"{n:#>20x}")    # alinhado à direita em hexadecimal num campo de 20 carateres
    print(f"{n:#>20o}")    # alinhado à direita em octal num campo de 20 carateres
   
    print(f"{n: <20d}")    # alinhado à esquerda em decimal num campo de 20 carateres
    print(f"{n:_<20d}")    # alinhado à esquerda em decimal num campo de 20 carateres
    print(f"{n:#<20d}")    # alinhado à esquerda em decimal num campo de 20 carateres
    print(f"{n:#<20b}")    # alinhado à esquerda em binário num campo de 20 carateres
    print(f"{n:#<20x}")    # alinhado à esquerda em hexadecimal num campo de 20 carateres
    print(f"{n:#<20o}")    # alinhado à esquerda em octal num campo de 20 carateres
    
    print(f"{n: ^20d}")    # centrado em decimal num campo de 20 carateres
    print(f"{n:_^20d}")    # centrado em decimal num campo de 20 carateres
    print(f"{n:#^20d}")    # centrado em decimal num campo de 20 carateres
    print(f"{n:#^20b}")    # centrado em binário num campo de 20 carateres
    print(f"{n:#^20x}")    # centrado em hexadecimal num campo de 20 carateres
    print(f"{n:#^20o}")    # centrado em octal num campo de 20 carateres
    
    print(f"{n:20}")       # abreviatura a f"{n: >20d}"

def main() -> None:
    print_int(12345)

main()
Eis o output do programa:
12345
               12345
_______________12345
###############12345
######11000000111001
################3039
###############30071
12345               
12345_______________
12345###############
11000000111001######
3039################
30071###############
       12345        
_______12345________
#######12345########
###11000000111001###
########3039########
#######30071########
               12345
Tipo dos reais
O tipo real - float - serve para trabalharmos com valores reais. Há muitos problemas que se resolvem usando números reais, por exemplo problemas da Física.
O Python, usa diretamente os números reais suportados no hardware. Atualmente, o padrão em vigor chama-se IEEE 754 e cada número real é representado usando 64 bits.

Os números reais incluem os inteiros, os números racionais (frações) e os restantes números chamam-se irracionais (como por exemplo a raiz de 2). A seguinte função, que já conhecemos bem, trabalha com valores reais:

import math

def hypotenuse(cathetus1: float, cathetus2: float) -> float:
    """ Length of the hypotenuse of a right triangle.
        Precondition: cathetus1 > 0 and cathetus2 > 0
    """
    return math.sqrt(cathetus1 ** 2 + cathetus2 ** 2)
Definição
Para descrever completamente o tipo dos reais temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos inteiros
Valores matemáticos
Em matemática o conjunto dos valores reais é constituído por infinitos valores e costuma ser representado da seguinte maneira, singela:
ℝ
Em Python, os literais correspondentes aos valores reais suportam notação científica. Nos casos mais complicados, um literal real tem uma parte inteira e uma parte decimal separadas por um ponto, seguido da letra e e ainda um inteiro que representa uma potência de 10:
Literais
Eis alguns exemplos de literais reais:

12.3e56
1e-2
3.14159
12.0
0.0
Quando usamos o 'e' num literal, diz-se que estamos a usar notação científica.

Os literais inteiros não são considerados literais reais, mas em alguns contextos eles são automaticamente convertidos em reais. Por exemplo, na divisão abaixo, os dois inteiros são automaticamente convertidos em reais antes de se efetuar a operação de divisão.

5/7
Operações
Eis as operações primitivas do Python mais importantes que estão ligadas ao tipo real:
Operador	Descrição	Associatividade
**	Expoente	direita
* / // %	Multiplicação / Divisões / Resto	esquerda
+ -	Adição / Subtração	esquerda
round	Arredondamento para inteiro	
float	Conversão para real	
Talvez surpreenda aparecerem na lista // e %. Provavelmente nunca iremos usar estes operadores aplicados a números reais. Em todo o caso podemos explicar: A primeira operação efetua a divisão real e depois arredonda para baixo, para inteiro; a segunda retira o segundo argumento do primeiro argumento o número máximo de vezes, e o que sobrar é o resultado.
Há outras operações que podem ser aplicadas a diversos tipos, incluindo os reais: por exemplo, as operações relacionais permitem comparar dois reais e retornam um booleano.

Módulo math
A maioria das funções do módulo math são sobre números reais. Ganhamos acesso a essas funções, escrevendo no nosso programa:

import math
O módulo é vasto a apresentam-se aqui apenas cerca de um terço delas - provavelmente as mais usadas:

Função	Descrição
pi inf
e nan	constantes
cos	coseno
sin	seno
tan	tangente
acos	arco-coseno
asin	arco-seno
atan	arco-tangente
cosh	coseno hiperbólico
sinh	seno hiperbólico
tanh	tangente hiperbólica
exp	exponencial
log	logaritmo natural
log10	logaritmo de base 10
pow	power (x elevado a y)
sqrt	raiz quadrada
ceil	teto (arredonda para cima)
floor	chão (arredonda para baixo)
fabs	valor absoluto
fmod	resto da divisão
isclose	testa se dois reais diferem menos de 1e-09
A função round
Uma operação primitiva importante que não tem operador associado é a função round, que arredonda um número real para o inteiro mais próximo.

Quando se efetua o arredondamento, no caso especial em que a parte decimal é exatamente 0.5, o Python usa a regra rounding ties to even que significa que, nos casos de empate, se dá preferência aos números pares. Este arredondamento é diferente do que se usa nas notas dos alunos.

Exemplos:

round(5.4)                # 5
round(5.6)                # 6
round(5.5)                # 6 rounding ties to even
round(6.5)                # 6 rounding ties to even
round(7.5)                # 8 rounding ties to even
Se precisarmos do arredondamento que se usa nas notas dos alunos (always round 0.5 up), estas são as possibilidades mais simples:
math.floot(nota + 0.5)
int(nota + 0.5)
A função round também pode ser usada para arredondar para um certo número de casas decimais. Exemplos:

round(3.14159, 3)         # 3.142
round(3.14149, 3)         # 3.141
round(3.1415, 3)          # 3.142
Conversões reais
Está disponível a função predefinida float, com um parâmetro. Esta função sabe converter alguns valores do Python para float, mas é relativamente limitada nos tipos de argumentos que aceita.
Alguns exemplos:

float(123.5)             # 123.5
float(0)                 # 0.0
float(False)             # 0.0
float(True)              # 1.0
float("1234")            # 1234.0
float("12.3e-30")        # 1.23e-29
float("olá")             # erro
float({1,2,3})           # erro
float(9999999999999999999999999999999999999999999999999999999999999999999999) # 1e+70
Inexatidão dos reais em Python
Em matemática, qualquer intervalo [a,b] de números reais, com a<b, contém um número infinito de valores. Contudo, um valor de tipo float implementa-se usando um número limitado de bits (64 bits).
Com um número limitado de bits não conseguimos representar infinitos valores distintos, claro!

Por isso, a maioria dos valores de tipo real têm de ser representados por valores aproximados! Na maioria dos casos o valor exato não está disponível, sendo necessário usar a aproximação mais próxima que estiver disponível.

Outro problema é que se o valor tiver uma magnitude demasiado pequena (muito perto do zero) ou demasiado grande (para os lados do infinito), então não pode ser representado de todo, nem sequer de forma aproximada, pois sai fora dos limites da representação.

O Python permite representar números reais com uma precisão de 15 dígitos e magnitude entre 1e-308 e 1e308. Eis dois valores extremos:

1.23456789012345e-308
1.23456789012345e308
Quando se trabalha com números reais, temos de aceitar que os valores e os resultados das expressões raramente serão exatos:

A partir do 15º dígito, o normal é que todos os dígitos sejam simples "ruído".
Adicionalmente, também temos de reconhecer que, quando se fazem muitas contas sucessivas com reais, os erros de aproximação vão-se acumulando e o "ruído" aumenta. Podemos ficar com apenas 13 ou 12 dígitos de precisão, ou pior ainda.
Mesmo assim, a precisão e magnitude dos números reais do Python são amplamente generosas para permitir resolver problemas de física e engenharia. As medições efetuadas no mundo físico são mais imprecisas...

Vamos examinar com detalhe a imprecisão de alguns reais. No exemplo seguinte, escrevemos alguns reais com 30 casas decimais. Vemos que o valor 1/2 é um caso raro que é representado de forma exata, o que não admira porque os reais são representados internamente em binário. Já o valor 1/3 não é representado de forma exata.

>>> print(f"{1/2:.30f}")
0.500000000000000000000000000000
>>> print(f"{1/3:.30f}")
0.333333333333333314829616256247
No seguinte exemplo vamos criar uma situação em que contas sucessivas "amplificam" um erro minúsculo inicial. Começamos com o valor 1/3. O programa decompõe o valor 1/3 em 16 partes iguais e depois adiciona essas 16 partes. Matematicamente, o programa deveria escrever "iguais", mas na realidade escreve "diferentes". (Usando só 8 partes, o programa já escreveria "iguais".)

def partition_and_sum(f: float) -> float:
    PARTS = 16
    small = f / PARTS
    total = 0.0
    for i in range(0,PARTS,1):
        total += small
    return total

def main():
    value = 1/3
    if partition_and_sum(value) == value:
        print("iguais")
    else:
        print("diferentes")

main()
Mas nem precisamos dum exemplo tão complicado. Há casos em que não precisamos de contas sucessivas e basta uma simples conta.
Veja a comparação abaixo. Os dois lados da comparação não são exatamente iguais em Python.

>>> 0.1+0.2 == 0.3
False
Realmente não são. Vamos escrever com 30 casas decimais. Veja:
>>> print(f"{0.1+0.2:.30f} ---- {0.3:.30f}")
0.300000000000000044408920985006 ---- 0.299999999999999988897769753748
Como sobreviver no mundo imperfeito dos reais dos computadores? Na verdade, não é difícil. Basta nunca esquecermos que, se precisarmos de escrever uma igualdade entre reais, devemos usar a operação math.isclose que verifica se a distância entre os dois valores é inferior a 1e-09.
>>> math.isclose(0.1+0.2, 0.3)
True
Valores reais especiais
Como vimos atrás, a magnitude dos valores reais tem limites em Python. O intervalo suportado é aproximadamente este: [1e-308, 1e308].
Quando se começam a fazer contas com valores próximos dos limites, os seguintes valores reais especiais podem começar a aparecer no resultado das nossas contas:

inf    -inf   nan
O primeiro indica infinito, o segundo indica menos infinito, e o terceiro indica not a number. O valor nan é gerado quando se tentam efetuar certas operações com argumentos inválidos, por exemplo fazer infinito menos infinito.
Exemplo em que forçamos o aparecimento de math.inf: A função repeat faz multiplicações sucessivas e numa dada altura supera o limite de 1e308 e atinge o infinito:

def repeat(f: float, n: int) -> float:
    mult = 1
    for i in range(0, n, 1):
        mult *= f
    return mult

def main():
    print(repeat(99.9, 1))
    print(repeat(99.9, 10))
    print(repeat(99.9, 100))
    print(repeat(99.9, 150))
    print(repeat(99.9, 180))
    print(repeat(99.9, 200))

main()
Eis o que o programa escreve:
99.9
9.900448802097491e+19
9.047921471137151e+199
8.606433826830446e+299
inf
inf
Escrever diretamente o valor infinito num programa não tem sentido prático. Mas se quisermos apesar de tudo fazer experiências, precisamos de usar as constantes definidas no módulo math.

Se o infinito aparecer acidentalmente na execução dum programa, durante os cálculos, o que acontece quando se começam a fazer contas com ele? Veja estes exemplos:

math.inf + 1.0                  # inf
math.inf * 2.0                  # inf
math.inf + math.inf             # inf
math.inf - math.inf             # nan
math.inf * math.inf             # inf
math.inf / math.inf             # nan
math.inf ** math.inf            # inf
(-1) * math.inf                 #-inf
1.0/math.inf                    # 0.0
1.0/0.0                         # erro
math.nan + 5.0                  # nan
Leitura de reais
Eis um exemplo que mostra como se faz a leitura dum real a partir do teclado:
x = float(input("Por favor introduza um valor real: "))
input é uma função predefinida que lê uma string a partir do teclado. Esta função tem como argumento um prompt, uma string que pede o valor ao utilizador. O prompt é opcional, mas a função raramente é usada sem ele.
float é a função de conversão descrita mais atrás. Neste caso, converte a string lida para real. Se a string lida não for um literal real válido, ocorre um erro de execução.

Escrita de reais
Para escrever um valor real usando o formato por omissão, basta usar a função predefinida print de forma direta. Por exemplo:
>>> print(123.00)
123.0
>>> x = 456.345
>>> print(x)
456.345
Para escrever um valor real com um certo número de casas decimais, temos de usar uma f-string especial. Por exemplo:
>>> r = 123.456789
>>> print(f"{r:.3f}")     # três casas decimais
123.457
>>> print(f"{r:.5f}")     # cinco casas decimais
123.45679
Controlar o alinhamento também é necessário para produzir tabelas e isso faz falta aos engenheiros e cientistas.

Nas f-strings, as regras de alinhamento dos reais são as mesmas dos inteiros. Mas há dois ingredientes novos: (1) indicação do número de casas decimais a escrever; (2) o estilo do real. Os estilos são: e - notação científica; f - ponto decimal simples; g - mostra os dígitos significativos indicados usando uma das anteriores (a que for mais compacta).

def print_float(v: float):
    print(v)                 # alinhado à esquerda
    
    print(f"{v: >20.6e}")    # notação científica, alinhado à direita, seis casas decimais, campo de 20 carateres
    print(f"{v: >20.6f}")    # notação de ponto fixo, alinhado à direita, seis casas decimais, campo de 20 carateres
    print(f"{v: >20.6g}")    # notação geral, alinhado à direita, máximo seis dígitos significativos, campo de 20 carateres
    
    print(f"{v: <20.6e}")    # notação científica, alinhado à esquerda, seis casas decimais, campo de 20 carateres
    print(f"{v: <20.6f}")    # notação de ponto fixo, alinhado à esquerda, seis casas decimais, campo de 20 carateres
    print(f"{v: <20.6g}")    # notação geral, alinhado à esquerda, máximo seis dígitos significativos, campo de 20 carateres
    
    print(f"{v: ^20.6e}")    # notação científica, centrado, seis casas decimais, campo de 20 carateres
    print(f"{v: ^20.6f}")    # notação de ponto fixo, centrado, seis casas decimais, campo de 20 carateres
    print(f"{v: ^20.6g}")    # notação geral, centrado, máximo seis dígitos significativos, campo de 20 carateres

def main():
    print_float(12345.678)

main()
Eis o output do programa:
12345.678
        1.234568e+04
        12345.678000
             12345.7
1.234568e+04        
12345.678000        
12345.7             
    1.234568e+04    
    12345.678000    
      12345.7       
