import os

path = r'C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Editor\POIAuthoring\RigLifecycle\POIAuthoringToolWindow.RigLifecycle.cs'

with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

old = '''private void CapturePositions(bool silentWhenRigMissing = false)
        {
            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
            {
                if (!silentWhenRigMissing)
                    Debug.LogWarning("[POIAuthoring] No POIAuthoringRig with children found. Populate first.");
                return;
            }

            int captured = 0;
            int skipped = 0;
            var sceneObjects = new Dictionary<string, Transform>();

            for (int i = 0; i < rig.childCount; i++)
            {
                var child = rig.GetChild(i);
                sceneObjects[child.name] = child;
            }

            foreach (var poi in _config.pois)
            {
                if (!sceneObjects.TryGetValue(poi.id, out var markerTransform))
                {
                    skipped++;
                    continue;
                }

                Vector3 localPos;
                if (_correctionAnchor != null)
                    localPos = _correctionAnchor.InverseTransformPoint(markerTransform.position);
                else
                    localPos = markerTransform.localPosition;

                poi.captured_position = new CapturedPosition
                {
                    x = localPos.x,
                    y = localPos.y,
                    z = localPos.z
                };
                poi.has_captured_position = true;
                poi.captured_position_source = "workflow_a_editor";
                poi.captured_position_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                captured++;
            }

            _hasUnsavedChanges = true;
            Debug.Log($"[POIAuthoring] Captured {captured} positions (skipped {skipped} missing scene objects).");
            Repaint();
        }'''

new = '''private void CapturePositions(bool silentWhenRigMissing = false)
        {
            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
            {
                if (!silentWhenRigMissing)
                    Debug.LogWarning("[POIAuthoring] No POIAuthoringRig with children found. Populate first.");
                return;
            }

            int captured = 0;
            int skipped = 0;
            var sceneObjects = new Dictionary<string, Transform>();

            for (int i = 0; i < rig.childCount; i++)
            {
                var child = rig.GetChild(i);
                sceneObjects[child.name] = child;
            }

            foreach (var poi in _config.pois)
            {
                if (!sceneObjects.TryGetValue(poi.id, out var markerTransform))
                {
                    skipped++;
                    continue;
                }

                // Rig is now at origin, so localPosition == world position
                Vector3 localPos = markerTransform.localPosition;

                poi.captured_position = new CapturedPosition
                {
                    x = localPos.x,
                    y = localPos.y,
                    z = localPos.z
                };
                poi.has_captured_position = true;
                poi.captured_position_source = "workflow_a_editor";
                poi.captured_position_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                captured++;
            }

            _hasUnsavedChanges = true;
            Debug.Log($"[POIAuthoring] Captured {captured} positions (skipped {skipped} missing scene objects).");
            Repaint();
        }'''

if old in content:
    content = content.replace(old, new)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print('Updated CapturePositions')
else:
    print('Old text not found')