using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TileStories
{
    public static class WallConfigLoader
    {
        public static IEnumerator LoadFromStreamingAssets(string relativePath, Action<WallConfigData> onLoaded)
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            string json;

            if (fullPath.Contains("://") || fullPath.StartsWith("jar:"))
            {
                using var req = UnityWebRequest.Get(fullPath);
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[WallConfigLoader] Failed to load config from: {fullPath}\n{req.error}");
                    onLoaded?.Invoke(null);
                    yield break;
                }

                json = req.downloadHandler.text;
            }
            else
            {
                if (!File.Exists(fullPath))
                {
                    Debug.LogError($"[WallConfigLoader] Config file not found at: {fullPath}");
                    onLoaded?.Invoke(null);
                    yield break;
                }

                json = File.ReadAllText(fullPath);
            }

            var config = JsonUtility.FromJson<WallConfigData>(json);
            if (config == null)
            {
                Debug.LogError("[WallConfigLoader] Failed to parse wall config JSON.");
            }

            onLoaded?.Invoke(config);
        }
    }
}