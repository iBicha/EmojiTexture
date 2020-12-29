#if TMPRO_EMOJIS
using System.Collections;
using TMPro;
using UnityEngine;

namespace iBicha.TMPro
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMP_EmojiSupport : MonoBehaviour
    {
        public bool githubFallback = true;

        private TMP_Text textComponent;
        private string lastProcessedText = "";

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            TMProEmojiAsset.HookTMP(textComponent);
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChange);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChange);
        }

        void OnTextChange(object obj)
        {
            TMP_Text tmp_Text = (TMP_Text) obj;
            if (tmp_Text == textComponent && lastProcessedText != textComponent.text)
            {
                var text = textComponent.text;
                lastProcessedText = text;

                TMProEmojiAsset.Process(tmp_Text, text);
            }
        }
    }
}

#else
using UnityEngine;

namespace iBicha.TMPro
{
    public class TMP_EmojiSupport : MonoBehaviour
    {
        private void Start()
        {
            Debug.LogWarning("EmojiTexture for TextMesh Pro is not active, please add 'TMPRO_EMOJIS' to the Scripting Define Symbols in the Player Settings.");
        }
    }
}

#endif