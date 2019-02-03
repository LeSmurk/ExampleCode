using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class script_Streaming : MonoBehaviour
{
    [Header("Utility")]
    public Button toggleButton;
    public script_GameManager scr_GameManager;
    public Camera mainCam;
    public Sprite unliveSprite;
    public Sprite liveSprite;

    [Header("Stream")]
    public bool streamOn = false;
    //prompts
    public GameObject promptImage;
    private Image promptSprite;
    public int promptButtonNum = 0;
    public int promptID = 0;
    public Image[] commandButtons;
    public Sprite[] streamPromptSprites;

    [Header("Chat")]
    public script_Chat scr_Chat;

    [Header("Particles")]
    public ParticleSystem clickParticles;
    public Color greenParticleColour;
    public Color redParticleColour;

    //raycasting
    private RaycastHit hit;
    private Ray ray;

    // Use this for initialization
    void Start ()
    {
        promptSprite = promptImage.GetComponent<Image>();

        //start at a random
        SetRandomPrompt();
	}
	
	// Update is called once per frame
	void Update ()
    {
        
	}

    public void ToggleStreaming()
    {
        //flip stream
        streamOn = !streamOn;

        //display
        if (streamOn)
        {
            toggleButton.GetComponent<Image>().sprite = liveSprite;

            //make game visible
            promptImage.SetActive(true);
            for (int i = 0; i < commandButtons.Length; i++)
                commandButtons[i].enabled = true;

            //set chat to online state
            scr_Chat.chatOffline = false;
        }

        else
        {
            toggleButton.GetComponent<Image>().sprite = unliveSprite;

            //turn off game
            promptImage.SetActive(false);
            for (int i = 0; i < commandButtons.Length; i++)
                commandButtons[i].enabled = false;

            //set chat to offline state
            scr_Chat.EnableOfflineChat();
        }

        //gain stream bar
        scr_GameManager.ToggleStreamUpdate(streamOn);
    }

    public void TurnOnStream()
    {
        streamOn = false;
        ToggleStreaming();
    }

    public void TurnOffStream()
    {
        streamOn = true;
        ToggleStreaming();
    }

    public void PressedCommandButton(int buttonPressed)
    {
        //only if live
        if (!streamOn)
            return;

        //check for mouse being blocked
        //raycast from camera to streaming button
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Letter"))
                return;
        }

        //Correct button
        if (buttonPressed == promptButtonNum)
        {
            //gain streaming points
            scr_GameManager.ClickedStreamEmote(true);

            //Play particle effect
            ParticleSystem.MainModule newMain = clickParticles.main;
            newMain.startColor = greenParticleColour;
            clickParticles.Play();

            //Setup new prompt
            SetRandomPrompt();
        }

        //incorrect button
        else
        {
            //lose streaming points
            scr_GameManager.ClickedStreamEmote(false);

            //play particle effect
            ParticleSystem.MainModule newMain = clickParticles.main;
            newMain.startColor = redParticleColour;
            clickParticles.Play();

        }
    }

    void SetRandomPrompt()
    {
        //don't pick this one again
        int[] avoidNumbers = new int[1];
        avoidNumbers[0] = promptID;

        //generate a new number that doesn't include the last one
        promptID = RandomNumberGenerator(0, streamPromptSprites.Length, avoidNumbers);

        //set the image of the prompt to the new ID
        promptSprite.sprite = streamPromptSprites[promptID];

        //pick a random position to put the correct prompt at
        promptButtonNum = Random.Range(0, 3);

        //put the correct image ID to this button number
        commandButtons[promptButtonNum].sprite = streamPromptSprites[promptID];

        ///////////////////////////////////////////////////////////////////////////////////////////
        //COULD BE NEATER
        //pick the next free button
        int freeButton = -1;
        for(int i = 0; i < 3; i++)
        {
            if(i != promptButtonNum)
            {
                freeButton = i;
            }
        }

        //put the current prompt image ID to avoid
        avoidNumbers[0] = promptID;
        //pick a random image ID that's not used already as a button
        int otherButtonImageID = RandomNumberGenerator(0, streamPromptSprites.Length, avoidNumbers);
        commandButtons[freeButton].sprite = streamPromptSprites[otherButtonImageID];

        //////////////////////////////////////////////////////////////////////////////////////////
        //COULD BE NEATER, REPEATING
        //pick the next free button
        int freeButton1 = -1;
        for (int i = 0; i < 3; i++)
        {
            if (i != promptButtonNum && i != freeButton)
            {
                freeButton1 = i;
            }
        }

        //put the last button image to avoid
        int[] avoidImageIds = new int[2];
        avoidImageIds[0] = promptID;
        avoidImageIds[1] = otherButtonImageID;
        //pick a random image ID that's not used already as a button
        int otherButton1 = RandomNumberGenerator(0, streamPromptSprites.Length, avoidImageIds);
        commandButtons[freeButton1].sprite = streamPromptSprites[otherButton1];
    }

    //Min inclusive max exclusive
    int RandomNumberGenerator(int min, int max, int[] numbersToAvoid)
    {
        //full possible range
        int maxSize = max - min;
        int[] numbersToChooseFrom = new int[maxSize];

        //check all numbers between min and max
        int currentElement = -1;
        for(int i = min; i < max; i++)
        {
            bool avoidNum = false;
            //test if any number to be excluded
            for(int z = 0; z < numbersToAvoid.Length; z++)
            {
                //number is to be avoided
                if (i == numbersToAvoid[z])
                    avoidNum = true;
            }

            //clear to store in choice array
            if(!avoidNum)
            {
                currentElement++;
                numbersToChooseFrom[currentElement] = i;
            }
        }

        //make sure a number was actually selected
        if(currentElement != -1)
        {
            //randomly choose a number from the possible numbers
            int randomSelected = numbersToChooseFrom[Random.Range(0, currentElement + 1)];

            return randomSelected;
        }

        //edge case
        return -1;
    }
}
