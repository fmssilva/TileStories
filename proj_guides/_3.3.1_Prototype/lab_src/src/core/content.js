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
var POIS=[["lamp","1. St George's Castle"],["cathedral","2. Lisbon Cathedral"],["painting","7. Ribeira Palace"],["camera","13. The Camera"]];
S.card.forEach(function(b){b.grp="inherit";b.enter="inherit";b.tap="inherit";b.orow="inherit"});
(function(){var i=S.card.findIndex(function(b){return b.id==="b_audio"});if(i>0){var a=S.card.splice(i,1)[0];S.card.splice(1,0,a)}})();
