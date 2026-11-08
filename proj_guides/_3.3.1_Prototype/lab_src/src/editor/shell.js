/* The Editor's shell: rows, segments, selects, sliders, foldouts (fo), renderEditor and renderAll, setOpt, and the Editor's own events; the path controls (pseg, psel, pslider) and the row helpers (rowEl, setRow, addRows, dropRow). */
/* ---------- Editor ---------- */
var ed=document.getElementById("ed");
function help(t){return '<button class="help" type="button" data-help="'+esc(t)+'" aria-label="Help">?</button>'}
function seg(key,vals,labels,cur){return '<div class="seg">'+vals.map(function(v,i){return'<button type="button" data-set="'+key+'" data-v="'+v+'" aria-pressed="'+(String(cur)===String(v))+'">'+esc(labels[i])+'</button>'}).join("")+'</div>'}
function row(label,ctrl,h,ind){return '<div class="row'+(ind?" ind":"")+'"><label>'+esc(label)+'</label><div>'+ctrl+'</div>'+(h?help(h):"<span></span>")+'</div>'}
function sel(key,opts,cur){return '<select class="u" data-sel="'+key+'">'+opts.map(function(o){return'<option value="'+esc(o[0])+'"'+(o[0]===cur?" selected":"")+'>'+esc(o[1])+'</option>'}).join("")+'</select>'}
function slider(key,min,max,step,val,adv){return '<div class="sl2"><input type="range" data-num="'+key+'" min="'+min+'" max="'+max+'" step="'+step+'" value="'+val+'" aria-label="'+key+'"><input class="u" type="number" data-num="'+key+'" min="'+min+'" max="'+max+'" step="'+step+'" value="'+val+'" aria-label="'+key+'"></div>'+(adv?'<div class="note warn">'+adv+'</div>':"")}
/* foldouts: open state by id; depth for "open to level" */
function isOpen(id,depth){if(S.op.hasOwnProperty(id))return S.op[id];return depth<=S.ui.level}
function fo(id,title,badge,body,depth,cur){return '<details class="fo'+(cur?" cur":"")+'" data-fo="'+id+'" data-depth="'+depth+'"'+(isOpen(id,depth)?" open":"")+'><summary>'+title+'<span class="sp"></span>'+(badge?'<span class="badge '+badge[0]+'">'+badge[1]+'</span>':"")+'</summary>'+(body?'<div class="inner">'+body+'</div>':"")+'</details>'}
function locked(t){return '<details class="fo locked"><summary>'+esc(t)+'<span class="sp"></span><span class="badge">EXISTING</span></summary></details>'}
function contentAt(path){var ans=firstAnswer("contentAt.answer",path);if(ans!==undefined)return ans;var p=path.split("."),r=byId(S.story,p[0]);if(!r)return null;if(p[1]==="c")return r.c=r.c||{};var j=+p[1].slice(1);return r.slots[j]=r.slots[j]||{}}
var lastRender=0;
/* noDraw: the self-test works the Editor's controls without drawing the Editor or the phone (show only sets the view) */
var noDraw=false;
function renderEditor(){if(noDraw)return;emitLatestFirst("renderEditor.before");lastRender=performance.now();document.querySelectorAll(".utab").forEach(function(t){t.setAttribute("aria-selected",String(t.dataset.tab===S.tab))});
  var h=firstAnswer("renderEditor.tab");ed.innerHTML=h!==undefined?h:S.tab==="card"?edCard():S.tab==="marker"?edMarker():edGlobal();
  /* the foldouts this draw opens fire their own toggle event later (after the rest of the task, the phone's draw too): each is marked, so
     it is always quiet (edQuiet), however long the task ran */
  lastRender=performance.now();ed.querySelectorAll("details[open]").forEach(function(d){d._rt=1});emit("renderEditor.after")}
/* a toggle that is the draw's own (a foldout drawn open), not the developer's */
function edQuiet(d){return!!d._rt||performance.now()-lastRender<150}
function renderAll(keep){if(noDraw)return;renderEditor();renderPhone(keep)}
/* ---------- Editor events ---------- */
ed.addEventListener("toggle",function(e){var d=e.target;if(!d.dataset||!d.dataset.fo)return;var id=d.dataset.fo,quiet=edQuiet(d);if(d._rt)setTimeout(function(){d._rt=0},0);S.op[id]=d.open;
  if(!d.open||quiet)return;
  if(S.ui.single){var par=d.parentElement;Array.prototype.forEach.call(par.children,function(s){if(s!==d&&s.tagName==="DETAILS"&&s.open){s.open=false;S.op[s.dataset.fo]=false}})}
  var k=id.split(":");
  if(k[0]==="el"){S.sel.kind=k[1];if(kindOfStyle(S.sel.style)!==k[1])S.sel.style=defStyle(k[1]).id;S.sel.layer="style";markCur();show("demo","s:"+S.sel.style)}
  else if(k[0]==="pt"){S.sel.pattern=k[1];S.sel.layer="pattern";markCur();show("demo","p:"+k[1])}
  else if(k[0]==="mo"){S.sel.motion=k[1];S.sel.layer="motion";markCur();show("demo","m:"+k[1])}
  else if(k[0]==="fx"){S.sel.effect=k[1];S.sel.layer="effect";markCur();show("demo","f:"+k[1])}
  else if(k[0]==="in"){S.sel.inter=k[1];S.sel.layer="inter";markCur();show("demo","i:"+k[1])}
  else if(id==="mcard"){show("card")}else if(id==="mpages"){show("story")}},true);
function markCur(){ed.querySelectorAll(".fo.cur").forEach(function(x){x.classList.remove("cur")});var s=S.sel,id={motion:"mo:"+s.motion,effect:"fx:"+s.effect,pattern:"pt:"+s.pattern,inter:"in:"+s.inter,style:"el:"+s.kind}[s.layer];var el=ed.querySelector('[data-fo="'+id+'"]');if(el)el.classList.add("cur")}
ed.addEventListener("click",function(e){var t=e.target;
  var hb=t.closest("[data-help]");if(hb){var nx=hb.parentNode.nextElementSibling;if(nx&&nx.dataset&&nx.dataset.h){nx.remove();return}var tip=document.createElement("div");tip.className="tip";tip.dataset.h="1";tip.textContent=hb.dataset.help;hb.parentNode.after(tip);return}
  var b=t.closest("[data-set]");if(b){setOpt(b.dataset.set,b.dataset.v);return}
  var ct=t.closest("[data-cat]");if(ct){S.ui.cat=ct.dataset.cat;renderEditor();return}
  var bl=t.closest("[data-block]");if(bl){S.sel.block=bl.dataset.block;renderEditor();return}
  var ps=t.closest("[data-pickstyle]");if(ps){S.sel.style=ps.dataset.pickstyle;S.sel.kind=kindOfStyle(S.sel.style);S.sel.layer="style";renderEditor();show("demo","s:"+S.sel.style);return}
  var op=t.closest("[data-openpicker]");if(op){S.sel.picker=S.sel.picker===op.dataset.openpicker?null:op.dataset.openpicker;S.sel.row=op.dataset.openpicker;renderEditor();show("story","r:"+S.sel.row);return}
  var ss=t.closest("[data-setstyle]");if(ss){var pr=ss.dataset.setstyle.split("|"),rr=byId(S.story,pr[0]);rr.style=pr[1]||null;S.sel.picker=null;renderAll(true);show("story","r:"+rr.id);return}
  var as=t.closest("[data-asset]");if(as){var q=as.dataset.asset.split("|"),c=contentAt(q[0]);c[q[1]]=q[2];renderAll(true);show("story","r:"+q[0].split(".")[0]);return}
  var as2=t.closest("[data-assets]");if(as2){var q2=as2.dataset.assets.split("|"),c2=contentAt(q2[0]),arr=c2[q2[1]]=c2[q2[1]]||[],ix=arr.indexOf(q2[2]);if(ix>=0)arr.splice(ix,1);else arr.push(q2[2]);renderAll(true);show("story","r:"+q2[0].split(".")[0]);return}
  var star=t.closest("[data-def]");if(star&&!star.disabled){var k=kindOfStyle(star.dataset.def);S.styles[k].forEach(function(s){s.def=s.id===star.dataset.def});renderAll(true);show("demo","s:"+star.dataset.def);return}
  var d=t.closest("[data-del]");if(d&&!d.disabled){var id=d.dataset.id;
    if(d.dataset.del==="motion"){S.motions=S.motions.filter(function(m){return m.id!==id});S.sel.motion="rise"}
    if(d.dataset.del==="style"){var kk=kindOfStyle(id);S.styles[kk]=S.styles[kk].filter(function(s){return s.id!==id});S.sel.style=defStyle(kk).id}
    if(d.dataset.del==="pattern"){S.patterns=S.patterns.filter(function(p){return p.id!==id});S.sel.pattern=S.patterns[0]&&S.patterns[0].id}
    renderAll(true);return}
  var a=t.closest("[data-add]");if(a){
    if(a.dataset.add==="motion"){var ty=a.dataset.type,base=S.motions.filter(function(m){return m.type===ty})[0],m=JSON.parse(JSON.stringify(base));m.id=nid("mo");m.name="New "+TYPES[ty];PATH[m.id]=PATH[base.id];S.motions.push(m);S.sel.motion=m.id;S.sel.layer="motion";S.op["mo:"+m.id]=true;renderAll(true);show("demo","m:"+m.id);return}
    if(a.dataset.add==="style"){var bs=styleById(S.sel.style)||defStyle(S.sel.kind),ns=JSON.parse(JSON.stringify(bs));ns.id=nid("st");ns.name=bs.name+" Copy";ns.def=false;S.styles[S.sel.kind].push(ns);S.sel.style=ns.id;renderAll(true);show("demo","s:"+ns.id);return}
    if(a.dataset.add==="pattern"){var np={id:nid("pt"),name:"New Pattern",cat:"Text",rows:[{kind:"heading",style:defStyle("heading").id},{kind:"paragraph",style:defStyle("paragraph").id}]};S.patterns.push(np);S.sel.pattern=np.id;S.sel.layer="pattern";S.op["pt:"+np.id]=true;renderAll(true);show("demo","p:"+np.id);return}}
  var pa=t.closest("[data-padd]");if(pa){var q3=pa.dataset.padd.split("."),p=patternById(q3[0]);p.rows.push({kind:q3[1],style:defStyle(q3[1]).id});S.story.forEach(function(r){if(r.pattern===p.id)r.slots.push({})});renderAll(true);show("demo","p:"+p.id);return}
  var pm=t.closest("[data-pmove]");if(pm){var q4=pm.dataset.pmove.split("."),p2=patternById(q4[0]),i=+q4[1],j=i+(+pm.dataset.dir);if(j<0||j>=p2.rows.length)return;swap(p2.rows,i,j);S.story.forEach(function(r){if(r.pattern===p2.id)swap(r.slots,i,j)});renderAll(true);show("demo","p:"+p2.id);return}
  var pd=t.closest("[data-pdel]");if(pd){var q5=pd.dataset.pdel.split("."),p3=patternById(q5[0]),k3=+q5[1];p3.rows.splice(k3,1);S.story.forEach(function(r){if(r.pattern===p3.id)r.slots.splice(k3,1)});renderAll(true);show("demo","p:"+p3.id);return}
  var sa=t.closest("[data-sadd]");if(sa){var kd=sa.dataset.sadd,nr={id:nid("r"),kind:kd,style:null,c:{}};S.story.push(nr);S.sel.row=nr.id;renderAll(true);show("story","r:"+nr.id);return}
  var sp=t.closest("[data-spadd]");if(sp){var pp=patternById(sp.dataset.spadd),r2={id:nid("r"),pattern:pp.id,slots:pp.rows.map(function(){return{}})};S.story.push(r2);S.sel.row=r2.id;renderAll(true);show("story","r:"+r2.id);return}
  var sm=t.closest("[data-smove]");if(sm){var i2=+sm.dataset.smove,dir=+sm.dataset.dir;if(dir===0)S.story.splice(i2,1);else{var j2=i2+dir;if(j2<0||j2>=S.story.length)return;swap(S.story,i2,j2)}renderAll(true);return}
  var cm=t.closest("[data-cmove]");if(cm){var i3=+cm.dataset.cmove,dir2=+cm.dataset.dir;if(dir2===0)S.card.splice(i3,1);else{var j3=i3+dir2;if(j3<0||j3>=S.card.length)return;swap(S.card,i3,j3)}renderAll(true);show("card");return}
  var ca=t.closest("[data-cadd]");if(ca){var nb={id:nid("b"),kind:ca.dataset.cadd,grp:"about",enter:"family",tap:"nothing",orow:"plain"};if(nb.kind==="Inline")nb.pattern="pt3";S.card.push(nb);S.sel.cblock=nb.id;renderAll(true);show("card","b:"+nb.id);return}
  var pk=t.closest("[data-pick]");if(pk&&!t.closest("select,input,textarea,button")){if(pk.dataset.pick==="row"){S.sel.row=pk.dataset.id;S.sel.picker=null;renderEditor();show("story","r:"+S.sel.row)}else{S.sel.cblock=pk.dataset.id;renderEditor();show("card","b:"+S.sel.cblock)}}});
function swap(a,i,j){var x=a[i];a[i]=a[j];a[j]=x}
function setOpt(key,v){if(firstAnswer("setOpt.answer",key,v)!==undefined)return;
  if(key==="test.on"){S.test.on=v==="true";renderEditor();S.view="card";renderPhone(false);return}
  if(key==="focusOn"){S.focusOn=v==="true";renderAll(true);return}
  if(key==="replay"){S.replay=v;renderAll(true);return}
  if(key==="ui.single"){S.ui.single=v==="true";renderEditor();return}
  if(key==="ui.level"){S.ui.level=+v;S.op={};renderEditor();return}
  if(key.indexOf("cb.")===0){var b=byId(S.card,S.sel.cblock);b[key.slice(3)]=v;renderAll(true);show("card","b:"+b.id);return}
  var m=motionById(S.sel.motion),st=styleById(S.sel.style),f=effectById(S.sel.effect);
  if(key.indexOf("motion.")===0&&m){m[key.slice(7)]=v;renderAll(true);show("demo","m:"+m.id);playRef("m:"+m.id);return}
  if(key.indexOf("o.")===0&&st){st.o[key.slice(2)]=v;renderAll(true);show("demo","s:"+st.id);return}
  if(key.indexOf("fx.")===0&&f){f.o[key.slice(3)]=v;renderAll(true);show("demo","f:"+f.id);return}}
function playRef(ref){if(noDraw)return;setTimeout(function(){var b=scr.querySelector('[data-ref="'+ref+'"] [data-play]');if(b)b.click()},380)}
ed.addEventListener("change",function(e){var t=e.target,k=t.dataset.sel;if(t.dataset.num){numChange(t,true);return}if(!k)return;
  if(k==="test.poi"){S.test.poi=t.value;S.view="card";renderAll(false);return}
  if(k==="family"){S.family=t.value;renderAll(true);return}
  if(k.indexOf("cb.")===0){var b=byId(S.card,S.sel.cblock);b[k.slice(3)]=t.value;renderAll(true);show("card","b:"+b.id);return}
  var st=styleById(S.sel.style);
  if(k==="style.enter"&&st){st.enter=t.value;renderAll(true);show("demo","s:"+st.id);setTimeout(function(){var it=scr.querySelector('[data-ref="s:'+st.id+'"]');if(it)replayIn(it)},420);return}
  if(k==="style.scroll"&&st){st.scroll=t.value;renderAll(true);show("demo","s:"+st.id);return}
  if(k==="style.turn"&&st){st.turn=t.value;renderAll(true);show("demo","s:"+st.id);return}
  if(k==="style.uses"&&st){st.o.uses=t.value;renderAll(true);show("demo","s:"+st.id);return}
  if(k.indexOf("prow.")===0){var q=k.split(".");patternById(q[1]).rows[+q[2]].style=t.value;renderAll(true);show("demo","p:"+q[1]);return}});
function numChange(t,final){var key=t.dataset.num,v=parseFloat(t.value);if(isNaN(v))return;ed.querySelectorAll('[data-num="'+key+'"]').forEach(function(o){if(o!==t)o.value=v});
  if(key.indexOf("motion.")===0){var m=motionById(S.sel.motion);if(!m)return;m[key.slice(7)]=v;if(final){renderAll(true);show("demo","m:"+m.id);playRef("m:"+m.id)}else refreshJson();return}
  if(key.indexOf("fx.")===0){var f=effectById(S.sel.effect);if(!f)return;f.o[key.slice(3)]=v;renderPhone(true);show("demo","f:"+f.id);refreshJson();return}
  if(key.indexOf("inter.")===0){var x=interById(S.sel.inter);if(!x)return;x.o[key.slice(6)]=v;if(final){renderPhone(true);show("demo","i:"+x.id)}refreshJson();return}}
ed.addEventListener("input",function(e){var t=e.target;
  if(t.dataset.num){numChange(t,false);return}
  if(t.dataset.q){S.ui.q=t.value;var pos=t.selectionStart;renderEditor();var n=ed.querySelector("[data-q]");if(n){n.focus();n.setSelectionRange(pos,pos)}return}
  if(t.dataset.text){var w=t.dataset.text,obj=w==="motion"||w==="motionfile"?motionById(S.sel.motion):w==="style"?styleById(S.sel.style):patternById(t.dataset.pid);if(obj){if(w==="motionfile")obj.file=t.value;else obj.name=t.value;renderPhone(true);refreshJson()}return}
  if(t.dataset.field){var q=t.dataset.field.split("|"),c=contentAt(q[0]);if(!c)return;if(q[2]){c[q[1]]=c[q[1]]||{};c[q[1]][q[2]]=q[3]==="lines"?t.value.split("\n"):t.value}else c[q[1]]=t.value;renderPhone(true);refreshJson()}});
document.querySelectorAll(".utab").forEach(function(t){t.addEventListener("click",function(){S.tab=t.dataset.tab;renderEditor();if(S.tab==="marker")show("card");else if(S.tab==="card")show("demo")})});
document.querySelectorAll("[data-step]").forEach(function(b){b.addEventListener("click",function(){var n=+b.dataset.step;S.step=n;document.querySelectorAll("[data-step]").forEach(function(x){x.setAttribute("aria-pressed",String(x===b))});
  S.op={};S.op.bc=n<=3;S.op.motion=n===1;S.op.elements=n===1;S.op.patterns=n===2;S.op.library=n===3;S.op.test=false;S.op.interaction=false;S.op.effects=false;S.op.mcard=n===4;S.op.mpages=n===5;S.op["pg:story"]=n===5;if(n===1){S.op["mt:entrance"]=true;S.op["mo:"+S.sel.motion]=true}
  if(n===1){S.tab="card";S.sel.layer="motion";renderEditor();show("demo","m:"+S.sel.motion)}
  if(n===2){S.tab="card";S.sel.layer="pattern";S.op["pt:"+S.sel.pattern]=true;renderEditor();show("demo","p:"+S.sel.pattern)}
  if(n===3){S.tab="card";renderEditor();show("demo")}
  if(n===4){S.tab="marker";renderEditor();S.view="card";renderPhone(false)}
  if(n===5){S.tab="marker";renderEditor();S.view="story";renderPhone(false)}
  if(window.innerWidth<1000)document.querySelector(".ed").scrollIntoView({behavior:"smooth"})})});
function pval(v){return v==="true"?true:v==="false"?false:v}
function pseg(path,vals,labels){return seg("@"+path,vals,labels,String(getPath(path)))}
function psel(path,opts){return sel("@"+path,opts,String(getPath(path)))}
function pslider(path,min,max,step){return slider("@"+path,min,max,step,getPath(path))}
function prow(label,ctrl,h,ind){return row(label,ctrl,h,ind).replace("</label>",' <span class="prop" title="Neither Unity nor the Lab had it: a proposal from the audit">PROPOSED</span></label>')}
function rowEl(d,fid,label){var f=innerOf(d,fid);if(!f)return null;var rows=f.querySelectorAll(":scope > .row");for(var i=0;i<rows.length;i++){var lb=rows[i].querySelector("label");if(lb&&lb.firstChild&&lb.firstChild.textContent===label)return rows[i]}return null}
function setRow(d,fid,label,html){var r=rowEl(d,fid,label);if(r){r.outerHTML=html;return true}return false}
function addRows(d,fid,html,before){var f=innerOf(d,fid);if(!f)return;var r=before&&rowEl(d,fid,before);if(r){r.insertAdjacentHTML("beforebegin",html);return}
  var ob=f.querySelector(":scope > .ownbar");if(ob)ob.insertAdjacentHTML("beforebegin",html);else f.insertAdjacentHTML("beforeend",html)}
function dropRow(d,fid,label){var r=rowEl(d,fid,label);if(r)r.remove()}
