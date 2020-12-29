using System;
using UnityEngine;

namespace iBicha
{
    internal class WindowsEmojiProvider : BaseEmojiProvider
    {
        public WindowsEmojiProvider(int width, int height) : base(width, height)
        {
        }

        public override int Render(string text, Texture2D texture, bool sanitize)
        {
            throw new NotImplementedException();
        }
    }
}