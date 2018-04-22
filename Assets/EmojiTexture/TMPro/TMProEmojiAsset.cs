#if TMPRO_EMOJIS
using System.Collections;
using System.Collections.Generic;
using TMPro;
using iBicha.Github;
using UnityEngine;

namespace iBicha.TMPro
{
    public class TMProEmojiAsset
    {
        //I don't know if it is worth resizing downloaded images...
        const int EMOJI_SIZE = 128; //IMPORTANT! Github returns 128x128 images
        const int SHEET_TILES = 4; //4x4 emojis 
        const int SHEET_SIZE = SHEET_TILES * EMOJI_SIZE;

        private static TMP_SpriteAsset rootEmojiAsset;
        private static TMP_SpriteAsset currentEmojiAsset;
        private static int currentEmojiIndex;

        private static bool textureNeedsApply;
        private static EmojiTexture emojiTexture;

        public static bool didProcessAsync { get; private set; }

        public static void HookTMP(TMP_Text tmp_Text)
        {
            if (tmp_Text == null)
                return;

            if (rootEmojiAsset == null)
            {
                rootEmojiAsset = CreateTMP_SpriteAsset();
                currentEmojiAsset = rootEmojiAsset;
                currentEmojiIndex = 0;
            }

            if (emojiTexture == null)
            {
                emojiTexture = new EmojiTexture(EMOJI_SIZE);
            }

            if (tmp_Text.spriteAsset == null)
            {
                tmp_Text.spriteAsset = rootEmojiAsset;
            }
            else if(tmp_Text.spriteAsset != rootEmojiAsset)
            {
                if (tmp_Text.spriteAsset.fallbackSpriteAssets == null)
                    tmp_Text.spriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();

                if (!tmp_Text.spriteAsset.fallbackSpriteAssets.Contains(rootEmojiAsset))
                    tmp_Text.spriteAsset.fallbackSpriteAssets.Add(rootEmojiAsset);
            }
        }

        public static void UnhookTMP(TMP_Text tmp_Text)
        {
            if (tmp_Text == null || tmp_Text.spriteAsset == null)
                return;

            if(tmp_Text.spriteAsset == rootEmojiAsset)
            {
                tmp_Text.spriteAsset = null;
                return;
            }

            if (tmp_Text.spriteAsset.fallbackSpriteAssets != null && tmp_Text.spriteAsset.fallbackSpriteAssets.Contains(rootEmojiAsset))
                tmp_Text.spriteAsset.fallbackSpriteAssets.Remove(rootEmojiAsset);
        }

        public static IEnumerator ProcessAsync(string text)
        {
            didProcessAsync = false;
            if (rootEmojiAsset == null)
                yield break;

            if (string.IsNullOrEmpty(text))
                yield break;

            List<string> detectedEmojis = new List<string>();

            string emojiText = null;
            while ((emojiText = EmojiTexture.GetFirstEmoji(text)) != null)
            {
                text = text.Substring(text.IndexOf(emojiText, System.StringComparison.Ordinal));

                if (!string.IsNullOrEmpty(emojiText))
                {
                    int spriteIndex = -1;
                    TMP_SpriteAsset spriteAsset = TMP_SpriteAsset
                        .SearchForSpriteByUnicode(rootEmojiAsset, char.ConvertToUtf32(emojiText, 0), true, out spriteIndex);
                    if (spriteAsset == null)
                    {
                        detectedEmojis.Add(emojiText);
                    }
                    text = text.Substring(emojiText.Length);
                }
                else
                {
                    break;
                }
            }

            if (detectedEmojis.Count > 0)
                yield return GithubHelper.Initialize();

            foreach (var detectedEmoji in detectedEmojis)
            {
                string hex = char.ConvertToUtf32(detectedEmoji, 0).ToString("X");

                if(GithubHelper.IsValid(hex)){
                    yield return emojiTexture.SetGithubEmoji(hex);
                    if(emojiTexture.didDownloadTexture){
                        PushSprite(emojiTexture);
                        didProcessAsync = true;
                    }
                }
            }

            //If the texture has unsaved changes, we apply them here
            //And make it non readable if it is full
            if (textureNeedsApply)
            {
                var makeNoLongerReadable = currentEmojiIndex == SHEET_TILES * SHEET_TILES;
                ((Texture2D)currentEmojiAsset.spriteSheet).Apply(false, makeNoLongerReadable);
                textureNeedsApply = false;
            }
        }

        public static bool Process(string text)
        {
            bool spriteSheetUpdated = false;

            if (rootEmojiAsset == null)
                return spriteSheetUpdated;

            if (string.IsNullOrEmpty(text))
                return spriteSheetUpdated;

            int index = -1;
            while ((index = EmojiTexture.IndexOfFirstEmoji(text)) != -1)
            {
                text = text.Substring(index);
                emojiTexture.Text = text;

                if(!string.IsNullOrEmpty(emojiTexture.Text)){
                    int spriteIndex = -1;
                    TMP_SpriteAsset spriteAsset = TMP_SpriteAsset
                        .SearchForSpriteByUnicode(rootEmojiAsset, emojiTexture.Unicode, true, out spriteIndex);
                    if (spriteAsset == null)
                    {
                        PushSprite(emojiTexture);
                        spriteSheetUpdated = true;
                    }
                    text = text.Substring(emojiTexture.Text.Length);
                } else {
                    break;
                }
            } 

            //If the texture has unsaved changes, we apply them here
            //And make it non readable if it is full
            if (textureNeedsApply)
            {
                var makeNoLongerReadable = currentEmojiIndex == SHEET_TILES * SHEET_TILES;
                ((Texture2D)currentEmojiAsset.spriteSheet).Apply(false, makeNoLongerReadable);
                textureNeedsApply = false;
            }

            return spriteSheetUpdated;
        }

        private static TMP_SpriteAsset CreateTMP_SpriteAsset()
        {
            var texture = new Texture2D(SHEET_SIZE, SHEET_SIZE, TextureFormat.RGBA32, false);
            if (EmojiTexture.CanCopyTextures)
            {
                //If we can copy textures on the GPU, we make it
                //non readable to free up the RAM copy
                //TODO: can we create a non readable texture in the first place?
                texture.Apply(false, true);
            }
            var spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            spriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
            spriteAsset.spriteInfoList = new List<TMP_Sprite>();
            spriteAsset.spriteSheet = texture;
            spriteAsset.material = new Material(Shader.Find("TextMeshPro/Sprite"));
            spriteAsset.material.mainTexture = spriteAsset.spriteSheet;
            return spriteAsset;
        }

        private static void PushSprite(EmojiTexture emojiTex)
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

            if (EmojiTexture.CanCopyTextures)
            {
                Graphics.CopyTexture(emojiTex, 0, 0, 0, 0, EMOJI_SIZE, EMOJI_SIZE,
                                     currentEmojiAsset.spriteSheet, 0, 0, row * EMOJI_SIZE,
                                     (SHEET_SIZE) - ((column + 1) * EMOJI_SIZE));
            }
            else
            {
                //If we can't copy on the GPU, we copy on the CPU
                var pixels = ((Texture2D)emojiTex).GetPixels32(0);
                ((Texture2D)currentEmojiAsset.spriteSheet).SetPixels32(
                    row * EMOJI_SIZE, (SHEET_SIZE) - ((column + 1) * EMOJI_SIZE), EMOJI_SIZE, EMOJI_SIZE, pixels, 0);

                //Free CPU copy of the texture (mark as non readable) if it's full
                var makeNoLongerReadable = currentEmojiIndex == SHEET_TILES * SHEET_TILES - 1;
                if (makeNoLongerReadable)
                {
                    ((Texture2D)currentEmojiAsset.spriteSheet).Apply(false, makeNoLongerReadable);
                }
                textureNeedsApply = !makeNoLongerReadable;
            }

            TMP_Sprite tmp_Sprite = new TMP_Sprite();

            tmp_Sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(emojiTex.Text);
            tmp_Sprite.height = EMOJI_SIZE;
            tmp_Sprite.id = currentEmojiIndex;
            tmp_Sprite.name = emojiTex.Text;
            tmp_Sprite.pivot = Vector2.one * 0.5f;
            tmp_Sprite.scale = 1;
            tmp_Sprite.unicode = emojiTexture.Unicode;
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
}
#endif