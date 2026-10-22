# %%
"""
===========================================================================
REVISÕES 7 - PROGRAMAS COMPLETOS E BEM ORGANIZADOS
Como pensar um programa desde o início?
Começar logo a escrever código? Se calhar não é boa ideia.
Primeiro a estrutura geral, depois implementar e testar cada função,
e no fim testar tudo junto.
IPCE 2026/2027
===========================================================================


Porquê um ficheiro só para isto?
  Até aqui, quase todos os exercícios eram funções soltas:
  recebem parâmetros e devolvem um resultado.
  Um programa a sério é mais do que isso. Fala com quem o usa
  (pergunta, valida, responde) e reparte o trabalho por várias funções.
  As peças já as conheces dos ficheiros 1 a 5.
  Aqui aprendes a JUNTÁ-LAS: partir um problema em funções, organizar a main,
  escolher o caminho pela resposta do utilizador, e escrever exatamente
  o output combinado.

O QUE VAIS APRENDER NESTE FICHEIRO (e porquê)

 21. Construir um programa, passo a passo ...................... [~30 min]
       Porquê: pensar o programa em português, dar um nome de função
       a cada passo, e ter logo um esqueleto que corre.
       Depois, implementar e testar UMA função de cada vez,
       e no fim testar tudo junto.

 22. Código que se lê ......................................... [~20 min]
       Porquê: bons nomes, uma tarefa por função e nada repetido
       tornam um programa fácil de ler, de testar e de corrigir.

 23. Escrever exatamente o formato pedido ..................... [~15 min]
       Porquê: um programa conversa com pessoas (e com o Mooshak).
       O texto, os espaços e as casas decimais fazem parte do resultado.

 24. Escolher pela resposta do utilizador ..................... [~15 min]
       Porquê: menus e opções ("escolha c ou f") aparecem em quase
       todos os programas interativos.

 25. Programas com ciclos e listas ............................. [~25 min]
       Porquê: tabelas e "pede N valores" juntam o for com o input.

 26. Desafios: programas completos ............................. [~45 min]
       Inclui o programa do Teste 1 2025/26.

  Resumo + soluções dos desafios

  Total: ~2h30. Não é para fazer de uma vez.

COMO USAR (resumo da secção 0 do ficheiro 1)
  - Ctrl + Enter corre a célula; Shift + Enter corre e salta para a seguinte.
  - Os programas deste ficheiro fazem input: quando corres a célula,
    escreve as respostas na consola (em baixo, à direita no Spyder).
  - Níveis:
      [EXEMPLO]  resolvido: lê, corre, experimenta outros valores.
      [FAZ]      contigo. Solução na célula a seguir, depois de muito espaço.
      [PENSA]    contigo. A "solução" é só a ideia, sem código.
      [DESAFIO]  nível de teste. Solução no fim do ficheiro.
      [SOZINHA]  sem solução.
  - Nos [DESAFIO], tenta primeiro numa folha de papel (obriga a pensar
    antes de correr), e só depois escreve e corre aqui.
  - Se encravares: email ao professor das práticas com o número do exercício,
    o que já fizeste, onde encravaste e o ficheiro em anexo.

  A numeração continua a dos ficheiros anteriores: este começa na secção 21.
"""






# %%
"""
===========================================================================
21. CONSTRUIR UM PROGRAMA, PASSO A PASSO [~30 min]
===========================================================================

    Qualquer programa, pequeno ou grande, constrói-se com estes 4 passos:

    PASSO 1. Perceber o problema e escrevê-lo em português.
             O que ENTRA? O que SAI? Quais são as REGRAS? E os casos INVÁLIDOS?
             Depois, os passos do programa, como se o explicasses a alguém.
             Cada passo "de pensar" (uma pergunta, uma conta) vira o NOME
             de uma função. Ler e escrever ficam na main.

    PASSO 2. Escrever o esqueleto: os cabeçalhos das funções e a main.
             Cada função devolve um valor qualquer, só para o programa correr.

    PASSO 3. Implementar e testar UMA função de cada vez.
             Escreves o corpo de uma função e testa-la logo, sozinha,
             com valores de que sabes a resposta.
             Só quando estiver certa passas à seguinte.

    PASSO 4. Testar tudo junto: correr a main com cada exemplo do enunciado
             (também os inválidos) e passar a checklist (Ex 21.6).

    Nesta secção vamos fazer os 4 passos com o mesmo problema (Ex 21.1 a 21.4),
    e depois com um enunciado (Ex 21.5).

Ex 21.1 [EXEMPLO] Passo 1: o programa em português -------------------------

    Problema: registar um utilizador. O programa pede o nome e o email.
    Se o email não for válido, ou já estiver registado, escreve uma
    mensagem de erro. Senão, regista o email e confirma o registo.

    Perceber o problema:
        entra:     o nome e o email
        sai:       uma mensagem (registado, ou o erro)
        regras:    o email tem de ser válido e ainda não estar registado
        inválidos: email inválido; email já registado

    Os passos em português, cada um com o nome de uma função:

        "ler o nome e o email"          -> fica na main (são dois input)
        "o email é válido?"             -> is_valid_email(email)
        "o email já está registado?"    -> email_exists(email, emails)
        "registar o email"              -> register_user(email, emails)
        "dizer o que aconteceu"         -> fica na main (são prints)

    Repara: ainda não escrevemos uma linha de código,
    e já sabemos que funções vamos ter e o que cada uma faz.
"""






# %%
"""
Ex 21.2 [EXEMPLO] Passo 2: o esqueleto -------------------------------------

    Escrever o esqueleto do programa:
      - converter os passos do Ex 21.1 em funções (só os cabeçalhos);
      - pensar que argumentos cada função recebe e o que devolve (os tipos);
      - escrever um comentário simples do que a função faz (a docstring:
        depois corrige-se e completa-se);
      - pôr um valor de retorno qualquer, só para o programa correr;
      - escrever a main completa, a chamar as funções pela ordem dos passos.
"""

def is_valid_email(email: str) -> bool:
    """ Check if email looks like a valid email address. """
    return True                     # esqueleto: para já, todos são válidos

def email_exists(email: str, emails: list[str]) -> bool:
    """ Check if email is already registered. """
    return False                    # esqueleto

def register_user(email: str, emails: list[str]) -> None:
    """ Register email (adds it to the list emails). """
    pass                            # esqueleto

def main() -> None:
    emails = ["ana@mail.pt", "rui@mail.pt"]     # os emails já registados
    name = input("Nome: ")
    email = input("Email: ")
    if not is_valid_email(email):
        print("Email inválido")
    elif email_exists(email, emails):
        print("Esse email já está registado")
    else:
        register_user(email, emails)
        print(f"Utilizador {name} registado")

main()

# Corre: o programa já funciona de ponta a ponta (regista sempre, para já).
#
# Lê a main em voz alta: é o português do Ex 21.1.
#   "Se o email não é válido: erro. Senão, se já existe: erro. Senão: regista."
# Os nomes das funções fazem o trabalho dos comentários.
#
# Ainda não sabemos COMO se verifica um email. Não faz mal:
# o esqueleto deixa pensar no programa todo primeiro, e corrê-lo já.
# Pensa-se em português; os nomes escrevem-se em inglês (regra da cadeira).
#
# O que fica na main, e o que vira função?
#   - falar com o utilizador (input, print): a main;
#   - perguntas e contas (é válido? existe? registar): funções.
#
# As mensagens são escritas pela main.
# As funções devolvem DADOS (True ou False), não frases.
# Assim a mesma função serve noutro programa, com outras mensagens.
#
# A lista emails é passada às funções, e não usada "de fora":
# cada função só usa o que recebe, e por isso testa-se sozinha.






# %%
"""
Ex 21.3 [EXEMPLO] Passos 3 e 4: uma função de cada vez, e depois tudo junto

    PASSO 3: escrever o código de UMA função e testá-la logo, sozinha:
    chamá-la diretamente com vários valores e ver se devolve o que esperamos.
    Só depois passar à função seguinte.
    (Neste exercício, um email é "válido" se tiver um "@" e um ".")
"""

# --- 1ª função: implementar...
def is_valid_email(email: str) -> bool:
    """ Check if email looks like a valid email address (has "@" and "."). """
    return "@" in email and "." in email

# ...e testar LOGO, antes de passar à seguinte
print(is_valid_email("ana@mail.pt"))    # esperado: True
print(is_valid_email("ana.mail.pt"))    # esperado: False

# --- 2ª função
def email_exists(email: str, emails: list[str]) -> bool:
    """ Check if email is already registered. """
    return email in emails

# testes: uma lista feita à mão, só para simular os emails já registados
test_emails = ["ana@mail.pt", "rui@mail.pt"]
print(email_exists("ana@mail.pt", test_emails))     # esperado: True
print(email_exists("eva@mail.pt", test_emails))     # esperado: False

# --- 3ª função
def register_user(email: str, emails: list[str]) -> None:
    """ Register email (adds it to the list emails). """
    emails.append(email)

register_user("eva@mail.pt", test_emails)
print(test_emails)      # esperado: ['ana@mail.pt', 'rui@mail.pt', 'eva@mail.pt']

# --- PASSO 4: tudo junto. A main é a do esqueleto, sem mudar NADA.
def main() -> None:
    emails = ["ana@mail.pt", "rui@mail.pt"]     # os emails já registados
    name = input("Nome: ")
    email = input("Email: ")
    if not is_valid_email(email):
        print("Email inválido")
    elif email_exists(email, emails):
        print("Esse email já está registado")
    else:
        register_user(email, emails)
        print(f"Utilizador {name} registado")

main()

# Corre três vezes: com eva@mail.pt, com rui@mail.pt e com eva.mail.pt.
# Agora sim, os testes globais: a main chama todas as funções,
# e testamos também o input, o output e o caminho todo do programa.
#
# Porquê uma função de cada vez?
#   - Se escreves tudo e só no fim testas, e dá errado: o erro está ONDE?
#   - Se testas cada função logo, sabes que o que está para trás funciona.
#     Um erro novo só pode estar na função que acabaste de escrever.
#
# Testar uma função sozinha é rápido: sem input, sem correr o programa todo.
# Testar só pela main obriga a escrever os inputs à mão, de cada vez.
#
# Prints de "debug" dentro das funções (print(x) para ver o que se passa)
# são para ti, enquanto procuras um erro. Tiram-se antes de entregar.






# %%
"""
Ex 21.4 [EXEMPLO] Arrumar os testes numa função ----------------------------

    Corre primeiro a célula do Ex 21.3 (é aí que estão as funções).

    No Ex 21.3, os testes são prints soltos no meio do ficheiro.
    Problema: aparecem também quando corremos o programa final.
    No Mooshak isso é GRAVE: o output deixa de ser igual ao pedido,
    e a resposta é dada como errada.

    Melhor: juntar os testes numa função de testes.
    Enquanto constróis, chamas a função. No fim, comentas a chamada
    (ou apagas), e os testes deixam de correr.
"""

def test_functions() -> None:
    """ The same local tests as in Ex 21.3, all together in one function. """
    # testes de is_valid_email
    print(is_valid_email("ana@mail.pt"), "(esperado: True)")
    print(is_valid_email("ana.mail.pt"), "(esperado: False)")

    # testes de email_exists
    print(email_exists("rui@mail.pt", ["ana@mail.pt", "rui@mail.pt"]), "(esperado: True)")
    print(email_exists("eva@mail.pt", ["ana@mail.pt"]), "(esperado: False)")

    # testes de register_user
    emails = ["ana@mail.pt"]
    register_user("eva@mail.pt", emails)
    print(emails, "(esperado: ['ana@mail.pt', 'eva@mail.pt'])")

test_functions() # no programa final comentamos esta linha e os testes não corem!!!

# Escrever o "esperado" ao lado do resultado ajuda a ver logo se bate certo.
# Se uma linha não bate certo, sabes QUAL função tem o problema.
#
# Quando acrescentas uma função nova, acrescentas os testes dela aqui.
# E sempre que mudas alguma coisa, voltas a correr TODOS os testes:
# confirmas que o que já funcionava continua a funcionar.






# %%
"""
Ex 21.5 [EXEMPLO] Os 4 passos, a partir de um enunciado --------------------

    Enunciado:
    Escreva um programa completo que calcule os portes de envio de uma
    encomenda. O programa pergunta o peso (em kg, real) e se o envio é
    urgente ('s' ou 'n'). As regras são:
      - até 1 kg (inclusive): 3.50 euros;
      - mais de 1 kg e até 5 kg (inclusive): 6.00 euros;
      - mais de 5 kg: 6.00 euros mais 1.20 euros por cada kg acima dos 5;
      - envio urgente: mais 50% sobre o valor anterior.
    Se o peso não for positivo, o programa escreve "Peso inválido".

    Exemplos de execução (imite esta apresentação de forma rigorosa):

        Peso (kg): 0.5            Peso (kg): 7              Peso (kg): -2
        Urgente (s/n)? n          Urgente (s/n)? s          Peso inválido
        Portes: 3.50 euros        Portes: 12.60 euros

    Corre três vezes, com os três exemplos.
"""

# Constantes: os números do enunciado, com nome
PRICE_UP_TO_1KG = 3.50
PRICE_UP_TO_5KG = 6.00
PRICE_PER_EXTRA_KG = 1.20
URGENT_RATE = 0.50

def base_cost(weight: float) -> float:
    """ Shipping cost before the urgent extra.
        Precondition: weight > 0
    """
    if weight <= 1:
        return PRICE_UP_TO_1KG
    if weight <= 5:
        return PRICE_UP_TO_5KG
    return PRICE_UP_TO_5KG + PRICE_PER_EXTRA_KG * (weight - 5)

def total_cost(weight: float, urgent: bool) -> float:
    """ Final shipping cost.
        Precondition: weight > 0
    """
    cost = base_cost(weight)
    if urgent:
        cost = cost * (1 + URGENT_RATE)
    return cost

def main() -> None:
    weight = float(input("Peso (kg): "))
    if not (weight > 0):
        print("Peso inválido")
    else:
        urgent = input("Urgente (s/n)? ") == "s"
        print(f"Portes: {total_cost(weight, urgent):.2f} euros")

main()

# Os 4 passos, neste enunciado:
#
# PASSO 1. Perceber e escrever em português:
#     entra o peso e "urgente?"; sai o valor dos portes;
#     regras: os escalões e os 50% da urgência; inválido: peso <= 0.
#       "ler o peso"                      -> main
#       "o peso é válido?"                -> main (um if: if not (weight > 0))
#       "ler se é urgente"                -> main
#       "quanto custa pelo peso?"         -> base_cost(weight)
#       "e com a urgência por cima?"      -> total_cost(weight, urgent)
#       "escrever os portes"              -> main
#     Os números do enunciado viram CONSTANTES, com nomes que se percebem.
#
# PASSO 2. Esqueleto: as constantes, os cabeçalhos de base_cost e total_cost
#     (com "return 0.0") e a main completa. O programa já corre.
#
# PASSO 3. Uma função de cada vez:
#     base_cost, e testá-la logo:
#         base_cost(0.5) -> 3.5    base_cost(5) -> 6.0    base_cost(7) -> 8.4
#     só depois total_cost:
#         total_cost(7, False) -> 8.4    total_cost(7, True) -> 12.600000000000001
#     (o float não é exato; o :.2f do print escreve 12.60, como pede o exemplo)
#
# PASSO 4. Tudo junto, com os 3 exemplos do enunciado:
#     7 kg urgente: 6.00 + 1.20 * 2 = 8.40;  8.40 * 1.5 = 12.60.  Bate certo.
#     -2 kg: "Peso inválido", e não pergunta se é urgente. Bate certo.
#
# Pormenores deste programa:
#   - A ORDEM das perguntas conta: no exemplo inválido, o programa nem
#     pergunta se é urgente. Por isso o 2º input está DENTRO do else.
#   - input(...) == "s" dá logo um bool: True se respondeu "s".
#   - A validação na main é a precondição "copiada" para um if not (...):
#     (ficheiro 3, Ex 10.2).






# %%
"""
Ex 21.6 [EXEMPLO] A checklist de um programa bem organizado ---------------

    Quando acabares um programa, confirma linha a linha.
    São as marcas de um programa bem organizado, fácil de ler e de corrigir.
"""

# ORGANIZAÇÃO
#   [ ] Há funções para a LÓGICA (as contas), sem input nem print.
#   [ ] Há uma main que lê (input), valida, chama e escreve (print).
#   [ ] A chamada main() está no fim do programa.
#   [ ] Os números fixos do enunciado são constantes (MAIÚSCULAS, no início).
#   [ ] Nenhuma variável global é usada dentro das funções (passa por parâmetro).
#   [ ] Os nomes dizem o que cada coisa é ou faz (secção 22).
#
# CADA FUNÇÃO
#   [ ] Tem um comentário inicial (docstring) a dizer o que faz.
#   [ ] Tem precondição, se houver valores que ela não aceita.
#   [ ] Tem tipos nos parâmetros e no resultado (x: float) -> float.
#   [ ] Devolve com return (e não com print).
#
# INPUT E OUTPUT
#   [ ] Os textos das perguntas e das respostas são IGUAIS aos do exemplo
#       (acentos, espaços, ":" ou "=", maiúsculas).
#   [ ] As casas decimais são as do exemplo (45.0? 45.00? 2.055556?).
#   [ ] O input foi convertido (int ou float) antes das contas.
#   [ ] Os casos inválidos escrevem exatamente a mensagem pedida.
#   [ ] A ordem das perguntas é a do exemplo (inclusive nos casos inválidos).
#
# E NO FIM
#   [ ] Cada função foi testada sozinha, logo que ficou escrita (passo 3).
#   [ ] Correste a main com TODOS os exemplos do enunciado e bate certo (passo 4).
#   [ ] Tiraste os prints de debug, e a chamada aos testes está comentada.
#
# Se uma das contas não te sair à primeira: escreve na mesma a main e os
# cabeçalhos das funções. Ficas com o programa organizado, e o que falta
# fica bem localizado (é só preencher aquela função).






# %%
"""
Ex 21.7 [FAZ] Os 4 passos: bilhete de comboio ----------------------------

    Escreva um programa completo que calcule o preço de um bilhete de comboio.
    O programa pergunta a idade (inteiro) e a distância da viagem (km, real).
      - O preço base é 0.10 euros por km, mas nunca menos de 2.00 euros.
      - Menores de 12 anos têm 50% de desconto; 65 ou mais anos, 40%.
      - Se a idade for negativa ou a distância não for positiva,
        o programa escreve "Dados inválidos".

        Idade: 70                 Idade: 30                 Idade: -1
        Distância (km): 150       Distância (km): 12        Distância (km): 50
        Preço: 9.00 euros         Preço: 2.00 euros         Dados inválidos

    a) Passo 1: o que entra, o que sai, as regras, os inválidos,
       e os passos em português (em comentário), cada um com a sua função.
    b) Passo 2: as constantes, as funções em esqueleto e a main completa.
       Tem de correr (vai escrever "Preço: 0.00 euros").
    c) Passo 3: implementa e testa uma função de cada vez.
    d) Passo 4: corre a main com os 3 exemplos e passa a checklist.
"""

# TODO















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================
















# %%
# Solução do Ex 21.7
#
# a) PASSO 1. Perceber e escrever em português:
#      entra a idade e a distância; sai o preço;
#      regras: 0.10/km com mínimo 2.00, e os descontos por idade;
#      inválido: idade < 0 ou distância <= 0.
#        "ler a idade e a distância"       -> main (o exemplo inválido pergunta as duas!)
#        "os dados são válidos?"           -> main (if not (age >= 0 and distance > 0))
#        "quanto custa pela distância?"    -> base_price(distance)
#        "que desconto tem esta idade?"    -> discount(age)
#        "o preço final"                   -> ticket_price(age, distance)
#        "escrever o preço"                -> main
#
# b) PASSO 2. O esqueleto: as funções abaixo, com "return 0.0" no corpo.
#    Com ele, a main já corria e escrevia "Preço: 0.00 euros".
#
# c) PASSO 3. Uma função de cada vez, cada uma testada logo:
#      base_price(150) -> 15.0     base_price(12) -> 2.0 (o mínimo)
#      discount(70) -> 0.4         discount(30) -> 0.0      discount(8) -> 0.5
#      ticket_price(70, 150) -> 9.0   (só junta as duas anteriores)
#
# d) PASSO 4. O programa todo, com os 3 exemplos (aqui em baixo).

PRICE_PER_KM = 0.10
MIN_PRICE = 2.00
CHILD_AGE = 12
SENIOR_AGE = 65
CHILD_DISCOUNT = 0.50
SENIOR_DISCOUNT = 0.40

def base_price(distance: float) -> float:
    """ Price before discounts: PRICE_PER_KM per km, at least MIN_PRICE.
        Precondition: distance > 0
    """
    price = PRICE_PER_KM * distance
    if price < MIN_PRICE:
        return MIN_PRICE
    return price

def discount(age: int) -> float:
    """ Discount rate for a given age (0.5, 0.4 or 0.0).
        Precondition: age >= 0
    """
    if age < CHILD_AGE:
        return CHILD_DISCOUNT
    if age >= SENIOR_AGE:
        return SENIOR_DISCOUNT
    return 0.0

def ticket_price(age: int, distance: float) -> float:
    """ Final price: base price minus the discount.
        Precondition: age >= 0 and distance > 0
    """
    return base_price(distance) * (1 - discount(age))

def main() -> None:
    age = int(input("Idade: "))
    distance = float(input("Distância (km): "))
    if not (age >= 0 and distance > 0):
        print("Dados inválidos")
    else:
        print(f"Preço: {ticket_price(age, distance):.2f} euros")

main()

# Confirmação dos exemplos:
#   70 anos, 150 km: 15.00 com 40% de desconto -> 9.00
#   30 anos, 12 km:  1.20 é menos do que o mínimo -> 2.00, sem desconto
#
# O exemplo inválido pergunta as duas coisas: os dois input vêm antes do if.
#
# O mínimo de 2.00 é antes ou depois do desconto?
# O enunciado diz "o preço base nunca é menos de 2.00": antes.
# Se um enunciado for ambíguo, escolhe uma interpretação e escreve
# a decisão num comentário: quem ler o código percebe porquê.






# %%
"""
===========================================================================
22. CÓDIGO QUE SE LÊ [~20 min]
===========================================================================

Ex 22.1 [EXEMPLO] Bons nomes -------------------------------------------------

    As duas versões fazem exatamente o mesmo. Qual percebes à primeira?
"""

# Versão A
def f(a: float, b: float) -> float:
    x = a * b
    if x > 100:
        x = x * 0.9
    return x

# Versão B
DISCOUNT_THRESHOLD = 100
DISCOUNT_RATE = 0.10

def total_price(unit_price: float, quantity: int) -> float:
    """ Price of quantity units, with a discount on big purchases. """
    total = unit_price * quantity
    if total > DISCOUNT_THRESHOLD:
        total = total * (1 - DISCOUNT_RATE)
    return total

print(f(30, 4), total_price(30, 4))         # 108.0 108.0

# As convenções da cadeira (as de quase todo o código Python):
#   - funções e variáveis: minúsculas_com_underscore   (total_price, unit_price)
#   - constantes: MAIÚSCULAS                           (DISCOUNT_RATE)
#   - funções que fazem algo: um verbo                 (read_values, register_user)
#   - funções que devolvem algo: o que devolvem        (total_price, average)
#   - booleanos: uma pergunta                          (is_valid_email, has_seats)
#   - listas: no plural                                (emails, grades)
#   - nomes curtos (i, v) só para índices e valores dentro de um ciclo
#
# Os tipos (: float, -> float) e a docstring não mudam o que o programa faz:
# o Python nem os verifica. Servem para quem LÊ saber o que entra e o que sai.
# Quem lê pode ser o professor, um colega, ou tu daqui a um mês.






# %%
"""
Ex 22.2 [EXEMPLO] Uma função, uma tarefa ------------------------------------
"""

# Mistura duas tarefas: calcula E escreve
def show_average(l: list[float]) -> None:
    """ Print the average of l. Precondition: len(l) > 0 """
    total = 0
    for v in l:
        total += v
    print(f"Média: {total / len(l):.2f}")

# Uma tarefa: calcula e devolve. Quem escreve é a main.
def average(l: list[float]) -> float:
    """ Average of the values of l. Precondition: len(l) > 0 """
    total = 0
    for v in l:
        total += v
    return total / len(l)

grades = [12.0, 15.5, 9.0]
show_average(grades)                        # Média: 12.17
print(f"Média: {average(grades):.2f}")      # Média: 12.17
print(average(grades) >= 9.5)               # True

# As duas escrevem o mesmo. Mas só a average serve para mais coisas:
# comparar médias, usar noutra conta, testar.
# A show_average não devolve nada: o resultado fica preso no ecrã.
#
# Um "efeito lateral" é uma função mexer em algo fora dela:
# escrever no ecrã, ou alterar uma lista que recebeu.
# Às vezes é exatamente a tarefa (register_user, print_table).
# Então o nome e o -> None dizem-no.
# O que se evita é o efeito ESCONDIDO: uma função chamada average
# que, sem avisar, também apagasse valores da lista.






# %%
"""
Ex 22.3 [EXEMPLO] O mesmo código em dois sítios? »» Uma função ------------------
"""

book = 20.0
pen = 2.0

# Antes: a mesma conta repetida
print(f"Livro com IVA: {book * 1.23:.2f}")      # Livro com IVA: 24.60
print(f"Caneta com IVA: {pen * 1.23:.2f}")      # Caneta com IVA: 2.46

# Depois: a conta tem um nome e mora num sítio só
VAT_RATE = 0.23

def with_vat(price: float) -> float:
    """ Price with VAT. """
    return price * (1 + VAT_RATE)

print(f"Livro com IVA: {with_vat(book):.2f}")   # Livro com IVA: 24.60
print(f"Caneta com IVA: {with_vat(pen):.2f}")   # Caneta com IVA: 2.46

# Mudar o código sem mudar o que ele faz chama-se REFACTORING.
#
# Porquê fazê-lo?
#   - se o IVA mudar, muda-se uma linha, e não todas as contas espalhadas;
#   - se a conta estiver errada, corrige-se num sítio;
#   - a função testa-se sozinha.
#
# A regra: escreveste a mesma coisa duas vezes? É uma função à espera de nome.






# %%
"""
Ex 22.4 [FAZ] Partir um programa em funções -------------------------------

    Este programa funciona, mas está todo dentro da main e com nomes que
    não dizem nada. Reescreve-o com: uma função que calcula o IMC, outra que
    dá a classificação, constantes para os limites, e bons nomes.
    O output tem de ficar IGUAL.

        Peso (kg): 70
        Altura (m): 1.75
        IMC: 22.9 (Normal)
"""

def main() -> None:
    w = float(input("Peso (kg): "))
    h = float(input("Altura (m): "))
    x = w / (h * h)
    if x < 18.5:
        c = "Magreza"
    elif x < 25:
        c = "Normal"
    elif x < 30:
        c = "Excesso de peso"
    else:
        c = "Obesidade"
    print(f"IMC: {x:.1f} ({c})")

main()

# TODO: a tua versão















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================
















# %%
# Solução do Ex 22.4

UNDERWEIGHT_LIMIT = 18.5
NORMAL_LIMIT = 25
OVERWEIGHT_LIMIT = 30

def bmi(weight: float, height: float) -> float:
    """ Body mass index. Precondition: height > 0 """
    return weight / (height * height)

def bmi_class(value: float) -> str:
    """ Name of the class of a body mass index. """
    if value < UNDERWEIGHT_LIMIT:
        return "Magreza"
    if value < NORMAL_LIMIT:
        return "Normal"
    if value < OVERWEIGHT_LIMIT:
        return "Excesso de peso"
    return "Obesidade"

def main() -> None:
    weight = float(input("Peso (kg): "))
    height = float(input("Altura (m): "))
    value = bmi(weight, height)
    print(f"IMC: {value:.1f} ({bmi_class(value)})")

main()

# Os passos, num programa que já existe:
#   PASSO 1: ler o código e escrevê-lo em português:
#       "ler peso e altura"                -> main
#       "calcular o IMC"                   -> bmi(weight, height)
#       "que classe é este IMC?"           -> bmi_class(value)
#       "escrever"                         -> main
#   PASSO 3: testar as funções sozinhas:
#       bmi(70, 1.75) -> 22.857...   bmi_class(22.9) -> "Normal"   bmi_class(30) -> "Obesidade"
#   PASSO 4: o output tem de ficar IGUAL ao do programa original.
#
# Aqui a função devolve um TEXTO (a classificação), e está certo:
# o nome da classe é o resultado da conta, não uma mensagem da conversa.
#
# A main ficou com 4 linhas: ler, ler, calcular, escrever.
#
# Faltou validar? O original também não validava (altura 0 dava erro).
# Num enunciado que peça validação: if not (height > 0) na main.






# %%
"""
===========================================================================
23. ESCREVER EXATAMENTE O FORMATO PEDIDO [~15 min]
===========================================================================

Ex 23.1 [EXEMPLO] Casas decimais: olha para o exemplo do enunciado ---------
"""

x = 45.0
print("Custo =", x)             # Custo = 45.0
print(f"Custo = {x}")           # Custo = 45.0
print(f"Custo = {x:.2f}")       # Custo = 45.00

y = 2.0555555555
print(f"{y:.6f}")               # 2.055556
print(f"{y:.1f}")               # 2.1

n = 7
print(f"{n:.2f}")               # 7.00   (um int também se escreve com casas decimais)

print(round(y, 2))              # 2.06

# A regra: copiar o que o EXEMPLO mostra.
#   Exemplo com "45.0"     -> print normal do float.
#   Exemplo com "45.00"    -> f"{x:.2f}".
#   Exemplo com "2.055556" -> f"{x:.6f}".
#
# O :.2f arredonda só na ESCRITA. O valor da variável não muda.
# O round(y, 2) muda o NÚMERO. Para escrever, o f-string chega.






# %%
"""
Ex 23.2 [EXEMPLO] Vírgulas, + e f-strings: onde aparecem os espaços --------
"""

a = 3
b = 8
print("A:", a, "B:", b)         # A: 3 B: 8      a vírgula põe UM espaço entre as peças
print("A:" + str(a))            # A:3            o + cola, sem espaço
print(f"A: {a} B: {b}")         # A: 3 B: 8      no f-string, os espaços são os que escreves
print(a, b, sep=" - ")          # 3 - 8          sep troca o espaço por outra coisa

# Nas perguntas, o espaço depois do ":" ou do "?" vai DENTRO do texto:
#     input("Peso (kg): ")      -> o cursor fica depois do espaço
#
# Para respostas com várias peças, o f-string é o mais seguro:
# o que escreves entre as aspas é exatamente o que aparece.






# %%
"""
Ex 23.3 [FAZ] Imita estes outputs ---------------------------------------

    Com as variáveis abaixo, escreve 4 prints que deem EXATAMENTE:

        Aluno 3: Rita, nota 15.46
        Total = 7.0
        10 km = 6.21 milhas
        Rita;15.5
"""

name = "Rita"
number = 3
grade = 15.456
total = 7.0
km = 10
miles = 6.2137119

# TODO: 4 prints
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================
















# %%
# Solução do Ex 23.3

name = "Rita"
number = 3
grade = 15.456
total = 7.0
km = 10
miles = 6.2137119

print(f"Aluno {number}: {name}, nota {grade:.2f}")     # Aluno 3: Rita, nota 15.46
print("Total =", total)                                # Total = 7.0
print(f"{km} km = {miles:.2f} milhas")                 # 10 km = 6.21 milhas
print(f"{name};{grade:.1f}")                           # Rita;15.5

# O 2º também se faz com f"Total = {total}".
# No 4º não há espaços: com vírgulas no print aparecia "Rita ; 15.5".






# %%
"""
Ex 23.4 [DESAFIO] Encontra as 3 diferenças ---------------------------------

    O enunciado pedia este output:

        Média: 12.50
        Aprovados: 3 de 4
        Melhor nota: 18.0

    Um aluno escreveu estes prints (com avg = 12.5, count = 3, n = 4, best = 18.0).
    Sem correr: que linhas saem diferentes, e porquê?
    Solução no fim do ficheiro.
"""

avg = 12.5
count = 3
n = 4
best = 18.0

print("Media:", round(avg, 2))
print("Aprovados:", count, "de", n)
print(f"Melhor: {best}")






# %%
"""
===========================================================================
24. ESCOLHER PELA RESPOSTA DO UTILIZADOR [~15 min]
===========================================================================

Ex 24.1 [EXEMPLO] Um menu de duas opções (e "qualquer outra resposta") -----

    Corre três vezes: com k, com m, e com outra letra qualquer.
"""

KM_PER_MILE = 1.609344

def km_to_miles(km: float) -> float:
    """ Convert kilometres to miles. """
    return km / KM_PER_MILE

def miles_to_km(miles: float) -> float:
    """ Convert miles to kilometres. """
    return miles * KM_PER_MILE

def main() -> None:
    option = input("Converter: k (km -> milhas) ou m (milhas -> km)? ")
    if option == "k":
        km = float(input("Quilómetros: "))
        print(f"{km:.2f} km = {km_to_miles(km):.2f} milhas")
    elif option == "m":
        miles = float(input("Milhas: "))
        print(f"{miles:.2f} milhas = {miles_to_km(miles):.2f} km")
    else:
        print("Opção inválida")

main()

# O if/elif/else da main decide a CONVERSA:
# que pergunta fazer a seguir e que função chamar.
# As contas continuam nas funções.
#
# "Qualquer outra resposta" é o else.
# Se o enunciado disser que o programa não escreve nada, não há else
# (é o caso do Ex 26.1).
#
# As strings comparam-se À LETRA: "K" (maiúsculo) não é "k" e cai no else.






# %%
"""
Ex 24.2 [EXEMPLO] Dados inválidos: duas formas de terminar mais cedo -------

    Corre com 0 e com 3 (e depois três números).
"""

def average3(a: float, b: float, c: float) -> float:
    """ Average of three numbers. """
    return (a + b + c) / 3

# Forma 1: if / else (a que usámos até agora)
def main() -> None:
    n = int(input("Quantos números (só 3 é válido)? "))
    if n != 3:
        print("Número inválido")
    else:
        a = float(input("a: "))
        b = float(input("b: "))
        c = float(input("c: "))
        print(f"Média: {average3(a, b, c):.2f}")

main()

# Forma 2: return logo a seguir à mensagem (a main é uma função!)
#
# def main() -> None:
#     n = int(input("Quantos números (só 3 é válido)? "))
#     if n != 3:
#         print("Número inválido")
#         return                  # sai da main aqui: o resto não corre
#     a = float(input("a: "))
#     ...
#
# As duas estão certas. A forma 2 evita ter o programa todo dentro de um else
# quando há várias validações seguidas. Usa a que te parecer mais clara.






# %%
"""
Ex 24.3 [FAZ] Áreas de figuras --------------------------------------------

    O programa pergunta a figura:
      - 'q' quadrado: pergunta o lado;
      - 'c' círculo: pergunta o raio;
      - 't' triângulo: pergunta a base e a altura;
      - outra resposta: escreve "Figura inválida".
    E escreve a área com 2 casas decimais. (área do círculo: pi * r * r)

        Figura (q/c/t)? c         Figura (q/c/t)? t         Figura (q/c/t)? x
        Raio: 2                   Base: 3                   Figura inválida
        Área: 12.57               Altura: 5
                                  Área: 7.50
"""

import math

def square_area(side: float) -> float:
    """ Area of a square. Precondition: side >= 0 """
    pass        # TODO

def circle_area(radius: float) -> float:
    """ Area of a circle. Precondition: radius >= 0 """
    pass        # TODO

def triangle_area(base: float, height: float) -> float:
    """ Area of a triangle. Precondition: base >= 0 and height >= 0 """
    pass        # TODO

def main() -> None:
    pass        # TODO

main()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================
















# %%
# Solução do Ex 24.3

import math

def square_area(side: float) -> float:
    """ Area of a square. Precondition: side >= 0 """
    return side * side

def circle_area(radius: float) -> float:
    """ Area of a circle. Precondition: radius >= 0 """
    return math.pi * radius * radius

def triangle_area(base: float, height: float) -> float:
    """ Area of a triangle. Precondition: base >= 0 and height >= 0 """
    return base * height / 2

def main() -> None:
    shape = input("Figura (q/c/t)? ")
    if shape == "q":
        side = float(input("Lado: "))
        print(f"Área: {square_area(side):.2f}")
    elif shape == "c":
        radius = float(input("Raio: "))
        print(f"Área: {circle_area(radius):.2f}")
    elif shape == "t":
        base = float(input("Base: "))
        height = float(input("Altura: "))
        print(f"Área: {triangle_area(base, height):.2f}")
    else:
        print("Figura inválida")

main()

# Os passos, em português:
#   "ler a figura"                      -> main
#   "conforme a figura, ler as medidas" -> main (cada ramo pergunta o seu)
#   "calcular a área"                   -> square_area / circle_area / triangle_area
#   "escrever a área, ou o erro"        -> main
# Passo 3: cada função de área testa-se num segundo:
#   square_area(2) -> 4    triangle_area(3, 5) -> 7.5    circle_area(1) -> 3.14159...
#
# Cada ramo pergunta só o que precisa: as perguntas DEPENDEM da resposta.
# Por isso os inputs estão dentro dos ramos, e não todos no início.
#
# O print("Área: ...") repete-se 3 vezes. Dava para guardar a área numa
# variável e escrever uma vez no fim, mas o ramo "inválida" não tem área.
# Um pouco de repetição na main não é grave.
# Contas na main são: misturam a conversa com a lógica.






# %%
"""
===========================================================================
25. PROGRAMAS COM CICLOS E LISTAS [~25 min]
===========================================================================

Ex 25.1 [EXEMPLO] Uma tabela feita com um ciclo -----------------------------

        Partida (km)? 10
        Quantas linhas? 4
        10.0 km = 6.214 milhas
        11.0 km = 6.835 milhas
        12.0 km = 7.456 milhas
        13.0 km = 8.078 milhas
"""

KM_PER_MILE = 1.609344

def km_to_miles(km: float) -> float:
    """ Convert kilometres to miles. """
    return km / KM_PER_MILE

def print_table(start: float, lines: int) -> None:
    """ Print a km -> miles table, from start, in steps of 1 km.
        Precondition: lines > 0
    """
    for i in range(lines):
        km = start + i
        print(f"{km:.1f} km = {km_to_miles(km):.3f} milhas")

def main() -> None:
    start = float(input("Partida (km)? "))
    lines = int(input("Quantas linhas? "))
    if not (lines > 0):
        print("Número de linhas inválido")
    else:
        print_table(start, lines)

main()

# Três camadas:
#   km_to_miles  -> a CONTA (sem input nem print);
#   print_table  -> ESCREVE a tabela: tem print, e não faz mal.
#                   É uma função "de conversa", chamada pela main.
#                   A conta continua a ser feita pela km_to_miles;
#   main         -> lê, valida e chama.
#
# Porquê km = start + i, e não km += 1 em cada volta?
# O i é um inteiro: start + i é sempre exato a partir do start.
# Somar 1 a um float muitas vezes vai acumulando erros (aula 4).
#
# Vais reencontrar este esqueleto no Ex 26.1.






# %%
"""
Ex 25.2 [EXEMPLO] Ler N valores para uma lista ------------------------------

        Quantos valores? 4
        Valor 1: 10
        Valor 2: 12
        Valor 3: 15
        Valor 4: 9
        Média: 11.50
        Acima da média: 2
"""

def read_values(n: int) -> list[float]:
    """ Read n real values from the user.
        Precondition: n > 0
    """
    values = []
    for i in range(n):
        values.append(float(input(f"Valor {i + 1}: ")))
    return values

def average(l: list[float]) -> float:
    """ Average of the values of l.
        Precondition: len(l) > 0
    """
    total = 0
    for v in l:
        total += v
    return total / len(l)

def count_above(l: list[float], limit: float) -> int:
    """ How many values of l are greater than limit. """
    count = 0
    for v in l:
        if v > limit:
            count += 1
    return count

def main() -> None:
    n = int(input("Quantos valores? "))
    if not (n > 0):
        print("Número inválido")
    else:
        values = read_values(n)
        avg = average(values)
        print(f"Média: {avg:.2f}")
        print(f"Acima da média: {count_above(values, avg)}")

main()

# O padrão:
#   1. ler o N e validar;
#   2. ler os N valores para uma lista (função de leitura: tem input, e não faz mal);
#   3. as contas sobre a lista, em funções sem input nem print
#      (são as funções da secção 14 do ficheiro 4);
#   4. escrever.
#
# Repara no f"Valor {i + 1}: ": o i começa em 0, as pessoas contam a partir de 1.
#
# Porquê guardar numa lista? Porque a média só se sabe no FIM,
# e depois é preciso voltar a olhar para todos os valores (acima da média?).






# %%
"""
Ex 25.3 [FAZ] Temperaturas da semana --------------------------------------

    O programa pede as temperaturas dos 7 dias da semana (reais)
    e escreve a média (1 casa decimal), a máxima e o dia em que aconteceu
    (dias de 1 a 7; se houver empate, o primeiro) e quantos dias ficaram
    acima da média. Sem usar sum nem max.

        Dia 1: 14.5
        Dia 2: 16
        Dia 3: 18.5
        Dia 4: 17
        Dia 5: 12
        Dia 6: 15.5
        Dia 7: 18.5
        Média: 16.0
        Máxima: 18.5 (dia 3)
        Dias acima da média: 3
"""

DAYS = 7

def read_temperatures() -> list[float]:
    """ Read the temperatures of the DAYS days of the week. """
    pass        # TODO

def average(l: list[float]) -> float:
    """ Average of the values of l. Precondition: len(l) > 0 """
    pass        # TODO

def position_of_max(l: list[float]) -> int:
    """ Position of the (first) largest value. Precondition: len(l) > 0 """
    pass        # TODO

def count_above(l: list[float], limit: float) -> int:
    """ How many values of l are greater than limit. """
    pass        # TODO

def main() -> None:
    pass        # TODO

main()
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================
















# %%
# Solução do Ex 25.3

DAYS = 7

def read_temperatures() -> list[float]:
    """ Read the temperatures of the DAYS days of the week. """
    temps = []
    for i in range(DAYS):
        temps.append(float(input(f"Dia {i + 1}: ")))
    return temps

def average(l: list[float]) -> float:
    """ Average of the values of l. Precondition: len(l) > 0 """
    total = 0
    for v in l:
        total += v
    return total / len(l)

def position_of_max(l: list[float]) -> int:
    """ Position of the (first) largest value. Precondition: len(l) > 0 """
    pos = 0
    for i in range(1, len(l)):
        if l[i] > l[pos]:           # ">" e não ">=": no empate fica o primeiro
            pos = i
    return pos

def count_above(l: list[float], limit: float) -> int:
    """ How many values of l are greater than limit. """
    count = 0
    for v in l:
        if v > limit:
            count += 1
    return count

def main() -> None:
    temps = read_temperatures()
    avg = average(temps)
    pos = position_of_max(temps)
    print(f"Média: {avg:.1f}")
    print(f"Máxima: {temps[pos]} (dia {pos + 1})")
    print(f"Dias acima da média: {count_above(temps, avg)}")

main()

# Os passos, em português:
#   "ler as 7 temperaturas"             -> read_temperatures() (devolve a lista)
#   "a média"                           -> average(temps)
#   "onde está a máxima?"               -> position_of_max(temps)
#   "quantos dias acima da média?"      -> count_above(temps, avg)
#   "escrever"                          -> main
# Passo 3: as funções de contas testam-se com uma lista feita à mão,
# sem escrever 7 temperaturas de cada vez:
#   average([1.0, 2.0, 6.0]) -> 3.0
#   position_of_max([3.0, 9.0, 9.0]) -> 1
#   count_above([1.0, 5.0, 9.0], 5.0) -> 1
#
# A position_of_max devolve a POSIÇÃO (0 a 6). Com ela temos as duas coisas:
#   o valor  -> temps[pos]
#   o dia    -> pos + 1   (as pessoas contam a partir de 1)
# Uma função que devolvesse só o máximo não dizia em que dia foi.
#
# Aqui não há validação: são sempre 7 dias, e o enunciado não fala em inválidos.
# "Máxima: 18.5": o exemplo não tem casas fixas, por isso é o print normal do float.






# %%
"""
Ex 25.4 [PENSA] Pauta: nem sempre é preciso uma lista -------------------

    O programa pergunta quantos alunos há (se não for positivo:
    "Número inválido"). Depois pede a nota de cada aluno e escreve LOGO
    se está aprovado (nota >= 9.5). No fim, escreve quantos foram aprovados.

        Quantos alunos? 3
        Nota do aluno 1: 12
        Aluno 1: aprovado
        Nota do aluno 2: 7.5
        Aluno 2: reprovado
        Nota do aluno 3: 9.5
        Aluno 3: aprovado
        Aprovados: 2 de 3

    Que funções fazes? Precisas de guardar as notas numa lista? Porquê?
"""

# TODO
















# ======================================================================
#   SOLUÇÃO NA CÉLULA DE BAIXO. Não espreites: faz primeiro por ti!
# ======================================================================
















# %%
# Ideia do Ex 25.4 (sem código):
#
#   Função de lógica: is_approved(grade: float) -> bool  (grade >= 9.5)
#   Constante: PASS_GRADE = 9.5
#
#   main:
#       ler n; se NÃO (n > 0): "Número inválido"
#       senão:
#           approved = 0
#           para i de 1 a n:
#               ler a nota do aluno i
#               se is_approved(nota): escrever "aprovado" e approved += 1
#               senão: escrever "reprovado"
#           escrever "Aprovados: approved de n"
#
#   NÃO é preciso lista: cada nota só é usada UMA vez, logo a seguir a ser lida.
#   Basta um contador. Compara com o Ex 25.2: aí a média só se sabia no fim
#   e era preciso voltar a ver todos os valores. Aí sim, lista.
#
#   Regra: "preciso de voltar a olhar para os valores depois de os ler todos?"
#     sim -> lista;   não -> trata cada um logo que é lido.






# %%
"""
===========================================================================
26. DESAFIOS: PROGRAMAS COMPLETOS [~45 min]
===========================================================================

    Para cada um, segue os 4 passos da secção 21:
      1. perceber (o que entra, o que sai, as regras, os inválidos)
         e escrever o programa em português, uma função por passo;
      2. o esqueleto, que já corre;
      3. implementar e testar uma função de cada vez;
      4. testar tudo junto, com cada exemplo, e passar a checklist (Ex 21.6).
    O passo 1 faz-se bem numa folha de papel. ~15 min cada.
    Soluções no fim do ficheiro.

Ex 26.1 [DESAFIO] Tabelas Celsius / Fahrenheit (Teste 1 2025/26, pergunta 6, 4 valores)

    A equação F = 1.8 * C + 32 converte graus Celsius para graus Fahrenheit.
    A equação de conversão em sentido contrário não é difícil de deduzir.
    Escreva um programa completo que escreva tabelas de conversão entre
    graus Celsius e graus Fahrenheit, nos dois sentidos.
    O seu programa começa por perguntar ao utilizador qual o sentido
    da conversão pretendida:
      - a resposta 'c' indica a escolha da conversão Celsius -> Fahrenheit;
      - a resposta 'f' indica a escolha da conversão Fahrenheit -> Celsius;
      - qualquer outra resposta fará o programa não escrever qualquer tabela.
    Depois o programa pergunta qual o valor inicial da tabela.
    Finalmente, o programa escreve uma tabela de conversão composta por
    uma sequência de 10 correspondências entre temperaturas.
    O incremento é sempre 1 grau.
    Eis um exemplo de execução. Imite este formato de forma rigorosa.

        Conversão: c->f ou f->c? f
        Valor de partida? 35.7
        Fahrenheit ===> Celsius
        35.700000 ===> 2.055556
        36.700000 ===> 2.611111
        37.700000 ===> 3.166667
        38.700000 ===> 3.722222
        39.700000 ===> 4.277778
        40.700000 ===> 4.833333
        41.700000 ===> 5.388889
        42.700000 ===> 5.944444
        43.700000 ===> 6.500000
        44.700000 ===> 7.055556

    Escreva um programa bem organizado, constituído por várias funções,
    seguindo as boas regras de estilo usadas nas aulas. Nas funções que
    escrever, inclua um pequeno comentário inicial e ainda uma precondição,
    se ela for necessária. Para escrever um número real com 6 casas
    decimais use o formato ".6f", por exemplo assim: print(f"{r:.6f}").
"""

# A tua resposta (programa completo):






# %%
"""
Ex 26.2 [DESAFIO] Bilhetes de um espetáculo -------------------------------

    O programa pergunta quantos bilhetes se vão comprar (se o número não for
    positivo, escreve "Número inválido" e termina). Depois pergunta a idade
    de cada pessoa (inteiro) e calcula o preço de cada bilhete:
      - menos de 12 anos: 5.00 euros (criança);
      - 65 anos ou mais: 7.00 euros (sénior);
      - os outros: 10.00 euros (adulto).
    No fim escreve o total a pagar e quantos bilhetes há de cada tipo.

        Quantos bilhetes? 4                   Quantos bilhetes? 0
        Idade 1: 8                            Número inválido
        Idade 2: 30
        Idade 3: 70
        Idade 4: 45
        Total: 32.00 euros
        Crianças: 1, Adultos: 2, Seniores: 1

    Programa bem organizado, com várias funções, comentário inicial e
    precondição quando for necessária. Use constantes para os valores fixos.
"""

# A tua resposta (programa completo):






# %%
"""
Ex 26.3 [DESAFIO] Estatísticas à escolha ----------------------------------

    O programa pergunta quantos números inteiros há (se não for positivo:
    "Número inválido") e lê-os. Depois pergunta a operação:
      - 's': escreve a soma;
      - 'm': escreve o máximo e a posição onde aparece pela 1ª vez (de 1 a N);
      - 'p': escreve quantos são pares;
      - outra resposta: escreve "Operação inválida".
    Não use sum nem max.

        Quantos números? 5           Quantos números? 3
        Número 1: 4                  Número 1: 2
        Número 2: 9                  Número 2: 6
        Número 3: -2                 Número 3: 1
        Número 4: 9                  Operação (s/m/p)? x
        Número 5: 6                  Operação inválida
        Operação (s/m/p)? m
        Máximo: 9 (posição 2)

    Programa bem organizado, com várias funções, comentário inicial e
    precondição quando for necessária.
"""

# A tua resposta (programa completo):






# %%
"""
Ex 26.4 [SOZINHA] Fatura do telemóvel -----------------------------------

    O programa pergunta os minutos de chamadas e o número de SMS do mês
    (inteiros). Se algum for negativo, escreve "Valores inválidos".
    Regras do tarifário:
      - mensalidade fixa: 10.00 euros, com 100 minutos incluídos;
      - cada minuto acima dos 100: 0.05 euros;
      - cada SMS: 0.02 euros;
      - ao total (mensalidade + extras) soma-se o IVA de 23%.

        Minutos: 160                  Minutos: 40
        SMS: 50                       SMS: -3
        Mensalidade: 10.00            Valores inválidos
        Extras: 4.00
        IVA: 3.22
        Total: 17.22

    Sem solução: confirma tu com os exemplos (faz as contas à mão primeiro).
"""

# A tua resposta (programa completo):






# %%
"""
===========================================================================
RESUMO: O QUE JÁ SABES
===========================================================================

  OS 4 PASSOS
  [ ] 1. perceber (entra, sai, regras, inválidos) e escrever em português:
         um nome de função por cada passo "de pensar"
  [ ] 2. o esqueleto (cabeçalhos + main) já corre
  [ ] 3. implementar e testar UMA função de cada vez
  [ ] 4. testar tudo junto, com todos os exemplos, e passar a checklist
  [ ] os testes ficam numa função de testes, e a chamada comenta-se no fim
  [ ] main: ler -> validar -> chamar -> escrever; main() no fim
  [ ] as funções devolvem dados; as mensagens são escritas pela main
  [ ] os números fixos do enunciado são constantes

  CÓDIGO QUE SE LÊ
  [ ] nomes que dizem o que é ou o que faz; booleanos como pergunta
  [ ] minúsculas_com_underscore; CONSTANTES em maiúsculas
  [ ] uma função, uma tarefa; efeitos laterais só quando são a tarefa
  [ ] o mesmo código em dois sítios -> uma função (refactoring)

  O FORMATO
  [ ] copiar as casas decimais do exemplo (45.0, 45.00, 2.055556)
  [ ] os textos à letra: acentos, espaços, ":" ou "="
  [ ] vírgulas no print põem um espaço; o f-string põe só o que escreves
  [ ] a ordem das perguntas, também nos casos inválidos

  OS PADRÕES
  [ ] menu: if/elif para cada resposta; "qualquer outra" é o else (ou nada)
  [ ] terminar mais cedo: if/else, ou print + return dentro da main
  [ ] tabela: for i in range(linhas), valor = início + i
  [ ] funções que só escrevem (tabelas) ou só leem (listas) são de "conversa"
  [ ] ler N valores: função de leitura -> lista -> funções de contas
  [ ] lista só se for preciso voltar a ver os valores; senão, tratar logo

  Se alguma linha ainda não te parece clara, volta à secção dela.

  PRÓXIMO: os testes de exemplo (revisoes_6_teste_exemplo_1.py).
  Sempre que escreveres um programa completo, passa-o pela checklist do Ex 21.6.
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
# Solução do Ex 23.4
#
# Saem diferentes as linhas 1 e 3. A linha 2 está certa.
#
# Linha 1: escreve "Media: 12.5". São 2 diferenças:
#   - falta o acento: "Media" em vez de "Média";
#   - round(12.5, 2) é o número 12.5, e escreve-se "12.5", não "12.50".
#     Para ter sempre 2 casas: f"Média: {avg:.2f}".
#
# Linha 2: "Aprovados: 3 de 4". Igual ao pedido: as vírgulas põem os espaços certos.
#
# Linha 3: escreve "Melhor: 18.0", mas o pedido era "Melhor nota: 18.0".
#
# Corrigido:

avg = 12.5
count = 3
n = 4
best = 18.0

print(f"Média: {avg:.2f}")              # Média: 12.50
print("Aprovados:", count, "de", n)     # Aprovados: 3 de 4
print(f"Melhor nota: {best}")           # Melhor nota: 18.0






# %%
# Solução do Ex 26.1 (Teste 1 2025/26, pergunta 6)

LINES = 10          # número de correspondências da tabela

def celsius_to_fahrenheit(c: float) -> float:
    """ Convert Celsius to Fahrenheit. """
    return 1.8 * c + 32

def fahrenheit_to_celsius(f: float) -> float:
    """ Convert Fahrenheit to Celsius. """
    return (f - 32) / 1.8

def print_c_to_f(start: float) -> None:
    """ Print a Celsius -> Fahrenheit table with LINES lines, from start. """
    print("Celsius ===> Fahrenheit")
    for i in range(LINES):
        c = start + i
        print(f"{c:.6f} ===> {celsius_to_fahrenheit(c):.6f}")

def print_f_to_c(start: float) -> None:
    """ Print a Fahrenheit -> Celsius table with LINES lines, from start. """
    print("Fahrenheit ===> Celsius")
    for i in range(LINES):
        f = start + i
        print(f"{f:.6f} ===> {fahrenheit_to_celsius(f):.6f}")

def main() -> None:
    direction = input("Conversão: c->f ou f->c? ")
    start = float(input("Valor de partida? "))
    if direction == "c":
        print_c_to_f(start)
    elif direction == "f":
        print_f_to_c(start)

main()

# Os passos, em português:
#   "ler o sentido e o valor de partida"     -> main
#   "converter uma temperatura"              -> celsius_to_fahrenheit / fahrenheit_to_celsius
#   "escrever a tabela de 10 linhas"         -> print_c_to_f / print_f_to_c
#   "se o sentido não é c nem f: nada"       -> main (sem else)
# Passo 3: as conversões testam-se sozinhas:
#   celsius_to_fahrenheit(100) -> 212.0      fahrenheit_to_celsius(212) -> 100.0
#
# A inversa: F = 1.8 * C + 32  ->  F - 32 = 1.8 * C  ->  C = (F - 32) / 1.8
#
# "Qualquer outra resposta fará o programa não escrever qualquer tabela":
# não há else. O programa simplesmente termina.
# E o valor de partida é perguntado SEMPRE: o enunciado diz "depois o
# programa pergunta qual o valor inicial", sem exceções.
# (é uma interpretação; se tivesses decidido não perguntar no caso inválido,
#  escrevias a decisão num comentário)
#
# É o Ex 25.1 (tabela) + o Ex 24.1 (menu) juntos.
# Constantes: o 10 virou LINES. O 1.8 e o 32 são a própria fórmula:
# podiam ser constantes, mas aqui lê-se bem assim.






# %%
# Solução do Ex 26.2

CHILD_AGE = 12
SENIOR_AGE = 65
CHILD_PRICE = 5.00
ADULT_PRICE = 10.00
SENIOR_PRICE = 7.00

def ticket_price(age: int) -> float:
    """ Price of a ticket for a person with the given age.
        Precondition: age >= 0
    """
    if age < CHILD_AGE:
        return CHILD_PRICE
    if age >= SENIOR_AGE:
        return SENIOR_PRICE
    return ADULT_PRICE

def read_ages(n: int) -> list[int]:
    """ Read the ages of n people.
        Precondition: n > 0
    """
    ages = []
    for i in range(n):
        ages.append(int(input(f"Idade {i + 1}: ")))
    return ages

def total_price(ages: list[int]) -> float:
    """ Total price of the tickets for these ages. """
    total = 0
    for age in ages:
        total += ticket_price(age)
    return total

def count_ages(ages: list[int], low: int, high: int) -> int:
    """ How many ages are in [low, high[. """
    count = 0
    for age in ages:
        if low <= age < high:
            count += 1
    return count

def main() -> None:
    n = int(input("Quantos bilhetes? "))
    if not (n > 0):
        print("Número inválido")
    else:
        ages = read_ages(n)
        children = count_ages(ages, 0, CHILD_AGE)
        adults = count_ages(ages, CHILD_AGE, SENIOR_AGE)
        seniors = len(ages) - children - adults
        print(f"Total: {total_price(ages):.2f} euros")
        print(f"Crianças: {children}, Adultos: {adults}, Seniores: {seniors}")

main()

# Os passos, em português:
#   "ler quantos bilhetes; é válido?"        -> main
#   "ler as idades"                          -> read_ages(n)
#   "quanto custa UM bilhete?"               -> ticket_price(age)
#   "o total"                                -> total_price(ages)
#   "quantos de cada tipo?"                  -> count_ages(ages, low, high)
#   "escrever"                               -> main
# Passo 3: ticket_price(8) -> 5.0   ticket_price(65) -> 7.0   ticket_price(30) -> 10.0
#          total_price([8, 30]) -> 15.0   count_ages([8, 30, 70], 0, 12) -> 1
#
# O preço de UM bilhete é uma função com if (como no ficheiro 3).
# O total e as contagens são ciclos sobre a lista (ficheiro 4, secção 14).
#
# Os seniores não precisam de outro ciclo: são os que sobram.
#
# Também dava sem lista, com 4 acumuladores no ciclo de leitura
# (total, crianças, adultos, seniores). Com a lista, cada função faz uma coisa
# só, e a main fica curta. As duas são aceites.






# %%
# Solução do Ex 26.3

def read_numbers(n: int) -> list[int]:
    """ Read n integers from the user.
        Precondition: n > 0
    """
    numbers = []
    for i in range(n):
        numbers.append(int(input(f"Número {i + 1}: ")))
    return numbers

def summation(l: list[int]) -> int:
    """ Sum of the values of l. """
    total = 0
    for v in l:
        total += v
    return total

def position_of_max(l: list[int]) -> int:
    """ Position of the first largest value.
        Precondition: len(l) > 0
    """
    pos = 0
    for i in range(1, len(l)):
        if l[i] > l[pos]:
            pos = i
    return pos

def count_even(l: list[int]) -> int:
    """ How many values of l are even. """
    count = 0
    for v in l:
        if v % 2 == 0:
            count += 1
    return count

def main() -> None:
    n = int(input("Quantos números? "))
    if not (n > 0):
        print("Número inválido")
        return
    numbers = read_numbers(n)
    option = input("Operação (s/m/p)? ")
    if option == "s":
        print(f"Soma: {summation(numbers)}")
    elif option == "m":
        pos = position_of_max(numbers)
        print(f"Máximo: {numbers[pos]} (posição {pos + 1})")
    elif option == "p":
        print(f"Pares: {count_even(numbers)}")
    else:
        print("Operação inválida")

main()

# Os passos, em português:
#   "ler N; é válido?"                       -> main
#   "ler os N números"                       -> read_numbers(n)
#   "ler a operação e fazer a escolhida"     -> main (if/elif/else)
#   "a soma" / "onde está o máximo" / "quantos pares"
#                                            -> summation / position_of_max / count_even
# Passo 3: as 3 funções de contas testam-se com uma lista feita à mão:
#   summation([4, 9, -2]) -> 11   position_of_max([4, 9, -2, 9]) -> 1   count_even([4, 9, -2]) -> 2
#
# Aqui usei a forma 2 do Ex 24.2 (print + return), para o menu não ficar
# todo dentro de um else. Com if/else também está certo.
#
# Os números são lidos ANTES da operação (é a ordem do exemplo),
# mesmo que a operação venha a ser inválida.
#
# Máximo: o 9 aparece nas posições 2 e 4; com ">" fica o primeiro (posição 2).