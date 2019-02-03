using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    //Racer thrust and steering
    public float Transmission = 2;
    public Vector3 Steering;
    // Whether the racer can keep going forwards
    private bool blockedForwards = false;

    [Header("Crash Info")]
    public float crashVelocity;
    public int hpLoss;

    [Header("Parts")]
    public CrashScript[] racerPartsRight = new CrashScript[2];
    public CrashScript[] racerPartsLeft = new CrashScript[2];
    public CrashScript[] racerPartsCentre = new CrashScript[1];

    public BoxCollider boxColRight;
    public BoxCollider boxColLeft;


    //Scripts
    private GameObject racer;
    private RacerScript racer_scr;
    private InfoRacerScript infoRacer_scr;
    private GameManagerScript gameManager_scr;

    // Use this for initialization
    void Start ()
    {
        Transmission = GetComponent<Racer>().Thrust;
        Steering = GetComponent<Racer>().Steering;

        //get game manager script
        gameManager_scr = GameObject.Find("GameManager").gameObject.GetComponent<GameManagerScript>();

        //get racer script
        racer_scr = gameObject.GetComponent<RacerScript>();

        //get info racer script
        infoRacer_scr = gameObject.GetComponent<InfoRacerScript>();

        //store engine transform info
        //forwardEngine = transform.Find("Model/Engines");
    }
	
	// Update is called once per frame
	void Update ()
    {          
        //finding local forwards (NOT BEST METHOD)
        //Vector3 localForward = transform.position + forwardEngine.transform.position;
        //Debug.DrawLine(transform.position, forwardEngine.transform.position, Color.magenta);
        //Debug.DrawLine(transform.position, Vector3.down, Color.cyan);

        //ALSO NEEDS TIDIED
        Transmission = GetComponent<Racer>().Thrust;
        Steering = GetComponent<Racer>().Steering;

        //Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        //Debug.DrawLine(transform.position, transform.position + (localForward), Color.magenta);
    }

    void OnCollisionEnter(Collision col)
    {
        foreach (ContactPoint contact in col.contacts)
        {
            //FRONT ON COLLISION
            if (contact.thisCollider.GetType() == typeof(CapsuleCollider))
            {
                if (gameManager_scr.GameMode == Mode.Arcade)
                {
                    ////loses full hp
                    //infoRacer_scr.ReduceHP("cockpit", infoRacer_scr.maxHPAmounts);
                    //infoRacer_scr.ReduceHP("engineRight", infoRacer_scr.maxHPAmounts);
                    //infoRacer_scr.ReduceHP("engineLeft", infoRacer_scr.maxHPAmounts);

                    //unsure if should set dead here too?
                    infoRacer_scr.Dead = true;
                }
            }
        }
    }


    void OnCollisionStay(Collision col)
    {

        foreach (ContactPoint contact in col.contacts)
        {
            //FRONT ON COLLISION
            if (contact.thisCollider.GetType() == typeof(CapsuleCollider))
            {
                //detect if environment
                if (col.collider.CompareTag("Environment"))
                {
                    //stop forwards thrusting
                    blockedForwards = true;

                    //tell racer to stop
                    racer_scr.KillPower(false);

                    //determine if crashed
                    if (GetComponent<Rigidbody>().velocity.magnitude > crashVelocity || gameManager_scr.GameMode == Mode.Arcade)
                    {
                        //REMOVE HP

                        //unsure if should set dead here too?
                        infoRacer_scr.Dead = true;
                    }
                }
            }

            //not front collider, allow to move
            else
            {
                blockedForwards = false;
                racer_scr.FullPower();
            }


            //hitting right collider
            if (contact.thisCollider == boxColRight)
            {
                //remove random hp from right parts
                for (int i = 0; i < racerPartsRight.Length; i++)
                    racerPartsRight[i].LoseHP(0, 0);

                //remove random hp from front part
                for (int i = 0; i < racerPartsCentre.Length; i++)
                    racerPartsCentre[i].LoseHP(0, 0);
            }

            //hitting left collider
            if (contact.thisCollider == boxColLeft)
            {
                //remove random hp from right parts
                for (int i = 0; i < racerPartsLeft.Length; i++)
                    racerPartsLeft[i].LoseHP(0, 0);

                //remove random hp from front part
                for (int i = 0; i < racerPartsCentre.Length; i++)
                    racerPartsCentre[i].LoseHP(0, 0);
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        blockedForwards = false;

        //Ping racer script to go back to max power
        racer_scr.FullPower();
    }

    private bool MyApproximation(float a, float b, float tolerance)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }
}
