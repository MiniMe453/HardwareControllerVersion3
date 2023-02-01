using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityTimer;
using UnityEngine.InputSystem;
using System;

namespace Rover.Interface
{
    public class MessageBox : MonoBehaviour
    {
        public Image backgroundImage;
        public TextMeshProUGUI textBox;
        public Canvas canvas;
        private InputActionMap messageBoxInputs = new InputActionMap();
        public event Action EOnInputConfirmed;
        private Timer m_clearTimer;

        void OnEnable()
        {
            messageBoxInputs.AddAction("confirm", binding: "<Keyboard>/enter");

            messageBoxInputs["confirm"].performed += Input_OnConfirm;
        }

        void OnDisable()
        {
            messageBoxInputs["confirm"].performed -= Input_OnConfirm;
        }

        public void ShowMessageBox(string text, Color color, float duration, bool waitForInput = false)
        {
            backgroundImage.color = color;
            textBox.color = color;
            textBox.text = text;

            UIManager.AddToViewport(canvas, 10);

            if (waitForInput)
            {
                messageBoxInputs.Enable();
            }
            else if(duration == -1f)
            {
                return;
            }
            else 
            {
                m_clearTimer = Timer.Register(duration, () => HideMessageBox());
            }
        }

        public void HideMessageBox()
        {
            UIManager.RemoveFromViewport(canvas);

            if(m_clearTimer != null)
                m_clearTimer.Cancel();

            DestroyImmediate(this.gameObject);
        }

        private void Input_OnConfirm(InputAction.CallbackContext context)
        {
            messageBoxInputs.Disable();
            EOnInputConfirmed?.Invoke();
            HideMessageBox();
        }

        public void SetMessageBoxText(string text)
        {
            textBox.text = text;
        }
    }
}

