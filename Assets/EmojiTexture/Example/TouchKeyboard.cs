using System;
using System.Collections;
using UnityEngine;

namespace iBicha
{
    public class TouchKeyboard : MonoBehaviour
    {
        public static TouchKeyboard get;
        TouchScreenKeyboard keyboard;

        string input = "";
        bool waitingForInput = false;
        bool wasCanceled = false;

        private void Start()
        {
            get = this;
        }

        public void EditText(string text, Action<bool, string> callback)
        {
            StartCoroutine(StartEditText(text, callback));
        }

        IEnumerator StartEditText(string text, Action<bool, string> callback)
        {
            input = text;
            waitingForInput = true;
            wasCanceled = false;

            if (!TouchScreenKeyboard.isSupported)
            {
                OnGUIEditing = true;
                yield return new WaitUntil(() => { return !waitingForInput; });
            }
            else
            {
                if (!TouchScreenKeyboard.isSupported)
                {
                    waitingForInput = false;
                    yield break;
                }
                keyboard = TouchScreenKeyboard.Open(input);
                yield return new WaitUntil(() => keyboard.status == TouchScreenKeyboard.Status.Canceled ||
                                                 keyboard.status == TouchScreenKeyboard.Status.Done ||
                                                 keyboard.status == TouchScreenKeyboard.Status.LostFocus);
                waitingForInput = true;
                wasCanceled = keyboard.status == TouchScreenKeyboard.Status.Canceled ||
                              keyboard.status == TouchScreenKeyboard.Status.LostFocus;
                input = keyboard.text;
            }

            callback(!wasCanceled, input);
        }

        public static bool OnGUIEditing = false;

        private void OnGUI()
        {
            if (!waitingForInput || !OnGUIEditing)
                return;

            GUILayout.BeginHorizontal();
            input = GUILayout.TextField(input, GUILayout.MinWidth(200f));
            if (GUILayout.Button("OK"))
            {
                wasCanceled = false;
                waitingForInput = OnGUIEditing = false;
            }
            if (GUILayout.Button("CANCEL"))
            {
                wasCanceled = true;
                waitingForInput = OnGUIEditing = false;
            }
            GUILayout.EndHorizontal();

            if (GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "", GUIStyle.none))
            {
                wasCanceled = true;
                waitingForInput = OnGUIEditing = false;
                return;
            }
        }

        private void OnDisable()
        {
            if (waitingForInput)
                waitingForInput = OnGUIEditing = false;
            if (keyboard != null)
                keyboard.active = false;
        }


    }
}
