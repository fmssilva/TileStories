# %% [Aula 1] Bem-vindo(a) à Programação em Python! 
"""
Este ficheiro é o teu resumo da primeira aula 
e uma base para preparar as próximas aulas.

A ideia é alterar este documento e experimentar diferentes operações para ver como as coisas funcionam e os outputs que obtemos 
Este ficheiro pode ser corrido como um ficheiro python normal, como um todo,
Ou pode também sere corrido por células. 

Se moveres o cursor vais reparar que diferentes blocos de código ficam iluminados no IDE
Isso são as células (blocos separados por # %%).

E é possível correr cada célula de forma independente. 

Para tal: 
    Tendo o cursor na célula pretendida, 
    fazer Ctrl + Enter
    e ver o resultado da célula na consola 
    
Experimenta já: 
    coloca o cursor dentro desta célula e faz Ctrl+Enter
    e vê o output na consola
"""
print("Output da 1º célula!")


# %% Comentário vs Código
"""
Uma linha que começa com # é um COMENTÁRIO: 
        o Python ignora-a por completo. 
        Serve para os humanos perceberem o código.

Tudo o resto é CÓDIGO: o Python lê, interpreta e executa.
"""
# Exemplo - isto é um comentário -> não faz nada quando corres a célula
print("Exemplo - isto é código -> aparece na consola")  # também podes comentar no fim da linha



# %% Indentação: a "gramática visual" do Python
"""
Nalgumas linguágens usamos chavetas para delimitar o código que pertence a uma função, if, ciclo, etc. 

Em Python usamos INDENTAÇÃO. 
Ou seja usamos avanços de 4 espaços para marcar o que está dentro de uma função etc. 
Na prática podemos clicar na tecla "tab" e o Spyder insere os 4 espaços autmaticamente. 

Exemplo de um bloco em que usamos o avanço para perceber o que está dentro do if e o que está dentro do else  
"""

idade = 20
if idade >= 18:
    print("És maior de idade")     # está "dentro" do if -> 4 espaços
else:
    print("És menor de idade")


# %% Tipos de dados básicos
"""
Todo o dado em Python tem um tipo. 
Os 5 tipos básicos: 
    
    int    -> números inteiros            ex: 3, -10, 2024
    float  -> números com casas decimais  ex: 3.14, -0.5
    str    -> texto (sempre entre aspas)  ex: "olá", 'Python'
    bool   -> verdadeiro ou falso         ex: True, False
    None   -> "não há valor"
"""

# TODO: melhorar e completar isto? meter vários exemplos de cada tipo de dados, sobretudo para bool? ou não vale a pena ainda mostrar que 0, [], "" , False é tudo false e resto é True??
idade = 20              # int
altura = 1.75           # float
nome = "Ana"             # str
esta_a_chover = False    # bool
resultado = None         # None

print(type(idade))
print(type(altura))
print(type(nome))
print(type(esta_a_chover))
print(type(resultado))

# TODO: meter aqui um bloco a falar de cast?
a = 1
print (type(str(a))) ??? é bom explicar isto e dar exemplos simples e fáceis de perceber??
    
# %% Operadores aritméticos
a = 7
b = 2

print("soma:", a + b)
print("subtração:", a - b)
print("multiplicação:", a * b)
print("divisão:", a / b)          # devolve sempre float
print("divisão inteira:", a // b)
print("resto:", a % b)
print("potência:", a ** b)

# TODO - é bom dar alguns exemplos para mostrar que python é tipagem forte e não inventa como noutras linguagens, ou seja cada tipo de dados aceita diferentes tipos de operações e alguns podemos misturar, exemplo int e float vira float... enquanto int e str da erro..???? só depois vi a proxima celula... talvz melhorar e completar e dar mais uns exemplos??
# %% Curiosidade: Python é "fortemente tipado"
"""
Ao contrário de algumas linguagens (como JavaScript), o Python NÃO
converte tipos "por magia". Se tentares somar um número com texto,
dá erro -- de propósito, para evitar bugs escondidos.
"""

# Seleciona a linha seguinte, faz Ctrl+1 para descomentar, e corre a célula:
# print(5 + "10")

# A forma correta é converter explicitamente:
print(5 + int("10"))
print(str(5) + "10")

# %% Operadores de comparação e lógicos
"""
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





# %% Próxima aula - Pequeno Spoiler!!
"""
Hoje aprendeste os "ingredientes": tipos de dados e operadores.
Na próxima aula vamos aprender a construir um programa com lógica específica com eles. 

Vamos aprender: 
"""
    
# if / else        -> tomar decisões e fazer uma opção ou outra
a = 2
if a % 2 == 0:
    print("a é par")
else: 
    print("a é impar")
    
# ciclos (for/while) -> repetir uma tarefa 1000 vezes... 
for numero in range(0, 5):
    print("numero = ", numero) # sem o ciclo iamos ter de escrever "print(0); print(1); etc linha a linha"
    
# funções           -> escrever um bloco de código com determinada lógica e depois usar chamar e usar esse bloco de código quando quisermos. 

def soma(a,b): # função muito complexa que soma 2 numeros dados como input (argumentos)
    return a + b

result = soma (2, 2)
print(result)
print(soma(4,5))
print(soma(6,5))

# recursividade     -> uma função pode chamar outras funções, e pode mesmo chamar-se a si mesmo, criando uma espécie de ciclo com um mecanismo diferente
def funcao_recursiva(numero):
    if numero < 0:
        break
    print(numero)
    funcao_recursiva(numero-1)
  
funcao_recursiva(5) 



# %% Atalhos úteis no Spyder
"""
Ao longo do tempo vai ajudar saber alguns atalhos para facilitar o trabalho. 

Não precisas de decorar tudo, mas estes poupam-te muito tempo:

    Ctrl + Enter   -> correr a célula atual
    Ctrl + 1       -> comentar / descomentar a linha selecionada
    Ctrl + s       -> guardar o ficheiro
    Ctrl + "cursor em cima de uma função/variável..." -> ver uma explicação sobre como usar, etc
    Ctrl + "clicar em cima de uma função/variável..." -> ir para a declaração da função, variavel, etc. 
    Tab             -> adicionar 4 espaços para dar avanço identar
    shift + tab para remover 
    shift + arrow up/down/right/left  -> selecionar código/comentarios
    alt + arrow up/down -> mover a linha ou bloco de código/comentarios para cima/baixo
    
"""
pass

