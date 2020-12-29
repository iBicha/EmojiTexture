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
        private static TMP_SpriteAsset rootEmojiAsset;
        private static TMP_SpriteAsset currentEmojiAsset;
        private static int currentEmojiIndex;

        private static bool textureNeedsApply;
        public static EmojiTexture emojiTexture { get; private set; }

        public static bool didProcessAsync { get; private set; }

        public static void HookTMP(TMP_Text tmp_Text)
        {
            if (tmp_Text == null)
                return;

            if (emojiTexture == null)
            {
                emojiTexture = new EmojiTexture();
            }

            if (rootEmojiAsset == null)
            {
                rootEmojiAsset = CreateTMP_SpriteAsset();
                currentEmojiAsset = rootEmojiAsset;
                currentEmojiIndex = 0;
            }

            if (tmp_Text.spriteAsset == null)
            {
                tmp_Text.spriteAsset = rootEmojiAsset;
            }
            else if (tmp_Text.spriteAsset != rootEmojiAsset)
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

            if (tmp_Text.spriteAsset == rootEmojiAsset)
            {
                tmp_Text.spriteAsset = null;
                return;
            }

            if (tmp_Text.spriteAsset.fallbackSpriteAssets != null &&
                tmp_Text.spriteAsset.fallbackSpriteAssets.Contains(rootEmojiAsset))
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
                        .SearchForSpriteByUnicode(rootEmojiAsset, (uint)char.ConvertToUtf32(emojiText, 0), true,
                            out spriteIndex);
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
            {
                yield return GithubEmojiProvider.Initialize();
                yield return new WaitUntil(() => GithubEmojiProvider.IsInitialized);
            }

            foreach (var detectedEmoji in detectedEmojis)
            {
                string hex = char.ConvertToUtf32(detectedEmoji, 0).ToString("X");

                if (GithubEmojiProvider.IsValid(hex))
                {
//                    yield return emojiTexture.SetGithubEmoji(hex);
//                    if(emojiTexture.didDownloadTexture){
//                        PushSprite(emojiTexture);
//                        didProcessAsync = true;
//                    }
                }
            }

            //If the texture has unsaved changes, we apply them here
            //And make it non readable if it is full
            if (textureNeedsApply)
            {
                var makeNoLongerReadable = currentEmojiIndex ==
                                           EmojiTextureSettings.Get.SheetTiles * EmojiTextureSettings.Get.SheetTiles;
                ((Texture2D) currentEmojiAsset.spriteSheet).Apply(false, makeNoLongerReadable);
                textureNeedsApply = false;
            }
            
        }

        public static void Process(TMP_Text tmp_Text, string text)
        {
            bool spriteSheetUpdated = false;

            if (rootEmojiAsset == null)
                return;

            if (string.IsNullOrEmpty(text))
                return;

            int index;
            while ((index = EmojiTexture.IndexOfFirstEmoji(text)) != -1)
            {
                text = text.Substring(index);
                emojiTexture.Text = text;

                if (!string.IsNullOrEmpty(emojiTexture.Text))
                {
                    int spriteIndex = -1;
                    TMP_SpriteAsset spriteAsset = TMP_SpriteAsset
                        .SearchForSpriteByUnicode(rootEmojiAsset, (uint)emojiTexture.Unicode, true, out spriteIndex);
                    if (spriteAsset == null)
                    {
                        PushSprite(emojiTexture);
                        spriteSheetUpdated = true;
                    }

                    text = text.Substring(emojiTexture.Text.Length);
                }
                else
                {
                    break;
                }
            }

            //If the texture has unsaved changes, we apply them here
            //And make it non readable if it is full
            if (textureNeedsApply)
            {
                var makeNoLongerReadable = currentEmojiIndex ==
                                           EmojiTextureSettings.Get.SheetTiles * EmojiTextureSettings.Get.SheetTiles;
                ((Texture2D) currentEmojiAsset.spriteSheet).Apply(false, makeNoLongerReadable);
                textureNeedsApply = false;
            }

            if (spriteSheetUpdated)
            {
                tmp_Text.StartCoroutine(ApplyTextChanges(tmp_Text));
            }
        }

        static IEnumerator ApplyTextChanges(TMP_Text tmp_Text)
        {
            yield return new WaitForEndOfFrame();
            tmp_Text.havePropertiesChanged = true;
        }
        
        
        private static TMP_SpriteAsset CreateTMP_SpriteAsset()
        {
            var sheetTiles = EmojiTextureSettings.Get.SheetTiles;
            var emojiSize = emojiTexture.width;
            var sheetSize = sheetTiles * emojiSize;

            var texture = new Texture2D(sheetSize, sheetSize, TextureFormat.RGBA32, false);
            texture.LoadRawTextureData(new byte[sheetSize * sheetSize * 4]);
            texture.Apply(false, TextureUtility.CanCopyTextures);

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
            var sheetTiles = EmojiTextureSettings.Get.SheetTiles;
            var emojiSize = emojiTex.width;
            var sheetSize = sheetTiles * emojiSize;
 
            if (currentEmojiIndex >= sheetTiles * sheetTiles)
            {
                var newSheet = CreateTMP_SpriteAsset();
                rootEmojiAsset.fallbackSpriteAssets.Add(newSheet);
                currentEmojiAsset = newSheet;
                currentEmojiIndex = 0;
            }

            int row = currentEmojiIndex % sheetTiles;
            int column = currentEmojiIndex / sheetTiles;

            if (TextureUtility.CanCopyTextures)
            {
                Graphics.CopyTexture(emojiTex, 0, 0, 0, 0, emojiSize, emojiSize,
                    currentEmojiAsset.spriteSheet, 0, 0, row * emojiSize,
                    (sheetSize) - ((column + 1) * emojiSize));
            }
            else
            {
                //If we can't copy on the GPU, we copy on the CPU
                var pixels = ((Texture2D) emojiTex).GetPixels32(0);
                ((Texture2D) currentEmojiAsset.spriteSheet).SetPixels32(
                    row * emojiSize, (sheetSize) - ((column + 1) * emojiSize), emojiSize, emojiSize, pixels, 0);

                //Free CPU copy of the texture (mark as non readable) if it's full
                var makeNoLongerReadable = currentEmojiIndex == sheetTiles * sheetTiles - 1;
                if (makeNoLongerReadable)
                {
                    ((Texture2D) currentEmojiAsset.spriteSheet).Apply(false, makeNoLongerReadable);
                }

                textureNeedsApply = !makeNoLongerReadable;
            }

            TMP_Sprite tmp_Sprite = new TMP_Sprite();

            tmp_Sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(emojiTex.Text);
            tmp_Sprite.height = emojiSize;
            tmp_Sprite.id = currentEmojiIndex;
            tmp_Sprite.name = emojiTex.Text;
            tmp_Sprite.pivot = Vector2.one * 0.5f;
            tmp_Sprite.scale = 1;
            tmp_Sprite.unicode = emojiTexture.Unicode;
            tmp_Sprite.width = emojiSize;
            tmp_Sprite.x = row * emojiSize;
            tmp_Sprite.xAdvance = emojiSize;
            tmp_Sprite.xOffset = 0;
            tmp_Sprite.y = (sheetSize) - ((column + 1) * emojiSize);
            tmp_Sprite.yOffset = emojiSize * 0.9f;

            currentEmojiAsset.spriteInfoList.Add(tmp_Sprite);
            currentEmojiAsset.UpdateLookupTables();
            currentEmojiIndex++;
        }
    }
}
#endif