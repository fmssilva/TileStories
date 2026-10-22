# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_2_funcoes_e_escopo.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as soluções em código, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Cada solução tem asserts: se a célula correr sem erro, a solução está certa.
Os exercícios com input estão escritos como funções (mesma lógica, testável);
a versão com input está em comentário.
"""






# %%
# ---- Secção 5 -----------------------------------------------------------
import math

# Ex 5.8 [FAZ]
def average(a: float, b: float) -> float:
    """ Average of two reals. """
    return (a + b) / 2.0
assert average(1.5, 3.5) == 2.5

# Ex 5.9 [FAZ]
def triple(x: int) -> int:
    return 3 * x
def celsius_to_fahrenheit(c: float) -> float:
    return 1.8 * c + 32
def is_even(n: int) -> bool:
    return n % 2 == 0
assert triple(7) == 21 and celsius_to_fahrenheit(0) == 32.0
assert celsius_to_fahrenheit(100) == 212.0 and is_even(10) and not is_even(7)

# Ex 5.10 [FAZ]
def seconds_of(h: int, m: int, s: int) -> int:
    return h * 3600 + m * 60 + s
def hours_of(t: int) -> int:
    return t // 3600
def minutes_of(t: int) -> int:
    return t % 3600 // 60
def seconds_left(t: int) -> int:
    return t % 60
assert seconds_of(1, 2, 3) == 3723
assert (hours_of(3723), minutes_of(3723), seconds_left(3723)) == (1, 2, 3)

# Ex 5.11 [PENSA]
def edges_length(a: float, b: float, c: float) -> float:
    """ Total length of the 12 edges of a box. """
    return 4 * (a + b + c)
def surface_area(a: float, b: float, c: float) -> float:
    """ Area of the 6 faces of a box. """
    return 2 * (a * b + b * c + a * c)
def volume(a: float, b: float, c: float) -> float:
    """ Volume of a box. """
    return a * b * c
assert (edges_length(1, 2, 3), surface_area(1, 2, 3), volume(1, 2, 3)) == (24, 22, 6)

# Ex 5.12 [PENSA]
def circle_area(r: float) -> float:
    """ Area of a circle. Precondition: r >= 0 """
    return math.pi * r * r
def ring_area(r_out: float, r_in: float) -> float:
    """ Area between two concentric circles. Precondition: r_out >= r_in >= 0 """
    return circle_area(r_out) - circle_area(r_in)
assert math.isclose(ring_area(2, 1), 3 * math.pi)

# Ex 5.13 [DESAFIO]
def f(x: int) -> int:
    return x + 1
def g(x: int) -> int:
    return 2 * f(x)
def h(a: int, b: int) -> int:
    return g(b) - f(a)
assert (h(3, 5), f(g(f(0))), h(f(1), g(1))) == (8, 5, 7)

# Ex 5.14 [DESAFIO]  TypeError None + None; trocar print por return
def rectangle_area(w: float, h: float) -> float:
    return w * h
assert rectangle_area(2.0, 3.0) + rectangle_area(4.0, 5.0) == 26.0

# Ex 5.15 [DESAFIO]
def to_minutes(h: int, m: int) -> int:
    return h * 60 + m
def minutes_between(h1: int, m1: int, h2: int, m2: int) -> int:
    return to_minutes(h2, m2) - to_minutes(h1, m1)
assert minutes_between(9, 30, 11, 15) == 105 and minutes_between(8, 0, 8, 0) == 0

# Ex 5.16 [SOZINHA]
def first_digit(n: int) -> int:
    """ First digit of a 3-digit number. Precondition: 100 <= n <= 999 """
    return n // 100
def middle_digit(n: int) -> int:
    """ Middle digit of a 3-digit number. Precondition: 100 <= n <= 999 """
    return n // 10 % 10
def last_digit(n: int) -> int:
    """ Last digit of a 3-digit number. Precondition: 100 <= n <= 999 """
    return n % 10
def digit_sum(n: int) -> int:
    """ Sum of the digits. Precondition: 100 <= n <= 999 """
    return first_digit(n) + middle_digit(n) + last_digit(n)
def reverse(n: int) -> int:
    """ Digits in reverse order. Precondition: 100 <= n <= 999 """
    return last_digit(n) * 100 + middle_digit(n) * 10 + first_digit(n)
assert digit_sum(472) == 13 and reverse(472) == 274 and reverse(100) == 1
print("Secção 5 OK")






# %%
# ---- Secção 6 -----------------------------------------------------------

# Ex 6.7 [FAZ]   1 10
# Ex 6.8 [FAZ]   10 / 14

# Ex 6.9 [PENSA]
def add_points(score: int, p: int) -> int:
    """ New score after adding p points. """
    return score + p
score = 10
score = add_points(score, 5)
assert score == 15

# Ex 6.10 [DESAFIO]  "3 4 17" e "5"
def mystery(x: int) -> int:
    y = x * 2
    x = y + 1
    return x + y
assert mystery(4) == 17 and mystery(mystery(0)) == 5

# Ex 6.11 [SOZINHA]
def deposit(balance: int, amount: int) -> int:
    """ Balance after depositing amount. """
    return balance + amount
def withdraw(balance: int, amount: int) -> int:
    """ Balance after withdrawing amount. """
    return balance - amount
balance = 100
balance = deposit(balance, 50)
balance = withdraw(balance, 30)
assert balance == 120
print("Secção 6 OK")
