"""The Lab's local http server, on this computer only:

    python tools/serve.py           serves until Ctrl+C:  http://localhost:<LAB_PORT>/lab_src/tilestories-lab.html
    python tools/serve.py --ensure  starts it in the background unless it already answers, waits until it does, then exits
    LAB_PORT=8643 python tools/serve.py   another port (default 8642): one per copy of the Lab (a git worktree), set the same in the tests

--ensure accepts a running server only if it serves THIS copy's folder (GET /__lab_root answers its absolute ROOT); a port
taken by another copy, or by another program, is refused with a message and exit 1.

It serves the Lab's folder: lab_src (the page and its assets/) and, beside it, voicebench/ and music/, which the page reads. Over
http the page loads the 3D mascot's models and can read a picture back (Photo of the view, the mascot's measured figure); opened
from disk (file://) it cannot. The tests (tests/pw.js's labUrl), run_tests.js, tests/runtest8.ps1 and tests/shots8b.ps1 call
--ensure first. Nothing is cached (Cache-Control: no-store), so a new build is what the next load shows."""
import functools, http.server, os, socket, subprocess, sys, time, urllib.request

PORT = int(os.environ.get('LAB_PORT') or 8642)
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the Lab's folder, lab_src's parent
PAGE = 'lab_src/tilestories-lab.html'
URL = 'http://localhost:%d/%s' % (PORT, PAGE)


class Handler(http.server.SimpleHTTPRequestHandler):
    extensions_map = dict(http.server.SimpleHTTPRequestHandler.extensions_map, **{'.glb': 'model/gltf-binary', '.js': 'text/javascript',
                                                                                  '.mp3': 'audio/mpeg', '.ogg': 'audio/ogg'})

    def end_headers(self):
        self.send_header('Cache-Control', 'no-store')
        super().end_headers()

    def do_GET(self):
        if self.path == '/__lab_root':  # identity: which copy of the Lab this server serves
            body = ROOT.encode('utf-8')
            self.send_response(200)
            self.send_header('Content-Type', 'text/plain; charset=utf-8')
            self.send_header('Content-Length', str(len(body)))
            self.end_headers()
            self.wfile.write(body)
            return
        super().do_GET()

    def log_message(self, *a):  # quiet: the tests load hundreds of files
        pass


def served_root():
    """the folder the server on the port serves (GET /__lab_root), or None when nothing of the Lab answers there"""
    try:
        with urllib.request.urlopen('http://127.0.0.1:%d/__lab_root' % PORT, timeout=2) as r:
            return r.read(2000).decode('utf-8', 'replace') if r.status == 200 else None
    except Exception:
        return None


def same(a, b):
    return os.path.normcase(os.path.normpath(a)) == os.path.normcase(os.path.normpath(b))


def port_taken():
    with socket.socket() as s:
        s.settimeout(1)
        return s.connect_ex(('127.0.0.1', PORT)) == 0


def refuse():
    other = served_root()
    if other is None:
        print('Port %d is taken by another program (or an older Lab server: stop it): close it, or set LAB_PORT to a free port.' % PORT)
    else:
        print('Port %d serves %s, not %s; set LAB_PORT to another port.' % (PORT, other, ROOT))
    return 1


def ensure(attempts=50):
    r = served_root()
    if r is not None:
        return 0 if same(r, ROOT) else refuse()
    if port_taken():  # another --ensure may be starting it this moment
        for _ in range(attempts):
            time.sleep(0.1)
            r = served_root()
            if r is not None:
                return 0 if same(r, ROOT) else refuse()
        return refuse()
    flags = 0x00000008 | 0x00000200 | 0x08000000 if os.name == 'nt' else 0  # DETACHED_PROCESS | NEW_PROCESS_GROUP | NO_WINDOW
    subprocess.Popen([sys.executable, os.path.abspath(__file__)], cwd=ROOT, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL,
                     stderr=subprocess.DEVNULL, close_fds=True, creationflags=flags, start_new_session=os.name != 'nt')
    for _ in range(100):
        r = served_root()
        if r is not None:
            if same(r, ROOT):
                print('The Lab is served at ' + URL)
                return 0
            return refuse()
        time.sleep(0.1)
    print('The Lab server did not start on port %d (python tools/serve.py shows why).' % PORT)
    return 1


def serve():
    httpd = http.server.ThreadingHTTPServer(('127.0.0.1', PORT), functools.partial(Handler, directory=ROOT))
    print('The TileStories Lab: ' + URL + '\n(this computer only; Ctrl+C stops it)')
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass


if __name__ == '__main__':
    sys.exit(ensure() if '--ensure' in sys.argv else serve())
