using UnityEngine;

namespace TileStories
{
    // Leader line component for displaced markers (spec _2.5 Section 6).
    // Draws a line from the marker's current (displaced) position back to
    // its true baseline position, using the marker's resolved category color.
    // Positioned as a sibling of MarkerView on the POI_Marker root so it
    // shares the billboarded rotation from MarkerBillboard.
    [RequireComponent(typeof(LineRenderer))]
    public class MarkerLeaderLine : MonoBehaviour
    {
        [Header("Rendering")]
        [Tooltip("Width of the leader line in world units.")]
        [SerializeField] private float _lineWidthWorld = 0.01f;

        // Runtime state set by MarkerView.Configure()
        private Vector3 _baselinePosition;
        private Color _categoryColor = Color.gray;
        private bool _isConfigured;

        // Per-cycle state set by MarkerView.UpdateLeaderLine()
        private Camera _cam;
        private DisplacementSettings _settings;

        private LineRenderer _lr;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            if (_lr != null)
            {
                _lr.positionCount = 0;
                _lr.enabled = false; // hidden until Evaluate() decides to show
                _lr.startWidth = _lineWidthWorld;
                _lr.endWidth = _lineWidthWorld;
                _lr.useWorldSpace = true;
            }
        }

        // Store the baseline (true) position and resolved category color.
        // Called by MarkerView.UpdateLeaderLine whenever displacement settings
        // or the marker's visual state changes.
        public void Configure(Vector3 baseline, Color color)
        {
            _baselinePosition = baseline;
            _categoryColor = color;
            _isConfigured = true;
        }

        // Provide the camera and current displacement settings for per-frame
        // visibility evaluation. Called on every ApplyDisplacement cycle.
        public void UpdateVisibility(Camera cam, DisplacementSettings settings)
        {
            _cam = cam;
            _settings = settings;
        }

        private void LateUpdate()
        {
            Evaluate();
        }

        // Extracted from LateUpdate so Tier-0 EditMode tests can invoke it
        // directly without a running scene/cycle.
        public void Evaluate()
        {
            if (_lr == null) _lr = GetComponent<LineRenderer>();
            if (!_isConfigured || _lr == null || _cam == null || _settings == null)
            {
                if (_lr != null) _lr.enabled = false;
                return;
            }

            if (!_settings.leader_lines_enabled)
            {
                _lr.enabled = false;
                return;
            }

            Vector3 displaced = transform.position;

            // Compute screen-space distance using viewport coordinates (normalized 0-1),
            // which work in EditMode where pixelWidth/pixelHeight may be 0.
            Vector3 vpDisplaced = _cam.WorldToViewportPoint(displaced);
            Vector3 vpBaseline = _cam.WorldToViewportPoint(_baselinePosition);
            float viewportDist = Vector2.Distance(
                new Vector2(vpDisplaced.x, vpDisplaced.y),
                new Vector2(vpBaseline.x, vpBaseline.y));
            float pixelHeight = _cam.pixelHeight > 0 ? _cam.pixelHeight : 1080f;
            float screenDist = viewportDist * pixelHeight;

            // Only draw when displacement exceeds the configured pixel threshold.
            if (screenDist < _settings.leader_line_min_distance_px)
            {
                _lr.enabled = false;
                return;
            }

            _lr.enabled = true;
            float lineWidth = _settings.leader_line_width > 0f ? _settings.leader_line_width : _lineWidthWorld;
            Color lineColor = _categoryColor;
            lineColor.a *= Mathf.Clamp01(_settings.leader_line_opacity);
            _lr.startColor = lineColor;
            _lr.endColor = lineColor;
            _lr.startWidth = lineWidth;
            _lr.endWidth = lineWidth;

            ApplyLinePositions(displaced);
        }

        // Set LineRenderer vertex count and positions based on the configured style.
        // straight/dashed: 2 vertices. elbow: 3 vertices with a right-angle bend.
        private void ApplyLinePositions(Vector3 displaced)
        {
            switch (_settings.leader_line_style)
            {
                case "dashed":
                    // Same geometry as straight; dash appearance is a material/texture property
                    // (set on the LineRenderer's material, not the positions).
                    _lr.textureMode = LineTextureMode.Tile;
                    _lr.positionCount = 2;
                    _lr.SetPosition(0, displaced);
                    _lr.SetPosition(1, _baselinePosition);
                    break;

                case "elbow":
                    // Right-angle L-shape: horizontal offset first (along world X/Z),
                    // then vertical drop to the baseline.
                    Vector3 bend = new Vector3(_baselinePosition.x, displaced.y, _baselinePosition.z);
                    _lr.positionCount = 3;
                    _lr.SetPosition(0, displaced);
                    _lr.SetPosition(1, bend);
                    _lr.SetPosition(2, _baselinePosition);
                    break;

                case "straight":
                default:
                    _lr.textureMode = LineTextureMode.Stretch;
                    _lr.positionCount = 2;
                    _lr.SetPosition(0, displaced);
                    _lr.SetPosition(1, _baselinePosition);
                    break;
            }
        }

        // Test seams (InternalsVisibleTo -> TileStories.Tests.Runtime)
        internal bool IsLineEnabled => _lr != null ? _lr.enabled : false;
        internal int CurrentPositionCount => _lr != null ? _lr.positionCount : 0;
        internal Vector3 BaselinePos => _baselinePosition;
        internal bool IsConfigured => _isConfigured;
    }
}
