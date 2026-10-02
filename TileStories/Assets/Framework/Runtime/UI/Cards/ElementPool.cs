using System;
using System.Collections.Generic;

namespace TileStories
{
    // The rows a card view shows for its items (facts, buttons, events, paragraphs...), built once and reused on every bind: work plan
    // rule 2, never instantiate-and-destroy per tap. Row i is always the same object, made the first time a bind needs i rows; Take hands
    // out the next one in order, ReleaseAll takes the shown rows off the card again and keeps them for the next bind. The caller adds a
    // row to whichever parent it belongs in (a view may use several) and says how a row leaves the card (one element, or a pin and its
    // loupe). Pure: nothing here touches UI Toolkit, so a test drives it with plain objects.
    public sealed class ElementPool<T>
    {
        private readonly Func<int, T> _create;
        private readonly Action<T> _release;
        private readonly List<T> _made = new();
        private readonly List<T> _shown = new();

        // `create` makes the row for place i (a row may keep i: a choice button answers with its own index); `release` takes a shown row
        // off the card (detach it, clear what it holds)
        public ElementPool(Func<int, T> create, Action<T> release)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        // The rows handed out since the last ReleaseAll, in order
        public IReadOnlyList<T> Shown => _shown;

        // How many rows were ever made (the most a bind has needed)
        public int Created => _made.Count;

        // The next row in order: the one made for this place before, else a new one
        public T Take()
        {
            int place = _shown.Count;
            while (_made.Count <= place) _made.Add(_create(_made.Count));
            var row = _made[place];
            _shown.Add(row);
            return row;
        }

        // Take every shown row off the card (in order) and keep them all for the next bind
        public void ReleaseAll()
        {
            foreach (var row in _shown) _release(row);
            _shown.Clear();
        }
    }
}
