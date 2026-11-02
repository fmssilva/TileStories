# %%
"""
===========================================================================
REVISÕES 11 - COLEÇÕES: TUPLOS, CONJUNTOS E DICIONÁRIOS
Listas, tuplos, strings, conjuntos, dicionários, matrizes...
O que tem cada uma de especial? E quando usar qual?
IPCE 2026/2027
===========================================================================


Porquê um ficheiro só para isto?
  Já conheces bem as listas (ficheiro 4), as matrizes (ficheiro 9)
  e as strings (ficheiro 10). O Python tem mais três coleções:
  os TUPLOS, os CONJUNTOS e os DICIONÁRIOS (teórica 6b).
  Cada uma resolve muito bem um tipo de problema:
    tuplo        -> poucos valores que andam juntos (um par, uma data)
    conjunto     -> "está lá?" e "sem repetidos"
    dicionário   -> "a cada chave, o seu valor" (nome -> nota, palavra -> contagem)
  Com a coleção certa, o código fica mais curto e mais claro.
  Com a errada, funciona na mesma, mas dá muito mais trabalho.

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 39. O mapa das coleções ....................................... [~15 min]
       Porquê: ver todas lado a lado, com o que cada uma tem de diferente.

 40. Tuplos ................................................... [~20 min]
       Porquê: devolver dois resultados de uma função, e listas de pares.

 41. Conjuntos ................................................ [~20 min]
       Porquê: tirar repetidos, e perguntar "está lá?" de forma direta.

 42. Dicionários .............................................. [~40 min]
       Porquê: associar valores a chaves. O padrão de contar com dicionário
       aparece em muitos problemas (letras, palavras, notas por aluno...).

 43. Qual escolher? E desafios ................................ [~40 min]
       Inclui a pergunta dos dicionários do Teste 2 2024/25.

  Resumo + soluções dos desafios

  Total: ~2h15. Não é para fazer de uma vez.

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

  Já viste uma primeira apresentação de tuplos, conjuntos e dicionários
  no ficheiro 4 (Ex 12.6 e 12.7), e a matriz esparsa com um dicionário
  na aula 7 (CONSOLIDAR 9). Aqui vão a sério.

  A numeração continua a dos ficheiros anteriores: este começa na secção 39.
"""






# %%
"""
===========================================================================
39. O MAPA DAS COLEÇÕES [~15 min]
===========================================================================

    coleção      escreve-se          tem ordem?  pode mudar?  repetidos?  acede-se por
    ----------   -----------------   ---------   ----------   ---------   -------------
    lista        [1, 2, 3]           sim         sim          sim         índice l[i]
    tuplo        (1, 2, 3)           sim         NÃO          sim         índice t[i]
    string       "abc"               sim         NÃO          sim         índice s[i]
    range        range(1, 4)         sim         NÃO          não         índice r[i]
    conjunto     {1, 2, 3}           não         sim          NÃO         (não há índice)
    dicionário   {"a": 1, "b": 2}    sim (*)     sim          chaves não  chave d["a"]
    matriz       [[1, 2], [3, 4]]    sim         sim          sim         m[i][j]

    (*) os dicionários guardam a ordem em que as chaves foram postas.

    O que TODAS têm: len(...), o operador in, e o for para as percorrer.
    (no dicionário, o in e o for são sobre as CHAVES)

Ex 39.1 [EXEMPLO] Os mesmos dados, em várias coleções -------------------------
"""

grades_list = [12, 15, 12, 18]
grades_tuple = (12, 15, 12, 18)
grades_set = {12, 15, 12, 18}
grades_dict = {"Ana": 12, "Rui": 15, "Eva": 12, "Teo": 18}

print(len(grades_list), len(grades_tuple), len(grades_set), len(grades_dict))   # 4 4 3 4
print(15 in grades_list, 15 in grades_set)          # True True
print("Rui" in grades_dict, 15 in grades_dict)      # True False   (o in procura nas CHAVES)
print(grades_list[0], grades_tuple[0], grades_dict["Ana"])   # 12 12 12
print(grades_set)                                   # {18, 12, 15}  (sem o 12 repetido)

# O conjunto perdeu um 12: não guarda repetidos.
# O dicionário guarda MAIS informação: de quem é cada nota.
# O print(grades_set) pode mostrar outra ordem: um conjunto não tem ordem.






# %%
"""
Ex 39.2 [PENSA] Prever -----------------------------------------------------------

    Sem correr: o que dá cada expressão? (algumas dão erro: qual?)

    a) len({1, 1, 2, 2, 3})
    b) type({})
    c) len(set("banana"))
    d) (1, 2) + (3,)
    e) {"a": 1, "b": 2}["b"]
    f) {"a": 1, "b": 2}["c"]
    g) [1, 2, 3][0] = 9      (como instrução)
    h) (1, 2, 3)[0] = 9      (como instrução)
"""

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 39.2 (sem código):
#
#   a) 3                 os repetidos desaparecem: {1, 2, 3}
#   b) <class 'dict'>    !!! o {} vazio é um dicionário; o conjunto vazio é set()
#   c) 3                 as letras distintas de "banana": b, a, n
#   d) (1, 2, 3)         juntar tuplos dá um tuplo NOVO; (3,) é um tuplo de 1 elemento
#                        (sem a vírgula, (3) era só o número 3 entre parênteses)
#   e) 2
#   f) KeyError: 'c'     a chave não existe
#   g) funciona          as listas mudam-se
#   h) TypeError         os tuplos não se mudam (como as strings)






# %%
"""
===========================================================================
40. TUPLOS [~20 min]
===========================================================================

    Um tuplo é uma "lista que não muda". Escreve-se com parênteses.

        point = (3, 4)          criar
        point[0]                ler (como numa lista)
        len(point)              tamanho
        x, y = point            DESEMPACOTAR: x fica 3, y fica 4
        (5,)                    um tuplo com 1 elemento (a vírgula é obrigatória)

    Não tem append, nem pop, nem point[0] = ... (TypeError).

    Para que serve, se é uma lista "pior"?
      - para valores que andam SEMPRE juntos (um ponto, uma data, um par nome-nota);
      - para uma função devolver MAIS do que um resultado;
      - como chave de um dicionário (as listas não podem: secção 42).

Ex 40.1 [EXEMPLO] Uma função com dois resultados (teórica 6b) -----------------
"""

def minmax(l: list[int]) -> tuple[int, int]:
    """ Min and max of an integer list.
        Precondition: l != []
    """
    smallest = l[0]
    largest = l[0]
    for v in l:
        if v < smallest:
            smallest = v
        if v > largest:
            largest = v
    return (smallest, largest)

result = minmax([2, 7, 5, -4])
print(result)                   # (-4, 7)
print(result[0], result[1])     # -4 7

low, high = minmax([2, 7, 5, -4])   # desempacotar: o mais habitual
print(f"de {low} a {high}")     # de -4 a 7

# Um return só devolve UMA coisa. Para devolver duas, junta-se num tuplo.
# Quem chama desempacota: low, high = ...
# (o número de variáveis tem de ser igual ao tamanho do tuplo,
#  senão: ValueError)






# %%
"""
Ex 40.2 [EXEMPLO] Uma lista de pares -----------------------------------------------
"""

students = [("Ana", 14), ("Rui", 9), ("Eva", 17)]

for pair in students:
    print(pair[0], pair[1])     # Ana 14 / Rui 9 / Eva 17

for name, grade in students:    # desempacotar logo no for: mais legível
    if grade >= 9.5:
        print(f"{name}: aprovado")
# Ana: aprovado
# Eva: aprovado

# Cada elemento da lista é um tuplo (nome, nota).
# "for name, grade in students" desempacota cada par numa volta.
# É o mesmo que o for de uma lista normal; só que cada elemento tem 2 partes.






# %%
"""
Ex 40.3 [FAZ] Tuplos ---------------------------------------------------------------

    a) sum_and_average(l): a soma e a média de uma lista, num tuplo
           sum_and_average([4, 8, 6]) == (18, 6.0)
    b) best(students): o nome do aluno com a nota mais alta
           best([("Ana", 14), ("Rui", 9), ("Eva", 17)]) == "Eva"
    c) passed(students): a lista dos nomes com nota >= 9.5
           passed([("Ana", 14), ("Rui", 9), ("Eva", 17)]) == ["Ana", "Eva"]
"""

def sum_and_average(l: list[int]) -> tuple[int, float]:
    """ Sum and average of l.
        Precondition: l != []
    """
    pass        # TODO

def best(students: list[tuple[str, int]]) -> str:
    """ Name of the student with the highest grade (the first, if repeated).
        Precondition: students != []
    """
    pass        # TODO

def passed(students: list[tuple[str, int]]) -> list[str]:
    """ Names of the students with grade >= 9.5. """
    pass        # TODO

students = [("Ana", 14), ("Rui", 9), ("Eva", 17)]
print(sum_and_average([4, 8, 6]), best(students), passed(students))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 40.3

def sum_and_average(l: list[int]) -> tuple[int, float]:
    """ Sum and average of l.
        Precondition: l != []
    """
    total = 0
    for v in l:
        total += v
    return (total, total / len(l))

def best(students: list[tuple[str, int]]) -> str:
    """ Name of the student with the highest grade (the first, if repeated).
        Precondition: students != []
    """
    best_name, best_grade = students[0]
    for name, grade in students:
        if grade > best_grade:
            best_name = name
            best_grade = grade
    return best_name

def passed(students: list[tuple[str, int]]) -> list[str]:
    """ Names of the students with grade >= 9.5. """
    names = []
    for name, grade in students:
        if grade >= 9.5:
            names.append(name)
    return names

students = [("Ana", 14), ("Rui", 9), ("Eva", 17)]
print(sum_and_average([4, 8, 6]))       # (18, 6.0)
print(best(students))                   # Eva
print(passed(students))                 # ['Ana', 'Eva']

# best: o "máximo até agora" (ficheiro 4), mas guardando o par todo:
# a nota para comparar, e o nome para devolver.






# %%
"""
===========================================================================
41. CONJUNTOS [~20 min]
===========================================================================

    Um conjunto é como na matemática: sem repetidos e sem ordem.

        s = {1, 2, 3}           criar
        s = set()               conjunto VAZIO (o {} é um dicionário!)
        set(l)                  um conjunto com os elementos de l (sem repetidos)
        x in s                  está lá? (muito rápido, mesmo com milhões)
        s.add(x)                acrescentar
        s.discard(x)            tirar (se não estiver, não faz nada)
        len(s)                  quantos elementos
        a | b                   união: os que estão em a OU em b
        a & b                   interseção: os que estão em a E em b
        a - b                   diferença: os que estão em a mas NÃO em b

    Não há índices (s[0] dá TypeError): não há "primeiro".

Ex 41.1 [EXEMPLO] Tirar os repetidos, e "está lá?" --------------------------------
"""

l = [3, 1, 3, 2, 1, 3]
print(len(l), len(set(l)))      # 6 3        3 valores distintos
print(set(l))                   # {1, 2, 3}

seen = set()
for v in [5, 7, 5, 9, 7]:
    if v in seen:
        print(f"{v} repetido")  # 5 repetido / 7 repetido
    else:
        seen.add(v)
print(len(seen))                # 3

# len(set(l)): quantos valores DIFERENTES há numa lista. Uma linha só!
# O seen é um conjunto que vai "lembrando" o que já apareceu.
# Com uma lista também dava (if v in seen_list), mas o in de uma lista
# percorre-a toda; o de um conjunto é quase imediato (teórica 6b).






# %%
"""
Ex 41.2 [EXEMPLO] As operações de conjuntos ----------------------------------------

    Os alunos inscritos em duas disciplinas.
"""

ipce = {"Ana", "Rui", "Eva", "Teo"}
algebra = {"Rui", "Teo", "Bia"}

both = ipce & algebra           # nas duas
either = ipce | algebra         # em pelo menos uma
only_ipce = ipce - algebra      # em IPCE, mas não em Álgebra

print(len(both), len(either), len(only_ipce))       # 2 5 2
print(both == {"Rui", "Teo"})                       # True
print(only_ipce == {"Ana", "Eva"})                  # True
print("Bia" in either, "Bia" in ipce)               # True False

# Comparamos com == em vez de escrever o print(both): o print de um conjunto
# de strings pode sair numa ordem diferente de cada vez que se corre.
# Para conjuntos, a ordem não interessa: {"Rui", "Teo"} == {"Teo", "Rui"} é True.






# %%
"""
Ex 41.3 [FAZ] Conjuntos --------------------------------------------------------------

    a) count_distinct(l): quantos valores diferentes há em l.
       Faz DUAS versões: uma com conjuntos (1 linha), outra só com listas
       (exercício da teórica 6b).
           count_distinct([1, 5, 3, 6, 3, 4, 2, 5, 4, 3, 6, 5, 4, 1, 2, 3]) == 6
    b) common_letters(w1, w2): o conjunto das letras que aparecem nas duas palavras
           common_letters("banana", "ananas") == {'a', 'n'}
    c) has_repeated(l): há algum valor repetido em l? (1 linha, com set)
           has_repeated([1, 2, 3]) == False     has_repeated([1, 2, 1]) == True
"""

def count_distinct(l: list[int]) -> int:
    """ How many distinct values in l (with a set). """
    pass        # TODO

def count_distinct_lists(l: list[int]) -> int:
    """ How many distinct values in l (only with lists). """
    pass        # TODO

def common_letters(w1: str, w2: str) -> set[str]:
    """ The letters that occur in both words. """
    pass        # TODO

def has_repeated(l: list[int]) -> bool:
    """ Is there a repeated value in l? """
    pass        # TODO

l = [1, 5, 3, 6, 3, 4, 2, 5, 4, 3, 6, 5, 4, 1, 2, 3]
print(count_distinct(l), count_distinct_lists(l),
      common_letters("banana", "ananas") == {'a', 'n'}, has_repeated([1, 2, 1]))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 41.3

def count_distinct(l: list[int]) -> int:
    """ How many distinct values in l (with a set). """
    return len(set(l))

def count_distinct_lists(l: list[int]) -> int:
    """ How many distinct values in l (only with lists). """
    distinct = []
    for v in l:
        if v not in distinct:       # só entra a primeira vez
            distinct.append(v)
    return len(distinct)

def common_letters(w1: str, w2: str) -> set[str]:
    """ The letters that occur in both words. """
    return set(w1) & set(w2)

def has_repeated(l: list[int]) -> bool:
    """ Is there a repeated value in l? """
    return len(set(l)) != len(l)

l = [1, 5, 3, 6, 3, 4, 2, 5, 4, 3, 6, 5, 4, 1, 2, 3]
print(count_distinct(l), count_distinct_lists(l))            # 6 6
print(common_letters("banana", "ananas") == {'a', 'n'})      # True
print(has_repeated([1, 2, 3]), has_repeated([1, 2, 1]))      # False True

# has_repeated: se há repetidos, o conjunto fica mais pequeno do que a lista.
# Compara com o Ex 14.20 do ficheiro 4 (for dentro de for): a mesma pergunta,
# agora numa linha.
#
# O conjunto é a coleção certa quando a pergunta é "quais são os diferentes?"
# ou "está lá?". Quando a ORDEM ou as REPETIÇÕES interessam, é uma lista.






# %%
"""
===========================================================================
42. DICIONÁRIOS [~40 min]
===========================================================================

    Um dicionário associa CHAVES a VALORES, como um dicionário de verdade
    associa palavras a significados.

        d = {"Ana": 14, "Rui": 9}   criar (chave: valor)
        d = {}                      dicionário vazio
        d["Ana"]                    o valor da chave "Ana" (KeyError se não existir)
        d["Eva"] = 17               muda o valor, ou ACRESCENTA a chave se não existir
        "Ana" in d                  a chave existe?
        d.pop("Rui")                tira a chave (e devolve o valor)
        len(d)                      quantas chaves
        for k in d                  percorre as CHAVES
        for k, v in d.items()       percorre os pares (chave, valor)

    Regras: cada chave aparece uma só vez; as chaves não podem mudar
    (números, strings, tuplos sim; listas não).

Ex 42.1 [EXEMPLO] Capitais (teórica 6b) ----------------------------------------------
"""

capitals = {"Portugal": "Lisboa", "Espanha": "Madrid", "França": "Paris"}

print(capitals["Portugal"])                 # Lisboa
print("Itália" in capitals)                 # False
# print(capitals["Itália"])                 # KeyError: 'Itália'

capitals["Itália"] = "Roma"                 # chave nova: acrescenta
capitals["Portugal"] = "Porto"              # chave que já existe: muda o valor
capitals["Portugal"] = "Lisboa"             # ... e volta a mudar
print(len(capitals))                        # 4

for country in capitals:                    # as chaves
    print(country, end=" ")                 # Portugal Espanha França Itália
print()

for country, city in capitals.items():      # os pares
    print(f"{country} - {city}")
# Portugal - Lisboa
# Espanha - Madrid
# França - Paris
# Itália - Roma

# Antes de ler uma chave que pode não existir, pergunta-se:
#     if country in capitals: print(capitals[country])
# Senão, KeyError.






# %%
"""
Ex 42.2 [EXEMPLO] Contar com um dicionário --------------------------------------------

    letter_count("banana") == {'b': 1, 'a': 3, 'n': 2}
"""

def letter_count(s: str) -> dict[str, int]:
    """ How many times each character occurs in s. """
    counts = {}                     # 1. o dicionário vazio, antes do ciclo
    for c in s:
        if c in counts:             # 2. já vi este caráter?
            counts[c] += 1          #    sim: mais um
        else:
            counts[c] = 1           #    não: é o primeiro
    return counts

print(letter_count("banana"))       # {'b': 1, 'a': 3, 'n': 2}

# A RECEITA DO DICIONÁRIO-ACUMULADOR (um contador para cada chave):
#   1. d = {} antes do ciclo
#   2. para cada elemento: qual é a chave?
#      se a chave ainda NÃO está no dicionário: criá-la com o valor inicial
#      se já está: atualizar o valor
#   3. confirmar à mão, com um caso pequeno ("aba": {'a': 2, 'b': 1})
#
# O erro clássico: fazer só counts[c] += 1, sem o if.
# Na primeira vez que aparece um caráter, a chave ainda não existe: KeyError.
#
# Compara com uma lista de contadores: com um dicionário não é preciso saber
# à partida que carateres vão aparecer. A chave é o próprio caráter.






# %%
"""
Ex 42.3 [FAZ] Dicionários ---------------------------------------------------------------

    a) word_count(s): quantas vezes aparece cada palavra
           word_count("o gato e o rato") == {'o': 2, 'gato': 1, 'e': 1, 'rato': 1}
    b) most_common(d): a chave com o maior valor (a primeira, se houver empate)
           most_common({'o': 2, 'gato': 1, 'e': 1}) == 'o'
    c) invert(d): troca chaves e valores (teórica 6b)
           invert({"Portugal": "Lisboa", "Espanha": "Madrid"}) == {"Lisboa": "Portugal", "Madrid": "Espanha"}
    d) total_by_category(expenses): soma os valores de cada categoria
           total_by_category([("casa", 50), ("comida", 20), ("casa", 30)]) == {"casa": 80, "comida": 20}
"""

def word_count(s: str) -> dict[str, int]:
    """ How many times each word occurs in s. """
    pass        # TODO

def most_common(d: dict[str, int]) -> str:
    """ The key with the largest value (the first, if repeated).
        Precondition: d != {}
    """
    pass        # TODO

def invert(d: dict[str, str]) -> dict[str, str]:
    """ New dictionary with keys and values swapped. """
    pass        # TODO

def total_by_category(expenses: list[tuple[str, int]]) -> dict[str, int]:
    """ Sum of the values of each category. """
    pass        # TODO

print(word_count("o gato e o rato"), invert({"Portugal": "Lisboa"}),
      total_by_category([("casa", 50), ("comida", 20), ("casa", 30)]))
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Solução do Ex 42.3

def word_count(s: str) -> dict[str, int]:
    """ How many times each word occurs in s. """
    counts = {}
    for w in s.split():
        if w in counts:
            counts[w] += 1
        else:
            counts[w] = 1
    return counts

def most_common(d: dict[str, int]) -> str:
    """ The key with the largest value (the first, if repeated).
        Precondition: d != {}
    """
    best = None
    for k in d:
        if best == None or d[k] > d[best]:
            best = k
    return best

def invert(d: dict[str, str]) -> dict[str, str]:
    """ New dictionary with keys and values swapped. """
    result = {}
    for k, v in d.items():
        result[v] = k
    return result

def total_by_category(expenses: list[tuple[str, int]]) -> dict[str, int]:
    """ Sum of the values of each category. """
    totals = {}
    for category, value in expenses:
        if category in totals:
            totals[category] += value
        else:
            totals[category] = value
    return totals

wc = word_count("o gato e o rato")
print(wc)                                       # {'o': 2, 'gato': 1, 'e': 1, 'rato': 1}
print(most_common(wc))                          # o
print(invert({"Portugal": "Lisboa", "Espanha": "Madrid"}))
# {'Lisboa': 'Portugal', 'Madrid': 'Espanha'}
print(total_by_category([("casa", 50), ("comida", 20), ("casa", 30)]))
# {'casa': 80, 'comida': 20}

# a) e d) são a receita do dicionário-acumulador: em a) conta-se 1,
#    em d) soma-se o valor. A mesma estrutura.
#
# b) Num dicionário não há d[0]: não há "primeiro" por índice.
#    Por isso começa-se com best = None ("ainda nenhum", teórica 5a) e o 1º k entra sempre.
#    (o "best == None" vem primeiro: o or pára logo, e o d[best] nunca é lido
#     com best = None)
#
# c) Cuidado: se dois países tivessem a mesma capital, a 2ª apagava a 1ª
#    (as chaves não se repetem). Inverter só é seguro com valores todos diferentes.






# %%
"""
Ex 42.4 [EXEMPLO] Um dicionário de listas -----------------------------------------------

    Às vezes, a cada chave corresponde MAIS do que um valor.
    O valor passa a ser uma lista.

    by_first_letter(["ana", "rui", "alice", "rita", "eva"])
        == {'a': ['ana', 'alice'], 'r': ['rui', 'rita'], 'e': ['eva']}
"""

def by_first_letter(names: list[str]) -> dict[str, list[str]]:
    """ The names grouped by their first letter.
        Precondition: no name is empty
    """
    groups = {}
    for name in names:
        letter = name[0]
        if letter not in groups:
            groups[letter] = []         # a primeira vez: uma lista vazia
        groups[letter].append(name)     # e depois, sempre: acrescentar à lista
    return groups

print(by_first_letter(["ana", "rui", "alice", "rita", "eva"]))
# {'a': ['ana', 'alice'], 'r': ['rui', 'rita'], 'e': ['eva']}

# A mesma receita do acumulador. O valor inicial é [] em vez de 0,
# e "atualizar" é append em vez de += 1.
#
# Repara na ordem dos dois passos: primeiro garantir que a chave existe
# (com a lista vazia), DEPOIS acrescentar. Assim o append fica escrito uma vez só.
# É exatamente o que pede a pergunta do Teste 2 2024/25 (Ex 43.3).






# %%
"""
Ex 42.5 [PENSA] Chaves -----------------------------------------------------------------

    a) Porque é que {[1, 2]: "x"} dá erro, e {(1, 2): "x"} não?
    b) Na matriz esparsa da aula 7 (CONSOLIDAR 9), as chaves eram (i, j).
       Porque não uma lista [i, j]?
    c) d = {"a": 1}; d["a"] = 2; d["a"] = 3. Quantas chaves tem o d no fim? E o valor?
    d) Como escreverias "o valor da chave k, ou 0 se a chave não existir"?
"""

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 42.5 (sem código):
#
#   a) As chaves não podem mudar. Uma lista pode mudar (append...), e se a chave
#      mudasse depois de lá estar, o dicionário perdia-a. O Python não deixa:
#      TypeError: unhashable type: 'list'. Um tuplo não muda: serve.
#   b) Pela mesma razão: (i, j) é um tuplo, [i, j] seria uma lista.
#   c) Uma chave só, com o valor 3. Cada atribuição a uma chave que já existe
#      SUBSTITUI o valor (não cria outra chave "a").
#   d)  if k in d:
#           value = d[k]
#       else:
#           value = 0
#      (há um atalho, d.get(k, 0), mas não é preciso)






# %%
"""
===========================================================================
43. QUAL ESCOLHER? E DESAFIOS [~40 min]
===========================================================================

    Perante um problema, pergunta:

        Preciso de associar cada coisa a um valor
        (nome -> nota, palavra -> contagem)?               -> dicionário
        Só me interessa "está lá?" ou "quais são os
        diferentes"? A ordem e as repetições não contam?   -> conjunto
        São poucos valores fixos que andam juntos
        (um par, uma data), ou dois resultados?            -> tuplo
        Uma sequência que cresce, com ordem e repetidos?   -> lista
        Uma tabela com linhas e colunas?                   -> matriz (lista de listas)

Ex 43.1 [EXEMPLO] O mesmo problema, duas coleções ------------------------------------

    Quantos votos teve cada candidato?
"""

votes = ["Ana", "Rui", "Ana", "Eva", "Ana", "Rui"]

# Com listas: duas listas "paralelas" (os nomes e as contagens, na mesma posição)
names = []
counts = []
for v in votes:
    if v in names:
        i = names.index(v)              # a posição do nome (teórica 4b)
        counts[i] += 1
    else:
        names.append(v)
        counts.append(1)
print(names, counts)                    # ['Ana', 'Rui', 'Eva'] [3, 2, 1]

# Com um dicionário: o nome É a chave
results = {}
for v in votes:
    if v in results:
        results[v] += 1
    else:
        results[v] = 1
print(results)                          # {'Ana': 3, 'Rui': 2, 'Eva': 1}

# As duas funcionam. Mas com as listas é preciso manter as duas sempre
# "alinhadas", e procurar a posição de cada nome.
# Com o dicionário, a pergunta "quantos votos tem a Ana?" é só results["Ana"].
# Quando há "a cada X, um Y", o dicionário é quase sempre a resposta.






# %%
"""
Ex 43.2 [PENSA] Que coleção? -----------------------------------------------------------

    Para cada caso, que coleção usavas? Porquê?

    a) Os dias de cada mês (12 números fixos).
    b) As palavras de um texto que já foram usadas (para não as repetir).
    c) O número de telefone de cada pessoa da turma.
    d) As temperaturas de cada dia de um mês, por ordem.
    e) As temperaturas de 7 cidades em 30 dias.
    f) Uma função que devolve o mínimo e o máximo de uma lista.
    g) Os alunos inscritos numa cadeira, e quem está inscrito em duas cadeiras.
    h) As notas de cada aluno em vários testes (nome -> várias notas).
"""

# TODO: responde em comentário
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================















# %%
# Ideia do Ex 43.2 (sem código):
#
#   a) tuplo: 12 valores fixos, que não mudam (DURATIONS do ficheiro 4).
#      (uma lista também serve; o tuplo diz ao leitor "isto não muda")
#   b) conjunto: só interessa "já foi usada?", sem ordem nem repetidos.
#   c) dicionário: pessoa -> telefone.
#   d) lista: uma sequência, com ordem, e pode haver temperaturas repetidas.
#   e) matriz: 7 linhas (cidades) e 30 colunas (dias).
#   f) tuplo: (mínimo, máximo), dois resultados de uma vez.
#   g) conjuntos: e o "inscrito nas duas" é a interseção (&).
#   h) dicionário de listas: nome -> [notas] (como o Ex 42.4).






# %%
"""
DESAFIOS ------------------------------------------------------------------------

    Mais perto do nível dos testes. Soluções no fim do ficheiro.

Ex 43.3 [DESAFIO] Listas de pares e dicionários (Teste 2 2024/25, pergunta 3, 4 valores) --

    Para especificar em Python uma associação entre nomes e valores inteiros,
    podemos usar diversas técnicas. Vamos considerar as duas técnicas abaixo,
    onde a ordem dos elementos não interessa:
      - Técnica 1 - Lista de pares ordenados, por exemplo: [('a',1), ('b',2)]
      - Técnica 2 - Dicionário, por exemplo: {'a':1, 'b':2}
    Mas existe aqui um problema. Enquanto a técnica das listas permite especificar
    qualquer associação, com toda a liberdade, a técnica dos dicionários só permite
    associar um único valor a cada nome. Por exemplo, a associação
    [('a',1), ('a',2), ('b',3)] não pode ser descrita por um dicionário simples.
    A única possibilidade é associar a cada nome uma lista de valores, assim:
    {'a':[1,2], 'b':[3]}. Portanto, modificando um pouco a segunda técnica,
    conseguimos recuperar o mesmo poder expressivo das listas de pares.
    Quanto mais simples e claras forem as funções e quanto menos trabalho
    desnecessário fizerem, melhor. Não programe nenhuma função main, nem use
    input ou print.

    a) [2 valores] Defina uma função to_dict, que traduza uma lista de pares para
       um dicionário, respeitando os tipos indicados no cabeçalho da função.
           to_dict([('a',1), ('b',2), ('a',5), ('a', 8), ('a',1)]) = {'a': [1, 5, 8, 1], 'b': [2]}

    b) [2 valores] Defina uma função to_list, que traduza um dicionário para uma
       lista de pares, respeitando os tipos indicados no cabeçalho da função.
           to_list({'a': [1, 5, 8, 1], 'b': [2]}) = [('a', 1), ('a', 5), ('a', 8), ('a', 1), ('b', 2)]
"""

def to_dict(l: list[tuple[str, int]]) -> dict[str, list[int]]:
    """ Convert list of pairs to dictionary. """
    pass        # TODO

def to_list(d: dict[str, list[int]]) -> list[tuple[str, int]]:
    """ Convert dictionary to list of pairs. """
    pass        # TODO

print(to_dict([('a', 1), ('b', 2), ('a', 5), ('a', 8), ('a', 1)]))






# %%
"""
Ex 43.4 [DESAFIO] Código Morse (teórica 6b) ------------------------------------------------

    Com o dicionário MORSE (só algumas letras, para ser curto):
    a) encode(msg): cada letra passa ao seu código, separados por um espaço.
       As letras que não estão no dicionário são ignoradas.
           encode("SOS") == "... --- ..."
    b) decode(code): o contrário (o código vem separado por espaços).
           decode("... --- ...") == "SOS"

    Pista para a b): um dicionário invertido (Ex 42.3 c)), e o split.
"""

MORSE = {'A': '.-', 'E': '.', 'O': '---', 'S': '...', 'T': '-', 'N': '-.'}

def encode(msg: str) -> str:
    """ Morse code of msg, one code per letter, separated by spaces. """
    pass        # TODO

def decode(code: str) -> str:
    """ Text of a Morse code (codes separated by spaces). """
    pass        # TODO

print(encode("SOS"))






# %%
"""
Ex 43.5 [DESAFIO] A conta do supermercado -------------------------------------------------

    PRICES é um dicionário produto -> preço. Uma compra é uma lista de pares
    (produto, quantidade).

    a) total(order): o preço total da compra. Os produtos que não estão
       em PRICES não se pagam.
           total([("pão", 4), ("leite", 2), ("caviar", 1)]) == 3.2
    b) missing(order): o conjunto dos produtos da compra que não há em PRICES.
           missing([("pão", 4), ("caviar", 1), ("trufas", 2)]) == {"caviar", "trufas"}
    c) average_by_student(grades): grades é uma lista de pares (nome, nota),
       com várias notas por aluno. Devolve o dicionário nome -> média.
           average_by_student([("Ana", 10), ("Rui", 14), ("Ana", 16)])
               == {"Ana": 13.0, "Rui": 14.0}
"""

PRICES = {"pão": 0.3, "leite": 1.0, "ovos": 2.5}

def total(order: list[tuple[str, int]]) -> float:
    """ Total price of the order (unknown products are free). """
    pass        # TODO

def missing(order: list[tuple[str, int]]) -> set[str]:
    """ The products of the order that are not in PRICES. """
    pass        # TODO

def average_by_student(grades: list[tuple[str, int]]) -> dict[str, float]:
    """ Average grade of each student. """
    pass        # TODO

print(total([("pão", 4), ("leite", 2), ("caviar", 1)]))






# %%
"""
Ex 43.6 [SOZINHA] Tradutor palavra a palavra ------------------------------------------------

    Com um dicionário português -> inglês, traduz uma frase palavra a palavra.
    As palavras que não estão no dicionário ficam como estão.

        WORDS = {"o": "the", "gato": "cat", "preto": "black", "come": "eats"}
        translate("o gato preto come peixe") == "the cat black eats peixe"

    (sim, a ordem das palavras fica mal em inglês: é um tradutor muito simples!)
"""

# TODO






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  O MAPA
  [ ] lista [ ]: ordem, muda, repetidos         tuplo ( ): ordem, NÃO muda
  [ ] conjunto { }: sem ordem, sem repetidos    dicionário {k: v}: chave -> valor
  [ ] todas: len, in, for (no dicionário: sobre as chaves)
  [ ] {} é um dicionário vazio; o conjunto vazio é set()

  TUPLOS
  [ ] dois resultados: return (a, b); quem chama: a, b = f(...)
  [ ] listas de pares: for name, grade in students
  [ ] (5,) é um tuplo de um elemento

  CONJUNTOS
  [ ] len(set(l)): quantos diferentes; len(set(l)) != len(l): há repetidos
  [ ] in rápido; add, discard; & (nos dois), | (num deles), - (só no primeiro)
  [ ] para comparar conjuntos, ==; o print pode vir noutra ordem

  DICIONÁRIOS
  [ ] d[k] lê (KeyError se não existir); d[k] = v muda ou acrescenta
  [ ] antes de ler uma chave que pode faltar: if k in d
  [ ] for k in d (chaves); for k, v in d.items() (pares)
  [ ] o acumulador: d = {}; se a chave não está, criá-la (0 ou []); depois atualizar
  [ ] chaves: números, strings, tuplos; listas não

  QUAL ESCOLHER?
  [ ] "a cada X, um Y" -> dicionário; "está lá?"/"diferentes" -> conjunto
  [ ] poucos valores juntos / dois resultados -> tuplo; sequência -> lista; tabela -> matriz

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: os testes de exemplo (revisoes_6_teste_exemplo_1.py, 2 e 3).
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
# Solução do Ex 43.3

def to_dict(l: list[tuple[str, int]]) -> dict[str, list[int]]:
    """ Convert list of pairs to dictionary. """
    d = {}
    for name, value in l:
        if name not in d:
            d[name] = []
        d[name].append(value)
    return d

def to_list(d: dict[str, list[int]]) -> list[tuple[str, int]]:
    """ Convert dictionary to list of pairs. """
    l = []
    for name, values in d.items():
        for value in values:
            l.append((name, value))
    return l

print(to_dict([('a', 1), ('b', 2), ('a', 5), ('a', 8), ('a', 1)]))
# {'a': [1, 5, 8, 1], 'b': [2]}
print(to_list({'a': [1, 5, 8, 1], 'b': [2]}))
# [('a', 1), ('a', 5), ('a', 8), ('a', 1), ('b', 2)]

# to_dict: o dicionário de listas do Ex 42.4. Primeiro garantir a chave, depois append.
# to_list: o caminho inverso, com um for dentro de outro:
#   para cada nome, para cada valor da sua lista, um par (nome, valor).
#
# (o cabeçalho original do teste tem "list[(str, int)]" e "-> l: list[...]";
#  aqui escrevemos os tipos como nas aulas: list[tuple[str, int]])






# %%
# Solução do Ex 43.4

MORSE = {'A': '.-', 'E': '.', 'O': '---', 'S': '...', 'T': '-', 'N': '-.'}

def encode(msg: str) -> str:
    """ Morse code of msg, one code per letter, separated by spaces. """
    result = ""
    for c in msg:
        if c in MORSE:
            if result != "":
                result += " "
            result += MORSE[c]
    return result

def invert(d: dict[str, str]) -> dict[str, str]:
    """ New dictionary with keys and values swapped. """
    result = {}
    for k, v in d.items():
        result[v] = k
    return result

def decode(code: str) -> str:
    """ Text of a Morse code (codes separated by spaces). """
    letters = invert(MORSE)
    result = ""
    for symbol in code.split():
        if symbol in letters:
            result += letters[symbol]
    return result

print(encode("SOS"))                    # ... --- ...
print(decode("... --- ..."))            # SOS
print(decode(encode("NOTA")))           # NOTA

# encode: a receita da string nova (ficheiro 10), com o dicionário a dizer
# o que juntar. O espaço só ANTES de cada código que não seja o primeiro.
# decode: inverter o dicionário (os códigos são todos diferentes: é seguro),
# partir o código com split, e procurar cada pedaço.






# %%
# Solução do Ex 43.5

PRICES = {"pão": 0.3, "leite": 1.0, "ovos": 2.5}

def total(order: list[tuple[str, int]]) -> float:
    """ Total price of the order (unknown products are free). """
    result = 0.0
    for product, quantity in order:
        if product in PRICES:
            result += PRICES[product] * quantity
    return result

def missing(order: list[tuple[str, int]]) -> set[str]:
    """ The products of the order that are not in PRICES. """
    result = set()
    for product, quantity in order:
        if product not in PRICES:
            result.add(product)
    return result

def average_by_student(grades: list[tuple[str, int]]) -> dict[str, float]:
    """ Average grade of each student. """
    sums = {}
    counts = {}
    for name, grade in grades:
        if name not in sums:
            sums[name] = 0
            counts[name] = 0
        sums[name] += grade
        counts[name] += 1
    averages = {}
    for name in sums:
        averages[name] = sums[name] / counts[name]
    return averages

print(total([("pão", 4), ("leite", 2), ("caviar", 1)]))      # 3.2
print(missing([("pão", 4), ("caviar", 1), ("trufas", 2)]) == {"caviar", "trufas"})   # True
print(average_by_student([("Ana", 10), ("Rui", 14), ("Ana", 16)]))   # {'Ana': 13.0, 'Rui': 14.0}

# total: o "if product in PRICES" evita o KeyError do caviar.
# (com preços em float, as contas podem ganhar "ruído" nas últimas casas,
#  teórica 3b: para escrever dinheiro, usa-se f"{x:.2f}")
#
# missing: um conjunto, porque só interessa QUAIS faltam (sem repetidos).
#
# average_by_student: dois acumuladores com as mesmas chaves (soma e contagem),
# e depois um terceiro dicionário com as médias.
# Outra forma: um dicionário de listas (Ex 42.4) e a média de cada lista.s