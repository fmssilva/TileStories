# %%
"""
===========================================================================
GUIA DE SESSÃO - Prática 08 - IPCE 2026/2027
===========================================================================

Cobre os guiões 08a (ex. 49) e 08b (ex. 50 e 51).

Site da Cadeira: https://ipce-184ea7.gitlab.io/
Este Guião na drive: https://drive.google.com/drive/folders/1twlkB4M0yg0tBxOt2c1diqJU6xtUsoay?usp=sharing


Tema de hoje: as CLASSES DE DADOS (data classes).
    Uma data class dá NOMES às peças de uma coisa que tem várias peças.
    Em vez de p[1] (o que é o 1?), escreve-se p.year.

    1ª parte: o que é, como se cria, como se usa.
              Depois o tipo Date do guião (ex 49).
    2ª parte: data classes dentro de data classes, a UI,
              as frações (ex 50) e a pastelaria (ex 51).
              No fim: os erros do costume.

Como usar este ficheiro no Spyder:
    - cada bloco que começa com "# %%" é uma célula
    - Ctrl + Enter  -> corre só a célula onde está o cursor
    - Shift + Enter -> corre a célula e salta para a seguinte
    - F5            -> corre o ficheiro todo (os programas pedem input!)

Para parar um programa que não acaba:
    - o quadrado vermelho no canto da consola (Interrupt kernel)
    - ou Ctrl + C com o cursor na consola

"""






# %%
"""
===========================================================================
LOGÍSTICA / Dúvidas [8 min -> 10:18]
===========================================================================

    - Folha de Presenças

    - Baixar este guia da drive e abrir no Spyder

    - Dúvidas da aula passada (while, matrizes, o Hi-Lo ao contrário)

    - Revisões para o Teste 2 na drive: while, matrizes, strings, coleções
"""






# %%
"""
===========================================================================
Guião 08a, exercício 49 - Classes de dados e o tipo Date [62 min -> 11:20]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
a) O problema: uma "pessoa" tem vários atributos [7 min -> 10:25] ----------------

    Queremos guardar pessoas: nome, ano de nascimento, morada e email.
    Com o que já sabemos, há três maneiras.
"""

# Maneira 1: variáveis soltas
maria_name = "Maria"
maria_year = 1990
maria_address = "Coimbra"
maria_email = ""

joao_name = "João"
joao_year = 1970
joao_address = "Lisboa"
joao_email = ""

# Já são 8 variáveis para 2 pessoas.

# E uma função que receba uma pessoa precisa de 4 parâmetros...
def do_something_with_person(name, year, address, email):
    print(f"{name} nasceu em {year} e mora em {address}")

do_something_with_person(maria_name, maria_year, maria_address, maria_email)
# Maria nasceu em 1990 e mora em Coimbra

# E 100 pessoas? 400 variáveis?




# Maneira 2: um tuplo (ou uma lista) com as peças por ordem
maria = ("Maria", 1990, "Coimbra", "")
joao = ("João", 1970, "Lisboa", "")

# Já melhorou: a função recebe a pessoa inteira, num só parâmetro
def do_something_with_person(person):
    print(f"{person[0]} nasceu em {person[1]} e mora em {person[2]}")

do_something_with_person(maria)     # Maria nasceu em 1990 e mora em Coimbra

# E dá para ter uma lista de pessoas:
people = [maria, joao]


# Mas para ir buscar cada peça, temos de saber a sua posição no tuplo:
print(maria[1])                 # 1990
print(joao[2])                  # Lisboa

# Funciona, mas daqui a um mês: o que é o maria[2]? E o [3]?
# Temos de decorar que 0 = nome, 1 = ano, 2 = morada, 3 = email.
# Um erro no número e o programa usa a peça errada sem dar erro nenhum.


# Podemos usar constantes para "guardar o numero de cada atributo no tuplo)
# Mas começa-se a aumentar a complexidade e duplicação... 
IDX_NAME = 0
IDX_YEAR = 1
IDX_CITY = 2
IDX_EMAIL = 3

print(maria[IDX_NAME])
print(joao[IDX_NAME])






# Maneira 3: um dicionário
maria = {
        "name": "Maria",
        "year": 1990,
        "address": "Coimbra",
        "email": ""
    }

# Já temos nomes em vez de posições:
print(maria["year"])            # 1990

# Mas:
#   - as chaves são strings: muitas aspas, e uma gralha só se descobre a correr
#     (maria["yaer"] dá KeyError)
#   - nada obriga todas as pessoas a terem as mesmas chaves/atributos:
#     outra pessoa pode vir sem "address", ou com "adress"
#   - não existe um "tipo pessoa": é só um dicionário qualquer






# O ideal era consguir definir um tipo de dados que: 
#       seja fácil de usar e intuitivo
#       obrigue a ter todos os atributos definidos
# É exatamente isto que uma classe de dados nos dá:






# %%
"""
b) A sintaxe, passo a passo [8 min -> 10:33] ------------------------------

1. Definir o molde (template): as peças que compõem o nosso tipo de dados
        Passo 1: o import (uma vez, no início do ficheiro)
        Passo 2: @dataclass, por cima da class
        Passo 3: os atributos (as peças), um por linha, cada um com o seu tipo
"""

from dataclasses import dataclass

@dataclass
class Person:
    name: str
    year: int
    address: str
    email: str = ""             # valor por omissão: se não for dado, fica ""

# Ainda não criámos pessoa nenhuma!
# Só dissemos ao Python COMO é uma pessoa: o molde, o template.

"""
2. Criar novos objetos seguindo esse molde:
"""
# Criar uma pessoa: chamar Person(...) como se fosse uma função.
# Os valores vão pela ordem dos atributos.
maria = Person("Maria", 1990, "Coimbra")
joao = Person("João", 1970, "Lisboa")

# O print já sabe mostrar uma pessoa, com os nomes das peças todos:
print(maria)                    # Person(name='Maria', year=1990, address='Coimbra', email='')
print(joao)                     # Person(name='João', year=1970, address='Lisboa', email='')

# É um tipo novo, tal como int ou str:
print(type(maria))              # <class '__main__.Person'>

# Ou seja: 
# Class == o template onde definimos o nome da class e atributos que deve ter
# Objeto == as instâncias de entidades reais que são criadas com base no template definido pela class 


#%%
# Ler uma peça: objeto.atributo
print(maria.name)               # Maria
print(maria.year)               # 1990

# Alterar uma peça: a Maria vai morar para o Algarve
maria.address = "Faro"
print(maria)                    # Person(name='Maria', year=1990, address='Faro', email='')



# Vocabulário:
#   classe de dados (Person)  -> o molde: diz que peças há
#   objeto ou instância       -> cada coisa feita com o molde (maria, joao)
#   atributos                 -> as peças (name, year, address, email)
#   Person(...)               -> a função construtora: o Python escreve-a sozinho
#
# Nome da classe com maiúscula (Person, Date, CakeShop).
# É a convenção: assim distingue-se logo de uma função ou variável.




# %%
# Tal como o tuplo, passa-se a pessoa inteira a uma função, e faz-se listas de pessoas.
# Mas agora cada peça tem nome:
def do_something_with_person(person: Person) -> None:
    print(f"{person.name} nasceu em {person.year} e mora em {person.address}")

people = [maria, joao]

for p in people:
    do_something_with_person(p)
# Maria nasceu em 1990 e mora em Faro
# João nasceu em 1970 e mora em Lisboa






# %%
"""
c) Exercícios: As classes Point e Book [5 min -> 10:38] -------------------------------

    1. Point
        Definam uma classe de dados Point, com atributos x, y, z (float).
        Criem 2 pontos (objetos dessa classe): (3, 4, 5) e (1, 2, 3).

        Escrevam os 2 pontos completos com todos os atributos.
        Escrevam só a coordenada x de cada um.
        Mudem o y de cada um para 10 e 20 e voltem a escrever os pontos.
"""

#  import
from dataclasses import dataclass

# definir o molde
# TODO


# criar novos objetos seguindo esse molde:
# TODO






# %%
# print a coordenada x dos objetos 
# TODO





# %%
# Alterar a coordenada y dos objetos 




# Com isto um programa de geometria passa a trabalhar com pontos inteiros:
# passa um ponto de um lado para o outro,
# em vez de 3 coordenadas soltas, que se trocam e se perdem facilmente.






# %%
"""
    2. Book
       Criem uma classe de dados Book, com title (str), author (str) e pages (int).
       Criem dois livros com páginas diferentes
       Print o livro com maior numero de paginas
"""
# TODO 





















# %%
"""
d) Funções para a classe de dados [8 min -> 10:46] -------------------------

    A classe é um tipo de dados e só guarda os atributos que definem um objeto.

    Mas um tipo de dados costuma vir com operações próprias:
        o int tem +, -, *, //, %
        a str tem len(), o + que junta, os [ ] para ir buscar um carácter
        a list tem len(), append(), sort()...

    Os nossos tipos também podem, e normalmente devem, ter as suas póprias funções associadas. 
    Então escrevemos as funções que fazem sentido para eles.
    Por exemplo, para o Point: a distância entre dois pontos, duplicar um ponto, mover um ponto...

    Estas funções definem-se logo a seguir à classe.
    Nome das funções: tipo_operação  (person_as_str, point_distance...)
    Assim, só pelo nome, sabe-se de que tipo é a função.
"""

# import
from dataclasses import dataclass

# definir o molde (agora a 2 dimensões, só x e y)
@dataclass
class Point:
    x: float
    y: float

def point_distance(p1: Point, p2: Point) -> float:
    """ Distance between two points. """
    dx = p2.x - p1.x
    dy = p2.y - p1.y
    return (dx ** 2 + dy ** 2) ** 0.5

def point_duplicate(p1: Point) -> Point:
    """ A new point, with the same coordinates. """
    return Point(p1.x, p1.y)

def point_move_in_place(p: Point, dx: float, dy: float) -> None:
    """ Move the point itself by dx and dy. """
    p.x += dx
    p.y += dy

def point_move_new(p: Point, dx: float, dy: float) -> Point:
    """ A NEW point, moved by dx and dy. """
    return Point(p.x + dx, p.y + dy)

# criar pontos:
p1 = Point(0, 0)
p2 = Point(3, 4)

print(p1)                       # Point(x=0, y=0)
print(p2)                       # Point(x=3, y=4)


#%%
# usar as funções:
print(point_distance(p1, p2))   # 5.0


# %%
p3 = point_move_in_place(p1, 10, 0)
print(p1)                       # Point(x=10, y=0)   o próprio p1 mudou!
print(p3)                       # None   a função não devolve nada (-> None)


# %%
p4 = point_move_new(p1, 10, 0)
print(p4)                       # Point(x=20, y=0)   um ponto novo
print(p1)                       # Point(x=10, y=0)   o p1 ficou igual

# Como é que o point_move_in_place consegue mudar o p1, se só recebe o p?
# Porque o p NÃO é uma cópia: o p e o p1 são dois nomes para o MESMO objeto.
# (o mesmo que já acontecia com as listas: um append dentro de uma função
#  muda a lista de quem chamou)
#
# Qual escolher?
#   - devolver um novo: mais seguro, ninguém fica com os dados mudados sem saber
#   - alterar: quando a coisa muda MESMO com o tempo (o stock de uma loja)
# Vamos ver as duas no guião: as frações (ex 50) e a pastelaria (ex 51).






# %%
"""
e) Valor por omissão e argumentos com nome [3 min -> 10:49] ----------------
"""

from dataclasses import dataclass


@dataclass
class Point:
    name: str
    description: str
    x: float = 0.0          # valor por omissão: se não for dado, fica 0.0
    y: float = 0.0
    z: float = 0.0

# Sem coordenadas: ficam os valores por omissão
print(Point("Origem", "O ponto de origem"))
# Point(name='Origem', description='O ponto de origem', x=0.0, y=0.0, z=0.0)

# Com coordenadas: substituem os valores por omissão
print(Point("Ponto A", "Um ponto qualquer", 3, 4, 5))
# Point(name='Ponto A', description='Um ponto qualquer', x=3, y=4, z=5)

# Com o nome dos atributos: aí a ordem já não importa
print(Point("Ponto B", "Outro ponto qualquer", y=10, x=20, z=30))
# Point(name='Ponto B', description='Outro ponto qualquer', x=20, y=10, z=30)

# E só o z, saltando o x e o y:
print(Point("Ponto C", "Só com altura", z=7))
# Point(name='Ponto C', description='Só com altura', x=0.0, y=0.0, z=7)

# Os atributos com valor por omissão têm de ficar no FIM da classe.
# Ou seja, a partir do momento que escrevemos um atributo com nome, 
# todos os restantes para a direita tem de ter nome:
print(Point("Ponto D", description="Um ponto qualquer", x=3, y=4, z=5))   # certo

# Erro (temos atributo sem nome depois de um com nome): 
# print(Point("Ponto E", description="Um ponto qualquer", 3, 4, 5))   # TypeError: Point() takes no positional arguments after keyword arguments
# »» SyntaxError: positional argument follows keyword argument









# %%
"""
f) Comparar objetos: ==, is e < [6 min -> 10:54] ---------------------------

    Três perguntas diferentes:
        d1 == d2   têm o mesmo VALOR? (as peças todas iguais)
        d1 is d2   são o MESMO OBJETO? (a mesma "caixa" na memória)
        d1 < d2    vem antes? (para os tipos que nós criamos,
                   o Python não sabe como os queremos ordenar)

    Antes de correr: o que escrevem os prints?
"""

from dataclasses import dataclass

@dataclass
class Date:
    day: int
    month: int
    year: int

d1 = Date(31, 12, 2026)
d2 = Date(31, 12, 2026)         # outra data, com os mesmos valores
d3 = d1                         # NÃO cria nada: fica a apontar para o mesmo objeto

print(d1 == d2)                 # True    o @dataclass compara as peças todas
print(d1 is d2)                 # False   mas são duas datas diferentes
print(d1 is d3)                 # True    d1 e d3 são a mesma data

# A prova: mudar o d2 não mexe no d1; mudar o d3 mexe!
d2.day = 30
print(d1)                       # Date(day=31, month=12, year=2026)
d3.day = 1
print(d1)                       # Date(day=1, month=12, year=2026)



# E o < ? Descomentem e corram:
# print(Date(1, 1, 2000) < d1)
# TypeError: '<' not supported between instances of 'Date' and 'Date'

# O == o @dataclass escreve por nós. O < não:
# o Python não sabe o que é "menor" para uma data.
#   Comparava o dia primeiro? O ano? Nós é que sabemos: primeiro o ano.
# Por isso o guião pede-nos uma função date_is_less.






# %%
"""
    O "is" é muito usado para ver se um resultado de uma pesquisa realmnte existe
    
    Uma função de PROCURA nem sempre encontra o que procura.
    Nesse caso devolve None: "não há nada para devolver".
    
    Então os clientes que chamam essa função têm de ter cuidado
    e verificar se realmente recebem um objeto ou se recebem None, 
    antes de tentar usar esse resultado.    

    Precisa da Date da célula de cima.
"""

agenda = [Date(30, 10, 2026), Date(1, 1, 2025), Date(31, 12, 2026)]

def dates_first_in_year(dates: list[Date], year: int) -> Date | None:
    """ The first date of that year; None if there is none. """
    for d in dates:
        if d.year == year:
            return d                # encontrou: devolve a data
    return None                     # deu a volta toda: não há


for year in [2025, 2026, 1900]:
    d = dates_first_in_year(agenda, year)
    if d is None:
        print(f"Não há datas de {year}.")
    else:
        print(f"Encontrei: {d.day}/{d.month}/{d.year}")


# O tipo "-> Date | None" avisa logo quem lê: pode vir uma Date, ou None.
#
# E se nos esquecermos de perguntar? Descomentem e corram:
# print(dates_first_in_year(agenda, 1999).day)
# AttributeError: 'NoneType' object has no attribute 'day'
# O None não tem peças: é o erro mais comum com funções de procura.
#
# Escreve-se "x is None" (e "x is not None"), e não "x == None":
# o == pode ser redefinido por quem escreve uma classe; o is não engana.







# %%
"""
g) EXERCICIO 49 (GUIÃO, Mooshak S) - o tipo Date: o que já vem feito [3 min -> 10:56]

    O guião dá a classe Date e várias funções prontas.
    Corram esta célula ANTES das seguintes: elas usam estas funções.

    Faltam escrever 7 funções:
        date_is_less, date_order_in_year, date_next, date_prev  (antes do intervalo)
        dates_len, dates_maximum, dates_minimum                 (depois)
"""

from dataclasses import dataclass

############# type Date #############

@dataclass
class Date:
    day: int
    month: int
    year: int

def date_as_str(dt: Date) -> str:
    """ Convert date to a string in the form D/M/Y. """
    return f"{dt.day}/{dt.month}/{dt.year}"

def date_input(prompt: str) -> Date | None:
    """ Input a date using the standard representation.
        Precondition: The input has the form "D/M/Y" or "" """
    END_MARK = ''
    s = input(prompt)
    if s == END_MARK:
        return None             # linha vazia: não há data
    d, m, y = s.split("/")
    return Date(int(d), int(m), int(y))

def date_print(dt: Date) -> None:
    """ Print a date in the standard representation. """
    print(date_as_str(dt))

def date_is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def date_month_length(month: int, year: int) -> int:
    """ Number of days of a given month (in a given year).
        Precondition: 1 <= month <= 12
    """
    durations = [None, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    if month == 2 and date_is_leap_year(year):
        return durations[2] + 1
    else:
        return durations[month]

def date_is_valid(dt: Date) -> bool:
    """ Is the data valid? """
    return (dt.year > 0          # estamos só interessados na "Era Comum"
        and 1 <= dt.month <= 12
        and 1 <= dt.day <= date_month_length(dt.month, dt.year))

def date_is_equal(dt1: Date, dt2: Date) -> bool:
    """ Are two dates equal?
        Precondition: date_is_valid(dt1) and date_is_valid(dt2) """
    return dt1 == dt2

print(date_as_str(Date(30, 10, 2026)))          # 30/10/2026
print(date_month_length(2, 2024), date_month_length(2, 2026))   # 29 28
print(date_is_valid(Date(31, 4, 2026)))         # False   abril tem 30 dias

# Truques que vale a pena roubar:
#   - durations começa com None: assim durations[1] é janeiro, sem fazer -1.
#   - date_input devolve None para dizer "não há data" (linha vazia).
#     O tipo "-> Date | None" diz isso mesmo: devolve uma Date OU None.
#   - Para perguntar "é None?" usa-se "is None" (e "is not None"), não "== None".
#     O == pode ser redefinido por quem escreve uma classe; o "is" não.
#     (o Python não se queixa do == None, mas a convenção é o is)
#   - date_is_valid está partida em várias linhas: os parênteses à volta deixam.






# %%
"""
h) EXERCICIO 49 (GUIÃO) - date_is_less [6 min -> 11:02] --------------------

    Precisa da célula g) (corram-na primeiro).

    date_is_less(Date(30, 1, 2026), Date(1, 2, 2026)) == True
    date_is_less(Date(1, 1, 2027), Date(31, 12, 2026)) == False

    Pensem como se faz à mão: qual é a primeira peça a comparar?
"""

def date_is_less(dt1: Date, dt2: Date) -> bool:
    """ Does one date come before the other?
        Precondition: date_is_valid(dt1) and date_is_valid(dt2) """
    pass # TODO


print(date_is_less(Date(30, 1, 2026), Date(1, 2, 2026)))     # True
print(date_is_less(Date(1, 1, 2027), Date(31, 12, 2026)))    # False
print(date_is_less(Date(5, 5, 2026), Date(5, 5, 2026)))      # False   igual não é menor



#%%
# Encontrem o bug nesta versão "mais curta":
def date_is_less_bug(dt1: Date, dt2: Date) -> bool:
    """ Does one date come before the other? (with a bug!) """
    return (dt1.year <= dt2.year
            and dt1.month <= dt2.month
            and dt1.day < dt2.day)

print(date_is_less_bug(Date(1, 1, 2026), Date(2, 1, 2026)))   # True    certo...
print(date_is_less_bug(Date(30, 1, 2026), Date(1, 2, 2026)))  # False   errado!

# 30 de janeiro vem antes de 1 de fevereiro, mas 30 < 1 é False.
# O dia só interessa quando o ano e o mês são IGUAIS.
# É como ordenar palavras no dicionário: a 1ª letra decide; a 2ª só se a 1ª empatar.

# Extra (curiosidade): os tuplos comparam-se exatamente assim, peça a peça.
# return (dt1.year, dt1.month, dt1.day) < (dt2.year, dt2.month, dt2.day)






# %%
"""
i) EXERCICIO 49 (GUIÃO) - date_order_in_year [6 min -> 11:08] --------------

    Precisa da célula g).

    Que dia do ano é? (1 de janeiro é o dia 1)
        date_order_in_year(Date(1, 1, 2026)) == 1
        date_order_in_year(Date(1, 2, 2026)) == 32
        date_order_in_year(Date(31, 12, 2026)) == 365
        date_order_in_year(Date(31, 12, 2024)) == 366    (2024 é bissexto)

    Pista: os dias de todos os meses ANTES deste, mais o dia.
"""

def date_order_in_year(dt: Date) -> int:
    """ The day order number inside the year
        Precondition: date_is_valid(dt) """
    pass # TODO




print(date_order_in_year(Date(1, 1, 2026)))      # 1
print(date_order_in_year(Date(1, 2, 2026)))      # 32
print(date_order_in_year(Date(31, 12, 2026)))    # 365
print(date_order_in_year(Date(31, 12, 2024)))    # 366

# Em janeiro, range(1, 1) está vazio: total fica 0 e devolve só o dia. Certo.
# Reaproveitámos o date_month_length: os bissextos já vêm tratados.






# %%
"""
j) EXERCICIO 49 (GUIÃO) - date_next e date_prev [12 min -> 11:20] ----------

    Precisa da célula g).

    O dia seguinte e o dia anterior. Os casos difíceis estão nas viragens:
        date_next(Date(30, 10, 2026)) -> 31/10/2026
        date_next(Date(31, 10, 2026)) -> 1/11/2026     (vira o mês)
        date_next(Date(31, 12, 2026)) -> 1/1/2027      (vira o ano)
        date_prev(Date(1, 3, 2024))   -> 29/2/2024     (bissexto)
        date_prev(Date(1, 1, 2026))   -> 31/12/2025

    Devolvem uma data NOVA (como o point_move_new): não mexem na que recebem.
"""

def date_next(dt: Date) -> Date:
    """ The following day.
        Precondition: date_is_valid(dt) """
    pass # TODO


def date_prev(dt: Date) -> Date:
    """ The previous day.
        Precondition: date_is_valid(dt) """
    pass # TODO



print(date_as_str(date_next(Date(30, 10, 2026))))   # 31/10/2026
print(date_as_str(date_next(Date(31, 10, 2026))))   # 1/11/2026
print(date_as_str(date_next(Date(31, 12, 2026))))   # 1/1/2027
print(date_as_str(date_prev(Date(1, 3, 2024))))     # 29/2/2024
print(date_as_str(date_prev(Date(1, 3, 2026))))     # 28/2/2026
print(date_as_str(date_prev(Date(1, 1, 2026))))     # 31/12/2025

# Testem sempre as viragens: o caso normal quase nunca tem o bug.






# %%
"""
    Precisa das células g) e j).

    E se o date_next ALTERASSE a data que recebe, em vez de criar uma nova?
    Antes de correr: o que escreve o "Prev max"?
"""

def date_next_bug(dt: Date) -> Date:
    """ The following day (changes dt: bug!). """
    if dt.day < date_month_length(dt.month, dt.year):
        dt.day += 1
    elif dt.month < 12:
        dt.day = 1
        dt.month += 1
    else:
        dt.day = 1
        dt.month = 1
        dt.year += 1
    return dt

max = Date(31, 12, 2026)
print(f"Next max: {date_as_str(date_next_bug(max))}")    # Next max: 1/1/2027
print(f"Prev max: {date_as_str(date_prev(max))}")        # Prev max: 31/12/2026   errado!
# Devia ser 30/12/2026. O date_next_bug mudou o próprio max para 1/1/2027,
# e o date_prev já partiu dessa data.
#
# Ninguém pediu ao date_next para mudar o max: mudou "sem avisar".
# Com a versão certa (que cria uma Date nova) isto não acontece:
max = Date(31, 12, 2026)
print(f"Next max: {date_as_str(date_next(max))}")        # Next max: 1/1/2027
print(f"Prev max: {date_as_str(date_prev(max))}")        # Prev max: 30/12/2026






# %%
"""
===========================================================================
INTERVALO [20 min -> 11:40]
===========================================================================
"""






# %%
"""
===========================================================================
Guião 08a, exercício 49 (fim) - Dates e a UI [20 min -> 12:00]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
a) Data classes dentro de data classes [8 min -> 11:48] --------------------

    Um atributo pode ser de QUALQUER tipo:
    um int, uma str, uma lista... ou outra data class.
"""

import math
from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

def point_distance(p1: Point, p2: Point) -> float:
    """ Distance between two points. """
    return ((p2.x - p1.x) ** 2 + (p2.y - p1.y) ** 2) ** 0.5


# Molde do Circle
@dataclass
class Circle:
    center: Point               # um círculo TEM um ponto lá dentro
    radius: float

# Funções do Circle
def circle_area(c: Circle) -> float:
    """ Area of a circle. """
    return math.pi * c.radius ** 2

def circle_perimeter(c: Circle) -> float:
    """ Perimeter of a circle. """
    return 2 * math.pi * c.radius

def circle_intersection(c1: Circle, c2: Circle) -> bool:
    """ Do the two circles touch or overlap? """
    return point_distance(c1.center, c2.center) <= c1.radius + c2.radius


# Criar e usar círculos
c = Circle(Point(0, 0), 5)
print(c)                        # Circle(center=Point(x=0, y=0), radius=5)
print(c.center)                 # Point(x=0, y=0)
print(c.center.x)               # 0       lê-se da esquerda para a direita: o x do centro do c
print(c.radius)                 # 5

print(round(circle_area(c), 2))         # 78.54
print(round(circle_perimeter(c), 2))    # 31.42
print(circle_intersection(c, Circle(Point(8, 0), 2)))   # False   distância 8 > 5 + 2
print(circle_intersection(c, Circle(Point(6, 0), 2)))   # True    distância 6 <= 5 + 2

c.center.x = 2                  # mudar o x do centro
print(c)                        # Circle(center=Point(x=2, y=0), radius=5)

# O circle_intersection reaproveita o point_distance:
# o círculo não precisa de saber fazer contas com pontos, pede ao Point.





#%%
# Molde do Polygon (polígono)
@dataclass
class Polygon:
    vertices: list[Point]       # uma lista de pontos

# Funções do Polygon
def polygon_perimeter(p: Polygon) -> float:
    """ Perimeter of a polygon. """
    n = len(p.vertices)
    total = 0
    for i in range(n):
        j = (i + 1) % n         # o vértice seguinte; o último volta ao primeiro (0)
        total += point_distance(p.vertices[i], p.vertices[j])
    return total

def polygon_area(p: Polygon) -> float:
    """ Area of a polygon (shoelace formula). """
    n = len(p.vertices)
    total = 0
    for i in range(n):
        j = (i + 1) % n
        total += p.vertices[i].x * p.vertices[j].y
        total -= p.vertices[j].x * p.vertices[i].y
    return abs(total) / 2


# Criar e usar um polígono
square = Polygon([Point(0, 0), Point(1, 0), Point(1, 1), Point(0, 1)])
print(len(square.vertices))     # 4
print(square.vertices[2])       # Point(x=1, y=1)
print(square.vertices[2].y)     # 1

for v in square.vertices:       # percorrer como uma lista qualquer
    print(v.x, v.y)
# 0 0
# 1 0
# 1 1
# 0 1

print(polygon_perimeter(square), polygon_area(square))     # 4.0 1.0
triangle = Polygon([Point(0, 0), Point(3, 0), Point(0, 4)])
print(polygon_perimeter(triangle), polygon_area(triangle)) # 12.0 6.0

# square.vertices      -> uma lista (de pontos)
# square.vertices[2]   -> um ponto
# square.vertices[2].y -> um número
# Cada . ou [ ] desce um nível. Na dúvida, façam print a cada passo.
#
# O (i + 1) % n: com n = 4, o i = 3 dá (3 + 1) % 4 = 0. O último liga-se ao primeiro.
# A fórmula da área (a "fórmula do atacador") não é para decorar:
# o que interessa aqui é o percorrer dos vértices e o p.vertices[i].x.
#
# É exatamente o que aí vem: o tipo Dates tem lá dentro uma lista de Date.






# %%
"""
b) EXERCICIO 49 (GUIÃO) - Dates: len, maximum e minimum [7 min -> 11:55] ---

    Precisa das células g), h), i) e j) da 1ª parte (corram-nas primeiro).

    Uma Dates guarda uma lista de datas: ds.dates
        dates_len     -> quantas datas há
        dates_maximum -> a data mais recente
        dates_minimum -> a data mais antiga
    (no enunciado o dates_len aparece com "-> None"; no código está "-> int", que é o certo)
"""

############# type Dates #############

@dataclass
class Dates:
    dates: list[Date]

def dates_input(prompt1: str, prompt2: str) -> Dates:
    """ Interactive date sequence reading. """
    print(prompt1)
    l = []
    while True:
        dt = date_input(prompt2)
        if dt is None:          # linha vazia: acabou
            break
        l.append(dt)
    return Dates(l)

def dates_print(ds: Dates) -> None:
    """ Print a sequence of dates """
    for d in ds.dates:
        print(d)



def dates_len(ds: Dates) -> int:
    """ Number of dates in the sequence. """
    pass # TODO


def dates_maximum(ds: Dates) -> Date:
    """ The maximum.
        Precondition: dates_len(ds) > 0 """
    pass # TODO



def dates_minimum(ds: Dates) -> Date:
    """ The minimum.
        Precondition: dates_len(ds) > 0 """
    pass # TODO



ds = Dates([Date(30, 10, 2026), Date(1, 1, 2000), Date(31, 12, 2026)])
print(dates_len(ds))                        # 3
print(date_as_str(dates_maximum(ds)))       # 31/12/2026
print(date_as_str(dates_minimum(ds)))       # 1/1/2000

# É o máximo de uma lista que já conhecem. Só duas coisas mudam:
#   - a lista está em ds.dates (e não em ds)
#   - o "max < d" passa a date_is_less(max, d), porque o < não funciona com Date
#
# Para "maior do que" não precisamos de uma função nova:
#   d > max   é o mesmo que   max < d   ->  date_is_less(max, d)






# %%
"""
c) EXERCICIO 49 (GUIÃO) - a UI e o main [5 min -> 12:00] -------------------

    Precisa das células g) a j) e b).

    A UI (User Interface) é a parte que fala com o utilizador: input e print.
    As outras funções só fazem contas com datas: não leem nem escrevem nada.

    Porquê separar?
        - as funções lógicas testam-se com um print, sem ter de escrever datas à mão
        - se um dia a interface mudar (uma janela, um site), a lógica fica igual
        - o que o programa tem de lembrar (aqui as datas, no 51 a loja)
          fica guardado num sítio só: o objeto ui

    Definimos a classe de dados UI com uma função ui_run que:
        - Leia uma sequência de datas.
        - Se a sequência for vazia, diz que não há datas.
        - Caso contrário escreve: data máxima, data mínima, posição da data máxima no ano, dia seguinte da data máxima, dia anterior da data máxima.

    Exemplo de execução
        Introduza uma data por linha, tudo terminado por uma linha vazia:
        D/M/Y: 
        Não há datas!

    Exemplo de execução
        Introduza uma data por linha, tudo terminado por uma linha vazia:
        D/M/Y: 30/10/2026
        D/M/Y: 1/1/2000
        D/M/Y: 31/12/2026
        D/M/Y: 
        Data máxima: 31/12/2026
        Data mínima: 1/1/2000
        Ord max: 365
        Next max: 1/1/2027
        Prev max: 30/12/2026
"""

############# type UI #############

# No validation here. We are assuming all the inputs are valid (to simplify).

@dataclass
class UI:    # User Interface.
    dates: Dates

def ui_run(ui: UI) -> None:
    """ Run the user interface. """
    ui.dates = dates_input(
                ("Introduza uma data por linha,"
            + " tudo terminado por uma linha vazia:"),
        "D/M/Y: ")
    if dates_len(ui.dates) == 0:
        print("Não há datas!")
    else:
        max = dates_maximum(ui.dates)
        min = dates_minimum(ui.dates)
        print(f"Data máxima: {date_as_str(max)}")
        print(f"Data mínima: {date_as_str(min)}")
        print(f"Ord max: {date_order_in_year(max)}")
        print(f"Next max: {date_as_str(date_next(max))}")
        print(f"Prev max: {date_as_str(date_prev(max))}")


############# main #############

def main() -> None:
    ui = UI(Dates([]))          # a UI começa com uma sequência vazia
    ui_run(ui)                  # e o ui_run enche-a com o que o utilizador escrever

main()

# Introduza uma data por linha, tudo terminado por uma linha vazia:
# D/M/Y: 30/10/2026
# D/M/Y: 1/1/2000
# D/M/Y: 31/12/2026
# D/M/Y:
# Data máxima: 31/12/2026
# Data mínima: 1/1/2000
# Ord max: 365
# Next max: 1/1/2027
# Prev max: 30/12/2026
#
# Reparem como o main ficou curto: cria UMA UI e manda-a correr.
# Vai ser sempre assim, também no 50 e no 51.
#
# Para o Mooshak (S): juntar num só ficheiro a célula g), as funções de h), i), j),
# o Dates de b) e esta célula.






# %%
"""
===========================================================================
Guião 08b, exercícios 50 e 51 - Frações e Pastelaria [60 min -> 13:00]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
d) Dois estilos: objetos que não mudam e objetos que mudam [4 min -> 12:04]

    No ex 49, date_next criava uma data NOVA. Hoje vamos ver os dois estilos:

    Ex 50, frações: como os números.
        3 + 2 não muda o 3: dá um número novo, o 5.
        rational_add(a, b) também não muda o a nem o b: dá uma fração nova.

    Ex 51, pastelaria: a loja muda MESMO com o tempo.
        Cada venda tira bolos do stock.
        cake_variety_sell altera a variedade de bolo que recebe.

    Nomes que vão ouvir (teórica 7b):
        imutável -> nunca muda depois de criado (int, float, str, tuple)
        mutável  -> pode mudar (list, e as nossas data classes)
"""

x = 3
y = x + 2
print(x, y)                     # 3 5     o 3 não mudou

l = [1, 2]
l.append(3)
print(l)                        # [1, 2, 3]   a própria lista mudou

# Uma data class pode sempre mudar (p.x = 10 funciona).
# Mas podemos escrever as funções como se não mudasse: é o que vamos fazer no ex 50.






# %%
"""
e) EXERCICIO 50 (GUIÃO, Mooshak T) - Frações: rational_normalize [8 min -> 12:12]

    Escreva um programa que faça contas com números racionais (frações). 
    O programa deve suportar as quatro operações básicas +, -, *, /. 
    Escreva o programa sem recorrer a nenhum tipo existente ou a uma biblioteca existente.

    O resultado de qualquer operação deve ser uma fração simplificada e normalizada. 
    Isto consegue-se dividindo tanto o numerador como o denominador pelo maior divisor comum. 
    Além disso, a normalização deve também garantir que apenas o numerador pode ser negativo.

    Escreva o programa usando duas classes de dados:
        Uma classe de dados Rational que implementa o tipo das frações.
        Uma classe de dados UI (user interface) que implementa a interação com o utilizador. Esta classe de dados preocupa-se apenas com a interação com o utilizador, e recorre à outra classe de dados para todos os aspetos lógicos dos números racionais.


    Uma fração está na forma normal quando:
        1. está simplificada: numerador e denominador divididos pelo maior divisor comum
           6/4 -> 3/2
        2. só o numerador pode ser negativo
           3/-5 -> -3/5      -30/-50 -> 3/5

    O guião dá o gcd (maior divisor comum) e algumas funções.
    Cuidado: o gcd do guião só aceita números >= 0 (está na pré-condição).
    Corram esta célula antes das seguintes.
"""

from dataclasses import dataclass

@dataclass
class Rational:
    num: int
    den: int

def gcd(m: int, n: int) -> int:
    """ Greatest common divisor. Use the Euclidean algorithm.
         Precondition: m >= 0 and n >= 0 """
    if m == 0:
        return n
    elif n == 0:
        return m
    while m != n:
        if m > n:
            m -= n
        else:
            n -= m
    return n

def rational_as_str(r: Rational) -> str:
    """ Convert to string. """
    return f"{r.num}/{r.den}"

def rational_is_valid(r: Rational) -> bool:
    """ Is the rational valid? """
    return r.den > 0

def rational_is_zero(r: Rational) -> bool:
    """ Is this zero?
        Precondition: rational_is_valid(r) """
    return r.num == 0


def rational_normalize(r: Rational) -> Rational:
    """ Put the rational in the normal form.
        Precondition: r.den != 0 """
    pass # TODO




def rational_create(num: int, den: int) -> Rational:
    """ Simplify rational number.
        Precondition: den != 0
    """
    return rational_normalize(Rational(num, den))

print(rational_as_str(rational_create(6, 4)))       # 3/2
print(rational_as_str(rational_create(3, -5)))      # -3/5
print(rational_as_str(rational_create(-30, -50)))   # 3/5
print(rational_as_str(rational_create(0, 7)))       # 0/1
print(gcd(30, 50), gcd(0, 4))                       # 10 4

# Porquê o abs?
# O gcd do guião só funciona com números >= 0 (é a pré-condição).
# Com gcd(-30, -50): -30 > -50, então m -= n dá 20; depois 20 > -50 dá 70...
# o m nunca chega ao n: um while que nunca acaba (aula 7!).
# E -30/-50 é precisamente um dos exemplos do guião.
#
# E o zero? gcd(0, 7) dá 7, e 0 // 7 = 0, 7 // 7 = 1: fica 0/1, como deve.
# (numa versão antiga do guião, o gcd(0, ...) também nunca acabava)
#
# O // aqui é seguro: o g divide os dois certinho, por isso não há arredondamentos.
# E o normalize devolve uma fração NOVA: não mexe no r.






# %%
"""
f) EXERCICIO 50 (GUIÃO) - as 4 operações e a UI [8 min -> 12:20] -----------

    Precisa da célula e).

    Contas com frações:
        a/b + c/d = (a*d + c*b) / (b*d)
        a/b - c/d = (a*d - c*b) / (b*d)
        a/b * c/d = (a*c) / (b*d)
        a/b / c/d = (a*d) / (b*c)        (multiplicar pelo inverso)
        
    Depois é só criar com rational_create, que normaliza.

    Inputs para experimentar (um de cada vez):
        3/5 + 2/1      ->  13/5
        -30/-50 * 1/1  ->  3/5
        2/5 / 1/2      ->  4/5
        1/2 - 1/2      ->  0/1
"""

def rational_add(r1: Rational, r2: Rational) -> Rational:
    """ Add two rational numbers.
        Precondition: rational_is_valid(r1) and rational_is_valid(r2) """
    pass # TODO



def rational_sub(r1: Rational, r2: Rational) -> Rational:
    """ Subtract two rational numbers.
        Precondition: rational_is_valid(r1) and rational_is_valid(r2) """
    pass # TODO



def rational_mul(r1: Rational, r2: Rational) -> Rational:
    """ Multiply two rational numbers.
        Precondition: rational_is_valid(r1) and rational_is_valid(r2) """
    pass # TODO



def rational_div(r1: Rational, r2: Rational) -> Rational:
    """ Divide two rational numbers.
        Precondition: rational_is_valid(r1)
                and rational_is_valid(r2) and not rational_is_zero(r2) """
    pass # TODO


a = Rational(1, 2)
b = Rational(1, 3)
print(rational_as_str(rational_add(a, b)))      # 5/6
print(rational_as_str(rational_sub(a, b)))      # 1/6
print(rational_as_str(rational_div(a, Rational(-1, 4))))    # -2/1
print(a, b)                     # Rational(num=1, den=2) Rational(num=1, den=3)   não mudaram




# %%
"""
e) EXERCICIO 50 (GUIÃO, Mooshak T) - Frações: rational_normalize [8 min -> 12:12]

    User Interface
    Esceve uma classe de dados UI (user interface) que implementa a interação com o utilizador. Esta classe de dados preocupa-se apenas com a interação com o utilizador, e recorre à outra classe de dados para todos os aspetos lógicos dos números racionais.

    Exemplo de execução
        A/B OP C/D: 3/5 + 2/1
        13/5

    Exemplo de execução
        A/B OP C/D: -30/-50 * 1/1
        3/5

    Exemplo de execução
        A/B OP C/D: 2/5 / 1/2
        4/5
    """

# No validation here. We are assuming all the inputs are valid (to simplify).

@dataclass
class UI:    # User Interface.
    pass     # no attributes


def ui_make_rational(s: str) -> Rational:
    """ Create a rational number from the format: 'n/d'. """
    n, d = s.split('/')
    return Rational(int(n), int(d))


def ui_process_input(s: str) -> tuple[Rational, str, Rational]:
    """ Process the format: 'n/d op n/d'. """
    r1, op, r2 = s.split()
    x = ui_make_rational(r1)
    y = ui_make_rational(r2)
    return (x, op, y)


def ui_eval_input(s: str) -> Rational | None:
    """ Decode the input and perform the operation. """
    x, op, y = ui_process_input(s)
    if op == '+':
        return rational_add(x, y)
    elif op == '-':
        return rational_sub(x, y)
    elif op == '*':
        return rational_mul(x, y)
    elif op == '/':
        return rational_div(x, y)
    else:
        return None


def ui_run() -> None:
    """ Run the user interface. """
    s = input("A/B OP C/D: ")
    result = ui_eval_input(s)
    if result is None:
        print("Operação inválida!")
    else:
        print(rational_as_str(result))


############# main #############

def main() -> None:
    ui_run()

main()

# A/B OP C/D: 3/5 + 2/1
# 13/5
#
# Reparem: a UI não sabe somar frações. Só parte o texto e chama as rational_*.
# Aqui a UI não guarda nada (pass): um programa de uma conta só não tem estado.
#
# Os tipos: tuple[Rational, str, Rational] é um tuplo com 3 coisas de tipos diferentes
# (uma lista, list[...], tem um só tipo lá dentro).
# E "-> Rational | None": devolve uma fração, ou None se a operação não existir
# (A/B % C/D dá "Operação inválida!").
# O Python não verifica os tipos (vamos ver isso nos erros): são para quem lê,
# e para ferramentas como o Pylance do VSCode, que avisam quando não batem certo.
#
# (o guião faz print(rational_as_str(ui_eval_input(s))) e calcula tudo duas vezes;
#  aqui guarda-se o resultado em result e usa-se esse)
#
# Para o Mooshak (T): a célula e) mais esta, num só ficheiro.






# %%
"""
g) EXERCICIO 51 (GUIÃO, Mooshak U) - Pastelaria: CakeVariety [9 min -> 12:29]

    A pastelaria tem um stock corrente de bolos. 
    O stock é constituído por várias unidades de cada variedade de bolo. 
    Para cada variedade de bolo é preciso saber quantas unidades estão disponíveis para venda. 
    Neste programa também queremos saber quais são os ingredientes usados para fazer cada variedade de bolo 
    e qual a percentagem de cada ingrediente na receita.

    O programa funciona assim:
        Lê o stock inicial a partir do teclado;
        Pede ao utilizador o nome duma variedade de bolos e o número de unidades pretendidas;
        Efetua a venda, mostrando a quantidade efetivamente vendida 
            (pode ser inferior à quantidade pedida por falta de stock);
        Escreve-se ainda o nome do ingrediente principal da variedade de bolo que foi vendida.


    Escreva o programa usando quatro classes de dados:
        Uma classe de dados Ingredient, que provavelmente não precisará de ter nenhum método para além do construtor. Os ingredientes são passivos.
        Uma classe de dados CakeVariety, com um método que descobre qual o ingrediente principal duma variedade bolos (se não houver ingredientes, retorna None), e outro método para concretizar as alterações internas relativas a uma venda (retorna o número de unidades realmente vendidas, que podem ser menos do que as pedidas).
        Uma classe de dados CakeShop que guarda o stock numa lista, com um método para descobrir uma variedade de bolo (se não existir, retorna None), e outro método para efetuar a venda duma variedade de bolo (se não existir, retorna None).
        Uma classe de dados UI (user interface) que implementa a interação com o utilizador. Cria-se apenas uma instância desta classe de dados. Essa instância precisa de guardar uma CakeShop. A classe de dados UI preocupa-se apenas com a interação com o utilizador, respeitando as responsabilidades das outras classes de dados.

        Quatro data classes, umas dentro das outras:
            UI
            └── CakeShop            stock: list[CakeVariety]
                └── CakeVariety     name, amount, ingredients: list[Ingredient]
                    └── Ingredient  name, percent
    
        ou seja: 
            Uma pastelaria tem várias variedades de bolos.
            Cada variedade tem: nome, quantas unidades há, e a lista dos ingredientes.
            Cada ingrediente tem: nome e percentagem na receita.
    
    Ao contrário do exercício anterior, que fornece simples operações sobre frações, 
    este programa tem estado interno porque precisa de gerir a situação corrente do stock da pastelaria, 
    o qual pode variar ao longo do tempo. 
    Uma pastelaria é um objeto mutável.


    Agora:
        cake_variety_main_ingredient -> o ingrediente com maior percentagem
                                        (None se não houver ingredientes)
        cake_variety_sell            -> vende e devolve quantas vendeu MESMO
                                        (pode haver menos bolos do que os pedidos)
"""

from dataclasses import dataclass

#########   type Ingredient   #########

@dataclass
class Ingredient:
    name: str
    percent: float

# Sem funções. Basta usar a função de construção.


#########   type CakeVariety   #########

@dataclass
class CakeVariety:
    name: str
    amount: int
    ingredients: list[Ingredient]

def cake_variety_main_ingredient(variety: CakeVariety) -> Ingredient | None:
    """ The main ingredient of the recipe of a cake variety. """
    pass # TODO


def cake_variety_sell(variety: CakeVariety, units: int) -> int:
    """ Sell some units.
        Precondition: units >= 0 """
    pass #TODO

delicia = CakeVariety("Delícia-compacta", 127,
                      [Ingredient("Farinha", 0.99), Ingredient("Leite", 0.01)])

print(cake_variety_main_ingredient(delicia).name)   # Farinha
print(cake_variety_sell(delicia, 100))              # 100
print(delicia.amount)                               # 27
print(cake_variety_sell(delicia, 100))              # 27    só havia 27
print(delicia.amount)                               # 0

puro_ar = CakeVariety("Puro-ar", 230, [])
print(cake_variety_main_ingredient(puro_ar))        # None

# O main_ingredient devolve o objeto Ingredient inteiro, não só o nome.
# Quem chama escolhe o que quer dele (.name, .percent).
#
# O sell é o contrário do date_next: aqui alterar é o que se QUER.
# A variedade tinha 127 bolos; depois de vender, tem menos. É a realidade.






# %%
"""
h) EXERCICIO 51 (GUIÃO) - CakeShop e a UI [9 min -> 12:38] -----------------

    Precisa da célula g).

        cake_shop_find_variety -> procura uma variedade pelo nome (None se não existir)
        cake_shop_sell         -> vende (None se a variedade não existir)
"""

#########   type CakeShop   #########

@dataclass
class CakeShop:
    stock: list[CakeVariety]

def cake_shop_size(shop: CakeShop) -> int:
    """ Number of cake varieties in the shop. """
    return len(shop.stock)

def cake_shop_find_variety(shop: CakeShop, vname: str) -> CakeVariety | None:
    """ Find a cake variety by name. """
    pass #TODO



def cake_shop_sell(shop: CakeShop, vname: str, units: int) -> int | None:
    """ Sell some units of a named cake variety.
        Precondition: units >= 0 """
    pass #TODO

# Experiência antes da UI: vender duas vezes na mesma loja.
shop = CakeShop([CakeVariety("Delícia-compacta", 127, [Ingredient("Farinha", 0.99)])])
print(cake_shop_sell(shop, "Delícia-compacta", 100))   # 100
print(cake_shop_sell(shop, "Delícia-compacta", 100))   # 27
print(cake_shop_sell(shop, "Pastel-de-nata", 5))       # None
print(shop.stock[0].amount)                            # 0

# Como é que vender pelo find mudou a loja?
# O find devolve o MESMO objeto que está dentro da lista, não uma cópia.
# (é o p e o p1 do point_move_in_place, na secção d) da 1ª parte)
# Aqui dá jeito: alterar a variedade encontrada é alterar a loja.
#
# O return dentro do for: o "procura e devolve" que já conhecem das listas.
# O return None fica FORA do for, com a indentação do for. Se ficasse dentro,
# desistia logo à primeira variedade que não fosse a certa.


"""
    #########   User Interface   #########

    Exemplo de execução
        Puro-açúcar 230 4
        Leite-Condensado 0.1
        Açúcar 0.76
        Farinha 0.12
        Fermento 0.02
        Delícia-compacta 127 2
        Farinha 0.99
        Leite 0.01
        fim
        BOLO: Delícia-compacta
        UNIDADES: 1000
        Vendidas 127 unidades de Delícia-compacta
        Ingrediente principal: Farinha

    Exemplo de execução
        Puro-ar 230 0
        fim
        BOLO: Puro-ar
        UNIDADES: 100
        Vendidas 100 unidades de Puro-ar
        Ingrediente principal: Sem ingredientes
"""

# No validation here. We are assuming all the inputs are valid (to simplify).

@dataclass
class UI:
    shop: CakeShop

def ui_make_ingredient(line: str) -> Ingredient:
    """ Create an ingredient from an input line. """
    name, percentage = line.split()
    return Ingredient(name, float(percentage))

def ui_make_cake_variety(line: str) -> CakeVariety:
    """ Create a cake variety ingredient from an input line plus ingredient lines. """
    name, amount, n_ingredients = line.split()
    n_ingredients = int(n_ingredients)
    l = []
    for _ in range(n_ingredients):
        line = input()
        i = ui_make_ingredient(line)
        l.append(i)
    return CakeVariety(name, int(amount), l)

def ui_input_shop() -> CakeShop:
    """ Load the shop with data read from the keyboard. """
    END_MARK = "fim"
    l = []
    while True:
        line = input()
        if line == END_MARK:    # saída do ciclo pelo meio
            break
        cake = ui_make_cake_variety(line)
        l.append(cake)
    return CakeShop(l)

def ui_process(ui: UI, name: str, units: int) -> None:
    """ User interface for selling some units of a cake variety.
        Precondition: units >= 0 """
    sold = cake_shop_sell(ui.shop, name, units)
    if sold is None:
        print(f"Não existe {name}")
    else:
        print(f"Vendidas {sold} unidades de {name}")
        cake = cake_shop_find_variety(ui.shop, name)
        ingr = cake_variety_main_ingredient(cake)
        if ingr is None:
            print("Ingrediente principal: Sem ingredientes")
        else:
            print(f"Ingrediente principal: {ingr.name}")

def ui_run(ui: UI) -> None:
    """ Run the user interface. """
    name = input("BOLO: ")            # pergunta o nome do bolo
    units = int(input("UNIDADES: "))  # pergunta a quantidade
    ui_process(ui, name, units)       # processa o pedido


############# main #############

def main() -> None:
    ui = UI(ui_input_shop())
    ui_run(ui)

main()

# Vendidas 127 unidades de Delícia-compacta
# Ingrediente principal: Farinha
#
# 2º exemplo do guião: "Puro-ar 230 0", "fim", BOLO: Puro-ar, UNIDADES: 100
#   -> Vendidas 100 unidades de Puro-ar
#   -> Ingrediente principal: Sem ingredientes
#
# Cada um no seu papel:
#   Ingredient, CakeVariety, CakeShop -> a lógica da loja (sem input nem print)
#   UI                                -> só lê, chama as funções certas e escreve
# A UI guarda a loja (ui.shop): é a única coisa que o programa precisa de lembrar.
#
# Para o Mooshak (U): a célula g) mais esta, num só ficheiro.










# %%
"""
i) E as classes "a sério" dos testes de anos anteriores? [4 min -> 12:42] ---

    Nos testes de anos anteriores vão encontrar "class" SEM o @dataclass,
    com funções lá DENTRO da classe (métodos), com um parâmetro self.
    Não estranhem: este ano a cadeira simplificou e usa só data classes
    com funções cá fora. A ideia é a mesma, só muda a forma de escrever.

    A tradução é quase direta:
        date_is_less(d1, d2)   (o nosso estilo: função normal)
        d1.is_less(d2)         (estilo classe: método, como l.append(4))
    O self é o objeto que vem antes do ponto: o nosso 1º parâmetro.
"""

# Extra (só para reconhecerem, não é matéria deste ano).
# Chama-se DateClass para não estragar a Date do ex 49 (que ainda vamos usar).

class DateClass:
    def __init__(self, day, month, year):    # a construtora, escrita à mão
        self.day = day
        self.month = month
        self.year = year

    def is_less(self, other):                # o nosso date_is_less
        if self.year != other.year:
            return self.year < other.year
        elif self.month != other.month:
            return self.month < other.month
        else:
            return self.day < other.day

    def __lt__(self, other):                 # método mágico: é o que o < chama
        return self.is_less(other)

d1 = DateClass(30, 1, 2026)
d2 = DateClass(1, 2, 2026)
print(d1.is_less(d2))           # True
print(d1 < d2)                  # True    com o __lt__, o < já funciona!

print(DateClass(1, 1, 2000) == DateClass(1, 1, 2000))   # False   !!!
print(d1)                       # <__main__.DateClass object at 0x...>   (o número muda)

# Numa class normal, a construtora (__init__), o print bonito (__repr__)
# e o == (__eq__) têm de ser escritos à mão.
# Sem __eq__, o == só dá True se for o MESMO objeto: daí o False de cima.
# O @dataclass escreve estes três por nós:
# é por isso que um Person("Maria", ...), um print(maria) e o == funcionam logo.
#
# E o < ? É o método mágico __lt__ ("less than"), como na teórica 7b:
#   d1 < d2   --->   d1.__lt__(d2)
# Os testes antigos faziam isto: escreviam o __lt__ e depois usavam o < à vontade.
# Nós, sem métodos, escrevemos date_is_less(d1, d2). É a mesma conta.
#
# Extra do extra: o @dataclass(order=True) também escreve o <, mas compara
# os atributos PELA ORDEM em que estão na classe. Na nossa Date (day, month, year)
# compararia o dia primeiro: 30/1 < 1/2 dava False. Errado!
# Por isso, mesmo podendo, a função escrita por nós é a escolha segura.
#
# E herança (class Circle(Shape), "um círculo É uma forma")? Existe,
# também nas data classes, mas fica fora de IPCE.








# %%
"""
j) Erros comuns [9 min -> 12:51] -------------------------------------------

    Cada célula a seguir tem um erro típico.
    Os que rebentam estão comentados: descomentem, corram, leiam a mensagem.
    Ler a mensagem de erro com calma é meio caminho para o corrigir.
"""

# Erro 1: esquecer o @dataclass

class Point:                     # sem @dataclass por cima
    x: float
    y: float

# p = Point(3, 4)
# TypeError: Point() takes no arguments
# Sem o @dataclass, o Python não escreve a construtora.


# Erro 2: esquecer o import (só se vê num ficheiro sem o "from dataclasses import dataclass";
#         aqui no Spyder o import já foi feito por células de cima, por isso não rebentava)
#
# @dataclass
# class Point:
#     x: float
#     y: float
# NameError: name 'dataclass' is not defined


# Erro 3: argumentos a menos (ou a mais)
from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

# p = Point(3)
# TypeError: Point.__init__() missing 1 required positional argument: 'y'


# Erro 4: um atributo com valor por omissão ANTES de um sem (descomentem e corram)
#
# @dataclass
# class Person:
#     email: str = ""
#     name: str
# TypeError: non-default argument 'name' follows default argument
# Rebenta logo ao definir a classe, antes de criar qualquer pessoa.
# Os com valor por omissão vão para o fim.






# %%
"""
    Erro 5: uma gralha no nome do atributo. 
    
    Este é o pior: NÃO dá erro!!!
    
    Antes de correr: onde mora a Maria?
"""

from dataclasses import dataclass

@dataclass
class Person:
    name: str
    year: int
    address: str
    email: str = ""

maria = Person("Maria", 1990, "Coimbra")
maria.ad_ress = "Faro"          # adress, com um d só!

print(maria.address)            # Coimbra   a mudança não foi para a morada!
print(maria)                    # Person(name='Maria', year=1990, address='Coimbra', email='')

print(maria.ad_ress)             # AttributeError: 'Person' object has no attribute 'adress'

# Ao alterar, o Python não confirma se o atributo existe:
# criou um atributo NOVO "ad_ress" e a morada ficou igual.
# Se a morada não muda quando devia, procurem uma gralha.
# (ao LER é diferente: print(maria.adres) já dava AttributeError)






# %%
"""
    Erro 6: os tipos da classe não são verificados.
    Precisa da Person da célula do Erro 5 (corram-na primeiro).
"""

ana = Person("Ana", "1990", "Lisboa")       # o ano vai como str, sem queixas
print(ana)                      # Person(name='Ana', year='1990', address='Lisboa', email='')

# O erro só aparece mais tarde, quando se usa:
# print(2026 - ana.year)
# TypeError: unsupported operand type(s) for -: 'int' and 'str'
#
# O "year: int" é uma promessa para quem lê, não uma regra que o Python obrigue.
# (o Python só descobre os tipos quando o programa corre: tipagem dinâmica)
# Muito comum com input(): lembrem-se do int(...).






# %%
"""
    Erro 7: achar que "=" faz uma cópia.

    Não faz. Com objetos é como com as listas:
    várias variáveis (ou parâmetros de funções) podem apontar para o MESMO objeto,
    e uma alteração por qualquer uma delas vê-se em todas.

    Para ter uma cópia, cria-se um objeto NOVO com os mesmos atributos
    (por exemplo, com uma função point_copy).
"""

from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

p1 = Point(3, 4)
p2 = p1                         # NÃO é uma cópia: é outro nome para o MESMO ponto
p2.x = 10
print(p1.x)                     # 10    o p1 também mudou!






# %%
# Uma cópia a sério é um ponto NOVO com as mesmas peças:
# (precisa do Point da célula de cima)

def point_copy(p: Point) -> Point:
    """ Copy a point. """
    return Point(p.x, p.y)

p1 = Point(3, 4)
p2 = point_copy(p1)             # o p2 é uma cópia do p1
p2.x = 10
print(p1)                       # Point(x=3, y=4)    o p1 ficou igual
print(p2)                       # Point(x=10, y=4)

# É o mesmo que acontece com as listas: l2 = l1 não copia a lista.
# E o mesmo que vimos no point_move_in_place e no find da pastelaria:
# às vezes é o que se quer (vender pela variedade encontrada),
# às vezes é um bug (o date_next_bug a mudar o max).






# %%
"""
    E os erros que já apanhámos hoje pelo caminho:

    - comparar com < :          TypeError. Escrever uma função (date_is_less).
    - print(objeto) na UI:      sai Date(day=31, ...). Para o utilizador, usar o _as_str.
    - mudar o que se recebe:    o date_next_bug. Se a função é "dá-me um novo", criem um novo.
    - esquecer o caso vazio:    dates_maximum de uma lista vazia, ou main_ingredient
                                sem ingredientes. Pensar sempre: e se não houver nenhum?
    - o caso que nunca acaba:   o gcd com números negativos (daí o abs no normalize).
    - comparar com None:        "is None", não "== None".
"""









# %%
"""
k) EXERCICIO - Uma turma [9 min -> 13:00] ---------------------------------

    Tudo junto, à maneira da pastelaria.

    Student: name (str), number (int), grades (list[float])
    Course:  name (str), students (list[Student])

    1. student_average(s)              -> a média das notas (0.0 se não houver notas)
    2. course_find_student(c, number)  -> o aluno com esse número, ou None
    3. course_best_student(c)          -> o aluno com melhor média (None se a turma for vazia)
    4. student_add_grade(s, grade)     -> junta uma nota ao aluno (altera o aluno)
"""

from dataclasses import dataclass

@dataclass
class Student:
    name: str
    number: int
    grades: list[float]

@dataclass
class Course:
    name: str
    students: list[Student]

def student_average(s: Student) -> float:
    """ Average of the student's grades (0.0 if no grades). """
    if len(s.grades) == 0:
        return 0.0
    total = 0
    for g in s.grades:
        total += g
    return total / len(s.grades)

def course_find_student(c: Course, number: int) -> Student | None:
    """ The student with that number, or None. """
    for s in c.students:
        if s.number == number:
            return s
    return None

def course_best_student(c: Course) -> Student | None:
    """ The student with the best average (None if no students). """
    if len(c.students) == 0:
        return None
    best = c.students[0]
    for s in c.students:
        if student_average(s) > student_average(best):
            best = s
    return best

def student_add_grade(s: Student, grade: float) -> None:
    """ Add a grade to the student. """
    s.grades.append(grade)

ipce = Course("IPCE", [Student("Ana", 70001, [15, 17]),
                       Student("Rui", 70002, [12, 14, 13]),
                       Student("Eva", 70003, [])])

print(student_average(ipce.students[0]))            # 16.0
print(course_find_student(ipce, 70002).name)        # Rui
print(course_find_student(ipce, 99999))             # None
print(course_best_student(ipce).name)               # Ana

student_add_grade(course_find_student(ipce, 70003), 20)
print(course_best_student(ipce).name)               # Eva

# É a receita da pastelaria:
#   procurar (find) -> for + return lá dentro + return None no fim
#   o melhor (best) -> começar no primeiro e comparar com todos
#   alterar         -> pelo objeto encontrado, que é o mesmo que está na lista






# %%
"""
===========================================================================
CONSOLIDAR - para casa
===========================================================================

    Tentem primeiro sem olhar para a solução.
    Do mais simples (uma classe sozinha) ao estilo de teste (várias classes juntas).
"""






# %%
"""
EXERCICIO (EXTRA) 1 - Parábolas ---------------------------------------------

    Uma parábola y = a*x**2 + b*x + c guarda-se com os seus 3 coeficientes.

    Escrevam a classe Parabola (a, b, c: float) e:
        parabola_value(p, x)  -> o y para esse x
        parabola_vertex(p)    -> o vértice, como Point (x = -b / (2*a))

    p = Parabola(1, -4, 3)
    parabola_value(p, 0) == 3
    parabola_value(p, 1) == 0
    parabola_vertex(p)   == Point(2.0, -1.0)
"""

from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

@dataclass
class Parabola:
    pass        # TODO: os 3 atributos a, b, c

def parabola_value(p: Parabola, x: float) -> float:
    """ The y of the parabola for this x. """
    pass        # TODO

def parabola_vertex(p: Parabola) -> Point:
    """ The vertex of the parabola.
        Precondition: p.a != 0 """
    pass        # TODO












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: parábolas

from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

@dataclass
class Parabola:
    a: float
    b: float
    c: float

def parabola_value(p: Parabola, x: float) -> float:
    """ The y of the parabola for this x. """
    return p.a * x ** 2 + p.b * x + p.c

def parabola_vertex(p: Parabola) -> Point:
    """ The vertex of the parabola.
        Precondition: p.a != 0 """
    x = -p.b / (2 * p.a)
    return Point(x, parabola_value(p, x))      # reaproveitar a função de cima

p = Parabola(1, -4, 3)
print(parabola_value(p, 0), parabola_value(p, 1))   # 3 0
print(parabola_vertex(p))                           # Point(x=2.0, y=-1.0)
print(parabola_vertex(p) == Point(2.0, -1.0))       # True

# O y do vértice não precisa de fórmula nova: é o valor da parábola nesse x.






# %%
"""
EXERCICIO (EXTRA) 2 - Mover um polígono -------------------------------------

    Os dois estilos da aula, agora com um polígono:
        polygon_moved(p, dx, dy)          -> um polígono NOVO, deslocado;
                                             o original fica igual
        polygon_move_in_place(p, dx, dy)  -> desloca o próprio polígono (-> None)

    t = Polygon([Point(0, 0), Point(3, 0), Point(0, 4)])
    t2 = polygon_moved(t, 1, 1)
    t2 -> Polygon(vertices=[Point(x=1, y=1), Point(x=4, y=1), Point(x=1, y=5)])
    t  -> igual ao que era

    Cuidado: no polygon_moved, os PONTOS também têm de ser novos.
"""

from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

@dataclass
class Polygon:
    vertices: list[Point]

def polygon_moved(p: Polygon, dx: float, dy: float) -> Polygon:
    """ A NEW polygon, moved by dx and dy. """
    pass        # TODO

def polygon_move_in_place(p: Polygon, dx: float, dy: float) -> None:
    """ Move the polygon itself by dx and dy. """
    pass        # TODO

t = Polygon([Point(0, 0), Point(3, 0), Point(0, 4)])
t2 = polygon_moved(t, 1, 1)
print(t2)
print(t)











# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: mover um polígono

from dataclasses import dataclass

@dataclass
class Point:
    x: float
    y: float

@dataclass
class Polygon:
    vertices: list[Point]

def polygon_moved(p: Polygon, dx: float, dy: float) -> Polygon:
    """ A NEW polygon, moved by dx and dy. """
    new_vertices = []
    for v in p.vertices:
        new_vertices.append(Point(v.x + dx, v.y + dy))     # cada ponto é novo
    return Polygon(new_vertices)

def polygon_move_in_place(p: Polygon, dx: float, dy: float) -> None:
    """ Move the polygon itself by dx and dy. """
    for v in p.vertices:
        v.x += dx               # o v é o próprio ponto que está na lista
        v.y += dy

t = Polygon([Point(0, 0), Point(3, 0), Point(0, 4)])
t2 = polygon_moved(t, 1, 1)
print(t2)       # Polygon(vertices=[Point(x=1, y=1), Point(x=4, y=1), Point(x=1, y=5)])
print(t)        # Polygon(vertices=[Point(x=0, y=0), Point(x=3, y=0), Point(x=0, y=4)])

polygon_move_in_place(t, 10, 0)
print(t)        # Polygon(vertices=[Point(x=10, y=0), Point(x=13, y=0), Point(x=10, y=4)])


# A armadilha: um polígono "novo" que reaproveita a lista do antigo
def polygon_moved_bug(p: Polygon, dx: float, dy: float) -> Polygon:
    """ A NEW polygon, moved by dx and dy (with a bug!). """
    result = Polygon(p.vertices)        # o Polygon é novo, mas a lista é a MESMA
    polygon_move_in_place(result, dx, dy)
    return result

t = Polygon([Point(0, 0), Point(3, 0), Point(0, 4)])
t3 = polygon_moved_bug(t, 1, 1)
print(t)        # Polygon(vertices=[Point(x=1, y=1), Point(x=4, y=1), Point(x=1, y=5)])   o t mudou!

# O Polygon(...) cria um polígono novo, mas o que vai lá para dentro
# é a mesma lista, com os mesmos pontos. Mexer num é mexer no outro.
# Um objeto "novo" a sério tem de ser novo até ao fundo: lista nova, pontos novos.






# %%
"""
EXERCICIO (EXTRA) 3 - Mais frações ------------------------------------------

    Precisa da célula e) da 2ª parte (o Rational, o gcd e o rational_create).

        rational_as_float(r)    -> 3/4 dá 0.75
        rational_is_less(r1, r2) -> r1 < r2 ?
                                   (com denominadores positivos: a/b < c/d  <=>  a*d < c*b)
        rational_sum(l)         -> a soma de uma lista de frações (0/1 se a lista for vazia)

    rational_as_float(Rational(3, 4)) == 0.75
    rational_is_less(Rational(1, 3), Rational(1, 2)) == True
    rational_is_less(Rational(-1, 2), Rational(-1, 3)) == True
    rational_sum([Rational(1, 2), Rational(1, 3), Rational(1, 6)]) -> 1/1
"""

def rational_as_float(r: Rational) -> float:
    """ The rational as a float. """
    pass        # TODO

def rational_is_less(r1: Rational, r2: Rational) -> bool:
    """ Is r1 < r2?
        Precondition: rational_is_valid(r1) and rational_is_valid(r2) """
    pass        # TODO

def rational_sum(l: list[Rational]) -> Rational:
    """ Sum of a list of rationals. """
    pass        # TODO











# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: mais frações (precisa da célula e) da 2ª parte)

def rational_as_float(r: Rational) -> float:
    """ The rational as a float. """
    return r.num / r.den

def rational_is_less(r1: Rational, r2: Rational) -> bool:
    """ Is r1 < r2?
        Precondition: rational_is_valid(r1) and rational_is_valid(r2) """
    return r1.num * r2.den < r2.num * r1.den

def rational_sum(l: list[Rational]) -> Rational:
    """ Sum of a list of rationals. """
    total = rational_create(0, 1)
    for r in l:
        total = rational_add(total, r)      # cada soma dá uma fração NOVA
    return total

def rational_add(r1: Rational, r2: Rational) -> Rational:
    """ Add two rational numbers. """
    return rational_create(r1.num * r2.den + r2.num * r1.den, r1.den * r2.den)

print(rational_as_float(Rational(3, 4)))                        # 0.75
print(rational_is_less(Rational(1, 3), Rational(1, 2)))         # True
print(rational_is_less(Rational(-1, 2), Rational(-1, 3)))       # True
print(rational_as_str(rational_sum([Rational(1, 2), Rational(1, 3), Rational(1, 6)])))  # 1/1
print(rational_as_str(rational_sum([])))                        # 0/1

# Porquê não comparar com os floats (rational_as_float(r1) < rational_as_float(r2))?
# Funciona quase sempre, mas os floats arredondam (0.1 + 0.2 da aula 4).
# Com a multiplicação cruzada é tudo com int: exato.
# (o rational_add está aqui outra vez para esta célula correr sozinha depois da e))






# %%
"""
EXERCICIO (EXTRA) 4 - Contar dias entre datas (com while) -------------------

    Precisa das células g) e j) da 1ª parte (Date, date_next...).

        date_add_days(dt, n)       -> a data n dias depois (n >= 0)
        date_days_between(d1, d2)  -> quantos dias de d1 até d2 (d1 <= d2)

    date_add_days(Date(30, 12, 2026), 3)  -> 2/1/2027
    date_days_between(Date(1, 1, 2026), Date(1, 3, 2026)) == 59
    date_days_between(Date(1, 1, 2024), Date(1, 3, 2024)) == 60   (bissexto)
    date_days_between(Date(5, 5, 2026), Date(5, 5, 2026)) == 0

    Pista: usem o date_next. No segundo, não se sabe quantas voltas: while.
"""

def date_add_days(dt: Date, n: int) -> Date:
    """ The date n days after dt.
        Precondition: n >= 0 """
    pass        # TODO

def date_days_between(d1: Date, d2: Date) -> int:
    """ Number of days from d1 to d2.
        Precondition: d1 == d2 or date_is_less(d1, d2) """
    pass        # TODO











# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: contar dias entre datas (precisa das células g) e j) da 1ª parte)

def date_add_days(dt: Date, n: int) -> Date:
    """ The date n days after dt.
        Precondition: n >= 0 """
    result = dt
    for _ in range(n):              # sabe-se quantas voltas: for
        result = date_next(result)
    return result

def date_days_between(d1: Date, d2: Date) -> int:
    """ Number of days from d1 to d2.
        Precondition: d1 == d2 or date_is_less(d1, d2) """
    count = 0
    d = d1
    while d != d2:                  # não se sabe quantas voltas: while
        d = date_next(d)
        count += 1
    return count

print(date_as_str(date_add_days(Date(30, 12, 2026), 3)))         # 2/1/2027
print(date_days_between(Date(1, 1, 2026), Date(1, 3, 2026)))     # 59
print(date_days_between(Date(1, 1, 2024), Date(1, 3, 2024)))     # 60
print(date_days_between(Date(5, 5, 2026), Date(5, 5, 2026)))     # 0

# O != entre datas funciona: é o contrário do == que o @dataclass nos dá.
# A pergunta da paragem (aula 7): o d anda sempre um dia para a frente,
# por isso chega ao d2... desde que o d2 não seja ANTES do d1.
# Com d2 antes de d1, o while nunca acabava: é por isso que existe a pré-condição.
# O result = dt no date_add_days não estraga o dt: o date_next cria sempre datas novas.






# %%
"""
EXERCICIO (EXTRA) 5 - Mais pastelaria ---------------------------------------

    Precisa da célula g) da 2ª parte (Ingredient e CakeVariety).

        cake_variety_is_valid(v)         -> as percentagens somam 1 (100%)?
        cake_shop_total_units(shop)      -> quantos bolos há na loja, ao todo
        cake_shop_restock(shop, vname, units) -> junta units bolos a essa variedade;
                                            se a variedade não existir, cria-a sem ingredientes

    Cuidado no is_valid: são floats. Somar e comparar com == 1 pode falhar.
"""

@dataclass
class CakeShop:
    stock: list[CakeVariety]

def cake_variety_is_valid(v: CakeVariety) -> bool:
    """ Do the percentages add up to 1? """
    pass        # TODO

def cake_shop_total_units(shop: CakeShop) -> int:
    """ Total number of cakes in the shop. """
    pass        # TODO

def cake_shop_restock(shop: CakeShop, vname: str, units: int) -> None:
    """ Add units cakes to the variety (create it if it does not exist).
        Precondition: units >= 0 """
    pass        # TODO











# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: mais pastelaria (precisa da célula g) da 2ª parte)

@dataclass
class CakeShop:
    stock: list[CakeVariety]

def cake_shop_find_variety(shop: CakeShop, vname: str) -> CakeVariety | None:
    """ Find a cake variety by name. """
    for variety in shop.stock:
        if variety.name == vname:
            return variety
    return None

def cake_variety_is_valid(v: CakeVariety) -> bool:
    """ Do the percentages add up to 1? """
    total = 0.0
    for ingr in v.ingredients:
        total += ingr.percent
    return abs(total - 1.0) < 0.000001      # "quase igual", por causa dos floats

def cake_shop_total_units(shop: CakeShop) -> int:
    """ Total number of cakes in the shop. """
    total = 0
    for variety in shop.stock:
        total += variety.amount
    return total

def cake_shop_restock(shop: CakeShop, vname: str, units: int) -> None:
    """ Add units cakes to the variety (create it if it does not exist).
        Precondition: units >= 0 """
    variety = cake_shop_find_variety(shop, vname)
    if variety is None:
        shop.stock.append(CakeVariety(vname, units, []))
    else:
        variety.amount += units

shop = CakeShop([CakeVariety("Puro-açúcar", 230, [Ingredient("Leite-Condensado", 0.1),
                                                  Ingredient("Açúcar", 0.76),
                                                  Ingredient("Farinha", 0.12),
                                                  Ingredient("Fermento", 0.02)]),
                 CakeVariety("Delícia-compacta", 127, [Ingredient("Farinha", 0.99),
                                                       Ingredient("Leite", 0.01)])])

print(cake_variety_is_valid(shop.stock[0]))                     # True
print(cake_variety_is_valid(CakeVariety("Mau", 1, [Ingredient("Ovo", 0.5)])))   # False
print(cake_shop_total_units(shop))                              # 357
cake_shop_restock(shop, "Delícia-compacta", 3)
cake_shop_restock(shop, "Pastel-de-nata", 50)
print(cake_shop_total_units(shop))                              # 410
print(shop.stock[2])            # CakeVariety(name='Pastel-de-nata', amount=50, ingredients=[])

# O restock reaproveita o find: procurar uma vez, e depois ou cria ou altera.
# E o 0.1 + 0.2 da aula 4 é a razão do "quase igual" no is_valid.






# %%
"""
EXERCICIO (EXTRA) 6 - Estilo teste: a biblioteca ----------------------------

    Uma biblioteca empresta livros.

    Book:    title (str), author (str), available (bool, por omissão True)
    Library: books (list[Book])

        library_find(lib, title)              -> o livro com esse título, ou None
        library_lend(lib, title)              -> empresta: devolve True se conseguiu
                                                 (False se não existe ou já está emprestado)
        library_give_back(lib, title)         -> devolve o livro (fica disponível)
        library_available_by(lib, author)     -> lista dos títulos DISPONÍVEIS desse autor

    lib = Library([Book("Os Maias", "Eça de Queirós"),
                   Book("O Primo Basílio", "Eça de Queirós"),
                   Book("Mensagem", "Fernando Pessoa")])
    library_lend(lib, "Os Maias")       == True
    library_lend(lib, "Os Maias")       == False     (já está emprestado)
    library_lend(lib, "Ensaio")         == False     (não existe)
    library_available_by(lib, "Eça de Queirós") == ['O Primo Basílio']
"""

from dataclasses import dataclass

@dataclass
class Book:
    title: str
    author: str
    available: bool = True

@dataclass
class Library:
    books: list[Book]

def library_find(lib: Library, title: str) -> Book | None:
    """ The book with that title, or None. """
    pass        # TODO

def library_lend(lib: Library, title: str) -> bool:
    """ Lend a book. True if it was possible. """
    pass        # TODO

def library_give_back(lib: Library, title: str) -> None:
    """ Give back a book.
        Precondition: library_find(lib, title) is not None """
    pass        # TODO

def library_available_by(lib: Library, author: str) -> list[str]:
    """ Titles of the available books by that author. """
    pass        # TODO











# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: a biblioteca

from dataclasses import dataclass

@dataclass
class Book:
    title: str
    author: str
    available: bool = True

@dataclass
class Library:
    books: list[Book]

def library_find(lib: Library, title: str) -> Book | None:
    """ The book with that title, or None. """
    for b in lib.books:
        if b.title == title:
            return b
    return None

def library_lend(lib: Library, title: str) -> bool:
    """ Lend a book. True if it was possible. """
    b = library_find(lib, title)
    if b is None or not b.available:
        return False
    b.available = False
    return True

def library_give_back(lib: Library, title: str) -> None:
    """ Give back a book.
        Precondition: library_find(lib, title) is not None """
    b = library_find(lib, title)
    b.available = True

def library_available_by(lib: Library, author: str) -> list[str]:
    """ Titles of the available books by that author. """
    result = []
    for b in lib.books:
        if b.author == author and b.available:
            result.append(b.title)
    return result

lib = Library([Book("Os Maias", "Eça de Queirós"),
               Book("O Primo Basílio", "Eça de Queirós"),
               Book("Mensagem", "Fernando Pessoa")])
print(library_lend(lib, "Os Maias"))                    # True
print(library_lend(lib, "Os Maias"))                    # False
print(library_lend(lib, "Ensaio"))                      # False
print(library_available_by(lib, "Eça de Queirós"))      # ['O Primo Basílio']
library_give_back(lib, "Os Maias")
print(library_available_by(lib, "Eça de Queirós"))      # ['Os Maias', 'O Primo Basílio']

# No library_lend, a ordem do "or" importa:
#   b is None or not b.available
# Se b for None, o or já é True e nem olha para b.available
# (que dava erro: o None não tem atributos).
# Trocado (not b.available or b is None) rebentava com um título que não existe.






# %%
"""
===========================================================================
PARA TREINAR MAIS (em casa)
===========================================================================

    Os exercícios 49, 50 e 51 sem olhar para a solução, e submeter no Mooshak (S, T, U).
    O 51 é o mais parecido com o que aparece nos testes: várias classes,
    umas dentro das outras, procurar, alterar, e uma UI.

    Nos testes de anos anteriores, as classes vêm "a sério" (com métodos):
    é só traduzir para o nosso estilo (secção i) da 2ª parte).
"""