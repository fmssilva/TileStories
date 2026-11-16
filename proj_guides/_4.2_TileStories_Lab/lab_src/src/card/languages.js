/* Languages: the list, the opening language, the texts per language in the Editor (S.langs). */
/* ----- the wall's languages (Languages & Terms > Languages): every localized field shows one field per language; an empty one shows the fallback's ----- */
S.langs={list:[{code:"en",name:"English"},{code:"pt",name:"Português (Portugal)"}],fallback:"en"};
/* ----- Languages & Terms ----- */
LANGPICK.push(["pt-BR","Português (Brasil)"],["nl","Nederlands"],["sv","Svenska"],["da","Dansk"],["nb","Norsk bokmål"],["fi","Suomi"],["pl","Polski"],["cs","Čeština"],["hu","Magyar"],["ro","Română"],["el","Ελληνικά"],["tr","Türkçe"],["ru","Русский"],["uk","Українська"],["ar","العربية"],["he","עברית"],["hi","हिन्दी"],["zh-Hans","中文（简体）"],["zh-Hant","中文（繁體）"],["ja","日本語"],["ko","한국어"],["ca","Català"],["gl","Galego"],["eu","Euskara"]);
S.langs.opensIn="phone";
on("edWords.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  addRows(d,"w1",prow("Opens in",pseg("langs.opensIn",["phone","fallback"],["The phone's language when the wall has it","The fallback"]),"The card's language at the first open; the visitor's own pick wins after that."),"Fallback language");
  var lt=innerOf(d,"w1");if(lt)lt.querySelectorAll("tbody tr").forEach(function(r,i){var c=r.lastElementChild;if(c)c.insertAdjacentHTML("afterbegin",'<button class="del" type="button" data-lmove="'+i+'|-1" aria-label="Up">^</button><button class="del" type="button" data-lmove="'+i+'|1" aria-label="Down">v</button>')});
  var fb=d.querySelector("[data-txf]");if(fb&&!d.querySelector('[data-txf="app"]'))fb.parentNode.insertAdjacentHTML("beforeend",'<button class="chip" type="button" data-txf="app" aria-pressed="'+(S.ui.txf==="app")+'">App texts</button>');
  return d.innerHTML});
on("txRows.answer",function(){if(S.ui.txf==="app")return '<tr><td colspan="9" class="uses">This wall\'s app adds no blocks of its own. An app\'s own block kinds bring their words here, each on its block.</td></tr>'});
var TXDEF=JSON.parse(TX0);
on("txPanel.html",function(h){var d=document.createElement("div");d.innerHTML=h;
  d.querySelectorAll("tbody tr").forEach(function(r){var inp=r.querySelector("[data-tx]");if(!inp)return;var k=inp.dataset.tx.split("|")[0];
    if(JSON.stringify(S.tx[k])!==JSON.stringify(TXDEF[k])){var c=r.lastElementChild;if(c)c.insertAdjacentHTML("afterbegin",'<button class="del" type="button" data-txreset="'+k+'" title="Back to the framework\'s wording">&#8634;</button>')}});return d.innerHTML});
ed.addEventListener("click",function(e){var t=e.target;
  var lm=t.closest("[data-lmove]");if(lm){e.stopPropagation();var q=lm.dataset.lmove.split("|"),i=+q[0],j=i+(+q[1]),a=S.langs.list;if(j<0||j>=a.length)return;var x=a[i];a[i]=a[j];a[j]=x;renderAll(true);return}
  var tr=t.closest("[data-txreset]");if(tr){e.stopPropagation();var k=tr.dataset.txreset;S.tx[k]=JSON.parse(JSON.stringify(TXDEF[k]));renderAll(true);return}},true);
var FOCUS9={w1:["card",".chrome i:last-child","full"]};Object.keys(FOCUS9).forEach(function(k){if(!FOCUS[k])FOCUS[k]=FOCUS9[k]});
