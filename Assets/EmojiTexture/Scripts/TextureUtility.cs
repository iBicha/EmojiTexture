using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Rendering;


namespace iBicha
{
    public class TextureUtility
    {
        public static List<IntPtr> bufferRef = new List<IntPtr>();
        public static CommandBuffer commandBuffer = new CommandBuffer();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetBufferPointerByIndexDelegate(int index);

        [MonoPInvokeCallback(typeof(GetBufferPointerByIndexDelegate))]
        public static IntPtr GetBufferPointerByIndex(int index)
        {
            if (index < bufferRef.Count)
                return bufferRef[index];
            return IntPtr.Zero;
        }

        public static bool CanCopyTextures
        {
            get
            {
                return EmojiTextureSettings.Get.UseGPUTextureCopy &&
                       SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            }
        }

        public static void UploadDataToTexture(Texture2D texture, IntPtr buffer, int bufferSize,
            IntPtr textureUpdateCallback)
        {
            if (CanCopyTextures && textureUpdateCallback != IntPtr.Zero)
            {
                commandBuffer.IssuePluginCustomTextureUpdate(
                    textureUpdateCallback, texture, (uint) (bufferRef.IndexOf(buffer))
                );
                Graphics.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
            }
            else
            {
                texture.LoadRawTextureData(buffer, bufferSize);
                texture.Apply();
            }
        }
    }
}