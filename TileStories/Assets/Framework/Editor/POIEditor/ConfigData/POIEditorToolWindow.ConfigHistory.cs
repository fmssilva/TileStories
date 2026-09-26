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

        // Same rule for a free-text note popup (one undo step per keystroke, like an inline field)
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
            DrawConfigMutationScope(edit, refreshRigOnChange: true);
            Repaint();
        }

        private void RecordConfigChange(string before, string after)
        {
            if (_isApplyingHistory)
                return;

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