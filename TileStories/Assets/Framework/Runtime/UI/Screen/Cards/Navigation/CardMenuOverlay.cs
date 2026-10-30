using System;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's ONE overlay owner (_3.2 sections 4.2 and 5.3, step 8): the `menu` layer of the card, over the sheet and BELOW the full view
    // (TakeoverView), and the one place that decides what is open over the card and when it closes. ONE thing at a time, of two kinds:
    //   - a MENU (the wall's Menu element: drawer_tree): modal -- a scrim over the whole card and the element's panel on it
    //   - an ANCHORED list (a bar's More list, bar_sheet's kinds list): drawn in place by its element (above the bar, sliding with the bottom
    //     area), no scrim -- the card behind stays readable and scrollable, so a scroll is one of the ways it closes
    // Every open thing closes the same ways, here and nowhere else: system Back (NavBackRule, after the full view and before the sheet goes
    // down), a tap on the scrim, any jump (NavController.Jumped: a pick in the list itself, or another element), a touch or a wheel on the
    // card's stack (a scroll starts with one; a tap on the card behind an anchored list closes it too, as a tap outside closes any popover),
    // the sheet moving to another stop, the full view opening, and every Show / Hide of the card. Opening one closes the other: never two
    // stacked (NN/g bottom sheets: no stacking, keep a visible close, support Back -- Laubheimer 2023). An element keeps one dismiss path of
    // its own only where it is the opener's own toggle (a second tap on More, on the family that opened its kinds).
    // The sheet owns one beside its CardChromeView; elements reach it through BlockBindContext.Overlay.
    public sealed class CardMenuOverlay
    {
        // On the layer while a MENU is open (the scrim shows)
        public const string ModalClass = "card-menu--modal";
        // On the layer while a POPOVER is open (the Aa settings, _3.3 P4.4b): modal like a menu -- a tap beside it closes it -- but the
        // scrim is clear, so the card behind shows what a pick changes (a new mode re-colours it live)
        public const string PopoverClass = "card-menu--popover";

        // The layer (fills the card layer; hidden while nothing modal is open), its scrim and the panel host the menu element is drawn in
        public VisualElement Root { get; }
        public VisualElement Scrim { get; }
        public VisualElement Panel { get; }

        // What is open now: its key (null = nothing), and whether it is the modal menu
        public string OpenKey { get; private set; }
        public bool IsOpen => OpenKey != null;
        public bool IsModal { get; private set; }
        // The wall's Menu element bound on this card (NavSlots.MenuView), or none: a bar's More opens it instead of its small list
        public bool HasMenu => _sheet.Slots.MenuView != null;

        // Raised after anything closed, whichever way (a test watches it; nothing in the card needs it)
        public event Action Closed;
        // Raised after anything opened or closed (the floating slot dims the card while its own element is open)
        public event Action Changed;

        private readonly PoiCardSheetView _sheet;
        private Action _onClose;

        public CardMenuOverlay(PoiCardSheetView sheet)
        {
            _sheet = sheet;
            Root = new VisualElement { name = "poi-card-menu", pickingMode = PickingMode.Ignore };
            // - the card's own tokens live on these classes (CardTokens.uss), the theme's arrive from the app root; the layer is the
            //   sheet's sibling, like the full view
            Root.AddToClassList("ts-card");
            Root.AddToClassList("ts-theme-default");
            Root.AddToClassList("card-menu");
            Scrim = new VisualElement { name = "poi-card-menu-scrim" };
            Scrim.AddToClassList("card-menu__scrim");
            Scrim.AddManipulator(new Clickable(Close));
            // - the host never takes a finger itself: a tap beside the element's own panel reaches the scrim under it
            Panel = new VisualElement { name = "poi-card-menu-panel", pickingMode = PickingMode.Ignore };
            Panel.AddToClassList("card-menu__panel");
            Root.Add(Scrim);
            Root.Add(Panel);
            // - over the sheet, under the full view: inserted right before the takeover's root in the card layer
            sheet.Layer.Insert(sheet.Layer.IndexOf(sheet.Takeover.Root), Root);
            Root.style.display = DisplayStyle.None;

            // - the close triggers: a jump, a touch or a wheel on the stack (on the way DOWN, so a block's own button keeps its click),
            //   the sheet resting at another stop
            sheet.Nav.Jumped += Close;
            sheet.Stack.Scroll.RegisterCallback<PointerDownEvent>(_ => Close(), TrickleDown.TrickleDown);
            sheet.Stack.Scroll.RegisterCallback<WheelEvent>(_ => Close(), TrickleDown.TrickleDown);
            sheet.StopChanged += _ => Close();
        }

        // Open the wall's Menu element over the card (the header's menu button, a bar's More). False with no Menu on this card
        public bool OpenMenu()
        {
            var menu = _sheet.Slots.MenuView;
            if (menu == null) return false;
            Open(_sheet.Slots.MenuKey, menu.Root, null);
            return true;
        }

        // Open `key`: `panel` = a modal menu drawn on the scrim; null = an anchored list its element draws in place. `onClose` runs once
        // when it closes, whichever way (the element hides its list there). Whatever was open closes first
        public void Open(string key, VisualElement panel, Action onClose)
        {
            Close();
            OpenKey = key;
            IsModal = panel != null;
            _onClose = onClose;
            if (IsModal)
            {
                Panel.Add(panel);
                Root.style.display = DisplayStyle.Flex;
            }
            Root.EnableInClassList(ModalClass, IsModal);
            Changed?.Invoke();
        }

        // Open `panel` as a popover over the card (a clear scrim: see PopoverClass); whatever was open closes first
        public void OpenPopover(string key, VisualElement panel)
        {
            Open(key, panel, null);
            Root.AddToClassList(PopoverClass);
        }

        // Close whatever is open (nothing open: nothing happens)
        public void Close()
        {
            if (!IsOpen) return;
            // - the state is cleared BEFORE the element's callback, so a callback that closes again finds nothing open
            OpenKey = null;
            var onClose = _onClose;
            _onClose = null;
            if (IsModal) Panel.Clear();
            IsModal = false;
            Root.EnableInClassList(ModalClass, false);
            Root.RemoveFromClassList(PopoverClass);
            Root.style.display = DisplayStyle.None;
            onClose?.Invoke();
            Closed?.Invoke();
            Changed?.Invoke();
        }

        // Close `key` if it is what is open (an element closing its own list through its toggle)
        public void Close(string key)
        {
            if (OpenKey == key) Close();
        }
    }
}
