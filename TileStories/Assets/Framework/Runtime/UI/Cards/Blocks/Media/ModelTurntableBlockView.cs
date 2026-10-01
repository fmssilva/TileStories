using UnityEngine;

namespace TileStories
{
    // The model_3d block (_3.1 step 10A.2b.3, Display Takeover in 10A.3.1; everything the stage, the teaser and the takeover share is
    // PreviewBlockView). Display Inline draws the card's model into a RenderTexture; a one-finger drag rotates it (TurntableRule), two
    // fingers pinch to zoom within limits, and -- when Auto Spin is on -- it turns on its own after a pause with no touch, the same
    // pause a touch resets. A render is asked for only after a real gesture or an auto-spin tick that actually moved the model.
    public sealed class ModelTurntableBlockView : PreviewBlockView
    {
        public TurntableState State { get; private set; } = TurntableState.Start;
        // What the surface is drawing now (the model's real frames, not the fallback picture)
        public bool ShowsModel => ShowsSubject;
        // The second instance drawn full screen while the takeover is open (null otherwise)
        public ModelTurntableBlockView FullScreenView => (ModelTurntableBlockView)FullScreen;

        // How many screen pixels of drag turn the model one degree: a plain gesture-sensitivity constant, not a "look"
        private const float DegreesPerPixel = 0.3f;

        private bool _autoSpin;
        // How the model is framed (the block's Fit, else the Block Library's default for model_3d): handed to the slot on every bind
        public ModelFitMode Fit { get; private set; }

        // `modifierClass`: the instance's place ("card-preview--full" for the full-screen page)
        public ModelTurntableBlockView(string modifierClass = null) : base("card-model3d", modifierClass) { }

        protected override BlockKindDefinition Kind => BuiltInBlocks.Model3D;
        protected override MediaKind SubjectKind => MediaKind.Model;
        protected override string SubjectField => BuiltInBlocks.Model3DModelField;
        protected override string FallbackField => BuiltInBlocks.Model3DFallbackField;
        protected override string TitleField => BuiltInBlocks.Model3DTitleField;
        protected override bool WantsTicks => _autoSpin;

        protected override void ResetState(BlockFieldReader read)
        {
            _autoSpin = read.Flag(BuiltInBlocks.Model3DAutoSpinField);
            State = TurntableState.Start;
            Fit = ModelFitRule.ModeOf(BlockLibraryRule.Choice(Context?.Taxonomy?.card_settings, BuiltInBlocks.Model3D,
                BuiltInBlocks.Model3D.Field(BuiltInBlocks.Model3DFitField), read));
        }

        protected override void OnSlotRequested(ICardPreviewSlot slot) => slot?.SetFit(Fit);

        protected override string HintText(CardStrings strings) => strings?.Get(CardStrings.Keys.Model3DHint);
        protected override string LoadingText(CardStrings strings) => strings?.Get(CardStrings.Keys.Model3DLoading);
        protected override PreviewBlockView NewFullScreenView() => new ModelTurntableBlockView("card-preview--full");
        protected override void RenderSlot(ICardPreviewSlot slot) => slot.RenderNow(State, default);
        protected override void OnDrag(Vector2 delta) => State = TurntableRule.Drag(State, delta, DegreesPerPixel);
        protected override void OnPinch(float factor) => State = TurntableRule.Pinch(State, factor);

        // Auto Spin: the idle clock grows while nothing touches the stage, and past TurntableRule.AutoSpinResumeAfter the model turns
        protected override bool OnTick(float deltaSeconds)
        {
            if (IsTouched) return false;
            float before = State.Yaw;
            State = TurntableRule.Idle(State, deltaSeconds);
            return !Mathf.Approximately(before, State.Yaw);
        }
    }
}
