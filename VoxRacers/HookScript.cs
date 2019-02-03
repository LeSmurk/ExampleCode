using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookScript : MonoBehaviour
{
    [Header("Hook object")]
    public GameObject Head;
    public GameObject Base;
    public float forwardForce;

    public float maxHookTime = 5;
    private float hookOutTimer;
    public float unhookDistance;
    private float destroyTimer;

    [Header("Line / particles")]
    public LineRenderer line;
    //body segments go from 1 - max (0 is Base and max + 1 is Head)
    private GameObject[] bodySegments = new GameObject[20];
    public Material[] Mats = new Material[8];


    // Use this for initialization
    void Start ()
    {
        //create timer for coming back to base
        destroyTimer = 100;

        //create timer for head max out time
        hookOutTimer = 100;

        FireHead();

        //INIT ROPE
        line.useWorldSpace = true;

        //init Base and Head points
        line.SetPosition(0, Base.transform.position);
        line.SetPosition(bodySegments.Length, Base.transform.position);

        // Setup rope line
        for (int i = 1; i < bodySegments.Length; i++)
        {
            //init line to all at body position
            line.SetPosition(i, Base.transform.position);

            //spawn segments
            bodySegments[i] = Instantiate(Resources.Load("Prefabs/Pickups/Hook Body"), Base.transform.position, Quaternion.identity) as GameObject;

            //store segments as a child of hook
            bodySegments[i].transform.parent = transform;

            //set segment colour to whatever the hypervox centre one is
            if (transform.parent != null)
            {
                string name = transform.root.name;
                bodySegments[i].GetComponent<ParticleSystemRenderer>().material = Mats[int.Parse(name[6].ToString())]; //Mats[int.Parse(name[6].ToString())];
            }
        }
    }

    void Update()
    {
        //TIDY these timers
        if (destroyTimer != 100)
            destroyTimer -= Time.deltaTime;

        if (hookOutTimer != 100)
            hookOutTimer -= Time.deltaTime;

        float dist = 100;
        bool behind = false;

        //Update base line point
        line.SetPosition(0, Base.transform.position);

        //detecting direction of head
        if(Head != null)
        {
            //set distance that head is from base
            dist = Vector3.Distance(Base.transform.position, Head.transform.position);

            //gets difference between the base and the Head then creates a normalized float to determine which is infront
            Vector3 difference = Head.transform.position - Base.transform.position;
            // -1 head is perfectly behind the base and 1 is perf infront
            //float direction = Vector3.Scale(difference, Base.transform.position).normalized.z;
            float direction = Vector3.Scale(difference, Base.transform.forward).normalized.z;

            if (direction <= 0)
                behind = true; //Debug.Log("behind");

            //position the end point of the line
            line.SetPosition(bodySegments.Length, Head.transform.position);

            //loop update all the block positions (ignore first and last, as head)
            for (int i = 1; i < bodySegments.Length; i++)
            {
                //point along the line towards the head, at regular intervals
                line.SetPosition(i, (Base.transform.position + (i * difference / bodySegments.Length)));
                bodySegments[i].transform.position = line.GetPosition(i);
            }
        }

        //head came back or has been out too long
        if ((dist <= unhookDistance && destroyTimer <= 0) || behind == true || hookOutTimer <= 0)
        {
            Destroy(gameObject);

            //destroy head
            Destroy(Head);

            //Reset all positions to base
            for(int i = 0; i < bodySegments.Length; i++)
            {
                //reset line back to base
                line.SetPosition(i, Base.transform.position);

                //destroy the blocks
                Destroy(bodySegments[i]);
            }

            //reset timer
            destroyTimer = 100;

            //give player boost if the head connnected
            if (Head.GetComponent<BulletScript>().grabbedItem != null)
                gameObject.GetComponentInParent<RacerScript>().Boost();
        }
    }

    void FireHead()
    {
        Rigidbody rb = Head.GetComponent<Rigidbody>();
        //fire head forwards
        rb.AddForce(Base.transform.forward * forwardForce, ForceMode.Impulse);

        //start timer for head to come back to base
        destroyTimer = 1;

        //start timer for how long head has been out
        hookOutTimer = maxHookTime;
    }
}
