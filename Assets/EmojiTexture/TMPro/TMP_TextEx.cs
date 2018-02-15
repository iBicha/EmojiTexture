#if TMPRO_EMOJIS
using TMPro;

namespace iBicha.TMPro
{
    public static class TMP_TextEx
    {

        public static void SetTextEx(this TMP_Text tmp_Text, string text)
        {
            TMProEmojiAsset.HookTMP(tmp_Text);
            TMProEmojiAsset.Process(text);
            tmp_Text.text = text;
        }
    }
}
#endif