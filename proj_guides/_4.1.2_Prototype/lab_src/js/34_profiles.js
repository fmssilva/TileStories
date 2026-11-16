/* ---------- Visitor profiles (v18, _4.1.9) ----------
   The visitor picks two things in onboarding (who's visiting, how long) and optional interests; the phone gives the text size,
   light / dark and the language; Settings can change all of them later. Each editor field belongs to exactly ONE dimension:
     look        (Premium, Playful…)  every design token          D.looks[k].tokens
     look × mode (light | dark)       colours only                 D.looks[k].colours[mode]
     time        (Quick, Classic, Deep) content depth              D.time[k]
     visitors    (Me / us, Children)  tone, mascot, default look   D.visitors[k]
     text size   nothing stored: one multiplier on the type scale
   so storage is the SUM of the dimensions, never their product. A look stores only what differs from its base; the app
   resolves (base → look → mode → text scale) once per profile change into one flat table. Nothing extra runs per frame.
   The editor writes into the profile shown in the profile bar; D.ds and D.themes[mode] are views of the resolved values,
   so every existing renderer and field keeps working and lands in the right place. Unity: one theme asset per look
   (light + dark colour sets → USS variables under a root class), ScriptableObject rows for time and visitors. */

const SCOPE = Symbol("scope");
D.profile = D.profile || { visitors: "adults", time: "classic", interests: [], look: "auto", appearance: null, text: "m", language: "en" };   // appearance null = the app's default
D.visitors = D.visitors || {
  adults:   { name: "Ages 12+ · full museum tour", short: "12+", tone: "warm", look: "premium", mascot: false, games_first: false, kid_text: false, coach: "Tap a glowing building to discover it" },
  children: { name: "Ages 7–11 · playful adventure", short: "7–11", tone: "playful", look: "playful", mascot: true, games_first: true, kid_text: true, coach: "Tap a glowing spot to start the adventure!" },
};
D.time = D.time || {
  quick:   { name: "Quick", minutes: 10, levels: "top2", card: "quick", deep_blocks: false, tour_max: 10, pointers: 2 },
  classic: { name: "Classic", minutes: 30, levels: "all", card: "classic", deep_blocks: false, tour_max: 25, pointers: 3 },
  deep:    { name: "Deep", minutes: 60, levels: "all", card: "deep", deep_blocks: true, tour_max: 60, pointers: 4 },
};
D.app.profiles = D.app.profiles || { interests_emphasis: "halo", follow_phone_text: true };
const TEXT_SCALE = { s: 0.9, m: 1, l: 1.2 };
const LOOK_NEW = { outline_pt: 0, shadow_style: "soft", title_face: "display", marker_preset: "by_level" };   // tokens a look may change; Premium's values are neutral

/* ---- paths ---- */
const getPath = (o, p) => { for (const k of p) { if (o == null || typeof o !== "object") return undefined; o = o[k]; } return o; };
const setPath = (o, p, v) => { for (let i = 0; i < p.length - 1; i++) { if (o[p[i]] == null || typeof o[p[i]] !== "object") o[p[i]] = {}; o = o[p[i]]; } o[p[p.length - 1]] = v; };
const delPath = (o, p) => { const par = getPath(o, p.slice(0, -1)); if (par && typeof par === "object") delete par[p[p.length - 1]]; };
const isObj = v => v && typeof v === "object" && !Array.isArray(v);

/* ---- which look and mode are live ---- */
const activeLook = () => { const k = D.profile.look === "auto" ? D.visitors[D.profile.visitors]?.look : D.profile.look; return D.looks[k] ? k : "premium"; };
const lookChain = k => { const out = []; let L = D.looks[k] || D.looks.premium, n = 0; while (L && n++ < 4) { out.push(L); L = L.base ? (D.looks[L.base] || (L !== D.looks.premium ? D.looks.premium : null)) : null; } return out; };   // a missing base falls back to Premium
const forKids = () => !!D.visitors[D.profile.visitors]?.kid_text;
const timeRow = () => D.time[D.profile.time] || D.time.classic;
const visitorRow = () => D.visitors[D.profile.visitors] || D.visitors.adults;
const textScale = () => D.profile.text === "system" ? (ui.sysText || 1) : TEXT_SCALE[D.profile.text] || 1;

/* ---- the views: reads resolve through the look chain, writes go to the live look (only its difference is stored) ---- */
function view(kind, path, lookKey) {   // kind: "tokens" | "colours:<mode>"
  const store = L => kind === "tokens" ? L.tokens : L.colours?.[kind.slice(8)];
  const chain = () => lookChain(lookKey ? lookKey() : activeLook());
  const read = p => { for (const L of chain()) { const v = getPath(store(L) || {}, p); if (v !== undefined) return v; } return undefined; };
  const keys = () => { const s = new Set(); for (const L of chain()) { const o = getPath(store(L) || {}, path); if (isObj(o)) Object.keys(o).forEach(k => s.add(k)); } return [...s]; };
  return new Proxy({}, {
    get(t, key) {
      if (key === SCOPE) return { kind, path, look: lookKey ? lookKey() : activeLook() };
      if (typeof key === "symbol") return undefined;
      const v = read([...path, key]); return isObj(v) ? view(kind, [...path, key], lookKey) : v;
    },
    set(t, key, val) {
      const L = D.looks[lookKey ? lookKey() : activeLook()]; if (!L) return true;
      if (kind !== "tokens") { L.colours = L.colours || {}; L.colours[kind.slice(8)] = L.colours[kind.slice(8)] || {}; } else L.tokens = L.tokens || {};
      const p = [...path, key], base = L.base ? (() => { for (const B of lookChain(L.base)) { const v = getPath(store(B) || {}, p); if (v !== undefined) return v; } })() : undefined;
      if (L.base && base !== undefined && JSON.stringify(base) === JSON.stringify(val)) delPath(store(L), p);   // back to the base value: no difference kept
      else setPath(store(L), p, isObj(val) ? JSON.parse(JSON.stringify(val)) : val);
      return true;
    },
    has(t, key) { return read([...path, key]) !== undefined; },
    ownKeys() { return keys(); },
    getOwnPropertyDescriptor(t, key) { const v = this.get(t, key); return v === undefined ? undefined : { value: v, enumerable: true, configurable: true, writable: true }; },
  });
}
/* the scope of a field's object, for the editor's tag and dot */
function scopeOf(obj, key) {
  const s = obj && obj[SCOPE]; if (!s) return null;
  if (s.kind === "dim") return { label: s.label, over: false };
  const L = D.looks[s.look], store = s.kind === "tokens" ? L.tokens : L.colours?.[s.kind.slice(8)];
  const own = getPath(store || {}, [...s.path, key]) !== undefined, mode = s.kind === "tokens" ? null : s.kind.slice(8);
  return { label: L.name + (mode ? " · " + mode : ""), look: s.look, mode, own, inherited: !!L.base && !own, base: L.base ? D.looks[L.base].name : null, path: [...s.path, key], kind: s.kind };
}
function resetScoped(sc) { const L = D.looks[sc.look]; snap(); delPath(sc.kind === "tokens" ? L.tokens : L.colours[sc.mode], sc.path); changed(); renderEditor(); renderPhone?.(); }
/* time and visitors rows: the editor edits the option chosen in the profile bar */
function dimView(dim) {
  const rowOf = () => (dim === "time" ? D.time : D.visitors)[D.profile[dim]];
  return new Proxy({}, {
    get(t, key) { if (key === SCOPE) return { kind: "dim", label: rowOf().name }; return rowOf()[key]; },
    set(t, key, v) { rowOf()[key] = v; return true; },
    ownKeys() { return Object.keys(rowOf()); }, getOwnPropertyDescriptor(t, k) { return { value: rowOf()[k], enumerable: true, configurable: true, writable: true }; },
  });
}

/* ---- install: move the v17 design (D.ds, D.themes) into the Premium look once, then expose the views ---- */
function installViews() {
  if (!D.looks) {
    const ds = Object.getOwnPropertyDescriptor(D, "ds")?.value, th = Object.getOwnPropertyDescriptor(D, "themes")?.value;
    const tokens = JSON.parse(JSON.stringify(ds || {}));
    D.looks = {
      premium: { name: "Premium", tokens: { ...LOOK_NEW, ...tokens }, colours: JSON.parse(JSON.stringify(th || {})) },
      // the Playful look (_4.1.9 §5, mk_review/playful_spec): only what differs from Premium; every text pair 7:1 in both modes
      playful: { name: "Playful", base: "premium",
        tokens: { text_size: 1.0625, corner: 24, density: 1.1, icon_stroke: 2.25, selected: "fill", button: { size: 1.15 }, glass: "solid", scheme: "expressive", scrim: 0.88,
          nav: { tabs: "3" }, sheet: { backdrop: 0.45 }, outline_pt: 2, shadow_style: "hard", title_face: "rounded", marker_preset: "point_style_6" },
        colours: { light: {"glass": {"color": "#FFF8EC", "tint": 0.96, "blur": 0}, "text": {"main": "#1B1446", "soft": "#3B3272"}, "border": {"color": "#1B1446", "opacity": 1.0}, "accent": "#2638B8", "accent_text": "#FFFFFF", "buttons": {"color": "#FFFFFF", "tint": 1.0, "icon": "#1B1446", "label": "#FFFFFF", "label_text": "#1B1446"}, "tags": {"color": "#FFFFFF", "tint": 1.0}, "sheet": {"color": "#FFF8EC", "tint": 1.0, "text": "#1B1446", "soft": "#3B3272"}, "labels": {"color": "#FFFFFF", "tint": 0.96, "text": "#1B1446"}}, dark: {"glass": {"color": "#221D45", "tint": 0.97, "blur": 0}, "text": {"main": "#FFF8EC", "soft": "#DCD6F5"}, "border": {"color": "#0B0820", "opacity": 1.0}, "accent": "#FFC83D", "accent_text": "#1B1446", "buttons": {"color": "#2D2760", "tint": 1.0, "icon": "#FFF8EC", "label": "#2D2760", "label_text": "#FFF8EC"}, "tags": {"color": "#2D2760", "tint": 1.0}, "sheet": {"color": "#221D45", "tint": 1.0, "text": "#FFF8EC", "soft": "#DCD6F5"}, "labels": {"color": "#140F33", "tint": 0.94, "text": "#FFFFFF"}} } },
    };
  }
  delete D.ds; delete D.themes;
  Object.defineProperty(D, "ds", { get: () => view("tokens", []), configurable: true, enumerable: false });
  Object.defineProperty(D, "themes", { get: () => ({ light: view("colours:light", []), dark: view("colours:dark", []) }), configurable: true, enumerable: false });
}
installViews();

/* ---- the profile bar: one line above the editor. Look and Light / Dark choose where design rows are saved; Visitors and
   Time choose where the Profiles rows are saved; Text previews only. Every choice also previews on the phone. ---- */
function setProfile(k, v) {
  if (k === "mode") { const def = (() => { const x = D.profile.appearance || D.app.appearance; return x === "follow" ? ui.appearance : x; })(); ui.forceMode = v === def ? null : v; }
  else if (k === "look") D.profile.look = v === visitorRow().look ? "auto" : v;
  else D.profile[k] = v;
  renderEditor(); if (ui.screen === "app") renderPhone(); else renderTapWall?.();
}
function renderProfileBar() {   // one line above the tabs: switch profile from any tab
  const bar = document.getElementById("pbar"); if (!bar) return;
  const seg = (k, cur, opts) => h("div", { class: "pseg" }, ...opts.map(([v, t, tip]) => h("button", { "aria-pressed": String(cur === v), title: tip || "", onclick: () => setProfile(k, v) }, t)));
  const grp = (label, tip, ...kids) => h("div", { class: "pgrp", title: tip }, h("span", { class: "pl" }, label), ...kids.filter(Boolean));
  const lk = activeLook();
  bar.replaceChildren(
    grp("Look", "Design rows save into this look", seg("look", lk, Object.entries(D.looks).map(([k, L]) => [k, L.name, k === visitorRow().look ? "The default look for " + visitorRow().name : ""])),
      D.profile.look === "auto" ? h("span", { class: "pauto", title: "The look follows the audience (Profiles › Audience › Look)" }, "auto") : null),
    grp("Mode", "Colours save into this mode", seg("mode", appearance(), [["light", "Light"], ["dark", "Dark"]])),
    grp("Audience", "Profiles › Audience rows save into this option", seg("visitors", D.profile.visitors, Object.entries(D.visitors).map(([k, V]) => [k, V.short || V.name, V.name]))),
    grp("Time", "Profiles › Time rows save into this option", seg("time", D.profile.time, Object.entries(D.time).map(([k, T]) => [k, T.name, `~${T.minutes} min`]))),
    grp("Text", "Preview only", seg("text", D.profile.text, [["s", "S", "Small (0.9×)"], ["m", "M", "Default"], ["l", "L", "Large (1.2×)"]])));
}

/* ---- icons for the profile choices ---- */
Object.assign(ICONS, {
  people: '<circle cx="9" cy="8.5" r="3"/><path d="M3.5 19c.6-3.2 2.8-5 5.5-5s4.9 1.8 5.5 5"/><circle cx="16.5" cy="9.5" r="2.4"/><path d="M15.5 14.2c2.6-.3 4.6 1.3 5 4.3"/>',
  child: '<circle cx="8" cy="6.5" r="2.6"/><path d="M3.5 19.5c.4-4 2-6.6 4.5-6.6s4.1 2.6 4.5 6.6"/><circle cx="17" cy="12.6" r="1.6"/><path d="M14.6 19.5c.3-2 1.1-3.3 2.4-3.3s2.1 1.3 2.4 3.3"/><path d="M12 14.5l2.6 1.4"/>',
  clock: '<circle cx="12" cy="12" r="8.5"/><path d="M12 7.5V12l3 2"/>', sun: '<circle cx="12" cy="12" r="4"/><path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M5.6 18.4L7 17M17 7l1.4-1.4"/>',
  moon: '<path d="M19 14.5A7.5 7.5 0 019.5 5a7.5 7.5 0 109.5 9.5z"/>', type: '<path d="M5 7V5h14v2M12 5v14M9 19h6"/>', globe: '<circle cx="12" cy="12" r="8.5"/><path d="M3.5 12h17M12 3.5c2.5 2.6 3.5 5.4 3.5 8.5s-1 5.9-3.5 8.5c-2.5-2.6-3.5-5.4-3.5-8.5s1-5.9 3.5-8.5z"/>',
  pin: '<path d="M12 21s-6.5-6-6.5-11a6.5 6.5 0 0113 0c0 5-6.5 11-6.5 11z"/><circle cx="12" cy="10" r="2.3"/>',
  sparkle: '<path d="M12 3.5l1.8 5.2 5.2 1.8-5.2 1.8L12 17.5l-1.8-5.2L5 10.5l5.2-1.8z"/>',
});
const INTEREST = { category_1: ["Royal life", "crown"], category_2: ["Churches", "church"], category_3: ["Military", "tower"], category_4: ["Daily life", "house"], category_5: ["Trade", "anchor"], category_6: ["Bridges and river", "bridge"] };
const mkIcon = k => `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="var(--stroke,1.6)" stroke-linecap="round" stroke-linejoin="round"><path d="${MK_ICONS[k] || ""}"/></svg>`;
const iconEl = k => { const s = h("span", { class: "ic" }); s.innerHTML = mkIcon(k); return s; };

/* ---- what the visitor's choices change on the main AR screen and the card ---- */
const levelIdx = p => D.levels.findIndex(l => l.key === p.level);
function shownByTime(p) { const L = timeRow().levels; return L === "all" || levelIdx(p) < (L === "top1" ? 1 : 2); }   // Quick: the top levels only
const isInterest = p => D.profile.interests.includes(p.category) && D.app.profiles.interests_emphasis !== "off" && D.profile.prefs?.interests !== false;
function storyOf(p) {   // layered text: Quick ⊂ Classic ⊂ Deep; the children's version where written, else the standard one
  const S = p.story || {}, depth = timeRow().card;
  if (forKids() && S.kids) return S.kids;
  return depth === "quick" ? (S.quick || p.summary || "") : depth === "deep" ? [S.classic || p.summary || "", S.deep].filter(Boolean).join(" ") : (S.classic || p.summary || "");
}

/* ---- the visitor's own app preferences (Settings › On the wall, Sound and motion). Stored per visitor, never in the design ---- */
D.profile.prefs = Object.assign({ minimap: "auto", labels: true, pointers: true, interests: true, life: true, sound: true, haptics: true, motion: "phone" }, D.profile.prefs || {});
D.app.onboarding = Object.assign({ ask_look: true, quick: true }, D.app.onboarding);
D.app.settings = Object.assign({ wall: true, sound: true, help: true }, D.app.settings || {});
D.app.soon = D.app.soon || {   // a POI marked on the wall whose story is not written yet (Specific Marker › Content: Coming soon)
  adults:   { badge: "Coming soon", line: "We're still researching this place.", body: "Its story, pictures and 3D are on their way. Save it and we'll show it to you when it's ready." },
  children: { badge: "Coming soon", line: "Azu is still digging up this story!", body: "There's a secret here we haven't found yet. Come back soon and see what we discovered!" },
};
const isSoon = p => !!p && (p.content === "soon" || (!p.summary && !p.story));
const soonText = () => D.app.soon[D.profile.visitors] || D.app.soon.adults;
const prefs = () => D.profile.prefs;
const seen = ui.seen || (ui.seen = new Set());   // places the visitor has opened (the You panel counts them)

/* the two looks as two small fixed pictures (Unity: two sprites), so each card shows its own look whatever look is live */
function lookSample(k) {
  const p = D.pois.find(x => x.id === "twin_tower_church") || D.pois[0];
  if (k === "playful") return h("div", { class: "lks lks-play", "aria-hidden": "true" },
    h("span", { class: "lks-tile" }), h("span", { class: "lks-tile t2" }),
    h("div", { class: "lks-card" }, h("b", {}, "Aa"), h("i"), h("i", { class: "s" })),
    typeof KIDS !== "undefined" ? h("img", { class: "lks-azu", src: KIDS.azu.head, alt: "" }) : null, h("span", { class: "lks-btn" }, "Go!"));
  return h("div", { class: "lks lks-prem", "aria-hidden": "true" }, h("span", { class: "lks-ph", style: poiImage(p, 0.9) }),
    h("span", { class: "lks-pin" }), h("div", { class: "lks-card" }, h("b", {}, "Aa"), h("i"), h("i", { class: "s" })), h("span", { class: "lks-btn" }));
}

/* ---- onboarding (v20): the look first (the page restyles as you pick), who the tour is for (with a live example), how long,
   then optional interests. Light / dark, language and text size sit as small optional icons at the top. One page, skippable. ---- */
function onboardingScreen(go) {
  const O = D.app.onboarding, P = D.profile, root = dsScreenRoot("ds-onb"), rp2 = () => { renderEditor(); renderPhone(); };   // the editor follows: its rows now save into the new look
  const kids = forKids(), lk = activeLook();
  // the small optional tools: each one tap, no wrong answer (the phone's own settings are the default)
  const qb = (icon, label, tip, cb) => h("span", { class: "oq hit", role: "button", tabindex: 0, title: tip, "aria-label": tip, onclick: cb }, h("span", { class: "i" }, svg(icon)), h("small", {}, label));
  const quick = O.quick ? h("div", { class: "oqt", role: "group", "aria-label": "Optional: light or dark, language, text size" },
    qb(appearance() === "dark" ? "moon" : "sun", appearance() === "dark" ? "Dark" : "Light", "Light or dark", () => { P.appearance = appearance() === "dark" ? "light" : "dark"; ui.forceMode = null; rp2(); }),
    O.ask_language ? qb("globe", P.language === "pt" ? "PT" : "EN", "Language", () => { P.language = P.language === "pt" ? "en" : "pt"; rp2(); }) : null,
    qb("type", { s: "Small", m: "Text", l: "Large", system: "Text" }[P.text] || "Text", "Text size", () => { P.text = { s: "m", m: "l", l: "s", system: "l" }[P.text] || "m"; rp2(); })) : null;
  const head = h("div", { class: "ob1" }, h("div", { class: "obtop" }, h("small", { class: "ds-eyebrow" }, "Welcome to the Grande Panorama"), quick),
    h("b", { class: "ds-d" }, kids ? "Ready to explore Lisbon?" : "Make the visit yours"));
  // 1 · the look: two pictures; a tap restyles this very page
  const lkCard = (k, L, sub) => h("div", { class: "ds-opt g hit olk lk-" + k + (lk === k ? " on" : ""), role: "radio", "aria-checked": String(lk === k), onclick: () => { P.look = k; rp2(); } },
    lookSample(k), h("div", { class: "olt" }, h("b", { class: "ds-h2" }, L.name), h("span", { class: "ds-cap" }, sub)), h("span", { class: "ck" }, lk === k ? svg("check") : null));
  const look = O.ask_look ? h("div", { class: "ob2 q-look" }, h("div", { class: "ds-sh" }, "Pick a look"), h("div", { class: "ds-opts two", role: "radiogroup" },
    lkCard("premium", D.looks.premium, "Calm and elegant"), lkCard("playful", D.looks.playful, "Hand-drawn and fun"))) : null;
  // 2 · who the tour is for: two plain buttons, then ONE card that shows the difference in practice
  const aud = (k, icon, t) => h("div", { class: "ds-opt g hit oab" + (P.visitors === k ? " on" : ""), role: "radio", "aria-checked": String(P.visitors === k), onclick: () => { P.visitors = k; rp2(); } },
    h("span", { class: "oi" }, svg(icon)), h("b", { class: "ds-h2" }, t), h("span", { class: "ck" }, P.visitors === k ? svg("check") : null));
  const vis = O.ask_visitors ? h("div", { class: "ob2 q-vis" }, h("div", { class: "ds-sh" }, "Who is this tour for?"),
    h("div", { class: "ds-opts two oaud", role: "radiogroup" }, aud("children", "child", "Ages 7–11"), aud("adults", "people", "Ages 12+")),
    audienceExample(),
    h("p", { class: "ds-cap onote" }, "Exploring together? Pick the youngest. You can switch anytime.")) : null;
  // 3 · how long
  const tcard = (k, T) => h("div", { class: "ds-opt g hit tc" + (P.time === k ? " on" : ""), role: "radio", "aria-checked": String(P.time === k), onclick: () => { P.time = k; rp2(); } },
    h("b", { class: "ds-h2" }, T.name), h("span", { class: "ds-cap" }, T.minutes >= 60 ? `${+(T.minutes / 60).toFixed(T.minutes % 60 ? 1 : 0)} h +` : `~${T.minutes} min`), h("span", { class: "ds-cap tcs" }, k === "quick" ? "Highlights" : k === "classic" ? "Main stories" : "Everything"));
  const shown = D.pois.filter(shownByTime).length;
  const time = O.ask_time ? h("div", { class: "ob2 q-time" }, h("div", { class: "ds-sh" }, "How long do you have?"), h("div", { class: "ds-opts three", role: "radiogroup" }, ...Object.entries(D.time).map(([k, T]) => tcard(k, T))),
    h("p", { class: "ds-cap otm" }, svg("pin"), `${shown} of ${D.pois.length} places on the wall`)) : null;
  // 4 · optional interests
  const chip = (k, [t, ic]) => { const on = P.interests.includes(k); return h("span", { class: "lchip g hit tsel" + (on ? " on" : ""), role: "checkbox", "aria-checked": String(on), onclick: () => { P.interests = on ? P.interests.filter(x => x !== k) : [...P.interests, k]; rp2(); } }, on ? h("span", { class: "ckm" }, svg("check")) : iconEl(ic), t); };
  const ints = O.ask_interests ? h("div", { class: "ob2 q-int" }, h("div", { class: "ds-sh" }, "Anything you love?", h("span", { class: "oopt" }, "optional")), h("div", { class: "kchips" }, ...Object.entries(INTEREST).map(([k, v]) => chip(k, v)))) : null;
  const acts = h("div", { class: "ds-acts" }, dsBtn("Let's go", "main", { size: "l", on: go }), dsBtn("Skip", "text soft", { on: go }));
  const foot = h("p", { class: "ds-cap ofoot" }, "You can change all of this later in Settings.");
  // landscape: two columns (the welcome, the look and who it's for | how long, interests and the buttons)
  root.append(isLand() ? h("div", { class: "ds-onbp cols" }, h("div", { class: "ocol c1" }, head, look, vis), h("div", { class: "ocol c2" }, time, ints, foot, acts))
    : h("div", { class: "ds-onbp" }, head, look, vis, time, ints, foot, acts));   // portrait: the buttons are a sticky footer
  return typeof KF !== "undefined" ? KF.decorateOnboarding(root) : root;   // Playful: Azu greeting + parallax layers
}
/* the one example card under the audience buttons: what this choice feels like on the wall (the guide, the text, what comes first) */
function audienceExample() {
  const p = D.pois.find(x => !isSoon(x)) || D.pois[0], V = visitorRow(), kids = forKids(), col = catColor(p);
  const thumb = h("div", { class: "oex-th", style: typeof KIDS !== "undefined" && activeLook() === "playful" ? KIDS.tileArt(p) : heroStyle(p, 0.1) });
  const said = kids ? (p.story?.kids || storyOf(p)) : storyOf(p);
  return h("div", { class: "oex g" + (kids ? " kid" : ""), "aria-live": "polite" },
    h("small", { class: "ds-eyebrow" }, kids ? "On the wall, ages 7–11 get…" : "On the wall, ages 12+ get…"),
    h("div", { class: "oex-poi" }, thumb, h("div", { class: "oex-tx" }, h("span", { class: "cat", style: `--cc:${col}` }, CATS[p.category]?.label || ""), h("b", {}, p.name))),
    kids ? h("div", { class: "oex-say" }, V.mascot && typeof KIDS !== "undefined" ? KIDS.azuImg("point", "oex-azu") : null, h("p", { class: "bub" }, said))
      : h("p", { class: "ds-body oex-story" }, said),
    h("div", { class: "oex-tags" }, ...(kids ? [["quiz", "Find-it games"], ["sparkle", "Stickers"], ["azu", "Azu the guide"]] : [["tour", "Guided tours"], ["model_wall", "3D and 360°"], ["card", "Sources and dates"]])
      .map(([i, t]) => h("span", { class: "oex-tag" }, svg(i), t))));
}

/* ---- Settings (v20): everything the visitor can change, by area. Opened from the gear next to the search, or from You ---- */
function settingsScreen(close) {
  const P = D.profile, R = prefs(), S = D.app.settings, root = dsScreenRoot("ds-settings"), rp2 = () => { renderEditor(); renderPhone(); };
  const line = (icon, t, ctl, sub) => h("div", { class: "ds-row srow" + (sub ? " two" : "") }, h("span", { class: "i" }, svg(icon)), h("div", { class: "rt" }, h("b", {}, t), sub ? h("span", { class: "rs" }, sub) : null), ctl);
  const stack = (icon, t, ctl, sub) => h("div", { class: "ds-row srow stk" }, h("div", { class: "sth" }, h("span", { class: "i" }, svg(icon)), h("div", { class: "rt" }, h("b", {}, t), sub ? h("span", { class: "rs" }, sub) : null)), ctl);   // a wide control under its title
  const seg = (opts, cur, cb) => dsSeg(opts.map(o => o[1]), Math.max(0, opts.findIndex(o => o[0] === cur)), i => cb(opts[i][0]));
  const sw = (key, cb) => dsSwitch(R[key], () => { R[key] = !R[key]; cb?.(); rp2(); });
  const area = (icon, t, ...rows) => [h("div", { class: "ds-sh sarea" }, svg(icon), t), dsList(...rows.filter(Boolean))];
  const go = st => () => { ui.appStep = st; renderPhone(); };
  // landscape: two columns (your visit, look and reading | the wall, sound, help), same order in the DOM (Unity: two columns in one ScrollView)
  const two = isLand() && (DEVICES[ui.device].kind === "tablet" || activeLook() === "playful");   // v20 F: Premium phone landscape is one column (two left the left one empty at the end of the scroll; Playful's balance)
  const cols = els => { if (!two) return els; const i = els.findIndex((e, k) => k > 0 && e.classList?.contains("sarea") && els.filter((x, j) => j < k && x.classList?.contains("sarea")).length === 2);
    return i < 0 ? els : [h("div", { class: "scol" }, ...els.slice(0, i)), h("div", { class: "scol" }, ...els.slice(i))]; };
  root.append(h("div", { class: "ds-dim d-full" }), h("div", { class: "ds-sheet d-full sheet-set" }, h("span", { class: "grab" }, h("i")),
    h("div", { class: "khd" }, h("span", { class: "khi" }, svg("gear")), h("b", { class: "ds-h1" }, "Settings"), h("span", { class: "ds-x hit", onclick: close }, svg("close"))),
    h("div", { class: "ds-scroll sbody" + (two ? " scols" : "") }, ...cols([
      ...area("pin", "Your visit",
        stack("people", "Tour for", seg([["children", "Ages 7–11"], ["adults", "Ages 12+"]], P.visitors, v => { P.visitors = v; rp2(); })),
        stack("clock", "Time", seg(Object.entries(D.time).map(([k, T]) => [k, T.name]), P.time, v => { P.time = v; rp2(); }), `${timeRow().minutes >= 60 ? "1 h +" : "~" + timeRow().minutes + " min"} · ${D.pois.filter(shownByTime).length} of ${D.pois.length} places`),
        stack("sparkle", "Interests", h("div", { class: "kchips sint" }, ...Object.entries(INTEREST).map(([k, [t, ic]]) => { const on = P.interests.includes(k); return h("span", { class: "lchip g hit tsel" + (on ? " on" : ""), role: "checkbox", "aria-checked": String(on), onclick: () => { P.interests = on ? P.interests.filter(x => x !== k) : [...P.interests, k]; rp2(); } }, on ? h("span", { class: "ckm" }, svg("check")) : iconEl(ic), t); })), "They glow on the wall; nothing is hidden")),
      ...area("type", "Look and reading",
        stack("brush", "Look", seg(Object.entries(D.looks).map(([k, L]) => [k, L.name]), activeLook(), v => { P.look = v; rp2(); })),
        stack(appearance() === "dark" ? "moon" : "sun", "Light or dark", seg([["follow", "Phone"], ["light", "Light"], ["dark", "Dark"]], P.appearance || D.app.appearance, v => { P.appearance = v; ui.forceMode = null; rp2(); })),
        stack("type", "Text size", seg([["system", "Phone"], ["s", "S"], ["m", "M"], ["l", "L"]], P.text, v => { P.text = v; rp2(); })),
        stack("globe", "Language", seg([["en", "English"], ["pt", "Português"]], P.language, v => { P.language = v; rp2(); }))),
      ...(S.wall ? area("grid", "On the wall",
        stack("map", "Wall mini-map", seg([["auto", "Auto"], ["always", "Always"], ["off", "Off"]], R.minimap, v => { R.minimap = v; rp2(); }), "Auto: it appears when you look away from the wall"),
        line("pin", "Names on the wall", sw("labels")),
        line("navigate", "Arrows to places off screen", sw("pointers")),
        line("sparkle", "Highlight my interests", sw("interests")),
        line("play", "Life on the tiles", sw("life", () => { D.test.reduce_motion = !R.life; }), "Birds, trams and little surprises")) : []),
      ...(S.sound ? area("sound", "Sound and motion",
        line("sound", "Sounds", sw("sound")),
        line("vibrate", "Vibration", sw("haptics")),
        stack("motion", "Reduce motion", seg([["phone", "Phone"], ["on", "On"], ["off", "Off"]], R.motion, v => { R.motion = v; D.test.reduce_motion = v === "on" || !R.life; rp2(); }))) : []),
      ...(S.help ? area("info", "Help and about",
        dsRow({ icon: "replay", title: "See the welcome again", on: go("onboarding") }),
        dsRow({ icon: "azu", title: "Show the first tips again", on: () => { ui.coachDone = false; go("main")(); } }),
        dsRow({ icon: "lock", title: "Privacy", sub: "The camera picture never leaves your phone", on: () => {} }),
        dsRow({ icon: "info", title: "About the Grande Panorama", sub: "Museu Nacional do Azulejo · credits", on: () => {} })) : [])]))));
  return root;
}

/* ---- You (v20): the visitor's own panel: who and how (tap to change), what they found, saved places, the album; Settings one row away ---- */
function youScreen(close) {
  const P = D.profile, root = dsScreenRoot("ds-you"), kids = forKids(), go = st => () => { ui.appStep = st; renderPhone(); }, N = D.pois.length;
  const favs = D.pois.filter(p => tapState.favs[p.id]), found = D.pois.filter(p => seen.has(p.id)).length, open = p => () => { ui.appPoi = D.pois.indexOf(p); ui.appStep = "tapped"; tapState.view = null; ui.cam = camCover(camPoseFor(p.pos, 50, 40, 1.2)); renderPhone(); };
  const items = typeof KF !== "undefined" ? KF.SETS.flatMap(x => x.items) : [], got = items.filter(it => KF.got(it)).length;
  const chip = (icon, t) => h("span", { class: "ychip" }, svg(icon), t);
  const stat = (n, of, t, icon) => h("div", { class: "ystat" }, h("span", { class: "yi" }, svg(icon)), h("b", {}, String(n), of != null ? h("small", {}, ` / ${of}`) : null), h("span", {}, t));
  const head = h("div", { class: "yhead g" },
    h("div", { class: "ytop" }, h("span", { class: "yav" }, kids && typeof KIDS !== "undefined" ? h("img", { src: KIDS.azu.head, alt: "" }) : svg("user")),
      h("div", { class: "ytx" }, h("b", { class: "yhi" }, kids ? "Hello, explorer!" : "The Grande Panorama"), h("span", { class: "ysub" }, kids ? "Lisbon before the big earthquake" : "Lisbon before 1755 · your visit today"))),
    h("div", { class: "ychips hit", role: "button", "aria-label": "Change in Settings", onclick: go("settings") }, chip(kids ? "child" : "people", visitorRow().short || ""), chip("clock", timeRow().name), chip("brush", D.looks[activeLook()].name), h("span", { class: "ychg" }, "Change")),
    h("div", { class: "ystats" }, stat(found, N, "Found", "pin"), stat(favs.length, null, kids ? "Hearts" : "Saved", "favourite"), stat(got, items.length || null, kids ? "Stickers" : "Album", "grid")),
    h("div", { class: "ybar", role: "progressbar", "aria-valuenow": String(found), "aria-valuemax": String(N) }, h("i", { style: `width:${Math.round(found / N * 100)}%` })));
  const saved = [h("div", { class: "ds-sh" }, kids ? "My favourite places" : "Saved places"),
    favs.length ? dsList(...favs.map(p => dsRow({ icon: "favourite", title: p.name, sub: CATS[p.category]?.label, on: open(p) })))
      : h("div", { class: "yempty g" }, h("span", { class: "yei" }, svg("favourite")), h("div", {}, h("b", {}, kids ? "No favourites yet" : "Nothing saved yet"), h("span", {}, kids ? "Tap the ♥ on a place to keep it here." : "Tap ♥ on any place on the wall to keep it here.")))];
  const strip = h("span", { class: "ystrip" }, ...items.slice(0, 5).map(it => KF.got(it) ? h("i", { style: poiImage(KF.poi(it[0]), 0.1) }) : h("i", { class: "e" })));
  const coll = [h("div", { class: "ds-sh" }, kids ? "My collection" : "Your collection"),
    dsList(h("div", { class: "ds-row hit yalb", onclick: go("album") }, h("div", { class: "tt" }, h("b", {}, "Tile album"), h("small", {}, kids ? `${got} of ${items.length} stickers found` : `${got} of ${items.length} details found on the wall`), strip), h("span", { class: "tr" }, svg("chev"))))];
  const set = [h("div", { class: "ds-sh" }, "Settings"), dsList(dsRow({ icon: "gear", title: "Settings", sub: "Your visit, look, text, the wall, sound", on: go("settings") }))];
  root.append(h("div", { class: "ds-dim d-full" }), h("div", { class: "ds-sheet d-full sheet-you" }, h("span", { class: "grab" }, h("i")),
    h("div", { class: "khd" }, h("b", { class: "ds-h1" }, kids ? "Your adventure" : "Your visit"), h("span", { class: "ds-x hit", onclick: close }, svg("close"))),
    isLand() ? h("div", { class: "ds-scroll sbody scols" }, h("div", { class: "scol" }, head, ...set), h("div", { class: "scol" }, ...saved, ...coll))
      : h("div", { class: "ds-scroll sbody" }, head, ...saved, ...coll, ...set)));
  return root;
}
Object.assign(ICONS, {
  gear: '<circle cx="12" cy="12" r="3"/><path d="M12 2.8l1.6 2.3 2.7-.7.7 2.7 2.6 1-.9 2.6 1.6 2.3-2.3 1.6.3 2.8-2.8.3-1.6 2.3-2.6-1.2-2.6 1.2-1.6-2.3-2.8-.3.3-2.8-2.3-1.6 1.6-2.3-.9-2.6 2.6-1 .7-2.7 2.7.7z"/>',
  brush: '<path d="M14.5 4.5l5 5-8 8-5-5z"/><path d="M6.5 12.5c-2 .5-3 2-3 4.5v2.5h2.5c2.5 0 4-1 4.5-3"/>',
  sound: '<path d="M4 9.5h3.5L12 5.5v13l-4.5-4H4z"/><path d="M15.5 9a4 4 0 010 6M18 6.5a7.5 7.5 0 010 11"/>',
  vibrate: '<rect x="8" y="3.5" width="8" height="17" rx="2"/><path d="M4.5 8v8M19.5 8v8M2 10v4M22 10v4"/>',
  motion: '<path d="M4 12h9M4 7h5M4 17h7"/><circle cx="17.5" cy="12" r="3"/>',
  replay: '<path d="M4.5 12a7.5 7.5 0 112.2 5.3"/><path d="M4 7.5V12h4.5"/>',
  lock: '<rect x="5" y="10.5" width="14" height="10" rx="2"/><path d="M8 10.5V8a4 4 0 018 0v2.5"/>',
  info: '<circle cx="12" cy="12" r="8.5"/><path d="M12 11v5.5M12 7.6v.4"/>',
});

/* ---- App › 0 Profiles: the dimensions' own rows. Each row edits the option chosen in the profile bar ---- */
function profilesBody() {
  const V = dimView("visitors"), T = dimView("time"), rp2 = () => { renderEditor(); renderPhone(); };
  const toTap = () => { runDemo({ step: "tapped", preview: null }, true); rp2(); }, toMain = () => { ui.appStep = "main"; rp2(); };
  const toWide = () => { ui.appStep = "main"; ui.compPreview = null; ui.cam = camCover({ ...CAM0, z: 1 }); rp2(); };   // the whole panel: every POI (and its interest glow) in view
  const toSheet = () => { ui.previewLayout = "tap_layout_5"; ui.appStep = "tapped"; ui.appPoi = D.pois.findIndex(p => p.id === "castle_hill"); tapState.view = null; ui.cam = camCover(camPoseFor(D.pois[ui.appPoi].pos, 50, 35, 1.1)); rp2(); };   // a screen card, where the deep-dive row sits
  const toOnb = () => { ui.appStep = "onboarding"; rp2(); };
  const live2 = (f, cb) => id => { const el = f(id); el.addEventListener("input", cb); el.addEventListener("change", cb); return el; };
  const sub = (id, title, subt, demo, body) => fold(id, title, { color: "#F2C27A", screen: "app", step: demo.step, demo, sub: subt }, body);
  return [
    h("p", { class: "guide" }, "What the visitor chooses (onboarding, then Settings) and what each choice changes. Every row here is saved into the option chosen in the profile bar: switch Visitors or Time there to edit the other option. Design rows (the rest of the App tab) are saved into the bar's Look and, for colours, Light / Dark."),
    sub("prof_vis", "Visitors", `editing: ${visitorRow().name}`, { step: "main" }, () => [
      ...row("Shown as", live2(fText(V, "name"), toOnb), "The title of this choice on the onboarding card."),
      ...row("Look", live2(fSelect(V, "look", Object.entries(D.looks).map(([k, L]) => [k, L.name])), rp2), "The look this choice opens with (the visitor's Settings › Look = Auto). Children → Playful."),
      ...row("Children's text", live2(fToggle(V, "kid_text"), toTap), "Cards use each POI's children's version where it is written (Specific Marker › Story); otherwise the standard text."),
      ...row("Azu the mascot", live2(fToggle(V, "mascot"), () => { mainDemo("coach"); }), "The mascot joins the coach mark and the tips."),
      ...row("Games first", live2(fToggle(V, "games_first"), () => { ui.appStep = "explore"; rp2(); }), "The Explore menu lists games before tours."),
      ...row("Coach line", live2(fText(V, "coach"), () => mainDemo("coach")), "The first-visit hint on the POI nearest the centre."),
      ...(D.profile.visitors === "children" ? row("Preview", id => h("div", { id, class: "btnrow" }, h("button", { class: "ubtn", onclick: () => { ui.appStep = "main"; rp2(); setTimeout(() => KF.celebrate(D.pois.find(p => p.id === "castle_hill") || D.pois[0]), 60); } }, "Sticker earned"), h("button", { class: "ubtn", onclick: () => { ui.appStep = "album"; rp2(); } }, "Tile album")), "Ages 7–11: a find on the wall earns a sticker (≤ 1.5 s, never a takeover); the album fills tile by tile. No points, streaks or leaderboards.") : []),
    ]),
    sub("prof_time", "Time", `editing: ${timeRow().name}`, { step: "main" }, () => [
      ...row("Shown as", live2(fText(T, "name"), toOnb), "The title of this choice on the onboarding card and in Settings."),
      ...row("About", live2(fRange(T, "minutes", 5, 90, 5, " min"), () => { ui.appStep = "onboarding"; rp2(); }), "Shown on the onboarding card."),
      ...row("POIs at rest", live2(fSelect(T, "levels", [["top1", "The top level only (Hub)"], ["top2", "The two top levels (Hub, Landmark)"], ["all", "Every level"]]), toMain), "Which hierarchy levels show on the main AR screen and in Play. A tapped POI always shows."),
      ...row("Card text", live2(fSelect(T, "card", [["quick", "Quick (one line)"], ["classic", "Classic (the main story)"], ["deep", "Deep (main + extras)"]]), toTap), "Which layer of each POI's story the card shows. Layers add up: Quick ⊂ Classic ⊂ Deep."),
      ...row("Deep-dive block", live2(fToggle(T, "deep_blocks"), toSheet), "A “Deep dive” row on the card: sources, dates and more stories."),
      ...row("Pointers at most", live2(fRange(T, "pointers", 0, 6, 1), () => mainDemo("away")), "Pointers to POIs off the screen (with Main AR screen › Pointing: the smaller of the two)."),
      ...row("Longest tour", live2(fRange(T, "tour_max", 5, 60, 5, " min"), () => { ui.appStep = "explore"; rp2(); }), "Tours longer than this are left out of Explore."),
    ]),
    sub("prof_int", "Interests", D.profile.interests.length ? D.profile.interests.map(k => INTEREST[k]?.[0]).join(" · ") : "none chosen", { step: "main" }, () => [
      ...row("Emphasis", live2(fSelect(D.app.profiles, "interests_emphasis", [["halo", "A glow and a ★ on the marker, and first among pointers"], ["off", "Off"]]), toWide), "Interests never hide anything. Choose some on the phone (onboarding or Settings) to see it."),
      ...row("Preview", id => h("div", { id, class: "seg" }, ...Object.entries(INTEREST).map(([k, [t]]) => h("button", { "aria-pressed": String(D.profile.interests.includes(k)), onclick: () => { D.profile.interests = D.profile.interests.includes(k) ? D.profile.interests.filter(x => x !== k) : [...D.profile.interests, k]; toWide(); } }, t)))),
    ]),
  ];
}
