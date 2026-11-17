/* Undo / Redo on D, the author's data (STATE_MAP.md): a step is a JSON copy of D taken before an Editor change (D is about 130 KB,
   a copy well under 1 ms, so the whole of D is kept, not the changed path), at most UNDO_MAX steps back. P (the visitor's) and ui
   (what is happening) never enter the history. A step put back is written into D in place (undoPut): every object keeps its
   identity, so what points into D (the open point's tour, which is also its written tour; POINTS) keeps pointing at the same data.
   The Editor's kit takes the steps (editor/shell.js: the Editor's one write point, its events) and draws the buttons.
   Unity: Undo.RecordObject on the edited asset before each change (the POI Editor's own Undo / Redo / Save).
   configChanged ("edit", "undo" or "redo") is emitted here, once for each change of D (Unity: the config asset's OnValidate / the
   Editor's change event), so a part that follows the author's config listens to one event. */
var UNDO_MAX=300,undoPast=[],undoFuture=[];
/* a step: D as it was before a change (a JSON copy), or nothing when the change left D as it was */
function undoSnap(before){if(before===JSON.stringify(D))return false;undoPast.push(before);if(undoPast.length>UNDO_MAX)undoPast.shift();undoFuture.length=0;emit("undo.changed");emit("configChanged","edit");return true}
/* undo.applied (kind, was): D put back; was is D just before, as JSON, for a module whose Editor change also set something outside D */
function undo(){if(!undoPast.length)return false;var was=JSON.stringify(D);undoFuture.push(was);undoPut(D,JSON.parse(undoPast.pop()));emit("undo.changed");emit("undo.applied","undo",was);emit("configChanged","undo");return true}
function redo(){if(!undoFuture.length)return false;var was=JSON.stringify(D);undoPast.push(was);undoPut(D,JSON.parse(undoFuture.pop()));emit("undo.changed");emit("undo.applied","redo",was);emit("configChanged","redo");return true}
function undoCan(){return{undo:undoPast.length,redo:undoFuture.length}}
/* src written into target in place: the same objects and arrays where both sides have one, new ones only where the shape changed */
function undoPut(target,src){var isObj=function(x){return x!==null&&typeof x==="object"};
  if(Array.isArray(target)){target.length=src.length;for(var i=0;i<src.length;i++){if(isObj(target[i])&&isObj(src[i])&&Array.isArray(target[i])===Array.isArray(src[i]))undoPut(target[i],src[i]);else target[i]=src[i]}return}
  Object.keys(target).forEach(function(k){if(!(k in src))delete target[k]});
  Object.keys(src).forEach(function(k){if(isObj(target[k])&&isObj(src[k])&&Array.isArray(target[k])===Array.isArray(src[k]))undoPut(target[k],src[k]);else target[k]=src[k]})}
