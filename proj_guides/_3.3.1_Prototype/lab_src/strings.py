import io, json, os, re, sys
import yaml
D = os.path.dirname(os.path.abspath(__file__))
SRC = r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Screen\Cards\CardStrings.asset'
raw = io.open(SRC, encoding='utf-8').read()
rows = yaml.safe_load(raw[raw.index('MonoBehaviour:'):])['MonoBehaviour']['rows']

# Every text lives on the smallest part that draws it: (owner foldout, part). The owner is the lowest foldout the Editor has; the part
# is the button, line or label inside it. Keys whose part no longer exists are retired, with the reason.
RETIRED = {
    'language_name_en': "now the language's Name (Languages & Terms > Languages)", 'language_name_pt': "now the language's Name (Languages & Terms > Languages)",
    'nav_more': 'retired with the Bottom Bar (its More slot)',
    'family_ar': 'retired: sections are the wall\'s own now (Add section)', 'family_community': 'retired: sections are the wall\'s own now (Add section)',
    'family_navigation': 'retired: sections are the wall\'s own now (Add section)', 'family_meta': 'retired: sections are the wall\'s own now (Add section)',
    'family_about': 'now the About section\'s own Name', 'family_stories': 'now the Stories section\'s own Name', 'family_media': 'now the Media section\'s own Name',
    'family_guides': 'now the Listen section\'s own Name', 'family_play': 'now the Play section\'s own Name', 'family_visit': 'now the Visit section\'s own Name',
}
for f in ['read', 'look', 'watch', 'listen', 'model', 'interact']:
    RETIRED['format_' + f] = 'retired with Group By = format (sections replace it)'
for k, to in [('rich_text', 'Inline'), ('fun_fact', 'the Fun Fact pattern'), ('pull_quote', 'the Quote Moment pattern'), ('process_steps', 'the Steps pattern'),
              ('person', 'the Person pattern'), ('swatches', 'the Swatch Row element'), ('story_chapters', 'Story'), ('zoom_image', 'Gallery'), ('show_on_wall', 'the Locate Button')]:
    RETIRED['kind_' + k] = 'retired: no longer a block kind (now ' + to + ')'

KIND = {'header': 'Header', 'status': 'Status', 'quick_facts': 'Quick Facts', 'actions': 'Actions', 'gallery': 'Gallery', 'before_after': 'Before and After',
        'hotspot_image': 'Hotspot Image', 'video': 'Video', 'model_3d': '3D Model', 'panorama_360': '360 Panorama', 'audio_guide': 'Audio Guide',
        'timeline': 'Timeline', 'compare_points': 'Compare Points', 'dialogue': 'Dialogue', 'place_in_ar': 'Place In AR', 'knowledge_check': 'Knowledge Check',
        'poll': 'Poll', 'collect': 'Collect', 'practical_info': 'Practical Info', 'wall_locator': 'Wall Locator', 'today_map': 'Today Map', 'related': 'Related',
        'feedback': 'Feedback', 'nav_contents': 'Contents', 'sources': 'Sources', 'story': 'Story'}

P = {  # key -> (owner, part)
    'close': ('s5top', 'Close button'), 'visitor_settings': ('s5top', 'Settings button'),
    'language_switch': ('s5settings', 'Language group title'), 
    'settings_appearance': ('s5settings', 'Appearance group title'),
    'mode_light': ('s5settings', 'Light option'), 'mode_dark': ('s5settings', 'Dark option'),
    'media_unavailable': ('eg:Media', 'Missing-media notice (every media element)'),
    'fun_fact_label': ('el:callout', 'Flip card › front heading'), 'fun_fact_hint': ('el:callout', 'Flip card › tap hint'),
    'before_label': ('el:before_after', 'Before label (when the block names none)'), 'after_label': ('el:before_after', 'After label (when the block names none)'),
    'hotspot_hint': ('el:hotspot', 'Hint line'), 'model_3d_hint': ('el:model_3d', 'Hint line'), 'model_3d_loading': ('el:model_3d', 'Loading line'),
    'panorama_360_drag_hint': ('el:panorama', 'Hint line'), 'panorama_360_gyro_hint': ('el:panorama', 'Hint line'), 'panorama_360_loading': ('el:panorama', 'Loading line'),
    'audio_play': ('el:audio', 'Play button'), 'audio_pause': ('el:audio', 'Pause button'), 'audio_speed': ('el:audio', 'Speed button'),
    'audio_captions': ('el:audio', 'Captions button'), 'audio_seek': ('el:audio', 'Seek bar'), 'audio_unavailable': ('el:audio', 'Missing-audio notice'),
    'video_full_screen': ('el:video', 'Full-screen button'), 'video_unavailable': ('el:video', 'Missing-video notice'), 'video_chapters': ('el:video', 'Chapters list'),
    'media_badge': ('el:video', 'Poster badge'),
    'status_heading': ('bd:Status', 'Heading'), 'status_unknown_mark': ('bd:Status', 'Unknown state'), 'status_unknown': ('bd:Status', 'Unknown state'),
    'status_percent': ('bd:Status', 'Percentage'),
    'sources_heading': ('bd:Sources', 'Heading'), 'sources_verified': ('bd:Sources', 'Verified / Draft tag'), 'sources_draft': ('bd:Sources', 'Verified / Draft tag'),
    'timeline_now': ('bd:Timeline', 'Now marker'),
    'story_chapter_of': ('bd:Story', 'Chapter counter'), 'story_previous': ('bd:Story', 'Chapter Previous / Next buttons'), 'story_next': ('bd:Story', 'Chapter Previous / Next buttons'),
    'open_row_story': ('bd:Story', 'Preview › Open Row'),
    'compare_heading': ('bd:Compare Points', 'Heading'), 'header_then': ('bd:Header', 'Then / Now switch'), 'header_now': ('bd:Header', 'Then / Now switch'),
    'hero_listen': ('bd:Header', 'Listen button'), 'gallery_name': ('bd:Gallery', 'Page title'), 'gallery_count': ('bd:Gallery', 'Picture counter'),
    'zoom_hint': ('bd:Gallery', 'Zoom hint'),
    'page_back': ('s5back', 'Button name (screen readers)'), 'page_previous': ('s5prevnext', 'Previous button (page foot)'), 'page_next': ('s5prevnext', 'Next button (page foot)'),
    'open_block_button': ('s5openrow', 'Row label'), 'open_row_meta': ('s5openrow', 'Row label with reading time'),
    'compact_meta': ('s5expand', 'Format with reading time'), 'compact_open': ('s5expand', 'Row name while closed (screen readers)'), 'compact_close': ('s5expand', 'Row name while open (screen readers)'), 'full_view_open': ('s5expand', 'Full-view button'),
    'wall_locator_heading': ('bd:Wall Locator', 'Heading'), 'wall_locator_you': ('bd:Wall Locator', 'You-are-here mark'),
    'today_map_heading': ('bd:Today Map', 'Heading'), 'today_map_directions': ('bd:Today Map', 'Directions button'), 'today_map_then': ('bd:Today Map', 'Then / Now switch'),
    'today_map_now': ('bd:Today Map', 'Then / Now switch'), 'today_map_coordinates': ('bd:Today Map', 'Coordinates line'),
    'related_heading': ('bd:Related', 'Heading'),
    'knowledge_check_heading': ('bd:Knowledge Check', 'Heading'), 'knowledge_question_of': ('bd:Knowledge Check', 'Question counter'),
    'knowledge_correct': ('bd:Knowledge Check', 'Answer feedback'), 'knowledge_wrong': ('bd:Knowledge Check', 'Answer feedback'),
    'knowledge_true': ('bd:Knowledge Check', 'True / False buttons'), 'knowledge_false': ('bd:Knowledge Check', 'True / False buttons'),
    'knowledge_swipe_hint': ('bd:Knowledge Check', 'Swipe hint'), 'knowledge_previous': ('bd:Knowledge Check', 'Previous / Next buttons'),
    'knowledge_next': ('bd:Knowledge Check', 'Previous / Next buttons'),
    'feedback_thumbs_question': ('bd:Feedback', 'Question line'), 'feedback_stars_question': ('bd:Feedback', 'Question line'),
    'feedback_thumb_up': ('bd:Feedback', 'Thumb buttons'), 'feedback_thumb_down': ('bd:Feedback', 'Thumb buttons'), 'feedback_star': ('bd:Feedback', 'Star buttons'),
    'feedback_thanks': ('bd:Feedback', 'Thanks line'),
    'poll_your_choice': ('bd:Poll', 'Your-choice mark'), 'poll_thanks': ('bd:Poll', 'Thanks line'), 'poll_percent': ('bd:Poll', 'Result percentage'),
    'collect_heading': ('bd:Collect', 'Heading'), 'collect_add': ('bd:Collect', 'Collect button'), 'collect_collected': ('bd:Collect', 'Collect button'),
    'collect_progress': ('bd:Collect', 'Progress line'),
    'dialogue_continue': ('bd:Dialogue', 'Continue / Again buttons'), 'dialogue_again': ('bd:Dialogue', 'Continue / Again buttons'), 'dialogue_you': ('bd:Dialogue', 'Visitor\'s speaker name'),
    'show_on_wall_button': ('s5sticky', 'Button label'), 'show_on_wall_nearby': ('s5loc', 'Nearby caption'), 'locate_label': ('s5loc', 'Label chip'),
    'audio_queued': ('bd:Audio Guide', 'Queued notice'), 'mini_player_open': ('s1mini', 'Open button'), 'mini_player_stop': ('s1mini', 'Stop button'),
    'place_in_ar_button': ('bd:Place In AR', 'AR button'), 'place_in_ar_remove': ('bd:Place In AR', 'AR button'), 'place_in_ar_placed': ('bd:Place In AR', 'Placed line'),
    'place_in_ar_not_localised': ('bd:Place In AR', 'Not-localised line'), 'placed_model_remove': ('bd:Place In AR', 'Placed model › Remove button'),
    'placed_model_other': ('bd:Place In AR', 'Other point\'s chip'),
    'nav_menu': ('s5contents', 'Button and sheet title'), 'nav_pager_position': ('s5pager', 'Section counter'),
    'nav_pager_previous': ('s5pager', 'Previous chevron'), 'nav_pager_next': ('s5pager', 'Next chevron'),
    'nav_contents_minutes': ('bd:Contents', 'Reading time'), 'nav_contents_read': ('bd:Contents', 'Read marks'), 'nav_contents_section_read': ('bd:Contents', 'Read marks'),
    'facts_category': ('bd:Quick Facts', 'Category fact'),
    'section_opener_block': ('s2start', 'Block count (one)'), 'section_opener_blocks': ('s2start', 'Block count (several)'),
    'nav_next_up': ('s2next', 'Heading'), 'nav_next_up_position': ('s2next', 'Position'),
    'nav_next_button': ('s2next', 'Next button'), 'nav_next_up_done': ('s2next', 'Line at the last block'),
}
for f in ['up_to', 'no_headphones', 'no_camera', 'no_network', 'clear']:
    P['nav_facet_' + f] = ('s5contents', {'up_to': 'Time chip ({0} min or less)', 'no_headphones': 'No-headphones chip', 'no_camera': 'No-camera chip', 'no_network': 'No-network chip', 'clear': 'Clear chip'}[f])
FATE = {}


def place(k):
    if k in RETIRED:
        return 'retired', '', RETIRED[k]
    if k.startswith('kind_') and k[5:] in KIND:
        return 'bd:' + KIND[k[5:]], 'Block name (contents, chips)', ''
    if k in P:
        o, part = P[k]
        fate = next((f for p, f in FATE.items() if k == p or (p.endswith('_') and k.startswith(p))), '')
        return o, part, fate
    return None, None, None


out, missing = [], []
for r in rows:
    k = r['key']
    t = {x['lang']: str(x['value']) for x in (r.get('text') or [])}
    o, part, fate = place(k)
    if o is None:
        missing.append(k)
    out.append([k, o, re.sub(r'\s+', ' ', str(r.get('where') or '')).strip(), t.get('en', ''), t.get('pt', ''), fate, part])
print(len(out), 'texts;', sum(1 for x in out if x[1] == 'retired'), 'retired;', 'no owner:', missing)
io.open(os.path.join(D, 'realstrings.json'), 'w', encoding='utf-8').write(json.dumps(out, ensure_ascii=False))
if missing:
    sys.exit(1)
