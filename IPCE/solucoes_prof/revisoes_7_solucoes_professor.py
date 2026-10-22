# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_7_programa_completo.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as respostas, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Cada programa é corrido com os inputs dos exemplos do enunciado,
e o output (com as perguntas e as respostas) é comparado à letra.
"""






# %%
import builtins, contextlib, io, math

def transcript(main, inputs: list[str]) -> str:
    """ Run main() with simulated inputs; return what the console shows. """
    queue = list(inputs)
    buf = io.StringIO()
    def fake_input(prompt: str = "") -> str:
        value = queue.pop(0)
        print(prompt + value)
        return value
    old = builtins.input
    builtins.input = fake_input
    try:
        with contextlib.redirect_stdout(buf):
            main()
    finally:
        builtins.input = old
    assert not queue, f"inputs não usados: {queue}"
    return buf.getvalue()

def lines(text: str) -> str:
    """ Example text from an enunciado, without the indentation. """
    return "\n".join(l.strip() for l in text.strip("\n").splitlines()) + "\n"






# %%
# ---- Secção 21 ----------------------------------------------------------

# Ex 21.2 / 21.3 / 21.4 [EXEMPLO] Registar utilizador
def is_valid_email(email: str) -> bool:
    return "@" in email and "." in email

def email_exists(email: str, emails: list[str]) -> bool:
    return email in emails

def register_user(email: str, emails: list[str]) -> None:
    emails.append(email)

def main_21_2() -> None:
    emails = ["ana@mail.pt", "rui@mail.pt"]
    name = input("Nome: ")
    email = input("Email: ")
    if not is_valid_email(email):
        print("Email inválido")
    elif email_exists(email, emails):
        print("Esse email já está registado")
    else:
        register_user(email, emails)
        print(f"Utilizador {name} registado")

assert transcript(main_21_2, ["Eva", "eva@mail.pt"]).endswith("Utilizador Eva registado\n")
assert transcript(main_21_2, ["Rui", "rui@mail.pt"]).endswith("Esse email já está registado\n")
assert transcript(main_21_2, ["Eva", "eva.mail.pt"]).endswith("Email inválido\n")
emails = ["ana@mail.pt"]; register_user("eva@mail.pt", emails)
assert emails == ["ana@mail.pt", "eva@mail.pt"]


# Ex 21.5 [EXEMPLO] Portes de envio (valores do passo 3 e exemplos)
def base_cost(w: float) -> float:
    if w <= 1:
        return 3.50
    if w <= 5:
        return 6.00
    return 6.00 + 1.20 * (w - 5)
def total_cost(w: float, urgent: bool) -> float:
    c = base_cost(w)
    if urgent:
        c = c * 1.5
    return c
assert (base_cost(0.5), base_cost(5), base_cost(7)) == (3.5, 6.0, 8.4)
assert total_cost(7, False) == 8.4 and total_cost(7, True) == 12.600000000000001
assert f"{total_cost(7, True):.2f}" == "12.60" and f"{total_cost(0.5, False):.2f}" == "3.50"

# Ex 21.7 [FAZ] Bilhete de comboio
PRICE_PER_KM = 0.10
MIN_PRICE = 2.00
CHILD_AGE = 12
SENIOR_AGE = 65
CHILD_DISCOUNT = 0.50
SENIOR_DISCOUNT = 0.40

def base_price(distance: float) -> float:
    """ Price before discounts. Precondition: distance > 0 """
    price = PRICE_PER_KM * distance
    if price < MIN_PRICE:
        return MIN_PRICE
    return price

def discount(age: int) -> float:
    """ Discount rate for a given age. Precondition: age >= 0 """
    if age < CHILD_AGE:
        return CHILD_DISCOUNT
    if age >= SENIOR_AGE:
        return SENIOR_DISCOUNT
    return 0.0

def ticket_price(age: int, distance: float) -> float:
    """ Final price. Precondition: age >= 0 and distance > 0 """
    return base_price(distance) * (1 - discount(age))

def main_21_7() -> None:
    age = int(input("Idade: "))
    distance = float(input("Distância (km): "))
    if not (age >= 0 and distance > 0):
        print("Dados inválidos")
    else:
        print(f"Preço: {ticket_price(age, distance):.2f} euros")

assert transcript(main_21_7, ["70", "150"]) == lines("""
    Idade: 70
    Distância (km): 150
    Preço: 9.00 euros""")
assert transcript(main_21_7, ["30", "12"]) == lines("""
    Idade: 30
    Distância (km): 12
    Preço: 2.00 euros""")
assert transcript(main_21_7, ["-1", "50"]) == lines("""
    Idade: -1
    Distância (km): 50
    Dados inválidos""")
assert base_price(150) == 15.0 and base_price(12) == 2.0
assert (discount(70), discount(30), discount(8)) == (0.4, 0.0, 0.5)
print("Secção 21 OK")






# %%
# ---- Secção 22 ----------------------------------------------------------

# Ex 22.4 [FAZ] IMC
UNDERWEIGHT_LIMIT, NORMAL_LIMIT, OVERWEIGHT_LIMIT = 18.5, 25, 30

def bmi(weight: float, height: float) -> float:
    return weight / (height * height)

def bmi_class(value: float) -> str:
    if value < UNDERWEIGHT_LIMIT:
        return "Magreza"
    if value < NORMAL_LIMIT:
        return "Normal"
    if value < OVERWEIGHT_LIMIT:
        return "Excesso de peso"
    return "Obesidade"

def main_22_4() -> None:
    weight = float(input("Peso (kg): "))
    height = float(input("Altura (m): "))
    value = bmi(weight, height)
    print(f"IMC: {value:.1f} ({bmi_class(value)})")

assert transcript(main_22_4, ["70", "1.75"]) == "Peso (kg): 70\nAltura (m): 1.75\nIMC: 22.9 (Normal)\n"
assert [bmi_class(v) for v in (18.4, 18.5, 24.9, 25, 29.9, 30)] == \
       ["Magreza", "Normal", "Normal", "Excesso de peso", "Excesso de peso", "Obesidade"]
print("Secção 22 OK")






# %%
# ---- Secção 23 ----------------------------------------------------------

# Ex 23.3 [FAZ]
name, number, grade, total, km, miles = "Rita", 3, 15.456, 7.0, 10, 6.2137119
assert f"Aluno {number}: {name}, nota {grade:.2f}" == "Aluno 3: Rita, nota 15.46"
assert f"Total = {total}" == "Total = 7.0"
assert f"{km} km = {miles:.2f} milhas" == "10 km = 6.21 milhas"
assert f"{name};{grade:.1f}" == "Rita;15.5"

# Ex 23.4 [DESAFIO]: linhas 1 e 3 erradas (acento + 12.5 em vez de 12.50; "Melhor" sem "nota")
avg = 12.5
buf = io.StringIO()
with contextlib.redirect_stdout(buf):
    print("Media:", round(avg, 2))
    print("Aprovados:", 3, "de", 4)
    print(f"Melhor: {18.0}")
assert buf.getvalue() == "Media: 12.5\nAprovados: 3 de 4\nMelhor: 18.0\n"
assert f"Média: {avg:.2f}" == "Média: 12.50"
print("Secção 23 OK")






# %%
# ---- Secção 24 ----------------------------------------------------------

# Ex 24.3 [FAZ]
def square_area(side: float) -> float:
    return side * side

def circle_area(radius: float) -> float:
    return math.pi * radius * radius

def triangle_area(base: float, height: float) -> float:
    return base * height / 2

def main_24_3() -> None:
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

assert transcript(main_24_3, ["c", "2"]) == lines("""
    Figura (q/c/t)? c
    Raio: 2
    Área: 12.57""")
assert transcript(main_24_3, ["t", "3", "5"]) == lines("""
    Figura (q/c/t)? t
    Base: 3
    Altura: 5
    Área: 7.50""")
assert transcript(main_24_3, ["x"]) == lines("""
    Figura (q/c/t)? x
    Figura inválida""")
print("Secção 24 OK")






# %%
# ---- Secção 25 ----------------------------------------------------------

# Ex 25.3 [FAZ]
DAYS = 7

def read_temperatures() -> list[float]:
    temps = []
    for i in range(DAYS):
        temps.append(float(input(f"Dia {i + 1}: ")))
    return temps

def average(l: list[float]) -> float:
    total = 0
    for v in l:
        total += v
    return total / len(l)

def position_of_max(l: list) -> int:
    pos = 0
    for i in range(1, len(l)):
        if l[i] > l[pos]:
            pos = i
    return pos

def count_above(l: list[float], limit: float) -> int:
    count = 0
    for v in l:
        if v > limit:
            count += 1
    return count

def main_25_3() -> None:
    temps = read_temperatures()
    avg = average(temps)
    pos = position_of_max(temps)
    print(f"Média: {avg:.1f}")
    print(f"Máxima: {temps[pos]} (dia {pos + 1})")
    print(f"Dias acima da média: {count_above(temps, avg)}")

assert transcript(main_25_3, ["14.5", "16", "18.5", "17", "12", "15.5", "18.5"]) == lines("""
    Dia 1: 14.5
    Dia 2: 16
    Dia 3: 18.5
    Dia 4: 17
    Dia 5: 12
    Dia 6: 15.5
    Dia 7: 18.5
    Média: 16.0
    Máxima: 18.5 (dia 3)
    Dias acima da média: 3""")

# Ex 25.4 [PENSA] Pauta
PASS_GRADE = 9.5

def is_approved(grade: float) -> bool:
    """ Check if a grade is a pass. """
    return grade >= PASS_GRADE

def main_25_4() -> None:
    n = int(input("Quantos alunos? "))
    if not (n > 0):
        print("Número inválido")
    else:
        approved = 0
        for i in range(1, n + 1):
            grade = float(input(f"Nota do aluno {i}: "))
            if is_approved(grade):
                print(f"Aluno {i}: aprovado")
                approved += 1
            else:
                print(f"Aluno {i}: reprovado")
        print(f"Aprovados: {approved} de {n}")

assert transcript(main_25_4, ["3", "12", "7.5", "9.5"]) == lines("""
    Quantos alunos? 3
    Nota do aluno 1: 12
    Aluno 1: aprovado
    Nota do aluno 2: 7.5
    Aluno 2: reprovado
    Nota do aluno 3: 9.5
    Aluno 3: aprovado
    Aprovados: 2 de 3""")
assert transcript(main_25_4, ["0"]) == "Quantos alunos? 0\nNúmero inválido\n"
print("Secção 25 OK")






# %%
# ---- Secção 26 ----------------------------------------------------------

# Ex 26.1 [DESAFIO] Celsius / Fahrenheit (Teste 1 2025/26, P6)
LINES = 10

def celsius_to_fahrenheit(c: float) -> float:
    return 1.8 * c + 32

def fahrenheit_to_celsius(f: float) -> float:
    return (f - 32) / 1.8

def print_c_to_f(start: float) -> None:
    print("Celsius ===> Fahrenheit")
    for i in range(LINES):
        c = start + i
        print(f"{c:.6f} ===> {celsius_to_fahrenheit(c):.6f}")

def print_f_to_c(start: float) -> None:
    print("Fahrenheit ===> Celsius")
    for i in range(LINES):
        f = start + i
        print(f"{f:.6f} ===> {fahrenheit_to_celsius(f):.6f}")

def main_26_1() -> None:
    direction = input("Conversão: c->f ou f->c? ")
    start = float(input("Valor de partida? "))
    if direction == "c":
        print_c_to_f(start)
    elif direction == "f":
        print_f_to_c(start)

assert transcript(main_26_1, ["f", "35.7"]) == lines("""
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
    44.700000 ===> 7.055556""")          # copiado do enunciado real
assert transcript(main_26_1, ["x", "3"]) == "Conversão: c->f ou f->c? x\nValor de partida? 3\n"

# Ex 26.2 [DESAFIO] Bilhetes
CHILD_PRICE, ADULT_PRICE, SENIOR_PRICE = 5.00, 10.00, 7.00

def ticket(age: int) -> float:
    if age < CHILD_AGE:
        return CHILD_PRICE
    if age >= SENIOR_AGE:
        return SENIOR_PRICE
    return ADULT_PRICE

def count_ages(ages: list[int], low: int, high: int) -> int:
    count = 0
    for age in ages:
        if low <= age < high:
            count += 1
    return count

def main_26_2() -> None:
    n = int(input("Quantos bilhetes? "))
    if not (n > 0):
        print("Número inválido")
    else:
        ages = []
        for i in range(n):
            ages.append(int(input(f"Idade {i + 1}: ")))
        total = 0
        for age in ages:
            total += ticket(age)
        children = count_ages(ages, 0, CHILD_AGE)
        adults = count_ages(ages, CHILD_AGE, SENIOR_AGE)
        seniors = len(ages) - children - adults
        print(f"Total: {total:.2f} euros")
        print(f"Crianças: {children}, Adultos: {adults}, Seniores: {seniors}")

assert transcript(main_26_2, ["4", "8", "30", "70", "45"]) == lines("""
    Quantos bilhetes? 4
    Idade 1: 8
    Idade 2: 30
    Idade 3: 70
    Idade 4: 45
    Total: 32.00 euros
    Crianças: 1, Adultos: 2, Seniores: 1""")
assert transcript(main_26_2, ["0"]) == "Quantos bilhetes? 0\nNúmero inválido\n"

# Ex 26.3 [DESAFIO] Estatísticas
def main_26_3() -> None:
    n = int(input("Quantos números? "))
    if not (n > 0):
        print("Número inválido")
        return
    numbers = []
    for i in range(n):
        numbers.append(int(input(f"Número {i + 1}: ")))
    option = input("Operação (s/m/p)? ")
    if option == "s":
        total = 0
        for v in numbers:
            total += v
        print(f"Soma: {total}")
    elif option == "m":
        pos = position_of_max(numbers)
        print(f"Máximo: {numbers[pos]} (posição {pos + 1})")
    elif option == "p":
        count = 0
        for v in numbers:
            if v % 2 == 0:
                count += 1
        print(f"Pares: {count}")
    else:
        print("Operação inválida")

assert transcript(main_26_3, ["5", "4", "9", "-2", "9", "6", "m"]) == lines("""
    Quantos números? 5
    Número 1: 4
    Número 2: 9
    Número 3: -2
    Número 4: 9
    Número 5: 6
    Operação (s/m/p)? m
    Máximo: 9 (posição 2)""")
assert transcript(main_26_3, ["3", "2", "6", "1", "x"]).endswith("Operação inválida\n")
assert transcript(main_26_3, ["3", "2", "6", "1", "s"]).endswith("Soma: 9\n")
assert transcript(main_26_3, ["3", "2", "6", "1", "p"]).endswith("Pares: 2\n")

# Ex 26.4 [SOZINHA] Fatura do telemóvel
MONTHLY_FEE = 10.00
FREE_MINUTES = 100
PRICE_PER_MINUTE = 0.05
PRICE_PER_SMS = 0.02
VAT_RATE = 0.23

def extras(minutes: int, sms: int) -> float:
    """ Cost of the extra minutes and of the SMS.
        Precondition: minutes >= 0 and sms >= 0
    """
    extra_minutes = minutes - FREE_MINUTES
    if extra_minutes < 0:
        extra_minutes = 0
    return extra_minutes * PRICE_PER_MINUTE + sms * PRICE_PER_SMS

def vat(amount: float) -> float:
    """ VAT over an amount. """
    return amount * VAT_RATE

def main_26_4() -> None:
    minutes = int(input("Minutos: "))
    sms = int(input("SMS: "))
    if not (minutes >= 0 and sms >= 0):
        print("Valores inválidos")
    else:
        extra = extras(minutes, sms)
        subtotal = MONTHLY_FEE + extra
        tax = vat(subtotal)
        print(f"Mensalidade: {MONTHLY_FEE:.2f}")
        print(f"Extras: {extra:.2f}")
        print(f"IVA: {tax:.2f}")
        print(f"Total: {subtotal + tax:.2f}")

assert transcript(main_26_4, ["160", "50"]) == lines("""
    Minutos: 160
    SMS: 50
    Mensalidade: 10.00
    Extras: 4.00
    IVA: 3.22
    Total: 17.22""")
assert transcript(main_26_4, ["40", "-3"]) == "Minutos: 40\nSMS: -3\nValores inválidos\n"
assert transcript(main_26_4, ["40", "0"]).endswith("Extras: 0.00\nIVA: 2.30\nTotal: 12.30\n")
print("Secção 26 OK")