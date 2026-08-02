$path = 'TileStories\Assets\Framework\Editor\POIAuthoringToolWindow.cs'
$content = [System.IO.File]::ReadAllText($path)

$oldText = 'Undo.RegisterCreatedObjectUndo(instance, $"Populate marker for {poi.id}");

                Debug.Log($"[POIAuthoring] Created marker'

$newText = 'Undo.RegisterCreatedObjectUndo(instance, $"Populate marker for {poi.id}");

                // Configure the marker instance with its real category/style/shape/effects
                // so the Scene view shows the actual marker, not the prefab raw default.
                var anchor = instance.GetComponentInChildren<POIAnchor>() ?? instance.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var markerView = instance.GetComponentInChildren<MarkerView>();
                if (markerView != null)
                {
                    var style = MarkerVisualsParser.ParseStyle(_config.marker_style);
                    var shape = MarkerVisualsParser.ParseShape(_config.marker_shape);
                    var effects = MarkerVisualsParser.ParseEffectFlags(poi.effect_mode);
                    markerView.Initialise(anchor, style, shape, effects);
                }

                Debug.Log($"[POIAuthoring] Created marker'

if ($content.Contains($oldText)) {
    $content = $content.Replace($oldText, $newText)
    [System.IO.File]::WriteAllText($path, $content)
    Write-Host 'PopulateRig Initialise added successfully'
} else {
    Write-Host 'Search text not found - may already be updated'
}