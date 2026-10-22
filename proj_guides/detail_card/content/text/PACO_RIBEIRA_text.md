# The Painting = Ribeira Palace / Paço da Ribeira -- card text, EN and PT-PT

Written for the LivingRoom test wall (P1a). Own words, paraphrased from general historical knowledge; nothing copied from a source.
Accuracy is approximate (the wall is a fixture); the SHAPE and LENGTH are real. Block ids are the ones in `config.json` today
(`painting`). `[[a|b]]` is not used here (this card has no glossary links). Picture files: `content/images/`.

---

## 0. The F5 text fixes (`_3.2.1_Audit.md` F5, items 2, 3, 5 and the "pinch" half of 5)

**F5-2 One name for the place.** Proposal -- the marker label and the card title are the SAME words:

| | EN | PT-PT |
|---|---|---|
| Marker label | Ribeira Palace | Paço da Ribeira |
| Card title (header `title`) | Ribeira Palace | Paço da Ribeira |
| Card subtitle (header `subtitle`) | Royal palace, lost in 1755 | Palácio real, perdido em 1755 |

Today the marker says "The Painting" (the point's `name`, an identity field) and the card says "The Ribeira Palace". Renaming `name`
re-bases the search tests (`_3.3` TODO "demo content"), so P1a only PROPOSES the pair. The 14 / 15 character title also fits the
collapsed header next to its three round buttons (today's 18 characters were cut to "The Ribeira Pal...").
The summary "POI_Painting" (the subtitle on header-only cards) would read: EN "Royal palace on the river, lost in the 1755 earthquake" /
PT "Palácio real à beira-rio, destruído no terramoto de 1755".

**F5-3 Translated category.** `royal_government` shows "Royal Government" in PT. Proposal: PT "Governo Real". (The same key is used by
`lamp`; its header subtitle says "Military", PT "Militar", which is already translated.)

**F5-5a Fun-fact headings.** Two identical "Did you know? / Tap to reveal" cards back to back. Each fact below has a HEADING that tells the
visitor what is behind the tap (section 7). The `fun_fact` kind has no heading field today: P1b adds one (or uses the first line).

**F5-5b No doubled "pinch" hint.** The zoom caption (section 6) describes the PICTURE only; the card's own hint ("Pinch to zoom in") is
the only place that says "pinch".

---

## 1. block_1 `header` image_parallax -- title, subtitle, hero caption

| Field | EN | PT-PT |
|---|---|---|
| title | Ribeira Palace | Paço da Ribeira |
| subtitle | Royal palace, lost in 1755 | Palácio real, perdido em 1755 |
| image (`paco_hero.jpg`) caption | The Terreiro do Paço in 1662, with the Ribeira Palace along the river front. Oil painting by Dirk Stoop, c. 1662. | O Terreiro do Paço em 1662, com o Paço da Ribeira ao longo do rio. Pintura a óleo de Dirk Stoop, c. 1662. |
| credit | Dirk Stoop, public domain | Dirk Stoop, domínio público |

(The header has no caption field today: the caption is the hero's alt text and the line a visitor sees in the full-screen picture.)

## 2. block_2 `quick_facts` grid_hairline -- six facts

| label EN | value EN | label PT | value PT |
|---|---|---|---|
| Built | From c. 1500 | Construção | Desde c. 1500 |
| Built for | King Manuel I | Mandado por | D. Manuel I |
| Remodelled | 1580s, under Philip I | Remodelação | Década de 1580, com Filipe I |
| Royal home for | About 250 years | Casa real durante | Cerca de 250 anos |
| Lost | 1 November 1755 | Perdido | 1 de novembro de 1755 |
| Stood where | Today's Praça do Comércio | Ficava onde | Hoje, a Praça do Comércio |

heading: EN "At a glance" / PT "Num relance".

## 3. block_3 `rich_text` lede -- "The story" (2 min read)

heading: EN "The story" / PT "A história".

**EN** (lead + three paragraphs)

> For about 250 years the kings of Portugal lived by the river, in a palace that opened straight onto the Tagus. It is gone now, but the square that took its place still carries its name.
>
> When King Manuel I found the old castle on the hill too cramped for a court that was growing rich on the sea trade, he moved down to the water's edge. The new palace stood beside the docks where ships from India and Brazil unloaded spices, sugar and gold, so the king could watch the fleet come in from his own windows. Offices, a chapel, workshops and the royal archive grew around it, and the palace became the true centre of the kingdom.
>
> Later kings kept adding to it. In the 1580s, under Philip I, who ruled Portugal and Spain together, the river front received a new facade and a domed corner tower, the Torreão, that sailors saw long before they saw the city. Inside were a famous library of tens of thousands of books, paintings, maps, and a royal opera house, the Ópera do Tejo, opened only eight months before the end.
>
> On the morning of 1 November 1755, All Saints' Day, an earthquake shook the palace, a wave came up the river and fire finished what was left. The royal family was away from the building that morning, but the library, the archive and almost everything inside were lost. Lisbon was rebuilt on a strict new plan, and where the palace had stood an open square was laid out, facing the river like a grand front door: the Praça do Comércio.

**PT-PT**

> Durante cerca de 250 anos, os reis de Portugal viveram junto ao rio, num palácio que se abria diretamente sobre o Tejo. Já não existe, mas a praça que ficou no seu lugar ainda guarda o seu nome.
>
> Quando D. Manuel I achou o velho castelo no alto pequeno demais para uma corte que enriquecia com o comércio marítimo, desceu até à beira da água. O novo paço ficava junto das docas onde os navios da Índia e do Brasil descarregavam especiarias, açúcar e ouro, e o rei podia ver a frota chegar das suas próprias janelas. À volta dele cresceram gabinetes, uma capela, oficinas e o arquivo real, e o paço tornou-se o verdadeiro centro do reino.
>
> Os reis seguintes foram acrescentando obra. Na década de 1580, com Filipe I, que reinava em Portugal e em Espanha, a frente ribeirinha ganhou uma nova fachada e um torreão de cúpula que os marinheiros avistavam muito antes de verem a cidade. Lá dentro havia uma célebre biblioteca com dezenas de milhares de livros, pinturas, mapas e uma ópera real, a Ópera do Tejo, inaugurada apenas oito meses antes do fim.
>
> Na manhã de 1 de novembro de 1755, dia de Todos os Santos, um terramoto abalou o paço, uma vaga subiu o rio e o fogo acabou o que restava. A família real estava fora do edifício nessa manhã, mas a biblioteca, o arquivo e quase tudo o que havia lá dentro perderam-se. Lisboa foi reconstruída segundo um plano rigoroso e, no lugar do paço, traçou-se uma praça aberta ao rio, como uma grande porta de entrada: a Praça do Comércio.

## 4. block_4 `timeline` vertical -- six events (1 min)

heading: EN "Timeline" / PT "Cronologia".

| date EN / PT | title EN | text EN | title PT | text PT |
|---|---|---|---|---|
| c. 1500 | The king moves to the river | Manuel I builds a new palace beside the docks, away from the old castle on the hill. | O rei muda-se para o rio | D. Manuel I constrói um novo paço junto às docas, longe do velho castelo no alto. |
| 1580s / Década de 1580 | The domed tower rises | Philip I, king of Portugal and Spain, gives the palace a new river front and its corner tower, the Torreão. | Ergue-se o torreão | Filipe I, rei de Portugal e de Espanha, dá ao paço uma nova frente ribeirinha e o torreão de cúpula. |
| 23 Apr 1662 / 23 abr. 1662 | A royal procession | The yard fills for the departure of Catherine of Braganza, who sailed to England to marry Charles II. | Um cortejo real | O terreiro enche-se para a partida de D. Catarina de Bragança, que seguiu para Inglaterra a fim de casar com Carlos II. |
| 2 Mar 1755 / 2 mar. 1755 | The opera opens | The Ópera do Tejo, the royal opera house beside the palace, opens its doors. | Abre a ópera | A Ópera do Tejo, o teatro de ópera real junto ao paço, abre as portas. |
| 1 Nov 1755 / 1 nov. 1755 | Earthquake, fire and wave | The palace, its library and its collections are lost in one day. | Terramoto, incêndio e maremoto | O paço, a biblioteca e as coleções perdem-se num só dia. |
| 1775 | A square takes its place | The statue of Joseph I is unveiled in the new Praça do Comércio, the heart of the rebuilt city. | Uma praça toma o seu lugar | É inaugurada a estátua de D. José I na nova Praça do Comércio, coração da cidade reconstruída. |

## 5. block_5 `gallery` carousel -- four pictures (one portrait), caption + credit

heading: EN "In pictures" / PT "Em imagens".

| # | file | shape | caption EN | caption PT | credit |
|---|---|---|---|---|---|
| 1 | `paco_gallery_1.jpg` | landscape 719x412 (the source is small) | A royal procession crosses the Terreiro do Paço, 23 April 1662. Anonymous painter. | Um cortejo real atravessa o Terreiro do Paço, 23 de abril de 1662. Pintor anónimo. | Public domain / Domínio público |
| 2 | `paco_gallery_2.jpg` | landscape 4:3 | The ruins of the Ópera do Tejo after the earthquake. Engraving by Jacques-Philippe Le Bas, 1757. | As ruínas da Ópera do Tejo depois do terramoto. Gravura de Jacques-Philippe Le Bas, 1757. | Public domain / Domínio público |
| 3 | `paco_gallery_3.jpg` | PORTRAIT 3:4 | The palace front and its formal garden, drawn for a firework display in 1687. Print by João dos Reis. | A fachada do paço e o seu jardim formal, desenhados para um fogo de artifício em 1687. Gravura de João dos Reis. | Public domain / Domínio público |
| 4 | `paco_gallery_4.jpg` | landscape 4:3 | The Praça do Comércio seen from the river today, with the castle hill behind. Photo, 2025. | A Praça do Comércio vista do rio nos dias de hoje, com a colina do castelo ao fundo. Fotografia, 2025. | Jakub Hałun, CC BY 4.0 |

## 6. block_10 `zoom_image` pinch -- the tiles (square picture)

heading: EN "The tiles, close up" / PT "Os azulejos de perto".
File `azulejo_detail.jpg` (1824x1824).

| | EN | PT-PT |
|---|---|---|
| caption | Ships at the Ribeira das Naus, the royal dockyard beside the palace, on the Great Panorama of Lisbon, a tile panel from about 1700. | Navios na Ribeira das Naus, o estaleiro real junto ao paço, no Grande Panorama de Lisboa, um painel de azulejos de cerca de 1700. |
| credit | Photo: RickMorais, CC BY-SA 4.0 | Fotografia: RickMorais, CC BY-SA 4.0 |

(No word "pinch" in the caption: F5-5b.)

## 7. block_6 `fun_fact` flip -- three facts, each with a heading

| # | heading EN | text EN | heading PT | text PT |
|---|---|---|---|---|
| 1 | Lost in a day | The royal library is said to have held around 70,000 books, one of the richest collections in Europe. Almost none survived 1 November 1755. | Perdida num dia | Diz-se que a biblioteca real guardava cerca de 70 000 livros, uma das coleções mais ricas da Europa. Quase nenhum sobreviveu a 1 de novembro de 1755. |
| 2 | A name that stayed | Many Lisboetas still call the Praça do Comércio by its old name, Terreiro do Paço, which means "palace yard". | Um nome que ficou | Muitos lisboetas ainda chamam à Praça do Comércio pelo nome antigo, Terreiro do Paço, que quer dizer "pátio do paço". |
| 3 | Eight months of music | The Ópera do Tejo opened in March 1755 and was said to be one of the grandest opera houses in Europe. It stood for eight months. | Oito meses de música | A Ópera do Tejo abriu em março de 1755 e dizia-se ser um dos teatros de ópera mais grandiosos da Europa. Durou oito meses. |

## 8. before / after (NEW for this card; `before_after` slider -- the card has none today, the Lamp's block_33 has one)

heading: EN "Palace then, square now" / PT "O paço de então, a praça de hoje".

| | EN | PT-PT |
|---|---|---|
| before_label (`paco_before_1755.jpg`) | Before 1755 | Antes de 1755 |
| after_label (`paco_after_today.jpg`) | Today | Hoje |
| caption | The same river front: the palace before the earthquake (anonymous painting, 18th century) and the Praça do Comércio today (photo, 2017). The two viewpoints are close, not identical. | A mesma frente ribeirinha: o paço antes do terramoto (pintura anónima, século XVIII) e a Praça do Comércio hoje (fotografia, 2017). Os dois pontos de vista são próximos, não idênticos. |
| credit | Before: public domain. After: Dguendel, CC BY 4.0 | Antes: domínio público. Depois: Dguendel, CC BY 4.0 |

Note: the Lamp's block_33 labels say "After the earthquake"; with this photo the right label is "Today" / "Hoje".

## 9. `hotspot_image` -- the palace before 1755 (five identifiable features)

File `paco_hotspots.jpg` (2048x1704). x, y are fractions of the picture (0 = left / top).

| # | x | y | title EN | text EN | title PT | text PT |
|---|---|---|---|---|---|---|
| 1 | 0.31 | 0.30 | The domed tower | The Torreão, the corner tower with its dome, was the palace's landmark from the river. | O torreão | O torreão, a torre de canto com a sua cúpula, era o ex-líbris do paço visto do rio. |
| 2 | 0.56 | 0.33 | A slender tower | A slim tower rises behind the long palace front. | Uma torre esguia | Uma torre esguia ergue-se por trás da longa fachada do paço. |
| 3 | 0.30 | 0.68 | The stone quay | A stone quay reached out into the river, where royal and merchant boats landed. The river steps of the square, the Cais das Colunas, still serve that purpose. | O cais de pedra | Um cais de pedra avançava pelo rio, onde atracavam barcos reais e de mercadores. Os degraus do rio da praça, o Cais das Colunas, ainda cumprem essa função. |
| 4 | 0.82 | 0.68 | Coaches in the yard | Coaches cross the open yard, where the court showed itself to the city. | Carruagens no terreiro | Carruagens atravessam o terreiro aberto, onde a corte se mostrava à cidade. |
| 5 | 0.88 | 0.22 | The town on the hill | Behind the palace the old town climbs the hill in towers and roofs. | A cidade na colina | Por trás do paço, a cidade velha sobe a colina em torres e telhados. |

heading: EN "Explore the painting" / PT "Explore a pintura".

## 10. block_7 `audio_guide` player -- the narration

title: EN "Guided tour of the Ribeira Palace" / PT "Visita guiada ao Paço da Ribeira". heading "Listen" / "Ouvir".
Scripts, audio and exact captions: `content/audio/` (`guide_paco_en.*`, `guide_paco_pt.*`; section "Audio" of CONTENT_MAP).

## 11. block_8 `related` -- "Nearby on the panel" (F5 item 7: real neighbours)

The block shows the nearest POIs, which are fixtures ("Painting - Infrastructure"). Identity fields are not renamed here; this is the list of
REAL neighbours the demo points should become, for the `_3.3` rename block:

| place | one-liner EN | one-liner PT |
|---|---|---|
| Cais das Colunas | The stone steps where the square meets the river, between two columns. | Os degraus de pedra onde a praça encontra o rio, entre duas colunas. |
| Arco da Rua Augusta | The triumphal arch that opens the square to the Baixa, finished in 1873. | O arco triunfal que abre a praça à Baixa, concluído em 1873. |
| Statue of Joseph I | The king who reigned in 1755, on horseback in the middle of the square. | O rei que reinava em 1755, a cavalo no centro da praça. |
| Casa dos Bicos | A house with a diamond-point front, a few minutes east along the river. | Uma casa de fachada com bicos de diamante, a poucos minutos para leste, junto ao rio. |

## 12. block_9 `sources` -- the list the card shows (content_status: draft)

| title EN | author | licence | title PT |
|---|---|---|---|
| Terreiro do Paço em 1662 (painting) | Dirk Stoop | Public domain | Terreiro do Paço em 1662 (pintura) |
| Terreiro do Paço before the earthquake of 1755 (painting) | Unknown, 18th century | Public domain | Terreiro do Paço antes do terramoto de 1755 (pintura) |
| Ruins of the Ópera do Tejo, 1757 (engraving) | Jacques-Philippe Le Bas | Public domain | Ruínas da Ópera do Tejo, 1757 (gravura) |
| Grande Panorama de Lisboa, detail (tile panel, photo) | RickMorais | CC BY-SA 4.0 | Grande Panorama de Lisboa, pormenor (painel de azulejos, fotografia) |
| Praça do Comércio today (photos) | Dguendel; Jakub Hałun | CC BY 4.0 | A Praça do Comércio hoje (fotografias) |
| Text | Written for this wall from general historical knowledge, in the writers' own words | -- | Texto escrito para esta parede a partir de conhecimento histórico geral, com palavras próprias |

Every file's full record (URL, date, changes) is in `content/SOURCES.md`.

## 13. Quizzes (the card has none today; the Lamp's three are in the castle file)

**Multiple choice** (`knowledge_check` multiple_choice, three questions; the wrong answers are plausible):

1. Q EN: Which king moved the royal court from the castle to the river?  PT: Que rei mudou a corte do castelo para o rio?
   a) Afonso Henriques b) Manuel I (correct) c) João V
   Explanation EN: Manuel I built his palace by the docks around 1500, where the ships arrived. PT: D. Manuel I construiu o seu paço junto às docas por volta de 1500, onde chegavam os navios.
2. Q EN: What was the royal opera house beside the palace called?  PT: Como se chamava o teatro de ópera real junto ao paço?
   a) Teatro de São Carlos b) Ópera do Tejo (correct) c) Coliseu dos Recreios
   Explanation EN: The Ópera do Tejo opened in March 1755 and was destroyed eight months later. PT: A Ópera do Tejo abriu em março de 1755 e foi destruída oito meses depois.
3. Q EN: What stands where the palace used to be?  PT: O que existe hoje onde ficava o paço?
   a) The Rossio b) The Praça do Comércio (correct) c) The Praça da Figueira
   Explanation EN: The open square was laid out facing the river after the earthquake. PT: A praça aberta ao rio foi traçada depois do terramoto.

**True / false swipe** (`true_false_swipe`): 
1. EN "The palace survived the earthquake of 1755." PT "O paço sobreviveu ao terramoto de 1755." -> False. Explanation EN: It was lost the same day, with its library. PT: Perdeu-se nesse mesmo dia, com a sua biblioteca.
2. EN "Terreiro do Paço means 'palace yard'." PT "Terreiro do Paço quer dizer 'pátio do paço'." -> True. Explanation EN: Terreiro is an open yard and Paço is the palace. PT: Terreiro é um pátio aberto e paço é o palácio.

**Image choice** (`image_choice`): Q EN "Which picture shows the river front BEFORE the earthquake?" PT "Qual das imagens mostra a frente ribeirinha ANTES do terramoto?" options (`paco_before_1755.jpg`, `paco_after_today.jpg`) EN "The painting" / "The photograph"; PT "A pintura" / "A fotografia"; correct 1. Explanation EN: The painting still shows the palace and its domed tower. PT: A pintura ainda mostra o paço e o seu torreão.

## 14. Person / curator line (`person`)

| | EN | PT-PT |
|---|---|---|
| heading | The king who moved | O rei que mudou a corte |
| name | King Manuel I | D. Manuel I |
| role | King of Portugal, 1495 to 1521 | Rei de Portugal, 1495 a 1521 |
| text | He moved the court to the river around 1500 and filled the new palace with the wealth of the voyages to India. | Mudou a corte para o rio por volta de 1500 e encheu o novo paço com a riqueza das viagens à Índia. |
| curator line | Texts and pictures chosen for the TileStories test wall. Draft content, approximate. | Textos e imagens escolhidos para a parede de teste TileStories. Conteúdo provisório, aproximado. |

## 15. Real shape: lengths (characters, EN / PT)

Measured on the text above (section 3 body = the four paragraphs together); the numbers are in CONTENT_MAP "Real lengths".
