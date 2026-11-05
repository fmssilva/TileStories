/* Starts the page (the #hash routes) and closes the shared scope. */
if(location.hash==="#selftest"){var r0=selfTest();showSelfTest(r0)}
S.view="card";S.tab="card";if(location.hash!=="#selftest")renderAll(false);
if(location.hash==="#guide"){S.stop="full";renderAll(false);startTour();G.i=4;runStep()}
if(location.hash==="#words"){S.tab="words";S.sel.term="t1";renderEditor();S.view="story";renderPhone(false)}
if(location.hash==="#texts"){["s0","s2","s3","s4","s7"].forEach(function(id){S.op[id]=false});S.op["tp:s1b:Pages layout › chevrons"]=true;S.op["tp:s5page:Previous / Next buttons"]=true;S.op["tp:s5page:Back button"]=true;goOwner("s1b",true);goOwner("s5page",true);show("card")}
if(location.hash.indexOf("#open=")===0){var ids=decodeURIComponent(location.hash.slice(6)).split(",");S.op={};["s0","s1","s2","s3","s4","s5","s7"].forEach(function(x){S.op[x]=false});var vw=null;ids.forEach(function(i){if(i.indexOf("tab:")===0)S.tab=i.slice(4);else if(i.indexOf("view:")===0)vw=i.slice(5);else S.op[i]=true});S.ui.level=0;renderEditor();if(vw){S.view=vw;renderPhone(false)}}
if(location.hash.indexOf("#toggle=")===0){var tg=decodeURIComponent(location.hash.slice(8)).split(",");S.op={};tg.slice(0,-1).forEach(function(i){S.op[i]=true});S.ui.level=0;renderEditor();setTimeout(function(){var dd=ed.querySelector('[data-fo="'+tg[tg.length-1]+'"]');if(dd){dd.open=true;window.scrollTo(0,0)}},400)}
if(location.hash==="#fr"){S.langs.list.push({code:"fr",name:"Français"});S.tab="words";S.ui.txf="content";S.sel.term="t1";renderEditor();S.termOpen="t1";S.view="term";renderPhone(false)}

})();
