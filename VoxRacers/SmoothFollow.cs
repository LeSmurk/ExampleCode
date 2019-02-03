// Smooth Follow from Standard Assets
// Converted to C# because I fucking hate UnityScript and it's inexistant C# interoperability
// If you have C# code and you want to edit SmoothFollow's vars ingame, use this instead.
using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{
    private Rigidbody rb;

    // The target we are following
    public Transform target;

    // The distance in the x-z plane to the target
    public float distance;
    // the height we want the camera to be above the target
    public float height;

    // Larger force, faster moves
    public float distanceXForce;
    public float distanceYForce;
    public float distanceZForce;
    public float rotationForce;

    //camera
    private bool pauseFollow = false;


    // Place the script in the Camera-Control group in the component menu
    [AddComponentMenu("Camera-Control/Smooth Follow")]

    void Start()
    {
        //init to wanted position
        transform.position = target.TransformPoint(new Vector3(0, height, -distance));
        transform.eulerAngles = target.eulerAngles;

        //rigidbody
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        ForceMove();
    }

    void FixedUpdate()
    {
        // Early out if we don't have a target
        if (!target || pauseFollow) return;

        // Where the camera should be
        Vector3 wantedPosition = target.TransformPoint(new Vector3(0, height, -distance));
        Vector3 currentPosition = transform.position;

        // How far away camera is
        Vector3 direction = wantedPosition - currentPosition;

        //if (direction.y <= 0.1)
        //{
        //    Debug.Log("closeu");
        //    //direction.y = 0;
        //}

        //Separated position forces
        direction.x *= distanceXForce;
        direction.y *= distanceYForce;
        direction.z *= distanceZForce;

        //direction = Vector3.Lerp(currentPosition, currentPosition + direction, 0.4f);

        // Move camera to wanted position
        rb.AddForce(direction * 2, ForceMode.Force);


        // Look at torque
        Vector3 wantedRotation = target.position - transform.position;
        Vector3 cross = Vector3.Cross(transform.forward, wantedRotation);

        rb.AddTorque(cross * rotationForce, ForceMode.Force);

        // Keeping upright torque
        wantedRotation = target.up - transform.up;
        cross = Vector3.Cross(transform.up, wantedRotation);

        rb.AddTorque(cross * rotationForce, ForceMode.Force);

        // Always look at the target
        //transform.LookAt(target.position);

    }

    //force to position
    public void ForceMove()
    {
        //position
        transform.position = target.TransformPoint(new Vector3(0, height, -distance));

        //rotation
        transform.rotation = target.rotation;// - transform.position;

        //resume following
        pauseFollow = false;
    }

    public void PanDeath(Vector3 direction)
    {
        //continue along path slightly
        rb.AddForce(direction * 10);

        //stare at point
        transform.LookAt(target);

        //stop following racer
        pauseFollow = true;
    }

    //// 1 is furthest possible
    //float FindPercentage(float wanted, float current)
    //{
    //    //calculate percentage that camera is from the desired point
    //    //(current - min) * (1/ max - min)
    //    float difference = wanted - current;
    //    float percentage = (difference) * (1.0f / (maxCamDist - 0));

    //    // Convert to difference value, not direction
    //    if (percentage < 0)
    //        percentage *= -1;

    //    //prevent from going greater than 1
    //    if (percentage > 1)
    //        percentage = 1;

    //    return percentage;
    //}
}