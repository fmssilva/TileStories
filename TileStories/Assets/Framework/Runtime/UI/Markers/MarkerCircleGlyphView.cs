using UnityEngine;
using UnityEngine.UI;

namespace TileStories
{
    // Renders one "coloured shape + centred icon" element. Reused at two
    // sizes/positions in a marker: the main Symbol (always present) and the
    // optional Badge (MarkerStyle.Badge only). This view makes no decisions -- it
    // only paints what MarkerView tells it to.
    public class MarkerCircleGlyphView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField, Range(0.35f, 0.9f)] private float iconSizeRatio = 0.56f;

        public RectTransform RectTransform => (RectTransform)transform;

        private void OnValidate()
        {
            BindExistingIconReference();
            ConfigureIconRect();
        }

        public void SetBackground(Sprite shapeSprite, Color color)
        {
            if (background == null) return;
            if (shapeSprite != null)
                background.sprite = shapeSprite;
            background.color = color;
        }

        public void SetIcon(Sprite iconSprite, Color tint, float opacity = 1f)
        {
            EnsureIconReference();
            if (icon == null) return;
            icon.enabled = iconSprite != null;
            if (iconSprite != null)
                icon.sprite = iconSprite;
            var c = tint;
            c.a = opacity;
            icon.color = c;
            ConfigureIconRect();
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public Image EnsureIconReference()
        {
            if (icon != null)
            {
                ConfigureIconRect();
                return icon;
            }

            if (BindExistingIconReference())
            {
                ConfigureIconRect();
                return icon;
            }

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rectTransform = (RectTransform)iconObject.transform;
            rectTransform.SetParent(transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            icon = iconObject.GetComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            ConfigureIconRect();
            return icon;
        }

        private bool BindExistingIconReference()
        {
            var existing = transform.Find("Icon");
            if (existing == null)
                return false;

            icon = existing.GetComponent<Image>();
            if (icon != null)
            {
                icon.preserveAspect = true;
            }
            return icon != null;
        }

        // Keep icon smaller than the base shape so the symbol remains visible underneath.
        private void ConfigureIconRect()
        {
            if (icon == null)
                return;

            float half = Mathf.Clamp01(iconSizeRatio) * 0.5f;
            var rt = (RectTransform)icon.transform;
            rt.anchorMin = new Vector2(0.5f - half, 0.5f - half);
            rt.anchorMax = new Vector2(0.5f + half, 0.5f + half);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
    }
}