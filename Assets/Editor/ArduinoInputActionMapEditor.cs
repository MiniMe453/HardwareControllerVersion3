using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Rover.Arduino
{
 public class ArduinoInputActionMapEditor : EditorWindow
    {
        private Vector2 m_inputEditorScrollPos;
        private ArduinoInputActionMap m_arduinoActionMap;

        [MenuItem ("Rover/Arduion InputActionMap Editor")]
        public static void  ShowWindow () 
        {
            EditorWindow.GetWindow(typeof(ArduinoInputActionMapEditor));
        }

        private void OnEnable()
        {
            m_inputEditorScrollPos = new Vector2(0f,0f);
            
        }
        void OnGUI()
        {
            GUILayout.Label("Arduino Input Action Map", EditorStyles.boldLabel);

            m_arduinoActionMap = (ArduinoInputActionMap)EditorGUILayout.ObjectField(m_arduinoActionMap, typeof(ArduinoInputActionMap), false);

            if(m_arduinoActionMap == null)
                return;

            GUILayout.BeginHorizontal();

            GUILayout.Label(" ", GUILayout.MaxWidth(20));
            GUILayout.Label("Type", GUILayout.MaxWidth(75));
            GUILayout.Label("PinMode", GUILayout.MaxWidth(75));
            GUILayout.Label("Input Name", GUILayout.MaxWidth(300));
            GUILayout.Label("Pin", GUILayout.MaxWidth(30));
            GUILayout.Label("Idx", GUILayout.MaxWidth(30));
            GUILayout.Label("Hold Button");

            GUILayout.EndHorizontal();

            m_inputEditorScrollPos = EditorGUILayout.BeginScrollView(m_inputEditorScrollPos);

            foreach(ArduinoInputData data in m_arduinoActionMap.InputDataList)
            {
                GUILayout.BeginHorizontal();

                if(GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    m_arduinoActionMap.RemoveInput(data.dataIndex);
                    return;
                }

                data.inputType = (InputType)EditorGUILayout.EnumPopup(data.inputType, GUILayout.MaxWidth(75));
                data.pinMode = (ArduinoPinMode)EditorGUILayout.EnumPopup(data.pinMode, GUILayout.MaxWidth(75));
                data.inputName = EditorGUILayout.TextField(data.inputName, GUILayout.MaxWidth(300));
                data.pinNumber = EditorGUILayout.IntField(data.pinNumber, GUILayout.MaxWidth(30));
                data.inputIndex = EditorGUILayout.IntField(data.inputIndex, GUILayout.MaxWidth(30));
                data.canHoldButton = EditorGUILayout.Toggle(data.canHoldButton, GUILayout.MaxWidth(20));

                GUILayout.EndHorizontal();
            }

            if(GUILayout.Button("+"))
            {
                m_arduinoActionMap.CreateNewInput();
                return;
            } 

            GUILayout.Label("Outputs", EditorStyles.boldLabel);

            foreach(ArduinoOutputData data in m_arduinoActionMap.OutputDataList)
            {
                GUILayout.BeginHorizontal();

                if(GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    m_arduinoActionMap.RemoveOutput(data.dataIndex);
                    return;
                }

                data.outputName = EditorGUILayout.TextField(data.outputName, GUILayout.MaxWidth(300));
                data.pinNumber = EditorGUILayout.IntField(data.pinNumber, GUILayout.MaxWidth(30));

                GUILayout.EndHorizontal();
            }

            if(GUILayout.Button("+"))
            {
                m_arduinoActionMap.CreateNewOutput();
                return;
            }      
            
            EditorUtility.SetDirty(m_arduinoActionMap);

            EditorGUILayout.EndScrollView();
        }
    }
}