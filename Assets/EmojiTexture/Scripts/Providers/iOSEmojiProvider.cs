using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace iBicha
{
    internal class iOSEmojiProvider : BaseEmojiProvider, IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetBufferPointerByIndexDelegate(int index);

        [DllImport("__Internal")]
        private static extern int EmojiTexture_render(string text, IntPtr buffer, int width, int height, int sanitize);

        [DllImport("__Internal")]
        private static extern IntPtr EmojiTexture_GetTextureUpdateCallback();

        [DllImport("__Internal")]
        private static extern void EmojiTexture_SetBufferRefByIndexFunction(GetBufferPointerByIndexDelegate fn);


        private IntPtr buffer;
        private int bufferSize;
        private IntPtr textureUpdateCallback;

        public iOSEmojiProvider(int width, int height) : base(width, height)
        {
            bufferSize = base.width * height * 4;
            buffer = Marshal.AllocHGlobal(bufferSize);

            if (EmojiTextureSettings.Get.UseGPUTextureCopy && TextureUtility.CanCopyTextures)
            {
                textureUpdateCallback = EmojiTexture_GetTextureUpdateCallback();
                EmojiTexture_SetBufferRefByIndexFunction(TextureUtility.GetBufferPointerByIndex);
                TextureUtility.bufferRef.Add(buffer);
            }
        }

        public override int Render(string text, Texture2D texture, bool sanitize)
        {
            var ret = EmojiTexture_render(text, buffer, width, height, sanitize ? 1 : 0);
            TextureUtility.UploadDataToTexture(texture, buffer, bufferSize, textureUpdateCallback);
            return ret;
        }

        public void Dispose()
        {
            TextureUtility.bufferRef.Remove(buffer);
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }
        }
    }
}