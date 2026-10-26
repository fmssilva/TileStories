using UnityEngine.UIElements;

namespace TileStories
{
    // The spacer element (_3.3 R14.2): extra room between two rows, small / medium / large on the spacing scale (Components.uss). Nothing to
    // read, nothing to tap.
    public sealed class SpacerElement : VisualElement, IElementComponent
    {
        public VisualElement Root => this;

        public SpacerElement()
        {
            ElementStyle.Init(this, BuiltInElements.Spacer);
            pickingMode = PickingMode.Ignore;
        }

        public void Bind(ElementRow row, ElementBindContext context) => ElementStyle.Apply(this, row, context);

        public void Unbind() { }
    }
}
