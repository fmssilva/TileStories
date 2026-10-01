# %%
"""
===========================================================================
REVISÕES 1 - PRIMEIROS PASSOS EM PYTHON
Variáveis, tipos, operadores, print/input e conversões
IPCE 2026/2027
===========================================================================

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

  0. Como usar este ficheiro ................................. [~10 min]

  1. Variáveis e atribuição ................................... [~40 min]
       Porquê: um programa guarda valores, dá-lhes nomes
       e vai-os transformando. Tudo o resto assenta nisto.

  2. Tipos de dados e operadores .............................. [~50 min]
       Porquê: 7 / 2 e 7 // 2 dão resultados diferentes.
       7 e "7" não são a mesma coisa. Saber o tipo de cada valor
       evita metade dos erros.

  3. Falar com o utilizador: print e input .................... [~30 min]
       Porquê: é assim que o programa mostra resultados
       e recebe dados. O Mooshak testa exatamente isto.

  4. Converter entre tipos .................................... [~30 min]
       Porquê: o input devolve sempre texto.
       Para fazer contas com o que o utilizador escreve, é preciso converter.

  Resumo + soluções dos desafios

  Total: ~2h40. Não é para fazer de uma vez: uma ou duas secções de cada vez.

A SÉRIE COMPLETA
  revisoes_1_variaveis_tipos_input.py   <- estás aqui
  revisoes_2_funcoes_e_escopo.py        (funções, return, escopo das variáveis)
  revisoes_3_condicoes_e_if.py          (booleanos, if/elif/else, recursividade, main)
  revisoes_4_ciclos_for_e_listas.py     (for, range, listas, in)
  revisoes_5_valores_referencias_escopo.py  (o que acontece às variáveis: setas e escopo)
  revisoes_6_teste_exemplo_1/2/3.py     (testes completos, ao nível dos testes 1)

  A numeração das secções e dos exercícios continua de ficheiro para ficheiro
  (o ficheiro 2 começa na secção 5). Assim "Ex 2.3" é sempre o mesmo exercício.
"""






# %%
"""
===========================================================================
0. COMO USAR ESTE FICHEIRO [~10 min]
===========================================================================

CÉLULAS
  - Cada bloco que começa com "# %%" é uma célula.
  - Ctrl + Enter   -> corre a célula onde está o cursor.
  - Shift + Enter  -> corre a célula e salta para a seguinte.
  - O resultado aparece na consola (por defeito, em baixo à direita).
  - Evita o F5 (corre o ficheiro todo de uma vez e pede todos os inputs seguidos).

AS CÉLULAS PARTILHAM A MEMÓRIA
  - Uma variável criada numa célula continua a existir nas células seguintes.
  - Por isso corre as células POR ORDEM.
  - Se uma célula der "NameError", o mais provável é teres saltado uma célula anterior.

NÍVEIS DOS EXERCÍCIOS
  [EXEMPLO]  já resolvido. Lê, corre, e muda valores para ver o que acontece.
  [FAZ]      é contigo. A solução está na célula a seguir, depois de muito espaço.
  [PENSA]    é contigo. A "solução" é só a ideia (pseudocódigo), sem código.
  [DESAFIO]  nível de teste. Solução só no fim do ficheiro.
  [SOZINHA]  sem solução. Tens de chegar lá por ti.

COMO TRABALHAR
  - Escreve as tuas soluções neste ficheiro, por baixo de cada "TODO".
  - Tenta SEMPRE antes de ver a solução. Errar e corrigir é o que ensina.
  - Mesmo que a tua solução esteja certa, lê a solução oficial:
    muitas vezes traz um pormenor novo.

SE ENCRAVARES
  Envia email ao professor das práticas com:
    - o número do exercício (ex: "Ex 2.11");
    - o que já conseguiste fazer;
    - a ideia que tens para a solução e onde encravaste;
    - este ficheiro em anexo.
"""






# %%
"""
Ex 0.1 [EXEMPLO] O primeiro programa ------------------------------------

    Corre esta célula (Ctrl + Enter) e olha para a consola.
"""

print("Olá, Python!")
print(2 + 3)
print("2 + 3")

# O print escreve na consola o que está dentro dos parêntesis.
# print(2 + 3)   -> o Python FAZ a conta e escreve 5.
# print("2 + 3") -> entre aspas é texto: escreve tal e qual, sem fazer contas.
#
# Cada print escreve uma linha.
# As linhas correm de cima para baixo, uma de cada vez.






# %%
"""
Ex 0.2 [FAZ] O teu primeiro programa ------------------------------------

    Escreve um programa com 3 prints:
      - o teu nome (texto);
      - o número de horas de um ano (365 dias vezes 24 horas);
      - o texto "365 * 24" (sem fazer a conta).
"""

# TODO: escreve aqui os teus 3 prints















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 0.2

print("Ana")
print(365 * 24)       # 8760
print("365 * 24")

# Em Python a multiplicação é "*" (e não "x" nem "·").






# %%
"""
===========================================================================
1. VARIÁVEIS E ATRIBUIÇÃO [~40 min]
===========================================================================

Ex 1.1 [EXEMPLO] Guardar valores com um nome ------------------------------
"""

age = 19
name = "Ana"
print(age)
print(name)
print(age, name)

# "age = 19" lê-se: "age passa a valer 19".
# Do lado ESQUERDO fica o nome. Do lado DIREITO fica o valor.
# A isto chama-se ATRIBUIÇÃO.
#
# A atribuição não escreve nada na consola.
# Só guarda o valor. Para o ver, é preciso print.
#
# Um print com várias coisas separadas por vírgulas
# escreve-as na mesma linha, separadas por um espaço.






# %%
"""
Ex 1.2 [EXEMPLO] O "=" não é o "=" da matemática ------------------------
"""

x = 5
x = x + 1
print(x)        # 6

# Na matemática, "x = x + 1" é impossível.
# Em Python quer dizer: "o novo x é o velho x mais 1".
#
# O Python faz SEMPRE assim:
#   1. calcula o lado direito com os valores atuais (5 + 1 = 6);
#   2. só depois guarda o resultado na variável da esquerda.
#
# O valor antigo (5) perde-se. Uma variável guarda um valor de cada vez.






# %%
"""
Ex 1.3 [EXEMPLO] Executar código "à mão" com uma tabela -----------------

    Nos testes não há computador.
    Aparecem perguntas do tipo "o que escreve este código?".
    A técnica é fazer uma tabela: uma coluna por variável,
    uma linha por cada instrução que muda alguma coisa.

    Antes de correr, tenta adivinhar o que aparece.
"""

a = 3
b = a
a = 5
print(a, b)

# Tabela:
#   instrução  |  a  |  b
#   a = 3      |  3  |  -
#   b = a      |  3  |  3
#   a = 5      |  5  |  3
#
# Escreve: 5 3
#
# "b = a" guardou em b o valor que o a tinha NAQUELE MOMENTO (3).
# Depois, mudar o a não muda o b. O b não "segue" o a.
#
# (com listas, no ficheiro 4, vais ver que isto tem uma surpresa)






# %%
"""
Ex 1.4 [FAZ] Executar à mão -----------------------------------------------

    Faz a tabela no papel e escreve o que aparece no print.
    Só depois corre para confirmar.
"""

x = 10
y = x * 2
x = y - 5
y = y + x
print(x, y)

# TODO: escreve aqui a tua tabela e a tua resposta, em comentário
#   instrução  |  x  |  y
#
# Resposta:















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 1.4
#
#   instrução    |  x  |  y
#   x = 10       |  10 |  -
#   y = x * 2    |  10 |  20
#   x = y - 5    |  15 |  20
#   y = y + x    |  15 |  35
#
# Escreve: 15 35
#
# Na última linha, o x já vale 15 (e não 10).
# Usa-se sempre o valor ATUAL de cada variável.






# %%
"""
Ex 1.5 [EXEMPLO] O primeiro erro: NameError -------------------------------

    Os erros fazem parte. Toda a gente os tem, todos os dias.
    O que interessa é saber LER a mensagem.

    Descomenta a linha do print (tira o "#" do início) e corre.
"""

# print(score)

# Aparece algo assim:
#
#   ----> 1 print(score)
#   NameError: name 'score' is not defined
#
# Como ler:
#   - a ÚLTIMA linha diz o tipo de erro e a explicação;
#   - a seta "---->" aponta para a linha onde aconteceu.
#
# NameError = "não conheço esse nome". Causas habituais:
#   1. a variável nunca foi criada;
#   2. o nome está mal escrito (score, Score e SCORE são 3 nomes diferentes);
#   3. a célula onde a variável é criada ainda não foi corrida.
#
# Volta a pôr o "#" na linha, para a célula correr sem erro.






# %%
"""
Ex 1.6 [EXEMPLO] Regras para os nomes das variáveis -----------------------
"""

total_price = 10        # letras minúsculas, palavras separadas por "_"
x2 = 5                  # pode ter algarismos...
# 2x = 5                # ...mas não pode COMEÇAR por algarismo (SyntaxError)
# my-price = 5          # "-" é o sinal de menos, não pode estar num nome
# class = 5             # palavras reservadas do Python não podem ser nomes

# Regras:
#   - letras, algarismos e "_";
#   - não começa por algarismo;
#   - maiúsculas e minúsculas contam (age e Age são diferentes);
#   - não pode ser palavra reservada (if, for, def, return, class, True...).
#
# Convenção da cadeira: nomes em inglês, minúsculas, palavras separadas por "_".
#
# SyntaxError = "isto não é Python válido".
# O Spyder costuma avisar ANTES de correres, com um sinal na margem esquerda.
#
# Evita também nomes que o Python já usa: print, input, sum, max, min, list...
# Funciona, mas "tapa" a função original:
#   print = 5       -> a partir daqui o print deixa de funcionar
#                      (até reiniciares a consola)






# %%
"""
Ex 1.7 [EXEMPLO] Atribuição aumentada: +=, -=, *=, /= ---------------------
"""

points = 10
points += 5         # o mesmo que: points = points + 5   -> 15
points -= 3         # o mesmo que: points = points - 3   -> 12
points *= 2         # o mesmo que: points = points * 2   -> 24
print(points)       # 24

# É só uma forma mais curta de escrever. A cadeira prefere esta forma.
#
# Cuidado: é "+=" e não "=+".
#   points =+ 3   -> quer dizer "points = +3" (o "+" é o sinal do número)
#   Não dá erro nenhum. Só dá o resultado errado. É um bug silencioso.






# %%
"""
Ex 1.8 [EXEMPLO] Atribuição paralela e troca de valores -------------------
"""

a, b = 1, 2         # a = 1 e b = 2, numa linha só
print(a, b)         # 1 2

a, b = b, a         # troca!
print(a, b)         # 2 1

# Na atribuição paralela, o Python primeiro calcula TODO o lado direito
# (b, a) -> (2, 1), e só depois guarda: a = 2, b = 1.
#
# A forma ERRADA de trocar, com duas atribuições simples:
a, b = 1, 2
a = b               # a = 2
b = a               # b = 2   (o 1 já se tinha perdido!)
print(a, b)         # 2 2






# %%
"""
Ex 1.9 [FAZ] Uma conta de compras -------------------------------------

    Um caderno custa 12.5 euros. Compraste 3.
    Tens 10% de desconto sobre o total.
    Calcula o total a pagar e escreve-o.

    Usa as variáveis já criadas. O resultado deve ser 33.75.
"""

price = 12.5
quantity = 3
discount = 0.10         # 10%

# TODO: calcula o total (numa variável chamada total) e escreve-o















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 1.9

price = 12.5
quantity = 3
discount = 0.10

total = price * quantity            # 37.5
total = total - total * discount    # 37.5 - 3.75 = 33.75
print(total)                        # 33.75

# Outra forma: pagar 90% do total
# total = price * quantity * (1 - discount)
#
# Reparem: os números decimais escrevem-se com PONTO (12.5) e não vírgula.
# Em Python, 12,5 quer dizer outra coisa (dois valores separados).






# %%
"""
Ex 1.10 [FAZ] Trocar dois valores sem atribuição paralela -----------------

    Troca os valores de a e b SEM usar "a, b = b, a".
    No fim deve aparecer: 20 10

    Pista: precisas de uma terceira variável para guardar um valor
    antes de ele se perder.
"""

a = 10
b = 20

# TODO: troca os valores aqui (3 linhas)

print(a, b)















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 1.10

a = 10
b = 20

temp = a            # guardar o 10 antes de o perder
a = b               # a = 20
b = temp            # b = 10
print(a, b)         # 20 10

# Tabela:
#   instrução  |  a  |  b  | temp
#   temp = a   |  10 |  20 |  10
#   a = b      |  20 |  20 |  10
#   b = temp   |  20 |  10 |  10






# %%
"""
Ex 1.11 [PENSA] Rodar três valores ------------------------------------

    a = 1, b = 2, c = 3.
    Queremos que fique: a = 2, b = 3, c = 1
    (cada um recebe o valor do seguinte, e o c recebe o do a).

    Só podes usar atribuições simples (sem "a, b, c = ...").
    Quantas linhas precisas?
"""

a, b, c = 1, 2, 3

# TODO

print(a, b, c)      # deve dar: 2 3 1















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 1.11 (sem código):
#
#   1. guardar o valor do a numa variável temporária (vai perder-se primeiro);
#   2. a recebe o b;
#   3. b recebe o c;
#   4. c recebe a temporária.
#
# São 4 atribuições. Confirma com uma tabela antes de escreveres o código.
#
# Pergunta extra: e se a rotação fosse ao contrário (a = 3, b = 1, c = 2)?
# Qual é a variável que tens de guardar primeiro?






# %%
"""
Ex 1.12 [DESAFIO] Sintaxe correta ou incorreta? (Teste 1 2024/25, 1b) -----

    Para cada linha, diz se o Python a aceita (correto) ou se é erro de
    sintaxe (incorreto). Assume que as variáveis já existem.
    Só interessam os erros de sintaxe (os que o Spyder marca a vermelho).

      a) a = 6b + 3c
      b) v = (a == 5)
      c) x + 1 = y
      d) total = total + 1
      e) 2nd_place = 4
      f) a, b = b, a
      g) my_var=3

    As alíneas a) e b) são do teste. As outras são do mesmo estilo.
    Solução no fim do ficheiro.
"""






# %%
"""
Ex 1.13 [DESAFIO] O que escreve? -----------------------------------------

    Faz a tabela à mão. Só depois corre.
    Solução no fim do ficheiro.
"""

a, b, c = 1, 2, 3
a = b + c
c = a * b
b, c = c, b
a += c
print(a, b, c)






# %%
"""
Ex 1.14 [SOZINHA] Trocar sem terceira variável ------------------------

    Troca os valores de a e b (números) sem usar uma terceira variável
    e sem atribuição paralela. Só podes usar somas e subtrações.

    Pista: começa por "a = a + b". Faz a tabela para ver o que tens depois.

    Sem solução. Se encravares, envia email (ver secção 0).
"""

a = 7
b = 4

# TODO

print(a, b)         # deve dar: 4 7






# %%
"""
===========================================================================
2. TIPOS DE DADOS E OPERADORES [~50 min]
===========================================================================

Ex 2.1 [EXEMPLO] Os 4 tipos básicos ---------------------------------------

    Cada valor tem um TIPO. A função type() diz qual é.
"""

print(type(42))         # <class 'int'>    inteiro
print(type(3.14))       # <class 'float'>  real (número com parte decimal)
print(type("olá"))      # <class 'str'>    string (texto)
print(type(True))       # <class 'bool'>   booleano (verdadeiro ou falso)

print(type(3.0))        # float -> tem ponto, é float, mesmo sendo "redondo"
print(type("42"))       # str   -> tem aspas, é texto, mesmo parecendo número

# int   -> 42, -7, 0, 1000000. Sem limite de tamanho (experimenta 2 ** 200).
# float -> 3.14, -0.5, 2.0, 1e3 (= 1000.0). Sempre com ponto.
# str   -> "olá" ou 'olá'. Aspas duplas ou simples, tanto faz.
# bool  -> só há dois: True e False. Com maiúscula! (true dá NameError)






# %%
"""
Ex 2.2 [EXEMPLO] Operadores aritméticos -----------------------------------
"""

print(7 + 2)        # 9     soma
print(7 - 2)        # 5     subtração
print(7 * 2)        # 14    multiplicação
print(7 ** 2)       # 49    potência (7 ao quadrado)
print(7 / 2)        # 3.5   divisão "normal": dá SEMPRE float
print(6 / 2)        # 3.0   ...mesmo quando a conta é exata!
print(7 // 2)       # 3     divisão inteira: quantas vezes o 2 cabe no 7
print(7 % 2)        # 1     resto da divisão inteira

# O // e o % são as contas da escola primária:
#   17 a dividir por 5 dá 3 e sobram 2.
#   17 // 5 = 3      (quociente)
#   17 % 5  = 2      (resto)
#   e confirma-se: 5 * 3 + 2 = 17
print(17 // 5, 17 % 5)      # 3 2






# %%
"""
Ex 2.3 [EXEMPLO] Para que servem o // e o % ---------------------------------

    Aparecem em muitos exercícios. Estes 4 truques valem ouro.
"""

n = 472
print(n % 10)           # 2    -> último algarismo
print(n // 10)          # 47   -> tira o último algarismo
print(n % 2)            # 0    -> resto 0 a dividir por 2: n é par

minutes = 135
print(minutes // 60)    # 2    -> horas completas
print(minutes % 60)     # 15   -> minutos que sobram (135 min = 2 h 15 min)

# Resumo:
#   n % 10      -> último algarismo
#   n // 10     -> n sem o último algarismo
#   n % 2 == 0  -> n é par (n % 2 == 1 -> ímpar)
#   n % k == 0  -> n é múltiplo de k
#   // e % por 60, 24, 7... -> converter unidades (min -> h, dias -> semanas)






# %%
"""
Ex 2.4 [EXEMPLO] Prioridade das operações ---------------------------------

    É a mesma da matemática: primeiro potências, depois * / // %,
    no fim + e -. Parêntesis primeiro que tudo.
"""

print(2 + 3 * 4)        # 14   (e não 20)
print((2 + 3) * 4)      # 20
print(10 - 4 - 3)       # 3    mesma prioridade: da esquerda para a direita
print(2 ** 3 * 2)       # 16   potência primeiro: 8 * 2
print(-2 ** 2)          # -4 (!!) a potência vem antes do sinal: -(2 ** 2)
print((-2) ** 2)        # 4

# Na dúvida, usa parêntesis. Nunca fazem mal e tornam o código mais claro.






# %%
"""
Ex 2.5 [EXEMPLO] Misturar int e float -------------------------------------
"""

print(2 + 0.5)          # 2.5   int com float dá float
print(10 * 1.0)         # 10.0
print(2 ** 100)         # um número enorme: os int não têm limite

print(0.1 + 0.2)        # 0.30000000000000004  (!!)

# Os float são guardados em binário e quase sempre com uma pequena
# aproximação nos últimos algarismos. Por isso 0.1 + 0.2 não dá 0.3 exato.
# Não é um bug do Python: todas as linguagens fazem igual.
# Consequência prática: nunca comparar floats com "==" (vês isto no ficheiro 3).






# %%
"""
Ex 2.6 [EXEMPLO] Operações com strings ------------------------------------
"""

first = "Ana"
last = "Silva"
print(first + " " + last)   # Ana Silva   -> "+" junta textos (concatenação)
print("ab" * 3)             # ababab      -> "*" repete
print(len("Silva"))         # 5           -> len dá o número de caracteres

print("3" + "4")            # 34   -> são textos: junta, não soma!
print(3 + 4)                # 7

# print("3" + 4)            # <- descomenta: TypeError

# TypeError = "esta operação não funciona com estes tipos".
# O Python recusa misturar texto com número: não adivinha se querias 7 ou "34".
# Diz-se que o Python tem TIPAGEM FORTE: não converte tipos às escondidas.
# (em JavaScript, "3" + 4 dá "34" sem avisar... e isso causa muitos bugs)






# %%
"""
Ex 2.7 [EXEMPLO] Comparações: o resultado é um bool -----------------------
"""

print(5 > 3)            # True
print(5 < 3)            # False
print(5 == 5)           # True    "é igual?" -> DOIS sinais de igual
print(5 != 5)           # False   "é diferente?"
print(5 >= 5)           # True    maior OU igual
print(3 == 3.0)         # True    int e float podem comparar-se
print(3 == "3")         # False   número e texto nunca são iguais

x = 4
print(1 < x < 10)       # True    comparações encadeadas, como na matemática

is_adult = x >= 18      # guardar o resultado de uma comparação numa variável
print(is_adult)         # False

# "=" guarda um valor.   "==" pergunta se dois valores são iguais.
# Trocar um pelo outro é dos erros mais comuns.
#
# "v = (a == 5)" é Python válido (Teste 1 2024/25, 1b):
# compara a com 5 e guarda True ou False em v.






# %%
"""
Ex 2.8 [EXEMPLO] Comparar strings: ordem alfabética, letra a letra ---------
"""

print("ana" == "ana")       # True
print("Ana" == "ana")       # False  maiúsculas contam
print("banana" < "uva")     # True   "b" vem antes de "u"
print("casa" < "caso")      # True   iguais até "cas", depois "a" < "o"
print("Zebra" < "abelha")   # True (!!) todas as MAIÚSCULAS vêm antes das minúsculas
print("10" < "9")           # True (!!) compara o carácter "1" com o "9"
print(10 < 9)               # False  como números, claro

# As strings comparam-se carácter a carácter, da esquerda para a direita.
# O primeiro carácter diferente decide.
# Se uma acabar primeiro (e até aí forem iguais), a mais curta é menor:
print("sol" < "solar")      # True
#
# Cuidado com números guardados como texto: "10" < "9" dá True.
# É mais um motivo para converter o input em número (secção 4).






# %%
"""
Ex 2.9 [FAZ] Prever o valor e o tipo --------------------------------------

    Para cada expressão, escreve em comentário o VALOR e o TIPO.
    Depois corre a célula da solução para confirmar.

      a) 10 / 5           e) 3 * "ab"
      b) 10 // 4          f) 5 > 3
      c) 10 % 4           g) "5" > "30"
      d) 2 ** 10          h) 1 + 2.0
"""

# TODO: as tuas respostas
# a)
# b)
# c)
# d)
# e)
# f)
# g)
# h)















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 2.9

print(10 / 5, type(10 / 5))           # a) 2.0 float   (o "/" dá sempre float)
print(10 // 4, type(10 // 4))         # b) 2 int
print(10 % 4, type(10 % 4))           # c) 2 int       (10 = 4 * 2 + 2)
print(2 ** 10, type(2 ** 10))         # d) 1024 int
print(3 * "ab", type(3 * "ab"))       # e) ababab str
print(5 > 3, type(5 > 3))             # f) True bool
print("5" > "30", type("5" > "30"))   # g) True bool   ("5" > "3", o 1º carácter decide)
print(1 + 2.0, type(1 + 2.0))         # h) 3.0 float






# %%
"""
Ex 2.10 [FAZ] Segundos em horas, minutos e segundos -----------------------

    Converte 7384 segundos em horas, minutos e segundos.
    Deve aparecer: 2 3 4   (2 h, 3 min, 4 s)

    Pista: 1 hora tem 3600 segundos. Usa // e %.
"""

total_seconds = 7384

# TODO: calcula hours, minutes e seconds e escreve-os















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 2.10

total_seconds = 7384

hours = total_seconds // 3600               # 2      (7200 segundos)
minutes = total_seconds % 3600 // 60        # 184 // 60 = 3
seconds = total_seconds % 60                # 4
print(hours, minutes, seconds)              # 2 3 4

# total_seconds % 3600 -> segundos que sobram depois de tirar as horas (184)
# 184 // 60            -> minutos completos nesses 184 segundos (3)
# total_seconds % 60   -> segundos que sobram de tudo (4)
#
# Outra forma para os minutos: (total_seconds // 60) % 60
# -> total de minutos (123), e depois tirar as horas completas (123 % 60 = 3).






# %%
"""
Ex 2.11 [FAZ] Soma dos algarismos -------------------------------------

    n é um número com 3 algarismos.
    Calcula a soma dos algarismos. Para n = 472: 4 + 7 + 2 = 13.

    Pista: o Ex 2.3 mostra como apanhar o último algarismo
    e como tirá-lo do número.
"""

n = 472

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 2.11

n = 472

units = n % 10              # 2
tens = n // 10 % 10         # 47 % 10 = 7
hundreds = n // 100         # 4
print(units + tens + hundreds)      # 13

# n // 10 % 10: primeiro tira o último algarismo (47), depois fica com o último (7).
# Experimenta com outros números de 3 algarismos: 100, 999, 305.






# %%
"""
Ex 2.12 [PENSA] Dias em anos, semanas e dias --------------------------

    Converte 1000 dias em anos (de 365 dias), semanas e dias.
    Deve dar: 2 anos, 38 semanas, 4 dias.
"""

days = 1000

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 2.12 (sem código):
#
#   anos    = quantos 365 cabem em days               (divisão inteira)
#   resto   = dias que sobram depois de tirar os anos (resto da divisão)
#   semanas = quantos 7 cabem no resto
#   dias    = o que sobra do resto depois das semanas
#
# É exatamente o mesmo esquema do Ex 2.10, com 365 e 7 em vez de 3600 e 60.






# %%
"""
Ex 2.13 [DESAFIO] Comparar strings (inspirado no Teste 2 2025/26, 1) ------

    Diz se cada comparação dá True ou False. Sem correr!
    Solução no fim do ficheiro.

      a) "aa" < "bbb"           e) "Zebra" < "abelha"
      b) "123" < "123"          f) "100" < "99"
      c) "uva" < "banana"       g) 100 < 99
      d) "zzzz" > "zzz"
"""






# %%
"""
Ex 2.14 [DESAFIO] Valor e tipo, versão difícil ----------------------------

    Para cada expressão, diz o VALOR e o TIPO. Sem correr!
    Solução no fim do ficheiro.

      a) 7 // 2 * 2 + 7 % 2       e) "ab" * 2 + "c"
      b) 2 ** -1                  f) 1 + 1 == 2
      c) -2 ** 2                  g) 9 ** 0.5
      d) 10 / 2 * 5               h) 2 ** 3 ** 2
"""






# %%
"""
Ex 2.15 [SOZINHA] Número ao contrário ---------------------------------

    n é um número de 4 algarismos (ex: 1234).
    Calcula o número ao contrário (4321) e guarda-o numa variável.
    Só podes usar operações com números (nada de strings).

    Pista: primeiro extrai os 4 algarismos (como no Ex 2.11).
    Depois pensa: 4321 = 4 * 1000 + 3 * 100 + ...

    Sem solução. Se encravares, envia email (ver secção 0).
"""

n = 1234

# TODO






# %%
"""
===========================================================================
3. FALAR COM O UTILIZADOR: PRINT E INPUT [~30 min]
===========================================================================

Ex 3.1 [EXEMPLO] Mais sobre o print ---------------------------------------
"""

print("Idade:", 19, "anos")         # várias coisas -> separadas por um espaço
print()                             # print vazio -> linha em branco
print(1, 2, 3, sep="-")             # sep muda o separador: 1-2-3
print(1, 2, 3, sep="")              # sem separador: 123

print("sem mudar", end="")          # end="" -> não muda de linha no fim
print(" de linha")                  # continua na mesma linha

# Por defeito, o print:
#   - separa as coisas com um espaço (sep=" ");
#   - muda de linha no fim (end="\n", o "\n" é o carácter "mudança de linha").
#
# O end="" aparece muito nos testes, para desenhar figuras com caracteres
# (um carácter de cada vez, na mesma linha).






# %%
"""
Ex 3.2 [EXEMPLO] f-strings: texto com valores lá dentro ------------------
"""

name = "Ana"
age = 19
print(f"A {name} tem {age} anos.")              # A Ana tem 19 anos.
print(f"Para o ano terá {age + 1} anos.")       # pode ter contas lá dentro

price = 2 / 3
print(f"Preço: {price}")                        # Preço: 0.6666666666666666
print(f"Preço: {price:.2f}")                    # Preço: 0.67  (2 casas decimais)
print(f"Preço: {price:.6f}")                    # Preço: 0.666667

print("A {name} tem {age} anos.")               # sem o f: escreve as chavetas!

# Uma f-string é uma string com um "f" antes das aspas.
# O que está entre { } é calculado e o resultado entra no texto.
# {valor:.2f} -> escreve o valor com 2 casas decimais (arredonda).
# Esquecer o "f" não dá erro: escreve as chavetas tal e qual.






# %%
"""
Ex 3.3 [EXEMPLO] input: ler o que o utilizador escreve --------------------

    Corre a célula. A consola fica à espera:
    escreve o teu nome e carrega em Enter.
"""

name = input("Como te chamas? ")
print(f"Olá, {name}!")

# input("texto") escreve o texto (chama-se "prompt") e espera.
# O que o utilizador escrever até ao Enter é devolvido e guardado em name.
# O espaço no fim do prompt é só para o cursor não ficar colado ao "?".






# %%
"""
Ex 3.4 [EXEMPLO] A surpresa do input --------------------------------------

    Corre e escreve 5 e depois 3.
"""

a = input("Primeiro número: ")
b = input("Segundo número: ")
print(a + b)            # 53 (!!)
print(type(a))          # <class 'str'>

# O input devolve SEMPRE uma string. Mesmo que escrevas números.
# "5" + "3" junta os textos: "53".
# A solução está na secção 4: converter o texto em número.






# %%
"""
Ex 3.5 [FAZ] Nome e cidade --------------------------------------------

    Pede o nome e a cidade ao utilizador e escreve uma frase assim:
        A Ana vive em Lisboa.
    Usa uma f-string.
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 3.5

name = input("Nome: ")
city = input("Cidade: ")
print(f"A {name} vive em {city}.")

# Sem f-string também dá, mas é mais fácil enganar nos espaços:
# print("A " + name + " vive em " + city + ".")
# print("A", name, "vive em", city + ".")






# %%
"""
Ex 3.6 [FAZ] Linha de uma fatura --------------------------------------

    Com as variáveis abaixo, escreve exatamente esta linha:
        4 x 3.50 = 14.00
    (preços sempre com 2 casas decimais)
"""

quantity = 4
price = 3.5

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 3.6

quantity = 4
price = 3.5
print(f"{quantity} x {price:.2f} = {quantity * price:.2f}")     # 4 x 3.50 = 14.00

# O :.2f também funciona com contas dentro das chavetas.
# A quantidade é int: fica sem casas decimais.






# %%
"""
Ex 3.7 [PENSA] Um print, quatro números -------------------------------

    Com UM só print, e passando-lhe os números 1, 2, 3 e 4
    como 4 valores separados, escreve:
        1 -> 2 -> 3 -> 4
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 3.7 (sem código):
#
#   O print separa os valores com um espaço por defeito.
#   Há uma forma de lhe dizer qual é o separador a usar (Ex 3.1).
#   Aqui o separador não é só a seta: repara nos espaços à volta dela.






# %%
"""
Ex 3.8 [DESAFIO] O que escreve? -------------------------------------------

    Escreve no papel EXATAMENTE o que aparece (linhas e espaços).
    Solução no fim do ficheiro.
"""

print("a", "b", sep="")
print("c", end="")
print("d", end="-")
print("e")
print(1, 2, 3, sep=", ")
x = 2.5
print(f"{x} {x:.3f} x")






# %%
"""
Ex 3.9 [SOZINHA] Nome sublinhado --------------------------------------

    Pede o nome ao utilizador e escreve-o sublinhado com "-",
    com o traço do MESMO tamanho do nome. Exemplo:
        Carolina
        --------

    Pista: o Ex 2.6 tem duas operações de strings que resolvem isto.

    Sem solução. Se encravares, envia email (ver secção 0).
"""

# TODO






# %%
"""
===========================================================================
4. CONVERTER ENTRE TIPOS [~30 min]
===========================================================================

Ex 4.1 [EXEMPLO] int(), float(), str() ------------------------------------
"""

print(int("42"))            # 42      texto -> inteiro
print(float("2.5"))         # 2.5     texto -> real
print(str(42))              # 42      número -> texto (agora é "42")
print(float(3))             # 3.0

print(int(3.99))            # 3  (!!) o int CORTA a parte decimal, não arredonda
print(int(-3.99))           # -3      corta sempre na direção do zero
print(round(3.99))          # 4       para arredondar, usa round

# print(int("3.5"))         # <- descomenta: ValueError
# print(int("abc"))         # <- descomenta: ValueError
print(int(float("3.5")))    # 3       primeiro texto -> float, depois float -> int

# ValueError = "o tipo está certo, mas este valor não dá".
# int("3.5") falha porque "3.5" não é um inteiro escrito como texto.
#
# Estas conversões também se chamam "cast".






# %%
"""
Ex 4.2 [EXEMPLO] O padrão para ler números --------------------------------

    Corre e escreve 5 e 3 (como no Ex 3.4).
"""

a = int(input("Primeiro número: "))
b = int(input("Segundo número: "))
print(a + b)            # 8, finalmente

# Lê-se de dentro para fora:
#   1. input(...)  -> lê o texto "5"
#   2. int("5")    -> converte em 5
#   3. a = 5       -> guarda
#
# Para reais: float(input("..."))
# Vais escrever esta linha centenas de vezes nesta cadeira.
#
# Se o utilizador escrever "abc", dá ValueError.
# Nesta cadeira assume-se que o utilizador escreve valores do tipo certo.






# %%
"""
Ex 4.3 [EXEMPLO] Juntar números a texto -----------------------------------
"""

age = 19
# print("Tenho " + age + " anos")             # TypeError: str + int
print("Tenho " + str(age) + " anos")          # funciona: converte primeiro
print(f"Tenho {age} anos")                    # mais simples: f-string

# Com f-strings não é preciso converter: o Python faz isso dentro das { }.
# É a forma recomendada.






# %%
"""
Ex 4.4 [FAZ] Daqui a 10 anos ------------------------------------------

    Pede a idade ao utilizador e escreve a idade que terá daqui a 10 anos.
        Idade: 19
        Daqui a 10 anos terás 29 anos.
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 4.4

age = int(input("Idade: "))
print(f"Daqui a 10 anos terás {age + 10} anos.")

# Sem o int(...) dava TypeError: "19" + 10 (str + int).






# %%
"""
Ex 4.5 [FAZ] Total de uma compra --------------------------------------

    Pede o preço unitário (real) e a quantidade (inteiro).
    Escreve o total com 2 casas decimais.
        Preço: 2.35
        Quantidade: 3
        Total: 7.05
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 4.5

price = float(input("Preço: "))
quantity = int(input("Quantidade: "))
print(f"Total: {price * quantity:.2f}")

# Preço -> float (tem casas decimais). Quantidade -> int (é uma contagem).
# 2.35 * 3 dá 7.050000000000001 (os floats...). O :.2f arruma isso.






# %%
"""
Ex 4.6 [FAZ] Quilómetros em milhas ------------------------------------

    Pede uma distância em km (real) e escreve-a em milhas, com 3 casas decimais.
    1 milha = 1.609344 km.
        km: 10
        10.0 km = 6.214 milhas
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 4.6

KM_PER_MILE = 1.609344

km = float(input("km: "))
miles = km / KM_PER_MILE
print(f"{km} km = {miles:.3f} milhas")

# KM_PER_MILE é uma CONSTANTE: um valor fixo com nome.
# Convenção: constantes em MAIÚSCULAS.
# Assim quem lê percebe o que é o 1.609344 (sem nome seria um "número mágico").
#
# km foi lido com float: o print escreve 10.0 e não 10.






# %%
"""
Ex 4.7 [PENSA] Média de três notas ------------------------------------

    Pede três notas (reais) e escreve a média com 1 casa decimal.
        Nota 1: 12
        Nota 2: 15.5
        Nota 3: 14
        Média: 13.8
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 4.7 (sem código):
#
#   1. ler as 3 notas, cada uma convertida em float;
#   2. média = soma das 3 a dividir por 3;
#   3. escrever com uma f-string e o formato de 1 casa decimal.
#
# Armadilha: nota1 + nota2 + nota3 / 3 está ERRADO. Porquê?
# (lembra-te da prioridade das operações, Ex 2.4)






# %%
"""
Ex 4.8 [DESAFIO] Prever conversões ----------------------------------------

    Diz o VALOR e o TIPO de cada expressão (ou "erro" e qual).
    Sem correr! Solução no fim do ficheiro.

      a) int("7") + int(7.9)        e) int(" 12 ")
      b) str(1) + str(2)            f) int(-7.5)
      c) float("1e3")               g) int("7.0")
      d) str(2.0) + "1"             h) round(7.5) + round(6.5)
"""






# %%
"""
Ex 4.9 [SOZINHA] Troco em moedas --------------------------------------

    Pede um valor em euros (real, ex: 4.35) e escreve quantas moedas
    de cada tipo são precisas, usando sempre as maiores primeiro.
    Moedas: 2€, 1€, 50, 20, 10, 5, 2 e 1 cêntimos.
        Valor: 4.35
        2 euros: 2
        1 euro: 0
        50 cent: 0
        20 cent: 1
        10 cent: 1
        5 cent: 1
        2 cent: 0
        1 cent: 0

    Pistas:
      - Trabalha em CÊNTIMOS (inteiros): 4.35 euros = 435 cêntimos.
      - Cuidado: int(4.35 * 100) dá 434 (!!). Corre print(4.35 * 100)
        para perceber porquê. Usa round em vez de int.
      - Depois é sempre o mesmo par de contas: // para saber quantas moedas,
        % para saber quanto sobra.

    Sem solução. Se encravares, envia email (ver secção 0).
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  [ ] criar variáveis, atribuir (=), += e companhia, trocar valores
  [ ] executar código à mão com uma tabela de variáveis
  [ ] ler uma mensagem de erro: NameError, SyntaxError, TypeError, ValueError
  [ ] os tipos int, float, str, bool e o type()
  [ ] / vs // vs %, prioridade das operações, comparações (== vs =)
  [ ] comparar strings (ordem alfabética, carácter a carácter)
  [ ] print com sep e end, f-strings com :.2f
  [ ] input devolve sempre str -> int(input(...)) e float(input(...))
  [ ] int() corta, round() arredonda

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: revisoes_2_funcoes_e_escopo.py
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
# Solução do Ex 1.12 (sintaxe correta ou incorreta)
#
#   a) a = 6b + 3c       INCORRETO. "6b" não é "6 vezes b". Escreve-se 6 * b.
#                        (e um nome não pode começar por algarismo)
#   b) v = (a == 5)      CORRETO. Compara e guarda True/False em v.
#   c) x + 1 = y         INCORRETO. À esquerda do "=" só pode estar um nome.
#   d) total = total + 1 CORRETO na sintaxe.
#                        (se total não existir dá NameError, mas isso é ao correr)
#   e) 2nd_place = 4     INCORRETO. Nome a começar por algarismo.
#   f) a, b = b, a       CORRETO. Atribuição paralela.
#   g) my_var=3          CORRETO. Os espaços à volta do "=" são opcionais.






# %%
# Solução do Ex 1.13
#
#   instrução        |  a  |  b  |  c
#   a, b, c = 1,2,3  |  1  |  2  |  3
#   a = b + c        |  5  |  2  |  3
#   c = a * b        |  5  |  2  |  10
#   b, c = c, b      |  5  |  10 |  2     (lado direito primeiro: (10, 2))
#   a += c           |  7  |  10 |  2
#
# Escreve: 7 10 2






# %%
# Solução do Ex 2.13 (comparar strings)
#
#   a) "aa" < "bbb"          True   'a' < 'b' no 1º carácter
#   b) "123" < "123"         False  são iguais (e iguais não é "menor")
#   c) "uva" < "banana"      False  'u' vem depois de 'b'
#   d) "zzzz" > "zzz"        True   iguais até acabar a mais curta -> a mais longa é maior
#   e) "Zebra" < "abelha"    True   maiúsculas vêm antes das minúsculas
#   f) "100" < "99"          True   '1' < '9' no 1º carácter (são textos!)
#   g) 100 < 99              False  são números

print("aa" < "bbb", "123" < "123", "uva" < "banana", "zzzz" > "zzz",
      "Zebra" < "abelha", "100" < "99", 100 < 99)






# %%
# Solução do Ex 2.14 (valor e tipo, versão difícil)
#
#   a) 7 // 2 * 2 + 7 % 2   7     int    (3 * 2 + 1)
#   b) 2 ** -1              0.5   float  (expoente negativo: 1 / 2)
#   c) -2 ** 2              -4    int    (potência antes do sinal)
#   d) 10 / 2 * 5           25.0  float  (5.0 * 5, da esquerda para a direita)
#   e) "ab" * 2 + "c"       ababc str
#   f) 1 + 1 == 2           True  bool   (a soma é feita antes da comparação)
#   g) 9 ** 0.5             3.0   float  (expoente 0.5 é a raiz quadrada)
#   h) 2 ** 3 ** 2          512   int    (!!) a potência faz-se da DIREITA
#                                        para a esquerda: 2 ** 9

for value in [7 // 2 * 2 + 7 % 2, 2 ** -1, -2 ** 2, 10 / 2 * 5,
              "ab" * 2 + "c", 1 + 1 == 2, 9 ** 0.5, 2 ** 3 ** 2]:
    print(value, type(value))






# %%
# Solução do Ex 3.8
#
# Output:
#   ab
#   cd-e
#   1, 2, 3
#   2.5 2.500 x
#
# Linha 1: sep="" -> sem espaço entre "a" e "b".
# Linha 2: "c" sem mudar de linha, "d" seguido de "-" sem mudar de linha,
#          e só o print("e") muda de linha no fim.
# Linha 3: separador ", ".
# Linha 4: {x} escreve 2.5, {x:.3f} escreve 2.500, e o " x" é texto normal.






# %%
# Solução do Ex 4.8 (prever conversões)
#
#   a) int("7") + int(7.9)     14       int    (7 + 7: o int corta o .9)
#   b) str(1) + str(2)         "12"     str    (junta textos)
#   c) float("1e3")            1000.0   float  (notação científica)
#   d) str(2.0) + "1"          "2.01"   str    (str(2.0) é "2.0")
#   e) int(" 12 ")             12       int    (os espaços à volta são ignorados)
#   f) int(-7.5)               -7       int    (corta na direção do zero)
#   g) int("7.0")              ValueError      ("7.0" não é um inteiro escrito como texto)
#   h) round(7.5) + round(6.5) 14       int    (8 + 6) (!!)
#
# h) round(6.5) dá 6 e não 7: nos empates exatos (.5), o round escolhe o número PAR.
#    Chama-se "arredondamento bancário". Faz com que, em muitas contas,
#    metade dos empates vá para cima e metade para baixo.
