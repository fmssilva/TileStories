/* ---------- App › 3 Main AR screen: the visitor's screen while looking at the wall ----------
   Parts, top to bottom, each optional: search · lenses (category chips) · wall mini-map · "near" chip ·
   coach mark on a POI · hint when the wall is out of view · buttons (a floating bar or the corners) with Azu.
   Pointing (off-screen POIs) is a subsection; it draws its own pointers. Unity: one UI Toolkit screen over the camera. */
Object.assign(ICONS, {
  search: '<circle cx="11" cy="11" r="6.5"/><path d="M20 20l-4.2-4.2"/>',
  mic: '<rect x="9" y="3" width="6" height="11" rx="3"/><path d="M5.5 11a6.5 6.5 0 0013 0M12 17.5V21"/>',
  sliders: '<path d="M4 7h10M18 7h2M4 17h4M12 17h8"/><circle cx="16" cy="7" r="2"/><circle cx="10" cy="17" r="2"/>',
  grid: '<rect x="4" y="4" width="6.5" height="6.5" rx="1.5"/><rect x="13.5" y="4" width="6.5" height="6.5" rx="1.5"/><rect x="4" y="13.5" width="6.5" height="6.5" rx="1.5"/><rect x="13.5" y="13.5" width="6.5" height="6.5" rx="1.5"/>',
  user: '<circle cx="12" cy="8.5" r="3.8"/><path d="M4.5 20.5c1.2-3.8 4.2-5.6 7.5-5.6s6.3 1.8 7.5 5.6"/>',
  scan: '<path d="M4 9V5.5A1.5 1.5 0 015.5 4H9M15 4h3.5A1.5 1.5 0 0120 5.5V9M20 15v3.5a1.5 1.5 0 01-1.5 1.5H15M9 20H5.5A1.5 1.5 0 014 18.5V15"/><path d="M4 12h16"/>',
  azu: '<path d="M12 3.5l1.9 4.9 5.1.4-3.9 3.3 1.2 5-4.3-2.7-4.3 2.7 1.2-5L5 8.8l5.1-.4z"/>',
});
D.app.main = {
  search: { at: "top", style: "pill", voice: true, filter: true, gear: true },   // gear: the Settings button left of the search
  lenses: { at: "filter" },          // behind the filter button of the search
  minimap: { at: "lost", dots: true },   // shown only while the wall is out of view
  near: "search",                    // in the search placeholder
  buttons: { arrange: "bar", azu: "mascot", explore: true, profile: "bar", items: ["explore", "azu", "you"] },   // items: the action bar, in order (2–5)
  hints: { tracking: true, coach: true },
};
ui.lens = ui.lens || "all";
/* the action bar's catalogue: what each button is and where it goes (Unity: one ScriptableObject row each) */
const BAR = {
  explore: { icon: "grid", label: "Explore", step: "explore", tip: "Tours, games, moments" },
  azu:     { icon: "azu", label: "Azu", tip: "The guide: ask anything (the Guide tab)" },
  you:     { icon: "user", label: "You", step: "you", tip: "Saved places, what you found, Settings" },
  tours:   { icon: "tour", label: "Tours", step: "explore", tip: "Straight to the tours" },
  play:    { icon: "quiz", label: "Play", step: "explore", tip: "Straight to the games" },
  saved:   { icon: "favourite", label: "Saved", step: "you", tip: "The places the visitor saved" },
  map:     { icon: "map", label: "Map", tip: "Shows the wall mini-map" },
  settings:{ icon: "gear", label: "Settings", step: "settings", tip: "Settings (when the gear is not by the search)" },
};
D.app.main.buttons.items = D.app.main.buttons.items || ["explore", "azu", "you"];

/* the wall's area a POI belongs to: the nearest POI to the centre of the screen, if it is close enough */
function nearPoi() {
  const r = dev.scr.getBoundingClientRect(), c = unproject(r.left + r.width / 2, r.top + r.height / 2);
  if (c.x < 0 || c.x > IW || c.y < 0 || c.y > IH) return null;
  let best = null, bd = 1e9; D.pois.forEach(p => { const d = Math.hypot(p.pos.x - c.x, p.pos.y - c.y); if (d < bd) { bd = d; best = p; } });
  return bd < IW * 0.22 ? best : null;
}
function wallAway() { const r = dev.scr.getBoundingClientRect(), c = unproject(r.left + r.width / 2, r.top + r.height / 2); return c.x < -IW * 0.05 || c.x > IW * 1.05 || c.y < -IH * 0.05 || c.y > IH * 1.05; }   // the phone points away from the panorama
const lensOk = p => ui.lens === "all" || p.category === ui.lens;
/* the Main AR rows whose effect needs a state show it: the wall out of view, the first visit, a tapped POI off the screen */
function mainDemo(kind) {
  setScreen("app"); ui.compPreview = null; ui.previewLayout = null; tapState.view = null; if (kind !== "coach") ui.coachDone = true;   // the first-visit coach mark hides pointers and labels: other demos dismiss it
  if (kind === "away") { ui.appStep = "main"; ui.cam = { ...camCover({ ...CAM0 }), cx: 1.9 }; }
  if (kind === "coach") { ui.appStep = "main"; ui.coachDone = false; ui.cam = camCover({ ...CAM0, z: 1.1 }); }
  if (kind === "offsel") { ui.appPoi = 0; ui.appStep = "tapped"; const p = D.pois[0]; ui.cam = camCover(camPoseFor(p.pos, 50, 45, 1.3, 0, 0, 0)); ui.cam.cx += 0.5 * dev.SW / camSize(ui.cam).imgW + 0.06; }
  renderPhone();
}
const demoToggle = (obj, key, kind) => id => h("input", { id, type: "checkbox", checked: !!obj[key], style: "justify-self:start", onchange: e => { snap(); obj[key] = e.target.checked; renderEditor(); mainDemo(kind); }, onfocus: () => mainDemo(kind) });

/* builds the main screen's chrome: a .tapc container (phone units, the shared glass and appearance) */
function mainChrome() {
  const M = D.app.main, kids = forKids(), go = st => () => { ui.appStep = st; tapState.view = null; renderPhone(); };
  const root = h("div", { class: "tapc mainc" + audCls() + (PHR > 1 ? " land" : ""), style: `--u:${dev.U}px` }); applyGlass(root);
  const S = M.search, ic = k => h("span", { class: "mi" }, svg(k));
  const away = wallAway();
  const np = M.near !== "off" && M.near !== false && !away ? nearPoi() : null;
  const filterBtn = S.filter ? h("span", { class: "mi hit" + (ui.lensOpen ? " on" : ""), title: "Filter by category", "aria-label": "Filter by category", onclick: () => { ui.lensOpen = !ui.lensOpen; renderPhone(); } }, svg("sliders")) : null;
  const search = S.at === "off" ? null : h("div", { class: `msearch g hit ${S.style}` }, ic("search"),
    h("span", { class: "ph1" }, np && M.near === "search" ? [h("i", {}, "Near "), np.name] : kids ? "Find a place…" : "Search a place or ask Azu…"),
    S.voice ? ic("mic") : null, filterBtn);
  const showLens = M.lenses.at !== "off" && (M.lenses.at !== "filter" || ui.lensOpen || ui.lens !== "all");
  const lenses = !showLens ? null : h("div", { class: "mlens" }, ...[["all", "All", null], ...Object.entries(CATS).map(([k, c]) => [k, c.label, c.color])].map(([k, label, col]) =>
    h("span", { class: "lchip g hit" + (ui.lens === k ? " on" : ""), onclick: () => { ui.lens = k; renderPhone(); } }, col ? h("i", { style: `background:${col}` }) : null, label)));
  const mmAt = prefs().minimap === "off" ? "off" : prefs().minimap === "always" || ui.mapOpen ? (M.minimap.at === "top" ? "top" : "bottom") : M.minimap.at;   // the visitor's Settings › Wall mini-map, or the Map button, win
  const mm = mmAt === "off" || (mmAt === "lost" && !away) ? null : minimap();
  const near = np && (M.near === "chip" || M.near === true) ? h("div", { class: "mnear g" }, h("i", { style: `background:${catColor(np)}` }), h("span", {}, "Near "), h("b", {}, np.name)) : null;
  // buttons
  const B = M.buttons, azuEl = B.azu === "off" ? null : h("div", { class: "mazu hit" + (B.azu === "mascot" ? " mascot" : " g"), title: "Ask Azu" }, svg("azu"));
  const itemEl = k => { const I = BAR[k]; if (!I) return null;
    if (k === "azu") return azuEl;
    if (k === "explore" && !B.explore) return null;
    if (k === "you" && B.profile !== "bar") return null;
    if (k === "settings" && S.gear && S.at !== "off") return null;   // the gear by the search already opens Settings
    return h("div", { class: "mbtn hit b-" + k + (k === "map" && ui.mapOpen ? " on" : ""), title: I.tip, onclick: k === "map" ? () => { ui.mapOpen = !ui.mapOpen; renderPhone(); } : go(I.step) }, svg(I.icon), h("span", {}, I.label)); };
  const items = (B.items || ["explore", "azu", "you"]).slice(0, 5).map(itemEl).filter(Boolean);
  const exploreEl = itemEl("explore");
  let bar = null; const corners = { tl: null, tr: null, bl: null, br: null };
  if (B.arrange === "bar") bar = h("div", { class: "mbar g n" + items.length + (PHR > 1 ? " rail" : "") }, ...items);
  else { corners.bl = exploreEl ? h("div", { class: "mcorner g" }, exploreEl) : null; corners.br = azuEl; }
  if (B.profile === "corner") corners.tr = h("div", { class: "mavatar g hit", onclick: go("you") }, svg("user"));
  // tips from Azu: the coach mark's caption sits next to the mascot when it is the first visit
  const lensTop = S.at === "top" && M.lenses.at !== "bottom", lensBot = !lensTop;
  const gear = S.gear && S.at !== "off" ? h("span", { class: "mgear g hit", title: "Settings", "aria-label": "Settings", onclick: go("settings") }, svg("gear")) : null;
  const srow = s => gear ? h("div", { class: "msrow s-" + S.style }, gear, s) : s;   // the gear sits left of the search (same height), wherever the search is
  const top = h("div", { class: "mtop" }, S.at === "top" ? srow(search) : null, lensTop ? lenses : null, near);
  const rail = PHR > 1 && bar;   // landscape: the bar is a vertical rail on the trailing edge, outside the bottom stack
  const bot = h("div", { class: "mbot" + (B.arrange === "corners" ? " above" : "") }, lensBot ? lenses : null, S.at === "bottom" ? srow(search) : null, rail ? null : bar);
  if (rail) root.append(bar);
  if (mm) { mm.classList.add(mmAt === "lost" ? "lost" : mmAt); root.append(mm); }   // the mini-map is a small framed thumbnail in a corner (top or bottom right)
  root.append(top, bot);
  Object.entries(corners).forEach(([k, el]) => el && root.append(h("div", { class: "mc " + k }, el)));
  // hint: the wall is out of view
  root.classList.toggle("away", away);
  if (M.hints.tracking && away) root.append(h("div", { class: "mhint g rise" }, h("span", { class: "scan" }, svg("scan")), h("b", {}, "Point your phone at the tiles"), h("small", {}, "The panorama is behind you or to the side")));
  // coach mark on the POI nearest the centre (first visit only)
  if (M.hints.coach && !ui.coachDone && !away) {
    const p = np || D.pois.map(p => ({ p, q: camProject(p.pos.x, p.pos.y) })).filter(o => onScreen(o.q)).sort((a, b) => Math.hypot(a.q.x - 50, a.q.y - 50) - Math.hypot(b.q.x - 50, b.q.y - 50))[0]?.p;
    const q = p && camProject(p.pos.x, p.pos.y);
    if (q && onScreen(q)) { dev.scr.classList.add("coaching"); dev.scr.querySelectorAll(".mk").forEach(m => m.classList.toggle("coach", D.pois[+m.dataset.i] === p)); }
    if (q && onScreen(q)) root.append(h("div", { class: "mcoach", style: `left:${q.x}%;top:${q.y}%` }, h("span", { class: "rg" }), h("span", { class: "rg d2" })),
      h("div", { class: "mtip g " + (q.y > 30 ? "up" : "down") + (visitorRow().mascot ? " has-az" : ""), style: `left:${clamp(q.x, 34, 66)}%;--tail:${(q.x - clamp(q.x, 34, 66)) / 100 * dev.SW}px;${q.y > 30 ? `bottom:calc(${100 - q.y}% + 10 * var(--u))` : `top:calc(${q.y}% + 10 * var(--u))`}` }, visitorRow().mascot ? h("span", { class: "mi az" }, activeLook() === "playful" && typeof KIDS !== "undefined" ? KIDS.azuImg("point", "az-img") : svg("azu")) : null, visitorRow().coach));
  }
  return root;
}
/* the whole panorama as a strip: the camera's view is the bright window, POIs are dots; tap the strip to look there */
function minimap() {
  const r = dev.scr.getBoundingClientRect(), pts = [[0, 0], [r.width, 0], [r.width, r.height], [0, r.height]].map(([x, y]) => unproject(r.left + x, r.top + y));
  const xs = pts.map(p => clamp(p.x / IW * 100, -5, 105)), ys = pts.map(p => clamp(p.y / IH * 100, -5, 105));
  const vw = h("div", { class: "vw", style: `left:${Math.min(...xs)}%;top:${Math.min(...ys)}%;width:${Math.max(...xs) - Math.min(...xs)}%;height:${Math.max(...ys) - Math.min(...ys)}%` });
  const el = h("div", { class: "mmap g hit", onclick: e => { const b = e.currentTarget.getBoundingClientRect(); camTo({ ...ui.cam, cx: (e.clientX - b.left) / b.width, cy: (e.clientY - b.top) / b.height, yaw: 0, pitch: 0 }); } },
    h("div", { class: "pic", style: `background-image:url(${SRC})` }, h("div", { class: "shade" }), vw,
      ...(D.app.main.minimap.dots ? D.pois.map(p => h("i", { class: lensOk(p) ? "" : "dim", style: `left:${p.pos.x / IW * 100}%;top:${p.pos.y / IH * 100}%;background:${catColor(p)}` })) : [])));
  return el;
}

/* the editor: App › 3 Main AR screen (Pointing is its subsection) */
function mainBody() {
  const M = D.app.main, re = () => renderPhone();
  const sel = (obj, key, opts) => fSelect(obj, key, opts, re);
  return [
    h("p", { class: "guide" }, "What the visitor sees while looking at the wall. The wall itself (points, regions, tags, Wall Life) comes from Global Scene and each POI; light or dark and the glass come from App › On tap › Base elements."),
    h("div", { class: "grp" }, "Search", h("span", {}, "find a place, or ask Azu")),
    ...row("Search", sel(M.search, "at", [["top", "At the top"], ["bottom", "At the bottom"], ["off", "Off"]])),
    ...(M.search.at !== "off" ? [...row("Shape", sel(M.search, "style", [["pill", "Compact pill"], ["bar", "Full-width bar"]]), null, true), ...row("Voice", fToggle(M.search, "voice"), null, true), ...row("Filter button", fToggle(M.search, "filter"), "Opens the category filter.", true), ...row("Settings gear", fToggle(M.search, "gear"), "A round gear button left of the search: opens Settings (App › 7).", true)] : []),
    h("div", { class: "grp" }, "Category filter", h("span", {}, "chips that filter the wall by category")),
    ...row("Category filter", sel(M.lenses, "at", [["filter", "Behind the filter button"], ["search", "Always, under the search"], ["bottom", "Always, above the buttons"], ["off", "Off"]]), "A row of category chips (Churches, Royal life…); the chosen one dims the other POIs on the wall and in the mini-map. “Behind the filter button” keeps the screen calm until the visitor asks."),
    h("div", { class: "grp" }, "Where am I", h("span", {}, "the visitor's place on a 20 m panel")),
    ...row("Wall mini-map", fSelect(M.minimap, "at", [["lost", "Only when the wall is out of view"], ["bottom", "Always, bottom corner"], ["top", "Always, top corner"], ["off", "Off"]], () => M.minimap.at === "lost" ? mainDemo("away") : renderPhone()), "The whole panorama as a thumbnail: a bright window shows what the camera sees; tap it to turn there."),
    ...(M.minimap.at !== "off" ? row("POI dots on the map", M.minimap.at === "lost" ? demoToggle(M.minimap, "dots", "away") : fToggle(M.minimap, "dots"), null, true) : []),
    ...row("“Near …”", sel(M, "near", [["search", "In the search placeholder"], ["chip", "A chip under the search"], ["off", "Off"]]), "The POI closest to the centre of the screen: “Near Twin-tower church”."),
    fold("app_bar", "Action bar · the main buttons", { color: "#9FD7C1", screen: "app", step: "main", demo: { step: "main", preview: null, view: null }, sub: barSummary() }, actionBarBody),
    h("div", { class: "grp" }, "Help", h("span", {}, "first visit, lost tracking")),
    ...row("Coach mark", demoToggle(M.hints, "coach", "coach"), "First visit: the POI nearest the centre glows and Azu says “Tap a glowing building”. Gone after the first tap."),
    ...row("Hint when the wall is out of view", demoToggle(M.hints, "tracking", "away"), "When the phone points away from the panorama: “Point your phone at the tiles”. Try it: drag the picture far to one side."),
    h("div", { class: "grp" }, "Wall labels", h("span", {}, "the POI names and pointers on the wall, per design")),
    colourFold("Label colours", { only: ["labels"], id: "lblcol" }),
    fold("app_point", "Pointing · off-screen POIs", { color: "#9FD7C1", screen: "app", step: "main", sub: pointingSummary() }, pointingBody),
  ];
}
/* App › 4 Main AR screen › Action bar: which buttons, in which order (2–5), and how they sit */
function barSummary() { const B = D.app.main.buttons; return B.arrange === "corners" ? "in the corners" : (B.items || []).map(k => BAR[k]?.label).filter(Boolean).join(" · "); }
function actionBarBody() {
  const B = D.app.main.buttons, re = () => { renderEditor(); ui.appStep = "main"; mainDemo(); }, items = B.items;
  const mv = (i, d) => () => { snap(); const j = i + d; [items[i], items[j]] = [items[j], items[i]]; changed(); re(); };
  const list = h("div", { class: "barlist" }, ...items.map((k, i) => h("div", { class: "barrow" }, h("span", { class: "bn" }, String(i + 1)), h("b", {}, BAR[k]?.label || k), h("small", {}, BAR[k]?.tip || ""),
    h("button", { class: "ib", title: "Move up (further left in the bar)", disabled: i === 0, onclick: mv(i, -1) }, "↑"), h("button", { class: "ib", title: "Move down (further right in the bar)", disabled: i === items.length - 1, onclick: mv(i, 1) }, "↓"),
    h("button", { class: "ib", title: items.length <= 2 ? "The bar keeps at least 2 buttons" : "Remove", disabled: items.length <= 2, onclick: () => { snap(); items.splice(i, 1); changed(); re(); } }, "−"))));
  const free = Object.keys(BAR).filter(k => !items.includes(k));
  const add = id => h("select", { id, disabled: items.length >= 5 || !free.length, onchange: e => { if (!e.target.value) return; snap(); items.push(e.target.value); changed(); re(); } },
    h("option", { value: "" }, items.length >= 5 ? "The bar is full (5)" : "+ Add a button…"), ...free.map(k => h("option", { value: k }, `${BAR[k].label} · ${BAR[k].tip}`)));
  return [
    h("p", { class: "guide" }, "The bar at the bottom of the camera screen (a rail on the side in landscape): 2 to 5 buttons, left to right. Three is the calm default; the middle one is raised when it is Azu. Every button stays ≥ 48 pt with its label."),
    ...row("Arrange", fSelect(B, "arrange", [["bar", "A floating bar at the bottom"], ["corners", "In the corners (Explore and Azu only)"]], re)),
    ...(B.arrange === "bar" ? [...row("Buttons", () => list, `${items.length} of at most 5`), ...row("Add", add)] : []),
    ...row("Azu", fSelect(B, "azu", [["mascot", "Mascot button (raised, with tips)"], ["button", "Plain button"], ["off", "Off"]], re)),
    ...row("Explore", fToggle(B, "explore")),
    ...row("You", fSelect(B, "profile", [["bar", "In the bar"], ["corner", "Avatar, top corner"], ["explore", "Inside Explore"]], re), "The visitor's own panel: saved places, what they found, the album and Settings."),
  ];
}
function pointingSummary() { const P = D.app.pointing; return P.others === "off" ? "selected POI only" : P.others === "all" ? "all off-screen POIs" : `the best ${P.max}`; }
