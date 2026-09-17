// IdentityRenameEditState.cs
//
// Editor-only commit-style editing of an IDENTITY string in a taxonomy table.
// One shared implementation for every identity-renaming table (category key in
// category_styles, key in badge_categories, key in search_fields -- each POIs or
// per-POI keyword lists reference). Behavior mirrors the proven category pattern:
//   - keystrokes write only a SessionState DRAFT; the row's identity stays
//     pristine until Commit
//   - Enter or click-outside commits the final word and propagates to every
//     referencing POI via IdentityRenameResolver
//   - ESC discards the draft (no change, row untouched)
//   - rejection (blank / collision) shows a dialog and keeps the original value
// This class is state + event handling only; it never draws the field (callers
// draw it and forward get/set to GetLabel/SetLabel).
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public sealed class IdentityRenameEditState<T> where T : class
    {
        // Rows and POIs are read through providers, never captured at construction.
        // The window replaces _config wholesale on undo/redo (ApplyConfigSnapshot)
        // and on reload, so a captured list reference would go stale -- a later
        // commit would then rewrite POIs in a detached object that is no longer the
        // one being drawn or saved. Reading through a provider makes that impossible.
        private readonly Func<IList<T>> _rowsProvider;
        private readonly Func<IList<POIData>> _poisProvider;
        private readonly IdentityRenameResolver.RowIdentityGetter<T> _rowGet;
        private readonly IdentityRenameResolver.RowIdentitySetter<T> _rowSet;
        private readonly IdentityRenameResolver.PoiIdentityRewrite _poiRewrite;
        private readonly string _sessionPrefix;

        public Rect FieldRect { get; private set; }

        public IdentityRenameEditState(
            string sessionPrefix,
            Func<IList<T>> rowsProvider,
            IdentityRenameResolver.RowIdentityGetter<T> rowGet,
            IdentityRenameResolver.RowIdentitySetter<T> rowSet,
            Func<IList<POIData>> poisProvider,
            IdentityRenameResolver.PoiIdentityRewrite poiRewrite)
        {
            _sessionPrefix = sessionPrefix;
            _rowsProvider = rowsProvider;
            _rowGet = rowGet;
            _rowSet = rowSet;
            _poisProvider = poisProvider;
            _poiRewrite = poiRewrite;
        }

        // Resolve Enter/ESC BEFORE the field draws (Unity's TextField consumes the
        // first Return), plus click-outside-to-commit. Call at the top of the
        // section's OnGUI pass, before drawing the table rows.
        public void HandleEditEvents()
        {
            var rows = _rowsProvider();
            if (rows == null) return;
            int row = SessionState.GetInt(_sessionPrefix + ".Row", -1);
            if (row < 0 || row >= rows.Count) return;

            var action = PoiRenameKeys.Resolve(Event.current.type, Event.current.keyCode);
            if (action == PoiRenameKeys.Action.Commit)
            {
                Commit(row);
                GUI.FocusControl(null);
                Event.current.Use();
            }
            else if (action == PoiRenameKeys.Action.Cancel)
            {
                Cancel();
                GUI.FocusControl(null);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseDown && !FieldRect.Contains(Event.current.mousePosition))
            {
                Commit(row); // click elsewhere = blur = commit
            }
        }

        // Value shown by the caller's primary-editable cell while editing. The
        // row's stored identity stays pristine until Commit -- the draft lives
        // only in SessionState, so a cancelled or rejected rename has already
        // touched neither the config nor any POI referencing it.
        public string GetLabel(T row)
        {
            var rows = _rowsProvider();
            if (rows == null) return _rowGet(row);
            int edit = SessionState.GetInt(_sessionPrefix + ".Row", -1);
            if (edit < 0 || edit >= rows.Count || !ReferenceEquals(rows[edit], row))
                return _rowGet(row);
            return SessionState.GetString(_sessionPrefix + ".Draft", _rowGet(row));
        }

        // Called by the caller's setter on every keystroke; only writes the draft
        // unless this is the first real change (begins a new edit session).
        public void SetLabel(T row, string value)
        {
            FieldRect = GUILayoutUtility.GetLastRect();

            var rows = _rowsProvider();
            if (rows == null) return;
            int edit = SessionState.GetInt(_sessionPrefix + ".Row", -1);
            if (edit >= 0 && edit < rows.Count && ReferenceEquals(rows[edit], row))
            {
                SessionState.SetString(_sessionPrefix + ".Draft", value ?? string.Empty);
                return; // active edit: draft only
            }

            int idx = IndexOf(rows, row);
            if (idx >= 0 && !string.Equals(_rowGet(row), value, System.StringComparison.Ordinal))
            {
                // First real change starts a commit-style session.
                SessionState.SetInt(_sessionPrefix + ".Row", idx);
                SessionState.SetString(_sessionPrefix + ".Original", _rowGet(row) ?? string.Empty);
                SessionState.SetString(_sessionPrefix + ".Draft", value ?? string.Empty);
            }
        }

        private void Commit(int row)
        {
            var rows = _rowsProvider();
            if (rows == null || row < 0 || row >= rows.Count)
            {
                Clear();
                return;
            }

            var entry = rows[row];
            string oldName = SessionState.GetString(_sessionPrefix + ".Original", _rowGet(entry));
            string newName = SessionState.GetString(_sessionPrefix + ".Draft", _rowGet(entry));

            if (string.Equals(oldName, newName, System.StringComparison.Ordinal))
            {
                Clear();
                return;
            }

            if (IdentityRenameResolver.TryCommit(rows, _rowGet, _poisProvider(), _poiRewrite,
                    oldName, newName, out string rejection))
            {
                _rowSet(entry, newName);
            }
            else
            {
                _rowSet(entry, oldName); // rejected: keep the original identity
                EditorUtility.DisplayDialog("Identity not changed", rejection, "OK");
            }

            Clear();
        }

        private void Cancel()
        {
            // The draft was never written to the row, so cancelling just drops it.
            Clear();
        }

        private void Clear()
        {
            SessionState.EraseInt(_sessionPrefix + ".Row");
            SessionState.EraseString(_sessionPrefix + ".Draft");
            SessionState.EraseString(_sessionPrefix + ".Original");
            FieldRect = default;
        }

        private int IndexOf(IList<T> rows, T row)
        {
            for (int i = 0; i < rows.Count; i++)
                if (ReferenceEquals(rows[i], row))
                    return i;
            return -1;
        }
    }
}
