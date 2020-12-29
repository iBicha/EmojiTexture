using System;
using System.Collections;
using UnityEngine;

namespace iBicha
{
    internal interface IEmojiProvider
    {
        int width { get; }
        int height { get; }
        
        bool IsAsync { get; }

        IEnumerator AsyncInit();
        
        int Render(string text, Texture2D texture, bool sanitize);
        int Render(int unicode, Texture2D texture);

        void RenderAsync(string text, Texture2D texture, bool sanitize, Action<bool, string> callback);
        void RenderAsync(int unicode, Texture2D texture, Action<bool, string> callback);
    }
}