#if TMPRO_EMOJIS
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class TMProEmojiAsset
{
    const int EMOJI_SIZE = 128;
    const int SHEET_TILES = 4; //4x4 emojis 
    const int SHEET_SIZE = SHEET_TILES * EMOJI_SIZE;

    private static TMP_SpriteAsset rootEmojiAsset;
    private static TMP_SpriteAsset currentEmojiAsset;
    private static int currentEmojiIndex;

    private static bool canCopyTextures;
    private static EmojiTexture emojiTexture;

    public static void HookTMP(TMP_Text tmp_Text)
    {
        if (tmp_Text == null)
            return;

        if (tmp_Text.spriteAsset == null)
            tmp_Text.spriteAsset = TMP_SpriteAsset.defaultSpriteAsset;

        if (tmp_Text.spriteAsset.fallbackSpriteAssets == null)
            tmp_Text.spriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();

        if (rootEmojiAsset == null)
        {
            rootEmojiAsset = CreateTMP_SpriteAsset();
            currentEmojiAsset = rootEmojiAsset;
            currentEmojiIndex = 0;

            canCopyTextures = SystemInfo.copyTextureSupport != UnityEngine.Rendering.CopyTextureSupport.None;
        }

        if (emojiTexture == null)
        {
            emojiTexture = new EmojiTexture(EMOJI_SIZE);
            emojiTexture.SanitizeText = false;
        }

        if (!tmp_Text.spriteAsset.fallbackSpriteAssets.Contains(rootEmojiAsset))
        {
            tmp_Text.spriteAsset.fallbackSpriteAssets.Add(rootEmojiAsset);
        }
    }

    public static void UnhookTMP(TMP_Text tmp_Text)
    {
        if (tmp_Text == null || tmp_Text.spriteAsset == null || tmp_Text.spriteAsset.fallbackSpriteAssets == null)
            return;
        if (tmp_Text.spriteAsset.fallbackSpriteAssets.Contains(rootEmojiAsset))
            tmp_Text.spriteAsset.fallbackSpriteAssets.Remove(rootEmojiAsset);
    }

    public static void Process(string text)
    {
        if (rootEmojiAsset == null)
            return;

        var matches = Regex.Matches(text, @"(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff]|[\u0023-\u0039]\ufe0f?\u20e3|\u3299|\u3297|\u303d|\u3030|\u24c2|\ud83c[\udd70-\udd71]|\ud83c[\udd7e-\udd7f]|\ud83c\udd8e|\ud83c[\udd91-\udd9a]|\ud83c[\udde6-\uddff]|[\ud83c[\ude01-\ude02]|\ud83c\ude1a|\ud83c\ude2f|[\ud83c[\ude32-\ude3a]|[\ud83c[\ude50-\ude51]|\u203c|\u2049|[\u25aa-\u25ab]|\u25b6|\u25c0|[\u25fb-\u25fe]|\u00a9|\u00ae|\u2122|\u2139|\ud83c\udc04|[\u2600-\u26FF]|\u2b05|\u2b06|\u2b07|\u2b1b|\u2b1c|\u2b50|\u2b55|\u231a|\u231b|\u2328|\u23cf|[\u23e9-\u23f3]|[\u23f8-\u23fa]|\ud83c\udccf|\u2934|\u2935|[\u2190-\u21ff])");
        foreach (Match match in matches)
        {
            string emoji = match.Value;

            int spriteIndex = -1;

            int unicode = 0;

            if (emoji.Length == 1)
            {
                unicode = emoji[0];
            }
            else
            {
                unicode = char.ConvertToUtf32(emoji[0], emoji[1]);
            }

            TMP_SpriteAsset spriteAsset = TMP_SpriteAsset.SearchFallbackForSprite(rootEmojiAsset, unicode, out spriteIndex);
            if (spriteAsset == null)
            {
                //As an optimization, render only when the emoji is a new one
                emojiTexture.Text = emoji;
                PushSprite(unicode);
            }
        }
    }

    private static TMP_SpriteAsset CreateTMP_SpriteAsset()
    {
        var spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
        spriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
        spriteAsset.spriteInfoList = new List<TMP_Sprite>();
        spriteAsset.spriteSheet = new Texture2D(SHEET_SIZE, SHEET_SIZE, TextureFormat.RGBA32, false);
        spriteAsset.material = new Material(Shader.Find("TextMeshPro/Sprite"));
        spriteAsset.material.mainTexture = spriteAsset.spriteSheet;
        return spriteAsset;
    }

    private static void PushSprite(int unicode)
    {
        if (currentEmojiIndex >= SHEET_TILES * SHEET_TILES)
        {
            var newSheet = CreateTMP_SpriteAsset();
            rootEmojiAsset.fallbackSpriteAssets.Add(newSheet);
            currentEmojiAsset = newSheet;
            currentEmojiIndex = 0;
        }

        int row = currentEmojiIndex % SHEET_TILES;
        int column = currentEmojiIndex / SHEET_TILES;

        if (canCopyTextures)
        {
            Graphics.CopyTexture(emojiTexture, 0, 0, 0, 0, EMOJI_SIZE, EMOJI_SIZE,
                                 currentEmojiAsset.spriteSheet, 0, 0, row * EMOJI_SIZE,
                                 (SHEET_SIZE) - ((column + 1) * EMOJI_SIZE));
        }
        else
        {
            //If we can't copy on the GPU, we copy on the CPU
            var pixels = ((Texture2D)emojiTexture).GetPixels32(0);
            ((Texture2D)currentEmojiAsset.spriteSheet).SetPixels32(
                row * EMOJI_SIZE, (SHEET_SIZE) - ((column + 1) * EMOJI_SIZE), EMOJI_SIZE, EMOJI_SIZE, pixels, 0);
            ((Texture2D)currentEmojiAsset.spriteSheet).Apply();
        }

        TMP_Sprite tmp_Sprite = new TMP_Sprite();

        tmp_Sprite.hashCode = unicode;
        tmp_Sprite.height = EMOJI_SIZE;
        tmp_Sprite.id = currentEmojiIndex;
        tmp_Sprite.name = unicode.ToString("X").ToLower();
        tmp_Sprite.pivot = Vector2.one * 0.5f;
        tmp_Sprite.scale = 1;
        tmp_Sprite.unicode = unicode;
        tmp_Sprite.width = EMOJI_SIZE;
        tmp_Sprite.x = row * EMOJI_SIZE;
        tmp_Sprite.xAdvance = EMOJI_SIZE;
        tmp_Sprite.xOffset = 0;
        tmp_Sprite.y = (SHEET_SIZE) - ((column + 1) * EMOJI_SIZE);
        tmp_Sprite.yOffset = EMOJI_SIZE * 0.9f;

        currentEmojiAsset.spriteInfoList.Add(tmp_Sprite);
        currentEmojiAsset.UpdateLookupTables();
        currentEmojiIndex++;
    }
}
#endif