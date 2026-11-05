"""Builds the Block Components Lab: joins the modules in MODULES and the styles in STYLES, and embeds the assets.
It never rewrites code. python build.py -> block-components-lab.html (not committed). PROTOTYPE_STRUCTURE.md says what each module owns."""
import base64, io, os, subprocess, sys
D = os.path.dirname(os.path.abspath(__file__))
M3 = os.path.join(D, '..', '..', 'mascote', '3d')

# the order is the load order: a module may use what an earlier one sets up at load, and the points are joined in this order
MODULES = ['src/core/open.js', 'src/core/assets.js', 'src/core/catalogues.js', 'src/core/state.js', 'src/core/words.js',
           'src/core/content.js', 'src/core/util.js', 'src/core/ext.js', 'src/editor/components.js', 'src/editor/shell.js',
           'src/editor/json.js', 'src/editor/card-tab.js', 'src/editor/demo.js', 'src/editor/global-tab.js',
           'src/editor/selftest-texts.js', 'src/phone/shell.js', 'src/card/motion.js', 'src/card/interactions.js',
           'src/card/sections.js', 'src/phone/sheet.js', 'src/phone/feedback.js', 'src/editor/selftest.js',
           'src/editor/words-tab.js', 'src/editor/focus.js', 'src/card/chrome.js', 'src/card/gallery.js',
           'src/card/contents.js', 'src/card/effects.js', 'src/guide/tour.js', 'src/editor/texts.js',
           'src/phone/preview-bar.js', 'src/phone/settings.js', 'src/card/terms.js', 'src/card/controls.js',
           'src/phone/preview-tools.js', 'src/phone/grabber.js', 'src/card/blocks.js', 'src/card/demo-card.js',
           'src/card/pages.js', 'src/card/elements.js', 'src/editor/setup-note.js', 'src/card/block-looks.js',
           'src/editor/marker-tab.js', 'src/card/languages.js', 'src/card/hero.js', 'src/card/story.js', 'src/card/peek.js',
           'src/phone/redraw.js', 'src/card/chrome-pinned.js', 'src/card/locate.js', 'src/card/hints.js', 'src/card/states.js',
           'src/guide/ask.js', 'src/guide/readalong.js', 'src/guide/focus.js', 'src/mascot/azu2d.js',
           'src/audiences/audiences.js', 'src/audiences/lenses.js', 'src/guide/person.js', 'src/guide/dock.js',
           'src/guide/voice.js', 'src/guide/voice-test.js', 'src/world/camera-ask.js', 'src/mascot/azu-gl.js',
           'src/world/wall.js', 'src/world/guide-button.js', 'src/agent/agent.js', 'src/agent/tools.js', 'src/agent/console.js',
           'src/agent/scenes.js', 'src/mascot/azu3d.js', 'src/mascot/models.js', 'src/mascot/tile-styles.js',
           'src/mascot/size.js', 'src/world/markers.js', 'src/agent/live-voice.js', 'src/mascot/mouth.js', 'src/core/boot.js']
# the cascade depends on this order: a new style goes at the end, or beside the rule it overrides
STYLES = ['src/core/base.css', 'src/card/card-base.css', 'src/phone/sheet.css', 'src/guide/tour.css', 'src/editor/texts.css',
          'src/card/controls.css', 'src/card/blocks.css', 'src/card/elements.css', 'src/editor/marker-tab.css', 'src/card/hero.css',
          'src/phone/grabber.css', 'src/card/hints.css', 'src/card/states.css', 'src/guide/readalong.css', 'src/guide/focus.css',
          'src/mascot/azu2d.css', 'src/audiences/audiences.css', 'src/audiences/lenses.css', 'src/guide/person.css',
          'src/guide/dock.css', 'src/guide/voice-test.css', 'src/world/camera-ask.css', 'src/mascot/azu-gl.css',
          'src/world/guide-button.css', 'src/agent/agent.css', 'src/agent/scenes.css', 'src/world/markers.css',
          'src/mascot/azu3d.css', 'src/mascot/models.css', 'src/mascot/tile-styles.css', 'src/mascot/size.css']


def rd(*p):
    return io.open(os.path.join(D, *p), encoding='utf-8').read()


def b64(path):
    return base64.b64encode(open(path, 'rb').read()).decode()


# the assets: each placeholder in the code becomes its file, as a data: URL
ASSETS = [('__REALSTR__', None, os.path.join(D, 'src', 'core', 'realstrings.json'))]
ASSETS += [('__' + k + '__', 'image/jpeg', os.path.join(D, 'assets', f + '.jpg')) for k, f in
           (('HERO', 'paco_hero'), ('G1', 'paco_gallery_1'), ('G2', 'paco_gallery_2'), ('G3', 'paco_gallery_3'),
            ('BEFORE', 'paco_before_1755'), ('BELEM', 'belem_wall'))]
ASSETS += [('__AZUGLB__', 'model/gltf-binary', os.path.join(M3, 'azu_tile.glb'))]
ASSETS += [('__AZUGLB_' + k + '__', 'model/gltf-binary', os.path.join(M3, f)) for k, f in
           (('AZU', 'azu.glb'), ('MOUSE', 'mouse.glb'), ('EXPLORER', 'explorer.glb'), ('POET', 'poet.glb'), ('SOLDIER', 'soldier.glb'))]
ASSETS += [('__AZTHUMB_' + k + '__', 'image/png', os.path.join(M3, 'thumbs', k.lower() + '.png')) for k in
           ('AZU', 'TILE', 'MOUSE', 'EXPLORER', 'POET', 'SOLDIER')]


def build():
    js = '\n'.join(rd(m) for m in MODULES)
    for key, mime, path in ASSETS:
        assert key in js, key
        data = io.open(path, encoding='utf-8').read() if mime is None else 'data:' + mime + ';base64,' + b64(path)
        js = js.replace(key, data)
    css = ''.join(rd(s) for s in STYLES)
    html = rd('src', 'page', 'head.html') + '<style>' + css + '</style>\n' + rd('src', 'page', 'page.html') + '\n<script>\n' + js + '\n</script>\n'
    io.open(os.path.join(D, 'block-components-lab.html'), 'w', encoding='utf-8').write(html)
    print('built', len(html))


if __name__ == '__main__':
    build()
    if os.path.exists(os.path.join(D, 'check_structure.js')) and '--no-check' not in sys.argv:
        sys.exit(subprocess.call(['node', os.path.join(D, 'check_structure.js')]))
