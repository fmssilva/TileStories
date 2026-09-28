using UnityEngine;

namespace TileStories
{
    // The app's card state store: the visitor's answers and votes kept in PlayerPrefs, so they survive closing the app. Every
    // write is flushed at once (Save): a phone that kills the app in the background never loses a chosen answer.
    public sealed class PlayerPrefsCardStateStore : ICardStateStore
    {
        public bool TryGet(string key, out string value)
        {
            if (string.IsNullOrEmpty(key) || !PlayerPrefs.HasKey(key))
            {
                value = "";
                return false;
            }
            value = PlayerPrefs.GetString(key, "");
            return true;
        }

        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            PlayerPrefs.SetString(key, value ?? "");
            PlayerPrefs.Save();
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key) || !PlayerPrefs.HasKey(key)) return;
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
