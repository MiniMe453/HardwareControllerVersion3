using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Rover.Interface;
using UnityEngine.InputSystem;
using System;

namespace Rover.OS
{
    public abstract class MonoBehaviourApplication : MonoBehaviour
    {
        private Command LOAD_APPLICATION;
        public string appCommand;
        public string appDescription;
        private int m_appID;
        public int AppID { get { return m_appID; } }
        public InputActionMap applicationInputs;
        private OSMode m_prevOSState;
        private bool m_appIsLoaded = false;
        public bool AppIsLoaded {get {return m_appIsLoaded;}}
        public event Action EOnAppUnloaded;
        public event Action EOnAppLoaded;

        void Awake()
        {
            m_appID = AppDatabase.RegisterApp(this);

            LOAD_APPLICATION = new Command(
                appCommand,
                appDescription,
                appCommand,
                () => Command_LoadAppWithID()
            );

            applicationInputs.AddAction("quitApp", binding: "<Keyboard>/q");

            applicationInputs["quitApp"].performed += Action_Quit;

            Init();
        }

        protected virtual void OnValidate()
        {

        }

        protected virtual void Init()
        {

        }

        public void LoadApp()
        {
            m_prevOSState = RoverOperatingSystem.OSMode;
            //OperatingSystem.SetOSState(OSState.Application);
            applicationInputs.Enable();
            m_appIsLoaded = true;
            EOnAppLoaded?.Invoke();
            OnAppLoaded();
        }

        public void QuitApp()
        {
            //OperatingSystem.SetOSState(m_prevOSState);
            applicationInputs.Disable();
            m_appIsLoaded = false;
            EOnAppUnloaded?.Invoke();
            OnAppQuit();
        }

        protected virtual void OnAppLoaded()
        {

        }

        protected virtual void OnAppQuit()
        {

        }

        private void Command_LoadAppWithID()
        {
            AppDatabase.LoadApp(AppID);
        }

        private void Action_Quit(InputAction.CallbackContext context)
        {
            AppDatabase.CloseApp(AppID);
        }
    }

    public static class AppDatabase
    {
        private static List<MonoBehaviourApplication> m_appList = new List<MonoBehaviourApplication>();

        public static List<MonoBehaviourApplication> Applications { get { return m_appList; } }
        private static MonoBehaviourApplication m_currentApplication;
        public static MonoBehaviourApplication CurrentlyLoadedApp {get{return m_currentApplication;}}

        public static int RegisterApp(MonoBehaviourApplication app)
        {
            m_appList.Add(app);

            return m_appList.Count - 1;
        }

        public static MonoBehaviourApplication GetAppFromID(int id)
        {
            return m_appList[id];
        }

        public static void LoadApp(int appID)
        {
            if(CurrentlyLoadedApp)
            {
                CurrentlyLoadedApp.QuitApp();
            }

            GetAppFromID(appID).LoadApp();
            m_currentApplication = GetAppFromID(appID);
        }

        public static void CloseApp(int appID)
        {
            GetAppFromID(appID).QuitApp();
        }
    }
}

