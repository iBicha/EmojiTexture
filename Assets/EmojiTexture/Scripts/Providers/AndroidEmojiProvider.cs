using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace iBicha
{
    internal class AndroidEmojiProvider : BaseEmojiProvider, IDisposable
    {
        private IntPtr buffer;
        private AndroidJavaObject jByteBuffer;
        private int bufferSize;
        private IntPtr textureUpdateCallback;

        private static AndroidJavaClass _EmojiTextureClass;
        private static AndroidJavaClass EmojiTextureClass {
            get {
                if(_EmojiTextureClass == null){
                    _EmojiTextureClass = new AndroidJavaClass("com.ibicha.emojitexture.EmojiTexture");
                }
                return _EmojiTextureClass;
            }
        }
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GetBufferPointerByIndexDelegate(int index);

        [DllImport("emojiTextureHelper")]
        private static extern void EmojiTexture_SetBufferRefByIndexFunction(GetBufferPointerByIndexDelegate fn);

        public AndroidEmojiProvider(int width, int height) : base(width, height)
        {            
            bufferSize = width * height * 4;
            buffer = Marshal.AllocHGlobal(bufferSize);
            jByteBuffer = new AndroidJavaObject("java.nio.DirectByteBuffer", buffer.ToInt64(), bufferSize);
        
            if (EmojiTextureSettings.Get.UseGPUTextureCopy && TextureUtility.CanCopyTextures)
            {
                textureUpdateCallback = new IntPtr(EmojiTextureClass.CallStatic<long>("jGetTextureUpdateCallback"));
                EmojiTexture_SetBufferRefByIndexFunction(TextureUtility.GetBufferPointerByIndex);
                TextureUtility.bufferRef.Add(buffer);
            }
        }

        public override int Render(string text, Texture2D texture, bool sanitize)
        {
            var ret = EmojiTextureClass.CallStatic<int>("render", text, jByteBuffer, texture.width, texture.height, sanitize);
            TextureUtility.UploadDataToTexture(texture, buffer, bufferSize, textureUpdateCallback);
            return ret;
        }

        public void Dispose()
        {
            TextureUtility.bufferRef.Remove(buffer);
            if (jByteBuffer != null) jByteBuffer.Dispose();
            if(buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }
        }
    }
}