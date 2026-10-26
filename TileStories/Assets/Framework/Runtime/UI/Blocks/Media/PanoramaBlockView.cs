using UnityEngine;

namespace TileStories
{
    // The panorama_360 block (_3.1 step 10A.4; everything the stage, the teaser and the takeover share is PreviewBlockView). The card's
    // 360 picture is drawn from inside a sphere; the view state (PanoramaViewState) is where it looks. Drag look: a one-finger drag
    // turns the view so the scene follows the finger (the drag is scaled by the field of view and the stage's shorter side, so a drag across
    // the stage turns the view by exactly the field of view), two fingers pinch the field of view within PanoramaViewRule's limits.
    // Gyro look: the phone's own attitude (DeviceAttitude, through the Input System) turns the view, anchored so the direction the
    // visitor faces when the viewer appears is the authored Start Heading; where the device has no attitude sensor (the Editor) it
    // behaves as the drag look, and its hint says so. The timer only reads the sensor; a render is asked for only when the view moved.
    public sealed class PanoramaBlockView : PreviewBlockView
    {
        public PanoramaViewState State { get; private set; } = PanoramaViewState.Start;
        // What the surface is drawing now (the panorama's real frames, not the fallback picture)
        public bool ShowsPanorama => ShowsSubject;
        // The second instance drawn full screen while the takeover is open (null otherwise)
        public PanoramaBlockView FullScreenView => (PanoramaBlockView)FullScreen;
        // The bound look is Gyro (whether or not the device has the sensor)
        public bool IsGyroLook { get; private set; }
        // The Gyro look AND a sensor was read on the last tick: drags are then ignored, the phone turns the view
        public bool IsFollowingSensor { get; private set; }
        // The authored Start Heading of the bound instance, degrees turned right from the picture's middle
        public float StartHeading { get; private set; }

        // A sensor reading that moved the view by less than this many degrees is noise: no new frame (a still phone draws nothing)
        private const float SensorNoiseDegrees = 0.05f;

        private PanoramaGyroFollower _follower = new(0f);
        private bool _attitudeBegun;

        // `modifierClass`: the instance's place ("card-preview--full" for the full-screen page)
        public PanoramaBlockView(string modifierClass = null) : base("card-panorama", modifierClass) { }

        protected override BlockKindDefinition Kind => BuiltInBlocks.Panorama360;
        protected override MediaKind SubjectKind => MediaKind.Panorama;
        protected override string SubjectField => BuiltInBlocks.Panorama360PanoramaField;
        protected override string FallbackField => BuiltInBlocks.Panorama360FallbackField;
        protected override string TitleField => BuiltInBlocks.Panorama360TitleField;
        protected override bool WantsTicks => IsGyroLook;

        protected override void ResetState(BlockFieldReader read)
        {
            IsGyroLook = Context.Variant == BuiltInBlocks.Panorama360Gyro;
            IsFollowingSensor = false;
            StartHeading = Mathf.Repeat(read.Number(Kind.Field(BuiltInBlocks.Panorama360StartHeadingField)), 360f);
            State = new PanoramaViewState(StartHeading, 0f, PanoramaViewState.DefaultFov);
            _follower = new PanoramaGyroFollower(StartHeading);
        }

        // The sensor draws power only while somebody follows it: on while a gyro look is bound (never for the static teaser)
        protected override void OnBound()
        {
            if (!IsGyroLook) return;
            DeviceAttitude.Begin();
            _attitudeBegun = true;
        }

        protected override void OnUnbinding()
        {
            if (!_attitudeBegun) return;
            DeviceAttitude.End();
            _attitudeBegun = false;
        }

        protected override string HintText(CardStrings strings) =>
            strings?.Get(IsGyroLook && DeviceAttitude.Available ? CardStrings.Keys.Panorama360GyroHint : CardStrings.Keys.Panorama360DragHint);

        protected override string LoadingText(CardStrings strings) => strings?.Get(CardStrings.Keys.Panorama360Loading);
        protected override PreviewBlockView NewFullScreenView() => new PanoramaBlockView("card-preview--full");
        protected override void RenderSlot(ICardPreviewSlot slot) => slot.RenderNow(default, State);

        // The scene follows the finger: the stage's shorter side is the field of view, so a drag of N pixels turns the view by
        // N * fov / shorter side degrees. With a sensor the phone decides, not the finger.
        protected override void OnDrag(Vector2 delta)
        {
            if (IsFollowingSensor) return;
            State = PanoramaViewRule.Drag(State, delta, PanoramaViewRule.DegreesPerUnit(State.Fov, StageShorterSide));
        }

        protected override void OnPinch(float factor) => State = PanoramaViewRule.Pinch(State, factor);

        protected override bool OnTick(float deltaSeconds) => PollAttitude();

        // Read the phone's attitude and move the view to it; true when the view moved enough to need a new frame. A timer tick calls it;
        // a test with a simulated sensor calls it directly. No sensor (or not the Gyro look): false, and the drag look is what is left.
        public bool PollAttitude()
        {
            bool have = false;
            Quaternion attitude = Quaternion.identity;
            if (IsGyroLook) have = DeviceAttitude.TryRead(out attitude);
            if (have != IsFollowingSensor)
            {
                IsFollowingSensor = have;
                if (!have) _follower.Reset();
                RefreshHint();
            }
            if (!have) return false;

            var next = _follower.Follow(State, attitude);
            bool moved = Mathf.Abs(Mathf.DeltaAngle(next.Yaw, State.Yaw)) > SensorNoiseDegrees || Mathf.Abs(next.Pitch - State.Pitch) > SensorNoiseDegrees;
            if (moved) State = next;
            return moved;
        }
    }
}
