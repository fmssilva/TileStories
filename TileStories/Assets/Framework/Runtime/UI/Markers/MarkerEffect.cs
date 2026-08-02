using UnityEngine;

namespace TileStories
{
    // Common contract for opt-in marker effects. MarkerView only ever talks to
    // markers through this interface -- a prefab variant with none of these
    // components attached simply never animates; nothing else changes.
    public abstract class MarkerEffect : MonoBehaviour
    {
        public abstract void SetActive(bool active);
    }
}