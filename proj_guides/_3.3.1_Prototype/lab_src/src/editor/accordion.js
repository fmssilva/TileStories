/* The Editor's accordion and its sticky headers (guidance round 20): one Editor feature any tab switches on (ACC_TABS.<tab>; the Guide tab
   does, editor/guide-tab.js). Such a tab opens as a list of its sections, each collapsed on one line (its number, name, a short sub-line
   and a quiet count of its parts), with Collapse all and One section open at a time at its top. A section opens alone (the others stay as
   they are) and its foldouts start collapsed; the open state is S.op as everywhere. While scrolling, the open section's header stays
   pinned at the top of the Editor and its open foldout's header just under it (two levels; a third does not pin); a tap on a pinned header
   scrolls back to its start (it does not collapse it). A link into the tab lands under the pinned headers (their scroll-margin).
   The developer (2026-11-13): "let's use some sort of accordion style. We start with the sections collapsed so the developer sees clearly
   all we have, and then he can open it, and we keep a sort of sticky headers of the sections and sub sections so they are still visible
   at the top when we scroll down". Unity: the POI Editor's tab as UI Toolkit Foldouts (value persisted per id, EditorPrefs), its toolbar
   a Button and a Toggle; the pinned headers: two header elements (position: absolute) at the top of the tab's ScrollView, filled with
   the section's and the foldout's names and moved from ScrollView.verticalScroller.valueChanged (the foldout whose header has scrolled
   above the view's top is the one pinned); a click on one sets scrollOffset to its foldout's top */
/* round 21 (21.9, the developer, 2026-11-14): the Detail Card and Global tabs are accordions too; the Guide tab switches itself on
   (editor/guide-tab.js). On these two the bar (Collapse all, One section open at a time) is put at the tab's top after its draw */
var ACC_TABS={card:true,global:true};
S.ui.accOne=false;
function accOn(tab){return!!ACC_TABS[tab===undefined?S.tab:tab]}
/* the tab's sections start collapsed: level 1 (the default) shows them as the accordion's list, level 2 opens them (and their foldouts),
   the self-test's walk (level 9) every one; a section the developer opened or closed keeps it (S.op, before this answer) */
on("isOpen.answer",function(id,depth){if(depth===1&&accOn())return S.ui.level>=2});
/* the tab's top: Collapse all, and One section open at a time (an Editor preference, Off by default) */
function accBar(){return'<div class="accbar" role="toolbar" aria-label="Sections"><button class="ubtn sm" type="button" data-acc="collapse">Collapse all</button><span class="sp"></span>'+
  '<span class="acclab">One section open at a time</span>'+seg("ui.accOne",["false","true"],["Off","On"],String(!!S.ui.accOne))+
  help("An Editor preference, not saved to the wall. Off: each section opens and closes on its own. On: opening a section closes the one that was open. Collapse all closes every section and foldout of this tab. While you scroll, the open section's name stays at the top, its open foldout's just under it; a tap on either scrolls back to its start.")+'</div>'}
/* each section's quiet count, on its line: its foldouts ("6 parts"), or its rows when it has none */
function accCount(t){[].forEach.call(t.querySelectorAll(":scope > details[data-fo]"),function(d){var sm=d.querySelector(":scope > summary"),sp=sm&&sm.querySelector(":scope > .sp");if(!sp||sm.querySelector(":scope > .accn"))return;
  var n=d.querySelectorAll(":scope > .inner > details[data-fo]").length,r=d.querySelectorAll(":scope > .inner > .row").length;if(!n&&!r)return;var w=n?n+(n===1?" part":" parts"):r+(r===1?" row":" rows");
  sp.insertAdjacentHTML("afterend",'<span class="accn">'+w+'</span>')})}
/* a foldout's level in the tab: 1 a section, 2 a foldout of it (pinned too), 3 and deeper (not pinned) */
function accLevel(d){var n=1,p=d.parentElement&&d.parentElement.closest("details[data-fo]");while(p&&ed.contains(p)){n++;p=p.parentElement&&p.parentElement.closest("details[data-fo]")}return n}
/* a header is pinned when its foldout's top has scrolled above it */
function accPinned(d){var s=d.querySelector(":scope > summary");return!!s&&d.open&&s.getBoundingClientRect().top-d.getBoundingClientRect().top>1}
/* back to a foldout's start: its top just under the headers pinned above it (its scroll-margin) */
function accBack(d){d.scrollIntoView({block:"start",behavior:"smooth"})}
on("renderEditor.after",function(){ed.classList.toggle("acc",accOn());if(!accOn())return;if(!ed.querySelector(":scope > .accbar"))ed.insertAdjacentHTML("afterbegin",accBar());accCount(ed)});
on("setOpt.answer",function(key,v){if(key==="ui.accOne"){S.ui.accOne=v==="true";renderEditor();return true}});
/* One section open at a time: a section opened by the developer closes the one that was open (never a draw's own toggle) */
ed.addEventListener("toggle",function(e){var d=e.target;if(!accOn()||!S.ui.accOne||!d.open||!d.dataset||!d.dataset.fo||d.parentElement!==ed||edQuiet(d))return;
  [].forEach.call(ed.querySelectorAll(":scope > details[data-fo][open]"),function(s){if(s!==d){s.open=false;S.op[s.dataset.fo]=false}})},true);
ed.addEventListener("click",function(e){if(!accOn())return;var t=e.target;
  if(t.closest("[data-acc]")){e.stopPropagation();ed.querySelectorAll("details[data-fo]").forEach(function(d){S.op[d.dataset.fo]=false});renderEditor();var top=ed.closest(".ed");if(top&&top.getBoundingClientRect().top<0)top.scrollIntoView({block:"start"});return}
  /* a tap on a pinned header (sections and their foldouts): back to its start, still open; a control inside the header keeps its own tap */
  var sm=t.closest("summary"),d=sm&&sm.parentElement;if(!d||!d.matches("details[data-fo]")||t.closest("button,input,select,textarea,a,label,[data-set]"))return;
  if(accLevel(d)<=2&&accPinned(d)){e.preventDefault();e.stopPropagation();accBack(d)}},true);
