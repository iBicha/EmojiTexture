#if TMPRO_EMOJIS
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace iBicha.TMPro
{
    public class TextMeshProExample : MonoBehaviour, IPointerClickHandler
    {
        private TextMeshProUGUI label;

        private void Start()
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TouchKeyboard.get.EditText(label.text, (success, text) =>
            {
                if (success)
                    label.text = text;
            });
        }
    }
}
#else
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace iBicha.TMPro
{
    public class TextMeshProExample : MonoBehaviour, IPointerClickHandler
    {

        private void Start()
        {
            Debug.LogWarning("EmojiTexture for TextMesh Pro is not active, please add 'TMPRO_EMOJIS' to the Scripting Define Symbols in the Player Settings.");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}
#endif