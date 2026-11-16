/* Feedback: haptics and sounds per event (S.fb, feedback). */
S.fb={haptics:true,hStrength:"medium",sound:false,volume:0.5,events:[
 {id:"open",ev:"The card opens",hap:"light",snd:"whoosh"},{id:"tap",ev:"A tap on a block",hap:"light",snd:"none"},{id:"jump",ev:"A jump to a section",hap:"selection",snd:"tick"},
 {id:"slide",ev:"A slider or scrub moves",hap:"selection",snd:"none"},{id:"right",ev:"A right answer",hap:"success",snd:"chime"},{id:"wrong",ev:"A wrong answer",hap:"warning",snd:"buzz"},
 {id:"collect",ev:"A stamp collected",hap:"heavy",snd:"stamp"},{id:"locate",ev:"Show me where it is",hap:"medium",snd:"ping"}]};
var HAPS=[["none","None"],["selection","Selection tick"],["light","Light"],["medium","Medium"],["heavy","Heavy"],["success","Success"],["warning","Warning"]];
var SNDS=[["none","None"],["tick","Tick"],["whoosh","Whoosh"],["chime","Chime"],["buzz","Soft buzz"],["stamp","Stamp"],["ping","Ping"]];
var actx=null;
function tone(f,t0,d,type,g){var o=actx.createOscillator(),v=actx.createGain();o.type=type||"sine";o.frequency.setValueAtTime(f,actx.currentTime+t0);v.gain.setValueAtTime(0,actx.currentTime+t0);v.gain.linearRampToValueAtTime(g*S.fb.volume,actx.currentTime+t0+0.01);v.gain.exponentialRampToValueAtTime(0.0001,actx.currentTime+t0+d);o.connect(v);v.connect(actx.destination);o.start(actx.currentTime+t0);o.stop(actx.currentTime+t0+d+0.02)}
function playSound(k){if(!S.fb.sound||k==="none")return;try{actx=actx||new (window.AudioContext||window.webkitAudioContext)();
  if(k==="tick")tone(1800,0,.04,"square",.08);if(k==="chime"){tone(880,0,.5,"sine",.25);tone(1318,.09,.6,"sine",.2)}if(k==="ping")tone(1568,0,.7,"sine",.25);
  if(k==="buzz")tone(140,0,.18,"sawtooth",.12);if(k==="stamp"){tone(90,0,.18,"sine",.45);tone(180,0,.08,"square",.08)}
  if(k==="whoosh"){var n=actx.createBufferSource(),b=actx.createBuffer(1,actx.sampleRate*.35,actx.sampleRate),d=b.getChannelData(0);for(var i=0;i<d.length;i++)d[i]=(Math.random()*2-1)*Math.sin(Math.PI*i/d.length);n.buffer=b;var fl=actx.createBiquadFilter();fl.type="bandpass";fl.frequency.value=900;var g=actx.createGain();g.gain.value=.25*S.fb.volume;n.connect(fl);fl.connect(g);g.connect(actx.destination);n.start()}}catch(e){}}
var HAPMS={selection:[8],light:[12],medium:[22],heavy:[40],success:[14,60,22],warning:[30,50,30]};
function buzz(k){if(!S.fb.haptics||k==="none")return;var p=HAPMS[k]||[10];if(S.fb.hStrength==="soft")p=p.map(function(x){return Math.round(x*.6)});if(S.fb.hStrength==="strong")p=p.map(function(x){return Math.round(x*1.5)});
  try{if(navigator.vibrate)navigator.vibrate(p)}catch(e){}var r=document.createElement("div");r.className="hapring hap-"+k;r.textContent=k==="selection"?"tick":k;ph.appendChild(r);setTimeout(function(){r.remove()},700)}
function feedback(id){var e=byId(S.fb.events,id);if(!e)return;buzz(e.hap);playSound(e.snd)}
ed.addEventListener("click",function(e){var tr=e.target.closest("[data-try]");if(tr){e.stopPropagation();feedback(tr.dataset.try)}},true);
