function fixSel() {
  ui.style = clamp(ui.style, 0, Math.max(0, D.styles.length - 1));
  ui.poi = clamp(ui.poi, 0, D.pois.length - 1);
  const W = D.pois[ui.poi].world; ui.shape = clamp(ui.shape, 0, Math.max(0, W.shapes.length - 1));
  if (!W.shapes[ui.shape] || ui.knot >= W.shapes[ui.shape].knots.length) ui.knot = -1;
  const list = ui.life.pi < 0 ? D.life.wall : D.pois[ui.life.pi]?.world.content || [];
  if (!list[ui.life.ci]) ui.life.ci = Math.max(0, list.length - 1);
}
const edb = $("edb");
function renderEditor() {
  fixSel(); uid = 0; if (typeof renderProfileBar === "function") renderProfileBar();
  const st = edb.scrollTop, act = document.activeElement?.id;
  edb.replaceChildren(...(ui.tab === "global" ? globalTab() : ui.tab === "lab" ? decisionsTab() : ui.tab === "exp" ? experiencesTab() : ui.tab === "app" ? appTab() : specificTab()).filter(Boolean));
  edb.className = "ed-body" + (ui.tab === "specific" ? " specific" : ui.tab === "lab" ? " lab" : ui.tab === "exp" ? " exp" : ui.tab === "app" ? " appx" : "");
  stickHeads();
  edb.scrollTop = st; if (act && document.getElementById(act) && edb.contains(document.getElementById(act))) document.getElementById(act).focus({ preventScroll: true });
  for (const [id, t, c] of [["tab-global", "global", "#8FB2E8"], ["tab-specific", "specific", "#7FD68F"], ["tab-lab", "lab", "#C9A6F2"], ["tab-exp", "exp", "#E3A6C8"], ["tab-app", "app", "#F2C27A"]]) { $(id).setAttribute("aria-selected", String(ui.tab === t)); $(id).style.outline = ui.tab === t ? `2px solid ${c}` : "none"; }
  drawSwatches(); if (typeof drawPointSwatches === "function") drawPointSwatches(); syncTools(); updateHud(); syncUndo();
  if (ui.screen === "app") renderPhone();
}
/* section and subsection titles stay at the top while you scroll: each one sticks below the titles that contain it */
function stickHeads() {
  const g = [...edb.children].find(e => e.classList.contains("grp")), gh = g ? g.offsetHeight + 2 : 0;
  edb.querySelectorAll(":scope > .grp").forEach(e => e.classList.add("stk"));
  edb.querySelectorAll(".fh, :scope > .poih").forEach(hd => {
    let top = gh, depth = 0;
    for (let el = hd.parentElement.parentElement; el && el !== edb; el = el.parentElement) {
      if (el.classList.contains("fold")) { top += el.firstElementChild.offsetHeight; depth++; }
      else if (el.classList.contains("fb") && el.previousElementSibling?.classList.contains("poih")) { top += el.previousElementSibling.offsetHeight; depth++; }
    }
    if (hd.classList.contains("poih")) { top = gh; depth = 0; }
    hd.style.top = top + "px"; hd.style.zIndex = 40 - depth; hd.classList.add("stk");
  });
}
/* any click or focus inside the editor opens the screen that shows it */
const toScreen = e => { const el = e.target.closest("[data-screen]"); if (el && edb.contains(el)) { setScreen(el.dataset.screen); const st = e.target.closest("[data-step]"); if (st && el.dataset.screen === "app" && ui.appStep !== st.dataset.step) { ui.appStep = st.dataset.step; renderPhone(); } } const d = e.target.closest(".hasdemo"); if (d && edb.contains(d) && !e.target.closest(".hasdemo > .fh")) runDemo(d._demo); };
edb.addEventListener("pointerdown", toScreen, true); edb.addEventListener("focusin", toScreen);
function scrollToPoi() { const el = edb.querySelectorAll(".poih")[ui.poi]; if (el) edb.scrollTop = el.offsetTop - edb.offsetTop - 4; }
$("tab-global").onclick = () => { if (drawing) finishDrawing(false); ui.tab = "global"; renderEditor(); };
$("tab-specific").onclick = () => { ui.tab = "specific"; ui.open["poi_" + ui.poi] = true; if (ui.screen === "style" || ui.screen === "precision") setScreen("wall"); renderEditor(); };
$("tab-lab").onclick = () => { if (drawing) finishDrawing(false); ui.tab = "lab"; renderEditor(); };
$("tab-app").onclick = () => { if (drawing) finishDrawing(false); ui.tab = "app"; setScreen("app"); renderEditor(); };
$("tab-exp").onclick = () => { if (drawing) finishDrawing(false); ui.tab = "exp"; renderEditor(); };

function drawSwatches() {
  edb.querySelectorAll("canvas.sw").forEach(c => {
    const st = styleByKey(c.dataset.style); if (!st) return;
    const x = c.getContext("2d"); x.setTransform(1, 0, 0, 1, 0, 0); x.clearRect(0, 0, c.width, c.height); x.fillStyle = "#3A3F47"; x.fillRect(0, 0, c.width, c.height);
    const sc = 0.55; x.setTransform(2 * sc, 0, 0, 2 * sc, 0, 0);
    const fake = { category: "category_1" }, S = st.vertices.show_at === "all" && st.fill.mode === "none" && st.line.pattern === "solid" && st.motion.idle.preset === "knot_pulse"
      ? { type: "points", default_corner: "sharp", knots: [K(30, 15), K(55, 15), K(80, 15)] }
      : { type: "polygon", default_corner: "sharp", knots: [K(16, 8), K(92, 8), K(92, 23), K(16, 23)] };
    if (isQuiet(st)) { x.setLineDash([4, 3]); x.strokeStyle = "rgba(255,233,168,.6)"; x.lineWidth = 1.2; x.strokeRect(16, 8, 76, 15); return; }
    drawRegion({ ctx: x, sc: 2 * sc / 2, dpr: 2 }, fake, S, st, { T: Infinity, t: 0, idle: false, f: 0 });
  });
}

