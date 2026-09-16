Sou professor das aulas práticas do turno **P1** (2a-feira de manhã),
ano letivo 2026/2027. A prática do P1 é dada **antes** da teórica da
mesma matéria (que é à tarde) — por isso cada aula vem sempre com um
guião de pré-aula e a lógica de "onde estávamos / hoje / a seguir".


Seguem-se os factos e convenções já estabelecidos nas aulas anteriores (Aula 1), para continuidade.

## Sobre a disciplina
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