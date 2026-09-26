# `Aula 3 - IPCE`
Sou professor das aulas práticas do turno **P1**, ano letivo 2026/2027. A prática do P1 é dada **antes** da teórica da mesma matéria (que é à tarde) — por isso as coisas têm de ser explicadas de forma simples e clara para que mesmo quem não sabe nada consiga acompanhar

## Alguns factos da aula: 
- Ferramentas da cadeira: **Miniconda + Spyder** (IDE), **Mooshak**  (exercícios/avaliação automática, precisa de Eduroam/VPN)
- Aula prática de 2:50 horas seguidas, entre as 10:10 e as 13:00 (é bom então pensar 1 intervalo maior de 20 minutos a meio para a turma descansar) 
- Aula prática começa às 10:10
  
## Agora vamos preparar a `aula 3` - Envio agora em anexo: 
* Envio em anexo a aula teorica que outro professor vai dar, A SEGUIR A ESTA AULA PRÁTICA (por isso não podemos assumir que sabem coisas e dizer: "como viram na teorica"). 
* Envio também o guião da aula prática que é o guião que eu devo seguir na aula pratica `3a e 3b` (como vês é um guia bem feito e completo e basicamente a aula podia ser eu dizer "leiam este guia de exercícios e façam e qualquer dúvida perguntem", mas pronto, quero ser um professor de qualidade que realmente sabe ensinar e cativar a aula e ir além desses guias práticos!). 
* Envio também as soluções dos exercícios
* Envio também uns exemplos de testes dos anos anteriores (é para este nível de dificuldade ou superior que devo preparar os alunos)
* Envio também o exemplo de plano guia de sessão da última aula.
 

## Estilo de ensino (importante manter)
- Dar prioridade a prática e eles experimentarem em vez de teoria formal. Aprender fazendo.
- Espírito leve e cativante — nunca "matar" a motivação com excesso de rigor ou material difícil/pouco intuitivo. Exemplo explicar todos os conceitos e detalhes importantes, mas dados de forma faseada e de forma leve e intuitiva como se estivesse a explicar a crianças do 8o ano, mas de forma a quee elees realmente percebam e estejam depois preparados para fazer exercicios dificeis nos testes (vê o exemplo de testes e exames dos anos anteriores - eles têm de aprender tudo de forma simples, mas temos que saber fazer exercicios dificeis!!!). 

## Filosofia pedagógica — a regra mais importante de todas
Aprendida da pior forma (uma aula que correu mal): nunca explicar um conceito sem primeiro pôr os alunos a programar algo simples relacionado com ele. Fluxo obrigatório por tópico/conceito:
1. Apresentar exemplos simples para mostrar as regras de syntax de uma ferramenta nova. exemplos simples com "for" só a imprimir para ver o range a funcionar, exemplos simples de print(1 in [1,2,3]) só pra ver como in funciona, etc.
2. Apresentar um ou mais exercícios mais simples sobre os conceitos a explicar - Eu explico o exercício e faço-os pensar nas coisas e digo para eles tentarem resolver: a) começo por mostrar só os cabeçalhos das funções que podem usar para resolver o exercício; b) uns minutos depois mostro o código (ou vou revelando aos poucos) e deixo eles acabarem e copiar mesmo a solução do projetor e correrem aquilo; 
3. Só depois de praticarem os exercícios necessários para prceberem o básico desse conceito a ensinar, então aí sim explicar pormenores e detalhes importantes sobre esse conceito, exemplo para o código funcionar melhor, ou ser mais simples ou bem organizado, ou mais eficiente, ou para se evitar cometer erros típicos que alunos cometem, ou para apresentar exercícios típicos de testes, etc... e consoante o tempo disponível para apresentar cada um destes pontos pode haver código e exercícios simples para demonstrar e alguns eu posso só mostrar e outros mando fazer também etc...
4. Depois mais alguns exeercícios finais para consolidar, em que aqui já não digo nada no início, passado uns minutos explico a ideia geral no quadro, depois vou mostrando código aos poucos... 


Este é o flow normal que se deve seguir nas aulas - primeiro prática com demonstração e eles realmente meterem código a correr. Só depois explicar conceitos. E ao explicar conceitos:
- Nunca matar a motivação com excesso de rigor ou material difícil/pouco intuitivo cedo demais — explicar como a alguém do 8º ano, mas de forma fundamentada, para que fiquem preparados para exercícios difíceis de teste.
- Onde fizer sentido, fundamentar os conceitos em ideias sólidas por trás (ex: comparadores/and-or-not/if como camadas de lógica proposicional) — mas em linguagem simples, sem jargão académico pesado (nunca dizer "First-Order Logic" aos alunos, por exemplo).
- Ligar sempre que possível a exercícios reais de testes/exames anteriores, para calibrar dificuldade e mostrar relevância direta.




## Estrutura e Formato do Ficheiro a produzir
Isto é um ficheiro que basicamente eu vou mostrando na aula e no fim partilho com os alunos. Então não quero que escrevas notas de ti claude para mim professor a dizer como explicar, mas sim quero que escrevas um documento como se fosse quase um programa normal em código constituido com várias funções e com muito bons comentários e explicações a explicar o que está a acontecer e conceitos e técnicas importantes, de forma simples e natural e informal e clara, como se fosse um coder guy a escrevr notas para outro coder guy. 
- Faz um ficheiro `*.py`, estilo Spyder, com células `# %%` para ser fácil correr um exercício ou demo de cada vez 
- Mantém linguagem deste guia de sessão concisa e simples e informal, MAS bem explicado com todos os detalhes importantes, MAS de forma progressiva - lembra-te que primeiro fazemos exercícios práticos para explicar os conceitos básicos sobre como as coisas funcionam, logo com eles a programar e escrevr esse código desses exercícios. Só depois então explicamos os conceitos em mais profundidade quase como a fazer um apanhado dos detalhes que eles já viram mas agora a falar claramente deles e explicar conceitos etc e explicar técnicas, boas práticas de fazer melhor codigo mais limpo simples e mais eficiente, erros a evitar etc... e depois então mais uns exercícios para consolidar... 
- Nos exercicios que são mesmo do guião da aula, identifica-os da forma correta para ser claro os exercicios inventados por mim para completar a aula dos exercicios do guião da aula mesmo que temos que seguir
- Se for um momento oportuno podemos introduzir algum conceito que é bom eles saberem para o teste ou para cultura geral... para os motivar e eles sentirem que ficam a saber mais que os outros... exemplo conceitos como tipagem forte ou fraca, dinamico ou estatico, passar dados by reference ou by value, etc... conceitos que um bom programador devia saber... e para a tornar a aula mais rica e interessante MAS LEMBRA-TE QUE ISTO É "INTRODUÇÃO À PROGRAMAÇÃO" - Quero entusiasmar e motivar a classe, então se é um bom momento para falar num conceito, ele tem de ser explicável de forma simples e com um exemplo pratico claro. Não vale a pena explicar conceito quando eles ainda não vão perceber as coisas e ficam só a olhar para mim e em vez de ser um momento de entusiamo e motivação acaba por apenas os desmotivar e quebrar o ritmo da aula. 
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
- Deixar 3 linhas em branco entre o fim de uma célula e o # %% seguinte, para separar bem ao projetar.
- Podes colocar e manter código duplicado comentado para mostrar uma solução alternativa - isto é bom e intencional e deve-se manter e não remover.

### Tempos: 
Depois pensa também em termos de tempo, o tempo que devo dar para cada exercício e para cada grande bloco para ser fácil eu ir gerindo a aula
- Formato uniforme em todos os blocos/exercícios: [X min -> HH:MM] (duração + hora-limite acumulada, não só duração).
- A soma tem de bater certo com a duração total da aula — verificar à mão, sub-item a sub-item e cumulativamente.
- Incluir tempo para logística/dúvidas no início (presenças, dúvidas da aula anterior).
- Incluir 1 intervalo de ~20 min - pensa o melhor local para colocar o intervalo para ser boa gestão de esforço e paragem natural em termos de conteúdo da aula se possível. 
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
  
## Coisas a evitar (erros já cometidos)
- Explicar conceitos antes de pôr a programar (o erro principal identificado).
- Criar sub-blocos artificiais sem necessidade real.
- Deixar timing desatualizado ao adicionar conteúdo.
- Escrever linguagem "meta" dirigida ao professor no documento final para os alunos.
- Remover código duplicado comentado (é intencional).
- Assumir que "corre sem erro" = "está conceptualmente certo" — pode haver bugs silenciosos ou inconsistências texto/código.

## Se bom e completo, não rápido
Esta é uma tarefa complexa e longa. Quero que te foques em fazer bem e não rápido. Não tentes fazer a tarefa neste comando. Se for preciso continuamos a tarefa no comando ou sessão seguintes. Quero que façam uma análise profunda e crítica a tudo e faças boa pesquisa se necessário para confirmar alguma coisa para garantir que tudo está correto tanto em termos de "conteúdo" como em termos "motivacionais" para dar uma classe que realmente cativa os alunos. Sê bom, não rápido.




# `guia de acompanhamento`
# `ppt`




## Alguns factos da aula: 
- Site oficial (já com conteúdo real): https://ipce-184ea7.gitlab.io/
- Docentes: Artur Miguel Dias, Miguel Monteiro, Ana Ribeiro, Francisco
  Silva (eu), + Monteiro, Tiago Nunes (nomes vindos de um screenshot
  cortado à direita — confirmar lista completa se precisar).
- Ferramentas da cadeira: **Miniconda + Spyder** (IDE), **Mooshak**
  (exercícios/avaliação automática, precisa de Eduroam/VPN), fórum em
  Google Groups (`https://groups.google.com/g/ipce-2627`).
- Meu email: fmso.silva@campus.fct.unl.pt

## Histórico de aulas:
- **Aula 1** cobriu: boas-vindas, set up (Miniconda/Spyder), primeiro
  contacto com Python (tipos de dados, cast, print/input), teaser de if/ciclos/funções/recursividade.
- **Aula 2** (a preparar agora) 

## Estilo de ensino (importante manter)
- Dar prioridade a prática e eles experimentarem em vez de teoria formal. Aprender fazendo.
- Espírito leve e cativante — nunca "matar" a motivação com excesso de
  rigor ou material difícil/pouco intuitivo. Exemplo o guião de complemento à aula, para consolidar o que se aprendeu e dar um teaser à matéria da aula seguinte, tem de ser um guia com todos os conceitos  detalhes importantes, mas dados de forma faseada e de forma leve e intuitiva como se estivesse a explicar a crianças do 8o ano, e não podem ser longos para não fazer desanimar o aluno. 

## PowerPoint - junto envio o `build.js` em anexo usado para as aulas anteriores, e vamos então continuar a construir a partir dele:
- Gerado via **pptxgenjs** (Node.js), não editado manualmente slide a
  slide — o `build.js` é a fonte da verdade. 
- Regra de tamanhos de letra (pedido explícito, para sala grande):
  título = 32, subtítulo = 24-28, corpo/bullets = 20. Só usar 16/18 em
  elementos que não são "conteúdo para ler" (kicker/rótulo pequeno,
  rodapé, número de página) — nunca no corpo principal.
- Paleta »» Isto é que temos que trocar. Vamos fazer o ppt com fundo escuro afinal em vez de fundo branco, pois vê-se melhor na sala. Ou seja atualmente acho que temos: navy `0F1F3D`/`16294D` (capa e fecho), azul NOVA `2B59F7`,
  gelo `E8EEFB`/`F3F6FC`, texto `1B2A4A`, muted `6B7690`.... mas então vamos meter tudo com fundo escuro (bem escuro) e texto claro (bem claro - para bom contraste - a sala tem muito sol a entrar - o contraste tem de ser máximo, mas mantendo um bom design) 
- Fundo aquarela com motivo de chavetas `{ }` na capa... podemos usar este fundo ou podes melhorar um fundo simples e fixe para a base de todos os slides?? 
- Logo NOVA FCT podes retirar no ppt da aula anterior que também envio em anexo (usa então apenas o da versã `logo_white_sm.png` para fundo escuro.
- Ícones: gerados via `react-icons` (Lucide) renderizados para PNG com
  `sharp`, na cor certa (azul/branco/muted) — não usar ícones
  a preto sólido (houve um bug destes já resolvido: perder o
  `stroke="currentColor"` do SVG ao reprocessar o markup).
- QR codes: gerados com a lib Python `qrcode`, um por link (nunca
  reutilizar o mesmo QR para dois sítios diferentes). »» mas a partir de agora podes tirar o QR do ppt que envio em anexo porque não vai haver mais QR codes novos. vamos reutilizar esses
- Slide "O nosso percurso" (tracker semanal) é o **template reutilizável
  todas as semanas**: 3 cartões — Aula anterior (muted/itálico) / Hoje
  (azul, destacado) / Próxima aula. Só isso muda de semana para semana.
- As secções de "apresentação da disciplina/logística" (site, contactos,
  "turno fora de ordem") foram só para a Aula 1 — podemos remover esses slides e criar apenas um para mostrar logo no início com "Anuncios / Logística" para se escrever aí as coisas necessárias. 
- Notas do orador: adicionadas via `slide.addNotes(...)` no `build.js`
  — lembretes curtos por slide.
- Sempre gerar PDF + imagens com LibreOffice/pdftoppm para QA visual
  antes de entregar — várias vezes já apanhei texto a sobrepor-se por
  causa dos tamanhos de letra maiores.

## Ficheiro de acompanhamento `.py` (estilo Spyder, células `# %%`)
- **Anexo os ficheiros das aulas anteriores para veres o conteúdo já abordado e veres o estilo simples e direto e informal, mas bem explicado (ou seja conciso quanto possível mas explica todos os detalhes importantes para se perceber bem).
- Sempre testar o ficheiro a correr do início ao fim antes de entregar
  (atenção especial a `input()` — testar com respostas simuladas via
  `printf "resposta\n" | python3 ficheiro.py`).
- Fecha sempre com secções a explicar de forma muito simples a matéria da próxima aula para quem quiser ver, servindo assim de spoiler. 

## Agora vamos preparar a aula 2.
* Envio em anexo a aula teorica que outro professor vai dar. 
* Envio também o guião da aula prática que é o guião que eu devo seguir. 
* Envio também uns exemplos de testes dos anos anteriores 

## Tarefa 1 - Plano Guia de Sessão:
» Faz-me um "Plano Guia de Sessão" sobre como eu devo explicar e/ou demonstrar e/ou mandar fazer esse guia de aula prática... faz um *.md simples com o que será um bom esquema de aula... exemplo eu explico algum conceito base ou teorico antes de tudo? ou começo simplesmente a seguir o guia e talvez fazer o 1º exercício com os alunos todos juntos... ou mais exercícios?? ou a certa altura digo para eles continuarem sozinhos e qualquer dúvida digo para chamarem e se depois vir que muitos estão encravados então paramos e continuamos todos juntos?? ou dou-lhes uns minutos para cada exercicio e depois corro eu também no projetor a solução...?? qual é a melhor estratégia para conduzir esta aula prática e este tipo de exercícios? 
então pensa o bom plano de sesão e como eu devo explicar/demonstrar/fazer com eles / mandar fazer a matéria... 
pensa também em termos de tempo, o tempo que devo dar para cada exercício ou bloco de exercícios para assim ser fácil de gerir a aula. Esta aula prática tem 3 horas seguidas. Então na prática convém fazer 1 ou 2 intervalos lá pelo meio para eles descansarem? Então quando é bom fazer estes intervalos e de que duração? 
depois pensa também algum conceito extra ou exercicio extra ou explicação extra que seja bom eu explicar-lhes e que não está no guia da aula prática mas que é uma boa adição? Exemplo algum conceito de cultura geral ou para melhor pereceberem como as coisas funcionam ou como se fazem ou edge cases ou exercicios parecidos com os dos testes anteriores em termos de dificuldade e "tipologia" etc..?? alguma boa adição a fazer à aula? Então apresenta de forma clara o local no plano de sessão em que eu devo introduzir algum desses elementos extra com a etiqueta `[EXTRA]​`
algum outro pormenor que aches importante para dar uma boa aula...?? 

depois de refinarmos e concluirmos este plano guia de sessão então aí sim depois, num comando futuro, vamos fazer o ppt 