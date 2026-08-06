#!/usr/bin/env python3
"""
Generate the new LivingRoom config.json with heritage taxonomy.
Maps 3 main POIs (royal_government, intact, intact) + 5 satellites each
to the 5 heritage categories (religious, military, residential, economic,
infrastructure) with matching badge/outline levels.
"""
import json

# Main POI data (kept from existing config)
MAIN_POIS = {
    "lamp": {
        "name": "The Lamp",
        "x_norm": 0.10000000149011612,
        "y_norm": 0.10000000149011612,
        "captured_position": {"x": -0.9999999403953552, "y": -0.8700000047683716, "z": -4.179999828338623},
        "has_captured_position": True,
        "captured_position_source": "workflow_a_editor",
        "captured_position_timestamp": 1785834802,
        "summary": "POI_Lamp",
        "effect_mode": "pulse,sun_contours",
        "rotate_contour": False,
    },
    "painting": {
        "name": "The Painting",
        "x_norm": 0.5,
        "y_norm": 0.30000001192092898,
        "captured_position": {"x": -2.0, "y": 0.0, "z": 0.0},
        "has_captured_position": True,
        "captured_position_source": "workflow_a_editor",
        "captured_position_timestamp": 1785834802,
        "summary": "POI_Painting",
        "effect_mode": "pulse,sun_circles",
        "rotate_contour": False,
    },
    "camera": {
        "name": "The Camera",
        "x_norm": 0.800000011920929,
        "y_norm": 0.699999988079071,
        "captured_position": {"x": -1.9300000667572022, "y": 0.7200000286102295, "z": 2.2300000190734865},
        "has_captured_position": True,
        "captured_position_source": "workflow_a_editor",
        "captured_position_timestamp": 1785834802,
        "summary": "POI_Camera",
        "effect_mode": "beacon",
        "rotate_contour": False,
    },
}

# Satellite positions from existing config (left/right/up/down)
# These are offsets from the main POI's captured_position
SATELLITE_POSITIONS = {
    "lamp": {
        "left":   {"x": -0.9999999403953552, "y": -1.1699999570846558, "z": -4.179999828338623},  # lamp_left
        "right":  {"x": -0.9999999403953552, "y": -0.8700000047683716, "z": -3.880000114440918},  # lamp_right
        "up":     {"x": -0.9999999403953552, "y": -0.5699999928474426, "z": -4.179999828338623},  # lamp_up
        "down":   {"x": -0.9999999403953552, "y": -0.8700000047683716, "z": -4.480000019073486},  # lamp_down
        "front":  {"x": -0.6999999403953552, "y": -0.8700000047683716, "z": -4.179999828338623},  # new: +0.3 x
    },
    "painting": {
        "left":   {"x": -2.0, "y": 0.0, "z": -0.30000001192092898},     # painting_left
        "right":  {"x": -2.0, "y": 0.0, "z": 0.30000001192092898},      # painting_right
        "up":     {"x": -2.0, "y": 0.30000001192092898, "z": 0.0},      # painting_up
        "down":   {"x": -2.0, "y": -0.30000001192092898, "z": 0.0},     # painting_down
        "front":  {"x": -1.7, "y": 0.0, "z": 0.0},                       # new: +0.3 x
    },
    "camera": {
        "left":   {"x": -1.9300000667572022, "y": 0.7200000286102295, "z": 1.9300000667572022},  # camera_left
        "right":  {"x": -1.9300000667572022, "y": 0.7200000286102295, "z": 2.5299999713897707},  # camera_right
        "up":     {"x": -1.9300000667572022, "y": 1.0199999809265137, "z": 2.2300000190734865},  # camera_up
        "down":   {"x": -1.9300000667572022, "y": 0.41999998688697817, "z": 2.2300000190734865}, # camera_down
        "front":  {"x": -1.6300000667572022, "y": 0.7200000286102295, "z": 2.2300000190734865}, # new: +0.3 x
    },
}

# Satellite effect_mode mapping (carried from existing spatial positions)
SATELLITE_EFFECTS = {
    "lamp": {
        "left": "",         # lamp_left
        "right": "",        # lamp_right
        "up": "simple_sun", # lamp_up
        "down": "beacon",   # lamp_down
        "front": "",        # new
    },
    "painting": {
        "left": "ring_pulse",  # painting_left
        "right": "",           # painting_right
        "up": "simple_sun",    # painting_up
        "down": "beacon",      # painting_down
        "front": "",           # new
    },
    "camera": {
        "left": "ring_pulse",  # camera_left
        "right": "",           # camera_right
        "up": "simple_sun",    # camera_up
        "down": "beacon",      # camera_down
        "front": "",           # new
    },
}

# Satellite rotate_contour mapping (carried from existing spatial positions)
SATELLITE_ROTATE = {
    "lamp": {
        "left": False,   # lamp_left
        "right": True,   # lamp_right
        "up": False,     # lamp_up
        "down": False,   # lamp_down
        "front": False,  # new
    },
    "painting": {
        "left": True,    # painting_left
        "right": True,   # painting_right
        "up": False,     # painting_up
        "down": False,   # painting_down
        "front": False,  # new
    },
    "camera": {
        "left": False,   # camera_left
        "right": True,   # camera_right
        "up": False,     # camera_up
        "down": False,   # camera_down
        "front": False,  # new
    },
}

# Category -> direction mapping
# left=religious(intact), right=military(partial), up=residential(partial),
# down=economic(destroyed), front=infrastructure(unknown)
SATELLITE_CATEGORIES = [
    ("religious",       "left",   "intact",         "intact",      0.0,  False),
    ("military",        "right",  "partial_damage", "partial_damage", 20.0, False),
    ("residential",     "up",     "partial_damage", "partial_damage", 20.0, False),
    ("economic",        "down",   "destroyed",      "destroyed",     100.0, False),
    ("infrastructure",  "front",  "unknown_damage", "unknown",       100.0, True),
]

def make_poi(poi_id, name, category, x_norm, y_norm, captured_pos,
             effect_mode, rotate_contour, badge_category, status_level_key,
             status_pct, has_status, status_unknown, is_hero):
    return {
        "id": poi_id,
        "name": name,
        "category": category,
        "x_norm": x_norm,
        "y_norm": y_norm,
        "captured_position": captured_pos,
        "has_captured_position": True,
        "captured_position_source": "workflow_a_editor",
        "captured_position_timestamp": 1785834802,
        "summary": "POI_" + poi_id,
        "status_pct": status_pct,
        "has_status": has_status,
        "status_unknown": status_unknown,
        "is_hero": is_hero,
        "hero_icon_key": "",
        "effect_mode": effect_mode,
        "rotate_contour": rotate_contour,
        "badge_category": badge_category,
        "status_level_key": status_level_key,
    }

# Build POIs
pois = []

# Main POI category/badge/outline for the 3 main POIs
MAIN_CAT = "royal_government"
MAIN_BADGE = "intact"
MAIN_OUTLINE = "intact"
MAIN_STATUS_PCT = 0.0

# 1. Add main POIs
for main_id in ["lamp", "painting", "camera"]:
    main = MAIN_POIS[main_id]
    pois.append(make_poi(
        main_id,
        main["name"],
        MAIN_CAT,
        main["x_norm"],
        main["y_norm"],
        main["captured_position"],
        main["effect_mode"],
        main["rotate_contour"],
        MAIN_BADGE,
        MAIN_OUTLINE,
        MAIN_STATUS_PCT,
        True,  # has_status
        False,  # status_unknown
        True,  # is_hero
    ))

# 2. Add satellites around each main POI
for main_id in ["lamp", "painting", "camera"]:
    main = MAIN_POIS[main_id]
    for category, direction, badge_cat, outline_key, status_pct, status_unknown in SATELLITE_CATEGORIES:
        sat_id = f"{main_id}_{category}"
        sat_name = f"{main['name']} - {category.replace('_', ' ').title()}"
        sat_pos = SATELLITE_POSITIONS[main_id][direction]
        sat_effect = SATELLITE_EFFECTS[main_id][direction]
        sat_rotate = SATELLITE_ROTATE[main_id][direction]

        pois.append(make_poi(
            sat_id,
            sat_name,
            category,
            main["x_norm"],  # same x_norm as parent (position resolved from captured_position)
            main["y_norm"],
            sat_pos,
            sat_effect,
            sat_rotate,
            badge_cat,
            outline_key,
            status_pct,
            True,  # has_status
            status_unknown,
            False,  # is_hero
        ))

# Build full config
config = {
    "wall_id": "living_room",
    "wall_name": "Living Room (Dev Test)",
    "immersal_map_id": 146267,
    "marker_style": "outline_gold",
    "marker_shape": "circle",
    "badge_shape": "",
    "marker_outline_mode": "gold",
    "marker_use_badge": True,
    "marker_icon_library_resources_path": "MarkerSymbols/living_room_IconLibrary",
    "pois": pois,
    "calibration_anchors": [
        {
            "id": "cal_left",
            "x_norm": 0.0,
            "y_norm": 0.5,
            "captured_position": {"x": 0.0, "y": 0.0, "z": 0.0}
        },
        {
            "id": "cal_right",
            "x_norm": 1.0,
            "y_norm": 0.5,
            "captured_position": {"x": 4.0, "y": 0.0, "z": 0.0}
        }
    ],
}

# Write with JsonUtility-compatible formatting (indented, no trailing commas)
# Use separators to avoid trailing whitespace, indent=2 for readability
output = json.dumps(config, indent=2, ensure_ascii=True, separators=(",", ": "))

# Fix: JsonUtility doesn't add space after colon in nested dict keys
# Actually, let's match the existing format exactly
# The existing format uses ": " (colon + space) and ",\n" (comma + newline)

# Also need to ensure captured_position objects are formatted correctly
# Let's do a custom serialization to match JsonUtility output

# Actually, let's just write it with indent=2 and ensure_ascii=True
# The format won't be byte-identical to JsonUtility but functionally equivalent

with open("config_new.json", "w") as f:
    f.write(output)

print(f"Generated config with {len(pois)} POIs")
print(f"POI IDs: {[p['id'] for p in pois]}")
for p in pois:
    print(f"  {p['id']}: category={p['category']}, badge={p['badge_category']}, outline={p['status_level_key']}, pct={p['status_pct']}, unknown={p['status_unknown']}")
