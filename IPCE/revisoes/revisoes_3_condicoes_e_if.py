# %%
"""
===========================================================================
REVISÕES 3 - CONDIÇÕES E IF
Booleanos, and/or/not, if/elif/else, recursividade e programa com main
IPCE 2026/2027
===========================================================================

Antes deste ficheiro: revisoes_2_funcoes_e_escopo.py

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

  7. Booleanos e condições .................................... [~40 min]
       Porquê: um programa só toma decisões se souber fazer perguntas
       com resposta True ou False. and, or e not juntam perguntas simples
       em perguntas complexas.

  8. if, elif, else ........................................... [~70 min]
       Porquê: é assim que o programa escolhe o que fazer.
       A ORDEM dos ramos e a forma de os escrever (com ou sem else,
       com ou sem if) decidem se o código está certo e se é simples.

  9. Recursividade: funções que se chamam a si próprias ....... [~25 min]
       Porquê: algumas definições da matemática (fatorial, MDC)
       traduzem-se diretamente assim. E aparece em perguntas de teste.

 10. Um programa bem organizado: a função main ................ [~25 min]
       Porquê: nos testes há sempre um "programa completo" para escrever.
       A main fala com o utilizador, as outras funções fazem as contas.

  Resumo + soluções dos desafios

  Total: ~2h40.

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

  A numeração continua a dos ficheiros anteriores: este começa na secção 7.
"""






# %%
"""
===========================================================================
7. BOOLEANOS E CONDIÇÕES [~40 min]
===========================================================================

Ex 7.1 [EXEMPLO] Perguntas com resposta True ou False -----------------------

    Recapitular (ficheiro 1, Ex 2.7): uma comparação dá um bool.
"""

age = 17
print(age >= 18)            # False
print(age == 17)            # True
print(13 <= age <= 19)      # True   (comparações encadeadas)

is_adult = age >= 18        # o resultado pode ficar guardado numa variável
print(is_adult)             # False

# Uma expressão que dá True ou False chama-se CONDIÇÃO (ou expressão booleana).
# É isto que vai dentro de um if (secção 8).






# %%
"""
Ex 7.2 [EXEMPLO] and, or, not -------------------------------------------
"""

print(True and True)        # True
print(True and False)       # False
print(False and True)       # False
print(False and False)      # False

print(True or True)         # True
print(True or False)        # True
print(False or True)        # True
print(False or False)       # False

print(not True)             # False
print(not False)            # True

# and -> True só se as DUAS forem True    ("as duas coisas ao mesmo tempo")
# or  -> True se PELO MENOS UMA for True  ("uma ou outra, ou as duas")
# not -> troca: True passa a False e vice-versa
#
# Atenção: o "or" do Python inclui o caso "as duas".
# No português do dia a dia, "queres sopa ou salada?" costuma querer dizer
# "só uma". Em programação não: True or True dá True.






# %%
"""
Ex 7.3 [EXEMPLO] Juntar condições ---------------------------------------
"""

age = 20
has_license = False

can_drive = age >= 18 and has_license
print(can_drive)                            # False: tem idade, mas não tem carta

is_child_or_senior = age < 12 or age >= 65
print(is_child_or_senior)                   # False

print(not has_license)                      # True

# Prioridade: primeiro as comparações (<, ==, ...), depois not, depois and,
# e só no fim or.
#     a or b and c     é lido como     a or (b and c)
print(True or False and False)              # True   -> True or (False and False)
print((True or False) and False)            # False

# Na dúvida, parêntesis. Ajudam o Python e quem lê.






# %%
"""
Ex 7.4 [EXEMPLO] Funções booleanas: dar nome a uma pergunta ----------------
"""

def is_even(n: int) -> bool:
    """ Check if n is even. """
    return n % 2 == 0

def is_valid_grade(g: float) -> bool:
    """ Check if g is a grade between 0 and 20. """
    return 0 <= g <= 20

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

print(is_even(4), is_even(7))                       # True False
print(is_valid_grade(15), is_valid_grade(21))       # True False
print(is_leap_year(2024), is_leap_year(1900), is_leap_year(2000))  # True False True

# Uma função booleana devolve True ou False.
# Convenção: o nome começa por "is_" (ou "has_", "can_"...),
# e lê-se como uma pergunta: "is_leap_year(2024)?" -> True.
#
# A grande vantagem: em vez de uma condição comprida e difícil de ler,
#     if (year % 4 == 0 and year % 100 != 0) or year % 400 == 0:
# escreve-se
#     if is_leap_year(year):
# e toda a gente percebe logo.
#
# Regra do ano bissexto: divisível por 4, exceto os divisíveis por 100,
# a não ser que sejam divisíveis por 400. (1900 não foi, 2000 foi)






# %%
"""
Ex 7.5 [EXEMPLO] and e or são "preguiçosos" -------------------------------
"""

x = 0
print(x != 0 and 10 / x > 1)        # False, e sem erro!

# print(10 / x > 1 and x != 0)      # <- descomenta: ZeroDivisionError

# O Python avalia da esquerda para a direita e PÁRA assim que sabe a resposta:
#   - and: se a esquerda é False, o resultado é False. A direita nem é calculada.
#   - or:  se a esquerda é True, o resultado é True. A direita nem é calculada.
# Chama-se avaliação em CURTO-CIRCUITO.
#
# Uso prático: a condição da esquerda "protege" a da direita.
# "x != 0 and 10 / x > 1" -> nunca divide por zero.
# Trocando a ordem, rebenta. A ORDEM importa.






# %%
"""
Ex 7.6 [EXEMPLO] Negar uma condição: leis de De Morgan ---------------------
"""

g = 25

print(not (0 <= g <= 20))           # True -> g é inválida
print(g < 0 or g > 20)              # True -> exatamente a mesma pergunta

a, b = True, False
print(not (a and b), (not a) or (not b))     # True True    (são sempre iguais)
print(not (a or b), (not a) and (not b))     # False False  (são sempre iguais)

# Leis de De Morgan:
#     not (p and q)   ==   (not p) or (not q)
#     not (p or q)    ==   (not p) and (not q)
# Ao "entrar" nos parêntesis, o not troca o and pelo or (e vice-versa)
# e nega cada pedaço.
#
# Exemplo: "não é verdade que 0 <= g <= 20"
#     = "não é verdade que (0 <= g and g <= 20)"
#     = "g < 0 or g > 20"
#
# Cuidado: o not aplica-se só ao que está logo a seguir.
lower, upper, n = 0, 1, 0
print(not (lower < upper and n > 1))    # True  -> nega a condição toda
print(not lower < upper and n > 1)      # False -> é (not lower < upper) and (n > 1)






# %%
"""
Ex 7.7 [EXEMPLO] Nunca comparar floats com == -----------------------------
"""

print(0.1 + 0.2 == 0.3)                 # False (!!)
print(0.1 + 0.2)                        # 0.30000000000000004

EPSILON = 1e-9

def almost_equal(a: float, b: float) -> bool:
    """ Check if two reals are so close that they should be equal. """
    return abs(a - b) < EPSILON

print(almost_equal(0.1 + 0.2, 0.3))     # True

# Os floats têm quase sempre um pequeno erro nos últimos algarismos.
# Por isso "==" entre floats calculados é uma armadilha.
# Em vez de "são iguais?", pergunta-se "estão MUITO perto um do outro?".
#   abs(x)  -> valor absoluto (abs(-3.5) == 3.5)
#   1e-9    -> 0.000000001
#
# Existe também math.isclose(a, b), que faz algo parecido.
# Mas atenção: math.isclose(x, 0.0) dá quase sempre False para x muito pequeno.
# Para comparar com zero, usa abs(x) < EPSILON.
#
# Com inteiros não há problema nenhum: 2 + 2 == 4 é sempre exato.






# %%
"""
Ex 7.8 [FAZ] Funções booleanas simples ------------------------------------

    Completa. Só um return em cada, sem if.
"""

def is_multiple(a: int, b: int) -> bool:
    """ Check if a is a multiple of b.
        Precondition: b != 0
    """
    pass        # TODO

def is_teenager(age: int) -> bool:
    """ Check if age is between 13 and 19 (inclusive). """
    pass        # TODO

def is_odd_and_positive(n: int) -> bool:
    """ Check if n is odd and greater than zero. """
    pass        # TODO

print(is_multiple(10, 5), is_multiple(10, 3))               # True False
print(is_teenager(13), is_teenager(19), is_teenager(20))    # True True False
print(is_odd_and_positive(7), is_odd_and_positive(-7), is_odd_and_positive(4))
                                                            # True False False
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 7.8

def is_multiple(a: int, b: int) -> bool:
    """ Check if a is a multiple of b.
        Precondition: b != 0
    """
    return a % b == 0

def is_teenager(age: int) -> bool:
    """ Check if age is between 13 and 19 (inclusive). """
    return 13 <= age <= 19

def is_odd_and_positive(n: int) -> bool:
    """ Check if n is odd and greater than zero. """
    return n % 2 == 1 and n > 0

print(is_multiple(10, 5), is_multiple(10, 3))               # True False
print(is_teenager(13), is_teenager(19), is_teenager(20))    # True True False
print(is_odd_and_positive(7), is_odd_and_positive(-7), is_odd_and_positive(4))

# Porque é que is_odd_and_positive(-7) dá False?
# -7 % 2 dá 1 em Python (é ímpar), mas -7 > 0 é False. E o and exige as duas.
#
# Precondição do is_multiple: b != 0, porque a % 0 dá ZeroDivisionError.






# %%
"""
Ex 7.9 [FAZ] Prever (sem correr) ------------------------------------------

    Para cada linha, escreve True, False ou "erro". Depois corre.
"""

a, b = 5, 0

print(a > 3 and b > 3)              # a)
print(a > 3 or b / 0 > 1)           # b)
print(not a > 3 or b == 0)          # c)
print(not (a > 3 or b == 0))        # d)
print(b != 0 and a / b > 1)         # e)
print(a == 5 and not b)             # f)   (aqui o b é um número...)

# TODO: as tuas respostas
# a)
# b)
# c)
# d)
# e)
# f)
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 7.9
#
#   a) False  -> 5 > 3 é True, 0 > 3 é False -> and dá False
#   b) True   -> a esquerda já é True: o or nem calcula b / 0 (senão rebentava)
#   c) True   -> é (not a > 3) or (b == 0) = False or True
#   d) False  -> not (True or True) = not True
#   e) False  -> b != 0 é False: o and pára ali, a / b nunca é calculado
#   f) True   -> not 0 dá True (!!)
#
# A f) é estranha: not aplicado a um NÚMERO.
# O Python trata o 0 como "falso" e qualquer outro número como "verdadeiro".
# Funciona, mas nesta cadeira não se escreve assim (vês porquê no Ex 8.8).
# A forma clara: a == 5 and b == 0






# %%
"""
Ex 7.10 [FAZ] Ano bissexto e mês válido -----------------------------------

    Sem olhar para o Ex 7.4, escreve:
      is_leap_year(year) -> divisível por 4, exceto os divisíveis por 100,
                            a não ser que sejam divisíveis por 400.
      is_valid_month(m)  -> m entre 1 e 12.
    Só um return em cada, sem if.
"""

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    pass        # TODO

def is_valid_month(m: int) -> bool:
    """ Check if m is a valid month number. """
    pass        # TODO

print(is_leap_year(2024), is_leap_year(2023), is_leap_year(1900), is_leap_year(2000))
# True False False True
print(is_valid_month(1), is_valid_month(12), is_valid_month(0), is_valid_month(13))
# True True False False
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 7.10

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def is_valid_month(m: int) -> bool:
    """ Check if m is a valid month number. """
    return 1 <= m <= 12

print(is_leap_year(2024), is_leap_year(2023), is_leap_year(1900), is_leap_year(2000))
print(is_valid_month(1), is_valid_month(12), is_valid_month(0), is_valid_month(13))

# Como chegar à condição do bissexto, passo a passo:
#   "divisível por 4"                     -> year % 4 == 0
#   "exceto os divisíveis por 100"        -> and year % 100 != 0
#   "a não ser que divisível por 400"     -> or year % 400 == 0
# Os parêntesis à volta do and não são obrigatórios (o and vem antes do or),
# mas mostram a ideia: (regra geral) or (exceção à exceção).






# %%
"""
Ex 7.12 [DESAFIO] Período do Natal (Teste 1 2024/25, pergunta 2, 4 valores)

    Escreva uma função booleana para testar se uma data se situa no período
    do Natal. Vamos convencionar que este período se inicia às zero horas
    de 23/Dez e termina às 24 horas de 1/Jan. Exemplos:
        christmas(23, 12) == True      christmas(1, 1) == True
        christmas(22, 12) == False     christmas(10, 2) == False

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def christmas(day: int, month: int) -> bool:
            ''' Check if it is a Christmas date. '''

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 7.13 [DESAFIO] É raiz? (Teste 1 2025/26, pergunta 2, 3 valores) ---------

    Escreva uma função booleana para testar se um valor r é raiz da
    equação de 2º grau ax²+bx+c=0. Note que não é preciso usar a fórmula
    resolvente (não complique!). Exemplos:
        is_root(0.0, 1.0, -3.0, 2.0) == False
        is_root(1.0, 1.0, -3.0, 2.0) == True

    Não programe nenhuma função main, nem use input ou print.

        def is_root(r: float, a: float, b: float, c: float) -> bool:
            ''' Check if r is a root of ax²+bx+c=0 '''

    Pista: o que quer dizer "r é raiz"? E que tipo de números são estes?
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 7.14 [DESAFIO] De Morgan ---------------------------------------------

    Reescreve cada condição SEM usar not (e sem mudar o resultado).
      a) not (x > 0 and y > 0)
      b) not (age < 18 or age > 65)
      c) not (a == b) and not (b == c)
      d) not (x <= 5 or y == 3)

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 7.15 [SOZINHA] Ou exclusivo e hora válida --------------------------

    a) xor(p, q): True se EXATAMENTE uma das duas for True.
       Só podes usar and, or, not (sem if, sem != entre booleanos).
           xor(True, False) == True     xor(True, True) == False

    b) is_valid_time(h, m, s): True se h:m:s for uma hora válida do dia
       (horas 0..23, minutos 0..59, segundos 0..59).
           is_valid_time(23, 59, 59) == True
           is_valid_time(24, 0, 0) == False

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
8. IF, ELIF, ELSE [~70 min]
===========================================================================

Ex 8.1 [EXEMPLO] if simples: fazer uma coisa só se... ---------------------
"""

temperature = 38.2

if temperature >= 37.5:
    print("Tens febre.")
    print("Fica em casa.")
print("Fim.")

# Estrutura:
#     if CONDIÇÃO:
#         linhas que só correm se a condição for True (indentadas, 4 espaços)
#     linha seguinte (sem indentação): corre sempre
#
# Não esquecer os ":" no fim da linha do if.
# Muda a temperatura para 36.5 e volta a correr: só aparece "Fim.".






# %%
"""
Ex 8.2 [EXEMPLO] if / else: um caminho ou o outro --------------------------
"""

def absolute(x: float) -> float:
    """ Absolute value of x. """
    if x >= 0:
        return x
    else:
        return -x

print(absolute(5.5), absolute(-5.5), absolute(0))      # 5.5 5.5 0

# if/else: corre EXATAMENTE um dos dois ramos. Nunca os dois, nunca nenhum.
# O else não tem condição: é "em todos os outros casos".






# %%
"""
Ex 8.3 [EXEMPLO] if / elif / else: vários casos -----------------------------
"""

def grade_label(g: float) -> str:
    """ Label for a grade between 0 and 20.
        Precondition: 0 <= g <= 20
    """
    if g < 9.5:
        return "Reprovado"
    elif g < 14:
        return "Suficiente"
    elif g < 17:
        return "Bom"
    else:
        return "Muito bom"

print(grade_label(8), grade_label(12), grade_label(15), grade_label(19))
# Reprovado Suficiente Bom Muito bom

# O Python testa as condições POR ORDEM, de cima para baixo.
# Corre o PRIMEIRO ramo cuja condição for True, e salta os outros todos.
# Se nenhuma for True, corre o else (se existir).
#
# Repara no elif g < 14: não é preciso escrever "9.5 <= g < 14".
# Se chegámos a esse elif, o if de cima falhou: já sabemos que g >= 9.5.
# Cada ramo aproveita o que os ramos de cima já excluíram.






# %%
"""
Ex 8.4 [EXEMPLO] A ORDEM dos ramos importa ---------------------------------

    Triagem num hospital:
      - VERMELHO (urgente): febre >= 39.5 E dificuldade em respirar
      - AMARELO: febre >= 37.5
      - VERDE: o resto
"""

def triage_wrong(fever: float, breathing_problem: bool) -> str:
    """ Triage color (WRONG ORDER). """
    if fever >= 37.5:
        return "AMARELO"
    elif fever >= 39.5 and breathing_problem:
        return "VERMELHO"
    else:
        return "VERDE"

def triage(fever: float, breathing_problem: bool) -> str:
    """ Triage color. """
    if fever >= 39.5 and breathing_problem:
        return "VERMELHO"
    elif fever >= 37.5:
        return "AMARELO"
    else:
        return "VERDE"

print(triage_wrong(39.8, True))     # AMARELO  (!!) devia ser VERMELHO
print(triage(39.8, True))           # VERMELHO

# Na versão errada, a condição mais FRACA (mais abrangente) vem primeiro.
# A condição "febre >= 37.5" em 1º lugar
# apanha também o doente com febre 39.8 e dificuldade a respirar
# (que devia ser VERMELHO/urgente).
# O ramo VERMELHO nunca chega a ser testado: é código morto.
#
# Regra: primeiro os casos mais RESTRITOS (mais específicos),
# depois os mais ABRANGENTES. O else apanha o resto.






# %%
"""
Ex 8.5 [EXEMPLO] ifs seguidos com return: dispensam o elif e o else ---------
"""

def sign(x: int) -> int:
    """ -1, 0 or 1, according to the sign of x. """
    if x < 0:
        return -1
    if x == 0:
        return 0
    return 1

print(sign(-7), sign(0), sign(7))       # -1 0 1

# Funciona sem elif e sem else. Porquê?
# O return TERMINA a função. Se x < 0, a função acaba no primeiro return.
# Só se chega ao 2º if se o 1º falhou. Só se chega ao último return
# se os dois falharam: é um "else" implícito.
#
# Estas duas formas são equivalentes quando CADA ramo faz return:
#
#   if x < 0:                  if x < 0:
#       return -1                  return -1
#   elif x == 0:               if x == 0:
#       return 0                   return 0
#   else:                      return 1
#       return 1
#
# As duas estão certas. Escolhe a que achares mais legível.
# A da direita é frequente quando há vários "casos especiais" a despachar
# primeiro, e o caso normal fica no fim, sem indentação.






# %%
"""
Ex 8.6 [EXEMPLO] ...mas SEM return, ifs seguidos e elif NÃO são iguais ------

    Desconto numa loja:
      - compras acima de 100 euros: 20% de desconto
      - compras acima de 50 euros: 10% de desconto
"""

def discount_wrong(total: float) -> float:
    """ Discount rate (WRONG: two separate ifs). """
    rate = 0.0
    if total > 100:
        rate = 0.20
    if total > 50:
        rate = 0.10
    return rate

def discount(total: float) -> float:
    """ Discount rate. """
    rate = 0.0
    if total > 100:
        rate = 0.20
    elif total > 50:
        rate = 0.10
    return rate

print(discount_wrong(150))      # 0.1 (!!) devia ser 0.2
print(discount(150))            # 0.2

# Com 150 euros, na versão errada:
#   1º if: 150 > 100 -> rate = 0.20
#   2º if: 150 > 50  -> rate = 0.10   <- também corre! e estraga o anterior
# Os dois ifs são INDEPENDENTES: cada um é testado, aconteça o que acontecer.
#
# Com elif, os ramos são ALTERNATIVOS: só corre um.
#
# Resumo:
#   - cada ramo termina com return -> ifs seguidos ou elif, tanto faz;
#   - os ramos NÃO terminam com return -> ifs seguidos podem correr vários.
#     Se os casos são alternativos (só um pode acontecer), usa elif.






# %%
"""
Ex 8.7 [EXEMPLO] Se o resultado é um bool, não é preciso if -----------------
"""

def is_adult_long(age: int) -> bool:
    """ Check if age is 18 or more (TOO LONG). """
    if age >= 18:
        return True
    else:
        return False

def is_adult(age: int) -> bool:
    """ Check if age is 18 or more. """
    return age >= 18

print(is_adult_long(20), is_adult(20))      # True True

# As duas funcionam. Mas "age >= 18" JÁ É True ou False.
# O if pergunta "é True? então devolve True" -> está a dizer a mesma coisa duas vezes.
#
# Esta é uma regra de estilo da cadeira (teórica 03b):
# não usar True e False desnecessariamente.
# Outros casos do mesmo erro:
#     if is_adult(age) == True:      ->   if is_adult(age):
#     if is_adult(age) == False:     ->   if not is_adult(age):
#
# Nos testes, a forma comprida perde pontos:
# "Quanto mais simples for a função, melhor."






# %%
"""
Ex 8.8 [EXEMPLO] O if aceita qualquer valor... mas NÃO se faz isso ----------
"""

print(bool(0), bool(7), bool(-2))           # False True True
print(bool(0.0), bool(0.1))                 # False True
print(bool(""), bool("olá"), bool(" "))     # False True True  (um espaço não é vazio)

name = ""
if name:
    print("tem nome")
else:
    print("nome vazio")                     # nome vazio

# Quando o if recebe algo que não é bool, o Python converte com bool():
#   - dá False para: 0, 0.0, "" (string vazia), e coleções vazias (ficheiro 4);
#   - dá True para tudo o resto.
#
# Vais encontrar isto em código da Internet: "if name:", "if n:", "if lista:".
# Tens de o saber LER.
#
# Mas nesta cadeira NÃO se escreve assim (teórica 03b, 2ª regra de estilo):
# no if usa-se sempre uma condição booleana a sério. É mais claro:
#     if name != "":         em vez de     if name:
#     if n != 0:             em vez de     if n:
# Nunca usar 1 e 0 no lugar de True e False.






# %%
"""
Ex 8.9 [EXEMPLO] ifs dentro de ifs, ou um and? ------------------------------
"""

def can_vote_nested(age: int, is_citizen: bool) -> bool:
    """ Check if someone can vote (nested ifs). """
    if is_citizen:
        if age >= 18:
            return True
    return False

def can_vote(age: int, is_citizen: bool) -> bool:
    """ Check if someone can vote. """
    return is_citizen and age >= 18

print(can_vote_nested(20, True), can_vote(20, True))     # True True
print(can_vote_nested(20, False), can_vote(20, False))   # False False

# Um if dentro de outro if (encaixado) só corre se os dois forem True.
# Isso é exatamente um and. Quando dá para juntar com and, fica mais simples.
#
# Os ifs encaixados fazem sentido quando cada nível tem a sua própria ação:
#
#     if is_citizen:
#         if age >= 18:
#             return "pode votar"
#         else:
#             return "ainda não tem idade"
#     else:
#         return "não é cidadão"






# %%
"""
Ex 8.10 [EXEMPLO] Variável criada só em alguns ramos: UnboundLocalError -----
"""

def classify(grade: int) -> str:
    """ Pass or fail (WITH A BUG). """
    if grade > 10:
        result = "Aprovado"
    elif grade < 10:
        result = "Reprovado"
    return result

print(classify(15))         # Aprovado
print(classify(5))          # Reprovado
# print(classify(10))       # <- descomenta: UnboundLocalError

# Com 10, nenhum ramo corre: 10 > 10 é False e 10 < 10 também.
# A variável result nunca é criada, e o return tenta lê-la.
#
# Correção: garantir que HÁ SEMPRE um ramo que dá valor a result.
# O mais seguro é terminar com else (ou dar um valor inicial antes do if).

def classify_ok(grade: int) -> str:
    """ Pass or fail. """
    if grade >= 10:
        result = "Aprovado"
    else:
        result = "Reprovado"
    return result

print(classify_ok(10))      # Aprovado

# Aqui nem era preciso a variável:
#     if grade >= 10:
#         return "Aprovado"
#     return "Reprovado"






# %%
"""
Ex 8.11 [FAZ] Máximo de dois e de três (ex 16 dos guiões) -----------------

    a) maximum(a, b): o maior de dois inteiros.
    b) maximum3(a, b, c): o maior de três. Usa a maximum, sem mais ifs.

    (O Python tem uma função max pronta, mas aqui o objetivo é treinar o if.)
"""

def maximum(a: int, b: int) -> int:
    """ Largest of two integers. """
    pass        # TODO

def maximum3(a: int, b: int, c: int) -> int:
    """ Largest of three integers. """
    pass        # TODO

print(maximum(3, 8), maximum(8, 3), maximum(5, 5))          # 8 8 5
print(maximum3(1, 2, 3), maximum3(3, 2, 1), maximum3(2, 3, 1))  # 3 3 3
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 8.11

def maximum(a: int, b: int) -> int:
    """ Largest of two integers. """
    if a >= b:
        return a
    return b

def maximum3(a: int, b: int, c: int) -> int:
    """ Largest of three integers. """
    return maximum(maximum(a, b), c)

print(maximum(3, 8), maximum(8, 3), maximum(5, 5))
print(maximum3(1, 2, 3), maximum3(3, 2, 1), maximum3(2, 3, 1))

# maximum3: o maior dos três é o maior entre (o maior de a e b) e c.
# Reaproveitar uma função que já existe evita escrever e testar ifs novos.
#
# Testa sempre os casos de fronteira: maximum(5, 5) tem de dar 5.






# %%
"""
Ex 8.12 [FAZ] Preço do bilhete ----------------------------------------

    Um museu cobra:
      - menos de 3 anos: grátis (0.0)
      - menos de 12 anos: 5.0
      - 65 anos ou mais: 6.0
      - restantes: 10.0
"""

def ticket_price(age: int) -> float:
    """ Ticket price for a given age.
        Precondition: age >= 0
    """
    pass        # TODO

print(ticket_price(2), ticket_price(3), ticket_price(11), ticket_price(12),
      ticket_price(64), ticket_price(65))
# 0.0 5.0 5.0 10.0 10.0 6.0
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 8.12

def ticket_price(age: int) -> float:
    """ Ticket price for a given age.
        Precondition: age >= 0
    """
    if age < 3:
        return 0.0
    elif age < 12:
        return 5.0
    elif age >= 65:
        return 6.0
    else:
        return 10.0

print(ticket_price(2), ticket_price(3), ticket_price(11), ticket_price(12),
      ticket_price(64), ticket_price(65))

# A ordem "< 3" antes de "< 12" é obrigatória (o mais restrito primeiro).
# O caso "65 ou mais" podia vir em qualquer posição: não se sobrepõe a nenhum.
#
# Testa sempre as FRONTEIRAS: 2 e 3, 11 e 12, 64 e 65.
# É aí que aparecem os erros de < vs <=.






# %%
"""
Ex 8.13 [FAZ] Dias de um mês (ex 23 dos guiões) ---------------------------

    month_length(month, year): número de dias do mês.
      - meses com 31 dias: 1, 3, 5, 7, 8, 10, 12
      - meses com 30 dias: 4, 6, 9, 11
      - fevereiro: 29 nos anos bissextos, 28 nos outros
    Usa a is_leap_year do Ex 7.10 (corre essa célula antes, para ela existir).
"""

def month_length(month: int, year: int) -> int:
    """ Number of days of a given month.
        Precondition: 1 <= month <= 12
    """
    pass        # TODO

print(month_length(1, 2023), month_length(4, 2023),
      month_length(2, 2024), month_length(2, 2023))       # 31 30 29 28
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 8.13

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def month_length(month: int, year: int) -> int:
    """ Number of days of a given month.
        Precondition: 1 <= month <= 12
    """
    if month == 4 or month == 6 or month == 9 or month == 11:
        return 30
    elif month == 2:
        if is_leap_year(year):
            return 29
        return 28
    else:
        return 31

print(month_length(1, 2023), month_length(4, 2023),
      month_length(2, 2024), month_length(2, 2023))       # 31 30 29 28

# Truque: há menos meses de 30 dias do que de 31.
# Testa-se o grupo mais pequeno, depois fevereiro, e o resto (else) são os de 31.
# A precondição garante 1 <= month <= 12: o else não apanha meses inválidos.
#
# Erro clássico: "if month == 4 or 6 or 9 or 11:"
# O Python lê (month == 4) or 6 or ... e o 6 sozinho conta como "verdadeiro":
# a condição é sempre verdadeira! Cada comparação tem de estar completa.
#
# No ficheiro 4 vais ver o operador "in", que deixa escrever:
#     if month in [4, 6, 9, 11]:






# %%
"""
Ex 8.14 [PENSA] Tipo de triângulo (ex 17 dos guiões) ------------------

    triangle_kind(a, b, c) devolve:
      0 se não for um triângulo (usa o is_proper_triangle do Ex 7.11)
      1 se for equilátero (3 lados iguais)
      2 se for isósceles (exatamente 2 lados iguais)
      3 se for escaleno (todos diferentes)
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 8.14 (sem código):
#
#   1. se NÃO for um triângulo -> 0  (despachar o caso inválido primeiro)
#   2. se a == b e b == c      -> 1
#   3. se a == b ou a == c ou b == c -> 2
#   4. senão                   -> 3
#
# A ordem 2 antes de 3 é obrigatória: um equilátero também tem
# "dois lados iguais". Se o teste do isósceles viesse primeiro,
# um equilátero seria classificado como isósceles.
# (é o mesmo problema da triagem do Ex 8.4)






# %%
"""
Ex 8.15 [PENSA] Aprovado ou reprovado (ex 18 dos guiões) --------------

    Um aluno tem duas notas de testes (t1, t2) e uma de projeto (pr).
      - Fica aprovado se a nota do projeto for >= 9.5
        E a média dos dois testes for >= 9.5.
      - Nesse caso, a nota final é 40% t1 + 40% t2 + 20% projeto.
    Escreve:
        passed(t1, t2, pr) -> bool
        final_grade(t1, t2, pr) -> float     (precondição: passed(...))
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 8.15 (sem código):
#
#   passed: um único return com duas condições ligadas por and.
#           A média dos testes pode vir de uma função average(a, b)
#           (o Ex 5.8 do ficheiro 2).
#   final_grade: só a conta, sem if nenhum.
#           A precondição diz que quem chama já verificou que o aluno passou.
#
# Quem decide se escreve a nota ou "REPROVADO" é a main (secção 10):
#     if passed(t1, t2, pr): escreve a nota final
#     senão: escreve REPROVADO






# %%
"""
Ex 8.16 [DESAFIO] and, or, not sem and, or, not (ex 25 dos guiões) --------

    Se o Python não tivesse os operadores and, or e not, que funções
    booleanas escreveria para os substituir? Complete da forma mais
    compacta possível. Não pode usar os três operadores lógicos,
    mas pode usar o if.

        def and_(a: bool, b: bool) -> bool:
        def or_(a: bool, b: bool) -> bool:
        def not_(a: bool) -> bool:

    ("and" é palavra reservada, por isso o nome leva um "_" no fim)
    Pista: em cada função basta perguntar "o a é True?".
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 8.17 [DESAFIO] Sintaxe e execução à mão (Teste 1 2024/25, 1b + estilo 1c)

    a) Sintaticamente correto ou incorreto? (as duas primeiras são do teste)
         1)  if a = b:                   3)  if x > 0
                 print("sim")                    print("sim")
         2)  if a < b < c:               4)  if x > 0:
                 print("sim")                print("sim")

    b) O que escreve? Sem correr!
"""

def mystery(n: int) -> str:
    if n % 3 == 0 and n % 5 == 0:
        return "A"
    elif n % 3 == 0:
        return "B"
    if n % 5 == 0:
        return "C"
    return str(n)

print(mystery(9), mystery(10), mystery(15), mystery(7), mystery(30))

# Solução no fim do ficheiro.






# %%
"""
Ex 8.18 [DESAFIO] Data válida (ex 24 dos guiões, primeira parte) ----------

    is_date_valid(day, month, year): True se a data existir.
        is_date_valid(29, 2, 2024) == True
        is_date_valid(29, 2, 2023) == False
        is_date_valid(31, 4, 2024) == False
        is_date_valid(1, 13, 2024) == False
    Usa o month_length do Ex 8.13 (corre essa célula antes, para ela existir).

    Cuidado: month_length tem a precondição 1 <= month <= 12.
    Se o mês for 13, a função NÃO pode ser chamada.
    Quanto mais simples, melhor (dá para fazer com um só return).
    Solução no fim do ficheiro.
"""

def is_date_valid(day: int, month: int, year: int) -> bool:
    """ Check if a date exists. """
    pass        # TODO






# %%
"""
Ex 8.19 [SOZINHA] Pedra, papel, tesoura -------------------------------

    winner(p1, p2): recebe as jogadas dos dois jogadores
    ("pedra", "papel" ou "tesoura") e devolve:
        0 se for empate, 1 se ganhar o jogador 1, 2 se ganhar o jogador 2.
    Regras: pedra ganha a tesoura, tesoura ganha ao papel, papel ganha à pedra.
        winner("pedra", "tesoura") == 1
        winner("pedra", "papel") == 2
        winner("papel", "papel") == 0

    Pistas:
      - despacha o empate primeiro;
      - depois basta uma condição para "o jogador 1 ganha" (3 casos com or);
      - se não é empate e o 1 não ganhou, quem ganhou?

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
9. RECURSIVIDADE: FUNÇÕES QUE SE CHAMAM A SI PRÓPRIAS [~25 min]
===========================================================================

Ex 9.1 [EXEMPLO] Contagem decrescente -------------------------------------
"""

def countdown(n: int) -> None:
    """ Print n, n-1, ..., 1 and then "Partida!".
        Precondition: n >= 0
    """
    if n == 0:
        print("Partida!")
    else:
        print(n)
        countdown(n - 1)        # a função chama-se a si própria

countdown(3)

# Output:
#   3
#   2
#   1
#   Partida!
#
# Já sabes que uma função pode chamar outras funções (ficheiro 2, Ex 5.7).
# Também se pode chamar a SI PRÓPRIA. Chama-se RECURSIVIDADE.
#
# Cada chamada resolve um bocadinho (escreve um número)
# e passa o resto do trabalho a uma chamada com um problema MAIS PEQUENO (n - 1).
#
# Duas peças obrigatórias:
#   1. CASO BASE: quando parar (aqui n == 0). Não há chamada recursiva.
#   2. CASO RECURSIVO: chama-se a si própria com um valor mais perto do caso base.
#
# Por isso a recursividade só aparece agora: sem if não há caso base.






# %%
"""
Ex 9.2 [EXEMPLO] Fatorial: a definição da matemática, tal e qual ------------

    Na matemática:   0! = 1
                     n! = n * (n-1)!     para n > 0
"""

def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    if n == 0:
        return 1
    else:
        return n * factorial(n - 1)

print(factorial(4))         # 24
print(factorial(10))        # 3628800

# Como o Python calcula factorial(4):
#
#   factorial(4) = 4 * factorial(3)
#                      factorial(3) = 3 * factorial(2)
#                                         factorial(2) = 2 * factorial(1)
#                                                            factorial(1) = 1 * factorial(0)
#                                                                               factorial(0) = 1
#                                                            factorial(1) = 1 * 1 = 1
#                                         factorial(2) = 2 * 1 = 2
#                      factorial(3) = 3 * 2 = 6
#   factorial(4) = 4 * 6 = 24
#
# Primeiro as chamadas vão "descendo" até ao caso base.
# Cada uma fica à ESPERA da resposta da chamada de baixo.
# Depois as respostas "sobem" e cada multiplicação é feita no regresso.






# %%
"""
Ex 9.3 [EXEMPLO] Sem caso base, a recursividade nunca para -----------------
"""

def forever(n: int) -> int:
    """ Recursion WITHOUT a base case. """
    return n + forever(n - 1)

# print(forever(3))         # <- descomenta: RecursionError
# print(factorial(-1))      # <- descomenta: RecursionError também!

# RecursionError: maximum recursion depth exceeded
# O Python deixa cerca de 1000 chamadas "à espera" ao mesmo tempo.
# Ao passar esse limite, pára o programa.
#
# factorial(-1) também rebenta: -1, -2, -3... nunca chega ao 0.
# É por isso que o factorial tem a precondição n >= 0.






# %%
"""
Ex 9.4 [EXEMPLO] print antes ou depois da chamada: a ordem inverte -----------

    Antes de correr, tenta adivinhar o output das duas funções.
"""

def down(n: int) -> None:
    """ Print n down to 1. """
    if n > 0:
        print(n)
        down(n - 1)

def up(n: int) -> None:
    """ Print 1 up to n. """
    if n > 0:
        up(n - 1)
        print(n)

down(3)         # 3 2 1
print("---")
up(3)           # 1 2 3

# down: escreve PRIMEIRO, depois chama. Os números saem pela ordem das chamadas.
# up:   chama PRIMEIRO, e só escreve quando a chamada de baixo terminar.
#       up(3) espera por up(2), que espera por up(1), que espera por up(0).
#       up(0) não faz nada. Depois, no regresso: up(1) escreve 1, up(2) escreve 2...
#
# Aqui o caso base está escondido: quando n == 0, o if falha e a função não faz nada.
#
# Este tipo de pergunta ("o que escreve?") aparece em testes.






# %%
"""
Ex 9.5 [FAZ] Soma de 0 a n --------------------------------------------

    sum_to(n) = 0 + 1 + 2 + ... + n, de forma recursiva.
    Pensa: qual é o caso base? E como se escreve sum_to(n) usando sum_to(n - 1)?
"""

def sum_to(n: int) -> int:
    """ Sum 0 + 1 + ... + n.
        Precondition: n >= 0
    """
    pass        # TODO

print(sum_to(0), sum_to(4), sum_to(100))        # 0 10 5050
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 9.5

def sum_to(n: int) -> int:
    """ Sum 0 + 1 + ... + n.
        Precondition: n >= 0
    """
    if n == 0:
        return 0
    return n + sum_to(n - 1)

print(sum_to(0), sum_to(4), sum_to(100))        # 0 10 5050

# A ideia: a soma até n é n mais a soma até n - 1.
#   sum_to(4) = 4 + sum_to(3) = 4 + 3 + sum_to(2) = ... = 4 + 3 + 2 + 1 + 0
# É o mesmo esquema do factorial, com + em vez de * e 0 em vez de 1.






# %%
"""
Ex 9.6 [FAZ] Potência -------------------------------------------------

    power(b, e) = b elevado a e, de forma recursiva, sem usar **.
        b elevado a 0 = 1
        b elevado a e = b * (b elevado a e-1)
"""

def power(b: int, e: int) -> int:
    """ b raised to e.
        Precondition: e >= 0
    """
    pass        # TODO

print(power(2, 0), power(2, 10), power(3, 3))       # 1 1024 27
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 9.6

def power(b: int, e: int) -> int:
    """ b raised to e.
        Precondition: e >= 0
    """
    if e == 0:
        return 1
    return b * power(b, e - 1)

print(power(2, 0), power(2, 10), power(3, 3))       # 1 1024 27

# O e diminui em cada chamada: chega sempre ao 0 (por causa da precondição).
# O b não muda: passa igual para a chamada seguinte.






# %%
"""
Ex 9.7 [PENSA] Contar algarismos --------------------------------------

    count_digits(n): quantos algarismos tem n (n >= 0).
        count_digits(7) == 1      count_digits(472) == 3      count_digits(0) == 1
    Pista: o n // 10 tira o último algarismo (ficheiro 1, Ex 2.3).
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 9.7 (sem código):
#
#   caso base:      se n tem só um algarismo (n < 10) -> 1
#   caso recursivo: 1 (o último algarismo) + os algarismos de n // 10
#
#   count_digits(472) = 1 + count_digits(47) = 1 + 1 + count_digits(4) = 1 + 1 + 1
#
# Porque é que o caso base é "n < 10" e não "n == 0"?
# Experimenta mentalmente com n == 0 como caso base: quanto daria count_digits(0)?






# %%
"""
Ex 9.8 [DESAFIO] O que escreve? -------------------------------------------

    a) O que escreve f(5)? (atenção aos espaços e à mudança de linha)
    b) Quanto vale g(6)?
    Sem correr! Solução no fim do ficheiro.
"""

def f(n: int) -> None:
    if n > 0:
        print(n, end=" ")
        f(n - 2)
        print(n, end=" ")

def g(n: int) -> int:
    if n <= 1:
        return n
    return g(n - 1) + g(n - 2)

f(5)
print()
print(g(6))






# %%
"""
Ex 9.9 [DESAFIO] Máximo divisor comum de Euclides (teóricas 01a e 02a) ----

    O algoritmo de Euclides (300 a.C.) para o MDC de dois inteiros positivos:
      - se m == n, o MDC é m;
      - se m > n, o MDC de (m, n) é igual ao MDC de (m - n, n);
      - se m < n, o MDC de (m, n) é igual ao MDC de (m, n - m).
    Escreve a função de forma recursiva.
        gcd(252, 105) == 21        gcd(123, 456) == 3        gcd(7, 7) == 7

        def gcd(m: int, n: int) -> int:
            ''' Greatest common divisor of two natural numbers.
                Precondition: m > 0 and n > 0
            '''

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 9.10 [SOZINHA] Mais recursividade ----------------------------------

    a) digit_sum(n): soma dos algarismos de n (n >= 0), de forma recursiva.
           digit_sum(472) == 13      digit_sum(5) == 5
    b) multiply(a, b): a * b, para b >= 0, usando só somas (sem *).
           multiply(7, 3) == 21      multiply(7, 0) == 0

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
10. UM PROGRAMA BEM ORGANIZADO: A FUNÇÃO MAIN [~25 min]
===========================================================================

Ex 10.1 [EXEMPLO] Um programa completo -------------------------------------

    Corre e escreve um ano (ex: 2024).
"""

# 1. imports (se houver)

# 2. constantes
DAYS_IN_LEAP_YEAR = 366
DAYS_IN_COMMON_YEAR = 365

# 3. funções da lógica: recebem parâmetros, devolvem resultados.
#    Nada de input nem print.
def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if is_leap_year(year):
        return DAYS_IN_LEAP_YEAR
    return DAYS_IN_COMMON_YEAR

# 4. a função main: fala com o utilizador (input e print)
def main() -> None:
    year = int(input("Introduza o ano: "))
    print(f"O ano {year} tem {year_length(year)} dias.")

# 5. no fim, a chamada que põe o programa a andar
main()

# As 5 zonas, sempre por esta ordem (teórica 02b):
#   imports -> constantes -> funções -> main -> main()
#
# A REGRA: separar a conversa com o utilizador da lógica.
#   - main: lê os dados (input), chama as funções, escreve os resultados (print).
#   - restantes funções: só calculam. Recebem por parâmetro, devolvem com return.
#
# Porquê? Uma função sem input nem print pode ser usada em qualquer sítio:
# noutro programa, num teste automático, dentro de outra função.
# E é exatamente o que os testes pedem: "Não programe nenhuma função main,
# nem use input ou print."






# %%
"""
Ex 10.2 [EXEMPLO] Precondição vs validação ----------------------------------

    Corre duas vezes: uma com 5, outra com -3.
"""

def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    if n == 0:
        return 1
    return n * factorial(n - 1)

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not (x >= 0):
        print("Argumento inválido")
    else:
        print(f"fatorial({x}) = {factorial(x)}")

main()

# A precondição é um CONTRATO: a função diz "eu assumo n >= 0, não verifico".
# A main é quem fala com o utilizador, logo é quem pode receber lixo.
# Por isso é a main que VALIDA, antes de chamar a função.
#
# Truque para escrever a validação sem pensar muito:
# copia a precondição para dentro de "if not ( ... ):".
#     Precondition: n >= 0      ->     if not (x >= 0):
#     Precondition: a > 0 and b > 0  ->  if not (a > 0 and b > 0):
#
# No Mooshak e nos testes, o output tem de ser IGUAL ao do enunciado,
# carácter a carácter: "Argumento inválido" não é "Argumento invalido",
# e "Custo = 6.0" não é "Custo: 6.0".






# %%
"""
Ex 10.3 [FAZ] Programa do máximo (ex 16 dos guiões) -------------------

    Escreve a main: pede dois inteiros e escreve o maior.
        A: 3
        B: 8
        8
    Usa a função maximum do Ex 8.11 (corre essa célula antes).
"""

def main() -> None:
    pass        # TODO

main()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 10.3

def maximum(a: int, b: int) -> int:
    """ Largest of two integers. """
    if a >= b:
        return a
    return b

def main() -> None:
    a = int(input("A: "))
    b = int(input("B: "))
    print(maximum(a, b))

main()

# A main não tem nenhum if: a decisão é da função maximum.
# A main só lê, chama e escreve.






# %%
"""
Ex 10.4 [FAZ] Programa das notas (ex 18 dos guiões) -------------------

    As funções já estão feitas (são as do Ex 8.15).
    Corre esta célula primeiro, mesmo antes de escreveres a main.
    Escreve a main: pede T1, T2 e PR (reais).
    Se o aluno passou, escreve a nota final; senão escreve REPROVADO.
        T1: 10          T1: 10
        T2: 12          T2: 12
        PR: 15          PR: 8
        11.8            REPROVADO
"""

def average(a: float, b: float) -> float:
    """ Average of two reals. """
    return (a + b) / 2

def passed(t1: float, t2: float, pr: float) -> bool:
    """ Check if the student passed. """
    return pr >= 9.5 and average(t1, t2) >= 9.5

def final_grade(t1: float, t2: float, pr: float) -> float:
    """ Final grade.
        Precondition: passed(t1, t2, pr)
    """
    return t1 * 0.4 + t2 * 0.4 + pr * 0.2

def main() -> None:
    pass        # TODO

main()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 10.4
# (usa as funções average, passed e final_grade da célula do enunciado)

def average(a: float, b: float) -> float:
    """ Average of two reals. """
    return (a + b) / 2

def passed(t1: float, t2: float, pr: float) -> bool:
    """ Check if the student passed. """
    return pr >= 9.5 and average(t1, t2) >= 9.5

def final_grade(t1: float, t2: float, pr: float) -> float:
    """ Final grade.
        Precondition: passed(t1, t2, pr)
    """
    return t1 * 0.4 + t2 * 0.4 + pr * 0.2

def main() -> None:
    t1 = float(input("T1: "))
    t2 = float(input("T2: "))
    pr = float(input("PR: "))
    if passed(t1, t2, pr):
        print(final_grade(t1, t2, pr))
    else:
        print("REPROVADO")

main()

# A main respeita a precondição do final_grade:
# só a chama depois de confirmar que passed(...) é True.
#
# 10 * 0.4 + 12 * 0.4 + 15 * 0.2 = 4.0 + 4.8 + 3.0 = 11.8
# Com outras notas pode aparecer o "lixo" dos floats:
# 12, 14 e 15 dão 13.400000000000002. Nesse caso escreve com f"{...:.1f}"
# (se o enunciado não disser outra coisa).






# %%
"""
Ex 10.5 [PENSA] Programa dos triângulos (ex 17 dos guiões) ------------

    Programa completo: pede três reais A, B, C e escreve o tipo de triângulo
    (0, 1, 2 ou 3, como no Ex 8.14).
    Que funções precisas, e o que faz a main?
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 10.5 (sem código):
#
#   Zona das funções:
#     is_proper_triangle(a, b, c) -> bool     (Ex 7.11)
#     triangle_kind(a, b, c) -> int           (Ex 8.14, usa a de cima)
#   main:
#     ler A, B, C com float(input(...))
#     escrever triangle_kind(a, b, c)
#   no fim: main()
#
# Aqui a main não valida nada: um "triângulo inválido" é uma resposta
# possível da função (0), não um erro de input.






# %%
"""
Ex 10.6 [DESAFIO] Conta da eletricidade (Teste 1 2024/25, pergunta 6, 4 valores)

    Escreva um programa completo que calcule a conta de eletricidade dum
    cliente. O preço de cada unidade depende do consumo:

        Consumo          Custo
        [0, 200[         0.12 €/unidade
        [200, 400[       0.15 €/unidade
        [400, 600[       0.18 €/unidade
        [600, inf[       0.20 €/unidade

    Por exemplo, 300 unidades custam 300*0.15€. Há mais duas regras:
      - Se o custo das unidades consumidas for superior a 45.0€, então
        adiciona-se uma sobretaxa de 15% à conta final.
      - A conta final mínima é de 10.0€, mesmo que se tenha consumido
        pouca ou nenhuma eletricidade.
    O número de unidades é um inteiro >= 0. Os valores em euros são reais.

    Três exemplos de execução (imite esta apresentação):
        Qual o consumo: 800        Qual o consumo: 300        Qual o consumo: 50
        Custo = 160.0              Custo = 45.0               Custo = 6.0
        Sobretaxa = 24.0           Sobretaxa = 0.0            Sobretaxa = 0.0
        Total = 184.0              Total = 45.0               Total = 10.0

    Recomenda-se uma função para o custo, outra para a sobretaxa, outra para
    o total, e a main. Inclua em cada uma um comentário e possível precondição.

    Solução no fim do ficheiro.
"""






# %%
"""
Ex 10.7 [SOZINHA] Calculadora -----------------------------------------

    Programa completo. Pede dois reais e uma operação (+, -, *, /):
        A: 7
        Operação: /
        B: 2
        Resultado: 3.5
    Casos especiais:
      - divisão por zero  -> escreve "Divisão por zero"
      - operação desconhecida -> escreve "Operação inválida"

    Organização: uma função calculate(a, op, b) -> float com precondição
    (op válida e, se for "/", b != 0), e uma main que valida antes de chamar.

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  [ ] and, or, not e a prioridade entre eles
  [ ] funções booleanas is_... que devolvem a condição diretamente
  [ ] curto-circuito: a condição da esquerda pode proteger a da direita
  [ ] De Morgan: negar uma condição composta
  [ ] floats: nunca ==, usar abs(a - b) < EPSILON
  [ ] if, if/else, if/elif/else; o primeiro ramo True ganha
  [ ] a ordem dos ramos: os casos mais restritos primeiro
  [ ] ifs seguidos com return vs elif; sem return, ifs seguidos podem correr todos
  [ ] não escrever "== True" nem "if ...: return True else: return False"
  [ ] o if aceita 0, "" (falsos), mas nesta cadeira usa-se sempre uma condição
  [ ] variáveis criadas só em alguns ramos -> UnboundLocalError
  [ ] recursividade: caso base + caso recursivo; RecursionError
  [ ] as 5 zonas de um programa; main faz I/O e valida, funções calculam

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_4_ciclos_for_e_listas.py
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
# Solução do Ex 7.12 (Teste 1 2024/25, pergunta 2)

def christmas(day: int, month: int) -> bool:
    """ Check if it is a Christmas date. """
    return (month == 12 and day >= 23) or (month == 1 and day == 1)

print(christmas(23, 12), christmas(1, 1), christmas(22, 12), christmas(10, 2))
# True True False False

# Um único return. A versão com if/return True/return False perde pontos
# ("quanto mais simples, melhor").
# Não é preciso "day <= 31": assume-se que a data é válida.






# %%
# Solução do Ex 7.13 (Teste 1 2025/26, pergunta 2)

def is_root(r: float, a: float, b: float, c: float) -> bool:
    """ Check if r is a root of ax^2+bx+c=0 """
    EPSILON = 1e-9
    return abs(a * r * r + b * r + c) < EPSILON

print(is_root(0.0, 1.0, -3.0, 2.0), is_root(1.0, 1.0, -3.0, 2.0))   # False True
print(is_root(0.7, 1.0, -1.4, 0.49))                                # True

# "r é raiz" = pondo r no lugar de x, a conta dá zero.
# Não é preciso a fórmula resolvente ("não complique!").
# São floats: em vez de "== 0", "muito perto de 0".
# Com == 0, o último exemplo daria False: a conta dá 5.6e-17, não 0.






# %%
# Solução do Ex 7.14 (De Morgan)
#
#   a) not (x > 0 and y > 0)       ->   x <= 0 or y <= 0
#   b) not (age < 18 or age > 65)  ->   age >= 18 and age <= 65   (ou 18 <= age <= 65)
#   c) not (a == b) and not (b == c) -> a != b and b != c
#   d) not (x <= 5 or y == 3)      ->   x > 5 and y != 3
#
# O contrário de > é <= (e não <). O contrário de == é !=.






# %%
# Solução do Ex 8.16 (ex 25 dos guiões)

def and_(a: bool, b: bool) -> bool:
    """ AND - Boolean operation. """
    if a:
        return b            # a é True -> quem decide é o b
    return False            # a é False -> nem é preciso olhar para o b

def or_(a: bool, b: bool) -> bool:
    """ OR - Boolean operation. """
    if a:
        return True         # a é True -> já está
    return b                # a é False -> quem decide é o b

def not_(a: bool) -> bool:
    """ NOT - Boolean operation. """
    if a:
        return False
    return True

print(and_(True, False), or_(False, True), not_(True))      # False True False

# O and_ faz exatamente o curto-circuito do Ex 7.5:
# se o a é False, o b nem é usado.






# %%
# Solução do Ex 8.17
#
# a) 1) if a = b:       INCORRETO. "=" é atribuição; para comparar é "==".
#    2) if a < b < c:   CORRETO. Comparações encadeadas.
#    3) if x > 0        INCORRETO. Faltam os ":" no fim.
#    4) if x > 0: sem indentação no print -> INCORRETO (IndentationError).
#
# b) Escreve: B C A 7 A
#    mystery(9):  9 % 3 == 0, 9 % 5 != 0          -> 1º falha, elif -> "B"
#    mystery(10): não é múltiplo de 3              -> if e elif falham;
#                 o if seguinte (separado): 10 % 5 == 0 -> "C"
#    mystery(15): múltiplo de 3 e de 5             -> "A"
#    mystery(7):  nenhum                           -> str(7) = "7"
#    mystery(30): múltiplo de 3 e de 5             -> "A" (o 1º ramo ganha)
#
# Repara: o "if n % 5 == 0" separado funciona como um elif,
# porque todos os ramos de cima fazem return.






# %%
# Solução do Ex 8.18

def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def month_length(month: int, year: int) -> int:
    """ Number of days of a given month.
        Precondition: 1 <= month <= 12
    """
    if month == 4 or month == 6 or month == 9 or month == 11:
        return 30
    elif month == 2:
        if is_leap_year(year):
            return 29
        return 28
    else:
        return 31

def is_date_valid(day: int, month: int, year: int) -> bool:
    """ Check if a date exists. """
    return 1 <= month <= 12 and 1 <= day <= month_length(month, year)

print(is_date_valid(29, 2, 2024), is_date_valid(29, 2, 2023),
      is_date_valid(31, 4, 2024), is_date_valid(1, 13, 2024))
# True False False False

# O truque é o CURTO-CIRCUITO (Ex 7.5):
# se o mês for 13, "1 <= month <= 12" é False e o and pára ali.
# O month_length nunca é chamado com um mês inválido.
# Com a ordem trocada, month_length(13, 2024) seria chamado
# (violando a precondição): com o month_length do Ex 8.13 até dava 31,
# e a data 1/13/2024 passava por válida.






# %%
# Solução do Ex 9.8
#
# a) f(5) escreve: 5 3 1 1 3 5   (com um espaço no fim; o print() muda de linha)
#
#    f(5): escreve 5, chama f(3)
#      f(3): escreve 3, chama f(1)
#        f(1): escreve 1, chama f(-1) (não faz nada), escreve 1
#      f(3): escreve 3
#    f(5): escreve 5
#
#    O 1º print acontece na "descida", o 2º na "subida" (como no Ex 9.4).
#
# b) g(6) = 8   (é a sucessão de Fibonacci: 0 1 1 2 3 5 8)
#    g(0) = 0, g(1) = 1             (caso base)
#    g(2) = g(1) + g(0) = 1
#    g(3) = g(2) + g(1) = 2
#    g(4) = 3, g(5) = 5, g(6) = 8






# %%
# Solução do Ex 9.9

def gcd(m: int, n: int) -> int:
    """ Greatest common divisor of two natural numbers.
        Precondition: m > 0 and n > 0
    """
    if m == n:
        return m
    elif m > n:
        return gcd(m - n, n)
    else:
        return gcd(m, n - m)

print(gcd(252, 105), gcd(123, 456), gcd(7, 7))      # 21 3 7

# A tradução é direta: cada regra do algoritmo é um ramo do if.
# Caso base: m == n. Em cada chamada, um dos números diminui
# e os dois continuam positivos: chega-se sempre ao caso base.
#
# Cuidado com números muito diferentes (gcd(1, 5000)): são precisas
# 5000 chamadas e dá RecursionError (o limite é ~1000).
# A teórica faz este algoritmo com um ciclo (while), que não tem esse limite.






# %%
# Solução do Ex 10.6 (Teste 1 2024/25, pergunta 6)

SURCHARGE_LIMIT = 45.0      # acima disto paga sobretaxa
SURCHARGE_RATE = 0.15       # 15%
MINIMUM_BILL = 10.0         # conta mínima

def cost(units: int) -> float:
    """ Cost of the consumed units.
        Precondition: units >= 0
    """
    if units < 200:
        return units * 0.12
    elif units < 400:
        return units * 0.15
    elif units < 600:
        return units * 0.18
    else:
        return units * 0.20

def surcharge(c: float) -> float:
    """ Surcharge for a given cost.
        Precondition: c >= 0
    """
    if c > SURCHARGE_LIMIT:
        return c * SURCHARGE_RATE
    return 0.0

def total(c: float) -> float:
    """ Final bill for a given cost (never below the minimum).
        Precondition: c >= 0
    """
    t = c + surcharge(c)
    if t < MINIMUM_BILL:
        return MINIMUM_BILL
    return t

def main() -> None:
    units = int(input("Qual o consumo: "))
    c = cost(units)
    print(f"Custo = {c}")
    print(f"Sobretaxa = {surcharge(c)}")
    print(f"Total = {total(c)}")

main()

# Nos elif não é preciso "200 <= units": se chegámos lá, o if de cima falhou.
# "Superior a 45.0" é > (e não >=): com 300 unidades o custo é 45.0
# e NÃO paga sobretaxa. Ler o enunciado com lupa!
# Constantes com nome: nada de valores mágicos espalhados pelo código.
# (os preços por unidade também podiam ser constantes)
