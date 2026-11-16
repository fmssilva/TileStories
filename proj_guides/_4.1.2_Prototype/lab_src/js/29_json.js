/* ---------- JSON preview ---------- */
function compact(obj) {
  return JSON.stringify(obj, (k, v) => (k === "knots" && Array.isArray(v)) ? v.map(q => "@@" + JSON.stringify(q) + "@@") : v, 2)
    .replace(/"@@(.*?)@@"/g, (m, s) => s.replace(/\\"/g, '"').replace(/,"/g, ', "').replace(/":/g, '": '));
}
function styleJson(s) {
  const o = clone(s);
  delete o.line.placement;
  const F = o.fill, mode = F.mode;
  o.fill = { mode, color_mode: F.color_mode, ...(F.color_mode === "fixed" ? { color: F.color } : {}),
    ...(mode === "tint" ? { opacity: F.opacity, feather_cm: F.feather_cm } : mode === "gradient" ? { grad: F.grad, ...(F.grad === "linear" ? { angle_deg: F.angle_deg } : {}), alpha_from: F.alpha_from, alpha_to: F.alpha_to, feather_cm: F.feather_cm } : mode === "pattern" ? { pattern: F.pattern, scale_cm: F.scale_cm, opacity: F.opacity } : mode === "media" ? { fit: F.fit, opacity: F.opacity, video_plays: F.media_play, feather_cm: F.feather_cm } : {}) };
  return o;
}
const rowJson = c => ({ key: c.key, name: c.name, kind: c.kind, anchor_shape_key: c.anchor, trigger: c.trigger, ...(c.trigger === "every" ? { every_s: [c.every_min_s, c.every_max_s] } : {}), ...(c.delay_s ? { delay_s: c.delay_s } : {}), ...(c.reduce ? { reduce_motion: c.reduce } : {}), prefab: c.name.replace(/[^A-Za-z0-9]+/g, "") + ".prefab", params: c.p });
const jsonGlobal = () => compact({ region_styles: D.styles.map(styleJson), wall_life: { settings: D.life.settings, rows: D.life.wall.map(rowJson) }, region_default: D.region_default, app: { profiles: { settings: D.app.profiles, looks: D.looks, visitors: D.visitors, time: D.time, interests: Object.keys(INTEREST), onboarding: D.app.onboarding, default_appearance: D.app.appearance, text_scale: TEXT_SCALE }, on_tap: { default_style: D.app.tap.default_layout, base: { buttons: D.tap_actions, button_look: D.tap_button }, tag_components: D.tap_tags, action_components: D.tap_action_comps, styles: D.tap_layouts } }, hierarchy_levels: D.levels.map(l => ({ key: l.key, level_name: l.name, "...": "existing fields", layers: Object.keys(l.layers).filter(k => l.layers[k]), region_style_key: l.region_style, tap_style_key: l.tap_layout || "(default)" })) });
const jsonPoi = p => compact({ id: p.id, category: p.category, hierarchy_level_key: p.level, position: toM(p.pos), world: {
  presence_override: p.world.presence_override, region_style_override: p.world.style_override,
  shapes: p.world.shapes.map(S => ({ key: S.key, name: S.name, type: S.type, region_style_override: S.style, ...(S.media ? { media_path: `POIs/${p.id}/media/` + (D.media.find(m => m.key === S.media)?.name || S.media).replace(/ \(.*\)$/, "") } : {}), ...(S.type !== "points" ? { default_corner: S.default_corner } : {}),
    knots: S.knots.map(q => { const o = { p: toM(q) }; if (q.corner && S.type !== "points") o.corner = q.corner; if (S.type !== "points" && (q.corner || S.default_corner) === "rounded") o.radius_cm = q.radius; return o; }) })),
  content: (p.world.content || []).map(rowJson) }, on_tap: p.on_tap || {}, actions_content: p.actions || {} });

