# %%
"""
===========================================================================
GUIA DE SESSÃO - Prática 05 - IPCE 2026/2027
===========================================================================

Cobre o guião 05a (ex. 32 e 33). Esta semana não há guião 05b.

Site da Cadeira: https://ipce-184ea7.gitlab.io/
Este Guião na drive: https://drive.google.com/drive/folders/1twlkB4M0yg0tBxOt2c1diqJU6xtUsoay?usp=sharing


Tema de hoje: LISTAS.
    Até agora, cada variável guardava UM valor.
    Hoje: uma variável que guarda MUITOS valores, por ordem
    E juntamos tudo o que já sabemos: for, if, acumuladores, funções.

Como usar este ficheiro no Spyder:
    - cada bloco que começa com "# %%" é uma célula
    - Ctrl + Enter  -> corre só a célula onde está o cursor
    - Shift + Enter -> corre a célula e salta para a seguinte
    - F5            -> corre o ficheiro todo (o interpretador do ex 33 pede comandos!)

Mooshak de hoje: M (ex 33, o interpretador de listas, vale 30 pontos)
"""






# %%
"""
===========================================================================
LOGÍSTICA / Dúvidas [8 min -> 10:18]
===========================================================================

    - Folha de Presenças

    - Baixar este guia da drive e abrir no Spyder

    - Dúvidas da aula passada (floats, tabelas, séries de Taylor)
"""






# %%
"""
===========================================================================
REVISÕES - padrões e técnicas que aparecem SEMPRE [17 min -> 10:35]
===========================================================================

    Os exercícios dos testes mudam de roupa, mas por baixo
    são quase sempre os mesmos padrões. Se os reconhecerem,
    metade do trabalho está feita.


    a) Acumuladores e contadores [2 min -> 10:23] ---------------------------
"""

# A receita, sempre igual:
#   1. ANTES do ciclo:  criar a variável com o valor inicial certo
#   2. DENTRO do ciclo: atualizá-la
#   3. DEPOIS do ciclo: usá-la ou devolvê-la

n = 5

total = 0               # soma    -> começa no neutro da soma, 0
product = 1             # produto -> começa no neutro do produto, 1 (com 0 dava sempre 0!)
count = 0               # contador: soma 1 só quando a condição é verdadeira
for i in range(1, n + 1):
    total += i
    product *= i
    if i % 2 == 0:
        count += 1
print(total)
print(product)
print(count)

# Os 3 bugs clássicos (aula 3, ex 22):
#   - inicializar DENTRO do ciclo  -> volta a zero em cada volta;
#   - return DENTRO do ciclo       -> a função acaba na 1ª volta;
#   - não inicializar              -> UnboundLocalError.






# %%
"""
    b) Par, ímpar, múltiplo de ... ------------
"""

# Par?          x % 2 == 0
# Ímpar?        x % 2 != 0
# Múltiplo de k?   x % k == 0
for x in [4, 7, 0, -3]:
    if x % 2 == 0:
        print(x, "é par")
    else:
        print(x, "é ímpar")

# (em Python, -3 % 2 dá 1: o resto com 2 é sempre 0 ou 1, mesmo nos negativos.
#  Por isso "x % 2 == 1" também serve para ímpar. Noutras linguagens, como C
#  ou Java, -3 % 2 dá -1: "x % 2 != 0" é a forma que funciona em todo o lado)







# %%
"""
    c) sinal que troca a cada volta ------------

    Um sinal que troca a cada volta: +1, -1, +1, -1, ... (séries alternadas)

"""


# Forma 1: uma variável que se multiplica por -1, DEPOIS de ser usada
sign = 1
for i in range(5):
    print(i, sign)
    sign *= -1          # prepara a volta seguinte

# Cuidado com a ordem: trocar ANTES de usar faz a série começar em -1.



# Forma 2: calcular o sinal a partir do i, sem precisar de memória
for i in range(5):
    print(i, (-1) ** i)

# A forma 2 é a mais curta de escrever num teste.
# A forma 1 faz menos contas (uma multiplicação por volta, sem potências).
# Também dá com um if: se i % 2 == 0 soma, senão subtrai (série ln(2), aula 3).








# %%
"""
    d) Percorrer um intervalo de reais: passo fixo e passo geométrico [3 min -> 10:26]

    d.1) Passo fixo: n PONTOS entre inf e sup (os dois incluídos) -> n - 1 espaços
        Usar o for i in range(n) para correr o numero de passos que queremos
        e depois tendo o i a dar o número do passo,
        calcular o valor do "comprimento total dos passos" a partir do ponto inicial.
"""

def f(x: float) -> float:
    """ The function x squared. """
    return x * x

# PASSO FIXO: n PONTOS entre inf e sup (os dois incluídos) -> n - 1 espaços
inf, sup, n = 0.0, 1.0, 5
step = (sup - inf) / (n - 1)
for i in range(n): # dá o número de passos que queremos
    x = inf + i * step    # calculamos o comprimento total até ao passo a partir do ponto inicial
    print(f"{x:.2f} {f(x):.4f}")

# Se o enunciado falar em n PARTES (espaços) em vez de n PONTOS:
#   n partes = n + 1 pontos  ->  step = (sup - inf) / n   e   range(n + 1)
# Leiam com atenção: é aqui que aparece o off-by-one.
#
# Sempre "x = inf + i * step", e não "x += step":
# os pequenos erros dos floats iam-se somando volta após volta (aula 4).






# %%
"""
    d.2) Passo geométrico: o próximo valor vem do anterior (dividir por 2, ...)

    Uma espécie de "acumulador" que vai sendo dividido ou multiplicado por um fator a cada passo.
"""
# PASSO GEOMÉTRICO: o próximo valor vem do anterior (dividir por 2, ...)
x = 1.0
for i in range(5):
    print(x, 1 / x)
    x /= 2

# Aqui não há "início + i * passo": cada x é metade do anterior (ex 31, sin(x)/x).






# %%
"""
e) Reaproveitar o que já se calculou [2 min -> 10:28] -------------------

    Nos problemas de séries, pensem como resolver o problema e depois pensem:

    Há forma de reaproveitar o calculo do termo anterior para calcular o próximo termo, em vez de recalcular tudo do zero?
"""

# Tabela dos fatoriais de 1 a 6.
# Recalcular cada i! do zero seria um ciclo dentro de um ciclo.
# Mas i! = (i - 1)! * i: basta multiplicar o anterior pelo i.
fact = 1
for i in range(1, 7):
    fact *= i
    print(i, fact)

# A mesma ideia apareceu:
#   - Zeno:   term /= 2                       (aula 3)
#   - Taylor: num *= x * x, den *= ...        (aula 4)
# Nos testes, "quanto menos trabalho desnecessário, melhor" quer muitas vezes dizer isto.






# %%
"""
f) Organizar o programa: lógica de um lado, conversa do outro [2 min -> 10:30]

    No teste:
        - Não esquecer pensar Precondições ou validações
        - Não esquecer meter os tipos dos argumentos e retorno das funções
        - Pensar "numeros mágicos" vs Constantes ou variáveis
"""

def celsius_to_fahrenheit(c: float) -> float:
    """ Convert Celsius to Fahrenheit. """
    return 1.8 * c + 32             # faz a conta e DEVOLVE. Nada de input nem print.

def main() -> None:
    c = 20.0                        # num programa a sério: float(input("C: "))
    print(f"{c} C = {celsius_to_fahrenheit(c)} F")

main()

# Regra por defeito:
#   - funções de lógica: recebem parâmetros, fazem contas, return. Sem input/print.
#   - main: lê os dados (input), chama as funções, escreve os resultados (print).
#
# Exceções que já vimos e estão certas:
#   - uma função cuja MISSÃO é escrever (uma tabela, um desenho) faz print
#     e não devolve nada (-> None). Ex: print_table (ex 26), draw_line.
#   - se o enunciado mistura leitura e contas (ler n números e ir somando, ex 21),
#     a leitura pode ficar dentro da função que soma.
#
# Nos testes quando diz: "Não programe nenhuma função main, nem use input ou print"
# quer dizer para escrever só a função de lógica, com return.







# %%
"""
g) A ORDEM das condições [5 min -> 10:35] -------------------------------

    Isto serve tanto para condições todas numa expressão com and/or/not,
    como para if/elif/else. A ordem das condições conta, e muito.

    Caso 1: as condições NÃO se sobrepõem (cada caso é diferente).
        -> pensar só em termos de eficiência:
            as mais simples/rápidas primeiro, as mais pesadas no fim.
"""

REGISTERED = ["ana@fct.unl.pt", "rui@fct.unl.pt"]

def email_exists(email: str) -> bool:
    """ Check if the email is already registered.
        (imaginem uma pesquisa lenta numa base de dados com milhões de emails)
    """
    return email in REGISTERED

def create_account(age: int, email: str) -> str:
    """ Message after trying to create an account. """
    if age < 18:                        # comparar um número: instantâneo
        return "Não pode criar conta: é menor de idade"
    elif "@" not in email:              # procurar um carácter: rápido
        return "Email inválido"
    elif email_exists(email):           # ir à base de dados: lento -> fica para o fim
        return "Email já registado"
    return "Conta criada com sucesso"

print(create_account(15, "joao@fct.unl.pt"))
print(create_account(20, "joao.fct.unl.pt"))
print(create_account(20, "ana@fct.unl.pt"))
print(create_account(20, "joao@fct.unl.pt"))

# Erro comum: escrever o "Conta criada com sucesso" DEPOIS do if/elif, sem else.
# Aí aparecia sempre, mesmo quando havia um erro.
#
# Numa condição só, com or, a ordem conta pela mesma razão (curto-circuito):
#     if age < 18 or "@" not in email or email_exists(email):
# o or pára no primeiro True: o email_exists só corre se os outros falharem.






# %%
"""
    Caso 2: as condições SOBREPÕEM-SE (um caso está dentro do outro).
        -> pensar em termos de restrição das condições:
            o mais RESTRITO primeiro, o mais geral no fim (no else).

    Um cinema faz descontos:
        65 anos ou mais:   30% de desconto
        menos de 18 anos:  50% de desconto
        os outros:         preço normal

    Sem correr: quanto dá discounted_price_wrong(10, 100.0)?
"""

def discounted_price_wrong(age: int, price: float) -> float:
    """ Price after the age discount (WITH A BUG). """
    if age >= 65:
        return price * 0.7
    elif age < 65:
        return price
    # elif age < 18:
    return price * 0.5

print(discounted_price_wrong(10, 100.0))   # 100.0 (!!) devia ser 50.0






# %%
# O "elif age < 65" apanha TODA a gente com menos de 65 anos,
# incluindo as crianças. O ramo "age < 18" nunca chega a ser testado.
# "menos de 18" está dentro de "menos de 65" -> o "< 18" tem de vir antes.

def discounted_price(age: int, price: float) -> float:
    """ Price after the age discount. """
    if age >= 65:
        return price * 0.7
    elif age < 18:
        return price * 0.5
    return price

print(discounted_price(70, 100.0))     # 70.0
print(discounted_price(30, 100.0))     # 100.0
print(discounted_price(10, 100.0))     # 50.0






# %%
"""
===========================================================================
LISTAS - primeiros passos [29 min -> 11:04]
===========================================================================

a) Criar uma lista e ler os valores [3 min -> 10:38] ---------------------

    Corram e, antes de cada print, tentem adivinhar o que aparece.
"""

rainbow = ["vermelho", "laranja", "amarelo", "verde", "azul", "anil", "violeta"]

print(rainbow)
print(len(rainbow))                 # len = quantos elementos tem
print(rainbow[0])                   # o PRIMEIRO
print(rainbow[1])
print(rainbow[6])                   # o último está na posição 6, e não 7!
# print(rainbow[7])                 # <- descomenta: IndexError: list index out of range
print(rainbow[len(rainbow) - 1])    # o último, sem precisar de saber o tamanho
print(rainbow[-1])                  # o último, à maneira do Python
print(len([]))                      # a lista vazia tem 0 elementos


# Uma lista escreve-se entre [ ], com os valores separados por vírgulas.
# Cada valor tem uma POSIÇÃO, que se chama ÍNDICE, e começa em 0.
# Numa lista com 7 elementos, os índices vão de 0 a 6.
# Pedir a posição 7 é pedir um elemento que não existe: IndexError.
#
# Os índices negativos contam a partir do fim:
#   rainbow[-1] é o último, rainbow[-2] o penúltimo, ...
#
# Porquê começar em 0? O índice é a DISTÂNCIA ao início da lista:
# o 1º elemento está à distância 0. (Python, C, Java... começam todos em 0)






# %%
"""
a.2) Mini-exercícios: ler valores [2 min -> 10:40] -----------------------

    Uma ou duas linhas cada. Tentem antes de olhar para a solução.
"""

rainbow = ["vermelho", "laranja", "amarelo", "verde", "azul", "anil", "violeta"]

# 1. Escrevam o PENÚLTIMO elemento, de duas formas diferentes.
print(rainbow[5], rainbow[-2])          # anil anil

# 2. Escrevam o primeiro e o último.
print(rainbow[0], rainbow[-1])          # vermelho violeta

# 3. Escrevam o elemento do MEIO (7 elementos: o do meio é o índice 3).
print(rainbow[len(rainbow) // 2])       # verde






# %%
"""
b) Mudar uma lista (1): trocar um valor [2 min -> 10:42] -----------------
"""

grades = [12, 15, 9]

grades[2] = 10                  # TROCA o valor da posição 2
print(grades)                   # [12, 15, 10]

# Mini: ponham o PRIMEIRO elemento a valer 20.
grades[0] = 20
print(grades)                   # [20, 15, 10]

# Mini: somem 1 ao último (sem escrever o 10 à mão).
grades[-1] = grades[-1] + 1
print(grades)                   # [20, 15, 11]

# grades[i] = v  -> TROCA o valor da posição i. O tamanho não muda.
# Do lado direito do =, grades[-1] é LER. Do lado esquerdo, é ESCREVER.
#
# As listas podem mudar depois de criadas: são MUTÁVEIS.
# (os números não: o 3 é sempre 3)






# %%
"""
b.2) Mudar uma lista (2): crescer e encolher [3 min -> 10:45] ------------
"""

grades = [12, 15, 10]

grades.append(17)               # acrescenta um valor no FIM
print(grades)                   # [12, 15, 10, 17]

last = grades.pop()             # tira o último e devolve-o
print(last, grades)             # 17 [12, 15, 10]

# Mini: construir uma lista a partir da lista VAZIA.
# A lista dos quadrados de 1 a 5:
squares = []
for k in range(1, 6):
    squares.append(k * k)
print(squares)                  # [1, 4, 9, 16, 25]

# grades.append(v)  -> a lista CRESCE uma posição, no fim.
# grades.pop()      -> a lista ENCOLHE uma posição, no fim.
#
# A escrita "grades.append(...)", com um ponto, é nova:
# chama-se um MÉTODO, uma função que pertence à lista e mexe nela.
#
# Começar com [] e ir fazendo append é a forma normal de CONSTRUIR uma lista.
# (é um acumulador: em vez de somar, acrescenta)






# %%
"""
c) Percorrer uma lista com for [2 min -> 10:47] -------------------------
"""

grades = [12, 15, 9, 18, 14]
for g in grades:
    print("nota:", g)

# Já conhecem isto! O for percorre qualquer sequência:
# um range(...), uma string, e agora uma lista.
# Em cada volta, o g fica com o valor seguinte da lista.
#
# Chama-se a 1ª TÉCNICA: percorrer pelos VALORES.

# Mini: escrevam só as notas positivas (>= 10).
for g in grades:
    if g >= 10:
        print("positiva:", g)






# %%
"""
d) EXERCICIO (EXTRA) - Média das notas [4 min -> 10:51] ------------------

    Escrevam uma função que calcule a média dos valores de uma lista.
    Sem usar a função sum do Python: com um for e um acumulador.
        average([12, 15, 9, 18, 14]) == 13.6
        average([10]) == 10.0
    Pergunta extra: e com a lista vazia []? O que acontece?
"""

def average(l: list[int]) -> float:
    """ Average of the values of a list.
        Precondition: len(l) > 0
    """
    total = 0
    for v in l:
        total += v
    return total / len(l)

print(average([12, 15, 9, 18, 14]))    # 13.6
print(average([10]))                   # 10.0
# print(average([]))                   # <- descomenta: ZeroDivisionError

# É o acumulador de sempre, agora a percorrer uma lista.
# len(l) diz quantos valores somámos: nem é preciso um contador.
#
# Lista vazia: total = 0 e len = 0 -> 0 / 0 -> ZeroDivisionError.
# Daí a precondição: len(l) > 0.
#
# Não chamem "sum" à variável: o Python já tem uma função sum,
# e a variável "tapa-a" (lembram-se da aula 3?).






# %%
"""
e) Percorrer pelos ÍNDICES [2 min -> 10:53] -----------------------------

    Às vezes o valor não chega: precisamos de saber em que POSIÇÃO estamos.
"""

grades = [12, 15, 9, 18, 14]
for i in range(len(grades)):              # i = 0, 1, 2, 3, 4: exatamente os índices válidos!
    print("posição", i, "-> nota", grades[i])

# 2ª TÉCNICA: percorrer pelos ÍNDICES.
#   range(len(l)) dá os índices da lista: 0, 1, ..., len(l) - 1.
#   (o FIM do range nunca entra, e o último índice é len - 1: encaixam!)
#
# Quando usar cada uma? É já a seguir.






# %%
"""
f) Pelos valores, pelos índices, ou os dois? [3 min -> 10:56] ------------

    A regra para escolher:
      - só USAR os valores (ler, somar, contar, testar) -> for v in l
      - ALTERAR valores da lista, ou comparar POSIÇÕES diferentes
        (o vizinho, duas listas ao mesmo tempo)         -> for i in range(len(l))
"""

grades = [12, 15, 9, 18, 14]

# 1. Contar as notas pares: só USAMOS os valores -> pelos valores.
count = 0
for v in grades:
    if v % 2 == 0:
        count += 1
print(count)                    # 3

# 2. Somar 1 a TODAS as notas, na própria lista: ALTERAMOS -> pelos índices.
for i in range(len(grades)):
    grades[i] = grades[i] + 1
print(grades)                   # [13, 16, 10, 19, 15]

# 3. E pelos valores, não dava para alterar? Corram e vejam:
for v in grades:
    v = v + 1                   # muda só a variável v, NÃO a lista
print(grades)                   # [13, 16, 10, 19, 15]  -> igual!

# O v recebe o valor que está na lista, mas é só uma variável.
# Mudar o v não escreve nada na lista.
# Para ESCREVER numa posição da lista é preciso dizer qual: grades[i] = ...






# %%
# EXTRA (ainda não foi dado na teórica): o enumerate dá os DOIS ao mesmo tempo.

grades = [12, 15, 9]
for i, v in enumerate(grades):
    print("posição", i, "-> nota", v)

# É o mesmo que:
#     for i in range(len(grades)):
#         v = grades[i]
#
# Dá jeito quando é preciso a posição E o valor.
# Mas passa SEMPRE por todas as posições: para comparar com o vizinho
# (l[i + 1]) não serve, porque na última volta o i + 1 já sai da lista.
# É o que vem a seguir.






# %%
"""
g) Vizinhos: os limites do range [4 min -> 11:00] ------------------------

    Para comparar um elemento com o vizinho há duas formas.
    As duas estão certas. O que muda são os LIMITES do range.
"""

l = [3, 5, 5, 8]                # 4 elementos -> 3 pares de vizinhos

# a) Com o SEGUINTE (i e i + 1): o i vai de 0 até len(l) - 2.
for i in range(len(l) - 1):
    print(i, i + 1, "->", l[i], l[i + 1])

print("---")

# b) Com o ANTERIOR (i - 1 e i): o i vai de 1 até len(l) - 1.
for i in range(1, len(l)):
    print(i - 1, i, "->", l[i - 1], l[i])

# As duas dão os mesmos 3 pares: (0,1) (1,2) (2,3).
#
# a) range(len(l) - 1): o último i é len(l) - 2, e o i + 1 é o último elemento.
# b) range(1, len(l)):  começa em 1, porque o elemento 0 não tem anterior.






# %%
# Os 2 erros a evitar, para verem com os próprios olhos:

l = [3, 5, 5, 8]

# Erro 1: range(len(l)) com o ANTERIOR.
for i in range(len(l)):
    print(i - 1, i, "->", l[i - 1], l[i])   # 1ª volta: -1 0 -> 8 3

# Na 1ª volta, l[-1] é o ÚLTIMO elemento: compara o 3 com o 8!
# Não dá erro nenhum. O resultado sai errado em silêncio.
# (o pior tipo de bug: o programa corre, mas está errado)

# Erro 2: range(len(l)) com o SEGUINTE.
# for i in range(len(l)):
#     print(l[i], l[i + 1])                 # <- descomenta: IndexError na última volta






# %%
def has_two_consecutive_equal(l: list[int]) -> bool:
    """ Contains two consecutive repeating elements? """
    n = len(l)
    for i in range(n - 1):
        if l[i] == l[i + 1]:
            return True
    return False

print(has_two_consecutive_equal([1, 2, 3, 4, 5]))       # False
print(has_two_consecutive_equal([1, 2, 3, 3, 4, 5]))    # True

# Os vizinhos com o SEGUINTE, num exercício (a forma a)).
#
# PERGUNTA (do guião): porque é que aqui o range é range(n - 1),
# e não o range(n) do costume?
#
# Resposta: em cada volta compara-se l[i] com o VIZINHO da direita, l[i + 1].
# Com range(n), na última volta i = n - 1 e o vizinho seria l[n]:
# não existe -> IndexError.
# Com range(n - 1), o último i é n - 2 e o vizinho é l[n - 1], o último. Certo.
# Para n = 5, os pares de vizinhos são (0,1) (1,2) (2,3) (3,4): n - 1 voltas.
#
# E reparem no padrão "procurar e sair" (aula 3):
#   return True DENTRO do ciclo, assim que encontra;
#   return False DEPOIS do ciclo, só quando já viu todos.










# %%
# Mini: contar quantas vezes a lista SOBE (um valor maior do que o ANTERIOR).
# Desta vez com a forma b).

def count_rises(l: list[int]) -> int:
    """ How many times an element is greater than the previous one. """
    count = 0
    for i in range(1, len(l)):          # com o ANTERIOR: começa em 1
        if l[i] > l[i - 1]:
            count += 1
    return count

print(count_rises([3, 5, 5, 8, 2]))     # 2   (3 -> 5 e 5 -> 8)
print(count_rises([7]))                 # 0   (range(1, 1) é vazio)






# %%
"""
h) Provar que TODOS cumprem: flag ou contra-exemplo? [4 min -> 11:04] ----

    Pergunta: "todos os valores da lista são pares?"
    Há duas formas de pensar.
"""

# a) Forma "positiva": uma FLAG que começa True e vai sendo confirmada.
def all_even_flag(l: list[int]) -> bool:
    """ Check if all values of l are even (flag version). """
    all_ok = True
    for v in l:
        all_ok = all_ok and v % 2 == 0     # basta um False para ficar False para sempre
    return all_ok

# b) Procurar um CONTRA-EXEMPLO: sair assim que aparecer um que estraga tudo.
def all_even(l: list[int]) -> bool:
    """ Check if all values of l are even. """
    for v in l:
        if v % 2 != 0:          # encontrei um ímpar: já sei a resposta
            return False
    return True                 # SÓ no fim, ao nível do for: vi todos e nenhum falhou

print(all_even_flag([2, 4, 6]), all_even([2, 4, 6]))      # True True
print(all_even_flag([2, 3, 6]), all_even([2, 3, 6]))      # False False
print(all_even_flag([]), all_even([]))                    # True True

# As duas dão o mesmo resultado. A diferença é o TRABALHO:
#   a) a flag percorre SEMPRE a lista toda,
#      mesmo que o 1º valor já seja ímpar.
#   b) o contra-exemplo pára no 1º ímpar. Numa lista com um milhão
#      de números que começa por 1, faz 1 volta em vez de um milhão.
# Nos testes: "quanto menos trabalho desnecessário, melhor" -> b).
#
# O erro clássico da b): pôr o return True DENTRO do for (num else).
# Isso decide logo na 1ª volta, só a olhar para o 1º valor.
# O return True só pode vir DEPOIS do for: só aí já vimos todos.
#
# Lista vazia: não há nenhum contra-exemplo, logo "todos são pares" é True.






# %%
# Mini: escrevam all_positive(l) com a forma b), a do contra-exemplo.

def all_positive(l: list[int]) -> bool:
    """ Check if all values of l are positive. """
    for v in l:
        if v <= 0:              # o contrário de "> 0" é "<= 0"
            return False
    return True

print(all_positive([1, 2, 3]))          # True
print(all_positive([1, -2, 3]))         # False






# %%
"""
===========================================================================
Guião 05a, exercício 32 - Índices e valores [9 min -> 11:13]
https://ipce-184ea7.gitlab.io/
===========================================================================

EXERCICIO 32 (GUIÃO) - Valores pares vs posições pares -----------------

    32 - Numa lista, cada célula tem associado um índice e um valor.
    Este exercício serve para ajudar a não confundir os dois.
    Escreva as duas seguintes funções sobre listas de inteiros:
      - sum_even_values: soma todos os valores pares duma lista de inteiros.
      - sum_values_at_even_positions: soma todos os valores duma lista
        de inteiros com índices pares (ou seja, índices 0, 2, 4, 6, etc.)

    Espera-se que o programa escreva 1942 e 1169.

    Pista: uma das funções percorre pelos VALORES, a outra pelos ÍNDICES.
"""

type IList = list[int]          # um nome curto para "lista de inteiros" (explicado mais à frente)

def sum_even_values(l: IList) -> int:
    """ Sum all the even values. """
    total = 0
    for v in l:                     # interessa o VALOR -> 1ª técnica
        if v % 2 == 0:
            total += v
    return total

def sum_values_at_even_positions(l: IList) -> int:
    """ Sum all the values with even indexes. """
    total = 0
    for i in range(len(l)):         # interessa a POSIÇÃO -> 2ª técnica
        if i % 2 == 0:              # a POSIÇÃO é par?
            total += l[i]           # então soma-se o VALOR que lá está
    return total

    # Sem if nenhum: o range salta de 2 em 2, direto às posições pares
    # total = 0
    # for i in range(0, len(l), 2):
    #     total += l[i]
    # return total

def main() -> None:
    l = [ 34,  56,   1, -4,  3,  2, 7,  6, 66, 666,
          77, 777, 978, -2, -2, 56, 7, -1, -2,  88 ]
    print(sum_even_values(l))
    print(sum_values_at_even_positions(l))

main()

# O erro clássico: na 2ª função testar "if l[i] % 2 == 0".
#   i     -> a POSIÇÃO (0, 1, 2, ...)
#   l[i]  -> o VALOR que está nessa posição
# Leiam o enunciado com lupa: "valores pares" ou "índices pares"?
#
# A versão com range(0, len(l), 2) dá metade das voltas e não tem if.
# "Quanto menos trabalho desnecessário, melhor" (frase de todos os testes,
# lembram-se do sum_multiples da aula 3?).









# %%
"""
===========================================================================
LISTAS - mais umas peças para o ex 33 [7 min -> 11:20]
===========================================================================

a) Funções que recebem listas [2 min -> 11:15] --------------------------

    Quando chamamos uma função com uma lista, a função recebe
    a MESMA lista (não uma cópia). Se a função a alterar,
    quem a chamou vê a alteração.
"""

def fill_with_zeros(l: list[int]) -> None:
    """ Put a 0 in every position of l (changes l). """
    for i in range(len(l)):
        l[i] = 0

nums = [5, 6, 7]
fill_with_zeros(nums)
print(nums)                     # [0, 0, 0]  -> a lista de fora mudou!

# A função não tem return (-> None) e mesmo assim "deu um resultado":
# alterou a lista que recebeu.
#
# É EXATAMENTE isto que vamos usar no ex 33, depois do intervalo:
# funções como reverse(l1, l2) recebem a lista B e escrevem lá dentro.
# Por isso as listas A e B não precisam de ser variáveis globais:
# vão "viajando" de função em função como parâmetros.
#
# O porquê disto (setas, mutável e imutável) fica para o fim do ficheiro,
# depois de usarem listas a sério no ex 33: "LISTAS POR DENTRO".






# %%
"""
b) Mais umas ferramentas [3 min -> 11:18] -------------------------------
"""

print([1, 2] + [3, 4])          # [1, 2, 3, 4]        junta (cria uma lista NOVA)
print([0] * 6)                  # [0, 0, 0, 0, 0, 0]  repete
print(list(range(5)))           # [0, 1, 2, 3, 4]     lista feita a partir de um range
print(3 in [1, 3, 5])           # True                o "in" da aula 3 funciona aqui
print(7 not in [1, 3, 5])       # True

numbers = [4, 8, 1]
print(sum(numbers), max(numbers), min(numbers))     # 13 8 1   já vêm no Python

s = "ola !"
print(s[0], len(s))             # o 5   -> uma string também tem índices!
# s[0] = "O"                    # <- descomenta: TypeError (as strings não mudam)

# sum, max e min existem... mas no ex 33 vamos programá-los à mão.
# Nos testes pede-se muitas vezes "sem usar sum, max, ...":
# é para treinar o raciocínio do ciclo.
#
# Lista vs string: as duas têm índices e len, e as duas se percorrem com for.
# Mas a lista é MUTÁVEL e a string é IMUTÁVEL.






# %%
"""
c) Dar um nome a um tipo: type MyIntList [2 min -> 11:20] -------------------
"""

type MyIntList = list[int]

def first(l: MyIntList) -> int:
    """ The first element of the list.
        Precondition: len(l) > 0
    """
    return l[0]

print(first([7, 8, 9]))         # 7

# list[int] quer dizer "lista de inteiros". MyIntList é só uma ALCUNHA,
# para os cabeçalhos ficarem mais fáceis de ler.
# Fica no início do programa, na "zona das definições de tipos" (teórica 02b).
#
# Se o vosso Spyder der SyntaxError nesta linha, o Python é antigo (< 3.12).
# Nesse caso escrevam só:    MyIntList = list[int]
#
# Nos ex 32 e 33 a alcunha usada é IList (Integer List): é a mesma ideia.








# %%
"""
===========================================================================
INTERVALO [20 min -> 11:40]
===========================================================================
"""









# %%
"""
===========================================================================
Guião 05a, exercício 33 - O interpretador de listas [80 min -> 13:00]
https://ipce-184ea7.gitlab.io/
===========================================================================

Apresentação: o que é um interpretador de comandos (Mooshak M) [10 min -> 11:50]

    33 - Escreva um programa que permita ao utilizador fazer experiências
    com listas de inteiros. (Este problema vai ocupar duas aulas práticas.)

    O programa:
      - guarda duas listas de inteiros, A e B, sempre do MESMO tamanho;
      - mostra as duas listas, pede um comando (uma letra) e executa-o;
      - repete tudo até receber o comando 'z'.
    Um programa assim chama-se um INTERPRETADOR DE COMANDOS.
    (o terminal do vosso computador, a consola do Spyder, ... são interpretadores)

    O lists.py do site já vem meio feito e já corre sem erros:
      - feito (não mexer): interpreter, input_list, print_list, copy,
        draw_line e o comandos a,b,c,z;
      - por fazer: as funções dos outros 10 comandos.

        s  summation                   soma de A
        x  dot_product                 produto interno de A por B
        g  equal                       A e B são iguais?
        q  proportion_of_even_numbers  proporção de pares em A
        r  is_ascending                A é estritamente crescente?
        m  maximum                     maior valor de A
        i  reverse                     copia A para B, ao contrário
        h  print_histogram             histograma de A
        p  copy_even_numbers           copia os pares de A para o início de B
        l  longest_ascending           maior troço crescente de A (difícil)

    (se no vosso lists.py algum nome for diferente, usem o que lá está)

    Como vamos trabalhar:
      1. As duas células seguintes montam o interpretador
         com versões PROVISÓRIAS das 10 funções (só dizem "entrei aqui").
         Corremos e vemos como o interpretador funciona.
      2. Depois fazemos UMA função de cada vez, por ordem,
         e testamo-la logo com prints, SEM o interpretador (é muito mais rápido).
      3. De vez em quando voltamos a correr a célula do interpretador
         e experimentamos o comando da função que acabámos de fazer.

    A regra mais importante deste exercício:
        NUNCA criar listas novas. As funções que "dão" uma lista
        escrevem DENTRO da lista B que recebem (como o fill_with_zeros).
"""






# %%
# 1. As 10 funções, em versão PROVISÓRIA
#
# Cada uma só avisa que foi chamada e devolve um valor qualquer do tipo certo.
# Assim o interpretador já corre sem erros (tal como o lists.py do site).
# Mais abaixo, cada uma vai ser substituída pela versão a sério,
# com o MESMO nome: a última definição é a que fica.
#
# Corram esta célula UMA vez. Se a voltarem a correr depois de fazerem
# as funções a sério, estas versões provisórias passam por cima delas!

type IList = list[int]

def summation(l: IList) -> int:
    print("  (entrei na summation: ainda por fazer)")
    return 0

def dot_product(l1: IList, l2: IList) -> int:
    print("  (entrei na dot_product: ainda por fazer)")
    return 0

def equal(l1: IList, l2: IList) -> bool:
    print("  (entrei na equal: ainda por fazer)")
    return False

def proportion_of_even_numbers(l: IList) -> float:
    print("  (entrei na proportion_of_even_numbers: ainda por fazer)")
    return 0.0

def is_ascending(l: IList) -> bool:
    print("  (entrei na is_ascending: ainda por fazer)")
    return False

def maximum(l: IList) -> int:
    print("  (entrei na maximum: ainda por fazer)")
    return 0

def reverse(l1: IList, l2: IList) -> None:
    print("  (entrei na reverse: ainda por fazer)")

def print_histogram(l: IList) -> None:
    print("  (entrei na print_histogram: ainda por fazer)")

def copy_even_numbers(l1: IList, l2: IList) -> None:
    print("  (entrei na copy_even_numbers: ainda por fazer)")

def longest_ascending(l: IList) -> int:
    print("  (entrei na longest_ascending: ainda por fazer)")
    return 0






# %%
"""
    Intrepretador Feito »» Não mexer nesta célula, só correr.
"""
# 2. O interpretador e as funções que já vêm feitas no lists.py
#
# Corram primeiro a célula de cima (as funções provisórias).
# Depois corram esta e experimentem: s, m, h, depois a (e escrevam 1 2 3),
# depois c, e por fim z para sair.
# Esta é a célula a que vamos voltar sempre que acabarmos uma função.

type IList = list[int]



# Comando "a" - Ler A - Pede ao utilizador uma linha de texto com números inteiros, que são lidos e guardados dentro da lista A. (Já está feito.)
# Comando "b" - Ler B - Pede ao utilizador uma linha de texto com números inteiros, que são lidos e guardados dentro da lista B. (Já está feito.)
def input_list(l: IList, name: str) -> None:
    """ Read only the values that fit in l; if there are few values,
        fill the rest with zeros. (given)
    """
    line = input(f"Introduza os valores de {name}: ")
    s = line.split()            # parte a linha nos espaços: "1 2 3" -> ["1", "2", "3"]
    m = len(s)
    n = len(l)
    i = -1
    for i in range(min(m, n)):  # converte cada string para um inteiro
        l[i] = int(s[i])
    for j in range(i + 1, n):
        l[j] = 0

# Comando "c" - Copiar - Copia o conteúdo da lista A para a lista B. No final, as duas listas ficam iguais, claro. (Já está feito.)
def copy(l1: IList, l2: IList) -> None:
    """ Copy the contents of l1 to l2. (given) """
    n = len(l1)
    for i in range(n):
        l2[i] = l1[i]

# Funçao auxiliar para mostrar as listas no terminal (já está feita).
def print_list(l: IList, name: str) -> None:
    """ Print l. (given) """
    print(f"    {name} = {str(l)}")


# Função auxiliar para desenhar uma linha de comprimento n com o char c (já está feita).
# vai ser util para desenhar o histograma do comando h.
def draw_line(c: str, n: int) -> None:
    """ Draw a line with length n using the char c.
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()


# Interpretador de comandos (já está feito, não mexer)
def interpreter() -> None:
    """ Command interpreter for two integer lists. (given) """
    la = [1, -2, 3, -4, 40, 50]     # lista A (local ao interpreter!)
    sa = "A"                        # nome da lista A

    lb = [0, 0, 0, 0, 0, 0]         # lista B
    sb = "B"                        # nome da lista B

    # Invariante global: len(la) == len(lb)
    if len(la) != len(lb) or len(la) == 0:
        print("ERRO INTERNO")
        return

    while True:                     # repete "para sempre"... até ao return do 'z'
        print_list(la, sa)
        print_list(lb, sb)

        # recebe input do terminal
        command_line = input("> ")
        if command_line == '':
            command = ''
        else:
            command = command_line[0]   # o comando é só a 1ª letra da linha

        # escolhe o que fazer com base no comando
        if command == 'a':
            input_list(la, sa)
        elif command == 'b':
            input_list(lb, sb)
        elif command == 'c':
            print("COPIAR")
            copy(la, lb)
        elif command == 'h':
            print("HISTOGRAMA")
            print_histogram(la)
        elif command == 'i':
            print("INVERTER")
            reverse(la, lb)
        elif command == 'g':
            if equal(la, lb):
                print("IGUAIS")
            else:
                print("DIFERENTES")
        elif command == 'l':
            print(f"MAIOR ASCENDENTE = {longest_ascending(la)}")
        elif command == 'm':
            if len(la) == 0:
                print("IMPOSSÍVEL")
            else:
                print(f"MÁXIMO = {maximum(la)}")
        elif command == 'p':
            print("PARES")
            copy_even_numbers(la, lb)
        elif command == 'q':
            if len(la) == 0:
                print("IMPOSSÍVEL")
            else:
                print(f"PROPORÇÃO DE PARES = {proportion_of_even_numbers(la):.6f}")
        elif command == 'r':
            if is_ascending(la):
                print("CRESCENTE")
            else:
                print("NÃO CRESCENTE")
        elif command == 's':
            print(f"SOMA = {summation(la)}")
        elif command == 'x':
            print(f"PRODUTO = {dot_product(la, lb)}")
        elif command == 'z':
            print("Fim de execução! Volte sempre.")
            return
        elif command == '':
            pass
        else:
            print(f"Comando desconhecido: \"{command}\"")

interpreter()

# Como ler o interpreter:
#   - while True: um ciclo que repete sem condição de paragem
#     (vem na teórica 04a; aqui só sai com o return do comando 'z');
#   - em cada volta: mostra A e B, lê uma linha, fica com a 1ª letra;
#   - uma fila de if/elif escolhe o que fazer com essa letra.
#     Cada ramo CHAMA uma função e ESCREVE o resultado.
#
# Reparem na divisão de trabalho:
#   - o interpreter só faz a CONVERSA: lê comandos, chama funções, escreve.
#   - as nossas funções só fazem CONTAS (ou mexem nas listas). Sem input, sem print
#     (a única exceção é o histograma, cuja missão é desenhar).
# É a regra "lógica de um lado, conversa do outro", num programa a sério.
#
# A e B são variáveis LOCAIS do interpreter, e não globais.
# São passadas às funções como parâmetros: summation(la), reverse(la, lb), ...
# Por isso podemos testar cada função com as listas que quisermos,
# sem o interpretador: summation([1, 2, 3]) funciona exatamente igual.
#
# O "IMPOSSÍVEL" no m e no q: o interpreter verifica a precondição len > 0
# ANTES de chamar a função. A função confia; quem chama verifica.







# %%
"""
Comando s - Soma [4 min -> 11:54] ---------------------------------------

    s - Soma - Determina a soma de todos os valores de A.
    Desafio: não usar a função sum do Python.
    Exemplo: A = [1, 2, 5, 7, 9, 100]  ->  SOMA = 124
"""

type IList = list[int]

def summation(l: IList) -> int:
    """ Sum of the values of l. """
    total = 0
    for v in l:
        total += v
    return total

# Testes locais: chamar a função diretamente, sem o interpretador
print(summation([1, 2, 5, 7, 9, 100]))      # 124
print(summation([1, -2, 3, -4, 40, 50]))    # 88

# Só interessam os valores -> 1ª técnica (for v in l).
# É o average de antes do intervalo, sem a divisão no fim.
#
# A solução oficial chama "sum" à variável. Funciona, mas dentro da função
# a variável tapa a função sum do Python. Usem nomes vossos: total, acc, soma...
#
# Agora voltem à célula do interpretador, corram-na e usem o comando s.









# %%
"""
Comando x - Produto interno [5 min -> 11:59] ----------------------------

    x - Produto interno - Calcula o produto interno de A por B:
        a1*b1 + ... + an*bn.
    Exemplo do guião:
        A = [1, -2, 3, -4, 40, 50]
        B = [-2, -4, 40, 50, 0, 0]    ->  PRODUTO = -74
    Pista: aqui o "for v in l1" não chega. Porquê?
"""

type IList = list[int]

def dot_product(l1: IList, l2: IList) -> int:
    """ Dot product of l1 and l2.
        Precondition: len(l1) == len(l2)
    """
    total = 0
    for i in range(len(l1)):
        total += l1[i] * l2[i]
    return total

print(dot_product([1, -2, 3, -4, 40, 50], [-2, -4, 40, 50, 0, 0]))    # -74
print(dot_product([1, 2, 3], [4, 5, 6]))                              # 32

# É preciso andar nas DUAS listas ao mesmo tempo, posição a posição.
# Um "for v in l1" só dá os valores de l1: não sabemos onde estamos na l2.
# Com o índice i, temos l1[i] e l2[i] lado a lado -> 2ª técnica.
#
# Conta à mão:
#   1*(-2) + (-2)*(-4) + 3*40 + (-4)*50 + 40*0 + 50*0
#  =  -2   +    8      + 120  +  -200   +  0   +  0    = -74








# %%
"""
Comando g - Iguais? [6 min -> 12:05] ------------------------------------

    g - Iguais - Testa se A e B são iguais.
    Desafio: não usar l1 == l2. Comparar os conteúdos, posição a posição.
    (neste programa as duas listas têm sempre o mesmo tamanho)

    Pensem antes de escrever:
      - para responder "SÃO IGUAIS", é preciso ver quantas posições?
      - e para responder "SÃO DIFERENTES"?
"""

type IList = list[int]

def equal(l1: IList, l2: IList) -> bool:
    """ Check if l1 and l2 have the same contents.
        Precondition: len(l1) == len(l2)
    """
    for i in range(len(l1)):
        if l1[i] != l2[i]:
            return False        # basta UMA diferença: já sei a resposta
    return True                 # vi TODAS as posições e não havia diferenças

print(equal([1, -2, 3], [1, -2, 3]))       # True
print(equal([1, -2, 3], [1, -2, 4]))       # False

# É o padrão "TODOS?" (o espelho do "procurar e sair"):
#   procura-se um CONTRA-EXEMPLO; se aparecer, return False logo ali;
#   o return True só pode vir DEPOIS do ciclo.
#
# "SÃO DIFERENTES" pode decidir-se a meio. "SÃO IGUAIS" só no fim.



# O erro clássico (está na teórica 04a): decidir logo na 1ª volta.
def equal_wrong(l1: IList, l2: IList) -> bool:
    """ Check if l1 and l2 have the same contents (WITH A BUG). """
    for i in range(len(l1)):
        if l1[i] != l2[i]:
            return False
        else:
            return True         # <- só olhou para a posição 0 e já respondeu!

print(equal_wrong([1, -2, 3], [1, -2, 4]))     # True (!!) devia ser False









# %%
"""
Comando q - Proporção de pares [5 min -> 12:10] ------------------------

    q - Proporção - Calcula a proporção de números pares em A.
    O resultado é um valor real entre 0.0 e 1.0,
    escrito com o formato ":.6f".
    Exemplo: A = [1, -2, 3, -4, 40, 50]  ->  PROPORÇÃO DE PARES = 0.666667
    Pistas: um contador e uma divisão. E o -2, é par?
"""

type IList = list[int]

def proportion_of_even_numbers(l: IList) -> float:
    """ Proportion of even numbers in l.
        Precondition: len(l) > 0
    """
    even = 0
    for v in l:
        if v % 2 == 0:
            even += 1
    return even / len(l)

p = proportion_of_even_numbers([1, -2, 3, -4, 40, 50])
print(p)                        # 0.6666666666666666
print(f"{p:.6f}")               # 0.666667

# 4 pares (-2, -4, 40, 50) em 6 valores: 4 / 6.
# Os negativos também podem ser pares: -2 % 2 dá 0.
#
# A função DEVOLVE o número. Quem o escreve com ":.6f" é o interpreter.
#
# Precondição len(l) > 0: com a lista vazia seria 0 / 0.






# %%
"""
Comando r - Crescente? [6 min -> 12:16] ---------------------------------

    r - Crescente - Testa se A consiste numa sequência crescente
    em sentido estrito.
    ("estrito": cada valor tem de ser MAIOR do que o anterior,
     "igual" já não serve)
    Exemplos:
        [1, 2, 5, 7, 9, 100]      ->  CRESCENTE
        [1, -2, 3, -4, 40, 50]    ->  NÃO CRESCENTE
        [1, 2, 2, 3]              ->  NÃO CRESCENTE (2 não é maior do que 2)
    Pista: juntem duas coisas que já fizemos hoje:
    os vizinhos do has_two_consecutive_equal e o "todos?" do equal.
"""

type IList = list[int]

def is_ascending(l: IList) -> bool:
    """ Check if l is strictly ascending. """
    for i in range(len(l) - 1):     # atenção ao "- 1": vamos olhar para o vizinho
        if l[i] >= l[i + 1]:        # encontrei um par que NÃO sobe
            return False
    return True

print(is_ascending([1, 2, 5, 7, 9, 100]))      # True
print(is_ascending([1, -2, 3, -4, 40, 50]))    # False
print(is_ascending([1, 2, 2, 3]))              # False
print(is_ascending([7]))                       # True

# Queremos "l[i] < l[i + 1]" em TODOS os pares de vizinhos.
# Procuramos o contra-exemplo: o contrário de "<" é ">=" (e não ">").
# Com ">" em vez de ">=", o [1, 2, 2, 3] passava por crescente.
#
# Lista com 1 elemento: range(0) é vazio, não há pares, devolve True.
# Não é preciso nenhum if especial para esse caso.
#
# São o g) e o h) de antes do intervalo juntos: vizinhos com o SEGUINTE
# (range(len(l) - 1)) + procurar o contra-exemplo.








# %%
"""
Comando m - Máximo [6 min -> 12:22] -------------------------------------

    m - Máximo - Determina qual o maior valor que aparece em A.
    Desafio: não usar max(l).
    Exemplo: A = [1, -2, 3, -4, 40, 50]  ->  MÁXIMO = 50
    Pergunta: com que valor deve começar a variável "maior até agora"?
    (testem também com uma lista só de negativos, por exemplo [-5, -2, -9])
"""

type IList = list[int]

def maximum(l: IList) -> int:
    """ Largest value of l.
        Precondition: len(l) > 0
    """
    largest = l[0]              # começar com um valor da PRÓPRIA lista
    for v in l:
        if v > largest:
            largest = v         # aqui é SUBSTITUIR (=), não acumular (+=)
    return largest

print(maximum([1, -2, 3, -4, 40, 50]))     # 50
print(maximum([-5, -2, -9]))               # -2






# %%
# O erro clássico: começar com 0.
type IList = list[int]          # (repetido para esta célula correr sozinha)

def maximum_wrong(l: IList) -> int:
    """ Largest value of l (WITH A BUG). """
    largest = 0
    for v in l:
        if v > largest:
            largest = v
    return largest

print(maximum_wrong([-5, -2, -9]))         # 0 (!!) um valor que nem está na lista

# Se todos forem negativos, nenhum é maior do que 0, e o 0 "ganha".
# Começando com l[0], o "maior até agora" é sempre um valor que existe.
# Daí a precondição len(l) > 0: com a lista vazia, l[0] dava IndexError.
# (o interpreter verifica isso antes de chamar: escreve IMPOSSÍVEL)







# %%
"""
Comando i - Inverter [6 min -> 12:28] -----------------------------------

    i - Inverter - Copia o conteúdo da lista A para a lista B,
    invertendo a ordem dos elementos.
    Desafio: não usar l.reverse(). E não criar listas novas:
    escrever DENTRO da l2, que já existe e tem o mesmo tamanho.
    Exemplo: A = [1, 2, 5, 7, 9, 100]  ->  B = [100, 9, 7, 5, 2, 1]

    Pista: completem esta tabela (n = 6).
    O valor da posição i de A vai para que posição de B?
        i  |  posição em B
        0  |      5
        1  |      4
        2  |      ?
        i  |      ?
"""

type IList = list[int]

def reverse(l1: IList, l2: IList) -> None:
    """ Copy l1 into l2 in reverse order.
        Precondition: len(l1) == len(l2)
    """
    n = len(l1)
    for i in range(n):
        l2[n - 1 - i] = l1[i]

a = [1, 2, 5, 7, 9, 100]
b = [0, 0, 0, 0, 0, 0]
reverse(a, b)
print(a, b)                     # [1, 2, 5, 7, 9, 100] [100, 9, 7, 5, 2, 1]

# A tabela completa (n = 6):  0 -> 5,  1 -> 4,  2 -> 3,  ...,  i -> n - 1 - i
# Truque para descobrir a fórmula: a soma das duas posições é sempre 5 = n - 1.
#
# Não há return (-> None): a função "devolve" o resultado escrevendo na l2.
# É como o fill_with_zeros de antes do intervalo:
# a l2 é a MESMA lista que o b de fora, por isso o b vê a mudança.
#
# Para testar uma função que MUDA uma lista: criar a lista antes,
# chamar a função, e só depois fazer print da lista.








# %%
# Curiosidade: e se chamarmos reverse(a, a)?
# Se passarmos a mesma lista (ou 2 nomes diferentes mas a apontar para a mesma lista):

def reverse(l1: list[int], l2: list[int]) -> None:
    """ Copy l1 into l2 in reverse order (the same as above).
        Precondition: len(l1) == len(l2)
    """
    n = len(l1)
    for i in range(n):
        l2[n - 1 - i] = l1[i]

a = [1, 2, 3, 4, 5, 6]
reverse(a, a)
print(a)                        # [1, 2, 3, 3, 2, 1]  (!!) e não [6, 5, 4, 3, 2, 1]

# A meio do ciclo, a função já está a ler posições que ELA PRÓPRIA estragou.
#   i = 0: a[5] = a[0] -> [1, 2, 3, 4, 5, 1]   (o 6 perdeu-se para sempre)
#   i = 1: a[4] = a[1] -> [1, 2, 3, 4, 2, 1]
#   i = 2: a[3] = a[2] -> [1, 2, 3, 3, 2, 1]
#   i = 3, 4, 5: copiam valores que já são iguais
# É por isso que o ex 33 tem DUAS listas: lê-se de uma, escreve-se na outra.







# %%
"""
Comando h - Histograma [6 min -> 12:34] ---------------------------------

    h - Histograma - Desenha o histograma da sequência de valores em A,
    com linhas horizontais de asteriscos.
    Qualquer valor negativo é tratado como zero e qualquer valor
    maior do que 40 é tratado como 40.
    Usem a função draw_line, que já vem feita no lists.py.
    Exemplo: A = [1, 2, 5, 7, 9, 100]
        *
        **
        *****
        *******
        *********
        ****************************************     (40, e não 100)
"""

type IList = list[int]

def draw_line(c: str, n: int) -> None:
    """ Draw a line with length n using the char c. (given, repeated here
        so that this cell also runs on its own)
        Precondition: len(c) == 1 and n >= 0
    """
    for i in range(n):
        print(c, end='')
    print()

def print_histogram(l: IList) -> None:
    """ Histogram of l, one line of '*' per value, between 0 and 40. """
    MAX_HISTOGRAM = 40              # constante local: sem números mágicos
    for v in l:
        size = v
        if size < 0:
            size = 0
        elif size > MAX_HISTOGRAM:
            size = MAX_HISTOGRAM
        draw_line('*', size)

        # Numa linha só, com as funções max e min do Python:
        # draw_line('*', min(max(v, 0), MAX_HISTOGRAM))

print_histogram([1, 2, 5, 7, 9, 100])
print("---")
print_histogram([-3, 0, 3])         # linha vazia, linha vazia, ***

# Esta função não calcula nada: a missão dela é ESCREVER (-> None).
# O trabalho de desenhar uma linha está noutra função (draw_line):
# cada função faz uma coisa só, e o print_histogram lê-se como português.
#
# "Encostar" um valor a um intervalo [0, 40] é um truque que aparece muito
# (em inglês diz-se "clamp").






# %%
# Desafio: e como criar o histogram de uma lista?

type IList = list[int]          # (repetido para esta célula correr sozinha)

def build_histogram(l: IList) -> IList:
    """ Build a histogram of l, as a list of counts.
        Values < 0 are treated as 0, values > 10 are treated as 10.
        The result is a list of 11 integers, where the value at index i
        is the number of times i appears in l (after clamping).
    """
    MAX_HISTOGRAM = 10
    histogram = [0] * (MAX_HISTOGRAM + 1)  # lista de contadores, inicializada a 0
    for v in l:
        size = v
        if size < 0:
            size = 0
        elif size > MAX_HISTOGRAM:
            size = MAX_HISTOGRAM
        histogram[size] += 1                # incrementa o contador do tamanho
    return histogram

print(build_histogram([1, 2, 5, 7, 9, 100]))
print("---")
print(build_histogram([-3, 0, 3]))







# %%
"""
Comando p - Copiar os pares [8 min -> 12:42] ----------------------------

    p - Pares - Copia para a parte inicial de B todos os valores pares de A.
    A ordem original dos valores pares dentro de A mantém-se dentro de B.
    O resto de B é preenchido com zeros.
    Exemplo: A = [1, -2, 3, -4, 40, 50]  ->  B = [-2, -4, 40, 50, 0, 0]

    Pista: são precisos DOIS índices:
      - um que anda em A (o do for), de 1 em 1;
      - outro que diz ONDE escrever em B, e que só avança quando se escreve.
    E no fim? As posições de B que sobraram têm de ficar a 0
    (o B pode ter "lixo" de comandos anteriores!).
"""

type IList = list[int]

def copy_even_numbers(l1: IList, l2: IList) -> None:
    """ Copy the even values of l1 to the beginning of l2, keeping the order.
        The remaining positions of l2 become 0.
        Precondition: len(l1) == len(l2)
    """
    n = len(l1)
    j = 0                           # próxima posição LIVRE de l2
    for i in range(n):              # i anda em l1, sempre
        if l1[i] % 2 == 0:
            l2[j] = l1[i]
            j += 1                  # j só anda quando se escreve
    for k in range(j, n):           # limpar o resto de l2
        l2[k] = 0

a = [1, -2, 3, -4, 40, 50]
b = [9, 9, 9, 9, 9, 9]              # B com "lixo", para testar a limpeza
copy_even_numbers(a, b)
print(b)                            # [-2, -4, 40, 50, 0, 0]

# Executar à mão:
#   i | l1[i] | par? | escreve em | j depois
#   0 |   1   | não  |     -      |    0
#   1 |  -2   | sim  |   l2[0]    |    1
#   2 |   3   | não  |     -      |    1
#   3 |  -4   | sim  |   l2[1]    |    2
#   4 |  40   | sim  |   l2[2]    |    3
#   5 |  50   | sim  |   l2[3]    |    4
#   limpar: k = 4, 5 -> l2[4] = 0, l2[5] = 0
#
# O j é ao mesmo tempo "onde escrevo a seguir" e "quantos pares já copiei".
# Sem a limpeza final, o B ficava [-2, -4, 40, 50, 9, 9].








# %%
"""
Comando l - A subida mais longa (difícil) [10 min -> 12:52] -------------

    l - Longa - Calcula o comprimento da subsequência contínua crescente
    mais longa que ocorre em A. (Difícil.)
    Exemplos:
        [1, -2, 3, -4, 40, 50]    ->  3   (o troço -4, 40, 50)
        [1, 2, 5, 7, 9, 100]      ->  6   (a lista toda)
        [5, 4, 3]                 ->  1   (cada número sozinho é um troço de 1)

    Pista: andar pelos vizinhos (como no is_ascending) com DUAS variáveis:
      - o tamanho do troço crescente ATUAL;
      - o MAIOR tamanho visto até agora.
    O que acontece ao troço atual quando a subida se interrompe?
"""

type IList = list[int]

def longest_ascending(l: IList) -> int:
    """ Length of the longest strictly ascending run of consecutive values.
        Precondition: len(l) > 0
    """
    count = 1                       # o troço atual: um valor sozinho já conta 1
    largest = 1                     # o melhor troço visto até agora
    for i in range(len(l) - 1):
        if l[i] < l[i + 1]:         # a subida continua
            count += 1
            if count > largest:
                largest = count
        else:                       # a subida interrompeu-se: recomeçar
            count = 1
    return largest

print(longest_ascending([1, -2, 3, -4, 40, 50]))   # 3
print(longest_ascending([1, 2, 5, 7, 9, 100]))     # 6
print(longest_ascending([5, 4, 3]))                # 1

# Executar à mão [1, -2, 3, -4, 40, 50]:
#   par        | sobe? | count | largest
#   (1, -2)    |  não  |   1   |   1
#   (-2, 3)    |  sim  |   2   |   2
#   (3, -4)    |  não  |   1   |   2
#   (-4, 40)   |  sim  |   2   |   2
#   (40, 50)   |  sim  |   3   |   3
#
# Junta 3 padrões de hoje:
#   - vizinhos l[i] e l[i + 1]       (is_ascending)
#   - um contador que RECOMEÇA       (count = 1 no else)
#   - o "maior até agora"            (maximum)
#
# O "count = 1" aparece em DOIS sítios, com papéis diferentes:
#   antes do for -> valor inicial;   no else -> recomeçar um troço novo.












# %%
"""
Juntar tudo e submeter no Mooshak [8 min -> 13:00] ----------------------

    Agora todas as funções a sério estão feitas.
    Corram esta célula (que vai chamar o interpretador)
    (corram primeiro a célula do interpretador e as das funções a sério)
    e testem as funções todas com os comandos dados:

    experimentem a sequência do guião:

    Exemplo de execução

    >>> interpreter()
        A = [1, -2, 3, -4, 40, 50]
        B = [0, 0, 0, 0, 0, 0]
    > m
    MÁXIMO = 50
        A = [1, -2, 3, -4, 40, 50]
        B = [0, 0, 0, 0, 0, 0]
    > p
    PARES
        A = [1, -2, 3, -4, 40, 50]
        B = [-2, -4, 40, 50, 0, 0]
    > q
    PROPORÇÃO DE PARES = 0.666667
        A = [1, -2, 3, -4, 40, 50]
        B = [-2, -4, 40, 50, 0, 0]
    > x
    PRODUTO = -74
        A = [1, -2, 3, -4, 40, 50]
        B = [-2, -4, 40, 50, 0, 0]
    > g
    DIFERENTES
        A = [1, -2, 3, -4, 40, 50]
        B = [-2, -4, 40, 50, 0, 0]
    > c
    COPIAR
        A = [1, -2, 3, -4, 40, 50]
        B = [1, -2, 3, -4, 40, 50]
    > g
    IGUAIS
        A = [1, -2, 3, -4, 40, 50]
        B = [1, -2, 3, -4, 40, 50]
    > r
    NÃO CRESCENTE
        A = [1, -2, 3, -4, 40, 50]
        B = [1, -2, 3, -4, 40, 50]
    > l
    MAIOR ASCENDENTE = 3
        A = [1, -2, 3, -4, 40, 50]
        B = [1, -2, 3, -4, 40, 50]
    > a
    Introduza os valores de A: 1 2 5 7 9 100
        A = [1, 2, 5, 7, 9, 100]
        B = [1, -2, 3, -4, 40, 50]
    > r
    CRESCENTE
        A = [1, 2, 5, 7, 9, 100]
        B = [1, -2, 3, -4, 40, 50]
    > h
    HISTOGRAMA
    *
    **
    *****
    *******
    *********
    ****************************************
        A = [1, 2, 5, 7, 9, 100]
        B = [1, -2, 3, -4, 40, 50]
    > l
    MAIOR ASCENDENTE = 6
        A = [1, 2, 5, 7, 9, 100]
        B = [1, -2, 3, -4, 40, 50]
    > s
    SOMA = 124
        A = [1, 2, 5, 7, 9, 100]
        B = [1, -2, 3, -4, 40, 50]
    > i
    INVERTER
        A = [1, 2, 5, 7, 9, 100]
        B = [100, 9, 7, 5, 2, 1]
    > z
    Fim de execução! Volte sempre.

"""

# Corram primeiro a célula do interpretador (para ele existir)
# e as células dos comandos (para as funções a sério existirem).
# se aparecer "entrei na ...", há uma função que ainda está provisória

interpreter()















# %%
"""
===========================================================================
LISTAS POR DENTRO: setas, mutável e imutável
(para casa, ou para o início da próxima aula)
===========================================================================

    Agora que já usaram listas a sério no ex 33,
    vamos ver o que está a acontecer por dentro.

    Cada bloco é um mini-exercício. Antes de correr, prevejam o print.
"""






# %%
# 1. Com números: o "y = 6" só muda o y
x = 5
y = x
y = 6
print(x, y)                     # 5 6 -> mudar o y não mexe no x






# %%
# 2. Com listas: "b = a" NÃO copia a lista
a = [1, 2, 3]
b = a
b[0] = 99
print(a, b)                     # [99, 2, 3] [99, 2, 3]  (!!) o a também mudou






# %%
# 3. O mesmo, com append
a = [1, 2, 3]
b = a
b.append(4)
print(a, b)                     # [1, 2, 3, 4] [1, 2, 3, 4]






# %%
# 4. Com listas: DAR UMA LISTA NOVA ao b (reatribuir)
a = [1, 2, 3]
b = a
b = [7, 8, 9]
print(a, b)                     # [1, 2, 3] [7, 8, 9]  -> agora o a não mudou!






# %%
# 5. Uma cópia a sério
a = [1, 2, 3]
c = a.copy()
c[0] = 0
print(a, c)                     # [1, 2, 3] [0, 2, 3]






# %%
# 6. Igual (==) vs a MESMA lista (is)
a = [1, 2, 3]
b = a
c = a.copy()
print(a == b, a is b)           # True True   -> mesmos valores E a mesma lista
print(a == c, a is c)           # True False  -> mesmos valores, listas diferentes



# O que está a acontecer?
#
# Uma variável não é uma caixa com o valor lá dentro.
# É um NOME com uma SETA para o valor (a teórica chama-lhe uma REFERÊNCIA).
#
#   a = [1, 2, 3]    a ---> [1, 2, 3]
#   b = a            a ---> [1, 2, 3] <--- b      (não se copia nada: 2 setas, 1 lista)
#
# A partir daqui há duas coisas MUITO diferentes:
#
#   b[0] = 99  /  b.append(4)     -> entra na lista e MUDA-A.
#                                    As duas setas apontam para lá: o a vê a mudança.
#
#   b = [7, 8, 9]                 -> cria uma lista nova e muda só a SETA do b.
#                                    O a continua a apontar para a lista antiga.
#
# A pergunta a fazer em cada linha:
#   "isto muda uma SETA (nome = ...) ou muda a LISTA (nome[i] = ..., nome.append)?"
#
# E os números? "y = 6" é sempre "mudar a seta": um número nunca se altera.
# Por isso com números nunca há surpresas. Com listas há.
#
# .copy() cria uma lista NOVA com os mesmos valores: duas listas, cada uma com a sua seta.
# ==  pergunta "têm os mesmos valores?"
# is  pergunta "é a mesma lista?" (as setas vão dar ao mesmo sítio?)






# %%
# 7. O "+" entre listas cria uma lista nova!!
# Mais um, estilo pergunta 1 dos testes. O que escreve?

x = [1, 2]
y = x
x.append(3)
y = y + [4]
print(x, y)                     # [1, 2, 3] [1, 2, 3, 4]

# Linha a linha:
#   y = x          -> 2 setas para a lista [1, 2]
#   x.append(3)    -> MUDA essa lista: [1, 2, 3] (o y vê, é a mesma)
#   y = y + [4]    -> o + cria uma lista NOVA [1, 2, 3, 4]; só a seta do y muda
# No fim: x aponta para a antiga [1, 2, 3], y para a nova [1, 2, 3, 4].






# %%
"""
Funções que recebem listas: o que se vê cá fora? ----------------------

    Antes do intervalo vimos o fill_with_zeros: a função mudou a lista de fora.
    Aqui estão os três casos em que NADA muda cá fora. Porquê?
"""






# %%
def fill_with_zeros_wrong(l: list[int]) -> None:
    """ Try to put a 0 in every position of l (WITH A BUG). """
    for v in l:
        v = 0                   # só muda a SETA do v, a lista fica igual

nums = [5, 6, 7]
fill_with_zeros_wrong(nums)
print(nums)                     # [5, 6, 7]  -> nada mudou

# Para MUDAR uma lista é preciso o ÍNDICE: l[i] = ...






# %%
def try_to_replace(l: list[int]) -> None:
    """ Try to replace l by another list (does NOTHING outside). """
    l = [0, 0, 0]               # muda só a seta do parâmetro l (que é local)

nums = [5, 6, 7]
try_to_replace(nums)
print(nums)                     # [5, 6, 7]






# %%
def add_one(n: int) -> None:
    """ Try to add one to n (does NOTHING outside). """
    n += 1

k = 5
add_one(k)
print(k)                        # 5  -> com números, nada sai da função sem return

# Resumo: a função recebe uma seta para a lista de quem a chamou.
#   - mudar o CONTEÚDO (l[i] = ..., l.append) -> vê-se cá fora;
#   - reatribuir o parâmetro (l = ...)        -> não se vê cá fora;
#   - números: nunca se veem cá fora; só saem com return.






# %%
"""
RESUMO: as duas regras que explicam TUDO isto ----------------------------
"""

a = [1, 2]
b = a
a += [3]                        # com listas, o += ALTERA a lista (como o append)
print(a, b)                     # [1, 2, 3] [1, 2, 3]

a = [1, 2]
b = a
a = a + [3]                     # o + cria uma lista NOVA; só a seta do a muda
print(a, b)                     # [1, 2, 3] [1, 2]

# REGRA 1:  "nome = ..." só MUDA A SETA desse nome.
#           Nunca altera nenhum objeto. Vale para números, strings e listas.
#
# REGRA 2:  só os objetos MUTÁVEIS (as listas) se alteram por dentro,
#           e só com:   l[i] = ...    l.append(...)    l.pop()    l += [...]
#           Todos os nomes que apontam para essa lista veem a alteração.
#
# TRUQUE:   chamar f(x) é fazer "parâmetro = x" escondido, no início da função.
#           A partir daí valem as regras 1 e 2:
#             - a função altera a lista (regra 2)   -> vê-se cá fora;
#             - a função faz "l = ..." (regra 1)    -> não se vê cá fora.
#
#   IMUTÁVEIS (nunca mudam):     int, float, bool, str, tuplo
#   MUTÁVEIS (mudam por dentro): list
#
# Os imutáveis não têm operações da regra 2.
# A única forma de os "mudar" é a regra 1 (mudar a seta).
# Por isso, com números e strings nunca há surpresas. Com listas há.
#
# A pergunta a fazer em cada linha:
#   "isto muda uma SETA (regra 1) ou muda a LISTA (regra 2)?"
#
# Cuidado com o +=, que parece igual mas não é:
#   x += 1       (número) -> regra 1: é o mesmo que x = x + 1, seta nova.
#   l += [4]     (lista)  -> regra 2: altera a lista que já existe.
#   l = l + [4]  (lista)  -> regra 1: o + cria uma lista NOVA.
#
# Para ver as setas a mexer: Python Tutor (https://pythontutor.com).






# %%
"""
===========================================================================
CONSOLIDAR - exercícios de testes anteriores
(se sobrar tempo na aula; senão, ficam para casa - todos com solução)
===========================================================================

    Tentem primeiro sem olhar para a solução:
    é exatamente este o nível do teste.
"""






# %%
"""
PERGUNTA DE TESTE (Teste 1 2025/26, pergunta 1c) - executar à mão ---------

    Escreva o resultado da seguinte chamada:

        accumulation([1,1,1,1,1,1,1,0]) = ____________

    Sem correr! Façam a tabela: i | l[i-1] | l[i] antes | l[i] depois
    Atenção: a lista vai mudando DURANTE o ciclo.
"""

def accumulation(l: list[int]) -> list[int]:
    for i in range(1, len(l), 1):
        l[i] = l[i] + l[i-1]
    return l















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================















# %%
# Solução: accumulation (Teste 1 2025/26, 1c)

def accumulation(l: list[int]) -> list[int]:
    for i in range(1, len(l), 1):
        l[i] = l[i] + l[i-1]
    return l

print(accumulation([1, 1, 1, 1, 1, 1, 1, 0]))     # [1, 2, 3, 4, 5, 6, 7, 7]

# Resposta: [1, 2, 3, 4, 5, 6, 7, 7]
#
#   i | l[i-1] (JÁ atualizado) | l[i] antes | l[i] depois
#   1 |          1             |     1      |     2
#   2 |          2             |     1      |     3
#   3 |          3             |     1      |     4
#   ...
#   7 |          7             |     0      |     7
#
# A armadilha: usar o valor ORIGINAL do vizinho (daria [1,2,2,2,2,2,2,1]).
# O l[i-1] já foi alterado na volta anterior: cada posição fica com
# a soma de tudo o que está para trás (somas acumuladas).
#
# E reparem: a função ALTERA a lista que recebe e ainda a devolve.
# É o reverse(a, a) da aula de hoje outra vez: ler e escrever na mesma lista.






# %%
"""
EXERCICIO (TESTE 1 2024/25, pergunta 4, 3 valores) - Todos iguais ----------

    Escreva uma função booleana para testar se todos os valores duma lista
    de inteiros são iguais entre si. Exemplos:
        all_the_same([5,5,5,5,5]) == True      all_the_same([]) == True
        all_the_same([5,5,6,5,5]) == False     all_the_same([10]) == True

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def all_the_same(l: list[int]) -> bool:
            ''' Check if all the elements are the same. '''
"""















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================















# %%
# Solução: todos iguais (Teste 1 2024/25, pergunta 4)

def all_the_same(l: list[int]) -> bool:
    """ Check if all the elements are the same. """
    for i in range(len(l) - 1):
        if l[i] != l[i + 1]:
            return False
    return True

print(all_the_same([5, 5, 5, 5, 5]), all_the_same([5, 5, 6, 5, 5]),
      all_the_same([]), all_the_same([10]))       # True False True True

# É o is_ascending com "!=" em vez de ">=": o padrão "todos?" com vizinhos.
# Se todos os vizinhos são iguais, são todos iguais.
#
# Lista vazia e lista de 1 elemento: range(-1) e range(0) são vazios,
# o ciclo não corre e devolve True. Os dois casos especiais saem "de graça".
#
# Outra ideia: comparar todos com o primeiro.
#     for v in l:
#         if v != l[0]: ...
# Funciona, mas com a lista vazia o l[0] dá IndexError: precisava de um if extra.






# %%
"""
EXERCICIO (EXAME DE RECURSO 2023/24, pergunta 3, 2.5 valores) - Posições pares

    Escreva uma função inteira para contar o número de vezes que um dado
    caráter ocorre em posições de índice par numa lista de carateres.
    Atenção, que o número zero também é par. Exemplos:
        how_many('a', ['a','o','l','a','e','o','a']) == 2
        how_many('a', ['z','a','o','a','e','a','x']) == 0

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def how_many(c: str, l: list[str]) -> int:

    (é o ex 32 outra vez, disfarçado)
"""















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================















# %%
# Solução: posições pares (Exame de Recurso 2023/24, pergunta 3)

def how_many(c: str, l: list[str]) -> int:
    """ Number of times c occurs at even positions of l. """
    count = 0
    for i in range(0, len(l), 2):       # só as posições pares: 0, 2, 4, ...
        if l[i] == c:
            count += 1
    return count

print(how_many('a', ['a', 'o', 'l', 'a', 'e', 'o', 'a']))   # 2  (posições 0 e 6)
print(how_many('a', ['z', 'a', 'o', 'a', 'e', 'a', 'x']))   # 0  (só aparece nas ímpares)

# Interessa a POSIÇÃO -> percorrer pelos índices.
# O range com passo 2 é o "menos trabalho desnecessário":
# metade das voltas e nenhum "if i % 2 == 0".






# %%
"""
EXERCICIO (EXAME DE RECURSO 2023/24, pergunta 5, 2.5 valores) - Médias parciais

    Escreva uma função que, dada uma lista de inteiros l, retorne uma nova
    lista m, constituída pelas médias parciais da lista original.
    Para cada i, o valor que fica em m[i] é a média dos valores
    l[0], l[1], ..., l[i]. Exemplos:
        partial_averages([1,2,6,9,2]) == [1.0,1.5,3.0,4.5,4.0]
        partial_averages([]) == []        partial_averages([17]) == [17.0]

    Quanto mais simples for a função e quanto menos trabalho desnecessário
    ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.

        def partial_averages(l: list[int]) -> list[float]:

    Aqui é ao contrário do ex 33: pede-se uma lista NOVA.
    Pista: não recalculem a soma desde o início em cada posição.
"""















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreitem: tentem primeiro!
# ======================================================================















# %%
# Solução: médias parciais (Exame de Recurso 2023/24, pergunta 5)

def partial_averages(l: list[int]) -> list[float]:
    """ New list where position i holds the average of l[0], ..., l[i]. """
    result = []
    total = 0
    for i in range(len(l)):
        total += l[i]                   # a soma vai crescendo: não se recomeça
        result.append(total / (i + 1))  # i + 1 valores somados até agora
    return result

print(partial_averages([1, 2, 6, 9, 2]))    # [1.0, 1.5, 3.0, 4.5, 4.0]
print(partial_averages([]))                 # []
print(partial_averages([17]))               # [17.0]

# Dois acumuladores ao mesmo tempo:
#   total  -> soma de tudo até agora (um número)
#   result -> a lista nova, que começa vazia e cresce com append
#
# A versão "pesada" somava de novo l[0] + ... + l[i] em cada posição:
# um ciclo dentro de um ciclo. Esta faz UMA conta por posição.
# (é o padrão "reaproveitar o que já se calculou" das revisões do início)
#
# Lista vazia: o ciclo não corre e devolve []. Sem if especial.
# (no enunciado do exame o nome aparecia com uma gralha: "parcial_averages")






# %%
"""
===========================================================================
PARA TREINAR MAIS (em casa)
===========================================================================

    A matéria desta aula está explicada com calma, com muitos exercícios
    com solução (por níveis: exemplos, exercícios, desafios de teste), em:

        revisoes_4_ciclos_for_e_listas.py          secções 12, 13 e 14 (listas, in, percorrer)
                                                   (Ex 14.2 e 14.7: valores vs índices, vizinhos, "todos?")
        revisoes_5_valores_referencias_escopo.py   secções 15, 18 e 19 (setas, funções com listas)

    E o Mooshak M (ex 33): acabem os comandos que faltarem e submetam.
"""