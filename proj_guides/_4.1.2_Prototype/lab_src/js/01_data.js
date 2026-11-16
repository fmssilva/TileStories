/* ---------- data (what config.json holds) ---------- */
const CATS = {
  category_1: { label: "Royal Government", color: "#FFC786" },
  category_2: { label: "Religious", color: "#CB80FF" },
  category_6: { label: "Infrastructure", color: "#9DDEFF" },
};
function mkStyle(key, label, o = {}) {
  const base = {
    key, label,
    line: { width_cm: 0.4, min_px: 1.2, max_px: 4, color_mode: "accent", color: "#FFFFFF", opacity: 0.95, pattern: "solid", dash_cm: 3, gap_cm: 2, smooth: 0.5,
            placement: "edge", offset_cm: 0, glow: true, glow_width_cm: 2, glow_intensity: 0.7, double: false, double_gap_cm: 1.5 },
    vertices: { show_at: "none", mark: "dot", size_cm: 1.6, color_mode: "line", color: "#FFFFFF" },
    fill: { mode: "tint", color_mode: "accent", color: "#FFFFFF", opacity: 0.18, feather_cm: 2, dim: 0.3, blend: "normal",
            grad: "radial", angle_deg: 90, alpha_from: 0.3, alpha_to: 0, pattern: "azulejo", scale_cm: 6, fit: "cover", media_play: "loop" },
    motion: { intro: { preset: "draw_on", duration_s: 0.9, segment_delay_s: 0.12, direction: "cw" },
              idle: { preset: "breathe", period_s: 4, count: 1, intensity: 0.35 },
              focus: { preset: "spotlight", amount: 0.55, duration_s: 0.36 } },
  };
  for (const [part, vals] of Object.entries(o)) {
    if (part === "motion") for (const [ph, v] of Object.entries(vals)) Object.assign(base.motion[ph], v);
    else Object.assign(base[part], vals);
  }
  return base;
}
const K = (x, y, corner = "", radius = 6) => ({ x, y, corner, radius });
function mkContent(kind, key, name, anchor, o = {}) {
  const k = KINDS[kind];
  return Object.assign({ key, name, kind, anchor, trigger: k.trig, delay_s: 0, every_min_s: 6, every_max_s: 12, reduce: "", p: clone(k.p) }, o, { p: Object.assign(clone(k.p), o.p || {}) });
}
const KINDS = {
  repaint:   { code: "K1", label: "Repaint", lives: "On the wall surface, covering a shape", made: "Painted wall canvas (Krita / Photoshop / AI style over the exported canvas) + a reveal shader (wipe, paint-on brush): “art comes alive”", prec: "high", anchor: "polygon", trig: "reveal", trigs: ["reveal", "loop", "focus", "unfocus", "lapse"],
               p: { look: "night", reveal: "brush", wipe_s: 2.4, feather_cm: 2, strength: 0.9 } },
  flat:      { code: "K2", label: "Flat animation", lives: "On the wall surface, inside a shape", made: "PSD layers → PSD Importer + 2D Animation (bones); or a flipbook / a video with alpha", prec: "medium", anchor: "polygon", trig: "loop", trigs: ["loop", "reveal", "focus", "every"],
               p: { subject: "curtain", color: "#F3E6C8", amp_cm: 1.2, speed: 1, cover: true } },
  hinged:    { code: "K3", label: "Hinged 3D piece", lives: "Out of the wall, starting flush", made: "ProBuilder slab cut to the shape, pivot on the hinge, wall-projection material; Animation window", prec: "high", anchor: "polygon", trig: "focus", trigs: ["focus", "reveal", "every"],
               p: { angle_deg: 70, dur_s: 0.9, interior: true, close_on_unfocus: true } },
  agent:     { code: "K4", label: "Agent on a path", lives: "In front of the wall, moving", made: "Any prefab (2D flipbook or 3D model) + Splines' Spline Animate on a Path shape", prec: "low", anchor: "path", trig: "every", trigs: ["every", "loop", "focus"],
               p: { sprite: "tram", size_cm: 6, speed_cm_s: 22 } },
  particles: { code: "K5", label: "Ambient particles", lives: "In front of the wall or in the sky", made: "Particle System; the emitter is a shape or a box", prec: "low", anchor: "any", trig: "every", trigs: ["every", "loop", "focus"],
               p: { sprite: "birds", count: 7, speed_cm_s: 30 } },
  guest:     { code: "K6", label: "Screen guest", lives: "On the screen, not in the world", made: "UI Toolkit (or uGUI) with a flipbook, a 2D rig or Rive", prec: "none", anchor: "screen", trig: "every", trigs: ["every"],
               p: { edge: "right", stay_s: 2.5, hide_on_card: true } },
};
const TRIGS = { reveal: "Reveal (once, when the POI appears)", loop: "Loop (always while visible)", focus: "Focus (on tap; back on close)", unfocus: "Unfocus (when the card closes)", every: "Every N–M s (random)", lapse: "Lapse (time period changes) · later" };

let D = {
  styles: [   // v17: premium presets that read well even on an imprecise outline (corner smoothing, contact shadow + light glow, brackets, spotlight)
    mkStyle("region_style_1", "Soft glow"),   // Hub and Landmark: a fine gold line lifted by a contact shadow and a glow, a warm wash; a tap dims the rest
    mkStyle("region_style_2", "Hairline", { line: { width_cm: 0.3, opacity: 0.9, glow_width_cm: 1.2, glow_intensity: 0.4, smooth: 0.6 }, fill: { mode: "none" },
      motion: { idle: { preset: "none" }, focus: { preset: "brighten", amount: 2 } } }),   // Detail: the lightest touch
    mkStyle("region_style_3", "Focus brackets", { line: { pattern: "brackets", width_cm: 0.5, offset_cm: 2, glow_intensity: 0.6 }, fill: { opacity: 0.08 },
      motion: { intro: { preset: "fade", duration_s: 0.5 }, idle: { preset: "none" }, focus: { preset: "brighten", amount: 2 } } }),   // the brackets close in on a tap
    mkStyle("region_style_4", "Route", { line: { width_cm: 0.35, opacity: 0.45, smooth: 0.5, glow_intensity: 0.5 }, fill: { mode: "none" },
      motion: { intro: { preset: "draw_on", duration_s: 1.2 }, idle: { preset: "comet", period_s: 3.2, count: 2, intensity: 0.8 }, focus: { preset: "brighten", amount: 2 } } }),
    mkStyle("region_style_5", "Wash", { line: { opacity: 0, glow: false }, fill: { mode: "gradient", grad: "radial", color_mode: "fixed", color: "#FFF1D0", blend: "screen", alpha_from: 0.5, alpha_to: 0, feather_cm: 3 },
      motion: { intro: { preset: "fade", duration_s: 0.6 }, idle: { preset: "sweep", period_s: 8, intensity: 0.35 }, focus: { preset: "brighten", amount: 2 } } }),
    mkStyle("region_style_6", "Media window", { line: { width_cm: 0.3, opacity: 0.25, glow_width_cm: 3, glow_intensity: 0.6, smooth: 0.5 },
      fill: { mode: "media", opacity: 0.94, feather_cm: 1, fit: "cover", media_play: "on_tap" },
      motion: { intro: { preset: "fade", duration_s: 0.6 }, idle: { preset: "none" }, focus: { preset: "brighten", amount: 2 } } }),
    mkStyle("region_style_7", "Quiet (appears on tap)", { line: { width_cm: 0.4, opacity: 0, glow_width_cm: 2, glow_intensity: 0.6 }, vertices: { mark: "ring", size_cm: 1.1 }, fill: { mode: "none" },
      motion: { intro: { preset: "fade", duration_s: 0.5 }, idle: { preset: "none" }, focus: { preset: "appear", amount: 0.95, duration_s: 0.36 } } }),
    mkStyle("region_style_8", "Spotlight (one hero)", { line: { width_cm: 0.3, opacity: 0.6, glow_intensity: 0.5 }, fill: { mode: "spotlight", dim: 0.3, feather_cm: 8 },
      motion: { intro: { preset: "fade", duration_s: 0.6 }, idle: { preset: "none" }, focus: { preset: "brighten", amount: 2 } } }),
    mkStyle("region_style_9", "Inner light", { line: { opacity: 0, glow: false }, fill: { mode: "tint", color_mode: "fixed", color: "#F4B740", blend: "normal", opacity: 0.26, feather_cm: 3 },
      motion: { intro: { preset: "fade", duration_s: 0.8 }, idle: { preset: "breathe", period_s: 4, intensity: 0.4 }, focus: { preset: "brighten", amount: 2 } } }),   // no line at all: the most forgiving when tracking drifts. v19 R6: a normal warm wash (a screen blend vanished on the light tin-glaze tiles)
  ],


  region_default: "region_style_1",   // the app-wide region style; a level or a POI may pick another
  levels: [
    { key: "level_1", name: "Hub", layers: { point: true, region: true, label: true, tag: false, model: false }, region_style: "region_style_1", point_style: "point_style_3", tap_layout: "", reveal_delay_s: 0 },
    { key: "level_2", name: "Landmark", layers: { point: false, region: true, label: true, tag: false, model: false }, region_style: "region_style_1", point_style: "point_style_1", tap_layout: "", reveal_delay_s: 0.4 },
    { key: "level_3", name: "Detail", layers: { point: true, region: false, label: true, tag: false, model: false }, region_style: "region_style_2", point_style: "point_style_2", tap_layout: "tap_layout_1", reveal_delay_s: 0.8 },
  ],
  pois: [
    { id: "twin_tower_church", status: "destroyed", name: "Twin-tower church", category: "category_2", level: "level_1", media: { pano: "castle_walls", sample: true }, pos: { x: 536, y: 300 }, summary: "A baroque church on the river square, lost in 1755.",
      world: { presence_override: "", style_override: "", shapes: [
        { key: "shape_1", name: "Outline", type: "polygon", style: "", default_corner: "sharp", knots: [K(507,372),K(507,279),K(510,277),K(517.5,256),K(525,277),K(528,279),K(528,300),K(538,295),K(556,302),K(560,300),K(560,287),K(566,268),K(573,287),K(574,304),K(614,304),K(632,316),K(632,372)] },
        { key: "shape_2", name: "Door", type: "polygon", style: "region_style_6", media: "media_1", default_corner: "sharp", knots: [K(525,362),K(525,342),K(534,334,"smooth"),K(543,342),K(543,362)] },
        { key: "shape_3", name: "Windows", type: "points", style: "region_style_7", default_corner: "sharp", knots: [K(533,321),K(517,322),K(566,322),K(590,332),K(605,332)] },
        { key: "shape_4", name: "Cornice", type: "path", style: "region_style_2", default_corner: "sharp", knots: [K(575,304),K(620,299)] },
        { key: "shape_5", name: "Tower window", type: "polygon", style: "region_style_7", default_corner: "sharp", knots: [K(511,300),K(524,300),K(524,318),K(511,318)] },
      ], content: [
        mkContent("repaint", "content_1", "Night facade", "shape_1"),
        mkContent("flat", "content_2", "Curtain", "shape_5"),
      ] } },
    { id: "domed_church", status: "partial_damage", name: "Domed church", category: "category_2", level: "level_2", media: { pano: "castle_walls", sample: true }, pos: { x: 579, y: 124 }, summary: "Its dome watched over the upper town.",
      world: { presence_override: "", style_override: "region_style_5", shapes: [
        { key: "shape_1", name: "Outline", type: "polygon", style: "", default_corner: "sharp", knots: [K(568,156),K(568,123),K(571,118,"smooth"),K(579,115.5,"smooth"),K(587,118,"smooth"),K(591.5,123),K(591.5,140),K(611,140),K(611,156)] },
        { key: "shape_2", name: "Window", type: "polygon", style: "region_style_7", default_corner: "sharp", knots: [K(573,123),K(586,123),K(586,140),K(573,140)] },
      ], content: [
        mkContent("hinged", "content_1", "Shutters", "shape_2"),
      ] } },
    { id: "arched_gate", status: "intact", name: "Arched gate", category: "category_6", level: "level_3", media: { pano: "castle_walls", sample: true }, pos: { x: 267, y: 356 }, summary: "Where goods came up from the river.",
      world: { presence_override: "both", style_override: "", shapes: [
        { key: "shape_1", name: "Riverside road", type: "path", style: "region_style_4", default_corner: "smooth", knots: [K(120,399),K(230,395),K(330,393),K(440,398)] },
      ], content: [
        mkContent("agent", "content_1", "Tram", "shape_1", { every_min_s: 5, every_max_s: 10 }),
      ] } },
    { id: "castle_hill", status: "partial_damage", name: "Castle hill", on_tap: { tap_layout: "", over: { gap: 6, buttons: { favourite: "hide" } } }, category: "category_1", level: "level_2", pos: { x: 252, y: 178 }, summary: "Sample media: today's photos of São Jorge Castle stand in for this hilltop.",
      media: { hero: "castle_day", today: "castle_day", pano: "castle_walls", night: "castle_night" },
      world: { presence_override: "", style_override: "", shapes: [
        { key: "shape_1", name: "Outline", type: "polygon", style: "", default_corner: "sharp", knots: [K(220,204),K(220,156),K(228,148),K(236,154),K(236,163),K(254.5,153.5),K(269.5,163),K(270,161.5),K(282,161.5),K(282,204)] },
      ], content: [] } },
    // v20: marked on the wall, story not written yet: the tap UI shows the name and "coming soon" (App › On tap › Coming soon)
    { id: "hilltop_convent", status: "", name: "Hilltop convent", category: "category_2", level: "level_3", content: "soon", pos: { x: 149, y: 139 }, summary: "",
      world: { presence_override: "", style_override: "", shapes: [], content: [] } },
    { id: "arcaded_house", status: "", name: "Arcaded house", category: "category_4", level: "level_3", content: "soon", pos: { x: 160, y: 361 }, summary: "",
      world: { presence_override: "", style_override: "", shapes: [], content: [] } },
  ],
  life: {
    settings: { reduce_motion_state: "static", show_regions: true },
    wall: [
      mkContent("particles", "wall_content_1", "Birds over the roofs", "", { every_min_s: 6, every_max_s: 11 }),
      mkContent("guest", "wall_content_2", "Mascot peek", "", { every_min_s: 14, every_max_s: 22 }),
    ],
  },
  test: { error_cm: 0, reduce_motion: false },
};

/* action types (framework code: each type knows its controls), named action sets (Global Scene), and per-POI content (Specific Marker) */
const ACTIONS = {
  card:     { label: "Open the card", controls: [] },
  model_wall: { label: "3D on the wall", controls: ["rotate", "zoom", "move", "moments"] },
  model_screen: { label: "3D on screen", controls: ["rotate", "zoom", "moments"] },
  pano:     { label: "360 visit", controls: ["look around", "zoom"] },
  map:      { label: "Today on the map", controls: ["open map", "directions"] },
  then_now: { label: "Then & now", controls: ["wipe"] },
  play:     { label: "Video / audio", controls: ["play", "scrub"] },
  ask:      { label: "Ask the guide", controls: [] },
  navigate: { label: "Navigate here", controls: [] },
  favourite:{ label: "Favourite", controls: [] },
  share:    { label: "Photo & share", controls: [] },
  tour:     { label: "Start a tour here", controls: [] },
  quiz:     { label: "Quiz", controls: [] },
};
D.pois[0].actions = { model: { file: "twin_tower_church.glb", controls: { rotate: true, zoom: true, move: false }, moments: [{ name: "Rebuild the church", plays: "Timeline · Reconstruct" }, { name: "1755: the earthquake", plays: "Timeline · Earthquake" }] } };
D.pois[1].actions = { model: { file: "", controls: { rotate: true, zoom: true, move: false }, moments: [] } };
D.pois[2].actions = { model: { file: "", controls: { rotate: true, zoom: true, move: false }, moments: [] } };
D.pois[3].actions = { model: { file: "castle_photogrammetry.fbx", controls: { rotate: true, zoom: true, move: true }, moments: [{ name: "The castle at night", plays: "Timeline · Night" }] } };
D.app = {
  start: { loading_lines: true },
  onboarding: { on: true, ask_visitors: true, ask_time: true, ask_language: true, ask_interests: true },
  explore: { tours: true, games: true, moments: true, layers: false },
  tap: { default_layout: "tap_layout_3", offscreen: "pin", keep_inside: true, card_yields: true },
  pointing: { sel_look: "chip", others: "top", max: 3, rank: "level_distance", min_level: "any", look: "chip", how_far: "off", while_tapped: false },
};
D.experiences = { moments: [
  { key: "moment_1", name: "1755: the earthquake", plays: "Timeline · Earthquake (whole wall)" },
  { key: "moment_2", name: "Day → night", plays: "Wall Life K1 · night repaint, all POIs" } ],
  tours: [
  { key: "tour_1", name: "Churches of the old city", theme: "Architecture", stops: ["castle_hill", "domed_church", "twin_tower_church"], minutes: 8 },
  { key: "tour_2", name: "Down to the river", theme: "Maritime", stops: ["castle_hill", "arched_gate", "twin_tower_church"], minutes: 9 } ],
  games: [
  { key: "game_1", name: "Put it back", type: "Find & place", pois: 3, rule: "drag each floating piece back to its place on the wall; harder each round" },
  { key: "game_2", name: "Quiz rally", type: "Quiz rally", pois: 3, rule: "find the places, then 5 questions each" } ] };

/* media for the Media fill: the style says how (fit, opacity, feather), the shape says which file */
D.media = [
  { key: "media_1", name: "Sunset over the river (sample video)", type: "video", src: "gen:sunset" },
  { key: "media_2", name: "Azulejo panel (sample image)", type: "image", src: "gen:azulejo" },
  { key: "media_3", name: "The wall in 1755 (sample image)", type: "image", src: "gen:then" },
  { key: "photo_castle_day", name: "São Jorge Castle, today (photo)", type: "image", src: "photo:castle_day" },
  { key: "photo_castle_night", name: "São Jorge Castle, at night (photo)", type: "image", src: "photo:castle_night" },
];
/* v18: each POI's story in layers (Quick ⊂ Classic ⊂ Deep extras) and an optional children's version; missing layers fall back to classic */
const STORIES = {
  twin_tower_church: { quick: "A baroque church lost in 1755.", classic: "A baroque church on the river square, its twin towers the first thing sailors saw. The 1755 earthquake brought it down.",
    deep: "Built in the 1600s over an older chapel, it kept the guild altars of the river trades. The ruins burned for days after the earthquake and were cleared for the new Baixa grid. Sources: the 1756 survey, the Panorama itself.",
    kids: "Two tall towers waved at the ships! Can you count the bells? One big shake in 1755 knocked them down." },
  domed_church: { quick: "Its dome watched over the upper town.", classic: "Its dome watched over the upper town, a landmark you could see from every boat on the Tejo.",
    deep: "Partly damaged in 1755, its dome was rebuilt lower; the panel shows it before the change. The tile painter added a little gold to the lantern.", kids: "Look for the round dome on top, like a giant helmet! It still stands today." },
  arched_gate: { quick: "Where goods came up from the river.", classic: "Where goods came up from the river: the city's front door for salt, fish and spices.",
    deep: "Customs officers counted every crate here; the arch's stones were reused after 1755. The gate still stands, now inside a later building.", kids: "Every boat's treasure came through this door: fish, salt and spices!" },
  castle_hill: { quick: "The castle on the hill, the city's oldest heart.", classic: "The castle on the hill, the city's oldest heart: walls first built by the Moors, home of kings until the 1500s.",
    deep: "Sample media: today's photos of São Jorge Castle stand in for this hilltop. The Panorama shows the walls before the 1755 damage.", kids: "A real castle with towers and walls! Kings lived here long ago." },
};
D.pois.forEach(p => { if (STORIES[p.id]) p.story = STORIES[p.id]; });
