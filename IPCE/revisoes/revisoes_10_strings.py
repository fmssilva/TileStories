# %%
"""
===========================================================================
REVISÕES 10 - STRINGS E CARATERES
O que é um caráter para o computador?
Como se percorre uma string, e como se constrói uma nova?
E os métodos que mais jeito dão: split, strip, upper, replace...
IPCE 2026/2027
===========================================================================


Porquê um ficheiro só para isto?
  Quase todos os programas trabalham com texto: nomes, frases, comandos,
  linhas lidas do teclado ou de um ficheiro.
  Uma string é uma SEQUÊNCIA de carateres: percorre-se como uma lista
  (ficheiro 4), com índices e com for.
  Mas tem uma diferença importante: uma string NÃO se pode mudar.
  Para "mudar" um texto, constrói-se outro. Aqui aprendes a fazê-lo,
  à mão (com ciclos) e com os métodos que o Python já traz.

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 35. Carateres e tipos ......................................... [~20 min]
       Porquê: para o computador, tudo são 0s e 1s.
       É o TIPO que diz se aqueles bits são um número ou uma letra.

 36. Strings como sequências .................................. [~30 min]
       Porquê: len, s[i], fatias, for, in. As mesmas peças das listas.

 37. Construir strings novas, e os métodos .................... [~35 min]
       Porquê: a receita da string nova (começar em "" e ir juntando),
       e os métodos que poupam ciclos: split, strip, upper, lower, replace...

 38. Desafios ................................................. [~40 min]
       Palíndromo (Teste 2 2024/25), comprimir texto, palavras de uma frase,
       passwords, e executar à mão.

  Resumo + soluções dos desafios

  Total: ~2h05. Não é para fazer de uma vez.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Cada célula corre sozinha: as funções de que precisa estão lá dentro.
  - Níveis:
      [EXEMPLO]  resolvido: lê, corre, experimenta outros valores.
      [FAZ]      contigo. Solução na célula a seguir, depois de muito espaço.
      [PENSA]    contigo. A "solução" é só a ideia, sem código.
      [DESAFIO]  mais difícil. Solução no fim do ficheiro.
      [SOZINHA]  sem solução.
  - Nos [DESAFIO], tenta primeiro numa folha de papel, e só depois escreve e corre aqui.
  - Se encravares: email ao professor das práticas com o número do exercício,
    o que já fizeste, onde encravaste e o ficheiro em anexo.

  Já resolvidos na aula 6 (aula_6.py) e na aula 7 (aula_7.py):
    next_char, print_chars, make_str_range, translate, a cifra de César,
    a tabela de carateres (ex 40 a 44), e a n-ésima ocorrência (Teste 2 2025/26).
  Também já viste o operador in com strings (ficheiro 4, secção 13).

  A numeração continua a dos ficheiros anteriores: este começa na secção 35.
"""






# %%
"""
===========================================================================
35. CARATERES E TIPOS [~20 min]
===========================================================================

    Em Python não há um tipo próprio para um caráter:
    um caráter é simplesmente uma string de tamanho 1.

        'a'   "Z"   '7'   ' '   '?'      <- cinco strings de tamanho 1

    E dentro do computador? Tudo são bits: 0s e 1s.
    Os mesmos bits podem querer dizer coisas diferentes:
    é o TIPO que diz como se leem. É por isso que existem tipos.

Ex 35.1 [EXEMPLO] Os mesmos bits, três significados ---------------------------
"""

bits = 0b1011010               # um inteiro escrito em binário (teórica 3b)
print(bits)                     # 90         lidos como um int
print(chr(bits))                # Z          lidos como um caráter (o código 90 é o 'Z')
print(type('Z'), len('Z'))      # <class 'str'> 1

# Se aqueles mesmos bits fossem lidos como um float (um real), davam
# 4.45e-322: um número minúsculo, que parece não ter nada a ver.
# (os reais guardam-se de outra maneira: sinal, expoente e mantissa)
#
# O computador não sabe se 1011010 é 90, 'Z' ou 4.45e-322.
# Quem diz é o tipo: int, str, float. Daí o int(input(...)) que fazes sempre:
# o input devolve TEXTO ("90" são os carateres '9' e '0'), e o int converte.






# %%
"""
Ex 35.2 [EXEMPLO] ord e chr: o número de cada caráter -------------------------

    ord(c) dá o número (o código) do caráter c; chr(n) faz o contrário.
    (o padrão chama-se Unicode; para as letras sem acento é o antigo ASCII)
"""

print(ord('A'), ord('B'), ord('Z'))     # 65 66 90
print(ord('a'), ord('0'), ord(' '))     # 97 48 32
print(chr(65), chr(97 + 2))             # A c
print(ord('a') - ord('A'))              # 32

# As maiúsculas estão seguidas (65 a 90), as minúsculas também (97 a 122),
# e os algarismos também ('0' é 48, ..., '9' é 57).
#
# Comparar carateres é comparar estes números:
print('A' < 'B', 'Z' < 'a', '9' < 'A')  # True True True
# Por isso todas as maiúsculas vêm antes de todas as minúsculas.






# %%
"""
Ex 35.3 [FAZ] Brincar com os códigos -----------------------------------------

    Sem usar int() nem métodos de strings, só com ord, chr e comparações:

    a) is_digit_char(c): c é um algarismo de '0' a '9'?
           is_digit_char('7') == True     is_digit_char('x') == False
    b) digit_value(c): o valor de um algarismo
           digit_value('7') == 7
    c) letter_position(c): a posição de uma minúscula no alfabeto, a começar em 1
           letter_position('a') == 1     letter_position('c') == 3
    d) letter_at(n): a letra minúscula na posição n (o contrário da c)
           letter_at(3) == 'c'           letter_at(26) == 'z'
"""

def is_digit_char(c: str) -> bool:
    """ Is c a digit from '0' to '9'?
        Precondition: len(c) == 1
    """
    pass        # TODO

def digit_value(c: str) -> int:
    """ Value of the digit c.
        Precondition: is_digit_char(c)
    """
    pass        # TODO

def letter_position(c: str) -> int:
    """ Position of the lowercase letter c in the alphabet, starting at 1.
        Precondition: 'a' <= c <= 'z'
    """
    pass        # TODO

def letter_at(n: int) -> str:
    """ The lowercase letter at position n of the alphabet.
        Precondition: 1 <= n <= 26
    """
    pass        # TODO

print(is_digit_char('7'), digit_value('7'), letter_position('c'), letter_at(3))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 35.3

def is_digit_char(c: str) -> bool:
    """ Is c a digit from '0' to '9'?
        Precondition: len(c) == 1
    """
    return '0' <= c <= '9'

def digit_value(c: str) -> int:
    """ Value of the digit c.
        Precondition: is_digit_char(c)
    """
    return ord(c) - ord('0')

def letter_position(c: str) -> int:
    """ Position of the lowercase letter c in the alphabet, starting at 1.
        Precondition: 'a' <= c <= 'z'
    """
    return ord(c) - ord('a') + 1

def letter_at(n: int) -> str:
    """ The lowercase letter at position n of the alphabet.
        Precondition: 1 <= n <= 26
    """
    return chr(ord('a') + n - 1)

print(is_digit_char('7'), is_digit_char('x'))      # True False
print(digit_value('7'))                            # 7
print(letter_position('a'), letter_position('c'))  # 1 3
print(letter_at(3), letter_at(26))                 # c z

# O truque de todas: "a distância a um ponto de partida".
#   ord('7') - ord('0') = 55 - 48 = 7
#   ord('c') - ord('a') = 2, e + 1 porque queremos começar em 1
# Funciona porque os algarismos e as letras estão SEGUIDOS na tabela.
#
# (é assim, por dentro, que o int("7") consegue dar 7)






# %%
"""
===========================================================================
36. STRINGS COMO SEQUÊNCIAS [~30 min]
===========================================================================

    Uma string é uma sequência de carateres, como uma lista de letras.
    Tudo o que já sabes de ler uma lista funciona:

        len(s)              o número de carateres
        s[i]                o caráter na posição i (começa em 0)
        s[-1]               o último
        s[a:b]              a fatia de a até b (o b não entra)
        for c in s          cada caráter (pelos valores)
        for i in range(len(s))   cada posição (pelos índices)
        c in s              está lá?
        s1 + s2, s * 3      juntar, repetir

    E uma regra nova: uma string NÃO se pode mudar (é imutável).

Ex 36.1 [EXEMPLO] Aceder e cortar --------------------------------------------

    Antes de correr, prevê cada print.
"""

s = "Python"

print(len(s))                   # 6
print(s[0], s[2], s[-1])        # P t n
print(s[1:4])                   # yth        posições 1, 2, 3 (o 4 não entra)
print(s[:2])                    # Py         do início até antes do 2
print(s[3:])                    # hon        do 3 até ao fim
print('th' in s, 'x' in s)      # True False
print(s + "!" * 3)              # Python!!!

# A fatia s[a:b] segue a regra do range: começa em a, acaba ANTES de b.
# Sem o a, começa no início; sem o b, vai até ao fim.
# Uma fatia é uma string NOVA: o s fica igual.






# %%
"""
Ex 36.2 [EXEMPLO] Percorrer: pelos valores ou pelos índices --------------------
"""

word = "banana"

# Pelos valores: só preciso de ler os carateres
count = 0
for c in word:
    if c == 'a':
        count += 1
print(count)                    # 3

# Pelos índices: preciso da posição
for i in range(len(word)):
    if word[i] == 'n':
        print(i, end=" ")       # 2 4
print()

# Pelos índices também quando preciso do VIZINHO (o caráter ao lado):
pairs = 0
for i in range(len(word) - 1):          # até ao penúltimo: o word[i + 1] tem de existir
    if word[i] == word[i + 1]:
        pairs += 1
print(pairs)                    # 0   (não há duas letras iguais seguidas)

# A regra é a das listas (ficheiro 4, Ex 14.2):
#   só ler os valores              -> for c in s
#   preciso da posição ou vizinho  -> for i in range(len(s))






# %%
"""
Ex 36.3 [EXEMPLO] Uma string não se pode mudar ----------------------------------
"""

s = "gato"
# s[0] = 'p'                    # TypeError: 'str' object does not support item assignment

s = 'p' + s[1:]                 # constrói-se uma string NOVA, e guarda-se no s
print(s)                        # pato

# As listas mudam-se por dentro (l[0] = ...); as strings não.
# Para "mudar" uma string, faz-se outra, com pedaços da antiga,
# e dá-se-lhe o mesmo nome. A antiga não mudou: foi substituída.
# (ficheiro 5: o s passou a apontar para outra string)






# %%
"""
Ex 36.4 [FAZ] Funções sobre strings ----------------------------------------------

    a) count_char(s, c): quantas vezes o caráter c aparece em s
           count_char("banana", 'a') == 3
    b) first_index(s, c): a posição do primeiro c em s, ou -1 se não houver
           first_index("banana", 'n') == 2     first_index("banana", 'x') == -1
    c) all_digits(s): todos os carateres de s são algarismos?
           all_digits("2026") == True     all_digits("20a6") == False
    d) initials(name1, name2): as iniciais de dois nomes, com pontos
           initials("Ana", "Silva") == "A.S."

    Sem usar métodos de strings (count, index...): só ciclos e o que já sabes.
"""

def count_char(s: str, c: str) -> int:
    """ How many times c occurs in s. """
    pass        # TODO

def first_index(s: str, c: str) -> int:
    """ Index of the first c in s, or -1. """
    pass        # TODO

def all_digits(s: str) -> bool:
    """ Are all the characters of s digits? """
    pass        # TODO

def initials(name1: str, name2: str) -> str:
    """ Initials of two names, with dots.
        Precondition: both names are not empty
    """
    pass        # TODO

print(count_char("banana", 'a'), first_index("banana", 'n'),
      all_digits("2026"), initials("Ana", "Silva"))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 36.4

def count_char(s: str, c: str) -> int:
    """ How many times c occurs in s. """
    count = 0
    for x in s:
        if x == c:
            count += 1
    return count

def first_index(s: str, c: str) -> int:
    """ Index of the first c in s, or -1. """
    for i in range(len(s)):
        if s[i] == c:
            return i
    return -1

def all_digits(s: str) -> bool:
    """ Are all the characters of s digits? """
    for c in s:
        if not ('0' <= c <= '9'):
            return False        # um contra-exemplo chega
    return True

def initials(name1: str, name2: str) -> str:
    """ Initials of two names, with dots.
        Precondition: both names are not empty
    """
    return name1[0] + "." + name2[0] + "."

print(count_char("banana", 'a'))                         # 3
print(first_index("banana", 'n'), first_index("banana", 'x'))   # 2 -1
print(all_digits("2026"), all_digits("20a6"))            # True False
print(initials("Ana", "Silva"))                          # A.S.

# São as receitas das listas (ficheiro 4), com strings:
#   contar        -> contador
#   procurar      -> for pelos índices + return a meio, -1 no fim
#   "todos?"      -> procurar um contra-exemplo (aula 5)
#
# E all_digits("")? Nenhum caráter falha, por isso dá True.
# "Todos os carateres de uma string vazia são algarismos" é verdade
# (não há nenhum que não seja). Se o enunciado quisesse False, era um if à parte.






# %%
"""
Ex 36.5 [PENSA] Prever ----------------------------------------------------------

    Sem correr: o que dá cada expressão?
"""

s = "Lisboa"
# a) s[1:3]
# b) s[-2:]
# c) s[0] + s[-1]
# d) "Ana" < "ana"
# e) "10" < "9"
# f) len(s * 2)
# g) "bo" in s

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 36.5 (sem código):
#
#   a) "is"        (posições 1 e 2)
#   b) "oa"        (do penúltimo até ao fim)
#   c) "La"
#   d) True        ('A' é 65, 'a' é 97: as maiúsculas vêm primeiro)
#   e) True        !!! comparam-se carateres, um a um: '1' (49) < '9' (57).
#                  Como TEXTO, "10" vem antes de "9" (como no dicionário).
#                  Para comparar como números: int("10") < int("9") dá False.
#   f) 12
#   g) True        (o in procura pedaços inteiros, não só um caráter)
#
# O e) é a armadilha clássica: números lidos com input e comparados
# sem o int(). Dá resultados "estranhos" e não dá erro nenhum.






# %%
"""
===========================================================================
37. CONSTRUIR STRINGS NOVAS, E OS MÉTODOS [~35 min]
===========================================================================

    Como a string não muda, a receita de "transformar" um texto é sempre:

        result = ""                     começar com a string vazia
        for c in s:                     para cada caráter...
            decidir o que juntar        (o próprio c? outro? nada?)
            result += ...
        return result

    É a receita da lista nova (ficheiro 4, Ex 14.6) com "" no lugar de [].

Ex 37.1 [EXEMPLO] Inverter e tirar os espaços -----------------------------------
"""

def reverse(s: str) -> str:
    """ s backwards. """
    result = ""
    for c in s:
        result = c + result     # cada caráter vai para a FRENTE
    return result

def remove_spaces(s: str) -> str:
    """ s without the spaces. """
    result = ""
    for c in s:
        if c != ' ':            # só junta o que não é espaço
            result += c
    return result

print(reverse("abc"))                   # cba
print(remove_spaces("o la, a mi go"))   # ola,amigo

# No reverse, c + result (e não result + c): o caráter novo fica à esquerda.
# À mão com "abc":  "" -> "a" -> "ba" -> "cba".
#
# No remove_spaces, o if decide se o caráter entra ou não (um filtro).






# %%
"""
Ex 37.2 [FAZ] Transformar texto à mão -------------------------------------------

    Sem métodos de strings: só a receita (result = "", for, +=).

    a) replace_char(s, old, new): troca cada caráter old pelo new
           replace_char("banana", 'a', 'o') == "bonono"
    b) double(s): cada caráter aparece duas vezes
           double("abc") == "aabbcc"
    c) censor(s): troca cada vogal (a, e, i, o, u) por '*'
           censor("programar") == "pr*gr*m*r"
"""

def replace_char(s: str, old: str, new: str) -> str:
    """ s with every old replaced by new. """
    pass        # TODO

def double(s: str) -> str:
    """ s with every character doubled. """
    pass        # TODO

def censor(s: str) -> str:
    """ s with every vowel replaced by '*'. """
    pass        # TODO

print(replace_char("banana", 'a', 'o'), double("abc"), censor("programar"))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 37.2

def replace_char(s: str, old: str, new: str) -> str:
    """ s with every old replaced by new. """
    result = ""
    for c in s:
        if c == old:
            result += new
        else:
            result += c
    return result

def double(s: str) -> str:
    """ s with every character doubled. """
    result = ""
    for c in s:
        result += c * 2         # ou c + c
    return result

VOWELS = "aeiou"

def censor(s: str) -> str:
    """ s with every vowel replaced by '*'. """
    result = ""
    for c in s:
        if c in VOWELS:
            result += '*'
        else:
            result += c
    return result

print(replace_char("banana", 'a', 'o'))    # bonono
print(double("abc"))                       # aabbcc
print(censor("programar"))                 # pr*gr*m*r

# As três são a mesma receita; só muda o "decidir o que juntar".
# O censor é o replace_char com uma condição maior (c in VOWELS).






# %%
"""
Ex 37.3 [EXEMPLO] Os métodos mais úteis (teórica 5b) -----------------------------

    O Python já traz muitas destas transformações feitas, como métodos:
    escrevem-se s.metodo(...). Estes são os que mais jeito dão.
"""

s = "  Olá Mundo  "

print(s.strip())                # "Olá Mundo"     tira os espaços do início e do fim
print(s.upper())                # "  OLÁ MUNDO  " tudo em maiúsculas
print(s.lower())                # "  olá mundo  " tudo em minúsculas
print("banana".replace("an", "AN"))     # bANANa     troca todos os pedaços
print("o gato preto".split())   # ['o', 'gato', 'preto']    parte nos espaços
print("12;7;30".split(";"))     # ['12', '7', '30']         parte nos ";"
print("2026".isdigit(), "20a6".isdigit())   # True False    só algarismos?
print("Olá".isalpha(), "Olá!".isalpha())    # True False    só letras?

print(s)                        # "  Olá Mundo  "   o s NÃO mudou!

# Os métodos de strings NUNCA mudam a string (ela é imutável):
# devolvem uma string NOVA. Para ficar com o resultado, guarda-se:
#     s = s.strip()
# Erro clássico: escrever só s.strip() numa linha, e esperar que o s mude.
#
# O split() devolve uma LISTA de strings: os pedaços.
#   sem nada        -> parte em qualquer quantidade de espaços
#   com um separador -> parte exatamente nesse separador
#
# Existem mais (join, lstrip, rstrip, isupper, islower, isspace, ...: teórica 5b),
# mas com estes resolves quase tudo.






# %%
"""
Ex 37.4 [EXEMPLO] split: palavras, e vários valores na mesma linha ---------------
"""

sentence = "  o  gato   preto "
words = sentence.split()
print(words, len(words))        # ['o', 'gato', 'preto'] 3

print(sentence.split(" "))      # ['', '', 'o', '', 'gato', '', '', 'preto', '']
# com " ", cada espaço é um corte: aparecem pedaços vazios. Para palavras: split().

# Ler vários números na mesma linha (teórica 5b):
line = "3 4 5"                  # (num programa: line = input("Valores: "))
parts = line.split()            # ['3', '4', '5']   ainda são TEXTO
total = 0
for p in parts:
    total += int(p)             # converter cada pedaço
print(total)                    # 12

# O padrão: input -> split -> converter cada pedaço -> usar.
# É o que faz a input_int_matrix do guião (aula 7) com o "ROWS COLS: 2 3".






# %%
"""
Ex 37.5 [FAZ] Com métodos ------------------------------------------------------------

    Agora podes usar os métodos do Ex 37.3.

    a) count_words(s): quantas palavras tem a frase
           count_words("  o  gato   preto ") == 3
    b) longest_word(s): a palavra mais comprida (a primeira, se houver empate)
           longest_word("o gato preto comeu") == "preto"
    c) capitalize(w): a palavra com a 1ª letra maiúscula e as outras minúsculas
           capitalize("sILVA") == "Silva"
    d) normalize_name(s): limpa um nome escrito à pressa
           normalize_name("  ana   SILVA ") == "Ana Silva"
       Pista: split, a capitalize da c), e juntar com um espaço entre palavras.
"""

def count_words(s: str) -> int:
    """ Number of words of s. """
    pass        # TODO

def longest_word(s: str) -> str:
    """ The longest word of s (the first, if there is a tie).
        Precondition: s has at least one word
    """
    pass        # TODO

def capitalize(w: str) -> str:
    """ w with the first letter in uppercase and the others in lowercase.
        Precondition: w is not empty
    """
    pass        # TODO

def normalize_name(s: str) -> str:
    """ The words of s, capitalized, separated by one space. """
    pass        # TODO

print(count_words("  o  gato   preto "), longest_word("o gato preto comeu"),
      capitalize("sILVA"), normalize_name("  ana   SILVA "))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 37.5

def count_words(s: str) -> int:
    """ Number of words of s. """
    return len(s.split())

def longest_word(s: str) -> str:
    """ The longest word of s (the first, if there is a tie).
        Precondition: s has at least one word
    """
    words = s.split()
    best = words[0]
    for w in words:
        if len(w) > len(best):          # > e não >=: fica a primeira do empate
            best = w
    return best

def capitalize(w: str) -> str:
    """ w with the first letter in uppercase and the others in lowercase.
        Precondition: w is not empty
    """
    return w[0].upper() + w[1:].lower()

def normalize_name(s: str) -> str:
    """ The words of s, capitalized, separated by one space. """
    result = ""
    for w in s.split():
        if result != "":                # antes de cada palavra, menos da primeira
            result += " "
        result += capitalize(w)
    return result

print(count_words("  o  gato   preto "))        # 3
print(longest_word("o gato preto comeu"))       # preto
print(capitalize("sILVA"))                      # Silva
print(normalize_name("  ana   SILVA "))         # Ana Silva

# O split() faz o trabalho difícil (os espaços a mais desaparecem).
# Depois é a receita da string nova, palavra a palavra.
#
# O pormenor do espaço: só se junta um " " ANTES de cada palavra que não seja
# a primeira. Assim não fica um espaço a sobrar no fim.
# (com o método join seria " ".join(...); não precisas dele para nada disto)






# %%
"""
Ex 37.6 [PENSA] Com ou sem métodos? -------------------------------------------------

    Nos testes, às vezes o enunciado diz "programe diretamente usando um ciclo"
    ou "sem recorrer a nenhuma função predefinida". E outras vezes não diz nada.

    a) Se o enunciado proíbe os métodos, como fazes count_words sem split?
       (pensa: o que marca o início de uma palavra?)
    b) Se o enunciado não diz nada, usar o split é batota?
"""

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 37.6 (sem código):
#
#   a) Uma palavra começa num caráter que não é espaço, e que está no início
#      da string ou logo depois de um espaço. Contar esses inícios:
#
#          count = 0
#          for i in range(len(s)):
#              if s[i] != ' ' and (i == 0 or s[i - 1] == ' '):
#                  count += 1
#
#      (o "i == 0 or" vem primeiro de propósito: com i = 0, o s[i - 1] seria
#       o s[-1], o último caráter. O or pára no primeiro True.)
#
#   b) Não é batota: se não está proibido, usar o que a linguagem oferece é
#      o mais simples e o mais legível. Mas é preciso saber as DUAS maneiras,
#      porque quando o enunciado proíbe, só a do ciclo vale.
#      Regra: ler o enunciado até ao fim antes de escolher.






# %%
"""
===========================================================================
38. DESAFIOS [~40 min]
===========================================================================

    Mais perto do nível dos testes. Soluções no fim do ficheiro.

Ex 38.1 [DESAFIO] Palíndromo (Teste 2 2024/25, pergunta 2, 3 valores) ---------------

    Escreva uma função para testar se uma string é um palíndromo.
    Um palíndromo é um texto que se lê da mesma maneira da esquerda para a direita
    ou da direita para a esquerda. Exemplos:

        is_palindrome('amor a roma') = True
        is_palindrome('ananana') = True
        is_palindrome('') = True
        is_palindrome('palindrome') = False

    Por favor, escreva a função de uma forma que analise ou manipule diretamente
    os caracteres da string, sem recorrer a nenhuma função predefinida do Python.
    Como exceção, pode usar a função len.

    Pista: compara o 1º com o último, o 2º com o penúltimo, ...
    Até onde é preciso ir?
"""

def is_palindrome(s: str) -> bool:
    """ Check if a string is a palindrome. """
    pass        # TODO

print(is_palindrome('amor a roma'), is_palindrome('palindrome'))






# %%
"""
Ex 38.2 [DESAFIO] Comprimir texto ------------------------------------------------------

    compress(s): cada grupo de carateres iguais seguidos passa a ser
    o caráter e quantas vezes aparece.

        compress("aaabcc") == "a3b1c2"
        compress("abc") == "a1b1c1"
        compress("") == ""

    Pista: um contador que recomeça cada vez que o caráter muda.
    E cuidado com o ÚLTIMO grupo (não há "mudança" depois dele).
"""

def compress(s: str) -> str:
    """ Run-length encoding of s. """
    pass        # TODO

print(compress("aaabcc"))






# %%
"""
Ex 38.3 [DESAFIO] As palavras de uma frase (teórica 5b) -------------------------------

    a) get_words(s): a lista das palavras de s, ignorando a pontuação
       (tudo o que não é letra conta como separador).
           get_words("Olá, mundo! Tudo bem?") == ['Olá', 'mundo', 'Tudo', 'bem']
    b) average_word_length(s): o comprimento médio das palavras
           average_word_length("Olá, mundo! Tudo bem?") == 3.75

    Pista para a a): constrói uma string nova em que cada caráter que não é
    letra (isalpha) passa a espaço, e depois usa o split.
"""

def get_words(s: str) -> list[str]:
    """ The words of s (sequences of letters). """
    pass        # TODO

def average_word_length(s: str) -> float:
    """ Average length of the words of s.
        Precondition: s has at least one word
    """
    pass        # TODO

print(get_words("Olá, mundo! Tudo bem?"))






# %%
"""
Ex 38.4 [DESAFIO] Uma password forte -----------------------------------------------------

    password_problems(p): a lista dos problemas da password (vazia se for forte).
    Uma password é forte se:
        tem pelo menos 8 carateres            senão: "curta"
        tem pelo menos uma maiúscula          senão: "sem maiúscula"
        tem pelo menos um algarismo           senão: "sem algarismo"

        password_problems("abc") == ['curta', 'sem maiúscula', 'sem algarismo']
        password_problems("abcdefgh1") == ['sem maiúscula']
        password_problems("Abcdefg1") == []

    Pista: duas bandeiras (has_upper, has_digit) e um só for.
"""

def password_problems(p: str) -> list[str]:
    """ List of the problems of the password p (empty if it is strong). """
    pass        # TODO

print(password_problems("abc"))






# %%
"""
Ex 38.5 [DESAFIO] Executar à mão ----------------------------------------------------------

    Sem correr: o que escreve cada bloco? Faz a tabela numa folha.
    Depois corre para confirmar.
"""

# a)
s = "banana"
r = ""
for i in range(len(s)):
    if i % 2 == 0:
        r += s[i].upper()
    else:
        r += s[i]
print(r)

# b)
count = 0
for c in "Mississippi":
    if c in "sp":
        count += 1
print(count)

# c)
s = "programa"
i = len(s) - 1
t = ""
while i >= 0:
    t += s[i]
    i -= 2
print(t)

# d)
words = "um dois tres".split()
r = ""
for w in words:
    r = r + w[0] + w[-1]
print(r, len(words))






# %%
"""
Ex 38.6 [SOZINHA] Anagramas -----------------------------------------------------------

    Duas palavras são anagramas se tiverem exatamente as mesmas letras,
    o mesmo número de vezes (só a ordem muda).

        is_anagram("roma", "amor") == True
        is_anagram("ana", "nana") == False

    Pista: a count_char do Ex 36.4. Para cada letra de uma, conta quantas há
    em cada palavra. (e os tamanhos têm de ser iguais)
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  CARATERES
  [ ] um caráter é uma string de tamanho 1
  [ ] tudo são bits: é o tipo que diz se é um número ou uma letra
  [ ] ord(c) dá o código; chr(n) dá o caráter; letras e algarismos estão seguidos
  [ ] ord(c) - ord('0'), ord(c) - ord('a'): a distância a um ponto de partida

  STRINGS COMO SEQUÊNCIAS
  [ ] len(s), s[i], s[-1], s[a:b] (o b não entra), in, +, *
  [ ] for c in s (valores) ou for i in range(len(s)) (posição, vizinho)
  [ ] uma string NÃO se pode mudar: s[0] = ... dá TypeError
  [ ] "10" < "9" é True: comparar texto não é comparar números

  STRINGS NOVAS
  [ ] result = "", for c in s, decidir o que juntar, result += ..., return
  [ ] inverter: result = c + result
  [ ] os métodos devolvem uma string NOVA: s = s.strip() (e não só s.strip())
  [ ] strip, upper, lower, replace, isdigit, isalpha
  [ ] split() -> lista de palavras; split(";") -> partir num separador
  [ ] input -> split -> converter cada pedaço -> usar
  [ ] se o enunciado proíbe os métodos: o ciclo à mão

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_11_colecoes.py (tuplos, conjuntos, dicionários: quando usar o quê).
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
# Solução do Ex 38.1

def is_palindrome(s: str) -> bool:
    """ Check if a string is a palindrome. """
    n = len(s)
    for i in range(n // 2):                 # só até ao meio
        if s[i] != s[n - 1 - i]:            # o i-ésimo a contar do fim
            return False
    return True

print(is_palindrome('amor a roma'), is_palindrome('ananana'),
      is_palindrome(''), is_palindrome('palindrome'))      # True True True False

# Até ao meio (n // 2) chega: depois disso, as comparações repetiam-se.
# Com n ímpar, o do meio fica de fora, e não faz mal (compara-se consigo próprio).
# O "n - 1 - i" é o mesmo das diagonais (aula 6, aula 7): a posição espelho.
#
# Outra forma, com dois índices que se aproximam (ficheiro 8):
#
#     left = 0
#     right = len(s) - 1
#     while left < right:
#         if s[left] != s[right]:
#             return False
#         left += 1
#         right -= 1
#     return True
#
# (o enunciado proíbe "funções predefinidas": nada de s[::-1] nem reversed)






# %%
# Solução do Ex 38.2

def compress(s: str) -> str:
    """ Run-length encoding of s. """
    if s == "":
        return ""
    result = ""
    current = s[0]
    count = 0
    for c in s:
        if c == current:
            count += 1
        else:                               # o grupo acabou: guardar e recomeçar
            result += current + str(count)
            current = c
            count = 1
    result += current + str(count)          # o último grupo, que ficou por guardar
    return result

print(compress("aaabcc"), compress("abc"), compress(""))     # a3b1c2 a1b1c1

# O erro típico: esquecer a linha depois do for. O último grupo nunca "muda",
# por isso dentro do ciclo nunca era guardado: "aaabcc" dava "a3b1".
# Confirmar à mão com o caso mais pequeno ("a") apanha logo isto.
#
# str(count): o número tem de passar a texto para se juntar com +.






# %%
# Solução do Ex 38.3

def get_words(s: str) -> list[str]:
    """ The words of s (sequences of letters). """
    clean = ""
    for c in s:
        if c.isalpha():
            clean += c
        else:
            clean += ' '                    # tudo o que não é letra vira separador
    return clean.split()

def average_word_length(s: str) -> float:
    """ Average length of the words of s.
        Precondition: s has at least one word
    """
    words = get_words(s)
    total = 0
    for w in words:
        total += len(w)
    return total / len(words)

print(get_words("Olá, mundo! Tudo bem?"))               # ['Olá', 'mundo', 'Tudo', 'bem']
print(average_word_length("Olá, mundo! Tudo bem?"))     # 3.75

# É a get_words da teórica 5b. A ideia: em vez de procurar a pontuação,
# troca-se por espaços, e o split() faz o resto.
# A teórica avisa: as palavras com hífen ("segunda-feira") ficam partidas em duas.






# %%
# Solução do Ex 38.4

MIN_LENGTH = 8

def password_problems(p: str) -> list[str]:
    """ List of the problems of the password p (empty if it is strong). """
    has_upper = False
    has_digit = False
    for c in p:
        if 'A' <= c <= 'Z':
            has_upper = True
        if c.isdigit():
            has_digit = True
    problems = []
    if len(p) < MIN_LENGTH:
        problems.append("curta")
    if not has_upper:
        problems.append("sem maiúscula")
    if not has_digit:
        problems.append("sem algarismo")
    return problems

print(password_problems("abc"))             # ['curta', 'sem maiúscula', 'sem algarismo']
print(password_problems("abcdefgh1"))       # ['sem maiúscula']
print(password_problems("Abcdefg1"))        # []

# Duas bandeiras (aula 5): começam False, passam a True quando se encontra.
# Um só for responde às duas perguntas.
# E três if SEPARADOS (não elif): cada problema é independente dos outros.
#
# 'A' <= c <= 'Z' (secção 35) e c.isdigit() (Ex 37.3): as duas maneiras servem.






# %%
# Solução do Ex 38.5

# a) BaNaNa
#    posições pares (0, 2, 4) em maiúscula: B a N a N a
#
# b) 6
#    em "Mississippi" há 4 's' e 2 'p'. O c in "sp" é "c é 's' ou 'p'?"
#
# c) aagr
#    "programa" tem 8 carateres. Primeiro a tabela das posições:
#        p r o g r a m a
#        0 1 2 3 4 5 6 7
#    i = 7, 5, 3, 1 (depois -1, e -1 >= 0 é falso: pára)
#    s[7] = 'a', s[5] = 'a', s[3] = 'g', s[1] = 'r'  ->  "aagr"
#    (fazer a tabela das posições ANTES de ler os carateres evita enganos)
#
# d) umdsts 3
#    "um" -> 'u' + 'm', "dois" -> 'd' + 's', "tres" -> 't' + 's'
#    w[0] é a primeira letra, w[-1] a última; e o split deu 3 palavras.