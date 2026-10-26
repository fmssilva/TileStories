using UnityEngine;

namespace TileStories
{
    // Common contract for opt-in marker effects. MarkerView only ever talks to
    // markers through this interface -- a prefab variant with none of these
    // components attached simply never animates; nothing else changes.
    public abstract class MarkerEffect : MonoBehaviour
    {
        // Shortest cycle any effect accepts. A config period of 0 (or less) would divide by
        // zero in the animation maths and produce NaN scales; [Min]/[Range] attributes only
        // guard the Inspector, not values assigned from config, so ApplyDefaults clamps to this.
        protected const float MinPeriodSeconds = 0.1f;

        // Layer sizes are multiples of the symbol's diameter (not fixed metres), so an effect keeps its look on
        // a 7 cm and on a 30 cm marker. Read at SetActive time, after MarkerView has sized the symbol.
        protected virtual RectTransform SizeReference => null;
        protected float SymbolDiameter => SizeReference != null ? SizeReference.sizeDelta.x : 0.2f;

        public abstract void SetActive(bool active);
    }
}