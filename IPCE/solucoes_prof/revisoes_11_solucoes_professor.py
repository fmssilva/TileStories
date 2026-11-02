# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_11_colecoes.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as respostas, incluindo [PENSA], [DESAFIO] e [SOZINHA],
e cada afirmação dos comentários (KeyError, TypeError, {} é dict, ...).
"""






# %%
def raises(fn, error) -> bool:
    """ Does fn() raise that error? """
    try:
        fn()
    except error:
        return True
    return False






# %%
# ---- Secção 39 ----------------------------------------------------------

gl = [12, 15, 12, 18]; gt = (12, 15, 12, 18); gs = {12, 15, 12, 18}
gd = {"Ana": 12, "Rui": 15, "Eva": 12, "Teo": 18}
assert (len(gl), len(gt), len(gs), len(gd)) == (4, 4, 3, 4)
assert 15 in gl and 15 in gs and "Rui" in gd and 15 not in gd
assert gs == {12, 15, 18}

assert len({1, 1, 2, 2, 3}) == 3 and type({}) is dict and type(set()) is set
assert len(set("banana")) == 3 and (1, 2) + (3,) == (1, 2, 3) and (3) == 3
assert {"a": 1, "b": 2}["b"] == 2
assert raises(lambda: {"a": 1, "b": 2}["c"], KeyError)
def set_tuple():
    t = (1, 2, 3)
    t[0] = 9
assert raises(set_tuple, TypeError)
print("Secção 39 OK")






# %%
# ---- Secção 40 ----------------------------------------------------------

def minmax(l):
    smallest = l[0]
    largest = l[0]
    for v in l:
        if v < smallest:
            smallest = v
        if v > largest:
            largest = v
    return (smallest, largest)

assert minmax([2, 7, 5, -4]) == (-4, 7)
def unpack_wrong():
    a, b, c = minmax([1, 2])
assert raises(unpack_wrong, ValueError)

def sum_and_average(l):
    total = 0
    for v in l:
        total += v
    return (total, total / len(l))

def best(students):
    best_name, best_grade = students[0]
    for name, grade in students:
        if grade > best_grade:
            best_name = name
            best_grade = grade
    return best_name

def passed(students):
    names = []
    for name, grade in students:
        if grade >= 9.5:
            names.append(name)
    return names

S = [("Ana", 14), ("Rui", 9), ("Eva", 17)]
assert sum_and_average([4, 8, 6]) == (18, 6.0)
assert best(S) == "Eva" and best([("Ana", 14), ("Rui", 14)]) == "Ana"
assert passed(S) == ["Ana", "Eva"]
print("Secção 40 OK")






# %%
# ---- Secção 41 ----------------------------------------------------------

assert set([3, 1, 3, 2, 1, 3]) == {1, 2, 3}
ipce = {"Ana", "Rui", "Eva", "Teo"}
algebra = {"Rui", "Teo", "Bia"}
assert ipce & algebra == {"Rui", "Teo"} and len(ipce | algebra) == 5
assert ipce - algebra == {"Ana", "Eva"}
small_set = {1, 2}
assert raises(lambda: small_set[0], TypeError)

def count_distinct(l):
    return len(set(l))

def count_distinct_lists(l):
    distinct = []
    for v in l:
        if v not in distinct:
            distinct.append(v)
    return len(distinct)

def common_letters(w1, w2):
    return set(w1) & set(w2)

def has_repeated(l):
    return len(set(l)) != len(l)

L = [1, 5, 3, 6, 3, 4, 2, 5, 4, 3, 6, 5, 4, 1, 2, 3]
assert count_distinct(L) == 6 == count_distinct_lists(L)
assert count_distinct("The quick brown fox jumps over the lazy dog") == 28   # o exemplo da teórica 6b
assert common_letters("banana", "ananas") == {'a', 'n'}
assert not has_repeated([1, 2, 3]) and has_repeated([1, 2, 1]) and not has_repeated([])
print("Secção 41 OK")






# %%
# ---- Secção 42 ----------------------------------------------------------

capitals = {"Portugal": "Lisboa", "Espanha": "Madrid", "França": "Paris"}
assert raises(lambda: capitals["Itália"], KeyError)
capitals["Itália"] = "Roma"
capitals["Portugal"] = "Porto"
capitals["Portugal"] = "Lisboa"
assert len(capitals) == 4 and list(capitals) == ["Portugal", "Espanha", "França", "Itália"]

def letter_count(s):
    counts = {}
    for c in s:
        if c in counts:
            counts[c] += 1
        else:
            counts[c] = 1
    return counts

def letter_count_no_if(s):
    counts = {}
    for c in s:
        counts[c] += 1
    return counts

assert letter_count("banana") == {'b': 1, 'a': 3, 'n': 2} and letter_count("aba") == {'a': 2, 'b': 1}
assert raises(lambda: letter_count_no_if("banana"), KeyError)

def word_count(s):
    counts = {}
    for w in s.split():
        if w in counts:
            counts[w] += 1
        else:
            counts[w] = 1
    return counts

def most_common(d):
    best = None
    for k in d:
        if best == None or d[k] > d[best]:
            best = k
    return best

def invert(d):
    result = {}
    for k, v in d.items():
        result[v] = k
    return result

def total_by_category(expenses):
    totals = {}
    for category, value in expenses:
        if category in totals:
            totals[category] += value
        else:
            totals[category] = value
    return totals

wc = word_count("o gato e o rato")
assert wc == {'o': 2, 'gato': 1, 'e': 1, 'rato': 1} and list(wc) == ['o', 'gato', 'e', 'rato']
assert most_common(wc) == 'o' and most_common({'x': 1, 'y': 1}) == 'x'
assert invert({"Portugal": "Lisboa", "Espanha": "Madrid"}) == {"Lisboa": "Portugal", "Madrid": "Espanha"}
assert invert({"A": "x", "B": "x"}) == {"x": "B"}          # a 2ª apaga a 1ª
assert total_by_category([("casa", 50), ("comida", 20), ("casa", 30)]) == {"casa": 80, "comida": 20}

def by_first_letter(names):
    groups = {}
    for name in names:
        letter = name[0]
        if letter not in groups:
            groups[letter] = []
        groups[letter].append(name)
    return groups

assert by_first_letter(["ana", "rui", "alice", "rita", "eva"]) == {'a': ['ana', 'alice'], 'r': ['rui', 'rita'], 'e': ['eva']}

assert raises(lambda: {[1, 2]: "x"}, TypeError) and {(1, 2): "x"}[(1, 2)] == "x"
d = {"a": 1}; d["a"] = 2; d["a"] = 3
assert d == {"a": 3} and len(d) == 1
assert {"a": 1}.get("z", 0) == 0
print("Secção 42 OK")






# %%
# ---- Secção 43 ----------------------------------------------------------

votes = ["Ana", "Rui", "Ana", "Eva", "Ana", "Rui"]
names, counts = [], []
for v in votes:
    if v in names:
        counts[names.index(v)] += 1
    else:
        names.append(v); counts.append(1)
assert names == ['Ana', 'Rui', 'Eva'] and counts == [3, 2, 1]
assert word_count(" ".join(votes)) == {'Ana': 3, 'Rui': 2, 'Eva': 1}

def to_dict(l):
    d = {}
    for name, value in l:
        if name not in d:
            d[name] = []
        d[name].append(value)
    return d

def to_list(d):
    l = []
    for name, values in d.items():
        for value in values:
            l.append((name, value))
    return l

assert to_dict([('a', 1), ('b', 2), ('a', 5), ('a', 8), ('a', 1)]) == {'a': [1, 5, 8, 1], 'b': [2]}
assert to_list({'a': [1, 5, 8, 1], 'b': [2]}) == [('a', 1), ('a', 5), ('a', 8), ('a', 1), ('b', 2)]
assert to_dict([]) == {} and to_list({}) == []

MORSE = {'A': '.-', 'E': '.', 'O': '---', 'S': '...', 'T': '-', 'N': '-.'}

def encode(msg):
    result = ""
    for c in msg:
        if c in MORSE:
            if result != "":
                result += " "
            result += MORSE[c]
    return result

def decode(code):
    letters = invert(MORSE)
    result = ""
    for symbol in code.split():
        if symbol in letters:
            result += letters[symbol]
    return result

assert len(set(MORSE.values())) == len(MORSE)                # códigos todos diferentes
assert encode("SOS") == "... --- ..." and decode("... --- ...") == "SOS"
assert decode(encode("NOTA")) == "NOTA" and encode("SOZ") == "... ---"

PRICES = {"pão": 0.3, "leite": 1.0, "ovos": 2.5}

def total(order):
    result = 0.0
    for product, quantity in order:
        if product in PRICES:
            result += PRICES[product] * quantity
    return result

def missing(order):
    result = set()
    for product, quantity in order:
        if product not in PRICES:
            result.add(product)
    return result

def average_by_student(grades):
    sums, counts = {}, {}
    for name, grade in grades:
        if name not in sums:
            sums[name] = 0
            counts[name] = 0
        sums[name] += grade
        counts[name] += 1
    averages = {}
    for name in sums:
        averages[name] = sums[name] / counts[name]
    return averages

assert total([("pão", 4), ("leite", 2), ("caviar", 1)]) == 3.2
assert missing([("pão", 4), ("caviar", 1), ("trufas", 2)]) == {"caviar", "trufas"}
assert average_by_student([("Ana", 10), ("Rui", 14), ("Ana", 16)]) == {"Ana": 13.0, "Rui": 14.0}

# Ex 43.6 [SOZINHA]
WORDS = {"o": "the", "gato": "cat", "preto": "black", "come": "eats"}

def translate(s):
    result = ""
    for w in s.split():
        if result != "":
            result += " "
        if w in WORDS:
            result += WORDS[w]
        else:
            result += w
    return result

assert translate("o gato preto come peixe") == "the cat black eats peixe"
print("Secção 43 OK")