# %%
"""
===========================================================================
REVISÕES 6 - TESTE DE EXEMPLO 3
Introdução à Programação para a Ciência e Engenharia - estilo Teste 1
IPCE 2026/2027
===========================================================================

Teste de treino, inventado, com a mesma estrutura dos Testes 1 reais.
É o mais exigente dos três: ligeiramente ACIMA do nível dos testes reais.
Se conseguires mais de 10 valores aqui, estás bem preparada para o Teste 1.

  Duração: 1 hora e 30 minutos.   Cotação total: 20 valores.

  1. [3 valores] Execução à mão
  2. [3 valores] Função booleana
  3. [4 valores] Produto com um ciclo
  4. [3 valores] Função que cria uma lista
  5. [3 valores] [Difícil] Função sobre listas
  6. [4 valores] Programa completo

COMO FAZER ESTE TESTE (para treinar a sério)
  - Marca 1h30 num relógio. Sem consultar nada: nem ficheiros, nem Internet.
  - O teste a sério é em PAPEL, sem computador.
    O ideal é resolveres numa folha de papel.
    Se resolveres aqui no Spyder, NÃO corras nada enquanto o tempo não acabar.
  - Quando o tempo acabar: agora sim, escreve/corre o código, testa com os
    exemplos dos enunciados, e só depois abre o ficheiro das soluções:
        revisoes_6_teste_exemplo_3_solucoes.py
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

a) [1 valor] O que escreve a chamada r(4)? (atenção aos espaços)

        def r(n: int) -> None:
            if n > 0:
                print(n, end=" ")
                r(n - 1)
                if n % 2 == 0:
                    print(n, end=" ")


b) [1 valor] O que escreve o código abaixo?

        def add(l: list[int], v: int) -> list[int]:
            l += [v]
            l = l + [v * 10]
            return l

        a = [1]
        b = add(a, 2)
        c = add(b, 3)
        print(a, b, c)


c) [1 valor] Qual das expressões é SEMPRE equivalente a
   not (x > 0 and y <= 5)   (x e y são inteiros)?
   Assinale uma única resposta.

        A.  x <= 0 or y > 5             B.  x <= 0 and y > 5
        C.  x < 0 or y > 5              D.  not x > 0 and not y <= 5
"""

# As tuas respostas:
#   a)
#   b)
#   c)






# %%
"""
===========================================================================
2. [3 valores] Função booleana
===========================================================================

Um número de Armstrong é um inteiro não negativo que é igual à soma dos
seus algarismos, cada um elevado ao número de algarismos do número.
Por exemplo:
        153  = 1³ + 5³ + 3³               (3 algarismos -> expoente 3)
        9474 = 9⁴ + 4⁴ + 7⁴ + 4⁴          (4 algarismos -> expoente 4)

Escreva uma função booleana para testar se um número é de Armstrong.
Exemplos:

        is_armstrong(153) == True       is_armstrong(10) == False
        is_armstrong(9474) == True      is_armstrong(154) == False
        is_armstrong(7) == True         (um algarismo: 7¹ = 7)

Sugestão: str(n) dá a string com os algarismos de n, e int(c) converte um
carácter-algarismo de volta em inteiro (int("5") == 5).
Quanto mais simples for a função, melhor.
Não programe nenhuma função main, nem use input ou print.
"""

def is_armstrong(n: int) -> bool:
    """ Check if n is an Armstrong number.
        Precondition: n >= 0
    """
    pass






# %%
"""
===========================================================================
3. [4 valores] Produto com um ciclo
===========================================================================

Em 1655, John Wallis descobriu este produto infinito, que converge para pi/2:

        pi/2  =  (2/1 * 2/3) * (4/3 * 4/5) * (6/5 * 6/7) * ...

O fator número k (k = 1, 2, 3, ...) é  (2k/(2k-1)) * (2k/(2k+1)),
o que é o mesmo que  4k² / (4k² - 1).

Escreva uma função real para calcular o produto dos primeiros n fatores.
Exemplos:

        wallis(0) == 1.0          wallis(2) == 1.4222 (aproximadamente)
        wallis(1) == 1.3333 (aprox.)   wallis(3) == 1.4629 (aproximadamente)

Programe a função usando um ciclo. Não use a biblioteca math.
Quanto mais simples for a função, melhor.
Não programe nenhuma função main, nem use input ou print.
"""

def wallis(n: int) -> float:
    """ Product of the first n factors of the Wallis product.
        Precondition: n >= 0
    """
    pass






# %%
"""
===========================================================================
4. [3 valores] Função que cria uma lista
===========================================================================

Escreva uma função que, dada uma lista de inteiros, devolva uma NOVA lista
em que cada grupo de elementos iguais SEGUIDOS fica reduzido a um só
elemento. Exemplos:

        remove_consecutive_duplicates([1, 1, 2, 2, 2, 3, 1]) == [1, 2, 3, 1]
        remove_consecutive_duplicates([5, 5, 5]) == [5]
        remove_consecutive_duplicates([1, 2, 1]) == [1, 2, 1]
        remove_consecutive_duplicates([]) == []

Repare que o 1 aparece duas vezes no primeiro resultado: só se removem
os repetidos que estão SEGUIDOS.
A lista original não pode ser modificada.
Quanto mais simples for a função e quanto menos trabalho desnecessário
ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.
"""

def remove_consecutive_duplicates(l: list[int]) -> list[int]:
    """ New list where each run of equal consecutive elements becomes one. """
    pass






# %%
"""
===========================================================================
5. [3 valores] [Difícil] Função sobre listas
===========================================================================

Escreva uma função inteira que calcule o comprimento da maior sequência
de elementos iguais SEGUIDOS numa lista. Exemplos:

        longest_run([1, 1, 2, 2, 2, 3]) == 3           (o 2, três vezes seguidas)
        longest_run([4, 4, 1, 4, 4, 4, 4]) == 4        (o 4 no fim)
        longest_run([1, 2, 3]) == 1
        longest_run([7]) == 1
        longest_run([]) == 0

A solução deve percorrer a lista UMA só vez.
Quanto mais simples for a função e quanto menos trabalho desnecessário
ela fizer, melhor. Não programe nenhuma função main, nem use input ou print.
"""

def longest_run(l: list[int]) -> int:
    """ Length of the longest run of equal consecutive elements. """
    pass






# %%
"""
===========================================================================
6. [4 valores] Programa completo
===========================================================================

Escreva um programa completo que analise as notas de uma turma.

O programa pergunta quantos alunos há. Se o número não for positivo,
escreve "Número inválido" e termina. Senão, pede as notas dos alunos,
uma a uma (números reais), e guarda-as numa lista.
Assuma que as notas introduzidas estão sempre entre 0 e 20.

No fim, o programa escreve:
  - a média da turma, com 2 casas decimais;
  - a melhor nota e o número do aluno que a teve (os alunos são numerados
    a partir de 1; se houver empate, o primeiro desses alunos);
  - quantos alunos foram aprovados (nota maior ou igual a 9.5).

Eis dois exemplos de execução. Imite esta apresentação de forma rigorosa:

        Quantos alunos? 4                       Quantos alunos? 0
        Nota 1: 8                               Número inválido
        Nota 2: 12.5
        Nota 3: 18
        Nota 4: 12.5
        Média: 12.75
        Melhor nota: 18.0 (aluno 3)
        Aprovados: 3 de 4

Não use as funções sum, max, min, nem métodos das listas além de append.
Escreva um programa bem organizado, constituído por várias funções,
seguindo as boas regras de estilo usadas nas aulas. Nas funções que
escrever, inclua um pequeno comentário inicial e ainda uma precondição,
se ela for necessária.
"""

# A tua resposta (programa completo):
