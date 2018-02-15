using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace iBicha
{
    public class EmojiTextureExample : MonoBehaviour, IPointerClickHandler
    {
        //Material of the changing emoji texture
        public Material material;

        //EmojiTexture used for rendering
        private EmojiTexture emojiTexture;

        //Emoji list to shuffle from
        private string[] emojis = { "☺", "❤", "🍆", "🍑" };

        void Start()
        {
            emojiTexture = new EmojiTexture();
            material.mainTexture = emojiTexture;
            InvokeRepeating("ChangeEmoji", 0f, 1f);
        }

        //Change emoji every second
        void ChangeEmoji()
        {
            emojiTexture.Text = emojis[Mathf.RoundToInt(Time.time) % emojis.Length];
        }

        //When the quad is tapped, we open the touch keyboard to input own emoji
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

            TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open(emojiTexture.Text);

            yield return new WaitUntil(() => {
                return keyboard.status == TouchScreenKeyboard.Status.Canceled ||
                keyboard.status == TouchScreenKeyboard.Status.LostFocus ||
                keyboard.status == TouchScreenKeyboard.Status.Done;
            });
            if (keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                //Stop shuffling emojis, and display the one entered with the keyboard
                CancelInvoke("ChangeEmoji");
                emojiTexture.Text = keyboard.text;
            }
        }
    }
}
