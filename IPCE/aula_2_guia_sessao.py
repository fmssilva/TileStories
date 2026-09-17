# %%
"""
===========================================================================
Prática 02 (P1, 2a-feira) — IPCE 2026/2027
===========================================================================
Cobre os guiões 02a (ex. 9-15) e 02b (ex. 16-18).

---------------------------------------------------------------------------
ESTRUTURA DA AULA (visão geral)
---------------------------------------------------------------------------
[10:10] (5 min)   Abertura — logística e recap da aula 1
[10:35] (20 min)  BLOCO A — funções com parâmetros/retorno, encadeamento
                   de chamadas, primer de // e %  (+ EXTRA 1: testar na
                   consola sem main/print)
[11:00] (25 min)  BLOCO B — ex. 9, 10, 11 (trovoada, h/m/s)
[11:15] (15 min)  BLOCO C — ex. 12, 13 (pêndulo, paralelepípedo)
                   (+ EXTRA 2: o que é uma precondição)
[11:35] (20 min)  INTERVALO
[11:50] (15 min)  BLOCO D — ex. 14, 15 (queda de pedra: visão e som)
[12:20] (30 min)  BLOCO E — if/elif/else, comparação e lógicos
                   (+ EXTRA 3: quiz de "gotchas" para os testes)
[12:50] (30 min)  BLOCO F — ex. 16, 17, 18 (máximo, triângulo, nota final)
[13:00] (10 min)  EXTRA 4 — teaser de ciclos for e recursividade + fecho

---------------------------------------------------------------------------
ESTRATÉGIA PEDAGÓGICA (como conduzir cada bloco)
---------------------------------------------------------------------------
Regra geral, em 3 passos, para CADA exercício ou mini-grupo de exercícios:

  1. DEMONSTRAR (2-5 min) — tu explicas/mostras um exemplo parecido
     (nunca o exercício exato) no projetor, pensando em voz alta.
  2. TENTAREM SOZINHOS/PARES (tempo definido, ver cada bloco) — eles
     escrevem no Spyder. Circulas pela sala. Não respondas logo às
     dúvidas — faz perguntas que os levem à resposta ("o que é que essa
     linha faz?").
  3. CORRIGIR EM CONJUNTO (2-5 min) — projetas a tua solução, mas não a
     "despejes": pergunta "quem fez diferente?", discute o raciocínio,
     não só o código. Nomeia o erro mais comum que viste a circular.

Quando parar a turma mais cedo? Pergunta "mãos no ar quem já tem isto a
funcionar". Se for menos de metade, para tudo e faz em conjunto no
projetor. Se for mais de metade, dá só mais 2 minutos e depois corrige.

Exercícios "rápidos" (9, 16) — não vale a pena dar tempo de trabalho
autónomo longo, resolve-os quase em conjunto, é só para aquecer.

Exercícios "desafio" (15, e a alínea final de 17) — não travar a turma
toda à espera de quem está preso; qualifica-os como bónus/opcional e
avança se o tempo apertar.

Mantém sempre um tom leve: "não faz mal errar, o Python diz-te logo onde
está o erro" — o objetivo de hoje é ganharem confiança a escrever
código, não escrever código perfeito à primeira.
"""

# %%
"""
===========================================================================
[10:10] ABERTURA (5 min)
===========================================================================
Fazer/dizer:
- Bom dia, chamada visual rápida (quem não tem Spyder aberto?).
- Recap de 30 segundos da aula 1: "vimos tipos (int, float, str, bool),
  cast, input/output. Hoje começamos mesmo a PROGRAMAR — se sabes montar
  um Lego seguindo instruções, sabes escrever estes programas."
- Plano do dia em 1 frase: "vamos escrever várias funções pequenas,
  sempre pelo mesmo processo: perceber o problema, pensar na fórmula ou
  na lógica, e traduzir para Python."
- Lembrar: Mooshak precisa de Eduroam/VPN — quem não tem, avisa já.
"""

# %%
"""
===========================================================================
[BLOCO A] (10:15-10:35, 20 min) — Funções, parâmetros, retorno,
encadeamento de chamadas + primer de // e %
===========================================================================
NÃO uses o exemplo do guião de hoje (evita "queimar" um exercício antes
da hora). Este bloco é só para consolidar o mecanismo de "uma função
chama outra função, usando o resultado de uma como entrada da seguinte"
— que é exatamente a técnica que precisam para os ex. 10 e 11.

O QUE FAZER:
1) (2 min) Escreve ao vivo o mini-exemplo dos ovos (primer de // e %).
   Pergunta antes de correr: "20 ovos, caixas de 6 — quantas caixas
   cheias? quantos sobram?"
"""
ovos = 20
caixas = ovos // 6
sobra = ovos % 6
print(f"{caixas} caixas cheias, sobram {sobra} ovos")

# %%
"""
2) (10 min) Demo principal — "decompor um valor em notas": dado um total
   em euros (inteiro), quantas notas de 20€, depois de 10€, e quanto
   sobra. Mostra que cada função faz UMA coisa, e a main() é só quem
   pergunta/mostra, chamando as funções por esta ordem.

   Enquanto escreves, verbaliza: "reparem que passo o RESULTADO desta
   função como ENTRADA da próxima — é isto que se chama encadear
   chamadas. Vão precisar disto mesmo a seguir, no exercício 11."
"""
def num_notas_20(total: int) -> int:
    """ Número de notas de 20€ num valor total em euros.
        Precondition: total >= 0
    """
    return total // 20

def resto_depois_notas_20(total: int) -> int:
    """ O que sobra depois de tirar as notas de 20€. """
    return total % 20

def num_notas_10(resto: int) -> int:
    """ Número de notas de 10€ no resto que já não dá para notas de 20€. """
    return resto // 10

def resto_final(resto: int) -> int:
    """ O que sobra depois de tirar também as notas de 10€. """
    return resto % 10

def main() -> None:
    total = int(input("Valor total em euros: "))
    n20 = num_notas_20(total)
    r1 = resto_depois_notas_20(total)
    n10 = num_notas_10(r1)
    r2 = resto_final(r1)
    print(f"{n20} notas de 20€, {n10} notas de 10€, sobra {r2}€")

main()

# %%
"""
---------------------------------------------------------------------------
[EXTRA 1] Testar funções sem main() nem print() — e porquê os testes pedem isto
---------------------------------------------------------------------------
(3-5 min, mesmo a seguir à demo acima)

Depois de correr a célula anterior, as funções ficam disponíveis na
consola do Spyder (canto inferior direito). Mostra ao vivo:

    >>> num_notas_20(97)
    4
    >>> resto_depois_notas_20(97)
    17

Diz-lhes: "reparem nos testes que vos vou passar hoje — quase todas as
perguntas de código dizem 'não programe main, nem use input ou print'.
Não é implicância dos professores: é para testarem SÓ a lógica da vossa
função, sem se preocuparem com a parte de interação (que é sempre igual
e menos interessante de avaliar). E para vocês: testar assim na consola
é muito mais rápido do que escrever um input() sempre que querem
verificar se uma função está certa."

Mostra os cabeçalhos de 2-3 exercícios dos testes anexos (ex: is_root,
zeno, christmas) como prova viva disto — todos sem main.
"""
print(num_notas_20(97))
print(resto_depois_notas_20(97))

# %%
"""
===========================================================================
[BLOCO B] (10:35-11:00, 25 min) — Guião 02a, exercícios 9, 10, 11
===========================================================================
Gestão de tempo sugerida dentro do bloco:
  - Ex. 9: 5 min (quase em conjunto, é de aquecimento)
  - Ex. 10: 8 min (5 sozinhos + 3 de correção)
  - Ex. 11: 12 min (7 sozinhos + 5 de correção — é o mais rico dos três,
    porque tem 3 funções auxiliares; liga explicitamente à demo do
    Bloco A: "é literalmente a mesma técnica das notas, mas ao contrário:
    em vez de juntar h/m/s para dar segundos, agora separam segundos em
    h/m/s")
"""

# ---- Ex. 9 — Trovoada -----------------------------------------------
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
# ---- Ex. 10 — h/m/s -> segundos --------------------------------------
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
# ---- Ex. 11 — segundos -> h/m/s (3 funções auxiliares + main) --------
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
[BLOCO C] (11:00-11:15, 15 min) — Guião 02a, exercícios 12 e 13
===========================================================================
  - Ex. 12: 5 min (é curto, mas a pergunta "inteiros ou reais?" do
    enunciado dá pé a uma discussão rápida e importante)
  - EXTRA 2: 3 min (encaixa mesmo aqui, antes do ex. 13)
  - Ex. 13: 7 min (3-4 sozinhos + correção rápida)
"""

# ---- Ex. 12 — Período do pêndulo -------------------------------------
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
# ---- Ex. 13 — Paralelepípedo -----------------------------------------
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
INTERVALO (11:15-11:35, 20 min)
===========================================================================
"""

# %%
"""
===========================================================================
[BLOCO D] (11:35-11:50, 15 min) — Guião 02a, exercício 14 (obrigatório)
e exercício 15 (desafio opcional)
===========================================================================
  - Ex. 14: 8 min (é direto, uma fórmula só)
  - Ex. 15: 7 min — apresenta como "quem acabou o 14 depressa, aqui está
    o desafio da aula". NÃO travar a turma toda por causa deste. Fazer
    a dedução no quadro só se sobrar tempo real, senão passa a solução
    já feita e explica o raciocínio verbalmente em 2 minutos.
"""

# ---- Ex. 14 — Altura do precipício (visão) ---------------------------
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
# ---- Ex. 15 — Altura do precipício (som) — DESAFIO -------------------
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
[BLOCO E] (11:50-12:20, 30 min) — if / elif / else, operadores de
comparação e lógicos
===========================================================================
Isto é matéria NOVA para eles nesta aula prática (a teórica só vem à
tarde) — por isso, ao contrário dos blocos anteriores, aqui começa-se
SEMPRE com uma demonstração tua antes de qualquer exercício do guião.

  - Aquecimento "desconto": 5 min
  - Demo "triagem" (se/elif, ordem importa): 8 min
  - EXTRA 3 — quiz de gotchas: 12 min
  - Ex. 16 (máximo): 5 min (rápido, quase em conjunto)

O guião propriamente dito (17, 18) fica para o Bloco F.
"""

# ---- Aquecimento: desconto numa loja ----------------------------------
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
---- Demo principal: triagem hospitalar (if / elif / else) ------------
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
"""
===========================================================================
[EXTRA 3] Quiz de "gotchas" — armadilhas típicas dos testes
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
# Repara que já usaram isto sem saber no ex.17 (a == b == c).

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
"""
===========================================================================
[BLOCO F] (12:20-12:50, 30 min) — Guião 02b, exercícios 16, 17, 18
===========================================================================
  - Ex. 16: 5 min (rápido, aplicação direta do que acabaram de ver)
  - Ex. 17: 15 min (o mais rico — 6 sozinhos/pares + 9 de correção com
    bastante discussão, porque tem is_proper_triangle + elif + o "a==b==c"
    que liga diretamente ao Gotcha 1)
  - Ex. 18: 10 min (mais rápido do que parece — a main() já vem feita
    no guião, só faltam as 3 funções)
"""

# ---- Ex. 16 — Máximo de dois inteiros ---------------------------------
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
# ---- Ex. 17 — Tipo de triângulo ---------------------------------------
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
# ---- Ex. 18 — Nota final de IPCE ---------------------------------------
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
[EXTRA 4] (12:50-13:00, 10 min) — Teaser de ciclos for e recursividade
===========================================================================
Vão ver isto formalmente na teórica desta tarde. Aqui é só um "trailer"
rápido — NÃO é para ficarem a perceber tudo, é para ficarem com vontade
de aprender mais. Corre ao vivo, sem te alongares em explicações.
"""

# --- Ciclo for: repetir sem copiar-colar --------------------------------
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
# --- Recursividade #1: traduzir uma fórmula matemática direta ----------
"""
Explicar em 1 frase: "uma função pode chamar-se a si própria — é como
uma boneca russa, cada camada resolve um bocadinho e passa o resto para
a camada seguinte, até chegar a um caso base muito simples."
"""
def soma_ate(n: int) -> int:
    """ Soma de 1 até n.
        Fórmula: soma_ate(n) = n + soma_ate(n-1), e soma_ate(0) = 0
        Precondition: n >= 0
    """
    if n == 0:
        return 0
    else:
        return n + soma_ate(n - 1)

print(soma_ate(5))  # 1+2+3+4+5 = 15
# Reparem: o código é quase uma cópia direta da fórmula matemática.

# %%
# --- Recursividade #2: o "poder" a sério — procura binária -------------
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
# tão usada em problemas a sério.

# %%
"""
===========================================================================
FECHO (últimos 1-2 min)
===========================================================================
Dizer:
- "Hoje à tarde, na teórica, vão ver 'a sério' o for e a recursividade
  que acabámos de espreitar — agora já não vos vai soar a chinês."
- "Qualquer dúvida dos exercícios que não deu para acabar, tragam para
  a próxima aula ou usem o Mooshak para praticar mais em casa."
- Lembrete rápido: Mooshak precisa de Eduroam/VPN fora do campus.
"""
