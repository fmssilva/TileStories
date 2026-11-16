/* The demo point's content: its card's blocks (S.card), its story rows (S.story), the demo texts (DEMO), the wall's points (POIS). */
/* the card of the marker: blocks in order with their preview settings */
S.card=[
 {id:"b_facts",kind:"Quick Facts",grp:"about",enter:"family",tap:"nothing",orow:"plain"},
 {id:"b_fun",kind:"Inline",grp:"about",pattern:"pt6",enter:"pop",tap:"nothing",orow:"plain"},
 {id:"b_story",kind:"Story",grp:"stories",enter:"family",tap:"page",orow:"nudge",preview:"auto"},
 {id:"b_ba",kind:"Before and After",grp:"media",enter:"fade",tap:"nothing",orow:"plain"},
 {id:"b_gal",kind:"Gallery",grp:"media",enter:"slide",tap:"page",orow:"plain"},
 {id:"b_audio",kind:"Audio Guide",grp:"listen",enter:"family",tap:"nothing",orow:"plain"},
 {id:"b_quiz",kind:"Knowledge Check",grp:"play",enter:"tilt",tap:"nothing",orow:"plain"},
 {id:"b_info",kind:"Practical Info",grp:"visit",enter:"family",tap:"expand",orow:"plain"}];
/* the story page (place: content only) */
S.story=[
 {id:"r1",pattern:"pt1",slots:[{img:"g3",cap:T("The Terreiro do Paço in 1662, the palace along the river front.","O Terreiro do Paço em 1662, o palácio ao longo da frente ribeirinha."),credit:"Dirk Stoop"},{text:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755")},{text:T("Two hundred and fifty years by the river","Duzentos e cinquenta anos à beira-rio")},{text:T("For about 250 years the kings of Portugal lived by the river, in a palace that opened straight onto the Tagus.","Durante cerca de 250 anos, os reis de Portugal viveram à beira do rio, num palácio aberto diretamente para o Tejo.")}]},
 {id:"r2",kind:"paragraph",style:"st9",c:{text:T("When King Manuel I found the old castle on the hill too cramped, he moved the court down to the docks around 1500 and filled the new palace with the wealth of the voyages to India.","Quando D. Manuel I achou o velho castelo da colina demasiado apertado, mudou a corte para junto das docas por volta de 1500 e encheu o novo palácio com a riqueza das viagens à Índia.")}},
 {id:"r3",kind:"heading",style:null,c:{text:T("Two centuries of additions","Dois séculos de acrescentos")}},
 {id:"r4",kind:"paragraph",style:null,c:{text:T("Later kings kept adding to it. In the 1580s, under Philip I, who ruled Portugal and Spain together, the river front received a new wing and its corner tower, the Torreão.","Os reis seguintes foram acrescentando obra. Na década de 1580, com Filipe I, que reinava em Portugal e em Espanha, a frente ribeirinha ganhou uma nova ala e o seu torreão de canto, o Torreão.")}},
 {id:"r5",pattern:"pt3",slots:[{text:T("What was inside","O que havia lá dentro")},{title:T("A royal city","Uma cidade real"),text:T("A library of some 70,000 books, paintings and maps, the royal archive, a chapel, offices and workshops and, next door, the new opera house.","Uma biblioteca com cerca de 70 000 livros, pinturas e mapas, o arquivo real, uma capela, gabinetes e oficinas e, ao lado, a nova ópera.")}]},
 {id:"r6",pattern:"pt7",slots:[{text:T("Then and now","Antes e agora")},{before:"before",after:"g2"},{text:T("Drag across: the palace before 1755, and the square that took its place.","Arraste: o palácio antes de 1755 e a praça que ficou no seu lugar.")}]},
 {id:"r7",pattern:"pt2",slots:[{},{text:T("Where the palace had stood, an open square was laid out to the river.","Onde estivera o palácio, abriu-se uma praça para o rio."),cite:"The Pombaline plan, 1758"},{}]},
 {id:"r8",kind:"heading",style:"st6",c:{text:T("One morning in November","Uma manhã de novembro")}},
 {id:"r9",kind:"paragraph",style:null,c:{text:T("On the morning of 1 November 1755, All Saints' Day, an earthquake, a fire and a tsunami destroyed the palace, its library and the opera house in a few hours.","Na manhã de 1 de novembro de 1755, Dia de Todos os Santos, um terramoto, um incêndio e um maremoto destruíram em poucas horas o palácio, a biblioteca e a ópera.")}},
 {id:"r10",kind:"carousel",style:"st27",c:{imgs:["g1","g2","g3"],cap:T("The square through three centuries.","A praça ao longo de três séculos.")}},
 {id:"r11",kind:"video",style:null,c:{img:"hero",cap:T("From the square to the castle · 0:55","Da praça ao castelo · 0:55")}}];
var DEMO={title:T("Two hundred and fifty years by the river","Duzentos e cinquenta anos à beira-rio"),eyebrow:T("Lisbon · c. 1500 to 1755","Lisboa · c. 1500 a 1755"),heading:T("Two centuries of additions","Dois séculos de acrescentos"),
 lede:T("For about 250 years the kings of Portugal lived by the river, in a palace that opened straight onto the Tagus.","Durante cerca de 250 anos, os reis de Portugal viveram à beira do rio, num palácio aberto diretamente para o Tejo."),
 paragraph:T("When King Manuel I found the old castle on the hill too cramped, he moved the court down to the docks and filled the new palace with the wealth of the voyages.","Quando D. Manuel I achou o velho castelo no alto da colina demasiado apertado, mudou a corte para junto das docas e encheu o novo palácio com a riqueza das viagens."),
 ctitle:T("What was inside","O que havia lá dentro"),ctext:T("A library of some 70,000 books, the royal archive, a chapel and, next door, the new opera house.","Uma biblioteca com cerca de 70 000 livros, o arquivo real, uma capela e, ao lado, a nova ópera."),
 back:T("The opera house opened in March 1755 and stood for only seven months.","A ópera abriu em março de 1755 e durou apenas sete meses."),
 quote:T("Where the palace had stood, an open square was laid out to the river.","Onde estivera o palácio, abriu-se uma praça para o rio."),cite:"The Pombaline plan, 1758",
 list:{en:["The royal library","The chapel","The opera house, opened in 1755"],pt:["A biblioteca real","A capela","A ópera, inaugurada em 1755"]},
 piccap:T("The Terreiro do Paço in 1662.","O Terreiro do Paço em 1662."),paircap:T("Before 1755 and today.","Antes de 1755 e hoje."),vidcap:T("From the square to the castle · 0:55","Da praça ao castelo · 0:55")};
var POIS=[["lamp","1. St George's Castle"],["cathedral","2. Lisbon Cathedral"],["painting","7. The Ribeira Palace"],["camera","13. The Camera"]];
S.card.forEach(function(b){b.grp="inherit";b.enter="inherit";b.tap="inherit";b.orow="inherit"});
(function(){var i=S.card.findIndex(function(b){return b.id==="b_audio"});if(i>0){var a=S.card.splice(i,1)[0];S.card.splice(1,0,a)}})();
/* guidance round 16: the words each block kind shows on the demo point's card, one place for them: the card's preview draws them
   (card/blocks.js's KPREV) and the guide's automatic tour says them (guide/autotour.js, through blockSays). A block's own words (b.c, the
   wall's other points below) come before its kind's. A picture's or a clip's words are its element's demo content (S.demoC). */
/* round 17: each picture's own caption (the gallery's page and the automatic tour's Tell me more: "In the other pictures: …"); Unity: the
   picture's caption field on the Gallery block */
var PIC_CAPS={g1:T("A painting of the square with the palace and its corner tower by the river","Uma pintura da praça com o paço e o seu torreão junto ao rio"),
  g2:T("An engraving of the ruins the earthquake left","Uma gravura das ruínas que o terramoto deixou"),
  g3:T("A drawing of fireworks over a garden laid out on the square","Um desenho de fogo de artifício sobre um jardim desenhado na praça"),
  before:T("The palace as it looked before 1755","O paço tal como era antes de 1755")};
var KWORDS={
 "Quick Facts":{facts:[[UI.home,UI.homev],[UI.lost,UI.lostv]],cat:T("Royal palace","Palácio real")},
 "Inline":{text:T("The palace had its own opera house, opened in March 1755.","O palácio tinha a sua própria ópera, inaugurada em março de 1755."),back:T("It stood for only seven months.","Durou apenas sete meses.")},
 "Knowledge Check":{q:T("Which king moved the court to the river?","Que rei mudou a corte para o rio?"),choices:["Manuel I","Philip I","José I"],ok:0},
 "Practical Info":{rows:[[T("Where","Onde"),T("Praça do Comércio","Praça do Comércio")],[T("Today","Hoje"),T("An open square, always","Praça aberta, sempre")],[T("Metro","Metro"),T("Terreiro do Paço","Terreiro do Paço")]]},
 "Timeline":{events:[["c. 1500",T("The court moves to the river","A corte muda-se para o rio"),"g3"],["1580s",T("The Torreão rises","Ergue-se o Torreão"),"before"],["1755",T("Earthquake, fire and tsunami","Terramoto, incêndio e maremoto"),"g1"]],now:T("The Praça do Comércio","A Praça do Comércio")},
 "Gallery":{cap:T("The square through three centuries.","A praça ao longo de três séculos."),n:4},
 "Dialogue":{lines:[T("Your Majesty, the river front is finished.","Majestade, a frente do rio está concluída."),T("And the tower?","E o torreão?")]},
 "Poll":{q:T("Should the opera house be rebuilt?","Devia a ópera ser reconstruída?"),choices:[T("Yes","Sim"),T("No","Não"),T("Only as a museum","Só como museu")]},
 "Status":{state:T("Lost","Perdido"),year:"1755"},
 "Compare Points":{points:["Ribeira Palace","Lisbon Cathedral"]}};
/* the wall's other points with a card of their own (guidance round 16, the developer: every point gets a tour from its card): the
   Torreão, the Terreiro do Paço, the Tagus and its ships. Each has its hero and its blocks with their own words (b.c); the palace's are
   S.card, S.hero and S.story, kept here while another point is open. Opening a point (its pin, Azu's remark, A tour of {point}, the
   marker, Specific Marker › Marker) is pointOpen: the card, the hero and the story become that point's, S.m.poi names it, and
   point.opened tells the guide (its tour). Unity: each marker's Detail Card asset; opening one is the card controller's Show(marker).
   Guidance round 16b: each hero's picture is a crop of the wall painting around its own point (crop: the wall, the point, the crop's
   height, where the point sits; drawn by card/hero.js's heroPaint; Unity: a uvRect on the wall's texture) */
function pointBlock(id,kind,c,o){return Object.assign({id:id,kind:kind,grp:"inherit",enter:"inherit",tap:"inherit",orow:"inherit",c:c},o||{})}
var POINTS={painting:{label:"7. The Ribeira Palace"},
 torreao:{label:"The Torreão",hero:{title:T("The Torreão","O Torreão"),eyebrow:T("The Ribeira Palace · 1580s to 1755","Paço da Ribeira · 1580 a 1755"),line:T("The palace's corner tower, on the water","A torre de canto do palácio, sobre a água"),fx:30,fy:40,pic:"g1",pic2:"before",crop:{wall:"ribeira",at:"torreao",h:.9,fy:.36}},story:[],
  card:[pointBlock("t_facts","Quick Facts",{facts:[[T("Built","Construído"),T("In the 1580s","Na década de 1580")],[T("Under","No reinado de"),T("Philip I","Filipe I")],[T("Lost","Perdido"),T("1 November 1755","1 de novembro de 1755")]]}),
   pointBlock("t_text","Inline",{title:T("On the river front","Na frente ribeirinha"),text:T("In the 1580s, under Philip I, who ruled Portugal and Spain together, the river front received a new wing and its corner tower, the Torreão. In the paintings of the river front, it is the tower with the dome at the corner of the palace, right on the water.","Na década de 1580, com Filipe I, que reinava em Portugal e em Espanha, a frente ribeirinha ganhou uma nova ala e o seu torreão de canto, o Torreão. Nas pinturas da frente ribeirinha, é a torre com a cúpula no canto do palácio, mesmo sobre a água.")}),
   pointBlock("t_time","Timeline",{events:[["c. 1500",T("The court moves to the river","A corte muda-se para o rio"),"g3"],["1580s",T("The Torreão rises on the river front","O Torreão ergue-se na frente ribeirinha"),"before"],["1755",T("Lost to the earthquake, the fire and the tsunami","Perdido no terramoto, no incêndio e no maremoto"),"g1"]]}),
   pointBlock("t_gal","Gallery",{imgs:["g3","before"],n:2,cap:T("The Torreão at the palace's corner, by the water.","O Torreão no canto do palácio, junto à água.")}),
   pointBlock("t_quiz","Knowledge Check",{q:T("Under which king did the Torreão rise?","Em que reinado se ergueu o Torreão?"),choices:[T("Philip I","Filipe I"),T("Manuel I","D. Manuel I"),T("José I","D. José I")],ok:0}),
   pointBlock("t_info","Practical Info",{rows:[[T("Where","Onde"),T("Praça do Comércio, the river side","Praça do Comércio, do lado do rio")]]})]},
 square:{label:"Terreiro do Paço",hero:{title:T("Terreiro do Paço","O Terreiro do Paço"),eyebrow:T("Lisbon · the palace square","Lisboa · a praça do paço"),line:T("The square by the palace, open to the river","A praça junto ao paço, aberta para o rio"),fx:50,fy:60,pic:"g1",pic2:"g3",crop:{wall:"ribeira",at:"square",h:.9,fy:.5}},story:[],
  card:[pointBlock("q_facts","Quick Facts",{facts:[[T("Laid out","Aberto"),T("Around 1500, with the palace","Por volta de 1500, com o paço")],[T("Today","Hoje"),T("Praça do Comércio","Praça do Comércio")]]}),
   pointBlock("q_text","Inline",{title:T("Its name","O seu nome"),text:T("Terreiro means an open yard: this was the palace's yard, between the royal palace and the city. After 1755 it was rebuilt as the Praça do Comércio, with arcades on three sides and the river on the fourth.","Terreiro quer dizer um largo aberto: era o largo do paço, entre o palácio real e a cidade. Depois de 1755 foi reconstruído como Praça do Comércio, com arcadas em três lados e o rio no quarto.")}),
   pointBlock("q_gal","Gallery",{imgs:["g1","g3"],n:2,cap:T("The square beside the palace, before 1755.","A praça junto ao paço, antes de 1755.")}),
   pointBlock("q_time","Timeline",{events:[["c. 1500",T("The square opens beside the palace","A praça abre-se junto ao paço"),"g3"],["1755",T("The earthquake destroys the palace","O terramoto destrói o paço"),"g1"],["1758",T("A new plan lays the square out to the river","Um novo plano abre a praça para o rio"),"before"]]}),
   pointBlock("q_quiz","Knowledge Check",{q:T("What is the square called today?","Como se chama hoje a praça?"),choices:["Praça do Comércio","Rossio","Praça da Figueira"],ok:0}),
   pointBlock("q_info","Practical Info",{rows:[[T("Where","Onde"),T("Baixa, by the river","Baixa, junto ao rio")],[T("Metro","Metro"),T("Terreiro do Paço","Terreiro do Paço")]]})]},
 river:{label:"The Tagus and its ships",hero:{title:T("The Tagus and its ships","O Tejo e os seus navios"),eyebrow:T("Lisbon · the river front","Lisboa · a frente ribeirinha"),line:T("Where the ships of the voyages came in","Onde chegavam os navios das viagens"),fx:20,fy:60,pic:"g1",pic2:"before",crop:{wall:"ribeira",at:"river",h:.9,fy:.42}},story:[],
  card:[pointBlock("v_text","Inline",{title:T("By the docks","Junto às docas"),text:T("Around 1500, King Manuel I moved the court down to the docks, and filled the new palace with the wealth of the voyages to India. The palace opened straight onto the Tagus.","Por volta de 1500, D. Manuel I mudou a corte para junto das docas e encheu o novo palácio com a riqueza das viagens à Índia. O palácio abria diretamente para o Tejo.")}),
   pointBlock("v_facts","Quick Facts",{facts:[[T("River","Rio"),T("The Tagus","O Tejo")],[T("In the paintings","Nas pinturas"),T("Ships at anchor off the palace","Navios fundeados diante do paço")]]}),
   pointBlock("v_time","Timeline",{events:[["c. 1500",T("The court moves down to the docks","A corte muda-se para junto das docas"),"g3"],["1755",T("A tsunami comes up the river","Um maremoto sobe o rio"),"g1"]]}),
   pointBlock("v_gal","Gallery",{imgs:["before","g1"],n:2,cap:T("Ships on the Tagus, off the palace.","Navios no Tejo, diante do paço.")}),
   pointBlock("v_quiz","Knowledge Check",{q:T("Which river runs past the palace?","Que rio passa diante do paço?"),choices:[T("The Tagus","O Tejo"),T("The Douro","O Douro"),T("The Mondego","O Mondego")],ok:0})]}};
function pointOpen(id){if(!POINTS[id])return false;var cur=S.m&&S.m.poi||"painting",p=POINTS[cur];if(p){p.card=S.card;p.hero=S.hero;p.story=S.story}
  if(cur===id)return true;var n=POINTS[id];S.card=n.card;S.hero=n.hero;S.story=n.story;S.m.poi=id;S.sel.cblock=null;emit("point.opened",id,cur);return true}
/* a point's card, open or kept: its blocks, its hero and its story */
function pointOf(id){id=id||S.m&&S.m.poi||"painting";return id===(S.m&&S.m.poi||"painting")?{card:S.card,hero:S.hero,story:S.story,id:id}:Object.assign({id:id},POINTS[id]||{})}
