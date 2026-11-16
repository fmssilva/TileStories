"""Unity feasibility lint for the Lab CSS (proj_guides/_4.1.9d_Unity_Feasibility.md section 5).
usage: python unity_lint.py [--all]   exit 1 on any strict problem or any ratchet increase.
  STRICT  css/kids_*.css and css/prem_*.css (Playful and Premium app UI): every item is a problem.
  RATCHET css/ds.css, css/tap.css (the shared app UI, not yet mapped to Unity): items are counted per file per rule name
          and compared with tests/unity_lint_ratchet.json; a count going UP fails, a count going DOWN prints a reminder
          to lower the json (never raise it to make a build pass).
  SKIPPED css/lab.css: it is the editor's own chrome (Unity Editor window), not app UI, so it has no Unity UI Toolkit target.
  --all   also lists every ratchet item.
Forbidden: backdrop-filter other than `none` (unless the rule is tagged /* unity: camera-glass */, the planned camera-blur shader),
filter: drop-shadow/brightness/saturate, mask(-image), mix-blend-mode, clip-path, perspective,
preserve-3d, rotateX/rotateY, cubic-bezier(), background-clip:text, outline(-offset), conic-gradient,
box-shadow with a blur (unless the rule is tagged /* unity: sprite */), @keyframes (unless tagged /* tween: */),
gradients (unless tagged /* unity: ramp */ or /* unity: sprite */)."""
import json, re, sys, glob, os
HERE = os.path.dirname(os.path.abspath(__file__))
RULES = [(r"backdrop-filter\s*:\s*(?!none\b)", "backdrop-filter (camera glass only)"), (r"filter\s*:\s*[^;}]*(drop-shadow|brightness|saturate)", "filter drop-shadow/brightness/saturate"),
    (r"(?<![-\w])mask(-image)?\s*:", "mask"), (r"-webkit-mask", "mask"), (r"mix-blend-mode", "mix-blend-mode"), (r"clip-path", "clip-path"),
    (r"perspective\s*[:(]", "perspective"), (r"preserve-3d", "preserve-3d"), (r"rotate[XY]\(", "3D rotate"), (r"cubic-bezier\(", "cubic-bezier (use named easing)"),
    (r"background-clip\s*:\s*text", "background-clip:text"), (r"border-image-outset", "border-image-outset (Unity: a sibling image, negative margins)"),
    (r"(?<![-\w])transform\s*:\s*translate", "transform: translate (use the translate property)"), (r":\s*inherit\b", "inherit"), (r"\bcqw\b", "cqw"), (r"dashed", "dashed border (Unity: a slot sprite)"), (r"(?<![-\w])outline(-offset)?\s*:", "outline"), (r"conic-gradient", "conic-gradient")]
def rules(css):
    for m in re.finditer(r"([^{}]+)\{([^{}]*)\}", re.sub(r"@media[^{]*\{", "", css)): yield re.sub(r"/\*.*?\*/", "", m.group(1), flags=re.S).strip(), m.group(2), m.group(0)
def lint(path):
    with open(path, encoding="utf-8") as f: css = f.read()
    out = []
    for m in re.finditer(r":has\(", css): out.append((":has()", css[max(0, m.start() - 60):m.start()].split("}")[-1].strip()[-60:]))
    for sel, body, full in rules(css):
        tagged = lambda t: t in full
        for rx, name in RULES:
            b2 = re.sub(r"--k-back\s*:\s*cubic-bezier\([^)]*\)", "", body) if "unity: ease-out-back" in full else body   # the one whitelisted easing token
            if name.startswith("backdrop-filter") and "unity: camera-glass" in body: continue   # the planned Unity camera-blur shader
            if re.search(rx, b2): out.append((name, sel[-90:]))
        for sh in re.findall(r"box-shadow\s*:\s*([^;}]*)", body):
            for part in re.split(r",(?![^(]*\))", sh):
                nums = re.findall(r"(-?[\d.]+)(px|pt|em)?|calc\([^)]*\)", part)
                lens = [t for t in re.findall(r"calc\([^()]*(?:\([^()]*\)[^()]*)*\)|-?[\d.]+px|\b0\b", part)]
                if part.strip() in ("none", "none!important") or "inset" in part: continue
                if len(lens) >= 3 and lens[2] not in ("0", "0px") and not tagged("unity: sprite"): out.append(("soft box-shadow untagged", sel[-90:]))
        if re.search(r"(linear|radial)-gradient", body) and not (tagged("unity: ramp") or tagged("unity: sprite")): out.append(("gradient untagged", sel[-90:]))
    for m in re.finditer(r"@keyframes\s+([\w-]+)", css):
        if "tween:" not in css[max(0, m.start() - 200):m.start() + 400]: out.append(("@keyframes untagged", m.group(1)))
    return out
STRICT = ["kids_*.css", "prem_*.css"]
RATCHET = ["ds.css", "tap.css"]
RATCHET_JSON = os.path.join(HERE, "unity_lint_ratchet.json")
def counts(items):
    c = {}
    for name, _ in items: c[name] = c.get(name, 0) + 1
    return c
if __name__ == "__main__":
    css = os.path.join(HERE, "..", "css"); bad = 0; per = {}
    files = sorted(f for pat in STRICT for f in glob.glob(os.path.join(css, pat)))
    for f in files:
        r = lint(f); bad += len(r); per[os.path.basename(f)] = len(r)
        for name, sel in r: print(f"{os.path.basename(f)}: {name} :: {sel}")
    print(f"strict: {len(files)} files (kids_*, prem_*), {bad} problems")
    for f, n in per.items():
        if n: print(f"  {f}: {n}")
    with open(RATCHET_JSON, encoding="utf-8") as fh: allowed = json.load(fh)
    up = down = 0
    for f in RATCHET:
        items = lint(os.path.join(css, f)); now = counts(items); was = allowed.get(f, {})
        print(f"ratchet {f}: {len(items)} items (allowed {sum(was.values())})")
        if "--all" in sys.argv:
            for name, sel in items: print(f"    {name} :: {sel}")
        for name in sorted(set(now) | set(was)):
            a, n = was.get(name, 0), now.get(name, 0)
            if n > a: print(f"  FAIL {f}: '{name}' went up {a} -> {n}"); up += 1
            elif n < a: print(f"  reminder: {f}: '{name}' went down {a} -> {n}; lower it in tests/unity_lint_ratchet.json"); down += 1
    print(f"unity lint: {bad} strict problems, {up} ratchet increases, {down} counts to lower")
    sys.exit(1 if bad or up else 0)
