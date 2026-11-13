/* The tile of light's styles: glaze, pattern, light, corners, and its preview. */
var AZU_GLAZES={
  cobalt:{cream:[.96,.93,.87],ink:[.12,.31,.61],ink2:[.12,.31,.61]},
  manganese:{cream:[.96,.93,.88],ink:[.37,.2,.33],ink2:[.37,.2,.33]},
  polychrome:{cream:[.97,.94,.85],ink:[.12,.31,.61],ink2:[.88,.62,.13]}};
var AZU_MOTIFS={rosette:0,diamond:1,sprigs:2,plain:3};
var AZU_CORNERS={crisp:.03,soft:.12,pillow:.22};
/* the light's four colours; "glaze" lifts the glaze's own colours towards light */
var AZU_LIGHTS={pastel:[[.48,.86,1],[.66,.52,1],[1,.55,.84],[1,.8,.66]],cobalt:[[.5,.78,1],[.25,.42,.95],[.8,.92,1],[.38,.6,1]],gold:[[1,.84,.42],[1,.68,.3],[1,.95,.78],[.96,.58,.32]]};
S.guideCfg.azuGlaze="cobalt";S.guideCfg.azuMotif="rosette";S.guideCfg.azuLight="pastel";S.guideCfg.azuShape="soft";
function azuLift(c){return c.map(function(v){return Math.min(1,v*.55+.5)})}
/* the Editor's choices become the shader's properties (the defaults give mascot/azu-gl.js's look) */
function azuApply(){var c=S.guideCfg,g=AZU_GLAZES[c.azuGlaze]||AZU_GLAZES.cobalt,l=c.azuLight==="glaze"?[azuLift(g.ink),g.ink.map(function(v){return Math.min(1,v*1.6+.08)}),azuLift(g.ink2),g.cream]:(AZU_LIGHTS[c.azuLight]||AZU_LIGHTS.pastel);
  AZU_UNI.uCream=g.cream;AZU_UNI.uInk=g.ink;AZU_UNI.uInk2=g.ink2;AZU_UNI.uMotif=AZU_MOTIFS[c.azuMotif]||0;AZU_UNI.uRound=AZU_CORNERS[c.azuShape]||.12;
  AZU_UNI.uP0=l[0];AZU_UNI.uP1=l[1];AZU_UNI.uP2=l[2];AZU_UNI.uP3=l[3]}
azuApply();
/* they change the shader's picture, not the page's: behaviour for the self-test */
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guideCfg\\.(azuGlaze|azuMotif|azuLight|azuShape)|"));

/* Editor: Guide tab > Azu > Mascot source = Tile of light: Glaze, Pattern, Light, Corners, Preview */
var azuPrev={key:null};
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;azuApply();
  if(S.guideCfg.azuStyle==="light"){var r=rowEl(d,"s7mascot","Mascot source");
    /* drawn again only when the style changes (an Editor render is not a reason to draw) */
    var key=[S.guideCfg.azuGlaze,S.guideCfg.azuMotif,S.guideCfg.azuLight,S.guideCfg.azuShape].join("|");
    if(azuPrev.key!==key){azuPrev={key:key,rest:AzuGL.still(0),lit:AzuGL.still(1)}}var rest=azuPrev.rest,lit=azuPrev.lit;
    if(r)r.insertAdjacentHTML("afterend",
      row("Glaze",pseg("guideCfg.azuGlaze",["cobalt","manganese","polychrome"],["Cobalt","Manganese","Polychrome"]),"The tile's colours, after the azulejo traditions: Cobalt, blue on tin-white (the 17th and 18th centuries); Manganese, aubergine on white; Polychrome, blue with an ochre accent (the 16th and 17th centuries' pattern tiles). A serious mood still deepens the ink towards violet.",true)+
      row("Pattern",pseg("guideCfg.azuMotif",["rosette","diamond","sprigs","plain"],["Rosette","Diamond point","Corner sprigs","Plain"]),"The motif on the tile's face, behind the eyes and the mouth: Rosette, a flower in a ring with quarter-discs at the corners; Diamond point, nested diamonds and a border (ponta de diamante); Corner sprigs, rings and leaves at the corners only; Plain, the glaze alone. Corner sprigs and Plain leave the face clearest.",true)+
      row("Light",pseg("guideCfg.azuLight",["pastel","glaze","cobalt","gold"],["Pastel clouds","Glaze colours","Cobalt","Gold"]),"The colours of the clouds inside the tile when it turns to light, to listen, think and travel: pastel clouds (cyan, violet, magenta, peach); the glaze's own colours, lifted to light; cobalt blues; gold.",true)+
      row("Corners",pseg("guideCfg.azuShape",["crisp","soft","pillow"],["Crisp","Soft","Pillow"]),"How round the tile's edges are; the tile keeps its size.",true)+
      row("Preview",rest?'<div class="azprev"><figure><img src="'+rest+'" alt="The mascot at rest" width="96" height="96"><figcaption>At rest</figcaption></figure><figure><img src="'+lit+'" alt="The mascot as light" width="96" height="96"><figcaption>As light</figcaption></figure></div>':'<span class="uses">This browser cannot draw WebGL: no preview (the guide shows the Tile (2.5D)).</span>',"Unity: the same values are the properties of the tile's Shader Graph material; each choice is a style asset.",true))}
  return d.innerHTML});
/* each row's demo: the guide's Azu in that style */
rowDemos({"guideCfg.azuGlaze":{tour:"g1",ring:".mascot"},"guideCfg.azuMotif":{tour:"g1",ring:".mascot"},"guideCfg.azuLight":{tour:"g1",ring:".mascot"},"guideCfg.azuShape":{tour:"g1",ring:".mascot"}});
