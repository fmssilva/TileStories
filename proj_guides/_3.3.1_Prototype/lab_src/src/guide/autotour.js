/* The guide's automatic tour (guidance round 16): a tour made from a Detail Card's blocks, the default of every point with no written tour,
   which the Editor can switch to, compare, make its own and edit. tourFromCard(card, story, lang, opts) is one pure function: it returns
   steps in the written tour's own shape ({id, view, ref, act, wait, text}), so every part of the guide plays it unchanged (the captions, the
   focus, Azu, the panel, the games on the tour, Audiences, the scheduler, Ask Azu, the voices). Step 0 is always the teaser at the hero
   (the point's name, one hook from the card, what the tour covers and its minutes); then one step per block in the card's order by its
   kind's rule (AT_RULES: Guide › 2 · The tour › Tour from the card › What each block says); links between the steps made from each
   block's own words, never the same twice in a row (round 16b); never a fact twice (round 16b: atFactKeys); the end step a line of recap
   and "Ask me anything". It says the card's own words and the link words only (atCheck proves it, with the word caps, no sentence twice,
   no fact twice and the links). A tour under 2 minutes is "a short tour" (round 16b). A point's tour: S.tours[point] (src: written | card; the
   written tour once it exists; made: copied by Make it mine; dismissed: blocks the notice no longer names; ai: the stored lines of
   Make it more fluid… with AI). S.guideSteps is the open point's tour: its written one, or the one made from its card, made again from
   the card whenever the phone is drawn and no tour plays.
   Unity: GuideTourBuilder.FromCard(DetailCard, language, rules) in plain C# (EditMode tests on the same cards), run by the POI Editor and
   at load for a marker with no GuideScript asset; its link words are CardStrings keys (at_*); the AI lines are stored in the marker's
   GuideScript asset by the Editor; a line with no recorded clip goes to the TTS path Ask Azu's answers take (the device voice or
   ElevenLabs, as the voice settings say) */
S.guideCfg.autoAI=false;S.guideCfg.atCap=40;S.guideCfg.atTeaserCap=45;S.guideCfg.atStoryMax=6;S.guideCfg.atHook="auto";
/* each block kind's rule: what the guide does at a block of that kind (the first value the default; a kind with nothing to tell skips) */
var AT_RULES={
 "Inline":{opts:[["read","Read its words (a flip card: try it)"],["show","Say its first sentence"],["skip","Skip"]]},
 "Story":{opts:[["read","Open it, then read its pages"],["show","Open it and say its title"],["skip","Skip"]]},
 "Quick Facts":{opts:[["facts","The two or three strongest, as one sentence"],["skip","Skip"]]},
 "Status":{opts:[["say","Say its condition"],["skip","Skip"]]},
 "Actions":{opts:[["skip","Skip (buttons, nothing to tell)"],["show","Show it"]]},
 "Timeline":{opts:[["ends","Its first and last dates"],["all","Every date"],["skip","Skip"]]},
 "Compare Points":{opts:[["show","Say the two points it compares"],["skip","Skip"]]},
 "Dialogue":{opts:[["show","Say its first line"],["skip","Skip"]]},
 "Gallery":{opts:[["show","Show it with its caption"],["skip","Skip"]]},
 "Before and After":{opts:[["try","Try it: the visitor drags"],["show","Show it with its two labels"],["skip","Skip"]]},
 "Hotspot Image":{opts:[["show","Show it: tap a spot"],["skip","Skip"]]},
 "Video":{opts:[["show","Show it with its caption"],["skip","Skip"]]},
 "3D Model":{opts:[["show","Show it: turn it with a finger"],["skip","Skip"]]},
 "360 Panorama":{opts:[["show","Show it: drag to look around"],["skip","Skip"]]},
 "Audio Guide":{opts:[["skip","Skip (the guide already speaks)"],["show","Show it"]]},
 "Place In AR":{opts:[["skip","Skip (an action)"],["show","Show it"]]},
 "Knowledge Check":{opts:[["try","Try it: the visitor answers"],["show","Say the question"],["skip","Skip"]]},
 "Poll":{opts:[["show","Say its question"],["skip","Skip"]]},
 "Collect":{opts:[["skip","Skip (a stamp, nothing to tell)"],["show","Show it"]]},
 "Feedback":{opts:[["skip","Skip (asks the visitor)"],["show","Show it"]]},
 "Wall Locator":{opts:[["skip","Skip (navigation)"],["show","Show it"]]},
 "Today Map":{opts:[["skip","Skip (navigation)"],["show","Show it"]]},
 "Related":{opts:[["skip","Skip (links)"],["show","Show it"]]},
 "Practical Info":{opts:[["skip","Skip (visit details, not the story)"],["show","Say its first row"]]},
 "Sources":{opts:[["skip","Skip (credits)"],["show","Show it"]]},
 "Contents":{opts:[["skip","Skip (navigation)"],["show","Show it"]]}};
S.guideCfg.atRule={};Object.keys(AT_RULES).forEach(function(k){S.guideCfg.atRule[k]=AT_RULES[k].opts[0][0]});
var AT_HOOKS=[["auto","Automatic: the story's first sentence, else the subtitle, else the first fact"],["line","The subtitle"],["fact","The first fact"],["story","The story's first sentence"]];
S.tours={painting:{src:"written"}};
COMP.s7auto=["Guide","Tour from the card"];FOCUS.s7auto=["card",".hero","full"];

/* ----- the pure parts: words, sentences, caps ----- */
function atWords(t){return(String(t||"").match(/\S+/g)||[]).length}
/* a sentence ends at . ! ? …, never after a title's abbreviation ("D. Manuel I", "St. George") or c. (circa) */
function atSentences(t){return String(t||"").trim().split(/(?<=[.!?…])(?<!\b(?:D|Dr|St|Sta|Sto|Mr|Mrs|c)\.)\s+(?=[^\s])/).filter(Boolean)}
function atNorm(s){return String(s).toLowerCase().replace(/[^\p{L}\p{N}]+/gu," ").trim()}
function atEnd(t){t=String(t||"").trim();return!t||/[.!?…]$/.test(t)?t:t+"."}
/* the first word in lower case, mid-sentence, when it is a common word (not a name): the base list or a word the card writes in lower case */
var AT_LOWER={en:"the a an for in on at by from with about around over under almost nearly some its it this that there here where when what later then today one two three four five six seven eight nine ten hundred",
  pt:"o a os as um uma uns umas durante em no na nos nas por para com de do da cerca quase mais hoje depois antes quando onde dois duas três quatro cinco cem"};
function atLowerFirst(t,lang,ok){var m=/^(\S+)/.exec(t||"");if(!m)return t;var w=m[1].replace(/[^\p{L}]/gu,""),lw=w.toLowerCase();
  if(!w||w===w.toLowerCase()||w.length>1&&w===w.toUpperCase())return t;if((" "+(AT_LOWER[lang]||AT_LOWER.en)+" ").indexOf(" "+lw+" ")<0&&!(ok&&ok(w)))return t;return t.charAt(0).toLowerCase()+t.slice(1)}
/* a name the card writes with a capital mid-sentence (Philip, Tagus, Praça) stays as written; any other word may go lower case mid-sentence */
function atProper(parts){var p={};parts.forEach(function(x){atSentences(x).forEach(function(sen){(sen.match(/[\p{L}][\p{L}'’-]*/gu)||[]).forEach(function(w,i){if(i>0&&/^\p{Lu}/u.test(w))p[w]=1})})});return p}
/* a fact as words: its label and its value joined as a phrase when the label leads into it (a preposition, a participle: "Royal home for
   about 250 years", "Built in the 1580s", "Lost 1 November 1755"), else with a colon ("River: the Tagus") */
var AT_LEAD={en:/(\b(for|in|on|at|by|under|since|until|from|of|out|with)|ed|lost|built)$/i,pt:/(\b(durante|em|no|na|nos|nas|de|do|da|desde|até|com|por)|ado|ada|ido|ida|ído|to|ta)$/i};
function atFact(f,lang,lf,first){var lab=first?f[0]:lf(f[0]),re=AT_LEAD[lang]||AT_LEAD.en;return re.test(f[0].trim())?lab+" "+lf(f[1]):lab+": "+lf(f[1])}
/* a long text in its first sentences, up to the cap (a first sentence over it is cut at the cap with …) */
function atCapText(t,cap){var ss=atSentences(t),out=[],n=0;for(var i=0;i<ss.length;i++){var w=atWords(ss[i]);if(out.length&&n+w>cap)break;out.push(ss[i]);n+=w}
  var r=out.join(" ");if(atWords(r)>cap)r=(r.match(/\S+/g)||[]).slice(0,cap-1).join(" ").replace(/[,;:]$/,"")+"…";return{text:r,cut:r!==String(t).trim()}}
function atJoin(items,and,lang){if(items.length<2)return items.join("");var last=items[items.length-1],head=items.slice(0,-1).join(", ");return head+(lang==="en"&&items.length>2?", ":" ")+and+" "+last}
/* a tour's seconds at a voice's speed: each line's words at 170 a minute, and 0.7 s between lines (stepSecs' rule for a line not recorded) */
function atSecs(texts,speed){return texts.reduce(function(a,t){return a+Math.max(1,atWords(t))*60/(170*(speed||1))+.7},0)}
function atMins(secs){return Math.max(1,Math.ceil(secs/60-.15))}
/* a story row's words, in one language: a paragraph, a heading, a before and after, or what it shows (its short title, its text, a caption) */
function atRowSays(r,lang){var x=function(o){var t=wordIn(o,lang);return typeof plainTerms==="function"?plainTerms(t):t};
  if(r.kind==="paragraph")return{kind:"par",text:x(r.c&&r.c.text)};if(r.kind==="heading")return{kind:"head",text:x(r.c&&r.c.text)};
  var parts=r.pattern?(r.slots||[]):[r.c||{}],texts=[],caps=[],ba=false;
  parts.forEach(function(p){if(!p)return;if(p.before&&p.after)ba=true;if(p.title)texts.push(x(p.title));if(p.text)texts.push(x(p.text));if(p.cap)caps.push(x(p.cap))});
  if(r.kind==="before_after")ba=true;texts=texts.filter(Boolean);caps=caps.filter(Boolean).map(function(c){return c.replace(/\s*·\s*\d+:\d+\s*$/,"")});
  var short=function(t){return atWords(t)<=6&&!/[.!?…]$/.test(t)},lead=texts.filter(short)[0]||"",body=texts.filter(function(t){return!short(t)}).sort(function(a,b){return atWords(b)-atWords(a)})[0]||"";
  return{kind:ba?"ba":body?"show":caps.length?"cap":"none",lead:lead,text:body,cap:caps[0]||"",title:texts.filter(short).slice(-1)[0]||""}}

/* ----- guidance round 16b (the developer, 2026-11-12): never a fact twice. A fact's keys, from each sentence or each item of a list (a
   fact, an event): its numbers (a year, a decade, a date, a number in digits or in words: "about 250 years" and "Two hundred and fifty
   years" are the same key, n:250) and its proper names (a run of capitalized words, the first word of a sentence only when the card writes
   it with a capital mid-sentence; a title such as King or D. left out: "King Manuel I" and "Manuel I" are p:manuel i). A year after
   before, after, since or until is a reference, not a fact; a bare year repeats a date of that year (1755 after 1 November 1755). A
   sentence, a fact or an event whose keys were all said already is not said again (one with a new fact stays); the point's own name is
   no sentence's fact (every line is about it) but it is a fact item's (River: the Tagus, after the teaser named the Tagus). A fact of the
   Quick Facts that a later sentence of the tour tells (all its keys in one sentence) is left to that sentence: the fuller telling wins,
   as the teaser's hook takes the strongest. Unity: the same keys in GuideTourBuilder (FactKeys), EditMode-tested on the same cards ----- */
var AT_MONTHS={en:"january february march april may june july august september october november december".split(" "),pt:"janeiro fevereiro março abril maio junho julho agosto setembro outubro novembro dezembro".split(" ")};
var AT_NUMW={en:{one:1,two:2,three:3,four:4,five:5,six:6,seven:7,eight:8,nine:9,ten:10,eleven:11,twelve:12,thirteen:13,fourteen:14,fifteen:15,sixteen:16,seventeen:17,eighteen:18,nineteen:19,twenty:20,thirty:30,forty:40,fifty:50,sixty:60,seventy:70,eighty:80,ninety:90,hundred:100,thousand:1000},
  pt:{um:1,uma:1,dois:2,duas:2,"três":3,quatro:4,cinco:5,seis:6,sete:7,oito:8,nove:9,dez:10,onze:11,doze:12,treze:13,catorze:14,quatorze:14,quinze:15,dezasseis:16,dezesseis:16,dezassete:17,dezessete:17,dezoito:18,dezanove:19,dezenove:19,vinte:20,trinta:30,quarenta:40,cinquenta:50,sessenta:60,setenta:70,oitenta:80,noventa:90,cem:100,cento:100,duzentos:200,duzentas:200,trezentos:300,trezentas:300,quatrocentos:400,quatrocentas:400,quinhentos:500,quinhentas:500,seiscentos:600,setecentos:700,oitocentos:800,novecentos:900,mil:1000}};
var AT_REF={en:/\b(before|after|since|until|till)\s+$/i,pt:/\b(antes\s+de|depois\s+de|desde|até|após)\s+$/i};
var AT_TITLES=/^(king|queen|prince|princess|saint|st|lord|lady|rei|rainha|príncipe|princesa|d|dom|dona|são|santo|santa|sto|sta)$/i,AT_ARTS=/^(the|a|an|o|os|as|um|uma)$/i;
/* a text's fact keys. o: proper (the words the card writes with a capital mid-sentence), all (every capitalized word: a name), not (keys
   left out: the point's name, for a sentence) */
function atFactKeys(t,lang,o){o=o||{};var s=String(t||""),keys=[],add=function(k){if(keys.indexOf(k)<0)keys.push(k)},mo=AT_MONTHS[lang]||AT_MONTHS.en,de=lang==="pt"?"\\s+de":"",mre="("+mo.join("|")+")";
  var blank=function(m){return m.replace(/\S/g,"§")};
  s=s.replace(new RegExp("\\b(\\d{1,2})"+de+"\\s+"+mre+de+"\\s+(\\d{3,4})\\b","gi"),function(m,d,mn,y){add("d:"+(+d)+"-"+(mo.indexOf(mn.toLowerCase())+1)+"-"+y);return blank(m)});
  s=s.replace(new RegExp("\\b"+mre+de+"\\s+(\\d{3,4})\\b","gi"),function(m,mn,y){add("m:"+(mo.indexOf(mn.toLowerCase())+1)+"-"+y);return blank(m)});
  s=s.replace(/\b(\d{3}0)s\b|\bdécada\s+de\s+(\d{3}0)\b/gi,function(m,a,b){add("c:"+(a||b));return blank(m)});
  s.replace(/\b\d{1,3}(?:[,\u00a0\u202f ]\d{3})+\b|\b\d+\b/g,function(m,at){var v=+m.replace(/\D/g,"");if(v>=1000&&v<=2100){if(!(AT_REF[lang]||AT_REF.en).test(s.slice(0,at)))add("y:"+v)}else if(v>=10)add("n:"+v);return m});
  var nw=AT_NUMW[lang]||AT_NUMW.en,toks=[],re=/[\p{L}\p{N}]+(?:['’-][\p{L}\p{N}]+)*/gu,m;while((m=re.exec(String(t||""))))toks.push({w:m[0],at:m.index});
  /* a number in words: a run of number words (and / e between them) */
  for(var i=0;i<toks.length;i++){var lw=toks[i].w.toLowerCase();if(nw[lw]==null)continue;var tot=0,cur=0,j=i;
    for(;j<toks.length;j++){var x=toks[j].w.toLowerCase();if((x==="and"||x==="e")&&j>i&&j+1<toks.length&&nw[toks[j+1].w.toLowerCase()]!=null)continue;var v=nw[x];if(v==null)break;
      if(v===100&&(x==="hundred"))cur=(cur||1)*100;else if(v===1000){tot+=(cur||1)*1000;cur=0}else cur+=v}
    var val=tot+cur;if(val>=20)add("n:"+val);i=j-1}
  /* a proper name: a run of capitalized words (do, da, de, of between them; a roman numeral after a name), its title and article left out */
  var start=function(k){var b=String(t).slice(0,toks[k].at).replace(/["“‘'«(¿¡\s]+$/,"");return!b||/[.!?…]$/.test(b)&&!/\b(D|Dr|St|Sta|Sto|Mr|Mrs|c)\.$/.test(b)};
  var cap=function(k){var w=toks[k].w;if(!/^\p{Lu}/u.test(w)||mo.indexOf(w.toLowerCase())>=0)return false;if(start(k)&&!o.all&&!(o.proper&&o.proper[w]))return false;return true};
  for(var a=0;a<toks.length;a++){if(!cap(a))continue;var run=[toks[a].w],b=a+1;
    for(;b<toks.length;b++){var y=toks[b].w;if(cap(b)||/^[IVX]+$/.test(y)&&run.length){run.push(y);continue}if(/^(do|da|dos|das|de|of)$/.test(y)&&b+1<toks.length&&cap(b+1)){run.push(y);continue}break}
    a=b-1;while(run.length&&(AT_TITLES.test(run[0])||AT_ARTS.test(run[0])))run.shift();while(run.length&&/^(do|da|dos|das|de|of)$/.test(run[run.length-1]))run.pop();
    if(!run.length||run.length===1&&run[0].length<2)continue;var key="p:"+run.join(" ").toLowerCase();
    if(o.not&&o.not.some(function(n){var nw=n.slice(2).split(" ");return key.slice(2).split(" ").every(function(x){return nw.indexOf(x)>=0})}))continue;add(key)}
  return keys}
/* a key already said: the same key, or a bare year said as a date of that year */
function atKnown(said,k){if(said[k])return true;var m=/^y:(\d+)$/.exec(k);return!!m&&Object.keys(said).some(function(x){return/^[dm]:/.test(x)&&x.split("-").pop()===m[1]})}
/* the points's name as keys (its proper names) */
function atNameKeys(name,lang){return atFactKeys(name,lang,{all:true}).filter(function(k){return/^p:/.test(k)})}

/* ----- the links (round 16b, the developer: links tied to the content): a link names what comes next from the block's own words, no AI.
   A sentence link stands alone (First, the facts. · Now, the facts. · Now the story. · Back on the card. · Ready for a question?); a link
   that leads into the words (Now, {its heading}: · Here's · In the pictures: · Then the dates: · And in {year}, · And around {year},) is at
   most 6 words before them, the words after it begin in lower case; where no words fit, one neutral link (And now,). Never the same link
   twice in a row: the second neutral is left out. Each link keeps its source for the Editor's step list (at.ls) ----- */
var AT_LINK_SRC={facts:"from the Quick Facts, first on the card",facts_n:"from the Quick Facts",story:"from the Story, as it opens",back:"from the card again, after the story",quiz:"from the Knowledge Check",
  head:"from its heading",cap:"from its caption",pics:"from its caption",dates:"from its dates",year:"from its date",circa:"from its date",neutral:"the neutral link: none of its words fit"};
var AT_LINK_KEY={facts:"at_link_facts",facts_n:"at_link_facts_n",story:"at_link_story",back:"at_link_back",quiz:"at_link_quiz",head:"at_link_head",cap:"at_link_cap",pics:"at_link_pics",dates:"at_link_dates",year:"at_link_year",circa:"at_link_circa",neutral:"at_link"};
var AT_DET={en:/^(the|a|an|its|this)\s/i,pt:/^(o|a|um|uma|este|esta)\s/i};

/* the tour from a card, in one language. card: its blocks in order; story: the story's rows; opts: name (the point's), line (its
   subtitle), says(block) (its words in this language: blockSays), W(key) (a link word in this language), rules, cap, teaserCap, storyMax,
   hook, speed, lowerOk(word), proper (the card's words written with a capital mid-sentence). Returns the steps, each with at: {key, block,
   kind, rule, lk (its link's kind), lw (the words in its link), ls (its link's source), link, body, units (each fact unit: its words t and
   keys k), cut}. A first pass without the Quick Facts finds the facts a later sentence tells (o.later) */
function tourFromCard(card,story,lang,o){
  if(!o.pre&&!o.later){var pre=tourFromCard(card,story,lang,Object.assign({},o,{pre:true})),fi=card.findIndex(function(b){return b.kind==="Quick Facts"}),later=[];
    if(fi>=0)pre.forEach(function(s){var bi=s.at&&s.at.block?card.findIndex(function(b){return b.id===s.at.block}):-1;if(bi>fi&&s.at.kind!=="Timeline")(s.at.units||[]).forEach(function(u){if(u.k.length)later.push(u.k)})});
    o=Object.assign({},o,{later:later})}
  var W=o.W,steps=[],said={},facts={},skipped=[],hookSrc="",lf=function(t){return atLowerFirst(t,lang,o.lowerOk)},nameKeys=atNameKeys(o.name,lang);
  var keysOf=function(t,item){return atFactKeys(t,lang,{proper:o.proper,not:item?null:nameKeys})};
  var old=function(t,item){var k=keysOf(t,item);return k.length>0&&k.every(function(x){return atKnown(facts,x)})};
  var fresh=function(t){return atSentences(t).filter(function(x){var n=atNorm(x);return n&&!said[n]&&!old(x)}).join(" ")};
  var mark=function(t,item){atSentences(t).forEach(function(x){said[atNorm(x)]=1});keysOf(t,item).forEach(function(k){facts[k]=1})};
  /* a step: its words with what was said left out; a sentence link may stand alone at a show or open step */
  var LONE={facts:1,facts_n:1,story:1,back:1,quiz:1};
  var dup={},add=function(st,lk,body,b,rule,x){x=x||{};body=fresh(body);var lone=!body&&lk&&LONE[lk]&&(st.act==="show"||st.act==="open");if(!body&&!lone){if(b)dup[b.id]=1;return null}
    var units=x.units||atSentences(body).map(function(s){return{t:s,k:keysOf(s)}});units.forEach(function(u){mark(u.t,x.items)});mark(body);
    st.at={key:st.id,block:b?b.id:null,kind:b?b.kind:null,rule:rule||"",lk:lk||null,lw:x.lw||"",ls:x.ls||"",body:body,units:units,gen:true};steps.push(st);return st};
  var rules=o.rules||{},rule=function(k){var r=AT_RULES[k];return r?(rules[k]||r.opts[0][0]):"skip"};
  /* the hook: the subtitle, the story's first sentence or the first fact, by the Editor's Hook */
  var storyB=card.filter(function(b){return b.kind==="Story"&&rule("Story")!=="skip"})[0],factsB=card.filter(function(b){return b.kind==="Quick Facts"})[0];
  var storyFirst=storyB?atSentences(o.says(storyB).text)[0]||"":"",ff=factsB?(o.says(factsB).facts||[])[0]:null,firstFact=ff?atFact(ff,lang,lf,true):"";
  var pick=o.hook==="line"?["line"]:o.hook==="fact"?["fact"]:o.hook==="story"?["story"]:["story","line","fact"],hook="";
  pick.some(function(k){var v=k==="line"?o.line:k==="story"?storyFirst:firstFact;if(v&&(k!=="story"||atWords(v)<=o.teaserCap-14)){hook=atSentences(v)[0];hookSrc=k;return true}return false});
  if(hook)mark(hook,hookSrc==="fact");nameKeys.forEach(function(k){facts[k]=1});var teaser={id:"a0",view:"card",ref:"hero",act:"teaser"};steps.push(teaser);
  /* one step per block, in the card's order */
  var lastView="card";
  card.forEach(function(b){var k=b.kind,rl=rule(k),w=o.says(b);if(rl==="skip"){skipped.push({block:b.id,kind:k,why:(AT_RULES[k]?AT_RULES[k].opts.filter(function(x){return x[0]==="skip"})[0][1]:"Skip")});return}
    var id="a:"+b.id,base={id:id,view:"card",ref:"b:"+b.id},back=lastView==="story",lk=back?"back":"neutral",st=null;
    var mk=function(act,lk2,body,extra,x){st=add(Object.assign({},base,{act:act},extra||{}),lk2,body,b,rl,x);if(st)lastView=st.view;return st};
    /* a heading or a title leads the words after it as the link (Now, what was inside:) when it is short enough */
    var head=function(t){t=String(t||"").replace(/[.:!?…]+$/,"").trim();return t&&atWords(fmt(W("at_link_head"),t))<=6?t:""};
    if(k==="Story"){var rows=story||[],first=rows[0];var open=add({id:id,view:first?"story":"card",ref:first?"r:"+first.id:"b:"+b.id,act:"open"},"story",atEnd(w.title),b,rl);if(open)lastView=open.view;
      if(rl!=="read"||!first)return;var n=0,lead="";
      rows.slice(1).forEach(function(r){if(n>=o.storyMax)return;var rs=atRowSays(r,lang),rid="a:r:"+r.id,rb={id:rid,view:"story",ref:"r:"+r.id},s2=null;
        if(rs.kind==="head"){lead=rs.text;return}
        if(rs.kind==="par"){var c=atCapText(rs.text,o.cap);s2=add(Object.assign(rb,{act:"read"}),null,c.text,b,rl);if(s2)s2.at.cut=c.cut}
        else if(rs.kind==="ba"){var hd=rs.lead||lead,tx=fresh(atEnd(rs.text));s2=add(Object.assign(rb,{act:"try",wait:"drag"}),hd?null:"neutral",(hd?atEnd(hd)+" ":"")+tx+(/drag|arraste/i.test(tx)?"":" "+W("at_try_drag")),b,rl)}
        else if(rs.kind==="show"){var l2=rs.lead||lead,hh=head(l2),cc=atCapText(rs.text,o.cap-8);
          s2=hh?add(Object.assign(rb,{act:"show"}),"head",atEnd(cc.text),b,rl,{lw:hh}):add(Object.assign(rb,{act:"show"}),l2?null:"neutral",l2?atEnd(l2.replace(/[.:]$/,"")+": "+lf(cc.text)):atEnd(cc.text),b,rl);if(s2)s2.at.cut=cc.cut}
        else if(rs.kind==="cap"){var cp=atEnd(rs.cap);s2=lead?add(Object.assign(rb,{act:"show"}),null,atEnd(lead)+" "+cp,b,rl):add(Object.assign(rb,{act:"show"}),(AT_DET[lang]||AT_DET.en).test(cp)?"cap":"pics",cp,b,rl)}
        if(s2){s2.at.key=rid;n++;lead="";lastView="story"}});return}
    if(k==="Quick Facts"){if(o.pre)return;var fs=(w.facts||[]).filter(function(f){return f[0]&&f[1]});if(hookSrc==="fact")fs=fs.slice(1);
      /* a fact said already, or told later by a sentence of the tour (all its keys in one), is left out */
      fs=fs.filter(function(f){var kk=keysOf(atFact(f,lang,lf,true),true);return!(kk.length&&(kk.every(function(x){return atKnown(facts,x)})||(o.later||[]).some(function(L){return kk.every(function(x){return L.indexOf(x)>=0})})))});
      fs=fs.slice(0,3);if(w.cat&&fs.length<3&&!old(atFact(w.cat,lang,lf,true),true))fs.push(w.cat);
      var ps=fs.map(function(f,i){return atFact(f,lang,lf,!i)}),sen=ps.join(ps.some(function(x){return x.indexOf(": ")>=0})?"; ":", "),units=ps.map(function(x){return{t:x,k:keysOf(x,true)}});
      var fl=back?"back":steps.length===1?"facts":"facts_n";if(fl==="facts_n"&&ps.length===1&&ps[0].indexOf(": ")>0&&head(ps[0].split(": ")[0])){var hd2=ps[0].split(": ");mk("show","head",atEnd(hd2.slice(1).join(": ")),null,{lw:hd2[0],units:units,items:true});return}
      mk("show",fl,ps.length?atEnd(sen.charAt(0).toUpperCase()+sen.slice(1)):"",null,{units:units,items:true});return}
    if(k==="Inline"){if(w.flip&&rl==="read"){mk("try",null,(w.title?atEnd(w.title)+" ":"")+atEnd(w.text)+" "+W("at_try_flip"),{wait:"flip"});return}
      if(rl==="read"&&b.c&&w.text){var ct=atCapText(w.text,o.cap);if(mk("read",null,ct.text))st.at.cut=ct.cut;return}
      var th=!back&&head(w.title);mk("show",th?"head":lk,atEnd(atSentences(w.text)[0]||""),null,{lw:th||""});return}
    if(k==="Timeline"){/* its first and last events (or all, by its rule), less those whose fact was said; two or more after Then the dates: …, one as And in 1755, … */
      var ev=(w.events||[]).filter(function(e){return e[0]&&e[1]});if(rl==="ends"&&ev.length>2)ev=[ev[0],ev[ev.length-1]];ev=ev.map(function(e){var c=/^c\.\s*/.test(e[0]),d=String(e[0]).replace(/^c\.\s*/,""),dt=c?W("at_circa")+" "+d:d,t=atEnd(dt+": "+lf(e[1]));return{c:c,d:d,dt:dt,x:e[1],t:t,k:keysOf(t,true)}})
        .filter(function(u){return!(u.k.length&&u.k.every(function(x){return atKnown(facts,x)}))});if(!ev.length){dup[b.id]=1;return}
      var eu=ev.map(function(u){return{t:u.t,k:u.k}});
      if(back){mk("show","back",ev.map(function(u){return u.t}).join(" "),null,{units:eu,items:true});return}
      if(ev.length===1){var u=ev[0],yr=/^\d{3,4}$/.test(u.d);if(u.c||yr){mk("show",u.c?"circa":"year",atEnd(u.x.charAt(0).toUpperCase()+u.x.slice(1)),null,{lw:u.d,units:eu,items:true});return}
        mk("show","neutral",u.t,null,{units:eu,items:true});return}
      mk("show","dates",atEnd(ev.map(function(u,i){return(i?lf(u.dt):u.dt)+", "+lf(u.x)}).join("; ")),null,{units:eu,items:true});return}
    if(k==="Gallery"){var gc=w.cap?fresh(atEnd(w.cap)):"";if(gc){mk("show",back?"back":(AT_DET[lang]||AT_DET.en).test(gc)?"cap":"pics",gc);return}mk("show",lk,fmt(W("at_pics"),w.n||""));return}
    if(k==="Before and After"){var lb=(w.labels||[]).filter(Boolean),pair=lb.length===2?fresh(atEnd(lb[0]+" "+W("at_and")+" "+lf(lb[1]))):"";
      if(rl==="try")mk("try",lk,(pair?pair+" ":"")+W("at_try_drag"),{wait:"drag"});else mk("show",lk,pair||W("at_show_look"));return}
    if(k==="Knowledge Check"){if(!w.q)return;if(rl==="try")mk("try",back?"back":"quiz",atEnd(w.q)+" "+W("at_try_answer"),{wait:"answer"});else mk("show",lk,atEnd(w.q));return}
    if(k==="Poll"){if(w.q)mk("show",lk,atEnd(w.q));return}
    if(k==="Status"){if(w.text)mk("show",lk,fmt(W("at_status"),lf(w.text)));return}
    if(k==="Compare Points"){var p=w.points||[];if(p.length>1)mk("show",lk,fmt(W("at_compare"),p[0],p[1]));return}
    if(k==="Dialogue"){if(w.lines&&w.lines[0])mk("show",lk,atEnd(w.lines[0]));return}
    if(k==="Video"){mk("show",lk,(w.cap?atEnd(w.cap)+" ":"")+W("at_show_video"));return}
    if(k==="Audio Guide"){mk("show",lk,(w.cap?atEnd(w.cap)+" ":"")+W("at_show_look"));return}
    if(k==="Hotspot Image"){mk("show",lk,W("at_show_spots"));return}
    if(k==="3D Model"){mk("show",lk,W("at_show_model"));return}
    if(k==="360 Panorama"){mk("show",lk,W("at_show_360"));return}
    if(k==="Practical Info"){var r0=(w.rows||[])[0];mk("show",lk,r0?atEnd(r0[0]+": "+r0[1]):W("at_show_look"));return}
    mk("show",lk,W("at_show_look"))});
  /* the end: one line of recap (the subtitle, else the story's title, else the first fact: one not said yet, no fact said) and "Ask me anything" */
  var recap=[hookSrc!=="line"?o.line:"",storyB?o.says(storyB).title:"",hookSrc!=="fact"?firstFact:""].filter(function(r,i){return r&&r.indexOf(":")<0&&!said[atNorm(r)]&&!old(r,i===2)})[0]||"";
  var endLine=(recap?nameIn(fmt(W("at_recap"),"{0}",lf(recap.replace(/[.!?…]+$/,""))),"{0}",o.name,lang):nameIn(W("at_recap_plain"),"{0}",o.name,lang))+" "+W("at_end");
  steps.push({id:"aend",view:"card",ref:"hero",act:"end",at:{key:"aend",block:null,kind:null,rule:"",lk:null,body:endLine,units:[],gen:true}});
  /* the links: each from its block's words (its kind's sentence, its heading, its caption, its dates), never the same twice in a row (a
     second neutral is left out); the words after a link that leads into them begin in lower case */
  var prev=null;steps.forEach(function(s){if(!s.at||!s.at.lk)return;var lk=s.at.lk,t=W(AT_LINK_KEY[lk]||"at_link");if(/\{0\}/.test(t))t=fmt(t,lk==="head"?lf(s.at.lw):s.at.lw);
    if(t===prev){s.at.lk=null;s.at.ls="";prev=null;return}s.at.link=t;s.at.ls=(lk==="head"?"from its words “"+s.at.lw+"”":AT_LINK_SRC[lk]||"");prev=t});
  steps.forEach(function(s){if(!s.at)return;var l=s.at.link,bd=s.at.body;s.text=l?(l+(bd?" "+(/[.!?…]$/.test(l)?bd:lf(bd)):"")):bd.charAt(0).toUpperCase()+bd.slice(1)});
  /* the teaser: the name, the hook, what the tour covers and how long (its minutes from the whole tour's words at the voice's speed) */
  var has=function(f){return steps.some(f)},cover=[];
  if(has(function(s){return s.at&&s.at.kind==="Story"}))cover.push(W("at_cover_story"));
  if(has(function(s){return s.wait==="drag"}))cover.push(W("at_cover_ba"));
  if(has(function(s){return s.wait==="flip"}))cover.push(W("at_cover_flip"));
  if(has(function(s){return s.at&&s.at.kind==="Timeline"}))cover.push(W("at_cover_dates"));
  if(has(function(s){return s.at&&s.at.kind==="Gallery"}))cover.push(W("at_cover_pics"));
  if(has(function(s){return s.at&&s.at.kind==="Quick Facts"&&s.at.body}))cover.push(W("at_cover_facts"));
  cover=cover.slice(0,has(function(s){return s.wait==="answer"})?2:3);if(has(function(s){return s.wait==="answer"}))cover.push(W("at_cover_quiz"));
  var nameLine=nameIn("{0}","{0}",o.name,lang),head=hook?atEnd(nameLine+": "+lf(hook).replace(/[.!?…]+$/,"")):atEnd(nameLine);
  if(atWords(head)>o.teaserCap-12)head=atCapText(head,o.teaserCap-12).text;
  /* round 16b (the developer): a tour under 2 minutes is "a short tour" (In a short tour: …), from 2 minutes its minutes (In about 3 minutes: …);
     the minutes from the words as said, the teaser's own included (made again until they agree) */
  var tease=function(n){return head+(cover.length?" "+(n<2?fmt(W("at_cover_short"),atJoin(cover,W("at_and"),lang)):fmt(W("at_cover"),n,atJoin(cover,W("at_and"),lang))):"")};
  var rest=steps.slice(1).map(function(s){return s.text}),mins=atMins(atSecs([tease(2)].concat(rest),o.speed));
  for(var it=0;it<3;it++){var m2=atMins(atSecs([tease(mins)].concat(rest),o.speed));if(m2===mins)break;mins=m2}
  teaser.text=tease(mins);teaser.at={key:"a0",block:null,kind:"Header",rule:"teaser",lk:null,body:teaser.text,gen:true,hook:hookSrc,mins:mins};
  steps.skipped=skipped;steps.said=dup;return steps}

/* ----- the quality rules, checked (the Editor's line under the tour, and the tests): the teaser first at the hero (two sentences at most,
   the name, the minutes), each line under its cap, no sentence twice, no two links in a row the same, each block with something to tell
   in the card's order, and nothing said that is not on the card (each word of a line is the card's, the name's or the link words') ----- */
function atCardWords(card,story,hero,lang,parts){var out=[];var push=function(v){if(v==null)return;if(Array.isArray(v))v.forEach(push);else if(typeof v==="object")Object.keys(v).forEach(function(k){if(k!=="id")push(v[k])});else out.push(String(v))};
  card.forEach(function(b){push(blockSays(b,lang))});(story||[]).forEach(function(r){var s=atRowSays(r,lang);push([s.text,s.lead,s.cap,s.title])});if(hero)push([wordIn(hero.title,lang),wordIn(hero.line||UI.line,lang)]);
  return parts?out:out.join(" ")}
function atLinkWords(lang){return Object.keys(S.tx).filter(function(k){return/^at_/.test(k)}).map(function(k){return wordIn(S.tx[k],lang)}).join(" ")}
function atCheck(steps,lang,src){var r=[],ok=function(name,cond,why){r.push({rule:name,ok:!!cond,why:cond?"":why})},cap=src.cap,tc=src.teaserCap,t0=steps[0]||{},tt=t0.text||"";
  var name=src.name||"";ok("teaser",t0.act==="teaser"&&t0.ref==="hero"&&atSentences(tt).length<=2&&atNorm(tt).indexOf(atNorm(name.replace(/^(the|o|a|os|as)\s+/i,"")))>=0&&/\d|minute|minuto|short|curta/i.test(tt),"step 0 is not the teaser at the hero, with the name and the minutes (or a short tour) in two sentences at most");
  var over=steps.filter(function(s,i){return atWords(s.text)>(i===0?tc:cap+12)});ok("cap",!over.length,over.length+" line"+(over.length>1?"s":"")+" over the cap: "+over.map(function(s){return"“"+String(s.text).slice(0,40)+"…”"}).join(", "));
  var seen={},twice=[];steps.forEach(function(s){atSentences(s.text).forEach(function(x){var k=atNorm(x);if(k&&seen[k])twice.push(x);seen[k]=1})});ok("twice",!twice.length,"said twice: “"+twice.join("”, “")+"”");
  var ls=steps.filter(function(s){return s.at&&s.at.link}).map(function(s){return s.at.link}),rep=ls.filter(function(l,i){return i&&ls[i-1]===l});ok("links",!rep.length,"the same link twice in a row: “"+rep.join("”, “")+"”");
  var want=src.card.filter(function(b){var k=AT_RULES[b.kind];return k&&(src.rules[b.kind]||k.opts[0][0])!=="skip"}).map(function(b){return b.id}),got=[];steps.forEach(function(s){if(s.at&&s.at.block&&got.indexOf(s.at.block)<0)got.push(s.at.block)});
  var miss=want.filter(function(id){return got.indexOf(id)<0&&!(steps.said||{})[id]}),order=got.filter(function(id){return want.indexOf(id)>=0}).join()===want.filter(function(id){return got.indexOf(id)>=0}).join();
  var skipT=steps.filter(function(s){return s.at&&s.at.block&&want.indexOf(s.at.block)<0});
  ok("blocks",!miss.length&&order&&!skipT.length,(miss.length?"no step for "+miss.map(function(id){return atBlockName(byId(src.card,id))}).join(", "):"")+(order?"":" not in the card's order")+(skipT.length?" a skipped kind told: "+skipT.map(function(s){return s.at.kind}).join(", "):""));
  var have=" "+atNorm(atCardWords(src.card,src.story,src.hero,lang)+" "+name+" "+atLinkWords(lang))+" ",stray=[];
  steps.forEach(function(s){atNorm(s.text).split(" ").forEach(function(w){if(w&&!/^\d+$/.test(w)&&have.indexOf(" "+w+" ")<0&&stray.indexOf(w)<0)stray.push(w)})});
  ok("card",!stray.length,"words not on the card: "+stray.slice(0,8).join(", "));
  /* round 16b: no fact twice. Each sentence (a list's facts and events each, split at , and ;) with fact keys must bring one not said
     before (the point's name counts as said by the teaser, for a list's item); a Quick Facts item's keys must not all come again in one
     later unit (the fuller telling is the later one's) */
  var nk=atNameKeys(name,lang),known={},units=[],twiceF=[],again=[];
  steps.forEach(function(s,i){var item=s.at&&(s.at.kind==="Quick Facts"||s.at.kind==="Timeline");atSentences(s.text).forEach(function(x){(item?x.split(/[;,]\s+/):[x]).forEach(function(u){units.push({i:i,t:u,qf:item&&s.at.kind==="Quick Facts",k:atFactKeys(u,lang,{proper:src.proper,not:item?null:nk})})})})});
  nk.forEach(function(k){known[k]=1});units.forEach(function(u){if(u.i>0&&u.k.length&&u.k.every(function(k){return atKnown(known,k)}))twiceF.push(u.t);u.k.forEach(function(k){known[k]=1})});
  units.forEach(function(u,j){if(!u.qf||!u.k.length)return;if(units.slice(j+1).some(function(v){return v.i>u.i&&u.k.every(function(k){return v.k.indexOf(k)>=0})}))again.push(u.t)});
  ok("facts",!twiceF.length&&!again.length,(twiceF.length?"a fact said twice: “"+twiceF.join("”, “")+"”":"")+(again.length?(twiceF.length?"; ":"")+"a fact told again later: “"+again.join("”, “")+"”":""));return r}

/* ----- a point's tour: its source, the steps made from its card (with the stored AI lines), its minutes ----- */
function atTour(poi){poi=poi||S.m&&S.m.poi||"painting";return S.tours[poi]=S.tours[poi]||{src:"card"}}
function atSrc(poi){var t=atTour(poi);return t.src==="written"&&(t.written||poi===(S.m&&S.m.poi||"painting")&&!atIsGen(S.guideSteps))?"written":"card"}
function atIsGen(a){return!!(a&&a.length&&a.every(function(s){return s.act==="intro"||s.at&&s.at.gen}))}
function atBlockName(b){return b?"the "+b.kind:"a block no longer on the card"}
function atOpts(poi,lang){var p=pointOf(poi),hero=p.hero||{},cfg=S.guideCfg,proper=atProper(atCardWords(p.card||[],p.story||[],hero,lang,true));
  return{name:wordIn(hero.title,lang),line:wordIn(hero.line||(poi==="painting"?UI.line:null),lang),says:function(b){return blockSays(b,lang)},W:function(k){return wordIn(S.tx[k],lang)},
    rules:cfg.atRule,cap:+cfg.atCap,teaserCap:+cfg.atTeaserCap,storyMax:+cfg.atStoryMax,hook:cfg.atHook,speed:typeof guideSpeed==="function"?guideSpeed():1,lowerOk:function(w){return!proper[w]},proper:proper,
    card:p.card||[],story:p.story||[],hero:hero}}
function atHash(o){return hashOf(o)}
/* the tour made from a point's card, in every language (EN's steps, each language's line), with its stored AI lines while the box is ticked */
function atSteps(poi,raw){poi=poi||S.m&&S.m.poi||"painting";var p=pointOf(poi),langs=langCodes(),by={};
  langs.forEach(function(l){by[l]=tourFromCard(p.card||[],p.story||[],l,atOpts(poi,l))});var base=by.en||by[langs[0]],ai=atTour(poi).ai||{};
  var out=base.map(function(st){var t={};langs.forEach(function(l){var m=by[l].filter(function(x){return x.id===st.id})[0];t[l]=m?m.text:st.text});
    var s=Object.assign({},st,{text:t,at:Object.assign({},st.at,{tpl:t})}),a=ai[st.id];
    if(!raw&&S.guideCfg.autoAI&&a&&!a.off&&a.base===atHash(t)&&langs.some(function(l){return a[l]&&a[l]!==t[l]})){s.text={};langs.forEach(function(l){s.text[l]=a[l]||t[l]});s.at.ai=true;if(s.act==="read")s.act="show"}return s});
  out.skipped=base.skipped;out.said=base.said;return out}
/* the open point's tour as the phone plays it: its written tour, or the one made from its card (made again when no tour plays) */
function atActivate(){if(G.on||S.fullSteps||S.howSaved)return;var poi=S.m&&S.m.poi||"painting",t=atTour(poi);
  if(!atIsGen(S.guideSteps))t.written=S.guideSteps;
  if(t.src==="written"&&t.written){if(S.guideSteps!==t.written)S.guideSteps=t.written}else S.guideSteps=atSteps(poi)}
on("point.opened",function(id,from){var ft=atTour(from);if(!atIsGen(S.guideSteps))ft.written=S.guideSteps;var t=atTour(id);if(!t.written&&t.src==="written")t.src="card";
  S.guideSteps=t.src==="written"&&t.written?t.written:atSteps(id);S.sel.gstep=null});
on("renderPhone.before",function(){atActivate()});
on("tourMinutes.answer",function(id){var poi=id||S.m&&S.m.poi||"painting";if(!POINTS[poi])return;if(poi===(S.m&&S.m.poi||"painting")?atSrc(poi)==="written":atTour(poi).src==="written"&&atTour(poi).written)return;
  return atMins(atSecs(atSteps(poi).map(function(s){return L(s.text)}),typeof guideSpeed==="function"?guideSpeed():1))});
/* a demo plays the tour made from the card even where the written one is the point's: for that tour only */
on("startTour.before",function(){if(!G.atForce||S.howSaved)return;var poi=S.m&&S.m.poi||"painting";if(!atIsGen(S.guideSteps)){atTour(poi).written=S.guideSteps;S.guideSteps=atSteps(poi);G.atSwapped=true}});
on("stopTour.after",function(){G.atForce=false;if(G.atSwapped){G.atSwapped=false;var t=atTour();if(t.written)S.guideSteps=t.written}});
/* Ask Azu and a step's name on another point's card: its blocks' own words (the demo point keeps its corpus) */
on("corpus.list",function(out){var poi=S.m&&S.m.poi||"painting";if(poi==="painting")return out;
  return out.filter(function(c){return c.view!=="card"&&c.view!=="story"}).concat(S.card.map(function(b){var w=blockSays(b,S.lang),bits=[];(function walk(v){if(v==null)return;if(Array.isArray(v))v.forEach(walk);else if(typeof v==="object")Object.keys(v).forEach(function(k){walk(v[k])});else if(typeof v==="string")bits.push(v)})(w);
    return{ref:"b:"+b.id,view:"card",title:b.kind,text:bits.join(" ")}}))});

/* ----- Make it mine, Regenerate this step, the changed blocks, Use the template line ----- */
function atMine(go){var poi=S.m&&S.m.poi||"painting",t=atTour(poi),has=t.written&&t.written.length&&!atIsGen(t.written);
  if(has&&!go){S.atDecide=poi;renderEditor();return}S.atDecide=null;var gen=atSteps(poi);
  var w=gen.map(function(s){var c=JSON.parse(JSON.stringify(s));c.at.gen=false;c.at.base=JSON.parse(JSON.stringify(c.text));delete c.at.ai;return c});t.written=w;t.src="written";t.made=true;t.dismissed=[];S.guideSteps=w;S.sel.gstep=null;renderAll(true)}
function atRegen(id){var st=byId(S.guideSteps,id);if(!st||!st.at)return;var g=atSteps().filter(function(x){return x.at.key===st.at.key})[0];
  if(!g){S.atNote="“"+(L(st.text)||"").slice(0,30)+"…”: its block is no longer told by the card's rules, so the step stays as it is.";renderEditor();return}
  st.text=JSON.parse(JSON.stringify(g.text));st.act=g.act;st.view=g.view;st.ref=g.ref;if(g.wait)st.wait=g.wait;else delete st.wait;st.at.base=JSON.parse(JSON.stringify(st.text));S.atNote=null;renderAll(true)}
function atEdited(st){return!!(st.at&&st.at.base&&JSON.stringify(st.text)!==JSON.stringify(st.at.base))}
/* the blocks with something to tell that a made tour does not hold (and were not dismissed) */
function atNewBlocks(){var poi=S.m&&S.m.poi||"painting",t=atTour(poi);if(!t.made||atSrc(poi)!=="written")return[];var w=S.guideSteps,dis=t.dismissed||[];
  var inT=function(id){return w.some(function(s){return s.at&&s.at.block===id||s.ref==="b:"+id})};
  var gen=atSteps(poi,true),ids=[];gen.forEach(function(s){var b=s.at&&s.at.block;if(b&&ids.indexOf(b)<0&&!inT(b)&&dis.indexOf(b)<0)ids.push(b)});return ids.map(function(id){return byId(S.card,id)}).filter(Boolean)}
function atAddNew(){var add=atNewBlocks(),gen=atSteps(null,true),w=S.guideSteps,order=S.card.map(function(b){return b.id});
  add.forEach(function(b){var mine=gen.filter(function(s){return s.at.block===b.id}).map(function(s){var c=JSON.parse(JSON.stringify(s));c.at.gen=false;c.at.base=JSON.parse(JSON.stringify(c.text));return c});
    var at=order.indexOf(b.id),j=-1;w.forEach(function(s,i){var bi=s.at&&s.at.block?order.indexOf(s.at.block):s.ref&&s.ref.indexOf("b:")===0?order.indexOf(s.ref.slice(2)):-1;if(bi>=0&&bi<at)j=i});
    if(j<0)j=0;var end=w.findIndex(function(s){return s.act==="end"});var ins=Math.min(j+1,end<0?w.length:end);Array.prototype.splice.apply(w,[ins,0].concat(mine))});renderAll(true)}

/* ----- Make it more fluid, entertaining and human with AI (the developer, 2026-11-12): one box for every automatic tour. The Editor (never the
   app) asks the model Ask Azu is set to for each tour's teaser, links and long summaries in Azu's voice, from the card's words only; the
   answer is stored as that tour's lines (S.tours[point].ai, each with the template line it rewrote: a changed card brings the template
   back), so the visitor hears stored words. A line with a new name or number, or over its cap, keeps the template. No model: the row
   says so and the template stays ----- */
function atAIWant(st){return st.act==="teaser"||st.act==="end"||!!(st.at&&(st.at.link||st.at.cut))}
function atAIRun(){if(!S.guideCfg.autoAI)return;if(typeof sampleFn!=="function"||!sampleFn){S.atAINote="No model on this page (offline, or no key for Ask Azu's model): the template tours stay.";renderEditor();return}
  var pts=Object.keys(POINTS),left=pts.length,res={lines:0,kept:0,why:[]};S.atAINote="Asking the model for "+pts.length+" tours…";S.atAIBusy=true;renderEditor();
  pts.forEach(function(poi){var gen=atSteps(poi,true),p=pointOf(poi),want=gen.filter(atAIWant),src={en:atCardWords(p.card||[],p.story||[],p.hero,"en"),pt:atCardWords(p.card||[],p.story||[],p.hero,"pt")};
    var prompt="You are Azu, the warm, lively guide of a heritage app. Rewrite these lines of a narrated tour of "+wordIn((p.hero||{}).title,"en")+" so they sound fluid, entertaining and human, spoken aloud. Use ONLY the facts in the CARD below: never add a name, a date, a number or a fact it does not have. Keep each line's meaning, its instructions to the visitor (tap, drag) and its length (at most "+S.guideCfg.atCap+" words; the teaser "+S.guideCfg.atTeaserCap+", two sentences, with the minutes it says). European Portuguese for pt. Answer with JSON only: {\"steps\":[{\"id\":\"…\",\"en\":\"…\",\"pt\":\"…\"}]}.\n\nCARD (en): "+src.en+"\nCARD (pt): "+src.pt+"\n\nLINES:\n"+JSON.stringify(want.map(function(s){return{id:s.id,en:s.text.en,pt:s.text.pt}}));
    var done=function(){if(--left>0)return;S.atAIBusy=false;S.atAINote="Rewritten by the model: "+res.lines+" line"+(res.lines===1?"":"s")+" in "+pts.length+" tours"+(res.kept?"; "+res.kept+" kept as the template ("+res.why.slice(0,3).join("; ")+")":"")+". Each step shows its template line beside it.";renderAll(true)};
    Promise.resolve().then(function(){return sampleFn(prompt,{modelTier:"quick"})}).then(function(r){var txt=String(r&&r.text||""),m=txt.indexOf("{"),j=JSON.parse(txt.slice(m,txt.lastIndexOf("}")+1)),ai={};
      (j.steps||[]).forEach(function(x){var st=byId(want,x.id);if(!st||!x.en||!x.pt)return;var cap=st.act==="teaser"?+S.guideCfg.atTeaserCap:+S.guideCfg.atCap+12,nw=newTokens([x.en,x.pt].join(" "),{en:src.en+" "+st.text.en+" "+atLinkWords("en"),pt:src.pt+" "+st.text.pt+" "+atLinkWords("pt")});
        if(nw.length||atWords(x.en)>cap||atWords(x.pt)>cap){res.kept++;res.why.push((nw.length?"new words: "+nw.slice(0,3).join(", "):"over its cap"));return}ai[x.id]={base:atHash(st.text),en:String(x.en),pt:String(x.pt)};res.lines++});
      atTour(poi).ai=ai;done()}).catch(function(e){res.why.push("the model did not answer for "+(POINTS[poi].label||poi)+" ("+(e&&e.code||e&&e.message||"error")+")");res.kept+=want.length;done()})})}
on("setOpt.answer",function(key,v){if(key==="@guideCfg.autoAI"){S.guideCfg.autoAI=v==="true";if(S.guideCfg.autoAI)atAIRun();else S.atAINote=null;renderAll(true);return true}
  var m=/^@tours\.([^.]+)\.src$/.exec(key);if(!m)return;var t=atTour(m[1]);if(v==="written"&&!t.written&&atIsGen(S.guideSteps)){S.atNote="This point has no written tour yet: Make it mine copies the one from its card.";renderEditor();return true}
  if(m[1]===(S.m&&S.m.poi||"painting")&&!atIsGen(S.guideSteps))t.written=S.guideSteps;t.src=v;S.atNote=null;atActivate();renderAll(true);return true});

/* ----- the Editor: Guide › 2 · The tour › Tour from the card; the same list in Specific Marker › 3 · Guide script ----- */
function atRuleLabel(k,v){var o=(AT_RULES[k]||{opts:[]}).opts.filter(function(x){return x[0]===v})[0];return o?o[1]:v}
function atStepBlock(st){if(st.ref==="hero")return st.act==="end"?"The end (on the hero)":"The hero · the teaser";var b=st.at&&st.at.block&&byId(S.card,st.at.block);
  if(st.ref.indexOf("r:")===0){var r=byId(S.story,st.ref.slice(2)),i=S.story.indexOf(r);return"The Story · page row "+(i+1)}return b?b.kind:st.ref}
function atChecksHtml(poi){var gen=atSteps(poi,true),rows=langCodes().map(function(l){var st=gen.map(function(s){return Object.assign({},s,{text:s.text[l]})});st.said=gen.said;return{l:l,r:atCheck(st,l,Object.assign(atOpts(poi,l),{rules:S.guideCfg.atRule}))}});
  var bad=[];rows.forEach(function(x){x.r.forEach(function(c){if(!c.ok)bad.push(x.l.toUpperCase()+" · "+c.why)})});
  return'<div class="note '+(bad.length?"warn":"atok")+'">'+(bad.length?"<b>Checks:</b> "+bad.map(esc).join("<br>"):"<b>Checks, EN and PT:</b> the teaser first, at the hero · every line under its cap · no sentence twice · no fact twice · no link twice in a row · every block with something to tell, in the card's order · only the card's words and the link words.")+'</div>'}
function atListHtml(where){var poi=S.m&&S.m.poi||"painting",gen=atSteps(poi),secs=0,nm=POINTS[poi]&&POINTS[poi].label||poi,t=atTour(poi);
  var h='<div class="note">The tour made from '+esc(L(pointOf(poi).hero.title))+'\'s card: '+gen.length+' steps, '+(tourMinutes(poi)<2?"a short tour (under 2 minutes)":"about "+tourMinutes(poi)+" min")+'. Read only: <b>Make it mine</b> copies it into the written tour, to edit each step\'s words, what it does, keep or skip, and its order.</div>';
  if(S.atDecide===poi){var n=(t.written||[]).length;h+='<div class="decide"><b>Replace the '+n+' written steps of '+esc(L(pointOf(poi).hero.title))+' with the '+gen.length+' made from its card?</b><span>The written tour is kept nowhere else; the card\'s tour stays here, made again from the card.</span><div class="btnrow"><button class="ubtn sm" type="button" data-at="mine-go">Replace the written tour</button><button class="del" type="button" data-at="mine-no">Keep the written tour</button></div></div>'}
  h+='<div class="btnrow"><button class="ubtn sm" type="button" data-at="mine">Make it mine</button></div>';
  h+='<table class="tbl attbl"><thead><tr><th style="width:22px">#</th><th style="width:24%">Block</th><th style="width:62px">Does</th><th>Says</th><th style="width:58px">Length</th></tr></thead><tbody>'+gen.map(function(st,j){var tx=L(st.text),w=atWords(tx),s=Math.round(atSecs([tx],typeof guideSpeed==="function"?guideSpeed():1));
    var ai=st.at.ai?'<div class="atai-was"><small>Template line</small> '+esc(L(st.at.tpl))+' <button class="del" type="button" data-at="tpl|'+esc(st.id)+'">Use the template line</button></div>':"";
    var lk=st.at.link?'<div class="atlnk"><small>Link</small> “'+esc(st.at.link)+'” <span>· '+esc(st.at.ls||"from its block")+'</span></div>':"";
    return'<tr data-atstep="'+esc(st.id)+'"><td class="uses">'+(j+1)+'</td><td>'+esc(atStepBlock(st))+'</td><td class="uses">'+st.act+(st.wait?" · "+st.wait:"")+'</td><td>'+(st.at.ai?'<small class="atai-tag">AI</small> ':"")+esc(tx)+lk+ai+'</td><td class="uses">'+w+' w · '+s+' s</td></tr>'}).join("")+'</tbody></table>';
  var sk=gen.skipped||[];if(sk.length)h+='<div class="note">Not in the tour: '+sk.map(function(x){return esc(x.kind)+" ("+esc(x.why.replace(/^Skip \(?|\)$/g,"")||"skipped")+")"}).join(" · ")+'. Their rule is in Guide › 2 · The tour › What each block says.</div>';
  return h+atChecksHtml(poi)}
function atGoneSteps(){var poi=S.m&&S.m.poi||"painting",t=atTour(poi);if(!t.made||atSrc(poi)!=="written")return[];return S.guideSteps.filter(function(s){var id=s.at&&s.at.block||(s.ref&&s.ref.indexOf("b:")===0?s.ref.slice(2):null);return id&&!byId(S.card,id)&&!s.skip})}
function atNoticeHtml(){var nb=atNewBlocks(),gone=atGoneSteps(),gh=gone.length?'<div class="note warn">'+gone.length+' step'+(gone.length===1?"'s block is":"s' blocks are")+' no longer on the card: '+gone.map(function(s){return esc(s.at&&s.at.kind?"the "+s.at.kind:"a block")}).join(", ")+'. '+(gone.length===1?"It stays":"They stay")+' in the tour until you skip '+(gone.length===1?"it":"them")+' (Keep › Skip) or Regenerate.</div>':"";if(!nb.length)return gh;return'<div class="decide atnew"><b>'+nb.length+' new block'+(nb.length===1?"":"s")+' not in the tour: '+nb.map(function(b){return esc(atBlockName(b).replace(/^the /,"the "))}).join(", ")+'.</b><span>The card changed after Make it mine. Add them puts their steps where the card has them.</span><div class="btnrow"><button class="ubtn sm" type="button" data-at="addnew">Add them</button><button class="del" type="button" data-at="dismiss">Dismiss</button></div></div>'+gh}
on("edCard.html",function(h){var d=document.createElement("div");d.innerHTML=h;var at=d.querySelector('[data-fo="s7voice"]');if(!at)return d.innerHTML;var poi=S.m&&S.m.poi||"painting",src=atSrc(poi),nm=L(pointOf(poi).hero.title),c=S.guideCfg;
  var ai=!!c.autoAI,noModel=typeof sampleFn!=="function"||!sampleFn;
  var body='<div class="note atbase">Every Detail Card gets an automatic tour made from its blocks, with no AI needed.</div>'+
    row("With AI",'<label class="atchk"><input type="checkbox" data-set="@guideCfg.autoAI" data-v="'+(!ai)+'"'+(ai?" checked":"")+'> Make it more fluid, entertaining and human with AI</label>'+(S.atAINote||noModel?'<span class="uses atainote">'+esc(S.atAINote||"No model on this page (offline, or no key for Ask Azu's model): ticking it keeps the template tours.")+'</span>':""),
      "Off: every automatic tour is the template made from the card's blocks (the base quality: it reads well on its own) and no model is ever called. On: the Editor asks the model Ask Azu is set to (5 · Ask Azu › The engine) to rewrite each automatic tour's teaser, links and long summaries in Azu's voice, from the card's words only; the answer is stored as that tour's lines, so a visitor hears stored words and the app never calls a model for the tour. Ticking it is the acceptance; each step keeps its template line beside it, with Use the template line. A line with a new name or number, or over its cap, keeps the template. With no key or offline, it says so here and the template tours stay.")+
    row("Tour of "+nm,pseg("tours."+poi+".src",["written","card"],["Written","From the card (automatic)"]),"Written: the tour written for this point (Specific Marker › 3 · Guide script); a written tour always wins. From the card: the tour made from the card's blocks, the default of a point with no written tour; switch to it to compare. A point keeps its own choice.")+
    (S.atNote?'<div class="note warn">'+esc(S.atNote)+'</div>':"")+
    (src==="card"?atListHtml("guide"):'<div class="note">'+esc(nm)+' plays its written tour ('+S.guideSteps.length+' steps, Specific Marker › 3 · Guide script). From the card shows the tour made from its blocks.</div>'+atNoticeHtml())+
    fo("at:rules",'What each block says <span class="sub">· one rule a kind of block, in the card\'s order; the blocks with nothing to tell are skipped</span>',null,
      row("Header · the teaser",psel("guideCfg.atHook",AT_HOOKS),"Step 0, always, said while the hero shows, before the first block: the point's name, one hook from the card and what the tour covers with its minutes (\"In about 4 minutes: its story, a before and after, and a question to finish.\"; under 2 minutes \"In a short tour: …\"). Two sentences at most. The hook's facts are said here first: no later step says them again.")+
      Object.keys(AT_RULES).map(function(k){return row(k,psel("guideCfg.atRule."+k,AT_RULES[k].opts),atRuleHelp(k),true)}).join(""),3)+
    row("Words a step",pslider("guideCfg.atCap",20,60,1),"The longest a step's own words may be (40): a long text is said in its first sentences up to it (cut with … only when its first sentence is longer); a link adds a few words.")+
    row("Teaser words",pslider("guideCfg.atTeaserCap",25,60,1),"The longest the teaser may be (45 words), its two sentences together.")+
    row("Story pages",pslider("guideCfg.atStoryMax",2,12,1),"How many of the story's rows the tour tells (6), in order: the paragraphs read, a picture or a callout shown, a before and after tried; a heading leads the row after it.")+
    '<div class="note">Unity: GuideTourBuilder.FromCard(card, language, rules), plain C#, run by the POI Editor and at load for a marker with no Guide Script; its link words are CardStrings (at_*). A line with no recorded clip is spoken by the TTS path Ask Azu\'s answers take (this device\'s voice, or ElevenLabs as Ask Azu › Voice of the answers says); Narration › Speaking now says which.</div>';
  at.insertAdjacentHTML("beforebegin",comp("s7auto","the tour made from each point's card: the default with no written tour",body));
  var f=innerOf(d,"s7auto");if(f)f.insertAdjacentHTML("beforeend",ownbar("s7auto")+ownPanels("s7auto"));return d.innerHTML});
function atRuleHelp(k){var o=AT_RULES[k].opts;return{"Inline":"A text block is read in its own words, the words lit as they are said (a long one in its first sentences); a flip card becomes a try step: Azu says its front and waits for the flip, then reads its back.",
  "Story":"The story opens at its title (\"Now the story.\"), then its rows are told in order, up to Story pages.","Quick Facts":"The facts as one sentence: the first two or three pairs, label and value (\"Royal home for about 250 years, lost 1 November 1755.\"), each fact not said yet and not told later by a sentence of the tour; with none left, only its link (\"First, the facts.\").",
  "Timeline":"Its first and last events (Every date: all of them), each with its date (c. is said as Around), less those whose fact the tour said already: two or more after \"Then the dates:\", one as \"And in 1755, …\".","Before and After":"A try step: its two labels, then \"Your turn: drag across the picture.\"; the guide waits for the drag (Invite to try).",
  "Knowledge Check":"A try step: \"Ready for a question?\", the question and \"Tap your answer.\"; the guide waits for the answer."}[k]||(o[0][0]==="skip"?"Nothing to tell by default: "+o[0][1].replace(/^Skip \(|\)$/g,"")+". Show it: a link and its words.":"Shown with its own words: "+o[0][1].toLowerCase()+".")}
on("edMarker.html",function(h){var d=document.createElement("div");d.innerHTML=h;var m3=innerOf(d,"m3");if(!m3)return d.innerHTML;var poi=S.m&&S.m.poi||"painting";
  if(atSrc(poi)==="card"){var tb=m3.querySelector(":scope > table.tbl"),nt=m3.querySelector(":scope > .note");var x=document.createElement("div");x.className="atlist";
    x.innerHTML='<div class="note">This point plays the tour made from its card (Guide › 2 · The tour › Tour from the card).</div>'+atListHtml("marker");if(tb)tb.replaceWith(x);else m3.insertAdjacentElement("afterbegin",x);if(nt)nt.remove();return d.innerHTML}
  m3.insertAdjacentHTML("afterbegin",(S.atNote?'<div class="note warn">'+esc(S.atNote)+'</div>':"")+atNoticeHtml());
  var st=S.sel.gstep&&byId(S.guideSteps,S.sel.gstep),slot=st&&st.at&&m3.querySelector('[data-pick="gstep"][data-id="'+cssq(st.id)+'"] + tr.slot td:last-child');
  if(slot)slot.insertAdjacentHTML("afterbegin",row("From the card",'<button class="del" type="button" data-at="regen|'+esc(st.id)+'">Regenerate this step</button>'+(atEdited(st)?' <span class="uses">edited</span>':""),"Made from "+atStepBlock(st)+" by Make it mine. Regenerate this step makes it again from the card as it is now (its words, what it does); the other steps keep their edits."));
  m3.querySelectorAll('tr.pick[data-pick="gstep"]').forEach(function(r){var s=byId(S.guideSteps,r.dataset.id);if(s&&atEdited(s)){var c=r.querySelector("td:nth-child(3)");if(c)c.insertAdjacentHTML("beforeend",' · edited')}});return d.innerHTML});
ed.addEventListener("click",function(e){var b=e.target.closest("[data-at]");if(!b)return;e.stopPropagation();var q=b.dataset.at.split("|"),t=atTour();
  if(q[0]==="mine"){atMine(false);return}if(q[0]==="mine-go"){atMine(true);return}if(q[0]==="mine-no"){S.atDecide=null;renderEditor();return}
  if(q[0]==="regen"){atRegen(q[1]);return}if(q[0]==="addnew"){atAddNew();return}
  if(q[0]==="dismiss"){t.dismissed=(t.dismissed||[]).concat(atNewBlocks().map(function(x){return x.id}));renderEditor();return}
  if(q[0]==="tpl"){var a=(t.ai||{})[q[1]];if(a)a.off=true;renderAll(true);return}},true);
/* the tour's state reaches the phone's signature: the open point's tour as it would play, and the demo card's tour (every kind's rule) */
on("modelSig.sig",function(h){if(!sigWant(/guideCfg\.(at|autoAI)|^@?tours\./))return h;var a=function(st){return st.map(function(s){return s.id+":"+s.act+":"+L(s.text)}).join("|")};
  var src=atSrc(),cur=src==="card"?atSteps():S.guideSteps,dc=tourFromCard(S.demoCard,POINTS.painting.story||S.story,S.lang,atOpts(null,S.lang)),d0=tourFromCard(S.demoCard,[],S.lang,atOpts(null,S.lang));
  /* round 16b: the demo card's tour without the story's rows too (the story tells most dates, so a Timeline's rule shows only there), and what each leaves out */
  var lo=function(st){return(st.skipped||[]).map(function(x){return x.kind}).join()+"/"+Object.keys(st.said||{}).join()};
  return h+"#at:"+src+"#"+a(cur)+"#"+tourMinutes()+"#demo:"+a(dc)+"#"+lo(dc)+"#demo0:"+a(d0)+"#"+lo(d0)});
BEHAVIOUR=new RegExp(BEHAVIOUR.source.replace("^@?(","^@?(guideCfg\\.autoAI|"));
/* each row's demo: the tour made from the card at that block's step, the block ringed (a demo plays it where the written tour is the
   point's); a kind not on this card or skipped: the card, and a chip saying why */
function atRing(){var st=G.on&&S.guideSteps[G.i];return st?(st.ref==="hero"?".hero":'[data-ref="'+st.ref+'"]'):".hero"}
function atDemoAt(kind,id){if(G.on)stopTour();var gen=atSteps(),j=id?gen.findIndex(function(s){return s.id===id}):!kind||kind==="Header"?0:gen.findIndex(function(s){return s.at&&s.at.kind===kind});
  if(j<0){var b=S.card.filter(function(x){return x.kind===kind})[0];S.view="card";S.stop="full";renderPhone(false);if(b){var e=scr.querySelector('[data-ref="b:'+b.id+'"]');if(e)scr.scrollTop=Math.max(0,e.offsetTop-160)}
    if(typeof gdChip==="function"){gdChip(b?kind+": skipped by its rule (nothing to tell)":"No "+kind+" on this card: its rule applies where one is");clearTimeout(G.atChipT);G.atChipT=setTimeout(function(){gdChip(null)},3200)}return}
  G.atForce=true;S.view="card";S.stop="full";S.introSeen=true;startTour();var want=gen[j].id,k=S.guideSteps.findIndex(function(s){return s.id===want});if(k>0){stopSpeech();G.i=k;runStep()}}
rowDemos(function(key){var m=/^guideCfg\.atRule\.(.+)$/.exec(key);if(m)return{act:function(){atDemoAt(m[1])},ringAfter:400,ring:function(){return G.on?atRing():'[data-ref^="b:"]'}};
  if(key==="guideCfg.atHook")return{act:function(){atDemoAt("Header")},ring:".hero"};
  if(key==="guideCfg.atTeaserCap")return{act:function(){atDemoAt("Header")},ring:".gdock|.gcap|.hero"};
  if(key==="guideCfg.atStoryMax")return{act:function(){atDemoAt("Story")},ringAfter:400,ring:function(){return atRing()}};
  if(key==="guideCfg.atCap"){return{act:function(){var g=atSteps(),best=g.slice(1).sort(function(a,b){return atWords(L(b.text))-atWords(L(a.text))})[0];atDemoAt(null,best&&best.id)},ringAfter:400,ring:function(){return atRing()}}}
  if(key==="guideCfg.autoAI")return{act:function(){atDemoAt("Header")},ring:".gdock|.gcap"};
  if(/^tours\.[^.]+\.src$/.test(key))return{act:function(){if(G.on)stopTour();S.view="card";S.stop="full";S.introSeen=true;startTour()},ring:".gdock|.gcap"}});
