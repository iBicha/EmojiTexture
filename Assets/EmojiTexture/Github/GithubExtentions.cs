using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace iBicha.Github
{
    public static class GithubExtentions
    {
        public static IEnumerator SetGithubEmoji(this EmojiTexture emojiTexture, string text)
        {
            yield return GithubHelper.SetGithubEmoji(emojiTexture, text);
        }

        public static IEnumerator SetGithubEmoji(this EmojiTexture emojiTexture, int unicode)
        {
            yield return GithubHelper.SetGithubEmoji(emojiTexture, unicode);
        }
}
}
