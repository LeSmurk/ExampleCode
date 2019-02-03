using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverScript : MonoBehaviour {

    private bool killHover = false;

    private Ray downRay;
    private RaycastHit hitInfo;
    private Rigidbody rb;

    //the hoverposition centre of mass
    public Transform centreOfMass;

    //ray positions
    public Transform[] rayPositions = new Transform[6];
    //ray height from ground
    private float[] rayHeights = new float[6];

    //extra down force
    public float downForce;
    public float extraDown = 100;

    //max angle that the rays will work at
    public float maxDetectionAngle;
    public float minTiltAngle = 0.3f;
    public float minRollAngle = 0.5f;
    private bool[] hitSteep = new bool[6];

    //ray max - Loses hover
    public float rayMax;
    //ray min dist - Too close to ground
    public float rayMin;

    //how much force required to hold up the racer
    //public float rayHoldUpForce;
    //how much force is currently being applied
    public float[] rayPushForce = new float[6];

    //random floating
    //minimum force given for random
    public float hoverPushForceMin;
    public float hoverPushForceMax;

    //timers for hovers
    private float[] hoverTimers = new float[6];
    private float[] hoverTimeVariation = new float[6];
    //
    public float hoverTimeMax;
    public float hoverTimeMin;

    //max separation between the rays
    public float heightDifferenceMax;
    public float reactionPushForce;

    [Header("Upright")]
    public float uprightXTiltTorque;
    public float uprightZRollTorque;

	// Use this for initialization
	void Start ()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        //MODIFIED CENTRE OF MASS
        rb.centerOfMass = centreOfMass.localPosition;
        
        //loop through the hover points
        for(int i = 0; i < rayPositions.Length; i++)
        {
            //first random hovering time number
            hoverTimeVariation[i] = Random.Range(hoverTimeMin, hoverTimeMax);

            //store the positions in transform array
            rayPositions[i] = centreOfMass.GetChild(i);

            //init ray force at max
            rayPushForce[i] = hoverPushForceMax;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < rayPositions.Length; i++)
        {
            HoverRay(i);
        }
    }

    void FixedUpdate ()
    {
        if (killHover)
        {
            //still apply downforce
            for (int i = 0; i < rayPositions.Length; i++)
            {
                rb.AddForceAtPosition(Vector3.down * downForce, rayPositions[i].position, ForceMode.Force);
            }
            //stop anything else from hovering
            return;
        }

        //rb.AddForce(Vector3.down * 60, ForceMode.Acceleration);
        int notTouching = 0;

        for (int i = 0; i < rayPositions.Length; i++)
        {
            //Hitting something
            if(rayHeights[i] >= 0)
            {
                //create percentage of how much force to apply
                //(current - min) * (1 / max - min)
                //inverts percentage too
                float percentage = (rayHeights[i] - rayMax) * (1.0f / (rayMin - rayMax));

                //add upforce based on how close racer is to ground
                rb.AddForceAtPosition(-Vector3.down * (rayPushForce[i]) * percentage, rayPositions[i].position, ForceMode.Force);
            }

            //Everything needs pushed down
            else if(rayHeights[i] == -1)
            {
                rb.AddForceAtPosition(Vector3.down * downForce, rayPositions[i].position, ForceMode.Force);

                notTouching++;
            }
            
        }

        //if all rays aren't hitting anything, force down
        if (notTouching == rayPositions.Length)
        {
            rb.AddForce(Vector3.down * extraDown, ForceMode.Force);
        }

        //stop form flipping over
        PreventUpside();
    }

    //check the difference between the current height and all the others (returns the highest one)
    float CheckDifference(int number, float heightDifference)
    {
        int currentMax = -1;

        for(int i = 0; i < rayPositions.Length; i++)
        {
            //make sure not testing ray against itself && make sure the ray being tested isn't null
            if (i != number && rayHeights[i] != -1)
            {
                //compare the difference between the current ray height and others
                if (rayPositions[i].position.y - rayPositions[number].position.y >= heightDifference)
                {
                    //There isn't a largest already
                    if (currentMax == -1)
                        currentMax = i;

                    //make sure using the largest difference
                    else if (rayPositions[i].position.y - rayPositions[number].position.y > rayPositions[currentMax].position.y - rayPositions[number].position.y)
                        currentMax = i;

                    //Debug.Log(rayPositions[i].position.y - rayPositions[number].position.y);
                }
            }
        }

        //current max is the ray too far from the current
        if(currentMax != -1)
        {
            float pushBackForce = reactionPushForce;

            ////current - min * (1/max - min)
            //float difference = rayHeights[currentMax] - rayHeights[number];
            //float percent = difference * (1 / rayMax);
            //pushBackForce = reactionPushForce * percent;

            float percentage = (rayHeights[currentMax] - rayMax) * (1.0f / (rayMin - rayMax));

            pushBackForce = (rayPushForce[currentMax]) * percentage;
            //Debug.Log(pushBackForce);

            // return the total force
            return pushBackForce;
        }

        return -1;
    }

    //Hover rays
    void HoverRay(int number)
    {
        //setup ray
        downRay.origin = rayPositions[number].position;
        downRay.direction = Vector3.down;

        //rayHeights[number] = downRay.origin.y;

        //show MAX ray
        Debug.DrawRay(downRay.origin, downRay.direction * rayMax, Color.blue);
        //show MIN ray
        Debug.DrawRay(downRay.origin, downRay.direction * rayMin, Color.red);

        //increment hovering
        hoverTimers[number] += Time.deltaTime;

        //layer mask
        int layerMask = 1 << LayerMask.NameToLayer("Environment");
        //Debug.Log(layerMask);

        //detection
        if (Physics.Raycast(downRay.origin, downRay.direction, out hitInfo, rayMax, layerMask))
        {
            //check for other rays detecting steepness
            int checkSteeps = 0;
            for (int i = 0; i < rayPositions.Length; i++)
                if (hitSteep[i])
                    checkSteeps++;

            //determine if object hit is too steep
            Vector3 forwardAngle = Vector3.Scale(Vector3.forward, hitInfo.normal);
            Vector3 localAngle = Vector3.Scale(forwardAngle, transform.forward);
            localAngle *= -1;

            if (localAngle.z >= maxDetectionAngle)
            {
                //tell all others that this is steep
                hitSteep[number] = true;

                Debug.Log(localAngle.z);
                //set force to minimum
                //rayPushForce[number] = hoverPushForceMin;     
           
                return;
            }

            else
            {
                //set this steep to not
                hitSteep[number] = false;

                //if any other rays detect steep, keep forces at current
                if(checkSteeps != 0)
                {
                    return;
                }
            }

            //store the height that the ray is from ground
            rayHeights[number] = hitInfo.distance;

            ////test if less than min, then do not allow closer
            if (hitInfo.distance <= rayMin)
            {
                //make sure force is enough to keep it off the ground
                if (rayPushForce[number] < hoverPushForceMax)
                    rayPushForce[number] = hoverPushForceMax;
                //increment past this, if it is not enough
                else
                    rayPushForce[number] += reactionPushForce;

                //reset hovering timer
                hoverTimers[number] = 0;
            }
        }

        //not hitting anything
        else
        {
            //height from ground is too far from ground
            rayHeights[number] = -1;

            //lower the amount of up force, but not below the min
            if (rayPushForce[number] > hoverPushForceMin)
                rayPushForce[number] -= reactionPushForce;

            else
                rayPushForce[number] = hoverPushForceMin;

        }


        //prevent the heights between each ray from being too different
        float diff = CheckDifference(number, heightDifferenceMax);
        if (diff != -1)
        {
            //Debug.Log("slight difference");

            //add force to push back the ray
            rayPushForce[number] += reactionPushForce;

            //rb.AddForceAtPosition(-downRay.direction * diff, downRay.origin, ForceMode.Acceleration);
        }
    }

    void PreventUpside()
    {
        //Vector3 rotZ =-transform.up - Vector3.down;
        Vector3 rotZ = Vector3.Scale(transform.forward, Vector3.down);
        Vector3 rotX = Vector3.Scale(transform.right, Vector3.down);

        rotZ *= -1;
        Vector3 tilt = rotZ;
        tilt.x = rotZ.y;
        tilt.y = 0;
        tilt.z = rotX.y;     

        //if (tilt.x <= minTiltAngle && tilt.x >= -minTiltAngle)
        //    tilt.x = 0;

        if (tilt.z <= minRollAngle && tilt.z >= -minRollAngle)
        {
            tilt.z = 0;
        }
        //else
        //    Debug.Log("rolling bak");

        //multiply by wanted amount for each type of rotation
        tilt.x *= uprightXTiltTorque;
        tilt.z *= uprightZRollTorque;

        //Debug.Log(tilt);


        //Quaternion qrot = Quaternion.FromToRotation(transform.up, Vector3.up);
        rb.AddRelativeTorque(tilt, ForceMode.Force);
    }

    public void StopHover()
    {
        killHover = true;
    }

    public void StartHover()
    {
        killHover = false;
    }
}
