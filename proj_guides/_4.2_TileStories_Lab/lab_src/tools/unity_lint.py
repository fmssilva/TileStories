"""Unity feasibility lint for the Lab's CSS (proj_guides/_4.1.9d_Unity_Feasibility.md section 5), run by build.py after
check_structure.js. Ported from _4.1.2_Prototype/lab_src/tests/unity_lint.py: the same rules and tags.
usage: python tools/unity_lint.py [--all] [--write-ratchet]   exit 1 on any strict problem or any ratchet increase.
  Every .css file under src/ is linted.
  RATCHET  a file named in tools/unity_lint_ratchet.json: its items are counted per rule and compared with the json; a count going UP
           fails, a count going DOWN prints a reminder to lower the json (never raise it to make a build pass).
  STRICT   a file the json does not name (a new file, or one fully ported and taken out of the json): every item is a problem.
  --all            also lists every item of the ratchet files.
  --write-ratchet  writes the json from today's counts (only to start the ratchet, or to lower it after a cleanup).
Forbidden: backdrop-filter other than `none` (unless the rule is tagged /* unity: camera-glass */, the planned camera-blur shader),
filter: drop-shadow/brightness/saturate, mask(-image), mix-blend-mode, clip-path, perspective,
preserve-3d, rotateX/rotateY, cubic-bezier() (but the one whitelisted --k-back token, tagged /* unity: ease-out-back */),
background-clip:text, border-image-outset, transform: translate, inherit, cqw, dashed borders, outline(-offset), conic-gradient,
:has(), box-shadow with a blur (unless the rule is tagged /* unity: sprite */), @keyframes (unless tagged /* tween: */),
gradients (unless tagged /* unity: ramp */ or /* unity: sprite */)."""
import json, re, sys, os
HERE = os.path.dirname(os.path.abspath(__file__))
D = os.path.dirname(HERE)
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
                lens = [t for t in re.findall(r"calc\([^()]*(?:\([^()]*\)[^()]*)*\)|-?[\d.]+px|\b0\b", part)]
                if part.strip() in ("none", "none!important") or "inset" in part: continue
                if len(lens) >= 3 and lens[2] not in ("0", "0px") and not tagged("unity: sprite"): out.append(("soft box-shadow untagged", sel[-90:]))
        if re.search(r"(linear|radial)-gradient", body) and not (tagged("unity: ramp") or tagged("unity: sprite")): out.append(("gradient untagged", sel[-90:]))
    for m in re.finditer(r"@keyframes\s+([\w-]+)", css):
        if "tween:" not in css[max(0, m.start() - 200):m.start() + 400]: out.append(("@keyframes untagged", m.group(1)))
    return out
RATCHET_JSON = os.path.join(HERE, "unity_lint_ratchet.json")
def counts(items):
    c = {}
    for name, _ in items: c[name] = c.get(name, 0) + 1
    return dict(sorted(c.items()))
def css_files():
    out = []
    for root, _, files in os.walk(os.path.join(D, "src")):
        out += [os.path.relpath(os.path.join(root, f), D).replace("\\", "/") for f in files if f.endswith(".css")]
    return sorted(out)
if __name__ == "__main__":
    files = css_files(); now = {f: lint(os.path.join(D, f)) for f in files}
    if "--write-ratchet" in sys.argv:
        with open(RATCHET_JSON, "w", encoding="utf-8", newline="\n") as fh: json.dump({f: counts(it) for f, it in now.items() if it}, fh, indent=1); fh.write("\n")
        print(f"unity lint: ratchet written, {sum(len(i) for i in now.values())} items in {sum(1 for i in now.values() if i)} files"); sys.exit(0)
    with open(RATCHET_JSON, encoding="utf-8") as fh: allowed = json.load(fh)
    bad = up = down = total = 0
    for f in files:
        items = now[f]; total += len(items)
        if f not in allowed:
            bad += len(items)
            for name, sel in items: print(f"  STRICT {f}: {name} :: {sel}")
            continue
        was = allowed[f]; c = counts(items)
        if "--all" in sys.argv:
            for name, sel in items: print(f"    {f}: {name} :: {sel}")
        for name in sorted(set(c) | set(was)):
            a, n = was.get(name, 0), c.get(name, 0)
            if n > a:
                print(f"  FAIL {f}: '{name}' went up {a} -> {n}"); up += 1
                for nm, sel in items:
                    if nm == name: print(f"       {sel}")
            elif n < a: print(f"  reminder: {f}: '{name}' went down {a} -> {n}; lower it in tools/unity_lint_ratchet.json"); down += 1
    gone = [f for f in allowed if f not in now]
    for f in gone: print(f"  reminder: {f} is in tools/unity_lint_ratchet.json but not under src/; take it out"); down += 1
    strict = [f for f in files if f not in allowed]
    print(f"{'FAIL ' if bad or up else 'ok   '}unity lint: {len(files)} CSS files, {total} items ({len(files) - len(strict)} on the ratchet, {len(strict)} strict); "
          f"{bad} strict problems, {up} ratchet increases, {down} counts to lower")
    if bad or up: print("The CSS got further from Unity: a rule the lint names was added (tools/unity_lint.py lists them; a tag allows the planned ones). See PROTOTYPE_STRUCTURE.md, \"Rules\".")
    sys.exit(1 if bad or up else 0)
