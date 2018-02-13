using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EmojiTexture
{

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void EmojiTexture_render(string text, IntPtr buffer, int width, int height);
#elif UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass _EmojiTextureClass;
    private static AndroidJavaClass EmojiTextureClass {
        get {
            if(_EmojiTextureClass == null){
                _EmojiTextureClass = new AndroidJavaClass("com.ibicha.emojitexture.EmojiTexture");
            }
            return _EmojiTextureClass;
        }
    }
#endif

    /// <summary>
    /// Get or set the text of the emoji. Usually a one character string representing the emoji.
    /// </summary>
    /// <value>Emoji string</value>
    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            if (value != text)
            {
                text = value;
                //Rendering
#if UNITY_IOS && !UNITY_EDITOR
                EmojiTexture_render(text, buffer, texture.width, texture.height);
#elif UNITY_ANDROID && !UNITY_EDITOR
                EmojiTextureClass.CallStatic("render", text, jByteBuffer, texture.width, texture.height);
#endif
                //Copy pixels to texture
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                isByteBufferDirty = true;
                texture.LoadRawTextureData(buffer, bufferSize);
                texture.Apply();
#endif
            }
        }
    }
    /// <summary>
    /// Copies the pixels from the native buffer and returns a byte array in the RGBA32 format.
    /// </summary>
    /// <value>The byte buffer.</value>
    public byte[] ByteBuffer
    {
        get
        {
            if (isByteBufferDirty || byteBuffer == null)
            {
                if (buffer != IntPtr.Zero && bufferSize > 0)
                {
                    Marshal.Copy(buffer, byteBuffer, 0, bufferSize);
                    isByteBufferDirty = false;
                }
            }
            return byteBuffer;
        }
    }

    private Texture2D texture;
    private string text;
    private IntPtr buffer;
    private AndroidJavaObject jByteBuffer;
    private int bufferSize;

    private byte[] byteBuffer;
    private bool isByteBufferDirty;

    public EmojiTexture() : this(null) { }

    public EmojiTexture(string text) : this(text, 256, 256) { }

    public EmojiTexture(string text, int size) : this(text, size, size) { }

    public EmojiTexture(int size) : this(null, size, size) { }

    public EmojiTexture(int width, int height) : this(null, width, height) { }

    public EmojiTexture(string text, int width, int height)
    {
        width = Mathf.Clamp(width, 8, 256);
        height = Mathf.Clamp(height, 8, 256);
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        bufferSize = 0;
        buffer = IntPtr.Zero;
        byteBuffer = null;
        isByteBufferDirty = false;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        bufferSize = width * height * 4;
        buffer = Marshal.AllocHGlobal(bufferSize);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        jByteBuffer = new AndroidJavaObject("java.nio.DirectByteBuffer", buffer.ToInt64(), bufferSize);
#endif

        Text = text;
    }

    ~EmojiTexture()
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        if(buffer != IntPtr.Zero)
            Marshal.FreeHGlobal(buffer);
#endif
    }

    static public implicit operator Texture(EmojiTexture emojiTexture)
    {
        return emojiTexture == null ? null : emojiTexture.texture;
    }

}