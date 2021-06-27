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

        public Bounds collider;
        public List<Vector3> collidingNormals;
        public List<Vector3> collidingVels;
        public List<float> collidingMasses;
        public List<int> collidingPoints;
        public bool isColliding;

        public int[] fixedNeigh;
        public float[] rests;
        public bool stationaryPoint;
        public int parentID;
    }

    struct ScenePoint
    {
        public Vector3 pos;
        public Bounds collider;
        public float mass;
        public Vector3 velocity;
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

    //COLLISIONS
    public GameObject sceneObjs;
    //private List<Vector3> sceneObjPos = new List<Vector3>();
    //private List<Bounds> sceneColliders = new List<Bounds>();
    private ScenePoint[] scenePoints;

    //movement of body
    public float pushForce;

    //number of masses in bodies so I don't have to figure out number manually
    public int bodySize0 = 0;
    public int bodySize1 = 0;


    //RENDER BODY
    private List<Color> colours = new List<Color>();
    private int[] indices;

    //split mass points into scripts
    private MassPoint_script[] massPointsScripts;

    // Use this for initialization
    void Start()
    {
        //get mesh
        //bodyMesh = GetComponent<MeshFilter>().mesh;
        //bodyMesh.MarkDynamic();

        //bodyMesh.triangles = tris;//new int[] {0, 1, 2,  3, 4, 5};
        //set collider to mesh
        //GetComponent<MeshFilter>().mesh = bodyMesh;
        //bodyCollider = GetComponent<MeshCollider>();
        //bodyCollider.sharedMesh = bodyMesh;

        //Vector3[] vertices = bodyMesh.vertices;
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

        //Get all coliders in the scene
        scenePoints = new ScenePoint[sceneObjs.transform.childCount];
        for(int i = 0; i < sceneObjs.transform.childCount; i++)
        {
            //SEPARATE LISTS REPLACED BY A STRUCT
            //sceneObjPos.Add(sceneObjs.transform.GetChild(i).position);
            //sceneColliders.Add(sceneObjs.transform.GetChild(i).GetComponent<Collider>().bounds);

            scenePoints[i].pos = sceneObjs.transform.GetChild(i).position;
            scenePoints[i].collider = sceneObjs.transform.GetChild(i).GetComponent<Collider>().bounds;
            // stationary objects have a negative mass just so that the aren't pushed (acts as if infinite mass)
            scenePoints[i].mass = -1;
            scenePoints[i].velocity = Vector3.zero;

            //disable unity collider, don't need it
            sceneObjs.transform.GetChild(i).GetComponent<Collider>().enabled = false;
            //Debug.Log(sceneColliders[i].extents);
        }

        //first obj moves
        //scenePoints[0].mass = -1;
        //scenePoints[0].velocity = new Vector3(0, 0.005f, 0);

        //SPAWN MASSES
        //LoadClothExample();
        LoadOneBigExample();
        //LoadOneHugeExample();
        //LoadTwoExample();

        //init arrays
        //set size of mass points to number of points given
        massPoints = new MassPoint[transform.childCount];
        massTran = new Transform[transform.childCount];
        massPointsScripts = new MassPoint_script[transform.childCount];

        //create into body in struct or script
        SpawnBody(0, 0, bodySize0);
        //COMMENT IN IF TWO BODIES ARE USED
        //SpawnBody(1, bodySize0, bodySize0 + bodySize1);

        //find starting nearest neighbours
        for (int i = 0; i < massPoints.Length; i++)
        {
            GetFixedNearestNeighbours(i);
        }
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

    //REMEMBER AND UNCOMMENT IN SPAWN BODY FUNC
    void LoadClothExample()
    {
        //cloth shape
        for (float y = -1f; y <= 1; y += 0.2f)
        {
            for (float z = -1f; z <= 0; z += 0.2f)
            {
                //for(float x = 5; x <= 6; x+= 0.2f)
                //{
                //spawn a new mass point
                Instantiate(massTemplate, transform.TransformPoint(new Vector3(5, y, z)), Quaternion.identity, transform);
                //}
                bodySize0++;
            }
        }
    }

    void LoadTwoExample()
    {
        //create box shape
        for (float y = -0.5f; y <= 0.2f; y += 0.2f)
        {
            for (float z = -0.5f; z <= 0.2f; z += 0.2f)
            {
                for (float x = -0.5f; x <= 0.2f; x += 0.2f)
                {
                    //spawn a new mass point
                    Instantiate(massTemplate, transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, transform);
                    bodySize0++;
                    ////add new point in the spring net
                    //springNet.Add(new List<int>());
                    //springRests.Add(new List<float>());
                }
            }
        }
        //create box shape
        for (float y = 3.5f; y <= 4.2f; y += 0.2f)
        {
            for (float z = -0.5f; z <= 0.2f; z += 0.2f)
            {
                for (float x = -0.5f; x <= 0.2f; x += 0.2f)
                {
                    //spawn a new mass point
                    Instantiate(massTemplate, transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, transform);

                    bodySize1++;
                    ////add new point in the spring net
                    //springNet.Add(new List<int>());
                    //springRests.Add(new List<float>());
                }
            }
        }
    }

    void LoadOneBigExample()
    {
        //create box shape
        for (float y = -1f; y <= 0f; y += 0.2f)
        {
            for (float z = -1f; z <= 0f; z += 0.2f)
            {
                for (float x = 5.5f; x <= 6.5f; x += 0.2f)
                {
                    //spawn a new mass point
                    Instantiate(massTemplate, transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, transform);
                    bodySize0++;
                    ////add new point in the spring net
                    //springNet.Add(new List<int>());
                    //springRests.Add(new List<float>());
                }
            }
        }

        Debug.Log(bodySize0);
    }

    void LoadOneHugeExample()
    {
        //create box shape
        for (float y = -1f; y <= 0.5f; y += 0.2f)
        {
            for (float z = -1f; z <= 0.5f; z += 0.2f)
            {
                for (float x = 5.5f; x <= 7f; x += 0.2f)
                {
                    //spawn a new mass point
                    Instantiate(massTemplate, transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, transform);
                    bodySize0++;
                    ////add new point in the spring net
                    //springNet.Add(new List<int>());
                    //springRests.Add(new List<float>());
                }
            }
        }
    }

    void InstantiateMasses()
    {
        ////cloth shape
        //for (float y = -1f; y <= 1; y += 0.2f)
        //{
        //    for (float z = -1f; z <= 1; z += 0.2f)
        //    {
        //        //for(float x = 5; x <= 6; x+= 0.2f)
        //        //{
        //        //spawn a new mass point
        //        Instantiate(massTemplate, transform.TransformPoint(new Vector3(5, y, z)), Quaternion.identity, transform);
        //        //}
        //    }
        //}

        //create box shape
        for (float y = -0.5f; y <= 0.5f; y += 0.2f)
        {
            for (float z = -0.5f; z <= 0.5f; z += 0.2f)
            {
                for (float x = -0.5f; x <= 0.5f; x += 0.2f)
                {
                    //spawn a new mass point
                    Instantiate(massTemplate, transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, transform);

                    ////add new point in the spring net
                    //springNet.Add(new List<int>());
                    //springRests.Add(new List<float>());
                }
            }
        }
    }

    void SpawnBody(int id, int bodyStart, int bodyEnd)
    {
        //collect the mass points

        //indices for the rendering
        //indices = new int[transform.childCount];

        for (int i = bodyStart; i < bodyEnd; i++)
        {
            massTran[i] = transform.GetChild(i);
            //storing position locally for mesh
            massPos.Add(massTran[i].localPosition);
            //setting colours
            //colours.Add(Color.blue);
            //indices[i] = i;

            //split method instead
            //massPointsScripts[i] = massTran[i].GetComponent<MassPoint_script>();
        }

        //set a new mesh filter
        //bodyMesh = new Mesh();
        //UpdateMesh();
        //GetComponent<MeshFilter>().mesh = bodyMesh;
        //bodyMesh = GetComponent<MeshFilter>().mesh;

        //check distances between two mass points
        //Debug.Log(Vector3.Distance(massPos[2], massPos[3]));

        //init mass points
        for (int i = bodyStart; i < bodyEnd; i++)
        {
            massPoints[i].position = massTran[i].position;
            massPoints[i].mass = 1000;
            massPoints[i].force = Vector3.zero;
            massPoints[i].accel = Vector3.zero;
            massPoints[i].velocity = Vector3.zero;

            massPoints[i].collidingNormals = new List<Vector3>();
            massPoints[i].collidingVels = new List<Vector3>();
            massPoints[i].collidingMasses = new List<float>();
            massPoints[i].collidingPoints = new List<int>();
            massPoints[i].isColliding = false;

            massPoints[i].stationaryPoint = false;

            //setting which body this belongs to
            massPoints[i].parentID = id;

            //store aabb box
            massPoints[i].collider = massTran[i].GetComponent<Collider>().bounds;
            //scaling bounds properly
            massPoints[i].collider.extents *= 0.7f;

            //turn off unity collider
            massTran[i].GetComponent<Collider>().enabled = false;

            ////for cloth, fix points
            //if (i >= 60) // 5 is 930 // 110
            //    massPoints[i].stationaryPoint = true;

            //pass a ref to all other mass points to the scripts
            //massPointsScripts[i].Init(massPointsScripts, neighboursNum);
        }
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

        //move scene objects
        for(int i = 0; i < scenePoints.Length; i++)
        {
            scenePoints[i].pos += scenePoints[i].velocity;
            sceneObjs.transform.GetChild(i).position = scenePoints[i].pos;
        }

        //move right
        if (Input.GetKey("d"))
        {
            for (int i = 0; i < massPoints.Length; i++)
            {
                //AddForce(i, Vector3.right);
            }
        }

        //move left
        if (Input.GetKey("a"))
        {
            for (int i = 0; i < massPoints.Length; i++)
            {
                //AddForce(i, Vector3.left);
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
        //ClosestPointsMethod();

        //SpringNetMethod();
        //UpdateMesh();
        bool pushing = false;
        //move right
        if (Input.GetKey("d"))
            pushing = true;

        //nearest neighbour method
        NearestNeighboursMethod(pushing);
        //NearestNeighboursMethodFixed(pushing);

        //NearestNeighboursSplitMethod();

        //update shader
        //for(int i = 0; i < bodyMesh.vertices.Length; i++)
        //{
        //    //move vertices to mass points

        //}

        //UpdateMesh();

    }

    void NearestNeighboursMethodFixed(bool push)
    {
        //loop through all mass points
        for (int i = 0; i < massPoints.Length; i++)
        {
            //this point is fixed, don't calculate its forces
            if (massPoints[i].stationaryPoint == true)
                continue;

            //find the nearest neighbours
            int[] neighbours = massPoints[i].fixedNeigh; //GetNearestNeighbours(i);
            float[] rests = massPoints[i].rests; //GetNearestNeighbours(i);

            Vector3 totalForce = Vector3.zero;

            //compare to neighbours
            for (int z = 0; z < neighbours.Length; z++)
            {
                int comparison = neighbours[z];

                //find distance to base spring
                float dist = Vector3.Distance(massPoints[comparison].position, massPoints[i].position);

                //direction to base spring
                Vector3 dir = (massPoints[comparison].position - massPoints[i].position).normalized;

                //compression
                // distance from other mass point, allowing for rest dist
                float compression = dist - massPoints[i].rests[z];
                //float compression = dist - springRest;

                //create relative vel between two mass points
                Vector3 vel = massPoints[i].velocity - massPoints[comparison].velocity;
                float relVel = Vector3.Dot(vel, dir);

                //f = kx (force of spring) - relative velocity between the two masses * dampener
                float force = (springConst * compression) - (relVel * springDamp);

                //push force on mass in correct direction
                //dir * force
                totalForce += dir * force;

                //force other point in other dir
                //massPoints[comparison].force -= dir * force;
            }

            //calculate accelleration
            massPoints[i].force = totalForce;
            if (push)
                AddForce(i, Vector3.right);
            massPoints[i].accel = massPoints[i].force / massPoints[i].mass;
            //add gravity
            massPoints[i].accel += Vector3.down / 1000f;
            //cal vel
            massPoints[i].velocity += massPoints[i].accel;

            CalculateFriction(i);

            //UpdatePositon(i);
        }
    }

    void NearestNeighboursMethod(bool push)
    {
        //loop through all mass points
        for (int i = 0; i < massPoints.Length; i++)
        {
            //this point is fixed, don't calculate its forces
            if (massPoints[i].stationaryPoint == true)
                continue;

            //find the nearest neighbours
            int[] neighbours = GetNearestNeighbours(i);//massPoints[i].fixedNeigh; //GetNearestNeighbours(i);
            float[] rests = massPoints[i].rests; //GetNearestNeighbours(i);

            Vector3 totalForce = Vector3.zero;

            //compare to neighbours
            for (int z = 0; z < neighbours.Length; z++)
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
                float force = (springConst * compression) - (relVel * springDamp);

                //push force on mass in correct direction
                //dir * force
                totalForce += dir * force;

                //force other point in other dir
                //massPoints[comparison].force -= dir * force;
            }

            //calculate accelleration
            massPoints[i].force = totalForce;
            if(push)
                AddForce(i, Vector3.right);
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
            massPoints[i].position = massPointsScripts[i].position;
            massPoints[i].velocity = massPointsScripts[i].velocity;

            ////find the nearest neighbours
            //int[] neighbours = GetNearestNeighboursSplit(i);

            ////make an array of the mass points scripts
            //MassPoint_script[] neighbourMassPoints = new MassPoint_script[neighbours.Length];
            //for(int n = 0; n < neighbours.Length; n++)
            //{
            //    neighbourMassPoints[n] = massPointsScripts[n];
            //}

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

    void GetFixedNearestNeighbours(int house)
    {
        //maximum neighbours for any point (eg is 8 flat cloth using diagonals)
        int[] neighbours = new int[neighboursNum];
        float[] rests = new float[neighboursNum];
        int totalNeigh = 0;

        //get the neighbours
        neighbours = GetNearestNeighbours(house);
        //get the rests
        for(int i = 0; i < neighbours.Length; i++)
        {
            rests[i] = Vector3.Distance(massPoints[house].position, massPoints[neighbours[i]].position);

            totalNeigh++;
        }

        //This is more like a closestpoints/neighbours hybrid
        ////loop through all mass points
        //for (int i = 0; i < massPoints.Length; i++)
        //{
        //    //don't use itself or ones of another body id
        //    if (house == i || massPoints[house].parentID != massPoints[i].parentID)
        //        continue;

        //    float dist = Vector3.Distance(massPoints[house].position, massPoints[i].position);
        //    //store ones close enough
        //    if (dist <= springRest) //should stay around 0.6f
        //    {
        //        //Debug.Log("dist from " + house + " : " + Vector3.Distance(massPoints[house].position, massPoints[i].position));
        //        neighbours[totalNeigh] = i;
        //        rests[totalNeigh] = dist;
        //        totalNeigh++;
        //    }
        //}

        //store into struct
        massPoints[house].fixedNeigh = new int[totalNeigh];
        massPoints[house].rests = new float[totalNeigh];
        for(int x = 0; x < totalNeigh; x++)
        {
            massPoints[house].fixedNeigh[x] = neighbours[x];
            massPoints[house].rests[x] = rests[x];
        }
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
            massPoints[i].force = (currForce / (numOfSprings * 0.8f));

            //calculate accelleration
            massPoints[i].accel = massPoints[i].force / massPoints[i].mass;
            //add gravity
            massPoints[i].accel += Vector3.down / 1000f;
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
        //check for collisions
        CheckCollision(currElement);

        //if colliding with any normals, remove the component that moves in that direction
        for (int i = 0; i < massPoints[currElement].collidingNormals.Count; i++)
        {
            //remove component from velocity if the object colliding with can't be pushed
            //if (massPoints[currElement].collidingMasses[i] == -1)
            //{

            //add the velocity that the obj is moving in
            massPoints[currElement].velocity += massPoints[currElement].collidingVels[i];

            float dotProd = Vector3.Dot(massPoints[currElement].velocity, massPoints[currElement].collidingNormals[i]);

            //only remove in direction we want
            if (dotProd < 0)
                massPoints[currElement].velocity -= massPoints[currElement].collidingNormals[i] * dotProd;
            //}

            ////////Moving collision
            //else
            //{
            //    //wrote it on two lines because its easier to see
            //    //v1 = u1 * m1 - m2 / m1 + m2 + (u2 * 2*m2 / m1 + m2)
            //    Vector3 vel = massPoints[currElement].velocity * (massPoints[currElement].mass - massPoints[currElement].collidingMasses[i]) / (massPoints[currElement].mass + massPoints[currElement].collidingMasses[i]);
            //    vel += (massPoints[currElement].collidingVels[i] * 2 * massPoints[currElement].collidingMasses[i] / (massPoints[currElement].mass + massPoints[currElement].collidingMasses[i]));
            //    //v2 = u1 * 2*m1 / m1 + m2 + (u2 * m2 - m1 / m1 + m2)
            //    Vector3 vel2 = massPoints[currElement].velocity * 2 * massPoints[currElement].mass / (massPoints[currElement].mass + massPoints[currElement].collidingMasses[i]);
            //    vel2 += (massPoints[currElement].collidingVels[i] * (massPoints[currElement].collidingMasses[i] - massPoints[currElement].mass) / (massPoints[currElement].mass + massPoints[currElement].collidingMasses[i]));

            //    massPoints[currElement].velocity = vel;
            //    massPoints[massPoints[currElement].collidingPoints[i]].velocity = vel2;
            //}
        }

        ////reset normals (Only needed for my colliders)
        massPoints[currElement].collidingNormals.Clear();
        massPoints[currElement].collidingVels.Clear();
        ////massPoints[currElement].collidingMasses.Clear();
        ////massPoints[currElement].collidingPoints.Clear();

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

        //massPointsScripts[currElement].position = massPoints[currElement].position;
        //massPointsScripts[currElement].velocity = massPoints[currElement].velocity;

        //decay force every frame
        //massPoints[currElement].force /= 1.5f;
    }

    //void UpdateMesh()
    //{
    //    bodyMesh.Clear();
    //    bodyMesh.SetVertices(massPos);
    //    bodyMesh.SetColors(colours);
    //    bodyMesh.SetIndices(indices, MeshTopology.Points, 0);
    //    //bodyMesh.SetUVs(0, massPos);
    //    //bodyMesh.RecalculateNormals();
    //    //bodyMesh.RecalculateBounds();
    //    //bodyCollider.sharedMesh = bodyMesh;
    //}

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

    //test collisions with my collision system
    void CheckCollision(int checkElem)
    {
        //compare to all mass points (check elem + 1 for not double check collisions)
        for (int i = 0; i < massPoints.Length; i++)
        {
            //not compare itself and don't compare things that belong to the same object (stored in body id)
            if(checkElem != i && massPoints[checkElem].parentID != massPoints[i].parentID)
            {
                //Collision detected
                Vector3 newCol = CompareCollisions(massPoints[checkElem].position, massPoints[i].position, massPoints[checkElem].collider, massPoints[i].collider);
                if (newCol.x != -100)
                {
                    //THIS WAS FOR TESTING ELASTIC COLLISIONS
                    ////wrote it on two lines because its easier to see
                    ////v1 = u1 * m1 - m2 / m1 + m2 + (u2 * 2*m2 / m1 + m2)
                    //Vector3 vel = massPoints[checkElem].velocity * (massPoints[checkElem].mass - massPoints[i].mass) / (massPoints[checkElem].mass + massPoints[i].mass);
                    //vel += (massPoints[i].velocity * 2 * massPoints[i].mass / (massPoints[checkElem].mass + massPoints[i].mass));
                    ////v2 = u1 * 2*m1 / m1 + m2 + (u2 * m2 - m1 / m1 + m2)
                    //Vector3 vel2 = massPoints[checkElem].velocity * 2 * massPoints[checkElem].mass / (massPoints[checkElem].mass + massPoints[i].mass);
                    //vel2 += (massPoints[i].velocity * (massPoints[i].mass - massPoints[checkElem].mass) / (massPoints[checkElem].mass + massPoints[i].mass));

                    //massPoints[checkElem].velocity = vel;
                    //massPoints[i].velocity = vel2;

                    massPoints[checkElem].collidingNormals.Add(newCol);
                    massPoints[checkElem].collidingVels.Add(massPoints[i].velocity);
                    //massPoints[checkElem].collidingMasses.Add(massPoints[i].mass);
                    //massPoints[checkElem].collidingPoints.Add(i);
                }
            }
        }

        //compare all colliders in scene to mass point
        for (int i = 0; i < scenePoints.Length; i++)
        {
            //Collision detected
            Vector3 newCol = CompareCollisions(massPoints[checkElem].position, scenePoints[i].pos, massPoints[checkElem].collider, scenePoints[i].collider);
            //colliding
            if (newCol.x != -100)
            {   
                ////store normal of colision and velocity and mass of other obj
                massPoints[checkElem].collidingNormals.Add(newCol);
                massPoints[checkElem].collidingVels.Add(scenePoints[i].velocity);
                //massPoints[checkElem].collidingMasses.Add(scenePoints[i].mass);
            }
        }
    }

    Vector3 AABBMax(Vector3 pos, Bounds col)
    {
        return new Vector3(pos.x + col.extents.x, pos.y + col.extents.y, pos.z + col.extents.z);
    }

    Vector3 AABBMin(Vector3 pos, Bounds col)
    {
        return new Vector3(pos.x - col.extents.x, pos.y - col.extents.y, pos.z - col.extents.z);
    }

    Vector3 ClosestPoint(Vector3 Max, Vector3 Min, Vector3 pos)
    {
        Vector3 point;

        if (pos.x > Max.x)
            point.x = Max.x;
        else if (pos.x < Min.x)
            point.x = Min.x;
        else
            point.x = pos.x;
        //yclose
        if (pos.y > Max.y)
            point.y = Max.y;
        else if (pos.y < Min.y)
            point.y = Min.y;
        else
            point.y = pos.y;
        //zclose
        if (pos.z > Max.z)
            point.z = Max.z;
        else if (pos.z < Min.z)
            point.z = Min.z;
        else
            point.z = pos.z;

        return point;
    }

    Vector3 CompareCollisions(Vector3 pos1, Vector3 pos2, Bounds col1, Bounds col2)
    {
        Vector3 collisionNormal = new Vector3(-100, -100, -100);

        Vector3 max1 = AABBMax(pos1, col1);
        Vector3 min1 = AABBMin(pos1, col1);
        Vector3 max2 = AABBMax(pos2, col2);
        Vector3 min2 = AABBMin(pos2, col2);

        bool isColliding = false;

        if (max1.x >= min2.x && min1.x < max2.x)
        {
            if (max1.y >= min2.y && min1.y < max2.y)
            {
                if (max1.z >= min2.z && min1.z < max2.z)
                {
                    //COLLIDING
                    isColliding = true;
                    //Debug.Log("dsjkfhsd");
                }
            }
        }

        if (isColliding)
        {
            //create normal vector based on point and position of ball
            Vector3 colPoint = ClosestPoint(max2, min2, pos1);
            Vector3 dir = pos1 - colPoint;
            dir = dir.normalized;

            collisionNormal = dir;
        }

        return collisionNormal;
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
