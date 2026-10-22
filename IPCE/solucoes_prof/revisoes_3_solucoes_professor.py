# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_3_condicoes_e_if.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as soluções em código, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Cada solução tem asserts: se a célula correr sem erro, a solução está certa.
Os programas com input estão escritos com a lógica em funções testáveis;
a main está em comentário quando pede input.
"""






# %%
# ---- Secção 7 -----------------------------------------------------------

# Ex 7.8 [FAZ]
def is_multiple(a: int, b: int) -> bool:
    return a % b == 0
def is_teenager(age: int) -> bool:
    return 13 <= age <= 19
def is_odd_and_positive(n: int) -> bool:
    return n % 2 == 1 and n > 0
assert (is_multiple(10, 5), is_multiple(10, 3)) == (True, False)
assert (is_teenager(13), is_teenager(19), is_teenager(20)) == (True, True, False)
assert (is_odd_and_positive(7), is_odd_and_positive(-7), is_odd_and_positive(4)) == (True, False, False)

# Ex 7.9 [FAZ]  a) F  b) T  c) T  d) F  e) F  f) T
a, b = 5, 0
assert (a > 3 and b > 3, a > 3 or b / 0 > 1, not a > 3 or b == 0,
        not (a > 3 or b == 0), b != 0 and a / b > 1, a == 5 and not b) \
    == (False, True, True, False, False, True)

# Ex 7.10 [FAZ]
def is_leap_year(year: int) -> bool:
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0
def is_valid_month(m: int) -> bool:
    return 1 <= m <= 12
assert [is_leap_year(y) for y in (2024, 2023, 1900, 2000)] == [True, False, False, True]
assert [is_valid_month(m) for m in (1, 12, 0, 13)] == [True, True, False, False]

# Ex 7.11 [PENSA]
def is_proper_triangle(a: float, b: float, c: float) -> bool:
    """ Check if a, b, c can be the sides of a triangle. """
    return a > 0 and b > 0 and c > 0 and a + b > c and a + c > b and b + c > a
assert is_proper_triangle(3, 4, 5) and not is_proper_triangle(1, 2, 3)
assert not is_proper_triangle(0, 1, 1)

# Ex 7.12 [DESAFIO]
def christmas(day: int, month: int) -> bool:
    return (month == 12 and day >= 23) or (month == 1 and day == 1)
assert (christmas(23, 12), christmas(1, 1), christmas(22, 12), christmas(10, 2)) \
    == (True, True, False, False)
assert christmas(31, 12) and not christmas(2, 1)

# Ex 7.13 [DESAFIO]
def is_root(r: float, a: float, b: float, c: float) -> bool:
    return abs(a * r * r + b * r + c) < 1e-9
assert not is_root(0.0, 1.0, -3.0, 2.0) and is_root(1.0, 1.0, -3.0, 2.0)
assert is_root(0.7, 1.0, -1.4, 0.49)

# Ex 7.14 [DESAFIO]  verificação exaustiva num pequeno domínio
for x in range(-2, 8):
    for y in range(-2, 5):
        assert (not (x > 0 and y > 0)) == (x <= 0 or y <= 0)
        assert (not (x <= 5 or y == 3)) == (x > 5 and y != 3)
        assert (not (x == y) and not (y == 3)) == (x != y and y != 3)
for age in range(0, 100):
    assert (not (age < 18 or age > 65)) == (18 <= age <= 65)

# Ex 7.15 [SOZINHA]
def xor(p: bool, q: bool) -> bool:
    """ True if exactly one of p, q is True. """
    return (p or q) and not (p and q)
def is_valid_time(h: int, m: int, s: int) -> bool:
    """ Check if h:m:s is a valid time of day. """
    return 0 <= h <= 23 and 0 <= m <= 59 and 0 <= s <= 59
assert [xor(p, q) for p in (False, True) for q in (False, True)] == [False, True, True, False]
assert is_valid_time(23, 59, 59) and not is_valid_time(24, 0, 0) and not is_valid_time(1, 60, 0)
print("Secção 7 OK")






# %%
# ---- Secção 8 -----------------------------------------------------------

# Ex 8.11 [FAZ]
def maximum(a: int, b: int) -> int:
    if a >= b:
        return a
    return b
def maximum3(a: int, b: int, c: int) -> int:
    return maximum(maximum(a, b), c)
assert (maximum(3, 8), maximum(8, 3), maximum(5, 5)) == (8, 8, 5)
assert (maximum3(1, 2, 3), maximum3(3, 2, 1), maximum3(2, 3, 1)) == (3, 3, 3)

# Ex 8.12 [FAZ]
def ticket_price(age: int) -> float:
    if age < 3:
        return 0.0
    elif age < 12:
        return 5.0
    elif age >= 65:
        return 6.0
    return 10.0
assert [ticket_price(a) for a in (2, 3, 11, 12, 64, 65)] == [0.0, 5.0, 5.0, 10.0, 10.0, 6.0]

# Ex 8.13 [FAZ]
def month_length(month: int, year: int) -> int:
    if month == 4 or month == 6 or month == 9 or month == 11:
        return 30
    elif month == 2:
        if is_leap_year(year):
            return 29
        return 28
    return 31
assert [month_length(m, 2023) for m in range(1, 13)] == [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
assert month_length(2, 2024) == 29

# Ex 8.14 [PENSA]
def triangle_kind(a: float, b: float, c: float) -> int:
    """ 0 invalid, 1 equilateral, 2 isosceles, 3 scalene. """
    if not is_proper_triangle(a, b, c):
        return 0
    elif a == b and b == c:
        return 1
    elif a == b or a == c or b == c:
        return 2
    return 3
assert [triangle_kind(*t) for t in ((1, 2, 3), (2, 2, 2), (2, 2, 3), (3, 4, 5))] == [0, 1, 2, 3]

# Ex 8.15 [PENSA]
def average(a: float, b: float) -> float:
    return (a + b) / 2
def passed(t1: float, t2: float, pr: float) -> bool:
    """ Check if the student passed. """
    return pr >= 9.5 and average(t1, t2) >= 9.5
def final_grade(t1: float, t2: float, pr: float) -> float:
    """ Final grade. Precondition: passed(t1, t2, pr) """
    return t1 * 0.4 + t2 * 0.4 + pr * 0.2
assert passed(10, 12, 15) and not passed(10, 12, 8) and not passed(5, 12, 15)
assert final_grade(10, 12, 15) == 11.8

# Ex 8.16 [DESAFIO]
def and_(a: bool, b: bool) -> bool:
    if a:
        return b
    return False
def or_(a: bool, b: bool) -> bool:
    if a:
        return True
    return b
def not_(a: bool) -> bool:
    if a:
        return False
    return True
for p in (False, True):
    assert not_(p) == (not p)
    for q in (False, True):
        assert and_(p, q) == (p and q) and or_(p, q) == (p or q)

# Ex 8.17 [DESAFIO]  a) I C I I    b) "B C A 7 A"
def mystery(n: int) -> str:
    if n % 3 == 0 and n % 5 == 0:
        return "A"
    elif n % 3 == 0:
        return "B"
    if n % 5 == 0:
        return "C"
    return str(n)
assert " ".join(mystery(n) for n in (9, 10, 15, 7, 30)) == "B C A 7 A"

# Ex 8.18 [DESAFIO]
def is_date_valid(day: int, month: int, year: int) -> bool:
    return 1 <= month <= 12 and 1 <= day <= month_length(month, year)
assert (is_date_valid(29, 2, 2024), is_date_valid(29, 2, 2023),
        is_date_valid(31, 4, 2024), is_date_valid(1, 13, 2024)) == (True, False, False, False)
assert not is_date_valid(0, 5, 2024)

# Ex 8.19 [SOZINHA]
def winner(p1: str, p2: str) -> int:
    """ 0 draw, 1 if player 1 wins, 2 if player 2 wins.
        Precondition: p1 and p2 are "pedra", "papel" or "tesoura"
    """
    if p1 == p2:
        return 0
    if ((p1 == "pedra" and p2 == "tesoura")
            or (p1 == "tesoura" and p2 == "papel")
            or (p1 == "papel" and p2 == "pedra")):
        return 1
    return 2
assert (winner("pedra", "tesoura"), winner("pedra", "papel"), winner("papel", "papel")) == (1, 2, 0)
assert (winner("tesoura", "papel"), winner("papel", "tesoura"), winner("papel", "pedra")) == (1, 2, 1)
print("Secção 8 OK")






# %%
# ---- Secção 9 -----------------------------------------------------------

# Ex 9.5 [FAZ]
def sum_to(n: int) -> int:
    if n == 0:
        return 0
    return n + sum_to(n - 1)
assert (sum_to(0), sum_to(4), sum_to(100)) == (0, 10, 5050)

# Ex 9.6 [FAZ]
def power(b: int, e: int) -> int:
    if e == 0:
        return 1
    return b * power(b, e - 1)
assert (power(2, 0), power(2, 10), power(3, 3)) == (1, 1024, 27)

# Ex 9.7 [PENSA]
def count_digits(n: int) -> int:
    """ Number of digits of n. Precondition: n >= 0 """
    if n < 10:
        return 1
    return 1 + count_digits(n // 10)
assert (count_digits(7), count_digits(472), count_digits(0), count_digits(10)) == (1, 3, 1, 2)

# Ex 9.8 [DESAFIO]  a) "5 3 1 1 3 5 "   b) 8
import io, contextlib
def f(n: int) -> None:
    if n > 0:
        print(n, end=" ")
        f(n - 2)
        print(n, end=" ")
def g(n: int) -> int:
    if n <= 1:
        return n
    return g(n - 1) + g(n - 2)
buf = io.StringIO()
with contextlib.redirect_stdout(buf):
    f(5)
assert buf.getvalue() == "5 3 1 1 3 5 " and g(6) == 8

# Ex 9.9 [DESAFIO]
def gcd(m: int, n: int) -> int:
    if m == n:
        return m
    elif m > n:
        return gcd(m - n, n)
    return gcd(m, n - m)
assert (gcd(252, 105), gcd(123, 456), gcd(7, 7)) == (21, 3, 7)

# Ex 9.10 [SOZINHA]
def digit_sum(n: int) -> int:
    """ Sum of the digits of n. Precondition: n >= 0 """
    if n < 10:
        return n
    return n % 10 + digit_sum(n // 10)
def multiply(a: int, b: int) -> int:
    """ a * b using only sums. Precondition: b >= 0 """
    if b == 0:
        return 0
    return a + multiply(a, b - 1)
assert (digit_sum(472), digit_sum(5), digit_sum(0)) == (13, 5, 0)
assert (multiply(7, 3), multiply(7, 0), multiply(-2, 4)) == (21, 0, -8)
print("Secção 9 OK")






# %%
# ---- Secção 10 ----------------------------------------------------------

# Ex 10.3 [FAZ]
#   def main() -> None:
#       a = int(input("A: ")); b = int(input("B: ")); print(maximum(a, b))

# Ex 10.4 [FAZ]
def grades_output(t1: float, t2: float, pr: float) -> str:
    if passed(t1, t2, pr):
        return str(final_grade(t1, t2, pr))
    return "REPROVADO"
assert grades_output(10, 12, 15) == "11.8" and grades_output(10, 12, 8) == "REPROVADO"

# Ex 10.5 [PENSA]
#   def main() -> None:
#       a = float(input("A: ")); b = float(input("B: ")); c = float(input("C: "))
#       print(triangle_kind(a, b, c))

# Ex 10.6 [DESAFIO] (Teste 1 2024/25, 6)
def cost(units: int) -> float:
    if units < 200:
        return units * 0.12
    elif units < 400:
        return units * 0.15
    elif units < 600:
        return units * 0.18
    return units * 0.20
def surcharge(c: float) -> float:
    if c > 45.0:
        return c * 0.15
    return 0.0
def total(c: float) -> float:
    t = c + surcharge(c)
    if t < 10.0:
        return 10.0
    return t
def bill_output(units: int) -> str:
    c = cost(units)
    return f"Custo = {c}\nSobretaxa = {surcharge(c)}\nTotal = {total(c)}"
assert bill_output(800) == "Custo = 160.0\nSobretaxa = 24.0\nTotal = 184.0"
assert bill_output(300) == "Custo = 45.0\nSobretaxa = 0.0\nTotal = 45.0"
assert bill_output(50) == "Custo = 6.0\nSobretaxa = 0.0\nTotal = 10.0"

# Ex 10.7 [SOZINHA]
def calculate(a: float, op: str, b: float) -> float:
    """ a op b.
        Precondition: op is one of + - * / and (op != "/" or b != 0)
    """
    if op == "+":
        return a + b
    elif op == "-":
        return a - b
    elif op == "*":
        return a * b
    return a / b
def is_valid_op(op: str) -> bool:
    return op == "+" or op == "-" or op == "*" or op == "/"
def calculator_output(a: float, op: str, b: float) -> str:
    if not is_valid_op(op):
        return "Operação inválida"
    elif op == "/" and b == 0:
        return "Divisão por zero"
    return f"Resultado: {calculate(a, op, b)}"
assert calculator_output(7, "/", 2) == "Resultado: 3.5"
assert calculator_output(7, "/", 0) == "Divisão por zero"
assert calculator_output(7, "%", 2) == "Operação inválida"
#   def main() -> None:
#       a = float(input("A: ")); op = input("Operação: "); b = float(input("B: "))
#       print(calculator_output(a, op, b))     (ou os ifs diretamente na main)
print("Secção 10 OK")
