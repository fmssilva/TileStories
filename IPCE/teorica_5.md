
Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 05a (14/out/2026)
Números complexos.
Tipo de None.
Carateres.
Números reais.

Tipo complexo
O Python suporta números complexos.
Definição
Para descrever completamente o tipo dos complexos temos de indicar:

O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos booleanos
Valores matemáticos
O conjunto matemático dos complexos costuma ser assim definido:
ℂ = {a+bi: a ∈ ℝ, b ∈ ℝ}
Literais
Nos literais complexos do Python, o símbolo da unidade imaginária i foi trocado por j, porque o i se confunde com a letra l e com o algarismo 1. Eis alguns exemplos de literais complexos:
1j
0j
12.3e56 + 14.5j
1.0 + 1.0j
Operações
Eis as operações mais importantes sobre complexos:
Operador	Descrição	Associatividade
z.real z.imag	Acesso	-
**	Expoente	direita
* / // %	Multiplicação / Divisões / Resto	esquerda
+ -	Adição / Subtração	esquerda
complex	Função de conversão	
Se tivermos dois valores reais nas variáveis a e b, não se pode escrever a+bj. Esta notação só se aplica a literais. Usando variáveis, temos de usar a operação de conversão complex, que tem dois argumentos. Veja um exemplo:
>>> q = 1+2j
>>> print(q)
(1+2j)
>>> a=1
>>> b=2
>>> z = a+bj
Traceback (most recent call last):
  File "", line 1, in 
NameError: name 'bj' is not defined. Did you mean: 'b'?
>>> z = complex(a,b)
>>> print(z)
(1+2j)
A biblioteca math só se aplica a números reais, mas existe outra biblioteca, chamada cmath, com operações matemáticas sobre complexos:
>>> import cmath
>>> cmath.sqrt(-1)
1j
>>> cmath.polar(1j)                          # número complexo -> coordenadas polares   
(1.0, 1.5707963267948966)
>>> cmath.rect(1.0, 1.5707963267948966)      # coord. polares -> coord. retangulares
(6.123233995736766e-17+1j)
Tipo None
O tipo None do Python é muito curioso, porque contém um único valor, o valor None. Compare com o tipo bool, que contém dois valores: False e True.
Um facto que se estuda na matemática é que não é possível representar informação através dum único valor. Por exemplo, com o tipo booleano conseguimos distinguir entre duas situações. Mas, com o tipo None não conseguimos distinguir entre nada.

Desta forma, o tipo None é usado para representar ausência de informação. Serve por exemplo, para indicar que uma variável não tem valor (se lhe atribuirmos o valor None) ou que uma função não tem resultado.

Definição
Vamos então à definição do tipo None. Para descrever completamente o tipo None temos de indicar:

O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos booleanos
Valores matemáticos
Em matemática, o chamado tipo unitário é um conjunto qualquer com cardinalidade 1. Por exemplo:
{{}}
Literais
Em Python, o valor do tipo unitário é escrito usando o seguinte literal:

None
Operações
Vejamos agora as operações.

As únicas operações disponíveis são ==, !=, type.

Veja estes exemplos de utilização:

>>> x = None      # atribuição (x recebe None))
>>> x == None     # igualdade
True
>>> x != None     # desigualdade
False
>>> type(x)       # tipo
<class 'NoneType'>
Funções sem resultado
Em Python, o tipo None é principalmente usado para exprimir a ausência de resultado duma função.
De forma geral, consideram-se duas grandes categorias de funções:

Funções com resultado. Por exemplo, eis uma função com resultado inteiro:
def add(a: int, b: int) -> int:
    """ Add two integers. """
    return a + b
Funções sem resultado. Estas funções usam None para indicar que não há resultado. Por exemplo:
def print6(f: float) -> None:
    """ Print a real with 6 decimal places. """
    print(f"{f:.6f}")
A forma como se invoca cada um dos dois tipos de funções é bastante diferente:

Quando se chama uma função com resultado, normalmente o resultado é usado para alguma coisa: é guardado numa variável, é escrito no ecrã, etc. Por exemplo:
>>> x = add(1,2)
>>> print(x)
3
Quando se chama uma função sem resultado, chama-se diretamente a função, ignorando o resultado (que não interessa, por ser None): Por exemplo:
>>> print6(34.6547654634)
34.654765
>>> print6(0)
0.000000
A maioria das funções None conseguem garantir que não há resultado, simplesmente evitando usar a instrução return.

Mas, e se uma função sem resultado desejar terminar imediatamente a meio dum ciclo? Nesse caso, o que se costuma fazer é usar a instrução return sem qualquer valor (porque a função não retorna nada). Também é possível escrever return None. Exemplo:

def print_first_perfect_number(a: int, b: int) -> None:
    """ Print the first perfect number in the integer  interval [a, b] """
    for i in range(a, b+1, 1):
        if is_perfect(i):
            print(i)            # escreve o primeiro número perfeito encontrado
            return
    print("Não há números perfeitos no intervalo indicado")
Tipo dos carateres
Em Python, os carateres são representados usando strings de comprimento 1. Por exemplo, a primeira letra do alfabeto latino, representa-se assim em minúsculas: 'a' ou "a" e, como se vê, podemos usar plicas ou aspas.
Um caráter pode ser, por exemplo, uma letra, um dígito, um sinal de pontuação, uma tabulação (tab), um espaço, uma mudança de linha, etc.

Apesar de não haver um tipo específico, os carateres (strings de tamanho 1) têm operações especiais, e por isso vamos tratá-los como se constituíssem um tipo à parte.

Definição
Para descrever completamente o tipo dos carateres temos de indicar:

O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos booleanos
Valores matemáticos
O Python usa o standard Unicode, criado em 1991, que associa um código numérico a cada caráter. Atualmente os códigos são representados por valores inteiros não negativos de 21 bits.
Por isso podemos dizer que a essência dos carateres pode ser capturada matematicamente pelo conjunto dos valores inteiros não negativos de 21 bits.

Literais
Os literais carateres são em quantidade imensa, pois o Unicode suporta a maioria dos sistemas de escrita da humanidade, usados atualmente e no passado.

Alguns exemplos de carateres literais:

' ' '!' '"' '#' '$' '%' '&' ''' '(' ')' '*' '+' ',' '-' '.' '/' '0' '1' '2' '3' '4' '5' '6' '7' '8' '9'
':' ';' '<' '=' '>' '?' '@' 'A' 'B' 'C' 'D' 'E' 'F' 'G' 'H' 'I' 'J' 'K' 'L' 'M' 'N' 'O' 'P' 'Q' 'R' 'S'
'T' 'U' 'V' 'W' 'X' 'Y' 'Z' '[' '\' ']' '^' '_' '`' 'a' 'b' 'c' 'd' 'e' 'f' 'g' 'h' 'i' 'j' 'k' 'l' 'm'
'n' 'o' 'p' 'q' 'r' 's' 't' 'u' 'v' 'w' 'x' 'y' 'z' '{' '|' '}' '~'

'ぁ' 'あ' 'ぃ' 'い' 'ぅ' 'う' 'ぇ' 'え' 'ぉ' 'お' 'か' 'が' 'き' 'ぎ' 'く' 'ぐ' 'け' 'げ' 'こ' 'ご' 'さ' 'ざ'
'し' 'じ' 'す' 'ず' 'せ' 'ぜ' 'そ' 'ぞ' 'た' 'だ' 'ち' 'ぢ' 'っ' 'つ' 'づ' 'て' 'で' 'と' 'ど' 'な' 'に' 'ぬ'
'ね' 'の' 'は' 'ば' 'ぱ' 'ひ' 'び' 'ぴ' 'ふ' 'ぶ' 'ぷ' 'へ' 'べ' 'ぺ' 'ほ' 'ぼ' 'ぽ' 'ま' 'み' 'む' 'め'
'も' 'ゃ' 'や' 'ゅ' 'ゆ' 'ょ' 'よ' 'ら' 'り' 'る' 'れ' 'ろ' 'ゎ' 'わ' 'ゐ' 'ゑ' 'を' 'ん' 'ゔ' 'ゕ' 'ゖ'

'一' '丁' '丂' '七' '丄' '丅' '丆' '万' '丈' '三' '上' '下' '丌' '不' '与' '丏' '丐' '丑' '丒' '专' '且'
'丕' '世' '丗' '丘' '丙' '业' '丛' '东' '丝' '丞' '丟' '丠' '両' '丢' '丣' '两' '严' '並' '丧' '丨' '丩'
'个' '丫' '丬' '中' '丮' '丯' '丰' '丱' '串' '丳' '临' '丵' '丶' '丷' '丸' '丹' '为' '主' '丼' '丽' '举'
'丿' '乀' '乁' '乂' '乃' '乄' '久' '乆' '乇' '么' '义' '乊' '之' '乌' '乍' '乎' '乏' '乐' '乑' '乒' '乓'
'乔' '乕' '乖' '乗' '乘' '乙' '乚' '乛' '乜' '九' '乞' '也' '习' '乡' '乢' '乣' '乤' '乥' '书' '乧' '乨'
 '乩' '乪' '乫' '乬' '乭' '乮' '乯' '买' '乱' '乲' '乳' '乴' '乵' '乶' '乷' '乸' '乹' '乺' '乻' '乼' '乽'

'😀' '😁' '😂' '😃' '😄' '😅' '😆' '😇' '😈' '😉' '😊' '😋' '😌' '😍' '😎' '😏' '😐' '😑' '😒' '😓' '😔'
'😕' '😖' '😗' '😘' '😙' '😚' '😛' '😜' '😝' '😞' '😟' '😠' '😡' '😢' '😣' '😤' '😥' '😦' '😧' '😨' '😩' '😪'
'😫' '😭' '😮' '😯' '😰' '😱' '😲' '😳' '😴' '😵' '😶' '😷' '😸' '😹' '😺' '😻' '😼' '😽' '😾' '😿' '🙀' 
Operações
Há duas operações que são exclusivas dos carateres (ou seja das strings de comprimento 1):
Operador	Descrição
chr	Converte código numérico em caráter
ord	Converte caráter em código numérico
Exemplos de uso:
>>> ord('A')
65
>>> chr(65)
'A'
>>> ord('Z')
90
>>> ord('AB')
Traceback (most recent call last):
  File "", line 1, in 
TypeError: ord() expected a character, but string of length 2 found
ASCII - American Standard Code for Information Interchange
A parte do Unicode dedicada ao alfabeto latino e sinais de pontuação equivale ao clássico padrão ASCII de 1968. Os carateres ASCII são representados por inteiros entre 0 e 127.
Veja a tabela completa:

    0 NUL    16 DLE    32      48 0    64 @    80 P    96 `   112 p 
    1 SOH    17 DC1    33 !    49 1    65 A    81 Q    97 a   113 q 
    2 STX    18 DC2    34 "    50 2    66 B    82 R    98 b   114 r 
    3 ETX    19 DC3    35 #    51 3    67 C    83 S    99 c   115 s 
    4 EOT    20 DC4    36 $    52 4    68 D    84 T   100 d   116 t 
    5 ENQ    21 NAK    37 %    53 5    69 E    85 U   101 e   117 u 
    6 ACK    22 SYN    38 &    54 6    70 F    86 V   102 f   118 v 
    7 BEL    23 ETB    39 '    55 7    71 G    87 W   103 g   119 w 
    8 BS     24 CAN    40 (    56 8    72 H    88 X   104 h   120 x 
    9 HT     25 EM     41 )    57 9    73 I    89 Y   105 i   121 y 
   10 LF     26 SUB    42 *    58 :    74 J    90 Z   106 j   122 z 
   11 VT     27 ESC    43 +    59 ;    75 K    91 [   107 k   123 { 
   12 FF     28 FS     44 ,    60 <    76 L    92 \   108 l   124 | 
   13 CR     29 GS     45 -    61 =    77 M    93 ]   109 m   125 } 
   14 SO     30 RS     46 .    62 >    78 N    94 ^   110 n   126 ~ 
   15 SI     31 US     47 /    63 ?    79 O    95 _   111 o   127 DEL 
Há alguns pontos importantes a enfatizar nesta tabela:
Os algarismos aparecem todos de seguida.
As minúsculas aparecem todas de seguida.
As maiúsculas aparecem todas de seguida.
Estas propriedades permitem-nos escrever algumas funções interessantes:
Converter algarismo para caráter
É um bom exercício escrever uma função para fazer isto. Contudo, a função de conversão str já resolve o problema.
def digitToChar(d: int) -> str:
    """ Convert digit to string.
        Precondition: 0 <= d <= 9
    """
    return chr(ord('0') + a)

>>> digitToChar(0)
'0'
>>> digitToChar(5)
'5'
>>> digitToChar(9)
'9'
>>> str(5)    # usa função predefinida
'5'
Converter minúscula para maiúscula
É um bom exercício escrever uma função para fazer isto. Contudo, o método upper, que se aplica a strings de qualquer comprimento, já resolve o problema.
def toUpper(c: str) -> str:
    """ Convert char to upper case.
        Precondition: len(c) == 1 and 'a' <= c <= 'z'
    """
    return chr(ord(c) - ord('a') + ord('A'))

>>> toUpper('d')
'D'
>>> toUpper('z')
'Z'
>>> 'q'.upper()    # usando um método das strings
'Q'
Nesta secção só falámos em carateres. As strings em geral, será o tema inicial da próxima aula.

#






Introdução à Programação para a Ciência e Engenharia (2026/2027)
Aulas teóricas
Artur Miguel Dias
Teórica 05b (14/out/2026)
Strings em Python.

Strings
Já vimos que o Python tem tipos para lidar com informação numérica (usando os tipos int, float e complex), para lidar com informação booleana (usando o tipo bool), para representar carateres individuais (através do tipo str), para exprimir ausência de informação (usando o tipo None), e para representar e lidar com sequências mutáveis (usando o tipo list).
Chegou a altura de discutir como se representa e manipula texto em Python. Isso é feito através do tipo str. Os valores deste tipo são sequências de carateres chamadas de strings. Tal como a maioria dos valores em Python, as strings são imutáveis, ou seja não podem ser alteradas depois de criadas.

Há situações em que o processamento de texto pode ser complexo. Os principais tipos de processamentos que se podem fazer com um sequência de linhas de texto são: analisar a estrutura do texto para extrair dados significativos; transformar o texto noutro texto, de acordo com um conjunto de regras.

Definição
Para descrever completamente o tipo das strings temos de indicar:
O conjunto de valores matemáticos representados
Quais os literais usados
Quais as operações ligadas aos inteiros
Valores matemáticos
A definição matemática das strings obtêm-se combinando a definição matemática das listas com a definição matemática dos carateres.
Em matemática, uma string é uma sequência finita a de comprimento n :

a = a0 a1 a2 ... an 
Os valores da sequência são carateres, modelados usando valores inteiros entre 0 e 221.
O conjunto de todas as sequências finitas de carateres, podemos tentar representá-lo assim:

𝕊fc = {a0 a1 a2 ... an : n ∈ ℕ, ai ∈ [0, 221]}
Literais
Os literais de tipo string são sequências de carateres colocados entre os delimitadores simples: aspas ou plicas. Há ainda uma variante que usa três delimitadores.
Vamos ver um exemplo para cada uma das três possibilidades:

Usar aspas - o literal fica escrito numa única linha:
"I am a string between double quotes!"
Usar plicas - o literal fica escrito numa única linha:
'I am a string between quotes!'
Usar três delimitadores - o literal pode estender-se ao longo de várias linhas:
"""I am a string that spans
across several lines
between triple double quotes!"""

'''I am a string that spans
across several lines
between triple quotes!'''
O caráter especial '\n' indica mudanças de linha. Veja:
>>> s = "I am a string that spans\nacross several lines\nbetween double quotes!"

>>> s
'I am a string that spans\nacross several lines\nbetween double quotes!'

>>> print(s)
I am a string that spans
across several lines
between double quotes!
Quando se usa uma string multilinha (com três delimitadores), o Python converte logo para um literal de uma linha, com ocorrências de '\n'. Veja:
>>> s = """I am a string that spans
across several lines
between triple double quotes!"""

>>> s
'I am a string that spans\nacross several lines\nbetween triple double quotes!'

>>> print(s)
I am a string that spans
across several lines
between triple double quotes!
Limitações:
Numa string entre entre aspas, não se pode colocar um aspa, a não ser que seja escrita desta maneira especial \".
Numa string entre entre plicas, não se pode colocar um plica, a não ser que seja escrita desta maneira especial \'.
Numa string entre entre três aspas, não se podem colocar três aspas, a não ser que sejam escritas desta maneira especial \"\"\".
Numa string entre entre três plicas, não se podem colocar três plicas, a não ser que sejam escritas desta maneira especial \'\'\'.
A string vazia pode ser escrita de quatro maneiras diferentes:
""      ''      """"""      ''''''
Operações
Todas operações das listas que não envolvem modificação estão também disponíveis para serem usadas com strings! As operações das listas estão disponíveis para sequências em geral, e as strings são sequências...
Vejamos alguns exemplos de utilização das operações das sequências:

>>> s = "ola ole "
>>> len(s)
8
>>> s[0]
'o'
>>> s[6]
'e'
>>> s[8]
Traceback (most recent call last):
  File "", line 1, in 
IndexError: string index out of range


>>> 'a' in s
True
>>> 'a' not in s
False
>>> "olazzz" < "ole"
True

>>> list(s)
['o', 'l', 'a', ' ', 'o', 'l', 'e', ' ']

>>> s+s
'ola ole ola ole '

>>> s*5
'ola ole ola ole ola ole ola ole ola ole '

>>> list(enumerate(s))
[(0, 'o'), (1, 'l'), (2, 'a'), (3, ' '), (4, 'o'), (5, 'l'), (6, 'e'), (7, ' ')]

>>> s.index('a')
2

>>> s.count('ol')
2

>>> u = ['O', 'L', 'A', 'O', 'L', 'E']
>>> s = ""
>>> for x in u:
...     s = s + x
... 
>>> s
'OLAOLE'
>>> print(s)
OLAOLE
Mas o Python oferece um vasto conjunto de métodos suplementares, que são especificamente para usar com strings.

Vamos destacar os métodos mais usados:

Operação	Descrição	Tipo
s.split(sep)	decomposição nos pontos onde o separador sep ocorre	str * str -> str -> list[str]
s.splitlines()	decomposição em linhas	str -> list[str]
s.strip()	limpa brancos do início e do final	str -> str
s.rstrip()	limpa brancos do final	str -> str
s.lstrip()	limpa brancos do início	str -> str
s.replace(s1,s2)	troca todas as ocorrências s1 por s2	str * str * str -> str
sep.join(l)	concatena todas as strings da lista l usando sep como separador	str * list[str] -> str
s.islower()	verifica se são tudo minúsculas	str -> bool
s.isupper()	verifica se são tudo maiúsculas	str -> bool
s.isdigit()	verifica se são tudo dígitos	str -> bool
s.isalpha()	verifica se são tudo letras	str -> bool
s.isalnum()	verifica se são tudo letras ou dígitos	str -> bool
s.isspace()	verifica se são tudo carateres brancos (espaço, tab e newline)	str -> bool
a.lower()	converte para minúsculas	str -> str
a.upper()	converte para maiúsculas	str -> str
str	conversão	
Exemplos de uso:
>>> "123 567 345 ola ole".split(' ')
['123', '567', '345', 'ola', 'ole']

>>> """ola
... ole
... oli
... olo
... olu""".splitlines()
['ola', 'ole', 'oli', 'olo', 'olu']

>>> "    texto \n\n   ".strip()
'texto'

>>> "a0b0c0d0e0f".replace('0','1')
'a1b1c1d1e1f'

>>> "".join(["ola", "ole", "oli"])
'olaoleoli'

>>> "-".join(["ola", "ole", "oli"])
'ola-ole-oli'


>>> "abc".islower()
True

>>> "abC".islower()
False

>>> "ABC".isupper()
True

>>> "123".isdigit()
True

>>> "     \t  \n".isspace()
True

>>> "ABc".lower()
'abc'

>>> "aBc".upper()
'ABC'
Exemplo: Identificação de todas as palavras numa string
Basicamente, basta converter para espaço em branco tudo o que não seja um caráter não alfabético. Depois aplicar a função split para separar as palavras:
def get_words(s: str) -> list[str]:
    """ Extract all the word from string. """
    s2 = ''
    for c in s:
        if c.isalpha():
            s2 += c
        else:
            s2 += ' '
    return s2.split()

>>> get_words("Basicamente, basta converter para espaço em branco tudo o que não seja um caráter não alfabético. Depois aplicar a função split para separar as palavras:") 
['Basicamente', 'basta', 'converter', 'para', 'espaço', 'em', 'branco', 'tudo', 'o', 'que',
 'não', 'seja', 'um', 'caráter', 'não', 'alfabético', 'Depois', 'aplicar', 'a', 'função',
 'split', 'para', 'separar', 'as', 'palavras']
Esta solução só não é perfeita porque há palavras em português que usam o hífen, por exemplo "segunda-feira". Resolver isto daria muito trabalho. Para começar, precisariamos duma boa lista de palavras com hífen.
Leitura de valores situados na mesma linha
Usando listas e string, agora conseguimos ler vários valores situados na mesma linha de entrada.
Eis uma função que lê três valores inteiros na mesma linha e escreve a sua soma:

def main() -> None:
    line = input(": ")
    values = line.split(' ')
    v0 = int(values[0])
    v1 = int(values[1])
    v2 = int(values[2])
    print(v0+v1+v2)

>>> main()
: 111 222 333
666
Forma mais compacta de escrever a mesma função, usando atribuição paralela:
def main() -> None:
    values = input(": ").split(' ')
    v0, v1, v2 = int(values[0]), int(values[1]), int(values[2])
    print(v0+v1+v2)

>>> main()
: 111 222 333
666
#