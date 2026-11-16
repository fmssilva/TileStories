/* ---------- placeholders for planned features (a card: purpose, configs, phase) ---------- */
const tagChip = t => h("span", { class: "tag " + ({ new: "t-new", extended: "t-ext", moved: "t-moved", merged: "t-merged", later: "t-later", planned: "t-later" }[t] || "t-same") }, ({ same: "AS TODAY", new: "NEW", extended: "EXTENDED", moved: "MOVED", merged: "MERGED", later: "LATER", planned: "PLANNED" })[t] || t.toUpperCase());
function planned(key, name, tag, purpose, configs, task, opts = {}) {
  return fold(key, name, { color: opts.color || "#D8D8D8", sub: opts.sub || "", extra: tagChip(tag), screen: opts.screen, step: opts.step, demo: opts.demo }, () => [
    h("div", { class: "sec" }, h("p", {}, purpose), configs.length ? h("div", { class: "chips" }, ...configs.map(c => h("span", {}, c))) : null, task ? h("p", { class: "insm" }, task) : null)]);
}
const grpHead = (n, t, hint) => h("div", { class: "grp" }, `${n} · ${t}`, h("span", {}, hint));
function groupify(list) { const res = []; let cur = null; for (const el of list.filter(Boolean)) { if (el.classList?.contains("grp")) { cur = h("div", { class: "gbody" }); res.push(el, cur); } else if (cur) cur.append(el); else res.push(el); } return res; }

