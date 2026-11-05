/* The Editor's focus map: opening a foldout shows its part on the phone (FOCUS, focusPhone). */
/* ----- opening a component shows that element on the phone, marked for a moment ----- */
var FOCUS={s0card:["democard",".hero"],s0tile:["card",".testtile","full"],s0bar:[null,"#ptool"],s1a:["card",".hero","half"],s1peek:["card",".peeknext","peek"],
 s1b:["card",".hero","full"],s1c:["card",'[data-ref="b:b_audio"]',"full"],s1mini:["card",'[data-ref="b:b_audio"]',"full"],
 s2cfg:["card",".gsec","full"],s2start:["card",'[data-ref="sec:stories"]',"full"],s2div:["card",".gsec","full"],s2tint:["card",".gsec","full"],
 s5top:["card",".chrome","full"],s5settings:["card",".chrome i:last-child","full"],s5bar:["card","#navbar","full"],s5openrow:["card",'[data-ref="b:b_story"] .open',"full"],
 s5expand:["card",'[data-ref="b:b_info"] .open',"full"],s5back:["story",".topbar button"],s5crumb:["story",".topbar span"],s5prevnext:["story",".pfoot"],
 s5loc:["card",".locpin","half"],s5sticky:["card",".sticky","half"],s5term:["story",".gterm"],s5termpage:["term",".el-title"],
 s7voice:["card",".gblock","full"],s7hero:["card",".gstart","full"],s7block:["card",".gblock","full"],s7cap:["card",".gblock","full"],s7coach:["card",".gblock","full"],
 s7tour:["card",".gblock","full"],s7resume:["card",".gblock","full"],s7mascot:["card",".gblock","full"],s7ask:["card",".gblock","full"],
 "g:10":["card",".walltip","closed"],"g:11":["card",".walltip","closed"],w2:["story",".gterm"]};
function focusPhone(view,sel,stop){if(view){var same=S.view===view&&(!stop||S.stop===stop);if(stop)S.stop=stop;S.view=view;if(!same)renderPhone(false)}
  setTimeout(function(){var el=ph.querySelector(sel)||document.querySelector(sel);if(!el)return;
    if(scr.contains(el)){var y=el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop-90;scr.scrollTo({top:Math.max(0,y),behavior:S.reduce?"auto":"smooth"})}
    document.body.dataset.lastfocus=sel+"|found|"+(scr.contains(el)?Math.round(el.getBoundingClientRect().top-scr.getBoundingClientRect().top+scr.scrollTop):"overlay");el.classList.remove("pfocus");void el.offsetWidth;el.classList.add("pfocus");setTimeout(function(){el.classList.remove("pfocus")},1800)},80)}
ed.addEventListener("toggle",function(e){var d=e.target;if(!d.dataset||!d.dataset.fo||!d.open||performance.now()-lastRender<150)return;var id=d.dataset.fo,f=FOCUS[id],k=id.split(":");
  if(f){focusPhone(f[0],f[1],f[2]);return}
  if(k[0]==="bd"){var b=S.card.filter(function(x){return x.kind===k[1]})[0];if(b){focusPhone("card",'[data-ref="b:'+b.id+'"]',"full");return}var c=S.demoCard.filter(function(x){return x.kind===k[1]})[0];if(c)focusPhone("democard",'[data-ref="b:'+c.id+'"]');return}
  if(k[0]==="sec"){focusPhone("card",'[data-ref="sec:'+k[1]+'"]',"full");return}
  if(k[0]==="eg"){var K=KINDS.filter(function(x){return x.c===k[1]})[0];if(K){var st=defStyle(K.k);if(st)show("demo","s:"+st.id)}}},true);
