# %%
"""
===========================================================================
REVISÕES 5 - VALORES, REFERÊNCIAS E ESCOPO
O que acontece MESMO às variáveis: em funções, ifs, ciclos e listas
IPCE 2026/2027
===========================================================================

Antes deste ficheiro: revisoes_4_ciclos_for_e_listas.py

Este ficheiro não traz matéria nova "grande". Junta peças que já viste
(ficheiro 1 Ex 1.3, ficheiro 2 secção 6, ficheiro 4 Ex 12.4 e 12.5)
num único modelo, e treina-o com muitos exercícios "o que escreve?".
Nos testes, a pergunta 1 é quase sempre deste tipo.

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 15. Variáveis são nomes que apontam para valores ............. [~15 min]
       Porquê: é o modelo que explica TUDO o resto deste ficheiro.

 16. Funções com números e strings ............................ [~20 min]
       Porquê: perceber porque é que uma função não consegue mudar
       um número de quem a chamou, e como os valores entram e saem.

 17. if e for não criam variáveis "privadas" ................... [~15 min]
       Porquê: só as funções têm variáveis locais. Uma variável criada
       dentro de um if ou de um for continua a existir depois.

 18. Listas: duas variáveis, a mesma lista .................... [~25 min]
       Porquê: com listas, "mudar" e "reatribuir" são coisas diferentes,
       e confundi-las dá os bugs mais difíceis de encontrar.

 19. Funções com listas ....................................... [~25 min]
       Porquê: uma função pode mudar a lista de quem a chama.
       Às vezes é isso que se quer. Às vezes é um bug.

 20. Tudo junto, ao nível dos testes .......................... [~20 min]

  Resumo + soluções dos desafios

  Total: ~2h.

FERRAMENTA RECOMENDADA: Python Tutor (https://pythontutor.com)
  Cola lá o código de qualquer exercício deste ficheiro e carrega em
  "Visualize Execution". Depois avança com "Next >" linha a linha.
  Do lado direito aparece um desenho com as variáveis e SETAS para os valores.
  É exatamente o modelo deste ficheiro, desenhado automaticamente.
  Usa-o para confirmar as tuas respostas, DEPOIS de tentares à mão.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Níveis:
      [EXEMPLO]  resolvido: lê, corre, muda valores.
      [FAZ]      contigo. Solução na célula a seguir, depois de muito espaço.
      [PENSA]    contigo. A "solução" é só a ideia, sem código.
      [DESAFIO]  nível de teste. Solução no fim do ficheiro.
      [SOZINHA]  sem solução.
  - Nos exercícios "o que escreve?": responde PRIMEIRO em comentário,
    e só depois corres a célula para confirmar.
  - Se encravares: email ao professor das práticas com o número do exercício,
    o que já fizeste, onde encravaste e o ficheiro em anexo.

  A numeração continua a dos ficheiros anteriores: este começa na secção 15.
"""






# %%
"""
===========================================================================
15. VARIÁVEIS SÃO NOMES QUE APONTAM PARA VALORES [~15 min]
===========================================================================

Ex 15.1 [EXEMPLO] O modelo: nome -> seta -> valor -------------------------
"""

a = 5
b = a
print(a is b)       # True   -> a e b apontam para o MESMO objeto 5
a = a + 1
print(a, b)         # 6 5
print(a is b)       # False  -> agora apontam para objetos diferentes

# Em Python, uma variável NÃO é uma caixa com um valor lá dentro.
# É um NOME com uma seta para um valor (um objeto) que está na memória.
# (a teórica 03a chama a esta seta uma REFERÊNCIA)
#
#   a = 5       ->  a ---> 5
#   b = a       ->  a ---> 5 <--- b      (não se copia nada: b aponta para o mesmo 5)
#   a = a + 1   ->  calcula 5 + 1, cria o objeto 6, e põe a seta do a nele
#                   a ---> 6
#                   b ---> 5             (a seta do b não mexeu)
#
# A atribuição "nome = valor" só faz uma coisa: MUDA A SETA do nome.
# Nunca muda o objeto para onde a seta apontava antes.
#
# Dois operadores para comparar:
#   a == b  -> os dois valores são IGUAIS?
#   a is b  -> as duas setas apontam para o MESMO objeto?
# No dia a dia usa-se ==. O is serve aqui para ver as setas
# (e no futuro para comparar com None: x is None).






# %%
"""
Ex 15.2 [EXEMPLO] O tipo pertence ao valor, não ao nome --------------------
"""

x = 5
print(type(x))      # <class 'int'>
x = "olá"
print(type(x))      # <class 'str'>
x = [1, 2]
print(type(x))      # <class 'list'>

# O x não "é um int". O x é um nome. Primeiro apontou para um int,
# depois para uma str, depois para uma lista.
# Quem tem tipo são os valores (os objetos), não os nomes.
# (pode-se fazer, mas é mau estilo: um nome deve apontar sempre para o mesmo tipo)
#
# Cultura geral: isto chama-se TIPAGEM DINÂMICA (ficheiro 2, Ex 5.6).
# São duas perguntas diferentes:
#
#   1. Os tipos são verificados ANTES ou DURANTE a execução?
#        estática: antes (o tipo é do NOME, declarado: int x = 5;)
#        dinâmica: durante (o tipo é do VALOR, o nome pode mudar de alvo)
#   2. A linguagem converte tipos às escondidas? (ficheiro 1, Ex 2.6)
#        forte: não ("3" + 4 dá erro)
#        fraca: sim ("3" + 4 dá "34")
#
#                  |  forte          |  fraca
#      ------------+-----------------+-------------------
#      dinâmica    |  Python         |  JavaScript, PHP
#      estática    |  Java, Rust     |  C






# %%
"""
Ex 15.3 [FAZ] O que escreve? ------------------------------------------

    Desenha as setas no papel. Responde em comentário, só depois corre.
"""

s = "ola"
t = s
s = s + "!"
print(s, t)
print(s is t)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 15.3
#
# Escreve:
#   ola! ola
#   False
#
#   s = "ola"     s ---> "ola"
#   t = s         s ---> "ola" <--- t
#   s = s + "!"   o + cria uma string NOVA "ola!" e a seta do s passa para ela
#                 s ---> "ola!"      t ---> "ola"
#
# As strings não mudam (são IMUTÁVEIS, tal como os números).
# Qualquer "alteração" de uma string cria uma string nova.






# %%
"""
Ex 15.4 [PENSA] Porque é que o y não mudou? ----------------------------

    x = 10
    y = x
    x += 5
    print(y)        # 10

    Explica por palavras, com o modelo das setas, porque é que o y continua 10.
    Em particular: o x += 5 MUDOU o objeto 10?
"""

# TODO: a tua explicação
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 15.4:
#
#   Depois de y = x, as duas setas apontam para o mesmo objeto 10.
#   x += 5 é o mesmo que x = x + 5 (para números):
#   calcula 10 + 5, cria o objeto 15, e MUDA A SETA do x para o 15.
#   O objeto 10 não foi alterado (os números nunca mudam).
#   A seta do y continua a apontar para o 10.
#
#   Guarda esta pergunta: no Ex 18.3 vais ver que com LISTAS
#   o += faz uma coisa diferente.






# %%
"""
Ex 15.5 [DESAFIO] O que escreve? -----------------------------------------

    Sem correr! Solução no fim do ficheiro.
"""

a = 1
b = a
c = b
b = 7
a = c + b
print(a, b, c)






# %%
"""
Ex 15.6 [SOZINHA] Confirmar com o Python Tutor -------------------------

    Cola o código do Ex 15.5 no Python Tutor (https://pythontutor.com)
    e avança linha a linha. Em cada passo, antes de carregar em "Next",
    diz em voz alta para onde vai mudar cada seta.
    Faz o mesmo com o Ex 15.3.

    Sem solução: o próprio Python Tutor mostra se acertaste.
"""






# %%
"""
===========================================================================
16. FUNÇÕES COM NÚMEROS E STRINGS [~20 min]
===========================================================================

Ex 16.1 [EXEMPLO] O parâmetro recebe uma seta para o mesmo valor ------------
"""

def add_ten(n: int) -> int:
    """ n plus ten. """
    n = n + 10          # muda a seta do n LOCAL; o x de fora não é tocado
    return n

x = 5
y = add_ten(x)
print(x, y)             # 5 15

# Na chamada add_ten(x), o parâmetro n passa a apontar para o MESMO objeto 5.
# (o Python não copia o valor: passa a seta)
#
# Dentro da função, "n = n + 10" cria o 15 e muda a seta do n.
# O n é um nome LOCAL da função: mudar a seta dele não mexe na seta do x.
# E o 5 em si não pode ser alterado (números são imutáveis).
#
# Resultado: com números e strings, é como se a função recebesse uma cópia.
# Nada do que a função faça ao parâmetro se vê cá fora.






# %%
"""
Ex 16.2 [EXEMPLO] A única forma de um resultado sair: return ----------------
"""

def add_ten(n: int) -> int:
    """ n plus ten. (the same function of Ex 16.1) """
    n = n + 10
    return n

x = 5
add_ten(x)              # calcula 15... e deita-o fora
print(x)                # 5

x = add_ten(x)          # guarda o resultado, mudando a seta do x
print(x)                # 15

# Erro comum: chamar a função e esperar que ela "mude o x".
# Com números e strings isso é impossível.
# O valor sai pelo return, e quem chama decide onde o guarda.






# %%
"""
Ex 16.3 [EXEMPLO] Revisão: nomes locais, nomes de fora ------------------------

    (é a secção 6 do ficheiro 2, em resumo)
"""

LIMIT = 100                 # constante de fora

def check(v: int) -> bool:
    """ Check if v is within the limit. """
    return v <= LIMIT       # LER um nome de fora: funciona

def change_limit() -> None:
    """ Try to change LIMIT (creates a LOCAL name instead). """
    LIMIT = 5               # atribuir: cria um LIMIT LOCAL, novo
    print("dentro:", LIMIT)

change_limit()              # dentro: 5
print("fora:", LIMIT)       # fora: 100

# As regras:
#   - nomes criados numa função (incluindo os parâmetros) são LOCAIS;
#   - ler um nome que não é local -> o Python procura-o cá fora;
#   - atribuir a um nome dentro da função -> esse nome é local EM TODA a função
#     (se for lido antes da atribuição: UnboundLocalError).






# %%
"""
Ex 16.4 [FAZ] O que escreve? ------------------------------------------

    Atenção à ORDEM dos argumentos na chamada!
"""

def f(a: int, b: int) -> int:
    a = a + b
    b = a - b
    return a * b

a, b = 2, 3
c = f(b, a)
print(a, b, c)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 16.4
#
# Escreve: 2 3 15
#
# A chamada é f(b, a) = f(3, 2): dentro da função, a = 3 e b = 2
# (os nomes de fora não interessam: conta a POSIÇÃO do argumento).
#   a = 3 + 2 = 5
#   b = 5 - 2 = 3
#   return 5 * 3 = 15
# O a e o b de fora nunca mudaram: continuam 2 e 3.
#
# Ter os mesmos nomes dentro e fora é uma armadilha de propósito.
# São variáveis diferentes.






# %%
"""
Ex 16.5 [FAZ] O que escreve? ------------------------------------------
"""

def repeat(s: str) -> str:
    s = s * 2
    return s + "!"

word = "ola"
r = repeat(word)
print(word, r)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 16.5
#
# Escreve: ola olaola!
#
# O s local passa a apontar para "olaola" (string nova).
# O word continua a apontar para "ola": as strings nunca mudam.






# %%
"""
Ex 16.6 [PENSA] Uma função que troque dois números? ---------------------

    Consegues escrever uma função swap(x, y) que, chamada como swap(a, b),
    troque os valores das variáveis a e b de quem a chamou?

        a, b = 1, 2
        swap(a, b)
        print(a, b)     # queríamos: 2 1

    Porquê? E como se troca então?
"""

# TODO: a tua explicação
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 16.6:
#
#   Não é possível.
#   O x e o y da função são nomes LOCAIS que apontam para o 1 e o 2.
#   Trocar as setas do x e do y não mexe nas setas do a e do b.
#   E não se pode mudar os objetos 1 e 2 (são imutáveis).
#
#   Troca-se diretamente onde as variáveis estão: a, b = b, a
#
#   Com uma LISTA já é possível trocar dois ELEMENTOS dentro de uma função
#   (a teórica 04b tem um swap(l, i, j)): a função recebe a seta para a lista
#   e muda o conteúdo da lista. Vês isso na secção 19.






# %%
"""
Ex 16.7 [DESAFIO] O que escreve? -----------------------------------------

    Três n diferentes! Sem correr. Solução no fim do ficheiro.
"""

n = 10

def h(m: int) -> int:
    n = m * 2
    return n + 1

def p(n: int) -> int:
    return h(n) + n

print(p(3), n)






# %%
"""
Ex 16.8 [SOZINHA] Qual dá erro? ---------------------------------------

    Para cada função, diz o que devolve, ou que erro dá (e porquê).

        t = 1

        def a1() -> int:
            return t + 1

        def a2() -> int:
            t = 5
            return t + 1

        def a3() -> int:
            t = t + 1
            return t

    Sem solução. Se encravares, envia email.
"""






# %%
"""
===========================================================================
17. IF E FOR NÃO CRIAM VARIÁVEIS "PRIVADAS" [~15 min]
===========================================================================

Ex 17.1 [EXEMPLO] Uma variável criada num if existe depois dele -------------
"""

x = 7
if x > 5:
    message = "grande"
print(message)              # grande

# Em Python, só as FUNÇÕES criam um espaço de nomes próprio.
# O if, o elif, o else e o for NÃO: o que lá se cria fica disponível
# no resto da função (ou do ficheiro).
#
# Cultura geral: em Java ou C é diferente. Uma variável declarada
# dentro das chavetas { } de um if só existe dentro delas.
#
# O perigo (ficheiro 3, Ex 8.10): se o ramo do if NÃO correr,
# a variável não é criada, e usá-la depois dá erro.






# %%
"""
Ex 17.2 [EXEMPLO] A variável do for fica com o último valor -----------------
"""

for i in range(5):
    pass
print(i)                    # 4   -> o último valor do range

total = 0
for k in range(3):
    total += k
print(k, total)             # 2 3

for never in range(0):      # range vazio: o ciclo não corre nenhuma vez
    pass
# print(never)              # <- descomenta: NameError (o never nunca recebeu valor)

# Depois de o ciclo acabar, a variável do for continua a existir,
# com o valor da ÚLTIMA volta.
# Se o ciclo não deu nenhuma volta, a variável nunca chegou a ser criada.






# %%
"""
Ex 17.3 [EXEMPLO] Mudar a variável do for dentro do ciclo não muda as voltas -
"""

for i in range(3):
    print("antes:", i)
    i = 100
    print("depois:", i)

# Output:
#   antes: 0
#   depois: 100
#   antes: 1
#   depois: 100
#   antes: 2
#   depois: 100
#
# Em cada volta, o for põe a seta do i no valor SEGUINTE do range,
# ignorando o que o corpo tenha feito ao i.
# Mudar o i dentro do corpo não salta nem repete voltas.
# (não se faz de propósito: só confunde quem lê)






# %%
"""
Ex 17.4 [FAZ] O que escreve? ------------------------------------------
"""

result = 0
for i in range(1, 4):
    if i % 2 == 0:
        last_even = i
    result = i * 10
print(i, last_even, result)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 17.4
#
# Escreve: 3 2 30
#
#   i = 1: ímpar, result = 10
#   i = 2: par, last_even = 2, result = 20
#   i = 3: ímpar, result = 30
#   Depois do ciclo: i fica 3 (o último), last_even fica 2, result fica 30.
#
# O last_even foi criado dentro de um if dentro de um for, e existe na mesma cá fora.






# %%
"""
Ex 17.5 [PENSA] Último negativo -----------------------------------------

    Esta função devia devolver o último número negativo da lista:
"""

def last_negative(l: list[int]) -> int:
    """ The last negative element of l (WITH A BUG). """
    for v in l:
        if v < 0:
            found = v
    return found

print(last_negative([3, -1, 4, -5, 2]))     # -5
# print(last_negative([3, 4]))              # <- descomenta e corre

"""
    O que acontece com [3, 4]? Porquê? Como se corrige?
"""

# TODO: a tua explicação
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 17.5:
#
#   Com [3, 4], o if nunca é verdadeiro: o found nunca é criado.
#   O return tenta ler uma variável local sem valor -> UnboundLocalError.
#
#   Correção: dar um valor inicial ao found ANTES do ciclo, que signifique
#   "não encontrei". Um número não serve bem (0 podia confundir-se com
#   um resultado). O habitual é None, e a docstring diz:
#   "devolve None se não houver negativos".
#   (o cabeçalho passaria a dizer -> int | None, mas isso fica para mais tarde)






# %%
"""
Ex 17.6 [DESAFIO] O que escreve? -----------------------------------------

    Sem correr! Solução no fim do ficheiro.
"""

x = 0
for i in range(3):
    for j in range(i):
        x = i + j
print(i, j, x)






# %%
"""
Ex 17.7 [SOZINHA] O for reutiliza o nome --------------------------------

    O que escreve, e porquê?

        n = 5
        for n in range(3):
            pass
        print(n)

    E se fosse assim?

        n = 5
        for n in range(0):
            pass
        print(n)

    Sem solução. Se encravares, envia email.
"""






# %%
"""
===========================================================================
18. LISTAS: DUAS VARIÁVEIS, A MESMA LISTA [~25 min]
===========================================================================

Ex 18.1 [EXEMPLO] b = a não copia a lista (revisão do ficheiro 4, Ex 12.4) ---
"""

a = [1, 2, 3]
b = a                       # b aponta para a MESMA lista
c = a.copy()                # c aponta para uma lista NOVA, com os mesmos valores

print(a == b, a is b)       # True True
print(a == c, a is c)       # True False

b.append(4)
print(a)                    # [1, 2, 3, 4]  -> mudou "através" do b
print(c)                    # [1, 2, 3]

# Com números, o modelo das setas não tinha surpresas: os números não mudam.
# As listas MUDAM (são mutáveis): append, pop, l[i] = ... alteram o objeto.
# Se duas setas apontam para a mesma lista, as duas "veem" a alteração.
#
#   a ---> [1, 2, 3, 4] <--- b
#   c ---> [1, 2, 3]






# %%
"""
Ex 18.2 [EXEMPLO] Mudar a lista vs mudar a seta ------------------------------
"""

a = [1, 2]
b = a
b = [9, 9]                  # muda a SETA do b (lista nova). A lista do a não é tocada
print(a, b)                 # [1, 2] [9, 9]

a = [1, 2]
b = a
b[0] = 9                    # muda a LISTA para onde o b aponta (que é a do a)
print(a, b)                 # [9, 2] [9, 2]

# Isto é O ponto mais importante do ficheiro:
#
#   nome = ...          -> muda a SETA do nome. Nunca altera nenhum objeto.
#   nome[i] = ...       -> altera a LISTA para onde o nome aponta.
#   nome.append(...)    -> altera a LISTA para onde o nome aponta.
#
# Pergunta a fazer em cada linha: "isto muda uma seta, ou muda um objeto?"






# %%
"""
Ex 18.3 [EXEMPLO] A armadilha do += com listas ---------------------------
"""

a = [1, 2]
b = a
a = a + [3]                 # o + cria uma lista NOVA; a seta do a vai para ela
print(a, b)                 # [1, 2, 3] [1, 2]

a = [1, 2]
b = a
a += [3]                    # com listas, o += ALTERA a própria lista (como o append)
print(a, b)                 # [1, 2, 3] [1, 2, 3]  (!!)

# Com números (Ex 15.4), x += 5 e x = x + 5 são iguais.
# Com listas NÃO são:
#   a = a + [3]   -> cria uma lista nova. Quem partilhava a lista antiga não vê nada.
#   a += [3]      -> acrescenta à lista que já existe. Quem a partilha vê a mudança.
#
# Para não ter dúvidas, usa append para acrescentar à lista
# e + quando queres mesmo uma lista nova.






# %%
"""
Ex 18.4 [EXEMPLO] Aviso para o futuro: listas dentro de listas --------------

    Daqui a umas semanas vais trabalhar com matrizes (listas de listas).
    Esta armadilha aparece logo.
"""

row = [0] * 3
print(row)                  # [0, 0, 0]  -> sem problemas

m = [[0] * 3] * 3           # "3 linhas de 3 zeros"... parece
m[0][0] = 1
print(m)                    # [[1, 0, 0], [1, 0, 0], [1, 0, 0]]  (!!)

# O "* 3" repete SETAS, não copia objetos.
#   [0] * 3        -> 3 setas para o MESMO 0. Não faz mal: o 0 nunca muda.
#   [[0]*3] * 3    -> 3 setas para a MESMA lista [0, 0, 0].
#                     Mudar m[0][0] muda "as três linhas", porque são uma só.
#
# A forma certa: criar cada linha de propósito, com um ciclo.

def create(nr: int, nc: int, value: int) -> list[list[int]]:
    """ Matrix with nr rows and nc columns, all with value. """
    m = []
    for _ in range(nr):
        m.append([value] * nc)      # uma lista NOVA em cada volta
    return m

m = create(3, 3, 0)
m[0][0] = 1
print(m)                    # [[1, 0, 0], [0, 0, 0], [0, 0, 0]]






# %%
"""
Ex 18.5 [FAZ] O que escreve? ------------------------------------------
"""

x = [1, 2, 3]
y = x
z = [1, 2, 3]
y[1] = 20
z[1] = 30
print(x, y, z)
print(x == y, x == z, x is y, x is z)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 18.5
#
# Escreve:
#   [1, 20, 3] [1, 20, 3] [1, 30, 3]
#   True False True False
#
#   x e y apontam para a mesma lista. z é OUTRA lista (escrita com os mesmos valores).
#   y[1] = 20 muda a lista do x e do y. z[1] = 30 muda só a do z.
#   x == z: os valores são diferentes agora ([1, 20, 3] vs [1, 30, 3]) -> False.
#   x is z: nunca foram a mesma lista -> False.






# %%
"""
Ex 18.6 [FAZ] O que escreve? ------------------------------------------

    Linha a linha: "muda uma seta ou muda um objeto?"
"""

a = [1]
b = a
a += [2]
a = a + [3]
a.append(4)
print(a, b)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 18.6
#
# Escreve: [1, 2, 3, 4] [1, 2]
#
#   b = a          a e b -> mesma lista [1]
#   a += [2]       ALTERA essa lista: [1, 2]      (o b também vê)
#   a = a + [3]    lista NOVA [1, 2, 3]; só a seta do a muda
#   a.append(4)    altera a lista NOVA: [1, 2, 3, 4]
#   O b ficou na lista antiga: [1, 2]






# %%
"""
Ex 18.7 [PENSA] Porque é que [0] * 3 não tem o problema do Ex 18.4? -------

    Nas duas expressões o "* 3" repete setas para o mesmo objeto.
    Porque é que numa dá problema e na outra não?
"""

# TODO: a tua explicação
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 18.7:
#
#   Em [0] * 3, as 3 setas apontam para o objeto 0.
#   "row[0] = 5" muda a SETA da posição 0 (passa a apontar para o 5).
#   As outras posições continuam a apontar para o 0. O 0 nunca é alterado.
#
#   Em [[0]*3] * 3, as 3 setas apontam para a mesma LISTA.
#   "m[0][0] = 1" não muda a seta m[0]: entra na lista m[0] e altera-a.
#   Como m[1] e m[2] apontam para essa mesma lista, "mudam" também.
#
#   Partilhar objetos IMUTÁVEIS nunca faz mal. Partilhar MUTÁVEIS pode fazer.






# %%
"""
Ex 18.8 [DESAFIO] O que escreve? -----------------------------------------

    Sem correr! Solução no fim do ficheiro.
"""

a = [1, 2]
b = [a, a]
a.append(3)
b[0] = [0]
print(a, b)






# %%
"""
Ex 18.9 [SOZINHA] Matriz sem armadilhas -------------------------------

    a) Usa a função create do Ex 18.4 para criar uma matriz 2x4 de zeros.
       Põe 7 no canto inferior direito. Confirma que só mudou uma posição.
    b) Escreve copy_matrix(m): uma cópia INDEPENDENTE de uma matriz.
       Teste: depois de m2 = copy_matrix(m) e m2[0][0] = 99, o m não pode mudar.
       Cuidado: m.copy() NÃO chega. Descobre porquê no Python Tutor.

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
19. FUNÇÕES COM LISTAS [~25 min]
===========================================================================

Ex 19.1 [EXEMPLO] Três funções, três comportamentos ------------------------
"""

def double_in_place(l: list[int]) -> None:
    """ Double every element of l (changes l). """
    for i in range(len(l)):
        l[i] = l[i] * 2

def doubled_copy(l: list[int]) -> list[int]:
    """ New list with the doubles of the elements of l (l is not changed). """
    result = []
    for v in l:
        result.append(v * 2)
    return result

def try_to_replace(l: list[int]) -> None:
    """ Try to replace l with another list (does NOTHING outside). """
    l = [0, 0, 0]

nums = [1, 2, 3]
double_in_place(nums)
print(nums)                     # [2, 4, 6]  -> a função alterou a lista de fora

nums = [1, 2, 3]
d = doubled_copy(nums)
print(nums, d)                  # [1, 2, 3] [2, 4, 6]  -> nums intacta

nums = [1, 2, 3]
try_to_replace(nums)
print(nums)                     # [1, 2, 3]  -> nada mudou

# O parâmetro l aponta para a MESMA lista que o nums. Logo:
#   - l[i] = ... altera essa lista: quem chamou vê a alteração;
#   - l = [...] só muda a seta do l (que é local): quem chamou não vê nada.
#
# É a mesma regra do Ex 18.2, agora entre uma função e quem a chama.
#
# Resumo de como o Python passa argumentos:
#   passa SEMPRE a seta para o objeto (nem cópia, nem "a própria variável").
#   - objeto imutável (int, float, str, tuplo): a função não o consegue alterar.
#     Na prática, é como se recebesse uma cópia.
#   - objeto mutável (lista): a função consegue alterá-lo, e quem chamou vê.
#   - reatribuir o parâmetro (l = ...) nunca afeta quem chamou.
#
# Noutras linguagens fala-se em "passagem por valor" e "por referência".
# O Python não é bem nenhuma das duas: alguns livros chamam-lhe
# "passagem por partilha" (call by sharing).






# %%
"""
Ex 19.2 [EXEMPLO] O bug do "for v in l: v = ..." ------------------------------
"""

def double_wrong(l: list[int]) -> None:
    """ Try to double the elements (WITH A BUG). """
    for v in l:
        v = v * 2               # muda a seta do v, não a lista!

nums = [1, 2, 3]
double_wrong(nums)
print(nums)                     # [1, 2, 3]  -> não mudou nada

# Em cada volta, v aponta para um elemento da lista (um número).
# "v = v * 2" cria um número novo e muda a seta do v.
# A lista continua a apontar para os números antigos.
#
# Para alterar os elementos de uma lista é preciso o ÍNDICE:
#     for i in range(len(l)):
#         l[i] = l[i] * 2
# (é a 2ª forma de percorrer do ficheiro 4, Ex 14.2)






# %%
"""
Ex 19.3 [EXEMPLO] A armadilha do sort: alterar NÃO é devolver ---------------
"""

l = [3, 1, 2]
r = l.sort()                # ordena a PRÓPRIA lista...
print(r, l)                 # None [1, 2, 3]  (!!) ...e devolve None

l = [3, 1, 2]
r = sorted(l)               # cria uma lista NOVA ordenada
print(r, l)                 # [1, 2, 3] [3, 1, 2]

# É a mesma diferença do Ex 19.1, nas funções que o Python já traz:
#   l.sort()      altera a lista, devolve None (como double_in_place)
#   sorted(l)     não altera, devolve uma lista nova (como doubled_copy)
# O mesmo acontece com append, insert, reverse: alteram e devolvem None.
#
# Erro clássico:   l = l.sort()   -> o l passa a apontar para None!






# %%
"""
Ex 19.4 [FAZ] O que escreve? ------------------------------------------
"""

def f(l: list[int]) -> None:
    l.append(len(l))
    l[0] = 99

a = [5, 6]
f(a)
f(a)
print(a)

# TODO: resposta
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 19.4
#
# Escreve: [99, 6, 2, 3]
#
#   1ª chamada: len = 2 -> append(2) -> [5, 6, 2]; l[0] = 99 -> [99, 6, 2]
#   2ª chamada: len = 3 -> append(3) -> [99, 6, 2, 3]; l[0] = 99 (já era)
#   As duas chamadas alteram a MESMA lista (a do a).






# %%
"""
Ex 19.5 [FAZ] Somar a todos: alterando e copiando --------------------

    a) add_to_all(l, k): soma k a todos os elementos, ALTERANDO a lista.
    b) added(l, k): devolve uma lista NOVA com k somado, sem mexer em l.
"""

def add_to_all(l: list[int], k: int) -> None:
    """ Add k to every element of l (changes l). """
    pass        # TODO

def added(l: list[int], k: int) -> list[int]:
    """ New list with k added to every element of l. """
    pass        # TODO

nums = [1, 2, 3]
add_to_all(nums, 10)
print(nums)                     # [11, 12, 13]

nums = [1, 2, 3]
print(added(nums, 10), nums)    # [11, 12, 13] [1, 2, 3]
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 19.5

def add_to_all(l: list[int], k: int) -> None:
    """ Add k to every element of l (changes l). """
    for i in range(len(l)):
        l[i] += k

def added(l: list[int], k: int) -> list[int]:
    """ New list with k added to every element of l. """
    result = []
    for v in l:
        result.append(v + k)
    return result

nums = [1, 2, 3]
add_to_all(nums, 10)
print(nums)                     # [11, 12, 13]

nums = [1, 2, 3]
print(added(nums, 10), nums)    # [11, 12, 13] [1, 2, 3]

# add_to_all: precisa dos índices (Ex 19.2). Não tem return: devolve None.
# added: pode percorrer pelos valores, porque não altera a l.
# Repara nos cabeçalhos: "-> None" avisa logo que a função altera algo;
# "-> list[int]" avisa que devolve uma lista nova.






# %%
"""
Ex 19.6 [PENSA] Trocar dois elementos (teórica 04b) ---------------------

    swap(l, i, j): troca os elementos das posições i e j da lista.
        l = [10, 20, 30]
        swap(l, 0, 2)
        print(l)            # [30, 20, 10]

    Porquê é que ESTA função consegue trocar, e o swap(x, y) do Ex 16.6 não?
    Precisa de return?
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 19.6:
#
#   Uma linha: l[i], l[j] = l[j], l[i]
#   Não precisa de return (cabeçalho -> None).
#
#   Consegue porque altera a LISTA (que é partilhada com quem chamou),
#   e não as setas de nomes locais. No Ex 16.6, a função só conseguia
#   mudar as setas dos seus próprios nomes x e y.






# %%
"""
Ex 19.7 [DESAFIO] Altera e devolve (estilo Teste 1 2025/26, 1c) -----------

    O accumulation do teste ALTERA a lista e DEVOLVE-A.
    O que escreve este código? Sem correr! Solução no fim do ficheiro.
"""

def accumulation(l: list[int]) -> list[int]:
    for i in range(1, len(l), 1):
        l[i] = l[i] + l[i-1]
    return l

a = [1, 2, 3]
b = accumulation(a)
c = accumulation(a.copy())
print(a, b, c, a is b)






# %%
"""
Ex 19.8 [DESAFIO] Parâmetros de todos os tipos ----------------------------

    Sem correr! Solução no fim do ficheiro.
"""

def g(l: list[int], n: int) -> list[int]:
    n += 1
    l.append(n)
    l = l + [n]
    l.append(0)
    return l

x = [1]
k = 5
y = g(x, k)
print(x, k, y)






# %%
"""
Ex 19.9 [SOZINHA] Negativos a zero ------------------------------------

    a) zero_negatives(l): põe a 0 todos os elementos negativos, ALTERANDO a lista.
           l = [3, -1, 4, -5]; zero_negatives(l); print(l)   # [3, 0, 4, 0]
    b) without_negatives(l): devolve uma lista NOVA só com os não negativos.
           without_negatives([3, -1, 4, -5]) == [3, 4]
    Escreve os dois cabeçalhos completos (com o tipo de retorno certo!).

    Sem solução. Se encravares, envia email.
"""

# TODO






# %%
"""
===========================================================================
20. TUDO JUNTO, AO NÍVEL DOS TESTES [~20 min]
===========================================================================

    Cada pergunta mistura funções, ciclos, ifs, números e listas.
    Para cada uma: tabela das variáveis, setas, e só depois a resposta.

Ex 20.1 [DESAFIO] O que escreve? -----------------------------------------
"""

def update(values: list[int], limit: int) -> int:
    count = 0
    for i in range(len(values)):
        if values[i] > limit:
            values[i] = limit
            count += 1
    limit = 0
    return count

v = [3, 8, 5, 10]
lim = 5
c = update(v, lim)
print(v, lim, c)

# Solução no fim do ficheiro.






# %%
"""
Ex 20.2 [DESAFIO] O que escreve? -----------------------------------------
"""

def f2(a: list[int]) -> list[int]:
    b = a
    b.append(1)
    a = [2]
    a.append(3)
    return b

x = []
y = f2(x)
z = f2(y)
print(x, y, z, x is z)

# Solução no fim do ficheiro.






# %%
"""
Ex 20.3 [DESAFIO] O que escreve? -----------------------------------------
"""

i = 10

def loop_sum(n: int) -> int:
    total = 0
    for i in range(n):
        total += i
    return total + i

print(loop_sum(4), i)

# Solução no fim do ficheiro.






# %%
"""
Ex 20.4 [SOZINHA] O que escreve? --------------------------------------

        def mystery(l: list[int]) -> None:
            for i in range(len(l) - 1):
                l[i + 1] = l[i] * 2

        m = [1, 5, 5, 5]
        mystery(m)
        print(m)

    E se a função fosse chamada com mystery(m.copy())?

    Sem solução. Confirma no Python Tutor.
"""






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  [ ] uma variável é um nome com uma seta para um objeto
  [ ] "nome = ..." muda uma seta; nunca altera o objeto antigo
  [ ] == compara valores; is compara setas (o mesmo objeto?)
  [ ] números, strings e tuplos não mudam; listas mudam
  [ ] o tipo pertence ao valor (tipagem dinâmica); forte vs fraca é outra pergunta
  [ ] uma função recebe setas: com imutáveis é como uma cópia;
      com listas, alterar o conteúdo vê-se cá fora; reatribuir o parâmetro não
  [ ] o único caminho para um número sair de uma função é o return
  [ ] if e for não criam variáveis locais; só as funções criam
  [ ] a variável do for fica com o último valor; com range vazio não existe
  [ ] a = a + [x] cria lista nova; a += [x] altera a lista
  [ ] [[0] * 3] * 3 partilha a mesma linha; usar um ciclo
  [ ] for v in l: v = ... não altera a lista; usar os índices

  Se alguma linha ainda não te parece clara, volta à secção dela
  e usa o Python Tutor.

  PRÓXIMO: os testes de exemplo (revisoes_6_teste_exemplo_1.py)
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
# Solução do Ex 15.5
#
# Escreve: 8 7 1
#
#   a = 1         a -> 1
#   b = a         b -> 1
#   c = b         c -> 1
#   b = 7         b -> 7          (só a seta do b muda; o c continua no 1)
#   a = c + b     a -> 1 + 7 = 8






# %%
# Solução do Ex 16.7
#
# Escreve: 10 10
#
#   p(3): o n do p é o parâmetro (3), não o n de fora.
#     h(3): o n do h é OUTRO nome local: n = 6, return 7
#     return 7 + 3 = 10
#   O n de fora nunca foi tocado: 10.
#
# Três n: o de fora (10), o parâmetro do p (3), o local do h (6).






# %%
# Solução do Ex 17.6
#
# Escreve: 2 1 3
#
#   i = 0: range(0) é vazio, o ciclo de dentro não corre
#   i = 1: j = 0 -> x = 1
#   i = 2: j = 0 -> x = 2;  j = 1 -> x = 3
#   No fim: i fica 2, j fica 1 (o último valor de cada ciclo), x fica 3.
#
# Repara: se o range do ciclo de fora fosse range(1), o ciclo de dentro
# nunca corria e o print(j) dava NameError.






# %%
# Solução do Ex 18.8
#
# Escreve: [1, 2, 3] [[0], [1, 2, 3]]
#
#   a = [1, 2]          a -> L1 = [1, 2]
#   b = [a, a]          b -> [seta para L1, seta para L1]
#   a.append(3)         L1 passa a [1, 2, 3]  (o b vê: as duas setas vão dar a L1)
#   b[0] = [0]          a posição 0 do b passa a apontar para uma lista NOVA [0]
#                       (muda uma seta dentro do b; L1 não é tocada)
#   a continua L1 = [1, 2, 3]; b = [[0], L1]






# %%
# Solução do Ex 19.7
#
# Escreve: [1, 3, 6] [1, 3, 6] [1, 4, 10] True
#
#   b = accumulation(a): ALTERA a lista do a para [1, 3, 6] e devolve
#                        a MESMA lista. Logo b is a.
#   c = accumulation(a.copy()): trabalha numa cópia de [1, 3, 6]
#                        -> [1, 4, 10]. O a não é tocado.
#
# Moral: uma função que altera a lista E a devolve engana quem lê.
# Parece que "cria uma lista nova", mas não cria.






# %%
# Solução do Ex 19.8
#
# Escreve: [1, 6] 5 [1, 6, 6, 0]
#
#   n += 1         n local passa a 6 (número: o k de fora fica 5)
#   l.append(n)    ALTERA a lista do x: [1, 6]
#   l = l + [n]    lista NOVA [1, 6, 6]; a seta do l muda; o x fica [1, 6]
#   l.append(0)    altera a lista NOVA: [1, 6, 6, 0]
#   return l       devolve a lista nova -> y






# %%
# Solução do Ex 20.1
#
# Escreve: [3, 5, 5, 5] 5 2
#
#   O ciclo altera a lista partilhada: 8 -> 5 e 10 -> 5 (o 5 não é > 5).
#   count = 2.
#   "limit = 0" só muda o parâmetro local: o lim de fora continua 5.






# %%
# Solução do Ex 20.2
#
# Escreve: [1, 1] [1, 1] [1, 1] True
#
#   y = f2(x):  b e a apontam para a lista do x (L).
#               b.append(1) -> L = [1]
#               a = [2] só muda a seta local; a.append(3) altera essa lista nova
#               return b -> L. Logo y é L.
#   z = f2(y):  mesma coisa com L: L = [1, 1]. z é L.
#   x, y e z são TODOS a mesma lista L = [1, 1].






# %%
# Solução do Ex 20.3
#
# Escreve: 9 10
#
#   Dentro do loop_sum, o i do for é LOCAL (atribuir-lhe valor torna-o local).
#   total = 0 + 1 + 2 + 3 = 6, e o i local fica 3 (último valor do range).
#   return 6 + 3 = 9.
#   O i de fora nunca mudou: 10.
