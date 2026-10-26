using UnityEngine;

namespace TileStories
{
    // Leader line of a displaced marker (spec _2.5 section 6): a thin world-space line from where the moved
    // element really belongs to where it is drawn now, in the marker's own category colour, so a visitor can
    // still tell which POI a shifted label belongs to. What moved and where comes from MarkerView
    // (TryGetLeaderLineEnds) every LateUpdate, because the billboard turns the marker every frame; the
    // displacement cycle only hands over the camera and settings (Refresh).
    [RequireComponent(typeof(LineRenderer))]
    public class MarkerLeaderLine : MonoBehaviour
    {
        // One dash + one gap per this many line widths ("dashed" style)
        public const float DashPeriodInWidths = 4f;

        private static Texture2D _dashTexture;
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        private LineRenderer _lr;
        private MarkerView _view;
        private MaterialPropertyBlock _block;
        private Camera _cam;
        private DisplacementSettings _settings;

        // True while the line is drawn (tests read it; the renderer's own flag says the same)
        public bool IsShown => _lr != null && _lr.enabled;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _view = GetComponent<MarkerView>();
            _lr.enabled = false; // hidden until a displacement cycle hands over its settings
            _lr.positionCount = 0;
            _lr.useWorldSpace = true;
            _lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lr.receiveShadows = false;
        }

        // Hand over the camera and settings of the latest displacement cycle; null settings hide the line
        public void Refresh(Camera cam, DisplacementSettings settings)
        {
            _cam = cam;
            _settings = settings;
            if (settings == null) Hide();
        }

        private void LateUpdate()
        {
            if (_settings == null || !_settings.leader_lines_enabled || _cam == null || _view == null
                || !_view.TryGetLeaderLineEnds(_cam, out Vector3 start, out Vector3 end, out float movedPx)
                || movedPx < _settings.leader_line_min_distance_px)
            {
                Hide();
                return;
            }
            Draw(start, end);
        }

        private void Hide()
        {
            if (_lr != null) _lr.enabled = false;
        }

        private void Draw(Vector3 start, Vector3 end)
        {
            float width = Mathf.Max(0.0005f, _settings.leader_line_width);
            Color color = _view.LeaderLineColor;
            color.a *= Mathf.Clamp01(_settings.leader_line_opacity);
            _lr.startColor = _lr.endColor = color;
            _lr.startWidth = _lr.endWidth = width;

            bool dashed = _settings.leader_line_style == "dashed";
            _block ??= new MaterialPropertyBlock();
            _lr.GetPropertyBlock(_block);
            _block.SetTexture(MainTexId, dashed ? DashTexture() : Texture2D.whiteTexture);
            _lr.SetPropertyBlock(_block);
            _lr.textureMode = dashed ? LineTextureMode.Tile : LineTextureMode.Stretch;
            // - Tile repeats the texture once per world unit; scale it so one dash+gap spans a few widths
            _lr.textureScale = dashed ? new Vector2(1f / (width * DashPeriodInWidths), 1f) : Vector2.one;

            if (_settings.leader_line_style == "elbow")
            {
                _lr.positionCount = 3;
                _lr.SetPosition(0, start);
                _lr.SetPosition(1, ElbowPoint(start, end, _cam.transform.up));
                _lr.SetPosition(2, end);
            }
            else
            {
                _lr.positionCount = 2;
                _lr.SetPosition(0, start);
                _lr.SetPosition(1, end);
            }
            _lr.enabled = true;
        }

        // Pure: the corner of an elbow line -- go straight up or down on screen first, then across, so the
        // bend is a right angle on screen whatever way the camera is turned
        public static Vector3 ElbowPoint(Vector3 start, Vector3 end, Vector3 cameraUp) =>
            start + cameraUp * Vector3.Dot(end - start, cameraUp);

        // Pure: how far from a rectangle's centre its edge is along a unit direction lying in its plane
        // (used to stop the line at the label's text edge instead of running through the text)
        public static float EdgeDistance(Vector3 direction, Vector3 rightUnit, Vector3 upUnit, float halfWidth, float halfHeight)
        {
            float alongRight = Mathf.Abs(Vector3.Dot(direction, rightUnit));
            float alongUp = Mathf.Abs(Vector3.Dot(direction, upUnit));
            float toSide = alongRight > 1e-5f ? halfWidth / alongRight : float.MaxValue;
            float toTop = alongUp > 1e-5f ? halfHeight / alongUp : float.MaxValue;
            float d = Mathf.Min(toSide, toTop);
            return d == float.MaxValue ? 0f : d;
        }

        // One opaque texel and one clear one, repeated along the line (created once, shared by every marker)
        private static Texture2D DashTexture()
        {
            if (_dashTexture != null) return _dashTexture;
            _dashTexture = new Texture2D(2, 1, TextureFormat.RGBA32, false)
            {
                name = "LeaderLineDash",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.DontSave,
            };
            _dashTexture.SetPixels(new[] { Color.white, Color.clear });
            _dashTexture.Apply();
            return _dashTexture;
        }
    }
}
