using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The motion of one composed block's rows (_3.3 R17.3, R6, R11 v4): ONE driver per composed view, run on its clock, writing only
    // opacity, translate and scale (never a layout property -- the AR camera runs beside the card). ElementMotionRule decides every number:
    //   - an entering row (reveal / slide in) waits hidden until its top crosses the enter line of the view it scrolls in, then fades in and
    //     travels to its place; rows entering together follow one by one (the block's own entry -- the preview rising onto the card -- is one
    //     more entering row)
    //   - a picture with parallax moves inside its frame with the scroll; a slow zoom or a pan moves by itself, and only ONE such row plays at
    //     a time in view (ElementMotionRule.Mover): the others stop where they are, off-screen ones included
    //   - Reduce Motion: everything still, at rest from the first frame (the rule, not a per-row switch)
    // The view it measures against is the nearest ScrollView around the block (the card's stack, the full view's page, a world panel's own
    // scroll), found on the panel it is drawn on: it never names the card's sheet (R15.2). Plain C#, owned by ComposedBlockView.
    public sealed class ElementMotion
    {
        // A row whose pictures move wears this: its pictures overhang their frames on every side (Components.uss), so nothing shows an edge
        public const string MovesPictureClass = "ts-el--moves-picture";

        // One moving thing: a row (or the block itself), what moves of it, its effect and strength, and where it stands
        private sealed class Moving
        {
            public VisualElement Root;
            public IReadOnlyList<VisualElement> Pictures;
            public string Effect;
            public string Strength;
            public bool Entered;
            public float Start;
            // How long a slow zoom / pan has played (it pauses while another row is the mover)
            public float Played;
        }

        // The time motion reads, in seconds (real time unless a test hands its own)
        public Func<float> Clock { get; set; } = () => Time.realtimeSinceStartup;
        public bool ReduceMotion { get; private set; }
        // How many things are bound to move (rows and the block's entry), and the row playing its slow zoom / pan now (null: none)
        public int Count => _moving.Count;
        public VisualElement Mover { get; private set; }

        private readonly VisualElement _owner;
        private readonly List<Moving> _moving = new();
        private readonly IVisualElementScheduledItem _tick;
        private ScrollView _scroll;
        private float _last = float.NaN;

        public ElementMotion(VisualElement owner)
        {
            _owner = owner;
            _tick = owner.schedule.Execute(Tick).Every(16);
            _tick.Pause();
            owner.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                _scroll = owner.GetFirstAncestorOfType<ScrollView>();
                Run();
            });
            owner.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                _tick.Pause();
                _scroll = null;
            });
        }

        // A new binding: nothing moves yet; `reduceMotion` = everything still
        public void Bind(bool reduceMotion)
        {
            Unbind();
            ReduceMotion = reduceMotion;
        }

        // `root` moves with `effect` (ElementMotionRule.EffectOf's answer) at `strength`; `pictures` are the pictures it moves inside their
        // frames (a parallax / zoom / pan), none for a row that only enters
        public void Add(VisualElement root, IReadOnlyList<VisualElement> pictures, string effect, string strength)
        {
            if (root == null || ReduceMotion || effect == null || effect == ElementOptions.MotionNone) return;
            bool movesPicture = ElementMotionRule.MovesPicture(effect) && pictures != null && pictures.Count > 0;
            if (!ElementMotionRule.Enters(effect) && !movesPicture) return;
            root.EnableInClassList(MovesPictureClass, movesPicture);
            _moving.Add(new Moving { Root = root, Pictures = movesPicture ? pictures : Array.Empty<VisualElement>(), Effect = effect, Strength = strength });
        }

        // Every row added: draw the first frame now (an entering row not yet in view hidden before it is ever painted), then run
        public void Start()
        {
            _last = float.NaN;
            if (_owner.panel != null)
            {
                _scroll = _owner.GetFirstAncestorOfType<ScrollView>();
                Tick();
            }
            Run();
        }

        // The binding ends: every row at rest (its component goes back to its pool), nothing ticks
        public void Unbind()
        {
            foreach (var m in _moving) Rest(m);
            _moving.Clear();
            Mover = null;
            _last = float.NaN;
            _tick.Pause();
        }

        // Settle every entering row at its place now, as if each had entered long ago (what a test asks before it judges a render)
        public void Finish()
        {
            float now = Clock();
            foreach (var m in _moving)
            {
                m.Entered = true;
                m.Start = now - ElementMotionRule.RevealDuration - ElementMotionRule.StaggerDelay(ElementMotionRule.MaxStaggerSteps);
            }
            Tick();
        }

        // One frame at Clock's time (also what a test calls after moving its clock)
        public void Tick()
        {
            if (_moving.Count == 0) return;
            var view = _scroll != null ? _scroll.contentViewport : _owner.panel?.visualTree;
            float height = view?.layout.height ?? float.NaN;
            float now = Clock();
            float step = float.IsNaN(_last) ? 0f : Mathf.Clamp(now - _last, 0f, 0.1f);
            _last = now;

            int entering = 0;
            var movers = new List<(bool, float, float)>(_moving.Count);
            var boxes = new List<Rect>(_moving.Count);
            foreach (var m in _moving)
            {
                // - the row's own box, never its drawn one: its own translate must not move the line it is judged against
                var box = view != null && m.Root.parent != null ? m.Root.parent.ChangeCoordinatesTo(view, m.Root.layout) : new Rect(float.NaN, float.NaN, 0f, 0f);
                boxes.Add(box);
                float visible = float.IsNaN(box.height) || box.height <= 0f || float.IsNaN(height) ? 0f
                    : Mathf.Clamp01((Mathf.Min(box.yMax, height) - Mathf.Max(box.yMin, 0f)) / box.height);
                movers.Add((ElementMotionRule.MovesByItself(m.Effect), visible, box.center.y - height * 0.5f));
                if (!ElementMotionRule.Enters(m.Effect)) continue;
                if (!m.Entered && ElementMotionRule.Entered(box.yMin, box.yMax, height))
                {
                    m.Entered = true;
                    m.Start = now + ElementMotionRule.StaggerDelay(entering++);
                }
                float progress = m.Entered ? ElementMotionRule.RevealProgress(now - m.Start) : 0f;
                var (opacity, offset) = ElementMotionRule.EnterPose(m.Effect, m.Strength, progress);
                m.Root.style.opacity = opacity;
                m.Root.style.translate = offset == Vector2.zero ? new StyleTranslate(StyleKeyword.Null) : new Translate(offset.x, offset.y);
            }

            int mover = ElementMotionRule.Mover(movers);
            Mover = mover >= 0 ? _moving[mover].Root : null;
            for (int i = 0; i < _moving.Count; i++)
            {
                var m = _moving[i];
                if (m.Pictures.Count == 0) continue;
                if (m.Effect == ElementOptions.MotionParallax)
                {
                    foreach (var picture in m.Pictures)
                    {
                        float room = RoomOf(picture).y;
                        float offset = ElementMotionRule.ParallaxOffset(boxes[i].center.y, height * 0.5f, m.Strength, room);
                        picture.style.translate = new Translate(0f, offset);
                    }
                    continue;
                }
                // - a slow zoom / pan plays only while it is THE mover; the others hold where they stopped
                if (i != mover) continue;
                m.Played += step;
                foreach (var picture in m.Pictures)
                {
                    if (m.Effect == ElementOptions.MotionSlowZoom)
                    {
                        float scale = ElementMotionRule.ZoomScale(m.Played, m.Strength);
                        picture.style.scale = new Scale(new Vector2(scale, scale));
                    }
                    else picture.style.translate = new Translate(ElementMotionRule.PanOffset(m.Played, m.Strength, RoomOf(picture).x), 0f);
                }
            }
        }

        private void Run()
        {
            if (_owner.panel != null && _moving.Count > 0 && !ReduceMotion) _tick.Resume();
            else _tick.Pause();
        }

        // How far a picture overhangs its frame on each side (x, y), from their laid-out sizes
        private static Vector2 RoomOf(VisualElement picture)
        {
            var frame = picture.parent;
            if (frame == null || float.IsNaN(picture.layout.width) || float.IsNaN(frame.layout.width)) return Vector2.zero;
            return new Vector2(Mathf.Max(0f, (picture.layout.width - frame.layout.width) * 0.5f), Mathf.Max(0f, (picture.layout.height - frame.layout.height) * 0.5f));
        }

        private static void Rest(Moving m)
        {
            m.Root.style.opacity = StyleKeyword.Null;
            m.Root.style.translate = StyleKeyword.Null;
            m.Root.RemoveFromClassList(MovesPictureClass);
            foreach (var picture in m.Pictures)
            {
                picture.style.translate = StyleKeyword.Null;
                picture.style.scale = StyleKeyword.Null;
            }
        }
    }
}
