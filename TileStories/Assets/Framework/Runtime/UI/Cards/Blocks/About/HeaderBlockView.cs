using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The header block (_3.1 Tiers 1-2): the title and the category chip (the category; "category - level" with Show
    // Level, PoiSubtitle) in its TOP part -- all the peek stop shows -- and the subtitle below it. Variant "compact" is
    // the top part only. The picture looks add a HERO: a picture under the pinned header, at the top of what scrolls
    // (BlockStackView places it), so the title stays in view while the picture scrolls away:
    //   image_parallax -- the picture slides at half the scroll's speed inside its frame (OnStackScrolled)
    //   split_then_now -- Picture (then) beside Second Picture (now), each with its CardStrings label
    //   spotlight_crop -- Picture enlarged around the focus point with a ring on it (SpotlightCropRule)
    //   video_loop     -- Picture as a poster, the Loop Clip's frames over it: the header asks the card's ONE video owner for its muted loop
    //                     (ICardVideo.PlayLoop) and shows the frames only while that loop holds the player and has a frame -- the poster
    //                     shows until the first frame, while a visitor's video holds the player, and always with ReduceMotion (no loop asked)
    // A picture look without what it needs (HeaderShowsPicture) shows no hero: the text-only look.
    // Only classes here; About.uss / Media.uss hold every size and colour.
    public sealed class HeaderBlockView : IBlockView
    {
        public VisualElement Root { get; }
        // The part the peek stop shows (PoiCardSheetView measures its bottom edge)
        public VisualElement PeekPart { get; }
        // The picture under the header (the picture looks); BlockStackView puts it first in the scroll while HasHero
        public VisualElement HeroPart { get; }
        public bool HasHero { get; private set; }
        public CardImage Picture { get; }
        public CardImage SecondPicture { get; }
        public Label ThenLabel { get; }
        public Label NowLabel { get; }
        public VisualElement Ring { get; }
        // video_loop: the loop's frames over the poster, the loop this header asks for, and whether its frames show now
        public VisualElement LoopFrames { get; }
        public VideoTrack LoopTrack { get; private set; }
        public bool LoopShowsFrames { get; private set; }
        // model_turntable (10A.3.2): the SAME turntable view a model_3d block uses, embedded in the hero -- built once,
        // added/removed from HeroPart as the variant changes, so drag/pinch/auto-spin never need re-implementing here
        public ModelTurntableBlockView ModelView { get; }

        // image_parallax: how far the picture lags behind the scroll (0.5 = half the speed)
        public const float ParallaxFactor = 0.5f;

        private readonly Label _title;
        private readonly Label _chip;
        private readonly Label _subtitle;
        private string _variant;
        private Vector2 _focus;
        private float _zoom = 1f;
        // The frame size the spotlight was last placed for: a layout pass that did not change it places nothing
        private Vector2 _placedFor;
        private ICardVideo _video;

        public HeaderBlockView()
        {
            Root = new VisualElement { name = "card-header" };
            Root.AddToClassList("card-header");
            PeekPart = new VisualElement { name = "card-header-top" };
            PeekPart.AddToClassList("card-header__top");
            _title = new Label { name = "card-header-title" };
            _title.AddToClassList("card-header__title");
            _chip = new Label { name = "card-header-chip" };
            _chip.AddToClassList("card-chip");
            _subtitle = new Label { name = "card-header-subtitle" };
            _subtitle.AddToClassList("card-header__subtitle");
            PeekPart.Add(_title);
            PeekPart.Add(_chip);
            Root.Add(PeekPart);
            Root.Add(_subtitle);

            HeroPart = new VisualElement { name = "card-header-hero" };
            HeroPart.AddToClassList("card-hero");
            Picture = new CardImage("card-hero__picture");
            SecondPicture = new CardImage("card-hero__picture");
            SecondPicture.Root.AddToClassList("card-hero__picture--now");
            ThenLabel = new Label { pickingMode = PickingMode.Ignore };
            ThenLabel.AddToClassList("card-hero__label");
            NowLabel = new Label { pickingMode = PickingMode.Ignore };
            NowLabel.AddToClassList("card-hero__label");
            Ring = new VisualElement { pickingMode = PickingMode.Ignore };
            Ring.AddToClassList("card-hero__ring");
            LoopFrames = new VisualElement { name = "card-hero-loop", pickingMode = PickingMode.Ignore };
            LoopFrames.AddToClassList("card-hero__loop");
            ModelView = new ModelTurntableBlockView();
            ModelView.Root.AddToClassList("card-hero__model");
            HeroPart.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                // - only a new frame size re-places the picture (its own new size must never feed back into the frame's)
                if (evt.newRect.size != evt.oldRect.size) PlaceSpotlight();
            });
        }

        public string TitleText => _title.text;
        public string ChipText => _chip.text;
        public string SubtitleText => _subtitle.text;
        // Laid out as shown: the compact variant, an empty subtitle and the collapsed header (a scrolled stack) all hide it
        public bool SubtitleShown => _subtitle.resolvedStyle.display != DisplayStyle.None;
        // image_parallax: the picture's current slide inside its frame (panel units, down)
        public float ParallaxOffset { get; private set; }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string title = read.Text(BlockStackBuilder.HeaderTitleField);
            _title.text = title.Length > 0 ? title : context.Poi?.name ?? "";

            _chip.text = PoiSubtitle.Of(context.Poi, context.Taxonomy, read.Flag(BuiltInBlocks.HeaderShowLevelField));
            // - a class, not an inline display: the collapsed header hides the chip through USS too
            _chip.EnableInClassList("card-chip--empty", _chip.text.Length == 0);

            bool compact = context.Variant == BuiltInBlocks.HeaderCompact;
            Root.EnableInClassList("card-header--compact", compact);
            _subtitle.text = read.Text(BlockStackBuilder.HeaderSubtitleField);
            // - classes only, no inline display: the collapsed header hides the subtitle through USS, which an inline
            //   display would override
            _subtitle.EnableInClassList("card-header__subtitle--empty", _subtitle.text.Length == 0);

            BindHero(instance, context, read);
        }

        public void Unbind()
        {
            _title.text = "";
            _chip.text = "";
            _subtitle.text = "";
            ClearHero();
        }

        // The stack scrolled to `scrollY`: image_parallax lets its picture lag behind (the frame scrolls away at full speed)
        public void OnStackScrolled(float scrollY)
        {
            if (!HasHero || _variant != BuiltInBlocks.HeaderImageParallax) return;
            float frame = HeroPart.layout.height;
            ParallaxOffset = Mathf.Clamp(scrollY * ParallaxFactor, 0f, float.IsNaN(frame) ? 0f : frame);
            Picture.Root.style.translate = new Translate(0f, ParallaxOffset);
        }

        private void BindHero(BlockInstanceData instance, BlockBindContext context, BlockFieldReader read)
        {
            ClearHero();
            if (!BuiltInBlocks.HeaderShowsPicture(context.Variant, instance)) return;
            _variant = context.Variant;
            HeroPart.AddToClassList("card-hero--" + _variant);
            if (_variant == BuiltInBlocks.HeaderModelTurntable)
            {
                HeroPart.Add(ModelView.Root);
                // - the SAME turntable a model_3d block binds, given a synthetic instance keyed off the header's own
                // fields (Model, Picture-as-Fallback) so ModelView never needs to know it is inside a header. Auto Spin
                // is always on here (no authored toggle for the header, unlike the block's own field) and the display
                // resolves to Model3D's default (inline): a header teaser/takeover is not offered.
                var modelInstance = new BlockInstanceData { key = (instance?.key ?? "") + "::header_model" };
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DModelField, asset = read.ValidAsset(BuiltInBlocks.HeaderModelField, MediaKind.Model) });
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DFallbackField, asset = read.ValidAsset(BuiltInBlocks.HeaderImageField, MediaKind.Image) });
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DAutoSpinField, flag = true });
                // - the header's own Fit, read against the HEADER's Block Library default (not model_3d's)
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DFitField, value = BlockLibraryRule.Choice(context.Taxonomy?.card_settings,
                    BuiltInBlocks.Header, BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderModelFitField), read) });
                ModelView.Bind(modelInstance, context);
                ModelView.Hint.style.display = DisplayStyle.None; // the hero has no room for a caption line
                HasHero = true;
                return;
            }
            string picture = read.ValidAsset(BuiltInBlocks.HeaderImageField, MediaKind.Image);
            // - a video loop may have no poster: a plain frame until its first frame, never the "picture unavailable" words
            if (_variant == BuiltInBlocks.HeaderVideoLoop && picture.Length == 0) Picture.Clear(null);
            else Picture.Show(context.Media, picture, context.Strings);
            HeroPart.Add(Picture.Root);
            if (_variant == BuiltInBlocks.HeaderVideoLoop)
            {
                HeroPart.Add(LoopFrames);
                LoopTrack = new VideoTrack(context.Poi?.id, read.ValidAsset(BuiltInBlocks.HeaderLoopClipField, MediaKind.Video), "");
                _video = context.Video;
                if (_video != null)
                {
                    _video.Changed += RefreshLoop;
                    if (!context.ReduceMotion) _video.PlayLoop(LoopTrack);
                }
                RefreshLoop();
            }
            else if (_variant == BuiltInBlocks.HeaderSplitThenNow)
            {
                SecondPicture.Show(context.Media, read.ValidAsset(BuiltInBlocks.HeaderSecondImageField, MediaKind.Image), context.Strings);
                HeroPart.Add(SecondPicture.Root);
                ThenLabel.text = context.Strings?.Get(CardStrings.Keys.HeaderThen) ?? "";
                NowLabel.text = context.Strings?.Get(CardStrings.Keys.HeaderNow) ?? "";
                Picture.Root.Add(ThenLabel);
                SecondPicture.Root.Add(NowLabel);
            }
            else if (_variant == BuiltInBlocks.HeaderSpotlightCrop)
            {
                var header = BuiltInBlocks.Header;
                _focus = new Vector2(read.Number(header.Field(BuiltInBlocks.HeaderFocusXField)), read.Number(header.Field(BuiltInBlocks.HeaderFocusYField)));
                _zoom = read.Number(header.Field(BuiltInBlocks.HeaderZoomField));
                HeroPart.Add(Ring);
                _placedFor = Vector2.zero;
                PlaceSpotlight();
            }
            HasHero = true;
        }

        // video_loop: show the loop's frames while THIS header's loop holds the player and has a frame, else the poster
        private void RefreshLoop()
        {
            LoopShowsFrames = _video != null && LoopTrack != null && _video.PlayingLoop && _video.IsCurrent(LoopTrack) && _video.HasFrame
                              && _video.Texture is RenderTexture;
            LoopFrames.style.backgroundImage = LoopShowsFrames ? new StyleBackground(Background.FromRenderTexture((RenderTexture)_video.Texture)) : StyleKeyword.Null;
            // - the texture's pixels change every frame without UI Toolkit knowing: ask for a repaint
            if (LoopShowsFrames) LoopFrames.MarkDirtyRepaint();
        }

        private void ClearHero()
        {
            ModelView.Unbind();
            if (_video != null)
            {
                _video.Changed -= RefreshLoop;
                if (LoopTrack != null) _video.StopLoop(LoopTrack);
            }
            _video = null;
            LoopTrack = null;
            LoopShowsFrames = false;
            LoopFrames.style.backgroundImage = StyleKeyword.Null;
            if (_variant != null) HeroPart.RemoveFromClassList("card-hero--" + _variant);
            _variant = null;
            HasHero = false;
            ParallaxOffset = 0f;
            Picture.Root.style.translate = StyleKeyword.Null;
            foreach (var e in new[] { Picture.Root.style, SecondPicture.Root.style })
            {
                e.width = StyleKeyword.Null;
                e.height = StyleKeyword.Null;
                e.left = StyleKeyword.Null;
                e.top = StyleKeyword.Null;
            }
            ThenLabel.RemoveFromHierarchy();
            NowLabel.RemoveFromHierarchy();
            Picture.Clear(null);
            SecondPicture.Clear(null);
            HeroPart.Clear();
        }

        // spotlight_crop: size and place the picture around the focus point, the ring on it (SpotlightCropRule)
        private void PlaceSpotlight()
        {
            if (_variant != BuiltInBlocks.HeaderSpotlightCrop) return;
            var frame = new Vector2(HeroPart.layout.width, HeroPart.layout.height);
            if (float.IsNaN(frame.x) || frame.x <= 0f || frame == _placedFor) return;
            _placedFor = frame;
            var place = SpotlightCropRule.Place(frame, Picture.Aspect, _focus, _zoom);
            var style = Picture.Root.style;
            style.width = place.Size.x;
            style.height = place.Size.y;
            style.left = place.Offset.x;
            style.top = place.Offset.y;
            Ring.style.left = place.Focus.x;
            Ring.style.top = place.Focus.y;
        }
    }
}
