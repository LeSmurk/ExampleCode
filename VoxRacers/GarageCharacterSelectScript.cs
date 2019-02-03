using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

struct PlayerInfo
{
    public bool connected;
    public bool ready;
    public int character;
    public Vector3 position;
    public Vector3 toyOffset;
}


public class GarageCharacterSelectScript : MonoBehaviour, IGarageComponent
{
    public bool isCenterStage;


    private GarageScript garageScript;
    public GarageScript GarageScript => garageScript;


    private Transform cameraTransform;
    public Transform CameraTransform => cameraTransform;


    private GarageComponent parentComponent;
    public GarageComponent ParentComponent => parentComponent;

    private bool complete;
    public bool Complete => complete;

    private MainMenuScript mms;

    float racer0timer = 0.0f;
    float racer1timer = 0.0f;
    bool racer0bool = false;
    bool racer1bool = false;

    const float racerNavMax = 0.5f;


    // Use this for initialization
    void Awake()
    {
        cameraTransform = transform.Find("CameraLocation");

        GetMainMenuScript();
    }

    // Update is called once per frame
    void Update()
    {
        float x;       

        if (isCenterStage)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                x = -1;
            if (Input.GetKeyDown(KeyCode.RightArrow))
                x = 1;

            //Test button clicks
            for(int i = 0; i < playersInfo.Length; i++)
            {
                //Enter
                if (Input.GetButtonDown(mms.playerControls[i].Enter) || Input.GetButtonDown(mms.playerControls[i].Fire))
                {
                    //connect player
                    if (!playersInfo[i].connected)
                        ConnectPlayer(i);
                    //ready player
                    else
                        ReadyPlayer(i);
                }

                //back button
                if (Input.GetButtonDown(mms.playerControls[i].Back) || Input.GetButtonDown(mms.playerControls[i].Escape) || Input.GetButtonDown("Keyboard - Escape"))
                {
                    //Not connected, leave segment (ONLY FOR PLAYER 1)
                    if(!playersInfo[i].connected && i == 0)
                    {
                        complete = true;

                        UnreadyPlayer(1);
                        UnreadyPlayer(0);

                        DisconnectPlayer(1);
                        DisconnectPlayer(0);
                    }

                    //connected - DC or Unready
                    else if(playersInfo[i].connected)
                    {
                        //player isn't ready - disconnect
                        if (!playersInfo[i].ready)
                            DisconnectPlayer(i);

                        //player is readied, unready
                        else
                            UnreadyPlayer(i);
                    }
               
                }


                //navigation
                x = Mathf.Clamp(Input.GetAxis(mms.playerControls[i].Horizontal), -0.5f, 0.5f) * -2; // -1 - 1


                // movement
                Navigation(x, i);
            }
        }


        else
        {
            x = 0;
        }

        //CHARACTER SELECT STUFF
        if (countdownEnabled)
        {
            countdownTimer -= Time.deltaTime;

            if (countdownTimer <= 0)
                mms.StartRace(playersInfo[0].character, playersInfo[1].character);

            //show on text
            //countdownText.GetComponent<Text>().text = Mathf.Ceil(countdownTimer).ToString();
            if(Mathf.Ceil(countdownTimer) == 2)
            {
                countdownNumbers[0].SetActive(false);
                countdownNumbers[1].SetActive(true);
            }
            else if (Mathf.Ceil(countdownTimer) == 1)
            {
                countdownNumbers[1].SetActive(false);
                countdownNumbers[2].SetActive(true);
            }

        }


        if (racer0bool)
            racer0timer += Time.deltaTime;
        if (racer1bool)
            racer1timer += Time.deltaTime;

        if (racer0timer > racerNavMax)
        {
            racer0bool = false;
            racer0timer = 0;
        }
        if (racer1timer > racerNavMax)
        {
            racer1bool = false;
            racer1timer = 0;
        }

    }

    public void Activate(GarageComponent parent = GarageComponent.Start)
    {
        isCenterStage = true;

        complete = false;

        parentComponent = parent;
    }

    public void Deactivate()
    {
        isCenterStage = false;

        complete = false;

        DisconnectPlayer(1);
        DisconnectPlayer(0);
    }

    private void GetMainMenuScript()
    {
        if (!mms)
        {
            GameObject mc = GameObject.Find("Menu Code");
            if (mc) mms = mc.GetComponent<MainMenuScript>();
        }
    }


    //////////////////////////////////////////////////////////////////////////////////
    // CHARACTER SELECT STUFF

    [Header("Infos")]
    //Player info
    private PlayerInfo[] playersInfo = new PlayerInfo[2];
    //Character info
    public int totalCharacterNum = 6;
    //countdown info
    private float countdownTimer = 3;
    private bool countdownEnabled = false;

    [Header("Canvai")]
    //UI canvas
    //private GameObject countdownText;
    //character canvas
    private GameObject characterGridCanvas;
    private GameObject[] characterIcons = new GameObject[6];
    //player canvas
    private GameObject playerShownCanvas;
    private GameObject[] playerIcons = new GameObject[6];
    private Transform centreTransform;
    private Transform leftTransform;
    private Transform rightTransform;
    //toy offset
    public Vector3 toyOffsetLeft = new Vector3(0, 0, 0);
    public Vector3 toyOffsetRight = new Vector3(0, 0, 0);
    public Vector3 toyOffsetCentre = new Vector3(0, 0, 0);
    //ready image
    private GameObject[] readyText = new GameObject[2];
    private float readyTextYDisplacement;
    //connect button
    private GameObject[] connectButton = new GameObject[2];
    private float connectButtonYDisplacement;

    //numbering and ready
    public GameObject[] countdownNumbers = new GameObject[3];

    [Header("Control Speeds")]
    public int maxSelectSpeed = 20;
    public int minSelectSpeed = 30;
    public int defaultSelectSpeed = 120;

    [Header("readonly")]
    public int intSelector;
    public int menuHowFast;
    public int intMenuOption;

    // Use this for initialization
    void Start()
    {
        //get canvases
        characterGridCanvas = transform.Find("CharSelectCanvas").gameObject;
        playerShownCanvas = transform.Find("PlayersShownCanvas").gameObject;

        //get positions
        centreTransform = playerShownCanvas.transform.Find("Centre Position");
        leftTransform = playerShownCanvas.transform.Find("Left Position");
        rightTransform = playerShownCanvas.transform.Find("Right Position");

        ////get countdown text
        //countdownText = playerShownCanvas.transform.Find("Countdown Text").gameObject;

        ////get ready text
        readyText[0] = playerShownCanvas.transform.Find("Ready Text Player 1").gameObject;
        readyText[1] = playerShownCanvas.transform.Find("Ready Text Player 2").gameObject;

        //create diff in dist of left pos Y and ready text y
        readyTextYDisplacement = readyText[0].transform.localPosition.y - leftTransform.localPosition.y;

        //get connect button
        connectButton[0] = playerShownCanvas.transform.Find("Connect Button Player 1").gameObject;
        connectButton[1] = playerShownCanvas.transform.Find("Connect Button Player 2").gameObject;

        //create difference in distance of left position Y to connect button Y
        connectButtonYDisplacement = leftTransform.localPosition.y - connectButton[0].transform.localPosition.y;

        //put canvas objects into array
        for (int i = 0; i < totalCharacterNum; i++)
        {
            characterIcons[i] = characterGridCanvas.transform.GetChild(i).gameObject;

            playerIcons[i] = playerShownCanvas.transform.GetChild(i).gameObject;
        }

        //init character list (IF YOU WANT TO ADD MORE PLAYERS DO IT HERE)
        InitPlayerList(0);
        InitPlayerList(1);

        //TEMPORARY
        ConnectPlayer(0);
        ConnectPlayer(1);
        //ReadyPlayer(0);
        //ReadyPlayer(1);
    }

    // Initialising
    void InitPlayerList(int playerNum)
    {
        PlayerInfo newPlayer = new PlayerInfo();
        newPlayer.connected = false;
        newPlayer.ready = false;
        newPlayer.character = -1;
        newPlayer.position = new Vector3(0, 0, 0);

        playersInfo[playerNum] = newPlayer;
    }

    // Connects player once button is pressed
    public void ConnectPlayer(int playerNum)
    {
        //only allow to connect if now already
        if (!playersInfo[playerNum].connected)
        {
            //remove connect button
            connectButton[playerNum].SetActive(false);

            //activate the next players' connect button
            if (playerNum + 1 < connectButton.Length)
                connectButton[playerNum + 1].SetActive(true);

            //connect info
            playersInfo[playerNum].connected = true;

            //set position of ALL the connected players
            for (int i = 0; i < playersInfo.Length; i++)
                if(playersInfo[i].connected)
                    SetPosition(i);

            //set to first available character slot
            ScrollCharacter(playerNum, true);
        }

        //already connected, disconnect
        else
            DisconnectPlayer(playerNum);
        
    }

    public void DisconnectPlayer(int playerNum)
    {
        //only allow to disconnect if all players after it are disconnected
        int connectedPlayers = 0;
        for (int i = playerNum + 1; i < playersInfo.Length; i++)
            if (playersInfo[i].connected)
                connectedPlayers++;

        if (connectedPlayers == 0)
        {
            //show connect button
            connectButton[playerNum].SetActive(true);

            //disable the next players' connect button
            if (playerNum + 1 < connectButton.Length)
                connectButton[playerNum + 1].SetActive(false);

            //set to null character
            ChangeCharacter(playerNum, -1);

            //info remove
            playersInfo[playerNum].connected = false;
            playersInfo[playerNum].ready = false;
        }

        else
            Debug.Log("Higher number players are still connected");       

    }

    // Take a wild guess what this does
    public void ReadyCharacter(int characterNum)
    {
        //the player that is pressing the button
        int currentPlayer = -1;

        //find which player is hovering this character
        for(int z = 0; z < playersInfo.Length; z++)
        {
            if (playersInfo[z].character == characterNum)
                currentPlayer = z;
        }

        //only allow to execute if player isn't already ready (and edge case)
        if (currentPlayer != -1)
        {
            if (!playersInfo[currentPlayer].ready)
            {
                Debug.Log("Ready player " + currentPlayer);

                //update info
                playersInfo[currentPlayer].ready = true;

                //show graphically
                readyText[currentPlayer].SetActive(true);

                //PUT SFX HERE
                switch (characterNum)
                {
                    case (0):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect1", transform.position);
                        break;

                    case (1):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect2", transform.position);
                        break;

                    case (2):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect3", transform.position);
                        break;

                    case (3):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect4", transform.position);
                        break;

                    case (4):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect5", transform.position);
                        break;

                    case (5):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect6", transform.position);
                        break;
                }

                //test if all players are ready
                int allReady = 0;

                for (int i = 0; i < playersInfo.Length; i++)
                    if (playersInfo[i].ready || !playersInfo[i].connected)
                        allReady++;

                if (allReady == playersInfo.Length)
                    StartCountdown();
            }

            //player is already ready, so unready
            else
                UnreadyPlayer(currentPlayer);
        }

    }

    //same as ready character, but given a player number instead of the character
    public void ReadyPlayer(int playerNum)
    {
        //only allow to execute if player isn't already ready (and edge case)
        if (playerNum != -1)
        {
            if (!playersInfo[playerNum].ready)
            {
                Debug.Log("Ready player " + playerNum);

                //update info
                playersInfo[playerNum].ready = true;

                //show graphically
                readyText[playerNum].SetActive(true);

                //PUT SFX HERE
                switch (playersInfo[playerNum].character)
                {
                    case(0):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect1", transform.position);
                        break;

                    case (1):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect2", transform.position);
                        break;

                    case (2):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect3", transform.position);
                        break;

                    case (3):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect4", transform.position);
                        break;

                    case (4):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect5", transform.position);
                        break;

                    case (5):
                        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/CharacterSelect/CharacterSelect6", transform.position);
                        break;
                }

                //test if all players are ready
                int allReady = 0;

                for (int i = 0; i < playersInfo.Length; i++)
                    if (playersInfo[i].ready || !playersInfo[i].connected)
                        allReady++;

                if (allReady == playersInfo.Length)
                    StartCountdown();

            }

            //player is already ready, so unready
            else
                UnreadyPlayer(playerNum);
        }
    }

    void UnreadyPlayer(int playerNum)
    {
        Debug.Log("UNReady player " + playerNum);

        //update info
        playersInfo[playerNum].ready = false;

        //stop countdown
        StopCountdown();

        //remove graphically
        readyText[playerNum].SetActive(false);
    }

    // Scrolls through player choices
    void ScrollCharacter(int playerNum, bool directionRight)
    {
        //only allow to scroll if not already readied
        if (playersInfo[playerNum].ready == true)
            return;

        int currentChar = playersInfo[playerNum].character;

        for (int i = 0; i < totalCharacterNum; i++)
        {
            bool retry = false;

            //increasing scroll num
            if (directionRight)
            {
                //aiming for this number
                currentChar++;
            }

            //decreasing scroll num
            else
            {
                //aiming for this number
                currentChar--;
            }

            //loop current character num (0 to 7)
            if (currentChar < 0)
                currentChar = totalCharacterNum - 1;
            else if (currentChar >= totalCharacterNum)
                currentChar = 0;

            //check another player isn't on this character (if they are, redo whole for loop)
            if (!CheckCharacterFree(currentChar))
                retry = true;

            //exit for loop if there are no issues with number
            if (!retry)
                break;
        }

        //set new graphics etc to the new wanted character
        ChangeCharacter(playerNum, currentChar);
    }

    //update to new character
    public void ChangeCharacter(int playerNum, int wantedCharNum)
    {
        //for mouse, playernum will be given as -1
        if(playerNum == -1)
        {
            //figure out which player this change is meant for
            for(int i = 0; i < playersInfo.Length; i++)
            {
                //if they aren't ready, make the player number this one and exit loop
                if (!playersInfo[i].ready)
                {
                    playerNum = i;
                    break;
                }
            }

            //if the playernum is still -1, break out of function as all players have been taken
            if (playerNum == -1)
                return;

        }

        //if player isn't connected, don't do this
        if (!playersInfo[playerNum].connected)
            return;

        //make sure the last character isn't null
        if(playersInfo[playerNum].character > -1)
        {
            //change last button back
            characterIcons[playersInfo[playerNum].character].GetComponent<Image>().color = characterIcons[playersInfo[playerNum].character].GetComponent<Button>().colors.normalColor;
            characterIcons[playersInfo[playerNum].character].GetComponent<HoverOverButtonScript>().buttonHighlighted = false;

            //turn off last player shown
            playerIcons[playersInfo[playerNum].character].SetActive(false);
        }

        //Regular character change (-1 would be forcing no new char)
        if(wantedCharNum != -1)
        {
            //change button
            characterIcons[wantedCharNum].GetComponent<Image>().color = characterIcons[wantedCharNum].GetComponent<Button>().colors.disabledColor;
            characterIcons[wantedCharNum].GetComponent<HoverOverButtonScript>().buttonHighlighted = true;

            //change player shown graphics
            playerIcons[wantedCharNum].transform.localPosition = playersInfo[playerNum].position;
            playerIcons[wantedCharNum].SetActive(true);

            //adjust toy position
            playerIcons[wantedCharNum].transform.GetChild(1).localPosition = playersInfo[playerNum].toyOffset;


            PlayAnim(wantedCharNum);
        }

        //update player number (even if -1)
        playersInfo[playerNum].character = wantedCharNum;
    }

    // Test if this character isn't already selected by another player (TRUE IS CHAR FREE)
    bool CheckCharacterFree(int currentChar)
    {
        bool characterIsFree = true;

        for (int z = 0; z < playersInfo.Length; z++)
            if (currentChar == playersInfo[z].character)
                characterIsFree = false;

        return characterIsFree;
    }

    //set the player position based on how many players there are
    void SetPosition(int playerNum)
    {
        //I WOULD HAVE TO IMPROVE THIS IF MORE PLAYERS THAN 2, but for now it'll do tee hee

        //if player 1 is alone, centralise
        if (playerNum == 0 && !playersInfo[playerNum + 1].connected)
        {
            playersInfo[playerNum].position = centreTransform.localPosition;
            playersInfo[playerNum].toyOffset = toyOffsetCentre;

            //set connect button position too
            //connectButton[playerNum].transform.localPosition = new Vector3(centreTransform.localPosition.x, centreTransform.localPosition.y + connectButtonYDisplacement, centreTransform.localPosition.z);
            //set ready text position too
            readyText[playerNum].transform.localPosition = new Vector3(centreTransform.localPosition.x, centreTransform.localPosition.y + readyTextYDisplacement, centreTransform.localPosition.z);

            //update position of current character (if it exists)
            if (playersInfo[playerNum].character != -1)
            {
                playerIcons[playersInfo[playerNum].character].transform.localPosition = playersInfo[playerNum].position;
                //adjust toy position
                playerIcons[playersInfo[playerNum].character].transform.GetChild(1).localPosition = playersInfo[playerNum].toyOffset;
            }
        }

        //if player 1 isn't alone
        else if (playerNum == 0 && playersInfo[playerNum + 1].connected)
        {
            playersInfo[playerNum].position = leftTransform.localPosition;
            playersInfo[playerNum].toyOffset = toyOffsetLeft;
            //set connect button position too
            //connectButton[playerNum].transform.localPosition = new Vector3(leftTransform.localPosition.x, leftTransform.localPosition.y + connectButtonYDisplacement, leftTransform.localPosition.z);
            //set ready text position too
            readyText[playerNum].transform.localPosition = new Vector3(leftTransform.localPosition.x, leftTransform.localPosition.y + readyTextYDisplacement, leftTransform.localPosition.z);

            //update position of current character (if it exists)
            if (playersInfo[playerNum].character != -1)
            {
                playerIcons[playersInfo[playerNum].character].transform.localPosition = playersInfo[playerNum].position;
                //adjust toy position
                playerIcons[playersInfo[playerNum].character].transform.GetChild(1).localPosition = playersInfo[playerNum].toyOffset;
            }

        }

        //if player 2, ALWAYS GOES RIGHT
        else if (playerNum == 1)
        {
            playersInfo[playerNum].position = rightTransform.localPosition;
            playersInfo[playerNum].toyOffset = toyOffsetRight;
            //adjust toy position
            if(playersInfo[playerNum].character != -1)
                playerIcons[playersInfo[playerNum].character].transform.GetChild(1).localPosition = playersInfo[playerNum].toyOffset;
        }
    }

    void PlayAnim(int playerNum)
    {
        playerIcons[playerNum].GetComponentInChildren<Animator>().Play("CharSelectAnim");
    }

    //Begin the countdown for launching into the next scene
    void StartCountdown()
    {
        //start timer
        countdownTimer = 3;
        countdownEnabled = true;

        //display countdown
        //countdownText.SetActive(true);
        //display numbers
        countdownNumbers[0].SetActive(true);

    }

    //Cancel the countdown and wipe
    void StopCountdown()
    {
        //display
        //countdownText.SetActive(false);

        //display numbers
        countdownNumbers[0].SetActive(false);
        //display numbers
        countdownNumbers[1].SetActive(false);
        //display numbers
        countdownNumbers[2].SetActive(false);

        //timer
        countdownEnabled = false;
    }



    private int Navigation(float x, int character)
    {
        //Debug.Log("X" + x);
        // Reset
        if (x < 0.3f && x > -0.3f)
        {
            intSelector = 0;
            menuHowFast = minSelectSpeed;
        }
        // left
        if (x < -0.3f)
        {
            intSelector--;
            if (intSelector <= -menuHowFast)
            {
                intSelector = 0;
                if (menuHowFast > maxSelectSpeed)
                {
                    menuHowFast = menuHowFast - 2;
                }
            }
            if (intSelector < 0 && intSelector >= -1)
            {
                //if (racer0bool == false)
                //{
                //    ScrollCharacter(character, true);
                //    intMenuOption++;
                //    if (character == 0)
                //        racer0bool = true;
                //    else
                //        racer1bool = true;
                //}
            }

            if (character == 0 && racer0bool == false)
            {
                intMenuOption--;
                racer0bool = true;

                ScrollCharacter(character, true);
            }
            if (character == 1 && racer1bool == false)
            {
                intMenuOption--;
                racer1bool = true;

                ScrollCharacter(character, true);
            }
        }
        // right
        if (x > 0.3f)
        {
            intSelector++;
            if (intSelector >= menuHowFast)
            {
                intSelector = 0;
                if (menuHowFast > maxSelectSpeed)
                {
                    menuHowFast = menuHowFast - 2;
                }
            }
            if (intSelector > 0 && intSelector <= 1)
            {
                //if (racer1bool == false)
                //{
                //    ScrollCharacter(character, false);
                //    intMenuOption--;
                //    if (character == 0)
                //        racer0bool = true;
                //    else
                //        racer1bool = true;
                //}
            }

            if (character == 0 && racer0bool == false)
            {
                intMenuOption--;
                racer0bool = true;

                ScrollCharacter(character, false);
            }
            if (character == 1 && racer1bool == false)
            {
                intMenuOption--;
                racer1bool = true;

                ScrollCharacter(character, false);
            }
        }


        if (intMenuOption >= 8)
        {
            intMenuOption = 0;
        }
        if (intMenuOption < 0)
        {
            intMenuOption = 7;
        }

        return intMenuOption;
    }
}
