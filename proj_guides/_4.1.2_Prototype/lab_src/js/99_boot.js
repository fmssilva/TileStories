new ResizeObserver(() => { const r = stageEl.getBoundingClientRect(), k = Math.round(r.width) + "x" + Math.round(r.height); if (k === dev.stageKey) return; dev.stageKey = k;   // only a real size change rebuilds the device
  buildDevice(); if (ui.screen === "app") renderPhone(); else renderTapWall(); }).observe(stageEl);
// every editor section starts collapsed (the developer opens what they need)
preloadSymbols(); syncScreens(); renderEditor(); buildDevice(); requestAnimationFrame(loop);
