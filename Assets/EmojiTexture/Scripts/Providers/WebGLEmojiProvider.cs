using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace iBicha
{
    internal class WebGLEmojiProvider : BaseEmojiProvider, IDisposable
    {
        [DllImport("__Internal")]
        private static extern int EmojiTexture_render(string text, IntPtr buffer, int width, int height, int sanitize);

        private IntPtr buffer;
        private int bufferSize;

        public WebGLEmojiProvider(int width, int height) : base(width, height)
        {
            bufferSize = width * height * 4;
            buffer = Marshal.AllocHGlobal(bufferSize);
        }

        public override int Render(string text, Texture2D texture, bool sanitize)
        {
            var ret = EmojiTexture_render(text, buffer, width, height, sanitize ? 1 : 0);
            TextureUtility.UploadDataToTexture(texture, buffer, bufferSize, IntPtr.Zero);
            return ret;
        }

        public void Dispose()
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }

        }
    }
}