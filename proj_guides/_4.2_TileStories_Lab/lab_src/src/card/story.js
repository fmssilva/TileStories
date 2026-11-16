/* The story page. */
function storyScreen(){emitLatestFirst("storyScreen.before");var h=topbar(esc(L(secById("stories").name))+" &#8250; <b>"+esc(L(DEMO.title))+"</b>")+'<div class="pg"><div class="blk">';
  S.story.forEach(function(r){if(r.pattern){var p=patternById(r.pattern);if(!p){h+='<div class="item" data-ref="r:'+r.id+'"><p class="el-par missing">(missing pattern)</p></div>';return}
      var parts=p.rows.map(function(pr,i){return renderEl(pr.kind,styleById(pr.style)||defStyle(pr.kind),r.slots[i]||{},i)});
      h+='<div class="item" data-ref="r:'+r.id+'">'+(r.pattern==="pt1"?'<div class="wide2"><div>'+parts[0]+'</div><div>'+parts.slice(1).join("")+'</div></div>':parts.join(""))+'</div>';}
    else h+='<div class="item" data-ref="r:'+r.id+'">'+renderEl(r.kind,(r.style&&styleById(r.style))||defStyle(r.kind),r.c,0)+'</div>'});
  return pipe("storyScreen.html",h+'</div></div>');}
S.story.forEach(function(r){if(r.checked==null)r.checked=r.id!=="r10"&&r.id!=="r11"});
