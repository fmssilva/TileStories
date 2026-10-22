// POIEditorToolWindow.BlockFieldDrawer.Assets.cs
//
// Partial: the media row of the generic block-field drawer (BlockFieldDrawer.cs dispatches to it) -- an Asset field's
// object field, its default-media buttons and warnings, and how a stored path resolves to the asset it names.

using System;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // An Asset field (a picture, an audio clip, a captions file): an object field that offers only files of the field's media kind, stored as the
        // path inside the wall's Media Folder (MediaPathRule.StoredPathFor). A file picked from outside that folder is
        // stored as picked and warned about (the card leaves it out, BlockStackBuilder's InvalidMedia), never silently
        // refused; a stored path with no file behind it is warned about too. Drawing never writes.
        private void DrawAssetRow(BlockFieldDefinition field, float indent, string probeName, int probeIndex, string mediaFolder, string defaultLibraryPath,
            Func<string> get, Action<string> set)
        {
            string current = get();
            var shown = MediaAssetFor(current, mediaFolder, field.Media, defaultLibraryPath);
            DrawEditorRow(out float rowWidth, out _, indent);
            EditorGUILayout.PrefixLabel(field.Label + (field.Required ? " (required)" : ""));
            var picked = EditorGUILayout.ObjectField(shown, MediaObjectType(field.Media), false,
                GUILayout.Width(Mathf.Max(40f, rowWidth - EditorGUIUtility.labelWidth - 36f)), GUILayout.ExpandWidth(false));
            ReportTableCellRect(probeName, probeIndex);
            HelpInfoButton.Draw(field.Label, field.Help);
            EditorRowEnd();
            if (picked != shown) set(picked == null ? "" : MediaPathRule.StoredPathFor(AssetDatabase.GetAssetPath(picked), mediaFolder));

            string stored = get();
            string defaultKey = MediaPathRule.DefaultKeyOf(stored);
            DrawEditorRow(out _, out _, indent);
            if (defaultKey != null)
            {
                GUILayout.Label("Default: " + defaultKey, EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
                // - right after its label, like "Pick default..." (a FlexibleSpace BEFORE it pushed it to the window's edge, far outside the row)
                if (GUILayout.Button("Clear default", EditorStyles.miniButton, GUILayout.Width(90f))) set("");
                ReportTableCellRect(probeName + " clear default", probeIndex);
                GUILayout.FlexibleSpace();
            }
            else
            {
                if (GUILayout.Button("Pick default...", EditorStyles.miniButton, GUILayout.Width(110f)))
                    EditorPopup.ShowAt(CreateCardMediaDefaultPickerPopup(field.Media, key => set(MediaPathRule.PathForDefaultKey(key))), GUILayoutUtility.GetLastRect());
                ReportTableCellRect(probeName + " pick default", probeIndex);
                GUILayout.FlexibleSpace();
            }
            EditorRowEnd();

            var problem = MediaPathRule.Check(stored, field.Media);
            if (problem == MediaPathProblem.OutsideFolder || problem == MediaPathProblem.WrongType)
                EditorGUILayout.HelpBox(CardMediaProblemText(field.Label, field.Media, problem), MessageType.Warning);
            else if (defaultKey != null && MediaAssetFor(stored, mediaFolder, field.Media, defaultLibraryPath) == null)
                EditorGUILayout.HelpBox(CardUnknownDefaultKeyText(field.Label, defaultKey), MessageType.Warning);
            else if (problem == MediaPathProblem.None && defaultKey == null && MediaAssetFor(stored, mediaFolder, field.Media, defaultLibraryPath) == null)
                EditorGUILayout.HelpBox(CardMediaMissingText(field.Label, stored, field.Media), MessageType.Warning);
        }

        // A Per Language Asset field's language rows (BlockFieldDefinition.PerLanguage): one asset row per wall language after the
        // first, labelled "Clip (pt)", holding that language's own file; blank = the row above. The first language never gets a
        // row: the field's own file already is its file. Drawing never writes.
        private void DrawPerLanguageAssetRows(BlockFieldDefinition field, System.Collections.Generic.List<string> languages, float indent,
            string probeName, int probeIndex, Func<string, string> get, Action<string, string> set)
        {
            if (languages == null) return;
            for (int i = 1; i < languages.Count; i++)
            {
                string lang = languages[i];
                var row = new BlockFieldDefinition
                {
                    Key = field.Key, Type = BlockFieldType.Asset, Media = field.Media, Label = field.Label + " (" + lang + ")",
                    Help = string.Format(PerLanguageAssetRowHelp, field.Label, lang),
                };
                DrawAssetRow(row, indent, probeName + " " + lang, probeIndex, _config.card_settings?.media_resources_path,
                    _config.card_settings?.default_media_library_resources_path, () => get(lang), value => set(lang, value));
            }
        }

        // One language's own value in a per-language list: a Per Language field's file, a category's name ("" = none: the shared one is used)
        internal static string LanguageEntryValue(System.Collections.Generic.List<LocalizedEntry> byLanguage, string lang) =>
            byLanguage?.Find(e => e != null && e.lang == lang)?.value ?? "";

        // Store (or clear, when blank) one language's own value; called only by a real edit
        internal static void SetLanguageEntry(System.Collections.Generic.List<LocalizedEntry> byLanguage, string lang, string value)
        {
            int at = byLanguage.FindIndex(e => e != null && e.lang == lang);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (at >= 0) byLanguage.RemoveAt(at);
            }
            else if (at >= 0) byLanguage[at].value = value;
            else byLanguage.Add(new LocalizedEntry { lang = lang, value = value });
        }

        // The asset a stored path names: a "default:<key>" resolves through the wall's own default library (if
        // configured) then the Framework's; a plain path resolves inside the Media Folder through Resources (what
        // the app loads), or as a project path as picked (outside the folder: shown so the developer sees what
        // they chose); else none
        internal static UnityEngine.Object MediaAssetFor(string stored, string mediaFolder, MediaKind kind = MediaKind.Image, string defaultLibraryPath = null)
        {
            if (string.IsNullOrWhiteSpace(stored)) return null;
            var type = MediaObjectType(kind);
            if (MediaPathRule.IsDefaultKey(stored))
                return CardMediaLibraryLookup.Resolve(MediaPathRule.DefaultKeyOf(stored), kind,
                    CardMediaLibraryLookup.WallFrom(defaultLibraryPath), CardMediaLibraryLookup.Framework);
            string p = stored.Trim().Replace('\\', '/');
            if (p.StartsWith("Assets/")) return AssetDatabase.LoadAssetAtPath(p, type);
            if (!MediaPathRule.IsValid(p, kind)) return null;
            string folder = (mediaFolder ?? "").Trim().Trim('/');
            string noExtension = p.Substring(0, p.LastIndexOf('.'));
            return Resources.Load(folder.Length > 0 ? folder + "/" + noExtension : noExtension, type);
        }

        // The Unity type an Asset field of this kind holds: a texture (a picture or a panorama), an AudioClip, a TextAsset (a captions file,
        // made by VttTextImporter), a VideoClip, a GameObject (a model: the prefab glTFast imports a .glb as)
        internal static System.Type MediaObjectType(MediaKind kind) => kind switch
        {
            MediaKind.Audio => typeof(AudioClip),
            MediaKind.Captions => typeof(TextAsset),
            MediaKind.Video => typeof(UnityEngine.Video.VideoClip),
            MediaKind.Model => typeof(GameObject),
            _ => typeof(Texture2D),
        };
    }
}
