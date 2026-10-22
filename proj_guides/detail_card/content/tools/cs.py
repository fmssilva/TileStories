"""Commons helper: python cs.py search "<query>" [mime-filter]  |  python cs.py info "File:Name.jpg" ..."""
import json, sys, re, html, urllib.parse, urllib.request, time

UA = "TileStoriesContentBot/1.0 (porteiroconsigo@gmail.com; test-wall content gathering)"
API = "https://commons.wikimedia.org/w/api.php"


def call(params):
    params["format"] = "json"
    url = API + "?" + urllib.parse.urlencode(params)
    req = urllib.request.Request(url, headers={"User-Agent": UA})
    with urllib.request.urlopen(req, timeout=40) as r:
        return json.load(r)


def strip(s):
    return html.unescape(re.sub(r"<[^>]+>", "", s or "")).strip()


def info(titles):
    out = []
    for i in range(0, len(titles), 20):
        d = call({"action": "query", "titles": "|".join(titles[i:i + 20]), "prop": "imageinfo",
                  "iiprop": "url|size|mime|extmetadata", "iiurlwidth": int(__import__("os").environ.get("W","2048"))})
        for p in d["query"]["pages"].values():
            ii = (p.get("imageinfo") or [{}])[0]
            m = ii.get("extmetadata", {})
            g = lambda k: strip(m.get(k, {}).get("value", ""))
            out.append(dict(title=p["title"], w=ii.get("width"), h=ii.get("height"), mime=ii.get("mime"),
                            lic=g("LicenseShortName"), artist=g("Artist")[:70], date=g("DateTimeOriginal")[:30],
                            credit=g("Credit")[:60], url=ii.get("url"), thumb=ii.get("thumburl"),
                            page=ii.get("descriptionurl"), dur=ii.get("duration")))
    return out


def show(r):
    print("%s | %sx%s %s | %s | %s | %s" % (r["title"], r["w"], r["h"], r["mime"], r["lic"], r["artist"], r["date"]))


if __name__ == "__main__":
    cmd = sys.argv[1]
    if cmd == "search":
        q = sys.argv[2]
        extra = sys.argv[3] if len(sys.argv) > 3 else ""
        d = call({"action": "query", "list": "search", "srsearch": q + " " + extra, "srnamespace": 6, "srlimit": 25})
        titles = [x["title"] for x in d["query"]["search"]]
        for r in info(titles):
            show(r)
    elif cmd == "info":
        for r in info(sys.argv[2:]):
            show(r)
            print("   ", r["page"], r.get("dur") or "")
    elif cmd == "get":
        out = sys.argv[2]
        import os
        os.makedirs(out, exist_ok=True)
        for r in info(sys.argv[3:]):
            u = r["thumb"] or r["url"]
            fn = os.path.join(out, re.sub(r"[^A-Za-z0-9._-]", "_", r["title"][5:])[:80])
            if not fn.lower().endswith((".jpg", ".jpeg", ".png")):
                fn += ".jpg"
            for attempt in range(4):
                try:
                    req = urllib.request.Request(u, headers={"User-Agent": UA})
                    data = urllib.request.urlopen(req, timeout=90).read()
                    open(fn, "wb").write(data)
                    print("saved", fn, len(data)); break
                except Exception as e:
                    print("retry", e); time.sleep(5 * (attempt + 1))
            time.sleep(1.5)
    elif cmd == "cat":
        d = call({"action": "query", "list": "categorymembers", "cmtitle": sys.argv[2], "cmtype": "file", "cmlimit": 60})
        titles = [x["title"] for x in d["query"]["categorymembers"]]
        for r in info(titles):
            show(r)
