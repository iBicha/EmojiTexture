using System.Collections;
using UnityEngine;
using iBicha.Github;
using UnityEngine.EventSystems;

namespace iBicha
{
    public class GithubEmojiExample : MonoBehaviour, IPointerClickHandler
    {
        //Materials for the changing emoji texture
        public Material material;

        //EmojiTexture used for rendering
        private EmojiTexture emojiTexture;

        //Emoji list to shuffle from
        private string[] githubEmojis = { ":blush:", ":heart:", ":eggplant:", ":peach:" };

        void Start()
        {
            emojiTexture = new EmojiTexture();
            material.mainTexture = emojiTexture;
            StartCoroutine(ChangeGithubEmoji());
        }

        //Change emoji every second
        //We use a coroutine because downloading emojis from github is an async operation
        IEnumerator ChangeGithubEmoji()
        {
            while (true)
            {
                yield return SetEmoji(githubEmojis[Mathf.RoundToInt(Time.time) % githubEmojis.Length]);
                yield return new WaitForSeconds(1f);
            }
        }

        //When the quad is tapped, we open the touch keyboard to input own emoji
        public void OnPointerClick(PointerEventData eventData)
        {
            //Cancel shuffling of the emojis
            StopAllCoroutines();
            //Read emoji from keyboard
            TouchKeyboard.get.EditText(emojiTexture.Text, (success, text) =>
            {
                if (success)
                {
                    StartCoroutine(SetEmoji(text));
                }
            });
        }

        IEnumerator SetEmoji(string text)
        {
            yield return emojiTexture.SetGithubEmoji(text);
        }

    }
}