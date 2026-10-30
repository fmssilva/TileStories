using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The header block (_3.1 Tiers 1-2; the bar and the hero since _3.3 P4.4a, HeroRule). Two parts:
    //   - the BAR (Root, pinned in the header slot): the card's title on ONE line beside the round buttons. It never changes size, so no
    //     language, Menu or title length moves a stop; it shows the title only while the hero's does not (SetBarTitleShown)
    //   - the HERO (HeroPart, first in what scrolls -- BlockStackView places it): the eyebrow, the title in the display face, the subtitle
    //     and, at its foot, the card's actions (Actions: the stack binds them, HeroActionsRule). The picture looks draw it over the
    //     picture, full width and the whole of Full's view tall (HeroRule.Height: the picture cropped to cover it), the words minimal on the
    //     picture as Words On Picture says (HeroWordsOnPicture), ending where Half's view ends (HeroRule.WordsBottom); Text Only and Compact draw it as a plain plate (Compact: the title alone)
    //       image_parallax -- the picture moves as Hero Motion says (pan / zoom / none) and slower than the scroll (HeroMotionRule)
    //       split_then_now -- Picture (then) beside Second Picture (now), each with its CardStrings label
    //       spotlight_crop -- Picture enlarged around the focus point with a ring on it (SpotlightCropRule)
    //       video_loop     -- Picture as a poster, the Loop Clip's frames over it: the header asks the card's ONE video owner for its muted
    //                         loop (ICardVideo.PlayLoop) and shows the frames only while that loop holds the player and has a frame -- the
    //                         poster shows until the first frame, while a visitor's video holds the player, and always with ReduceMotion
    //       model_turntable -- the same turntable view a model_3d block uses
    //     A picture look without what it needs (HeaderShowsPicture) draws the plate. The words rise and fade as the card scrolls.
    // Only classes here (and the measured sizes / motion numbers the rules give); About.uss / Media.uss hold every size and colour.
    public sealed class HeaderBlockView : IBlockView
    {
        // The bar: what the peek stop shows (PoiCardSheetView measures its bottom edge)
        public VisualElement Root { get; }
        public VisualElement PeekPart => Root;
        // The hero, first in the scroll (always, a plate for the text looks): its frame (picture looks) and the moving part inside it, then
        // the FOOT -- the words and the actions row, minimal on the picture (_3.3 S4a; the premium target, v5): no plate; Words On Picture
        // (_3.3 P3c addendum: a halo on the letters, a soft tone falling off from their corner, only as strong as 4.5:1 needs)
        public VisualElement HeroPart { get; }
        public VisualElement Frame { get; }
        public VisualElement Media { get; }
        public VisualElement Foot { get; }
        // Words On Picture (HeroWordsRule): the layer over the picture, under the words (the tone or the scrim), and its owner
        public VisualElement ToneLayer { get; }
        public HeroWordsOnPicture Words { get; }
        public VisualElement Text { get; }
        public VisualElement Actions { get; }
        public Label Eyebrow { get; }
        public Label HeroTitle { get; }
        // True once bound: every header has a hero (picture or plate)
        public bool HasHero { get; private set; }
        // True when the look draws a picture (HeaderShowsPicture); false = the plate
        public bool HasPicture { get; private set; }
        public CardImage Picture { get; }
        public CardImage SecondPicture { get; }
        public Label ThenLabel { get; }
        public Label NowLabel { get; }
        public VisualElement Ring { get; }
        // video_loop: the loop's frames over the poster, the loop this header asks for, and whether its frames show now
        public VisualElement LoopFrames { get; }
        public VideoTrack LoopTrack { get; private set; }
        public bool LoopShowsFrames { get; private set; }
        // model_turntable (10A.3.2): the SAME turntable view a model_3d block uses, embedded in the hero
        public ModelTurntableBlockView ModelView { get; }

        // The classes the bar and the hero wear (About.uss / Media.uss)
        public const string BarTitleShownClass = "card-header--title-shown";
        public const string PlateClass = "card-hero--plate";
        public const string CompactClass = "card-hero--compact";
        public const string NarrowClass = "card-hero--narrow";

        // What time it is for the pan / zoom (seconds): the real clock; a test sets its own
        public System.Func<float> Clock { get; set; } = () => Time.unscaledTime;

        // What the hero's motion did last (HeroMotionRule): the pan's sideways offset, the zoom's scale, the parallax slide, the words'
        // rise and opacity -- what the tests and the frame sequences read
        public string Motion { get; private set; } = HeroMotionRule.None;
        public bool Moving { get; private set; }
        public float PanOffset { get; private set; }
        public float ZoomScale { get; private set; } = 1f;
        public float ParallaxOffset { get; private set; }
        public float TextRise { get; private set; }
        public float TextOpacity { get; private set; } = 1f;
        // How much wider the picture is than its frame (0: no pan possible)
        public float Overflow { get; private set; }
        public bool BarTitleShown => Root.ClassListContains(BarTitleShownClass);

        private readonly Label _title;
        private readonly Label _subtitle;
        private string _variant;
        private bool _reduceMotion;
        private float _scrollY;
        private Vector2 _focus;
        private float _zoom = 1f;
        // The frame size the spotlight / the pan was last placed for: a layout pass that did not change it places nothing
        private Vector2 _placedFor;
        private ICardVideo _video;
        private IVisualElementScheduledItem _tick;
        // The layout the stops gave (SetHeroLayout)
        private float _heroHeight;
        private float _wordsBottom;
        private float _underBars;

        public HeaderBlockView()
        {
            Root = new VisualElement { name = "card-header" };
            Root.AddToClassList("card-header");
            _title = new Label { name = "card-header-title", pickingMode = PickingMode.Ignore };
            _title.AddToClassList("card-header__title");
            Root.Add(_title);

            HeroPart = new VisualElement { name = "card-header-hero" };
            HeroPart.AddToClassList("card-hero");
            Frame = new VisualElement { name = "card-hero-frame", pickingMode = PickingMode.Ignore };
            Frame.AddToClassList("card-hero__frame");
            Media = new VisualElement { name = "card-hero-media", pickingMode = PickingMode.Ignore };
            Media.AddToClassList("card-hero__media");
            Frame.Add(Media);
            Foot = new VisualElement { name = "card-hero-foot", pickingMode = PickingMode.Ignore };
            Foot.AddToClassList("card-hero__foot");

            Text = new VisualElement { name = "card-hero-text", pickingMode = PickingMode.Ignore };
            Text.AddToClassList("card-hero__text");
            Eyebrow = new Label { name = "card-hero-eyebrow", pickingMode = PickingMode.Ignore };
            Eyebrow.AddToClassList("card-hero__eyebrow");
            HeroTitle = new Label { name = "card-hero-title", pickingMode = PickingMode.Ignore };
            HeroTitle.AddToClassList("card-hero__title");
            _subtitle = new Label { name = "card-header-subtitle", pickingMode = PickingMode.Ignore };
            _subtitle.AddToClassList("card-header__subtitle");
            _subtitle.AddToClassList("card-hero__subtitle");
            Text.Add(Eyebrow);
            Text.Add(HeroTitle);
            Text.Add(_subtitle);
            Actions = new VisualElement { name = "card-hero-actions" };
            Actions.AddToClassList("card-hero__actions");
            Foot.Add(Text);
            Foot.Add(Actions);
            // - Words On Picture's layer (_3.3 P3c addendum): over the picture (the frame goes in first at bind), under the words; the whole
            //   hero's size, its gradient anchored at the words' corner; it rises and fades with them
            ToneLayer = new VisualElement { name = "card-hero-tone", pickingMode = PickingMode.Ignore };
            ToneLayer.AddToClassList("card-hero__tone");
            Words = new HeroWordsOnPicture(ToneLayer);
            HeroPart.Add(ToneLayer);
            HeroPart.Add(Foot);

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
            // - the words' fade reads their place in the view: placed again once they are laid out (at rest too, with no scroll to say so)
            Foot.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                OnStackScrolled(_scrollY);
                // - a new stop moves the foot, never the words inside it: the layer follows them here
                FitWords();
            });
            // - the layer follows the words: a longer title, another language, a stop that re-lays them
            Text.RegisterCallback<GeometryChangedEvent>(_ => FitWords());
            Actions.RegisterCallback<GeometryChangedEvent>(_ => FitWords());
            ToneLayer.RegisterCallback<GeometryChangedEvent>(_ => FitWords());
            HeroPart.RegisterCallback<AttachToPanelEvent>(_ => FitWords());
            Frame.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                // - only a new frame size re-places the picture (its own new size must never feed back into the frame's)
                if (evt.newRect.size != evt.oldRect.size) PlacePicture();
            });
            HeroPart.RegisterCallback<GeometryChangedEvent>(evt => HeroPart.EnableInClassList(NarrowClass, HeroRule.IsNarrow(evt.newRect.width)));
        }

        // The hero frame's height and where its words end (from its top) as the stops gave them last (NaN while it has none: the plate)
        public float HeroHeight { get; private set; } = float.NaN;
        public float WordsBottom { get; private set; } = float.NaN;

        public string TitleText => _title.text;
        public string SubtitleText => _subtitle.text;
        // Laid out as shown: the compact variant and an empty subtitle hide it
        public bool SubtitleShown => _subtitle.resolvedStyle.display != DisplayStyle.None;

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            var read = new BlockFieldReader(instance, context.Language, context.FallbackLanguage);
            string title = read.Text(BlockStackBuilder.HeaderTitleField);
            _title.text = title.Length > 0 ? title : context.Poi?.name ?? "";
            HeroTitle.text = _title.text;
            _reduceMotion = context.ReduceMotion;

            // - classes only, no inline display: the compact look hides the eyebrow and the subtitle through USS
            Eyebrow.text = read.Text(BuiltInBlocks.HeaderEyebrowField);
            Eyebrow.EnableInClassList("card-hero__eyebrow--empty", Eyebrow.text.Length == 0);
            _subtitle.text = read.Text(BlockStackBuilder.HeaderSubtitleField);
            _subtitle.EnableInClassList("card-header__subtitle--empty", _subtitle.text.Length == 0);
            HeroPart.EnableInClassList(CompactClass, context.Variant == BuiltInBlocks.HeaderCompact);

            BindHero(instance, context, read);
            HasHero = true;
            // - a picture look or the plate is known now: the layout the stops gave applies to it
            ApplyHeroLayout();
        }

        public void Unbind()
        {
            _title.text = "";
            HeroTitle.text = "";
            Eyebrow.text = "";
            _subtitle.text = "";
            SetBarTitleShown(false);
            ClearHero();
            HasHero = false;
        }

        // The bar shows the title (HeroRule.BarShowsTitle): at Peek, and once the hero's title scrolled away
        public void SetBarTitleShown(bool shown) => Root.EnableInClassList(BarTitleShownClass, shown);

        // The hero's layout from the sheet's stops (HeroRule): its height -- a MINIMUM: words that need more make it taller, never squashed --
        // and where its words and actions end, `wordsBottom` from its top (the foot keeps the rest of the picture under it); the plate
        // keeps its own
        public void SetHeroLayout(float height, float wordsBottom, float underBars = 0f)
        {
            _heroHeight = height;
            _wordsBottom = wordsBottom;
            _underBars = Mathf.Max(0f, float.IsNaN(underBars) ? 0f : underBars);
            ApplyHeroLayout();
        }

        private void ApplyHeroLayout()
        {
            bool shown = HasPicture && _heroHeight > 0f;
            HeroHeight = shown ? _heroHeight : float.NaN;
            WordsBottom = shown ? Mathf.Clamp(_wordsBottom, 0f, _heroHeight) : float.NaN;
            HeroPart.style.minHeight = shown ? _heroHeight : new StyleLength(StyleKeyword.Null);
            // - a picture rises under the floating bars (the stack's top inset): it starts at the sheet's top edge; a plate stays below them
            HeroPart.style.marginTop = shown ? -_underBars : new StyleLength(StyleKeyword.Null);
            Foot.style.marginBottom = shown ? _heroHeight - WordsBottom : new StyleLength(StyleKeyword.Null);
        }

        // The bottom of the hero's title as drawn (risen by the scroll), in the stack's content coordinates: what the bar's title waits for
        public float HeroTitleBottom()
        {
            if (HeroPart.panel == null) return float.NaN;
            float bottom = HeroPart.layout.y + Foot.layout.y + Text.layout.y + HeroTitle.layout.yMax + TextRise;
            return bottom;
        }

        // The foot of the chrome floating over the stack's view (the bars, a shown strip), from the view's top: where the words finish fading
        public float FadeTop { get; set; }

        // The words block's top as drawn, from the stack's view top, for this scroll (NaN before layout)
        private float WordsTopInView(float scrollY)
        {
            if (HeroPart.panel == null) return float.NaN;
            float top = HeroPart.layout.y + Foot.layout.y;
            return float.IsNaN(top) ? float.NaN : top - scrollY + TextRise;
        }

        // The words block's top with no scroll (and so no rise), from the stack's view top (NaN before layout)
        private float WordsTopAtRest() => HeroPart.panel == null ? float.NaN : HeroPart.layout.y + Foot.layout.y;

        // The hero's bottom edge in the stack's content coordinates (NaN when it is not in the stack)
        public float HeroBottom() => HeroPart.panel == null ? float.NaN : HeroPart.layout.yMax;

        // Fit Words On Picture to the words as laid out now (each word, Listen's icon and words) over the picture they lie on: a still
        // picture is read for the strength; a video's frames, the turntable or the split's two pictures get the look's own strength
        private void FitWords()
        {
            // - off a panel no word is drawn and a text measure has no DPI to read (Unity warns, once per word): the stack binds a view
            //   before it attaches it, so the fit waits for the attach (AttachToPanelEvent) and the layout after it
            if (!HasPicture || HeroPart.panel == null) return;
            bool still = _variant != BuiltInBlocks.HeaderModelTurntable && _variant != BuiltInBlocks.HeaderSplitThenNow && !LoopShowsFrames;
            Words.Fit(WordsOnPicture, ToneSpread, HeroPart, new[] { Text, Actions }, still ? Picture : null, Media, Frame, Motion, _reduceMotion,
                Overflow, TextRise);
        }

        // The look of the words on the picture (the header's Words On Picture, else the Block Library's default) and how far its darkening
        // reaches (the header's Tone Spread)
        public string WordsOnPicture { get; private set; } = HeroWordsRule.ToneHalo;
        public float ToneSpread { get; private set; } = HeroWordsRule.SpreadDefault;

        // The stack scrolled to `scrollY`: the picture lags behind (image_parallax), the words rise and fade
        public void OnStackScrolled(float scrollY)
        {
            _scrollY = scrollY;
            if (!HasHero) return;
            // - the words block moves as one, Listen and the actions with the words: it rises past the picture, and fades near the top
            TextRise = HeroMotionRule.TextRise(scrollY, _reduceMotion);
            // - the fade is the scroll's alone: whole at rest wherever the stop puts the words (_3.3 P3a review b)
            TextOpacity = HeroMotionRule.TextOpacity(WordsTopAtRest(), WordsTopInView(scrollY), FadeTop, _reduceMotion);
            Foot.style.translate = new Translate(0f, TextRise);
            Foot.style.opacity = TextOpacity;
            // - the layer stays over the whole hero (moving its box would bare the picture's foot under a hard edge): its gradient and the
            //   halo rise with the words
            Words.Rise(TextRise);
            ToneLayer.style.opacity = TextOpacity;
            if (_variant == BuiltInBlocks.HeaderImageParallax) PlaceMedia();
        }

        // One frame of the pan / zoom on this header's clock (scheduled while the look moves by itself; a test calls it after moving
        // its clock)
        public void Animate()
        {
            if (HasPicture && _variant == BuiltInBlocks.HeaderImageParallax) PlaceMedia();
        }

        private void BindHero(BlockInstanceData instance, BlockBindContext context, BlockFieldReader read)
        {
            ClearHero();
            HasPicture = BuiltInBlocks.HeaderShowsPicture(context.Variant, instance);
            HeroPart.EnableInClassList(PlateClass, !HasPicture);
            HeroPart.Insert(0, Frame);
            Frame.style.display = HasPicture ? DisplayStyle.Flex : DisplayStyle.None;
            ToneLayer.style.display = HasPicture ? DisplayStyle.Flex : DisplayStyle.None;
            if (!HasPicture) return;
            WordsOnPicture = HeroWordsRule.Of(BlockLibraryRule.Choice(context.Settings, BuiltInBlocks.Header,
                BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderWordsOnPictureField), read));
            ToneSpread = HeroWordsRule.Spread(read.Number(BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderToneSpreadField)));
            _variant = context.Variant;
            HeroPart.AddToClassList("card-hero--" + _variant);
            if (_variant == BuiltInBlocks.HeaderModelTurntable)
            {
                Media.Add(ModelView.Root);
                // - the SAME turntable a model_3d block binds, given a synthetic instance keyed off the header's own
                // fields (Model, Picture-as-Fallback) so ModelView never needs to know it is inside a header. Auto Spin
                // is on here (no authored toggle for the header, unlike the block's own field) -- except under Reduce Motion,
                // where the model stands still until a finger turns it -- and the display resolves to Model3D's default
                // (inline): a header teaser/takeover is not offered.
                var modelInstance = new BlockInstanceData { key = (instance?.key ?? "") + "::header_model" };
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DModelField, asset = read.ValidAsset(BuiltInBlocks.HeaderModelField, MediaKind.Model) });
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DFallbackField, asset = read.ValidAsset(BuiltInBlocks.HeaderImageField, MediaKind.Image) });
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DAutoSpinField, flag = !context.ReduceMotion });
                // - the header's own Fit, read against the HEADER's Block Library default (not model_3d's)
                modelInstance.fields.Add(new BlockFieldValue { key = BuiltInBlocks.Model3DFitField, value = BlockLibraryRule.Choice(context.Settings,
                    BuiltInBlocks.Header, BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderModelFitField), read) });
                ModelView.Bind(modelInstance, context);
                ModelView.Hint.style.display = DisplayStyle.None; // the hero has no room for a caption line
                return;
            }
            string picture = read.ValidAsset(BuiltInBlocks.HeaderImageField, MediaKind.Image);
            // - a video loop may have no poster: a plain frame until its first frame, never the "picture unavailable" words
            if (_variant == BuiltInBlocks.HeaderVideoLoop && picture.Length == 0) Picture.Clear(null);
            else Picture.Show(context.Media, picture, context.Strings);
            Media.Add(Picture.Root);
            if (_variant == BuiltInBlocks.HeaderImageParallax)
            {
                // - the look's own motion, against the Block Library's default for the header
                Motion = BlockLibraryRule.Choice(context.Settings, BuiltInBlocks.Header, BuiltInBlocks.Header.Field(BuiltInBlocks.HeaderHeroMotionField), read);
                Moving = HeroMotionRule.Moves(Motion, _reduceMotion);
                // - a frame every ~30 ms only while the picture moves by itself (a still look schedules nothing)
                if (Moving) _tick = HeroPart.schedule.Execute(Animate).Every(33);
            }
            else if (_variant == BuiltInBlocks.HeaderVideoLoop)
            {
                Media.Add(LoopFrames);
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
                Media.Add(SecondPicture.Root);
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
                Media.Add(Ring);
            }
            _placedFor = Vector2.zero;
            PlacePicture();
        }

        // video_loop: show the loop's frames while THIS header's loop holds the player and has a frame, else the poster
        private void RefreshLoop()
        {
            bool showedFrames = LoopShowsFrames;
            LoopShowsFrames =_video != null && LoopTrack != null && _video.PlayingLoop && _video.IsCurrent(LoopTrack) && _video.HasFrame
                              && _video.Texture is RenderTexture;
            LoopFrames.style.backgroundImage = LoopShowsFrames ? new StyleBackground(Background.FromRenderTexture((RenderTexture)_video.Texture)) : StyleKeyword.Null;
            // - the texture's pixels change every frame without UI Toolkit knowing: ask for a repaint
            if (LoopShowsFrames) LoopFrames.MarkDirtyRepaint();
            // - poster <-> frames: what the words lie on changed
            if (showedFrames != LoopShowsFrames) FitWords();
        }

        private void ClearHero()
        {
            _tick?.Pause();
            _tick = null;
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
            HasPicture = false;
            Motion = HeroMotionRule.None;
            Moving = false;
            PanOffset = ParallaxOffset = TextRise = Overflow = 0f;
            ZoomScale = TextOpacity = 1f;
            _scrollY = 0f;
            Media.style.translate = StyleKeyword.Null;
            Media.style.scale = StyleKeyword.Null;
            Foot.style.translate = StyleKeyword.Null;
            Foot.style.opacity = StyleKeyword.Null;
            ToneLayer.style.opacity = StyleKeyword.Null;
            Words.Rise(0f);
            Words.Painter.Options = null;
            HeroPart.RemoveFromClassList(HeroWordsOnPicture.NoHaloClass);
            HeroPart.style.minHeight = StyleKeyword.Null;
            HeroPart.style.marginTop = StyleKeyword.Null;
            Foot.style.marginBottom = StyleKeyword.Null;
            HeroHeight = WordsBottom = float.NaN;
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
            Media.Clear();
        }

        // Size the picture for its frame: the spotlight around its focus (SpotlightCropRule); image_parallax as tall as the frame and as
        // wide as its own proportions ask, so a wide picture overflows the frame sideways and can pan
        private void PlacePicture()
        {
            var frame = new Vector2(Frame.layout.width, Frame.layout.height);
            if (float.IsNaN(frame.x) || frame.x <= 0f || frame.y <= 0f || frame == _placedFor) return;
            _placedFor = frame;
            if (_variant == BuiltInBlocks.HeaderSpotlightCrop)
            {
                var place = SpotlightCropRule.Place(frame, Picture.Aspect, _focus, _zoom);
                var style = Picture.Root.style;
                style.width = place.Size.x;
                style.height = place.Size.y;
                style.left = place.Offset.x;
                style.top = place.Offset.y;
                Ring.style.left = place.Focus.x;
                Ring.style.top = place.Focus.y;
            }
            else if (_variant == BuiltInBlocks.HeaderImageParallax)
            {
                float width = Mathf.Max(frame.x, frame.y * Picture.Aspect);
                Overflow = width - frame.x;
                Picture.Root.style.width = width;
                PlaceMedia();
            }
            // - the picture's place (and the pan's room) is known now: the shade reads what lies under the words
            FitWords();
        }

        // Move the picture: the pan / zoom of this moment and the parallax of the scroll (HeroMotionRule), transform only
        private void PlaceMedia()
        {
            float now = Clock?.Invoke() ?? 0f;
            PanOffset = HeroMotionRule.PanOffset(Motion, _reduceMotion, now, Overflow);
            ZoomScale = HeroMotionRule.Scale(Motion, _reduceMotion, now, Overflow);
            ParallaxOffset = HeroMotionRule.Parallax(_scrollY, Frame.layout.height, _reduceMotion);
            Media.style.translate = new Translate(PanOffset, ParallaxOffset);
            Media.style.scale = new Scale(new Vector2(ZoomScale, ZoomScale));
        }
    }
}
