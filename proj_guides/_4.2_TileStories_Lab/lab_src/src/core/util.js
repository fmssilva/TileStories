/* Small helpers: esc, byId, setPath and getPath (a dotted path that starts with its root: "D.guide.voice", "LAB.test.on"). */
function esc(s){return String(s==null?"":s).replace(/[&<>"]/g,function(c){return{"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;"}[c]})}
function byId(a,id){for(var i=0;i<a.length;i++)if(a[i].id===id)return a[i];return null}
/* this computer: the page opened from disk or from the local server (tools/serve.py), never the published Lab (the keys, the private voice
   sets and the MP3s read directly are this computer's) */
/* a script from the first of its places that answers ("x/f.js" in the published layout, "../x/f.js" beside lab_src/ on this computer):
   never two copies (at the published root both names are the same file) */
function scriptFirst(srcs,onload){if(!srcs.length)return;var e=document.createElement("script");e.src=srcs[0];e.onload=function(){try{onload&&onload()}catch(x){}};e.onerror=function(){e.remove();scriptFirst(srcs.slice(1),onload)};document.head.appendChild(e)}
function labLocal(){return location.protocol==="file:"||/^(localhost|127\.0\.0\.1|\[::1\])$/.test(location.hostname)}
function pathRoot(name){return{D:D,P:P,ui:ui,LAB:LAB}[name]}
function setPath(path,v){var p=path.split("."),o=pathRoot(p[0]);for(var i=1;i<p.length-1;i++)o=o[isNaN(p[i])?p[i]:+p[i]];var last=p[p.length-1];o[isNaN(last)?last:+last]=v}
function getPath(path){var p=path.split("."),o=pathRoot(p[0]);for(var i=1;i<p.length;i++){if(o==null)return undefined;o=o[isNaN(p[i])?p[i]:+p[i]]}return o}
