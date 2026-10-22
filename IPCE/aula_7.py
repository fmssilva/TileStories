# %%
"""
===========================================================================
GUIA DE SESSÃO - Prática 07 - IPCE 2026/2027
===========================================================================

Cobre os guiões 07a (ex. 45 a 47) e 07b (ex. 48).

Site da Cadeira: https://ipce-184ea7.gitlab.io/
Este Guião na drive: https://drive.google.com/drive/folders/1twlkB4M0yg0tBxOt2c1diqJU6xtUsoay?usp=sharing


Tema de hoje: o ciclo WHILE e as MATRIZES.
    1ª parte: o while. Repete "enquanto" uma condição for verdadeira.
              Primeiro: coisas que já fizemos com for, agora com while.
              Depois: o que o for NÃO consegue fazer (ex: um jogo que acaba quando se acerta).
    2ª parte: matrizes = listas de listas.
              Uma tabela com linhas e colunas: m[i][j].
              E o for dentro de for da aula 6 volta a aparecer, agora com dados.

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

    - Dúvidas da aula passada (desenhos, ord e chr, o CONSOLIDAR das diagonais)
"""






# %%
"""
===========================================================================
Guião 07a, exercícios 45 e 46 - O ciclo while [62 min -> 11:20]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
a) ciclo for vs while... [6 min -> 10:24] -----

    Efeito semelhante, só syntax diferente

    Exemplo 1: print 0 a 4 com for e com while
"""

# Com for
for i in range(5):
    print(i, end=" ")
print()                         # 0 1 2 3 4

# Com while
i = 0                           # 1. inicialização: onde começa
while i < 5:                    # 2. condição: enquanto for True, repete
    print(i, end=" ")
    i += 1                      # 3. avanço: andar um passo
print()                         # 0 1 2 3 4



# Um while tem sempre 3 peças:
#   1. inicialização (antes do ciclo)
#   2. condição      (na linha do while)
#   3. avanço        (dentro do corpo)

#   inicialização
#   while condição:
#       corpo       # inclui o "avanço"

# O for faz as 3 sozinho: range(5) diz onde começa, onde acaba e o passo.
# No while, somos nós que tratamos de cada uma.

# O while: 
#   - Testa a condição ANTES de cada volta. Quando dá False, sai.
#   - Repete o corpo ENQUANTO a condição for verdadeira.


#%%
"""
    Exemplo 2: correr uma lista com for e com while
"""
l = [10, 20, 30, 40, 50]

# Com for e lendo os valores diretamente (!!! Não conseguimos alterar a lista in place assim)
for v in l:
    print(v, end=" ")
print()                         # 10 20 30 40 50


# Com for e lendo os índices (» Conseguimos alterar a lista in place assim)
for i in range(len(l)):
    print(l[i], end=" ")
print()                         # 10 20 30 40 50

# Com while
i = 0
while i < len(l):
    print(l[i], end=" ")
    i += 1
print()                         # 10 20 30 40 50




#%%
"""

b) Exercicios for --> while 

    Exercicio 1: escrever 10 9 8 ... 1 com while
"""
# Com for
for i in range(10, 0, -1):
    print(i, end=" ")
print()

# Com while
i = 10
while i >= 1:
    print(i, end=" ")
    i -= 1
print()                         # 10 9 8 7 6 5 4 3 2 1



#%%
"""
   Exercicio 2: escrever os pares de 0 a 10 com while
"""

# Com for
for i in range(0, 11, 2):
    print(i, end=" ")
print()

# Com while
i = 0
while i <= 10:
    print(i, end=" ")
    i += 2
print()                         # 0 2 4 6 8 10




# %%
"""
    Exercicio 3: contar os números negativos numa lista com while
"""

l = [3, 1, 4, -1, 5, -9, 2, -6, 7]

# Com for
tot_neg = 0
for v in l:
    if v < 0:
        tot_neg += 1
print(f"Existem {tot_neg} números negativos na lista.")

# Com while
tot_neg = 0
i = 0
while i < len(l):
    if l[i] < 0:
        tot_neg += 1
    i += 1
print(f"Existem {tot_neg} números negativos na lista.")



#%%
"""
    Exercicio 4: somar os elementos de uma lista com while
"""
# Com for
def sum_for(l: list[int]) -> int:
    """ Add the elements of a list. """
    total = 0
    for v in l:
        total += v
    return total

def sum_while(l: list[int]) -> int:
    """ Add the elements of a list. """
    total = 0
    i = 0
    while i < len(l):
        total += l[i]
        i += 1
    return total

print(sum_for([3, 1, 4]))   # 8
print(sum_while([3, 1, 4]))  # 8



# %%
"""
    Exercicio 5: Fatorial com while
"""

# Com for
def factorial_for(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1
    for i in range(1, n + 1):
        fact *= i
    return fact

def factorial_while(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1
    i = 1                       # inicialização
    while i <= n:               # condição (o n + 1 do range virou <=)
        fact *= i
        i += 1                  # avanço
    return fact

print(factorial_for(5))   # 120
print(factorial_while(5))  # 120



# Na prática são quase iguais: só a syntax muda.
#
# Mas olhem para a versão for: numa linha só, vê-se tudo
#   for i in range(1, n + 1)   -> de 1 a n
#   for v in l                 -> todos os elementos da lista
#
# Na versão while, para saber onde começa e acaba,
# temos de procurar 3 sítios diferentes (e confirmar que estão certos).
#
# Por isso a regra da cadeira (teórica 4a):
#   percorrer um contentor (range, lista, string...) -> for
#   o resto                                           -> while








# %%
"""
c) EXERCICIO 45 (GUIÃO) - De for para while [8 min -> 10:40] ---------------

    Transformar o programa abaixo (só com for) num equivalente só com while.

    Note bem que este programa fica muito melhor tal como está, escrito com ciclos for. 
    Este é apenas um exercício de treino.
"""
# Com for
def f(n: int) -> None:
    for i in range(n):
        if i % 2 == 0:
            print(i)

f(7)
print()

# Com while
def f(n: int) -> None:
    i = 0
    while i < n:
        if i % 2 == 0:
            print(i)
        i += 1

f(7)

# Estamos a imprimir os números pares de 0 a n-1. 
# O for faz isso sozinho, o while precisa de inicialização, condição e avanço.



#%%
# Com for
def g(m: int, n: int) -> None:
    for i in range(m):
        for j in range(n):
            if i % 2 == 0 and j % 2 == 0:
                print((i,j))

g(4,5)
print()

# Com while
def g(m: int, n: int) -> None:
    i = 0
    while i < m:
        j = 0                   # o j volta a 0 em CADA linha i
        while j < n:
            if i % 2 == 0 and j % 2 == 0:
                print((i,j))
            j += 1
        i += 1


def main() -> None:
    g(4,5)

main()

# Estamos a imprimir os pares de índices (i, j) que são ambos pares
#
# Cuidado para iniciar i e j fora do respetivo while 
# ("j = 0" tem de ficar DENTRO do while de fora)
#
# Se o "j = 0" ficar lá em cima, ao lado do "i = 0":
#   na linha i = 0, o j vai até 5;
#   nas linhas seguintes, o j já está em 5 e o while de dentro nem entra.
# Resultado: só sai (0, 0) (0, 2) (0, 4). Experimentem!







# %%
"""
d) Quando o for não chega [8 min -> 10:48] ---------------------------------

    Até agora o while só imitou o for.
    Mas há problemas em que NÃO sabemos quantas voltas vão ser.
    Não há contentor para percorrer: só uma condição para parar.
"""


#%%
# 1. Quantos algarismos tem um número?
def count_digits(n: int) -> int:
    """ Number of digits of n.
        Precondition: n > 0
    """
    count = 0
    while n > 0:
        count += 1
        n //= 10                # tira o último algarismo: 1234 -> 123
    return count

print(count_digits(1234))
print(count_digits(7))
print(count_digits(1000))   

# Atenção: a condição "n > 0" é logo False,
# o ciclo dá ZERO voltas e devolve 0. Mas o 0 tem 1 algarismo!
# Por isso a precondição diz n > 0.
# (o while pode dar zero voltas, tal como o for com range(0))







# %%
# 2. Quantos anos até um depósito duplicar a 3% ao ano? 

def years_to_double(rate: float) -> int:
    """ Years until a deposit doubles, at a yearly rate.
        Precondition: rate > 0
    """
    money = 1000.0
    years = 0
    while money < 2000.0:
        money *= 1 + rate
        years += 1
    return years

print(years_to_double(0.03))    # 24
print(years_to_double(0.10))    # 8

# A pergunta a fazer: "sei à partida o que vou percorrer?"
#   sim (uma lista, um range, uma string) -> for
#   não, só sei quando parar              -> while






#%%
# 3. O algoritmo de Euclides (teórica 1a e 2a): o mdc de dois números
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

print(euclides(252, 105))       # 21
print(euclides(123, 456))      # 3

# Nos três, ninguém sabe à partida quantas voltas vai dar:
#   depende dos números. O ciclo só pára quando a condição deixa de ser verdade.
#
# É aqui que o while é a ferramenta certa.





# %%
# 4. Um interpretador de comandos que recebe inputs do user sem limite definido

def command_A(): 
    print("Comando A executado")

def command_B(): 
    print("Comando B executado")

# interpreter em que "exit" é uma condição avaliada no corpo
def interpret() -> None:
    """ Interpret a sequence of commands until the user types "Q" == exit. """
    while True:
        cmd = input("Comando (A, B, C ou exit)? ")
        if cmd == "A":
            command_A()
        elif cmd == "B":
            command_B()
        elif cmd == "Q":
            break
        else:
            print("Comando desconhecido")

# interpreter em que "exit" é iniciado antes e avaliado na condição 
def interpret2() -> None:
    """ Interpret a sequence of commands until the user types "Q" == exit. """
    cmd = ""
    while cmd != "Q":
        cmd = input("Comando (A, B, C ou Q)? ")
        if cmd == "A":
            command_A()
        elif cmd == "B":
            command_B()
        elif cmd != "Q":
            print("Comando desconhecido")


# este caso de iniciar o END_MARK antes e usa-lo na condição no while
# é preciso ter cuidado, caso o essa marca


# Exemplo: Ler inteiros do terminal até user escrever -1, e devolver a soma.

END_MARK = -1

# Correto: lê o input antes do ciclo e dentro do ciclo depois de somar
def read_and_add() -> int:
    """ Read and add a sequence of integers with unknown length. """
    sum = 0
    v = int(input("> "))     # lê (fora do ciclo)             
    while v != END_MARK:     # testa
        sum += v             # soma
        v = int(input("> ")) # lê (dentro do ciclo)  
    return sum
print("correcto:", read_and_add())  # lê inteiros até -1 e devolve a soma

#%%
# Errado: lê o input só dentro do ciclo, e soma o END_MARK
def read_and_add_wrong() -> int:
    """ Read and add a sequence of integers with unknown length. """
    sum = 0
    v =  0
    while v != END_MARK:     
        v = int(input("> "))  
        sum += v             # reparem que aqui vamos adicionar o END_MARK à soma, o que não faz sentido
    return sum
print("wrong:", read_and_add_wrong()) # lê inteiros até -1 e devolve a soma (mas soma o -1, o que não faz sentido)






# %%
"""
e) Riscos/Erros no while [6 min -> 10:54] ---------

    As chamadas estão comentadas de propósito. 
    Tirem o # de UMA de cada vez e corram SÓ esta célula (Ctrl + Enter).
"""

# 1. Esquecer o avanço  (i+=1)  -> CICLO INFINITO
def forgot_the_advance() -> int:
    """ BUG: the advance is missing, so this loop never ends. """
    i = 0
    total = 0
    while i < 5:
        total += i              # falta o i += 1 !
    return total

# print(forgot_the_advance())   # nunca acaba: parem com o quadrado vermelho ou Ctrl + C





#%%
# 2. Avanço não aproxima da condição de paragem  -> CICLO INFINITO
def advance_wrong() -> int:
    """ BUG: the advance does not approach the end condition, so this loop never ends. """
    i = 0
    total = 0
    while i < 5:
        total += i
        i += 3                  # o passo é demasiado grande: salta por cima do fim
    return total

# print(advance_wrong())        # nunca acaba: parem com o quadrado vermelho ou Ctrl + C





#%%
# 3. Condição de paragem impossível  -> CICLO INFINITO
def impossible_condition() -> int:
    """ BUG: the end condition is impossible, so this loop never ends. """
    i = 0
    total = 0
    while i != 10:              # o i nunca vai ser igual a 10
        total += i
        i += 3                  # o passo é demasiado grande: salta por cima do fim
    return total 

# O i passa por 9, salta para 12 e nunca é igual a 10.
# Com "while i < 10" já parava.
# Quando o passo pode "saltar por cima" do fim, < e <= são mais seguros do que !=.







# %%
"""
f) EXERCICIO 46 (GUIÃO) - Jogo Hi-Lo [14 min -> 11:16] --------------------

    O computador pensa num número secreto entre 0 e 1023.
    Depois o utilizador tenta adivinhar qual é esse número, no menor número de jogadas que conseguir.
    
    O programa gera internamente um número secreto entre 0 e 1023, 
    e depois vai lendo as tentativas do utilizador do terminal, 
    e consoante cada tentativa, o computador vai responde:
        ">"  o secreto é MAIOR do que a tentativa
        "<"  o secreto é MENOR
        "="  acertou (e o jogo acaba)

    Cuidado com o sentido dos símbolos:
        o ">" responde à pergunta "o secreto é maior?".
        Por isso, se a tentativa é MENOR do que o secreto, escreve-se ">".

    Exemplo de execução:
        JOGO HI-LO
        Já pensei num número...
        Tente adivinhar: 500
        <
        Tente adivinhar: 300
        <
        ...
        Tente adivinhar: 66
        =
        Parabéns! Acertou em 10 tentativas! O número secreto era 66!
"""

import random

# Degrau: um dado de 6 faces. Corram várias vezes.
print(random.randint(1, 6))     # 1, 2, 3, 4, 5 ou 6 (os dois limites entram)

MAX_HILO = 1023

def ask_guess() -> int:
    """ Ask the user's guess. """
    guess = int(input("Tente adivinhar: "))
    return guess

def final_message(attempts: int, secret: int) -> None:
    """ Final message, after victory. """
    print(f"Parabéns! Acertou em {attempts} tentativas! O número secreto era {secret}!")

def play_hilo(lo: int, hi: int) -> None:
    """ The classic HI-LO game in the closed range lo to hi. """
    secret = random.randint(lo, hi)    # geração do número secreto!!! É assim que se faz!
    print("Já pensei num número...")
    attempts = 0
    while True:
        guess = ask_guess()
        attempts += 1
        if guess < secret:
            print('>')
        elif guess > secret:
            print('<')
        else:
            print('=')
            break
    final_message(attempts, secret)

def main() -> None:
    print("JOGO HI-LO")
    play_hilo(0, MAX_HILO)

main()

# Porquê while? Não se sabe quantas tentativas vão ser. Só se sabe parar: quando acerta.
#
# Porquê while True + break? A decisão de parar só existe DEPOIS de ler a tentativa.
# (é o padrão "ler até à marca" da célula anterior: aqui a "marca" é acertar)
#
# random.randint(lo, hi) dá um inteiro ao calhas, com lo e hi incluídos.
# (ao contrário do range, aqui o último entra)










# %%
"""
===========================================================================
INTERVALO [20 min -> 11:40]
===========================================================================
"""






# %%
"""
===========================================================================
Guião 07a, exercício 47 - Hi-Lo ao contrário [25 min -> 12:05]
https://ipce-184ea7.gitlab.io/
===========================================================================

    Agora trocam-se os papéis: o UTILIZADOR pensa no número
    e o COMPUTADOR tenta adivinhar.


    Eis os detalhes da iteração com o utilizador:

        O programa começa por escrever: "Pense num número secreto entre 0 e 1023.". 
        Nessa altura o utilizador pensa num número secreto (mas fica calado e não o diz ao computador).

        Depois, durante o jogo, o programa vai tentando adivinhar o número e vai perguntando: 
        "A minha tentativa é X. Diga-me se o número secreto é maior (>), menor (<) ou igual (=) ? ".

        No final do jogo há duas reações possíveis do programa: 
        "Viva! Acertei em N tentativas! O número secreto era X!" ou, "Você é batoteiro!".

        Ainda mais um detalhe. Desta vez queremos validação do input! 
        Quando o programa pede um dos símbolos (>) (<) ou (=), se o utilizador introduzir um símbolo errado, 
        o programa deve repetir a pergunta. Se o utilizador voltar a enganar-se, o programa repete a pergunta. 
        E assim sucessivamente, até finalmente o utilizador dar uma resposta com sentido.

        Este programa só precisa de usar inteiros e carateres; 
        não precisa de listas, nem de manipular strings, etc. 
        A novidades são só a programação de dois ciclos while: 
        (1) o ciclo de leitura com validação; (2) o ciclo que efetua a condução do jogo.


    Exemplo de interação onde o jogador humano não faz batota:

        JOGO HI-LO (REVERSE)
        Pense num número secreto entre 0 e 1023!
        Já pensou? <ENTER>
        A minha tentativa é 511. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 255. O número secreto é maior (>), menor (<) ou igual (=) ? >
        A minha tentativa é 383. O número secreto é maior (>), menor (<) ou igual (=) ? =
        Viva! Acertei em 3 tentativas! O número secreto era 383!

        
    Exemplo de interação, onde o jogador humano faz batota:
        JOGO HI-LO (REVERSE)
        Pense num número secreto entre 0 e 1023!
        Já pensou? <ENTER> 
        A minha tentativa é 511. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 255. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 127. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 63. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 31. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 15. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 7. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 3. O número secreto é maior (>), menor (<) ou igual (=) ? <
        A minha tentativa é 1. O número secreto é maior (>), menor (<) ou igual (=) ? >
        A minha tentativa é 2. O número secreto é maior (>), menor (<) ou igual (=) ? >
        Você é batoteiro!
"""


MAX_HILO = 1023

def initial_request(lo: int, hi: int) -> None:
    """ Start game. """
    print(f"Pense num número secreto entre {lo} e {hi}!")
    input("Já pensou? <ENTER> ")    # wait

def ask(guess: int) -> str:
    """ Read input until "<", ">" ou "=" """
    while True:
        prompt = f"A minha tentativa é {guess}. "
        prompt += "O número secreto é maior (>), menor (<) ou igual (=) ? "
        reply = input(prompt)
        if reply in {'>', '<', '='}:
            return reply

def final_message(lo: int, hi: int, attempts: int, guess: int) -> None:
    """ End game. """
    if lo > hi:
        print("Você é batoteiro!")
    else:
        print(f"Viva! Acertei em {attempts} tentativas! O número secreto era {guess}!")

def play_hilo_reverse(lo: int, hi: int) -> None:
    """ The classic HI-LO game in the closed range lo to hi. """
    initial_request(lo, hi)
    attempts = 0
    while lo <= hi:
        guess = (lo + hi) // 2      # o meio do intervalo que ainda resta
        attempts += 1
        reply = ask(guess)
        if reply == '>':
            lo = guess + 1          # o secreto está à direita do meio
        elif reply == '<':
            hi = guess - 1          # o secreto está à esquerda do meio
        else:                       # reply == '=' de certeza (o ask garante)
            break
    final_message(lo, hi, attempts, guess)

def main() -> None:
    print("JOGO HI-LO (REVERSE)")
    play_hilo_reverse(0, MAX_HILO)

main()

# Com o exemplo do guião (secreto 383):
#
#   lo    hi    guess  resposta
#   0     1023  511    <        -> hi = 510
#   0     510   255    >        -> lo = 256
#   256   510   383    =        -> acertou à 3ª
#
# O "+ 1" e o "- 1": o guess já foi testado, não vale a pena voltar a ele.
#
# E a batota? Se as respostas forem contraditórias, o intervalo fica vazio:
#   lo passa à frente de hi (lo > hi), a condição do while dá False e o ciclo acaba.
#   O final_message vê lo > hi e sabe que alguém mentiu.
# No 2º exemplo do guião, acaba com lo = 3 e hi = 2.
#
# Aqui o while tem uma condição a sério (lo <= hi) E um break:
#   saída normal  -> o intervalo esvaziou (batota)
#   saída a meio  -> acertou






# %%
"""
    Porque é que o computador é tão bom? [5 min -> 12:05] ----------------------

    Quantas tentativas, no PIOR caso, para um número de 0 a 1023?
"""

def attempts_needed(secret: int, lo: int, hi: int) -> int:
    """ Attempts of the halving strategy to find secret in [lo, hi].
        Precondition: lo <= secret <= hi
    """
    attempts = 0
    while True:
        guess = (lo + hi) // 2
        attempts += 1
        if guess < secret:
            lo = guess + 1
        elif guess > secret:
            hi = guess - 1
        else:
            return attempts

print(attempts_needed(383, 0, 1023))     # 3
print(attempts_needed(0, 0, 1023))       # 10

worst = 0
for secret in range(1024):               # experimentar os 1024 segredos todos
    a = attempts_needed(secret, 0, 1023)
    if a > worst:
        worst = a
print(worst)                             # 11

# Cada resposta corta o intervalo a meio:
#   1024 -> 512 -> 256 -> ... -> 2 -> 1   (10 cortes)
# Por isso, no pior caso, bastam 11 tentativas para 1024 números.
# Para um milhão de números, bastavam 20. Para mil milhões, 30.
#
# Chama-se PESQUISA BINÁRIA. É das ideias mais usadas em informática:
# procurar uma palavra num dicionário, um nome numa lista ordenada...
# (em matemática: o número de cortes é o logaritmo de base 2)










# %%
"""
===========================================================================
Guião 07b, exercício 48 - Matrizes [55 min -> 13:00]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""



# %%
"""
a) Uma matriz é uma lista de listas [6 min -> 12:11] -----------------------

    Antes de correr, prevejam cada print.
"""

m = [[1, 2, 3, 4, 5],
     [6, 7]
     [8, 9, 10]]

# No 1º "nível" acedemos às linhas: 
print(m[0])                     # [1, 2, 3, 4, 5]  a linha 0 (é uma lista)
print(m[1])                     # [6, 7]           a linha 1 (é uma lista)
print(m[2])                     # [8, 9, 10]       a linha 2 (é uma lista)

print(m[-1])                    # [4, 5, 6]

# %%
for r in m:                     # cada linha r da matriz m
    print(r)                    # [1, 2, 3, 4, 5],
                                # [6, 7],
                                # [8, 9, 10]

# %%
print(len(m))                   # 2           número de linhas
print(len(m[0]))                # tamanho da linha 0 (número de colunas - se a matriz for quadrada)





# %%
# No 2º "nível" acedemos às celulas: 

print(m[1][2])                  # 6           linha 1, coluna 2

# para obtermos uma coluna temos que iterar pelas linhas para ir busar o valor de cada coluna
# Exemplo valores da coluna 1
for r in m:
    print(r[1])



# %%
# Exercicios

m = [[1, 2, 3, 4, 5],
     [6, 7]
     [8, 9, 10]]


# Fazer print do valor 7 no terminal 
print(m[1][1])                  

#%%
# Trocar o 1 por 100 na matriz
m[0][0] = 100
print(m)                        # [[100, 2, 3], [4, 5, 6]]




# m[i] é a linha i (uma lista normal).
# m[i][j] é o elemento da linha i, coluna j.
# Ler da esquerda para a direita: primeiro escolho a linha, depois a coluna.
#
# Numa matriz a sério, todas as linhas têm o mesmo tamanho.
# Por isso o número de colunas é len(m[0]).



# %%
"""
b) Percorrer uma matriz: pelos valores ou pelos índices [5 min -> 12:16] ---

    As duas formas da aula 6 (revisão do for), agora em 2D.
"""

m = [[1, 2, 3],
     [4, 5, 6],
     [7, 8, 9]]

# Pelos VALORES: cada linha, e dentro dela cada valor
total = 0
for r in m:                     # r é uma linha (uma lista)
    for v in r:
        total += v
print(total)                    # 21

# Pelos ÍNDICES: cada (i, j), como nos desenhos da aula 6
for i in range(len(m)):
    for j in range(len(m[0])):
        print(f"m[{i}][{j}] = {m[i][j]}")
# m[0][0] = 1   m[0][1] = 2   ...   m[1][2] = 6



# %%
"""
    c) Exercícios:
"""
# 1. Contar os pares da matriz (pelos valores chega)
m = [[1, 2, 3],
     [4, 5, 6],
     [7, 8, 9]]

count = 0
for r in m:
    for v in r:
        if v % 2 == 0:
            count += 1
print(count)                    # 3

# A regra é a mesma das listas:
#   só preciso de LER os valores                -> for r in m / for v in r
#   preciso da posição, ou de ESCREVER na matriz -> for i ... / for j ...
#
# É o for dentro de for da aula 6:
#   lá, para cada (i, j) decidíamos o que DESENHAR;
#   aqui, para cada (i, j) há um VALOR guardado.



#%%

# 2. Print a soma dos elementos de cada linha da matriz (pelos valores chega)
m = [[1, 2, 3],
     [4, 5, 6],
     [7, 8, 9]]

for r in m:
    total = 0
    for v in r:
        total += v
    print(total)



# %%
"""
d) EXERCICIO 48 (GUIÃO, Mooshak R) - maximum [5 min -> 12:24] ---------------

    O valor máximo de uma matriz de inteiros não vazia.
        maximum([[1, 2, 3, 4], [5, 6, 7, 8]]) == 8
"""
type IMatrix = list[IList]       # matriz de inteiros

def maximum(m: IMatrix) -> int:
    """ Maximum of matrix.
        Precondition: is_valid(m) and not is_empty(m) """
    max = m[0][0]               # começar com um valor que existe mesmo
    for r in m:
        for v in r:
            if v > max:
                max = v
    return max

print(maximum([[1, 2, 3, 4], [5, 6, 7, 8]]))       # 8
print(maximum([[9, 1], [2, 3]]))                   # 9

# Encontrem o bug nesta versão:
def maximum_bug(m: IMatrix) -> int:
    """ Maximum of matrix (with a bug!). """
    max = m[0][0]
    for r in m:
        for v in r:
            if v > max:
                max = v
    return v

print(maximum_bug([[1, 2, 3, 4], [5, 6, 7, 8]]))   # 8   parece certo...
print(maximum_bug([[9, 1], [2, 3]]))               # 3   ... mas não está

# O bug: return v devolve o ÚLTIMO valor visto, não o máximo.
# Com o exemplo do guião passa por sorte: o máximo (8) também é o último.
#
# Lição (teórica 6a): um teste só, e ainda por cima um caso "arrumadinho",
# não chega. Testar também com o máximo no início, no meio, negativos...
#
# Porquê começar com m[0][0] e não com 0?
# Com maximum([[-5, -2]]) o 0 nunca seria ultrapassado e devolvia 0,
# que nem está na matriz.





# %%
"""
c) As ferramentas do guião (teórica 6a) [3 min -> 12:19] -------------------

    Corram esta célula ANTES das seguintes: os exercícios usam estas funções.
    
    Estes tipos são para ajudar nas operações com Matrizes,
    Criando-se algumas funções auxiliares prontas a usar para resover problemas
    
"""


type GList = list                # lista genérica
type GMatrix = list[GList]       # matriz genérica

type IList = list[int]           # lista de inteiros
type IMatrix = list[IList]       # matriz de inteiros

def create(nr: int, nc: int, value: any) -> GMatrix:
    """ Create and initialize a matrix with nr rows and nc columns.
        Precondition: nr >= 0 and nc >= 0
    """
    m = [None]*nr         # cria espaço para as linhas
    for i in range(nr):   # preenche as várias linhas
        m[i] = [value]*nc
    return m

print(create(2, 5, 0))          # [[0, 0, 0, 0, 0], [0, 0, 0, 0, 0]]

#%%
def is_valid(m: GMatrix) -> bool:
    """ Check if all the lines have the same length. """
    nr = len(m)
    for i in range(nr-1):  # verifica se todas as linhas têm igual comprimento
        if len(m[i]) != len(m[i+1]):
            return False
    return True

# local test
m = [[1, 2, 3],
     [4, 5, 6]]
n = [[1, 2, 3],
     [4, 5]]

print(is_valid(m))  # True
print(is_valid(n))  # False


def nrows(m: GMatrix) -> int:
    """ Number of rows of the matrix
        Precondition: is_valid(m)
    """
    return len(m)             # número de linhas

def is_empty(m: GMatrix) -> bool:
    """ Is the matrix empty? """
    return nrows(m) == 0      # testa se matriz vazia

def ncols(m: GMatrix) -> int:
    """ Number of columns of the matrix.
        Precondition: is_valid(m)
    """
    if is_empty(m):           # a matriz vazia tem 0 colunas
        return 0
    return len(m[0])      # número de colunas é o comprimento duma linha

def row(m: GMatrix, i: int) -> GList:
    """ Get the row i of the matrix.
        Precondition: is_valid(m) and i in range(nrows(m))
    """
    return m[i].copy()      # faz cópia porque col também faz cópia

def col(m: GMatrix, j: int) -> GList:
    """ Get the column j of the matrix.
        Precondition: is_valid(m) and j in range(ncols(m))
    """
    col = []
    for r in m:             # acumula os vários elementos da coluna j
        col.append(r[j])
    return col

m = [[1, 2, 3],
     [4, 5, 6]]
print(create(2, 3, 0))          # [[0, 0, 0], [0, 0, 0]]
print(nrows(m), ncols(m))       # 2 3
print(row(m, 1), col(m, 2))     # [4, 5, 6] [3, 6]

# São as peças de base. Os nomes dizem o que fazem:
#   create(nr, nc, value) -> uma matriz nova, toda com o mesmo valor
#   nrows(m), ncols(m)    -> número de linhas e de colunas
#   row(m, i), col(m, j)  -> uma cópia da linha i, ou da coluna j
#
# Com elas, o resto do código lê-se melhor: nrows(m) diz mais do que len(m).
#
# (o guião anota is_empty com -> int, mas o resultado é True/False: -> bool)










# %%
"""
e) EXERCICIO 48 (GUIÃO) - multiply [6 min -> 12:30] ------------------------

    Multiplicar uma matriz por um número. 
    O resultado é uma NOVA matriz.
        multiply([[1, 2, 3, 4], [5, 6, 7, 8]], 5) == [[5, 10, 15, 20], [25, 30, 35, 40]]
"""

def multiply(m: IMatrix, scalar: int) -> IMatrix:
    """ Multiply matriz by scalar.
        Precondition: is_valid(m)
    """
    nr, nc = nrows(m), ncols(m)
    m2 = create(nr, nc, 0)      # 1. criar a matriz nova, já com o tamanho certo
    for i in range(nr):         # 2. preencher posição a posição
        for j in range(nc):
            m2[i][j] = m[i][j] * scalar
    return m2                   # 3. devolver

m1 = [[1, 2, 3, 4],
      [5, 6, 7, 8]]
print(multiply(m1, 5))          # [[5, 10, 15, 20], [25, 30, 35, 40]]
print(m1)                       # [[1, 2, 3, 4], [5, 6, 7, 8]]   a original não mudou

# A receita das matrizes NOVAS (é a mesma da sum da teórica 6a):
#   1. create com o tamanho do resultado
#   2. for i, for j: escrever m2[i][j]
#   3. return
#
# Aqui são precisos os índices: para ESCREVER em m2[i][j],
# tenho de saber em que posição estou.
#
# E a matriz original fica intacta, como o cabeçalho promete (-> IMatrix):
# quem devolve uma matriz, não estraga a que recebeu.






# %%
"""
f) EXERCICIO 48 (GUIÃO) - diagonals [6 min -> 12:36] -----------------------

    Copiar as duas diagonais de uma matriz quadrada, 
    para um par de listas. 
    
    Exemplo: 
        m = [[1, 2, 3],
             [4, 5, 6],
             [7, 8, 9]]
        
        »» diagonals(m) == ([1, 5, 9], [3, 5, 7])
"""

def diagonals(m: GMatrix) -> tuple[GList, GList]:
    """ Get the two diagonals of the matrix.
        Precondition: is_valid(m) and nrows(m) == ncols(m)
    """
    nr = nrows(m)
    main_diag = []
    other_diag = []
    for i in range(nr):
        main_diag.append(m[i][i])           # a coluna é igual à linha
        other_diag.append(m[i][nr - 1 - i]) # linha + coluna dá sempre nr - 1
    return (main_diag, other_diag)

m3 = [[1, 2, 3],
      [4, 5, 6],
      [7, 8, 9]]

print(diagonals(m3))            # ([1, 5, 9], [3, 5, 7])

# São as diagonais do CONSOLIDAR da aula 6:
#   a descer: j == i            -> m[i][i]
#   a subir:  j == n - 1 - i    -> m[i][n - 1 - i]
#
# Lá usámos for i, for j e um if para decidir.
# Aqui basta um for: para cada linha i, a coluna da diagonal já se sabe.
#
# O resultado é um tuplo com duas listas (teórica 6b): (lista1, lista2).






# %%
"""
g) EXERCICIO 48 (GUIÃO) - transpose [7 min -> 12:43] -----------------------

    Trocar linhas por colunas. O resultado é uma NOVA matriz.
        transpose([[1, 2, 3, 4], [5, 6, 7, 8]]) == [[1, 5], [2, 6], [3, 7], [4, 8]]
"""

def transpose(m: GMatrix) -> GMatrix:
    """ Transpose of matrix .
        Precondition: is_valid(m)
    """
    nr, nc = nrows(m), ncols(m)
    m2 = create(nc, nr, 0)      # ATENÇÃO: nc linhas e nr colunas
    for i in range(nr):
        for j in range(nc):
            m2[j][i] = m[i][j]  # o que estava em (i, j) vai para (j, i)
    return m2

m1 = [[1, 2, 3, 4],
      [5, 6, 7, 8]]
print(transpose(m1))            # [[1, 5], [2, 6], [3, 7], [4, 8]]

# A receita das matrizes novas outra vez, com dois pormenores:
#   - o tamanho troca: uma 2x4 dá uma 4x2  -> create(nc, nr, 0)
#   - a posição troca: m2[j][i] = m[i][j]
#
# Se se esquecerem da troca no create, com uma matriz não quadrada
# dá IndexError (ou fica errado). Com uma quadrada nem se nota:
# por isso testem SEMPRE com uma matriz não quadrada, como a do guião.






# %%
"""
h) EXERCICIO 48 (GUIÃO) - product [8 min -> 12:51] -------------------------

    Produto de duas matrizes. 
    Na posição (i, j) do resultado fica
    o produto da linha i da 1ª pela coluna j da 2ª.

    Exemplo: 
        m1 = [ [1, 2, 3, 4],
               [5, 6, 7, 8] ]

        m2 = [ [ 1,  2,  3],
               [ 4,  5,  6],
               [ 7,  8,  9],
               [10, 11, 12] ]
        
        product(m1, m2) == [[70, 80, 90], [158, 184, 210]]

    https://www.mathsisfun.com/algebra/matrix-multiplying.html
"""

def dot(l1: IList, l2: IList) -> int:
    """ Sum of the products of corresponding elements.
        Precondition: len(l1) == len(l2)
    """
    total = 0
    for k in range(len(l1)):
        total += l1[k] * l2[k]
    return total

print(dot([1, 2, 3, 4], [1, 4, 7, 10]))     # 70 = 1*1 + 2*4 + 3*7 + 4*10

def product(m1: IMatrix, m2: IMatrix) -> IMatrix:
    """ Multiply two matrices.
        Precondition: is_valid(m1) and is_valid(m2)
                      and ncols(m1) == nrows(m2)
    """
    nr, nc = nrows(m1), ncols(m2)
    m3 = create(nr, nc, 0)
    for i in range(nr):
        for j in range(nc):
            m3[i][j] = dot(row(m1, i), col(m2, j))
    return m3

    # Outra forma (sem a dot): um 3º for, que faz a conta ali mesmo
    # n = ncols(m1)
    # m3 = create(nr, nc, 0)
    # for i in range(nr):
    #     for j in range(nc):
    #         total = 0
    #         for k in range(n):
    #             total += m1[i][k] * m2[k][j]
    #         m3[i][j] = total
    # return m3

m1 = [[1, 2, 3, 4],
      [5, 6, 7, 8]]
m2 = [[ 1,  2,  3],
      [ 4,  5,  6],
      [ 7,  8,  9],
      [10, 11, 12]]
print(product(m1, m2))          # [[70, 80, 90], [158, 184, 210]]

# Partir em peças, como sempre:
#   a dot faz a conta de UMA posição (linha vezes coluna);
#   a product só tem de a chamar para cada (i, j).
# E as funções row e col do guião finalmente servem para alguma coisa!
#
# O tamanho do resultado: linhas da 1ª e colunas da 2ª (2x4 vezes 4x3 dá 2x3).
# E a precondição: as colunas da 1ª têm de ser tantas como as linhas da 2ª,
# senão a linha e a coluna não têm o mesmo comprimento.






# %%
"""
i) EXERCICIO 48 (GUIÃO) - o main com os 5 testes [2 min -> 12:53] -----------

    Precisa das funções das células anteriores (corram-nas primeiro).
"""

def main() -> None:
    m1 = [ [1, 2, 3, 4],
           [5, 6, 7, 8] ]

    m2 = [ [ 1,  2,  3],
           [ 4,  5,  6],
           [ 7,  8,  9],
           [10, 11, 12] ]

    m3 = [ [1,  2,  3],
           [4,  5,  6],
           [7,  8,  9] ]

    s = input("TESTE: ")
    if s == "1":
        max = maximum(m1)
        print(max)
    elif s == "2":
        m = multiply(m1, 5)
        print(m)
    elif s == "3":
        t = diagonals(m3)
        print(t)
    elif s == "4":
        m = transpose(m1)
        print(m)
    elif s == "5":
        m = product(m1, m2)
        print(m)
    else:
        print("No test")

main()

# TESTE: 1 -> 8
# TESTE: 2 -> [[5, 10, 15, 20], [25, 30, 35, 40]]
# TESTE: 3 -> ([1, 5, 9], [3, 5, 7])
# TESTE: 4 -> [[1, 5], [2, 6], [3, 7], [4, 8]]
# TESTE: 5 -> [[70, 80, 90], [158, 184, 210]]
#
# Para o Mooshak (R): juntar num só ficheiro as ferramentas, as 5 funções e este main.








# %%
"""
j) Matrizes por dentro: porque é que o create é assim? [5 min -> 12:58] -----

    Uma forma "mais curta" de criar uma matriz 3x3 de zeros.
    Antes de correr: o que escreve o último print?
"""

m = [[0] * 3] * 3
print(m)                        # [[0, 0, 0], [0, 0, 0], [0, 0, 0]]   parece bem...

m[0][0] = 7
print(m)                        # [[7, 0, 0], [7, 0, 0], [7, 0, 0]]   !!!

m = create(3, 3, 0)
m[0][0] = 7
print(m)                        # [[7, 0, 0], [0, 0, 0], [0, 0, 0]]   assim sim

# [[0] * 3] * 3 NÃO faz 3 linhas. Faz UMA linha e 3 referências para ela.
# Mudar a "linha 0" é mudar a única linha que existe: aparece 3 vezes.
#
# É a mesma ideia das revisões 5 (valores e referências):
#   b = a, com listas, não copia: a e b são a mesma lista.
#
# O create cria uma linha NOVA em cada volta: m[i] = [value]*nc.
# 3 voltas, 3 listas diferentes.
#
# (o [value]*nc dentro da linha não faz mal: os valores são números,
#  e um número não se altera, só se troca por outro)















# %%
"""
===========================================================================
CONSOLIDAR - para casa
===========================================================================

    Tentem primeiro sem olhar para a solução.
    Os primeiros 6 são de treino; os últimos 3 são perguntas de testes a sério.
"""






# %%
"""
EXERCICIO (EXTRA) 1 - Somar os algarismos (while) ----------------------------

    sum_digits(1234) == 10
    sum_digits(7) == 7
    sum_digits(0) == 0

    Pista: n % 10 dá o último algarismo; n // 10 tira-o.
"""

def sum_digits(n: int) -> int:
    """ Sum of the digits of n.
        Precondition: n >= 0
    """
    pass        # TODO

print(sum_digits(1234), sum_digits(7), sum_digits(0))











# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: somar os algarismos

def sum_digits(n: int) -> int:
    """ Sum of the digits of n.
        Precondition: n >= 0
    """
    total = 0
    while n > 0:
        total += n % 10         # o último algarismo
        n //= 10                # e tirá-lo
    return total

print(sum_digits(1234), sum_digits(7), sum_digits(0))     # 10 7 0

# Não se sabe quantos algarismos tem o n: while.
# O avanço é o n //= 10: o n encolhe até chegar a 0.
# Com n = 0 dá zero voltas e devolve 0, que aqui está certo.






# %%
"""
EXERCICIO (EXTRA) 2 - A sequência de Collatz (while) -------------------------

    Começa-se num n > 0 e repete-se:
        se n for par,   n passa a n // 2
        se n for ímpar, n passa a 3 * n + 1
    até chegar a 1. Quantos passos?

    collatz_steps(6) == 8       (6, 3, 10, 5, 16, 8, 4, 2, 1)
    collatz_steps(1) == 0
    collatz_steps(27) == 111

    (curiosidade: ninguém conseguiu ainda provar que chega SEMPRE a 1.
     É um problema famoso da matemática, em aberto há quase 100 anos.)
"""

def collatz_steps(n: int) -> int:
    """ Number of Collatz steps from n down to 1.
        Precondition: n > 0
    """
    pass        # TODO

print(collatz_steps(6), collatz_steps(1), collatz_steps(27))














# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: a sequência de Collatz

def collatz_steps(n: int) -> int:
    """ Number of Collatz steps from n down to 1.
        Precondition: n > 0
    """
    steps = 0
    while n != 1:
        if n % 2 == 0:
            n = n // 2
        else:
            n = 3 * n + 1
        steps += 1
    return steps

print(collatz_steps(6), collatz_steps(1), collatz_steps(27))   # 8 0 111

# O while por excelência: o n sobe e desce, e não há fórmula
# para saber quantas voltas vão ser. Só se sabe quando parar.






# %%
"""
EXERCICIO (EXTRA) 3 - Média de notas, com validação (while) --------------------

    a) read_grade(): pede uma nota até ela estar entre 0 e 20.
           Nota: 25
           Nota: -1
           Nota: 14.5        -> devolve 14.5
    b) average_grades(): enquanto o utilizador responder 's' a "Mais uma nota (s/n)? ",
       lê uma nota com a read_grade. No fim devolve a média.
       Se não houver notas, devolve 0.0.

    Pista: a pergunta s/n é a ask_yes_no da aula (está aqui outra vez).
"""

def ask_yes_no() -> str:
    """ Read input until "s" or "n". """
    while True:
        reply = input("Mais uma nota (s/n)? ")
        if reply == 's' or reply == 'n':
            return reply

def read_grade() -> float:
    """ Read a grade until it is between 0 and 20. """
    pass        # TODO

def average_grades() -> float:
    """ Average of the grades read, while the user wants more. """
    pass        # TODO

print(average_grades())












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: média de notas, com validação

def ask_yes_no() -> str:
    """ Read input until "s" or "n". """
    while True:
        reply = input("Mais uma nota (s/n)? ")
        if reply == 's' or reply == 'n':
            return reply

def read_grade() -> float:
    """ Read a grade until it is between 0 and 20. """
    while True:
        grade = float(input("Nota: "))
        if 0 <= grade <= 20:
            return grade

def average_grades() -> float:
    """ Average of the grades read, while the user wants more. """
    total = 0.0
    count = 0
    while ask_yes_no() == 's':
        total += read_grade()
        count += 1
    if count == 0:
        return 0.0
    return total / count

print(average_grades())         # com s, 25, 14.5, s, 17, n dá 15.75 (o 25 é pedido outra vez)

# Três ciclos while a trabalhar juntos:
#   o da ask_yes_no repete a pergunta até a resposta ser s ou n;
#   o da read_grade repete até a nota ser válida;
#   o da average_grades continua enquanto a resposta for 's'.
# É a mesma estrutura do ex 47 (ask + play_hilo_reverse).
#
# Porquê não usar 0 como marca de fim (como o -1 da aula)?
# Porque 0 é uma nota válida: quem tirou 0 nunca a conseguia introduzir.
#
# O caso "sem notas": sem o if, dava 0.0 / 0 -> ZeroDivisionError.






# %%
"""
EXERCICIO (EXTRA) 4 - Somar cada linha (matrizes) ----------------------------

    Uma lista com a soma de cada linha.
        row_sums([[1, 2, 3], [4, 5, 6]]) == [6, 15]
        row_sums([]) == []
"""

def row_sums(m: list[list[int]]) -> list[int]:
    """ The sum of each row of m. """
    pass        # TODO

print(row_sums([[1, 2, 3], [4, 5, 6]]), row_sums([]))












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: somar cada linha

def row_sums(m: list[list[int]]) -> list[int]:
    """ The sum of each row of m. """
    sums = []
    for row in m:
        total = 0
        for v in row:
            total += v
        sums.append(total)
    return sums

print(row_sums([[1, 2, 3], [4, 5, 6]]), row_sums([]))     # [6, 15] []

# Pelos valores chega: só lemos.
# O "total = 0" fica DENTRO do for de fora: recomeça em cada linha.
# (é o mesmo cuidado do "j = 0" do ex 45)






# %%
"""
EXERCICIO (EXTRA) 5 - Matriz simétrica (matrizes) ----------------------------

    Uma matriz quadrada é simétrica se m[i][j] == m[j][i] em todas as posições
    (fica igual à sua transposta).
        is_symmetric([[1, 2], [2, 1]]) == True
        is_symmetric([[1, 2], [3, 1]]) == False
        is_symmetric([[1, 7, 3], [7, 4, 5], [3, 5, 6]]) == True

    Pista: é uma pesquisa de um contra-exemplo (aula 5).
"""

def is_symmetric(m: list[list[int]]) -> bool:
    """ Is the square matrix m symmetric?
        Precondition: m is square
    """
    pass        # TODO

print(is_symmetric([[1, 2], [2, 1]]), is_symmetric([[1, 2], [3, 1]]),
      is_symmetric([[1, 7, 3], [7, 4, 5], [3, 5, 6]]))












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: matriz simétrica

def is_symmetric(m: list[list[int]]) -> bool:
    """ Is the square matrix m symmetric?
        Precondition: m is square
    """
    n = len(m)
    for i in range(n):
        for j in range(n):
            if m[i][j] != m[j][i]:
                return False        # um contra-exemplo chega
    return True

print(is_symmetric([[1, 2], [2, 1]]), is_symmetric([[1, 2], [3, 1]]),
      is_symmetric([[1, 7, 3], [7, 4, 5], [3, 5, 6]]))    # True False True

# A receita do contra-exemplo (aula 5), agora com dois for:
#   ao primeiro par que não bate, return False;
#   se os ciclos acabarem sem encontrar nenhum, return True.
#
# Melhoria: cada par é testado duas vezes ((0,1) e (1,0)).
# Com for j in range(i + 1, n) só se testa acima da diagonal.






# %%
"""
EXERCICIO (EXTRA) 6 - Matriz identidade (matrizes) ---------------------------

    A matriz n x n com 1 na diagonal principal e 0 no resto.
        identity(3) == [[1, 0, 0], [0, 1, 0], [0, 0, 1]]

    Usem a create do guião (está aqui outra vez, para a célula correr sozinha).
"""

def create(nr: int, nc: int, value: any) -> list[list]:
    """ Create and initialize a matrix with nr rows and nc columns.
        Precondition: nr >= 0 and nc >= 0
    """
    m = [None]*nr
    for i in range(nr):
        m[i] = [value]*nc
    return m

def identity(n: int) -> list[list[int]]:
    """ The identity matrix n x n.
        Precondition: n >= 0
    """
    pass        # TODO

print(identity(3))












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: matriz identidade

def create(nr: int, nc: int, value: any) -> list[list]:
    """ Create and initialize a matrix with nr rows and nc columns.
        Precondition: nr >= 0 and nc >= 0
    """
    m = [None]*nr
    for i in range(nr):
        m[i] = [value]*nc
    return m

def identity(n: int) -> list[list[int]]:
    """ The identity matrix n x n.
        Precondition: n >= 0
    """
    m = create(n, n, 0)
    for i in range(n):
        m[i][i] = 1
    return m

print(identity(3))              # [[1, 0, 0], [0, 1, 0], [0, 0, 1]]

# Criar tudo a 0 e depois só mudar a diagonal (j == i).
# Não é preciso o for j: na diagonal, a coluna já se sabe.
#
# (multiplicar uma matriz pela identidade, com a product, devolve a mesma
#  matriz: é o "1" das matrizes. Experimentem!)






# %%
"""
EXERCICIO (EXTRA) 7 - Teste 1 2025/26, pergunta 4 - get_month com while ------

    Já resolvemos este na aula 3, com for. Agora com while.

    Dado o número de ordem de um dia no ano, qual o mês?
        get_month(5, False) == 1        # janeiro
        get_month(31, False) == 1       # janeiro
        get_month(60, True) == 2        # fevereiro
        get_month(60, False) == 3       # março
        get_month(366, True) == 12      # dezembro

    Ideia: enquanto o dia não couber no mês atual, gasta-se esse mês e avança-se.
"""

def get_month(order: int, leap_year: bool) -> int:
    """ Calculate the month corresponding to some day order in a year
        Precondition: 1 <= order <= 366 """
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    pass        # TODO

print(get_month(5, False), get_month(31, False), get_month(60, True),
      get_month(60, False), get_month(366, True))












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: get_month com while

def get_month(order: int, leap_year: bool) -> int:
    """ Calculate the month corresponding to some day order in a year
        Precondition: 1 <= order <= 366 """
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    month = 1
    days = DURATIONS[0]
    while order > days:                 # ainda não cabe neste mês
        order -= days                   # gastar o mês
        month += 1                      # e passar ao seguinte
        days = DURATIONS[month - 1]
        if month == 2 and leap_year:
            days += 1
    return month

print(get_month(5, False), get_month(31, False), get_month(60, True),
      get_month(60, False), get_month(366, True))       # 1 1 2 3 12

# Comparem com a versão for da aula 3:
#   lá, o for ia de 1 a 12 e saía com return a meio;
#   aqui, a condição de paragem ("o dia já cabe") está na linha do while.
# As duas valem a nota toda. Escolham a que vos parecer mais clara.






# %%
"""
EXERCICIO (EXTRA) 8 - Teste 2 2025/26, pergunta 2 - a n-ésima ocorrência ------

    O índice da n-ésima ocorrência de um caráter numa string.
    Se não existir, o resultado é -1. (não usar o método index das strings)
        index("trelharlherah", 'h', 1) == 4     # primeira ocorrência
        index("trelharlherah", 'h', 3) == 12    # terceira ocorrência
        index("trelharlherah", 'h', 4) == -1    # quarta ocorrência

    (no teste: "se a função funcionar para n = 1, em princípio já terá 50% da nota")
"""

def index(s: str, c: str, n: int) -> int:
    """ Index of nth occurrence of char in string. """
    pass        # TODO

print(index("trelharlherah", 'h', 1), index("trelharlherah", 'h', 3),
      index("trelharlherah", 'h', 4))












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: a n-ésima ocorrência

def index(s: str, c: str, n: int) -> int:
    """ Index of nth occurrence of char in string. """
    count = 0
    for i in range(len(s)):
        if s[i] == c:
            count += 1
            if count == n:
                return i            # é esta: sair já
    return -1                       # a string acabou antes da n-ésima

print(index("trelharlherah", 'h', 1), index("trelharlherah", 'h', 3),
      index("trelharlherah", 'h', 4))       # 4 12 -1

# Percorrer uma string -> for (pelos índices, porque o resultado é um índice).
# Um contador de ocorrências e um return a meio quando chega a n.
# Se o for acabar sem return, é porque não há n ocorrências: -1.






# %%
"""
EXERCICIO (EXTRA) 9 - Exame de recurso 2023/24, pergunta 4 - matriz esparsa ----

    Uma matriz quase toda com zeros desperdiça memória.
    Melhor guardar num dicionário (teórica 6b) só as posições que não são 0:

        m = [[5,0,0],        ->     d = { (0, 0): 5,
             [0,0,0],                     (2, 2): 9 }
             [0,0,9]]

    a) compress(m): da matriz para o dicionário.
    b) expand(d, nr, nc): do dicionário de volta para a matriz.
    (no exame podiam usar create, nrows e ncols)
"""

def create(nr: int, nc: int, value: any) -> list[list]:
    """ Create and initialize a matrix with nr rows and nc columns.
        Precondition: nr >= 0 and nc >= 0
    """
    m = [None]*nr
    for i in range(nr):
        m[i] = [value]*nc
    return m

def compress(m: list[list[int]]) -> dict[tuple[int,int], int]:
    """ Dictionary with the non zero values of m, by position. """
    pass        # TODO

def expand(d: dict[tuple[int,int], int], nr: int, nc: int) -> list[list[int]]:
    """ Matrix nr x nc rebuilt from the dictionary d. """
    pass        # TODO

m = [[5,0,0],
     [0,0,0],
     [0,0,9]]
print(compress(m))
print(expand(compress(m), 3, 3))












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================

















# %%
# Solução: matriz esparsa

def create(nr: int, nc: int, value: any) -> list[list]:
    """ Create and initialize a matrix with nr rows and nc columns.
        Precondition: nr >= 0 and nc >= 0
    """
    m = [None]*nr
    for i in range(nr):
        m[i] = [value]*nc
    return m

def compress(m: list[list[int]]) -> dict[tuple[int,int], int]:
    """ Dictionary with the non zero values of m, by position. """
    nr, nc = len(m), len(m[0])
    d = dict()
    for i in range(nr):
        for j in range(nc):
            if m[i][j] != 0:
                d[(i, j)] = m[i][j]     # a chave é o par (linha, coluna)
    return d

def expand(d: dict[tuple[int,int], int], nr: int, nc: int) -> list[list[int]]:
    """ Matrix nr x nc rebuilt from the dictionary d. """
    m = create(nr, nc, 0)               # tudo a zero...
    for (i, j) in d:                    # ... e repor só o que estava guardado
        m[i][j] = d[(i, j)]
    return m

m = [[5,0,0],
     [0,0,0],
     [0,0,9]]
print(compress(m))                      # {(0, 0): 5, (2, 2): 9}
print(expand(compress(m), 3, 3))        # [[5, 0, 0], [0, 0, 0], [0, 0, 9]]

# compress: percorrer pelos índices (a chave precisa da posição) e guardar só o que não é 0.
# expand: a receita da matriz nova (create) e depois escrever só as posições do dicionário.
#
# As chaves são tuplos (i, j): podem ser chave porque os tuplos não mudam (teórica 6b).
# (a matriz vazia, len(m[0]), dava erro: no exame podiam usar ncols, que trata disso)






# %%
"""
===========================================================================
PARA TREINAR MAIS (em casa)
===========================================================================

    while:
        os exercícios 45, 46 e 47 sem olhar para a solução
        (o 47 é o mais interessante: dois while e a pesquisa binária)

    Matrizes:
        o Mooshak R (ex 48)
        e o ex 9 de cima (exame de recurso 2023/24) é o tipo de pergunta que aparece

    Testes de exemplo:
        revisoes_6_teste_exemplo_1.py, _2.py e _3.py (para o Teste 1)
"""