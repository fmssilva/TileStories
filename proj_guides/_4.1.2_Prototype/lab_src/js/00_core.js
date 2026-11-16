const SRC = "@@PANORAMA@@";
const PHOTOS = { castle_day: "@@PHOTO_CASTLE_DAY@@", castle_walls: "@@PHOTO_CASTLE_WALLS@@", castle_night: "@@PHOTO_CASTLE_NIGHT@@" };
const IW = 640, IH = 441.5, PXCM = 3.8, ORIGIN = [0, 441.5];
const clamp = (v, a = 0, b = 1) => Math.max(a, Math.min(b, v));
const ease = x => 1 - Math.pow(1 - clamp(x), 3);
const smooth = x => { x = clamp(x); return x * x * (3 - 2 * x); };
const clone = o => JSON.parse(JSON.stringify(o));
const rnd = (a, b) => a + Math.random() * Math.max(0, b - a);

