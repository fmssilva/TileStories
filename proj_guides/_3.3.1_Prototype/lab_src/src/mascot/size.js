/* The mascot's size, and a character per audience. */
/* ----- the size: one factor, a CSS variable on the phone; the guide's Azu, its bubble and the floating button follow it (the peek and the walk keep theirs) ----- */
var AZU_SIZES={s:.8,m:1,l:1.35};
S.guideCfg.azuSize="m";
function azSize(){return AZU_SIZES[S.guideCfg.azuSize]||1}
/* ----- a character per audience: "" keeps the guide's own; "light", "tile", or "glb:<model id>" (mascot/models.js) ----- */
AUDS.forEach(function(a){S.aud.looks[a].char=""});
function azCharOpts(){return[["","Same as the guide"],["light","Tile of light"],["tile","Tile (2.5D)"]].concat(AZ3D_MODELS.map(function(m){return["glb:"+m.id,"3D · "+m.name]}))}
/* as the audiences' liveliness (audiences/audiences.js), the character is written into the guide's settings when the audience (or its choice) changes */
function azCharApply(){var a=aud(),c=S.aud.looks[a].char||"",key=a+"|"+c;if(S.audCharApplied===key)return;S.audCharApplied=key;if(!c)return;
  if(c.indexOf("glb:")===0){S.guideCfg.azuStyle="glb";S.guideCfg.azuModel=c.slice(4)}else S.guideCfg.azuStyle=c}
on("renderPhone.before",function(keep){azCharApply();ph.style.setProperty("--azs",azSize())});
/* both act on the guide's Azu, which the card screens the self-test compares do not draw: behaviour */
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guideCfg\\.azuSize|aud\\.looks\\.[a-z]+\\.char|"));

/* ----- Editor: 6 · Guide > Mascot gets Size; 7 · Audiences > Looks per audience gets Character ----- */
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  var r=rowEl(d,"s7mascot","Liveliness");
  if(r)r.insertAdjacentHTML("beforebegin",row("Size",pseg("guideCfg.azuSize",["s","m","l"],["Small","Medium","Large"]),"How big Azu is on the card and as the guide's button: Small (80 %), Medium, Large (135 %). Its peek from behind a block and its walk to the wall keep their own sizes. Unity: the Azu element's scale."));
  var tb=d.querySelector('[data-fo="s8look"] table tbody');
  if(tb)tb.insertAdjacentHTML("afterbegin",'<tr><td>Character</td>'+AUDS.map(function(a){return'<td>'+psel("aud.looks."+a+".char",azCharOpts())+'</td>'}).join("")+'</tr>');
  return d.innerHTML});
/* the row's demo: the guide's Azu at that size */
rowDemos({"guideCfg.azuSize":{tour:"g1",ring:".mascot"}});
