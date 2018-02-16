using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if TMPRO_EMOJIS
using TMPro;
#endif

namespace iBicha.TMPro
{
    public class TextMeshProExample : MonoBehaviour, IPointerClickHandler
    {
#if TMPRO_EMOJIS
        private TextMeshProUGUI label;
#endif

        private void Start()
        {
#if TMPRO_EMOJIS
            label = GetComponent<TextMeshProUGUI>();
#else
            Debug.LogWarning("EmojiTexture for TextMesh Pro is not active, please add 'TMPRO_EMOJIS' to the Scripting Define Symbols in the Player Settings.");
#endif
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartEditText();
        }

        public void StartEditText()
        {
            StopAllCoroutines();
            StartCoroutine(EditText());
        }

        IEnumerator EditText()
        {
            if (!TouchScreenKeyboard.isSupported)
            {
                yield break;
            }

            var text = "";

#if TMPRO_EMOJIS
            text = label.text;
#endif

            TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open(text);

            yield return new WaitUntil(() => {
                return keyboard.status == TouchScreenKeyboard.Status.Canceled ||
                keyboard.status == TouchScreenKeyboard.Status.LostFocus ||
                keyboard.status == TouchScreenKeyboard.Status.Done;
            });
            if (keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                text = keyboard.text;
#if TMPRO_EMOJIS
                //Calling SetTextEx fixes the rendering of the emojis in TextMeshPro
                label.SetTextEx(text);
#endif
            }

        }

    }
}
