
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 7a (28/out/2026)

Definição de novos tipos.
Classes de dados (data classes).
Definição de novos tipos
A definição de novos tipos permite-nos introduzir conceitos que se apresentam de forma natural e intuitiva!

Imagine que queremos fazer um programa no qual precisamos de representar o conceito de Pessoa.

Para representar pessoas, gostaríamos de dispor de objetos com as seguintes propriedades, no mínimo:

nome
ano de nascimento
morada
endereço de email
A maneira certa e elegante de resolver de forma simples este problema em Python é através da definição duma classe de dados (data class).
As classes de dados vão ser a novidade introduzida desta aula!

Definição de novos tipos em Python
Em Python podemos introduzir novos tipos de duas maneiras.

Sinónimos de tipos
A primeira forma usa a construção type. Não cria nenhum tipo realmente novo. Permite apenas dar um nome conveniente a um tipo que já existe.

Veja este exemplo:

type IList = list[int]
Definição de dataclass
A segunda forma usa o mecanismo das data classe e introduz um tipo genuinamente novo, suportanto objetos com as propriedades desejadas.

Classes de dados
Cada classe de dados, definida por nós, introduz um novo tipo de objetos.

Ao definir uma classe de dados, indicamos quais são os atributos dos seus objetos - todos esses objetos têm a mesma estrutura, podendo diferir apenas nos valores guardados internamente.

Quando se gera um objeto usando uma classe de dados, diz-se que se faz uma instanciação dessa classe de dados. Os objetos gerados por uma determinada classe de dados, dizem-se instâncias dessa classe de dados.

Existe uma sintaxe própria para definir classse de dados, que iremos ver já a seguir.

Numa classe de dados, os elementos de dados chamam-se atributos.

Definição e uso da classe de dados Person
Eis a nossa definição da classe de dados Person, com quatro atributos - significa que cada um dos nossos objetos conterá esses quatro atributos.
from dataclasses import dataclass

@dataclass
class Person:
    name: str
    year: int
    address: str
    email: str = ""
O Python define automaticamente uma função construtora com o mesmo nome da classe de dados. Essa função recebe os valores dos atributos, para construir cada objeto. Veja a função construtora a ser usada para criar uma pessoa, a maria:
maria = Person("Maria", 1990, "Coimbra")
Se quisermos, também podemos indicar o email da maria. Mas isso é opcional, porque indicámos um valor por omissão quando definimos o atributo email.
maria = Person("Maria", 1990, "Coimbra", "maria@campus.fct.unl.pt")
Agora um exemplo maior. A seguinte função, cria duas pessoas (a maria e o joão) e escreve-as no ecrã. Observe com atenção o formato usado para a escrita.
def example():
    maria = Person("Maria", 1990, "Coimbra")
    joao = Person("João", 1970, "Lisboa")
    print(maria)
    print(joao)
    
>>> example()
Person(name='Maria', year=1990, address='Coimbra', email='')
Person(name='João', year=1970, address='Lisboa', email='')
Uso dos atributos
Dado um objeto, por exemplo a maria, existe uma sintaxe especial, para aceder aos seus atributos: por exemplo maria.nome.
A seguinte função faz algumas experiências com os atributos da maria. Procure a linha em que a maria vai morar para o Algarve.

def example2():
    maria = Person("Maria", 1990, "Coimbra")
    print(maria.name)
    print(maria.year)
    maria.address = "Faro"
    print(maria)
    
>>> example2()
Maria
1990
Person(name='Maria', year=1990, address='Faro', email='')
Funções da classe de dados
Normalmente, equipa-se cada classe de dados com um conjunto de funções. Essas funções costumam, ser escritas logo a seguir à definição da classe de dados.
Eis o nosso exemplo, agora também enriquecido com um conjunto de funções:

from dataclasses import dataclass

@dataclass
class Person:
    name: str
    year: int
    address: str
    email: str = ""

def person_as_str(p: Person) -> str:
    """ Convert person to string. """
    return f"{p.name} nasceu em {p.year} e mora em {p.address}" 

def person_get_name(p: Person) -> str:
    """ Get person name. """
    return p.name 

def person_have_same_name(p1: Person, p2: Person) -> bool:
    """ Check if two persons have the same name. """
    return person_get_name(p1) == person_get_name(p2)

def person_input() -> Person:   # este não tem self. Porquê?
    """ Interactive person reading. """
    name = input("NAME: ")
    year = int(input("YEAR: "))
    address = input("ADDRESS: ")
    return Person(name, year, address)
Acrescentámos as seguintes funções:
person_as_str - converte uma pessoa para string. A função str já converte para string, mas aqui estamos a especificar uma conversão diferente.
person_get_name - obtém o nome duma pessoa.

person_have_same_name - testa se duas pessoas têm o mesmo nome.

person_input - lê uma pessoa a partir do teclado. A função pede o nome, o ano e o endereço; no final cria uma pessoa constituída pelos três valores lidos.

Utilização da classe de dados Person
Agora, uma pequena função main para testar as funcionalidades da classe de dados Person:

def main():
    maria = Person("Maria", 1990, "Coimbra")
    joao = Person("João", 1970, "Lisboa")
    print(person_as_str(maria))
    print(person_as_str(joao))
    print("---")
    print(person_get_name(maria))
    print(person_have_same_name(maria, joao))
    print("---")
    q = person_input()
    print(person_as_str(q))
    print(q)

>>> main()
Maria nasceu em 1990 e mora em Coimbra
João nasceu em 1970 e mora em Lisboa
---
Maria
False
---
NAME: Rita
YEAR: 1999
ADDRESS: Faro
Rita nasceu em 1999 e mora em Faro
Person(name='Rita', year=1999, address='Faro', email='')
Mais exemplos de classes de dados
A seguinte classe de dados representa números racionais (frações):

@dataclass
class Rational:
    numerator: int
    denominator: int  
A seguinte classe de dados representa pontos num espaço a duas dimensões:
@dataclass
class Point:
    x: float  
    y: float
A seguinte classe de dados representa parábolas, através dos coeficientes dum polinómio de 2º grau:
@dataclass
class Parabola:  
    a: float
    b: float
    c: float
A seguinte classe de dados representa círculos:
@dataclass
class Circle:
    center: Point  
    radius: float
A seguinte classe de dados representa datas de calendário:
@dataclass
class Date:
    day: int
    month: int  
    year: int
A seguinte classe de dados representa triângulos:
@dataclass
class Triangle:  
    a: Point
    b: Point
    c: Point
A seguinte classe de dados representa polígonos gerais, com qualquer número de vértices:
@dataclass
class Polygon:
    vertices: list[Point]  
Programação baseada em classes de dados
Modelação duma realidade com riqueza de entidades
Cada tipo de entidade que seja identificada no problema deve ser programada numa classe de dados distinta. Desta forma, conseguimos modelar de forma conveniente cada aspeto da realidade com que queremos trabalhar. Os conceitos envolvidos ficam explicitados e o programa fica bem organizado.

Considere o enunciado do Problema da pastelaria:

Precisamos dum programa para gerir uma pastelaria. A pastelaria tem um Stock corrente de bolos. O Stock é constituído por várias unidades de cada variedade de bolo. Também queremos saber quais são os ingredientes usados para fazer cada variedade de bolo, e qual a percentagem de cada ingrediente na sua constituição.

O programa deve oferecer diversas operações: por exemplo, descobrir uma variedade de bolo no Stock, validar uma variedade de bolo (verificar se o total das percentagens dá 100%), descobrir o ingrediente principal duma variedade de bolo, efetuar vendas de bolos.

Para resolver este problema, convém introduzir:

a classe de dados Ingredient - com atributos name e percent;
a classe de dados CakeVariety - com atributos name, amount e ingredients;
a classe de dados CakeShop - com atributo stock.
Veja o exercício 51 das aulas práticas.

#









Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 07b (28/out/2026)
Objetos em Python.
Programação baseada em classes de dados

Objetos em Python
Em Python, todos os elementos de dados com que trabalhamos são designados como objetos.
Já trabalhámos com numerosos tipos predefinidos de objetos. Só para lembrar: bool, int, float, complex, str, list, tuple, range, set, dict.

Também já aprendemos a definir novos tipos de objetos usando classes de dados.

Chegou agora a altura de compreender melhor os objetos do Python.

O que é um objeto
Um objeto é uma entidade individual, que agrega dados (atributos) e métodos.
Chamamos métodos às funções especiais que fazem parte integrante dos objetos.
Como definir novos tipos de objetos
Em Python, usa-se o mecanismo das classes para definir novos tipos de objetos. Ao definir uma nova classe, podemos especificar os campos de dados e os métodos desse novo tipo de objeto.
Mas não faz parte na matéria de IPCE aprender a definir novas classes. Queremos simplificar a disciplina!

Em IPCE, limitamo-nos às classes de dados, as quais permitem definir novos tipos de objetos com os atributos que desejarmos, mas sem métodos. Em vez de métodos, usaremos funções normais, definidas fora da classe de dados. O mecanismo das classes de dados é muito mais simples do que o mecanismo das classes gerais.

Em IPCE, os métodos aparecem apenas no contexto dos objetos de tipos predefinidos. Também há alguns métodos que o Python insere automaticamente nas nossas classes de dados.

Distinção entre funções normais e métodos
Funções normais
Têm autonomia própria.
Todas as funções que definimos até agora, nas aulas teóricas e nos exercícios das aulas práticas, têm sido funções normais.
A sintaxe que se usa para invocar uma função normal é f(args), por exemplo: hypotenuse(2.34, 44.7).
Métodos
São funções especiais que fazem parte integrante dos objetos.
Em IPCE não iremos definir novos métodos.
A sintaxe que se usa para chamar um método é obj.m(args), por exemplo l.append(4).
Porquê objetos?
Historicamente, a ideia de agregar dados e métodos provou ser muito útil e hoje em dia são muitas as linguagens de programação que suportam o conceito de objeto.
Sintaxe e métodos mágicos
Já vimos a sintaxe usada para aplicar um método a um objeto: Escreve-se:

primeiro o objeto;
depois o operador ponto ".";
depois o nome do método;
finalmente os argumentos entre parêntesis.
Se não existirem argumentos, escrevem-se os parêntesis na mesma.
Eis alguns exemplos de chamadas de métodos para a lista l:

l.append(22)
l.insert(2,99)
l.pop()
l.reverse()
l.sort()
Mas, o que se passa com as seguintes operações sobre lista, que se escrevem sem usar o operador ponto? As funções que estão a ser chamadas não parecem ser métodos... Por exemplo:

l[3]
l+l
l*5
Na verdade, esses casos constituem simples açúcar sintático (i.e. sintaxe mais agradável) para a chamada dum certo número de métodos ditos métodos mágicos.

Veja como o Python traduz a sintaxe "açucarada", que usamos habitualmente, para a sintaxe técnica, usada internamente:

açucarada       básica
l[3]       ---> l.__getitem__(3)
l[3] = 99  ---> l.__setitem__(3,99)
l + l      ---> l.__add__(l)
l * 5      ---> l.__mul__(5)
l += l     ---> l.__iadd__(l)
str(l)     ---> l.__str__()
l == l     ---> l.__eq__(l)
l <> l     ---> l.__ne__(l)
Seria possível programar chamando diretamente os métodos mágicos, mas ninguém gosta de fazer isso. A sintaxe açucarada é mais legível e intuitiva.
Vejamos um exemplo. Nos inteiros, a operação de soma está programada no método mágico __add__. Veja:

>>> v = 5
>>> v + 6
11
>>> v.__add__(6)    # método mágico para a soma
11
Eis algumas correspondências de tradução, para alguns operadores que conhecemos bem:
a-b       --->   a.__sub__(b)
a/b       --->   a.__truediv__(b)
a//b      --->   a.__floordiv__(b)
a%b       --->   a.__mod__(b)
a**b      --->   a.__pow__(b)
a and b   --->   a.__and__(b)
a or b    --->   a.__or__(b)
Há muitas dezenas de métodos mágicos. Eis os nomes da maioria:

'__abs__', '__add__', '__and__', '__bool__', '__ceil__', '__class__',
'__class_getitem__', '__contains__', '__delattr__', '__delitem__',
'__dir__', '__divmod__', '__doc__', '__eq__', '__float__', '__floor__',
'__floordiv__', '__format__', '__ge__', '__getattribute__',
'__getitem__', '__getnewargs__', '__getstate__', '__gt__',
'__hash__', '__iadd__', '__imul__', '__index__', '__init__',
'__init_subclass__', '__int__', '__invert__', '__iter__', '__le__',
'__len__', '__lshift__', '__lt__', '__mod__', '__mul__', '__ne__',
'__neg__', '__new__', '__or__', '__pos__', '__pow__', '__radd__',
'__rand__', '__rdivmod__', '__reduce__', '__reduce_ex__', '__repr__',
'__reversed__', '__rfloordiv__', '__rlshift__', '__rmod__',
'__rmul__', '__ror__', '__round__', '__rpow__', '__rrshift__',
'__rshift__', '__rsub__', '__rtruediv__', '__rxor__', '__setattr__',
'__setitem__', '__sizeof__', '__str__', '__sub__', '__subclasshook__',
'__truediv__', '__trunc__', '__xor__'
Objetos mutáveis e objetos imutáveis
Para terminar a discussão sobre objetos em Python, convém saber que há tipos de objetos mutáveis e tipos de objetos imutáveis:

Por exemplo, as listas são mutáveis. Operações como append alteram diretamente o objeto que é alvo da operação. Basta que exista um método destes, que altere o estado do seu objeto, para o tipo ser considerado mutável.
Por exemplo, os inteiros são imutáveis. Os seus métodos nunca alteram o objeto alvo, sendo gerado um novo inteiro como resultado. Nos inteiros não existe qualquer operação que altere o alvo da operação.
Ver o que está definido dentro dum objeto
Usando a operação dir do Python, podemos observar o que está definido dentro dum objeto. Mas não se preocupe em perceber tudo o que é mostrado.
Por exemplo, para as listas:

>>> dir([1,2,3])
['__add__', '__class__', '__class_getitem__', '__contains__',
 '__delattr__', '__delitem__', '__dir__', '__doc__', '__eq__',
'__format__', '__ge__', '__getattribute__', '__getitem__', '__getstate__',
'__gt__', '__hash__', '__iadd__', '__imul__', '__init__',
'__init_subclass__', '__iter__', '__le__', '__len__', '__lt__',
'__mul__', '__ne__', '__new__', '__reduce__', '__reduce_ex__',
'__repr__', '__reversed__', '__rmul__', '__setattr__', '__setitem__',
'__sizeof__', '__str__', '__subclasshook__', 'append', 'clear', 'copy',
'count', 'extend', 'index', 'insert', 'pop', 'remove', 'reverse', 'sort'] 
Agora para os inteiros:
>>> dir(3)
['__abs__', '__add__', '__and__', '__bool__', '__ceil__', '__class__',
'__delattr__', '__dir__', '__divmod__', '__doc__', '__eq__', '__float__',
'__floor__', '__floordiv__', '__format__', '__ge__', '__getattribute__',
'__getnewargs__', '__getstate__', '__gt__', '__hash__', '__index__',
'__init__', '__init_subclass__', '__int__', '__invert__', '__le__',
'__lshift__', '__lt__', '__mod__', '__mul__', '__ne__', '__neg__',
'__new__', '__or__', '__pos__', '__pow__', '__radd__', '__rand__',
'__rdivmod__', '__reduce__', '__reduce_ex__', '__repr__', '__rfloordiv__',
'__rlshift__', '__rmod__', '__rmul__', '__ror__', '__round__', '__rpow__',
'__rrshift__', '__rshift__', '__rsub__', '__rtruediv__', '__rxor__',
'__setattr__', '__sizeof__', '__str__', '__sub__', '__subclasshook__',
'__truediv__', '__trunc__', '__xor__', 'as_integer_ratio', 'bit_count',
'bit_length', 'conjugate', 'denominator', 'from_bytes', 'imag', 'is_integer',
'numerator', 'real', 'to_bytes']
Agora para uma pessoa:
>>> maria = Person("Maria", 1990, "Coimbra")
>>> dir(maria)
['__annotate_func__', '__annotations_cache__', '__class__', '__dataclass_fields__',
'__dataclass_params__', '__delattr__', '__dict__', '__dir__', '__doc__', '__eq__',
'__firstlineno__', '__format__', '__ge__', '__getattribute__', '__getstate__',
'__gt__', '__hash__', '__init__', '__init_subclass__', '__le__', '__lt__',
'__match_args__', '__module__', '__ne__', '__new__', '__reduce__', '__reduce_ex__',
'__replace__', '__repr__', '__setattr__', '__sizeof__', '__static_attributes__',
'__str__', '__subclasshook__', '__weakref__', 'address', 'email', 'name', 'year']
É interessante observar que, nas classes de dados, o Python insere automaticamente métodos para os operadores relacionais (de comparação). Contudo, muitas vezes esses métodos de comparação não nos interessam e precisamos de definir as nossas próprias funções de comparação.

Programação baseada em classes de dados
Há duas situações em que a utilização de classes de dados faz todo o sentido.
Criar um novo tipo de dados, muito desenvolvido
Por exemplo, faz sentido usar classes de dados para implementar tipos de dados para:

matrizes;
datas de calendário;
números racionais;
etc.
Nos exercícios 49 e 50 das aulas práticas criam-se classes que se enquadram nesta categoria.

Modelação duma realidade com riqueza de entidades
Cada tipo de entidade que seja identificada no problema deve ser programada numa classes de dados distinta. Desta forma, conseguimos modelar de forma conveniente a realidade com que queremos trabalhar. Os conceitos envolvidos ficam explicitados e o programa fica bem organizado.

Considere o enunciado do Problema da pastelaria:

Precisamos dum programa para gerir uma pastelaria. A pastelaria tem um Stock corrente de bolos. O Stock é constituído por várias unidades de cada variedade de bolo. Também queremos saber quais são os ingredientes usados para fazer cada variedade de bolo, e qual a percentagem de cada ingrediente na sua constituição.

O programa deve oferecer diversas operações: por exemplo, descobrir uma variedade de bolo no Stock, validar uma variedade de bolo (verificar se o total das percentagens dá 100%), descobrir o ingrediente principal duma variedade de bolo, efetuar vendas de bolos.

Para resolver este problema, convém introduzir:

a classes de dados Ingredient - com atributos name e percent;
a classes de dados CakeVariety - com atributos name, amount e ingredients;
a classes de dados CakeShop - com atributo stock.
Veja o exercício 51 das aulas práticas.
Interação com o utilizador
Em qualquer aplicação, qualquer que seja o estilo de programação usado, é muito importante tentar separar claramente, sem haver misturas:
As partes responsáveis pela funcionalidade/lógica dos vários objetos;
As partes responsáveis pela interação com o utilizador.
Num programa que esteja organizado em várias classes de dados, recomendamos que a interface com o utilizador seja programada numa classe chamada UI (user interface). O controlo geral do programa deve ser feito no contexto da classe de dados UI.

Por exemplo, para o programa da Pastelaria, um objeto da class UI precisa de criar e guardar uma instância da classe CakeShop.

from dataclasses import dataclass

########### Ingredient ###########

...

########### CakeVariety ###########

...

########### CakeShop ###########

...

########### UI ###########

@dataclass
class UI:   # User Interface
    shop: CakeShop

def ui_run(ui: UI) -> None:
    """ Run the user interface. """
    name = input("BOLO: ")            # pergunta o nome do bolo
    units = int(input("UNIDADES: "))  # pergunta a quantidade
    ui_process(ui, name, units)       # processa o pedido
Depois a função main cria uma única instância de UI e invoca a função ui_run.
############# main #############

def main() -> None:
    ui = UI(ui_input_shop())
    ui_run(ui)

main()
A classe UI deve ser instanciada uma só vez e guarda objetos das classes lógicas. Por exemplo, no caso do programa da Pastelaria, guarda uma instância da classe CakeShop:
Veja o exercício 51 das aulas práticas.

#