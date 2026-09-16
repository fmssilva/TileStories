
Introdução à Programação para a Ciência e Engenharia (2025/2026)
Lista de exercícios
Artur Miguel Dias
Prática 02a (Semana de 16-22/set/2025)
Primeiros programas. Exercícios de 9 a 15.

A ideia destas primeiras aulas práticas é programar usando como inspiração os exemplos da teórica 02. Não tenha qualquer problema em copiar as ideias desses exemplos. É mesmo isso que se pretende, nesta fase inicial.

Uma das funções da aula teórica 2 foi copiada para aqui, para o ajudar a ganhar inspiração para resolução dos problemas desta aula:

Solução do problema do cubo
def cube(x: int) -> int:
    """ Cube of integer value. """
    return x * x * x

def main() -> None:
    i = int(input("Introduza um valor inteiro: "))
    print(f"O cubo de {i} é {cube(i)}.")

main()
9 - Como sabe, o som propaga-se no ar à velocidade de 340m/s. Escreva um programa que determine a que distância se encontra uma trovoada.
A entrada do programa é o número de segundos que separam o momento do relâmpago do momento do trovão.

Nas contas, use só números inteiros e apresente o resultado em metros.

[Quando não é especificado, o formato do output é livre.]

C : 10 - Escreva um programa que receba três números inteiros correspondentes a um número de horas, minutos e segundos e converta esse período de tempo para segundos.
A função que calcula o resultado terá naturalmente três parâmetros.

Nas contas, use só números inteiros.

Exemplo de execução
Horas: 1
Minutos: 1
Segundos: 1
3661
11 - Escreva um programa que receba uma duração em segundos e escreva esse tempo sob a forma de horas, minutos e segundos. Por exemplo, 1000000 segundos equivalem a 277 horas, 46 minutos e 40 segundos.
Para lá da função main, escreva mais três funções: uma para extrair o número de segundos duma duração (um valor até 59); outra para extrair o número de minutos (outro valor até 59); e outra para extrair o número de horas (um valor sem limite). Apresente os resultados no formato que quiser.

Nas contas, use só números inteiros. As contas obrigam a pensar um pouco...

Em Python, a divisão inteira é denotada pelo operador // e o resto da divisão pelo operador %. Por exemplo:

81//60 = 1
81%60 = 21

12 - Um pêndulo simples realiza um movimento periódico constante. Chama-se período ao tempo que demora a executar um ciclo completo envolvendo uma oscilação para a esquerda e uma oscilação para a direita.
O período T dum pêndulo simples depende do comprimento do fio (L) e da aceleração local da gravidade (g = 9.8 m/s2). Para pequenas amplitudes de oscilação, a fórmula que permite calcular o período é relativamente simples. Procure a fórmula na Net.

Escreva um programa que receba o valor de L e escreva o período do pêndulo. Acha que o programa deve usar números inteiros ou números reais?

D : 13 - Considere um paralelepípedo retângulo (ou seja, um ortoedro) de comprimento a, largura b e altura c. Usando só números reais, escreva um programa com várias funções que produza a seguinte informação:
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
14 - Escreva um programa que permita determinar a altura dum precipício a partir do número de segundos que demora a ver-se uma pedra a chegar lá a baixo. É o observador que deixa cair a pedra.
Assuma que a aceleração da gravidade no local da experiência é a=9.8m/s2. Saiba também que a fórmula que dá a distância percorrida em função do tempo é d=0.5*a*t2.

Faça com que o seu programa leia um número inteiro (representando segundos), mas responda com um número real (representando metros).

15 - Escreva um programa que permita determinar a altura dum precipício a partir do número de segundos que demora a ouvir-se uma pedra chegar lá abaixo. É o observador que deixa cair a pedra.
Este problema é semelhante ao exercício 13, simplesmente o sentido da visão é substituído pelo sentido da audição.

Comentário: Este problema obriga a pensar bastante mais. A melhor abordagem consiste em escrever um sistema de equações e resolvê-lo em ordem à variável "distância". Só depois podemos escrever o programa. Repare que "programar" não é só escrever código. Resolver o problema também faz parte da tarefa do programador.

