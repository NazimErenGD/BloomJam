using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Loads NPC portrait sprites from StreamingAssets/NpcSprites/<SpeakerName>.{png|jpg|jpeg}.
// Results are cached so each sprite is read from disk at most once.
public static class NpcSpriteLoader
{
    private const string SubFolder = "NpcSprites";
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg" };

    private static readonly Dictionary<string, Sprite> _cache = new();

    public static Sprite GetByName(string speakerName)
    {
        if (string.IsNullOrEmpty(speakerName)) return null;
        if (_cache.TryGetValue(speakerName, out var cached)) return cached;

        string baseDir = Path.Combine(Application.streamingAssetsPath, SubFolder);
        for (int i = 0; i < Extensions.Length; i++)
        {
            string path = Path.Combine(baseDir, speakerName + Extensions[i]);
            if (!File.Exists(path)) continue;

            byte[] data;
            try { data = File.ReadAllBytes(path); }
            catch (System.Exception ex)
            {
                Debug.LogError($"NpcSpriteLoader: failed to read '{path}': {ex.Message}");
                continue;
            }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data))
            {
                Object.Destroy(tex);
                continue;
            }
            tex.filterMode = FilterMode.Bilinear;

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = speakerName;

            _cache[speakerName] = sprite;
            return sprite;
        }

        // Negative cache: avoid hitting disk again for a missing speaker.
        _cache[speakerName] = null;
        Debug.LogWarning($"NpcSpriteLoader: no sprite found for speaker '{speakerName}' under {baseDir}.");
        return null;
    }

    public static void ClearCache() => _cache.Clear();
}
