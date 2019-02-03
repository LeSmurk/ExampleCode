using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    //TYPES FOR REFERENCE

    //OFFENSIVE
    //bullet

    //DEFENSIVE
    //shield

    //UTILITY
    //hook

    [Header("Functionality")]
    public int numberOfPickupsInEachSection = 3;
    private bool active = true;
    private string type;
    private string pickupName;
    private BoxCollider boxCollider;
    private float timer;
    public float reactivateTime = 5;
    private bool reactivate = false;


    [Header("Movement")]
    public float verticalBob;

    [Header("Appearance")]
    public Material redCentreMat;
    public Material redOutlineMat;
    public Material blueCentreMat;
    public Material blueOutlineMat;
    public Material greenCentreMat;
    public Material greenOutlineMat;
    public Material yellowCentreMat;
    public Material yellowOutlineMat;
    private ParticleSystem outlineParticles;
    private ParticleSystem centreParticles;


	// Use this for initialization
	void Start ()
    {
        //find particle systems
        outlineParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
        centreParticles = transform.GetChild(1).GetComponent<ParticleSystem>();

        //find box collider
        boxCollider = GetComponent<BoxCollider>();

        //Init
        Activate();

    }
	
	// Update is called once per frame
	void Update ()
    {
		//bob up and down ?

        //after a period of time of being deactive, reactivate
        if(!active)
        {
            timer -= Time.deltaTime;

            //activate after certain time
            if (timer <= 0)
                Activate();
        }
	}

    //detect collision with player
    void OnTriggerEnter(Collider other)
    {
        //player collided
        if(other.CompareTag("racer"))
        {
            //give player pickup
            string lastPickup = GivePickup(other.gameObject);

            //whether the player does or doesn't have a pickup, deactivate for now
            Deactivate();

            //determine if they had a pickup already
            if (lastPickup != "null")
            {
                //set this cube to last pickup's type
                type = GetPickupType(lastPickup);

                //activate but only after the current player is out the way (seconds)
                reactivate = true;
                timer = 1;
            }
        }
    }

    void Activate()
    {
        //set to active
        active = true;

        //should use last stored type or get a random one
        bool random = true;

        //if this is being reactivated, use last stored type
        if (reactivate)
            random = false;

        //toggle off reactivate
        reactivate = false;

        //if randomly selecting one
        if (random)
        {
            //randomly pick a type
            switch (Random.Range(0, 4))
            {
                //case(0):
                //    type = "Offensive";
                //    break;
                //case (1):
                //    type = "Defensive";
                //    break;
                case (0):
                    type = "Utility";
                    break;
                case (1):
                    type = "Utility";
                    break;
                case (2):
                    type = "Utility";
                    break;
                case (3):
                    type = "Health";
                    break;
            }
        }

        //set colour based on type
        switch (type)
        {
            case ("Offensive"):
                centreParticles.GetComponent<ParticleSystemRenderer>().material = redCentreMat;
                outlineParticles.GetComponent<ParticleSystemRenderer>().material = redOutlineMat;
                break;

            case ("Defensive"):
                centreParticles.GetComponent<ParticleSystemRenderer>().material = blueCentreMat;
                outlineParticles.GetComponent<ParticleSystemRenderer>().material = blueOutlineMat;
                break;

            case ("Utility"):
                centreParticles.GetComponent<ParticleSystemRenderer>().material = yellowCentreMat;
                outlineParticles.GetComponent<ParticleSystemRenderer>().material = yellowOutlineMat;
                break;

            case ("Health"):
                centreParticles.GetComponent<ParticleSystemRenderer>().material = greenCentreMat;
                outlineParticles.GetComponent<ParticleSystemRenderer>().material = greenOutlineMat;
                break;

            default:
                break;
        }

        //show in scene
        outlineParticles.gameObject.SetActive(true);
        centreParticles.gameObject.SetActive(true);

        //turn on collider
        boxCollider.enabled = true;
    }

    void Deactivate()
    {
        active = false;

        //turn off visible in scene
        outlineParticles.gameObject.SetActive(false);
        centreParticles.gameObject.SetActive(false);

        //turn off collider
        boxCollider.enabled = false;

        //start timer for being deactive
        timer = reactivateTime;
    }

    string GivePickup(GameObject player)
    {
        string lastPickup = "null";
        RacerScript infoRacer_scr = player.GetComponent<RacerScript>();

        //test if player already has a pickup
        lastPickup = infoRacer_scr.currentPickup;

        //figure out what position the player is in(HIGHER NUMBERS SHOULD BE STRONGER PICKUPS)

        //get "random" one, within range
        string newPickup = "null";
        int pickup = 0;

        //randomly pick ones within the type
        //pickup = Random.Range(0, numberOfPickupsInEachSection);

        //separate between pickup types (COULD ALWAYS BE TIDIER?)
        switch (type)
        {
            case("Offensive"):
                //if (pickup == 0 || pickup == 1)
                //    newPickup = "bullet";

                //if (pickup == 2)
                //    newPickup = "impact";
                newPickup = "bullet";
                break;

            case ("Defensive"):
                //if (pickup == 0 || pickup == 1)
                //    newPickup = "shield";

                //if (pickup == 2)
                //    newPickup = "bubble";
                newPickup = "shield";
                break;

            case ("Utility"):
                //if (pickup == 0 || pickup == 1)
                //    newPickup = "boost";
                ////if (pickup == 1)
                ////    newPickup = "glitch";
                //if (pickup == 2)
                //    newPickup = "hook";
                newPickup = "boost";
                break;

            case ("Health"):
                //if (pickup == 0 || pickup == 1 || pickup == 2)
                    newPickup = "heal";
                break;

            default:
                break;
        }

        //store new one in
        infoRacer_scr.SetPickup(newPickup);

        //return the last one the player had
        return lastPickup;
    }

    string GetPickupType(string pickup)
    {
        //offensive ones
        if (pickup == "bullet" || pickup == "impact")
            return "Offensive";

        //defensive ones
        else if (pickup == "shield" || pickup == "bubble")
            return "Defensive";

        //utility ones
        else if (pickup == "hook" || pickup == "glitch" || pickup == "boost")
            return "Utility";

        //health ones
        else if (pickup == "heal")
            return "Health";

        //edge case
        return "null";
    }
}
