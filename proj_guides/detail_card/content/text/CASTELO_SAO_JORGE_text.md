# The Lamp = St George's Castle / Castelo de São Jorge -- card text, EN and PT-PT

Written for the LivingRoom test wall (P1a). Own words, paraphrased from general historical knowledge; nothing copied from a source.
Accuracy is approximate (the wall is a fixture); the SHAPE and LENGTH are real. Block ids are the ones in `config.json` today (`lamp`).
`[[shown|term]]` is the card's glossary-link markup (terms `keep`, `curtain wall`, `barbican` exist in `card_settings.glossary`): kept as is.
Where today's text is a fixture joke about "the panel" (a tile panel that does not exist), the block gets a real line instead.

---

## 0. The names (same rule as the Painting; `_3.2.1` F5 item 2)

| | EN | PT-PT |
|---|---|---|
| Marker label = card title | St George's Castle | Castelo de São Jorge |
| Subtitle | Medieval castle and royal palace | Castelo medieval e paço real |
| Category chip | Military | Militar |

Today the marker says "The Lamp" (the point's `name`, an identity field: renaming it is the `_3.3` rename block, not P1a). The point's
`category` key is `royal_government`; the card's own category chip reads "Military" and is already translated.

## 1. block_1 `header` image_parallax

| Field | EN | PT-PT |
|---|---|---|
| title | St George's Castle | Castelo de São Jorge |
| subtitle | Medieval castle and royal palace | Castelo medieval e paço real |
| image `castle_hero.jpg` caption | The towers and walls of St George's Castle, on the highest hill of Lisbon. Photo, 2011. | As torres e as muralhas do Castelo de São Jorge, na colina mais alta de Lisboa. Fotografia, 2011. |
| credit | deror_avi, CC BY-SA 3.0 | deror_avi, CC BY-SA 3.0 |

The split_then_now header pair: `castle_then.jpg` -- EN "The castle on its hill, on the Great Panorama of Lisbon, a tile panel from about 1700." PT "O castelo na sua colina, no Grande Panorama de Lisboa, um painel de azulejos de cerca de 1700." (credit RickMorais, CC BY-SA 4.0).
`castle_now.jpg` -- EN "The castle hill seen from the river today. Photo, 2009." PT "A colina do castelo vista do rio hoje. Fotografia, 2009." (credit Massimo Catarinella, CC BY 3.0).

## 2. quick_facts (blocks 6, 7, 8)

**block_6 chips -- "At a glance"** (PT "Num relance")

| label EN | value EN | label PT | value PT |
|---|---|---|---|
| Style | Medieval fortress | Estilo | Fortaleza medieval |
| Material | Limestone | Material | Calcário |
| Access | On foot, tram 28 nearby | Acesso | A pé, elétrico 28 por perto |

**block_7 hairline grid -- "Key dates"** (PT "Datas-chave")

| label EN | value EN | label PT | value PT |
|---|---|---|---|
| Taken | 1147 | Tomado | 1147 |
| Towers | 11 | Torres | 11 |
| Court left | c. 1500 | A corte saiu | c. 1500 |
| Rebuilt | 1938 to 1940 | Reconstruído | 1938 a 1940 |

**block_8 big numbers -- "In numbers"** (PT "Em números")

| label EN | value | label PT |
|---|---|---|
| towers | 11 | torres |
| years under the Portuguese crown | 800+ | anos sob a coroa portuguesa |

## 3. rich_text blocks (2, 3, 4, 5)

**block_2 plain -- "The story"** (3 min read). PT heading "A história".

EN
> [[Keep|keep]]s, towers and a long [[curtain wall|curtain wall]] crown the highest hill of Lisbon, with the whole river in view. People lived on this hilltop long before Portugal existed, and the Moors fortified it in the 11th century. The stone walls you see still follow the line of that old citadel.
>
> In 1147 Afonso Henriques, the first king of Portugal, besieged the town for about four months with the help of crusaders on their way east, and took it. The fortress then became a royal residence, the Alcáçova palace, and the kings lived inside its walls for centuries, until Manuel I moved the court down to the river around 1500.
>
> After that the castle served as a barracks and a prison, and the earthquake of 1755 shook much of it down. The walls you walk on today were largely rebuilt in the 1930s and 1940s, and the old palace buildings gave way to gardens and viewpoints.

PT-PT
> [[Torres de menagem|keep]], torres e uma longa [[cortina de muralhas|curtain wall]] coroam a colina mais alta de Lisboa, com todo o rio à vista. Viveu-se neste cume muito antes de Portugal existir, e os mouros fortificaram-no no século XI. As muralhas de pedra que hoje se veem seguem ainda o traçado dessa antiga cidadela.
>
> Em 1147, D. Afonso Henriques, o primeiro rei de Portugal, cercou a cidade durante cerca de quatro meses, com a ajuda de cruzados a caminho do Oriente, e tomou-a. A fortaleza passou então a residência real, o paço da Alcáçova, e os reis viveram dentro das suas muralhas durante séculos, até D. Manuel I mudar a corte para o rio por volta de 1500.
>
> Depois disso o castelo serviu de quartel e de prisão, e o terramoto de 1755 deitou abaixo grande parte dele. As muralhas onde hoje se anda foram em boa parte reconstruídas nas décadas de 1930 e 1940, e os antigos edifícios do paço deram lugar a jardins e miradouros.

(The first sentence uses the plural of keep; if the glossary link cannot take "Keeps", write "[[Keep|keep]] and walls" as today.)

**block_3 drop_cap -- heading EN "From fortress to palace" / PT "De fortaleza a paço"**

EN > Built by the Moors and taken in 1147, the castle became the home of the kings of Portugal. Its halls, gardens and courtyards held the court for the best part of four centuries, long before anyone thought of a palace by the river.

PT > Construído pelos mouros e tomado em 1147, o castelo tornou-se a casa dos reis de Portugal. As suas salas, jardins e pátios albergaram a corte durante quase quatro séculos, muito antes de alguém pensar num palácio junto ao rio.

**block_4 lede -- heading EN "Why it matters" / PT "Porque importa"**

EN > A castle that was a palace, a barracks and a prison, and is now a garden above the city.
>
> From its walls you read Lisbon in one glance: the river, the bridge, the straight streets of the Baixa laid out after 1755, and the tangled lanes of the Alfama that the earthquake spared.

PT > Um castelo que foi paço, quartel e prisão, e hoje é um jardim sobre a cidade.
>
> Das suas muralhas lê-se Lisboa de um só olhar: o rio, a ponte, as ruas direitas da Baixa traçadas depois de 1755 e as vielas emaranhadas da Alfama, que o terramoto poupou.

**block_5 sections -- heading EN "Through the centuries" / PT "Ao longo dos séculos"**, intro (4 min read)

| | EN | PT-PT |
|---|---|---|
| body | Three moments of the castle, as the chronicles tell them. | Três momentos do castelo, contados pelas crónicas. |
| 1 title | The siege of 1147 | O cerco de 1147 |
| 1 body | The crusaders camped below the walls for four months, and the town on the hill finally gave in. | Os cruzados acamparam sob as muralhas durante quatro meses, e a cidade da colina acabou por se render. |
| 2 title | The royal palace | O paço real |
| 2 body | Kings lived here until Manuel I built his new palace by the river; the [[barbican]] guarded their gate. | Os reis viveram aqui até D. Manuel I construir o seu novo paço junto ao rio; a [[barbacã&#124;barbican]] guardava a sua porta. |
| 3 title | After the earthquake | Depois do terramoto |
| 3 body | The earthquake of 1755 left the keep standing but brought down much of the palace; the walls were rebuilt in the 1900s. | O terramoto de 1755 deixou a torre de menagem de pé, mas deitou abaixo grande parte do paço; as muralhas foram reconstruídas no século XX. |

(In the PT barbican cell `&#124;` stands for the glossary separator `|` -- the markup is `[[barbacã|barbican]]` -- because a table cell cannot hold a bare pipe.)

## 4. Timelines

**block_22 vertical -- six events** (2 min). Heading "Timeline" / "Cronologia". `highlight_now` stays on.

| date | title EN | text EN | title PT | text PT |
|---|---|---|---|---|
| 11th c. / séc. XI | The Moorish citadel | The Moors fortify the hilltop above the river town. | A cidadela moura | Os mouros fortificam o cume sobre a cidade ribeirinha. |
| 1147 | The siege | After about four months the town on the hill is taken by Afonso Henriques and the crusaders. | O cerco | Ao fim de cerca de quatro meses, a cidade da colina é tomada por D. Afonso Henriques e pelos cruzados. |
| c. 1300 | A royal palace | The kings make the castle their home; the Alcáçova palace grows inside the walls. | Um paço real | Os reis fazem do castelo a sua casa; o paço da Alcáçova cresce dentro das muralhas. |
| c. 1500 | The court leaves | Manuel I moves to a new palace by the river and the castle loses its royal role. | A corte sai | D. Manuel I muda-se para um novo paço junto ao rio e o castelo perde a função real. |
| 1755 | The earthquake | Much of the old palace falls; the keep and the walls stand. | O terramoto | Grande parte do velho paço cai; a torre de menagem e as muralhas resistem. |
| 1938 to 1940 | Rebuilt | The walls are rebuilt and the castle opens as a monument. | Reconstruído | As muralhas são reconstruídas e o castelo abre como monumento. |

**block_23 horizontal -- four events** (heading EN "Timeline at a glance" / PT "Cronologia num relance"): 1147 The siege / O cerco; 1511 The court moves to the river / A corte muda-se para o rio; 1755 The earthquake / O terramoto; 1940 Rebuilt / Reconstruído.
(block_23 says 1511, block_22 says c. 1500: use "c. 1500" in both when P1b imports; "1511" is only the end of the move.)

**block_26 story_chapters -- "The siege, in chapters"** (5 min): keep today's three chapters (The siege / The palace / The earthquake); they are already real-shaped (56 to 85 characters each). Optional longer wording of chapter 1:
EN "Ships of crusaders stopped in the river and joined the king's army outside the walls. For four months they held the town in a ring." PT "Navios de cruzados pararam no rio e juntaram-se ao exército do rei junto às muralhas. Durante quatro meses mantiveram a cidade cercada."

## 5. Pictures and captions

**block_29 gallery carousel / block_30 grid / block_31 filmstrip / block_32 stack** -- the same four pictures in all four looks:

| # | file | shape | caption EN | caption PT | credit |
|---|---|---|---|---|---|
| 1 | `castle_gallery_1.jpg` | landscape 4:3 | The cobbled approach to the castle, with the walls and a tower ahead. Photo, 2010. | A subida empedrada ao castelo, com as muralhas e uma torre em frente. Fotografia, 2010. | Ricardo Tulio Gandelman, CC BY 2.0 |
| 2 | `castle_gallery_2.jpg` | landscape 4:3 | The river and the 25 de Abril Bridge, seen from the castle walls. Photo, 2014. | O rio e a Ponte 25 de Abril, vistos das muralhas do castelo. Fotografia, 2014. | jad99, CC BY-SA 2.0 |
| 3 | `castle_gallery_3.jpg` | PORTRAIT 3:4 | The baroque Dom Fradique Gate, in the old town below the castle. Photo, 2009. | A Porta de Dom Fradique, barroca, na cidade velha por baixo do castelo. Fotografia, 2009. | to lobato, CC BY 2.0 |
| 4 | `castle_gallery_4.jpg` | landscape 4:3 | A tower and the wall walk against an evening sky. Photo, 2009. | Uma torre e o caminho de ronda contra o céu do fim da tarde. Fotografia, 2009. | Robert Nyman, CC BY 2.0 |

Extras for the fun fact and the video posters: `castle_peacock.jpg` -- EN "A peacock in the castle gardens. Photo, 2017." PT "Um pavão nos jardins do castelo. Fotografia, 2017." (Dguendel, CC BY 4.0). `castle_photo_1.jpg` -- EN "Towers and wall walks on the way to the keep. Photo, 2013." PT "Torres e caminhos de ronda a caminho da torre de menagem. Fotografia, 2013." (Aleksandr Zykov, CC BY-SA 2.0). `castle_photo_2.jpg` -- EN "Steps and walls climbing to the keep. Photo, 2013." PT "Escadas e muros a subir para a torre de menagem. Fotografia, 2013." (Peter, CC BY-SA 2.0).

**block_33 before_after.** Today's heading "What the earthquake took" and label "After the earthquake" fit a ruin picture. The pair supplied is the 1755 palace painting and the square today, so the Lamp block should show `paco_before_1755.jpg` / `paco_after_today.jpg` with labels "Before 1755" / "Today" (PT "Antes de 1755" / "Hoje") and heading EN "The river front, then and now" / PT "A frente ribeirinha, antes e agora". Caption and credits: see PACO_RIBEIRA_text.md section 8.

**block_34 zoom_image -- heading EN "Look closer" / PT "Veja de perto"**: `azulejo_detail.jpg`, caption EN "Ships at the Ribeira das Naus on the Great Panorama of Lisbon, a tile panel from about 1700." PT "Navios na Ribeira das Naus, no Grande Panorama de Lisboa, um painel de azulejos de cerca de 1700." (credit RickMorais, CC BY-SA 4.0). No "pinch" in the caption.

**block_35 hotspot_image numbered / block_36 loupes -- the castle on the tile panel** (`castle_then.jpg`, 2048x1536, five features; x, y fractions)

| # | x | y | title EN | text EN | title PT | text PT |
|---|---|---|---|---|---|---|
| 1 | 0.40 | 0.09 | The castle on its hill | The fortress crowns the hilltop: on the tile panel a cluster of towers and roofs above the town. | O castelo na sua colina | A fortaleza coroa o cume: no painel de azulejos, um conjunto de torres e telhados sobre a cidade. |
| 2 | 0.73 | 0.06 | A tall tower | A tall tower closes the castle on the right. | Uma torre alta | Uma torre alta fecha o castelo à direita. |
| 3 | 0.10 | 0.28 | The wall on the slope | The curtain wall runs down the hillside and ties the castle to the town below. | A muralha na encosta | A cortina de muralhas desce a encosta e liga o castelo à cidade lá em baixo. |
| 4 | 0.44 | 0.66 | The cathedral | Twin towers that match the Sé, the cathedral of Lisbon, one of the oldest churches in the city. | A catedral | Duas torres gémeas que correspondem à Sé, a catedral de Lisboa, uma das igrejas mais antigas da cidade. |
| 5 | 0.88 | 0.35 | A bell tower | A bell tower rises over the houses of the lower town. | Uma torre sineira | Uma torre sineira ergue-se sobre as casas da cidade baixa. |

heading EN "Explore the panel" / PT "Explore o painel" (block_35), EN "The towers up close" / PT "As torres de perto" (block_36): keep.

**block_39 / block_40 today_map** -- `castle_map.jpg` (768x432, OpenStreetMap, pin on the castle, attribution drawn on the picture). Heading of block_40 EN "From the panel to today" / PT "Do painel a hoje"; caption EN "The castle in the old town today. Map data from OpenStreetMap." PT "O castelo na cidade velha hoje. Dados do mapa do OpenStreetMap." (the picture carries "© OpenStreetMap contributors"). lat 38.7139, lng -9.1334 (already in the block).

## 6. Fun facts (blocks 9 flip, 10 postcard) -- with headings

| # | block | heading EN | text EN | heading PT | text PT |
|---|---|---|---|---|---|
| 1 | 9 flip | Peacocks in the gardens | Peacocks have lived in the castle gardens for longer than most visitors can remember. | Pavões nos jardins | Há pavões nos jardins do castelo desde que a maior parte dos visitantes se lembra. |
| 2 | 9 flip | A periscope in a tower | The Ulysses Tower holds a camera obscura: a mirror and a lens that show the city moving on a white dish. | Um periscópio numa torre | A Torre de Ulisses tem uma câmara escura: um espelho e uma lente que mostram a cidade em movimento num prato branco. |
| 3 | 10 postcard | The knight in the door | A legend says the knight Martim Moniz died holding a castle gate open for the Christian army in 1147. The small gate still carries his name. | O cavaleiro na porta | Diz a lenda que o cavaleiro Martim Moniz morreu a segurar uma porta do castelo aberta para o exército cristão em 1147. A pequena porta ainda leva o seu nome. |

(Today's fixture fact "the keep has no door at ground level" is dropped: it could not be checked. Today's "painter left out a tower" joke is
about the invented panel.)

## 7. Persons (blocks 24, 25) and the pull quotes (blocks 11, 12)

| block | heading EN | name | role EN | text EN |
|---|---|---|---|---|
| 24 row | Who took it | Afonso Henriques | First king of Portugal | He took the town and its castle in 1147, with the help of crusaders. |
| 25 card | The king who moved out | Manuel I | King of Portugal, 1495 to 1521 | He left the castle to the garrison around 1500 and built his palace where the ships came in. |

| block | heading PT | role PT | text PT |
|---|---|---|---|
| 24 | Quem o tomou | Primeiro rei de Portugal | Tomou a cidade e o seu castelo em 1147, com a ajuda dos cruzados. |
| 25 | O rei que saiu | Rei de Portugal, 1495 a 1521 | Deixou o castelo à guarnição por volta de 1500 e construiu o seu paço onde chegavam os navios. |

Curator line: EN "Texts and pictures chosen for the TileStories test wall. Draft content, approximate." PT "Textos e imagens escolhidos para a parede de teste TileStories. Conteúdo provisório, aproximado."

**Pull quotes (blocks 11, 12): NOT rewritten.** Today's quote is attributed to "Osbern, a crusader priest, Letter on the conquest, 1147" and block_12 to "A traveller": the words are invented. A real, checked quote needs the source text in hand, which P1a did not gather. Recommendation for P1b: keep the fixture quote only while the wall is a fixture, and drop the attribution to a real person, or replace it with a quote copied from a public-domain translation with its page cited.

## 8. Quizzes (blocks 43 to 45) -- real questions, plausible wrong answers

**block_43 multiple choice**

1. Q EN: In which year did Afonso Henriques take the castle?  PT: Em que ano tomou D. Afonso Henriques o castelo?
   a) 1095 b) 1147 (correct) c) 1255
   Explanation EN: The siege of Lisbon lasted about four months and ended in the autumn of 1147. PT: O cerco de Lisboa durou cerca de quatro meses e terminou no outono de 1147.
2. Q EN: What is the keep of a castle?  PT: O que é a torre de menagem de um castelo?
   a) The gate tower b) The strongest tower, the last refuge (correct) c) The tower with the bell
   Explanation EN: When the walls fell, the defenders retreated into the keep. PT: Quando as muralhas caíam, os defensores recolhiam à torre de menagem.
3. Q EN: Which animals walk freely in the castle gardens?  PT: Que animais andam à solta nos jardins do castelo?
   a) Peacocks (correct) b) Flamingos c) Swans
   Explanation EN: Peacocks have lived in the gardens for a very long time. PT: Há pavões nos jardins há muito tempo.

**block_44 true / false swipe**

1. EN "A curtain wall joins the towers of a castle." PT "Uma cortina de muralhas liga as torres de um castelo." -> True. Explanation EN: It runs between the towers and closes the ring. PT: Corre entre as torres e fecha o anel.
2. EN "The castle was never used as a prison." PT "O castelo nunca serviu de prisão." -> False. Explanation EN: After the court left it served as a barracks and a prison. PT: Depois de a corte sair, serviu de quartel e de prisão.

**block_45 image choice**: Q EN "Which picture shows the river front BEFORE the earthquake?" PT "Qual das imagens mostra a frente ribeirinha ANTES do terramoto?" options `paco_before_1755.jpg` ("The painting" / "A pintura") and `paco_after_today.jpg` ("The photograph" / "A fotografia"), correct 1. Explanation EN "The painting still shows the palace and its domed tower." PT "A pintura ainda mostra o paço e o seu torreão."

(The poll, dialogue, size_comparison, feedback, collect and process_steps / swatches blocks are about invented fixtures -- a tile panel -- and are short UI text; they are left as they are in `config.json`.)

## 9. practical_info (block_28)

| icon | label EN | value EN | label PT | value PT |
|---|---|---|---|---|
| time | Open | Every day, 9:00 to 21:00 (summer); 9:00 to 19:00 (winter) | Aberto | Todos os dias, 9:00 às 21:00 (verão); 9:00 às 19:00 (inverno) |
| ticket | Tickets | About 15 EUR for adults (check the current price) | Bilhetes | Cerca de 15 EUR para adultos (confirme o preço atual) |
| access | Getting in | Steep streets; tram 28 stops near the gate | Acesso | Ruas íngremes; o elétrico 28 pára perto da porta |
| location | Where | Upper old town | Onde | Alta da cidade velha |
| light | Best light | Late afternoon | Melhor luz | Fim da tarde |
| info | Good to know | Bring a jacket: the walls are windy | Bom saber | Traga um casaco: as muralhas têm vento |

## 10. Video and audio blocks

| block | title EN | title PT |
|---|---|---|
| block_56 video inline | Lisbon, the square and the castle | Lisboa, a praça e o castelo |
| block_57 video chapters | The square and the castle, chapter by chapter | A praça e o castelo, capítulo a capítulo |
| block_54 / 55 audio_guide | Guided tour of the castle | Visita guiada ao castelo |

Video: `video/lisbon_square_castle_55s.mp4` (55 s, 854x480, with the original ambient sound), captions `video/lisbon_square_castle_55s.vtt` (EN) and
`..._pt.vtt` (PT): they DESCRIBE what the picture shows (the clip has no narration to transcribe), poster `video/lisbon_square_castle_poster.jpg`.
Chapters for the clip: 0:00 "The square" / "A praça", 0:20 "The view" / "A vista", 0:28 "In the castle" / "No castelo".
Audio: `audio/guide_castle_en.*`, `audio/guide_castle_pt.*` (section "Audio" of CONTENT_MAP).

## 11. block_15 / block_16 `sources`

| title EN | author | licence | title PT |
|---|---|---|---|
| The Great Panorama of Lisbon, castle detail (tile panel, photo) | RickMorais | CC BY-SA 4.0 | O Grande Panorama de Lisboa, pormenor do castelo (painel de azulejos, fotografia) |
| The castle: photographs | deror_avi; Ricardo Tulio Gandelman; jad99; to lobato; Robert Nyman; Dguendel; Aleksandr Zykov; Peter; Massimo Catarinella | CC BY / CC BY-SA (each file in SOURCES.md) | O castelo: fotografias |
| Lisbon, the square and the castle (video, three clips joined) | deror_avi | CC BY-SA 3.0 | Lisboa, a praça e o castelo (vídeo, três clips reunidos) |
| Map | OpenStreetMap contributors | ODbL | Mapa |
| Text | Written for this wall from general historical knowledge, in the writers' own words | -- | Texto escrito para esta parede a partir de conhecimento histórico geral, com palavras próprias |

Today's two rows ("Letter on the conquest of Lisbon" by Osbern, "inventory sheet 14" of the National Tile Museum, "With permission") are
fixtures and say things P1a could not verify; the rows above are true of the files the wall now holds.
