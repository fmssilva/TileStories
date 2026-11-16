/* Small helpers: esc, byId, setPath and getPath (a dotted path into S). */
function esc(s){return String(s==null?"":s).replace(/[&<>"]/g,function(c){return{"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;"}[c]})}
function byId(a,id){for(var i=0;i<a.length;i++)if(a[i].id===id)return a[i];return null}
function setPath(path,v){var p=path.split("."),o=S;for(var i=0;i<p.length-1;i++)o=o[isNaN(p[i])?p[i]:+p[i]];var last=p[p.length-1];o[isNaN(last)?last:+last]=v}
function getPath(path){var p=path.split("."),o=S;for(var i=0;i<p.length;i++){if(o==null)return undefined;o=o[isNaN(p[i])?p[i]:+p[i]]}return o}
