# %%
"""
===========================================================================
GUIA DE SESSÃO - Prática 09 - IPCE 2026/2027
===========================================================================

Cobre os guiões 09a (ex. 52 a 55) e 09b (ex. 56, 57 e 59).
O ex. 58 fica no CONSOLIDAR, no fim.

Site da Cadeira: https://ipce-184ea7.gitlab.io/
Este Guião na drive: https://drive.google.com/drive/folders/1twlkB4M0yg0tBxOt2c1diqJU6xtUsoay?usp=sharing


Tema de hoje: as coleções que faltavam (SETS e DICIONÁRIOS) e os FICHEIROS.

    1ª parte: sets e dicionários.
              Exercícios 52 e 53, e o atlas (ex 54).
    2ª parte: strings vs listas e o Jogo da Forca (ex 55).
              O quadro das 5 coleções: qual usar quando.
              Ficheiros: ler, escrever e o next.
              A Alice (ex 56), os jogadores (ex 57), a pastelaria (ex 59).

Ficheiros de dados para hoje: estão no guião 09b, no site da cadeira.
    alice.txt, jogadores.txt, pastelaria.txt e atlas.txt
    Guardem-nos na MESMA pasta deste ficheiro .py
    (botão direito no link -> "Save link as...")

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

    - Baixar este guia da drive, e os 4 ficheiros .txt do guião 09b no site,
      todos para a mesma pasta

    - Dúvidas da aula passada (data classes, frações, pastelaria)
"""






# %%
"""
===========================================================================
Guião 09a, exercícios 52 a 54 - Sets e dicionários [62 min -> 11:20]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
a) Sets: a sintaxe e as 2 diferenças para as listas [8 min -> 10:26] -------

    Um set (conjunto) usa-se quase como uma lista: len(), in, for.
    Mas tem 2 diferenças:
        1. NÃO tem repetidos:  cada valor está lá no máximo uma vez
        2. NÃO tem ordem:      não há "primeiro" nem "último", logo não há s[0]

    Lista: [ ]       Set: { }
"""

# Criar
l = ["azul", "verde", "azul", "rosa"]       # lista: guarda o azul 2 vezes
s = {"azul", "verde", "azul", "rosa"}       # set:   o azul repetido desaparece

print(len(l))                   # 4
print(len(s))                   # 3
print(l)                        # ['azul', 'verde', 'azul', 'rosa']
print(s)                        # {'verde', 'rosa', 'azul'}   (a ordem pode variar!)

# Corram esta célula 2 ou 3 vezes (ou reiniciem a consola) e vejam o print(s):
# a ordem pode mudar. É a diferença 2: um set não guarda ordem.


#%%
# Converter uma lista (ou string) num set: set(...). Os repetidos desaparecem.
print(set([3, 1, 3, 2, 1]))     # {1, 2, 3}   (com números pequenos costuma sair por ordem, por acaso)
print(len(set("banana")))       # 3           b, a, n

print(list(set("banana"))[0])     # »» não sabemos a ordem que vai ser

# print(s[0])
# TypeError: 'set' object is not subscriptable      sem ordem, não há índices



#%%
# O set vazio é set(). Atenção: { } vazio NÃO é um set, é um dicionário (secção e).
vazio = set()
print(len(vazio))               # 0
print(type(vazio))                 # <class 'set'>
print(type({}))                 # <class 'dict'>






# %%
"""
    O que é igual às listas: len(), in, not in, for
    O que é diferente: add (não append), tirar é discard
"""

s = {"azul", "verde", "azul", "rosa"}       # set:   o azul repetido desaparece
print(s)

print("azul" in s)              # True
print("preto" not in s)         # True

# Mais uma vantagem: num set, o "in" é quase imediato, mesmo com um milhão de valores.
# Numa lista, o "in" procura um a um, do início ao fim.


#%%
for cor in s:
    print(cor)                  # verde, rosa e azul, um por linha (a ordem pode variar)

#%%
s.add("preto")                  # o append das listas
s.add("azul")                   # já lá estava: não acontece nada
print(s)

s.discard("verde")              # tirar
s.discard("roxo")               # não estava lá: não acontece nada, nem dá erro
print(s)







# %%
"""
b) Sets para ENCONTRAR DUPLICADOS [6 min -> 10:32]

    Uma lista tem algum valor repetido?
"""

# Podemos correr a lista e confirmar: 
def has_repeated_list(l: list[int]) -> bool:
    """ Has the list l any repeated value? (with lists only) """
    seen = []
    for x in l:
        if x in seen:           # já tinha aparecido: é repetido
            return True
        seen.append(x)
    return False

print(has_repeated_list([3, 1, 4, 1, 5]))      # True
print(has_repeated_list([3, 1, 4]))            # False


# Ou podemos converter para set e comparar os tamanhos: se havia repetidos, o set fica mais pequeno.
def has_repeated(l: list[int]) -> bool:
    """ Has the list l any repeated value? """
    return len(set(l)) != len(l)    # se havia repetidos, o set fica mais pequeno

print(has_repeated([3, 1, 4, 1, 5]))           # True
print(has_repeated([3, 1, 4]))                 # False

# 6 linhas contra 1. E a de 1 linha diz exatamente a ideia:
# "tirando os repetidos, o tamanho muda?"






# %%
"""
c) Os outros usos típicos dos sets [8 min -> 10:40] ------------------------

    1. CONTAR VALORES ÚNICOS (distintos): len(set(...))
"""

# Quantos valores diferentes tem a lista? 
def count_unique(l: list[int]) -> int:
    """ How many distinct values in l? """
    return len(set(l))

print(count_unique([1, 5, 3, 5, 1, 1]))        # 3
print(count_unique([]))                         # 0

# E quantas letras diferentes tem a palavra?
print(len(set("mississippi")))                  # 4     m, i, s, p


#%%
"""
    2. TIRAR OS REPETIDOS de uma lista: set(...)
       (mas perde-se a ordem: se a ordem importa, faz-se com lista)
"""

l = [5,5,2,2,1]
l_unique = list(set(l)) # MAS A ORDEM PODE TROCAR!!
print(l_unique)





# %%
"""
    3. INTERSEÇÃO, UNIÃO e DIFERENÇA: as operações dos conjuntos da matemática

        a & b     interseção: o que está nos DOIS         (o que há em comum)
        a | b     união:      o que está num OU no outro  (juntar sem repetir)
        a - b     diferença:  o que está no a, mas NÃO no b
        a == b    os mesmos valores? (a ordem e as repetições não contam)
"""

a = {1, 2, 3, 4}
b = {3, 4, 5}

print(a & b)                    # {3, 4}
print(a | b)                    # {1, 2, 3, 4, 5}
print(a - b)                    # {1, 2}
print(b - a)                    # {5}

#%%
print({1, 2, 3} == {3, 2, 1, 3})    # True      nos sets a ordem e as repetições não contam

print([1, 2, 3] == [3, 2, 1])       # False     nas listas, a ordem conta!



#%%
"""
    Exercício: as disciplinas da Ana e as do Rui.
        Que disciplinas têm os dois?           (interseção)
        Quantas disciplinas há ao todo?        (união)
        Quais tem a Ana que o Rui não tem?     (diferença)
"""

ana = {"IPCE", "Cálculo", "Física"}
rui = {"IPCE", "Física", "Química", "Inglês"}

print(ana & rui == {"IPCE", "Física"})     # True
print(len(ana | rui))                      # 5
print(ana - rui)                           # {'Cálculo'}

# Com sets de strings, comparamos com == em vez de fazer print:
# assim não depende da ordem (que pode variar).






# %%
"""
d) EXERCICIO 52 (GUIÃO) - has_duplicates e same_words [5 min -> 10:45] ----

    has_duplicates: a lista de palavras tem alguma palavra repetida?
                    -> é o has_repeated da secção b)
    same_words:     duas listas têm exatamente as mesmas palavras?
                    (a ordem e o número de vezes de cada palavra não contam)
                    -> é o == dos sets da secção c)

    Usando sets, basta um return com uma expressão booleana.
"""

type WordList = list[str]

def has_duplicates(l: WordList) -> bool:
    """ Has the list l any duplicate values? """
    return len(l) != len(set(l))

def same_words(a: WordList, b: WordList) -> bool:
    """ Do the lists a and b have  exactly the same words? """
    return set(a) == set(b)

def main() -> None:
    a = ['a', 'mera', 'gnosiologia', 'explícita', 'ou', 'deliberadamente', 'divorciada', 'da',
         'axiologia', 'por', 'um', 'lado', 'e', 'da', 'ontologia', 'por', 'outro', 'não', 'se',
         'converte', 'sem', 'mais', 'em', 'fundamentação', 'deontológica', 'em', 'razão', 'do',
         'dever', 'ser']
    b = ['a', 'mera', 'gnosiologia', 'simples', 'afirmação', 'teorética', 'deliberadamente',
         'divorciada', 'da', 'axiologia', 'e', 'da', 'ontologia', 'não', 'nos', 'pode', 'fornecer',
         'uma', 'fundamentação', 'deontológica']
    c = ['a', 'axiologia', 'converte', 'da', 'deliberadamente', 'deontológica', 'dever', 'divorciada',
         'do', 'e', 'em', 'em', 'explícita', 'fundamentação', 'gnosiologia', 'lado', 'mais', 'mera',
         'não', 'ontologia', 'ou', 'outro', 'por', 'por', 'razão', 'se', 'sem', 'ser', 'um']
    print(has_duplicates(a))
    print(has_duplicates(b))
    print(has_duplicates(c))
    print()
    print(same_words(a,b))
    print(same_words(a,c))
    print(same_words(b,c))

main()

# True
# True
# True
#
# False
# True
# False
#
# O a e o c têm as mesmas palavras, por outra ordem: same_words(a, c) dá True.
#
# Resumo, para decorar: pensem em SET quando o problema fala de
#   duplicados, valores únicos/distintos, "está lá?", em comum, juntar sem repetir.









# %%
"""
e) Dicionários: a sintaxe e as características [9 min -> 10:54] ------------

    Um dicionário guarda PARES chave: valor.
    Vai-se buscar o valor pela CHAVE (não por uma posição 0, 1, 2...).
    É como um dicionário a sério: procura-se a palavra (chave), lê-se o significado (valor).

    Características:
        1. as chaves NÃO se repetem (os valores podem)
        2. guarda a ORDEM em que os pares entraram
        3. a chave tem de ser imutável: número, string, tuplo (não pode ser lista)

    Lista: [ ]     Set: { }     Dicionário: { chave: valor, ... }
"""

# Criar
notas = {"Ana": 15, "Rui": 12, "Rita": 18}

print(notas)                    # {'Ana': 15, 'Rui': 12, 'Rita': 18}
print(len(notas))               # 3     3 pares


# Ler um valor: d[chave]
print(notas["Rita"])            # 18

# Ler uma chave que não existe dá erro:
# print(notas["Maria"])
# KeyError: 'Maria'


# Perguntar se uma CHAVE existe: in
print("Rui" in notas)           # True
print("Maria" in notas)         # False
print(15 in notas)              # False     o in procura nas CHAVES, não nos valores!

if "Maria" in notas:            # por isso, quando não se sabe, pergunta-se primeiro
    print(notas["Maria"])
else:
    print("Não há nota da Maria")   # Não há nota da Maria


# O dicionário vazio: {} ou dict()
vazio = dict()
print(len(vazio))               # 0






# %%
"""
    Alterar e acrescentar: d[chave] = valor   (a MESMA instrução faz as duas coisas)
        - se a chave já existe, troca o valor
        - se a chave não existe, acrescenta um par novo, no fim
    Tirar: d.pop(chave)
    Percorrer: o for dá as CHAVES; o valor vai-se buscar com d[chave]
    (precisa das notas da célula de cima)
"""

notas["Rui"] = 13               # o Rui já existe: a nota passa de 12 para 13
notas["Zé"] = 10                # o Zé não existe: entra um par novo
print(notas)                    # {'Ana': 15, 'Rui': 13, 'Rita': 18, 'Zé': 10}

notas.pop("Zé")
print(notas)                    # {'Ana': 15, 'Rui': 13, 'Rita': 18}

for nome in notas:
    print(f"{nome} tem {notas[nome]}")
# Ana tem 15
# Rui tem 13
# Rita tem 18

# Para ver todas as operações: teórica 06b, secção Dicionários.
# (lá aparece também o d.items(), para percorrer chave e valor ao mesmo tempo)






# %%
"""
f) Dicionários para PROCURAR UM VALOR PELA CHAVE: lista vs dicionário [5 min -> 10:59]

    Qual a nota da Rita?

    1. Com listas: duas listas "paralelas" (o nome e a nota na mesma posição i).
"""

nomes = ["Ana", "Rui", "Rita"]
lista_notas = [15, 12, 18]

def grade_of_list(nomes: list[str], lista_notas: list[int], nome: str) -> int | None:
    """ The grade of nome, or None. (with lists only) """
    for i in range(len(nomes)):
        if nomes[i] == nome:
            return lista_notas[i]
    return None

print(grade_of_list(nomes, lista_notas, "Rita"))       # 18

# Um ciclo para cada procura. E se alguém apaga um nome de uma lista
# e se esquece da outra? Fica tudo desalinhado.


"""
    2. Com um dicionário: uma linha, sem ciclo.
"""

notas = {"Ana": 15, "Rui": 12, "Rita": 18}
print(notas["Rita"])            # 18


"""
    Exercício: uma TABELA DE PREÇOS (produto -> preço).
        order_total(menu, order) -> o total a pagar por uma lista de produtos.
        Os produtos que não estão no menu não contam.
"""

menu = {"café": 0.8, "torrada": 1.5, "sumo": 2.0}

def order_total(menu: dict[str, float], order: list[str]) -> float:
    """ Total price of the order. Products not in the menu are ignored. """
    total = 0
    for produto in order:
        if produto in menu:
            total += menu[produto]
    return total

print(order_total(menu, ["café", "café", "torrada"]))     # 3.1
print(order_total(menu, ["bolo"]))                        # 0

# O tipo dict[str, float]: as chaves são str e os valores são float.
# Sem o "if produto in menu", o "bolo" dava KeyError.








# %%
"""
g) Dicionários para CONTAR OCORRÊNCIAS, e o EXERCICIO 53 (GUIÃO) [9 min -> 11:08]

    Quantas vezes aparece cada letra numa palavra?
    O padrão de contagem, para decorar:
        - valor novo?      entra no dicionário com contagem 0
        - depois, sempre:  soma 1
"""

def count_letters(s: str) -> dict[str, int]:
    """ How many times each letter occurs in s. """
    d = dict()
    for c in s:
        if c not in d:
            d[c] = 0            # 1ª vez que aparece: começa a 0
        d[c] = d[c] + 1         # fora do if: acontece SEMPRE
    return d

print(count_letters("banana"))  # {'b': 1, 'a': 3, 'n': 2}

# Sigam à mão os primeiros passos:
#   'b' -> não está -> d['b'] = 0 -> d['b'] = 1         {'b': 1}
#   'a' -> não está -> d['a'] = 0 -> d['a'] = 1         {'b': 1, 'a': 1}
#   'n' -> não está -> d['n'] = 0 -> d['n'] = 1         {'b': 1, 'a': 1, 'n': 1}
#   'a' -> JÁ está  ->               d['a'] = 2         {'b': 1, 'a': 2, 'n': 1}
#   ...






# %%
"""
    EXERCICIO 53 (GUIÃO) - how_many_of_each

    O mesmo padrão, com palavras de uma lista em vez de letras de uma string.
    Só com in, d[k] e d[k] = v.
"""

type WordList = list[str]

def how_many_of_each(l:  WordList) -> dict[str, int]:
    """ Count the ocurrences of each word in the list l. """
    d = dict()      # cria dicionário vazio
    for w in l:
        if w not in d:
            d[w] = 0
        d[w] = d[w] + 1
    return d

def main() -> None:
    a = ['a', 'mera', 'gnosiologia', 'explícita', 'ou', 'deliberadamente', 'divorciada', 'da',
         'axiologia', 'por', 'um', 'lado', 'e', 'da', 'ontologia', 'por', 'outro', 'não', 'se',
         'converte', 'sem', 'mais', 'em', 'fundamentação', 'deontológica', 'em', 'razão', 'do',
         'dever', 'ser']
    b = ['a', 'mera', 'gnosiologia', 'simples', 'afirmação', 'teorética', 'deliberadamente',
         'divorciada', 'da', 'axiologia', 'e', 'da', 'ontologia', 'não', 'nos', 'pode', 'fornecer',
         'uma', 'fundamentação', 'deontológica']
    c = ['a', 'axiologia', 'converte', 'da', 'deliberadamente', 'deontológica', 'dever', 'divorciada',
         'do', 'e', 'em', 'em', 'explícita', 'fundamentação', 'gnosiologia', 'lado', 'mais', 'mera',
         'não', 'ontologia', 'ou', 'outro', 'por', 'por', 'razão', 'se', 'sem', 'ser', 'um']
    for wl in [a,b,c]:
        print(how_many_of_each(wl))

main()

# {'a': 1, 'mera': 1, 'gnosiologia': 1, 'explícita': 1, 'ou': 1, 'deliberadamente': 1, ...
#  ..., 'da': 2, ..., 'por': 2, ..., 'em': 2, ...}
# (e mais dois dicionários, para o b e o c)
#
# As palavras saem pela ordem em que apareceram pela 1ª vez: a ordem em que entraram.






# %%
"""
h) EXERCICIO 54 (GUIÃO, Mooshak V) - O atlas que aprende [12 min -> 11:20]

    Um dicionário país -> capital, que vai aprendendo.
    Pergunta-se um país:
        - se sabe a capital, mostra-a
        - se não sabe, pede ao utilizador para o ensinar, e fica a saber
    Termina quando o utilizador escreve "fim".

    Exemplo de execução
        PAÍS: Portugal
        Portugal -> Lisboa
        PAÍS: Alemanha
        Não conheço a capital de Alemanha. Por favor ensine-me: Berlim
        PAÍS: Burkina Faso
        Não conheço a capital de Burkina Faso. Por favor ensine-me: Ouagadougou
        PAÍS: Espanha
        Espanha -> Madrid
        PAÍS: Alemanha
        Alemanha -> Berlim
        PAÍS: Burkina Faso
        Burkina Faso -> Ouagadougou
        PAÍS: fim

    Pistas: "sabe?" é um in. "Ensinar" é um d[k] = v.
"""

type Atlas = dict[str, str]            # país -> capital

END_MARK = "fim"

def interaction(atlas: Atlas) -> None:
    """ User interaction. """
    while True:
        country = input("PAÍS: ")
        if country == END_MARK:
            break
        if country in atlas:
            capital = atlas[country]
            print(f"{country} -> {capital}")
        else:
            capital = input(f"Não conheço a capital de {country}. Por favor ensine-me: ")
            atlas[country] = capital            # aprende: acrescenta o par novo

def main() -> None:
    atlas = {"Portugal":"Lisboa", "Espanha":"Madrid", "França":"Paris"}
    interaction(atlas)

main()

# O interaction não devolve nada, mas o atlas do main fica mudado:
# o dicionário é mutável, e a função recebe o MESMO dicionário (como as listas).
#
# Problema: quando o programa acaba, esquece tudo o que aprendeu.
# A solução são os FICHEIROS (2ª parte, e o ex 58 no CONSOLIDAR).
#
# Resumo, para decorar: pensem em DICIONÁRIO quando o problema fala de
#   procurar X por Y (capital pelo país, preço pelo produto), contar cada um,
#   somar/juntar por grupo.
#
# Para o Mooshak (V): esta célula toda.






# %%
"""
===========================================================================
INTERVALO [20 min -> 11:40]
===========================================================================
"""






# %%
"""
===========================================================================
Guião 09a, ex 55, e Guião 09b, ex 56, 57 e 59 - Forca e Ficheiros [80 min -> 13:00]
https://ipce-184ea7.gitlab.io/
===========================================================================
"""






# %%
"""
i) Strings são imutáveis, listas são mutáveis [7 min -> 11:47] -------------

    Imutável: depois de criado, não se pode mudar.
        string -> imutável      lista -> mutável
"""

s = "gato"
print(s[0])                     # g     ler, pode

# s[0] = "p"
# TypeError: 'str' object does not support item assignment

l = ["g", "a", "t", "o"]
l[0] = "p"                      # numa lista, pode
print(l)                        # ['p', 'a', 't', 'o']


"""
    E quando queremos o contrário?

        Uma "lista imutável"?  -> um TUPLO: (1, 2, 3)
                                  (já os conhecem: o return de vários valores)
        Uma "string mutável"?  -> passar para uma LISTA, mudar a lista,
                                  e no fim juntar outra vez numa string

    Passar a string para lista:
        list(s)       -> lista de carateres     (é o que a forca vai precisar)
        s.split()     -> lista de palavras      (teórica 05b)
    Juntar a lista numa string:
        separador.join(lista)
"""

t = (1, 2, 3)
# t[0] = 10
# TypeError: 'tuple' object does not support item assignment

l = list("gato")                # string -> lista de carateres
print(l)                        # ['g', 'a', 't', 'o']
l[0] = "p"
print(''.join(l))               # pato      lista -> string, sem nada no meio
print(' '.join(l))              # p a t o   com um espaço no meio

palavras = "o gato preto".split()           # string -> lista de palavras
palavras[1] = "cão"
print(' '.join(palavras))       # o cão preto


"""
    Mais 2 coisas que a forca precisa:
        f"{n:02d}"     -> inteiro sempre com 2 algarismos (0 à esquerda)
        s.isalpha()    -> a string só tem letras?
"""

print(f"{3:02d}", f"{12:02d}")  # 03 12
print("a".isalpha(), "7".isalpha())     # True False






# %%
"""
    Exercício: revelar uma letra (a "string mutável" da forca).
        O segredo é "ananas". Começa tudo escondido:   # # # # # #
        O jogador diz a letra "a". Revelem os "a":      a # a # a #

    Pista: percorrer as POSIÇÕES do segredo (range), para saber ONDE mudar a lista.
"""

secret = "ananas"
visible = ['#'] * len(secret)   # o * repete a lista: ['#', '#', '#', '#', '#', '#']

c = "a"
for i in range(len(secret)):
    if secret[i] == c:
        visible[i] = c          # muda a lista na mesma posição

print(' '.join(visible))        # a # a # a #

# É a função update_visible do exercício a seguir.
# Por isso o visible é uma lista: se fosse string, visible[i] = c dava erro.






# %%
"""
j) EXERCICIO 55 (GUIÃO, Mooshak W) - O Jogo da Forca [15 min -> 12:02] -----

    O utilizador tem no máximo 10 tentativas para adivinhar o segredo, letra a letra.

    Exemplo de execução
        Bem-vindo ao Jogo da Forca!
        # # # # # #
        01 - Adivinhe uma letra: a
        a # a # a #
        02 - Adivinhe uma letra: g
        A letra 'g' não faz parte do segredo.
        a # a # a #
        03 - Adivinhe uma letra: i
        A letra 'i' não faz parte do segredo.
        a # a # a #
        04 - Adivinhe uma letra: s
        a # a # a s
        05 - Adivinhe uma letra: z
        A letra 'z' não faz parte do segredo.
        a # a # a s
        06 - Adivinhe uma letra: n
        Parabéns! Adivinhou em 6 tentativas o segredo 'ananas'!

    Já vêm feitas: print_visible, got_it_right, input_letter e end_game.
    Leiam-nas primeiro: usam o join e o isalpha da secção i).

    Falta fazer:
        update_visible -> o exercício de cima, numa função
        interaction    -> o ciclo do jogo:
                            - no máximo max_attempts vezes
                            - pede a letra (o número da tentativa com 2 algarismos)
                            - se a letra está no segredo: revela-a;
                              se já adivinhou tudo, o jogo acaba logo
                            - se não está: avisa
                            - mostra o estado
                          No fim, o end_game.
"""

def update_visible(visible: list[str], secret: str, c: str) -> None:
    """ Copy to 'visible' all the ocurrences of 'c' in 'secret'.
        Precondition: len(c) == 1
    """
    n = len(secret)
    for i in range(n):
        if secret[i] == c:
            visible[i] = c

def print_visible(visible: list[str]) -> None:
    """ Show the current state of the discovery. """
    print(' '.join(visible))

def got_it_right(visible: list[str], secret: str) -> bool:
    """ Has discovered the secret yet? """
    return ''.join(visible) == secret

def input_letter(prompt: str) -> str:
    """ Input a letter (alphabetic char). """
    while True:
        s = input(prompt)
        if len(s) == 1 and s.isalpha():
            return s

def end_game(visible: list[str], secret: str, attempts: int) -> None:
    """ Action at the end of the game. """
    if got_it_right(visible, secret):
        print(f"Parabéns! Adivinhou em {attempts} tentativas o segredo '{secret}'!")
    else:
        print(f"Lamento, mas esgotou o número de tentativas. O segredo era '{secret}'.")

def interaction(secret: str, max_attempts: int) -> None:
    """ User interaction. """
    print("Bem-vindo ao Jogo da Forca!")
    visible = ['#'] * len(secret)   # é preciso usar uma lista mutável; uma string não serve
    attempts = 0
    print_visible(visible)
    for i in range(max_attempts):
        attempts += 1
        letter = input_letter(f"{attempts:02d} - Adivinhe uma letra: ")
        if letter in secret:
            update_visible(visible, secret, letter)
            if got_it_right(visible, secret):
                break                   # adivinhou: sai já do ciclo
        else:
            print(f"A letra '{letter}' não faz parte do segredo.")
        print_visible(visible)
    end_game(visible, secret, attempts)

def main() -> None:
    MAX_ATTEMPTS = 10
    interaction("ananas", MAX_ATTEMPTS)

main()

# Para testar o "Lamento": escrevam "a" 10 vezes.
#   ...
#   10 - Adivinhe uma letra: a
#   a # a # a #
#   Lamento, mas esgotou o número de tentativas. O segredo era 'ananas'.
#
# Porquê o break logo a seguir ao update_visible?
# Quando acerta a última letra, o exemplo não volta a mostrar o "a n a n a s":
# passa direto para o "Parabéns!". O break salta o print_visible.
#
# O input_letter só sai quando a resposta é UMA letra:
# "ab", "7" ou Enter vazio voltam a perguntar, sem gastar tentativa.
#
# Para o Mooshak (W): esta célula toda.






# %%
"""
k) Quadro final: as 5 coleções, e quando usar cada uma [5 min -> 12:07] ----

               escreve-se     muda?    repete?   ordem?   acesso
    lista      [1, 2, 2]      sim      sim       sim      l[i]  (posição)
    tuplo      (1, 2, 2)      NÃO      sim       sim      t[i]  (posição)
    string     "abba"         NÃO      sim       sim      s[i]  (posição)
    set        {1, 2}         sim      NÃO       NÃO      só "in" (não há s[i])
    dicionário {"a": 1}       sim      chaves    sim      d[chave]
                                       NÃO

    Vazios:    []   ()   ""   set()   {} ou dict()       ({} é dicionário, não set!)

    Quando usar:
        lista       -> uma sequência que vai mudando (acrescentar, alterar),
                       onde a ordem conta e pode haver repetidos
        tuplo       -> uma sequência fixa: vários valores que vão juntos
                       (o return de vários resultados, um ponto (x, y))
        string      -> texto. Para mudar, passar a lista (list ou split) e voltar com join
        set         -> duplicados, valores únicos, "está lá?" rápido,
                       interseção / união / diferença
        dicionário  -> procurar um valor por uma chave, contar cada um,
                       somar por grupo

    O que todas têm igual: len(c), x in c, for x in c.

    E só as imutáveis (números, strings, tuplos) podem ser
    elementos de um set ou chaves de um dicionário.
"""






# %%
"""
l) Ficheiros: abrir, ler linha a linha, fechar [10 min -> 12:17] -----------

    Tudo o que está nas variáveis desaparece quando o programa acaba (está na RAM).
    Um ficheiro fica guardado no disco: continua lá amanhã.

    Em IPCE: só ficheiros de TEXTO, lidos linha a linha. Três passos, sempre:
        1. abrir:    f = open(nome, 'r')       'r' de read: ler
        2. usar:     for line in f:            uma linha de cada vez
        3. fechar:   f.close()

    Precisa do alice.txt na mesma pasta deste ficheiro.
"""

f = open("alice.txt", 'r', encoding="utf-8")
n = 0
for line in f:
    if n < 13:
        print(line, end='')     # mostra só as 13 primeiras linhas
    n += 1
f.close()
print(n)                        # 2652      o livro tem 2652 linhas

# O encoding="utf-8": para os acentos serem bem lidos em qualquer computador
# (ver os problemas mais abaixo). Usem-no sempre.
#
# Porquê o end=''? Cada linha lida já traz o '\n' (mudança de linha) no fim.
# Sem o end='', o print acrescentava outro e saía uma linha em branco entre cada uma.
#
# O for não carrega o livro todo para a memória: só uma linha de cada vez.
# Assim dá para ler ficheiros maiores do que a RAM.






# %%
"""
    Exercícios rápidos com o alice.txt

    1. count_lines_with(file_name, s): em quantas linhas aparece a string s?
       count_lines_with("alice.txt", "Rainha") == 74
"""

def count_lines_with(file_name: str, s: str) -> int:
    """ Number of lines of the file where s occurs. """
    f = open(file_name, 'r', encoding="utf-8")
    n = 0
    for line in f:
        if s in line:           # o in das strings: s aparece dentro da linha?
            n += 1
    f.close()
    return n

print(count_lines_with("alice.txt", "Rainha"))     # 74
print(count_lines_with("alice.txt", "Alice"))      # 436


"""
    2. count_empty_lines(file_name): quantas linhas vazias?
       Primeiro tentem com line == "\n". Depois com line.rstrip() == "".

       line.rstrip() -> cria uma cópia sem os brancos do FIM da linha
                        (espaços e o '\n')
"""

def count_empty_lines(file_name: str) -> int:
    """ Number of empty (or only spaces) lines of the file. """
    f = open(file_name, 'r', encoding="utf-8")
    n = 0
    for line in f:
        if line.rstrip() == "":
            n += 1
    f.close()
    return n

print(count_empty_lines("alice.txt"))      # 64

# Com line == "\n" dava 51: há 13 linhas que só têm espaços antes do '\n'.
# Quase sempre que uma linha vai ser USADA (comparar, chave de um dicionário),
# faz-se primeiro o rstrip.
#
# O padrão é sempre o mesmo: open, for line in f, close.
# Só muda o que se faz com cada linha.






# %%
"""
    Abrir para ler um ficheiro que não existe dá erro:
        open("nao_existe.txt", 'r')
        FileNotFoundError: [Errno 2] No such file or directory: 'nao_existe.txt'

    Para perguntar ANTES, em IPCE usamos sempre esta função (já vem feita nos guiões).
    O try/except: "tenta abrir; se der FileNotFoundError, em vez de rebentar, devolve False".
    É a ÚNICA vez que o try/except aparece em IPCE.
"""

def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

print(file_exists("alice.txt"))         # True
print(file_exists("nao_existe.txt"))    # False

# Os 3 problemas que vão aparecer (teórica 08a, "Possíveis problemas na abertura de ficheiros"):
#
# 1. "O ficheiro está lá, mas dá FileNotFoundError!"
#    O Python procura na pasta corrente, que pode não ser a do vosso .py.
#    Para saber qual é:
#        import os
#        print(os.getcwd())
#    Ponham lá o ficheiro, ou mudem a pasta no canto superior direito do Spyder.
#
# 2. "Dá um erro com a palavra encoding" (ou os acentos saem esquisitos)
#    Alguns computadores (Windows, sobretudo) não leem em utf-8 por omissão.
#    Solução: encoding="utf-8" no open. O Mooshak usa utf-8.
#
# 3. "Gravei alice.txt mas não o encontra"
#    O Windows esconde as extensões: o ficheiro pode ter ficado alice.txt.txt!






# %%
"""
m) EXERCICIO 56 (GUIÃO, Mooshak X) - A Alice no País das Maravilhas [7 min -> 12:24]

    Três contagens no alice.txt:
        - o número de linhas do ficheiro
        - o número de linhas onde aparece "Alice"       (é o exercício rápido 1)
        - o número de vezes que a PALAVRA "Alice" aparece ("ALICE" não conta)

    Regras do guião:
        - percorrer o ficheiro UMA só vez, com 3 contadores
        - por isso, uma única função process_file, que devolve um triplo (tuplo)

    Exemplo de execução
        Nome do ficheiro: alice.txt
        (2652, 436, 437)

    Exemplo de execução
        Nome do ficheiro: inexistente.txt
        Não foi possível abrir o ficheiro

    Precisa do alice.txt.

    Para contar palavras numa linha, o split não chega. Vejam:
"""

line = 'um livro", pensou Alice, "sem figuras nem diálogos?"\n'
print(line.split().count("Alice"))      # 0     o split deixa a vírgula colada: 'Alice,'

# O get_words (teórica 05b) troca tudo o que não é letra por espaço, e só depois faz o split.
# E depois, o count das listas (teórica 04b) conta as vezes:
def get_words(s: str) -> list[str]:
    """ Extract all the word from string. """
    s2 = ''
    for c in s:
        if c.isalpha():
            s2 += c
        else:
            s2 += ' '
    return s2.split()

print(get_words(line))                  # ['um', 'livro', 'pensou', 'Alice', 'sem', 'figuras', 'nem', 'diálogos']
print(get_words(line).count("Alice"))   # 1


def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

def process_file(file_name: str, search_str: str) -> tuple[int,int,int]:
    """ Count three things in the file:
            - number of lines,
            - number of lines where the search_str occurs,
            - number of occurrences of search_str in the file
        Precondition: file_exists(file_name)
    """
    f = open(file_name, 'r', encoding="utf-8")
    n_lines = n_find_lines = n_finds = 0       # os 3 contadores
    for line in f:
        n_lines += 1
        if search_str in line:
            n_find_lines += 1
            n_finds += get_words(line).count(search_str)
    f.close()
    return (n_lines, n_find_lines, n_finds)

def main() -> None:
    SEARCH = "Alice"
    fname = input("Nome do ficheiro: ")
    if not (file_exists(fname)):
        print("Não foi possível abrir o ficheiro")
    else:
        res = process_file(fname, SEARCH)
        print(res)

main()

# Nome do ficheiro: alice.txt
# (2652, 436, 437)
#
# 436 linhas, mas 437 vezes: há uma linha com a Alice 2 vezes.
# Com o split em vez do get_words dava só 273: perdia todos os 'Alice,' 'Alice.' 'Alice!'.
#
# Atenção: a solução proposta devolve uma LISTA [n_lines, n_find_lines, n_finds].
# O guião pede um tuplo e mostra (2652, 436, 437), com parênteses. Seguimos o guião.
#
# Para o Mooshak (X): esta célula toda.






# %%
"""
n) Registos em 2 linhas com next(f); escrever ficheiros; o EXERCICIO 57 [9 min -> 12:33]

    No jogadores.txt, cada jogador ocupa DUAS linhas:
        João Leixões        <- nome e clube
        5                   <- golos

    O for dá uma linha de cada vez. Para ler a linha SEGUINTE dentro do ciclo: next(f).

    O next é uma função do próprio Python (como o len ou o print), não é do professor.
    Dá o próximo elemento de uma sequência que se lê "um a um", como um ficheiro aberto.
    E avança: o for já não volta a dar essa linha.

    Quando usar: quando cada registo ocupa um número FIXO de linhas
    (2 por jogador, 2 por país...). Cada volta do for lê o registo inteiro:
        for line1 in f:
            line2 = next(f)
            ... usar line1 e line2

    Precisa do jogadores.txt na mesma pasta deste ficheiro.
"""

f = open("jogadores.txt", 'r', encoding="utf-8")
for line1 in f:
    line2 = next(f)             # a linha logo a seguir à line1
    name, club = line1.split()
    goals = int(line2)
    print(f"{name} ({club}): {goals}")
f.close()
# João (Leixões): 5
# Tarquínio (Benfica): 6
# ... (11 jogadores)


"""
    Escrever num ficheiro: abrir com 'w' e usar o print que já conhecem, com file=g

        g = open("novo.txt", 'w', encoding="utf-8")   # 'w' de write
        print("uma linha", file=g)     # vai para o ficheiro, não para o ecrã
        g.close()

    O 'w' cria o ficheiro. Se ele já existir, APAGA tudo o que lá estava!
    O close é obrigatório: só aí o Python grava tudo de vez no disco.
"""






# %%
"""
    EXERCICIO 57 (GUIÃO) - Converter o ficheiro: cada jogador numa só linha

    Exemplo de execução:
        Nome do ficheiro: jogadores.txt
        João Leixões 5
        Tarquínio Benfica 6
        Pedro Sporting 3
        Luís Belenenses 8
        Miguel Leixões 2
        João Sporting 1
        Manuel Académica 5
        Camões Porto 5
        Fernando Benfica 10
        Joaquim Boavista 7
        Gervásio Belenenses 7

    Sem carregar o ficheiro para memória: lê 2 linhas, escreve 1, e assim até ao fim.
    São DOIS ficheiros abertos ao mesmo tempo: um para ler (f) e outro para escrever (g).
    O resultado deve ficar igual ao jogadores_new.txt do site.
"""

def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

def print_file(file_name: str) -> None:
    """ Print a file on the screen.
        Precondition: file_exists(src_name)
    """
    f = open(file_name, 'r', encoding="utf-8")
    for line in f:
        print(line, end='')
    f.close()

def convert_file(src_name: str, target_name: str) -> None:
    """ Convert a file of players to the new format.
        Precondition: file_exists(src_name)
    """
    f = open(src_name, 'r', encoding="utf-8")
    g = open(target_name, 'w', encoding="utf-8")
    for line in f:
        line = line.rstrip()
        line2 = next(f).rstrip()
        print(f"{line} {line2}", file=g)
    f.close()
    g.close()

def main() -> None:
    src_name = input("Nome do ficheiro: ")
    target_name = src_name.replace(".txt","_new.txt")
    if not (file_exists(src_name)):
        print("Não foi possível abrir o ficheiro")
    else:
        convert_file(src_name, target_name)
        print_file(target_name)

main()

# O target_name: "jogadores.txt".replace(".txt", "_new.txt") dá "jogadores_new.txt".
# (o programa reescreve o jogadores_new.txt que veio do site: fica igual)
#
# Sem os rstrip, o '\n' da line ficava no meio e cada jogador continuava em 2 linhas.






# %%
"""
o) EXERCICIO 59 (GUIÃO) - A pastelaria que se lembra do stock [20 min -> 12:53]

    Na aula 8 (ex 51), o stock lia-se do teclado, e perdia-se no fim.
    Agora lê-se do pastelaria.txt (do site) e, depois da venda, grava-se lá outra vez.
    Na execução seguinte, o stock já vem com a venda descontada: é a PERSISTÊNCIA.

    O pastelaria.txt tem o formato do teclado, mas sem o "fim":
    o for termina sozinho quando o ficheiro acaba.
        Puro-açúcar 230 4           <- variedade: nome, unidades, nº de ingredientes
        Leite-Condensado 0.1        <- e logo a seguir, uma linha por ingrediente
        Açúcar 0.76
        ...

    O que muda em relação ao ex 51? Só a UI:
        ui_make_cake_variety(line, f) -> as linhas dos ingredientes vêm do ficheiro f,
                                         com next(f), em vez de input()
        ui_load_shop (era o ui_input_shop) -> lê a loja do ficheiro
        ui_store_shop (nova)               -> grava a loja no ficheiro, no MESMO formato
    O resto é copiar do ex 51 (aula 8), tal e qual.

    Aqui o next(f) lê um número de linhas que não é fixo: o nº de ingredientes
    vem escrito na própria linha da variedade.

    O type File: o tipo dos ficheiros abertos, para pôr no "f: File" dos parâmetros.
"""

from dataclasses import dataclass

#########   type File   #########

import _io
type File = _io.TextIOWrapper   # tipo dos ficheiros

def file_exists(file_name: str) -> bool:
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False


#########   type Ingredient   #########

@dataclass
class Ingredient:
    name: str
    percent: float

# Sem funções.


#########   type CakeVariety   #########

@dataclass
class CakeVariety:
    name: str
    amount: int
    ingredients: list[Ingredient]

def cake_variety_main_ingredient(variety: CakeVariety) -> Ingredient | None:
    """ The main ingredient of the recipe of a cake variety. """
    if len(variety.ingredients) == 0:
        return None
    else:
        max_ingr = variety.ingredients[0]
        for ingr in variety.ingredients:
            if ingr.percent > max_ingr.percent:
                max_ingr = ingr
        return max_ingr

def cake_variety_sell(variety: CakeVariety, units: int) -> int:
    """ Sell some units.
        Precondition: units >= 0 """
    if units <= variety.amount:
        sold = units
    else:
        sold = variety.amount
    variety.amount -= sold
    return sold


#########   type CakeShop   #########

@dataclass
class CakeShop:
    stock: list[CakeVariety]

def cake_shop_size(shop: CakeShop) -> int:
    """ Number of cake varieties in the shop. """
    return len(shop.stock)

def cake_shop_find_variety(shop: CakeShop, vname: str) -> CakeVariety | None:
    """ Find a cake variety by name. """
    for cake in shop.stock:
        if cake.name == vname:
            return cake
    return None

def cake_shop_sell(shop: CakeShop, vname: str, units: int) -> int | None:
    """ Sell some units of a named cake variety.
        Precondition: units >= 0 """
    cake = cake_shop_find_variety(shop, vname)
    if cake is None:
        return None
    return cake_variety_sell(cake, units)


#########   User Interface   #########

# No validation here. We are assuming all the inputs are valid (to simplify).

@dataclass
class UI:
    shop: CakeShop

CAKE_SHOP_FILE_NAME = "pastelaria.txt"  # poderá ser necessário o path

def ui_make_ingredient(line: str) -> Ingredient:
    """ Create an ingredient from an input line. """
    name, percentage = line.split()
    return Ingredient(name, float(percentage))

def ui_make_cake_variety(line: str, f: File) -> CakeVariety:
    """ Create a cake variety ingredient from an input line plus ingredient lines. """
    name, amount, n_ingredients = line.split()
    n_ingredients = int(n_ingredients)
    l = []
    for _ in range(n_ingredients):
        line = next(f)                  # era input(): agora vem do ficheiro
        i = ui_make_ingredient(line)
        l.append(i)
    return CakeVariety(name, int(amount), l)

def ui_load_shop(ui: UI, file_name: str) -> None:
    """ Load the data of the shop.
        Precondition: file_exists(file_name)
    """
    f = open(file_name, 'r', encoding="utf-8")
    l = []
    for line in f:                      # a linha de uma variedade...
        cake = ui_make_cake_variety(line, f)    # ... e lá dentro, os next(f) dos ingredientes
        l.append(cake)
    f.close()
    ui.shop = CakeShop(l)               # a UI passa a guardar a loja lida

def ui_store_shop(ui: UI, file_name: str) -> None:
    """ Store the data of the shop.
        Precondition: file_exists(file_name)
    """
    g = open(file_name, 'w', encoding="utf-8")
    for cake in ui.shop.stock:
        print(f"{cake.name} {cake.amount} {len(cake.ingredients)}", file=g)
        for ingr in cake.ingredients:
            print(f"{ingr.name} {ingr.percent}", file=g)
    g.close()

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
    if not (file_exists(CAKE_SHOP_FILE_NAME)):
        print("Não foi possível abrir o ficheiro")
    else:
      ui_load_shop(ui, CAKE_SHOP_FILE_NAME)
      name = input("BOLO: ")            # pergunta o nome do bolo
      units = int(input("UNIDADES: "))  # pergunta a quantidade
      ui_process(ui, name, units)       # processa o pedido
      ui_store_shop(ui, CAKE_SHOP_FILE_NAME)


############# main #############

def main() -> None:
    ui = UI(CakeShop([]))
    ui_run(ui)

main()

# 1ª execução:
#   BOLO: Delícia-compacta
#   UNIDADES: 1000
#   Vendidas 127 unidades de Delícia-compacta
#   Ingrediente principal: Farinha
#
# Abram o pastelaria.txt no Spyder: a linha da Delícia-compacta passou a "Delícia-compacta 0 2".
#
# 2ª execução (corram a célula outra vez):
#   BOLO: Delícia-compacta
#   UNIDADES: 5
#   Vendidas 0 unidades de Delícia-compacta
#   Ingrediente principal: Farinha
# O programa lembra-se de que já não há!
# Para repor o stock: voltar a baixar o pastelaria.txt do site.
#
# O main cria a UI com uma loja VAZIA: a loja a sério vem do ficheiro, no ui_load_shop.
#
# No guião, o ui_store_shop diz "Precondition: file_exists(file_name)".
# Para escrever não é preciso: o 'w' cria o ficheiro se ele não existir.
#
# A solução proposta deste exercício está escrita com classes com métodos,
# que não damos em IPCE. Aqui está no estilo da aula 8: classes de dados + funções.






# %%
"""
p) Erros comuns [7 min -> 13:00] -------------------------------------------

    Os que rebentam estão comentados: descomentem, corram, leiam a mensagem.

    Coleções
"""

# Erro 1: {} não é um set vazio, é um dicionário vazio
s = {}
# s.add(1)
# AttributeError: 'dict' object has no attribute 'add'
s = set()                       # assim, sim


# Erro 2: uma lista não pode ir para dentro de um set, nem ser chave de um dicionário
# s = {[1, 2], [3]}
# TypeError: unhashable type: 'list'
# Só valores imutáveis (números, strings, tuplos). Um tuplo serve:
s = {(1, 2), (3,)}
print(len(s))                   # 2


# Erro 3: somar 1 a uma contagem que ainda não existe
d = dict()
# d["a"] = d["a"] + 1
# KeyError: 'a'
# É por isso que o padrão de contagem tem o "if c not in d: d[c] = 0" antes.


# Erro 4: o in de um dicionário procura nas CHAVES
atlas = {"Portugal": "Lisboa"}
print("Lisboa" in atlas)        # False   Lisboa é um valor, não uma chave






# %%
"""
    Ficheiros
"""

# Erro 5: esquecer o rstrip quando a linha vai ser chave de um dicionário
line = "Portugal\n"             # assim chega uma linha lida de um ficheiro
atlas = {line: "Lisboa"}        # sem rstrip!
print("Portugal" in atlas)      # False   a chave é "Portugal\n", não "Portugal"
atlas = {line.rstrip(): "Lisboa"}
print("Portugal" in atlas)      # True


# Erro 6: o 'w' APAGA o ficheiro antes de escrever.
#   g = open("alice.txt", 'w')    <- NÃO CORRAM: o livro ficava vazio!
#   Enganem-se no nome (ou no 'r'/'w') e perdem o ficheiro. Confirmem sempre o modo.


# Erro 7: esquecer o close depois de escrever.
#   O Python vai juntando o que se escreve e só grava tudo de vez no close.
#   Sem close, o ficheiro pode ficar vazio ou a meio.
#   (na teórica 08a, o players_store não tem o f.close() no fim: falta lá!)


# Erro 8: next(f) num ficheiro sem o formato certo
#   Se o jogadores.txt tivesse um jogador sem a linha dos golos (nº ímpar de linhas),
#   o next da última volta já não tinha linha para ler:
#       StopIteration
#   Com next, o ficheiro tem de respeitar SEMPRE o formato.


# Erro 9: FileNotFoundError com o ficheiro "lá"
#   Ver a célula do file_exists: os.getcwd(), e o alice.txt.txt do Windows.






# %%
"""
    E os erros que já apanhámos hoje pelo caminho:

    - a ordem de um set:        não confiar nela. Não há s[0]; comparar com ==.
    - alterar uma string:       s[0] = "p" dá TypeError. Passar a lista (a forca).
    - ler chave que não existe: KeyError. Perguntar antes com in.
    - o split com pontuação:    'Alice,' não é 'Alice'. Usar o get_words.
    - o end='' no print_file:   a linha lida já traz o '\n'.
    - linhas "vazias" com espaços: line == "\n" falha; usar line.rstrip() == "".
"""






# %%
"""
===========================================================================
CONSOLIDAR - para casa
===========================================================================

    Tentem primeiro sem olhar para a solução.
    Do guião (o atlas que não esquece) ao estilo de teste (dicionários + ficheiros).
"""






# %%
"""
EXERCICIO (EXTRA) 1 - EXERCICIO 58 (GUIÃO): o atlas que não esquece -------

    O atlas do ex 54 aprende, mas esquece tudo quando acaba.
    Agora:
        - no início, lê os países do ficheiro atlas.txt
          (se o ficheiro não existir, começa vazio)
        - no fim, grava TODOS os países que conhece no mesmo ficheiro
    No ficheiro, cada país ocupa 2 linhas: o país e a capital.

    O atlas.txt do site já traz 2 países (Portugal e Espanha):
        2 países carregados
        PAÍS: Espanha
        Espanha -> Madrid
        PAÍS: França
        Não conheço a capital de França. Por favor ensine-me: Paris
        PAÍS: fim
        3 países armazenados

    Os exemplos do guião começam sem atlas.txt na pasta (apaguem-no para experimentar):

    Exemplo de 1ª execução (ainda não há atlas.txt):
        0 países carregados
        PAÍS: Portugal
        Não conheço a capital de Portugal. Por favor ensine-me: Lisboa
        PAÍS: Espanha
        Não conheço a capital de Espanha. Por favor ensine-me: Madrid
        PAÍS: França
        Não conheço a capital de França. Por favor ensine-me: Paris
        PAÍS: fim
        3 países armazenados

    Exemplo de 2ª execução:
        3 países carregados
        PAÍS: Portugal
        Portugal -> Lisboa
        PAÍS: Angola
        Não conheço a capital de Angola. Por favor ensine-me: Luanda
        PAÍS: fim
        4 países armazenados

    (no guião, os exemplos começam com "Nome do ficheiro: paises.txt", mas o
     programa do guião não pede o nome: usa sempre a constante ATLAS_FILE_NAME.
     Essa linha está a mais nos exemplos.)

    Pistas: o load_atlas é o for + next dos jogadores, com rstrip nas duas linhas.
            O store_atlas é o for k in d da secção e), com 2 prints por país.
"""

type Atlas = dict[str, str]  # país -> capital

ATLAS_FILE_NAME = "atlas.txt"
END_MARK = "fim"

def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

def load_atlas(file_name: str) -> Atlas:
    """ Load the initial atlas from a file.
        The file is allowed to not exist.
    """
    atlas = dict()
    pass        # TODO
    return atlas

def store_atlas(file_name: str, atlas: Atlas) -> None:
    """ Store the data of the atlas. """
    pass        # TODO

def interaction(atlas: Atlas) -> None:
    """ User interaction. """
    pass        # TODO: o mesmo código do exercício 54

def main() -> None:
    at = load_atlas(ATLAS_FILE_NAME)
    print(f"{len(at)} países carregados")
    interaction(at)
    store_atlas(ATLAS_FILE_NAME, at)
    print(f"{len(at)} países armazenados")

# main()












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: o atlas que não esquece (ex 58)

type Atlas = dict[str, str]  # país -> capital

ATLAS_FILE_NAME = "atlas.txt"
END_MARK = "fim"

def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

def load_atlas(file_name: str) -> Atlas:
    """ Load the initial atlas from a file.
        The file is allowed to not exist.
    """
    atlas = dict()
    if file_exists(file_name):              # se não existe, fica o atlas vazio
        f = open(file_name, 'r', encoding="utf-8")
        for line in f:
            line2 = next(f)
            atlas[line.rstrip()] = line2.rstrip()   # país -> capital, sem os '\n'
        f.close()
    return atlas

def store_atlas(file_name: str, atlas: Atlas) -> None:
    """ Store the data of the atlas. """
    f = open(file_name, 'w', encoding="utf-8")
    for p in atlas:
        print(p, file=f)                    # 1ª linha: o país
        print(atlas[p], file=f)             # 2ª linha: a capital
    f.close()

def interaction(atlas: Atlas) -> None:
    """ User interaction. """
    while True:
        country = input("PAÍS: ")
        if country == END_MARK:
            break
        if country in atlas:
            capital = atlas[country]
            print(f"{country} -> {capital}")
        else:
            capital = input(f"Não conheço a capital de {country}. Por favor ensine-me: ")
            atlas[country] = capital

def main() -> None:
    at = load_atlas(ATLAS_FILE_NAME)
    print(f"{len(at)} países carregados")
    interaction(at)
    store_atlas(ATLAS_FILE_NAME, at)
    print(f"{len(at)} países armazenados")

main()

# Corram 2 vezes, e da 2ª o atlas já sabe o que aprendeu na 1ª.
# Para voltar ao início: baixem outra vez o atlas.txt do site.
# Para começar do zero (0 países, como no guião): apaguem o atlas.txt da pasta.
#
# Reparem no "The file is allowed to not exist": o load_atlas não pode ter
# a precondição file_exists, porque na 1ª execução o ficheiro pode ainda não existir.
# Por isso o file_exists está DENTRO do load_atlas.
#
# Sem os rstrip, a chave ficava "Portugal\n" e o "Portugal" in atlas dava False
# (o Erro 5). O programa voltava a pedir a capital de Portugal todas as vezes.






# %%
"""
EXERCICIO (EXTRA) 2 - O parque de estacionamento (teórica 08b) -------------

    Quanto paga um carro no parque dos Restauradores?
    Dados: hora e minuto de entrada; hora e minuto de saída.

        1ª hora      120 cêntimos
        2ª hora      140 cêntimos
        3ª hora      150 cêntimos
        seguintes    155 cêntimos cada

    - As horas acumulam: 2 horas pagam 120 + 140.
    - Arredonda-se SEMPRE para cima: 1 hora e 1 minuto paga 2 horas.
    - O carro nunca fica mais de 24 horas. Se sai a uma hora "mais cedo" do que
      entrou, saiu no dia seguinte: entra 12:30, sai 10:31 -> 23 horas -> 3510 cêntimos.
    - Só inteiros (nada de float). Dica: passar tudo para minutos primeiro.

    Já vem feito o parking, que liga as funções todas. É a técnica bottom-up:
    primeiro as funções pequenas, testadas uma a uma; a principal é a última.
    (a top-down faz ao contrário: começa pela principal, e as pequenas ficam para depois)

    Testes da teórica (entrada, saída):
        06:30 18:35  ->  12h05 -> 13 horas -> 1960
        06:30 18:25  ->  11h55 -> 12 horas -> 1805
        06:30 18:30  ->  12h00 -> 12 horas -> 1805
        18:25 06:30  ->  12h05 -> 13 horas -> 1960
        18:35 06:30  ->  11h55 -> 12 horas -> 1805
        18:30 06:30  ->  12h00 -> 12 horas -> 1805
        09:00 09:00  ->  24 horas          -> 3665
        09:00 09:01  ->  1 minuto -> 1 hora -> 120
        09:59 10:00  ->  1 minuto -> 1 hora -> 120
"""

MINUTES_IN_HOUR = 60
HOURS_IN_DAY = 24
MINUTES_IN_DAY = HOURS_IN_DAY * MINUTES_IN_HOUR

def valid_time(h: int, m: int) -> bool:
    """ Is the time valid? """
    return 0 <= h < HOURS_IN_DAY and 0 <= m < MINUTES_IN_HOUR

def convert_to_minutes(h: int, m: int) -> int:
    """ Convert a time to minutes.
        Precondition: valid_time(h, m)
    """
    pass        # TODO

def minutes_of_stay(entrance_in_minutes: int, exit_in_minutes: int) -> int:
    """ Calculate the stay from the entrance and the exit times given in minutes.
        Same entrance and exit means 24 hours.
    """
    pass        # TODO

def round_up_to_hours(minutes: int) -> int:
    """ Round up minutes to hours.
        Precondition: 0 < minutes <= MINUTES_IN_DAY
    """
    pass        # TODO

def price(hours: int) -> int:
    """ How much to pay, given the hours of stay?
        Precondition: 0 < hours <= HOURS_IN_DAY
    """
    pass        # TODO

def parking(he: int, me: int, hs: int, ms: int) -> int:
    """ How much to pay, given the entrance and the exit times?
        Precondition: valid_time(he, me) and valid_time(hs, ms)
    """
    entrance = convert_to_minutes(he, me)
    exit = convert_to_minutes(hs, ms)
    stay_minutes = minutes_of_stay(entrance, exit)
    stay_hours = round_up_to_hours(stay_minutes)
    return price(stay_hours)

# print(parking(12, 30, 10, 31) == 3510)












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: o parque de estacionamento

MINUTES_IN_HOUR = 60
HOURS_IN_DAY = 24
MINUTES_IN_DAY = HOURS_IN_DAY * MINUTES_IN_HOUR

def valid_time(h: int, m: int) -> bool:
    """ Is the time valid? """
    return 0 <= h < HOURS_IN_DAY and 0 <= m < MINUTES_IN_HOUR

def convert_to_minutes(h: int, m: int) -> int:
    """ Convert a time to minutes.
        Precondition: valid_time(h, m)
    """
    return h * MINUTES_IN_HOUR + m

def minutes_of_stay(entrance_in_minutes: int, exit_in_minutes: int) -> int:
    """ Calculate the stay from the entrance and the exit times given in minutes.
        Same entrance and exit means 24 hours.
    """
    if exit_in_minutes > entrance_in_minutes:
        return exit_in_minutes - entrance_in_minutes                    # no mesmo dia
    else:
        return exit_in_minutes + MINUTES_IN_DAY - entrance_in_minutes   # saiu no dia seguinte

def round_up_to_hours(minutes: int) -> int:
    """ Round up minutes to hours.
        Precondition: 0 < minutes <= MINUTES_IN_DAY
    """
    hours = minutes // MINUTES_IN_HOUR
    if minutes % MINUTES_IN_HOUR != 0:      # sobraram minutos: mais uma hora
        hours += 1
    return hours

def price(hours: int) -> int:
    """ How much to pay, given the hours of stay?
        Precondition: 0 < hours <= HOURS_IN_DAY
    """
    total = 0
    for h in range(1, hours + 1):           # h = 1, 2, ..., hours
        if h == 1:
            total += 120
        elif h == 2:
            total += 140
        elif h == 3:
            total += 150
        else:
            total += 155
    return total

def parking(he: int, me: int, hs: int, ms: int) -> int:
    """ How much to pay, given the entrance and the exit times?
        Precondition: valid_time(he, me) and valid_time(hs, ms)
    """
    entrance = convert_to_minutes(he, me)
    exit = convert_to_minutes(hs, ms)
    stay_minutes = minutes_of_stay(entrance, exit)
    stay_hours = round_up_to_hours(stay_minutes)
    return price(stay_hours)

# Testar cada função sozinha (é a vantagem do bottom-up):
print(convert_to_minutes(6, 30))        # 390
print(minutes_of_stay(390, 1115))       # 725
print(minutes_of_stay(1115, 390))       # 715
print(round_up_to_hours(725))           # 13
print(round_up_to_hours(720))           # 12
print(price(2))                         # 260

# Os testes da teórica, todos de uma vez:
print(parking(6, 30, 18, 35), parking(6, 30, 18, 25), parking(6, 30, 18, 30))     # 1960 1805 1805
print(parking(18, 25, 6, 30), parking(18, 35, 6, 30), parking(18, 30, 6, 30))     # 1960 1805 1805
print(parking(9, 0, 9, 0), parking(9, 0, 9, 1), parking(9, 59, 10, 0))            # 3665 120 120
print(parking(12, 30, 10, 31))          # 3510

# O 09:00 -> 09:00: o if do minutes_of_stay é ">" e não ">=",
# para a mesma hora contar como 24 horas e não como 0.
#
# Na teórica, a precondição do minutes_of_stay diz "0 < entrance_in_minutes",
# mas um carro que entra às 00:00 tem entrance 0. Aqui trocámos por uma frase.
# E o main da parte top-down da teórica tem "if not valid_time(...) and valid_time(...)":
# falta um parêntese, o not só se aplica ao 1º. O certo (como na parte bottom-up):
# if not (valid_time(he, me) and valid_time(hs, ms)):






# %%
"""
EXERCICIO (EXTRA) 3 - A palavra preferida da Alice ---------------------------

    Dicionários + ficheiros: qual é a palavra (com 4 letras ou mais) que mais
    aparece no alice.txt? E quantas vezes?

        word_counts(file_name, min_len) -> dicionário palavra -> número de vezes,
                                           só com as palavras de min_len letras ou mais
        most_common(d)                  -> o par (palavra, vezes) com mais vezes
                                           (se houver empate, a que apareceu primeiro)

    Uma só passagem pelo ficheiro. Usem o get_words (secção m).
    Resultado: ('Alice', 437)
    (sem o limite das 4 letras, ganhava o 'a', com 774 vezes)
"""

def get_words(s: str) -> list[str]:
    """ Extract all the word from string. """
    s2 = ''
    for c in s:
        if c.isalpha():
            s2 += c
        else:
            s2 += ' '
    return s2.split()

def word_counts(file_name: str, min_len: int) -> dict[str, int]:
    """ How many times each word with at least min_len letters occurs in the file.
        Precondition: file_exists(file_name)
    """
    pass        # TODO

def most_common(d: dict[str, int]) -> tuple[str, int]:
    """ The word with the highest count, and the count.
        Precondition: len(d) > 0
    """
    pass        # TODO

# print(most_common(word_counts("alice.txt", 4)))      # ('Alice', 437)












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: a palavra preferida da Alice (precisa do alice.txt)

def get_words(s: str) -> list[str]:
    """ Extract all the word from string. """
    s2 = ''
    for c in s:
        if c.isalpha():
            s2 += c
        else:
            s2 += ' '
    return s2.split()

def word_counts(file_name: str, min_len: int) -> dict[str, int]:
    """ How many times each word with at least min_len letters occurs in the file.
        Precondition: file_exists(file_name)
    """
    f = open(file_name, 'r', encoding="utf-8")
    d = dict()
    for line in f:
        for w in get_words(line):
            if len(w) >= min_len:
                if w not in d:              # o padrão de contagem da secção g)
                    d[w] = 0
                d[w] = d[w] + 1
    f.close()
    return d

def most_common(d: dict[str, int]) -> tuple[str, int]:
    """ The word with the highest count, and the count.
        Precondition: len(d) > 0
    """
    best = ""
    best_n = 0
    for w in d:
        if d[w] > best_n:                   # ">" e não ">=": no empate fica a 1ª
            best = w
            best_n = d[w]
    return (best, best_n)

counts = word_counts("alice.txt", 4)
print(most_common(counts))              # ('Alice', 437)
print(counts["disse"])                  # 276
print(most_common(word_counts("alice.txt", 1)))     # ('a', 774)

# O most_common é o "procurar o máximo" das listas, mas a percorrer as chaves.
# Começar com best_n = 0 funciona porque todas as contagens são pelo menos 1.






# %%
"""
EXERCICIO (EXTRA) 4 - Estilo teste: golos por clube --------------------------

    Com o jogadores.txt do site (2 linhas por jogador: "nome clube" e golos):

        goals_by_club(file_name) -> dicionário clube -> total de golos
        clubs_with_at_least(d, n) -> o CONJUNTO dos clubes com pelo menos n golos

    goals_by_club("jogadores.txt") ==
        {'Leixões': 7, 'Benfica': 16, 'Sporting': 4, 'Belenenses': 15,
         'Académica': 5, 'Porto': 5, 'Boavista': 7}
    clubs_with_at_least(d, 10) == {'Benfica', 'Belenenses'}

    Dica: é o padrão de contagem, mas em vez de somar 1 soma os golos.
"""

def goals_by_club(file_name: str) -> dict[str, int]:
    """ Total goals of each club.
        Precondition: file_exists(file_name)
    """
    pass        # TODO

def clubs_with_at_least(d: dict[str, int], n: int) -> set[str]:
    """ The clubs with at least n goals. """
    pass        # TODO

# d = goals_by_club("jogadores.txt")
# print(d)
# print(clubs_with_at_least(d, 10) == {'Benfica', 'Belenenses'})      # True












# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================






# %%
# Solução: golos por clube (precisa do jogadores.txt do site)

def goals_by_club(file_name: str) -> dict[str, int]:
    """ Total goals of each club.
        Precondition: file_exists(file_name)
    """
    f = open(file_name, 'r', encoding="utf-8")
    d = dict()
    for line1 in f:
        line2 = next(f)
        name, club = line1.split()          # o split sem argumentos já ignora o '\n' e os espaços
        goals = int(line2)
        if club not in d:
            d[club] = 0
        d[club] = d[club] + goals
    f.close()
    return d

def clubs_with_at_least(d: dict[str, int], n: int) -> set[str]:
    """ The clubs with at least n goals. """
    s = set()
    for club in d:
        if d[club] >= n:
            s.add(club)
    return s

d = goals_by_club("jogadores.txt")
print(d)        # {'Leixões': 7, 'Benfica': 16, 'Sporting': 4, 'Belenenses': 15, 'Académica': 5, 'Porto': 5, 'Boavista': 7}
print(clubs_with_at_least(d, 10) == {'Benfica', 'Belenenses'})      # True
print(len(clubs_with_at_least(d, 100)))                             # 0

# Aqui não foi preciso rstrip: o split() e o int() já tratam dos espaços e do '\n'.
# As 3 coleções juntas: lista (as linhas), dicionário (os totais), conjunto (o resultado).