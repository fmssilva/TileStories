
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 08a (04/nov/2026)
Ficheiros em Python.
Modelação dum problema com dados em ficheiro.

Ficheiros
Vamos estudar a forma mais simples de manipular ficheiros de texto em Python. Existem várias formas alternativas que iremos ignorar.
Os ficheiros são agrupamentos de dados que são armazenados de forma permanente num periférico do computador. O periférico que pode ser um disco rígido, um disco SSD, uma memória flash, etc. O computador pode ser desligado sem que os ficheiros desapareçam.

Em contraste, os dados guardados na memória central do computador (RAM) perdem-se quando o computador é desligado.

Cada ficheiro caracteriza-se por: nome e dados. O programa pede ao sistema operativo acesso a um ficheiro usando a função predefinida do Python open. Para aceder a um ficheiro é preciso indicar o nome do ficheiro.

Existem duas grandes categorias de ficheiros:

Ficheiros de texto: cada ficheiro de texto é constituído por uma sequência de linhas, sendo cada linha constituída por uma sequência de carateres. Cada linha é terminada pelo caráter '\n'. Geralmente, um ficheiro de texto destina-se a ser compreendido por seres humanos, pois envolve texto normal.
Ficheiros binários: cada ficheiro binário contém dados que não representam texto, como por exemplo: músicas, filmes, fotos, dados comprimidos, etc. Geralmente, um ficheiro binário não pode ser diretamente compreendido por seres humanos, sendo necessário usar programas para interpretar os dados.
Em IPCE, só iremos trabalhar com ficheiros de texto, que serão processados linha a linha. Não trabalharemos com ficheiros binários.

Características dos ficheiros
Há dois aspetos em que os ficheiros são superiores à RAM:

A capacidade de armazenamento em ficheiro é normalmente milhares de vezes superior à da RAM dum computador normal. Note que conseguimos processar esses ficheiros usando um programa, mesmo sem eles caberem na RAM do computador.
Os ficheiros guardam os dados de forma persistente, ou seja os dados em ficheiro não se perdem quando um programa termina ou quando o computador é desligado.
Contudo existe esta desvantagem:
A leitura e escrita de dados em ficheiro é tipicamente 1000 vezes mais lenta do que a leitura e escrita de dados na memória central RAM
Quando processamos um ficheiro, é preferível tratar os dados fazendo um único percurso do ficheiro.

Este é um caso em que estamos dispostos a aceitar um algoritmo um pouco mais complicado e menos legível, desde que faça um único percurso do ficheiro.

Abertura de ficheiros
Antes de começar a usar um ficheiro de texto, primeiro é necessário abrir esse ficheiro usando a função predefinida open.
A função open tem como parâmetros o nome do ficheiro e o modo de abertura do ficheiro. Retorna um objeto que representa o ficheiro aberto. O tipo desse objeto é io.TextIOWrapper (note que o módulo de biblioteca io é automaticamente importado e você não precisa de o fazer).

Os modos de abertura que consideramos em IPCE são apenas estes dois:

'r' - leitura
'w' - escrita
Eis um exemplo de abertura dum ficheiro para leitura:
f = open("myfile.txt", 'r')
Se o ficheiro não existir, ou se não existirem direitos de leitura, ocorre um erro de execução:
>>> f = open("olaole.txt",'r')
Traceback (most recent call last):
  File "", line 1, in 
FileNotFoundError: [Errno 2] No such file or directory: 'olaole.txt'
Eis um exemplo de abertura dum ficheiro para escrita.

g = open("myfile2.txt", 'w')
Quando o modo de abertura é 'w':

Se o ficheiro não existir, o sistema operativo cria um ficheiro novo.
Se o ficheiro já existir, o seu conteúdo é apagado para se poder começar a escrever desde o início! Tenha sempre muito cuidado com a abertura de ficheiros em modo de escrita!
Teste da existência de ficheiro
Antes de tentar abrir um ficheiro, podemos querer verificar se ele existe. Para fazer isso, em IPCE, usamos esta função:
def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

>>> file_exists("myfile.txt")
True
>>> file_exists("nonexistent.txt")
False
Nesta função usa-se a construção try-except que faz aqui uma breve aparição em IPCE. É usada apenas aqui.

A função file_exists tenta abrir o ficheiro para leitura e retorna True se tudo correr bem.

Mas se não for possível abrir o ficheiro (devido a sua inexistência ou à falta de direitos de leitura), então ocorre um erro de execução, o erro é "capturado", e a função retorna False.

Possíveis problemas na abertura de ficheiros
Há vários problemas que podem ocorrer quando se tenta abrir um ficheiro.

A pasta corrente não é a que se espera
No Spyder, espera-se que a pasta (diretoria) corrente seja a pasta onde se encontra o programa que vai correr. No entanto, nem sempre é assim. Depende do sistema operativo que está a ser usado (Windows, MacOS, Linux) e da forma como o Spyder foi ativado.
Se o Python indicar que o ficheiro que se quer ler não existe, uma solução é dar o path completo do ficheiro, em vez de apenas o nome.

Outra hipótese é determinar qual a pasta corrente e colocar lá o ficheiro que se quer ler. Para saber qual a pasta corrente, escrever na consola:

import os
os.getcwd()    # get current working directory
Formato não padronizado
Em Portugal, são normalmente usadas duas formas de representar os carateres acentuados: o formato universal utf-8 e o formato especial iso-8859-1 (também designado como latin1).
Em IPCE, usamos sempre o formato utf-8. Por exemplo, o Mooshak usa esse formato. A maioria das instalações do Spyder também usa este formato.

O problema é se alguma instalação do Spyder estiver a usar o formato iso-8859-1.

Se, depois da abertura do ficheiro, quando o programa começa a ler o ficheiro, ocorrer um erro de execução onde aparece a palavra "encoding", então a solução é indicar um encoding explícito na abertura do ficheiro.

Faça assim para leitura:

f = open("myfile.txt", 'r', encoding="utf-8")
E assim para escrita:
g = open("myfile2.txt", 'w', encoding="utf-8")
Problemas com a extensão no nome do ficheiro
A extensão dos nossos ficheiros de dados é sempre ".txt.". Contudo o Windows pode não mostrar a extensão. Normalmente, o Windows está configurado para não mostrar. Mas é possível configurá-lo para a mostrar sempre.
Um problema que por vezes ocorre é alguém, confusamente, criar um ficheiro cujo nome interno tem a forma fich.txt.txt, portanto com dupla extensão.

Fecho de ficheiros
Após o tratamento dum ficheiro, quando o programa já não tiver de lidar mais com ele, devemos fechar o ficheiro usando a função predefinida close. A mesma função serve para ficheiros abertos para leitura e abertos para escrita:
Neste exemplo fecham-se os dois ficheiros que foram abertos na secção sobre a abertura de ficheiros:

f.close()
g.close()
Em todo o caso, quando o programa termina, todos os ficheiros são fechados automaticamente. Mesmo assim é importante fechar qualquer ficheiro logo que deixa de ser preciso, porque manter os ficheiros desnecessariamente abertos durante a execução pode causar problemas. O número máximo de ficheiros que podem estar simultaneamente abertos é limitado.
Eis um exemplo em que abrimos o mesmo ficheiro demasiadas vezes sem nunca fechar. Veja o que acontece:

def too_many_files(file_name: str, n: int) -> list[any]:
    """ Abre um ficheiro n vezes, sem nunca fechar.
        Precondition: file_exists(file_name) and n >= 0
    """
    l = []    # list of files
    for i in range(n):
      l.append(open(file_name, 'r'))
    return l
       
>>> too_many_files("myfile.txt", 1024)
Traceback (most recent call last):
  File "", line 1, in 
  File "", line 4, in too_many_files
OSError: [Errno 24] Too many open files: 'myfile.txt'
Leitura sequencial de ficheiros de texto
Em IPCE, lemos sempre os ficheiros linha a linha, começando na primeira linha e terminando na última linha. Este tipo de leitura diz-se sequencial.
O Python oferece diversas formas de fazer leituras de ficheiros. Nós vamos especializar-nos na utilização de ciclos for. Basicamente, depois de abrir o ficheiro para leitura, o ficheiro é percorrido linha a linha desta forma:

for line in f:
    ... processar: line
Apesar de raramente ser preciso, podemos também usar a útil operação next, que permite ler uma linha adicional. Por exemplo, o seguinte ciclo, lê três linhas em cada iteração:
for line1 in f:
    line2 = next(f)
    line3 = next(f)
    ... processar: line1, line2, line3
Note que, quando o ficheiro é lido linha a linha, o ficheiro não é todo carregado em memória. Só fica em memória uma linha de cada vez (ou três linhas, no caso do exemplo anterior). Isto pode ser importante, caso o ficheiro seja enorme. Repare que o tamanho máximo possível dum ficheiro é da ordem dos terabytes, enquanto o tamanho da memória RAM dum computador é da ordem dos gigabytes.
Um pormenor importante é que cada linha vem com uma mudança de linha no final. Se precisarmos de nos livrar dessa mudança de linha, a melhor forma é fazer assim, usando uma função da teórica 11:

line = line.rstrip()     # cria cópia da linha, com os brancos finais removidos. O '\n' é considerado um caráter branco.
Escrita em ficheiro de texto
O Python oferece várias alternativas para escrever em ficheiros, mas nós vamos especializar-nos na utilização duma operação que até já conhecemos bem:
A operação print
Por omissão, a operação print escreve no ecrã (consola). Mas é possível indicar um ficheiro, num argumento suplementar. Neste caso o output não vai para o ecrã, mas vai para o ficheiro.
A seguinte sequência de código, cria um ficheiro chamado "mytext2.txt" contendo apenas uma linha com o texto "Hello!":

g = open("myfile2.txt", 'w')
print("Hello!", file=g)     # escrita no ficheiro g
g.close()
Exemplos de processamentos simples
1º exemplo: Listar o conteúdo dum ficheiro no ecrã
def print_file(file_name: str) -> None:
    """ Precondition: file_exists(file_name) """
    f = open(file_name, 'r')
    for line in f:
        print(line, end='')
    f.close()

>>> print_file("myfile.txt")
111
222
333
some random words
Por que razão foi preciso usar no print end=''?
- Porque a linha já tem um '\n' no final.
2º exemplo: Contar as linhas dum ficheiro
def count_lines(file_name: str) -> int:
    """ Precondition: file_exists(file_name) """
    f = open(file_name, 'r')
    n = 0
    for _ in f:
        n += 1
    f.close()
    return n

>>> count_lines("myfile.txt")
4
Por que razão não fizemos desta forma mais simples, sem usar ciclo n = len(list(f))?
- Porque isso carregaria todo o ficheiro em memória.
3º exemplo: Criar lista com todas as linhas dum ficheiro
def load_lines(file_name: str) -> list[str]:
    """ Precondition: file_exists(file_name) """
    f = open(file_name, 'r')
    l = []
    for line in f:
        l.append(line)
    f.close()
    return l

>>> load_lines("myfile.txt")
['111\n', '222\n', '333\n', 'some random words\n']
Também se podia ter trocado a parte do código que faz a leitura pela expressão list(f) (mantinha-se o open e o close).
4º exemplo: Criar lista com parte das linhas do ficheiro - linha-não-linha-sim
def load_every_second_line(file_name: str) -> list[str]:
    """ Precondition: file_exists(file_name) """
    f = open(file_name, 'r')
    l = []
    for _ in f:            # ignora 1ª linha
        line2 = next(f)    # usa 2ª linha
        l.append(line2.rstrip())
    f.close()
    return l

>>> load_every_second_line("myfile.txt")
['222', 'some random words']
Porque foi usada a função next?
- Porque nos interessa ler duas linhas em casa iteração.
5º exemplo: Copiar ficheiro
def copy_file(src_name: str, target_name: str) -> None:
    """ Precondition: file_exists(src_name) """
    f = open(src_name, 'r')
    g = open(target_name, 'w')   # abertura para escrita!
    for line in f:
        print(line, end='', file=g)  # escrita no ficheiro g
    f.close()
    g.close()

>>> copy_file("myfile.txt", "myfile2.txt")
>>> print_file("myfile2.txt")
111
222
333
some random words
6º exemplo: Uma função main que lida com um ficheiro
def main() -> None:
    file_name = input("Nome do ficheiro: ")
    if file_exists(file_name):
        n = count_lines(file_name)
        print(f"O ficheiro tem {n} linhas")
    else:
        print("Não foi possível abrir o ficheiro")

main()

>>> main()
Nome do ficheiro: myfile.txt
O ficheiro tem 4 linhas
>>> main()
Nome do ficheiro: nonexistent.txt
Não foi possível abrir o ficheiro
Antes de chamar uma função que processa um ficheiro, muitas vezes temos de garantir que o ficheiro existe.

Carregamento dum ficheiro de dados
Enunciado: A Seleção Nacional vai participar no próximo Campeonato Europeu de Futebol. Desenvolva um pequeno programa que permita representar e manipular a seguinte informação referente a cada um dos jogadores:
Nome do jogador
Clube de origem
Número de golos marcados
Para saber qual o formato dos dados, eis um exemplo de ficheiro:
João Leixões                                         
5
Tarquínio Benfica
6
Pedro Sporting
3
Luís Belenenses
8
Miguel Leixões
2
João Sporting
1
Manuel Académica
5
Camões Porto
5
Fernando Benfica
10
Joaquim Boavista
7
Gervásio Belenenses
7
Resolução: O programa não especifica o que fazer. Para já, vamos fazer o mínimo necessário:
Criar uma dataclass Player para implementar cada jogador.
Criar uma dataclass Players para implementar o conjunto dos jogadores.
Leitura - Efetuar o carregamento do ficheiro para um objeto Players.
Escrita - Efetuar o armazenamento em ficheiro a partir dum objeto Players.
Este é apenas um princípio de solução. Espera-se que o programa cresça no futuro.
Neste miniprograma, chamamos a atenção para o seguinte:

Este é um programa com poucas pretensões. Iremos melhorá-lo na secção seguinte.
A função players_load, em cada iteração do seu ciclo, lê duas linhas e usa-as para criar um jogador que é acrescentado a uma lista.
A função players_store, em cada iteração do seu ciclo, toma um jogador e escreve-o usando duas linhas, para respeitar o formato.
from dataclasses import dataclass

def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

#########   type Player   #########

@dataclass
class Player:
    name: str
    club: str
    goals: int

def player_as_str(p: Player) -> str:
        return f"({p.name},{p.club},{p.goals})"

#########   type Players   #########

@dataclass
class Players:
    players: list[Player]

def players_load(file_name: str) -> Players:
    """ Load the file of players.
        Precondition: file_exists(file_name) """
    f = open(file_name, 'r')
    l = []
    for line1 in f:
        line2 = next(f)
        name, club = line1.split()
        goals = int(line2)
        p = Player(name, club, goals)
        l.append(p)
    f.close()
    return Players(l)

def players_store(file_name: str, ps: Players) -> None:
    """ Store the file of players. """
    f = open(file_name, 'w')
    for p in ps.players:
        print(f"{p.name} {p.club}", file=f)
        print(p.goals, file= f)

#########   User Interface   #########

PLAYERS_FILE_NAME = "jogadores.txt"
PLAYERS_FILE_NAME2 = "jogadores2.txt"

@dataclass
class UI:
    ps: Players

def ui_run(ui: UI) -> None:
    if not (file_exists(PLAYERS_FILE_NAME)):
        print("Não foi possível abrir o ficheiro")
    else:
        ui.ps = players_load(PLAYERS_FILE_NAME)
        print( ui.ps)
        players_store(PLAYERS_FILE_NAME2,  ui.ps)

#########  main   #########

def main() -> None:
    ui = UI(Players([]))
    ui_run(ui)

main()

>>> main()
[(João,Leixões,5), (Tarquínio,Benfica,6), (Pedro,Sporting,3), (Luís,Belenenses,8), (Miguel,Leixões,2), (João,Sporting,1),
(Manuel,Académica,5), (Camões,Porto,5), (Fernando,Benfica,10), (Joaquim,Boavista,7), (Gervásio,Belenenses,7)]
Modelação dum problema com dados em ficheiro
Vamos estender o enunciado do problema anterior com as seguintes funcionalidades:
Escrever o número total de golos marcados pelos jogadores do Leixões.
Escrever o número total de golos marcados pelos jogadores do Benfica.
Escrever o nome e clube dos jogadores que marcaram, pelo menos, cinco golos.
Agora o problema ficou com mais conteúdo. Vamos abordá-lo com mais exigência! Vamos criar uma dataclass Players para representar o grupo dos jogadores e vamos criar uma dataclass UI para a interface com o utilizador.
Para praticar, considere acrescentar ao programa ainda esta outra funcionalidade:

Número de jogadores cujo número de golo difere da média em menos de 1 unidade.
from dataclasses import dataclass

def file_exists(file_name: str) -> bool:
    """ Check if a file can be opened in read mode. """
    try:
       f = open(file_name, 'r')
       f.close()
       return True
    except FileNotFoundError:
       return False

#########   type Player   #########

@dataclass
class Player:
    name: str
    club: str
    goals: int
    
def player_as_str(p: Player) -> str:
    return f"({p.name},{p.club},{p.goals})"

#########   type Players   #########

@dataclass
class Players:
    players: list[Player]

def players_load(file_name: str) -> Players:
    """ Load the file of players.
        Precondition: file_exists(file_name) """
    f = open(file_name, 'r')
    l = []
    for line1 in f:
        line2 = next(f)
        name, club = line1.split()
        goals = int(line2)
        p = Player(name, club, goals)
        l.append(p)
    f.close()
    return Players(l)
    
def players_goals_of_club(ps: Players, club: str) -> int:
    sum = 0
    for p in ps.players:
        if p.club == club:
            sum += p.goals
    return sum

def players_select_players_above_minimum(ps: Players, min_goals: int) -> list[Player]:
    l = []
    for p in ps.players:
        if p.goals >= min_goals:
            l.append(p)
    return l


#########   User Interface   #########

PLAYERS_FILE_NAME = "jogadores.txt"
GOALS_MINIMUM = 5

@dataclass
class UI:
    ps: Players

def ui_create() -> UI:
    """ Precondition: file_exists(PLAYERS_FILE_NAME)  """
    return UI(players_load(PLAYERS_FILE_NAME))

def ui_show_goals_of_club(ui: UI, club: str) -> None:
    n = players_goals_of_club(ui.ps, club)
    print(f"Golos marcados pelo {club} = {n}")

def ui_show_players_above_minimum(ui: UI, min_goals: int) -> None:
    l = players_select_players_above_minimum(ui.ps, min_goals)
    for p in l:
        print(player_as_str(p))

def ui_run(ui: UI) -> None:
    ui_show_goals_of_club(ui, "Leixões")
    print()
    ui_show_goals_of_club(ui, "Benfica")
    print()
    ui_show_players_above_minimum(ui, GOALS_MINIMUM)

def main() -> None:
    if not (file_exists(PLAYERS_FILE_NAME)):
        print("Não foi possível abrir o ficheiro")
    else:
        ui = ui_create()
        ui_run(ui)

main()

>>> main()
Golos marcados pelo Leixões = 7

Golos marcados pelo Benfica = 16

(João,Leixões,5)
(Tarquínio,Benfica,6)
(Luís,Belenenses,8)
(Manuel,Académica,5)
(Camões,Porto,5)
(Fernando,Benfica,10)
(Joaquim,Boavista,7)
(Gervásio,Belenenses,7)
#20






Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 08b (04/nov/2026)
Identificação dos elementos necessários para a resolução dum problema de programação.
Técnicas de desenvolvimento de programas: top-down e bottom-up.

Identificação dos elementos necessários para a resolução dum problema de programação
Identificar quais os elementos que convém programar, é uma das partes mais importantes da programação.

O objetivo é encontrar uma solução simples, clara e natural. Uma solução que seja fácil de perceber e fácil ser modificada no futuro.

No caso dum programa baseado em funções, o problema consiste em identificar os tipos de dados que vão ser usados e identificar um conjunto de boas funções. Cada função deve ter uma missão específica e não ter duas ou mais missões simultâneas.

Problema do Parque de Estacionamento
Desenvolva um programa que calcule o preço a pagar no parque de estacionamento subterrâneo dos Restauradores, em Lisboa, a partir dos seguintes dados fornecidos pelo utilizador:
Hora de entrada;
Minuto de entrada;
Hora de saída;
Minuto de saída.
Em Março de 1999, a tabela de preços (originalmente em escudos, mas aqui já convertida para cêntimos) era a seguinte:
1ª hora        120 cêntimos
2ª hora        140 cêntimos
3ª hora        150 cêntimos
seguintes      155 cêntimos
Mais informação:
As horas acumulam. Por exemplo, um carro que esteja no parque 2 horas paga 120+140 cêntimos.
Os tempos de permanência são arredondados para horas completas e sempre para cima. Por exemplo, um carro que permaneça no parque uma hora e um minuto paga duas horas completas.
Assuma que o carro nunca está no parque mais do que 24:00. Se o momento de entrada for, por exemplo, 12:30 e o momento de saída 10:31 (do dia seguinte), então há a pagar 23 horas (o custo é 3510 cêntimos).
Procure fazer um programa claro e fácil de compreender. Para isso defina diversas funções pequenas, cada qual com uma tarefa bem determinada.
Este é um problema sobre números inteiros. Não use números reais. Além disso, repare que o programa fica mais simples de escrever se começar por converter para minutos o tempo de entrada e o tempo de saída.

NOTA - Problema adaptado de "Pedro Guerreiro, Pascal - Técnicas de Programação, Publicações Europa-América, 1988".

Discussão
Para começar, convém reconhecer que este é um programa que pode ser resolvido usando o estilo procedimental, i.e. usando funções. O problema é pequeno e não há aqui entidades que requeiram o esforço de desenvolver uma ou mais classes. [Mesmo assim, a discussão que estamos a fazer também é útil no contexto das classes, porque este problema poderia corresponder a uma funcionalidade dum programa maior envolvendo classes, por exemplo um sistema de gestão de parques de estacionamento.]
Também devemos reconhecer que este é um problema que se resolve bem usando apenas números inteiros. Não há vantagem em usar números reais.

Para representar tempos, vamos usar simplesmente dois inteiros. Vamos definir já algumas constantes e uma função para validar um tempo:

MINUTES_IN_HOUR = 60
HOURS_IN_DAY = 24
MINUTES_IN_DAY = HOURS_IN_DAY * MINUTES_IN_HOUR

def valid_time(h: int, m: int) -> bool:
    """ Is the time valid? """
    return 0 <= h < HOURS_IN_DAY and 0 <= m < MINUTES_IN_HOUR
Uma questão que se levanta é: em que unidades vamos fazer as contas com tempos? Vamos trabalhar diretamente com tempos ou será que devemos converter previamente tudo para minutos? Podemos experimentar, mas se tentarmos programar uma função para calcular o tempo de permanência usando tempos veremos que o código começa a ficar muito complicado, cheio de casos particulares. Usar tempos em minutos, torna tudo mais simples.

Já identificámos as nossas duas primeiras funções:

Converte tempos para minutos:
def convert_to_minutes(h: int, m: int) -> int:
Calcula o tempo de permanência em minutos:
def minutes_of_stay(entrance_in_minutes: int, exit_in_minutes: int) -> int:
Mas, neste parque só se pagam horas inteiras. Por isso temos de converter o número de minutos de presença para horas, arredondando para cima. Também é necessária uma função para calcular o valor a pagar com base no número inteiro de horas. Temos assim mais duas funções:
Arredonda minutos para horas:
def round_up_to_hours(minutes: int) -> int:
Calcula a quantia a pagar, a partir do número de horas de presença:
def price(hours: int) -> int:
Repare que temos estado a procurar identificar funções tão simples quanto possível, e essa simplicidade resulta em parte dos parâmetros das funções representarem o conceito que nos dá "mais jeito". Por exemplo, decidimos que a função hours recebe o número de horas a pagar.

Repare também que cada uma das nossas funções tem uma tarefa única e bem definida, e que isso contribui para a clareza do programa.

Vamos agora escrever a função que resolve a parte principal do nosso problema, usando as funções anteriores. Esta função também tem uma única tarefa: resolver o problema global, ligando todas as funções anteriores. Leia o código e siga a sequência do raciocínio:

def parking(he: int, me: int, hs: int, ms: int) -> int:
    """ How much to pay, given the entrance and the exit times?
        Precondition: valid_time(he, me) and valid_time(hs, ms)
    """
    entrance = convert_to_minutes(he, me)
    exit = convert_to_minutes(hs, ms)
    stay_minutes = minutes_of_stay(entrance, exit)
    stay_hours = round_up_to_hours(stay_minutes)
    return price(stay_hours)
Repare como se usam variáveis suplementares para se guardarem os resultados produzidos por algumas funções. Os resultados ficam assim disponíveis para poderem ser usados um pouco mais à frente, como argumentos de outras funções.
Complete o programa:

MINUTES_IN_HOUR = 60
HOURS_IN_DAY = 24
MINUTES_IN_DAY = HOURS_IN_DAY * MINUTES_IN_HOUR

def valid_time(h: int, m: int) -> bool:
    """ Is the time valid? """
    return 0 <= h < HOURS_IN_DAY and 0 <= m < MINUTES_IN_HOUR
 
def convert_to_minutes(h: int, m: int) -> int:
    """ Convert a time to minutes.
        Precondition: valid_time(time)
    """
    # FAZER
    pass

def minutes_of_stay(entrance_in_minutes: int, exit_in_minutes: int) -> int:
    """ Calculate the stay from the entrance and the exit times given in minutes.
        Precondition: 0 < entrance_in_minutes <= MINUTES_IN_DAY
                  and 0 < exit_in_minutes <= MINUTES_IN_DAY
    
    """
    # FAZER
    pass

def round_up_to_hours(minutes: int) -> int:
    """ Round up minutes to hours.
        Precondition: 0 < minutes <= MINUTES_IN_DAY
    """
    # FAZER
    pass

def price(hours: int) -> int:
    """ How much to pay, given the hours of stay?
        Precondition: 0 < hours <= HOURS_IN_DAY
    """
    # FAZER
    pass

def parking(he: int, me: int, hs: int, ms: int) -> int:
    """ How much to pay, given the entrance and the exit times?
        Precondition: valid_time(he, me) and valid_time(hs, ms)
    """
    entrance = convert_to_minutes(he, me)
    exit = convert_to_minutes(hs, ms)
    stay_minutes = minutes_of_stay(entrance, exit)
    stay_hours = round_up_to_hours(stay_minutes)
    return price(stay_hours)

def main() -> None:
    """ User interface. """
    he = int(input("Introduza a hora de entrada: "))
    me = int(input("Introduza o minuto de entrada: "))
    hs = int(input("Introduza a hora de saída: "))
    ms = int(input("Introduza o minuto de saída: "))
    if not (valid_time(he, me) and valid_time(hs, ms)):
        print("Dados inválidos.")
    else:
        price = parking(he, me, hs, ms)
        print(price)

main()
Testes
Para nos convencermos de que o programa está correto, convém testá-lo bem. O problema está cheio de subtilezas nos arredondamentos e na mudança de dia.
Eis um conjunto de testes que considera alguns casos normais e alguns casos de fronteira:

não muda o dia - testam-se as contas incluindo arredondamentos:
    06:30   18:35
    06:30   18:25
    06:30   18:30 

muda o dia - testam-se as contas incluindo arredondamentos:
    18:25   06:30
    18:35   06:30
    18:30   06:30

casos de fronteira:
    09:00   09:00  (24 horas)
    09:00   09:01  (1 minuto)
    09:59   10:00  (1 minuto)
Técnicas de desenvolvimento de programas: top-down e bottom-up
Usando este Problema do Parque de Estacionamento, vamos discutir brevemente duas formas diferentes que o programador tem para organizar as suas ideias por forma a conseguir programar problemas não triviais. Cada uma das duas técnicas pode ser usada isoladamente, mas também podem ser usadas misturadas, dependendo da vontade e da inspiração do programador:
Top-down e Bottom-up
Técnica bottom-up (construção da base para o topo)
Perante um problema com algum complexidade, podemos abordá-lo de forma controlada e organizada da seguinte forma:
Começamos por identificar os elementos mais simples do problema e programamos primeiro as funções correspondentes.
Depois usamos essas funções simples como elementos que ajudam a escrever outras funções mais ambiciosas. Cria-se assim um segundo nível de funções que dependem das funções do nível anterior.
Prosseguindo sempre da mesma forma, o programa vai ganhando forma a pouco e pouco. A função principal é a última função que se escreve!
Mesmo que o problema a programar seja complexo e caso o programador tenha dificuldade em abarcar todos os detalhes ao mesmo tempo, este processo sistemático de escrita do programa orienta os esforços do programador que assim não precisa de ficar confuso.

Exemplo
O problema do Parque de Estacionamento foi resolvido usando a estratégia bottom-up. Primeiro foram identificadas as operações mais básicas, e só depois destas estarem decididas (e também testadas) é que se avançou para as funções do nível seguinte.

Técnica top-down (técnica de refinamentos sucessivos)
Perante um problema com algum complexidade, podemos abordá-lo de forma controlada e organizada, usando a estratégia contrária da técnica top-down:
Começamos por escrever a função principal do problema, tentando logo decompor o problema completo em subproblemas um pouco mais pequenos. Assim, no corpo da função principal escrevemos chamadas a diversas funções que ainda não foram definidas, sendo que cada uma dessas funções corresponde a um dos subproblemas identificados.
Depois atacamos cada um dos subproblemas de cada vez e escrevemos a função correspondente a cada um deles. Para começar a resolver cada subproblema, podemos usar a mesma receita já usada na função main, ou seja, podemos decompor esse subproblema em subproblemas mais pequenos.
Prosseguindo sempre da mesma forma, o problema geral vai sendo progressivamente refinado, cada vez com mais detalhes.
Mesmo que o problema a programar seja complexo e mesmo que o programador tenha dificuldade em abarcar todos os detalhes ao mesmo tempo, este processo sistemático de escrita do programa orienta os esforços do programador que assim não precisa de ficar confuso.

Exemplo
Para exemplificar, vejamos como pode ser atacado o problema do parque de estacionamento usando a técnica top-down. Começamos por tentar escrever a função main.
def valid_time(h: int, m: int) -> bool:
    """ Is the time valid? """
    pass

def parking(he: int, me: int, hs: int, ms: int) -> int:
    """ How much to pay, given the entrance and the exit times?
        Precondition: valid_time(he, me) and valid_time(hs, ms)
    """
    pass

def main() -> None:
    """ User interface. """
    he = int(input("Introduza a hora de entrada: "))
    me = int(input("Introduza o minuto de entrada: "))
    hs = int(input("Introduza a hora de saída: "))
    ms = int(input("Introduza o minuto de saída: "))
    if not valid_time(he, me) and valid_time(hs, ms):
        print("Dados inválidos.")
    else:
        price = parking(he, me, hs, ms)
        print(price)
No código anterior, fomos já escrevendo, com corpo vazio, as funções que são chamadas a partir de main.
No passo seguinte, escrevemos as funções incompletas, as quais poderão ser expressas à custa de outras funções ainda mais simples.

Continuamos a atuar desta forma até o programa ficar completo. Em cada passo, inventamos e chamamos novas funções que depois será necessário programar.

Reutilização e legibilidade
Como se disse logo no início, muitas vezes estas duas técnicas são usadas misturadas.
Alguns programadores acham mais intuitiva a técnica top-down e não há mal nenhum nisso. Mas a técnica bottom-up tem algumas vantagens:

Podemos ir testando as funções à medida que as escrevemos. [Com a técnica top-down, só conseguimos testar as funções depois de todo o programa estar escrito.]
Na técnica bottom-up, a maioria das funções são imaginadas e escritas fora dum contexto particular de utilização, pelo que costumam resultar mais gerais e mais fáceis de utilizar noutros programas, ou seja, são mais fáceis de reutilizar.
Enfatizando o segundo ponto, a técnica top-down pode convidar a definir funções que só fazem sentido no contexto que circunda a chamada - essas funções podem tornar-se estranhas e difíceis de entender quando são alvo de análise fora do contexto original.

