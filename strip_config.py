import json, os

paths = [
    r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\Apps\LivingRoom\config.json',
    r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\StreamingAssets\LivingRoom\config.json',
]

for p in paths:
    with open(p, 'r', encoding='utf-8') as f:
        config = json.load(f)

    removed_anchors = 'calibration_anchors' in config
    if removed_anchors:
        del config['calibration_anchors']

    removed_norms = 0
    for poi in config.get('pois', []):
        if 'x_norm' in poi:
            del poi['x_norm']
            removed_norms += 1
        if 'y_norm' in poi:
            del poi['y_norm']
            removed_norms += 1

    with open(p, 'w', encoding='utf-8') as f:
        json.dump(config, f, indent=4, ensure_ascii=False)
        f.write('\n')

    print(os.path.basename(os.path.dirname(p)), '| anchors removed:', removed_anchors, '| norm fields removed:', removed_norms)
