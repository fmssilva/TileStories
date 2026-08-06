using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIAuthoringToolWindow
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
        }

        private void RedoConfigChange()
        {
            if (!CanRedoConfigChange())
                return;

            _configHistoryIndex++;
            ApplyConfigSnapshot(_configHistory[_configHistoryIndex]);
            _hasUnsavedChanges = true;
            RefreshRigVisuals();
        }

        private void ApplyConfigSnapshot(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
                return;

            _isApplyingHistory = true;
            _config = JsonUtility.FromJson<WallConfigData>(snapshot);
            TryResolveWallIconLibraryFromConfig();
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