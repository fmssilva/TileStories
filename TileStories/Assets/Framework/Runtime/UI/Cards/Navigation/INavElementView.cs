using UnityEngine.UIElements;

namespace TileStories
{
    // A navigation element's view (_3.2 section 4.1): tabs, a bar, a menu... Views only -- the structure is the CardNavModel's and
    // every jump goes through the card's NavController, so an element never computes sections or scrolls the stack itself. Made by
    // its NavElementRegistry factory, pooled per key by the slot that shows it: Bind on every Show of a card that uses it, Unbind when
    // it leaves the card (the card closes, the wall picks another element, the model shows no navigation).
    public interface INavElementView
    {
        VisualElement Root { get; }
        // Draw this card's map and follow its controller; `context` is the card's own (its words, settings, reduce motion)
        void Bind(CardNavModel model, NavController nav, BlockBindContext context);
        // Let go of the model and the controller (no event of an old card may reach the view again)
        void Unbind();
    }
}
