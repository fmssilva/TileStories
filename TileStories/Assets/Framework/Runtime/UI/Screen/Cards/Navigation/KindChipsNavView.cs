using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The kind_chips navigation element (_3.2 section 5.1, canvas A3, L2 flat): every kind the card holds as one chip, in the order the
    // stack shows them, each dotted with its family's colour beside its words. It lists kinds, not sections, so a one-family card still
    // gets it (NavChoice.Blocks). A long list scrolls sideways and hides chips off-screen -- its (i) says so.
    public sealed class KindChipsNavView : INavElementView
    {
        public VisualElement Root => Row.Root;
        public NavChipRow Row { get; } = new(BuiltInNavElements.KindChips, "nav-kind-chips");

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            var kinds = new List<CardNavModel.Kind>();
            foreach (var section in model.Sections) kinds.AddRange(section.Kinds);
            Row.Bind(kinds, model, nav, context);
        }

        public void Unbind() => Row.Unbind();
    }
}
