# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_4_ciclos_for_e_listas.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as soluções em código, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Cada solução tem asserts: se a célula correr sem erro, a solução está certa.
"""






# %%
# ---- Secção 11 ----------------------------------------------------------
import io, contextlib

def captured(fn, *args) -> str:
    """ Output printed by fn(*args). """
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        fn(*args)
    return buf.getvalue()

# Ex 11.12 [FAZ]
def sum_naturals(n: int) -> int:
    total = 0
    for i in range(n):
        total += i
    return total
def sum_squares(n: int) -> int:
    total = 0
    for i in range(n):
        total += i * i
    return total
assert (sum_naturals(4), sum_naturals(0), sum_squares(4), sum_squares(1)) == (6, 0, 14, 0)

# Ex 11.13 [FAZ]
def count_divisors(n: int) -> int:
    count = 0
    for d in range(1, n + 1):
        if n % d == 0:
            count += 1
    return count
def is_prime(n: int) -> bool:
    return count_divisors(n) == 2
assert (count_divisors(12), count_divisors(1), count_divisors(7)) == (6, 1, 2)
assert [is_prime(n) for n in (7, 12, 1, 2)] == [True, False, False, True]

# Ex 11.14 [FAZ]
def draw_line(c: str, n: int) -> None:
    for _ in range(n):
        print(c, end="")
    print()
def draw_triangle(c: str, n: int) -> None:
    for i in range(1, n + 1):
        draw_line(c, i)
assert captured(draw_triangle, "*", 4) == "*\n**\n***\n****\n"
def draw_triangle_nested(c: str, n: int) -> None:      # Ex 11.10 (EXEMPLO)
    for i in range(1, n + 1):
        for _ in range(i):
            print(c, end="")
        print()
assert captured(draw_triangle_nested, "*", 4) == captured(draw_triangle, "*", 4)

# Ex 11.15 [FAZ]
def count_pairs(n: int, target: int) -> int:
    count = 0
    for i in range(1, n + 1):
        for j in range(1, n + 1):
            if i + j == target:
                count += 1
    return count
def count_pairs_fast(n: int, target: int) -> int:     # versão com um só ciclo
    count = 0
    for i in range(1, n + 1):
        if 1 <= target - i <= n:
            count += 1
    return count
assert (count_pairs(3, 4), count_pairs(3, 7), count_pairs(6, 7)) == (3, 0, 6)
assert all(count_pairs(n, t) == count_pairs_fast(n, t) for n in range(1, 8) for t in range(0, 17))

# Ex 11.16 [PENSA]
def is_perfect(n: int) -> bool:
    """ Check if n is equal to the sum of its proper divisors. Precondition: n > 0 """
    total = 0
    for d in range(1, n):
        if n % d == 0:
            total += d
    return n == total
assert is_perfect(6) and is_perfect(28) and not is_perfect(8)

# Ex 11.17 [PENSA]
#   def read_and_sum(n: int) -> int:
#       total = 0
#       for i in range(1, n + 1):
#           total += int(input(f"{i}> "))
#       return total
#   def main() -> None:
#       n = int(input("Introduza a quantidade de números a somar: "))
#       print(read_and_sum(n))

# Ex 11.18 [DESAFIO]
def sum_multiples(m: int, lim: int) -> int:
    total = 0
    for i in range(0, lim + 1, m):
        total += i
    return total
assert [sum_multiples(m, 10) for m in (10, 5, 2, 1)] == [10, 15, 30, 55]

# Ex 11.19 [DESAFIO]
def zeno(k: int) -> float:
    total, term = 0.0, 0.5
    for _ in range(k):
        total += term
        term /= 2
    return total
assert [zeno(k) for k in range(4)] == [0.0, 0.5, 0.75, 0.875]

# Ex 11.20 [DESAFIO]  D. 13

# Ex 11.21 [DESAFIO]  a) 10   b) "0 \n0 2 \n0 3 6 \n"
def block_b() -> None:
    for i in range(1, 4):
        for j in range(i):
            print(i * j, end=" ")
        print()
count = 0
for i in range(4):
    for j in range(i, 4):
        count += 1
assert count == 10 and captured(block_b) == "0 \n0 2 \n0 3 6 \n"

# Ex 11.22 [DESAFIO]
def draw_segment(x: str, n: int) -> None:
    for i in range(n):
        print(x, end='')
def draw_tree_line(spaces: int, x: str, count: int) -> None:
    draw_segment(' ', spaces)
    draw_segment(x, count)
    print()
def draw_pine_tree(a: str, b: str, c: str, n: int) -> None:
    draw_tree_line(n - 1, a, 1)
    for k in range(1, n):
        draw_tree_line(n - 1 - k, b, 2 * k + 1)
    for _ in range(3):
        draw_tree_line(n - 2, c, 3)
lines = captured(draw_pine_tree, '*', '^', '#', 10).split("\n")
assert lines[0] == " " * 9 + "*" and lines[1] == " " * 8 + "^^^"
assert lines[9] == "^" * 19 and lines[10] == lines[12] == " " * 8 + "###"

# Ex 11.23 [SOZINHA]
def ln2(n: int) -> float:
    """ Sum of the first n terms of 1 - 1/2 + 1/3 - ... """
    total = 0.0
    sign = 1
    for k in range(1, n + 1):
        total += sign / k
        sign = -sign
    return total
assert (ln2(0), ln2(1), ln2(2)) == (0.0, 1.0, 0.5) and abs(ln2(3) - 0.8333) < 1e-4

def draw_frame(c: str, n: int) -> None:
    """ Square frame of side n. Precondition: len(c) == 1 and n >= 2 """
    draw_line(c, n)
    for _ in range(n - 2):
        draw_segment(c, 1)
        draw_segment(" ", n - 2)
        draw_segment(c, 1)
        print()
    draw_line(c, n)
assert captured(draw_frame, "*", 4) == "****\n*  *\n*  *\n****\n"
print("Secção 11 OK")






# %%
# ---- Secção 12 ----------------------------------------------------------

# Ex 12.8 [FAZ]  a) 4  b) 60  c) [20, 20, 30, 40]  d) 5 20  e) 20 [20, 20, 30, 40]

# Ex 12.9 [FAZ]
def is_leap_year(year: int) -> bool:
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0
def month_length(month: int, year: int) -> int:
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    if month == 2 and is_leap_year(year):
        return 29
    return DURATIONS[month - 1]
assert (month_length(1, 2023), month_length(4, 2023), month_length(2, 2024),
        month_length(2, 2023), month_length(12, 2023)) == (31, 30, 29, 28, 31)

# Ex 12.10 [PENSA]
def swap_ends(l: list[int]) -> None:
    """ Swap the first and the last elements. Precondition: len(l) > 0 """
    l[0], l[-1] = l[-1], l[0]
l = [1, 2, 3, 4]; swap_ends(l); assert l == [4, 2, 3, 1]
l = [5]; swap_ends(l); assert l == [5]

# Ex 12.11 [DESAFIO]  [1, 2, 3, 0] / [1, 2, 3, 0] / [7, 2]

# Ex 12.12 [SOZINHA]
def rotate_left(l: list[int]) -> None:
    """ Move the first element to the end. Precondition: len(l) > 0 """
    l.append(l.pop(0))
l = [1, 2, 3, 4]; rotate_left(l); assert l == [2, 3, 4, 1]
print("Secção 12 OK")






# %%
# ---- Secção 13 ----------------------------------------------------------

# Ex 13.4 [FAZ]
def is_weekend(day: str) -> bool:
    return day in {"sábado", "domingo"}
def is_yes_or_no(answer: str) -> bool:
    return answer in {"s", "S", "n", "N"}
assert is_weekend("sábado") and not is_weekend("terça")
assert is_yes_or_no("S") and not is_yes_or_no("talvez") and not is_yes_or_no("")

# Ex 13.5 [DESAFIO]  T F F T F T F T
assert ("" in "abc", "ac" in "abc", 15 in range(0, 15, 5), 15 in range(0, 16, 5),
        [1] in [1, 2], 2 in {1: "a", 2: "b"}, "a" in {1: "a"}, "Abc" in "abcAbc") \
    == (True, False, False, True, False, True, False, True)

# Ex 13.6 [SOZINHA]
LETTERS = "abcdefghijklmnopqrstuvwxyz"
DIGITS = "0123456789"
VOWELS = "aeiou"
def char_kind(c: str) -> str:
    """ "vogal", "consoante", "dígito" or "outro". Precondition: len(c) == 1 """
    if c in VOWELS:
        return "vogal"
    elif c in LETTERS:
        return "consoante"
    elif c in DIGITS:
        return "dígito"
    return "outro"
assert [char_kind(c) for c in "ab7?"] == ["vogal", "consoante", "dígito", "outro"]
print("Secção 13 OK")






# %%
# ---- Secção 14 ----------------------------------------------------------

# Ex 14.8 [FAZ]
def count(l: list[int], x: int) -> int:
    c = 0
    for v in l:
        if v == x:
            c += 1
    return c
def average(l: list[float]) -> float:
    total = 0
    for v in l:
        total += v
    return total / len(l)
assert (count([1, 2, 1, 3, 1], 1), count([1, 2], 5), average([10, 12, 17])) == (3, 0, 13.0)

# Ex 14.9 [FAZ]
def evens(l: list[int]) -> list[int]:
    result = []
    for v in l:
        if v % 2 == 0:
            result.append(v)
    return result
def count_vowels(s: str) -> int:
    c = 0
    for ch in s:
        if ch in "aeiou":
            c += 1
    return c
assert evens([1, 2, 3, 4, 6]) == [2, 4, 6] and count_vowels("programar") == 3

# Ex 14.10 [FAZ]
def sum_even_values(l: list[int]) -> int:
    total = 0
    for v in l:
        if v % 2 == 0:
            total += v
    return total
def sum_values_at_even_positions(l: list[int]) -> int:
    total = 0
    for i in range(0, len(l), 2):
        total += l[i]
    return total
assert sum_even_values([5, 2, 7, 4, 1]) == 6 and sum_values_at_even_positions([5, 2, 7, 4, 1]) == 13
l38 = [34, 56, 1, -4, 3, 2, 7, 6, 66, 666, 77, 777, 978, -2, -2, 56, 7, -1, -2, 88]   # guião 38
assert sum_even_values(l38) == sum(v for v in l38 if v % 2 == 0)

# Ex 14.11 [PENSA]
def position_of_min(l: list[int]) -> int:
    """ Position of the (first) smallest element. Precondition: len(l) > 0 """
    pos = 0
    for i in range(1, len(l)):
        if l[i] < l[pos]:
            pos = i
    return pos
assert position_of_min([5, 2, 7, 2]) == 1 and position_of_min([3]) == 0

# Ex 14.12 [PENSA]
def reversed_copy(l: list[int]) -> list[int]:
    """ New list with the elements of l in reverse order. """
    result = []
    for i in range(len(l) - 1, -1, -1):
        result.append(l[i])
    return result
def reversed_copy_v2(l: list[int]) -> list[int]:
    result = []
    for v in l:
        result.insert(0, v)
    return result
orig = [1, 2, 3]
assert reversed_copy(orig) == [3, 2, 1] == reversed_copy_v2(orig) and orig == [1, 2, 3]
assert reversed_copy([]) == []

# Ex 14.13 [DESAFIO]  b) 4   c) [1, 2, 3, 4, 5, 6, 7, 7]

# Ex 14.14 [DESAFIO]
def all_the_same(l: list[int]) -> bool:
    for i in range(len(l) - 1):
        if l[i] != l[i + 1]:
            return False
    return True
assert [all_the_same(x) for x in ([5, 5, 5, 5, 5], [5, 5, 6, 5, 5], [], [10])] == [True, False, True, True]

# Ex 14.15 [DESAFIO]
def has_duplicates(l: list[int]) -> bool:
    for i in range(len(l)):
        for j in range(i + 1, len(l)):
            if l[i] == l[j]:
                return True
    return False
assert (has_duplicates([3, 1, 4, 1]), has_duplicates([3, 1, 4]), has_duplicates([]),
        has_duplicates([7]), has_duplicates([2, 2])) == (True, False, False, False, True)

# Ex 14.16 [DESAFIO]
def how_many(c: str, l: list[str]) -> int:
    n = 0
    for i in range(0, len(l), 2):
        if l[i] == c:
            n += 1
    return n
assert how_many('a', list("aolaeoa")) == 2 and how_many('a', list("zaoaeax")) == 0

# Ex 14.17 [DESAFIO]
def get_month(order: int, leap_year: bool) -> int:
    DURATIONS = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]
    for month in range(1, 13):
        days = DURATIONS[month - 1]
        if month == 2 and leap_year:
            days += 1
        if order <= days:
            return month
        order -= days
    return 12
assert [get_month(5, False), get_month(31, False), get_month(60, True),
        get_month(60, False), get_month(366, True), get_month(365, False)] == [1, 1, 2, 3, 12, 12]

# Ex 14.18 [DESAFIO]
def is_geometric(l: list[float]) -> bool:
    if l[0] == 0:
        return False
    r = l[1] / l[0]
    for i in range(1, len(l) - 1):
        if abs(l[i + 1] - l[i] * r) > 1e-9:
            return False
    return True
assert [is_geometric(x) for x in ([2, 4, 8, 16, 32, 64], [2, 4, 8, 16, 32, 65], [1.1, 5.5],
                                  [0, 5.5], [2.33] + [0] * 9)] == [True, False, True, False, True]

# Ex 14.19 [SOZINHA]
def partial_averages(l: list[int]) -> list[float]:
    """ m[i] is the average of l[0..i]. """
    result = []
    total = 0
    for i in range(len(l)):
        total += l[i]
        result.append(total / (i + 1))
    return result
assert partial_averages([1, 2, 6, 9, 2]) == [1.0, 1.5, 3.0, 4.5, 4.0]
assert partial_averages([]) == [] and partial_averages([17]) == [17.0]

def day_order(day: int, month: int, year: int) -> int:
    """ Position of a date within its year. Precondition: valid date """
    total = day
    for m in range(1, month):
        total += month_length(m, year)
    return total
assert (day_order(1, 1, 2008), day_order(31, 12, 2024), day_order(10, 3, 2024)) == (1, 366, 70)
print("Secção 14 OK")
