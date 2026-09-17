# %%
"""
===========================================================================
GUIA DE SESSÃO — Prática 02 (P1, 2a-feira 10:10-13:00) — IPCE 2026/2027
===========================================================================
Cobre os guiões 02a (ex. 9-15) e 02b (ex. 16-18).

Durante a aula ir variando a forma de ensinar exercícios/blocos:
[EXPLAIN]  — eu explico e demonstro ao vivo e alunos acompanham.
                  (exercícios iniciais, etc..)
[ASK FIRST]       — eu vou perguntando ideias de solução e como escrever o código
                  espero por respostas e depois vou fazendo
[THEY DO]      — eles fazem THEY DO/pares e falam entre eles.
                  eu vou passando pela sala a ver como vai e tirar duvidas.
                  passado um tempo mostro uma solução e falamos raciocínio.
"""

# %%
"""
===========================================================================
ABERTURA (10:10-10:18, 8 min)
===========================================================================
1. Folha Presenças; Vão ligando Spyder...
2. Dúvidas aula passada?
    - Todos tem Spyder a funcionar?
    - Testaram VPN + Mooshak?
    - Têm wifi eduroam a funcionar?
    - Dúvidas sobre tipos de dados, cast, input/print?

3. MOTIVAÇÃO:
    Hoje vamos treinar a programar para resolver problemas práticos do mundo real.
    Vão ver que, assim que conseguimos pensar uma solução "matemática" do problema,
    depois implementar isso em python é fácil (a gramática/syntax aprende-se rápido)

4. ESTRUTURA DA AULA:
    BLOCO A — funções, parâmetros/retorno, encadeamento de chamadas, operadores /, // e %
    BLOCO B — ex. 9, 10, 11 (trovoada, h/m/s)
    BLOCO C — ex. 12, 13 (pêndulo, paralelepípedo)
    
    INTERVALO
    
    BLOCO D — ex. 14, 15 (queda de pedra: visão e som)
    BLOCO E — if/elif/else, comparação/lógicos + ex. 16
    BLOCO F — ex. 17, 18 (triângulo, nota final)
    TEASER - ciclos for e recursividade
"""
# %%
"""
===========================================================================
[BLOCO A] (10:18-10:40, 22 min) — Funções, parâmetros, retorno,
encadeamento de chamadas + operadores /, // e %
===========================================================================

A.1) [EXPLAIN] operadores /, // e %
"""
num = 20
div = num / 6
parte_inteira = num // 6
modulo = num % 6

print("div: ", div)
print("parte_inteira: ", parte_inteira)
print("modulo / resto", modulo)


# %%
"""
A.2) [DIALOGO] operadores /, // e % - exemplo aplicação

   Dado um valor,
   Quantas notas de 20€ e 10€, e moedas de 1€, são precisas para obter esse valor?

"""

valor = 57
notas_20 = valor // 20
resto = valor % 20
notas_10 = resto // 10
moedas = resto % 10
print(f"{valor}€ = {notas_20} notas 20€ + {notas_10} notas 10€ + {moedas} moedas 1€")

# %%
"""
A.3) [EXPLAIN] main() + funçoes()

    O problema anterior, num programa bem organizado poderia:
        - ter main() para:
                - IO - receber input
                - chamar funções auxiliares (o main não faz contas - chama funções que fazem essas contas)
                - encadear chamadas - receber o resultado de uma função
                    e passar para a função seguinte
                - IO - print resultados (output)
        - ter funções que implementam a lógica do programa
            estas funções podem ser complexas, e chamar outras funções, encadeiar resultados etc. e no fim retornam o resultado final

    Nota: Nos testes normalmente não se pede o main() e IO.
    Pede-se apenas as funções com a lógica pura (vejam os testes dos anos anterires).

"""
def num_notas(total: int, nota: int) -> int:
    """ retorna o numero máximo de notas do valor indicado
        que cabem dentro do valor total
        Precondition: total >= 0 and nota > 0
    """
    return total // nota

def resto(total: int, nota: int) -> int:
    """ retorna o que sobra depois de darmos o máximo em notas do valor indicado
        Precondition: total >= 0 and nota > 0
    """
    return total % nota

def main() -> None:
    total = int(input("Valor total em euros: "))
    n20 = num_notas(total, 20)
    r1 = resto(total, 20)
    n10 = num_notas(r1, 10)
    r2 = resto(r1, 10)
    print(f"{n20} notas de 20€, {n10} notas de 10€, sobra {r2}€")

main()

# %%
"""
A.4) [EXPLAIN] Boas Práticas - bons nomes às funções e variáveis

Nota:   uma forma boa de programar é basicamente escrever as ações que queremos fazer,
        e converter cada uma dessas frases/ações no nome de uma função.
        No Exemplo anterior, podiamos escrever o main passo a passo de forma intuitiva:
        "Dado total quantas notas de 20 cabem?"
            == nome da função a chamar: num_notas(total,20)
        "Depois quanto sobra?" == função: sobra(total,20)
        "Depois pego neste resto e faço a mesma coisa para notas de 10"
        "E depois o resto final fica para moedas de 1"


    Isto é uma forma muito simples e prática de fazer um programa,
    é basicamente divilo em funções que são as ações que temos que fazer na pratica

    Se as funções tiverem nomes bem escolhidos,
    escrever código é quase como falar português (ou inglês).

    Outro exemplo de programa, para registar um user,
    onde não sabemos os pormenores sobre como implementar cada função,
    mas sabemos que é mesmo isto que temos que fazer na prática:

"""
# pensamos as ações de alto nível que temos que fazer:
def register_user(id, name, email) -> str:
    if not (is_free_id(id) and is_free_email(email)):
        return "id ou email já usados"
    if not is_email_valid(email):
        return "email inválido"
    add_user_to_db(id, name, email)
    return "utilizador registado com sucesso"

# para já podemos deixar estas funções "stub" (esqueleto), com um valor
# fixo de retorno ou só "pass", assim já temos um programa que corre
# de ponta a ponta, mesmo sem termos implementado a lógica a sério ainda
def is_free_id(id):
    # TODO
    return True

def is_free_email(email):
    #TODO
    return True

def is_email_valid(email):
    # TODO
    return True

def add_user_to_db(id, name, email):
    # TODO
    pass

"""
    Desta forma, dividindo o programa em funções com nomes claros e bem representativos,

    - é fácil pensar um programa
    - é fácil outra pessoa ler e perceber o programa (ex professor a corrigir)
    - poupa em comentários, porque os nomes são self-explanatory
    - permite dividir tarefas e fazer testes locais
        » Exemplo após o grupo decidir um bom "register_user" geral,
        depois podem dividir tarefas e cada um vai implementar e testar uma função
"""

# %%
"""
===========================================================================
[BLOCO B] (10:40-11:01, 21 min) — Guião 02a, exercícios 9, 10, 11
===========================================================================


B.1) Ex. 9 — Trovoada [ASK FIRST] (4 min)

"""

def distancia_trovoada(segundos: int) -> int:
    """ Distância (m) a que está uma trovoada, dado o nº de segundos
        entre o relâmpago e o trovão. Velocidade do som: 340 m/s.
        Precondition: segundos >= 0
    """
    return 340 * segundos

def main() -> None:
    t = int(input("Segundos entre relâmpago e trovão: "))
    print(f"A trovoada está a {distancia_trovoada(t)} metros.")

main()

# %%
"""

B.2) Ex. 10 — h/m/s -> segundos [THEY DO] (6 min)
    
"""
def to_seconds(h: int, m: int, s: int) -> int:
    """ Converte uma duração dada em horas, minutos e segundos, para o
        total de segundos.
        Precondition: h >= 0 and 0 <= m < 60 and 0 <= s < 60
    """
    return h * 3600 + m * 60 + s

def main() -> None:
    h = int(input("Horas: "))
    m = int(input("Minutos: "))
    s = int(input("Segundos: "))
    print(to_seconds(h, m, s))

main()
# Exemplo do guião: h=1, m=1, s=1 -> 3661.

# %%
"""

B.3) Ex. 11 — segundos -> h/m/s [THEY DO] (11 min)

    Fazer semelhante ao das moeadas, 
    mas aqui ter o main a mandar o total original para as funções
    assim nã temos o main a fazer a lógica de encadeamento.
        Ou seja mesmo para fazer os minutos nós mandamos o total original, etc.
        
"""

def get_hours(total: int) -> int:
    """ Horas duma duração total dada em segundos. """
    return total // 3600

def get_minutes(total: int) -> int:
    """ Minutos duma duração total dada em segundos. """
    return (total // 60) % 60 # tot_minutos = total // 60; tot_minutos_nao_incluidos_em_horas = tot_minutos % 60  

def get_seconds(total: int) -> int:
    """ Segundos (0-59) duma duração total dada em segundos. """
    return total % 60

def main() -> None:
    total = int(input("Duração em segundos: "))
    h = get_hours(total)
    m = get_minutes(total)
    s = get_seconds(total)
    print(f"{h} horas, {m} minutos e {s} segundos.")

main()

# Exemplo do guião: 1_000_000 segundos -> 277 horas, 46 minutos, 40 segundos.


# %%
"""
===========================================================================
[BLOCO C] (11:01-11:14, 13 min) — Guião 02a, exercícios 12 e 13
===========================================================================

C.1) Ex. 12 — Período do pêndulo [THEY DO] (4 min)
    
    
"""

# tal como nós podemos escrever as nossas funções num ficheiro
# e depois importar para usar noutros ficheiros, 
# em python existem muitas bibliotecas já prontas a usar
# algumas temos que instalar primeiro como matplotlib
# e outras podemos importar diretamente sem fazer mais nada como o math ou random etc.. 
import math

def periodo_pendulo(L: float) -> float:
    """ Período (s) dum pêndulo simples de comprimento L (m), para
        pequenas amplitudes de oscilação. T = 2*pi*sqrt(L/g)
        Precondition: L > 0
    """
    g = 9.8
    return 2 * math.pi * math.sqrt(L / g)

def main() -> None:
    L = float(input("Comprimento do fio (m): "))
    print(f"Período = {periodo_pendulo(L)} segundos")

main()
# L é um comprimento medido — pode ter casas decimais, por isso float.
# O mesmo para g. Não há razão nenhuma para usar int aqui.

# %%
"""
C.2) EXTRA 2 [EXPLAIN] (3 min) — encaixa mesmo aqui, antes do ex. 13

---------------------------------------------------------------------------
[EXTRA 2] O que é uma "Precondition"?
---------------------------------------------------------------------------
[EXPLAIN]

Repara nos testes anexos: quase todas as funções trazem no comentário uma
linha "Precondition: ...". Por exemplo, no Teste 2 de 25/26:
"Precondition: len(l) >= 2".

Uma precondição é um CONTRATO, não uma validação. A função não se
preocupa em verificar se recebeu argumentos válidos — ela simplesmente
CONFIA que quem a chamou respeitou o combinado. Se não respeitou, o
comportamento da função "não interessa" (não é bug, é uso indevido).

Ex.: em vez de escrever
    def periodo_pendulo(L: float) -> float:
        if L <= 0:
            return -1   # valor "de erro" inventado
        ...
escrevemos simplesmente a precondição "L > 0" no docstring, e a função
assume que é verdade.

Onde é que isto poupa trabalho AGORA? No ex. 13 (paralelepípedo), não
precisam de validar se a, b, c são positivos — escrevem só a precondição
"a > 0 and b > 0 and c > 0" no docstring de cada função. Guardem o `if`
de validação para quando o PRÓPRIO enunciado pede explicitamente para
tratar o caso inválido (é o que acontece já a seguir, no ex. 14/15, e
mais tarde no triangle_kind).
"""

# %%
"""
C.3) Ex. 13  [THEY DO] (6 min)

"""
# ---- Ex. 13 — Paralelepípedo [THEY DO] ------------------------------
def comprimento_arestas(a: float, b: float, c: float) -> float:
    """ Comprimento total das 12 arestas dum paralelepípedo a x b x c.
        Precondition: a > 0 and b > 0 and c > 0
    """
    return 4 * (a + b + c)

def area_total(a: float, b: float, c: float) -> float:
    """ Área total das 6 faces dum paralelepípedo a x b x c.
        Precondition: a > 0 and b > 0 and c > 0
    """
    return 2 * (a * b + a * c + b * c)

def volume(a: float, b: float, c: float) -> float:
    """ Volume dum paralelepípedo a x b x c.
        Precondition: a > 0 and b > 0 and c > 0
    """
    return a * b * c

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    print(f"Comprimento = {comprimento_arestas(a, b, c)}")
    print(f"Área = {area_total(a, b, c)}")
    print(f"Volume = {volume(a, b, c)}")

main()
# Exemplo do guião: A=B=C=1.0 -> Comprimento=12.0, Área=6.0, Volume=1.0.
# Nota de estilo: 3 funções pequenas, cada uma "uma coisa só", em vez de
# uma função gigante que calcula tudo.

# %%
"""
===========================================================================
INTERVALO (11:14-11:34, 20 min)
===========================================================================
"""

# %%
"""
===========================================================================
[BLOCO D] (11:34-11:48, 14 min) — Guião 02a, exercício 14 e exercício 15
===========================================================================
Ex. 14 [ASK FIRST] (6 min) — fórmula direta.
Ex. 15 [EXPLAIN] (8 min) — desafio da aula: envolve resolver uma
equação de 2º grau à mão antes de programar.
"""

# ---- Ex. 14 — Altura do precipício (visão) [ASK FIRST] -----------------
def altura_precipicio(t: int) -> float:
    """ Altura (m) dum precipício, a partir do tempo (s) que uma pedra
        demora a chegar ao fundo (o observador VÊ a pedra a chegar).
        d = 0.5*a*t^2, com a = 9.8 m/s^2
        Precondition: t >= 0
    """
    a = 9.8
    return 0.5 * a * (t ** 2)

def main() -> None:
    t = int(input("Segundos até se ver a pedra chegar: "))
    print(f"Altura = {altura_precipicio(t)} metros")

main()

# %%
# ---- Ex. 15 — Altura do precipício (som) [EXPLAIN] — DESAFIO -----
"""
Raciocínio:
  d = 0.5 * a * t_queda^2         (queda livre da pedra)
  d = v_som * (t_total - t_queda)  (o som percorre a mesma distância d,
                                      no tempo que resta até ao t_total
                                      ouvido pelo observador)
Substituindo e reescrevendo como A*x^2 + B*x + C = 0, com x = t_queda:
  0.5*a*t_queda^2 + v_som*t_queda - v_som*t_total = 0
Resolve-se com a fórmula resolvente, ficando só com a raiz positiva.
"""
import math

def altura_precipicio_som(t_total: int) -> float:
    """ Altura (m) dum precipício, a partir do tempo total (s) entre
        largar a pedra e OUVIR o som do impacto.
        Precondition: t_total >= 0
    """
    a = 9.8
    v_som = 340.0
    A = 0.5 * a
    B = v_som
    C = -v_som * t_total
    t_queda = (-B + math.sqrt(B ** 2 - 4 * A * C)) / (2 * A)
    return 0.5 * a * (t_queda ** 2)

def main() -> None:
    t_total = int(input("Segundos até se ouvir o som do impacto: "))
    print(f"Altura = {altura_precipicio_som(t_total)} metros")

main()
# t_total=3 -> cerca de 40.65 metros (menor que no ex.14 com o mesmo t,
# porque parte do tempo é gasto pelo som a viajar de volta).

# %%
"""
===========================================================================
[BLOCO E] (11:48-12:21, 33 min) — if / elif / else, operadores de
comparação e lógicos + exercício 16
===========================================================================
Aquecimento "desconto" [ASK FIRST] (6 min)
Demo "triagem" (if/elif, ordem importa) [EXPLAIN] (6 min)
Ex. 16 (máximo) [ASK FIRST] (4 min)
EXTRA 3 — quiz de 6 gotchas [ASK FIRST] (17 min)

O guião 02b continua no Bloco F (ex. 17, 18).
"""

# ---- Aquecimento: desconto numa loja [ASK FIRST] -------------------------
"""
Regra: um cliente tem desconto SE for sócio E gastar mais de 50€,
OU SE não for sócio mas gastar mais de 100€.

Pensa antes de correr: o António não é sócio e gastou 80€, tem desconto?
"""
def tem_desconto(socio: bool, gasto: float) -> bool:
    """ Verifica se o cliente tem direito a desconto. """
    return (socio and gasto > 50) or (not socio and gasto > 100)

print(tem_desconto(True, 60))     # True  - sócio, gastou 60
print(tem_desconto(True, 30))     # False - sócio, gastou pouco
print(tem_desconto(False, 150))   # True  - não sócio, gastou muito
print(tem_desconto(False, 80))    # False - não sócio, só 80 (< 100)

# %%
"""
---- Demo principal: triagem hospitalar (if / elif / else) [EXPLAIN]
Regras (por ordem de gravidade):
  - Dificuldade a respirar E febre >= 39   -> VERMELHO (urgente)
  - Dificuldade a respirar OU febre >= 39  -> AMARELO (atenção)
  - Febre >= 37.5                          -> VERDE (observação)
  - Caso contrário                         -> AZUL (sem gravidade)

Ponto crucial: num elif, a ORDEM importa. O Python testa de cima para
baixo e para na primeira condição verdadeira. Por isso as regras mais
graves têm de vir PRIMEIRO.
"""
def triagem(febre: float, dificuldade_respirar: bool) -> str:
    """ Classifica a urgência dum paciente. """
    if dificuldade_respirar and febre >= 39:
        return "VERMELHO - Urgente"
    elif dificuldade_respirar or febre >= 39:
        return "AMARELO - Atenção"
    elif febre >= 37.5:
        return "VERDE - Observação"
    else:
        return "AZUL - Sem gravidade"

print(triagem(39.5, True))   # VERMELHO
print(triagem(38.0, False))  # VERDE
print(triagem(36.5, False))  # AZUL

# %%
# ---- Ex. 16 — Máximo de dois inteiros [ASK FIRST] -----------------------
def maximum(a: int, b: int) -> int:
    """ Máximo entre dois valores inteiros. """
    if a >= b:
        return a
    else:
        return b

def main() -> None:
    x = int(input("Primeiro valor: "))
    y = int(input("Segundo valor: "))
    print(f"O máximo é {maximum(x, y)}")

main()

# %%
"""
===========================================================================
[EXTRA 3] Quiz de "gotchas" [ASK FIRST] — armadilhas típicas dos testes
===========================================================================
Pensa antes de correr cada célula.
"""

# --- Gotcha 1: comparações encadeadas -----------------------------------
"""
Pergunta: o que achas que `2 < 5 < 3` devolve?
"""
print("2 < 5 < 3  ->", 2 < 5 < 3)
# Explicação: em Python podes encadear comparações — "a < b < c" equivale
# a "(a < b) and (b < c)". Aqui 2<5 é True, mas 5<3 é False, por isso o
# resultado final é False. Isto NÃO é assim em todas as linguagens!
# Repara que vão usar isto já a seguir no ex.17 (a == b == c).

# %%
# --- Gotcha 2: = vs == ---------------------------------------------------
"""
Pergunta: o que achas que este código faz?

    if a = 5:
        print("a é cinco")

(NÃO corras isto — dá erro de propósito, é só para pensarem)
"""
# Resposta: SyntaxError! Em Python, "=" é atribuição e "==" é comparação.
# Já apareceu num teste real (Teste 1 de 19/Out/2024, pergunta 1b) —
# fica atento a isto nos vossos códigos.

# %%
# --- Gotcha 3: divisão inteira vs divisão real ---------------------------
"""
Pergunta: quanto dá 7 / 2 ?  E 7 // 2 ?  E 7.0 // 2 ?
"""
print("7 / 2   ->", 7 / 2)      # 3.5  (a divisão normal dá SEMPRE float)
print("7 // 2  ->", 7 // 2)     # 3    (divisão inteira)
print("7.0 // 2 ->", 7.0 // 2)  # 3.0  (mistura int/float -> float, mesmo
                                 #       sendo divisão inteira)

# %%
# --- Gotcha 4: "not" + "and" — a ordem de leitura engana --------------
"""
Pergunta: queremos validar que a e b são AMBOS positivos, senão é erro.
Alguém escreveu:

    if not a > 0 and b > 0:
        print("erro")

Testa com a=5 (válido) e b=-3 (INVÁLIDO). O que achas que acontece?
"""
a, b = 5, -3
resultado = not a > 0 and b > 0
print("not a > 0 and b > 0  (a=5, b=-3) ->", resultado)
# Resultado: False -> o "if" NÃO dispara, ou seja, o erro NÃO é
# detetado, mesmo com b inválido! Porquê? Porque em Python o "not" só
# "agarra" o que vem logo a seguir (a > 0), NÃO a expressão toda.
# É como se tivesse escrito: (not (a > 0)) and (b > 0)
#                           = (not True)   and (False)
#                           = False        and False = False
#
# A forma CORRETA de validar "os dois têm de ser positivos, senão erro":
correto = not (a > 0 and b > 0)
print("not (a > 0 and b > 0)  ->", correto)   # True - agora deteta o erro!
#
# LIÇÃO: sempre que combinares "not" com "and"/"or", usa parênteses para
# a tua intenção ficar sem ambiguidade. Isto é especialmente importante
# nos exercícios 13, 14 e 15 se decidirem validar os argumentos.

# %%
# --- Gotcha 5: a ordem dos elif pode mudar o resultado --------------
"""
Pergunta: aqui está a MESMA função de triagem, mas com a ordem dos elif
trocada (a condição "febre >= 37.5" passou para primeiro). O que achas
que acontece a um paciente com febre=39.5 e dificuldade a respirar=True
(que deveria ser VERMELHO/urgente)?
"""
def triagem_errada(febre: float, dificuldade_respirar: bool) -> str:
    """ Versão com bug: testa a condição mais fraca primeiro. """
    if febre >= 37.5:
        return "VERDE - Observação"
    elif dificuldade_respirar and febre >= 39:
        return "VERMELHO - Urgente"
    elif dificuldade_respirar or febre >= 39:
        return "AMARELO - Atenção"
    else:
        return "AZUL - Sem gravidade"

print("Versão correta:", triagem(39.5, True))
print("Versão com bug:", triagem_errada(39.5, True))
# A versão errada devolve "VERDE" para um caso URGENTE! Num hospital
# a sério isto seria perigoso. LIÇÃO: num elif, as condições mais
# específicas/graves têm de vir sempre primeiro.

# %%
# --- Gotcha 6: "or" com um valor solto (confusão and/or) ---------------
"""
Pergunta: alguém quer verificar se x é 1 OU 2, e escreve:

    if x == 1 or 2:
        print("é 1 ou 2")

Testa com x = 99. O que achas que imprime?
"""
x = 99
if x == 1 or 2:
    print("é 1 ou 2")
# Isto imprime SEMPRE "é 1 ou 2", seja qual for o x! Porquê? Porque
# "x == 1 or 2" é lido como "(x == 1) or (2)", e em Python qualquer
# número diferente de zero conta como "verdadeiro" (truthy) quando usado
# como condição. Logo a segunda parte (o simples "2") é sempre True, e o
# "or" todo dá sempre True, independentemente do x.
#
# Forma CORRETA:
if x == 1 or x == 2:
    print("é 1 ou 2 (agora sim)")
else:
    print("não é 1 nem 2")
# LIÇÃO: cada lado de "and"/"or" tem de ser uma condição COMPLETA por si
# só. Nunca "if x == 1 or 2", sempre "if x == 1 or x == 2".

# %%
"""
===========================================================================
[BLOCO F] (12:21-12:47, 26 min) — Guião 02b, exercícios 17 e 18
===========================================================================
Ex. 17 [THEY DO] (16 min) — o mais rico: is_proper_triangle + elif +
"a==b==c" (liga diretamente à Gotcha 1).
Ex. 18 [THEY DO] (10 min) — mais rápido do que parece, a main() já vem
feita no guião, só faltam as 3 funções.
"""

# ---- Ex. 17 — Tipo de triângulo [THEY DO] -----------------------------
def is_proper_triangle(a: float, b: float, c: float) -> bool:
    """ Verifica se a, b, c definem um triângulo próprio (área > 0).
        Precondition: nenhuma (a função valida tudo sozinha, incluindo
        lados <= 0, porque o ENUNCIADO pede para tratar esse caso)
    """
    return a > 0 and b > 0 and c > 0 and a < b + c and b < a + c and c < a + b

def triangle_kind(a: float, b: float, c: float) -> int:
    """ Tipo de triângulo definido pelos lados a, b, c.
        Resultado: 0 - não é triângulo; 1 - equilátero;
                   2 - isósceles; 3 - escaleno
    """
    if is_proper_triangle(a, b, c):
        if a == b == c:
            return 1
        elif a == b or b == c or a == c:
            return 2
        else:
            return 3
    else:
        return 0  # Invalid triangle

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    print(triangle_kind(a, b, c))

main()
# Exemplo do guião: A=B=C=0.5 -> 1 (equilátero).
#
# "a == b == c" é a mesma comparação encadeada da Gotcha 1 — agora já
# sabemos exatamente o que ela significa por dentro.
#
# Porque é que o elif do meio ("a==b or b==c or a==c") nunca apanha por
# engano um caso equilátero? Porque o if de cima já capturou esse caso
# primeiro — ordem outra vez.
#
# is_proper_triangle valida tudo com if, ao contrário do ex.13 — aqui faz
# sentido porque o próprio enunciado pede explicitamente "0 se não
# definirem um triângulo próprio" (ver EXTRA 2: aqui a validação faz
# parte do que é pedido, não é opcional).

# %%
# ---- Ex. 18 — Nota final de IPCE [THEY DO] -----------------------------
def average(grade1: float, grade2: float) -> float:
    """ Média de duas notas. """
    return (grade1 + grade2) / 2

def passed(test1: float, test2: float, proj: float) -> bool:
    """ Verifica se o aluno foi aprovado por avaliação contínua. """
    return proj >= 9.5 and average(test1, test2) >= 9.5

def final_grade(test1: float, test2: float, proj: float) -> float:
    """ Nota final não arredondada.
        Precondition: passed(test1, test2, proj)
    """
    return 0.4 * test1 + 0.4 * test2 + 0.2 * proj

# main() já vem dada no guião — não precisam de a reescrever:
def main() -> None:
    t1 = float(input("T1: "))
    t2 = float(input("T2: "))
    pr = float(input("PR: "))
    if passed(t1, t2, pr):
        print(final_grade(t1, t2, pr))
    else:
        print("REPROVADO")

main()
# É literalmente a fórmula da vossa nota nesta cadeira, se fizerem a
# avaliação por testes. Reparem que passed() chama average() — outra vez
# encadeamento de chamadas, tal como na demo do Bloco A.

# %%
"""
===========================================================================
[EXTRA 4] (12:47-13:00, 13 min) — Teaser de ciclos for e recursividade
===========================================================================
Isto vai ser dado a sério na teórica desta tarde — aqui é só um
"trailer", para ficarem com vontade de aprender mais.
"""

# --- Ciclo for: repetir sem copiar-colar [EXPLAIN] (2 min) -----------
"""
E se tivéssemos 1000 preços numa lista de compras? Íamos escrever 1000
linhas de "total = total + preco"?
"""
precos = [12.5, 3.2, 45.0, 7.8, 22.1]
total = 0
for p in precos:
    total = total + p
print(f"Total da fatura: {total}€")
# Sem o for, seria "total = total + precos[0]", depois precos[1], etc.,
# 5 vezes seguidas. Com 1000 preços, o for nem precisa de mudar.

# %%
# --- Recursividade: a "receita" de 3 passos [EXPLAIN] (3 min) --------
"""
A receita mental para pensar em recursivo (mais importante do que o
código em si):

  1. Qual é o caso mais simples que sei resolver diretamente, sem
     precisar de mais nada? (o "caso base")
  2. Se eu já soubesse resolver uma versão mais PEQUENA deste problema,
     conseguia usar essa resposta para resolver a versão maior?
  3. Traduzir as duas respostas para código: if (caso base): ...
     else: usa a chamada recursiva.

Se a resposta a (2) for "sim, consigo pegar na solução da parte mais
pequena e construir a solução do problema todo a partir dela" — isso é
o "clique mental" da recursividade (a mesma ideia que, em cadeiras
futuras, vão reconhecer como o coração da programação dinâmica).

Quando a fórmula matemática já é recursiva, o passo 3 é quase copiar-
colar. Exemplo: soma de 1 até n.
"""
def soma_ate(n: int) -> int:
    """ Soma de 1 até n.
        Fórmula: soma_ate(n) = n + soma_ate(n-1), e soma_ate(0) = 0
        Precondition: n >= 0
    """
    if n == 0:              # <- caso base (passo 1)
        return 0
    else:                   # <- usa a versão mais pequena (passo 2)
        return n + soma_ate(n - 1)

print(soma_ate(5))  # 1+2+3+4+5 = 15

# %%
"""
Mais um exemplo do mesmo padrão — traduzir a fórmula matemática
diretamente para código.
"""
def fibonacci(n: int) -> int:
    """ n-ésimo termo da sucessão de Fibonacci: 0,1,1,2,3,5,8,13,...
        Fórmula: fibonacci(n) = fibonacci(n-1) + fibonacci(n-2),
        com fibonacci(0)=0 e fibonacci(1)=1
        Precondition: n >= 0
    """
    if n <= 1:               # caso base
        return n
    else:
        return fibonacci(n - 1) + fibonacci(n - 2)

print(fibonacci(10))  # 55

# %%
# --- Recursividade "a sério": busca binária [EXPLAIN] (4 min) --------
"""
Analogia: pensa no jogo de adivinhar um número entre 1 e 100 — perguntas
"é maior que 50?" e cada resposta elimina METADE das hipóteses. Encontras
o número muito mais depressa do que testando 1, 2, 3, 4... um a um. É
esta ideia que se usa para procurar um valor numa lista ORDENADA.
"""
def procura_binaria(lista: list[int], alvo: int, ini: int, fim: int) -> bool:
    """ Verifica se alvo está em lista[ini..fim], assumindo lista ordenada.
        Precondition: lista está ordenada por ordem crescente
    """
    if ini > fim:
        return False
    meio = (ini + fim) // 2
    if lista[meio] == alvo:
        return True
    elif lista[meio] < alvo:
        return procura_binaria(lista, alvo, meio + 1, fim)
    else:
        return procura_binaria(lista, alvo, ini, meio - 1)

numeros = [1, 3, 5, 7, 9, 11, 13, 15, 17, 19]
print(procura_binaria(numeros, 13, 0, len(numeros) - 1))  # True
print(procura_binaria(numeros, 8, 0, len(numeros) - 1))   # False
# Com 1 milhão de números ordenados, isto encontra qualquer valor em
# cerca de 20 passos — em vez de, no pior caso, 1 milhão de passos a
# percorrer um a um. É por isto que a recursividade (bem aplicada) é tão
# usada em problemas a sério. (Coisas como quicksort ou programação
# dinâmica ficam para uma aula futura dedicada a isto — hoje é só para
# verem o potencial.)

# %%
"""
===========================================================================
FECHO
===========================================================================
Hoje à tarde, na teórica, vão ver "a sério" o for e a recursividade que
acabámos de espreitar — já não vos vai soar a chinês.

[REGRA DA CASA] Como (não) usar IA nesta cadeira:
- Tenta sempre sozinho primeiro. Mesmo que demore, é isso que te ensina
  a programar.
- Só depois de umas boas tentativas (10 minutos a sofrer sem conseguir)
  faz sentido perguntar a uma IA (ChatGPT, Claude, etc.).
- Regra de ouro do prompt: nunca peças "resolve-me isto". Pede para te
  explicarem o conceito em que estás preso, ou para darem mais exemplos
  parecidos.
- Outra boa técnica: escreve a tua tentativa e pede uma "análise
  crítica" — o que está errado, o que pode melhorar — em vez de pedires
  logo a solução certa.
- Ótima técnica de estudo: pega num teste de anos anteriores e pede à
  IA "dá-me mais 3 exercícios parecidos com o exercício X deste teste,
  com dificuldade semelhante" — ótimo para treinar para a avaliação.
- Usa a IA como um professor particular que te ensina, não como alguém
  que te faz os trabalhos de casa. Se copiares sem perceber, no teste
  (sem consulta!) ninguém te vai safar.

Dúvidas dos exercícios que não deu para acabar: tragam para a próxima
aula ou pratiquem mais no Mooshak (lembrete: precisa de Eduroam/VPN
fora do campus).
"""