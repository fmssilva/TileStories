"""The Lab's local http server, on this computer only:

    python tools/serve.py           serves until Ctrl+C:  http://localhost:8642/lab_src/block-components-lab.html
    python tools/serve.py --ensure  starts it in the background unless it already answers, waits until it does, then exits

It serves the Lab's folder: lab_src (the page and its assets/) and, beside it, voicebench/ and music/, which the page reads. Over
http the page loads the 3D mascot's models and can read a picture back (Photo of the view, the mascot's measured figure); opened
from disk (file://) it cannot. The tests (tests/pw.js's labUrl), run_tests.js, tests/runtest8.ps1 and tests/shots8b.ps1 call
--ensure first. Nothing is cached (Cache-Control: no-store), so a new build is what the next load shows."""
import functools, http.server, os, socket, subprocess, sys, time, urllib.request

PORT = 8642
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the Lab's folder, lab_src's parent
PAGE = 'lab_src/block-components-lab.html'
URL = 'http://localhost:%d/%s' % (PORT, PAGE)
MARK = "The Lab's local http server"  # this file's first words: how --ensure knows the port is this server


class Handler(http.server.SimpleHTTPRequestHandler):
    extensions_map = dict(http.server.SimpleHTTPRequestHandler.extensions_map, **{'.glb': 'model/gltf-binary', '.js': 'text/javascript',
                                                                                  '.mp3': 'audio/mpeg', '.ogg': 'audio/ogg'})

    def end_headers(self):
        self.send_header('Cache-Control', 'no-store')
        super().end_headers()

    def log_message(self, *a):  # quiet: the tests load hundreds of files
        pass


def answers():
    """this server answers on the port (it serves this very file back)"""
    try:
        with urllib.request.urlopen('http://127.0.0.1:%d/lab_src/tools/serve.py' % PORT, timeout=2) as r:
            return MARK in r.read(400).decode('utf-8', 'replace')
    except Exception:
        return False


def port_taken():
    with socket.socket() as s:
        s.settimeout(1)
        return s.connect_ex(('127.0.0.1', PORT)) == 0


def ensure():
    if answers():
        return 0
    if port_taken():  # another --ensure may be starting it this moment
        for _ in range(50):
            time.sleep(0.1)
            if answers():
                return 0
        print('Port %d is taken by another program: close it, or change PORT in tools/serve.py.' % PORT)
        return 1
    flags = 0x00000008 | 0x00000200 | 0x08000000 if os.name == 'nt' else 0  # DETACHED_PROCESS | NEW_PROCESS_GROUP | NO_WINDOW
    subprocess.Popen([sys.executable, os.path.abspath(__file__)], cwd=ROOT, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL,
                     stderr=subprocess.DEVNULL, close_fds=True, creationflags=flags, start_new_session=os.name != 'nt')
    for _ in range(100):
        if answers():
            print('The Lab is served at ' + URL)
            return 0
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
