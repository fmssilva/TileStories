# `Aula 5 - IPCE` (guia de sessão da aula prática)
Sou professor das aulas práticas do turno **P1**, ano letivo 2026/2027. A prática do P1 é dada **antes** da teórica da mesma matéria (que é à tarde) — por isso as coisas têm de ser explicadas de forma simples e clara para que mesmo quem não sabe nada consiga acompanhar

## Alguns factos da aula: 
- Ferramentas da cadeira: **Miniconda + Spyder** (IDE), **Mooshak**  (exercícios/avaliação automática, precisa de Eduroam/VPN)
- Aula prática de 2:50 horas seguidas, entre as 10:10 e as 13:00 (é bom então pensar 1 intervalo maior de 20 minutos a meio para a turma descansar) 
- Aula prática começa às 10:10
  
## Agora vamos preparar a `aula 5` (guiões `5a` e `5b`) - Envio agora em anexo: 
* Envio em anexo as aulas teoricas dadas até agora. Reparar que nem sempre a aula teórica bate certo com a prática. Por vezes a prática aborda conceitos ainda praticamente não dados na teorica... por isso não podemos assumir que sabem coisas e dizer: "como viram na teorica". 
* Envio também o guião da aula prática que é o guião que eu devo seguir na aula pratica `5a e 5b - 5b não existe esta semana, só 5a` (como vês é um guia bem feito e completo e basicamente a aula podia ser eu dizer "leiam este guia de exercícios e façam e qualquer dúvida perguntem", mas pronto, quero ser um professor de qualidade que realmente sabe ensinar e cativar a aula e ir além desses guias práticos!). 
* Envio também as soluções dos exercícios (repara que envio as soluções de todos os exercícios da cadeira então confirma os exercicios em concretos desta aula prática) `e envio um doc só para as soluções do exercicio grande do 5a`.
* Envio também uns exemplos de testes dos anos anteriores (é para este nível de dificuldade ou superior que devo preparar os alunos)
* Envio também o exemplo de plano guia de sessão da última aula `aula_4.py` e tambem a `aula_3.py` em que ensinamos o "for" então agora podemos cntinaur essa linha para ensinar as listas... ?? 
* Envio também uns ficheiros de revisões com exercicios bem de forma progressiva para preprar alunos para o teste. Podemos repetir alguns desses exercicios na aula se for util. 
* Envio também umas notas de "revisões / conslidar que pdemos dar no inicio da aula 5???
 

## Estilo de ensino (importante manter)
- Dar prioridade a prática e eles experimentarem em vez de teoria formal. Aprender fazendo.
- Espírito leve e cativante — nunca "matar" a motivação com excesso de rigor ou material difícil/pouco intuitivo. Exemplo explicar todos os conceitos e detalhes importantes, mas dados de forma faseada e de forma leve e intuitiva como se estivesse a explicar a crianças do 8o ano, mas de forma a quee elees realmente percebam e estejam depois preparados para fazer exercicios dificeis nos testes (vê o exemplo de testes e exames dos anos anteriores - eles têm de aprender tudo de forma simples, mas temos que saber fazer exercicios dificeis!!!). 

## Filosofia pedagógica — a regra mais importante de todas
Aprendida da pior forma (uma aula que correu mal): nunca explicar um conceito sem primeiro pôr os alunos a programar algo simples relacionado com ele. Fluxo obrigatório por tópico/conceito:
1. Apresentar exemplos simples para mostrar as regras de syntax de uma ferramenta nova. exemplos simples com "for" só a imprimir para ver o range a funcionar, exemplos simples de print(1 in [1,2,3]) só pra ver como in funciona, etc.
2. Apresentar um ou mais exercícios mais simples sobre os conceitos a explicar - Eu explico o exercício e faço-os pensar nas coisas e digo para eles tentarem resolver: a) começo por mostrar só os cabeçalhos das funções que podem usar para resolver o exercício; b) uns minutos depois mostro o código (ou vou revelando aos poucos) e deixo eles acabarem e copiar mesmo a solução do projetor e correrem aquilo; 
3. Só depois de praticarem os exercícios necessários para prceberem o básico desse conceito a ensinar, então aí sim explicar pormenores e detalhes importantes sobre esse conceito, exemplo para o código funcionar melhor, ou ser mais simples ou bem organizado, ou mais eficiente, ou para se evitar cometer erros típicos que alunos cometem, ou para apresentar exercícios típicos de testes, etc... e consoante o tempo disponível para apresentar cada um destes pontos pode haver código e exercícios simples para demonstrar e alguns eu posso só mostrar e outros mando fazer também etc...
4. Depois mais alguns exeercícios finais para consolidar, em que aqui já não digo nada no início, passado uns minutos explico a ideia geral no quadro, depois vou mostrando código aos poucos... 

### A escada: explicação sempre intercalada com prática
Dentro de cada ferramenta nova, a explicação nunca vem em bloco. Sobe-se por degraus:
1. A regra MAIS básica (uma só ideia de syntax), em 2 ou 3 linhas de comentário.
2. Logo a seguir, 2 ou 3 mini-exercícios muito simples só para testar essa syntax
   (cada um com 1 a 3 linhas de código, cada um na sua célula).
3. O detalhe de syntax seguinte (outra ideia só) + mais 2 ou 3 mini-exercícios.
4. Repetir até terem as peças todas. Depois: exercícios normais (guião), que juntam as peças.
5. Só NO FIM (depois de vários exercícios normais resolvidos): os pormenores, edge cases,
   "o que está a acontecer por dentro" e as armadilhas.
- Regra prática: nunca mais de ~5 min de explicação seguida sem os alunos escreverem e correrem código.
- Uma célula com várias ferramentas novas de uma vez (ex: troca de valor + append + pop + "métodos" + "mutável")
  é sinal de que falta partir em degraus.
- Exemplo (listas, aula 5). Ordem certa:
    criar a lista, len e l[i] -> mini-exercícios (o 1º, o último, trocar um valor)
    -> for pelos valores -> mini-exercício (imprimir, somar)
    -> append a partir de [] -> mini-exercício (lista dos quadrados de 1 a 5)
    -> for pelos índices -> mini-exercício
    -> exercícios do guião
    -> só no fim: duas variáveis para a mesma lista, funções que mudam listas,
       + vs append, cópias, lista vazia e IndexError.
  O que correu mal na aula 5: as referências, o "+ vs append" e as funções que mudam listas
  vieram logo a seguir ao 1º exercício, antes de os alunos terem consolidado o básico.


Este é o flow normal que se deve seguir nas aulas - primeiro prática com demonstração e eles realmente meterem código a correr. Só depois explicar conceitos. E ao explicar conceitos:
- Nunca matar a motivação com excesso de rigor ou material difícil/pouco intuitivo cedo demais — explicar como a alguém do 8º ano, mas de forma fundamentada, para que fiquem preparados para exercícios difíceis de teste.
- Onde fizer sentido, fundamentar os conceitos em ideias sólidas por trás (ex: comparadores/and-or-not/if como camadas de lógica proposicional) — mas em linguagem simples, sem jargão académico pesado (nunca dizer "First-Order Logic" aos alunos, por exemplo).
- Ligar sempre que possível a exercícios reais de testes/exames anteriores, para calibrar dificuldade e mostrar relevância direta.




## Estrutura e Formato do Ficheiro a produzir
Isto é um ficheiro que basicamente eu vou mostrando na aula e no fim partilho com os alunos. Então não quero que escrevas notas de ti claude para mim professor a dizer como explicar, mas sim quero que escrevas um documento como se fosse quase um programa normal em código constituido com várias funções e com muito bons comentários e explicações a explicar o que está a acontecer e conceitos e técnicas importantes, de forma simples e natural e informal e clara, como se fosse um coder guy a escrevr notas para outro coder guy. 
- Faz um ficheiro `*.py`, estilo Spyder, com células `# %%` para ser fácil correr um exercício ou demo de cada vez 
- Mantém linguagem deste guia de sessão concisa e simples e informal, MAS bem explicado com todos os detalhes importantes, MAS de forma progressiva - lembra-te que primeiro fazemos exercícios práticos para explicar os conceitos básicos sobre como as coisas funcionam, logo com eles a programar e escrevr esse código desses exercícios. Só depois então explicamos os conceitos em mais profundidade quase como a fazer um apanhado dos detalhes que eles já viram mas agora a falar claramente deles e explicar conceitos etc e explicar técnicas, boas práticas de fazer melhor codigo mais limpo simples e mais eficiente, erros a evitar etc... e depois então mais uns exercícios para consolidar... 
- Nos exercicios que são mesmo do guião da aula, identifica-os da forma correta para ser claro os exercicios inventados por mim para completar a aula dos exercicios do guião da aula mesmo que temos que seguir.
- Se for um momento oportuno podemos introduzir algum conceito que é bom eles saberem para o teste ou para cultura geral... para os motivar e eles sentirem que ficam a saber mais que os outros... exemplo conceitos como tipagem forte ou fraca, dinamico ou estatico, scope de variaveis e passar dados by reference ou by value (nos termos de python), etc... conceitos que um bom programador devia saber... e para a tornar a aula mais rica e interessante MAS LEMBRA-TE QUE ISTO É "INTRODUÇÃO À PROGRAMAÇÃO" - Quero entusiasmar e motivar a classe, então se é um bom momento para falar num conceito, ele tem de ser explicável de forma simples e com um exemplo pratico claro. Não vale a pena explicar conceito quando eles ainda não vão perceber as coisas e ficam só a olhar para mim e em vez de ser um momento de entusiamo e motivação acaba por apenas os desmotivar e quebrar o ritmo da aula. 
- Podes dividir  ficheiro / aula em grandes blocos bem demarcados para se saber o que aí vem, exemplo
===========================================================================
Guião 02a, exercícios 9, 10, 11, 12, 13 [30 min -> 11:21]
https://ipce-184ea7.gitlab.io/
===========================================================================

e depois sub partes, exemplo
EXERCICIO 9 — Trovoada [4 min -> 10:55] ----------------------------------------

para ficar tudo bem organizado. 

- Sequência natural e contínua — não inventar "Blocos" artificiais (Bloco A, B, C…) só por organizar. Só criar sub-secções quando faz mesmo sentido conceptual.
- Cada tópico/exercício distinto tem de ter o seu próprio # %% — nunca deixar dois exercícios/conceitos na mesma célula.
- Deixar 6 linhas em branco entre o fim de uma célula e o # %% seguinte, para separar bem ao projetar.
- Podes colocar e manter código duplicado comentado para mostrar uma solução alternativa - isto é bom e intencional e deve-se manter e não remover.

### Soluções: o guia é um ficheiro normal, com as soluções diretas
- O guia da aula traz TODAS as soluções no próprio exercício, como no `aula_5.py`:
  enunciado curto (docstring com o que fazer e os exemplos) e logo a seguir a função resolvida.
- NÃO usar no corpo da aula o esquema das revisões (separador "SOLUÇÃO NA CÉLULA DE BAIXO",
  `pass` no enunciado e solução noutra célula). Eu é que, ao preparar a aula, apago as soluções e ponho `# TODO`.
- NÃO repetir os cabeçalhos das funções (`def f(l: list[int]) -> int`) nos comentários ou no enunciado:
  já estão no código.
- Nos blocos "prevejam o print", a resposta pode ficar em comentário ao lado do print (eu apago ao preparar).
- Exceção: os exercícios extra de CONSOLIDAR, no fim do ficheiro, mantêm o esquema das revisões
  (enunciado com `def` + docstring + `pass`, separador "SOLUÇÃO NA CÉLULA DE BAIXO", solução na célula seguinte).

### Cabeçalho do ficheiro
- Título da aula, links para site da cadeira e drive (https://ipce-184ea7.gitlab.io/ ,  https://drive.google.com/drive/folders/1twlkB4M0yg0tBxOt2c1diqJU6xtUsoay?usp=sharing ) e como usar o ficheiro no Spyder e tarefas iniciais de logistica da aula... 


### Matéria ainda não dada
- Não usar ferramentas que ainda não foram dadas (nem na teórica nem nos guiões anteriores), por exemplo numa aula onde ainda não foi dado nada sobre `enumerate`, `while`, `break`, métodos de strings, etc.
- Se valer a pena mostrar, fica como curiosidade comentada, claramente marcada como extra.


### Tempos: 
Depois pensa também em termos de tempo, o tempo que devo dar para cada exercício e para cada grande bloco para ser fácil eu ir gerindo a aula
- Formato uniforme em todos os blocos/exercícios: [X min -> HH:MM] (duração + hora-limite acumulada, não só duração).
- A soma tem de bater certo com a duração total da aula — verificar à mão, sub-item a sub-item e cumulativamente.
- Incluir tempo para logística/dúvidas no início (presenças, dúvidas da aula anterior).
- Incluir 1 intervalo de ~20 min - pensa o melhor local para colocar o intervalo para ser boa gestão de esforço e paragem natural em termos de conteúdo da aula se possível - MAS tentar fazer o intervalo a começar às 11:20 e terminar às 11:40 se pssível. 
- Sempre que se adiciona conteúdo novo a um bloco já existente, recalcular o timing todo — nunca deixar tags desatualizadas.

## Estilo de escrita
- Linguagem natural, simples, informal mas bem explicada — "de coder para coder". Nunca escrever como "Claude a explicar ao professor como ensinar os alunos" (nada de "explica-lhes desta forma", "pergunta-lhes X", "deixa-os arriscar"). O texto final é para os alunos lerem diretamente.
- Comentários # quebrados por ideia, uma linha por ideia — não texto corrido/justificado em parágrafo. Exemplo do formato certo:
  - # Estamos a testar uma condição mais fraca == mais abrangente, primeiro.
  - # A condição "febre >= 37.5" em 1º lugar 
  - # captura um paciente com febre=39.5 e dificuldade a respirar=True 
  - # (que deveria ser VERMELHO/urgente).

## Verificações finais
No fim: 
- Confirmar sintaxe válida com ast.parse().
- Correr o ficheiro inteiro de ponta a ponta (python3 ficheiro.py) com inputs simulados via printf | python3, na ordem exata das chamadas input().
- Correr também célula a célula, simulando o Spyder (execução sequencial com estado partilhado), inspecionando o output de cada uma.
- Confirmar valores e outputs para ver se bate tudo certo e está tudo bem, e em caso de demo de "isto está errado" testa com o valor exato usado no ficheiro para confirmar que demonstra mesmo o bug (já apanhei casos em que o "bug" tinha sido corrigido sem querer, e a demo passava a dar a resposta certa).
- Ao haver duas versões da mesma função (com/sem underscore, por exemplo), confirmar que os "clientes" main(), etc estão a chamar a função correta. 
- Rever todo o texto explicativo, não só o código — procurar inconsistências entre o que o texto promete e o que o código realmente faz, erros conceptuais, e omissões.
- Verificar o timing à mão (soma total + checkpoints acumulados).
- Verificar que cada secção com tag de tempo tem o seu próprio separador # %%.
- Verificar typos e caracteres partidos (acentos mal escritos).
- F5 tem de correr o ficheiro até ao fim sem rebentar. Demos de erros que lançam exceção
  (UnboundLocalError, IndexError...) ficam comentadas com "descomenta e corre", ou o ficheiro pára a meio.
- Correr cada célula ISOLADA (estado limpo, como quem abre o ficheiro e corre só aquela célula).
  Se falhar porque precisa de uma função ou import de outra célula: ou se redefine/importa ali,
  ou se põe uma nota "corre primeiro a célula X". Cada `import math` vai na célula que o usa.
- Confirmar que os exemplos do enunciado do guião dão mesmo o valor que o guião diz
  (ex: se o guião diz que f(4) dá 14, correr f(4)), e que a `main` usa esses mesmos valores.
- Todo o valor escrito num comentário (`# dá 0.30000000000000004`, "faz 1077 divisões") é confirmado a correr, nunca de cabeça.
- Procurar e remover prints de debug esquecidos dentro das funções.
- No corpo da aula: confirmar que cada exercício tem a solução completa logo a seguir ao enunciado,
  sem separador "SOLUÇÃO NA CÉLULA DE BAIXO" e sem cabeçalhos de funções repetidos nos comentários.
- Só no CONSOLIDAR final: confirmar que nenhum enunciado tem a resposta à vista
  e que o separador "SOLUÇÃO NA CÉLULA DE BAIXO" aparece uma só vez por exercício.
- Percorrer o ficheiro e confirmar a escada: cada ferramenta nova tem mini-exercícios logo a seguir,
  e os pormenores "por dentro" e edge cases só aparecem depois de exercícios normais com essa ferramenta.
- Zero travessões (—): usar hífen, dois pontos ou vírgula.
- Contar as células e os separadores no fim e dizer-me os números, com a prova das verificações
  (o que foi corrido e o resultado), não só "está verificado".
  
## Coisas a evitar (erros já cometidos)
- Explicar conceitos antes de pôr a programar (o erro principal identificado).
- Criar sub-blocos artificiais sem necessidade real.
- Deixar timing desatualizado ao adicionar conteúdo.
- Escrever linguagem "meta" dirigida ao professor no documento final para os alunos.
- Remover código duplicado comentado (é intencional).
- Assumir que "corre sem erro" = "está conceptualmente certo" — pode haver bugs silenciosos ou inconsistências texto/código.
- Deixar uma demo de erro descomentada, que faz o F5 rebentar a meio do ficheiro.
- Copiar um exemplo de `range` ou um valor do guião sem confirmar (ex: `range(n+1)` em vez de `range(n)`, que dava 30 em vez de 14).
- Escrever valores em comentários sem os correr.
- No CONSOLIDAR final: deixar as respostas de "o que escreve?" ao lado do código no enunciado.
- Blocos longos de explicação sem prática pelo meio, e pormenores avançados cedo demais
  (aula 5: referências, + vs append e funções que mudam listas antes de consolidar o básico das listas).
- Usar o esquema "SOLUÇÃO NA CÉLULA DE BAIXO" no corpo da aula (é só para o CONSOLIDAR final).
- Repetir nos comentários os cabeçalhos das funções que já estão no código.
- Usar matéria que ainda não foi dada fora de uma curiosidade comentada.

## Entrega
- Fazer primeiro o guia completo, verificado, e enviá-lo como ficheiro na conversa.
- Não fazer upload para o Google Drive reescrevendo o conteúdo do ficheiro (gasta tokens e pode perder partes).
  Eu próprio ponho o ficheiro no Drive.
- Se eu pedir alterações depois, dizer exatamente o que mudou e confirmar que os tempos continuam a somar até 13:00.

## Se bom e completo, não rápido
Esta é uma tarefa complexa e longa. Quero que te foques em fazer bem e não rápido. Não tentes fazer a tarefa neste comando. Se for preciso continuamos a tarefa no comando ou sessão seguintes. Quero que façam uma análise profunda e crítica a tudo e faças boa pesquisa se necessário para confirmar alguma coisa para garantir que tudo está correto tanto em termos de "conteúdo" como em termos "motivacionais" para dar uma classe que realmente cativa os alunos. Sê bom, não rápido.