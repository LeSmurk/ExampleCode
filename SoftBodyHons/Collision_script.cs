using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision_script : MonoBehaviour {

    public SoftBody_script Body_scr;
    public int id;

    //store a list of all the normals this is colliding with
    private List<Vector3> collidingNormals = new List<Vector3>();
    //place the ids or names of the objects colliding with
    private List<int> collidingIDs = new List<int>();

	// Use this for initialization
	void Start ()
    {
        //get script
        Body_scr = GetComponentInParent<SoftBody_script>();

        //determine which id this mass point is
        id = transform.GetSiblingIndex();
        //int.TryParse(gameObject.name.Substring(0, 1), out id);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.DrawLine(Vector3.zero, collidingNormals[0], Color.red);
	}

    //test collisions

    void OnTriggerEnter(Collider other)
    {
        
        //create normal vector based on point and position of ball
        Vector3 colPoint = other.ClosestPoint(transform.position);
        Vector3 dir = transform.position - colPoint;
        dir = dir.normalized;

        //store as coliding normal
        collidingIDs.Add(other.GetInstanceID());
        collidingNormals.Add(dir);

        Body_scr.UpdateNormals(id, collidingNormals);

    }

    void OnTriggerExit(Collider other)
    {
        int normalToRemove = -1;

        //Find which colliding object is being removed
        for(int i = 0; i < collidingIDs.Count; i++)
        {
            //test if this is the normal being removed
            if(other.GetInstanceID() == collidingIDs[i])
            {
                normalToRemove = i;
                break;
            }
        }

        //edge case check
        if(normalToRemove != -1)
        {
            //remove from both id and normal
            collidingIDs.RemoveAt(normalToRemove);
            collidingNormals.RemoveAt(normalToRemove);
        }

        Body_scr.UpdateNormals(id, collidingNormals);
    }

    //void OnCollisionEnter(Collision col)
    //{
    //    if (col.gameObject.name == "Cube")
    //    {
    //        //force back based on contact point normal
    //        Body_scr.PushBack(col.contacts[0].point, col.contacts[0].normal);
    //    }
    //}

    ////void OnCollisionStay(Collision col)
    ////{
    ////    //force back based on contact point normal
    ////    Body_scr.ForceBack(id, col.contacts[0].normal);
    ////}

    //void OnCollisionExit(Collision col)
    //{
    //    if (col.gameObject.name == "Cube")
    //    {
    //        //force back based on contact point normal
    //        Body_scr.LeftCollision(id);
    //    }
    //}
}
