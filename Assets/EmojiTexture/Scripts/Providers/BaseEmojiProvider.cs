using System;
using System.Collections;
using UnityEngine;

namespace iBicha
{
    internal class BaseEmojiProvider : IEmojiProvider
    {
        private static bool didWarnAboutSizeOverride;

        public int width { get; private set; }
        public int height { get; private set; }

        public BaseEmojiProvider(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        
        public virtual bool IsAsync
        {
            get { return false; }
        }

        public virtual IEnumerator AsyncInit()
        {
            yield break;
        }

        public virtual int Render(string text, Texture2D texture, bool sanitize)
        {
            throw new NotImplementedException();
        }

        public virtual int Render(int unicode, Texture2D texture)
        {
            return Render(char.ConvertFromUtf32(unicode), texture, false);
        }

        public virtual void RenderAsync(string text, Texture2D texture, bool sanitize, Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }

        public virtual void RenderAsync(int unicode, Texture2D texture, Action<bool, string> callback)
        {
            RenderAsync(char.ConvertFromUtf32(unicode), texture, false, callback);
        }

        protected void WarnAboutSizeOverride(int width, int height, int widthOverride, int heightOverride)
        {
            if (didWarnAboutSizeOverride) return;
            if (width == widthOverride && height == heightOverride) return;
            
            Debug.Log(string.Format("{0}: texture size overriden to {1}x{2}",
                GetType().Name, this.width, this.height));
            didWarnAboutSizeOverride = true;
        }

    }
}