using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using Rover.Settings;
using System.IO;
using System.Linq;

public class DataLogImporter : AssetPostprocessor
{
    static int columns = 5;
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string assetPath in importedAssets)
        {
            if(!assetPath.Contains(".csv"))
                return;
            
            TextAsset dataLogCsv = (TextAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset));
            string[] data = dataLogCsv.text.Split(new string [] {"|", "\n"}, StringSplitOptions.None);

            if(data[0] != "DataLogMarker")
            {
                Debug.LogError($"{assetPath} file does not conform to the data log structure. Ignore if this doesn't matter to you");
                continue;
            }

            string assetName = GetAssetNameFromPath(assetPath);
            string dataLogDataAssetPath = GameSettings.DATA_LOG_FOLDER + assetName + ".asset";
            string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + dataLogDataAssetPath;
            
            if(File.Exists(fullPath))
            {
                DataLog dataLog = (DataLog)AssetDatabase.LoadAssetAtPath(dataLogDataAssetPath, typeof(DataLog));
                SetDataLogAssetData(dataLog, data);
            }
            else
            {
                DataLog newDataLog = ScriptableObject.CreateInstance<DataLog>();
                AssetDatabase.CreateAsset(newDataLog, dataLogDataAssetPath);
                SetDataLogAssetData(newDataLog, data);
            }

            Debug.Log($"Data Log Entry {assetName} was created!");
        }
        foreach (string assetPath in deletedAssets)
        {
            string assetName = GetAssetNameFromPath(assetPath);
            string dataLogDataAssetPath = GameSettings.DATA_LOG_FOLDER + assetName + ".asset";
            string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + dataLogDataAssetPath;
            
            if(File.Exists(fullPath))
            {
                AssetDatabase.DeleteAsset(dataLogDataAssetPath);
            }
        }

        // for (int i = 0; i < movedAssets.Length; i++)
        // {
        //     Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        // }

        // if (didDomainReload)
        // {
        //     Debug.Log("Domain has been reloaded");
        // }
    }

    static string GetAssetNameFromPath(string path)
    {
        string[] assetPathSplit = path.Split("/", StringSplitOptions.None);

        return assetPathSplit[assetPathSplit.Length - 1].Split(".")[0];
    }

    static void SetDataLogAssetData(DataLog asset, string[] data)
    {
        bool importWithErrors = false;
        int numOfRows = data.Length/columns;

        float corruptionChance = 0;
        bool floatParseSuccesful = float.TryParse(data[1], out corruptionChance);
        corruptionChance = floatParseSuccesful? corruptionChance : GameSettings.DEFAULT_CORRUPTION_CHANCE;

        asset.dataLogName = CorruptString(data[columns*1], corruptionChance);
        
        CrewMembers crewMember;
        Enum.TryParse(data[columns*2], out crewMember);
        asset.Author = crewMember;

        asset.dateUpdated = data[columns*3];
        // asset.entries.Clear();

        asset.dataPortID = data[columns*4];

        for(int i = 5; i < numOfRows; i++)
        {
            DataLogEntry newEntry = new DataLogEntry();

            int idx = asset.entries.FindIndex((x) => x.date + x.time == data[i * columns] + data[i * columns + 1]);

            if(idx != -1)
            {
                UpdateExistingEntry(i, idx, data, corruptionChance, asset);
                continue;
            }

            newEntry.date = data[i * columns];
            newEntry.time = data[i * columns + 1];
            newEntry.subject = CorruptString(data[i * columns + 2], corruptionChance);
            
            string bodyText = data[i * columns + 3];
            string imageText = data[i * columns + 4];
            
            if(bodyText == "")
            {
                Debug.LogError($"Entry {asset.dataLogName} has no body text! Maybe it needs to have an image assigned to it.");
                importWithErrors = true;
                //We want to continue because sometimes there shouldn't be any body text.
            }

            bodyText = bodyText.Replace('&','\n');
            bodyText = bodyText.Replace('�', '\'');
            bodyText = CorruptString(bodyText, corruptionChance);

            imageText = imageText.Replace('&','\n');
            imageText = imageText.Replace('�', '\'');
            imageText = CorruptString(imageText, corruptionChance);

            newEntry.textEntry = bodyText;
            newEntry.imageTitle = imageText;

            asset.entries.Add(newEntry);
        }

        if(importWithErrors)
        {
            Debug.Log("Scriptable object was updated, but there were some errors. Please check the error log.");
        }
        Debug.Log("If you removed any entries, check the data asset. Updating on removed entries doesn't exist yet :P");
    }

    static string CorruptString(string str, float corruptionChance)
    {
        StringBuilder newString = new StringBuilder(str);

        for(int i = 0; i++ < str.Length - 1;)
        {
            float chance = UnityEngine.Random.Range(0f, 1f);
            Debug.Log(chance < corruptionChance || str[i] == ' ');

            if(chance < corruptionChance || GameSettings.PUNCTUATION.Contains(str[i]))
                continue;

            newString[i] = GameSettings.ALLOWED_REPLACE_CHARS[UnityEngine.Random.Range(0,GameSettings.ALLOWED_REPLACE_CHARS.Length - 1)];
        }

        return newString.ToString();
    }

    static void UpdateExistingEntry(int columnIdx, int entryIdx, string[] data, float corruptionChance, DataLog dataLog)
    {
        DataLogEntry newEntry = new DataLogEntry
        {
            image = dataLog.entries[entryIdx].image,
            date = data[columnIdx * columns],
            time = data[columnIdx * columns + 1]
        };

        string bodyText = data[columnIdx * columns + 3];
        string imageText = data[columnIdx * columns + 4];
            
        if(bodyText == "")
        {
            Debug.LogError($"Entry {dataLog.dataLogName} has no body text! Maybe it needs to have an image assigned to it.");
        }

        bodyText = bodyText.Replace('&','\n');
        bodyText = bodyText.Replace('�', '\'');
        bodyText = CorruptString(bodyText, corruptionChance);

        imageText = imageText.Replace('&','\n');
        imageText = imageText.Replace('�', '\'');
        imageText = CorruptString(imageText, corruptionChance);

        newEntry.textEntry = bodyText;
        newEntry.imageTitle = imageText;

        dataLog.entries[entryIdx] = newEntry;
    }
}