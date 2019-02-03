using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class script_GameManager : MonoBehaviour {

    public int barMaxNum = 100;

    public script_AudioManager audioManager_scr;

    [Header("Game State")]
    public bool gameLive = true;
    private bool workFreeze = false;
    private bool streamFreeze = false;
    private bool energyFreeze = false;
    public GameObject menuObject;
    public GameObject pauseObject;
    public GameObject leaderboardObject;
    public script_Leaderboards scr_leaderboards;
    public GameObject tutorialObject;
    public GameObject optionsObject;
    public GameObject creditsObject;
    public GameObject endObject;
    private Text endScore;

    [Header("Clear Game")]
    public Animator titleAnimator;
    public ParticleSystem titleParticles;
    public float showMenuTime;
    private float menuShowTimer = 0;
    private bool menuActive = true;

    [Header("Difficulty Progression")]
    public int difficultyLevel;
    public float rateDifficultyChange;
    public float difficultyUpdateTime;
    //every x many seconds go by, increase the difficulty by an amount
    private float difficultyTimer;
    //use how long game has been going for to make rate at which difficulty changes go faster
    private float gameTime = 0;
    public float increaseDifficultyRate;
    private float increaseDifficultyTimer;
    public Text scoreUI;

    [Header("Work Bar")]
    public Slider workBar;
    public int workBarNum;
    //editor script
    public script_MovePanels panels_scr;
    //base starting rate bar updates at
    public float workRateSlow;
    //current rate bar goes updates at
    public float workRate;
    private float workRateTimer;
    //
    public int editingWorkGain = 10;

    [Header("Stream Bar")]
    public Slider streamBar;
    public int streamBarNum;
    public float streamRateSlow;
    public float streamRate;
    private float streamRateTimer;
    //
    public int streamingEmoteGain = 5;
    public int streamingEmoteLoss = 2;
    private bool streaming = false;
    //spawning letters
    public script_Spawning letterSpawning_scr;

    [Header("Chat")]
    public script_Chat chat_scr;
    public script_Streaming streaming_scr;

    [Header("Energy Bar")]
    public Slider energyBar;
    public int energyBarNum;
    public float energyRateSlow;
    public float energyRate;
    private float energyRateTimer;
    //
    public int energyStreamDrain = -2;
    public int energyFoodGain = 1;

    [Header("Food")]
    public GameObject orderFoodButton;
    private float foodReorderTimer = 0;
    public float foodReorderMaxTime = 3f;
    public GameObject foodClickButton;
    public int foodSize;
    private int foodLeft;
    public int foodEatAmount;
    public Slider foodSlider;

    [Header("Quake")]
    public Transform[] barTransforms;
    public float maxVal = 10;
    public float aimingVal = 1;
    public float quakeRate = 0.5f;

    //GAME STATE
    public void StartGame()
    {
        titleAnimator.gameObject.SetActive(false);
        menuActive = false;

        //unpause
        UnPauseGame();

        //init rate to slowest
        workRate = workRateSlow;
        streamRate = streamRateSlow;
        energyRate = energyRateSlow;

        //init timers
        workRateTimer = workRate;
        streamRateTimer = streamRate;
        energyRateTimer = energyRate;

        difficultyTimer = difficultyUpdateTime;
        increaseDifficultyTimer = increaseDifficultyRate;

        //init bars to max
        ChangeWorkBar(100);
        ChangeStreamBar(100);
        ChangeEnergyBar(100);

        //disable menu
        menuObject.SetActive(false);

    }

    public void StartTutorial()
    {
        titleAnimator.gameObject.SetActive(false);

        tutorialObject.SetActive(true);

        //reset tutorial
        for(int i = 0; i < tutorialObject.transform.childCount; i++)
        {
            tutorialObject.transform.GetChild(i).gameObject.SetActive(false);
        }
        //enable first panel
        tutorialObject.transform.GetChild(0).gameObject.SetActive(true);

        //pause game
        PauseGame();

        //disable menu object
        menuObject.SetActive(false);

    }

    void LostGame()
    {
        endObject.SetActive(true);
        //put score into text
        int finalScore = Mathf.RoundToInt(gameTime * (gameTime / 50));
        endScore.text = finalScore.ToString();

        scr_leaderboards.ChangeLeaderboard(finalScore);

        audioManager_scr.PlayEndGameSFX();

        PauseGame();
        //ClearGame();

        audioManager_scr.StopMusic(false);
    }

    public void ClearGame()
    {
        //play title anim
        titleAnimator.gameObject.SetActive(true);
        titleAnimator.SetTrigger("Clear");
        titleParticles.Play();

        audioManager_scr.PlaySwipeSFX();

        //clear all objects in the scene
        letterSpawning_scr.DestroyLetters();

        gameTime = 0;

        //turn off stream
        streaming_scr.TurnOffStream();

        //reset chat
        chat_scr.ResetChat();

        //reset difficulty level
        difficultyLevel = 0;

        //reset food
        DisableFood();
        orderFoodButton.SetActive(true);
        foodReorderTimer = 0;

        //init bars to max
        ChangeWorkBar(100);
        ChangeStreamBar(100);
        ChangeEnergyBar(100);

        //reset editor panels
        panels_scr.ResetPanels();

        audioManager_scr.StopMusic(true);
    }

    //PAUSE SHOULD SHOW ONLY RESUME, OPTIONS and EXIT
    //ENDGAME SHOULD PAUSE BUT SHOW MENU
    public void PauseGame()
    {
        gameLive = false;

        //freeze all bars
        workFreeze = true;
        streamFreeze = true;
        energyFreeze = true;

        //pause chat
        chat_scr.FreezeChat();
        //letter spawn
        letterSpawning_scr.spawnFreeze = true;

        //enable pause menu
        if (!menuActive && !endObject.active)
        {
            pauseObject.SetActive(true);
            //pause music
            audioManager_scr.TogglePauseMusic(true);
        }
    }

    public void UnPauseGame()
    {
        gameLive = true;

        //unfreeze all bars
        workFreeze = false;
        streamFreeze = false;
        energyFreeze = false;

        //unpause chat
        chat_scr.UnFreezeChat();

        //letter spawn
        letterSpawning_scr.spawnFreeze = false;

        //disable pause menu
        pauseObject.SetActive(false);

        //unpause music
        audioManager_scr.TogglePauseMusic(false);
    }

    //Main Menu
    public void ExitToMenu()
    {
        //play sound
        audioManager_scr.PlayClickSFX();

        //disable pause menu
        pauseObject.SetActive(false);
        //show menu
        menuShowTimer = showMenuTime;
        menuActive = true;

        PauseGame();

        ClearGame();
    }

    //Back button
    public void BackButton()
    {
        //go back to whatever we want (starting with the things on top)
        //Endgame screen
        if(endObject.activeSelf)
        {
            endObject.SetActive(false);
            ExitToMenu();
            return;
        }
        //leaderboard - back to main menu
        if(leaderboardObject.activeSelf)
        {
            leaderboardObject.SetActive(false);
            ExitToMenu();
            return;
        }
        //tutorial - back to main menu
        if(tutorialObject.activeSelf)
        {
            tutorialObject.SetActive(false);
            ExitToMenu();
            return;
        }
        //options - close options (goes back to either main or pause)
        if(optionsObject.activeSelf)
        {
            optionsObject.SetActive(false);
            if(!pauseObject.activeSelf)
                ExitToMenu();
            return;
        }
        //credits - back to main menu
        if(creditsObject.activeSelf)
        {
            creditsObject.SetActive(false);
            ExitToMenu();
            return;
        }
        //game - pause menu
        if (gameLive)
        {
            //play sound
            audioManager_scr.PlayClickSFX();
            PauseGame();
            return;
        }
        //pause menu - back to game
        if (!gameLive && pauseObject.activeSelf)
        {
            //play sound
            audioManager_scr.PlayClickSFX();
            UnPauseGame();
            return;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    //Leaderboard

    //Tutorial

    //Options

    //Credits


    ///////////////////////////////////////////////////////////////////

    // Use this for initialization
    void Start ()
    {
        endScore = endObject.transform.Find("Score Text").GetChild(0).GetComponent<Text>();

        PauseGame();

        //einit options
        optionsObject.GetComponent<script_Options>().Init();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //always detect back button
        if(Input.GetKeyDown("escape"))
        {
            BackButton();
        }

        //timer to show menu after clear
        if(menuShowTimer != -100)
            menuShowTimer -= Time.deltaTime;
        if (menuShowTimer <= 0 && menuShowTimer != -100)
        {
            menuObject.SetActive(true);
            menuShowTimer = -100;
        }

        //detect overlay pause
        if (scr_leaderboards.overlayOpen && scr_leaderboards.overlayToggle)
        {
            //only if in game
            if(gameLive)
                PauseGame();
            //flip back so pause/unpause only happens once
            scr_leaderboards.overlayToggle = false;
        }
        else if(!scr_leaderboards.overlayOpen && scr_leaderboards.overlayToggle)
        {
            ////only if pause menu was opened
            //if(pauseObject.active)
            //    UnPauseGame();
            scr_leaderboards.overlayToggle = false;
        }

        //update game live
        if (gameLive)
        {
            gameTime += Time.deltaTime;

            //update values
            workRateTimer -= Time.deltaTime;
            streamRateTimer -= Time.deltaTime;
            energyRateTimer -= Time.deltaTime;

            //Progression
            UpdateDifficulty();

            //quake bars for how low they are
            UpdateCamQuake(barTransforms[0], workBarNum);
            UpdateCamQuake(barTransforms[1], streamBarNum);
            UpdateCamQuake(barTransforms[2], energyBarNum);
        }

        //update score
        scoreUI.text = "Score: " + Mathf.Round(gameTime * (gameTime / 50)).ToString();

        //WORK
        WorkUpdate();

        //STREAM
        StreamUpdate();

        //ENERGY RATE
        EnergyUpdate();
    }

    //DIFFICULTY PROGRESSION
    void UpdateDifficulty()
    {
        //update difficulty timer
        difficultyTimer -= Time.deltaTime;

        //increase difficulty
        if(difficultyTimer <= 0)
        {
            if(workRate - rateDifficultyChange > 0.1f)
                workRate -= rateDifficultyChange;
            if (streamRate - rateDifficultyChange > 0.1f)
                streamRate -= rateDifficultyChange;
            if (energyRate - rateDifficultyChange > 0.1f)
                energyRate -= rateDifficultyChange;

            //change letter spawning based on difficulty
            letterSpawning_scr.ChangeSpawnRate(streamRate);

            //reset timer
            difficultyTimer = difficultyUpdateTime;
        }

        //RATE OF CHANGE OF DIFFICULTY
        //every so often, make rate that difficulty changes at decrease
        increaseDifficultyTimer -= Time.deltaTime;
        //Debug.Log(gameTime % 10);
        if(increaseDifficultyTimer <= 0)
        {
            //remove a second off of the updating difficulty time
            if(difficultyUpdateTime > 1)
                difficultyUpdateTime--;

            //reset timer
            increaseDifficultyTimer = increaseDifficultyRate;

            //DIFFICULTY LEVEL
            if(difficultyLevel < 8)
                difficultyLevel++;

            //Increase chat excited state
            chat_scr.PingBarChanges(-1, 0.2f * difficultyLevel);

            //ramp up music
            audioManager_scr.ChangeMusicState(difficultyLevel);
        }
    }

    //WORK BAR
    void WorkUpdate()
    {
        //WORK UPDATE
        if (workRateTimer <= 0 && !workFreeze)
        {
            //decrease by 1
            ChangeWorkBar(-2);

            //reset timer
            workRateTimer = workRate;

            //check how low bar is
            if (workBarNum <= barMaxNum * 0.5f)
            {
                float severity = (float)workBarNum / (float)barMaxNum;
                chat_scr.PingBarChanges(0, 0.6f - severity);
            }
        }

    }

    void ChangeWorkBar(int valueChange)
    {
        //CANCEL IF FROZEN  
        if (workFreeze)
            return;

        //increase or decrease by certain amount
        workBarNum += valueChange;

        //cap number
        if (workBarNum > barMaxNum)
            workBarNum = barMaxNum;

        if (workBarNum <= 0)
        {
            workBarNum = 0;
            LostGame();
        }

        //update the bar
        workBar.value = workBarNum;
    }

    public void FinishedEdit(bool correct)
    {
        //correct
        if(correct)
        {
            ChangeWorkBar(editingWorkGain);
            audioManager_scr.PlayDingSFX();
        }
        else
        {
            audioManager_scr.PlayFalseDingSFX();
        }

    }

    //STREAM BAR
    void StreamUpdate()
    {
        if (streamRateTimer <= 0 && !streamFreeze)
        {
            //not live
            if (!streaming)
            {
                //decrease by 1
                ChangeStreamBar(-2);
            }

            //LIVE
            else
            {
                //make bar go down half the speed
                ChangeStreamBar(-1);
            }

            //reset timer
            streamRateTimer = streamRate;

            //check how low bar is
            if(streamBarNum <= barMaxNum * 0.5f)
            {
                float severity = (float)streamBarNum / (float)barMaxNum;
                chat_scr.PingBarChanges(1, 0.6f - severity);
            }
        }

    }

    void ChangeStreamBar(int valueChange)
    {
        //CANCEL IF FROZEN
        if (streamFreeze)
            return;

        //increase or decrease by certain amount
        streamBarNum += valueChange;

        //cap number
        if (streamBarNum > barMaxNum)
            streamBarNum = barMaxNum;

        //lost
        if (streamBarNum <= 0)
        {
            streamBarNum = 0;
            LostGame();
        }

        //update the bar
        streamBar.value = streamBarNum;
    }

    public void ToggleStreamUpdate(bool streamingOn)
    {
        //starting stream
        if(streamingOn)
        {
            streaming = true;

            //turn on letters script if it hadn't been on before
            letterSpawning_scr.enabled = true;
        }

        //stopping steam
        else
        {
            streaming = false;
        }
    }

    public void ClickedStreamEmote(bool correct)
    {
        //get points
        if (correct)
        {
            audioManager_scr.PlayLittleDingSFX();
            ChangeStreamBar(streamingEmoteGain);
        }

        //lose points
        else
        {
            audioManager_scr.PlayFalseDingSFX();
            ChangeStreamBar(streamingEmoteLoss);

        }
    }

    //ENERGY BAR
    void EnergyUpdate()
    {
        //REORDER FOOD
        if (foodReorderTimer > 0 && gameLive)
        {
            foodReorderTimer -= Time.deltaTime;

            //turn back on button
            if (foodReorderTimer <= 0)
                orderFoodButton.SetActive(true);
        }


        if (energyRateTimer <= 0 && !energyFreeze)
        {
            //not live
            if (!streaming)
            {
                //decrease by 1
                ChangeEnergyBar(-2);
            }

            //LIVE
            else
            {
                ChangeEnergyBar(energyStreamDrain);
            }

            //reset timer
            energyRateTimer = energyRate;

            //check how low bar is
            if (energyBarNum <= barMaxNum * 0.5f)
            {
                float severity = (float)energyBarNum / (float)barMaxNum;
                chat_scr.PingBarChanges(2, 0.6f - severity);
            }
        }
    }

    void ChangeEnergyBar(int valueChange)
    {
        //CANCEL IF FROZEN
        if (energyFreeze)
            return;

        //increase or decrease by certain amount
        energyBarNum += valueChange;

        //cap number
        if (energyBarNum > barMaxNum)
            energyBarNum = barMaxNum;

        //Lost
        if (energyBarNum <= 0)
        {
            energyBarNum = 0;
            LostGame();
        }

        //update the bar
        energyBar.value = energyBarNum;
    }

    //food
    public void OrderFood()
    {
        //toggle off button
        orderFoodButton.SetActive(false);

        //drop work bar
        ChangeWorkBar(-20);

        //make food appear
        foodClickButton.SetActive(true);

        //set food to full
        foodLeft = foodSize;
        foodSlider.value = 0;
    }

    public void EatFood()
    {
        //consume food
        foodLeft -= foodEatAmount;

        //increase slider for the food eaten
        foodSlider.value = foodSize - foodLeft;

        //increase energy bar
        ChangeEnergyBar(energyFoodGain);

        //test if eaten all the food
        if (foodLeft <= 0)
            DisableFood();
    }

    void DisableFood()
    {
        foodLeft = 0;

        //turn off visiblity
        foodClickButton.SetActive(false);

        //turn on timer to allow reorder
        foodReorderTimer = foodReorderMaxTime;
    }

    //quake camera
    void UpdateCamQuake(Transform objTrans, int barNum)
    {
        Vector3 currentAngle = objTrans.rotation.eulerAngles;

        //only quake if low enough
        if(barNum <= 30)
        {
            //using negatives
            if (currentAngle.z >= 180)
                currentAngle.z -= 360;

            //flip direction
            if(currentAngle.z >= maxVal)
                aimingVal = -1;

            else if(currentAngle.z <= -maxVal)
                aimingVal = 1;

            //quake faster for how low bar is
            currentAngle.z += aimingVal * (quakeRate / barNum);

        }

        //set to 0 if above
        else
        {
            currentAngle.z = 0;
        }

         objTrans.rotation = Quaternion.Euler(currentAngle.x, currentAngle.y, currentAngle.z);
    }
}
