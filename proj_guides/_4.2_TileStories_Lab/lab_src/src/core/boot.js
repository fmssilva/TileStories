/* Starts the page (the #hash routes) and closes the shared scope. */
/* #selftest walks every tab; #selftest=guide,words only those (the texts come with "words") */
/* round 21 (21.8): the texts' walk after the controls, in slices (editor/selftest-texts.js's selfTestWordsAsync); its JSON pending until done */
if(isSelfTest()){G.stwAsync=true;var r0=selfTest(selfTestTabs());if(stWalks("words")){G.stwPending=true;showSelfTest(r0);selfTestWordsAsync(function(r1){G.stwPending=false;showSelfTest(r0.concat(r1))})}else showSelfTest(r0)}
goScreen("card",{stop:"closed",reset:true});ui.tab="card";if(!isSelfTest())renderAll(false);
if(location.hash==="#guide"){sheetTo("full");renderAll(false);startTour();G.i=4;runStep()}
if(location.hash==="#words"){ui.tab="words";ui.sel.term="t1";renderEditor();goScreen("story");renderPhone(false)}
if(location.hash==="#texts"){["s0","s2","s3","s4","s7"].forEach(function(id){ui.op[id]=false});ui.op["tp:s1b:Pages layout › chevrons"]=true;ui.op["tp:s5page:Previous / Next buttons"]=true;ui.op["tp:s5page:Back button"]=true;goOwner("s1b",true);goOwner("s5page",true);show("card")}
if(location.hash.indexOf("#open=")===0){var ids=decodeURIComponent(location.hash.slice(6)).split(",");ui.op={};["s0","s1","s2","s3","s4","s5","s7"].forEach(function(x){ui.op[x]=false});var vw=null;ids.forEach(function(i){if(i.indexOf("tab:")===0)ui.tab=i.slice(4);else if(i.indexOf("view:")===0)vw=i.slice(5);else ui.op[i]=true});ui.level=0;renderEditor();if(vw){goScreen(vw);renderPhone(false)}}
if(location.hash.indexOf("#toggle=")===0){var tg=decodeURIComponent(location.hash.slice(8)).split(",");ui.op={};tg.slice(0,-1).forEach(function(i){ui.op[i]=true});ui.level=0;renderEditor();setTimeout(function(){var dd=ed.querySelector('[data-fo="'+tg[tg.length-1]+'"]');if(dd){dd.open=true;window.scrollTo(0,0)}},400)}
if(location.hash==="#fr"){D.wall.langs.list.push({code:"fr",name:"Français"});ui.tab="words";ui.txf="content";ui.sel.term="t1";renderEditor();ui.termOpen="t1";goScreen("term");renderPhone(false)}

})();
