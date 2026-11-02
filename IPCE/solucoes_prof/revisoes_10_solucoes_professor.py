# %%
"""
===========================================================================
GABARITO DO PROFESSOR - revisoes_10_strings.py
NÃO PARTILHAR COM OS ALUNOS
===========================================================================

Todas as respostas, incluindo [PENSA], [DESAFIO] e [SOZINHA],
e cada afirmação dos comentários (TypeError, "10" < "9", 4.45e-322, ...).
"""






# %%
import struct

def raises(fn, error) -> bool:
    """ Does fn() raise that error? """
    try:
        fn()
    except error:
        return True
    return False






# %%
# ---- Secção 35 ----------------------------------------------------------

assert 0b1011010 == 90 and chr(90) == 'Z' and type('Z') is str and len('Z') == 1
as_float = struct.unpack('<d', struct.pack('<q', 90))[0]
assert f"{as_float:.2e}" == "4.45e-322"
assert (ord('A'), ord('B'), ord('Z'), ord('a'), ord('0'), ord(' ')) == (65, 66, 90, 97, 48, 32)
assert chr(65) == 'A' and chr(97 + 2) == 'c' and ord('a') - ord('A') == 32
assert 'A' < 'B' and 'Z' < 'a' and '9' < 'A'
assert [ord(c) for c in "09"] == [48, 57] and [ord(c) for c in "az"] == [97, 122]

def is_digit_char(c: str) -> bool:
    return '0' <= c <= '9'

def digit_value(c: str) -> int:
    return ord(c) - ord('0')

def letter_position(c: str) -> int:
    return ord(c) - ord('a') + 1

def letter_at(n: int) -> str:
    return chr(ord('a') + n - 1)

assert is_digit_char('7') and not is_digit_char('x')
assert [digit_value(c) for c in "0123456789"] == list(range(10))
assert letter_position('a') == 1 and letter_position('c') == 3 and letter_position('z') == 26
assert letter_at(3) == 'c' and letter_at(26) == 'z'
assert all(letter_at(letter_position(c)) == c for c in "abcdefghijklmnopqrstuvwxyz")
print("Secção 35 OK")






# %%
# ---- Secção 36 ----------------------------------------------------------

s = "Python"
assert len(s) == 6 and (s[0], s[2], s[-1]) == ('P', 't', 'n')
assert s[1:4] == "yth" and s[:2] == "Py" and s[3:] == "hon"
assert 'th' in s and 'x' not in s and s + "!" * 3 == "Python!!!"

word = "banana"
assert sum(1 for c in word if c == 'a') == 3
assert [i for i in range(len(word)) if word[i] == 'n'] == [2, 4]
assert sum(1 for i in range(len(word) - 1) if word[i] == word[i + 1]) == 0

def assign_char():
    t = "gato"
    t[0] = 'p'
assert raises(assign_char, TypeError)
assert 'p' + "gato"[1:] == "pato"

def count_char(s: str, c: str) -> int:
    count = 0
    for x in s:
        if x == c:
            count += 1
    return count

def first_index(s: str, c: str) -> int:
    for i in range(len(s)):
        if s[i] == c:
            return i
    return -1

def all_digits(s: str) -> bool:
    for c in s:
        if not ('0' <= c <= '9'):
            return False
    return True

def initials(name1: str, name2: str) -> str:
    return name1[0] + "." + name2[0] + "."

assert count_char("banana", 'a') == 3 and count_char("", 'a') == 0
assert first_index("banana", 'n') == 2 and first_index("banana", 'x') == -1
assert all_digits("2026") and not all_digits("20a6") and all_digits("")
assert initials("Ana", "Silva") == "A.S."

s = "Lisboa"
assert s[1:3] == "is" and s[-2:] == "oa" and s[0] + s[-1] == "La"
assert "Ana" < "ana" and "10" < "9" and not (int("10") < int("9"))
assert len(s * 2) == 12 and "bo" in s
print("Secção 36 OK")






# %%
# ---- Secção 37 ----------------------------------------------------------

def reverse(s: str) -> str:
    result = ""
    for c in s:
        result = c + result
    return result

def remove_spaces(s: str) -> str:
    result = ""
    for c in s:
        if c != ' ':
            result += c
    return result

assert reverse("abc") == "cba" and reverse("") == ""
assert remove_spaces("o la, a mi go") == "ola,amigo"

def replace_char(s: str, old: str, new: str) -> str:
    result = ""
    for c in s:
        if c == old:
            result += new
        else:
            result += c
    return result

def double(s: str) -> str:
    result = ""
    for c in s:
        result += c * 2
    return result

def censor(s: str) -> str:
    result = ""
    for c in s:
        if c in "aeiou":
            result += '*'
        else:
            result += c
    return result

assert replace_char("banana", 'a', 'o') == "bonono" == "banana".replace('a', 'o')
assert double("abc") == "aabbcc" and censor("programar") == "pr*gr*m*r"

t = "  Olá Mundo  "
assert t.strip() == "Olá Mundo" and t.upper() == "  OLÁ MUNDO  " and t.lower() == "  olá mundo  "
assert "banana".replace("an", "AN") == "bANANa"
assert "o gato preto".split() == ['o', 'gato', 'preto'] and "12;7;30".split(";") == ['12', '7', '30']
assert "2026".isdigit() and not "20a6".isdigit() and "Olá".isalpha() and not "Olá!".isalpha()
assert t == "  Olá Mundo  "
assert not "".isdigit()                      # ao contrário do all_digits("")

sentence = "  o  gato   preto "
assert sentence.split() == ['o', 'gato', 'preto']
assert sentence.split(" ") == ['', '', 'o', '', 'gato', '', '', 'preto', '']
assert sum(int(p) for p in "3 4 5".split()) == 12

def count_words(s: str) -> int:
    return len(s.split())

def longest_word(s: str) -> str:
    words = s.split()
    best = words[0]
    for w in words:
        if len(w) > len(best):
            best = w
    return best

def capitalize(w: str) -> str:
    return w[0].upper() + w[1:].lower()

def normalize_name(s: str) -> str:
    result = ""
    for w in s.split():
        if result != "":
            result += " "
        result += capitalize(w)
    return result

assert count_words("  o  gato   preto ") == 3 and count_words("") == 0
assert longest_word("o gato preto comeu") == "preto"
assert capitalize("sILVA") == "Silva" and capitalize("a") == "A"
assert normalize_name("  ana   SILVA ") == "Ana Silva" and normalize_name("") == ""

# Ex 37.6 a): contar palavras sem split
def count_words_by_hand(s: str) -> int:
    count = 0
    for i in range(len(s)):
        if s[i] != ' ' and (i == 0 or s[i - 1] == ' '):
            count += 1
    return count

for x in ["  o  gato   preto ", "", "um", "a b c", "   "]:
    assert count_words_by_hand(x) == count_words(x), x
print("Secção 37 OK")






# %%
# ---- Desafios ------------------------------------------------------------

def is_palindrome(s: str) -> bool:
    n = len(s)
    for i in range(n // 2):
        if s[i] != s[n - 1 - i]:
            return False
    return True

def is_palindrome_while(s: str) -> bool:
    left = 0
    right = len(s) - 1
    while left < right:
        if s[left] != s[right]:
            return False
        left += 1
        right -= 1
    return True

for x, expected in [('amor a roma', True), ('ananana', True), ('', True), ('palindrome', False),
                    ('a', True), ('ab', False), ('abba', True), ('abca', False)]:
    assert is_palindrome(x) == expected == is_palindrome_while(x) == (x == x[::-1]), x

def compress(s: str) -> str:
    if s == "":
        return ""
    result = ""
    current = s[0]
    count = 0
    for c in s:
        if c == current:
            count += 1
        else:
            result += current + str(count)
            current = c
            count = 1
    result += current + str(count)
    return result

def compress_forgot_last(s: str) -> str:
    result = ""
    current = s[0]
    count = 0
    for c in s:
        if c == current:
            count += 1
        else:
            result += current + str(count)
            current = c
            count = 1
    return result

assert compress("aaabcc") == "a3b1c2" and compress("abc") == "a1b1c1" and compress("") == ""
assert compress("a") == "a1" and compress_forgot_last("aaabcc") == "a3b1"

def get_words(s: str) -> list[str]:
    clean = ""
    for c in s:
        if c.isalpha():
            clean += c
        else:
            clean += ' '
    return clean.split()

def average_word_length(s: str) -> float:
    words = get_words(s)
    total = 0
    for w in words:
        total += len(w)
    return total / len(words)

assert get_words("Olá, mundo! Tudo bem?") == ['Olá', 'mundo', 'Tudo', 'bem']
assert average_word_length("Olá, mundo! Tudo bem?") == 3.75
assert get_words("segunda-feira") == ['segunda', 'feira']

def password_problems(p: str) -> list[str]:
    has_upper = False
    has_digit = False
    for c in p:
        if 'A' <= c <= 'Z':
            has_upper = True
        if c.isdigit():
            has_digit = True
    problems = []
    if len(p) < 8:
        problems.append("curta")
    if not has_upper:
        problems.append("sem maiúscula")
    if not has_digit:
        problems.append("sem algarismo")
    return problems

assert password_problems("abc") == ['curta', 'sem maiúscula', 'sem algarismo']
assert password_problems("abcdefgh1") == ['sem maiúscula']
assert password_problems("Abcdefg1") == []

# Ex 38.5
def trace_a() -> str:
    s = "banana"
    r = ""
    for i in range(len(s)):
        if i % 2 == 0:
            r += s[i].upper()
        else:
            r += s[i]
    return r

def trace_c() -> str:
    s = "programa"
    i = len(s) - 1
    t = ""
    while i >= 0:
        t += s[i]
        i -= 2
    return t

def trace_d() -> tuple[str, int]:
    words = "um dois tres".split()
    r = ""
    for w in words:
        r = r + w[0] + w[-1]
    return (r, len(words))

assert trace_a() == "BaNaNa"
assert sum(1 for c in "Mississippi" if c in "sp") == 6
assert trace_c() == "aagr"
assert trace_d() == ("umdsts", 3)

# Ex 38.6 [SOZINHA] Anagramas
def is_anagram(a: str, b: str) -> bool:
    if len(a) != len(b):
        return False
    for c in a:
        if count_char(a, c) != count_char(b, c):
            return False
    return True

assert is_anagram("roma", "amor") and not is_anagram("ana", "nana")
assert not is_anagram("aab", "abb") and is_anagram("", "")
print("Desafios OK")