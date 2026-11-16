/* ===== v19 flows (group E, integrated): Azu greeting, onboarding parallax, sticker earned, the tile album. Playful only. CSS: css/kids_50_flows.css ===== */
const KF = {
  /* the album's sets: one per AREA of the wall, each tile a detail you find by looking. [poiId] = a POI's tile; [null, hint, doodle] = a find
     that is not a POI yet (content: Experiences › Games › Find it). Unity: a ScriptableObject list (set → items). */
  SETS: [
    { name: "Up on the hill", where: "The castle and its gate", doodle: "compass", story: "Hear the hill's story", items: [["castle_hill"], ["arched_gate"]] },
    { name: "Bells and domes", where: "Churches along the river", doodle: "rosette", story: "Hear the bells' story", items: [["twin_tower_church"], ["domed_church"], [null, "A bell tower with seven bells", "star"], [null, "A convent by the water", "rosette"]] },
    { name: "On the river", where: "Ships, boats and the tide mill", doodle: "ship", story: "Hear the river's story", items: [[null, "A ship with three sails", "ship"], [null, "A rowing boat", "wave"], [null, "Fishermen with nets", "swirl"], [null, "The tide mill", "compass"]] },
  ],
  found: () => (ui.kidsFound ||= new Set(D.pois.map(p => p.id))),   // Lab: every POI counts as found; the app: filled by the find games
  got: it => !!it[0] && KF.found().has(it[0]),
  poi: id => D.pois.find(p => p.id === id),

  /* §1 Azu greets beside the onboarding title, stepping out of a tile. Plays its entrance once per session (one motion). */
  hello() {
    const first = !ui.kfAzuSeen; ui.kfAzuSeen = true;
    return h("div", { class: "ob-hello", "aria-hidden": "true" }, h("span", { class: "ob-tile k-frame k-panel-b" }),
      KIDS.azuImg("wave", first ? "enter" : ""), h("span", { class: "ob-bub k-frame k-chip" }, "Olá!"));
  },
  /* §2 parallax: three layers behind the onboarding content (0 = the tile texture, 1 = far doodles, 2 = near sparkles).
     [doodle, left %, top %, size pt, rotation°]; placed in the margins, faint, never under a control. */
  PAR: {
    l1: [["ship", -7, 21, 72, -6], ["wave", 86, 37, 62, 8], ["compass", -6, 57, 54, 12], ["swirl", 87, 73, 58, -10], ["ros", -4, 91, 46, 6]],
    l2: [["spark", 3, 13, 22, 0], ["star", 92, 27, 20, 14], ["spark", 93, 63, 18, -8], ["star", 4, 80, 20, -12], ["spark", 90, 95, 22, 10]],
  },
  par() {
    const lay = (cls, list) => h("i", { class: cls }, ...list.map(([k, x, y, s, r]) => h("b", { class: "d-" + k, style: `left:${x}%;top:${y}%;--s:${s};rotate:${r}deg` })));
    return h("div", { class: "ob-par", "aria-hidden": "true" }, h("i", { class: "l0" }), lay("l1", KF.PAR.l1), lay("l2", KF.PAR.l2));
  },
  /* the Lab's stand-in for the gyro: the pointer over the phone, −1…1 (CSS smooths with a 180 ms linear transition on translate).
     Unity: AttitudeSensor (Input System) → yaw/pitch delta from the pose at screen open → low-pass (k ≈ .12/frame) → clamp ±1 →
     layer.style.translate = (x·depth, y·depth) with depth 4 / −6 / −9 pt. Reduce Motion: not driven. */
  drive(root) {
    const par = root.querySelector(".ob-par"); if (!par || reducedMotion()) return;
    const set = (x, y) => { par.style.setProperty("--px", x.toFixed(3)); par.style.setProperty("--py", y.toFixed(3)); };
    root.addEventListener("pointermove", e => { const r = root.getBoundingClientRect(); set(Math.max(-1, Math.min(1, (e.clientX - r.left) / r.width * 2 - 1)), Math.max(-1, Math.min(1, (e.clientY - r.top) / r.height * 2 - 1))); });
    root.addEventListener("pointerleave", () => set(0, 0));
  },
  /* call at the end of onboardingScreen (js/34_profiles.js), before `return root;` */
  decorateOnboarding(root) {
    if (activeLook() !== "playful") return root;
    root.classList.add("has-par"); root.prepend(KF.par());
    const head = root.querySelector(".ob1"); if (head && D.visitors[D.profile.visitors]?.mascot) { head.classList.add("has-azu"); head.append(KF.hello()); }
    KF.drive(root); return root;
  },

  /* §3 sticker earned: a card above the bottom chrome (not a takeover), motion ≤ 1.2 s, back to the wall at 2.6 s.
     Call only after the child FINDS something on the wall (a find game answered, a hidden detail tapped). */
  celebrate(p, opts = {}) {
    const set = KF.SETS.find(s => s.items.some(it => it[0] === p.id)) || KF.SETS[0], n = set.items.filter(KF.got).length;
    const root = dsScreenRoot("ds-cel"); root.classList.remove("scrn");
    const el = h("div", { class: "cel " + (opts.still ? "still" : "go"), role: "status", "aria-live": "polite" },
      KIDS.azuImg("cheer", "cel-azu"),
      h("div", { class: "cel-card k-frame k-panel" },
        h("div", { class: "cel-stk k-frame k-card" }, h("div", { class: "al-ph", style: KIDS.tileArt(p) }), h("span", { class: "al-ck k-frame k-badge" }, svg("check"))),
        h("div", { class: "cel-tx" }, h("small", { class: "ds-eyebrow" }, activeLook() === "playful" ? "New sticker!" : "Added to your album"), h("b", {}, `You found the ${p.name.toLowerCase()}!`),
          h("span", {}, `In your album: ${set.name}, ${n}\u00a0of\u00a0${set.items.length}`))),
      h("div", { class: "cel-burst", "aria-hidden": "true" }, ...Array.from({ length: 8 }, () => h("i"))));
    root.append(el); dev.layer.append(root);
    navigator.vibrate?.(30);   // Unity: the success haptic + a short chime (muted-safe: the card says it all)
    if (!opts.still) { dev.scr?.classList.add("celebrating"); setTimeout(() => el.classList.add("out"), 1500); setTimeout(() => { root.remove(); dev.scr?.classList.remove("celebrating"); }, 1850); }   // ≤ 1.5 s, then back to the wall
    return root;
  },

  /* §4 the album: a blank azulejo panel per area that fills tile by tile; a full set unlocks its story. No points, streaks or timers. */
  tile(it) {
    if (KF.got(it)) { const p = KF.poi(it[0]);
      return h("div", { class: "al-tile got hit", role: "button", "aria-label": p.name + ", found" },
        h("div", { class: "al-sq k-frame k-card" }, h("div", { class: "al-ph", style: KIDS.tileArt(p) }), h("span", { class: "al-ck k-frame k-badge" }, svg("check"))), h("small", {}, p.name)); }
    return h("div", { class: "al-tile e hit", role: "button", "aria-label": "Not found yet: " + it[1] }, h("div", { class: "al-sq" }, KIDS.doodle(it[2] || "sparkle")), h("small", {}, it[1]));
  },
  set(s) {
    const n = s.items.filter(KF.got).length;
    return h("section", { class: "al-set k-frame k-panel-b k-flat" },
      h("div", { class: "al-head" }, KIDS.doodle(s.doodle), h("div", { class: "al-ht" }, h("b", {}, s.name), h("small", {}, s.where)), h("span", { class: "al-n k-frame k-badge" }, `${n} of ${s.items.length}`)),
      h("div", { class: "al-grid" }, ...s.items.map(KF.tile)),
      n === s.items.length ? h("span", { class: "al-story k-frame k-btn2 k-press hit" }, svg("play"), s.story) : null);
  },
  album(close) {
    const root = dsScreenRoot("ds-album");
    root.append(h("div", { class: "ds-dim d-full" }), h("div", { class: "ds-sheet d-full sheet-album" }, h("span", { class: "grab" }, h("i")),
      h("div", { class: "khd" }, h("b", { class: "ds-h1" }, "Your tile album"), h("span", { class: "ds-x hit", onclick: close }, svg("close"))),
      h("div", { class: "ds-scroll sbody" }, h("p", { class: "al-intro" }, "Find these details on the wall. Each one you find is painted in here."), ...KF.SETS.map(KF.set))));
    return root;
  },
  /* the album's door, first in Explore (Playful only) */
  entry(open) {
    const all = KF.SETS.flatMap(s => s.items), n = all.filter(KF.got).length;
    return h("div", { class: "al-entry k-frame k-panel k-press hit", role: "button", onclick: open },
      h("b", {}, "Your tile album"), h("span", { class: "ds-cap" }, `${n} of ${all.length} details found on the wall`),
      h("div", { class: "al-strip" }, ...all.slice(0, 8).map(it => KF.got(it) ? h("i", { style: KIDS.tileArt(KF.poi(it[0])) }) : h("i", { class: "e" }))),
      KIDS.azuImg("point"));
  },
};

