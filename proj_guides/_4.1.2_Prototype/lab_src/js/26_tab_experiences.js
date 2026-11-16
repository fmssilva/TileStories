function experiencesTab() {
  const E = D.experiences, nm = id => D.pois.find(p => p.id === id)?.name || id;
  return [
    h("p", { class: "lab-intro" }, h("b", {}, "Experiences · the wall's sequences and layers. "), "Content over many POIs. The Guide tab's Azu invites to them and narrates them."),
    fold("tours", "Tours & routes", { color: "#E3A6C8", badge: "new", defaultOpen: true, screen: "app", step: "explore", demo: { step: "explore", preview: null } }, () => [
      h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Name"), h("th", {}, "Theme"), h("th", {}, "Stops"), h("th", {}, "Min"))),
        h("tbody", {}, ...E.tours.map(t => h("tr", {}, h("td", {}, t.name), h("td", {}, t.theme), h("td", { style: "color:#AAB" }, t.stops.map(nm).join(" → ")), h("td", { class: "num" }, String(t.minutes)))))),
      h("p", { class: "guide" }, "Each step: a POI + narration (Guide). Seen / not seen; “next” adapts to the real visit; themes double as topic lenses for filters. Phase D: step through a tour on the Wall screen with edge arrows.")]),
    fold("moments", "Moments on the wall", { color: "#E3A6C8", badge: "new", defaultOpen: true, screen: "app", step: "explore", demo: { step: "explore", preview: null } }, () => [
      h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Name"), h("th", {}, "Plays"))), h("tbody", {}, ...E.moments.map(m => h("tr", {}, h("td", {}, m.name), h("td", { style: "color:#AAB" }, m.plays))))),
      h("p", { class: "guide" }, "Wall-wide animations the visitor starts from the Explore menu: the 1755 earthquake over the whole wall, day to night. Each plays a Timeline or Wall Life rows.")]),
    fold("games", "Games", { color: "#E3A6C8", badge: "new", defaultOpen: true, screen: "app", step: "explore", demo: { step: "explore", preview: null } }, () => [
      h("table", { class: "ut" }, h("thead", {}, h("tr", {}, h("th", {}, "Name"), h("th", {}, "Type"), h("th", {}, "Rule"))),
        h("tbody", {}, ...E.games.map(g => h("tr", {}, h("td", {}, g.name), h("td", {}, g.type), h("td", { style: "color:#AAB" }, g.rule))))),
      h("p", { class: "guide" }, "Questions come from each POI's Quiz content. Phase E. Leaderboards and prizes: parked (online).")]),
    planned("layers", "Wall layers", "planned", "Switchable overlays over the whole wall: today's map on the tiles; then and now; time of day and season; under the river; today's landmarks beyond the panel and post-1755 buildings with their year; 360 of the surroundings.", ["layer type", "asset", "geo-reference", "time slider"], "Task: later; then & now and time of day first (no internet).", { color: "#E3A6C8", screen: "app", step: "explore", demo: { step: "explore", preview: null } }),
    planned("capture", "Capture & share", "planned", "Photo and screen recording, stickers, finger drawing with brushes, share to social media.", ["tools on / off", "credit / watermark", "share targets"], "Task: its own domain later. The place is kept here.", { color: "#E3A6C8" }),
  ];
}

