using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class script_Leaderboards : MonoBehaviour {

    public GameObject leaderboardObj;
    private int viewingType = 0;

    private bool foundLeaderboard = false;
    private int recentScore = -1;

    [Header("Text objects")]
    public List<Text> namesText;
    public List<Text> scoresText;
    public List<Text> ranksText;
    public Button friendsButton;
    public Button globalCloseButton;
    public Button globalButton;
    public Text thisName;
    public Text thisScore;
    public Text thisRank;

    [Header("SteamWorks")]
    public bool overlayOpen;
    public bool overlayToggle;
    public float refreshSteamTime;
    private float refreshSteamTimer;
    public string leaderboardName = "Scores";
    private static SteamLeaderboard_t leaderboard_t;
    private CallResult<LeaderboardFindResult_t> findResultLeaderboard;
    private CallResult<LeaderboardScoreUploaded_t> uploadResultLeaderboard;
    private CallResult<LeaderboardScoresDownloaded_t> downloadResultLeaderboard;
    private CallResult<LeaderboardScoresDownloaded_t> downloadResultPersonal;
    private Callback<GameOverlayActivated_t> overlayCall;

    private struct LeaderboardElement
    {
        public string rank;
        public string name;
        public string score;
    }

    private int wantedType = 0;
    private List<LeaderboardElement> leaderboardElementsFriends = new List<LeaderboardElement>();
    private List<LeaderboardElement> leaderboardElementsGlobalClose = new List<LeaderboardElement>();
    private List<LeaderboardElement> leaderboardElementsGlobal = new List<LeaderboardElement>();

    bool flip = false;

    // Use this for initialization
    void Start ()
    {
        SteamAPI.Init();

        //create overlay call
        overlayCall = Callback<GameOverlayActivated_t>.Create(OverlayActivate);

        //new call to find the leaderboard
        SteamAPICall_t initLeaderboard = SteamUserStats.FindLeaderboard(leaderboardName);
        findResultLeaderboard = CallResult<LeaderboardFindResult_t>.Create(FindResult);
        findResultLeaderboard.Set(initLeaderboard);

        //create upload inst
        uploadResultLeaderboard = CallResult<LeaderboardScoreUploaded_t>.Create(UploadResult);

        //create download inst
        downloadResultLeaderboard = CallResult<LeaderboardScoresDownloaded_t>.Create(DownloadResult);
        downloadResultPersonal = CallResult<LeaderboardScoresDownloaded_t>.Create(DownloadPersonalResult);
	}

    public void ClearSteam()
    {
        SteamAPI.Shutdown();
    }

    // Update is called once per frame
    void Update ()
    {
        refreshSteamTimer -= Time.deltaTime;

        if(refreshSteamTimer <= 0)
        {
            SteamAPI.RunCallbacks();
            refreshSteamTimer = refreshSteamTime;

            //if its active, ping regularly to get scores
            if (leaderboardObj.activeSelf)
                GetAllScores();
        }
    }

    void OverlayActivate(GameOverlayActivated_t overlayAct)
    {
        if(overlayAct.m_bActive != 0)
        {
            //activated
            overlayOpen = true;
        }
        else
        {
            //deactivated
            overlayOpen = false;
        }

        //overlay was toggled
        overlayToggle = true;
    }

    public void ChangeLeaderboard(int newScore)
    {
        //store for a retry?
        recentScore = newScore;

        if(foundLeaderboard)
        {
            //create new call instance
            SteamAPICall_t newApiCall = SteamUserStats.UploadLeaderboardScore(leaderboard_t, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, newScore, null, 0);
            //upload call
            uploadResultLeaderboard.Set(newApiCall);
        }
    }

    public void GetAllScores()
    {
        //force update based on one selected
        switch (viewingType)
        {
            case(0):
                ViewFriendsBoard();
                break;

            case (1):
                ViewGlobalCloseBoard();
                break;

            case (2):
                ViewGlobalBoard();
                break;
        }

        //also get personal score
        GetThisScore();
    }

    void GetScores(ELeaderboardDataRequest requestType)
    {
        //set to which type we want
        switch (requestType)
        {
            case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal:
                wantedType = 2;
                break;
            case ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser:
                wantedType = 1;
                break;
            case ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends:
                wantedType = 0;
                break;
            default:
                break;
        }
        if(wantedType == 2 || wantedType == 0)
        {
            SteamAPICall_t newApiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard_t, requestType, 1, 10);
            downloadResultLeaderboard.Set(newApiCall);
        }
        else
        {
            SteamAPICall_t newApiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard_t, requestType, -4, 5);
            downloadResultLeaderboard.Set(newApiCall);
        }
    }

    void GetThisScore()
    {
        SteamAPICall_t newApiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard_t, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, 0, 1);
        downloadResultPersonal.Set(newApiCall);
    }
    
    void FindResult(LeaderboardFindResult_t findResult, bool failure)
    {
        if (failure)
            foundLeaderboard = false;

        else
        {
            Debug.Log("STEAM LEADERBOARDS: Found - " + findResult.m_bLeaderboardFound + " leaderboardID - " + findResult.m_hSteamLeaderboard.m_SteamLeaderboard);
            leaderboard_t = findResult.m_hSteamLeaderboard;
            foundLeaderboard = true;

            //try resending score
            if (recentScore != -1)
                ChangeLeaderboard(recentScore);

        }
    }

    void UploadResult(LeaderboardScoreUploaded_t uploadResult, bool failure)
    {
        Debug.Log("STEAM LEADERBOARDS: isfailure = " + failure + ") isCompleted = " + uploadResult.m_bSuccess + ") GlobalNew: " + uploadResult.m_nGlobalRankNew + ") UploScore " + uploadResult.m_nScore + ") HasChanged - " + uploadResult.m_bScoreChanged);
    }
    void DownloadResult(LeaderboardScoresDownloaded_t downResult, bool failure)
    {
        //Debug.Log("STEAM LEADERBOARDS: isfailure = " + failure + ") Num Entries = " + downResult.m_cEntryCount);

        List<LeaderboardElement> newList = new List<LeaderboardElement>();

        LeaderboardEntry_t returnEntry;
        int[] details = new int[1];
        int maxDetailElems = 1;

        //getting full leaderboard
        //downResult.m_cEntryCount
        for (int i = 0; i < 10; i++)
        {
            if(i < downResult.m_cEntryCount)
            {
                SteamUserStats.GetDownloadedLeaderboardEntry(downResult.m_hSteamLeaderboardEntries, i, out returnEntry, details, maxDetailElems);
                LeaderboardElement newElem;
                newElem.rank = returnEntry.m_nGlobalRank.ToString();
                newElem.name = SteamFriends.GetFriendPersonaName(returnEntry.m_steamIDUser);
                newElem.score =  returnEntry.m_nScore.ToString();
                newList.Add(newElem);
            }
            else
            {
                LeaderboardElement newElm;
                newElm.rank = "NULL";
                newElm.name = "NULL";
                newElm.score = "NULL";

                newList.Add(newElm);
            }
        }

        //store in the list we want
        switch (wantedType)
        {
            case(0):
                leaderboardElementsFriends = new List<LeaderboardElement>(newList);
                break;
            case (1):
                leaderboardElementsGlobalClose = new List<LeaderboardElement>(newList);
                break;
            case (2):
                leaderboardElementsGlobal = new List<LeaderboardElement>(newList);
                break;
        }

        //Debug.Log("Updated " + wantedType + " leaderboard");
    }

    void DownloadPersonalResult(LeaderboardScoresDownloaded_t downResult, bool failure)
    {
        LeaderboardEntry_t returnEntry;
        int[] details = new int[1];
        int maxDetailElems = 1;

        //getting own score/rank/name
        SteamUserStats.GetDownloadedLeaderboardEntry(downResult.m_hSteamLeaderboardEntries, 0, out returnEntry, details, maxDetailElems);
        thisRank.text = returnEntry.m_nGlobalRank.ToString();
        thisName.text = "YOU";
        thisScore.text = returnEntry.m_nScore.ToString();

        //Debug.Log("Updated personal leaderboard");
    }

    public void ViewFriendsBoard()
    {
        GetScores(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);
        viewingType = 0;

        //unhighlighted others
        globalCloseButton.image.color = globalCloseButton.colors.pressedColor;
        globalButton.image.color = globalButton.colors.pressedColor;

        //highlight friends
        friendsButton.image.color = friendsButton.colors.highlightedColor;
        //update text

        for (int i = 0; i < leaderboardElementsFriends.Count; i++)
        {
            ranksText[i].text = leaderboardElementsFriends[i].rank;
            namesText[i].text = leaderboardElementsFriends[i].name;
            scoresText[i].text = leaderboardElementsFriends[i].score;
        }       
    }

    public void ViewGlobalCloseBoard()
    {
        GetScores(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser);
        viewingType = 1;

        //unhighlighted others
        friendsButton.image.color = friendsButton.colors.pressedColor;
        globalButton.image.color = globalButton.colors.pressedColor;

        //highlight global close
        globalCloseButton.image.color = globalCloseButton.colors.highlightedColor;

        //update text
        //get top 10 players
        for (int i = 0; i < leaderboardElementsGlobalClose.Count; i++)
        {
            ranksText[i].text = leaderboardElementsGlobalClose[i].rank;
            namesText[i].text = leaderboardElementsGlobalClose[i].name;
            scoresText[i].text = leaderboardElementsGlobalClose[i].score;
        }
    }

    public void ViewGlobalBoard()
    {
        GetScores(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal);
        viewingType = 2;

        //unhighlighted others
        globalCloseButton.image.color = globalCloseButton.colors.pressedColor;
        friendsButton.image.color = friendsButton.colors.pressedColor;

        //highlight friends
        globalButton.image.color = globalButton.colors.highlightedColor;

        //update text
        //get top 10 players
        for (int i = 0; i < leaderboardElementsGlobal.Count; i++)
        {
            ranksText[i].text = leaderboardElementsGlobal[i].rank;
            namesText[i].text = leaderboardElementsGlobal[i].name;
            scoresText[i].text = leaderboardElementsGlobal[i].score;
        }
    }
}
