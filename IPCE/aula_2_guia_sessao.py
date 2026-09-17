# %%
"""
===========================================================================
GUIA DE SESSÃO — Prática 02 (P1, 2a-feira 10:10-13:00) — IPCE 2026/2027
===========================================================================
Cobre os guiões 02a (ex. 9-15) e 02b (ex. 16-18).

Durante a aula ir variando a forma de ensinar exercícios/blocos:
[DEMO/EXPLAIN]  — eu explico e demonstro ao vivo e alunos acompanham. 
                  (exercícios iniciais, etc..) 
[DIÁLOGO]       — eu vou perguntando ideias de solução e como escrever o código
                  espero por respostas e depois vou fazendo
[SOZINHOS]      — eles fazem sozinhos/pares e falam entre eles.
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

3. MOTIVAÇAO: 
    Hoje vamos treinar a programar para resolver problemas práticos do mundo real. 
    Vão ver que, assim que conseguimos pensar uma solução "matemática" do problema, 
    depois implementar isso em python é fácil (a gramática/syntax aprende-se rápido) 
    
3. ESTRUTURA DA AULA: 
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

A.1) [DEMO/EXPLAIN] operadores /, // e %
"""
num = 20
div = 20 / 6
parte_inteira = 20//6
modulo = 20 % 6

print("div: ", div)
print("parte_inteira: ", parte_inteira)
print("modulo", modulo)



# %%
"""
A.2) [DIALOGO] operadores /, // e % - exemplo aplicação
   
   Dado um valor, 
   Quantas notas de 20€ e 10€, e moedas de 1€, são precisas para obter esse valor? 
   
"""








valor = 57
notas_20 = 57 // 20
resto = valor % 20
notas_10 = resto // 10
moedas = resto % 10 
print(f"{valor}€ = {notas_20} notas 20€ + {notas_10} notas 10€ + {moedas} moedas 1€")

# %%
"""
A.3) [DEMO/EXPLAIN] main() + funçoes()
   
    O problema anterior, num programa bem organizado poderia: 
        - ter main() para: 
                - IO - receber input
                - chamar funções auxiliares (o main não faz contas - chama funções que fazem essas contas)
                - encadear chamadas - receber o resultado de uma função 
                    e passar para a função seguinte 
                - IO - print resultados (output) 
        - ter funções que implementam a lógica do programa
            estas funções podem ser complexas, e chamar outras funções, encadeiar resultados etc. e no fim retornam o resultado final 
"""
def num_notas(total: int, nota: int) -> int:
    """ retorna o numero máximo de notas do valor indicado 
        que cabem dentro do valor total
        Precondition: total >= 0
    """
    return total // nota

def resto(total: int, nota: int) -> int:
    """ retorna o que sobra depois de darmos o máximo em notas do valor indicado"""
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
---------------------------------------------------------------------------
[EXTRA 1] Testar funções sem main() nem print() — e porquê os testes pedem isto
---------------------------------------------------------------------------
[DEMO/EXPLAIN] (5 min, mesmo a seguir à demo acima)

Depois de correres a célula anterior, as funções ficam disponíveis na
consola do Spyder (canto inferior direito). Mostra ao vivo:

    >>> num_notas_20(97)
    4
    >>> resto_depois_notas_20(97)
    17

Diz-lhes: "reparem nos testes que vos vou passar hoje — quase todas as
perguntas de código dizem 'não programe main, nem use input ou print'.
Não é implicância dos professores: é para testarem SÓ a lógica da vossa
função, sem se preocuparem com a parte de interação. E para vocês:
testar assim na consola é muito mais rápido do que escrever um input()
sempre que querem verificar se uma função está certa."

Mostra os cabeçalhos de 2-3 exercícios dos testes anexos (ex: is_root,
zeno, christmas) como prova viva disto — todos sem main.
"""
print(num_notas_20(97))
print(resto_depois_notas_20(97))

# %%
"""
---------------------------------------------------------------------------
[BÓNUS] O poder de dar bons nomes às funções (3 min, ainda dentro do Bloco A)
---------------------------------------------------------------------------
[DEMO/EXPLAIN] Não é para escreverem, é só para verem o efeito. Projeta e
lê em voz alta, sem correr (tem nomes por definir, não corre):

Se as funções tiverem nomes bem escolhidos, o código de alto nível
quase se lê como português. Exemplo:

    def register_user(id, name, email) -> str:
        if not (is_free_id(id) and is_free_email(email)):
            return "id ou email já usados"
        if not is_email_valid(email):
            return "email inválido"
        add_user_to_db(id, name, email)
        return "utilizador registado com sucesso"

Sem saberem como is_free_id ou is_email_valid funcionam por dentro, já
percebem o que register_user faz, só pelos nomes. É esta a mesma ideia
que estão a aplicar hoje com num_notas_20, e vão aplicar já a seguir com
is_proper_triangle: dividir um problema em peças pequenas e bem
nomeadas, e depois combiná-las. Um bom nome poupa comentários.
"""

# %%
"""
===========================================================================
[BLOCO B] (10:40-11:01, 21 min) — Guião 02a, exercícios 9, 10, 11
===========================================================================
  - Ex. 9: [DIÁLOGO] 4 min
  - Ex. 10: [DIÁLOGO] 6 min
  - Ex. 11: [DIÁLOGO] para a 1ª função + [SOZINHOS] para o resto, 11 min
    (liga explicitamente à demo do Bloco A: "é literalmente a mesma
    técnica das notas, mas ao contrário: em vez de juntar h/m/s para
    dar segundos, agora separam segundos em h/m/s")
"""

# ---- Ex. 9 — Trovoada [DIÁLOGO] --------------------------------------
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
# ---- Ex. 10 — h/m/s -> segundos [DIÁLOGO] ----------------------------
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
# Exemplo do guião: h=1, m=1, s=1 -> 3661. Corre e confirma com a turma.

# %%
# ---- Ex. 11 — segundos -> h/m/s [DIÁLOGO + SOZINHOS] -----------------
"""
Sugestão de condução: faz get_seconds em conjunto (diálogo, ~2 min,
perguntando "que operador nos dá só o resto de uma divisão?"), depois
dá-lhes ~6 min sozinhos para get_minutes, get_hours e main, e fecha com
~3 min de correção.
"""
def get_seconds(total: int) -> int:
    """ Segundos (0-59) duma duração total dada em segundos. """
    return total % 60

def get_minutes(total: int) -> int:
    """ Minutos (0-59) duma duração total dada em segundos. """
    return (total // 60) % 60

def get_hours(total: int) -> int:
    """ Horas (sem limite) duma duração total dada em segundos. """
    return total // 3600

def main() -> None:
    total = int(input("Duração em segundos: "))
    h = get_hours(total)
    m = get_minutes(total)
    s = get_seconds(total)
    print(f"{h} horas, {m} minutos e {s} segundos.")

main()
# Exemplo do guião: 1000000 segundos -> 277 horas, 46 minutos, 40 segundos.
# ERRO TÍPICO a apontar: fazer get_minutes como (total % 3600) // 60 em vez
# de (total // 60) % 60 — ambos funcionam, mas se alguém só fizer
# total % 60 para os minutos, está a repetir get_seconds por engano.
# Vale a pena perguntar "quem fez diferente de mim?" aqui.

# %%
"""
===========================================================================
[BLOCO C] (11:01-11:14, 13 min) — Guião 02a, exercícios 12 e 13
===========================================================================
  - Ex. 12: [DIÁLOGO] 4 min
  - EXTRA 2: [DEMO/EXPLAIN] 3 min (encaixa mesmo aqui, antes do ex. 13)
  - Ex. 13: [SOZINHOS] 6 min
"""

# ---- Ex. 12 — Período do pêndulo [DIÁLOGO] ---------------------------
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
# Discussão rápida: "L é um comprimento medido — pode ter casas decimais,
# por isso float. g também. Não há razão nenhuma para usar int aqui."

# %%
"""
---------------------------------------------------------------------------
[EXTRA 2] O que é uma "Precondition"?
---------------------------------------------------------------------------
[DEMO/EXPLAIN]

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
# ---- Ex. 13 — Paralelepípedo [SOZINHOS] ------------------------------
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
# Ponto de estilo a reforçar: 3 funções pequenas, cada uma "uma coisa só",
# em vez de uma função gigante que calcula tudo.

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
  - Ex. 14: [DIÁLOGO] 6 min (é direto, uma fórmula só)
  - Ex. 15: [DEMO/EXPLAIN] 8 min — demasiado avançado para resolverem
    sozinhos no tempo disponível (envolve resolver uma equação de 2º
    grau à mão). Apresenta como "o desafio da aula" e faz a dedução
    tu mesmo, mas mantendo-os no diálogo (pergunta "e agora, o que
    fazemos com isto?" em vez de despejar a fórmula de repente).
"""

# ---- Ex. 14 — Altura do precipício (visão) [DIÁLOGO] -----------------
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
# ---- Ex. 15 — Altura do precipício (som) [DEMO/EXPLAIN] — DESAFIO -----
"""
Raciocínio (mostrar no quadro, resumido):
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
Matéria NOVA para eles nesta aula prática (a teórica só vem à tarde) —
por isso, ao contrário dos blocos anteriores, aqui começa-se SEMPRE com
uma demonstração tua antes de qualquer exercício do guião.

  - Aquecimento "desconto": [DIÁLOGO] 6 min
  - Demo "triagem" (if/elif, ordem importa): [DEMO/EXPLAIN] 6 min
  - Ex. 16 (máximo): [DIÁLOGO] 4 min (aplicação imediata, mantém o embalo)
  - EXTRA 3 — quiz de 6 gotchas: [DIÁLOGO] 17 min

O guião 02b continua no Bloco F (ex. 17, 18).
"""

# ---- Aquecimento: desconto numa loja [DIÁLOGO] -------------------------
"""
Regra: um cliente tem desconto SE for sócio E gastar mais de 50€,
OU SE não for sócio mas gastar mais de 100€.

Antes de correr, pergunta: "o António não é sócio e gastou 80€,
tem desconto?" — deixa arriscarem antes de revelar.
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
---- Demo principal: triagem hospitalar (if / elif / else) [DEMO/EXPLAIN]
Regras (por ordem de gravidade):
  - Dificuldade a respirar E febre >= 39   -> VERMELHO (urgente)
  - Dificuldade a respirar OU febre >= 39  -> AMARELO (atenção)
  - Febre >= 37.5                          -> VERDE (observação)
  - Caso contrário                         -> AZUL (sem gravidade)

Ponto CRUCIAL a martelar: num elif, a ORDEM importa. O Python testa de
cima para baixo e para na primeira condição verdadeira. Por isso as
regras mais graves têm de vir PRIMEIRO.
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
# ---- Ex. 16 — Máximo de dois inteiros [DIÁLOGO] -----------------------
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
[EXTRA 3] Quiz de "gotchas" [DIÁLOGO] — armadilhas típicas dos testes
===========================================================================
Formato sugerido: lê a pergunta em voz alta, dá 20-30 segundos para
arriscarem oralmente, só depois corres a célula para revelar.
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
(Esta é a resposta definitiva à pergunta "tem de haver parênteses?")
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
  - Ex. 17: [SOZINHOS] 16 min (o mais rico — 7 min sozinhos/pares + 9 min
    de correção com bastante discussão, porque tem is_proper_triangle +
    elif + o "a==b==c" que liga diretamente ao Gotcha 1)
  - Ex. 18: [SOZINHOS] 10 min (mais rápido do que parece — a main() já
    vem feita no guião, só faltam as 3 funções)
"""

# ---- Ex. 17 — Tipo de triângulo [SOZINHOS] -----------------------------
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
# Exemplo do guião: A=B=C=0.5 -> 1 (equilátero). Corre e confirma.
#
# Pontos a discutir na correção:
#  - "a == b == c" é a mesma comparação encadeada do Gotcha 1 — agora já
#    sabem exatamente o que ela significa por dentro.
#  - Perguntar: "porque é que o elif do meio ('a==b or b==c or a==c')
#    nunca vai apanhar por engano um caso equilátero?" (resposta: porque
#    o if de cima já capturou esse caso primeiro — ordem outra vez!)
#  - is_proper_triangle é uma boa aplicação do EXTRA 2 ao contrário: aqui
#    SIM validamos tudo com if, porque o enunciado pede explicitamente
#    "0 se não definirem um triângulo próprio".

# %%
# ---- Ex. 18 — Nota final de IPCE [SOZINHOS] -----------------------------
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
# Ponto motivacional fácil: "é literalmente a fórmula da vossa nota
# nesta cadeira, se fizerem por testes" — costuma prender a atenção.
# Reparem que passed() chama average() — outra vez encadeamento de
# chamadas, tal como na demo do Bloco A.

# %%
"""
===========================================================================
[EXTRA 4] (12:47-13:00, 13 min) — Teaser de ciclos for e recursividade
===========================================================================
Vão ver isto formalmente na teórica desta tarde. Aqui é só um "trailer"
rápido — NÃO é para ficarem a perceber tudo, é para ficarem com vontade
de aprender mais. Corre ao vivo, sem te alongares em explicações.
"""

# --- Ciclo for: repetir sem copiar-colar [DEMO/EXPLAIN] (2 min) -----------
"""
Pergunta retórica: "e se eu tivesse 1000 preços numa lista de compras,
ia escrever 1000 linhas de 'total = total + preco'?"
"""
precos = [12.5, 3.2, 45.0, 7.8, 22.1]
total = 0
for p in precos:
    total = total + p
print(f"Total da fatura: {total}€")
# Sem o for, seria "total = total + precos[0]", depois precos[1], etc.,
# 5 vezes seguidas. Com 1000 preços, o for nem precisa de mudar.

# %%
# --- Recursividade: a "receita" de 3 passos [DEMO/EXPLAIN] (3 min) --------
"""
Antes do código, dá-lhes a receita mental (isto é o mais importante do
bloco — mais do que o código em si):

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
Mais um exemplo de "traduzir a fórmula diretamente" — mesmo padrão a
repetir-se. Se o tempo apertar, só mostra o código sem correr e diz
"fica para espreitarem em casa, é só mais um exemplo da mesma receita".
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
# --- Recursividade "a sério": busca binária [DEMO/EXPLAIN] (4 min) --------
"""
Analogia (dizer antes de correr): "pensa no jogo de adivinhar um número
entre 1 e 100 — perguntas 'é maior que 50?' e cada resposta elimina
METADE das hipóteses. Encontras o número muito mais depressa do que
testando 1, 2, 3, 4... um a um. É esta ideia que se usa para procurar
um valor numa lista ORDENADA."
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
# percorrer um a um. É por isto que a recursividade (bem aplicada) é
# tão usada em problemas a sério. (Coisas como quicksort ou programação
# dinâmica ficam para uma aula futura dedicada a isto — hoje é só para
# verem o potencial.)

# %%
"""
===========================================================================
FECHO (últimos 3 min)
===========================================================================
1. "Hoje à tarde, na teórica, vão ver 'a sério' o for e a recursividade
   que acabámos de espreitar — agora já não vos vai soar a chinês."

2. [REGRA DA CASA] Como (não) usar IA nesta cadeira — vale a pena dizer
   isto devagar, é importante:
   - Tentem SEMPRE sozinhos primeiro. Mesmo que demore, é isso que vos
     ensina a programar.
   - Só depois de umas boas tentativas (10 minutos a sofrer sem
     conseguir) faz sentido perguntar a uma IA (ChatGPT, Claude, etc.).
   - Regra de ouro do prompt: NUNCA peçam "resolve-me isto". Peçam para
     vos EXPLICAREM o conceito em que estão presos, ou para darem mais
     exemplos parecidos.
   - Outra boa técnica: escrevam a vossa tentativa e peçam uma "análise
     crítica" — o que está errado, o que pode melhorar — em vez de
     pedirem logo a solução certa.
   - Ótima técnica de estudo: peguem num teste de anos anteriores (como
     os que vos vou disponibilizar) e peçam à IA "dá-me mais 3
     exercícios parecidos com o exercício X deste teste, com
     dificuldade semelhante" — ótimo para treinar para a avaliação.
   - Resumindo: usem a IA como um professor particular que vos ensina,
     não como alguém que vos faz os trabalhos de casa. Se copiarem sem
     perceber, no teste (sem consulta!) ninguém vos vai safar.

3. Dúvidas finais dos exercícios que não deu para acabar — tragam para
   a próxima aula ou pratiquem mais no Mooshak (lembrete: precisa de
   Eduroam/VPN fora do campus).
"""

# %%
"""
===========================================================================
BACKLOG — ideias para aulas FUTURAS (não é para hoje, só para não perder)
===========================================================================
Matéria explicitamente adiada para não sobrecarregar a aula 2:

- Boas práticas de nomes (snake_case para funções/variáveis, CAPSLOCK
  para constantes, nomes de booleanos como pergunta: is_valid, has_...)
- Efeitos laterais em funções (o que são, porque evitar)
- Pass by value vs pass by reference, com exemplos práticos
- Variáveis globais vs locais — regras dentro de funções e ciclos
- Tipagem dinâmica em Python (i=2 depois i="2" sem erro) vs tipagem
  estática (Java/C, falha à compilação). Python é interpretado, o tipo
  "vive" no objeto, não na variável — boa forma de o explicar.
- f-strings: reforçar que dentro de {} pode ir qualquer EXPRESSÃO, não
  só uma variável, e falar em formatação (".2f", etc.)
- Separação de responsabilidades: interação com utilizador (I/O) vs
  lógica de negócio — pensar "frontend vs backend" como metáfora
- Refactoring: extrair lógica repetida para uma função só
- Testar um ficheiro inteiro de funções pondo vários casos de teste
  dentro do próprio main() do ficheiro (útil sobretudo para o projeto —
  complementa o que já ensinamos hoje sobre testar na consola)
- Recursividade "a sério": programação dinâmica, quicksort, ou outro
  exemplo com o mesmo "clique mental" de dividir para conquistar. Fica
  para uma aula dedicada, mais para a frente, quando já tiverem ciclos
  bem consolidados. Hoje só entrou o teaser leve (Extra 4).

Ideia em aberto (conversar mais tarde, não decidir já):
- Reorganizar o "guia de acompanhamento" (o ficheiro que levam para
  casa, tipo aula_1_complement.py) por TÓPICO (ciclos.py, if.py,
  funcoes.py, recursividade.py) em vez de por aula, como referência
  viva que vai crescendo — complementar aos ficheiros por aula, não
  substituto. Vale mais a pena falar nisto com mais aulas já dadas,
  para termos massa crítica de conteúdo para organizar assim.
"""