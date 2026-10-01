# %%
"""
===========================================================================
REVISÕES 6 - TESTE DE EXEMPLO 2
Introdução à Programação para a Ciência e Engenharia - estilo Teste 1
IPCE 2026/2027
===========================================================================

Teste de treino, inventado, com a mesma estrutura dos Testes 1 reais.
Um pouco mais exigente do que o Teste de exemplo 1.

  Duração: 1 hora e 30 minutos.   Cotação total: 20 valores.

  1. [3 valores] Execução à mão
  2. [3 valores] Função booleana sobre listas
  3. [4 valores] Série com um ciclo
  4. [3 valores] Função que cria uma lista
  5. [3 valores] [Difícil] Desenho com caracteres
  6. [4 valores] Programa completo

COMO FAZER ESTE TESTE (para treinar a sério)
  - Marca 1h30 num relógio. Sem consultar nada: nem ficheiros, nem Internet.
  - O teste a sério é em PAPEL, sem computador.
    O ideal é resolveres numa folha de papel.
    Se resolveres aqui no Spyder, NÃO corras nada enquanto o tempo não acabar.
  - Quando o tempo acabar: agora sim, escreve/corre o código, testa com os
    exemplos dos enunciados, e só depois abre o ficheiro das soluções:
        revisoes_6_teste_exemplo_2_solucoes.py
  - Corrige-te com honestidade, usando os critérios de cotação das soluções.

REGRAS DOS TESTES DE IPCE (copiadas da folha de rosto dos testes reais)
  - A interpretação do enunciado faz parte da resolução do teste.
    Se encontrar incoerências ou ambiguidades, resolva-as da melhor maneira,
    e explicite as decisões tomadas.
  - O seu código Python deve tirar o melhor partido da matéria lecionada.
  - Mesmo que não consiga resolver alguns dos problemas de forma perfeita e
    completa, tente evitar respostas em branco. Normalmente, respostas
    parciais ou com erros merecem alguma pontuação.

DICAS DE GESTÃO DO TEMPO
  - Lê o teste todo primeiro (3 min). Começa pelas perguntas que achas mais fáceis.
  - Média de 15 min por pergunta. Se uma encravar mais de 20 min, passa à seguinte.
  - Guarda 5 min no fim para reler: ":" no fim dos if/for/def, return vs print,
    off-by-one nos range, precondições e docstrings.
"""






# %%
"""
===========================================================================
1. [3 valores] Execução à mão
===========================================================================

a) [1 valor] O que escreve o código abaixo?

        def g(x: int) -> int:
            y = x * 2
            return y + 1

        def f(x: int, y: int) -> int:
            x = g(y)
            return x - y

        x, y = 3, 4
        z = f(y, x)
        print(x, y, z)


b) [1 valor] O que escreve o código abaixo?

        a = [1, 2, 3]
        b = a
        c = a + [4]
        b[0] = 10
        c[1] = 20
        a = [5]
        print(a, b, c)


c) [1 valor] Quantas vezes é escrito o asterisco? Assinale uma única resposta.

        for i in range(1, 5):
            for j in range(i, 5):
                if (i + j) % 2 == 0:
                    print("*")

        A.  6          B.  10           C.  8            D.  4
"""

# As tuas respostas:
#   a)
#   b)
#   c)






# %%
"""
===========================================================================
2. [3 valores] Função booleana sobre listas
===========================================================================

Escreva uma função booleana para testar se uma lista de inteiros está
ordenada por ordem crescente (são permitidos elementos iguais seguidos).
Exemplos:

        is_sorted([1, 2, 2, 5]) == True         is_sorted([]) == True
        is_sorted([3, 1, 2]) == False           is_sorted([7]) == True
        is_sorted([1, 2, 3, 0]) == False

Oferecemos-lhe o cabeçalho da função já escrito. Programe diretamente
com um ciclo: não use sort, sorted, nem outras funções prontas do Python.
Quanto mais simples for a função e quanto menos trabalho desnecessário
ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.
"""

def is_sorted(l: list[int]) -> bool:
    """ Check if the list is in increasing order (equal neighbours allowed). """
    pass






# %%
"""
===========================================================================
3. [4 valores] Série com um ciclo
===========================================================================

O número de Euler (e = 2.718281828...) pode ser calculado com esta série:

        e  =  1/0! + 1/1! + 1/2! + 1/3! + 1/4! + ...

(recorde que 0! = 1 e que n! = 1 * 2 * ... * n)

Escreva uma função real para calcular a soma dos primeiros n termos
desta série. Exemplos:

        euler(0) == 0.0         euler(3) == 2.5
        euler(1) == 1.0         euler(4) == 2.6667 (aproximadamente)
        euler(2) == 2.0

Programe a função usando um ciclo. Não use a biblioteca math.
Atenção: uma solução que calcula cada fatorial do zero, em cada termo,
faz muito trabalho desnecessário e não terá a cotação máxima.
Não programe nenhuma função main, nem use input ou print.
"""

def euler(n: int) -> float:
    """ Sum of the first n terms of the series 1/0! + 1/1! + 1/2! + ...
        Precondition: n >= 0
    """
    pass






# %%
"""
===========================================================================
4. [3 valores] Função que cria uma lista
===========================================================================

Escreva uma função que, dada uma lista de inteiros l, devolva uma NOVA
lista m com os máximos parciais da lista original: para cada i, o valor
m[i] é o maior dos valores l[0], l[1], ..., l[i]. Exemplos:

        running_max([3, 1, 4, 1, 5]) == [3, 3, 4, 4, 5]
        running_max([-2, -5, -1]) == [-2, -2, -1]
        running_max([]) == []

A lista original não pode ser modificada. Não use as funções max ou min.
Quanto mais simples for a função e quanto menos trabalho desnecessário
ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.
"""

def running_max(l: list[int]) -> list[int]:
    """ New list with the partial maximums of l. """
    pass






# %%
"""
===========================================================================
5. [3 valores] [Difícil] Desenho com caracteres
===========================================================================

Escreva uma função para desenhar um X com o caráter c dentro de um quadrado
de lado n. O caráter c aparece nas duas diagonais do quadrado; as restantes
posições são espaços. Cada linha tem exatamente n caracteres (contando com
os espaços), seguidos de uma mudança de linha.

Este X foi desenhado com a chamada draw_x('*', 5):

        *   *
         * *
          *
         * *
        *   *

(a 2ª e a 4ª linhas terminam com um espaço, que não se vê:
 " * * " tem 5 caracteres)

E este com draw_x('#', 4):

        #  #
         ##
         ##
        #  #

(aqui a 2ª e a 3ª linhas também terminam com um espaço: " ## ")

Quanto mais simples for a sua solução, melhor.
Não programe nenhuma função main, nem use input.
"""

def draw_x(c: str, n: int) -> None:
    """ Draw an X with the char c in a square of side n.
        Precondition: len(c) == 1 and n >= 1
    """
    pass






# %%
"""
===========================================================================
6. [4 valores] Programa completo
===========================================================================

Escreva um programa completo que mostre a evolução de um depósito a prazo
com juros compostos. O programa pergunta o capital inicial (real), a taxa
de juro anual em percentagem (real) e o número de anos (inteiro).

Em cada ano, o capital é multiplicado por (1 + taxa/100). O programa
escreve uma tabela com o capital no fim de cada ano, desde o ano 0 (o capital
inicial) até ao último ano, e no fim escreve o total de juros ganhos.
Todos os valores em euros são escritos com 2 casas decimais.

Se o capital não for positivo, ou a taxa for negativa, ou o número de anos
for negativo, o programa escreve apenas "Valores inválidos".

Eis dois exemplos de execução. Imite esta apresentação de forma rigorosa:

        Capital: 1000                       Capital: 1000
        Taxa (%): 4                         Taxa (%): 4
        Anos: 3                             Anos: -1
        Ano 0: 1000.00                      Valores inválidos
        Ano 1: 1040.00
        Ano 2: 1081.60
        Ano 3: 1124.86
        Juros: 124.86

Escreva um programa bem organizado, constituído por várias funções,
seguindo as boas regras de estilo usadas nas aulas. Nas funções que
escrever, inclua um pequeno comentário inicial e ainda uma precondição,
se ela for necessária.
"""

# A tua resposta (programa completo):
