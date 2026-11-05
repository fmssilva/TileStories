/* Extension points: a shared function calls its point by name; a feature joins it with on(). Joining order = module order. */
function extPoint(n){var r=extPoint.r||(extPoint.r={});return r[n]||(r[n]=[])}
function on(n,fn){extPoint(n).push(fn)}
function emit(n,a,b,c){var l=extPoint(n);for(var i=0;i<l.length;i++)l[i](a,b,c)}
function emitLatestFirst(n,a,b,c){var l=extPoint(n);for(var i=l.length-1;i>=0;i--)l[i](a,b,c)}
function pipe(n,v,a,b){var l=extPoint(n);for(var i=0;i<l.length;i++)v=l[i](v,a,b);return v}
function firstAnswer(n,a,b,c){return answerBelow(n,null,a,b,c)}
/* the same from the joiner older than j: a joiner that cannot answer after all hands the call on to the ones before it */
function answerBelow(n,j,a,b,c){var l=extPoint(n),i=j?l.indexOf(j):l.length;for(i--;i>=0;i--){var r=l[i](a,b,c);if(r!==undefined)return r}}
