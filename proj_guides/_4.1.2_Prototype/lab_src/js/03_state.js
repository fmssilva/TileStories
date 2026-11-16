/* ---------- UI state ---------- */
const ui = { appStep: "main", appPoi: 0, tab: "global", screen: "wall", style: 0, poi: 0, shape: 0, knot: -1, open: {}, help: null, tool: "edit", life: { pi: 0, ci: 0 }, keys: false };
let playing = false, playT0 = 0, poiStart = {}, focus = null, focusT0 = 0, unfocusT0 = -1e9, styleT0 = performance.now() / 1000;
let drawing = false, rectStart = null, cursor = null, drag = null, hover = null, shiftDown = false;
let lifeRun = {}, lastAmbientEnd = -1e9, lastGuest = -1e9;
const past = [], future = [];
const snap = () => { past.push(JSON.stringify(D)); if (past.length > 300) past.shift(); future.length = 0; syncUndo(); };
function undo() { if (!past.length) return; future.push(JSON.stringify(D)); D = JSON.parse(past.pop()); drawing = false; afterData(); }
function redo() { if (!future.length) return; past.push(JSON.stringify(D)); D = JSON.parse(future.pop()); drawing = false; afterData(); }
function afterData() { if (typeof installViews === "function") installViews(); fixSel(); renderEditor(); syncUndo(); }

const styleByKey = k => D.styles.find(s => s.key === k);
const levelOf = p => D.levels.find(l => l.key === p.level);
const levelPresence = l => !l ? "point" : l.layers.region && l.layers.point ? "both" : l.layers.region ? "region" : "point";
const presenceOf = p => p.world.presence_override || levelPresence(levelOf(p));
const styleKeyFor = (p, S) => S.style || p.world.style_override || levelOf(p)?.region_style || D.region_default || "";
const styleFor = (p, S) => styleByKey(styleKeyFor(p, S));
const catColor = p => CATS[p.category]?.color || "#FFFFFF";
const cornerOf = (S, i) => { const n = S.knots.length; if (S.type === "path" && (i === 0 || i === n - 1)) return "sharp"; return S.knots[i].corner || S.default_corner; };
const usesOfStyle = key => D.levels.filter(l => l.region_style === key).length + D.pois.reduce((a, p) => a + (p.world.style_override === key ? 1 : 0) + p.world.shapes.filter(s => s.style === key).length, 0);
const shapeByKey = (p, k) => p?.world.shapes.find(s => s.key === k);
const effPlacement = L => L.offset_cm > 0 ? "outside" : "edge";
/* a quiet style draws nothing at rest (line opacity 0, no fill, no marks): the shape still anchors World Content and taps */
const isQuiet = st => !!st && st.line.opacity < 0.05 && st.fill.mode === "none" && st.vertices.show_at === "none";

