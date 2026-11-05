/* Each element's demo content (S.demoC) and where a demo field lives. */
S.demoC={title:{text:DEMO.title},eyebrow:{text:DEMO.eyebrow},heading:{text:DEMO.heading},lede:{text:DEMO.lede},paragraph:{text:DEMO.paragraph},
 callout:{title:DEMO.ctitle,text:DEMO.ctext,back:DEMO.back},pull_quote:{text:DEMO.quote,cite:DEMO.cite},list:{items:DEMO.list},
 picture:{img:"g3",cap:DEMO.piccap},picture_pair:{imgs:["before","g2"],cap:DEMO.paircap},carousel:{imgs:["g1","g2","g3"]},video:{img:"hero",cap:DEMO.vidcap},
 audio:{cap:T("Guided tour · 3:11","Visita guiada · 3:11")},animation:{file:"tile_bloom.lottie"},
 before_after:{before:"before",after:"g2",blabel:T("Before 1755","Antes de 1755"),alabel:T("Today","Hoje")},hotspot:{img:"g3"},model_3d:{img:"g1"},panorama:{img:"hero"},
 timeline:{items:{en:["c. 1500 · The court moves to the river","1580s · The Torreão rises","1755 · Earthquake, fire and tsunami"],pt:["c. 1500 · A corte muda-se para o rio","1580s · Ergue-se o Torreão","1755 · Terramoto, incêndio e maremoto"]}},
 scrolly:{items:{en:["1662: the palace along the river.","Before 1755: the Torreão and the opera.","Today: the square that took its place."],pt:["1662: o palácio ao longo do rio.","Antes de 1755: o Torreão e a ópera.","Hoje: a praça que ficou no seu lugar."]}}};
var DEMOC0=JSON.parse(JSON.stringify(S.demoC));
on("renderEl.content",function(c,kind){if(!c||!Object.keys(c).length)c=S.demoC[kind]||{};return c});
on("renderEl.html",function(h,a){return a.kind==="animation"&&S.view!=="demo"?h.replace(/<small>[\s\S]*?<\/small>/,""):h});
on("contentAt.answer",function(path){if(path.indexOf("demo:")===0){var k=path.slice(5);return S.demoC[k]=S.demoC[k]||{}}
  if(path.indexOf("termpage:")===0){var q=path.split(":"),t=byId(S.terms,q[1]),r=t&&t.page[+q[2]];return r?(r.c=r.c||{}):null}});
ed.addEventListener("focusin",function(e){var f=e.target.dataset&&e.target.dataset.field;if(f&&(f.indexOf("demo:")===0||f.indexOf("termpage:")===0))pathShow(f)});
