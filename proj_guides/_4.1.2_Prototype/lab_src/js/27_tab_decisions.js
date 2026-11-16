/* ---------- Decisions (Lab only): what is settled, and what is still open ---------- */
const SETTLED = [
  ["1 · Precision", "“Hides ≈ X cm” next to each style + a warning when the measured error is larger.", "precision"],
  ["2 · Line placement", "One “Offset outward” value in cm (0 = on the edge); paths and Points ignore it.", "precision"],
  ["3 · Fill", "None / Tint / Gradient / Pattern now, plus Media: an image or video seen only inside the shape. Try the Twin-tower church door (style “Media window”).", "wall"],
  ["4 · Wall Life kinds", "Six kinds K1–K6, one row type; the kind picks the template and helpers.", "life"],
  ["5 · Triggers", "Reveal / Loop / Focus / Unfocus / Every now; Lapse later.", "life"],
  ["6 · Attention budget", "No rule. The mascot keeps a per-row option “Hide while a card is open” (on by default).", "life"],
  ["7 · Anchor shapes", "Your idea: no special type. An anchor is a normal shape with a quiet style (line opacity 0, no fill); Focus › Appear shows its edge on tap. Try the windows (style “Quiet (appears on tap)”) and tap them in ▶.", "wall"],
  ["8 · Media file", "The style says how (fit, opacity, feather, when a video plays); each shape picks its image or video.", "wall"],
  ["9 · Video playback", "A style option: Loop while visible, or On tap (still frame at rest).", "wall"],
  ["10–12 · Organisation", "Global Scene in 7 groups (Wall & content → Looks → Interaction → Levels → Life → Behaviour → Test); per POI: Identity, Position, Presentation (At rest · Shapes · On tap · Alive), Content for actions, Card; a new Experiences tab.", "wall"],
  ["13 · Tag", "Built from the Detail Card's UI Toolkit blocks, anchored to its POI on the wall.", "wall"],
  ["14 · Tap result", "Every option in the editor (open card · Tag · action buttons · focus only), chosen per level; the developer tries them.", "wall"],
  ["15 · Map, geo-reference, capture & share", "Their places exist in the editor; built later.", "wall"],
  ["16 · Actions", "No action-library section: action types are framework code; each action's settings sit next to its content (Specific Marker), its wall layout in the App tab, its screen layout in the Detail Card; Azu's in the Guide tab.", "app"],
  ["17 · 3D formats", "Any format in the Editor, one runtime form (prefab or GLB).", "wall"],
  ["18 · Label inside Tag", "A label is the simplest Tag (one title block); Tag styles go from a plain label to a card.", "app"],
  ["20 · Wall-wide moments", "In Experiences › Moments (the 1755 earthquake, day to night), started from the App's Explore menu.", "app"],
  ["21 · On tap = named layouts", "A tap shows up to five parts (focus, tag, actions, screen card, ask field); named layouts combine them; each level picks one, a POI may override; App › 4 lists them as foldouts (Unity ReorderableList); opening one plays it on the phone.", "app"],
  ["19 · App tab", "A new first tab composes the app's screens: start, onboarding, main AR screen, a tapped POI, the Explore menu, the look.", "app"],
];
const OPEN = [];
let ANS = {};
try { ANS = JSON.parse(localStorage.getItem("wel_v3") || "{}") || {}; } catch (e) {}
const saveAns = () => { try { localStorage.setItem("wel_v3", JSON.stringify(ANS)); } catch (e) {} };
function decisionsTab() {
  const out = [
    h("div", { class: "lab-intro" },
      h("p", { style: "margin:0 0 6px" }, h("b", {}, "What this Lab covers. "), "The World Design domain (4.1): how a POI is identified and brought to life on the wall itself. G1 Point marker (built in Unity) · G2 Region: shapes, styles, motion (Wall, Style, Precision screens) · G3 Wall Life (Wall Life screen) · G4 Label + Tap (after the Detail Card)."),
      h("p", { style: "margin:0" }, h("b", {}, "Settled 13 Nov. "), "Click a line to open its screen.")),
    h("table", { class: "ftab" }, h("tbody", {}, ...SETTLED.map(([t, txt, scr]) => h("tr", { "data-screen": scr, style: "cursor:pointer" }, h("td", { style: "white-space:nowrap;color:#9EE2B5" }, "✓ " + t), h("td", {}, txt))))),
    h("p", { class: "lab-intro", style: "margin:4px 2px" }, h("b", {}, OPEN.length ? "Open:" : "Nothing open right now.")),
  ];
  for (const d of OPEN) {
    const a = ANS[d.id] || (ANS[d.id] = { pick: "", note: "" });
    out.push(h("div", { class: "dcard" + (a.pick ? " done" : ""), "data-screen": d.screen },
      h("h4", {}, `${d.n} · ${d.title}`, h("span", { class: "grow" }), h("button", { class: "ubtn", onclick: () => { if (d.tab) { ui.tab = d.tab; renderEditor(); } else { setScreen(d.screen); if (!playing) togglePlay(); } } }, d.tab ? "Open " + d.tab.toUpperCase() + " ▸" : "Show ▸")),
      h("p", {}, h("span", { class: "lbl" }, "Feature "), d.feature),
      h("p", {}, d.what),
      ...d.opts.map(([v, l, txt]) => h("label", { class: "opt" },
        h("input", { type: "radio", name: "d_" + d.id, checked: a.pick === v, onchange: () => { a.pick = v; saveAns(); renderEditor(); } }),
        h("span", {}, h("b", {}, l), v === d.rec ? h("span", { class: "rec" }, "RECOMMENDED") : null, h("br"), txt))),
      h("p", {}, h("span", { class: "lbl" }, "Why "), d.why),
      h("textarea", { class: "uin", placeholder: "Your note (optional)", rows: 2, oninput: e => { a.note = e.target.value; saveAns(); } }, a.note || "")));
  }
  const summary = () => OPEN.map(d => { const a = ANS[d.id] || {}; const o = d.opts.find(x => x[0] === a.pick); return `${d.n}. ${d.title}: ${o ? o[1] : "(open)"}${o && a.pick === d.rec ? " (recommended)" : ""}${a.note ? " — " + a.note : ""}`; }).join("\n");
  if (!OPEN.length) return out;
  out.push(h("div", { class: "drow" }, h("span", { class: "grow" }),
    h("button", { class: "ubtn", onclick: () => { for (const d of OPEN) { ANS[d.id] = ANS[d.id] || { note: "" }; ANS[d.id].pick = d.rec; } saveAns(); renderEditor(); } }, "Accept the recommendations"),
    h("button", { class: "ubtn", onclick: async e => { const b = e.currentTarget; try { await navigator.clipboard.writeText(summary()); b.textContent = "Copied ✓"; } catch (_) { b.textContent = "Select the text below"; } } }, "Copy answers")));
  out.push(h("pre", { class: "json", style: "white-space:pre-wrap" }, summary()));
  return out;
}

