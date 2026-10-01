# %%
"""
===========================================================================
REVISÕES 6 - TESTE DE EXEMPLO 1
Introdução à Programação para a Ciência e Engenharia - estilo Teste 1
IPCE 2026/2027
===========================================================================

Teste de treino, inventado, com a mesma estrutura e o mesmo nível
dos Testes 1 de 2024/25 e 2025/26.

  Duração: 1 hora e 30 minutos.   Cotação total: 20 valores.

  1. [3 valores] Escolha múltipla e execução à mão
  2. [3 valores] Função booleana
  3. [4 valores] Série com um ciclo
  4. [3 valores] Função sobre listas
  5. [3 valores] [Difícil] Desenho com caracteres
  6. [4 valores] Programa completo

COMO FAZER ESTE TESTE (para treinar a sério)
  - Marca 1h30 num relógio. Sem consultar nada: nem ficheiros, nem Internet.
  - O teste a sério é em PAPEL, sem computador.
    O ideal é resolveres numa folha de papel.
    Se resolveres aqui no Spyder, NÃO corras nada enquanto o tempo não acabar.
  - Quando o tempo acabar: agora sim, escreve/corre o código, testa com os
    exemplos dos enunciados, e só depois abre o ficheiro das soluções:
        revisoes_6_teste_exemplo_1_solucoes.py
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
1. [3 valores] Escolha múltipla e execução à mão
===========================================================================

a) [1 valor] Qual o valor da expressão abaixo? Assinale uma única resposta.

        17 // 5 * 2 + 17 % 5

        A.  8          B.  4.4          C.  7          D.  5.4


b) [1 valor] Para cada um dos seis casos, indique se o código está
   sintaticamente correto ou incorreto. Só importam os erros de sintaxe
   (os que o Spyder assinala a vermelho). Assuma que as variáveis já
   foram definidas.

        1)  for i in range(10, 0, -1):           4)  def f(x: int) -> int
                print(i)                                 return x

        2)  if x => 5:                           5)  x, y = y, x
                print("sim")

        3)  total =+ 1                           6)  if x > 5:
                                                         print("a")
                                                     else if x > 3:
                                                         print("b")


c) [1 valor] Qual o valor que fica em x quando o ciclo termina?
   Assinale uma única resposta.

        x = 0
        for i in range(1, 10, 2):
            if i % 3 == 0:
                x += i
            elif i > 5:
                x = x * 2

        A.  15         B.  12           C.  6            D.  30
"""

# As tuas respostas:
#   a)
#   b)  1)        2)        3)        4)        5)        6)
#   c)






# %%
"""
===========================================================================
2. [3 valores] Função booleana
===========================================================================

Escreva uma função booleana para testar se uma data se situa no verão.
Vamos convencionar que o verão começa a 21 de junho e termina a 22 de
setembro (os dois dias incluídos). Exemplos:

        is_summer(21, 6) == True        is_summer(20, 6) == False
        is_summer(15, 8) == True        is_summer(22, 9) == True
        is_summer(23, 9) == False       is_summer(1, 1) == False

Oferecemos-lhe o cabeçalho da função já escrito. Quanto mais simples for
a função e quanto menos trabalho desnecessário ela fizer, melhor.
Não programe nenhuma função main, nem use input ou print.
Assuma que a data é válida.
"""

def is_summer(day: int, month: int) -> bool:
    """ Check if the date is in summer (21/6 to 22/9, inclusive). """
    pass






# %%
"""
===========================================================================
3. [4 valores] Série com um ciclo
===========================================================================

Em 1734, Euler resolveu o "problema de Basileia": mostrou que a soma
dos inversos dos quadrados dos números naturais vale pi²/6 (cerca de 1.6449).

        1/1² + 1/2² + 1/3² + 1/4² + ...  =  pi²/6

Escreva uma função real para calcular a soma dos primeiros k termos
desta série. Exemplos:

        basel(0) == 0.0         basel(2) == 1.25
        basel(1) == 1.0         basel(3) == 1.3611 (aproximadamente)

Programe a função usando um ciclo. Não use a biblioteca math.
Quanto mais simples for a função, melhor.
Não programe nenhuma função main, nem use input ou print.
"""

def basel(k: int) -> float:
    """ Sum of the first k terms of the series 1/1² + 1/2² + 1/3² + ...
        Precondition: k >= 0
    """
    pass






# %%
"""
===========================================================================
4. [3 valores] Função sobre listas
===========================================================================

Escreva uma função inteira que conte quantos elementos de uma lista de
inteiros são estritamente maiores do que a média da lista. Exemplos:

        count_above_average([1, 2, 3, 4, 10]) == 1     (a média é 4.0)
        count_above_average([5, 5, 5]) == 0            (a média é 5.0)
        count_above_average([1, 2]) == 1               (a média é 1.5)

Programe diretamente com ciclos: não use as funções sum, max, min,
nem métodos das listas. Pode definir uma função auxiliar
(com comentário e precondição, se necessária).
Quanto mais simples, melhor. Não programe main, nem use input ou print.
"""

def count_above_average(l: list[int]) -> int:
    """ How many elements are greater than the average of the list.
        Precondition: len(l) > 0
    """
    pass






# %%
"""
===========================================================================
5. [3 valores] [Difícil] Desenho com caracteres
===========================================================================

Escreva uma função para desenhar um losango com o caráter c.
A metade de cima do losango tem n linhas: a 1ª linha tem 1 caráter, a 2ª
tem 3, a 3ª tem 5, e assim por diante. A metade de baixo é igual à de cima,
virada ao contrário, mas sem repetir a linha do meio (a mais comprida).
No total, o losango tem 2n-1 linhas.

Este losango foi desenhado com a chamada draw_diamond('*', 4):

           *
          ***
         *****
        *******
         *****
          ***
           *

(a 1ª linha tem 3 espaços à esquerda, a 4ª linha tem 0 espaços)

Oferecemos-lhe algum código de partida, que é de uso opcional. Talvez
deseje definir uma função auxiliar (com comentário e precondição).
Quanto mais simples for a sua solução e quanto menos trabalho
desnecessário fizer, melhor. Não programe nenhuma função main, nem use input.
"""

def draw_segment(x: str, n: int) -> None:
    """ Draw a partial line with length n using the char x.
        Precondition: len(x) == 1 and n >= 0
    """
    for i in range(n):
        print(x, end='')

def draw_diamond(c: str, n: int) -> None:
    """ Draw a diamond whose top half has n lines.
        Precondition: len(c) == 1 and n >= 1
    """
    pass






# %%
"""
===========================================================================
6. [4 valores] Programa completo
===========================================================================

Escreva um programa completo que calcule o preço de um estacionamento.
O programa pergunta quantos minutos o carro esteve estacionado e escreve
o valor a pagar, com 2 casas decimais. As regras são:

  - Até 30 minutos (inclusive), o estacionamento é gratuito.
  - Acima de 30 minutos, paga-se 1.50 € por cada hora ou fração de hora,
    contando desde o início. Por exemplo, 61 minutos são 2 horas
    (1 hora completa e uma fração), e custam 3.00 €.
  - O valor máximo a pagar é 12.00 €, por muito tempo que o carro fique.
  - O tempo de estacionamento é um número inteiro de minutos entre 0 e 1440
    (24 horas). Se o utilizador escrever um valor fora deste intervalo,
    o programa escreve "Valor inválido".

Eis cinco exemplos de execução distintos. Imite esta apresentação de forma rigorosa:

        Minutos: 20              Minutos: 61              Minutos: 600
        A pagar: 0.00 euros      A pagar: 3.00 euros      A pagar: 12.00 euros

        Minutos: 60              Minutos: 2000
        A pagar: 1.50 euros      Valor inválido

Escreva um programa bem organizado, constituído por várias funções,
seguindo as boas regras de estilo usadas nas aulas. Use constantes para
os valores fixos do enunciado. Nas funções que escrever, inclua um pequeno
comentário inicial e ainda uma precondição, se ela for necessária.
"""

# A tua resposta (programa completo):
