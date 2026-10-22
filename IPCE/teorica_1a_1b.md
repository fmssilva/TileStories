
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 01a (14/set/2026)
Apresentação da disciplina.

Apresentação da disciplina
Descrição de IPCE
Bibliografia
Docentes & Horários
Regras de Avaliação
Testes
Outros aspetos:
Convém fazer a disciplina por avaliação contínua e considerar o exame final é para usar só se alguma coisa correr mal (ou então para subir a nota).
O normal é participar em todas as aulas práticas. No entanto, em termos de obrigatoriedade, participar em 50% das aulas práticas é o requisito mínimo para alunos inscritos no 1º ano.
Para estimular a resolução de exercícios, estará disponível na página da disciplina um serviço de avaliação automática de exercícios de programação chamado Mooshak. Espera-se que os alunos resolvam a maioria dos exercícios instalados no Mooshak, apesar disso não ser obrigatório. A resolução dos exercícios do Mooshak não contribui de forma direta para a nota final, apesar de fornecer aos professores informação positiva sobre os alunos. Na aula prática 01b, temos estado a fazer as primeiras experiências com o Mooshak.
Faz parte dos objetivos formais da disciplina desenvolver algumas capacidades de trabalho em equipa. Nas aulas práticas, normalmente haverá dois alunos por mesa e os alunos trabalham aos pares. Mas o projeto final já será obrigatoriamente feito por grupos de 2 alunos. Só em raros casos, muito especiais, serão autorizados grupos com um único aluno, por exemplo no caso de certos trabalhadores-estudantes.
Funcionamento
6 créditos segundo o sistema ECTS
1 crédito = 28 horas de trabalho
168 horas de trabalho durante o semestre
Horas em contacto
Aulas teóricas (2h por semana)
Aulas práticas (3h por semana)
Esclarecimento de dúvidas (no horário de atendimento)
Horas em autonomia
Estudo da matéria das aulas, preparação para os testes e para o projecto prático, completar exercícios das práticas
Realização do projecto prático
Documentação oferecida
Folhas das aulas teóricas
Lista de exercícios
Enunciado do projeto
Objetivos de aprendizagem
Saber: as construções do fragmento coberto de Python; construir uma aplicação no fragmento a partir de uma especificação informal, com a metodologia definida; os componentes e ferramentas básicas de um ambiente de desenvolvimento de software e sua função.
Fazer: desenvolver programas de pequena dimensão, segundo certas convenções; desenvolver algoritmos simples; entender código escrito no fragmento coberto de Python; utilizar ferramentas de programação e interpretar os seus resultados; realizar, em grupo, um mini-projecto de desenvolvimento de software, integrando competências.
Desenvolver: hábitos de trabalho, individuais e em grupo, e de cumprimento de prazos; preocupação com a organização, o rigor e a execução de planos de trabalho.
Exemplo de programa escrito na linguagem Python
Nesta disciplina vamos aprender a programar usando a linguagem Python!
O objetivo principal é aprender a escrever programas de pequena dimensão. Mas também existe o objetivo secundário de conhecer parte da própria linguagem Python, a ferramenta que usaremos para escrever os programas.

O seguinte programa, escrito em Python, quando executado num computador, determina qual é máximo divisor comum entre dois números inteiros positivos que são pedidos ao utilizador:

def euclides(m: int, n: int) -> int:
    """ Greatest common divisor of two natural numbers.
        Precondition: m > 0 and n > 0
    """
    while m != n:
        if m > n:
            m -= n
        else:
            n -= m
    return m

def main() -> None:
    a = int(input("Introduza o primeiro inteiro: "))
    b = int(input("Introduza o segundo inteiro: "))
    print(f"euclides({a}, {b}) = {euclides(a,b)}")

main()
Eis um exemplo de execução do programa:
Introduza o primeiro inteiro: 123
Introduza o segundo inteiro: 456
euclides(123, 456) = 3
Razões pelas quais esta disciplina é importante!
Os aluno deste curso não vão ser programadores profissionais. Será que esta disciplina é assim tão importante?
A resposta é SIM! Por diversas razões:

Na vida profissional é uma enorme vantagem saber escrever pequenos programas para, por exemplo, processar uma coleção de dados guardados em ficheiros. É verdade que alguns dos problemas mais simples podem ser resolvidos carregando os dados numa folha de cálculo, fazendo umas ordenações e escrevendo uma fórmulas. Mas muitos problemas requerem mesmo a escrita dum pequeno programa.
Por vezes precisamos de resolver um programa e até descobrimos na Net um programa que faz mais ou menos aquilo que queremos, mas não o faz completamente. É importante ter a capacidade de olhar para um programa já escrito, entender as partes do código que nos interessam, e conseguir alterar alguns pormenores para o programa passar a fazer o que precisamos.
Saber escrever alguns pequenos programas de processamento de dados e de cálculo de estatísticas poderá é importante para a tese de final de curso. Também poderá ser uma vantagem para o aluno nos trabalhos de outras disciplinas.
Programar!
Programar é uma atividade que consiste em instruir uma máquina a realizar uma tarefa! Usa-se a máquina para resolver problemas que nos interessam!
Exemplos de tarefas complicadas realizáveis por computador: gerir os stocks dum grande supermercado; jogar xadrez; compreender e falar português.

A programação é uma tarefa atividade interessante, criativa e trabalhosa que exige que se pense duma forma muito especial. Só se adquire essa forma de pensar com a prática.

Aprender a programar requer persistência e que se pratique muito.

Conclusão: Aprender a programar exige tempo e dedicação.

A programação pode ser efetuada usando diversas estratégias, umas mais inteligentes do que outras. A técnica menos inteligente de todas chama-se força bruta. Aproveita-se a velocidade do computador para resolver o problema. Por exemplo, se quisermos saber quantas vezes o número 7 cabe no número 50, podemos subtrair o 7 sucessivamente até se chegar a um valor inferior a 7:
def fit7(number: int) -> int:
    how_many = 0
    while number >= 7:
        number -= 7
        how_many += 1
    return how_many
Mas a estratégia anterior é um pouco pateta, não é? A solução inteligente usa diretamente a divisão inteira:
def fit7(number: int) -> int:
     return number//7
Programar com confiança!
Como em tudo na vida, para desempenhar uma tarefa com confiança é preciso ganhar experiência, ou seja praticar bastante.
Ao contrário de outras disciplinas, como a Álgebra ou a Análise Matemática, a maioria dos alunos não tem qualquer experiência prévia de programação. Ou seja, o grau de novidade é muito grande.

Portanto pratique muito ao longo do semestre. Vá resolvendo o máximo de exercícios, logo a partir da 2ª semana de aulas.


Fred Brooks
Programar é divertido!
Em 1975, Fred Brooks identificou 5 razões pelas quais programar é divertido. Foi no seu livro "The Mythical Man-Month: Essays on Software Engineering", concretamente no capítulo 7, chamado "Joys of the Craft". O que se segue é uma adaptação livre:
É divertido programar pelo prazer de construir coisas. Tal como as crianças gostam de criar objetos com plasticina e alguns adultos de pintar quadros ou reconstruir automóveis antigos.
É divertido programar pelo prazer de construir coisas que são úteis para as outras pessoas. Tal como as crianças gostam de desenhar uns bonecos horríveis para o pai ou a mãe dependurarem na parede do gabinete de trabalho.
É divertido programar pelo fascínio de criar sistemas complexos, com diversas partes que se ligam de forma sofisticada. Faz também parte do fascínio observar os detalhes funcionamento do sistema, entendendo a sua arquitetura interna.
É divertido programar pelo prazer de estar sempre a aprender, o que advém do facto de programar nunca ser uma atividade repetitiva. Todos os problemas contém algo de novo e o programador aprende sempre algo de novo com cada problema.
É divertido programar pela maravilha que é trabalhar com uma "matéria prima" infinitamente flexível, próxima do pensamento puro. O programador desenvolve nos seus programas estruturas de ideias que refletem diretamente a sua imaginação.
Nota: Evidentemente, os programadores não têm o monopólio da criatividade; a maioria destas 5 razões também se aplicam a outras atividades humanas. No entanto são poucas as atividades que usam uma "matéria prima" tão flexível: conceitos do pensamento.
Fred Brooks também escreveu no seu livro um capítulo chamado "The Woes of the Craft" ("As Desventuras da Programação"). Alguns dos problemas referidos: os programas precisam de ser perfeitos e de confiança; descobrir e corrigir erros de pormenor nos programas pode ser demorado, maçador e trabalhoso; quando aproveitamos código existente, que não foi escrito por nós, por vezes esse código é confuso, difícil de perceber e de usar.

#120




Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 01b (14/set/2026)
Antevisão da programação em Python através da análise de vários miniprogramas já feitos.

Prólogo
Nesta aula apresentam-se alguns miniprogramas desenvolvidos usando uma parte limitada da linguagem Python.
Nestes exemplos, usam-se:

funções
tipos int e float
input e output
instrução condicional if
variáveis
biblioteca matemática
operadores de comparação e lógicos
recursividade
ciclo for.
Na próximas aulas teóricas, todos os mecanismos aqui usados serão esclarecidos devidamente. Mas, neste primeiro contacto, já deverá ser possível começar a entender diversas ideias importantes.

As primeiras aulas práticas de IPCE envolverão a escrita de pequenos programas em Python, e a programação será feita por imitação dos exemplos desta aula.

Programa das boas-vindas
Enunciado do problema
Desenvolva um programa que escreva simplesmente "Bem-vindos!".
Solução
def main() -> None:
    """ Say Welcome!  (this is a commentary) """
    print("Bem-vindos!")

main()
Execução
Escrevemos o programa, mandamos o programa correr, eis o que é escrito no ecrã:

Bem-vindos!
Explicações
Os nossos programas em Python consistirão numa sequência de unidades chamadas funções. Este primeiro programa contém apenas uma função chamada main.
Nos nossos programas existirá sempre uma função especial chamada main. Na última linha do programa existirá sempre uma invocação dessa função, para indicar onde o programa começa a correr.
O conceito de função tem em Python um sentido técnico específico, próximo do sentido que tem na Matemática, mas não exatamente igual. Esta questão será discutida em aulas posteriores.
Olhando para o cabeçalho da função, neste caso vê-se que ela não tem parâmetros nem resultado ("None" significa que não há resultado).
No corpo da função, neste caso ocorre apenas uma instrução, que consiste numa chamada da função predefinida print para escrever uma string particular (chamamos "string" a uma cadeia de carateres, ou seja um pedaço de texto).
O cabeçalho duma função tem de começar no início duma linha (sem espaços atrás), mas o corpo duma função tem de ficar indentado, normalmente usando 4 espaços.
Um programa mínimo, que faz uma conta com valores inteiros
Enunciado do problema
Desenvolva um programa que calcule o cubo dum valor inteiro.
Solução
def cube(x: int) -> int:
    """ Cube of integer value. """
    return x * x * x

def main() -> None:
    i = int(input("Introduza um valor inteiro: "))
    print(f"O cubo de {i} é {cube(i)}.")

main()
Execução
Introduza um valor inteiro: 5
O cubo de 5 é 125.
Explicações
Quem escreve um programa tem a responsabilidade de escrever código claro, fácil de entender por outras pessoas. Os nossos programas em Python serão constituído por várias funções. Cada função deve desempenhar uma tarefa bem definida, deve ter um nome bem escolhido que sugira a sua tarefa.
A função cube tem um parâmetro inteiro e produz um resultado inteiro (veja o cabeçalho da função). A função calcula e retorna o cubo dum número, o que resolve o o problema proposto. O uso da palavra return é essencial, porque sem usar return, uma função não produz resultado nenhum.
A função main trata da interação com o utilizador - neste e em todos os programas, pois é uma regra geral. A função pede os dados, manda fazer as contas, neste caso na função cube, e escreve o resultado.
Uma novidade! Dentro da função main definimos uma variável inteira, chamada i. Uma variável tem um valor associado e neste caso queremos associar à variável o valor inteiro introduzido pelo utilizador. Para associar um valor à variável i, usámos o operador de atribuição, que se escreve = (cuidado, não se trata da igualdade matemática).
A leitura dos dados é feita pela função predefinida input, que obtém a linha de texto introduzida pelo utilizador. A função int converte essa string para um valor inteiro.
Outra novidade! A string usada na função print é precedida pelo caráter f, que indica formatação. Numa string de formatação podem ocorrer expressões entre chavetas: as expressões são avaliadas e os resultados inseridos na string. Para perceber bem este mecanismo, quando correr o programa, examine a relação entre a string de formatação e o output produzido.
Num programa bem organizado, para evitar uma grande confusão, há dois aspetos que precisam de tratados separadamente: (1) interação com o utilizador e (2) lógica da resolução dos problemas. A função main é especializada na interação com o utilizador (e poderão existir funções de interação adicionais se a interação for complexa). As restantes funções são especializadas na lógica da resolução dos problemas.
Explicações adicionais
Para quem está a começar, este segundo programa está cheio de novidades. As duas mais importantes são:

A expressão cube(i) que usa a função cube para fazer uma conta;
O uso da variável i para guardar o valor lido do teclado e para o poder usar mais adiante. Precisamos dum nome para nos referirmos ao valor lido.
Um programa que usa números reais e a instrução condicional if
Enunciado do problema
Desenvolva um programa que calcule o valor absoluto dum valor real.
Solução
def absolute(x: float) -> float:
    """ Absolute value of float. """
    if x >= 0:
        return x
    else:
        return -x

def main() -> None:
    r = float(input("Introduza um valor real: "))
    print(f"O valor absoluto de {r} é {absolute(r)}.")

main()
Execução
Introduza um valor real: -45.79
O valor absoluto de -45.79 é 45.79.
Explicações
A função absolute tem um parâmetro real e produz um resultado real. Em Python, o nome float designa o tipo dos valores reais. Tal como na Matemática, na linguagem Python existe distinção entre números inteiros e números reais. Em cada exercício, devemos escolher o tipo de números mais adequado, mas por vezes o enunciado já diz qual o tipo de números a usar.
Num programa, é possível poder tomar decisões em função do estado corrente do programa. Para isso usa-se a instrução condicional if. Uma instrução if envolve: uma condição e dois ramos alternativos. Só um dos ramos será executado, consoante a condição seja verdadeira ou falsa.
Há uma terceira pequena novidade neste programa: o uso da função float para converter o input do utilizador num número real.
Um programa que contém uma função com dois parâmetros
Enunciado do problema
Desenvolva um programa que calcule o comprimento da hipotenusa de um triângulo retângulo a partir dos comprimentos dos respetivos catetos.
Solução
import math

def hypotenuse(cathetus1: float, cathetus2: float) -> float:
    """ Length of the hypotenuse of a right triangle.
        Precondition: cathetus1 > 0 and cathetus2 > 0
    """
    return math.sqrt(cathetus1 ** 2 + cathetus2 ** 2)

def main() -> None:
    cat1 = float(input("Primeiro cateto: "))
    cat2 = float(input("Segundo cateto: "))
    if not (cat1 > 0 and cat2 > 0):
        print("Argumentos inválidos")
    else:
        print(f"Hipotenusa({cat1},{cat2}) = {hypotenuse(cat1, cat2)}")

main()
Execução
Primeiro cateto: 1.0
Segundo cateto: 1.0
Hipotenusa(1.0,1.0) = 1.4142135623730951
Explicações
A função hypotenuse tem dois parâmetros reais e um resultado também real. Pela primeira vez, vemos uma função com dois parâmetros.
Dentro da função main definem-se duas variáveis, às quais ficam associados os valores reais introduzidos pelo utilizador. Usamos duas variáveis porque há dois valores desconhecidos a guardar.
A diretiva import, na primeira linha permite que este programa use os serviços do módulo de biblioteca math. Por uma questão de organização da linguagem, há diversos serviços que não fazem parte do núcleo do Python, mas estão guardados numa biblioteca de módulos. Só devemos importar um módulo se precisarmos do que ele oferece. Neste programa, importamos o módulo math por precisamos da função sqrt, que calcula a raiz quadrada dum número.
Um programa com uma função booleana e que usa operadores de comparação e operadores lógicos
Enunciado do problema
Desenvolva um programa que determine o número de dias dum ano dado.
Solução
def is_leap_year(year: int) -> bool:
    """ Check if the year is a leap year. """
    return (year % 4 == 0 and year % 100 != 0) or year % 400 == 0

def year_length(year: int) -> int:
    """ Number of days of a given year. """
    if is_leap_year(year):
        return 366
    else:
        return 365

def main() -> None:
    y = int(input("Introduza o ano: "))
    print(f"O ano {y} tem {year_length(y)} dias.")

main()
Execução
Introduza o ano: 2026
O ano 2026 tem 365 dias.
Explicações
A função is_leap_year testa se um dado ano é ou não bissexto. É uma função booleana. Os resultados possíveis são os valores True ou False.
A função usa os operadores de comparação: == (igualdade) e != (diferença).
Usa também são usados os operadores lógicos: and (conjunção) e or (disjunção).
O operador matemático % representa a operação inteira módulo, também conhecida por resto da divisão.
Quando o nome das nossas funções e variáveis tiver mais do que uma palavra, deve ser escrito assim, is_leap_year, usando o caráter sublinhado como separador.
Explicações adicionais
Este programa já tem três funções. Foi escrito com três funções para maximizar a clareza. Cada função tem uma tarefa específica. Não faz mal um programa ter muitas funções.

É provável que os tipos int e float não lhe causem estranheza porque você já está habituado a usar números inteiros e números reais na Matemática e nas calculadoras.

O tipo bool permite lidar com as noções de verdade e falsidade. Alguns alunos estudaram estas noções no ensino secundário, mas talvez tenham esquecido isso por ter sido dedicado pouco tempo ao assunto. O estudo da Lógica já vem da atualidade clássica, mas foi George Boole que tornou esse assunto um ramo da matemática.

Um programa com uma função recursiva
Enunciado do problema
A função da matemática fatorial, determina o número de permutações de n objetos. Por exemplo, de quantas formas diferentes 5 alunos se podem sentar em 5 cadeiras.
Desenvolva um programa que calcule o fatorial dum número inteiro não negativo. Use a seguinte definição recursiva, que conhecemos da Matemática:


Solução
def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    if n == 0:
        return 1
    else:
        return n * factorial(n - 1)

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not (x >= 0):
        print("Argumentos inválidos")
    else:
        print(f"fatorial({x}) = {factorial(x)}")

main()
Execução
Introduza um número natural: 10
fatorial(10) = 3628800
Explicações
A função factorial chama-se a ela própria. É uma técnica por vezes útil. O corpo desta função consiste na tradução direta para Python da fórmula matemática dada. Não discutimos aqui a razão de ser da fórmula (apesar de não ser complicado).
A função factorial é uma função parcial, ou seja é uma função que não está definida em todo o seu domínio. Esta função só está definida para valores não negativos. A maioria das funções que iremos programar serão totais, mas ocasionalmente aparecerá uma função parcial, como esta.
A terceira linha da função é uma precondição. Neste caso, a precondição está a informar que a função presume que os parâmetros são valores não negativos e que a função não se responsabiliza pelo que aconteça nos outros casos. As precondições são uma parte muito importante das nossas funções.
Um programa que usa um ciclo
Enunciado do problema
Em Matemática, há outra forma de definir a função fatorial.
Desenvolva um programa que calcule o fatorial dum número inteiro não negativo, através da multiplicação direta de inteiros consecutivos, usando a ideia do seguinte piatório:


Solução
def factorial(n: int) -> int:
    """ Factorial of a natural number.
        Precondition: n >= 0
    """
    fact = 1     # accumulator (this is also a commentary)
    for i in range(1,n+1,1):
        fact = fact * i
    return fact

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not (x >= 0):
        print("Argumentos inválidos")
    else:
        print(f"fatorial({x}) = {factorial(x)}")

main()
Execução
Introduza um número natural: 10
fatorial(10) = 3628800
Explicações
Os ciclos são uma das partes mais complicadas da programação. Este programa usa um ciclo for que faz a variável i variar de 1 até n. O corpo do ciclo é executado n vezes, neste caso. Cada execução do corpo dum ciclo chama-se uma iteração.
Usa-se aqui uma estratégia de acumulação. A variável fact é inicializada com o elemento neutro da multiplicação. Depois, ao longo das várias iterações do ciclo, os valores sucessivos de i vão sendo multiplicados a fact. No final, ficamos com o valor pretendido em fact.
Uma programa igual ao anterior, mas que escreve os sucessivos valores das variáveis para percebermos melhor como o ciclo funciona
Enunciado do problema
Em Matemática, há outra forma de definir a função fatorial.
Desenvolva um programa que calcule o fatorial dum número inteiro não negativo, através da multiplicação direta de inteiros consecutivos. Durante os cálculos, o programa deve mostrar a evolução do valor das variáveis usadas nas contas.

Solução
def factorial(n: int) -> int:
    """ Factorial of a natural number (showing the steps.)
        Precondition: n >= 0
    """
    fact = 1     # accumulator
    for i in range(1,n+1,1):
        fact = fact * i
        print(f"{i:2d} -> {fact}")
    return fact

def main() -> None:
    x = int(input("Introduza um número natural: "))
    if not (x >= 0):
        print("Argumentos inválidos")
    else
        print(f"fatorial({x}) = {factorial(x)}")

main()
Execução
Introduza um número natural: 10
 1 -> 1
 2 -> 2
 3 -> 6
 4 -> 24
 5 -> 120
 6 -> 720
 7 -> 5040
 8 -> 40320
 9 -> 362880
10 -> 3628800
fatorial(10) = 3628800
Explicações
Foi só adicionado um print dentro do ciclo for.
