using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script_Spawning : MonoBehaviour {

    public bool spawnFreeze;

    //base spawning time
    public float letterTimeMax;
    //fastest letters are allowed to spawn
    public float letterTimeMin;
    //current rate that letters spawn at
    public float letterTime;
    //change in rate
    public float letterSpawnRateChange;
    private float spawnTimer;
    public GameObject letterPrefab;

    public Transform spawnPos;
    public float sideRandomRange;

    private List<GameObject> lettersStored = new List<GameObject>();
    private Rigidbody letterSelected = null;

    public Canvas parentCanvas;

    private Vector3 movePos;
    private Vector3 mouseVel;
    public float throwForce;
    public float holdDist;

    //raycasting
    private RaycastHit hit;
    private Ray ray;

    // Use this for initialization
    void Start ()
    {
        mouseVel = new Vector3(0, 0, 0);
        letterTime = letterTimeMax;
        spawnTimer = letterTime;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!spawnFreeze)
        {
            spawnTimer -= Time.deltaTime;

            //every few seconds, spawn a tos letter
            if (spawnTimer <= 0)
                SpawnLetter();
        }

        //letter selected
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            ////create direction vector that mouse is travelling in
            //mouseVel = hit.point - movePos;
            //mouseVel.z = 0;

            ////determine mouse position in world
            //movePos = hit.point;
            ////fixed distance away
            //movePos.z = holdDist;

            //detect mouse button down
            if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.CompareTag("Letter") && letterSelected == null)
            {
                //store as selected
                letterSelected = hit.collider.gameObject.GetComponent<Rigidbody>();
                //remove gravity
                letterSelected.useGravity = false;
                letterSelected.velocity = Vector3.zero;

                //set holding distance to whatever it is when grabbed
                holdDist = hit.collider.transform.position.z;
            }

            ////move letter to position
            //if (letterSelected)
            //{
            //    letterSelected.MovePosition(movePos);
            //}
        }

        //Move letter to mouse pos
        Vector2 newPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, Input.mousePosition, parentCanvas.worldCamera, out newPos);

        //create direction vector that mouse is travelling in
        mouseVel = new Vector3(newPos.x, newPos.y, 0) - movePos;
        mouseVel.z = 0;

        movePos = new Vector3(newPos.x, newPos.y, holdDist);
        Vector3 relPos = parentCanvas.transform.TransformPoint(movePos);
        relPos.z = holdDist;
        if (letterSelected)
        {
            //letterSelected.MovePosition(relPos);
            Vector3 force = relPos - letterSelected.transform.position;//Vector3.Lerp(Vector3.zero, relPos - letterSelected.transform.position, 0.7f);
            letterSelected.AddForce(force * 1500, ForceMode.Impulse);
        }

        //released mouse button
        if (Input.GetMouseButtonUp(0) && letterSelected)
        {
            //turn back on gravity
            letterSelected.useGravity = true;
            //add force to held item
            letterSelected.AddForce(mouseVel * throwForce, ForceMode.Impulse);
            //remove as selected
            letterSelected = null;
        }
    }

    void SelectLetter()
    {

    }

    void SpawnLetter()
    {
        //randomise position and rotation
        Vector3 spawnRand = spawnPos.position + new Vector3(Random.Range(-sideRandomRange, sideRandomRange), 0, 0);
        Vector3 spawnRot = new Vector3(Random.Range(-45, 45), Random.Range(-20, 20), Random.Range(-20, 20));

        GameObject newLetter = Instantiate(letterPrefab, spawnRand, Quaternion.Euler(spawnRot));
        lettersStored.Add(newLetter);

        spawnTimer = letterTime;
    }

    public void ChangeSpawnRate(float changeRate)
    {
        if(changeRate != -1)
        {
            //don't allow under min
            if(letterTime - letterSpawnRateChange >= letterTimeMin)
            {
                //change spawn rate based on difficulty
                letterTime -= letterSpawnRateChange;
            }
        }
        //reset
        else
        {
            letterTime = letterTimeMax;
        }
    }

    public void DestroyLetters()
    {
        for(int i = 0; i < lettersStored.Count; i++)
        {
            //check not null
            if (lettersStored[i])
                Destroy(lettersStored[i]);
        }

        lettersStored.Clear();

        //reset values
        mouseVel = new Vector3(0, 0, 0);
        letterTime = letterTimeMax;
        spawnTimer = letterTime;
    }
}
