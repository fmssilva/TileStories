/* ===================== v8r: recorded voices: the guide plays audio rendered at authoring time (voicebench), each word lit at its own time; a blind test to choose the voice ===================== */
S.guideCfg.voiceSet="";S.vt={line:"",order:[],ratings:{},saved:false,results:null,note:""};
COMP.s7vtest=["Guide","Voice test"];FOCUS.s7vtest=["card",".gblock","full"];
/* the voice sets (voicebench/out: rendered with a provider's key; out-private: Edge, on this computer only, never published) */
window.VOICESETS=window.VOICESETS||[];
(function(){var srcs=["voicebench/out/voicesets.js","../voicebench/out/voicesets.js"];if(location.protocol==="file:")srcs.push("voicebench/out-private/voicesets.js","../voicebench/out-private/voicesets.js");
  srcs.forEach(function(s){var e=document.createElement("script");e.src=s;e.onload=function(){try{renderEditor()}catch(x){}};e.onerror=function(){};document.head.appendChild(e)})})();
/* a line is found by what it says: FNV-1a of "lang|text", the same key voicebench writes */
function fnvKey(s){var b=new TextEncoder().encode(s),h=0x811c9dc5;for(var i=0;i<b.length;i++){h^=b[i];h=Math.imul(h,0x01000193)>>>0}return("0000000"+h.toString(16)).slice(-8)}
function voiceSets(){return window.VOICESETS||[]}
function currentSet(){var s=voiceSets(),id=S.guideCfg.voiceSet;return s.filter(function(x){return x.id===id})[0]||s.filter(function(x){return!x.private})[0]||s[0]||null}
function recFile(text,set){set=set||currentSet();if(!set||!text)return null;var f=set.files[fnvKey(S.lang+"|"+String(text).trim())];return f?{set:set,file:f,url:set.url+f.src}:null}
/* ----- playing a recorded line: the audio's clock lights each word (its real start and end: the glide is exact) ----- */
var sayV8r=say;
say=function(text,onWord,onEnd){var r=S.guideCfg.voice==="recorded"?recFile(text):null;if(!r){sayV8r(text,onWord,onEnd);return}
  stopSpeech();var words=[],re=/\S+/g,m;while((m=re.exec(text)))words.push({i:m.index,w:m[0]});G.words=words;G.recWords=r.file.words;
  G.hl={k:-1,at:0,p:0,cpm:0,ended:false,last:0,seen:true,start:performance.now(),target:G.hl&&G.hl.target};G.noVoice=false;
  var a=new Audio(r.url);a.preload="auto";a.muted=!!S.guideMute;a.playbackRate=S.guideCfg.speed||1;G.recAudio=a;G.utter={recorded:true};G.lastWordAt=performance.now();musicDuck(true);
  var fin=function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;musicDuck(false);if(G.hl)G.hl.ended=true;onEnd()};
  a.addEventListener("ended",fin);a.addEventListener("error",function(){if(G.recAudio!==a)return;G.recAudio=null;G.utter=null;G.recWords=null;sayV8r(text,onWord,onEnd)});
  var tick=function(){if(G.recAudio!==a)return;var t=a.currentTime,w=G.recWords,k=-1;for(var j=0;j<w.length;j++){if(w[j][0]<=t+.02)k=j;else break}
    if(k>=0&&k!==G.hl.k){onWord(k);G.lastWordAt=performance.now()}if(k>=0&&G.hl){G.hl.at=performance.now()-(t-w[k][0])*1000/(a.playbackRate||1)}if(!a.paused)G.lastWordAt=performance.now();requestAnimationFrame(tick)};
  var p=a.play();if(p&&p.catch)p.catch(function(){if(G.recAudio===a){a.muted=true;a.play().catch(function(){})}});requestAnimationFrame(tick)};
/* the glide uses the word's real length */
var wordMsV8r=wordMs;wordMs=function(k){if(G.recWords&&G.recWords[k])return Math.max(60,(G.recWords[k][1]-G.recWords[k][0])*1000/((G.recAudio&&G.recAudio.playbackRate)||1));return wordMsV8r(k)};
var stopSpeechV8r=stopSpeech;stopSpeech=function(){if(G.recAudio){try{G.recAudio.pause()}catch(e){}G.recAudio=null;G.utter=null;G.recWords=null}stopSpeechV8r()};
/* pause and resume: the audio just pauses and plays on (the app's AudioSource does the same) */
var pauseTourV8r=pauseTour;pauseTour=function(){if(!G.on)return;if(G.recAudio){if(S.guideCfg.onScroll==="follow"&&G.userScrollAt&&performance.now()-G.userScrollAt<60&&!G.explicitPause)return;G.paused=true;G.recAudio.pause();renderGuide();return}pauseTourV8r()};
var resumeTourV8r=resumeTour;resumeTour=function(){if(!G.on)return;if(G.recAudio&&G.paused){G.paused=false;G.askOpen=false;G.recAudio.muted=!!S.guideMute;G.recAudio.play().catch(function(){});renderGuide();return}resumeTourV8r()};
/* mute keeps the words going (the captions follow the audio's clock) */
document.addEventListener("click",function(e){if(!G.recAudio||!e.target.closest||!e.target.closest('#ph [data-g="mute"]'))return;e.stopImmediatePropagation();e.stopPropagation();S.guideMute=!S.guideMute;G.recAudio.muted=S.guideMute;renderGuide()},true);

/* ----- the lines of this point, for the coverage and the test ----- */
function guideLines(lang){var keepL=S.lang,keepA=S.vis.aud,out=[],seen={};S.lang=lang;
  var add=function(id,label,t){t=String(t||"").trim();if(!t||seen[t])return;seen[t]=1;out.push({id:id,label:label,text:t})};
  AUDS.forEach(function(a){S.vis.aud=a;ovMap();(S.fullSteps||S.guideSteps).forEach(function(st,j){add(st.id,"Step "+(j+1)+(a!=="explorer"?" · "+AUDN[a]:""),stepText(st));if(st.after)add(st.id+"a","Step "+(j+1)+" · after",L(st.after))})});
  S.vis.aud=keepA;["guide_intro","guide_moving_on","guide_nudge_flip_1","guide_nudge_flip_2","guide_try_moveon","guide_after_flip","guide_after_answer"].forEach(function(k){add(k,k.replace("guide_","").replace(/_/g," "),k==="guide_intro"?fmt(TXT(k),pointName()||"Ribeira Palace"):TXT(k))});
  S.lang=keepL;ovMap();return out}
/* ----- the blind test: a line, the voices in a shuffled order as A, B, C…, a score each, then the names and everyone's means ----- */
var vtDB=null,vtUser=null;(window.claude&&window.claude.use?Promise.all([window.claude.use("db"),window.claude.use("user")]):Promise.resolve([null,null])).then(function(r){vtDB=r[0];vtUser=r[1]}).catch(function(){});
function vtSets(line){return voiceSets().filter(function(s){return!!recFile(line,s)})}
function vtStart(){var line=S.vt.line||"",sets=vtSets(line);S.vt.order=sets.map(function(s){return s.id}).sort(function(){return Math.random()-.5});S.vt.ratings={};S.vt.saved=false;S.vt.note="";renderEditor()}
function vtPlay(i){var id=S.vt.order[i],s=voiceSets().filter(function(x){return x.id===id})[0],r=s&&recFile(S.vt.line,s);if(!r)return;if(G.vtA)G.vtA.pause();G.vtA=new Audio(r.url);G.vtA.play().catch(function(){})}
function vtSave(){var lang=S.lang,line=S.vt.line,out={};S.vt.order.forEach(function(id){var x=S.vt.ratings[id];if(x)out[lang+"|"+fnvKey(line)+"|"+id]={nat:x.nat||0,acc:x.acc||0,best:S.vt.best===id}});
  S.vt.saved=true;var done=function(n){S.vt.note=n;vtLoad()};
  if(!vtDB||!vtUser){try{var k="tsVoiceRatings",all=JSON.parse(localStorage.getItem(k)||"{}");Object.assign(all,out);localStorage.setItem(k,JSON.stringify(all))}catch(e){}done("Saved in this browser only (the shared results need the published Lab).");return}
  vtUser.id().then(function(uid){if(!uid){done("Sign in to save to the shared results.");return}var ref=vtDB.doc("voicerate/"+uid);return ref.get().then(function(sn){var cur=sn.exists?(sn.data().ratings||{}):{},m=Object.assign({},cur,out);return ref.set({ratings:m,at:new Date().toISOString()})}).then(function(){done("Saved to the shared results.")})}).catch(function(e){done("Could not save ("+(e&&e.code||"error")+").")})}
function vtLoad(){var sum={},add=function(rs){Object.keys(rs||{}).forEach(function(k){var p=k.split("|"),id=p[2],x=rs[k];var s=sum[id]=sum[id]||{nat:0,acc:0,n:0,na:0,best:0};if(x.nat){s.nat+=x.nat;s.n++}if(x.acc){s.acc+=x.acc;s.na++}if(x.best)s.best++})},fin=function(){S.vt.results=sum;renderEditor()};
  if(vtDB){vtDB.collection("voicerate").get().then(function(q){q.docs.forEach(function(d){add(d.data().ratings)});fin()}).catch(function(){fin()})}else{try{add(JSON.parse(localStorage.getItem("tsVoiceRatings")||"{}"))}catch(e){}fin()}}
function stars(id,k,v){var h="";for(var i=1;i<=5;i++)h+='<button type="button" class="vstar'+(i<=v?" on":"")+'" data-vtr="'+id+'|'+k+'|'+i+'" aria-label="'+i+'">★</button>';return'<span class="vstars">'+h+'</span>'}

/* Editor: Narration gets the recorded voice and its coverage; 6 · Guide gets Voice test */
var edCardV8r=edCard;
edCard=function(){var d=document.createElement("div");d.innerHTML=edCardV8r();var sets=voiceSets(),cur=currentSet(),lines=guideLines(S.lang),have=cur?lines.filter(function(l){return!!recFile(l.text,cur)}).length:0;
  addRows(d,"s7voice",row("Recorded voice",sets.length?psel("guideCfg.voiceSet",[["","Best available"]].concat(sets.map(function(s){return[s.id,s.label+(s.private?" (this computer only)":"")]}))):'<span class="uses">None rendered yet</span>',
    "With Voice = Recorded files: the audio rendered at authoring time (voicebench: a provider's voice, rendered once, with each word's time), as the app plays it. "+(cur?have+" of "+lines.length+" lines rendered in "+S.lang.toUpperCase()+" for this point; a line not rendered (an edited text, an answer) speaks with the device voice.":"Render a set with voicebench/render.py (see voicebench/README).")));
  var vt=d.querySelector('[data-fo="s7voice"]');if(!vt)return d.innerHTML;
  var line=S.vt.line||"",opts=lines.filter(function(l){return vtSets(l.text).length>=2}),body;
  if(sets.length<2)body='<div class="note">Render at least two voice sets (two providers or two voices) to compare them here.</div>';
  else{body=row("Line",'<select class="u" data-vtline="1">'+'<option value="">(pick a line)</option>'+opts.map(function(l){return'<option'+(l.text===line?" selected":"")+' value="'+esc(l.text)+'">'+esc(l.label+" · "+l.text.slice(0,48))+'</option>'}).join("")+'</select>',"A line rendered by two or more voices, in the language on the phone.")+
      (line?'<div class="btnrow"><button class="ubtn sm" type="button" data-vtstart="1">'+(S.vt.order.length?"New round":"Start")+'</button></div>':"");
    if(line&&S.vt.order.length){body+='<table class="tbl vtt"><thead><tr><th>Voice</th><th></th><th>Natural</th>'+(S.lang==="pt"?'<th>pt-PT accent</th>':"")+'<th>Best</th></tr></thead><tbody>'+S.vt.order.map(function(id,i){var x=S.vt.ratings[id]||{},s=voiceSets().filter(function(z){return z.id===id})[0];
        return'<tr><td><b>'+String.fromCharCode(65+i)+'</b>'+(S.vt.saved&&s?' <span class="uses">'+esc(s.label)+'</span>':"")+'</td><td><button class="ubtn sm" type="button" data-vtplay="'+i+'">▶ Play</button></td><td>'+stars(id,"nat",x.nat||0)+'</td>'+(S.lang==="pt"?'<td>'+stars(id,"acc",x.acc||0)+'</td>':"")+'<td><input type="radio" name="vtbest" data-vtbest="'+id+'"'+(S.vt.best===id?" checked":"")+'></td></tr>'}).join("")+'</tbody></table>'+
        '<div class="btnrow"><button class="ubtn sm" type="button" data-vtsave="1">Save my scores and show the names</button></div>'+(S.vt.note?'<div class="note">'+esc(S.vt.note)+'</div>':"")}
    if(S.vt.results){var r=S.vt.results;body+='<table class="tbl"><thead><tr><th>Results (everyone)</th><th>Natural</th><th>Accent</th><th>Best</th><th>Scores</th></tr></thead><tbody>'+Object.keys(r).map(function(id){var s=voiceSets().filter(function(z){return z.id===id})[0],x=r[id];
        return'<tr><td>'+esc(s?s.label:id)+'</td><td>'+(x.n?(x.nat/x.n).toFixed(2):"-")+'</td><td>'+(x.na?(x.acc/x.na).toFixed(2):"-")+'</td><td>'+x.best+'</td><td>'+x.n+'</td></tr>'}).join("")+'</tbody></table>'}}
  vt.insertAdjacentHTML("afterend",comp("s7vtest","a blind test: the same line in each voice, unnamed, scored, then the names and everyone's means",body+'<div class="note">Share the Lab with native listeners (as Contributors) and they score here too; each listener’s scores are their own, everyone sees the means. Level the voices (−16 LUFS) when rendering.</div>'));
  return d.innerHTML};
ed.addEventListener("change",function(e){var t=e.target;if(!t.dataset)return;if(t.dataset.vtline){e.stopImmediatePropagation();S.vt.line=t.value;S.vt.order=[];S.vt.saved=false;renderEditor();return}
  if(t.dataset.vtbest){e.stopImmediatePropagation();S.vt.best=t.dataset.vtbest}},true);
ed.addEventListener("click",function(e){var t=e.target.closest("[data-vtstart],[data-vtplay],[data-vtr],[data-vtsave]");if(!t)return;e.stopPropagation();
  if(t.dataset.vtstart){vtStart();return}if(t.dataset.vtplay!=null){vtPlay(+t.dataset.vtplay);return}if(t.dataset.vtsave){vtSave();return}
  var q=t.dataset.vtr.split("|"),x=S.vt.ratings[q[0]]=S.vt.ratings[q[0]]||{};x[q[1]]=+q[2];renderEditor()},true);
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("guideCfg\\.(","guideCfg\\.(voiceSet|"));
