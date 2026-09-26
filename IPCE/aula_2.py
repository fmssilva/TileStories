# %%
"""
===========================================================================
GUIA DE SESSÃO — Prática 02 — IPCE 2026/2027
===========================================================================

Cobre os guiões 02a (ex. 9-15) e 02b (ex. 16-18).

"""

# %%
"""
===========================================================================
Warm up / Revisões [10 min -> 10:25]
===========================================================================

a)  EXERCICIO [4 min -> 10:19] ---------------------------------------------
    Escrever este programa à mão no spyder e meter a correr, 
    analisar, e perguntar dúvidas que tenham
    
    Nota: para correr apenas esta célula de código, press: 
        Ctrl + Enter
    
"""
def cube(x: int) -> int:
    """ Cube of integer value. """
    return x * x * x

def main() -> None:
    i = int(input("Introduza um valor inteiro: "))
    print(f"O cubo de {i} é {cube(i)}.")

main()



# %%
"""
b)  ler o programa em conjunto [6 min -> 10:25]
    exemplo mesmo programa decomposto com prints para vermos o flow do programa a correr
"""
def cube_2(x: int) -> int:
    """ Cube of integer value. """
    return x * x * x 
    # também podíamos escrever o operador "potência" diretamente. 
    # return x ** 3  

def main() -> None:
    s = input("Introduza um valor inteiro: ")
    
    # reparem que o tipo que o input retorna do que escrevemos no terminal
    # é sempre uma string, mesmo que tenhamos escrito um número:
    print("s: ", s, type(s)) 
    
    # por isso depois temos que converter para o formato que queremos
    # neste caso convertemos para int (inteiro)
    # é esta conversão entre tipo de dados que se chama de "cast"
    i = int(s)
    print("i: ", i, type(i)) 
    
    # depois chamamos a função com o valor
    # e neste caso a função vai retornar também um int
    c = cube_2(i)
    print("c: ", c, type(c)) 
    
    # depois imprime no terminal
    # para o print() podemos escrever o texto e variaveis separados por vírgulas: 
    print("O cubo de ", i, "é ", c, ".")
    
    # ou podemos usar o f antes da string, e isso vai "formatar" a string, 
    # e assim podemos colocar variáveis e funções diretamente dentro da string,
    # entre {}
    print(f"O cubo de {i} é {c}.")
    print(f"O cubo de {i} é {cube_2(i)}.")

main()



# %%
"""
===========================================================================
CONCEITOS [26 min -> 10:51]
===========================================================================

a) Tipos de dados básicos [3 min -> 10:28] ---------------------------------------------------------------------------

Todo o dado em Python tem um tipo.
Os 5 tipos básicos:

    int    -> números inteiros            ex: 3, -10, 2024
    float  -> números com casas decimais  ex: 3.14, -0.5
    str    -> texto (sempre entre aspas)  ex: "olá", 'Python'
    bool   -> verdadeiro ou falso         ex: True, False
    None   -> "não há valor"
"""

idade = 20                # int
altura = 1.75             # float
nome = "Ana"              # str
esta_a_chover = False     # bool
resultado = None          # None

print(idade, type(idade))
print(altura, type(altura))
print(nome, type(nome))
print(esta_a_chover, type(esta_a_chover))
print(resultado, type(resultado))



# %%
"""
b) Converter entre tipos (cast) [4 min -> 10:32] ---------------------------------------------------------------------------

    Às vezes temos um valor num tipo e precisamos dele noutro tipo.
    A isso chama-se fazer um CAST (conversão). 
    Fazemos isso "envolvendo" o valor com o nome do tipo que queremos:
        int(...), float(...), str(...).
        
    por exemplo quando recebemos um numero da funçao input()
    na pratica isso é uma string e temos que converter para int() ou float()
    como vimos no caso anterior
"""

# int para string: 
a = 1                   # int 
b = str(a)              # "1"  (agora é str)
print("int -> str")
print(a, type(a))
print(b, type(b), "\n")       

# string para int: 
a = "25"     # string
b = int(a)   # 25   (agora é int, já dá para somar/multiplicar)
print("str -> int")
print(a, type(a))
print(b, type(b), "\n")
       
# string para float 
a = "3.5"
b = float(a)
print("str -> float")
print(a, type(a))
print(b, type(b), "\n")       

# float para int (repara que a parte decimal é truncada - não arredonda)
a = 3.9
b = int(a)
print("float -> int")
print(a, type(a))
print(b, type(b))

# Algumas conversão não são exequíveis e dão erro
# exemplo isto vai dar erro (remove o comentário e corre para veres): 

# a = int("vinte")



# %% 
"""
c) ---- operadores aritmeticos [3 min -> 10:35] ---------------------------------------------------------------------------
    Estes operadores funcionam como esperado com numeros (int ou float)
    
    +   soma
    -   subtraçao
    *   multiplicacao 
    **  expoente
    /   divisao
    //  divisao inteira
    %  resto da divisao 
    
"""

print("soma:", 2.5 + 2)
print("subtração:", 2.5 - 2)
print("multiplicação:", 2.5 * 2)
print("potência:", 2.5 ** 2)
print("divisão:", 2.5 / 2)          # devolve sempre float
print("divisão inteira:", 2.5 // 2)
print("resto:", 2.5 % 2)


# Quando fazemos operações com int, o resultado é sempre int
# EXCETO na divisão que devolve float
print (3/2)



# %%
""" 
C.1) EXERCICIO [5 min -> 10:40]:        
       Dado um valor,
       quantas notas de 20€ e 10€, e moedas de 1€, são precisas para obter esse valor?
"""

valor = 57
print("valor: ", valor)

notas_20 = valor // 20

restante = valor % 20
notas_10 = restante // 10
moedas = restante % 10
print("notas_20: ", notas_20)
print("restante: ", restante)
print("notas_10: ", notas_10)
print("moedas: ", moedas)



# %%
"""
d) Funções, Argumentos e Variáveis [5 min -> 10:45]
    
    Analisar o exemplo seguinte, 
    que é o programa anterior das notas, 
    mas escrito de forma bem organizada com funções:
        
"""
def num_notas(total: int, nota: int) -> int:
    return total // nota

def resto(total: int, nota: int) -> int:
    return total % nota

def main() -> None:
    total = int(input("Valor total em euros: "))
    n20 = num_notas(total, 20)
    r1 = resto(total, 20)
    n10 = num_notas(r1, 10)
    r2 = resto(r1, 10)   
    print(f"{n20} notas de 20€, {n10} notas de 10€, sobra {r2}€")

main()

"""
    Para declarar uma função usamos: 
        def <nome da função> (argumento_1, argumento_2, ...):
    
    Para criar variáveis podemos fazer de 2 formas: 
        declarar normalmente com uma atribuição 
            a = 20 
        e na pratica, quando declaramos os argumentos de uma função
            lá por tras na pratica a função está a declarar essas variaveis tambem
            def num_notas(total: int, nota: int) -> int:
                está a declarar as variáveis total e nota para usar dentro da função, 
                e depois atribui-lhes os valores que forem enviados como argumentos 
    
    Contexto das variaveis
        Reparem que temos 3 variáveis "total" e 2 variáveis "nota"
        e todas elas são diferentes porque estão em contextos diferentes
        cada variável está "dentro" da sua função, 
        e não é usada fora dessa função, 
        por isso não há conflito nenhum...
        
        o main tem a sua variavel "total"
        e depois envia o seu valor 
        para ser atribuido à variavel total da funçao num_notas
        (que por acaso até tem o mesmo nome)
"""



# %%
"""
 e) Boa organização de programa [3 min -> 10:48]
     
    O exemplo anterior já mostra uma boa divião/organização de um programa, 
    divido em 2 partes: 
        - main(), responsável pela interação com o utilizador:
                - IO - receber input
                - chamar funções auxiliares (o main não faz contas »
                  chama funções que fazem essas contas)
                - encadear chamadas - receber o resultado de uma função
                  e passar para a função seguinte
                - IO - imprimir resultados (output)
        - funções() que implementam a lógica do programa: podem ser
          complexas, chamar outras funções, encadear resultados, etc.,
          e no fim devolvem o resultado final.

    Nota: nos testes normalmente não se pede o main() nem IO. Pede-se
        apenas as funções com a lógica pura (vejam os testes de anos
        anteriores).
"""



# %%
"""
f) Comentários e Declaração de tipos de dados [3 min -> 10:51]
    
    Pegando no exemplo anterior, para fazer disso um bom programa 
    com código bem organizado, 
    além da boa organização entre main() e funções()
    está a faltar: 
        - comentários - para explicar o código
            pode ser comentário simples numa linha (# ...)
            ou várias linhas (\"\"\"   \"\"\")
        - declarar tipos de dados dos argumentos e retorno da função
            Isto não é mesmo validado pelo Python,
            são apenas uma 2ª forma de comentários incorporada no python
            para ajudar a perceber melhor as funções mais rapidamente
"""

def num_notas_(total: int, nota: int) -> int:
    """ retorna o numero máximo de notas do valor indicado
        que cabem dentro do valor total
        Precondition: total >= 0 and nota > 0
    """
    return total // nota

def resto_(total: int, nota: int) -> int:
    """ retorna o que sobra depois de darmos o máximo em notas do valor indicado
        Precondition: total >= 0 and nota > 0
    """
    return total % nota

def main() -> None:
    total = int(input("Valor total em euros: "))
    n20 = num_notas_(total, 20)
    r1 = resto_(total, 20)
    n10 = num_notas_(r1, 10)
    r2 = resto_(r1, 10)    
    print(f"{n20} notas de 20€, {n10} notas de 10€, sobra {r2}€")

main()



# %%
"""
===========================================================================
Guião 02a, exercícios 9, 10, 11, 12, 13 [30 min -> 11:21]
https://ipce-184ea7.gitlab.io/
===========================================================================

EXERCICIO 9 — Trovoada [4 min -> 10:55] ----------------------------------------
    
    Como sabe, o som propaga-se no ar à velocidade de 340m/s.
    Escreva um programa que determine a que distância se encontra uma trovoada.
    A entrada do programa é o número de segundos que separam o momento
    do relâmpago do momento do trovão.

    Nas contas, use só números inteiros e apresente o resultado em metros.

    [Quando não é especificado, o formato do output é livre.]
"""

# O exercicio final deve ser escrito algo neste formato: 
def distancia_trovoada(segundos: int) -> int:
    """ Distância (m) a que está uma trovoada, dado o nº de segundos
        entre o relâmpago e o trovão. Velocidade do som: 340 m/s.
        Precondition: segundos >= 0
    """
    return 340 * segundos

def main() -> None:
    t = int(input("Segundos entre relâmpago e trovão: "))
    print(f"A trovoada está a {distancia_trovoada(t)} metros.")

main()



# %%
# Mas durante a aula, para ser mais rápido, 
# foquem-se na lógica, e então escrevam algo assim mais simples
# sem a lógica do input() e cast e usando variaveis diretamente
# e para ser mais fácil de testar
def distancia_trovoada_(segundos: int) -> int:
    return 340 * segundos

def main() -> None:
    t = 5
    print(distancia_trovoada_(t))

main()



# %%
"""

EXERCICIO 10 — h/m/s -> segundos [5 min -> 11:00] ---------------------------------------- 
    
    C : 10 - Escreva um programa que receba três números inteiros
    correspondentes a um número de horas, minutos e segundos
    e converta esse período de tempo para segundos.
    A função que calcula o resultado terá naturalmente três parâmetros.

    Nas contas, use só números inteiros.

    Exemplo de execução
        Horas: 1
        Minutos: 1
        Segundos: 1
        3661
"""
def to_seconds(h: int, m: int, s: int) -> int:
    """ Converte uma duração dada em horas, minutos e segundos, para o
        total de segundos.
        Precondition: h >= 0 and 0 <= m < 60 and 0 <= s < 60
    """
    return h * 3600 + m * 60 + s

def main() -> None:
    # forma compacta durante a aula
    h = 2
    m = 3
    s = 1
    print(to_seconds(h, m, s))
    
    # forma correta
    # h = int(input("Horas: "))
    # m = int(input("Minutos: "))
    # s = int(input("Segundos: "))
    # print(to_seconds(h, m, s))

main()



# %%
"""
EXERCICIO 11 — segundos -> h/m/s [7 min -> 11:07] ----------------------------------------
    
    11 - Escreva um programa que receba uma duração em segundos
    e escreva esse tempo sob a forma de horas, minutos e segundos.
    Por exemplo, 1000000 segundos equivalem a 277 horas, 46 minutos e 40 segundos.
    Para lá da função main, escreva mais três funções:
        uma para extrair o número de segundos duma duração (um valor até 59);
        outra para extrair o número de minutos (outro valor até 59);
        e outra para extrair o número de horas (um valor sem limite).
    Apresente os resultados no formato que quiser.

    Nas contas, use só números inteiros. As contas obrigam a pensar um pouco...

    dica: é parecido com o problema das moedas — mas aqui é o main que
    manda o total original para cada uma das três funções, em vez de
    ser o main a encadear os restos entre elas. Ou seja, para calcular
    os minutos também se manda o total original, não o resto de horas.
"""

def get_hours(total: int) -> int:
    """ Horas duma duração total dada em segundos. """
    return total // 3600 # ou total // (60 * 60)

def get_minutes(total: int) -> int:
    """ Minutos duma duração total dada em segundos. """
    # return (total // 60) % 60 
    return (total % 3600) // 60 # resto das horas e qts minutos cabem nesse resto

def get_seconds(total: int) -> int:
    """ Segundos (0-59) duma duração total dada em segundos. """
    return total % 60

def main() -> None:
    total = 1_000_000 # int(input("Duração em segundos: "))
    h = get_hours(total)
    m = get_minutes(total)
    s = get_seconds(total)
    print(f"{h} horas, {m} minutos e {s} segundos.")

main()

# Exemplo do guião: 1_000_000 segundos -> 277 horas, 46 minutos, 40 segundos.



# %%
"""

EXERCICIO 12 — Período do pêndulo [4 min -> 11:11] ----------------------------------------

    12 - Um pêndulo simples realiza um movimento periódico constante.
    Chama-se período ao tempo que demora a executar um ciclo completo
    envolvendo uma oscilação para a esquerda e uma oscilação para a direita.

    O período T dum pêndulo simples depende do comprimento do fio (L)
    e da aceleração local da gravidade (g = 9.8 m/s2).

    Para pequenas amplitudes de oscilação, a fórmula que permite calcular
    o período é relativamente simples. Procure a fórmula na Net.

    Escreva um programa que receba o valor de L e escreva o período do pêndulo.
    Acha que o programa deve usar números inteiros ou números reais?

    dica: dá para usar a biblioteca math, para "pi" e "sqrt" — faz-se
    import math. Tal como nós escrevemos as nossas funções num ficheiro
    e depois as importamos para usar noutros ficheiros, o Python já
    vem com várias bibliotecas prontas a usar. Algumas têm de ser
    instaladas primeiro (como o matplotlib), outras já vêm prontas a
    importar sem mais nada, como o math ou o random.
"""

import math

def periodo_pendulo(L: float) -> float:
    """ Período (s) dum pêndulo simples de comprimento L (m), para
        pequenas amplitudes de oscilação. T = 2*pi*sqrt(L/g)
        Precondition: L > 0
    """
    g = 9.8
    return 2 * math.pi * math.sqrt(L / g)
    # return 2 * math.pi * (L / g)**(1/2)

def main() -> None:
    L = 50 #float(input("Comprimento do fio (m): "))
    print(f"Período = {periodo_pendulo(L)} segundos")

main()
# L é um comprimento medido — pode ter casas decimais, por isso float.
# O mesmo para g. Não há razão nenhuma para usar int aqui.



# %%
"""

CONCEITO - "Precondition" [4 min -> 11:15]

    Já apareceu várias vezes "precondition" nos comentários, por
    exemplo no exercício anterior, assumimos que o comprimento L é > 0.

    Uma pré condição É UM CONTRATO: 
        Um ALERTA para o "Cliente" que chama a função, fazer isso de forma correta,
        pois se não o fizer, e enviar parametros com valores fora dos limites aceites, 
        a função pode retornar valores errados, ou pode "rebentar" (exemplo se dividirmos por 0, etc.)
        
    Uma pré condição NÃO É UMA VALIDAÇÃO: 
       Validação seria se nós, no código, realmente precavesemos esses casos limite
       para evitar a função falhar. 
       Exemplo de validação:

           def periodo_pendulo(L: float) -> float:
               if L <= 0:
                   return -1   # valor "de erro" inventado
               ... mais validações e depois a lógica da função. 
       
    Vantagens das pré condições: 
        Simplificar o código. 
        Para não termos que implementar em cada funções todas as validações possíveis 
        de valores de parametros fora dos limites, de tipos de dados, etc, 
        que podiam fazer a nossa função falhar. 
        
        Então em vez de implementarmos código de validação para cada caso limite, 
        simplesmente escrevemos o alerta no comentário, exemplo escevemos "precondição 'L > 0'" 
        e depois cabe ao "cliente" que chama essa função, faze-lo de forma correta. 
        
        Isso poupa trabalho às FUNÇÕES em si — por exemplo, a seguir no
        ex 13 (paralelepípedo), as funções comprimento_arestas, area_total
        e volume não têm nenhum código de validação lá dentro, escrevem
        só a precondição "a > 0 and b > 0 and c > 0" e confiam nela.
        É o main() (o "cliente" que as chama) que opcionalmente pode
        validar antes de chamar, tal como fizemos agora no
        periodo_pendulo_/main() — e é o que vamos voltar a fazer no
        main() do ex 13.
        
        Noutros casos, pode ser pedido ou útil tratar os casos inválidos, 
        por exemplo no exercício 14 e 15 de seguida. 
"""

# Exemplo do exercicio anterior em que usamos bons comentários de pré-condições
# e então deve ser o cliente (quem chama a função) a validar os dados
# neste caso deve ser o main a validar que L > 0 antes de chamar a função 

def periodo_pendulo_(L: float) -> float:
    """ Período (s) dum pêndulo simples de comprimento L (m), para
        pequenas amplitudes de oscilação. T = 2*pi*sqrt(L/g)
        
        »» escrevemos este comentário para alertar:  
        Precondition: L > 0 
    """
    g = 9.8
    return 2 * math.pi * math.sqrt(L / g)

def main() -> None:
    L = float(input("Comprimento do fio (m): "))
    
    # main, que é quem chama a função periodo_pendulo_(L)
    # faz as validações necessárias, exemplo: 
    if L <= 0: 
        print("Comprimento do Pendulo não é válido!")
        return
    print(f"Período = {periodo_pendulo_(L)} segundos")

main()



# %%
"""
EXERCICIO 13 - paralelepípedo [6 min -> 11:21] --------------------------------

    13 - Considere um paralelepípedo retângulo (ou seja, um ortoedro)
    de comprimento a, largura b e altura c.
    Usando só números reais, escreva um programa com várias funções
    que produza a seguinte informação:
        O comprimento total das 12 arestas do paralelepípedo;
        A área total das suas 6 faces;
        O volume do paralelepípedo.

    Exemplo de execução
        A: 1.0
        B: 1.0
        C: 1.0
        Comprimento = 12.0
        Área = 6.0
        Volume = 1.0
"""

def comprimento_arestas(a: float, b: float, c: float) -> float:
    """ Comprimento total das 12 arestas dum paralelepípedo a x b x c.
        Precondition: a > 0 and b > 0 and c > 0
    """
    return 4 * (a + b + c)

def area_total(a: float, b: float, c: float) -> float:
    """ Área total das 6 faces dum paralelepípedo a x b x c.
        Precondition: a > 0 and b > 0 and c > 0
    """
    return 2 * (a * b + a * c + b * c)

def volume(a: float, b: float, c: float) -> float:
    """ Volume dum paralelepípedo a x b x c.
        Precondition: a > 0 and b > 0 and c > 0
    """
    return a * b * c

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    if (a <= 0 or b <= 0 or c <= 0):
        print("wrong values!")
        return
    print(f"Comprimento = {comprimento_arestas(a, b, c)}")
    print(f"Área = {area_total(a, b, c)}")
    print(f"Volume = {volume(a, b, c)}")

main()
# Exemplo do guião: A=B=C=1.0 -> Comprimento=12.0, Área=6.0, Volume=1.0.
# Nota de estilo: 3 funções pequenas, cada uma "uma coisa só", em vez de
# uma função gigante que calcula tudo.



# %%
"""
===========================================================================
INTERVALO [20 min -> 11:41]
===========================================================================
"""



# %%
"""
EXERCICIO 14 - Altura do precipício (visão) [5 min -> 11:46] ----------------------------------------

    14 - Escreva um programa que permita determinar a altura dum precipício
    a partir do número de segundos que demora a ver-se uma pedra a chegar lá a baixo.
    É o observador que deixa cair a pedra.
    Assuma que a aceleração da gravidade no local da experiência é a=9.8m/s2.
    Saiba também que a fórmula que dá a distância percorrida em função do tempo
    é d=0.5*a*t^2.

    Faça com que o seu programa leia um número inteiro (representando segundos),
    mas responda com um número real (representando metros).
"""
def altura_precipicio(t: int) -> float:
    """ Altura (m) dum precipício, a partir do tempo (s) que uma pedra
        demora a chegar ao fundo (o observador VÊ a pedra a chegar).
        d = 0.5*a*t^2, com a = 9.8 m/s^2
        Precondition: t >= 0
    """
    a = 9.8
    return 0.5 * a * (t ** 2)

def main() -> None:
    t = int(input("Segundos até se ver a pedra chegar: "))
    print(f"Altura = {altura_precipicio(t)} metros")

main()



# %%
"""
EXERCICIO 15 — Altura do precipício (som) [10 min -> 11:56] ------------------------------------

    15 - Escreva um programa que permita determinar a altura dum precipício
    a partir do número de segundos que demora a ouvir-se uma pedra
    chegar lá abaixo. É o observador que deixa cair a pedra.
    Este problema é semelhante ao exercício 14, simplesmente o sentido
    da visão é substituído pelo sentido da audição.
    [o enunciado original do guião tem um erro de referência e diz
     "exercício 13" — o exercício comparável é mesmo o 14, o da queda
     da pedra a olho.]

    Este problema obriga a pensar bastante mais. A melhor abordagem
    consiste em escrever um sistema de equações e resolvê-lo em ordem à
    variável "distância" antes de escrever o programa. "Programar" não
    é só escrever código. Resolver o problema também faz parte da
    tarefa.

    Exemplo de como pensar nisto, passo a passo:

    No ex.14 o observador VÊ a pedra chegar ao fundo: o tempo lido é
    diretamente o tempo de queda. Aqui o observador só OUVE o impacto,
    e o som demora tempo a subir de volta. Logo:

        t_total (o que medimos)  =  t_queda  +  t_som

    uma equação duas incógnitas: 
        - t_queda da pedra e t_som a voltar desde o chão ao observador 
    falta informação para resolver a equação. 
    Mas sabemos que os 2 movimentos percorrem a MESMA distância d 
    (a altura do precipício). Então:
    
    1) a pedra cai em queda livre durante t_queda:
        d = 0.5 * g * t_queda^2
                              
    2) o som sobe à velocidade constante (340 m/s):
        d = 340 * t_som »» e podemos eliminar esta variavel:
        d = 340 * (t_total - t_queda)

    3) Temos um sistema de equações:
        d = 0.5 * g * t_queda^2
        d = 340 * (t_total - t_queda)

    3.1) substituindo a primeira equaçao na segunda temos: 
        0.5*g*t_queda^2  =  340*t_total - 340*t_queda

        » já só temos a variavel t_queda (tudo o resto são constantes dadas) 
        arrumar tudo do mesmo lado, no formato A*x² + B*x + C = 0, com x = t_queda:
        
        0.5*g*t_queda^2  +  340*t_queda  -  340*t_total  =  0
       
        »  Aplicar fórmula resolvente: x = (-b ± √(b²-4ac)) / (2a). 
            e podemos aproveitar só a raiz "+" (a raiz "-" não faz sentido para tempo)
            
    3.2) tendo t_queda substituímos na primeira equação para obter d:
        d = 0.5 * g * (t_queda ** 2)

"""

def altura_precipicio_som(t_total: int) -> float:
    """ Altura (m) dum precipício, a partir do tempo total (s) entre
        largar a pedra e OUVIR o som do impacto.
        Precondition: t_total >= 0
    """
    g = 9.8       # aceleração da gravidade
    v_som = 340.0 # velocidade do som no ar

    # organizando para a formula quadratica temos: 
    A = 0.5 * g
    B = v_som
    C = -v_som * t_total

    # aplicando formula resolvente para obter a raiz positiva temos: 
    t_queda = (-B + math.sqrt(B ** 2 - 4 * A * C)) / (2 * A)

    # substitindo na 1ª equaçao temos 
    return 0.5 * g * (t_queda ** 2)

def main() -> None:
    t_total = int(input("Segundos até se ouvir o som do impacto: "))
    print(f"Altura = {altura_precipicio_som(t_total)} metros")

main()
# Exemplo para testar ao vivo: t_total=3 -> cerca de 40.65 metros.
# Reparem que é MENOS que os 44.1m do ex.14 com o mesmo t
# faz sentido, porque no ex.14 os 3 segundos são só de queda, enquanto
# aqui parte desses 3 segundos já foi "gasta" pelo som a subir de volta,
# logo sobra menos tempo de queda livre e a pedra cai menos.



# %%
"""
===========================================================================
CONCEITOS [11 min -> 12:07]
===========================================================================

a) Comparadores Lógicos [4 min -> 12:00]

    Comparação -> devolvem sempre True ou False (bool)
    ==  igual            !=  diferente
    >   maior            <   menor
    >=  maior ou igual   <=  menor ou igual

    
    Não confundir atribuição (=) vs comparação de igualdade (==)
    Este código dá erro: 
                # if a = 5:
                #     print("a é cinco")
                # Resposta: SyntaxError! Em Python, "=" é atribuição e "==" é comparação.
"""
print(3<5)
print(3<=5)
print(3==5)
print(3!=5)
print(3=="3")



# %%
"""
a.1) Encadear comparações: [3 min -> 12:03]
    
    "a < b < c" equivale a "(a < b) and (b < c)"

"""

print("2 < 5 < 3  ->", 2 < 5 < 3)
# Aqui 2<5 é True, mas 5<3 é False, por isso o resultado final é False. 
# Em Python podemos encadear operações assim,
# MAS isto NÃO é assim em todas as linguagens!




# %%
"""
a.2) Produtores de boolean [4 min -> 12:07]
        
    Na prática qualquer função que retorna True ou False

"""
def is_adult(idade: int) -> bool: 
    return idade >= 18

res = is_adult(18)
print(res, type(res))

res = is_adult(12)
print(res, type(res))



# %%
"""

b) Operadores Logicos [3 min -> 12:10] --------------------------------------------
    
    Para combinar condições e produzir expressoes mais complexas
    
    and  -> as duas têm de ser verdadeiras
    or   -> pelo menos uma tem de ser verdadeira
    not  -> inverte o valor
    
"""



# %%
"""
b.1) EXERCICIO [5 min -> 12:15]: 
    Escreve uma função que avalia se o cliente tem direito a desconto  
    Tem direito a desconto:
        SE for sócio E gastar mais de 50€,
        OU SE não for sócio mas gastar mais de 100€.
"""

def tem_desconto(socio: bool, gasto: float) -> bool:
    """ Verifica se o cliente tem direito a desconto. """
    if (socio and gasto > 50) or (not socio and gasto > 100): 
        return True
    else:
        return False 
    # já vamos ver a seguir que este código pode ser simplificado para: 
    # return (socio and gasto > 50) or (not socio and gasto > 100)
    
print(tem_desconto(True, 60))     # True  - sócio, gastou 60
print(tem_desconto(True, 30))     # False - sócio, gastou pouco
print(tem_desconto(False, 150))   # True  - não sócio, gastou muito
print(tem_desconto(False, 80))    # False - não sócio, só 80 (< 100)



# %%
"""

c) if/elif/else [3 min -> 12:18]
    
    permite controlar o fluxo do programa

    exemplo, se > 38º o programa entra num "branch"
    else, entra no outro
"""
def esta_doente(temperatura: float) -> str:
    if temperatura >= 38:
        return "Entrou no ramo para iniciar tratamento"
    else:
        return "Entrou no ramo para ser mandado embora"

print(esta_doente(38)) 
print(esta_doente(36))  



# %%
"""
c.1) EXERCICIO 16 - Máximo de dois inteiros [5 min -> 12:23]

    16 - Escreva um programa que permita achar o máximo
    entre dois valores inteiros.
    A escolha do valor maior deve ser efetuada numa função com dois argumentos
    chamada maximum. Dentro desta função, use a instrução if do Python.
"""
def maximum(a: int, b: int) -> int:
    """ Máximo entre dois valores inteiros. """
    if a >= b:
        return a
    else:
        return b
    
def main() -> None:
    x = int(input("Primeiro valor: "))
    y = int(input("Segundo valor: "))
    print(f"O máximo é {maximum(x, y)}")

main()



# %%
"""
c.2) EXERCICIO [6 min -> 12:29]: 
    
    Definir uma função para triagem de utentes do hospital
    dados temperatura e dificuldade em respirar, 
      - Dificuldade a respirar E febre >= 39   -> VERMELHO (urgente)
      - Dificuldade a respirar OU febre >= 39  -> AMARELO (atenção)
      - Febre >= 37.5                          -> VERDE (observação)
      - Caso contrário                         -> AZUL (sem gravidade)    
"""
    
def triagem(febre: float, dificuldade_respirar: bool) -> str:
    """ Classifica a urgência dum paciente. """
    if dificuldade_respirar and febre >= 39:
        return "VERMELHO - Urgente"
    elif dificuldade_respirar or febre >= 39:
        return "AMARELO - Atenção"
    elif febre >= 37.5:
        return "VERDE - Observação"
    else:
        return "AZUL - Sem gravidade"

print(triagem(39.5, True))   # VERMELHO
print(triagem(38.0, False))  # VERDE
print(triagem(36.5, False))  # AZUL



# %%
"""
c.3) A ordem importa!! [5 min -> 12:34]
    
    Ponto crucial: num elif, a ORDEM importa. O Python testa de cima
    para baixo e para na primeira condição verdadeira. Por isso as
    regras mais restritas/específicas têm de vir PRIMEIRO.

    Exemplo, esta função de triagem está mal:
    
"""
def triagem_1(febre: float, dificuldade_respirar: bool) -> str:
    if febre >= 37.5:
        return "VERDE - Observação"
    elif dificuldade_respirar and febre >= 39:
        return "VERMELHO - Urgente"
    elif dificuldade_respirar or febre >= 39:
        return "AMARELO - Atenção"
    else:
        return "AZUL - Sem gravidade"

print("Triagem 1: ", triagem_1(39.5, True))
# Estamos a testar uma condição mais fraca == mais abrangente, primeiro.
# A condição "febre >= 37.5" em 1º lugar 
# captura um paciente com febre=39.5 e dificuldade a respirar=True 
# (que deveria ser VERMELHO/urgente).



# %%
"""
EXERCICIO 17 Tipo de triângulo [13 min -> 12:47] - ------------------------------------------

    17 - Sejam a,b,c, valores reais, supostamente comprimentos dos lados dum triângulo.
    Escreva uma função com três argumentos chamada triangle_kind,
    que receba os tamanhos a, b, c dos lados dum triângulo e produza:
        0 se a,b,c não definirem um triângulo próprio;
        1 se a,b,c definirem um triângulo equilátero;
        2 se a,b,c definirem um triângulo isósceles;
        3 se a,b,c definirem um triângulo escaleno.

    Um triângulo próprio é um triângulo com área superior a zero!
    O comprimento de todos os lados tem de ser superior a zero
    e o comprimento de cada lado tem de ser estritamente inferior
    à soma do comprimento dos outros dois - só assim se garante que os lados
    definem um polígono fechado com área superior a zero.

    Depois de escrita a função triangle_kind, integre-a num programa
    que peça ao utilizador os três valores e mostre o resultado no ecrã.
"""

def is_proper_triangle(a: float, b: float, c: float) -> bool:
    """ Verifica se a, b, c definem um triângulo próprio (área > 0).
        Precondition: nenhuma (a função valida tudo sozinha, incluindo
        lados <= 0, porque o ENUNCIADO pede para tratar esse caso)
    """
    return a > 0 and b > 0 and c > 0 and a < b + c and b < a + c and c < a + b

def triangle_kind(a: float, b: float, c: float) -> int:
    """ Tipo de triângulo definido pelos lados a, b, c.
        Resultado: 0 - não é triângulo; 1 - equilátero;
                   2 - isósceles; 3 - escaleno
    """
    if not is_proper_triangle(a, b, c):
        return 0  # Invalid triangle
    elif a == b == c:
        return 1 # Equilateral triangle
    elif a == b or b == c or a == c:
        return 2 # Isosceles triangle
    else: # if a!=b and a!=c and b!=c
        return 3 # Scalene triangle

def main() -> None:
    a = float(input("A: "))
    b = float(input("B: "))
    c = float(input("C: "))
    print(triangle_kind(a, b, c))

main()
# Exemplo do guião: A=B=C=0.5 -> 1 (equilátero).
#
# "a == b == c" é a mesma comparação encadeada da gotcha (a). Agora já
# sabemos exatamente o que ela significa por dentro.
#
# Porque é que o elif do meio ("a==b or b==c or a==c") nunca apanha por
# engano um caso equilátero? Porque o if de cima já capturou esse caso
# primeiro — ordem outra vez.
#
# is_proper_triangle (a função booleana) + o if em triangle_kind validam
# tudo, ao contrário do ex.13, aqui faz sentido porque o próprio
# enunciado pede explicitamente "0 se não definirem um triângulo
# próprio" (aqui a validação faz parte do que é pedido, não é opcional).



# %%
"""
EXERCICIO 18 - Nota final de IPCE [8 min -> 12:55] ------------------------------------------

    18 - A nota final não arredondada da disciplina IPCE é um número real.
    No caso dos alunos que fazem a cadeira por testes,
    essa nota é calculada com base nas seguintes notas parcelares,
    todas valores reais entre 0 e 20 valores:
        1º Teste - 40% da nota final
        2º Teste - 40% da nota final
        Projeto - 20% da nota final

    Para um aluno ser aprovado por avaliação contínua, há dois requisitos:
        A nota do projeto tem de ser igual ou superior a 9.5 valores.
        A média dos dois testes tem de ser igual ou superior a 9.5 valores.

    Escreva um programa que leia as três notas parcelares dum aluno e que:
        No caso do aluno ficar aprovado, mostra a nota final não arredondada;
        No caso do aluno não ficar aprovado, escreve a palavra "REPROVADO".

    Recomendações:
        Para calcular a média dos dois testes, escreva uma função
        com dois argumentos reais e resultado real.
            def average(grade1: float, grade2: float) -> float:

        Para testar se o aluno foi aprovado, escreva uma função
        com três argumentos reais e resultado booleano.
            def passed(test1: float, test2: float, proj: float) -> bool:

        Para calcular a nota final não arredondada escreva uma função
        com três argumentos reais e resultado real.
        O resultado é calculado presumindo que o aluno foi aprovado.
            def final_grade(test1: float, test2: float, proj: float) -> float:
                "Precondition: passed(test1, test2, proj)"
"""
def average(grade1: float, grade2: float) -> float:
    """ Média de duas notas. """
    return (grade1 + grade2) / 2

def passed(test1: float, test2: float, proj: float) -> bool:
    """ Verifica se o aluno foi aprovado por avaliação contínua. """
    return proj >= 9.5 and average(test1, test2) >= 9.5

def final_grade(test1: float, test2: float, proj: float) -> float:
    """ Nota final não arredondada.
        Precondition: passed(test1, test2, proj)
    """
    return 0.4 * test1 + 0.4 * test2 + 0.2 * proj

# main() já dada no guião
def main() -> None:
    t1 = float(input("T1: "))
    t2 = float(input("T2: "))
    pr = float(input("PR: "))
    if passed(t1, t2, pr):
        print(final_grade(t1, t2, pr))
    else:
        print("REPROVADO")

main()
# Reparem que passed() chama average() — outra vez o encadeamento de
# chamadas.

