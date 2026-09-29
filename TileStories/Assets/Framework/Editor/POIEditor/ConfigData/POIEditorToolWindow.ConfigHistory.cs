using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        private void DrawConfigMutationScope(Action drawContent, bool refreshRigOnChange)
        {
            if (_config == null)
            {
                drawContent?.Invoke();
                return;
            }

            string before = JsonUtility.ToJson(_config, prettyPrint: false);
            drawContent?.Invoke();
            string after = JsonUtility.ToJson(_config, prettyPrint: false);

            if (before == after)
                return;

            RecordConfigChange(before, after);
            _hasUnsavedChanges = true;

            if (refreshRigOnChange)
                RefreshRigVisuals();

            LivePlayModeConfigPush.PushToRunningWall(_config);
        }

        // A popup (the curated symbol picker, a Details note) writes back from its OWN OnGUI, after
        // this window's DrawConfigMutationScope has already closed -- so the write skipped undo, the
        // unsaved flag, the rig refresh and the live Play Mode push. Every popup is built by one of
        // these two factories, which give its write-back a mutation scope of its own.
        private ExistingSymbolPickerPopup CreateSymbolPickerPopup(Action<string> assignKey)
        {
            EnsureDefaultIconLibraryLoaded();
            return new ExistingSymbolPickerPopup(_wallIconLibrary, _defaultIconLibrary,
                key => ApplyPopupEdit(() => assignKey(key)), IsStillEditing(_config));
        }

        // _3.1 step 13: the default-media key picker, filtered to one MediaKind, offering the wall's own default
        // library (if configured) and the Framework's shipped one
        private CardMediaDefaultPickerPopup CreateCardMediaDefaultPickerPopup(MediaKind kind, Action<string> assignKey)
        {
            var wall = CardMediaLibraryLookup.WallFrom(_config?.card_settings?.default_media_library_resources_path);
            var framework = CardMediaLibraryLookup.Framework;
            return new CardMediaDefaultPickerPopup(kind, wall, framework,
                key => ApplyPopupEdit(() => assignKey(key)), IsStillEditing(_config));
        }

        // Same rule for a free-text note popup (one undo step per keystroke: its field lives in another window, see ActiveEditGesture)
        private EntryDetailsPopup CreateDetailsPopup(string title, Func<string> get, Action<string> set)
        {
            return new EntryDetailsPopup(title, get, value => ApplyPopupEdit(() => set(value)), IsStillEditing(_config));
        }

        // A popup stays open next to the window, but its get/set close over a row of the config it was
        // opened on. Undo/redo and a reload REPLACE _config, so after one that row is a dead copy: the
        // popup must close instead of writing into it. True while this window is alive and still holds
        // that same config object.
        private Func<bool> IsStillEditing(WallConfigData openedOn)
        {
            return () => this != null && openedOn != null && ReferenceEquals(_config, openedOn);
        }

        // Run a popup's edit inside a mutation scope, then repaint so the window shows the new value
        private void ApplyPopupEdit(Action edit)
        {
            _recordingPopupEdit = true;
            try { DrawConfigMutationScope(edit, refreshRigOnChange: true); }
            finally { _recordingPopupEdit = false; }
            Repaint();
        }

        // ---- Grouped undo: one history step per gesture, not per keystroke / drag frame ----
        // The gesture the last recorded change came from; null = that change was a one-shot edit.
        private string _historyGesture;
        private bool _recordingPopupEdit;

        // Which control the current edit comes from: the text field being typed in, or the control the mouse
        // holds (a slider drag). Null for a one-shot edit (a toggle click, a popup pick, a button) and for a
        // popup's edit -- a popup's controls live in ANOTHER window, so this window cannot see its focus.
        private string ActiveEditGesture()
        {
            if (_recordingPopupEdit) return null;
            if (EditorGUIUtility.editingTextField && GUIUtility.keyboardControl != 0) return "text:" + GUIUtility.keyboardControl;
            if (GUIUtility.hotControl != 0) return "drag:" + GUIUtility.hotControl;
            return null;
        }

        // A gesture ends as soon as focus / the mouse leaves its control, even for one event: typing into the
        // same cell again later is a NEW undo step (IMGUI control ids are stable, so the id alone can't tell)
        private void EndEditGestureIfFocusMoved()
        {
            if (_historyGesture != null && ActiveEditGesture() != _historyGesture)
                _historyGesture = null;
        }

        private void RecordConfigChange(string before, string after)
        {
            if (_isApplyingHistory)
                return;

            // Still the same gesture and nothing else happened since: widen the last step instead of adding one
            // (typing "Cracked" = one Ctrl+Z). A gesture that ends where it started removes its step.
            string gesture = ActiveEditGesture();
            if (gesture != null && gesture == _historyGesture && _configHistoryIndex > 0
                && _configHistoryIndex == _configHistory.Count - 1
                && string.Equals(_configHistory[_configHistoryIndex], before, StringComparison.Ordinal))
            {
                if (string.Equals(_configHistory[_configHistoryIndex - 1], after, StringComparison.Ordinal))
                {
                    _configHistory.RemoveAt(_configHistoryIndex);
                    _configHistoryIndex--;
                    _historyGesture = null;
                }
                else
                    _configHistory[_configHistoryIndex] = after;
                return;
            }
            _historyGesture = gesture;

            if (_configHistory.Count == 0)
            {
                _configHistory.Add(before);
                _configHistoryIndex = 0;
            }

            if (_configHistoryIndex < _configHistory.Count - 1)
                _configHistory.RemoveRange(_configHistoryIndex + 1, _configHistory.Count - (_configHistoryIndex + 1));

            if (!string.Equals(_configHistory[_configHistoryIndex], before, StringComparison.Ordinal))
            {
                _configHistory.Add(before);
                _configHistoryIndex = _configHistory.Count - 1;
            }

            if (!string.Equals(_configHistory[_configHistoryIndex], after, StringComparison.Ordinal))
            {
                _configHistory.Add(after);
                _configHistoryIndex = _configHistory.Count - 1;
            }
        }

        private void InitializeConfigHistory()
        {
            _configHistory.Clear();
            _configHistoryIndex = -1;
            _historyGesture = null;

            if (_config == null)
                return;

            _configHistory.Add(JsonUtility.ToJson(_config, prettyPrint: false));
            _configHistoryIndex = 0;
        }

        private bool CanUndoConfigChange() => _configHistoryIndex > 0;
        private bool CanRedoConfigChange() => _configHistoryIndex >= 0 && _configHistoryIndex < _configHistory.Count - 1;

        private void UndoConfigChange()
        {
            if (!CanUndoConfigChange())
                return;

            _configHistoryIndex--;
            _historyGesture = null;
            ApplyConfigSnapshot(_configHistory[_configHistoryIndex]);
            _hasUnsavedChanges = true;
            RefreshRigVisuals();
            LivePlayModeConfigPush.PushToRunningWall(_config);
        }

        private void RedoConfigChange()
        {
            if (!CanRedoConfigChange())
                return;

            _configHistoryIndex++;
            _historyGesture = null;
            ApplyConfigSnapshot(_configHistory[_configHistoryIndex]);
            _hasUnsavedChanges = true;
            RefreshRigVisuals();
            LivePlayModeConfigPush.PushToRunningWall(_config);
        }

        private void ApplyConfigSnapshot(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
                return;

            _isApplyingHistory = true;
            _config = JsonUtility.FromJson<WallConfigData>(snapshot);
            TryResolveWallIconLibraryFromConfig();
            TryResolveWallFontLibraryFromConfig();
            _isApplyingHistory = false;
            Repaint();
        }

        private void HandleUndoShortcuts()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown)
                return;

            bool ctrl = e.control || e.command;
            if (!ctrl)
                return;

            if (e.keyCode == KeyCode.Z && !e.shift)
            {
                UndoConfigChange();
                e.Use();
            }
            else if ((e.keyCode == KeyCode.Z && e.shift) || e.keyCode == KeyCode.Y)
            {
                RedoConfigChange();
                e.Use();
            }
        }
    }
}