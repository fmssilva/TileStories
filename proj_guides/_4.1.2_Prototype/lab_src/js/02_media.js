const mediaEls = {};
function genCanvas(w, hh) { const c = document.createElement("canvas"); c.width = w; c.height = hh; return c; }
function mediaSource(key, now) {
  const m = D.media.find(x => x.key === key); if (!m) return null;
  if (m.src === "gen:sunset") {
    const c = mediaEls.sunset || (mediaEls.sunset = genCanvas(320, 240)), x = c.getContext("2d"), t = now;
    const g = x.createLinearGradient(0, 0, 0, 150); g.addColorStop(0, "#3B2A6B"); g.addColorStop(0.55, "#E0674A"); g.addColorStop(1, "#F7C26B");
    x.fillStyle = g; x.fillRect(0, 0, 320, 150);
    const sy = 120 + 10 * Math.sin(t * 0.25); x.fillStyle = "#FFE3A0"; x.beginPath(); x.arc(160, sy, 26, 0, 7); x.fill();
    x.fillStyle = "#24345E"; x.fillRect(0, 150, 320, 90);
    for (let i = 0; i < 14; i++) { const y = 156 + i * 6, w = 40 + (i * 37) % 90, xx = 160 - w / 2 + Math.sin(t * 1.5 + i) * 18; x.fillStyle = `rgba(255,214,140,${0.55 - i * 0.03})`; x.fillRect(xx, y, w, 2); }
    x.fillStyle = "#16182A"; x.beginPath(); x.moveTo(0, 150); for (let i = 0; i <= 8; i++) x.lineTo(i * 40, 140 - (i % 3) * 8 - (i % 2) * 6); x.lineTo(320, 150); x.fill();
    const bx = (t * 14) % 380 - 30; x.fillStyle = "#101320"; x.beginPath(); x.moveTo(bx, 172); x.lineTo(bx + 30, 172); x.lineTo(bx + 24, 180); x.lineTo(bx + 6, 180); x.fill(); x.fillRect(bx + 14, 152, 2, 20);
    return c;
  }
  if (m.src === "gen:azulejo") {
    if (mediaEls.azulejo) return mediaEls.azulejo;
    const c = genCanvas(240, 240), x = c.getContext("2d"); x.fillStyle = "#F4F1E8"; x.fillRect(0, 0, 240, 240); x.strokeStyle = "#1F4FA3"; x.fillStyle = "#1F4FA3"; x.lineWidth = 2;
    for (let i = 0; i < 4; i++) for (let j = 0; j < 4; j++) { const ox = i * 60, oy = j * 60; x.strokeRect(ox, oy, 60, 60); x.beginPath(); x.moveTo(ox + 30, oy + 8); x.lineTo(ox + 52, oy + 30); x.lineTo(ox + 30, oy + 52); x.lineTo(ox + 8, oy + 30); x.closePath(); x.stroke(); x.beginPath(); x.arc(ox + 30, oy + 30, 7, 0, 7); x.fill(); for (const [a, b] of [[0, 0], [60, 0], [0, 60], [60, 60]]) { x.beginPath(); x.arc(ox + a, oy + b, 9, 0, 7); x.fill(); } }
    return mediaEls.azulejo = c;
  }
  if (m.src === "gen:then") {
    if (mediaEls.then) return mediaEls.then; if (!img.complete || !img.naturalWidth) return null;
    const c = genCanvas(IW, IH), x = c.getContext("2d"); x.filter = "sepia(1) contrast(1.4) brightness(0.9) grayscale(0.3)"; x.drawImage(img, 0, 0, IW, IH); x.filter = "none";
    x.strokeStyle = "rgba(60,40,20,.25)"; for (let y = 0; y < IH; y += 3) { x.beginPath(); x.moveTo(0, y); x.lineTo(IW, y + 2); x.stroke(); }
    return mediaEls.then = c;
  }
  if (m.src.startsWith("photo:")) { const k = m.src.slice(6); return mediaEls[k] || (mediaEls[k] = Object.assign(new Image(), { src: PHOTOS[k] })); }
  return mediaEls[m.key] || null; // a file loaded by the author (image or video element)
}
function loadMediaFile(file, S) {
  const url = URL.createObjectURL(file), isVideo = file.type.startsWith("video");
  const key = "media_" + (D.media.length + 1);
  const el = isVideo ? Object.assign(document.createElement("video"), { src: url, muted: true, loop: true, playsInline: true, autoplay: true }) : Object.assign(new Image(), { src: url });
  if (isVideo) { el.muted = true; el.play().catch(() => {}); }
  mediaEls[key] = el;
  D.media.push({ key, name: file.name + " (this session only)", type: isVideo ? "video" : "image", src: "file" });
  snap(); S.media = key; renderEditor();
}
const mediaReady = el => el && ((el.videoWidth || el.naturalWidth || el.width) > 0);

