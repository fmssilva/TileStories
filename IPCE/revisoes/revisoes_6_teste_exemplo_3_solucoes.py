# %%
"""
===========================================================================
REVISÕES 6 - TESTE DE EXEMPLO 3 - SOLUÇÕES
IPCE 2026/2027
===========================================================================

Só para abrires DEPOIS de fazeres o teste (1h30, sem consulta).

Para cada pergunta:
  - a solução recomendada ("quanto mais simples, melhor");
  - alternativas que também estão certas;
  - erros comuns;
  - critérios de cotação SUGERIDOS para te corrigires.
    (não são os critérios oficiais da cadeira: são uma estimativa
     razoável, para teres uma ideia da tua nota)

A última célula testa todas as soluções automaticamente.
"""






# %%
"""
===========================================================================
1. [3 valores] Execução à mão
===========================================================================

a) Escreve: 4 3 2 1 2 4   (com um espaço no fim)
       r(4): escreve 4, chama r(3)
         r(3): escreve 3, chama r(2)
           r(2): escreve 2, chama r(1)
             r(1): escreve 1, chama r(0) (não faz nada); 1 é ímpar: não escreve
           r(2): no regresso, 2 é par: escreve 2
         r(3): 3 é ímpar: não escreve
       r(4): 4 é par: escreve 4
   O 1º print acontece na "descida" (todos os n). O 2º só no "regresso",
   e só para os pares. (ficheiro 3, Ex 9.4 e 9.8)

b) Escreve: [1, 2] [1, 2, 20, 3] [1, 2, 20, 3, 30]
       b = add(a, 2):
           l += [2]          ALTERA a lista do a: a = [1, 2]
           l = l + [20]      lista NOVA [1, 2, 20]; só a seta do l muda
           return            b = [1, 2, 20]   (a lista nova)
       c = add(b, 3):
           l += [3]          ALTERA a lista do b: b = [1, 2, 20, 3]
           l = l + [30]      lista NOVA [1, 2, 20, 3, 30]
           return            c = [1, 2, 20, 3, 30]
       O a ficou com o que o 1º += lhe fez: [1, 2].
   A armadilha: com listas, l += [v] ALTERA a lista (quem chamou vê),
   mas l = l + [...] cria uma lista nova (quem chamou não vê).
   (ficheiro 5, Ex 18.3 e secção 19)

c) Resposta: A. x <= 0 or y > 5
   De Morgan (ficheiro 3, Ex 7.6): not (p and q) == (not p) or (not q)
       not (x > 0)    é  x <= 0
       not (y <= 5)   é  y > 5
   As outras:
       B. troca o "or" por "and": esquece que o not também troca o operador.
       C. o contrário de "x > 0" é "x <= 0", não "x < 0" (o 0 fica de fora).
       D. é not (x > 0) and not (y <= 5): o mesmo erro da B.

Cotação sugerida: 1 valor por alínea. Na b), 0.5 se só uma das 3 listas
estiver errada.
"""






# %%
"""
===========================================================================
2. [3 valores] Função booleana
===========================================================================
"""

def is_armstrong(n: int) -> bool:
    """ Check if n is an Armstrong number.
        Precondition: n >= 0
    """
    digits = str(n)
    k = len(digits)                 # número de algarismos = o expoente
    total = 0
    for c in digits:
        total += int(c) ** k
    return total == n

# A sugestão do enunciado resolve o problema dos algarismos:
#   str(153)  -> "153"   (percorre-se com for, como qualquer string)
#   len("153") -> 3      (o expoente)
#   int("5")  -> 5
# Depois é um acumulador normal, e no fim devolve-se a comparação (sem if).
#
# Alternativa sem strings: extrair os algarismos com % 10 e // 10.
# Mas isso precisa de saber primeiro quantos algarismos há, e com um ciclo
# while (que ainda não demos) ou recursividade. Muito mais trabalho.
#
# Erros comuns:
#   - expoente fixo 3 (só funciona para números de 3 algarismos);
#   - somar os caracteres sem int(c): "1" ** 3 dá TypeError;
#   - comparar total == str(n) (número com string: sempre False);
#   - if total == n: return True / else: return False (perde pela simplicidade).
#
# Cotação sugerida:
#   3.0 -> correta
#   2.0 -> correta só para 3 algarismos (expoente fixo)
#   1.5 -> ideia certa, com erros de conversão (int/str)
#   1.0 -> acumulador certo, comparação final errada






# %%
"""
===========================================================================
3. [4 valores] Produto com um ciclo
===========================================================================
"""

def wallis(n: int) -> float:
    """ Product of the first n factors of the Wallis product.
        Precondition: n >= 0
    """
    product = 1.0                   # elemento neutro da multiplicação
    for k in range(1, n + 1):
        product *= 4 * k * k / (4 * k * k - 1)
    return product

# É um acumulador de PRODUTO: começa em 1.0 (se começasse em 0, dava sempre 0).
# k vai de 1 a n (inclusive): range(1, n + 1). Com n = 0, devolve 1.0.
#
# Alternativa, com os dois "meio-fatores" do enunciado:
#     product *= (2 * k / (2 * k - 1)) * (2 * k / (2 * k + 1))
#
# Executar à mão wallis(2):
#   k = 1: 4 / 3              -> 1.3333
#   k = 2: 1.3333 * 16 / 15   -> 1.4222
#
# Erros comuns:
#   - product = 0 (tudo dá 0);
#   - range(n) -> k = 0 dá 0 / (-1) = -0.0 e o produto fica 0;
#   - 4 * k * k / 4 * k * k - 1 sem parêntesis
#     (lê-se ((4*k*k) / 4) * k * k - 1: outra coisa);
#   - somar em vez de multiplicar (+= em vez de *=).
#
# Cotação sugerida:
#   4.0 -> correta
#   3.0 -> off-by-one no range
#   2.0 -> parêntesis errados no fator, resto certo
#   1.0 -> acumulador de soma em vez de produto, ou início em 0






# %%
"""
===========================================================================
4. [3 valores] Função que cria uma lista
===========================================================================
"""

def remove_consecutive_duplicates(l: list[int]) -> list[int]:
    """ New list where each run of equal consecutive elements becomes one. """
    result = []
    for v in l:
        if len(result) == 0 or v != result[-1]:
            result.append(v)
    return result

# Cada elemento entra no resultado se for o primeiro, ou se for diferente
# do ÚLTIMO que entrou. Os iguais seguidos são saltados.
# O curto-circuito do "or" protege o result[-1] quando o result está vazio.
#
# Alternativa pelos índices (comparar com o vizinho da lista original):
#     result = []
#     for i in range(len(l)):
#         if i == 0 or l[i] != l[i - 1]:
#             result.append(l[i])
#     return result
#
# Erros comuns:
#   - "if v not in result": remove TODOS os repetidos, não só os seguidos
#     ([1, 2, 1] daria [1, 2]);
#   - result[-1] sem proteger o caso vazio -> IndexError no primeiro elemento;
#   - comparar l[i] com l[i + 1] e esquecer o último elemento;
#   - alterar a lista l (pop) em vez de criar uma nova.
#
# Cotação sugerida:
#   3.0 -> correta
#   2.0 -> correta exceto com a lista vazia ou no primeiro/último elemento
#   1.0 -> remove todos os repetidos (not in), em vez dos seguidos






# %%
"""
===========================================================================
5. [3 valores] [Difícil] Função sobre listas
===========================================================================
"""

def longest_run(l: list[int]) -> int:
    """ Length of the longest run of equal consecutive elements. """
    if len(l) == 0:
        return 0
    best = 1                        # a maior sequência encontrada até agora
    current = 1                     # o tamanho da sequência atual
    for i in range(1, len(l)):
        if l[i] == l[i - 1]:
            current += 1            # a sequência continua
        else:
            current = 1             # começa uma sequência nova
        if current > best:
            best = current
    return best

# Duas variáveis:
#   current -> quantos iguais seguidos há, a acabar na posição i
#   best    -> o maior current visto até agora (padrão do máximo, ficheiro 4, Ex 14.4)
# Uma só passagem pela lista, como pede o enunciado.
#
# Executar à mão [4, 4, 1, 4, 4, 4, 4]:
#   i | l[i] | igual ao anterior? | current | best
#   - |  4   |        -           |    1    |  1
#   1 |  4   |       sim          |    2    |  2
#   2 |  1   |       não          |    1    |  2
#   3 |  4   |       não          |    1    |  2
#   4 |  4   |       sim          |    2    |  2
#   5 |  4   |       sim          |    3    |  3
#   6 |  4   |       sim          |    4    |  4
#
# Erros comuns:
#   - atualizar o best só quando a sequência ACABA (no else):
#     a sequência do fim da lista nunca é contada ([4, 4, 1, 4, 4, 4, 4] daria 2);
#   - best e current a começar em 0 (com [7] daria 0);
#   - esquecer o caso da lista vazia (se começar com 1, [] daria 1);
#   - contar quantas vezes aparece o elemento mais frequente (não é o pedido).
#
# Cotação sugerida:
#   3.0 -> correta, uma só passagem
#   2.0 -> falha só na sequência do fim da lista, ou só na lista vazia
#   1.0 -> ideia de contar seguidos certa, mas várias falhas






# %%
"""
===========================================================================
6. [4 valores] Programa completo
===========================================================================
"""

PASS_GRADE = 9.5

def read_grades(n: int) -> list[float]:
    """ Read n grades from the user, one per line.
        Precondition: n > 0
    """
    grades = []
    for i in range(1, n + 1):
        grades.append(float(input(f"Nota {i}: ")))
    return grades

def average(l: list[float]) -> float:
    """ Average of the elements.
        Precondition: len(l) > 0
    """
    total = 0.0
    for v in l:
        total += v
    return total / len(l)

def best_position(l: list[float]) -> int:
    """ Position (from 0) of the first largest element.
        Precondition: len(l) > 0
    """
    pos = 0
    for i in range(1, len(l)):
        if l[i] > l[pos]:           # > e não >=: no empate fica o primeiro
            pos = i
    return pos

def count_passed(l: list[float]) -> int:
    """ How many grades are passing grades. """
    count = 0
    for v in l:
        if v >= PASS_GRADE:
            count += 1
    return count

def main() -> None:
    n = int(input("Quantos alunos? "))
    if not (n > 0):
        print("Número inválido")
    else:
        grades = read_grades(n)
        pos = best_position(grades)
        print(f"Média: {average(grades):.2f}")
        print(f"Melhor nota: {grades[pos]} (aluno {pos + 1})")
        print(f"Aprovados: {count_passed(grades)} de {n}")

main()

# A read_grades mistura leitura e lógica (como o ex 21 dos guiões):
# o próprio enunciado pede para ler as notas uma a uma.
# As outras funções só calculam, e a main escreve.
#
# best_position devolve a POSIÇÃO (a partir de 0), e não a nota:
# com a posição sabe-se a nota (grades[pos]) e o número do aluno (pos + 1).
# Uma função que só devolvesse a nota obrigava a procurar a posição outra vez.
#
# Melhor nota: 18.0 e não 18, porque as notas foram lidas com float.
#
# Erros comuns:
#   - aluno numerado a partir de 0 (escrever "aluno 2" em vez de "aluno 3");
#   - ">=" no best_position (no empate ficava o ÚLTIMO);
#   - "> 9.5" em vez de ">= 9.5" nos aprovados;
#   - média com :.2f esquecido, ou calculada com // (divisão inteira);
#   - pedir as notas com o prompt errado ("Nota 0:" em vez de "Nota 1:");
#   - tudo dentro da main, sem funções.
#
# Cotação sugerida:
#   4.0 -> correto, bem organizado, formato exato
#   3.0 -> correto, mas tudo (ou quase) na main
#   2.5 -> organização boa, com um erro (numeração do aluno, empate, >= 9.5)
#   1.5 -> lê as notas e calcula a média; o resto em falta ou errado






# %%
"""
===========================================================================
TESTES AUTOMÁTICOS DAS SOLUÇÕES
===========================================================================

    Corre esta célula DEPOIS de correres todas as células de cima
    (usa as funções que lá estão definidas). Se não aparecer nenhum erro,
    todas as soluções dão os resultados dos exemplos do enunciado.
    Podes também usá-la para testar AS TUAS funções: copia-as para cima
    desta célula (com os mesmos nomes) e corre.
"""

import io, contextlib

def captured(fn, *args) -> str:
    """ What fn(*args) prints. """
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        fn(*args)
    return buf.getvalue()

# Pergunta 1
def r(n: int) -> None:
    if n > 0:
        print(n, end=" ")
        r(n - 1)
        if n % 2 == 0:
            print(n, end=" ")
assert captured(r, 4) == "4 3 2 1 2 4 "

def add(l: list[int], v: int) -> list[int]:
    l += [v]
    l = l + [v * 10]
    return l
a = [1]
b = add(a, 2)
c = add(b, 3)
assert (a, b, c) == ([1, 2], [1, 2, 20, 3], [1, 2, 20, 3, 30])

for x in range(-3, 4):
    for y in range(2, 9):
        assert (not (x > 0 and y <= 5)) == (x <= 0 or y > 5)

# Pergunta 2
assert is_armstrong(153) and is_armstrong(9474) and is_armstrong(7) and is_armstrong(0)
assert not is_armstrong(10) and not is_armstrong(154)

# Pergunta 3
assert wallis(0) == 1.0
assert abs(wallis(1) - 1.3333) < 1e-4 and abs(wallis(2) - 1.4222) < 1e-4
assert abs(wallis(3) - 1.4629) < 1e-4

# Pergunta 4
original = [1, 1, 2, 2, 2, 3, 1]
assert remove_consecutive_duplicates(original) == [1, 2, 3, 1]
assert original == [1, 1, 2, 2, 2, 3, 1]
assert remove_consecutive_duplicates([5, 5, 5]) == [5]
assert remove_consecutive_duplicates([1, 2, 1]) == [1, 2, 1]
assert remove_consecutive_duplicates([]) == []

# Pergunta 5
assert longest_run([1, 1, 2, 2, 2, 3]) == 3 and longest_run([4, 4, 1, 4, 4, 4, 4]) == 4
assert longest_run([1, 2, 3]) == 1 and longest_run([7]) == 1 and longest_run([]) == 0

# Pergunta 6
g = [8.0, 12.5, 18.0, 12.5]
assert f"{average(g):.2f}" == "12.75" and best_position(g) == 2 and count_passed(g) == 3
assert best_position([5.0, 9.0, 9.0]) == 1                 # empate: o primeiro
assert count_passed([9.5, 9.4]) == 1                        # 9.5 aprova

print("Todas as soluções passam nos testes.")
