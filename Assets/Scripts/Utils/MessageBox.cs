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

            UIManager.AddToViewport(canvas, 100);

            if (waitForInput)
            {
                messageBoxInputs.Enable();
            }
            else
            {
                Timer.Register(duration, () => HideMessageBox());
            }
        }

        public void HideMessageBox()
        {
            UIManager.RemoveFromViewport(canvas);
        }

        private void Input_OnConfirm(InputAction.CallbackContext context)
        {
            messageBoxInputs.Disable();
            EOnInputConfirmed?.Invoke();
            HideMessageBox();
        }
    }
}

