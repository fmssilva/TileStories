# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_8_while.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as respostas, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Os programas com input são corridos com os inputs dos enunciados,
e o output (com as perguntas e as respostas) é comparado à letra.
Os ciclos "infinitos" do Ex 30.3 são testados com um limite de voltas.
"""






# %%
import builtins, contextlib, io, math

def transcript(fn, inputs: list[str], *args) -> str:
    """ Run fn(*args) with simulated inputs; return what the console shows. """
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
            fn(*args)
    finally:
        builtins.input = old
    assert not queue, f"inputs não usados: {queue}"
    return buf.getvalue()

def output(fn, *args) -> str:
    """ What fn(*args) prints. """
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        fn(*args)
    return buf.getvalue()

def lines(text: str) -> str:
    """ Example text from an enunciado, without the indentation. """
    return "\n".join(l.strip() for l in text.strip("\n").splitlines()) + "\n"






# %%
# ---- Secção 27 ----------------------------------------------------------

# Ex 27.2
def sum_to_n_while(n: int) -> int:
    total = 0
    i = 1
    while i <= n:
        total += i
        i += 1
    return total

assert sum_to_n_while(5) == 15 and sum_to_n_while(0) == 0
assert list(range(0, 10, 2)) == [0, 2, 4, 6, 8]

# Ex 27.5
def print_squares_below(limit: int) -> None:
    i = 1
    while i * i < limit:
        print(i * i, end=" ")
        i += 1
    print()

assert output(print_squares_below, 50) == "1 4 9 16 25 36 49 \n"
assert output(print_squares_below, 1) == "\n"

# Ex 27.6
def smallest_n(limit: int) -> int:
    n = 0
    total = 0
    while total < limit:
        n += 1
        total += n
    return n

assert [smallest_n(x) for x in (10, 100, 0, 1, 6, 7)] == [4, 14, 0, 1, 3, 4]

# Ex 27.6, passo 3: a versão com o avanço no sítio errado devolve um a mais
def smallest_n_wrong(limit: int) -> int:
    n = 0
    total = 0
    while total < limit:
        total += n
        n += 1
    return n

assert smallest_n_wrong(10) == 5

def squares_small(limit: int) -> None:
    i = 1
    while i * i < limit:
        print(i * i, end=" ")
        i += 1

assert output(squares_small, 10) == "1 4 9 " and output(squares_small, 1) == ""

# Ex 27.3
def ex_27_4_ab() -> None:
    i = 10
    while i >= 1:
        print(i, end=" ")
        i -= 1
    print()
    i = 0
    while i <= 30:
        print(i, end=" ")
        i += 3
    print()

assert output(ex_27_4_ab) == "10 9 8 7 6 5 4 3 2 1 \n0 3 6 9 12 15 18 21 24 27 30 \n"

def sum_list_while(l: list[int]) -> int:
    total = 0
    i = 0
    while i < len(l):
        total += l[i]
        i += 1
    return total

def count_vowels_while(s: str) -> int:
    count = 0
    i = 0
    while i < len(s):
        if s[i] in "aeiou":
            count += 1
        i += 1
    return count

assert sum_list_while([4, 8, 15, 16]) == 43 and sum_list_while([]) == 0
assert count_vowels_while("programar") == 3 and count_vowels_while("") == 0

# Ex 27.4
def count_multiples_for(a: int, b: int, k: int) -> int:
    count = 0
    for i in range(a, b + 1):
        if i % k == 0:
            count += 1
    return count

def first_index_of_for(l: list[int], v: int) -> int:
    for i in range(len(l)):
        if l[i] == v:
            return i
    return -1

assert count_multiples_for(1, 20, 3) == 6 and count_multiples_for(0, 0, 5) == 1
assert first_index_of_for([5, 7, 9, 7], 7) == 1 and first_index_of_for([5, 7, 9, 7], 8) == -1

# Ex 27.8 c): a condição dupla não dá IndexError graças ao and
def sum_until_negative(l: list[int]) -> int:
    total = 0
    i = 0
    while i < len(l) and l[i] >= 0:
        total += l[i]
        i += 1
    return total

# Ex 27.7, caso 6: com n = 0 nunca fica ímpar (testado com limite de voltas)
def halve_until_odd_capped(n: int, cap: int) -> int:
    turns = 0
    while n % 2 == 0 and turns < cap:
        n //= 2
        turns += 1
    return turns

assert halve_until_odd_capped(0, 1000) == 1000 and halve_until_odd_capped(40, 1000) == 3

assert sum_until_negative([3, 4, -1, 10]) == 7 and sum_until_negative([1, 2]) == 3
print("Secção 27 OK")






# %%
# ---- Secção 28 ----------------------------------------------------------

def halvings(n: int) -> int:
    count = 0
    while n > 1:
        n //= 2
        count += 1
    return count

def first_power_of_2(n: int) -> int:
    power = 1
    while power < n:
        power *= 2
    return power

def reverse_number(n: int) -> int:
    result = 0
    while n > 0:
        result = result * 10 + n % 10
        n //= 10
    return result

def bounces(h: float, ratio: float, min_height: float) -> int:
    count = 0
    while h >= min_height:
        h *= ratio
        count += 1
    return count

def is_prime(n: int) -> bool:
    if n < 2:
        return False
    for d in range(2, n):
        if n % d == 0:
            return False
    return True

def next_prime(n: int) -> int:
    while not is_prime(n):
        n += 1
    return n

assert (halvings(1), halvings(8), halvings(100), halvings(0)) == (0, 3, 6, 0)
assert (first_power_of_2(1), first_power_of_2(5), first_power_of_2(64)) == (1, 8, 64)
assert (reverse_number(1234), reverse_number(7), reverse_number(1200), reverse_number(0)) == (4321, 7, 21, 0)
assert bounces(2.0, 0.6, 0.1) == 6 and bounces(2.0, 0.9, 0.1) == 29
heights = [2.0]
for _ in range(6):
    heights.append(heights[-1] * 0.6)
assert [round(h, 6) for h in heights] == [2.0, 1.2, 0.72, 0.432, 0.2592, 0.15552, 0.093312]
assert (next_prime(14), next_prime(17), next_prime(1)) == (17, 17, 2)
print("Secção 28 OK")






# %%
# ---- Secção 29 ----------------------------------------------------------

END_MARK = ""

def read_temperatures() -> list[float]:
    temps = []
    while True:
        s = input("Temperatura: ")
        if s == END_MARK:
            break
        temps.append(float(s))
    return temps

def maximum(l: list[float]) -> float:
    best = l[0]
    for v in l:
        if v > best:
            best = v
    return best

def main_29_1() -> None:
    temps = read_temperatures()
    if len(temps) == 0:
        print("Não há temperaturas")
    else:
        print(f"Máxima: {maximum(temps)}")

assert transcript(main_29_1, ["18.5", "22", "19.0", ""]) == (
    "Temperatura: 18.5\nTemperatura: 22\nTemperatura: 19.0\nTemperatura: \nMáxima: 22.0\n")
assert transcript(main_29_1, [""]) == "Temperatura: \nNão há temperaturas\n"

def read_age() -> int:
    while True:
        age = int(input("Idade: "))
        if 0 <= age <= 120:
            return age

def main_29_2() -> None:
    print(f"Tem {read_age()} anos.")

assert transcript(main_29_2, ["150", "-3", "25"]) == lines("""
    Idade: 150
    Idade: -3
    Idade: 25
    Tem 25 anos.""")

def read_positive(prompt: str) -> int:
    while True:
        n = int(input(prompt))
        if n > 0:
            return n
        print("Tem de ser positivo")

def main_29_3() -> None:
    width = read_positive("Largura: ")
    height = read_positive("Altura: ")
    print(f"Área: {width * height}")

assert transcript(main_29_3, ["0", "4", "-2", "3"]) == lines("""
    Largura: 0
    Tem de ser positivo
    Largura: 4
    Altura: -2
    Tem de ser positivo
    Altura: 3
    Área: 12""")

def read_int_list() -> list[int]:
    l = []
    while True:
        s = input(": ")
        if s == END_MARK:
            break
        l.append(int(s))
    return l

def main_29_4() -> None:
    l = read_int_list()
    if len(l) == 0:
        print("Lista vazia")
    else:
        evens = 0
        total = 0
        for v in l:
            total += v
            if v % 2 == 0:
                evens += 1
        print(f"Pares: {evens}")
        print(f"Soma: {total}")

assert transcript(main_29_4, ["4", "7", "10", ""]) == ": 4\n: 7\n: 10\n: \nPares: 2\nSoma: 21\n"
assert transcript(main_29_4, [""]) == ": \nLista vazia\n"

def read_two() -> tuple[int, int]:
    a = int(input("a: "))
    b = int(input("b: "))
    return (a, b)

def run() -> None:
    while True:
        cmd = input("Comando (s, m, q)? ")
        if cmd == "s":
            a, b = read_two()
            print(f"{a} + {b} = {a + b}")
        elif cmd == "m":
            a, b = read_two()
            print(f"{a} * {b} = {a * b}")
        elif cmd == "q":
            print("Adeus!")
            break
        else:
            print("Comando inválido")

assert transcript(run, ["s", "3", "4", "x", "m", "5", "6", "q"]) == lines("""
    Comando (s, m, q)? s
    a: 3
    b: 4
    3 + 4 = 7
    Comando (s, m, q)? x
    Comando inválido
    Comando (s, m, q)? m
    a: 5
    b: 6
    5 * 6 = 30
    Comando (s, m, q)? q
    Adeus!""")
print("Secção 29 OK")






# %%
# ---- Secção 30 ----------------------------------------------------------

def mystery(n: int) -> int:
    a = 0
    b = 1
    while b < n:
        a, b = b, a + b
    return a

assert mystery(20) == 13 and mystery(21) == 13 and mystery(22) == 21

# Ex 30.2
def ex_30_2() -> None:
    x = 100
    count = 0
    while x > 10:
        x = x // 3
        count += 1
    print(x, count)
    i = 1
    s = ""
    while i <= 16:
        s += str(i) + " "
        i *= 2
    print(s)
    n = 5
    while n != 0:
        if n % 2 == 1:
            n -= 1
        else:
            n //= 2
        print(n, end=" ")
    print()

assert output(ex_30_2) == "3 3\n1 2 4 8 16 \n4 2 1 0 \n"

# Ex 30.3: com limite de voltas, para provar quais não acabam
LIMIT = 10_000

def capped_b() -> bool:
    i, n = 1, 0
    while i != 10 and n < LIMIT:
        i += 2; n += 1
    return n == LIMIT

def capped_c() -> bool:
    i, n = 10, 0
    while i > 0 and n < LIMIT:
        i += 1; n += 1
    return n == LIMIT

def capped_d() -> bool:
    x, n = 0.0, 0
    while x != 1.0 and n < LIMIT:
        x += 0.1; n += 1
    return n == LIMIT

def loop_a() -> int:
    i = 0
    while i < 10:
        i += 2
    return i

def loop_e(n: int) -> int:
    while n > 0:
        n -= 3
    return n

assert loop_a() == 10
assert capped_b() and capped_c() and capped_d()       # os três não acabam
x = 0.0
for _ in range(10):
    x += 0.1
assert x == 0.9999999999999999
x, turns = 0.0, 0
while x < 1.0:
    x += 0.1; turns += 1
assert turns == 11
assert loop_e(10) == -2 and loop_e(9) == 0
assert 0.1 + 0.2 != 0.3

# Ex 30.4
def sum_to(n: int) -> int:
    total = 0
    i = 1
    while i <= n:
        total += i
        i += 1
    return total

def sum_to_bug(n: int) -> int:
    total = 0
    i = 1
    while i < n:
        total += i
        i += 1
    return total

def count_even(l: list[int]) -> int:
    count = 0
    i = 0
    while i < len(l):
        if l[i] % 2 == 0:
            count += 1
        i += 1
    return count

def print_row_sums(m: list[list[int]]) -> None:
    i = 0
    while i < len(m):
        total = 0
        j = 0
        while j < len(m[i]):
            total += m[i][j]
            j += 1
        print(total)
        i += 1

def print_row_sums_bug(m: list[list[int]]) -> None:
    i = 0
    total = 0
    while i < len(m):
        j = 0
        while j < len(m[i]):
            total += m[i][j]
            j += 1
        print(total)
        i += 1

assert sum_to_bug(4) == 6 and sum_to(4) == 10
assert count_even([2, 4, 5, 6]) == 3
assert output(print_row_sums_bug, [[1, 2, 3], [4, 5, 6]]) == "6\n21\n"
assert output(print_row_sums, [[1, 2, 3], [4, 5, 6]]) == "6\n15\n"
print("Secção 30 OK")






# %%
# ---- Desafios ------------------------------------------------------------

def play(secret: int, max_attempts: int) -> None:
    attempts = 0
    won = False
    while attempts < max_attempts and not won:
        guess = int(input("Tentativa: "))
        attempts += 1
        if guess < secret:
            print("O número é maior")
        elif guess > secret:
            print("O número é menor")
        else:
            won = True
    if won:
        print(f"Acertou em {attempts} tentativas!")
    else:
        print(f"Perdeu! O número era {secret}.")

assert transcript(play, ["50", "40", "42"], 42, 3) == lines("""
    Tentativa: 50
    O número é menor
    Tentativa: 40
    O número é maior
    Tentativa: 42
    Acertou em 3 tentativas!""")
assert transcript(play, ["10", "20"], 42, 2) == lines("""
    Tentativa: 10
    O número é maior
    Tentativa: 20
    O número é maior
    Perdeu! O número era 42.""")
assert transcript(play, ["42"], 42, 1) == "Tentativa: 42\nAcertou em 1 tentativas!\n"

def sum_digits(n: int) -> int:
    total = 0
    while n > 0:
        total += n % 10
        n //= 10
    return total

def digital_root(n: int) -> int:
    while n >= 10:
        n = sum_digits(n)
    return n

assert (digital_root(9875), digital_root(7), digital_root(0)) == (2, 7, 0)
assert sum_digits(9875) == 29 and sum_digits(29) == 11

PRECISION = 1e-12

def my_sqrt(x: float) -> float:
    lo = 0.0
    hi = x
    if x < 1:
        hi = 1.0
    while hi - lo > PRECISION:
        mid = (lo + hi) / 2
        if mid * mid < x:
            lo = mid
        else:
            hi = mid
    return (lo + hi) / 2

for v in (2.0, 9.0, 0.25, 0.0, 1.0, 1e6):
    assert math.isclose(my_sqrt(v), math.sqrt(v), abs_tol=1e-9), v

def zeno_terms(eps: float) -> int:
    total = 0.0
    term = 0.5
    count = 0
    while 1 - total >= eps:
        total += term
        term /= 2
        count += 1
    return count

assert (zeno_terms(0.1), zeno_terms(0.01), zeno_terms(0.5)) == (4, 7, 2)
assert 1/2 + 1/4 + 1/8 + 1/16 == 0.9375

# Ex 30.9 [SOZINHA] O multibanco
NOTES = [50, 20, 10, 5]

def withdraw(amount: int) -> None:
    for note in NOTES:
        count = 0
        while amount >= note:
            amount -= note
            count += 1
        print(f"{note}: {count}")

def withdraw_no_while(amount: int) -> None:
    for note in NOTES:
        print(f"{note}: {amount // note}")
        amount %= note

def main_30_9() -> None:
    withdraw(int(input("Quantia: ")))

assert transcript(main_30_9, ["185"]) == "Quantia: 185\n50: 3\n20: 1\n10: 1\n5: 1\n"
assert output(withdraw, 185) == output(withdraw_no_while, 185)
print("Desafios OK")