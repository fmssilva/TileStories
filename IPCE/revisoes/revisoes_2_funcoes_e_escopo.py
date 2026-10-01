# %%
"""
===========================================================================
REVISÕES 2 - FUNÇÕES E ESCOPO
IPCE 2026/2027
===========================================================================

Antes deste ficheiro: revisoes_1_variaveis_tipos_input.py
(variáveis, tipos, operadores, print/input e conversões)

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

  5. Funções .................................................. [~60 min]
       Porquê: nos testes, quase todas as perguntas são
       "escreva uma função que...". É A peça mais importante da cadeira.

  6. Escopo: onde é que cada variável existe .................. [~35 min]
       Porquê: explica erros estranhos como NameError e
       UnboundLocalError, e aparece em perguntas de "o que escreve este código?".

  Resumo + soluções dos desafios

  Total: ~1h40.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Corre as células POR ORDEM (partilham a memória).
  - Níveis:
      [EXEMPLO]  resolvido: lê, corre, muda valores.
      [FAZ]      contigo. Solução na célula a seguir, depois de muito espaço.
      [PENSA]    contigo. A "solução" é só a ideia, sem código.
      [DESAFIO]  nível de teste. Solução no fim do ficheiro.
      [SOZINHA]  sem solução.
  - Se encravares: email ao professor das práticas com o número do exercício,
    o que já fizeste, onde encravaste e o ficheiro em anexo.

  A numeração continua a do ficheiro 1: este começa na secção 5.
  Quando aparecer "Ex 2.3 (ficheiro 1)", é o exercício 2.3 do ficheiro 1.
"""






# %%
"""
===========================================================================
5. FUNÇÕES [~60 min]
===========================================================================

Uma função é um pedaço de código com nome, que recebe valores,
faz um cálculo e devolve um resultado.
Escreve-se uma vez e usa-se as vezes que quisermos.

Ex 5.1 [EXEMPLO] A primeira função ----------------------------------------
"""

def square(x: int) -> int:
    """ Square of an integer. """
    return x * x

print(square(3))                    # 9
print(square(10))                   # 100
result = square(4) + square(5)     # 16 + 25
print(result)                       # 41

# Peça a peça:
#   def                -> "vou definir uma função"
#   square             -> o nome da função
#   (x: int)           -> o PARÂMETRO: a função recebe um valor e chama-lhe x
#                         ": int" diz que se espera um inteiro
#   -> int             -> o tipo do resultado (a função devolve um inteiro)
#   """ ... """        -> a docstring: uma frase a dizer o que a função faz
#   return x * x       -> calcula x * x e DEVOLVE esse valor a quem chamou
#
# square(3) é uma CHAMADA da função:
#   1. o x recebe o valor 3;
#   2. o corpo da função corre;
#   3. o return devolve 9;
#   4. square(3) é substituído por 9 no sítio onde foi chamado.
#
# Os cabeçalhos com ": int" e "-> int" e a docstring são obrigatórios
# nesta cadeira. Os testes dão-te sempre o cabeçalho: tens de escrever o corpo.






# %%
"""
Ex 5.2 [EXEMPLO] Indentação: o que é corpo da função e o que não é ---------

    Antes de correr, tenta adivinhar a ORDEM das 3 linhas no output.
"""

def hello() -> None:
    """ Print a greeting. """
    print("Olá")
    print("dentro da função")
print("fora da função")

hello()

# Output:
#   fora da função
#   Olá
#   dentro da função
#
# O corpo da função são as linhas com 4 espaços à frente (indentação).
# O 3º print não tem espaços: NÃO faz parte da função. Corre logo.
#
# O "def" só DEFINE a função: guarda-a, não a corre.
# O corpo só corre quando a função é CHAMADA: hello().
#
# Em Python a indentação não é decoração: muda o significado do programa.
# Erros típicos (podes experimentar):
#   - linha do corpo sem espaços    -> IndentationError: expected an indented block
#   - espaços a mais sem motivo     -> IndentationError: unexpected indent
#
# "-> None" quer dizer: esta função não devolve nenhum valor.
# Ela faz uma AÇÃO (escrever no ecrã), não um cálculo.






# %%
"""
Ex 5.3 [EXEMPLO] Vários parâmetros: a ordem conta -------------------------
"""

def power(base: int, exponent: int) -> int:
    """ base raised to exponent. """
    return base ** exponent

print(power(2, 3))      # 8   base = 2, exponent = 3
print(power(3, 2))      # 9   base = 3, exponent = 2

# Os valores da chamada (ARGUMENTOS) vão para os parâmetros pela ordem:
# o 1º argumento para o 1º parâmetro, o 2º para o 2º.
#
# print(power(2))       # <- descomenta: TypeError: missing 1 required positional argument
# Tem de haver um argumento para cada parâmetro.






# %%
"""
Ex 5.4 [EXEMPLO] return vs print: A diferença mais importante --------------
"""

def double_print(x: int) -> None:
    """ Print the double of x. """
    print(x * 2)

def double_return(x: int) -> int:
    """ Double of x. """
    return x * 2

a = double_print(5)         # escreve 10 no ecrã...
b = double_return(5)        # não escreve nada...
print("a =", a)             # a = None  (!!)
print("b =", b)             # b = 10
print(b + 1)                # 11
# print(a + 1)              # <- descomenta: TypeError (None + 1)

# print   -> MOSTRA o valor a um humano. O valor não fica guardado em lado nenhum.
# return  -> DEVOLVE o valor a quem chamou a função, que o pode guardar e usar.
#
# Uma função sem return devolve None (o valor "nada").
# Por isso a ficou com None: double_print escreveu 10, mas não devolveu nada.
#
# Regra: funções que CALCULAM usam return, não print.
# Nos testes aparece sempre: "Não programe nenhuma função main, nem use input ou print."
# Uma função com print em vez de return nesses exercícios está ERRADA.






# %%
"""
Ex 5.5 [EXEMPLO] O return termina a função --------------------------------
"""

def test_return() -> int:
    """ Show that return ends the function. """
    print("antes do return")
    return 1
    print("depois do return")       # esta linha NUNCA corre

print(test_return())

# Output:
#   antes do return
#   1
#
# O return faz duas coisas:
#   1. devolve o valor;
#   2. termina a função ali mesmo. O que está a seguir é ignorado.






# %%
"""
Ex 5.6 [EXEMPLO] Os tipos no cabeçalho são só documentação ----------------
"""

def double(x: int) -> int:
    """ Double of x. """
    return x * 2

print(double(4))        # 8
print(double("ab"))     # abab  (!!) o Python não reclamou do ": int"
print(double(2.5))      # 5.0

# O Python IGNORA o ": int" e o "-> int". Não verifica nada.
# Só descobre se um tipo está mal quando a operação falha a correr.
# Chama-se TIPAGEM DINÂMICA: os tipos são verificados durante a execução,
# não antes. (em Java ou C, double("ab") nem chegava a correr)
#
# Então para que servem? Para os HUMANOS:
# quem lê o cabeçalho sabe logo o que entra e o que sai.
#
# Resumindo o Python: tipagem FORTE (não converte às escondidas, Ex 2.6 do ficheiro 1)
# e DINÂMICA (verifica os tipos só a correr).






# %%
"""
Ex 5.7 [EXEMPLO] Funções que usam outras funções --------------------------
"""

import math

def square_f(x: float) -> float:
    """ Square of a real. """
    return x * x

def hypotenuse(a: float, b: float) -> float:
    """ Hypotenuse of a right triangle with legs a and b.
        Precondition: a > 0 and b > 0
    """
    return math.sqrt(square_f(a) + square_f(b))

print(hypotenuse(3, 4))             # 5.0
print(square_f(square_f(2)))        # 16   -> square_f(4)

# Quando há chamadas dentro de chamadas, o Python calcula de DENTRO para FORA:
#   square_f(square_f(2)) -> square_f(4) -> 16
#
# hypotenuse(3, 4):
#   square_f(3) -> 9
#   square_f(4) -> 16
#   math.sqrt(25) -> 5.0
#
# import math dá acesso à biblioteca de matemática:
#   math.sqrt(x)  -> raiz quadrada
#   math.pi       -> 3.141592653589793 (é uma constante: sem parêntesis)
#   math.floor(x) -> arredonda para baixo
# Os import ficam no início do programa.
#
# PRECONDIÇÃO: uma linha na docstring que diz o que a função ASSUME.
# Aqui, assume catetos positivos. Se lhe derem -3, o problema é de quem chamou.
# Usa-se quando a função não funciona para todos os valores.






# %%
"""
Ex 5.8 [FAZ] Média de dois números (Teste 1 2025/26, 1a) --------------

    Parte 1: no teste aparecia esta função e perguntava-se o resultado
    de average(1.5, 3.5). Responde primeiro em comentário, sem correr.

        def average(a: float, b: float) -> float:
            return (a + b) / 2.0

    Parte 2: escreve a função (com docstring) e confirma a tua resposta.
"""

# Parte 1 - resposta:

# Parte 2:
def average(a: float, b: float) -> float:
    """ TODO """
    pass                # TODO: substitui o pass pelo return certo

print(average(1.5, 3.5))















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 5.8

def average(a: float, b: float) -> float:
    """ Average of two reals. """
    return (a + b) / 2.0

print(average(1.5, 3.5))        # 2.5

# (1.5 + 3.5) / 2.0 = 5.0 / 2.0 = 2.5
# Os parêntesis são essenciais: a + b / 2.0 dava 1.5 + 1.75 = 3.25.
#
# O "pass" é uma instrução que não faz nada.
# Serve para deixar um corpo "vazio" sem dar erro.
# Uma função só com pass devolve None.






# %%
"""
Ex 5.9 [FAZ] Funções simples ------------------------------------------

    Completa as três funções. Os prints dizem o resultado esperado.
"""

def triple(x: int) -> int:
    """ Triple of x. """
    pass        # TODO

def celsius_to_fahrenheit(c: float) -> float:
    """ Convert Celsius to Fahrenheit (F = 1.8 * C + 32). """
    pass        # TODO

def is_even(n: int) -> bool:
    """ Check if n is even. """
    pass        # TODO  (pista: Ex 2.3 do ficheiro 1. E o resultado é um bool!)

print(triple(7))                    # 21
print(celsius_to_fahrenheit(0))     # 32.0
print(celsius_to_fahrenheit(100))   # 212.0
print(is_even(10), is_even(7))      # True False















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 5.9

def triple(x: int) -> int:
    """ Triple of x. """
    return 3 * x

def celsius_to_fahrenheit(c: float) -> float:
    """ Convert Celsius to Fahrenheit. """
    return 1.8 * c + 32

def is_even(n: int) -> bool:
    """ Check if n is even. """
    return n % 2 == 0

print(triple(7))                    # 21
print(celsius_to_fahrenheit(0))     # 32.0
print(celsius_to_fahrenheit(100))   # 212.0
print(is_even(10), is_even(7))      # True False

# O is_even devolve DIRETAMENTE a comparação.
# "n % 2 == 0" já é True ou False: não é preciso mais nada.
# (no ficheiro 3 vais ver porque é que isto é melhor do que usar um if)






# %%
"""
Ex 5.10 [FAZ] Converter tempos (ex 10 e 11 dos guiões) ----------------

    a) seconds_of(h, m, s): total de segundos de h horas, m minutos e s segundos.
    b) hours_of(t), minutes_of(t), seconds_left(t): o inverso.
       Dado um total t de segundos, quantas horas, minutos e segundos.
       (é o Ex 2.10 do ficheiro 1, agora em funções)
"""

def seconds_of(h: int, m: int, s: int) -> int:
    """ Total seconds of h hours, m minutes and s seconds. """
    pass        # TODO

def hours_of(t: int) -> int:
    """ Full hours in t seconds. """
    pass        # TODO

def minutes_of(t: int) -> int:
    """ Minutes (0..59) left in t seconds after removing the full hours. """
    pass        # TODO

def seconds_left(t: int) -> int:
    """ Seconds (0..59) left in t seconds after removing the full minutes. """
    pass        # TODO

print(seconds_of(1, 2, 3))      # 3723
print(hours_of(3723), minutes_of(3723), seconds_left(3723))     # 1 2 3















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 5.10

SECONDS_PER_MINUTE = 60
SECONDS_PER_HOUR = 3600

def seconds_of(h: int, m: int, s: int) -> int:
    """ Total seconds of h hours, m minutes and s seconds. """
    return h * SECONDS_PER_HOUR + m * SECONDS_PER_MINUTE + s

def hours_of(t: int) -> int:
    """ Full hours in t seconds. """
    return t // SECONDS_PER_HOUR

def minutes_of(t: int) -> int:
    """ Minutes (0..59) left in t seconds after removing the full hours. """
    return t % SECONDS_PER_HOUR // SECONDS_PER_MINUTE

def seconds_left(t: int) -> int:
    """ Seconds (0..59) left in t seconds after removing the full minutes. """
    return t % SECONDS_PER_MINUTE

print(seconds_of(1, 2, 3))      # 3723
print(hours_of(3723), minutes_of(3723), seconds_left(3723))     # 1 2 3

# Confirmação: seconds_of(hours_of(t), minutes_of(t), seconds_left(t)) == t
print(seconds_of(hours_of(3723), minutes_of(3723), seconds_left(3723)))  # 3723






# %%
"""
Ex 5.11 [PENSA] A caixa (ex 13 dos guiões) ----------------------------

    Uma caixa (paralelepípedo) tem lados a, b, c.
    Escreve três funções:
      edges_length(a, b, c) -> comprimento total das 12 arestas
      surface_area(a, b, c) -> área das 6 faces
      volume(a, b, c)       -> volume
    Para a = 1, b = 2, c = 3: 24, 22 e 6.
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 5.11 (sem código):
#
#   arestas: há 4 arestas de cada um dos 3 comprimentos -> 4 vezes a soma dos lados
#   área:    há 2 faces de cada tipo (a*b, b*c, a*c)    -> 2 vezes a soma das 3 áreas
#   volume:  produto dos 3 lados
#
# Cabeçalhos (os 3 parâmetros são float e o resultado também):
#   def edges_length(a: float, b: float, c: float) -> float:
#   def surface_area(a: float, b: float, c: float) -> float:
#   def volume(a: float, b: float, c: float) -> float:






# %%
"""
Ex 5.12 [PENSA] Coroa circular ----------------------------------------

    Escreve circle_area(r) e, USANDO essa função, escreve
    ring_area(r_out, r_in): a área de um anel (a zona entre dois círculos).
    Para r_out = 2, r_in = 1: 9.42477796076938 (= 3 * pi)
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 5.12 (sem código):
#
#   circle_area(r)          -> pi vezes r ao quadrado (math.pi, não 3.14)
#   ring_area(r_out, r_in)  -> área do círculo grande menos a do pequeno,
#                              chamando circle_area duas vezes
#
# Precondição do ring_area: r_out >= r_in >= 0
# A vantagem: a fórmula da área do círculo fica escrita num sítio só.






# %%
"""
Ex 5.13 [DESAFIO] O que escreve? (estilo Teste 1 2025/26, 1) --------------

    Escreve o resultado de cada print. Sem correr!
    Solução no fim do ficheiro.
"""

def f(x: int) -> int:
    return x + 1

def g(x: int) -> int:
    return 2 * f(x)

def h(a: int, b: int) -> int:
    return g(b) - f(a)

print(h(3, 5))
print(f(g(f(0))))
print(h(f(1), g(1)))






# %%
"""
Ex 5.14 [DESAFIO] Encontrar o erro --------------------------------------

    Este programa devia escrever a soma das áreas de 2 retângulos (26.0).
    Descomenta a última linha e corre. Explica o erro e corrige a função.
    Solução no fim do ficheiro.
"""

def rectangle_area(w: float, h: float) -> float:
    """ Area of a rectangle. """
    print(w * h)

# print(rectangle_area(2.0, 3.0) + rectangle_area(4.0, 5.0))






# %%
"""
Ex 5.15 [DESAFIO] Minutos entre duas horas ------------------------------

    Escreve a função minutes_between(h1, m1, h2, m2), que diz quantos
    minutos passam entre as h1:m1 e as h2:m2 do mesmo dia.
    Precondição: h2:m2 é igual ou depois de h1:m1.
        minutes_between(9, 30, 11, 15) == 105
        minutes_between(8, 0, 8, 0) == 0

    Faz uma função auxiliar (com docstring) que converta h:m em minutos
    desde a meia-noite. Quanto mais simples, melhor.
    Solução no fim do ficheiro.
"""

def minutes_between(h1: int, m1: int, h2: int, m2: int) -> int:
    """ Minutes from h1:m1 to h2:m2 in the same day.
        Precondition: h1:m1 is not after h2:m2
    """
    pass        # TODO






# %%
"""
Ex 5.16 [SOZINHA] Algarismos em funções -------------------------------

    Para números de 3 algarismos, escreve:
      first_digit(n), middle_digit(n), last_digit(n)
      digit_sum(n)    -> usa as 3 funções anteriores
      reverse(n)      -> o número ao contrário (472 -> 274), usa as 3 primeiras
    Todas com cabeçalho completo e docstring com precondição.

    Pista: é o Ex 2.11 do ficheiro 1 dividido em funções.

    Sem solução. Se encravares, envia email (ver secção 0).
"""

# TODO






# %%
"""
===========================================================================
6. ESCOPO: ONDE É QUE CADA VARIÁVEL EXISTE [~35 min]
===========================================================================

Ex 6.1 [EXEMPLO] Variáveis criadas numa função só existem lá dentro -------
"""

def make_greeting() -> str:
    """ A greeting message. """
    message = "Olá!"
    return message

print(make_greeting())      # Olá!
# print(message)            # <- descomenta: NameError: name 'message' is not defined

# message é uma variável LOCAL: nasce quando a função é chamada
# e desaparece quando a função termina.
# Cá fora, ninguém a vê. A única coisa que sai da função é o valor do return.






# %%
"""
Ex 6.2 [EXEMPLO] Mesmo nome, variáveis diferentes ------------------------
"""

x = 100

def add_one(x: int) -> int:
    """ x plus one. """
    x = x + 1
    return x

print(add_one(x))       # 101
print(x)                # 100  (!!) o x de fora não mudou

# Há DOIS x diferentes:
#   - o x de fora (vale 100);
#   - o parâmetro x da função (é uma variável local).
# Na chamada, o parâmetro recebe uma CÓPIA do valor (100).
# Mudar o parâmetro lá dentro não mexe no x de fora.
#
# (para números e strings é sempre assim. Com listas há uma surpresa: ficheiro 4)






# %%
"""
Ex 6.3 [EXEMPLO] Cada chamada começa do zero -----------------------------
"""

def count_call() -> int:
    """ Try to count how many times it was called (it does not work!). """
    calls = 0
    calls += 1
    return calls

print(count_call(), count_call(), count_call())     # 1 1 1

# Cada chamada cria as suas variáveis locais de novo.
# A função não se "lembra" de nada da chamada anterior.






# %%
"""
Ex 6.4 [EXEMPLO] Ler uma variável de fora: funciona -----------------------
"""

VAT = 0.23                  # IVA: constante global (maiúsculas)

def price_with_vat(price: float) -> float:
    """ Price plus VAT. """
    return price * (1 + VAT)

print(price_with_vat(100))  # 123.0

# Dentro da função não existe nenhum VAT local.
# Então o Python vai procurá-lo cá fora e encontra-o.
#
# Ler CONSTANTES de fora é normal e recomendado:
# o valor fica escrito num sítio só, com um nome claro.






# %%
"""
Ex 6.5 [EXEMPLO] Atribuir dentro da função cria uma variável LOCAL ---------
"""

total = 0

def add_to_total(v: int) -> None:
    """ Try to add v to the outside total (it does not work!). """
    total = v               # cria um total LOCAL, novo
    print("dentro:", total)

add_to_total(5)             # dentro: 5
print("fora:", total)       # fora: 0  -> o total de fora não mudou

# E se tentarmos somar ao total de fora?
#
#   def add_to_total_v2(v: int) -> None:
#       total = total + v
#
#   add_to_total_v2(5)      -> UnboundLocalError
#
# Porquê? O Python olha para a função INTEIRA antes de a correr.
# Vê "total = ..." -> decide que total é LOCAL em toda a função.
# Depois, ao calcular "total + v", vai ler o total local... que ainda não tem valor.
# UnboundLocalError = "variável local usada antes de ter valor".






# %%
"""
Ex 6.6 [EXEMPLO] A palavra global (existe, mas NÃO se usa nesta cadeira) ---
"""

counter = 0

def increment() -> None:
    """ Increment the global counter. """
    global counter          # "o counter desta função é o de fora"
    counter += 1

increment()
increment()
print(counter)              # 2

# Funciona. Mas a cadeira proíbe variáveis globais (teórica 02b).
# Porquê: se qualquer função pode mudar uma variável,
# quando o valor está errado não se sabe quem o estragou.
#
# A forma certa: o valor entra por parâmetro e sai pelo return.

def increment_v2(c: int) -> int:
    """ c plus one. """
    return c + 1

counter = 0
counter = increment_v2(counter)
counter = increment_v2(counter)
print(counter)              # 2

# Resumo do escopo:
#   1. variáveis criadas numa função (e os parâmetros) são locais:
#      não se veem cá fora e desaparecem quando a função acaba;
#   2. cada chamada começa do zero;
#   3. uma função pode LER variáveis de fora (usa-se para constantes);
#   4. se a função ATRIBUI a um nome, esse nome é local em toda a função;
#   5. global existe, mas nesta cadeira não se usa.






# %%
"""
Ex 6.7 [FAZ] O que escreve? -------------------------------------------

    Responde em comentário, sem correr. Depois confirma.
"""

a = 1

def f6(a: int) -> int:
    a = a * 10
    return a

b = f6(a)
print(a, b)

# TODO: resposta















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 6.7
#
# Escreve: 1 10
#
# O parâmetro a da função recebe uma cópia do 1, passa a 10 e é devolvido.
# O a de fora nunca muda: continua 1. O b recebe o 10 do return.






# %%
"""
Ex 6.8 [FAZ] O que escreve? -------------------------------------------

    Responde em comentário, sem correr. Atenção à ordem das linhas!
"""

n = 5

def g6() -> int:
    return n * 2

print(g6())
n = 7
print(g6())

# TODO: resposta















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 6.8
#
# Escreve:
#   10
#   14
#
# A função lê o n de fora no momento em que é CHAMADA, não quando é definida.
# Na 2ª chamada o n já vale 7.






# %%
"""
Ex 6.9 [PENSA] Corrigir sem global ------------------------------------

    Esta função quer somar pontos a uma pontuação, mas dá UnboundLocalError
    (descomenta a chamada para ver).
    Como se reescreve sem global, para que no fim a pontuação seja 15?
"""

score = 10

def add_points(p: int) -> None:
    score = score + p

# add_points(5)
# print(score)















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 6.9 (sem código):
#
#   A função passa a receber DOIS parâmetros: a pontuação atual e os pontos.
#   Devolve (return) a nova pontuação, em vez de tentar mudar a de fora.
#   Cá fora, guarda-se o resultado na mesma variável: score = add_points(score, 5)
#
# Cabeçalho: def add_points(score: int, p: int) -> int:
# (é o mesmo esquema do increment_v2 do Ex 6.6)






# %%
"""
Ex 6.10 [DESAFIO] O que escreve? (estilo teste) ---------------------------

    Faz a tabela à mão, com uma coluna para as variáveis de fora
    e outra para as da função. Sem correr!
    Solução no fim do ficheiro.
"""

x = 3
y = 4

def mystery(x: int) -> int:
    y = x * 2
    x = y + 1
    return x + y

z = mystery(y)
print(x, y, z)
print(mystery(mystery(0)))






# %%
"""
Ex 6.11 [SOZINHA] Conta bancária sem global ---------------------------

    Este programa funciona, mas usa global.
    Reescreve-o SEM global: deposit e withdraw recebem o saldo
    e o valor, e devolvem o novo saldo.
    No fim deve continuar a escrever 120.

    Sem solução. Se encravares, envia email (ver secção 0).
"""

balance = 100

def deposit(amount: int) -> None:
    """ Add amount to the balance. """
    global balance
    balance += amount

def withdraw(amount: int) -> None:
    """ Remove amount from the balance. """
    global balance
    balance -= amount

deposit(50)
withdraw(30)
print(balance)          # 120

# TODO: a tua versão sem global






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  [ ] definir e chamar funções; parâmetros e argumentos (a ordem conta)
  [ ] a indentação define o corpo da função; definir não é correr
  [ ] return vs print; uma função sem return devolve None
  [ ] o return termina a função
  [ ] os tipos no cabeçalho são documentação (tipagem forte e dinâmica)
  [ ] funções que chamam funções, import math
  [ ] docstrings e precondições
  [ ] variáveis locais, cada chamada começa do zero
  [ ] ler constantes de fora; atribuir cria uma variável local (UnboundLocalError)
  [ ] porque não usar global: parâmetro entra, return sai

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_3_condicoes_e_if.py
"""






# %%
"""
===========================================================================
SOLUÇÕES DOS DESAFIOS
===========================================================================

    Só para veres DEPOIS de tentares.
"""















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 5.13
#
#   h(3, 5)   = g(5) - f(3)
#             = 2 * f(5) - 4
#             = 2 * 6 - 4      = 8
#
#   f(g(f(0))): de dentro para fora
#     f(0) = 1
#     g(1) = 2 * f(1) = 2 * 2 = 4
#     f(4) = 5                 -> 5
#
#   h(f(1), g(1)) = h(2, 4)
#                 = g(4) - f(2)
#                 = 2 * 5 - 3  = 7
#
# Escreve:
#   8
#   5
#   7






# %%
# Solução do Ex 5.14
#
# O erro: TypeError: unsupported operand type(s) for +: 'NoneType' and 'NoneType'
#
# A função faz print em vez de return.
# Escreve 6.0 e 20.0 no ecrã, mas devolve None (as duas vezes).
# Depois o Python tenta fazer None + None -> erro.
#
# Correção: trocar o print por return.

def rectangle_area(w: float, h: float) -> float:
    """ Area of a rectangle. """
    return w * h

print(rectangle_area(2.0, 3.0) + rectangle_area(4.0, 5.0))     # 26.0






# %%
# Solução do Ex 5.15

MINUTES_PER_HOUR = 60

def to_minutes(h: int, m: int) -> int:
    """ Minutes since midnight of the time h:m. """
    return h * MINUTES_PER_HOUR + m

def minutes_between(h1: int, m1: int, h2: int, m2: int) -> int:
    """ Minutes from h1:m1 to h2:m2 in the same day.
        Precondition: h1:m1 is not after h2:m2
    """
    return to_minutes(h2, m2) - to_minutes(h1, m1)

print(minutes_between(9, 30, 11, 15))   # 105   (675 - 570)
print(minutes_between(8, 0, 8, 0))      # 0

# A ideia: converter as duas horas para a mesma unidade (minutos)
# e depois é só uma subtração.
# Tentar fazer "horas menos horas e minutos menos minutos" complica:
# 11:15 - 9:30 dava 2 horas e -15 minutos.






# %%
# Solução do Ex 6.10
#
#   Fora:   x = 3, y = 4
#   z = mystery(y) -> mystery(4):
#       dentro: x = 4 (parâmetro), y = 8 (local), x = 9
#       return 9 + 8 = 17
#   Fora nada mudou: x = 3, y = 4, z = 17
#
#   mystery(mystery(0)):
#       mystery(0): y = 0, x = 1, return 1
#       mystery(1): y = 2, x = 3, return 5
#
# Escreve:
#   3 4 17
#   5
