using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBody_script : MonoBehaviour
{

    struct MassPoint
    {
        public Vector3 position;
        public float mass;
        public Vector3 force;
        public Vector3 accel;
        public Vector3 velocity;
        public List<Vector3> collidingNormals;
        public bool isColliding;
    }

    //first mass point
    public GameObject massTemplate;

    //mass points
    private MassPoint[] massPoints;
    //local positions are used
    public Transform[] massTran;
    //store positions for mesh
    private List<Vector3> massPos = new List<Vector3>();

    //SPRING NETWORK
    private List<List<int>> springNet = new List<List<int>>();
    public List<List<float>> springRests = new List<List<float>>();
    private List<Vector3> triangles = new List<Vector3>();

    //SPRING INFO
    //spring loses force, greater increases dampening
    public float springDamp;
    //How "Strong" the spring is greater number == stronger
    public float springConst;
    //Rest distance off spring extension
    public float springRest;
    //maximum distance that a spring will exist between two mass points
    public float springMaxLength;

    //NEAREST NEIGHBOURS METHOD
    public int neighboursNum;

    public Rigidbody mass;

    //friction force should be a negative number
    public float frictionForce;

    private Mesh bodyMesh;
    private MeshCollider bodyCollider;

    //movement of body
    public float pushForce;


    //split mass points into scripts
    private MassPoint_script[] massPointsScripts;

    // Use this for initialization
    void Start()
    {
        //get mesh
        bodyMesh = GetComponent<MeshFilter>().mesh;
        bodyMesh.MarkDynamic();

        //bodyMesh.triangles = tris;//new int[] {0, 1, 2,  3, 4, 5};
        //set collider to mesh
        GetComponent<MeshFilter>().mesh = bodyMesh;
        bodyCollider = GetComponent<MeshCollider>();
        bodyCollider.sharedMesh = bodyMesh;

        Vector3[] vertices = bodyMesh.vertices;
        //Debug.Log(vertices.Length);

        //get template
        //massTemplate = transform.GetChild(0).gameObject;
        //massTemplate.transform.localPosition = vertices[0];

        ////create mass points for each vertice in the sphere
        //for (int i = 1; i < vertices.Length; i++)
        //{
        //    //spawn a new mass point
        //    Instantiate(massTemplate, transform.TransformPoint(vertices[i]), Quaternion.identity, transform);

        //    //add new point in the spring net
        //    springNet.Add(new List<int>());
        //    springRests.Add(new List<float>());
        //}

        //create box shape
        for(float y = -1f; y <= 1f; y += 0.4f)
        {
            for(float z = -1f; z <= 1f; z += 0.4f)
            {
                for(float x = -1f; x <= 1f; x += 0.4f)
                {
                    //spawn a new mass point
                    Instantiate(massTemplate, transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, transform);

                    ////add new point in the spring net
                    //springNet.Add(new List<int>());
                    //springRests.Add(new List<float>());
                }
            }       
        }           
                    
        //collect the mass points
        massTran = new Transform[transform.childCount];
        massPointsScripts = new MassPoint_script[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            massTran[i] = transform.GetChild(i);
            massPos.Add(massTran[i].localPosition);

            //split method instead
            //massPointsScripts[i] = massTran[i].GetComponent<MassPoint_script>();
        }

        //check distances between two mass points
        //Debug.Log(Vector3.Distance(massPos[2], massPos[3]));

        //set size of mass points to number of points given
        massPoints = new MassPoint[transform.childCount];

        //init mass points
        for (int i = 0; i < massPoints.Length; i++)
        {
            massPoints[i].position = massTran[i].position;
            massPoints[i].mass = 1000;
            massPoints[i].force = Vector3.zero;
            massPoints[i].accel = Vector3.zero;
            massPoints[i].velocity = Vector3.zero;
            massPoints[i].collidingNormals = new List<Vector3>();
            massPoints[i].isColliding = false;

            //pass a ref to all other mass points to the scripts
            //massPointsScripts[i].Init(massPointsScripts, neighboursNum);
        }

        //find starting nearest neighbours
        //NearestNeighboursSplitMethod();

        ////CREATING SPRINGS
        ////loop through all mass points
        //for(int i = 0; i < massPoints.Length; i++)
        //{
        //    //loop through all other masspoints
        //    for(int z = 0; z < massPoints.Length; z++)
        //    {
        //        if(z!= i)
        //        {
        //            if(Vector3.Distance(massPoints[z].position, massPoints[i].position) <= springRest)
        //            {
        //                AddSpringNet(i, z);
        //            }
        //        }
        //    }
        //}

        ////store triangles in sets of 3
        //int[] tris = bodyMesh.triangles;
        //for (int i = 0; i < tris.Length; i += 3)
        //{
        //    //add the current elemnt plus the next two
        //    triangles.Add(new Vector3(tris[i], tris[i + 1], tris[i + 2]));
        //}

        ////create springs between the triangle points
        //for (int i = 0; i < triangles.Count; i++)
        //{
        //    //WHY AM I CONVERTING INTS TO FLOATS THEN BACK TO INTS?????? stop using vector3
        //    //order the numbers from lowest to highest
        //    int x = Mathf.FloorToInt(Mathf.Min(triangles[i].x, triangles[i].y, triangles[i].z));

        //    //lowest out of first two
        //    int low = Mathf.FloorToInt(Mathf.Min(triangles[i].x, triangles[i].y));
        //    //lowest out of second two
        //    int low1 = Mathf.FloorToInt(Mathf.Min(triangles[i].x, triangles[i].z));
        //    //lowest of of third two
        //    int low2 = Mathf.FloorToInt(Mathf.Min(triangles[i].y, triangles[i].z));

        //    //midplace is highest of lowests
        //    int y = Mathf.FloorToInt(Mathf.Max(low, low1, low2));

        //    //highest number
        //    int z = Mathf.FloorToInt(Mathf.Max(triangles[i].x, triangles[i].y, triangles[i].z));

        //    //place back into triangles
        //    triangles[i] = new Vector3(x, y, z);

        //    //make a spring from lowest to highest, checking there isn't one already
        //    AddSpringNet(x, y);
        //    AddSpringNet(x, z);
        //    AddSpringNet(y, z);
        //}

        //Debug.Log(triangles[0]);
        //Debug.Log(triangles[1]);
        //Debug.Log(triangles[2]);
    }

    //spring net
    void AddSpringNet(int baseMass, int connectedMass)
    {
        //don't connect if the two mass points are the same
        if (baseMass == connectedMass)
            return;

        bool alreadyConnected = false;

        //loop through the springs at the mass point in question
        for (int i = 0; i < springNet[baseMass].Count; i++)
        {
            //check to see if there is a spring already between the two points
            if (springNet[baseMass][i] == connectedMass)
                alreadyConnected = true;
        }

        //add a spring between the two mass points if there isn't one already
        if (!alreadyConnected)
        {
            springNet[baseMass].Add(connectedMass);
            //create rest distance based on current separation
            springRests[baseMass].Add(Vector3.Distance(massPoints[baseMass].position, massPoints[connectedMass].position));
            //Debug.Log("Creating spring from: " + baseMass + " to: " + connectedMass);

        }

    }

    void FixedUpdate()
    {
        //mass.AddForce(massPoints[1].force);
        for (int i = 0; i < massPoints.Length; i++)
        {
            UpdatePositon(i);
        }

        //move right
        if (Input.GetKey("d"))
        {
            for (int i = 0; i < massPoints.Length; i++)
            {
                AddForce(i, Vector3.right);
            }
        }

        //move left
        if (Input.GetKey("a"))
        {
            for (int i = 0; i < massPoints.Length; i++)
            {
                AddForce(i, Vector3.left);
            }
        }

        ////LIFTOFF
        //if (Input.GetKey("w"))
        //{
        //    for (int i = 0; i < massPoints.Length; i++)
        //    {
        //        AddForce(i, Vector3.up * 100);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //Any close method
        ClosestPointsMethod();

        //SpringNetMethod();
        //UpdateMesh();

        //nearest neighbour method
        //NearestNeighboursMethod();

        //NearestNeighboursSplitMethod();

    }

    void NearestNeighboursMethod()
    {
        //loop through all mass points
        for (int i = 0; i < massPoints.Length; i++)
        {
            //find the nearest neighbours
            int[] neighbours = GetNearestNeighbours(i);

            //compare to neighbours
            for(int z = 0; z < neighbours.Length; z++)
            {
                int comparison = neighbours[z];

                //find distance to base spring
                float dist = Vector3.Distance(massPoints[comparison].position, massPoints[i].position);

                //direction to base spring
                Vector3 dir = (massPoints[comparison].position - massPoints[i].position).normalized;

                //compression
                // distance from other mass point, allowing for rest dist
                float compression = dist - springRest;

                //create relative vel between two mass points
                Vector3 vel = massPoints[i].velocity - massPoints[comparison].velocity;
                float relVel = Vector3.Dot(vel, dir);

                //f = kx (force of spring) - relative velocity between the two masses * dampener
                float force = (springConst * compression) - relVel * springDamp;

                //push force on mass in correct direction
                //dir * force
                massPoints[i].force += dir * force;

                //force other point in other dir
                massPoints[comparison].force -= dir * force;
            }

            //calculate accelleration
            massPoints[i].accel = massPoints[i].force / massPoints[i].mass;
            //add gravity
            massPoints[i].accel += Vector3.down / 1000f;
            //cal vel
            massPoints[i].velocity += massPoints[i].accel;

            CalculateFriction(i);

            //UpdatePositon(i);
        }
    }

    void NearestNeighboursSplitMethod()
    {
        //loop through all mass points
        for (int i = 0; i < massPointsScripts.Length; i++)
        {
            //find the nearest neighbours
            int[] neighbours = GetNearestNeighboursSplit(i);

            //make an array of the mass points scripts
            MassPoint_script[] neighbourMassPoints = new MassPoint_script[neighbours.Length];
            for(int n = 0; n < neighbours.Length; n++)
            {
                neighbourMassPoints[n] = massPointsScripts[n];
            }

            //ping the massscript to update
            //massPointsScripts[i].UpdateNeighbours(neighbourMassPoints);
        }
    }

    int[] GetNearestNeighbours(int house)
    {
        //flat find 6 nearest points, no exclusivity lock or far point grab
        int[] neighbours = new int[neighboursNum];

        //store all the distances from the house
        float[] allDist = new float[massPoints.Length];
        //don't include itself as neighbour
        allDist[house] = float.PositiveInfinity;

        for(int i = 0; i < massPoints.Length; i++)
        {
            //don't include itself
            if(i != house)
                allDist[i] = Vector3.Distance(massPoints[house].position, massPoints[i].position);
        }

        for(int x = 0; x < neighbours.Length; x++)
        {
            //find closest
            float closest = Mathf.Min(allDist);

            //find out which mass point it is
            for (int i = 0; i < allDist.Length; i++)
            {
                if(closest == allDist[i] && i != house)
                {
                    //set this mass point as a neighbour
                    neighbours[x] = i;

                    //remove the distance set (POSSIBLE ERROR HERE?)
                    allDist[i] = float.PositiveInfinity;

                    break;
                }
            }
        }

        return neighbours;
    }

    int[] GetNearestNeighboursSplit(int house)
    {
        //flat find 6 nearest points, no exclusivity lock or far point grab
        int[] neighbours = new int[neighboursNum];

        //store all the distances from the house
        float[] allDist = new float[massPointsScripts.Length];
        //don't include itself as neighbour
        allDist[house] = float.PositiveInfinity;

        for (int i = 0; i < massPointsScripts.Length; i++)
        {
            //don't include itself
            if (i != house)
                allDist[i] = Vector3.Distance(massPointsScripts[house].position, massPointsScripts[i].position);
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

        return neighbours;
    }

    void ClosestPointsMethod()
    {
        //update all stored mass points
        for (int i = 0; i < massPoints.Length; i++)
        {
            Vector3 currForce = new Vector3(0, 0, 0);
            int numOfSprings = 0;

            //spring to other mass points next to it in array
            for (int z = 0; z < massPoints.Length; z++)
            {
                //don't compare to its own mass point, and only use viable numbers
                if (i != z)
                {
                    ////set z to a fake num
                    int comparison = z;

                    //find distance to base spring
                    float dist = Vector3.Distance(massPoints[comparison].position, massPoints[i].position);

                    //ONLY CONNECT SPRING IF CLOSE ENOUGH
                    if (dist < springMaxLength)
                    {
                        //direction to base spring
                        Vector3 dir = (massPoints[comparison].position - massPoints[i].position).normalized;

                        //compression
                        //
                        float compression = dist - springRest;

                        //damping coefficient (we want underdamped, so coeff is < 1)
                        //c = 2 sqr (k / m)
                        //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

                        //create relative vel between two mass points
                        Vector3 vel = massPoints[i].velocity - massPoints[comparison].velocity;
                        float relVel = Vector3.Dot(vel, dir);

                        //f = kx (force of spring) - relative velocity between the two masses * dampener
                        float force = (springConst * compression) - relVel * springDamp;

                        //push force on mass iin correct direction
                        //dir * force
                        //massPoints[i].force += dir * force;
                        currForce += dir * force;

                        //add to spring count
                        numOfSprings++;

                        //force other point in other dir
                        //massPoints[comparison].force -= dir * force;
                    }
                }
            }

            //reduce the force based on number of springs
            massPoints[i].force += (currForce / (numOfSprings * 0.8f));

            //calculate accelleration
            massPoints[i].accel = massPoints[i].force / massPoints[i].mass;
            //add gravity
            //massPoints[i].accel += Vector3.down / 1000f;
            //cal vel
            massPoints[i].velocity += massPoints[i].accel;
            //UpdatePositon(i);
        }
    }

    void SpringNetMethod()
    {
        //loop through all spring net positions
        for (int i = 0; i < springNet.Count; i++)
        {
            //test all springs connected to this mass
            for (int z = 0; z < springNet[i].Count; z++)
            {
                //BASE MASS POINT IS i
                //MASS POINT THAT BASE IS CONNECTED TO
                int comparison = springNet[i][z];

                //Debug.Log("comparing mass point: " + i + " to: " + comparison);
                if (i == 0)
                    Debug.DrawLine(massPoints[comparison].position, massPoints[i].position, Color.red);

                //direction to base spring
                Vector3 dir = (massPoints[comparison].position - massPoints[i].position).normalized;

                float dist = Vector3.Distance(massPoints[comparison].position, massPoints[i].position);

                //compression
                //
                float compression = dist - springRests[i][z];

                //damping coefficient (we want underdamped, so coeff is < 1)
                //c = 2 sqr (k / m)
                //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

                //create relative vel between two mass points
                Vector3 vel = massPoints[i].velocity - massPoints[comparison].velocity;
                float relVel = Vector3.Dot(vel, dir);

                //f = kx (force of spring) - relative velocity between the two masses * dampener
                float force = (springConst * compression) - relVel * springDamp;

                //push force on mass iin correct direction
                //dir * force
                massPoints[i].force += dir * force;

                //force other point in other dir
                //massPoints[comparison].force -= dir * force;
            }
        }

        //loop through the mass points
        for (int i = 0; i < massPoints.Length; i++)
        {
            //calculate accelleration
            massPoints[i].accel = massPoints[i].force / massPoints[i].mass;
            //add gravity
            massPoints[i].accel += Vector3.down / 1000f;
            //cal vel
            massPoints[i].velocity += massPoints[i].accel;
            //UpdatePositon(i);
        }
    }

    void UpdatePositon(int currElement)
    {
        //if colliding with any normals, remove the component that moves in that direction
        for(int i = 0; i < massPoints[currElement].collidingNormals.Count; i++)
        {
            float dotProd = Vector3.Dot(massPoints[currElement].velocity, massPoints[currElement].collidingNormals[i]);

            //only remove in direction we want
            if (dotProd < 0)
                massPoints[currElement].velocity -= massPoints[currElement].collidingNormals[i] * dotProd;
        }

        //if(massPoints[currElement].isColliding)
        //{
        //    //remove the velocity component of the direction that the colliding object is in
        //    Vector3 projected = Vector3.Project(massPoints[currElement].velocity, massPoints[currElement].collidingNormal);
        //    massPoints[currElement].velocity -= Vector3.Project(massPoints[currElement].velocity, massPoints[currElement].collidingNormal);
        //    //Debug.DrawLine(Vector3.zero, projected * 2000, Color.red);
        //    //Debug.Log(massPoints[currElement].collidingNormal);
        //}

        //Debug.Log(massPoints[currElement].velocity);
        massPoints[currElement].position += massPoints[currElement].velocity;
        massTran[currElement].position = massPoints[currElement].position;
        massPos[currElement] = massTran[currElement].localPosition;

        //decay force every frame
        massPoints[currElement].force /= 1.5f;
    }

    void UpdateMesh()
    {
        bodyMesh.SetVertices(massPos);
        bodyMesh.RecalculateNormals();
        bodyMesh.RecalculateBounds();
        bodyCollider.sharedMesh = bodyMesh;
    }

    //movement
    void AddForce(int id, Vector3 direction)
    {
        //move in direction
        massPoints[id].force += direction * pushForce;
    }

    void AddForceSplit(int id, Vector3 direction)
    {
        //move in direction
        massPointsScripts[id].force += direction * pushForce;
    }

    //collisions
    public void UpdateNormals(int currElement, List<Vector3> newNormals)
    {
        //set list to new list
        massPoints[currElement].collidingNormals = newNormals;
    }

    //fairly basic friction method
    void CalculateFriction(int currElement)
    {
        //if colliding with ground
        if(massPoints[currElement].collidingNormals.Count > 0)
        {
            //create force against the direction of velocity
            Vector3 friction = massPoints[currElement].velocity.normalized * frictionForce;
            massPoints[currElement].force += friction;
        }
    }

    //Unused now
    public void ForceBack(int currElement, Vector3 dir)
    {
        //Debug.Log("collision");
        //use current velocity but inverse the direction
        //massPoints[currElement].velocity = massPoints[currElement].velocity.magnitude * dir;
        //reduce velocity too, absorbed vel
        //massPoints[currElement].velocity /= 2f;

        //normal vector of surface hit
        //massPoints[currElement].collidingNormal = dir;
        //massPoints[currElement].isColliding = true;
    }
    
    public void LeftCollision(int currElement)
    {
        massPoints[currElement].isColliding = false;
    }

    public void PushBack(Vector3 point, Vector3 normal)
    {
        //don't allow nearby points to go past the plane?
        for(int i = 0; i < massPoints.Length; i++)
        {
            //see if close enough to point
            if(Vector3.Distance(massPoints[i].position, point) < 0.6f)
            {
                //Debug.Log(i);
                //massPoints[i].collidingNormal = normal;
                massPoints[i].isColliding = true;
            }
        }
    }
}
