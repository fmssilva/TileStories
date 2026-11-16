"""The whole safety net in one command. usage: python tests/run_checks.py [out.html] [--quick]
Steps: build -> regress (+ report) -> compare with tests/report_baseline.json -> sweep Premium -> sweep Playful
(SWEEP_INIT="setProfile('visitors','children')") -> unity_lint. --quick = build + regress + compare + lint (no sweeps).
Prints one summary table; exits 1 if any step fails. The build and the reports go to out.html's folder (default: a temp dir)."""
import os, re, subprocess, sys, tempfile, time
HERE = os.path.dirname(os.path.abspath(__file__)); LAB = os.path.dirname(HERE); PY = sys.executable
args = [a for a in sys.argv[1:] if not a.startswith("--")]; quick = "--quick" in sys.argv
out = os.path.abspath(args[0]) if args else os.path.join(tempfile.mkdtemp(prefix="lab_checks_"), "lab.html")
os.makedirs(os.path.dirname(out), exist_ok=True)
report = os.path.splitext(out)[0] + "_report.json"
env = {**os.environ, "PYTHONUTF8": "1", "PYTHONIOENCODING": "utf-8"}
rows = []
def run(name, cmd, parse, extra_env=None):
    t = time.time()
    p = subprocess.run([PY, *cmd], cwd=LAB, env={**env, **(extra_env or {})}, capture_output=True, text=True, encoding="utf-8", errors="replace")
    txt = p.stdout + p.stderr; ok, detail = parse(p.returncode, txt)
    rows.append((name, ok, detail, time.time() - t))
    if not ok: print(f"--- {name} output (tail) ---\n" + "\n".join(txt.strip().splitlines()[-25:]) + "\n---", flush=True)
    return ok
def last(rx, txt, default="?"):
    m = re.findall(rx, txt); return m[-1] if m else default
def p_build(rc, t): return rc == 0 and os.path.exists(out), (last(r"(\d+) bytes", t) + " bytes")
def p_regress(rc, t):
    m = re.search(r"(\d+) checks, (\d+) failed (\[.*?\]); errors (\d+)", t)
    if not m or rc: return False, "did not finish"
    return m.group(2) == "0" and m.group(4) == "0", f"{m.group(1)} checks, {m.group(2)} failed, {m.group(4)} page errors"
def p_compare(rc, t):
    n = last(r"differences: (\d+)", t); e = last(r"errors: (\d+)", t)
    return rc == 0 and n == "0" and e == "0", f"{n} differences, {e} errors"
def p_sweep(rc, t):
    m = re.search(r"(\d+) poses.*page errors: (\d+)", t)
    bad = [l for l in t.splitlines() if l.startswith("  ") and "clean" not in l and ":" in l and "off-screen" not in l and "page error" not in l]
    off = re.search(r"off-screen: (\{.*\})", t); offbad = bool(off) and "FAIL" in off.group(1)
    if not m or rc: return False, "did not finish"
    ok = m.group(2) == "0" and not bad and not offbad
    return ok, f"{int(m.group(1)):,} poses, {len(bad)} styles with violations, {m.group(2)} page errors" + (", off-screen FAIL" if offbad else "")
def p_lint(rc, t):
    m = re.search(r"unity lint: (\d+) strict problems, (\d+) ratchet increases, (\d+) counts to lower", t)
    if not m: return False, "did not finish"
    return rc == 0, f"{m.group(1)} strict, {m.group(2)} ratchet up, {m.group(3)} to lower"
ok = run("build", ["build.py", out], p_build)
if ok:
    run("regress", ["tests/regress.py", out, report], p_regress)
    if os.path.exists(report): run("compare", ["tests/compare.py", "tests/report_baseline.json", report], p_compare)
    else: rows.append(("compare", False, "no report", 0.0))
    if not quick:
        run("sweep Premium", ["tests/sweep.py", out], p_sweep)
        run("sweep Playful", ["tests/sweep.py", out], p_sweep, {"SWEEP_INIT": "setProfile('visitors','children')"})
run("unity_lint", ["tests/unity_lint.py"], p_lint)
w = max(len(r[0]) for r in rows)
print(f"\n{'step'.ljust(w)}  result  {'time':>6}  detail")
for n, o, d, s in rows: print(f"{n.ljust(w)}  {'PASS' if o else 'FAIL':6}  {s:5.0f}s  {d}")
print(f"\nbuild: {out}" + ("" if quick else f"\nreport: {report}"))
bad = [r[0] for r in rows if not r[1]]
print("ALL CHECKS PASSED" if not bad else "FAILED: " + ", ".join(bad)); sys.exit(1 if bad else 0)
