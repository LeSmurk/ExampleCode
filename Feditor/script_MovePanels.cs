using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class script_MovePanels : MonoBehaviour {

    [Header("Game Manager")]
    public script_GameManager scr_GameManager;

    [Header("Panels")]
    private Vector3[] panelStartPositions = new Vector3[5];
    public GameObject[] panelsStored = new GameObject[5];
    private Image[] panelsImage = new Image[5];
    public int[] panelInsideEditorID = new int[] { -1, -1, -1, -1, -1 };
    public Canvas parentCanvas;

    [Header("Mouse Movement")]
    public int panelSelectedNum = -1;
    public Vector3 offset;
    private Vector3 movePos;

    //raycast
    private RaycastHit hit;
    private Ray ray;

    [Header("Editor Drop")]
    public GameObject[] editors;
    private script_Editor[] scr_editors;

    [Header("Formulae")]
    public int[] formula = new int[] { 0, 1, 2, 3, 4, 5 };
    private int[] panelImageOrder = new int[] { 0, 1, 2, 3, 4 };
    public Sprite[] baseImages;
    //What the formula number of the editor a panel is in
    public int[] panelInsideEditorNum = new int[] {-1, -1, -1, -1, -1};

    [Header("Particles")]
    public ParticleSystem confirmParticles;
    public Gradient correctColour;
    public Gradient wrongColour;

    // Use this for initialization
    void Start ()
    {
        //set editor script length
        scr_editors = new script_Editor[editors.Length];

        //find editor script
        for(int i = 0; i < scr_editors.Length; i++)
            scr_editors[i] = editors[i].GetComponent<script_Editor>();

        //store all start positions
        for(int i = 0; i < panelsStored.Length; i++)
        {
            //set to starting position
            panelStartPositions[i] = panelsStored[i].transform.position;

            //get all panel image componenets
            panelsImage[i] = panelsStored[i].GetComponent<Image>();
        }

        GenerateRandomFormula();

    }
	
	// Update is called once per frame
	void Update ()
    {
        //ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if(Physics.Raycast(ray, out hit))
        //{
        //    movePos = hit.point;
        //    //fix distance away
        //    movePos.z = 0;

        //    //move panel to pos
        //    if (panelSelectedNum != -1)
        //        panelsStored[panelSelectedNum].transform.position = movePos;// (Input.mousePosition / 30) - offset;
        //}

        Vector2 newPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform,Input.mousePosition, parentCanvas.worldCamera, out newPos);

        movePos = new Vector3(newPos.x, newPos.y, 0);
        if(panelSelectedNum != -1)
            panelsStored[panelSelectedNum].transform.position = parentCanvas.transform.TransformPoint(movePos);
    }

    //FORMULAE
    public void SetToEditor(int editorID, int editorFormNum)
    {
        if(panelSelectedNum != -1)
        {
            panelInsideEditorID[panelSelectedNum] = editorID;
            panelInsideEditorNum[panelSelectedNum] = editorFormNum;
        }
    }

    public void GenerateRandomFormula()
    {
        //random new formula
        for (int i = 0; i < formula.Length; i++)
        {
            //swap two random ids
            int temp = formula[i];
            int newPos = Random.Range(0, formula.Length);

            formula[i] = formula[newPos];
            formula[newPos] = temp;
        }

        //set panel images to selected formula
        for (int i = 0; i < panelImageOrder.Length; i++)
            panelImageOrder[i] = formula[i];

        //panel images random order (so different from formula order)
        for(int i = 0; i < panelImageOrder.Length; i++)
        {
            //swap two random ids
            int temp = panelImageOrder[i];
            int newPos = Random.Range(0, panelImageOrder.Length);

            panelImageOrder[i] = panelImageOrder[newPos];
            panelImageOrder[newPos] = temp;
        }

        SetFormula();
    }

    //given in from a random generated one
    private void SetFormula()
    {
        for (int i = 0; i < editors.Length; i++)
        {
            //set editor to new id
            // scr_editors[formula[i]].ChangeNum(i);
            scr_editors[i].ChangeNum(formula[i]);

            //set panels at top to required images (shuold be different order than formula)
            panelsImage[i].sprite = baseImages[panelImageOrder[i]];
        }
    }

    //MOVING PANELS
    public void SelectPanel(int panel)
    {
        //check nothing in the way first
        //raycast from camera to streaming button
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Letter"))
            {
                panelSelectedNum = -1;
                return;
            }
        }

        panelSelectedNum = panel;
    }

    public void ReleasePanel(int panelNum)
    {
        //edge case of -1 getting through
        if (panelSelectedNum < 0)
            return;

        //check if inside an id
        if(panelInsideEditorID[panelNum] != -1)
        {
            int prevStoredPanel = -1;

            //check if any panel is already stored
            for(int i = 0; i < panelsStored.Length; i++)
            {
                //compare editor id stored of loop and currently held panel, as well as ignoring itself
                if (panelInsideEditorID[i] == panelInsideEditorID[panelSelectedNum] && panelSelectedNum != i)
                    prevStoredPanel = i;
            }

            //if one is already stored, bump it out the way
            if (prevStoredPanel != -1)
            {
                //Debug.Log(prevStoredPanel);
                panelsStored[prevStoredPanel].transform.position = panelStartPositions[prevStoredPanel];
                panelInsideEditorID[prevStoredPanel] = -1;
                panelInsideEditorNum[prevStoredPanel] = -1;
            }

            //set panels position
            panelsStored[panelNum].transform.position = editors[panelInsideEditorID[panelNum]].transform.position;

        }

        //incorrect, snap back
        else
        {
            panelsStored[panelSelectedNum].transform.position = panelStartPositions[panelSelectedNum];
        }

        //remove moving
        panelSelectedNum = -1;

    }

    //RESETS
    //check correct
    public void ConfirmEdit()
    {
        bool allCorrect = true;

        //check panels are in the correct positions (only checking 5 panels)
        for(int i = 0; i < 5; i++)
        {
            //compare num of panel at editor to formula
            if (panelInsideEditorNum[i] != panelImageOrder[i])
                allCorrect = false;
        }

        //reset
        if(allCorrect)
        {
            //gain back stats
            scr_GameManager.FinishedEdit(true);

            //Play particles
            ParticleSystem.MainModule newMain = confirmParticles.main;
            ParticleSystem.MinMaxGradient temp = new ParticleSystem.MinMaxGradient(correctColour);
            temp.mode = ParticleSystemGradientMode.RandomColor;
            newMain.startColor = temp;
            confirmParticles.Play();

            //reset
            ResetPanels();

            //generate new formula
            GenerateRandomFormula();
        }

        else
        {
            scr_GameManager.FinishedEdit(false);

            //play particles
            ParticleSystem.MainModule newMain = confirmParticles.main;
            ParticleSystem.MinMaxGradient temp = new ParticleSystem.MinMaxGradient(wrongColour);
            temp.mode = ParticleSystemGradientMode.RandomColor;
            newMain.startColor = temp;
            confirmParticles.Play();
        }

    }

    //reset all positions
    public void ResetPanels()
    {
        for (int i = 0; i < panelsStored.Length; i++)
        {
            panelsStored[i].transform.position = panelStartPositions[i];
            panelInsideEditorID[i] = -1;
            panelInsideEditorNum[i] = -1;
        }
    }

}
