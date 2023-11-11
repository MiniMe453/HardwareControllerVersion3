using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DataLogEntry
{
    public string subject;
    public string date;
    public string time;
    [TextArea(15,20)]
    public string textEntry;
    public Sprite image;
    public string imageTitle;
}

[CreateAssetMenu(fileName = "Data Log", menuName = "Rover/Data Log", order = 1)]
public class DataLog : ScriptableObject
{
    public string dataPortID;
    public string dataLogName;
    public string dateUpdated;
    [SerializeField] private CrewMembers author;
    public CrewMembers Author {get{return author;}set{author = value;}}
    public string AuthorAsString {get {
        string authorName = "";
        
        switch(author)
        {
            case CrewMembers.Kenneth_Williams:
                authorName = "KENNETH W.";
                break;
            case CrewMembers.Connie_Hoskins:
                authorName = "CONNIE H.";
                break;
            case CrewMembers.Shirley_Thompson:
                authorName = "SHIRLEY T.";
                break;
            case CrewMembers.Thomas_Anderson:
                authorName = "THOMAS A.";
                break;
            case CrewMembers.Ron_Davis:
                authorName = "RON D.";
                break;
            case CrewMembers.Lillie_Nunez:
                authorName = "LILLIE N.";
                break;
        }
        
        return authorName;
    }}
    public bool hasBeenRead = false;
    public List<DataLogEntry> entries = new List<DataLogEntry>();
}
