using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassPoint_script : MonoBehaviour
{
    public int id;

    public Vector3 position;
    public float mass;
    public Vector3 force;
    public Vector3 accel;
    public Vector3 velocity;
    //public List<Vector3> collidingNormals;
    //public bool isColliding;
    public float frictionForce = -0.1f;

    //store a list of all the normals this is colliding with
    private List<Vector3> collidingNormals = new List<Vector3>();
    //place the ids or names of the objects colliding with
    private List<int> collidingIDs = new List<int>();

    //spring info
    public float springDamp;
    //How "Strong" the spring is greater number == stronger
    public float springConst;
    //Rest distance off spring extension
    public float springRest;
    //maximum distance that a spring will exist between two mass points
    public float springMaxLength;

    //all other mass points
    public MassPoint_script[] massPointScripts;
    private int[] neighbours;

    // Use this for initialization
    void Start ()
    {
        //determine which id this mass point is
        id = transform.GetSiblingIndex();

        //init values
        position = transform.position;
        mass = 1000;
        force = Vector3.zero;
        accel = Vector3.zero;
        velocity = Vector3.zero;      
    }

    public void Init(MassPoint_script[] massPoints, int neighboursNum)
    {
        massPointScripts = new MassPoint_script[massPoints.Length];
        for(int i = 0; i < massPointScripts.Length; i++)
        {
            massPointScripts[i] = massPoints[i];
        }

        neighbours = new int[neighboursNum];
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //if colliding with any normals, remove the component that moves in that direction
        for (int i = 0; i < collidingNormals.Count; i++)
        {
            //Vector3 projected = Vector3.Project(massPoints[currElement].velocity, massPoints[currElement].collidingNormals[i]);
            //Vector3 projected = Vector3.Project(massPoints[currElement].velocity, massPoints[currElement].collidingNormals[i]);
            float dotProd = Vector3.Dot(velocity, collidingNormals[i]);

            //only remove in direction we want
            if (dotProd < 0)
                velocity -= collidingNormals[i] * dotProd;

        }

        position += velocity;
        transform.position = position;
        //massPos[currElement] = massTran[currElement].localPosition;

        //decay force every frame
        force /= 1.5f;
    }

    void Update()
    {
        //calculate nearest neighbours
        GetNearestNeighbours(id);

        UpdateMassPoint();
    }

    public void UpdateNeighbours(int[] neigh)
    {
        for(int i = 0; i < neigh.Length; i++)
        {
            neighbours[i] = neigh[i];
        }
    }

    //update mass point
    void UpdateMassPoint()
    {
        force = Vector3.zero;
        //get this nearest neighbours from the master script

        //loop through this list connecting springs
        //compare to neighbours
        for (int z = 0; z < neighbours.Length; z++)
        {
            int comparison = neighbours[z];

            //find distance to base spring
            float dist = Vector3.Distance(massPointScripts[comparison].position, position);

            //direction to base spring
            Vector3 dir = (massPointScripts[comparison].position - position).normalized;

            //compression
            //
            float compression = dist - springRest;

            //create relative vel between two mass points
            Vector3 vel = velocity - massPointScripts[comparison].velocity;
            float relVel = Vector3.Dot(vel, dir);

            //f = kx (force of spring) - relative velocity between the two masses * dampener
            float tempForce = (springConst * compression) - relVel * springDamp;

            //push force on mass iin correct direction

            //dir * force
            force += dir * tempForce;

            //force other point in other dir
            //massPointScripts[comparison].force -= dir * tempForce;
        }

        //calculate accelleration
        accel = force / mass;
        //add gravity
        accel += Vector3.down / 1000f;
        //cal vel
        velocity += accel;

        //CalculateFriction();
    }

    void GetNearestNeighbours(int house)
    {
        //flat find 6 nearest points, no exclusivity lock or far point grab
        //int[] neigh = new int[neighbours.Length];

        //store all the distances from the house
        float[] allDist = new float[massPointScripts.Length];
        //don't include itself as neighbour
        allDist[house] = float.PositiveInfinity;

        for (int i = 0; i < massPointScripts.Length; i++)
        {
            //don't include itself
            if (i != house)
                allDist[i] = Vector3.Distance(massPointScripts[house].position, massPointScripts[i].position);
        }

        for (int x = 0; x < neighbours.Length; x++)
        {
            //find closest
            float closest = Mathf.Min(allDist);

            //find out which mass point it is
            for (int i = 0; i < allDist.Length; i++)
            {
                if (closest == allDist[i] && i != house)
                {
                    //set this mass point as a neighbour
                    neighbours[x] = i;

                    //remove the distance set (POSSIBLE ERROR HERE?)
                    allDist[i] = float.PositiveInfinity;

                    break;
                }
            }
        }

        //return neighbours;
    }

    //collisions
    //void OnTriggerEnter(Collider other)
    //{

    //    //create normal vector based on point and position of ball
    //    Vector3 colPoint = other.ClosestPoint(transform.position);
    //    Vector3 dir = transform.position - colPoint;
    //    dir = dir.normalized;

    //    //store as coliding normal
    //    collidingIDs.Add(other.GetInstanceID());
    //    collidingNormals.Add(dir);

    //    //Body_scr.UpdateNormals(id, collidingNormals);

    //}

    //void OnTriggerExit(Collider other)
    //{
    //    int normalToRemove = -1;

    //    //Find which colliding object is being removed
    //    for (int i = 0; i < collidingIDs.Count; i++)
    //    {
    //        //test if this is the normal being removed
    //        if (other.GetInstanceID() == collidingIDs[i])
    //        {
    //            normalToRemove = i;
    //            break;
    //        }
    //    }

    //    //edge case check
    //    if (normalToRemove != -1)
    //    {
    //        //remove from both id and normal
    //        collidingIDs.RemoveAt(normalToRemove);
    //        collidingNormals.RemoveAt(normalToRemove);
    //    }

    //    //Body_scr.UpdateNormals(id, collidingNormals);
    //}

    //fairly basic friction method
    void CalculateFriction()
    {
        //if colliding with ground
        if (collidingNormals.Count > 0)
        {
            //create force against the direction of velocity
            Vector3 friction = velocity.normalized * frictionForce;
            force += friction;
        }
    }
}
