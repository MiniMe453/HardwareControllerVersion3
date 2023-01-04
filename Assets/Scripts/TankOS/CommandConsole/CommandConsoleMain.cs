using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rover.OS;
using System;

public class CommandConsoleMain : MonoBehaviourApplication
{
    public static CommandConsoleMain Instance;
    public GameObject commandOutputLinePrefab;
    public RectTransform commandOutputTransform;
    public TextMeshProUGUI commandInputText;
    public TMP_InputField commandInputField;
    private Command m_clearConsole = new Command(
        "CLR",
        "Clears the console",
        "CLR",
        ClearConsoleOutput
    );
    
    
    void OnEnable()
    {
        Instance = this;
        commandInputField.onValueChanged.AddListener(OnCommandInputFieldUpdated);
        commandInputField.onSubmit.AddListener(OnCommandInputFieldSubmitted);
        commandInputText.text = "> |";
        //commandInputField.Select();

    }

    void Start()
    {
        commandInputField.ActivateInputField();
    }

    void OnDisable()
    {
        commandInputField.DeactivateInputField();
    }
    
    //Have override functions here

    void OnCommandInputFieldUpdated(string newValue)
    {
        commandInputText.text = "> " + newValue.ToUpper() + "|";
    }

    void OnCommandInputFieldSubmitted(string value)
    {
        UpdateConsoleOutput(value.ToUpper());
        commandInputText.text = "> |";

        if(!CommandDatabase.InvokeCommand(value))
            UpdateConsoleOutput("ERR: UNRECOGNIZED COMMAND");

        commandInputField.ActivateInputField();
    }

    public void UpdateConsoleOutput(string newLine)
    {
       GameObject newCmdLine = Instantiate(commandOutputLinePrefab);
       newCmdLine.GetComponent<TextMeshProUGUI>().text = "  " + newLine;

       newCmdLine.transform.SetParent(commandOutputTransform);
       newCmdLine.transform.SetAsFirstSibling();

       if(commandOutputTransform.childCount > 25)
       {
            Destroy(commandOutputTransform.GetChild(commandOutputTransform.childCount - 1).gameObject);
       }
    }

    public static void ClearConsoleOutput()
    {
        for(int i = 0; i < Instance.commandOutputTransform.childCount; i++)
        {
            Destroy(Instance.commandOutputTransform.GetChild(i).gameObject);
        }
    }
}

public static class CommandDatabase
{
    private static List<CommandBase> m_Commands = new List<CommandBase>();
    public static List<CommandBase> Commands {get {return m_Commands;}}

    private static Command m_helpCommand = new Command(
        "HELP",
        "Displays all commands",
        "HELP",
        CMD_Help
    );

    public static void RegisterCommand(CommandBase newCommand)
    {
        foreach(CommandBase command in Commands)
        {
            if(command.commandID == newCommand.commandID)
                return;
        }

        Commands.Add(newCommand);
        Debug.LogError(newCommand.commandID);
    }

    public static bool InvokeCommand(string commandID)
    {
        foreach(CommandBase command in Commands)
        {
            if(command.commandID.ToLower() == commandID)
            {
                (command as Command).Invoke();
                return true;
            }
        }

        return false;
    }

    public static void CMD_Help()
    {
        CommandConsoleMain.Instance.UpdateConsoleOutput("======= COMMANDS =======");

        foreach(CommandBase command in Commands)
        {
            CommandConsoleMain.Instance.UpdateConsoleOutput(command.commandFormat + ": " + command.commandDescription);
        }
        
        CommandConsoleMain.Instance.UpdateConsoleOutput("========================");
    }
}

public class CommandBase
{
    private string _commandID;
    private string _commandDescription;
    private string _commandFormat;
    private string _returnMessage;

    public string commandID { get { return _commandID; } }
    public string commandDescription { get { return _commandDescription; } }
    public string commandFormat { get { return _commandFormat; } }
    public string returnMessage { get { return _returnMessage; } }

    public CommandBase(string id, string description, string format, string returnMessage = "")
    {
        _commandID = id;
        _commandDescription = description;
        _commandFormat = format;
        _returnMessage = returnMessage;
        CommandDatabase.RegisterCommand(this);
    }
}

public class Command : CommandBase
{
    public Action command;

    public Command(string id, string description, string format, Action command, string returnMessage = "") : base(id, description, format, returnMessage)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command?.Invoke();
    }
}
