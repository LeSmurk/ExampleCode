using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

//globally known enum
public enum ChatState
{
    Random,
    Excited,
    Offline,
    Edit,
    Stream,
    Energy
}

public class script_Chat : MonoBehaviour
{

    //ref to all the chat boxes
    [Header("Text Boxes")]
    public List<Text> textBoxes;
    public List<Image> emoteBoxes = new List<Image>();
    private List<float> emotePos = new List<float>();
    public List<Sprite> storedEmotes;
    public float letterSeparation;

    [Header("Chat")]
    public float chatMoveRate;
    public float chatSlowestRate;
    public float chatIncreaseMoveRate;
    private float chatTimer;
    public int chatRandomRate;
    //how frequently a ping from bars being low affects chat
    public float barUpdateStateRate;
    private float workBarTimer;
    private float streamBarTimer;
    private float energyBarTimer;

    [Header("Text files")]
    public TextAsset randomChatFile;
    public string[] randomChatLines;
    public TextAsset excitedChatFile;
    public string[] excitedChatLines;
    public TextAsset offlineChatFile;
    public string[] offlineChatLines;
    public TextAsset editChatFile;
    public string[] editChatLines;
    public TextAsset streamChatFile;
    public string[] streamChatLines;
    public TextAsset energyChatFile;
    public string[] energyChatLines;

    //List with current state of chat, determined by multiple states
    [Header("Chat State")]
    public List<ChatState> stateChances = new List<ChatState>();
    public List<ChatState> stateChancesOffline = new List<ChatState>();
    public bool chatFreeze;
    public bool chatOffline;

    private bool temp = false;


    int current = -1;

    // Use this for initialization
    void Start ()
    {
        chatTimer = chatMoveRate;

        //For now, placed in this start
        StoreTextFiles();

        //init to random chat state at start
        for (int i = 0; i < 10; i++)
            stateChances.Add(ChatState.Random);

        //init to offline chat state at start
        for (int i = 0; i < 10; i++)
            stateChancesOffline.Add(ChatState.Offline);

        //get the emote boxes
        for (int i = 0; i < textBoxes.Count; i++)
        {
            emoteBoxes.Add(textBoxes[i].transform.GetChild(0).GetComponent<Image>());
            emotePos.Add(-1f);
        }

        ResetChat();
    }

    ////////////////////////////////////////////////////////
    //UPDATING CHAT
    // Update is called once per frame
    void Update ()
    {
        if(!chatFreeze)
        {
            chatTimer -= Time.deltaTime;

            if(chatTimer <= 0)
            {
                //only randomly spawn text
                if(Random.Range(0, chatRandomRate) == 0)
                    PushChat();

                //always reset the timer
                chatTimer = chatMoveRate;
            }

            //Bars updating chat
            workBarTimer -= Time.deltaTime;
            streamBarTimer -= Time.deltaTime;
            energyBarTimer -= Time.deltaTime;
        }
        
	}

    void PushChat()
    {
        //start at the top and go down the list, stop before last one
        for(int i = textBoxes.Count - 1; i > 0; i--)
        {
            //set this one to the text under it
            textBoxes[i].text = textBoxes[i - 1].text;

            //Check if beneath emote is active
            if (emoteBoxes[i - 1].enabled)
            {
                //turn on emote, change it to one below and place at correct place
                emoteBoxes[i].enabled = true;
                emoteBoxes[i].sprite = emoteBoxes[i - 1].sprite;
                emotePos[i] = emotePos[i - 1];
                emoteBoxes[i].rectTransform.localPosition = new Vector3(emotePos[i], emoteBoxes[i].rectTransform.localPosition.y, emoteBoxes[i].rectTransform.localPosition.z);
            }
            //turn off this emote
            else
                emoteBoxes[i].enabled = false;
        }

        //get new text from text files and set to bottom element
        textBoxes[0].text = GetNewChat();
        //parse text for emote
        Vector2 newEmote = GetNewEmote(textBoxes[0].text);
        //no emote
        if(newEmote.x == -1)
            emoteBoxes[0].enabled = false;
        //emote in text
        else
        {
            //remove number from text
            string editText = textBoxes[0].text;
            string removeNum = newEmote.x.ToString();//(Mathf.RoundToInt(newEmote.y)).ToString();
            textBoxes[0].text = editText.Replace(removeNum, "  ");

            //enable emote
            emoteBoxes[0].enabled = true;

            //set to new emote
            emoteBoxes[0].sprite = storedEmotes[(int)newEmote.x];
            //set to position
            emotePos[0] = -110 + (newEmote.y * letterSeparation);
            emoteBoxes[0].rectTransform.localPosition = new Vector3(emotePos[0], emoteBoxes[0].rectTransform.localPosition.y, emoteBoxes[0].rectTransform.localPosition.z);
        }

    }

    //////////////////////////////////////////////////////////////
    //CREATING CHAT
    string GetNewChat()
    {
        //pick a random text file from the state       
        ChatState txtState = stateChances[Random.Range(0, stateChances.Count)];
        if(chatOffline)
            txtState = stateChancesOffline[Random.Range(0, stateChances.Count)];

        string[] textLines = randomChatLines;
        switch (txtState)
        {
            case ChatState.Random:
                textLines = randomChatLines;
                break;
            case ChatState.Excited:
                textLines = excitedChatLines;
                break;
            case ChatState.Offline:
                textLines = offlineChatLines;
                break;
            case ChatState.Edit:
                textLines = editChatLines;
                break;
            case ChatState.Stream:
                textLines = streamChatLines;
                break;
            case ChatState.Energy:
                textLines = energyChatLines;
                break;
            default:
                break;
        }

        //pick a random line from the wanted text file
        //string returnLine = "empty " + Random.Range(0, 10);
        string returnLine = textLines[Random.Range(0, textLines.Length)];

        return returnLine;
    }

    string GetForcedChat()
    {
        if (current + 1 < streamChatLines.Length)
            current++;
        else
            current = 0;

        return streamChatLines[current];
    }

    ////make the 2D array based on the size of the txt
    //string[][] chatLines = new string[lines.Length][];
    ////loops through each line separating each character on the line, based on spaces
    //for (int i = 0; i < lines.Length; i++)
    //{
    //    string[] stringsOfLine = Regex.Split(lines[i], ",");
    //    chatLines[lines.Length - i - 1] = stringsOfLine;
    //}

    //read emotes
    Vector2 GetNewEmote(string text)
    {
        //parse text
        for(int i = 0; i < text.Length; i++)
        {
            //check if the current char is a number first
            if (char.IsDigit(text[i]))
            {
                //only one emote allowed per msg
                return new Vector2(int.Parse(text[i].ToString()), i);
            }
        }

        return new Vector2(-1, -1);
    }

    //SHOULD ONLY BE CALLED ONCE, AT GAME START
    public void StoreTextFiles()
    {
        //store each text file into separate lines
        randomChatLines = ReadFile(randomChatFile.text);
        excitedChatLines = ReadFile(excitedChatFile.text);
        offlineChatLines = ReadFile(offlineChatFile.text);
        editChatLines = ReadFile(editChatFile.text);
        streamChatLines = ReadFile(streamChatFile.text);
        energyChatLines = ReadFile(energyChatFile.text);

    }

    //read a file and send a 2d array of chars back
    string[] ReadFile(string file)
    {
        //separates the info in the txt file, based on each return or new line
        string[] lines = Regex.Split(file, "\r\n");

        return lines;
    }

    /////////////////////////////////////////////////////////////
    //Changing Chat State (chances betwen 0 - 1)
    public void SetStateChance(ChatState newState, float chances, bool stateOffline)
    {
        List<ChatState> stateList = stateChances;
        if (stateOffline)
            stateList = stateChancesOffline;

        //determine what states already exist in the storage
        int random = 0, excited = 0, offline = 0, work = 0, stream = 0, energy = 0;
        for(int i = 0; i < stateList.Count; i++)
        {
            switch (stateList[i])
            {
                case ChatState.Random:
                    random++;
                    break;
                case ChatState.Excited:
                    excited++;
                    break;
                case ChatState.Offline:
                    offline++;
                    break;
                case ChatState.Edit:
                    work++;
                    break;
                case ChatState.Stream:
                    stream++;
                    break;
                case ChatState.Energy:
                    energy++;
                    break;
                default:
                    break;
            }
        }

        //fractions for each state (0 - 10)
        int randFrac = Mathf.FloorToInt(10 * random / stateList.Count);
        int exciFrac = Mathf.FloorToInt(10 * excited / stateList.Count);
        int offlineFrac = Mathf.FloorToInt(10 * offline / stateList.Count);
        int workFrac = Mathf.FloorToInt(10 * work / stateList.Count);
        int streamFrac = Mathf.FloorToInt(10 * stream / stateList.Count);
        int energyFrac = Mathf.FloorToInt(10 * energy / stateList.Count);

        //reduce each state by the percentage we want to add in (rounding down)
        random = Mathf.RoundToInt(randFrac * (1 - chances));
        excited = Mathf.RoundToInt(exciFrac * (1 - chances));
        offline = Mathf.RoundToInt(offlineFrac * (1 - chances));
        work = Mathf.RoundToInt(workFrac * (1 - chances));
        stream = Mathf.RoundToInt(streamFrac * (1 - chances));
        energy = Mathf.RoundToInt(energyFrac * (1 - chances));

        //set the enw state to the percentage wanted
        switch (newState)
        {
            case ChatState.Random:
                random = Mathf.FloorToInt(stateList.Count * chances);
                break;
            case ChatState.Excited:
                excited = Mathf.FloorToInt(stateList.Count * chances);
                break;
            case ChatState.Offline:
                offline = Mathf.FloorToInt(stateList.Count * chances);
                break;
            case ChatState.Edit:
                work = Mathf.FloorToInt(stateList.Count * chances);
                break;
            case ChatState.Stream:
                stream = Mathf.FloorToInt(stateList.Count * chances);
                break;
            case ChatState.Energy:
                energy = Mathf.FloorToInt(stateList.Count * chances);
                break;
            default:
                break;
        }

        //if offline chat, replace random with offline and remove offline from normal
        if (!stateOffline)
            offline = 0;
        else
            random = 0;

        //if the number of states was reduced below 10, add some random state chances, or remove
        int numOff = 10 - (random + excited + offline + work + stream + energy);
        //change random to even out
        if (!stateOffline)
            random += numOff;
        //change offline to even out
        else
            offline += numOff;

        //edge case
        if (random < 0)
            random = 0;
        //edge case
        if (offline < 0)
            offline = 0;

        //set the state chances to the new set
        stateList.Clear();
        for(int i = 0; i < random; i++)
            stateList.Add(ChatState.Random);
        for (int i = 0; i < excited; i++)
            stateList.Add(ChatState.Excited);
        for (int i = 0; i < offline; i++)
            stateList.Add(ChatState.Offline);
        for (int i = 0; i < work; i++)
            stateList.Add(ChatState.Edit);
        for (int i = 0; i < stream; i++)
            stateList.Add(ChatState.Stream);
        for (int i = 0; i < energy; i++)
            stateList.Add(ChatState.Energy);

        if (!stateOffline)
        {
            //Debug.Log("chaning LIVE");
            stateChances = stateList;
        }
        else
        {
            //Debug.Log("Chaning offline");
            stateChancesOffline = stateList;
        }
    }

    public void RemoveState(ChatState removeState)
    {
       //remove any version of this state from the chances and add in random instead
       for(int i = 0; i < stateChances.Count; i++)
        {
            if(stateChances[i] == removeState)
                stateChances[i] = ChatState.Random;
        }
    }

    public void PingBarChanges(int barType, float severity)
    {
        //severity is how low the bars are, therefore more percentage should be added
        //if bars go past threshholds, allow to update chat state

        //-1 is just update excited (difficulty level determines this)
        if (barType == -1)
        {
            //prevent going too high
            if (severity > 1)
                severity = 1;
            
            //add more excited based on difficulty level (passed in by severity)
            SetStateChance(ChatState.Excited, severity, false);

            //increase speed of chat
            chatMoveRate -= chatIncreaseMoveRate;
            //capped to hard coded num hehehe
            if (chatMoveRate < 0.03f)
                chatMoveRate = 0.03f;
        }

        //0 is work, 1 is stream, 2 is energy
        if (barType == 0 && workBarTimer <= 0)
        {
            //Debug.Log("Pinging work bar " + severity);
            SetStateChance(ChatState.Edit, severity, false);
            workBarTimer = barUpdateStateRate;
        }
        else if (barType == 1 && streamBarTimer <= 0)
        {
            SetStateChance(ChatState.Stream, severity, false);
            streamBarTimer = barUpdateStateRate;
        }
        else if (barType == 2 && energyBarTimer <= 0)
        {
            SetStateChance(ChatState.Energy, severity, false);
            energyBarTimer = barUpdateStateRate;
        }

        //if the chat is offline, ping to update its states too
        if (chatOffline)
            EnableOfflineChat();
    }

    //offline chat takes whatever the current chat states are, and adds at least 30% offline
    public void EnableOfflineChat()
    {
        chatOffline = true;

        for(int i = 0; i < 10; i++)
            stateChancesOffline[i] = stateChances[i];

        SetStateChance(ChatState.Offline, 0.3f, true);
    }

    //Reset all chat numbers to beginning
    public void ResetChat()
    {
        //reset states
        SetStateChance(ChatState.Random, 1, false);
        //and offline one
        SetStateChance(ChatState.Offline, 1, true);

        //reset timers
        workBarTimer = barUpdateStateRate;
        streamBarTimer = barUpdateStateRate;
        energyBarTimer = barUpdateStateRate;

        //reset rate
        chatMoveRate = chatSlowestRate;

        chatOffline = true;

        //set all text boxes to blank
        for(int i = 0; i < textBoxes.Count; i++)
        {
            textBoxes[i].text = "";
            emoteBoxes[i].enabled = false;
        }
    }

    //freeze or not
    public void FreezeChat()
    {
        chatFreeze = true;
    }

    public void UnFreezeChat()
    {
        chatFreeze = false;
    }
}
