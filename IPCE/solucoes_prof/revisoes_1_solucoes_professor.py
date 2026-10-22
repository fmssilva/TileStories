# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_1_variaveis_tipos_input.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as soluções em código, incluindo [PENSA], [DESAFIO] e [SOZINHA].
Cada solução tem asserts: se a célula correr sem erro, a solução está certa.
Os exercícios com input estão escritos como funções (mesma lógica, testável);
a versão com input está em comentário.
"""






# %%
# ---- Secção 1 -----------------------------------------------------------

# Ex 0.2 [FAZ]
assert 365 * 24 == 8760

# Ex 1.4 [FAZ]
x = 10; y = x * 2; x = y - 5; y = y + x
assert (x, y) == (15, 35)

# Ex 1.9 [FAZ]
total = 12.5 * 3
total = total - total * 0.10
assert total == 33.75

# Ex 1.10 [FAZ]
a, b = 10, 20
temp = a; a = b; b = temp
assert (a, b) == (20, 10)

# Ex 1.11 [PENSA] rodar: a=2, b=3, c=1
a, b, c = 1, 2, 3
temp = a; a = b; b = c; c = temp
assert (a, b, c) == (2, 3, 1)
# Pergunta extra (a=3, b=1, c=2): guardar primeiro o c
a, b, c = 1, 2, 3
temp = c; c = b; b = a; a = temp
assert (a, b, c) == (3, 1, 2)

# Ex 1.12 [DESAFIO] a) I  b) C  c) I  d) C  e) I  f) C  g) C

# Ex 1.13 [DESAFIO]
a, b, c = 1, 2, 3
a = b + c; c = a * b; b, c = c, b; a += c
assert (a, b, c) == (7, 10, 2)

# Ex 1.14 [SOZINHA] troca só com somas/subtrações
a, b = 7, 4
a = a + b       # 11
b = a - b       # 7
a = a - b       # 4
assert (a, b) == (4, 7)
print("Secção 1 OK")






# %%
# ---- Secção 2 -----------------------------------------------------------

# Ex 2.9 [FAZ]
assert (10 / 5, 10 // 4, 10 % 4, 2 ** 10, 3 * "ab", 5 > 3, "5" > "30", 1 + 2.0) \
    == (2.0, 2, 2, 1024, "ababab", True, True, 3.0)

# Ex 2.10 [FAZ]
t = 7384
assert (t // 3600, t % 3600 // 60, t % 60) == (2, 3, 4)

# Ex 2.11 [FAZ]
n = 472
assert n % 10 + n // 10 % 10 + n // 100 == 13

# Ex 2.12 [PENSA]
days = 1000
years = days // 365
rest = days % 365
weeks = rest // 7
days_left = rest % 7
assert (years, weeks, days_left) == (2, 38, 4)

# Ex 2.13 [DESAFIO]
assert ("aa" < "bbb", "123" < "123", "uva" < "banana", "zzzz" > "zzz",
        "Zebra" < "abelha", "100" < "99", 100 < 99) \
    == (True, False, False, True, True, True, False)

# Ex 2.14 [DESAFIO]
assert (7 // 2 * 2 + 7 % 2, 2 ** -1, -2 ** 2, 10 / 2 * 5,
        "ab" * 2 + "c", 1 + 1 == 2, 9 ** 0.5, 2 ** 3 ** 2) \
    == (7, 0.5, -4, 25.0, "ababc", True, 3.0, 512)

# Ex 2.15 [SOZINHA] número de 4 algarismos ao contrário
n = 1234
d1 = n // 1000
d2 = n // 100 % 10
d3 = n // 10 % 10
d4 = n % 10
reversed_n = d4 * 1000 + d3 * 100 + d2 * 10 + d1
assert reversed_n == 4321
print("Secção 2 OK")






# %%
# ---- Secção 3 -----------------------------------------------------------

# Ex 3.5 [FAZ]
def sentence(name: str, city: str) -> str:
    return f"A {name} vive em {city}."
assert sentence("Ana", "Lisboa") == "A Ana vive em Lisboa."
# name = input("Nome: "); city = input("Cidade: "); print(f"A {name} vive em {city}.")

# Ex 3.6 [FAZ]
quantity, price = 4, 3.5
assert f"{quantity} x {price:.2f} = {quantity * price:.2f}" == "4 x 3.50 = 14.00"

# Ex 3.7 [PENSA]
print(1, 2, 3, 4, sep=" -> ")          # 1 -> 2 -> 3 -> 4

# Ex 3.8 [DESAFIO] output:
#   ab
#   cd-e
#   1, 2, 3
#   2.5 2.500 x

# Ex 3.9 [SOZINHA]
def underline(name: str) -> str:
    return name + "\n" + "-" * len(name)
assert underline("Carolina") == "Carolina\n--------"
# name = input("Nome: "); print(name); print("-" * len(name))
print("Secção 3 OK")






# %%
# ---- Secção 4 -----------------------------------------------------------

# Ex 4.4 [FAZ]   age = int(input("Idade: ")); print(f"Daqui a 10 anos terás {age + 10} anos.")
assert f"Daqui a 10 anos terás {int('19') + 10} anos." == "Daqui a 10 anos terás 29 anos."

# Ex 4.5 [FAZ]
assert f"Total: {float('2.35') * int('3'):.2f}" == "Total: 7.05"

# Ex 4.6 [FAZ]
km = float("10")
assert f"{km} km = {km / 1.609344:.3f} milhas" == "10.0 km = 6.214 milhas"

# Ex 4.7 [PENSA]
g1, g2, g3 = float("12"), float("15.5"), float("14")
assert f"Média: {(g1 + g2 + g3) / 3:.1f}" == "Média: 13.8"
# Armadilha: g1 + g2 + g3 / 3 = 12 + 15.5 + 4.67 (só a 3ª nota é dividida)

# Ex 4.8 [DESAFIO]
assert (int("7") + int(7.9), str(1) + str(2), float("1e3"), str(2.0) + "1",
        int(" 12 "), int(-7.5), round(7.5) + round(6.5)) \
    == (14, "12", 1000.0, "2.01", 12, -7, 14)
try:
    int("7.0"); assert False
except ValueError:
    pass

# Ex 4.9 [SOZINHA] troco
def change(euros: float) -> list:
    cents = round(euros * 100)          # int(4.35 * 100) daria 434
    result = []
    for coin in [200, 100, 50, 20, 10, 5, 2, 1]:   # (a aluna escreve as 8 contas à mão)
        result.append(cents // coin)
        cents = cents % coin
    return result
assert change(4.35) == [2, 0, 0, 1, 1, 1, 0, 0]
assert change(3.88) == [1, 1, 1, 1, 1, 1, 1, 1]
# Versão esperada da aluna (sem ciclo):
#   cents = round(float(input("Valor: ")) * 100)
#   print(f"2 euros: {cents // 200}"); cents = cents % 200
#   print(f"1 euro: {cents // 100}");  cents = cents % 100
#   ... (50, 20, 10, 5, 2, 1)
print("Secção 4 OK")
