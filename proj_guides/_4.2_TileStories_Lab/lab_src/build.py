"""Builds the TileStories Lab: joins the modules in MODULES and the styles in STYLES, embeds the words and the faces, and puts the
3D mascot's files in assets/ beside the page. It never rewrites code. python build.py -> block-components-lab.html (not committed)
and assets/mascot/ (copied, not committed), then the structure check (check_structure.js) and the Unity lint (tools/unity_lint.py):
either one failing fails the build. The page with its assets/ is served by tools/serve.py and published as page + files (README.md).
PROTOTYPE_STRUCTURE.md says what each module owns."""
import io, os, re, shutil, subprocess, sys
D = os.path.dirname(os.path.abspath(__file__))
M3 = os.path.join(D, '..', '..', 'mascote', '3d')

# the order is the load order: a module may use what an earlier one sets up at load, and the points are joined in this order
MODULES = ['src/core/open.js', 'src/core/assets.js', 'src/core/catalogues.js', 'src/core/state.js', 'src/core/words.js',
           'src/core/content.js', 'src/core/util.js', 'src/core/ext.js', 'src/core/undo.js', 'src/editor/components.js', 'src/editor/shell.js',
           'src/editor/json.js', 'src/editor/card-tab.js', 'src/editor/demo.js', 'src/editor/global-tab.js',
           'src/editor/selftest-texts.js', 'src/phone/shell.js', 'src/card/motion.js', 'src/card/interactions.js',
           'src/card/sections.js', 'src/phone/sheet.js', 'src/phone/feedback.js', 'src/editor/selftest.js',
           'src/editor/words-tab.js', 'src/editor/focus.js', 'src/editor/accordion.js', 'src/editor/guide-tab.js',
           'src/card/chrome.js', 'src/card/gallery.js',
           'src/card/contents.js', 'src/card/effects.js', 'src/guide/tour.js', 'src/editor/texts.js',
           'src/phone/preview-bar.js', 'src/phone/settings.js', 'src/card/terms.js', 'src/card/controls.js',
           'src/phone/preview-tools.js', 'src/phone/grabber.js', 'src/card/blocks.js', 'src/card/demo-card.js',
           'src/card/pages.js', 'src/card/page-open.js', 'src/card/elements.js', 'src/editor/setup-note.js', 'src/card/block-looks.js',
           'src/editor/marker-tab.js', 'src/card/languages.js', 'src/card/hero.js', 'src/card/story.js', 'src/card/peek.js',
           'src/phone/redraw.js', 'src/card/chrome-pinned.js', 'src/card/locate.js', 'src/card/hints.js', 'src/card/states.js',
           'src/guide/ask.js', 'src/guide/readalong.js', 'src/guide/highlight.js', 'src/guide/focus.js', 'src/mascot/azu2d.js',
           'src/audiences/audiences.js', 'src/audiences/lenses.js', 'src/guide/person.js', 'src/guide/dock.js',
           'src/guide/voice.js', 'src/guide/controls.js', 'src/guide/voice-test.js', 'src/guide/autotour.js', 'src/world/camera-ask.js', 'src/mascot/azu-gl.js',
           'src/world/wall.js', 'src/world/guide-button.js', 'src/agent/agent.js', 'src/agent/tools.js', 'src/agent/console.js',
           'src/agent/scenes.js', 'src/mascot/azu3d.js', 'src/mascot/models.js', 'src/mascot/tile-styles.js',
           'src/mascot/size.js', 'src/world/markers.js', 'src/guide/guide-scenes.js', 'src/agent/live-voice.js', 'src/mascot/mouth.js',
           'src/guidance/catalog.js', 'src/guidance/scheduler.js', 'src/guidance/service.js', 'src/guidance/present.js',
           'src/guidance/editor.js', 'src/guidance/scenes.js', 'src/guide/tell-more.js', 'src/guide/saved-place.js', 'src/guide/tour-end.js', 'src/guide/game-ask.js', 'src/guide/voice-script.js', 'src/guide/mascot-profile.js', 'src/guide/hero-start.js', 'src/guide/teleprompter.js', 'src/guide/panel-look.js', 'src/guide/demo-hand.js', 'src/guide/tour-scroll.js', 'src/core/boot.js']
# the cascade depends on this order: a new style goes at the end, or beside the rule it overrides
STYLES = ['src/core/fonts.css', 'src/core/base.css', 'src/card/card-base.css', 'src/phone/sheet.css', 'src/guide/tour.css', 'src/editor/texts.css', 'src/editor/accordion.css',
          'src/card/controls.css', 'src/card/blocks.css', 'src/card/elements.css', 'src/editor/marker-tab.css', 'src/card/hero.css',
          'src/phone/grabber.css', 'src/card/hints.css', 'src/card/states.css', 'src/guide/readalong.css', 'src/guide/focus.css',
          'src/mascot/azu2d.css', 'src/audiences/audiences.css', 'src/audiences/lenses.css', 'src/guide/person.css',
          'src/guide/dock.css', 'src/guide/controls.css', 'src/guide/voice-test.css', 'src/world/camera-ask.css', 'src/mascot/azu-gl.css',
          'src/world/guide-button.css', 'src/agent/agent.css', 'src/agent/scenes.css', 'src/world/markers.css',
          'src/mascot/azu3d.css', 'src/mascot/models.css', 'src/mascot/tile-styles.css', 'src/mascot/size.css', 'src/editor/demo.css',
          'src/guidance/present.css', 'src/guidance/editor.css', 'src/guide/autotour.css', 'src/guide/tell-more.css',
          'src/guide/saved-place.css', 'src/guide/tour-end.css', 'src/guide/game-ask.css', 'src/guide/mascot-profile.css', 'src/guide/hero-start.css', 'src/guide/highlight.css', 'src/guide/teleprompter.css', 'src/guide/panel-look.css', 'src/card/page-open.css', 'src/guide/demo-hand.css', 'src/guide/tour-scroll.css']


def rd(*p):
    return io.open(os.path.join(D, *p), encoding='utf-8').read()


# embedded: each placeholder becomes its file as it is: the framework's words (realstrings.json) and the faces (src/core/fonts.css:
# media/FONT_*.txt, each a data: URL), so the page has its words and its faces even opened from disk
EMBED = [('__REALSTR__', os.path.join(D, 'src', 'core', 'realstrings.json'))]
EMBED += [('__FONT_' + k + '__', os.path.join(D, 'media', 'FONT_' + k + '.txt')) for k in
          ('FRAUNCES', 'ATK400', 'ATK600', 'ATK700', 'MONO400', 'MONO500')]
# beside the page, in assets/: the pictures live there (committed); the 3D mascot's models and thumbnails are copied there from
# proj_guides/mascote/3d (assets/mascot/, not committed). The code names each file by its path ("assets/..."); a path it names that
# assets/ lacks fails the build. Over http (tools/serve.py, the published Lab) they load; opened from disk the models cannot be
# fetched nor a picture read back from a canvas, and the Editor says so (editor/shell.js's fileOnlyNotice)
MASCOT = ('azu', 'mouse', 'explorer', 'poet', 'soldier')
COPY = [(os.path.join(M3, m + '.glb'), os.path.join('assets', 'mascot', m + '.glb')) for m in MASCOT]
COPY += [(os.path.join(M3, 'thumbs', m + '.png'), os.path.join('assets', 'mascot', 'thumbs', m + '.png')) for m in MASCOT]
ASSET_REF = re.compile(r'assets/[\w./-]+\.(?:jpg|jpeg|png|glb|webp|mp3)')


def copy_assets():
    for src, dst in COPY:
        dst = os.path.join(D, dst)
        if not os.path.exists(dst) or os.path.getmtime(dst) < os.path.getmtime(src) or os.path.getsize(dst) != os.path.getsize(src):
            os.makedirs(os.path.dirname(dst), exist_ok=True)
            shutil.copy2(src, dst)


def build():
    copy_assets()
    js = '\n'.join(rd(m) for m in MODULES)
    css = ''.join(rd(s) for s in STYLES)
    for key, path in EMBED:
        assert key in js or key in css, key
        data = io.open(path, encoding='utf-8').read()
        js, css = js.replace(key, data), css.replace(key, data)
    named = sorted(set(ASSET_REF.findall(js + css)))
    missing = [a for a in named if not os.path.isfile(os.path.join(D, a))]
    assert not missing, 'the page names files that assets/ lacks: ' + ', '.join(missing)
    html = rd('src', 'page', 'head.html') + '<style>' + css + '</style>\n' + rd('src', 'page', 'page.html') + '\n<script>\n' + js + '\n</script>\n'
    io.open(os.path.join(D, 'block-components-lab.html'), 'w', encoding='utf-8').write(html)
    size = sum(os.path.getsize(os.path.join(D, a)) for a in named)
    print('built', len(html), '+ assets/: %d files, %.1f MB' % (len(named), size / 1e6))


if __name__ == '__main__':
    build()
    if '--no-check' not in sys.argv:
        rc = subprocess.call(['node', os.path.join(D, 'check_structure.js')])
        rc = subprocess.call([sys.executable, os.path.join(D, 'tools', 'unity_lint.py')]) or rc
        sys.exit(rc)
