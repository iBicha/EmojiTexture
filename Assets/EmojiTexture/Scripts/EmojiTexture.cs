#if UNITY_2017_2_OR_NEWER
//#define ENABLE_CUSTOM_TEXTURE_UPDATE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Rendering;

namespace iBicha
{
    public class EmojiTexture
    {
        private IEmojiProvider EmojiProvider;

        private void InitProvider(int width, int height)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            EmojiProvider = new macOSEmojiProvider(width, height);
            //EmojiProvider = new GithubEmojiProvider(width, height);
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            //TODO: implement WindowsEmojiProvider
            //EmojiProvider = new WindowsEmojiProvider(width, height);
            EmojiProvider = new GithubEmojiProvider(width, height);
#elif UNITY_ANDROID
            EmojiProvider = new AndroidEmojiProvider(width, height);
#elif UNITY_IOS
            EmojiProvider = new iOSEmojiProvider(width, height);
#elif UNITY_WEBGL
            EmojiProvider = new WebGLEmojiProvider(width, height);
#else
            EmojiProvider = new GithubEmojiProvider(width, height);
#endif
        }

        public int width
        {
            get { return EmojiProvider.width; }
        }

        public int height
        {
            get { return EmojiProvider.height; }
        }

        /// <summary>
        /// Get or set how the text if filtered. If set to true, the first emoji character found is extracted from the text to be rendered.
        /// If set to false, text is rendered as is. Defaults to true.
        /// </summary>
        public bool SanitizeText
        {
            get { return sanitizeText; }
            set
            {
                sanitizeText = value;
                this.Text = Text;
            }
        }

        /// <summary>
        /// Get or set the text of the emoji. Usually a one character string representing the emoji.
        /// </summary>
        /// <value>Emoji string</value>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (SanitizeText)
                {
                    int index = IndexOfFirstEmoji(text);
                    if (index != -1)
                        text = text.Substring(index, Mathf.Min(16, text.Length - index));
                    else
                        text = "";
                }

                Render();
            }
        }

        /// <summary>
        /// Get or set the unicode character of the emoji
        /// </summary>
        public int Unicode
        {
            get
            {
                if (string.IsNullOrEmpty(text))
                    return 0;
                if (text.Length == 1)
                    return text[0];
                return char.ConvertToUtf32(text, 0);
            }
            set { Text = char.ConvertFromUtf32(value); }
        }

        /// <summary>
        /// Copies the pixels from the native buffer and returns a byte array in the RGBA32 format.
        /// </summary>
        /// <value>The byte buffer.</value>
        [Obsolete("ByteBuffer is obsolete. Use the Texture2D to read the data.")]
        public byte[] ByteBuffer
        {
            get
            {
                return null;
            }
        }

        public bool IsRendering { get; private set; }
        
        private void Render()
        {
            IsRendering = true;
            if (EmojiProvider.IsAsync)
            {
                EmojiProvider.RenderAsync(text, texture, SanitizeText,
                    (success, newText) =>
                    {
                        if (success)
                        {
                            text = newText;
                        }

                        IsRendering = false;
                    });
            }
            else
            {
                int length = EmojiProvider.Render(text, texture, SanitizeText);
                if (!string.IsNullOrEmpty(text) && text.Length != length)
                    text = text.Substring(0, length);

                IsRendering = false;
                //Copy pixels to texture
//                TextureUtility.UploadDataToTexture(texture, buffer, bufferSize);
//                isByteBufferDirty = true;
            }
        }

        private Texture2D texture;
        private string text;
        private bool sanitizeText;

        public EmojiTexture() : this(null)
        {
        }

        public EmojiTexture(string text) : this(text, EmojiTextureSettings.Get.EmojiSizeInPixels, EmojiTextureSettings.Get.EmojiSizeInPixels)
        {
        }

        public EmojiTexture(string text, int size) : this(text, size, size)
        {
        }

        public EmojiTexture(int size) : this(null, size, size)
        {
        }

        public EmojiTexture(int width, int height) : this(null, width, height)
        {
        }

        public EmojiTexture(string text, int width, int height)
        {
            width = Mathf.Clamp(width, 8, 256);
            height = Mathf.Clamp(height, 8, 256);

            InitProvider(width, height);
            
            texture = new Texture2D(EmojiProvider.width, EmojiProvider.height, TextureFormat.RGBA32, false);

            sanitizeText = true;

            Text = text;
        }

        public static implicit operator Texture2D(EmojiTexture emojiTexture)
        {
            return emojiTexture == null ? null : emojiTexture.texture;
        }

        /// <summary>
        /// Extracts the first emoji character from the text. returns String.Empty if no emojis are found.
        /// </summary>
        /// <param name="text">The input text</param>
        /// <returns>A string representing an emoji. String.Empty if not emojis found in the input text.</returns>
        public static int IndexOfFirstEmoji(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var match = System.Text.RegularExpressions.Regex.Match(text,
                    @"(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff]|[\u0023-\u0039]\ufe0f?\u20e3|\u3299|\u3297|\u303d|\u3030|\u24c2|\ud83c[\udd70-\udd71]|\ud83c[\udd7e-\udd7f]|\ud83c\udd8e|\ud83c[\udd91-\udd9a]|\ud83c[\udde6-\uddff]|[\ud83c[\ude01-\ude02]|\ud83c\ude1a|\ud83c\ude2f|[\ud83c[\ude32-\ude3a]|[\ud83c[\ude50-\ude51]|\u203c|\u2049|[\u25aa-\u25ab]|\u25b6|\u25c0|[\u25fb-\u25fe]|\u00a9|\u00ae|\u2122|\u2139|\ud83c\udc04|[\u2600-\u26FF]|\u2b05|\u2b06|\u2b07|\u2b1b|\u2b1c|\u2b50|\u2b55|\u231a|\u231b|\u2328|\u23cf|[\u23e9-\u23f3]|[\u23f8-\u23fa]|\ud83c\udccf|\u2934|\u2935|[\u2190-\u21ff])");
                if (match.Success)
                    return match.Index;
            }

            return -1;
        }

        public static string GetFirstEmoji(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var match = System.Text.RegularExpressions.Regex.Match(text,
                    @"(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff]|[\u0023-\u0039]\ufe0f?\u20e3|\u3299|\u3297|\u303d|\u3030|\u24c2|\ud83c[\udd70-\udd71]|\ud83c[\udd7e-\udd7f]|\ud83c\udd8e|\ud83c[\udd91-\udd9a]|\ud83c[\udde6-\uddff]|[\ud83c[\ude01-\ude02]|\ud83c\ude1a|\ud83c\ude2f|[\ud83c[\ude32-\ude3a]|[\ud83c[\ude50-\ude51]|\u203c|\u2049|[\u25aa-\u25ab]|\u25b6|\u25c0|[\u25fb-\u25fe]|\u00a9|\u00ae|\u2122|\u2139|\ud83c\udc04|[\u2600-\u26FF]|\u2b05|\u2b06|\u2b07|\u2b1b|\u2b1c|\u2b50|\u2b55|\u231a|\u231b|\u2328|\u23cf|[\u23e9-\u23f3]|[\u23f8-\u23fa]|\ud83c\udccf|\u2934|\u2935|[\u2190-\u21ff])");
                if (match.Success)
                    return match.Value;
            }

            return null;
        }
    }
}