/* ---------- App: the app's screens on a phone (start, onboarding, main AR screen, a tapped POI, the Explore menu) ---------- */
let phoneEls = null;

/* where the label sits beside a marker on the phone: the style's side, past the marker's own size */
function lblPos(P, flip, up) {   // up: a "below" label near the bottom edge goes above the marker;   // measured from the marker's centre: past its radius, its status ring and a 6 pt gap, so a label never covers its own marker
  const u = dev.U / 3.9, R = (P.size_pt / 2 + (P.status === "ring" ? Math.max(P.size_pt * 0.09, P.status_gap_pt) + 2 : 0) + 6) * u, fs = `font-size:${mkLabelPt(P) * u}px`;   // v19 R6: follows the text size
  return P.label_pos === "below" ? (up ? `left:50%;bottom:calc(50% + ${R}px);top:auto;transform:translateX(-50%);${fs}` : `left:50%;top:calc(50% + ${R}px);transform:translateX(-50%);${fs}`) : `${flip ? `right:calc(50% + ${R}px);left:auto` : `left:calc(50% + ${R}px)`};top:50%;transform:translateY(-50%);${fs}`; }
function tapPoiOnPhone(i) {
  if (justDragged) return;
  ui.coachDone = true; ui.appPoi = i; seen.add(D.pois[i].id); ui.appStep = "tapped"; ui.previewLayout = null; ui.compPreview = null; tapState.view = null; ui.tapAnchor = null;
  paintAR(false);   // the camera never moves on a tap: on the device we cannot move the visitor's phone
}
function renderPhone() {
  if (ui.screen !== "app" || !dev.scr) return;
  renderSide(); buildPhone(ui.appStep);
}
function buildPhone(step) {
  const A = D.app, kids = forKids(), go = st => () => { ui.appStep = st; tapState.view = null; renderPhone(); };
  dev.scr.classList.remove("kids"); dev.scr.classList.add("premium"); for (const k of Object.keys(D.looks)) dev.scr.classList.toggle("look-" + k, activeLook() === k); 
  dev.glue.replaceChildren(); phoneEls = null;
  applyGlass(dev.scr);
  if (step === "start") { dev.layer.replaceChildren(startScreen(go(A.onboarding.on ? "onboarding" : "main"))); return; }   // built from the Design system
  if (step === "onboarding" && !A.onboarding.on) { ui.appStep = step = "main"; ui.toastOnb = performance.now(); }   // onboarding off: Start opens the camera
  if (step === "onboarding") { dev.layer.replaceChildren(onboardingScreen(go("main"))); return; }
  phoneEls = { ph: dev.scr, world: dev.world, layer: dev.layer };
  paintAR(false, step);
}
/* everything that depends on the camera: the wall image, markers, edge arrows, chrome, the tap UI. still = no entry animations (the camera is moving) */
function paintAR(still, stepArg) {
  const E = phoneEls; if (!E || ui.screen !== "app") return;
  const { ph, layer } = E, A = D.app, M = A.main, T = A.tap, kids = forKids();
  let step = stepArg || ui.appStep;
  const go = st => () => { if (justDragged) return; ui.appStep = st; tapState.view = null; renderPhone(); };
  applyCam();
  const sel = step === "tapped" ? D.pois[ui.appPoi] : null, L = sel ? tapLayoutOf(sel) : null;   // the preview, else the POI's own style, components and overrides resolved
  let sq = sel ? camProject(sel.pos.x, sel.pos.y) : null;
  const viewOpen = sel && tapState.view?.p === sel;   // an open action view (card, 360, then & now, 3D) fills the screen: moving the phone does not close it
  if (sel && !viewOpen && !onScreen(sq) && T.offscreen === "close" && !ui.camMoving) { ui.appStep = step = "main"; }
  const out = [];
  D.pois.forEach((p, i) => {
    const q = camProject(p.pos.x, p.pos.y), lv = levelOf(p), isSel = step === "tapped" && p === sel, PS = pointStyleOf(p);
    if (!onScreen(q) || (!shownByTime(p) && !isSel)) return;   // Time › Quick shows only the top levels
    out.push(h("div", { class: "mk" + (isSel && step === "tapped" ? " sel" : "") + (isInterest(p) ? " fav" : ""), "data-i": i, style: `left:${q.x}%;top:${q.y}%;opacity:${step === "tapped" && !isSel ? .7 : step === "main" && !lensOk(p) ? .25 : 1}`, onclick: () => tapPoiOnPhone(i) },
      markerCanvas(p, PS, isSel && step === "tapped" ? 1 : 0), lv?.layers.label !== false && PS.label !== "off" && (prefs().labels !== false || isSel) && (PS.label !== "focus" || isSel) && !(isSel && L.tag !== "none") ? h("div", { class: `lbl ls-${PS.label_style} lf-${PS.label_face}`, style: lblPos(PS, q.x > 62, q.y > 86) }, p.name) : null));
  });
  if ((step === "main" && prefs().pointers !== false && !(D.app.main.hints.tracking && wallAway())) || (step === "tapped" && D.app.pointing.while_tapped)) out.push(...pointers(step === "tapped" ? sel : null));   // while the wall is out of view, the hint and the mini-map guide instead
  applyGlass(dev.scr);   // the wall labels and pointers read the theme too
  dev.scr.classList.remove("coaching");
  if (step === "main") out.push(mainChrome());
  if (step === "main" && ui.toastOnb && performance.now() - ui.toastOnb < 2600) { const t = h("div", { class: "tapc" + audCls(), style: `--u:${dev.U}px` }); applyGlass(t); t.append(h("div", { class: "ktoast g onbt" }, h("span", { class: "mi" }, svg("info")), "Onboarding is off: Start opens the camera")); out.push(t); }   // App › 3 Main AR screen (31_main.js)

  if (step === "explore") out.push(exploreScreen(go("main")));   // built from the Design system's sheet and list rows
  if (step === "settings") out.push(settingsScreen(go("main")));
  if (step === "you") out.push(youScreen(go("main")));
  if (step === "album") out.push(KF.album(go("explore")));   // built from the Design system's sheet and list rows
  if (step === "main" && ui.demoTap && performance.now() - ui.demoTap < 900) { const p = D.pois[ui.appPoi], q = camProject(p.pos.x, p.pos.y);
    out.push(h("div", { class: "finger", style: `left:${q.x}%;top:${q.y}%` }), h("div", { class: "tapfx", style: `left:${q.x}%;top:${q.y}%` })); }
  if (step === "tapped" && sel && ui.demoTapFx && performance.now() - ui.demoTapFx < 500) out.push(h("div", { class: "tapfx", style: `left:${sq.x}%;top:${sq.y}%` }));   // the demo's tap ripple runs on as the tap UI opens
  layer.replaceChildren(...out);
  dev.glue.replaceChildren();
  if (step === "tapped" && ui.compPreview?.type === "kit") { layer.replaceChildren(uiKit()); ui.tapInfo = "design system · UI kit"; return; }
  if (step === "tapped" && ui.compPreview?.type === "buttons") { const g = layer.appendChild(buttonGallery()); fitGallery(g); ui.tapInfo = "base elements · the buttons"; return; }
  if (step === "tapped" && sel && !ui.camMoving) paintTap(layer, sel, L, { still, rerender: () => paintAR(false), preview: !!(ui.previewLayout || ui.compPreview) });
  fitLabels(); hideCoveredLabels(); clearPointers();
}
/* v19 F-tap: a wall label stays on the screen by measurement (the style's side, else the other side, else slid inside), not by the marker's x alone
   (text L made a right-side label run off the edge; neither side fits: below the marker). Unity: the same rect test on the label layer after layout */
function fitLabels() {
  const ph = dev.scr; if (!ph) return; const pr = ph.getBoundingClientRect(), m = pr.width * 0.03, kx = ph.offsetWidth / (pr.width || 1);
  ph.querySelectorAll(".mk .lbl").forEach(l => {
    const mk = l.parentElement, p = D.pois[+mk.dataset.i]; if (!p) return;
    const PS = pointStyleOf(p), q = camProject(p.pos.x, p.pos.y), up = q.y > 86, out = r => r.left < pr.left + m || r.right > pr.right - m;
    let r = l.getBoundingClientRect(); if (!out(r)) return;
    if (PS.label_pos !== "below") {
      const flip = r.right > pr.right - m; l.setAttribute("style", lblPos(PS, flip, up)); r = l.getBoundingClientRect();
      if (!out(r)) return;
      l.setAttribute("style", lblPos({ ...PS, label_pos: "below" }, false, up)); r = l.getBoundingClientRect();   // neither side has room: under (or over) the marker, so a slide never covers it
      if (!out(r)) return;
    }
    const dx = r.left < pr.left + m ? pr.left + m - r.left : pr.right - m - r.right;
    l.style.translate = `${dx * kx}px 0`;   // still too wide: slide it inside
  });
}
/* a wall label never sits under the status bar, the search bar or a sheet / card: it hides there (Unity: the same rect test on the label layer) */
function hideCoveredLabels() {
  const ph = dev.scr; if (!ph) return; const rr = s => [...ph.querySelectorAll(s)].map(e => e.getBoundingClientRect()).filter(r => r.width > 0);
  const top = Math.max(0, ...rr(".sb").map(r => r.bottom), ...rr(".msearch, .mtop").map(r => r.bottom)) + 4, covers = rr(".s-card, .ds-sheet, .t-pan .tx, .t-pan .im, .cel-card, .mbar.rail, .mc > *, .mmap, .tapc > .t-imm, .tapc > .t-card, .tapc > .t-chip, .tapc > .t-label, .tapc > .a-cap, .tapc > .a-dock, .tapc > .a-rad, .tapc > .galp"), bot = Math.min(1e9, ...rr(".mbot > *").map(r => r.top)) - 4;
  ph.querySelectorAll(".mk .lbl").forEach(l => { const r = l.getBoundingClientRect(), hit = r.top < top || r.bottom > bot || covers.some(c => r.left < c.right + 8 && r.right > c.left - 8 && r.top < c.bottom && r.bottom > c.top - 8); l.classList.toggle("covered", hit); });
}
/* a pointer never sits under the search bar, the lenses, the bottom bar / rail, a corner button or the mini-map: it slides out of the way
   (a bar: below it in the top half, above it in the bottom half; a rail: sideways, inwards). Unity: the same rect test after layout */
function clearPointers() {
  const ph = dev.scr; if (!ph) return;
  const pr = ph.getBoundingClientRect(), blocks = [...ph.querySelectorAll(".mtop > *, .mbot > *, .mbar, .mc > *, .mmap")].map(e => e.getBoundingClientRect()).filter(r => r.width > 0);
  ph.querySelectorAll(".edge").forEach(el => {
    for (let n = 0; n < 3; n++) {
      const r = el.getBoundingClientRect(), b = blocks.find(c => r.left < c.right + 6 && r.right > c.left - 6 && r.top < c.bottom + 6 && r.bottom > c.top - 6); if (!b) break;
      if (b.height > b.width) el.style.left = `calc(${el.style.left} + ${r.left + r.width / 2 < b.left + b.width / 2 ? b.left - 6 - r.right : b.right + 6 - r.left}px)`;
      else el.style.top = `calc(${el.style.top} + ${r.top + r.height / 2 - pr.top > pr.height / 2 ? b.top - 6 - r.bottom : b.bottom + 6 - r.top}px)`;
    }
  });
  // v19 F-tap: nor under the tap UI (a card, a sheet, a panel, the buttons): it takes the nearest free spot (slides along its edge, or steps past the panel), else it hides
  const ui2 = [...ph.querySelectorAll(".tapc > .s-card, .tapc > .t-pan .tx, .tapc > .t-imm, .tapc > .t-card, .tapc > .t-chip, .tapc > .t-label, .tapc > .a-cap, .tapc > .a-dock, .tapc > .a-rad, .tapc > .galp, .ds-sheet")].map(e => e.getBoundingClientRect()).filter(r => r.width > 0)
    .map(r => ({ left: r.left, right: r.right, top: r.top - 3 * dev.U * pr.width / dev.SW, bottom: r.bottom }));   // the panels rise 3 u into place: measure where they end
  if (!ui2.length) return stackPointers(ph, blocks);
  const kx = ph.offsetWidth / (pr.width || 1);
  const sb = Math.max(pr.top + pr.height * 0.06, ...[...ph.querySelectorAll(".sb")].map(e => e.getBoundingClientRect().bottom)), all = [...blocks, ...ui2];
  const hits = (x, y, w, hh) => all.some(c => x < c.right + 6 && x + w > c.left - 6 && y < c.bottom + 6 && y + hh > c.top - 6);
  ph.querySelectorAll(".edge").forEach(el => {
    const r = el.getBoundingClientRect(); if (!hits(r.left, r.top, r.width, r.height)) return;
    const xs = [r.left], ys = [r.top];
    for (const c of ui2) { xs.push(c.left - 8 - r.width, c.right + 8); ys.push(c.top - 8 - r.height, c.bottom + 8); }
    let best = null, bd = 1e9;
    for (const x of xs) for (const y of ys) {
      if (x < pr.left + 4 || x + r.width > pr.right - 4 || y < sb + 2 || y + r.height > pr.bottom - pr.height * 0.03 || hits(x, y, r.width, r.height)) continue;
      const d = Math.hypot(x - r.left, y - r.top); if (d < bd) { bd = d; best = [x, y]; }
    }
    if (!best || bd > pr.height * 0.3) { el.style.display = "none"; return; }
    el.style.left = `calc(${el.style.left} + ${(best[0] - r.left) * kx}px)`; el.style.top = `calc(${el.style.top} + ${(best[1] - r.top) * kx}px)`;
  });
  stackPointers(ph, all);
}
/* v20 F: pointers never overlap each other. In rank order (the selected POI's pin first, then the ranked list), each pointer keeps its spot if it is free,
   else slides along its own edge to the nearest free spot: at least 6 pt from any other pointer, inside the screen (status bar to home bar) and clear of
   the UI blocks. An edge with no room left hides the lower-ranked pointer. Unity: the same 1-D search per pointer on the pointer layer after layout */
function stackPointers(ph, blocks) {
  const pr = ph.getBoundingClientRect(), kx = ph.offsetWidth / (pr.width || 1), gap = 7 * dev.U / 3.9 / kx, bm = 6;
  const els = [...ph.querySelectorAll(".tapc > .pin"), ...ph.querySelectorAll(".edge")].filter(e => e.style.display !== "none" && e.getBoundingClientRect().width > 0);
  if (!els.length) return;
  const sb = Math.max(pr.top + pr.height * 0.06, ...[...ph.querySelectorAll(".sb")].map(e => e.getBoundingClientRect().bottom)) + 2;
  const box = { l: pr.left + 4, r: pr.right - 4, t: sb, b: pr.bottom - pr.height * 0.03 };
  const obst = blocks.map(c => ({ left: c.left - bm, right: c.right + bm, top: c.top - bm, bottom: c.bottom + bm }));
  const hit = (x, y, w, hh) => obst.some(c => x < c.right && x + w > c.left && y < c.bottom && y + hh > c.top);
  for (const el of els) {
    const r = el.getBoundingClientRect(), w = r.width, hh = r.height, cx = (r.left + r.right) / 2 - pr.left, cy = (r.top + r.bottom) / 2 - pr.top;
    const v = cx < pr.width * 0.3 || cx > pr.width * 0.7;   // on the left or right edge it slides up and down; on the top or bottom edge, sideways
    let x = clamp(r.left, box.l, box.r - w), y = clamp(r.top, box.t, box.b - hh);
    if (hit(x, y, w, hh)) {
      const c0 = v ? y : x, cs = [c0];
      for (const c of obst) { if (v ? (x < c.right && x + w > c.left) : (y < c.bottom && y + hh > c.top)) cs.push(v ? c.bottom : c.right, (v ? c.top : c.left) - (v ? hh : w)); }
      const lo = v ? box.t : box.l, hi = v ? box.b - hh : box.r - w;
      const ok = cs.filter(p => p >= lo - 0.5 && p <= hi + 0.5 && !(v ? hit(x, p, w, hh) : hit(p, y, w, hh))).sort((a, b) => Math.abs(a - c0) - Math.abs(b - c0));
      if (!ok.length) { el.style.display = "none"; continue; }
      if (v) y = ok[0]; else x = ok[0];
    }
    const dx = x - r.left, dy = y - r.top;
    if (Math.abs(dx) > 0.3) el.style.left = `calc(${el.style.left} + ${dx * kx}px)`;
    if (Math.abs(dy) > 0.3) el.style.top = `calc(${el.style.top} + ${dy * kx}px)`;
    obst.push({ left: x - gap, right: x + w + gap, top: y - gap, bottom: y + hh + gap });
  }
}
/* Base elements preview: every button in its look, in priority order, with when it shows */
function buttonGallery() {
  const B = btnLook(), kids = forKids();
  const tl = h("div", { class: "tapc gal" + audCls() + (B.shape === "rounded" ? " b-round" : "") + (B.surface === "solid" ? " b-solid" : ""), style: `--u:${dev.U}px;--bk:${B.size}` });
  applyGlass(tl);
  const live = D.tap_actions.filter(a => a.show !== "off").slice(0, 4);
  tl.append(h("div", { class: "galcap", style: "top:calc(100% - 50 * var(--u))" }, "On the wall"),
    h("div", { class: "galw", style: "top:calc(100% - 44 * var(--u))" }, ...live.map(a => h("div", { class: "gi" }, h("div", { class: "a-rad st" }, h("div", { class: "c g" }, svg(a.key))), h("span", { class: "gl" }, a.label)))));
  tl.append(h("div", { class: "galp g rise" }, h("div", { class: "galh" }, h("b", {}, "Buttons"), h("small", {}, "in priority order · the first ones show, the rest go under more")),
    h("div", { class: "galg" }, ...D.tap_actions.map((a, i) => h("div", { class: "gi" + (a.show === "off" ? " off" : ""), style: `animation-delay:${i * .03}s` },
      h("div", { class: "a-rad st" }, h("div", { class: "c g" }, svg(a.key))), h("span", { class: "gl" }, a.label),
      h("small", {}, a.show === "off" ? "off" : a.show === "always" ? "always" : "if " + (NEEDS[a.key]?.[1] || "content")))))));
  return tl;
}
/* v19 F-tap: the "On the wall" caption and row never sit under the panel (text L makes the panel taller): they move below it; with no room
   below, the panel moves up to the status bar; still no room, the caption goes (the row stays). Unity: the same measure after layout */
function fitGallery(tl) {
  const p = tl.querySelector(".galp"), cap = tl.querySelector(".galcap"), row = tl.querySelector(".galw"); if (!p || !cap || !row || !tl.isConnected) return;
  tl.classList.add("measuring");
  const tr = tl.getBoundingClientRect(), k = tl.offsetHeight / (tr.height || 1), bot = tr.bottom - tr.height * 0.025, gap = 10 / k;
  const pb = () => p.getBoundingClientRect().bottom, c = cap.getBoundingClientRect(), w = row.getBoundingClientRect(), need = (w.bottom - c.top);
  if (c.top >= pb() + gap) { tl.classList.remove("measuring"); return; }
  const sb = Math.max(tr.top + tr.height * 0.06, ...[...dev.scr.querySelectorAll(".sb")].map(e => e.getBoundingClientRect().bottom)) + 6, pt = p.getBoundingClientRect().top;
  const over = pb() + gap + need - bot; if (over > 0) p.style.top = (Math.max(sb, pt - over) - tr.top) * k + "px";
  let y = pb() + gap;
  if (y + need > bot) { cap.style.display = "none"; y = Math.min(y, bot - (w.bottom - w.top)); } else { cap.style.top = (y - tr.top) * k + "px"; y += w.top - c.top; }
  row.style.top = (y - tr.top) * k + "px";
  tl.classList.remove("measuring");
}
/* the tap UI of POI sel with layout L: parts on the screen go in `container`, parts on the wall in the glue plane (inside the 3D world) */
function paintTap(container, sel, L, { still = false, rerender, preview = false, id } = {}) {
  const T = D.app.tap, kids = audCls(), P = D.app.pointing;
  let sq = camProject(sel.pos.x, sel.pos.y);
  const viewOpen = tapState.view?.p === sel, on = onScreen(sq);
  // on the wall: the focus is painted on the tiles (0 cm); the tag and the buttons float at the style's distance. The marker dot never moves.
  const W = L.on_wall || {}, focusSel = W.focus ? ".f-ring,.f-pulse" : "", liftSel = [W.tag && ".t-label,.t-chip,.t-card,.t-imm,.t-pan,.irow", W.actions && ".a-rad,.a-cap,.a-dock"].filter(Boolean).join(","), wallSel = [focusSel, liftSel].filter(Boolean).join(",");
  const tl = h("div", { class: "tapc" + kids + (still ? " still" : "") + (PHR > 1 ? " land" : ""), style: `--u:${dev.U}px`, "data-ar": PHR, "data-uw": dev.U / dev.SW * 100 });
  applyGlass(tl);   // the tap layer carries the look's colours and mode (kit frames need them)
  if (id) tl.id = id;
  container.append(tl);
  if (!on && T.offscreen === "pin" && !viewOpen) {
    const e = edgePoint(sq), rot = Math.atan2(sq.y - 50, (sq.x - 50) * PHR) * 180 / Math.PI;
    tl.append(h("div", { class: "pin g hit" + (P.sel_look === "arrow" ? " arrow" : "") + (e.x < 30 ? " at-l" : e.x > 70 ? " at-r" : ""), style: `left:${e.x}%;top:${e.y}%`, title: sel.name, onclick: () => camTo(centreOn(sel)) },
      P.sel_look === "arrow" ? null : h("span", { class: "cd", style: `background:${catColor(sel)}` }), P.sel_look === "arrow" ? null : sel.name, h("span", { class: "chev", style: `rotate:${rot}deg` }, svg("chev"))));
  } else {
    if (!on) sq = { x: clamp(sq.x, 8, 92), y: clamp(sq.y, 14, 86) };
    tapUI(tl, sel, sq.x, sq.y, L, { interactive: true, rerender });
    if (wallSel) tl.querySelectorAll(":scope > *").forEach(e => { if (e.matches(wallSel)) e.remove(); });
  }
  if (!viewOpen) ui.tapInfo = (`${levelOf(sel)?.name} · ${L.name}${preview ? " (preview)" : ""} · ${on ? `on screen ${Math.round(sq.x)}%, ${Math.round(sq.y)}%` : "off screen: " + T.offscreen}${wallSel ? ` · on the wall ${L.depth_cm ?? 0} cm` : ""}`);   // shown in the stage's top line, not on the phone
  const poi = on ? sq : null;
  if (dev.scr.isConnected) fitInside(tl, dev.scr, T.keep_inside, poi); else requestAnimationFrame(() => tl.isConnected && fitInside(tl, dev.scr, T.keep_inside, poi));
  if (wallSel && !viewOpen) paintGlue(sel, L, focusSel, liftSel, still, rerender);
  return tl;
}
/* glued parts: laid out once, as the screen was when the POI was tapped (the anchor), then they stay on the wall,
   lifted off it by the layout's distance, and move with it as the phone moves */
function paintGlue(p, L, focusSel, liftSel, still, rerender) {
  let A = ui.tapAnchor; if (!A || A.poi !== p.id) A = ui.tapAnchor = { ...ui.cam, yaw: 0, pitch: 0, poi: p.id };
  const { imgW, imgH } = camSize(), s = imgW / camSize(A).imgW, k = imgW / IW, gw = dev.SW * s, gh = dev.SH * s;
  const q = camProject(p.pos.x, p.pos.y, { ...A, yaw: 0, pitch: 0 });
  dev.glue.replaceChildren();
  const plane = (depthCm, keep, fit) => {   // one plane parallel to the wall, the size of the screen at the anchor
    const dzA = depthCm * PXCM * camSize(A).imgW / IW, fA = dev.P / (dev.P - dzA);   // lifted parts look bigger: shrink them so, from the anchor, they sit where they were laid out
    const g = h("div", { class: "gplane", style: `left:${A.cx * imgW - gw / 2}px;top:${A.cy * imgH - gh / 2}px;width:${gw}px;height:${gh}px;transform-origin:${q.x}% ${q.y}%;transform:translateZ(${depthCm * PXCM * k}px) rotate(${A.r}deg) scale(${1 / fA})` });
    const gl = h("div", { class: "tapc" + audCls() + (still ? " still" : ""), style: `--u:${dev.U * s}px`, "data-ar": PHR, "data-uw": dev.U / dev.SW * 100 });
    g.append(gl); dev.glue.append(g);
    tapUI(gl, p, q.x, q.y, L, { interactive: true, rerender });
    gl.querySelectorAll(":scope > *").forEach(e => { if (!e.matches(keep)) e.remove(); });
    if (fit) {   // measured flat, as on the screen at the anchor
      const wt = dev.world.style.transform, gt = g.style.transform;
      dev.world.style.transform = "none"; g.style.transform = "none";
      fitInside(gl, g, D.app.tap.keep_inside, q);
      dev.world.style.transform = wt; g.style.transform = gt;
    }
  };
  if (focusSel) plane(0, focusSel, false);
  if (liftSel) plane(L.depth_cm ?? 0, liftSel, true);
  if (liftSel && (L.depth_cm ?? 0) > 0)   // a thin needle from the marker on the wall up to the floating parts (seen from the side)
    dev.glue.append(h("div", { class: "needle", style: `left:${p.pos.x * k}px;top:${p.pos.y * k}px;height:${L.depth_cm * PXCM * k}px` }));
}
/* how far a POI is: the turn of the phone (degrees), or the distance on the wall from the point at the screen centre (metres) */
function howFar(p, q) {
  if (D.app.pointing.how_far === "m") { const r = dev.scr.getBoundingClientRect(), c = unproject(r.left + r.width / 2, r.top + r.height / 2); return (Math.hypot(p.pos.x - c.x, p.pos.y - c.y) / PXCM / 100).toFixed(1) + " m"; }
  const dx = (q.x - 50) / 100 * dev.SW, dy = (q.y - 50) / 100 * dev.SH; return Math.round(Math.atan(Math.hypot(dx, dy) / dev.P) * 180 / Math.PI) + "°";
}
/* off-screen pointers to other POIs (App › Pointing): off, all, or the best N by level then by how far you must turn */
function pointers(sel) {
  const P = D.app.pointing; if (P.others === "off") return [];
  const lvIdx = p => Math.max(0, D.levels.indexOf(levelOf(p)));
  let list = D.pois.filter(p => p !== sel && shownByTime(p) && (P.min_level === "any" || lvIdx(p) <= D.levels.findIndex(l => l.key === P.min_level)))
    .map(p => ({ p, q: camProject(p.pos.x, p.pos.y) })).filter(o => !onScreen(o.q)).map(o => ({ ...o, d: Math.hypot(o.q.x - 50, (o.q.y - 50) / PHR) }));
  const byDist = (a, b) => a.d - b.d, byLevel = (a, b) => lvIdx(a.p) - lvIdx(b.p);
  list.sort(P.rank === "distance" ? byDist : P.rank === "level" ? (a, b) => byLevel(a, b) || byDist(a, b) : (a, b) => byLevel(a, b) || byDist(a, b));
  if (P.rank === "level_distance") list.sort((a, b) => byLevel(a, b) || byDist(a, b));
  if (D.profile.interests.length && D.app.profiles.interests_emphasis !== "off") list.sort((a, b) => (isInterest(b.p) ? 1 : 0) - (isInterest(a.p) ? 1 : 0));   // the visitor's interests come first
  if (P.others === "top") list = list.slice(0, Math.min(P.max, timeRow().pointers));   // Time sets how many pointers at most
  return list.map(({ p, q, d }) => {
    const e = edgePoint(q), rot = Math.atan2(q.y - 50, (q.x - 50) * PHR) * 180 / Math.PI, far = P.how_far === "off" ? null : h("small", {}, howFar(p, q));
    const arrow = h("span", { class: "chev", style: `rotate:${rot}deg` }, svg("chev"));
    return h("div", { class: `edge ${P.look} glass${e.x < 30 ? " at-l" : e.x > 70 ? " at-r" : ""}`, title: p.name, style: `left:${e.x}%;top:${e.y}%;transform:translate(${e.x < 30 ? "-14%" : e.x > 70 ? "-86%" : "-50%"},-50%)`, onclick: () => camTo(centreOn(p)) },
      P.look === "dot" ? h("span", { class: "cd", style: `background:${catColor(p)}` }) : P.look === "arrow" ? arrow : [h("span", { class: "cd", style: `background:${catColor(p)}` }), p.name, far, arrow]);
  });
}
/* after layout: keep the tap UI inside the safe area (status bar above, the screen card or home bar below), move the ring as one group, and move the tag off the buttons */
function fitInside(tl, ph, keepInside = true, poi = null) {
  const pr = ph.getBoundingClientRect(), b = 0, W = pr.width - 2 * b, Hh = pr.height - 2 * b;
  const box = { l: pr.left + b + W * 0.025, r: pr.right - b - W * 0.025, t: pr.top + b + Hh * 0.075, btm: pr.bottom - b - Math.max(Hh * 0.015, 16 * dev.U / 3.9 * pr.width / (ph.offsetWidth || pr.width)) };   // v20 F: at least 16 pt above the bottom edge (the home-indicator zone; phone landscape sat 8 px off it)
  tl.classList.add("measuring");
  const card = tl.querySelector(":scope > .s-card");
  if (card && poi && D.app.tap.card_yields !== false) {   // the POI is under the screen card: tuck the card down to its title so the POI stays visible
    const tr = tl.getBoundingClientRect(), py = tr.top + poi.y / 100 * tr.height;
    const px = tr.left + poi.x / 100 * tr.width, cr = card.getBoundingClientRect(), side = tl.classList.contains("land");
    const uPx = parseFloat(tl.style.getPropertyValue("--u")) || 4;   // landscape: the card also yields when the POI's ring of buttons would reach it
    const reach = tl.querySelector(":scope > .a-rad") ? 40 : 14;   // v19 F-tap: only a ring of buttons needs the wide berth; a lone tag or none needs the marker's
    if (side ? cr.left < px + reach * uPx : cr.top < py + 24) { card.classList.add("tuck"); card.addEventListener("click", () => card.classList.remove("tuck", "tuck-top"), { once: true }); }
    if (side && card.classList.contains("tuck")) {   // landscape: the tucked title tab still over the POI (lower-right) → it moves to the top-right corner
      const tr2 = card.getBoundingClientRect(), R = 7 * uPx;
      if (px + R > tr2.left && px - R < tr2.right && py + R > tr2.top - 8 && py - R < tr2.bottom) card.classList.add("tuck-top");
    }
  }
  if (card) { const cr = card.getBoundingClientRect(), land = tl.classList.contains("land");
    if (land && !card.classList.contains("tuck")) box.r = Math.min(box.r, cr.left - 6);
    else if (card.classList.contains("tuck-top")) box.t = Math.max(box.t, cr.bottom + 6);
    else box.btm = Math.min(box.btm, cr.top - 6); }
  const union = els => { const rs = els.map(e => e.getBoundingClientRect()).filter(r => r.width && r.height); return { left: Math.min(...rs.map(r => r.left)), right: Math.max(...rs.map(r => r.right)), top: Math.min(...rs.map(r => r.top)), bottom: Math.max(...rs.map(r => r.bottom)) }; };
  const plus = (v, px) => !v ? `${px}px` : v.startsWith("calc(") ? `calc(${v.slice(5, -1)} + ${px}px)` : `calc(${v} + ${px}px)`;
  const shift = (el, dx, dy) => {   // positions may be % or calc(): add the move in px
    if (dx) el.style.left = plus(el.style.left, dx * tl.clientWidth / (tl.getBoundingClientRect().width || 1));
    if (dy) { const k = tl.clientHeight / (tl.getBoundingClientRect().height || 1); if (el.style.bottom) el.style.bottom = plus(el.style.bottom, -dy * k); else el.style.top = plus(el.style.top, dy * k); }
  };
  const into = rc => [rc.left < box.l ? box.l - rc.left : rc.right > box.r ? box.r - rc.right : 0, rc.top < box.t ? box.t - rc.top : rc.bottom > box.btm ? box.btm - rc.bottom : 0];
  const ring = [...tl.querySelectorAll(":scope > .a-rad")];
  const withLabels = els => els.flatMap(e => [e, ...e.querySelectorAll(":scope > .l")]);   // the ring's extent includes its name tags (they hang below the discs)
  const groups = [...tl.querySelectorAll(":scope > .t-label, :scope > .t-chip, :scope > .t-card, :scope > .t-imm, :scope > .a-cap, :scope > .irow, :scope > .t-pan, :scope > .a-dock")].map(el => [el]);
  if (ring.length) groups.push(ring);
  if (keepInside && ring.length > 1) {   // a ring taller or wider than the safe area (big buttons: Playful; landscape; a big gap) pulls its buttons in towards the POI
    const rr = union(withLabels(ring)), f = Math.min(1, (box.r - box.l) / (rr.right - rr.left || 1), (box.btm - box.t) / (rr.bottom - rr.top || 1));
    if (f < 1) { const cx = (rr.left + rr.right) / 2, cy = (rr.top + rr.bottom) / 2, k = f * 0.97;
      ring.forEach(el => { const r = el.getBoundingClientRect(), ex = (r.left + r.right) / 2, ey = (r.top + r.bottom) / 2; shift(el, (cx + (ex - cx) * k) - ex, (cy + (ey - cy) * k) - ey); }); }
  }
  const rectOf = g => g[0].classList.contains("t-pan") ? union([...g[0].children]) : union(g === ring ? withLabels(g) : g);
  if (keepInside) for (const g of groups) { const [dx, dy] = into(rectOf(g)); if (dx || dy) g.forEach(el => shift(el, dx, dy)); }
  const pin = tl.querySelector(":scope > .pin"); if (pin) { const [dx, dy] = into(pin.getBoundingClientRect()); shift(pin, dx, dy); }   // the pin always stays readable
  // the tag must not sit on the buttons: move it past them, on the side with room, then keep it inside again
  const tag = tl.querySelector(":scope > .t-label, :scope > .t-chip, :scope > .t-card, :scope > .t-imm");
  const btnGroup = ring.length ? ring.flatMap(e => [...e.querySelectorAll(".c, .l")]) : [...tl.querySelectorAll(":scope > .a-cap")];
  if (tag && btnGroup.length) {
    const u = union(btnGroup), t = tag.getBoundingClientRect(), hit = Math.min(t.right, u.right) > Math.max(t.left, u.left) && Math.min(t.bottom, u.bottom) > Math.max(t.top, u.top);
    if (hit) {   // candidates: below, above, left, right of the buttons; the first that fits inside and stays off the POI wins
      const th = t.bottom - t.top, tw = t.right - t.left, tr = tl.getBoundingClientRect();
      const P = poi ? { x: tr.left + poi.x / 100 * tr.width, y: tr.top + poi.y / 100 * tr.height } : null;
      const cands = [[t.left, u.bottom + 12], [t.left, u.top - 12 - th], [u.left - 12 - tw, (u.top + u.bottom - th) / 2], [u.right + 12, (u.top + u.bottom - th) / 2]];
      const fits = ([x, y]) => x >= box.l && x + tw <= box.r && y >= box.t && y + th <= box.btm && !(P && P.x > x && P.x < x + tw && P.y > y && P.y < y + th);
      const pick = cands.map(([x, y]) => [clamp(x, box.l, box.r - tw), y]).find(c => fits(c) && !(Math.min(c[0] + tw, u.right) > Math.max(c[0], u.left) && Math.min(c[1] + th, u.bottom) > Math.max(c[1], u.top)))
        || (() => {   // none of the four fits: the free spot nearest to where the tag was (a scan of the safe area, 8 px steps)
          let best = null, bd = 1e9;
          for (let y = box.t; y + th <= box.btm; y += 8) for (let x = box.l; x + tw <= box.r; x += 8) {
            if (!fits([x, y]) || (Math.min(x + tw, u.right) + 12 > Math.max(x, u.left) && Math.min(y + th, u.bottom) + 12 > Math.max(y, u.top))) continue;
            const d = Math.hypot(x - t.left, y - t.top); if (d < bd) { bd = d; best = [x, y]; }
          }
          return best || [t.left, box.btm - u.bottom > u.top - box.t ? u.bottom + 12 : u.top - 12 - th];
        })();
      tag.style.transform = "none"; tag.style.bottom = ""; tag.style.left = (pick[0] - tr.left) / tr.width * 100 + "%"; tag.style.top = (pick[1] - tr.top) / tr.height * 100 + "%";
      if (keepInside) { const [dx, dy] = into(tag.getBoundingClientRect()); shift(tag, dx, dy); }
    }
  }
  // the tag never sits on the POI: try right, left, above, below the POI; keep the first that fits (inside, off the buttons)
  if (tag && poi) {   // v19 F-tap: the POI is its marker's disc (size, status ring, badge), not a point: a card edge on the disc hid the pin
    const tr = tl.getBoundingClientRect(), px = tr.left + poi.x / 100 * tr.width, py = tr.top + poi.y / 100 * tr.height, t = tag.getBoundingClientRect();
    const mk = ph.querySelector?.(":scope .mk.sel"), PS = mk ? pointStyleOf(D.pois[+mk.dataset.i]) : null, Rm = PS ? (PS.size_pt / 2 + 5) * dev.U / 3.9 * pr.width / dev.SW : 10;
    if (px + Rm > t.left && px - Rm < t.right && py + Rm > t.top && py - Rm < t.bottom) {
      const w = t.width, hh = t.height, gap = Rm + 10, u = btnGroup.length ? union(btnGroup) : null;
      const ok = (x, y) => x >= box.l && x + w <= box.r && y >= box.t && y + hh <= box.btm && !(u && Math.min(x + w, u.right) > Math.max(x, u.left) && Math.min(y + hh, u.bottom) > Math.max(y, u.top));
      const cands = [[px + gap, py - hh / 2], [px - gap - w, py - hh / 2], [px - w / 2, py - gap - hh], [px - w / 2, py + gap]].map(([x, y]) => [clamp(x, box.l, box.r - w), clamp(y, box.t, box.btm - hh)]);
      const offP = (x, y) => !(px + Rm > x - 6 && px - Rm < x + w + 6 && py + Rm > y - 6 && py - Rm < y + hh + 6);
      let best = cands.find(([x, y]) => ok(x, y) && offP(x, y));
      if (!best) { let bd = 1e9; for (let y = box.t; y + hh <= box.btm; y += 8) for (let x = box.l; x + w <= box.r; x += 8) if (ok(x, y) && offP(x, y)) { const d = Math.hypot(x - t.left, y - t.top); if (d < bd) { bd = d; best = [x, y]; } } }
      best = best || cands.find(([x, y]) => offP(x, y));
      if (best) { tag.style.transform = "none"; tag.style.bottom = ""; tag.style.left = (best[0] - tr.left) / tr.width * 100 + "%"; tag.style.top = (best[1] - tr.top) / tr.height * 100 + "%";
        tl.querySelectorAll(":scope > .t-conn").forEach(c => c.style.display = "none"); }   // the connector pointed up or down to the old place
    }
  }
  tl.classList.remove("measuring");
}

let demoTimer = 0;
function frameForDemo() {   // editor demos only: bring the POI to the upper middle so the whole UI (and a screen card) fits
  const p = D.pois[ui.appPoi], q = camProject(p.pos.x, p.pos.y);
  if (onScreen(q) && q.y > 18 && q.y < 44 && q.x > 12 && q.x < 88 && camCovers(ui.cam)) return;
  camTo(demoPose(p), 350);
}
/* v19 F-tap: the demo camera puts the POI at the upper middle with the wall over the whole screen; a POI low on the wall needs the camera a little
   closer (the wall's edge would otherwise hold it low and push the tap UI off the bottom) */
function demoPose(p) {
  let pose = null;
  for (const z of [Math.max(ui.cam.z, 1.3), 1.5, 1.75, 2.0]) { pose = camCover(camPoseFor(p.pos, 50, 38, z, ui.cam.r, 0, 0)); const q = camProject(p.pos.x, p.pos.y, pose); if (q.y <= 42 && q.x > 12 && q.x < 88) break; }
  return pose;
}
function demoLayout(key) {   // the main screen, a finger taps the marker (the camera first brings it into view), then the layout opens
  clearTimeout(demoTimer); ui.demoTap = 0; ui.demoTapFx = 0; ui.compPreview = null; ui.previewLayout = key; tapState.view = null; ui.appStep = "main"; setScreen("app"); renderPhone();
  const p = D.pois[ui.appPoi], q = camProject(p.pos.x, p.pos.y);
  // v19 F-tap: the camera framing (wall over the whole screen: camCover) and the finger run together, so the tap UI has settled ~0.5 s after the fold opens
  if (!(onScreen(q) && q.x > 12 && q.x < 88 && q.y > 18 && q.y < 44 && camCovers(ui.cam))) camTo(demoPose(p), 260);
  if (!phoneEls?.layer) { ui.appStep = "tapped"; paintAR(false); return; }
  ui.demoTap = performance.now(); paintAR(false);   // the finger is part of the main screen until the tap (survives any re-render; it follows the POI while the camera moves)
  demoTimer = setTimeout(() => { ui.demoTap = 0; ui.demoTapFx = performance.now(); const keep = ui.previewLayout; tapPoiOnPhone(ui.appPoi); ui.previewLayout = keep; paintAR(false); }, 380);
}
const usedTap = key => (D.app.tap.default_layout === key ? 1 : 0) + D.levels.filter(l => l.tap_layout === key).length + D.pois.filter(p => p.on_tap?.tap_layout === key).length;
const usedComp = (field, key) => D.tap_layouts.filter(s => s[field] === key).length + D.pois.filter(p => p.on_tap?.over?.[field] === key).length;
function newKey(list, prefix) { let n = 1; while (list.find(t => t.key === prefix + n)) n++; return prefix + n; }
/* Unity's ReorderableList: one foldout per element, edited in place; the + adds at the end */
function listElement(id, title, sub, list, i, onDemo, body, opts = {}) {
  const it = list[i], used = opts.used ? opts.used(it.key) : 0;
  const btns = h("span", { style: "display:flex;gap:3px;align-items:center" },
    used ? h("span", { class: "sub", style: "margin-right:4px" }, `used by ${used}`) : null,
    onDemo ? h("button", { class: "ib", title: "Play the demo", onclick: () => onDemo(it) }, "▶") : null,
    h("button", { class: "ib", title: "Move up", disabled: i === 0, onclick: () => { snap(); list.splice(i - 1, 0, list.splice(i, 1)[0]); changed(); } }, "↑"),
    h("button", { class: "ib", title: "Duplicate", onclick: () => { snap(); const c = clone(it); c.key = newKey(list, opts.prefix); c.name = it.name + " copy"; list.splice(i + 1, 0, c); ui.open[id.replace(it.key, c.key)] = true; changed(); onDemo && onDemo(c); } }, "⧉"),
    h("button", { class: "ib", title: used ? "In use: pick another one there first" : "Remove", disabled: used > 0, onclick: () => { snap(); list.splice(i, 1); changed(); } }, "−"));
  return fold(id, title, { color: "#F2C14E", sub, extra: btns, screen: opts.screen, step: opts.step, onToggle: o => { if (o && onDemo) onDemo(it); } }, body);
}
function tapLayoutsBody() {
  const out = [...row("Default for the whole app", fSelect(D.app.tap, "default_layout", D.tap_layouts.map(t => [t.key, t.name]), () => { ui.previewLayout = null; ui.compPreview = null; renderPhone(); }), "Every POI uses this style unless its level (Global Scene › Levels › On tap) or the POI itself (Specific Marker › On tap) picks another; a POI can also override single fields."),
    h("p", { class: "guide" }, "A style puts the components together: focus, a tag component, an action component, the screen card and the ask field, and how they sit around the marker. Open one: the phone plays it (▶ replays).")];
  D.tap_layouts.forEach((L, i) => {
    const prev = () => { ui.previewLayout = L.key; ui.appStep = "tapped"; tapState.view = null; ui.tapAnchor = null; };
    const sel = (key, opts) => id => h("select", { id, onchange: e => { snap(); L[key] = e.target.value; prev(); changed(); } }, opts.map(([v, t]) => h("option", { value: v, selected: L[key] === v }, t)));
    const T = tagComp(L.tag), A = actComp(L.actions);
    out.push(listElement("tapL_" + L.key, L.name, tapSummary(L), D.tap_layouts, i, x => demoLayout(x.key), () => [
      ...row("Name", fText(L, "name")),
      ...row("Focus", sel("focus", TAP_OPTS.focus)),
      ...row("Tag", sel("tag", [["none", "None"], ...D.tap_tags.map(t => [t.key, t.name])])),
      ...(T && T.kind !== "panel" ? row("Tag placement", sel("place", TAP_OPTS.place), null, true) : []),
      ...row("Actions", sel("actions", [["none", "None"], ...D.tap_action_comps.map(t => [t.key, t.name])])),
      ...row("Distance to the marker", id => { const f = fRange(L, "gap", 0, 20, 1)(id); f.addEventListener("input", () => { prev(); renderPhone(); }); return f; }, "How far the tag and the buttons sit from the marker, in phone units (0 = right next to it). A POI can override it: a big marker or a 3D model needs more room."),
      h("div", { class: "grp" }, "Screen or wall", h("span", {}, "glued parts stay on the wall as the phone moves")),
      ...["focus", "tag", "actions"].map(k => row(`${k[0].toUpperCase() + k.slice(1)} on the wall`, id => h("input", { id, type: "checkbox", checked: !!L.on_wall?.[k], disabled: k === "focus" ? ["none", "spotlight"].includes(L.focus) : k === "tag" ? !T : !A || ["in_tag", "tiles", "dock"].includes(A.kind), title: "Off when this style has no such part, or the part lives in the tag or on the screen", style: "justify-self:start", onchange: e => { snap(); L.on_wall = { ...(L.on_wall || {}), [k]: e.target.checked }; prev(); changed(); } }),
        k === "focus" ? "On the wall, the ring sits on the POI and you see it from the side when you walk around. Spotlight always dims the whole screen." : k === "tag" ? "Off: the tag is drawn on the screen next to the POI and follows it. On: the tag is placed once, when the POI is tapped, and then stays in the world, lifted off the wall by the distance below." : "Same for the action buttons.")).flat(),
      ...(L.on_wall && (L.on_wall.focus || L.on_wall.tag || L.on_wall.actions) ? row("Distance from the wall", id => { const f = fRange(L, "depth_cm", 0, 40, 1, " cm")(id); f.addEventListener("input", () => { prev(); renderPhone(); }); return f; }, "How far in front of the tiles the glued parts float. 0 = painted on the wall. Walk to the side (Alt + drag) to see the gap.", true) : []),
      h("div", { class: "grp" }, "On the screen", h("span", {}, "these never go on the wall")),
      ...row("Screen card", sel("sheet", TAP_OPTS.sheet)),
      ...row("Ask field", sel("ask", TAP_OPTS.ask.map(([v, t]) => [v, t + (v === "capsule" && A?.kind !== "capsule" ? " (needs the capsule)" : v === "sheet" && L.sheet === "none" ? " (needs a screen card)" : ""), (v === "capsule" && A?.kind !== "capsule") || (v === "sheet" && L.sheet === "none")]))),
      ...(A?.kind === "tiles" && L.sheet === "none" ? [h("div", { class: "notice" }, "Tiles live in the screen card: pick a screen card.")] : []),
      ...(A?.kind === "in_tag" && !["card", "immersive", "panel", "chip"].includes(T?.kind) ? [h("div", { class: "notice" }, "“Links in the tag” needs a chip, card, immersive card or panel tag.")] : []),
    ], { used: usedTap, prefix: "tap_layout_", screen: "app", step: "tapped" }));
  });
  out.push(h("div", { class: "addrow", style: "justify-content:flex-end" }, h("button", { class: "ubtn", style: "flex:0 0 auto", onclick: () => { snap(); const key = newKey(D.tap_layouts, "tap_layout_"); D.tap_layouts.push({ key, name: "New style", focus: "ring", tag: "tag_comp_2", place: "auto", actions: "act_comp_2", sheet: "none", ask: "off", on_wall: { ...SCREEN }, depth_cm: 8, gap: 0 }); ui.open["tapL_" + key] = true; changed(); demoLayout(key); } }, "+ Add style")));
  return out;
}
/* base elements: every action button in priority order (the first ones show, the rest go under "more"), and the button look */
function tapBaseBody() {
  const preview = () => { ui.previewLayout = null; ui.compPreview = { type: "buttons" }; ui.appStep = "tapped"; tapState.view = null; renderPhone(); };
  const rows = D.tap_actions.map((a, i) => h("div", { class: "brow" + (a.show === "off" ? " off" : "") },
    h("button", { class: "ib", title: "Move up (earlier = shown first)", disabled: i === 0, onclick: () => { snap(); D.tap_actions.splice(i - 1, 0, D.tap_actions.splice(i, 1)[0]); changed(); preview(); } }, "↑"),
    h("span", { class: "bi" }, svg(a.key)),
    h("input", { class: "uin", value: a.label, "aria-label": "Button label", onchange: e => { const v = e.target.value.trim(); if (!v) return; snap(); a.label = v; changed(); preview(); } }),
    h("select", { "aria-label": a.label + " shows", onchange: e => { snap(); a.show = e.target.value; changed(); preview(); } },
      [["auto", NEEDS[a.key] ? `When the POI has ${NEEDS[a.key][1]}` : "Auto"], ["always", "Always"], ["off", "Off"]].filter(([v]) => v !== "auto" || NEEDS[a.key]).map(([v, t]) => h("option", { value: v, selected: a.show === v }, t))),
    h("small", { class: "bk" }, ACTIONS[a.key]?.label || "")));
  const B = D.tap_button;
  return [h("p", { class: "guide" }, "One line per action type (the types are framework code). Order = priority: a component shows the first ones and puts the rest under “more”. “When the POI has …” hides the button on POIs without that content; a POI can still hide or force one."),
    h("div", { class: "blist" }, ...rows),
    h("div", { class: "grp" }, "Button look", h("span", {}, "from App › 1 Design system, unless overridden here")),
    ...row("Use the Design system's buttons", id => h("input", { id, type: "checkbox", checked: B.use_ds !== false, style: "justify-self:start", onchange: e => { snap(); B.use_ds = e.target.checked; changed(); preview(); } })),
    ...(B.use_ds === false ? [
      ...row("Size", id => { const f = fRange(B, "size", 0.8, 1.3, 0.05, "×")(id); f.addEventListener("input", preview); return f; }, null, true),
      ...row("Shape", fSelect(B, "shape", [["circle", "Circle"], ["rounded", "Rounded square"]], preview), null, true),
      ...row("Surface", fSelect(B, "surface", [["glass", "Glass (blurred)"], ["solid", "Solid light"]], preview), null, true)] : []),
    h("p", { class: "guide" }, "Colours for dark and for light, type, corners and depth: App › 1 Design system.")];
}
function tagCompsBody() {
  const out = [h("p", { class: "guide" }, "Finished tags, ready to use in a style. What each shows is set here once.")];
  D.tap_tags.forEach((T, i) => {
    const demo = () => { clearTimeout(demoTimer); ui.previewLayout = null; ui.compPreview = { type: "tag", key: T.key }; ui.appStep = "tapped"; tapState.view = null; renderPhone(); frameForDemo(); };
    const sub = [TAP_OPTS.tag_kind.find(k => k[0] === T.kind)?.[1].split(" (")[0], T.kind === "immersive" ? `width ${T.size} u` : null, T.media && T.kind !== "label" && T.kind !== "chip" ? T.media : null].filter(Boolean).join(" · ");
    out.push(listElement("tagc_" + T.key, T.name, sub, D.tap_tags, i, demo, () => [
      ...row("Name", fText(T, "name")),
      ...row("Kind", fSelect(T, "kind", TAP_OPTS.tag_kind, demo)),
      ...(T.kind === "immersive" ? row("Width", id => { const f = fRange(T, "size", 48, 80, 1, " u")(id); f.addEventListener("input", () => renderPhone()); return f; }, "The card width (1 u = 1 % of a 390 pt phone): 54 ≈ small, 66 medium, 78 large.", true) : []),
      ...(["card", "immersive", "panel"].includes(T.kind) ? row("Picture", fSelect(T, "media", [["image", "Photo"], ["video", "Video (loops, muted)"], ["none", "None"]], demo), "Photo: the POI's hero photo. Video: the POI's clip, looping and muted (here: the photo with a play badge). None: text only.", true) : []),
      ...(T.kind !== "label" ? [...row("Category", fToggle(T, "show_cat"), null, true), ...row("One-line summary", fToggle(T, "show_summary"), null, true),
        ...row("Buttons inside", fToggle(T, "buttons", demo), "The POI's buttons as a row inside the tag, when the style has no other action component (or uses “Links in the tag”).", true)] : []),
    ], { used: k => usedComp("tag", k), prefix: "tag_comp_", screen: "app", step: "tapped" }));
  });
  out.push(h("div", { class: "addrow", style: "justify-content:flex-end" }, h("button", { class: "ubtn", style: "flex:0 0 auto", onclick: () => { snap(); const key = newKey(D.tap_tags, "tag_comp_"); D.tap_tags.push({ key, name: "New tag", kind: "chip", show_cat: true, show_summary: true }); ui.open["tagc_" + key] = true; changed(); } }, "+ Add tag component")));
  out.push(h("div", { class: "grp" }, "Colours", h("span", {}, "the tags' surface, per design")), colourFold("Tag colours", { only: ["tags"], id: "tagcol" }));
  return out;
}
function actCompsBody() {
  const out = [h("p", { class: "guide" }, "Finished button groups, ready to use in a style. The buttons themselves come from Base elements.")];
  D.tap_action_comps.forEach((A, i) => {
    const demo = () => { clearTimeout(demoTimer); ui.previewLayout = null; ui.compPreview = { type: "act", key: A.key }; ui.appStep = "tapped"; tapState.view = null; renderPhone(); frameForDemo(); };
    out.push(listElement("actc_" + A.key, A.name, `${TAP_OPTS.act_kind.find(k => k[0] === A.kind)?.[1]} · up to ${A.max}${A.labels ? " · labels" : ""}`, D.tap_action_comps, i, demo, () => [
      ...row("Name", fText(A, "name")),
      ...row("Kind", fSelect(A, "kind", TAP_OPTS.act_kind, demo)),
      ...row("At most", id => { const f = fRange(A, "max", 1, 8, 1)(id); f.addEventListener("change", demo); return f; }, "The rest go under “more”."),
      ...row("Labels under the icons", fToggle(A, "labels")),
    ], { used: k => usedComp("actions", k), prefix: "act_comp_", screen: "app", step: "tapped" }));
  });
  out.push(h("div", { class: "addrow", style: "justify-content:flex-end" }, h("button", { class: "ubtn", style: "flex:0 0 auto", onclick: () => { snap(); const key = newKey(D.tap_action_comps, "act_comp_"); D.tap_action_comps.push({ key, name: "New buttons", kind: "capsule", max: 4, labels: true }); ui.open["actc_" + key] = true; changed(); } }, "+ Add action component")));
  return out;
}
/* the two "when the phone moves" switches show their case: the POI under the screen card, or at the screen's edge */
function edgeDemo(key) {
  const i = 0, p = D.pois[i]; ui.appPoi = i; ui.appStep = "tapped"; ui.compPreview = null; tapState.view = null; setScreen("app");
  ui.previewLayout = key === "card_yields" ? "tap_layout_5" : "tap_layout_2";
  ui.cam = key === "card_yields" ? camPoseFor(p.pos, 50, 80, 1.3, 0, 0, 0) : camPoseFor(p.pos, 90, 14, 1.3, 0, 0, 0); renderPhone();
}
const edgeToggle = key => id => h("input", { id, type: "checkbox", checked: D.app.tap[key] !== false, style: "justify-self:start", onchange: e => { snap(); D.app.tap[key] = e.target.checked; renderEditor(); edgeDemo(key); }, onfocus: () => edgeDemo(key) });
function appTab() {
  const A = D.app, M = A.main, O = A.onboarding, X = A.explore;
  const sec = (key, title, step, body, opts = {}) => fold(key, title, Object.assign({ color: "#F2C27A", screen: "app", step, demo: { step, preview: null, view: null } }, opts), body);   // v19: a section header shows its own screen
  const POS = [["top-left", "Top left"], ["top-right", "Top right"], ["bottom-left", "Bottom left"], ["bottom-right", "Bottom right"]];
  return [
    h("p", { class: "lab-intro" }, h("b", {}, "App · the screens around the wall. "), "Compose the app's own screens: what a visitor sees before the camera, the main AR screen's buttons, and what a tap on a POI shows. The phone on the left follows every change; click a section to open its screen. Looks and content still come from Global Scene and Specific Marker; Azu's behaviour from the Guide tab."),
    sec("app_prof", "0 · Profiles", "main", profilesBody, { color: "#FFD27A", badge: "new", sub: `${visitorRow().name} · ${timeRow().name} · ${D.looks[activeLook()].name} ${appearance()}` }),
    sec("app_ds", "1 · Design system", "tapped", designBody, { color: "#C9B7FF", demo: kitDemo("look"), sub: "foundations · components · patterns" }),
    sec("app_start", "2 · Start", "start", () => [...row("Loading lines", fToggle(A.start, "loading_lines")), h("p", { class: "guide" }, "Splash, download of the wall's media on first launch (parked), friendly loading lines instead of a spinner.")]),
    sec("app_onb", "3 · Onboarding", "onboarding", () => [...row("Show onboarding", fToggle(O, "on")), ...(O.on ? [
      ...row("Ask the look", fToggle(O, "ask_look"), "Premium or Playful as two pictures; a tap restyles the page itself, so the visitor sees the difference at once. Untouched, the look follows who the tour is for.", true),
      ...row("Quick icons", fToggle(O, "quick"), "Small, optional icons at the top: light / dark, language, text size. The phone's own settings are the default.", true),
      ...row("Ask who the tour is for", fToggle(O, "ask_visitors"), "Ages 7–11 · playful adventure, or Ages 12+ · full museum tour (research: design for 9, core 8–10; the word \"kids\" pushes teens away). Two plain buttons (7–11 left, 12+ right) and one example card under them: the same place as each audience gets it (App › 0 Profiles › Visitors).", true),
      ...row("Ask how long", fToggle(O, "ask_time"), "Quick · Classic · Deep (App › 0 Profiles › Time).", true),
      ...row("Ask interests (optional)", fToggle(O, "ask_interests"), "The categories as chips; they emphasise, never hide.", true),
      ...row("Language icon", fToggle(O, "ask_language"), "Part of the quick icons: EN ⇄ PT.", true)] : []),
      h("p", { class: "guide" }, "One page, top to bottom: the look, who the tour is for, how long, then optional interests. Skippable; everything can be changed later in Settings (the gear by the search). Choosing on the phone also sets the profile bar.")]),
    sec("app_main", "4 · Main AR screen", "main", mainBody),
    sec("app_tap", "5 · On tap", "tapped", () => [
      fold("tap_base", "Base elements", { color: "#9FD7C1", screen: "app", step: "tapped", demo: { step: "tapped", preview: { type: "buttons" } }, sub: `${D.tap_actions.filter(a => a.show !== "off").length} buttons · button look` }, tapBaseBody),
      fold("tap_tags", "Tag components", { color: "#9FD7C1", screen: "app", step: "tapped", demo: { step: "tapped", preview: { type: "tag", key: D.tap_tags[0]?.key }, after: frameForDemo }, sub: D.tap_tags.map(t => t.name).join(" · ") }, tagCompsBody),
      fold("tap_acts", "Action components", { color: "#9FD7C1", screen: "app", step: "tapped", demo: { step: "tapped", preview: { type: "act", key: D.tap_action_comps[0]?.key }, after: frameForDemo }, sub: D.tap_action_comps.map(t => t.name).join(" · ") }, actCompsBody),
      fold("tap_styles", "On-tap styles", { color: "#F2C14E", screen: "app", step: "tapped", demo: { step: "tapped", preview: null, keepLayout: true, after: frameForDemo }, sub: "default: " + (tapStyleByKey(D.app.tap.default_layout)?.name || "—") }, tapLayoutsBody),
      fold("tap_soon", "Coming soon · places without a story yet", { color: "#9FD7C1", screen: "app", step: "tapped", demo: { step: "tapped", preview: null, view: null, after: soonDemo }, sub: `${D.pois.filter(isSoon).length} on the wall` }, soonBody),
      h("div", { class: "grp" }, "When the phone moves", h("span", {}, "safe area, screen card")),
      ...row("Screen card makes way for the POI", edgeToggle("card_yields"), "When the POI ends up under the screen card, the card tucks down to its title; tap it to open it again."),
      ...row("Keep the tap UI inside the screen", edgeToggle("keep_inside"), "After layout, every part is moved inside the safe area: below the status bar, above the home bar or the screen card. The ring moves as one group; the tag always moves off the buttons and off the POI."),
      h("p", { class: "guide" }, "A tap never moves the camera: the tap UI opens around the POI where it is. Scroll = walk closer / further, drag = look around, Alt + drag = walk around, Shift + scroll = tilt. When the selected POI leaves the screen: see Pointing.")], { demo: { step: "tapped", preview: null, keepLayout: true, after: frameForDemo } }),
    sec("app_exp", "6 · Explore menu", "explore", () => [
      ...row("Tours", fToggle(X, "tours")), ...row("Games", fToggle(X, "games")), ...row("Moments on the wall", fToggle(X, "moments")), ...row("Wall layers", fToggle(X, "layers")),
      h("p", { class: "guide" }, "The wall-level features. Their content lives in the Experiences tab.")], { demo: { step: "explore", preview: null } }),
    sec("app_set", "7 · Settings and You", "settings", () => [
      h("p", { class: "guide" }, "Settings: everything the visitor can change, by area (Your visit · Look and reading · On the wall · Sound and motion · Help). Opened from the gear by the search (Main AR › Search) or from You. You: the visitor's own panel: the visit at a glance, saved places, what they found, the album."),
      h("div", { class: "grp" }, "Areas offered", h("span", {}, "Your visit and Look and reading are always there")),
      ...row("On the wall", fToggle(D.app.settings, "wall"), "Mini-map (when lost / always / off), names, arrows to off-screen places, interest glow, life on the tiles."),
      ...row("Sound and motion", fToggle(D.app.settings, "sound"), "Sounds, vibration, reduce motion."),
      ...row("Help and about", fToggle(D.app.settings, "help"), "See the welcome again, the first tips again, privacy, about."),
      ...row("Preview", id => h("div", { id, class: "btnrow" }, h("button", { class: "ubtn", onclick: () => { ui.appStep = "settings"; renderPhone(); } }, "Settings"), h("button", { class: "ubtn", onclick: () => { ui.appStep = "you"; renderPhone(); } }, "You")))], { demo: { step: "settings", preview: null } }),
  ];
}

/* App › 5 On tap › Coming soon: a POI marked on the wall before its story is written. The name always shows; the text follows the audience */
function soonDemo(open) { const i = D.pois.findIndex(isSoon); if (i < 0) return; ui.appPoi = i; ui.appStep = "tapped"; ui.tapAnchor = null; tapState.view = open ? { p: D.pois[i], a: "card" } : null; ui.cam = camCover(camPoseFor(D.pois[i].pos, 50, 34, 1.25)); renderPhone(); }
function soonBody() {
  const V = new Proxy({}, { get: (t, k) => soonText()[k], set: (t, k, v) => { soonText()[k] = v; return true; } });
  const live3 = f => id => { const el = f(id); el.addEventListener("input", () => soonDemo(!!tapState.view)); return el; };
  return [
    h("p", { class: "guide" }, "Mark a POI as “Coming soon” in Specific Marker › Content (or leave its summary and story empty). The visitor sees its name on the wall; a tap shows the badge, one line and two buttons: what's coming and save. The text below is for the audience chosen in the profile bar."),
    ...row("Badge", live3(fText(V, "badge"))), ...row("Line", live3(fText(V, "line")), "On the tag, the panel and the screen card."),
    ...row("Page text", live3(fText(V, "body")), "On the “What's coming” page."),
    ...row("Preview", id => h("div", { id, class: "btnrow" }, h("button", { class: "ubtn", onclick: () => soonDemo(false) }, "On the wall"), h("button", { class: "ubtn", onclick: () => soonDemo(true) }, "What's coming page"))),
  ];
}
/* ---------- placeholders for planned features (a card: purpose, configs, phase) ---------- */

/* App › 3 Main AR screen › Pointing: the selected POI after a tap, and the other POIs, when they are off the screen */
function pointingBody() { const A = D.app; const P = A.pointing, re = () => { renderPhone(); }, off = () => mainDemo("offsel"); return [
      h("div", { class: "grp" }, "The selected POI", h("span", {}, "after a tap, when the phone turns away")),
      ...row("When it leaves the screen", fSelect(A.tap, "offscreen", [["pin", "A pointer at the edge"], ["close", "Close the tap UI"], ["keep", "Keep the tap UI, clamped inside"]], off), "pointer: a small chip at the screen edge points back to the POI; tap it to turn back. close: back to the main screen. keep: the tag stays, pushed inside the screen."),
      ...(A.tap.offscreen === "pin" ? row("Its pointer", fSelect(P, "sel_look", [["chip", "Name + arrow"], ["arrow", "Arrow only"]], off), null, true) : []),
      ...row("Keep other pointers while a POI is open", demoToggle(P, "while_tapped", "offsel")),
      h("div", { class: "grp" }, "Other POIs", h("span", {}, "on the main AR screen")),
      ...row("Point to", fSelect(P, "others", [["off", "None"], ["top", "The best few"], ["all", "All of them"]], re)),
      ...(P.others === "top" ? row("How many", fRange(P, "max", 1, 8, 1), null, true) : []),
      ...(P.others !== "off" ? [
        ...row("Pick by", fSelect(P, "rank", [["level_distance", "Level first, then the nearest"], ["distance", "The nearest (least turning)"], ["level", "Level only"]], re), "Level = the POI's hierarchy level (Global Scene › Levels, top = most important). Nearest = how far you must turn the phone to see it.", true),
        ...row("Only from level", fSelect(P, "min_level", [["any", "Any level"], ...D.levels.map(l => [l.key, l.name + " and above"])], re), null, true),
        ...row("Look", fSelect(P, "look", [["chip", "Name + arrow"], ["arrow", "Arrow"], ["dot", "Coloured dot"]], re), null, true),
        ...(P.look === "chip" ? row("Show how far", fSelect(P, "how_far", [["off", "No"], ["deg", "Degrees to turn the phone"], ["m", "Metres on the wall"]], re), "Degrees: how much the visitor must turn the phone. Metres: the distance on the wall from the point at the centre of the screen to the POI.", true) : [])] : []),
      h("p", { class: "guide" }, "Turn the phone away from a POI (drag the picture) to see the pointers. Later: the next stop of a tour, unseen POIs first.")]; }

/* Dark design / Light design: the same rows for both; opening one previews it. opts.only: the colour groups to show (Design system › Colour: the roles; On tap: tags; Main AR: wall labels) */
function colourFold(title, opts = {}) {
  const mode = appearance(), T = D.themes[mode], O = D.themes[mode === "dark" ? "light" : "dark"], only = opts.only || ["glass", "text", "buttons", "tags", "sheet", "labels"], show = () => renderPhone();
  const lum = hex => { const c = [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16) / 255).map(v => v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4); return 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2]; };
  const ratio = (a, b) => { const x = lum(a), y = lum(b); return (Math.max(x, y) + 0.05) / (Math.min(x, y) + 0.05); };
  const col = (obj, key, label, guide, on) => row(label, id => { const f = fColor(obj, key)(id); f.addEventListener("pointerdown", show, true); if (on) {   // the contrast in this mode, and in the other mode of the same look (a change here must not break it there)
        const sc = obj[SCOPE], oo = sc ? sc.path.reduce((o, k) => o?.[k], O) : null, badge = (r, m) => h("span", { class: "cbadge" + (r >= 7 ? " ok" : r >= 4.5 ? " mid" : " bad"), title: `Contrast on its surface, ${m}. The rule (D1): text 7:1 or more, in both modes` }, (m === mode ? "" : m[0].toUpperCase() + " ") + r.toFixed(1) + ":1");
        f.append(badge(ratio(obj[key], on()), mode)); if (oo && oo[key]) f.append(badge(ratio(oo[key], on(O)), mode === "dark" ? "light" : "dark")); } return f; }, guide, true);
  const tint = (obj, label = "Opacity") => row(label, fRange(obj, "tint", 0.1, 1, 0.01), null, true);
  const G = {
    glass: () => [h("div", { class: "grp" }, "Glass", h("span", {}, "anything floating over the camera: search, bars, chips, glass panels")),
      ...col(T.glass, "color", "Colour"), ...tint(T.glass), ...row("Blur", fRange(T.glass, "blur", 0, 40, 1, " px"), null, true)],
    text: () => [h("div", { class: "grp" }, "Text, lines and accent"),
      ...col(T.text, "main", "Text", null, (X = T) => X.glass.color), ...col(T.text, "soft", "Secondary text", null, (X = T) => X.glass.color), ...col(T.border, "color", "Border"), ...row("Border opacity", fRange(T.border, "opacity", 0, 0.5, 0.01), null, true),
      ...col(T, "accent", "Accent", "The main button (one per view), selected chips, segments and tabs, switches, the “more” button, tile icons, the coach mark. Pick from the closed list in Colour › Accent; a free colour must keep 7:1."), ...col(T, "accent_text", "Text on accent", null, (X = T) => X.accent)],
    buttons: () => [h("div", { class: "grp" }, "Icon buttons", h("span", {}, "the actions: radial, dock, tiles, list-row icons")),
      ...col(T.buttons, "color", "Button"), ...tint(T.buttons), ...col(T.buttons, "icon", "Icon", null, (X = T) => X.buttons.color), ...col(T.buttons, "label", "Label pill"), ...col(T.buttons, "label_text", "Label text", null, (X = T) => X.buttons.label)],
    tags: () => [h("div", { class: "grp" }, "Tags", h("span", {}, "label, chip, compact card, floating panel")), ...col(T.tags, "color", "Tag"), ...tint(T.tags)],
    sheet: () => [h("div", { class: "grp" }, "Panels", h("span", {}, "screen card, sheets, dialogs, app pages")),
      ...col(T.sheet, "color", "Panel"), ...tint(T.sheet), ...col(T.sheet, "text", "Text", null, (X = T) => X.sheet.color), ...col(T.sheet, "soft", "Secondary text", null, (X = T) => X.sheet.color)],
    labels: () => [h("div", { class: "grp" }, "POI labels on the wall", h("span", {}, "names and pointers")), ...col(T.labels, "color", "Label"), ...tint(T.labels), ...col(T.labels, "text", "Label text", null, (X = T) => X.labels.color)],
  };
  const groups = { glass: ["glass"], text: ["text", "border", "accent", "accent_text"], buttons: ["buttons"], tags: ["tags"], sheet: ["sheet"], labels: ["labels"] };
  const copy = () => { snap(); only.forEach(g => groups[g].forEach(k => T[k] = JSON.parse(JSON.stringify(O[k])))); changed(); show(); renderEditor(); };
  const subt = only.includes("glass") ? `glass ${T.glass.color} · buttons ${T.buttons.color} · accent ${T.accent}` : only.map(g => g === "tags" ? `tag ${T.tags.color}` : g === "labels" ? `label ${T.labels.color}` : g).join(" · ");
  return fold((opts.id || "theme"), title, { color: mode === "dark" ? "#9AA4B8" : "#E9DFC8", screen: "app", ...(opts.demo ? { step: "tapped", demo: opts.demo } : {}), sub: `${D.looks[activeLook()].name} · ${mode} · ` + subt, onToggle: o => o && show() }, () => [
    ...only.flatMap(g => G[g]()),
    h("div", { class: "addrow", style: "justify-content:flex-end" }, h("button", { class: "ubtn", style: "flex:0 0 auto", title: "Replace these values with the other design's (then adjust)", onclick: copy }, `Copy from ${mode === "dark" ? "light" : "dark"} (this look)`)),
    only.includes("sheet") ? h("p", { class: "guide" }, "The immersive card keeps its photo with a dark scrim in both modes. Unity: one USS theme file per mode, same variable names.") : null,
  ]);
}
