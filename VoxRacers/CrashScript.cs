using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashScript : MonoBehaviour
{
    [Header("Parts storage")]
    public int partHP = 1000;
    private int partMaxHP;
    //random amoutn of hp loss
    public int collisionHPLossMin = 1; // Inclusive
    public int collisionHPLossMax = 11; //Exclusive
    public float speedModifierEqualiser = 20;
    private Transform fallenParts;
    private Vector3 startingPos;
    private Transform startingParent;

    [Header("Effects")]
    public ParticleSystem thrusterEffect;
    public ParticleSystem reconnectEffect;
    public ParticleSystem smokeEffect;
    public ParticleSystem fireEffect;
    private float timer;

    [Header("Launching")]
    private bool detached = false;
    private float launchForce = 20;
    private Rigidbody rb;

    [Header("Racer info")]
    private GameObject player;
    private RacerScript racer_scr;

    public BoxCollider thisbox;

    // Use this for initialization
    void Start ()
    {
        //store current lcoal position as starting pos
        startingPos = transform.localPosition;
        startingParent = transform.parent;

        //get racer script
        racer_scr = transform.parent.parent.GetComponent<RacerScript>();

        //set max hp to whatever the default hp value is
        partMaxHP = partHP;

        rb = GetComponent<Rigidbody>();

        //Get racer info
        player = transform.root.GetChild(0).gameObject;

        //get fallen part transform
        fallenParts = transform.root.Find("Fallen Parts");
    }

    //set loss to greater than 0, and part will lose that much hp, if max true then down to 0hp
    public void LoseHP(int loss, float speed)
    {
        //if already detached don't bother
        if (detached)
            return;

        //lose hp given (a percentage)
        if(loss > 0)
        {
            partHP -= (loss * (partMaxHP / 100));
        }

        //lose hp based on standard loss rate
        else
        {
            //change loss amount based on speed
            if(speed > 0)
            {
                //based on velocity
                int roundedAmountMin = Mathf.RoundToInt(collisionHPLossMin * speed / speedModifierEqualiser);
                int roundedAmountMax = Mathf.RoundToInt(collisionHPLossMax * speed / speedModifierEqualiser);

                //with a slight random
                partHP -= Random.Range(roundedAmountMin, roundedAmountMax);
            }

            //just random
            else
            {
                //random amount
                partHP -= Random.Range(collisionHPLossMin, collisionHPLossMax);
            }
        }

        //check if hp is low enough, disconnect this part
        if (partHP <= 0)
            Disconnect();
    }

    public void GainHP(int gain)
    {
        //reattach part if detached
        if (detached)
            Reconnect();

        //gain percentage amount of hp
        partHP += (gain * (partMaxHP / 100));

        //prevent from going over max (couldve combined but meh)
        if (partHP > partMaxHP)
            partHP = partMaxHP;

    }

    void Disconnect()
    {
        //only if not already detached
        if(!detached)
        {
            if(smokeEffect != null)
            {
                //set smoke parent to the ship
                smokeEffect.transform.parent = transform.parent;
                //play smoke
                smokeEffect.Play();

                fireEffect.transform.parent = transform.parent;
                fireEffect.Play();
            }

            //change parent to fallen parts
            transform.parent = fallenParts;
            //turn off kinematic
            rb.isKinematic = false;
            //turn on gravity
            rb.useGravity = true;
            //turn on collider
            GetComponent<MeshCollider>().enabled = true;

            //launch?
            rb.AddForce(transform.up * launchForce, ForceMode.Impulse);

            //turn off thrusters
            if (thrusterEffect != null)
                thrusterEffect.Stop();

            detached = true;

            //toggle off collision box on racer, if there is one
            if (thisbox != null)
                thisbox.enabled = false;

            //set hp to 0
            partHP = 0;

            //tell racer this comp fell off
            racer_scr.DecreaseCrashVel();
        }
        
    }

    void Reconnect()
    {
        reconnectEffect.Play();

        transform.parent = startingParent;

        //fix in place
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity.Set(0, 0, 0);
        GetComponent<MeshCollider>().enabled = false;

        transform.localPosition = startingPos;
        transform.localRotation = Quaternion.identity;

        detached = false;

        //turn on thrusters
        if (thrusterEffect != null)
            thrusterEffect.Play();

        //toggle on collision box on racer, if there is one
        if (thisbox != null)
            thisbox.enabled = true;

        if(smokeEffect != null)
        {
            //set smoke parent to this again
            smokeEffect.transform.parent = transform.parent;
            //stop smoke
            smokeEffect.Stop();

            fireEffect.transform.parent = transform.parent;
            fireEffect.Stop();
        }

        //tell racer this comp back one
        racer_scr.IncreaseCrashVel();

    }
}

//static class Extension
//{
//    public static Transform NameContains(this Transform transf, string start)
//    {
//        Transform t = null;
//        if (transf.name.Contains(start))
//        {
//            return transf;
//        }
//        else
//        {
//            if (transf.childCount <= 0) return null;

//            foreach (Transform child in transf)
//            {
//                t = NameContains(child, start);
//            }
//        }
//        return t;
//    }
//}
