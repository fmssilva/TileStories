using System.Collections;
using UnityEngine;

namespace TileStories
{
    // Handles the initial reveal animation for a POI marker: waits revealDelaySeconds, then fades
    // alpha 0 -> resting alpha and scales localScale 0 -> resting scale over durationSeconds. The
    // resting values belong to MarkerView (LOD visibility, selection dim, crowding shrink/fade) and
    // may change mid-reveal: the animation always heads for the CURRENT resting values, so a reveal
    // never overwrites a later LOD decision with "fully visible". Only animates at runtime; in Edit
    // Mode (the POI Editor rig) the marker snaps to rest since coroutines do not tick there.
    [DisallowMultipleComponent]
    public class MarkerRevealEffect : MonoBehaviour
    {
        [Header("References (auto-resolved if unassigned)")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rootRect;

        private float _restAlpha = 1f;
        private float _restScale = 1f;

        // True while the reveal coroutine owns the root's alpha and scale.
        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_rootRect == null)
                _rootRect = GetComponent<RectTransform>();
        }

        // Play the reveal sequence: wait delaySeconds, then grow from nothing to the resting look over
        // durationSeconds. In Edit Mode, snap to the resting look immediately.
        public void Play(float delaySeconds, float durationSeconds)
        {
            ResolveReferences();
            if (!Application.isPlaying)
            {
                SnapToRest();
                return;
            }

            StopAllCoroutines();
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            if (_rootRect != null) _rootRect.localScale = Vector3.zero;
            IsPlaying = true;
            StartCoroutine(RevealCoroutine(delaySeconds, durationSeconds));
        }

        // Set the look the reveal ends on (and, when no reveal runs, the look itself)
        public void SetRest(float alpha, float scale)
        {
            _restAlpha = alpha;
            _restScale = scale;
            if (!IsPlaying) SnapToRest();
        }

        // Cut a running reveal short and land on the resting look (galleries and tests that need a
        // settled marker right away)
        public void SkipToEnd()
        {
            StopAllCoroutines();
            IsPlaying = false;
            SnapToRest();
        }

        // Jump straight to the resting look (Edit Mode, or once the reveal is over)
        public void SnapToRest()
        {
            ResolveReferences();
            if (_canvasGroup != null) _canvasGroup.alpha = _restAlpha;
            if (_rootRect != null) _rootRect.localScale = Vector3.one * _restScale;
        }

        private IEnumerator RevealCoroutine(float delaySeconds, float durationSeconds)
        {
            if (delaySeconds > 0f)
                yield return new WaitForSeconds(delaySeconds);

            float elapsed = 0f;
            while (elapsed < durationSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / durationSeconds);
                float smooth = t * t * (3f - 2f * t); // smoothstep

                // - read the rest values every frame: LOD may hide or shrink this marker mid-reveal
                if (_canvasGroup != null) _canvasGroup.alpha = _restAlpha * smooth;
                if (_rootRect != null) _rootRect.localScale = Vector3.one * (_restScale * smooth);
                yield return null;
            }

            IsPlaying = false;
            SnapToRest();
        }
    }
}
