# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_5_valores_referencias_escopo.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as respostas, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Os exercícios "o que escreve?" são verificados capturando o output real.
"""






# %%
import io, contextlib

def out(code: str) -> str:
    """ Output printed by running code in a fresh namespace. """
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        exec(code, {})
    return buf.getvalue()

# Ex 15.3 [FAZ]
assert out('s = "ola"\nt = s\ns = s + "!"\nprint(s, t)\nprint(s is t)') == "ola! ola\nFalse\n"
# Ex 15.4 [PENSA]  x += 5 cria o 15 e muda a seta do x; o 10 não muda; y continua no 10
# Ex 15.5 [DESAFIO]
assert out("a = 1\nb = a\nc = b\nb = 7\na = c + b\nprint(a, b, c)") == "8 7 1\n"
# Ex 15.6 [SOZINHA]  (Python Tutor)

# Ex 16.4 [FAZ]
assert out('''
def f(a, b):
    a = a + b
    b = a - b
    return a * b
a, b = 2, 3
c = f(b, a)
print(a, b, c)''') == "2 3 15\n"
# Ex 16.5 [FAZ]
assert out('''
def repeat(s):
    s = s * 2
    return s + "!"
word = "ola"
r = repeat(word)
print(word, r)''') == "ola olaola!\n"
# Ex 16.6 [PENSA]  impossível; a, b = b, a; com listas swap(l, i, j) funciona
# Ex 16.7 [DESAFIO]
assert out('''
n = 10
def h(m):
    n = m * 2
    return n + 1
def p(n):
    return h(n) + n
print(p(3), n)''') == "10 10\n"
# Ex 16.8 [SOZINHA]  a1() -> 2 ; a2() -> 6 ; a3() -> UnboundLocalError
t = 1
def a1() -> int:
    return t + 1
def a2() -> int:
    t = 5
    return t + 1
def a3() -> int:
    t = t + 1
    return t
assert a1() == 2 and a2() == 6
try:
    a3(); assert False
except UnboundLocalError:
    pass
print("Secções 15-16 OK")






# %%
# Ex 17.4 [FAZ]
assert out('''
result = 0
for i in range(1, 4):
    if i % 2 == 0:
        last_even = i
    result = i * 10
print(i, last_even, result)''') == "3 2 30\n"
# Ex 17.5 [PENSA]  [3, 4] -> UnboundLocalError; inicializar found = None antes do ciclo
def last_negative(l: list[int]):
    found = None
    for v in l:
        if v < 0:
            found = v
    return found
assert last_negative([3, -1, 4, -5, 2]) == -5 and last_negative([3, 4]) is None
# Ex 17.6 [DESAFIO]
assert out('''
x = 0
for i in range(3):
    for j in range(i):
        x = i + j
print(i, j, x)''') == "2 1 3\n"
# Ex 17.7 [SOZINHA]  "2" (o for reutiliza o nome n) ; com range(0): "5" (o for não corre)
assert out("n = 5\nfor n in range(3):\n    pass\nprint(n)") == "2\n"
assert out("n = 5\nfor n in range(0):\n    pass\nprint(n)") == "5\n"
print("Secção 17 OK")






# %%
# Ex 18.5 [FAZ]
assert out('''
x = [1, 2, 3]
y = x
z = [1, 2, 3]
y[1] = 20
z[1] = 30
print(x, y, z)
print(x == y, x == z, x is y, x is z)''') == "[1, 20, 3] [1, 20, 3] [1, 30, 3]\nTrue False True False\n"
# Ex 18.6 [FAZ]
assert out("a = [1]\nb = a\na += [2]\na = a + [3]\na.append(4)\nprint(a, b)") == "[1, 2, 3, 4] [1, 2]\n"
# Ex 18.7 [PENSA]  partilhar imutáveis não faz mal; m[0][0] = 1 altera a lista partilhada
# Ex 18.8 [DESAFIO]
assert out("a = [1, 2]\nb = [a, a]\na.append(3)\nb[0] = [0]\nprint(a, b)") == "[1, 2, 3] [[0], [1, 2, 3]]\n"
# Ex 18.9 [SOZINHA]
def create(nr: int, nc: int, value: int) -> list[list[int]]:
    m = []
    for _ in range(nr):
        m.append([value] * nc)
    return m
m = create(2, 4, 0)
m[-1][-1] = 7
assert m == [[0, 0, 0, 0], [0, 0, 0, 7]]
def copy_matrix(m: list[list[int]]) -> list[list[int]]:
    """ Independent copy of a matrix (m.copy() only copies the row ARROWS). """
    result = []
    for row in m:
        result.append(row.copy())
    return result
m2 = copy_matrix(m)
m2[0][0] = 99
assert m[0][0] == 0
shallow = m.copy(); shallow[0][0] = 99
assert m[0][0] == 99            # prova de que m.copy() não chega
print("Secção 18 OK")






# %%
# Ex 19.4 [FAZ]
assert out('''
def f(l):
    l.append(len(l))
    l[0] = 99
a = [5, 6]
f(a)
f(a)
print(a)''') == "[99, 6, 2, 3]\n"
# Ex 19.5 [FAZ]
def add_to_all(l: list[int], k: int) -> None:
    for i in range(len(l)):
        l[i] += k
def added(l: list[int], k: int) -> list[int]:
    result = []
    for v in l:
        result.append(v + k)
    return result
nums = [1, 2, 3]; add_to_all(nums, 10); assert nums == [11, 12, 13]
nums = [1, 2, 3]; assert added(nums, 10) == [11, 12, 13] and nums == [1, 2, 3]
# Ex 19.6 [PENSA]
def swap(l: list[int], i: int, j: int) -> None:
    l[i], l[j] = l[j], l[i]
l = [10, 20, 30]; swap(l, 0, 2); assert l == [30, 20, 10]
# Ex 19.7 [DESAFIO]
assert out('''
def accumulation(l):
    for i in range(1, len(l), 1):
        l[i] = l[i] + l[i-1]
    return l
a = [1, 2, 3]
b = accumulation(a)
c = accumulation(a.copy())
print(a, b, c, a is b)''') == "[1, 3, 6] [1, 3, 6] [1, 4, 10] True\n"
# Ex 19.8 [DESAFIO]
assert out('''
def g(l, n):
    n += 1
    l.append(n)
    l = l + [n]
    l.append(0)
    return l
x = [1]
k = 5
y = g(x, k)
print(x, k, y)''') == "[1, 6] 5 [1, 6, 6, 0]\n"
# Ex 19.9 [SOZINHA]
def zero_negatives(l: list[int]) -> None:
    for i in range(len(l)):
        if l[i] < 0:
            l[i] = 0
def without_negatives(l: list[int]) -> list[int]:
    result = []
    for v in l:
        if v >= 0:
            result.append(v)
    return result
l = [3, -1, 4, -5]; zero_negatives(l); assert l == [3, 0, 4, 0]
assert without_negatives([3, -1, 4, -5]) == [3, 4]
print("Secção 19 OK")






# %%
# Ex 20.1 [DESAFIO]
assert out('''
def update(values, limit):
    count = 0
    for i in range(len(values)):
        if values[i] > limit:
            values[i] = limit
            count += 1
    limit = 0
    return count
v = [3, 8, 5, 10]
lim = 5
c = update(v, lim)
print(v, lim, c)''') == "[3, 5, 5, 5] 5 2\n"
# Ex 20.2 [DESAFIO]
assert out('''
def f2(a):
    b = a
    b.append(1)
    a = [2]
    a.append(3)
    return b
x = []
y = f2(x)
z = f2(y)
print(x, y, z, x is z)''') == "[1, 1] [1, 1] [1, 1] True\n"
# Ex 20.3 [DESAFIO]
assert out('''
i = 10
def loop_sum(n):
    total = 0
    for i in range(n):
        total += i
    return total + i
print(loop_sum(4), i)''') == "9 10\n"
# Ex 20.4 [SOZINHA]  [1, 2, 4, 8] ; com mystery(m.copy()): m fica [1, 5, 5, 5]
mystery_code = '''
def mystery(l):
    for i in range(len(l) - 1):
        l[i + 1] = l[i] * 2
m = [1, 5, 5, 5]
'''
assert out(mystery_code + "mystery(m)\nprint(m)") == "[1, 2, 4, 8]\n"
assert out(mystery_code + "mystery(m.copy())\nprint(m)") == "[1, 5, 5, 5]\n"
print("Secção 20 OK")
