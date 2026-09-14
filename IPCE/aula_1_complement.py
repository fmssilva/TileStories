# %%
print("a")


# %%
print("b")


# %% 
"""
---------------------------------------------------------------------------
----- [Aula 1] Bem-vindo(a) à Programação em Python! 
---------------------------------------------------------------------------

Este ficheiro é o teu resumo da primeira aula
e uma base para preparares as próximas aulas.

A ideia é alterares este documento e experimentares diferentes
operações para ver como as coisas funcionam e que outputs obténs.

Este ficheiro pode ser corrido como um ficheiro Python normal, todo de uma vez,
ou pode também ser corrido por células.

Se moveres o cursor vais reparar que diferentes blocos de código ficam
iluminados no IDE. Isso são as células (blocos separados por # %%).

É possível correr cada célula de forma independente. Para tal:
    Com o cursor dentro da célula pretendida,
    faz Ctrl + Enter
    e vê o resultado da célula na consola.

Experimenta já:
    coloca o cursor dentro desta célula e faz Ctrl+Enter
    e vê o output na consola deste print abaixo:
"""
print("Output da 1ª célula!")


# %% 
"""
---------------------------------------------------------------------------
---- Comentário vs Código 
---------------------------------------------------------------------------

Uma linha que começa com # é um COMENTÁRIO:
    o Python ignora-a por completo.
    Serve para os humanos perceberem o código.

Tudo o resto é CÓDIGO: o Python lê, interpreta e executa.
"""
# Exemplo - isto é um comentário -> não faz nada quando corres a célula
print("Exemplo - isto é código -> aparece na consola")  # também podes comentar no fim da linha


# %% 
"""
---------------------------------------------------------------------------
---- Indentação: a "gramática visual" do Python 
---------------------------------------------------------------------------

Nalgumas linguagens usamos chavetas { } para delimitar o código que
pertence a uma função, ciclo, if, etc.

Em Python usamos INDENTAÇÃO: avanços de 4 espaços para marcar o que
está "dentro" de uma função, de um if, de um ciclo, etc.
Na prática, basta carregar em "Tab" e o Spyder insere os 4 espaços automaticamente.

Exemplo: usamos o avanço para perceber o que está dentro do if e o que
está dentro do else.
"""

idade = 20
if idade >= 18:
    print("És maior de idade")     # está "dentro" do if -> 4 espaços
else:
    print("És menor de idade")


# %% 
"""
---------------------------------------------------------------------------
---- Tipos de dados básicos
---------------------------------------------------------------------------

Todo o dado em Python tem um tipo.
Os 5 tipos básicos:

    int    -> números inteiros            ex: 3, -10, 2024
    float  -> números com casas decimais  ex: 3.14, -0.5
    str    -> texto (sempre entre aspas)  ex: "olá", 'Python'
    bool   -> verdadeiro ou falso         ex: True, False
    None   -> "não há valor"
"""

idade = 20                # int
altura = 1.75             # float
nome = "Ana"              # str
esta_a_chover = False     # bool
resultado = None          # None

print(idade, type(idade))
print(altura, type(altura))
print(nome, type(nome))
print(esta_a_chover, type(esta_a_chover))
print(resultado, type(resultado))


# %% 
"""
---------------------------------------------------------------------------
---- Converter entre tipos (cast)
---------------------------------------------------------------------------

Às vezes temos um valor num tipo e precisamos dele noutro tipo.
A isso chama-se fazer um CAST (conversão). Fazemos isso "envolvendo"
o valor com o nome do tipo que queremos: int(...), float(...), str(...).
"""

# int para string: 
a = 1                   # int 
print(a, type(a))
b = str(a)              # "1"  (agora é str)
print(b, type(b))       
print()

# string para int: 
texto_numero = "25"     # string
print(texto_numero, type(texto_numero))
c = int(texto_numero)   # 25   (agora é int, já dá para somar/multiplicar)
print(c, type(c))       
print()

# string para float 
texto_decimal = "3.5"
print(texto_decimal, type(texto_decimal))
d = float(texto_decimal)
print(d, type(d))       
print()

# float para int (repara que a parte decimal é truncada - não arredonda)
num_float = 3.9
print(num_float, type(num_float))
e = int(num_float)
print(e, type(e))

# Algumas conversão não são exequíveis e dão erro
# exemplo isto vai dar erro (remove o comentário e corre para veres): 
# int("vinte")


# %% 
"""
---------------------------------------------------------------------------
---- Input e Output: print() e input()
---------------------------------------------------------------------------

Estas duas funções são a forma mais simples de "comunicar com o programa"

    print(...)  -> ESCREVE algo no ecrã            (OUTPUT)
    input(...)  -> LÊ o que a pessoa escreve        (INPUT)

Com input() podemos passar uma mensagem prompt para mostrar no terminal 
e assim o user saber o que tem de escrever

Com print() podemos usar f"...{variavel}" - ou seja, colocando um f antes da string
permite depois colocar variaveis no interior da string entre chavetas
"""

nome = input("Como te chamas? - escreve aqui o teu nome:")
# Aqui repara na consola e escreve lá o teu nome
print("Olá, " + nome + "!")

# Important: input() devolve sempre texto (str)
# Se queremos obter "números" temos que converter, exemplo para int: 
idade = int(input("Que idade tens?"))

# repara como usamos as variaveis entre chavetas no meio da string, 
# ao usar f"" no inicio da string 
print(f"O {nome} tem {idade} anos")



# %% 
"""
---------------------------------------------------------------------------
---- Operadores aritméticos
---------------------------------------------------------------------------

Estes operadores funcionam como esperado com numeros (int ou float)

E podemos misturar à vontade numeros int com float em operações:
"""

# Quando usamos um float e um int - as operações devolvem sempre um float
num_float = 2.5
num_int = 2

print("soma:", num_float + num_int)
print("subtração:", num_float - num_int)
print("multiplicação:", num_float * num_int)
print("divisão:", num_float / num_int)          # devolve sempre float
print("divisão inteira:", num_float // num_int)
print("resto:", num_float % num_int)
print("potência:", num_float ** num_int)


# Quando fazemos operações com int, o resultado é sempre int
# EXCETO na divisão que devolve float
print (3/2)


# %% 
"""
---------------------------------------------------------------------------
---- Operadores com String
---------------------------------------------------------------------------

"""

# Com strings o "+" significa concatenar strings
print("string a ..." + "...string b")

# Não podemos misturar em operações string com int/float
# Python não tenta adivinhar que número a string representa
# Ou seja - Python é FORTEMENTE TIPADO
# Exemplo se correres a linha seguinte vai dar erro (remove o comentario para veres): 
# print(5 + "10")   # TypeError: unsupported operand type(s) for +: 'int' and 'str'

# A forma correta é converter explicitamente, para dizeres tu o que queres:
print(5 + int("10"))    # queremos somar -> converte o texto para número: 15
print(str(5) + "10")    # queremos concatenar -> converte o número para texto: "510"

# Algumas linguagens são de tipagem fraca (Exemplo JavaScript)
# Nessa linguagem podemos fazer "5"-2 e obtemos 3; "5"*2 e obtemos 10... 
# Mas se fizerms "5"+2 a linguagem interpreta como string e obtemos "52"


# %% 
"""
---------------------------------------------------------------------------
---- Operadores de comparação e lógicos
---------------------------------------------------------------------------

Comparação -> devolvem sempre True ou False (bool)
    ==  igual            !=  diferente
    >   maior            <   menor
    >=  maior ou igual   <=  menor ou igual

Lógicos -> combinam condições
    and  -> as duas têm de ser verdadeiras
    or   -> pelo menos uma tem de ser verdadeira
    not  -> inverte o valor
"""

idade = 20
tem_bilhete = True

print(idade >= 18)
print(idade >= 18 and tem_bilhete)
print(idade < 18 or tem_bilhete)
print(not tem_bilhete)


# %% 
"""
---------------------------------------------------------------------------
---- Próxima aula - Pequeno Spoiler!!
---------------------------------------------------------------------------

Hoje aprendeste os "ingredientes": tipos de dados e operadores.
Na próxima aula vamos aprender a combiná-los com lógica para construir
um programa a sério. 

Vamos aprender:
"""

# if / else        -> tomar decisões e seguir um caminho ou outro
a = 2
if a % 2 == 0:
    print("a é par")
else:
    print("a é ímpar")

# ciclos (for/while) -> repetir uma tarefa muitas vezes
for numero in range(5):
    print("numero =", numero)  # sem o ciclo tínhamos de escrever "print(0); print(1); ..." linha a linha

# funções -> escrever um bloco de código com uma lógica e depois
#            poder chamá-lo (reutilizá-lo) sempre que precisarmos
def soma(a, b):  # função simples que soma 2 números dados como input (argumentos)
    return a + b

result = soma(2, 2)
print(result)
print(soma(4, 5))
print(soma(6, 5))

# recursividade -> uma função pode chamar-se a si mesma, criando uma
#                  espécie de ciclo, mas com um mecanismo diferente
def funcao_recursiva(numero):
    if numero < 0:      # condição de paragem -- sem isto, corria para sempre!
        return
    print(numero)
    funcao_recursiva(numero - 1)

funcao_recursiva(5)
# repara que aqui vamos fazer print de valores 0-5, 
# mas ao contrário porque começamos com 5 e vamos decrescendo até 0


# %% 
"""
---------------------------------------------------------------------------
---- Atalhos úteis no Spyder
---------------------------------------------------------------------------

Ao longo do tempo vai ajudar saber alguns atalhos para facilitar o trabalho.

Não precisas de decorar tudo já, mas estes poupam-te muito tempo:

    Ctrl + Enter              -> correr a célula atual
    Ctrl + 1                  -> comentar / descomentar a linha selecionada
    Ctrl + S                  -> guardar o ficheiro
    passar o rato por cima de uma função/variável -> ver uma dica rápida
    Ctrl + clique numa função/variável            -> ir para a sua declaração
    Tab                        -> indentar (avançar 4 espaços)
    Shift + Tab                -> remover indentação
    Shift + seta (cima/baixo/esq/dir) -> selecionar código/texto
    Alt + seta (cima/baixo)    -> mover a linha (ou bloco selecionado) para cima/baixo
"""
pass