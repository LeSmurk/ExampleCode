using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

public class PlaceParticles : MonoBehaviour
{
    //public GameObject original;
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] blocks;
    private Vector3[] startingPos;
    private Vector3[] startingRot;
    private bool off = false;
    private bool destroyed = false;
    private bool pullback = false;
    private int size;

    private StreamReader read;
    private Vector3[] txtPositions;
    private Color[] txtColours;

    public string posFileName;
    public string colFileName;

    private int totalLines;

    //Returning rate
    public float returnRate = 0.05f;
    private float returnTimer;
    private int currentReturner = 0;
    public int returnChunkAmount = 10;

    //public TextAsset posTxtFile;
    //public TextAsset colTextFile;

    //colours possible
    //private Color grey = new Color(0.1f, 0.1f, 0.1f);
    //private Color blue = new Color(0.1f, 0.1f, 0.1f);

    // Use this for initialization
    void Start()
    {
        string path = "Particle Text Files/" + posFileName + ".txt";
        Resources.Load(path);

        //init lengths and read from file
        read = new StreamReader(path);
        totalLines = System.IO.File.ReadAllLines(path).Length;
        int arraysizes = Mathf.CeilToInt((float)totalLines / 6);
        txtPositions = new Vector3[arraysizes];
        txtColours = new Color[arraysizes];
        Debug.Log(arraysizes);
        Debug.Log(totalLines);

        //storing into the array in use
        int currentArrNum = 0;
        //store file info into Vector array
        for(int i = 0; i < arraysizes - 1; i++)
        {
            //burn a read line
            read.ReadLine();
            read.ReadLine();
            read.ReadLine();
            read.ReadLine();
            read.ReadLine();
            //read line and store
            string newString = read.ReadLine();
            //split line into 3 parts
            string[] values = newString.Split(' ');
            //store 3 parts as position vector
            txtPositions[i] = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            currentArrNum++;
        };

        read.Close();

        //Store file into colours array
        path = "Particle Text Files/" + colFileName + ".txt";
        //Resources.Load(path);

        read = new StreamReader(path);

        //storing into the array in use reset
        currentArrNum = 0;
        for (int i = 0; i < arraysizes - 1; i++)
        {
            //burn a read line
            read.ReadLine();
            read.ReadLine();
            read.ReadLine();
            read.ReadLine();
            read.ReadLine();
            string newString = read.ReadLine();
            string[] values = newString.Split(' ');
            //positions are in even number
            txtColours[i] = new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            currentArrNum++;
        };

        read.Close();

        //get the particle system info
        particleSystem = GetComponentInChildren<ParticleSystem>();

        //create max particles vbased on how many lines there are
        ParticleSystem.MainModule newMain = particleSystem.main;
        newMain.maxParticles = arraysizes;

        //init particle blocks array
        blocks = new ParticleSystem.Particle[particleSystem.main.maxParticles];

        //init starting position storage of the particles
        startingPos = new Vector3[particleSystem.main.maxParticles];
        startingRot = new Vector3[particleSystem.main.maxParticles];

    }

    // Update is called once per frame
    void Update()
    {
        //////HAVE TO GIVE A SECOND FOR PARTICLES TO SPAWN
        //if (Time.timeSinceLevelLoad > 2f && !off)
        //{
        //    //Debug.Log("DING");
        //    off = true;
        //    CreateRacer();
        //}

        //if (Time.timeSinceLevelLoad > 4.0f && !destroyed)
        //{
        //    ParticleSystem.MainModule newMain = particleSystem.main;
        //    newMain.gravityModifier = 3f;

        //    ParticleSystem.VelocityOverLifetimeModule newvel = particleSystem.velocityOverLifetime;
        //    newvel.enabled = true;

        //    destroyed = true;
        //}

        //if(Time.timeSinceLevelLoad > 8 && thing2)
        //{
        //    if(!pullback)
        //    {
        //        ParticleSystem.MainModule newMain = particleSystem.main;
        //        newMain.gravityModifier = 0f;

        //        ParticleSystem.VelocityOverLifetimeModule newvel = particleSystem.velocityOverLifetime;
        //        newvel.enabled = false;

        //        ParticleSystem.CollisionModule newCol = particleSystem.collision;
        //        newCol.enabled = false;

        //        particleSystem.GetParticles(blocks);

        //        pullback = true;
        //    }

        //    //lerp back
        //    LerpBack();
        //}

        //if (Time.timeSinceLevelLoad > 10 && thing)
        //{
        //    Debug.Log("sopt");
        //    MoveParticles();
        //    thing = false;
        //    particleSystem.SetParticles(blocks, size);
        //    thing2 = false;
        //}

        if (pullback)
        {
            if(returnTimer <= 0)
            {
                LerpBack();
                returnTimer = returnRate;
                //Debug.Log("Place");
            }

            returnTimer -= Time.deltaTime;
        }

    }

    public void CreateRacer()
    {
        pullback = false;
        destroyed = false;

        size = particleSystem.GetParticles(blocks);
        MoveParticles();
        particleSystem.SetParticles(blocks, size);

    }

    public void ResetRacer()
    {
        pullback = false;
        destroyed = false;

        size = particleSystem.GetParticles(blocks);
        for(int i = 0; i < blocks.Length; i++)
        {
            blocks[i].position = new Vector3(0, 0, 0);
        }

        particleSystem.SetParticles(blocks, size);
    }

    public void DestroyRacer(Vector3 deathVel)
    {
        if(!destroyed)
        {
            ParticleSystem.MainModule newMain = particleSystem.main;
            newMain.gravityModifier = 2f;

            ParticleSystem.VelocityOverLifetimeModule newvel = particleSystem.velocityOverLifetime;
            newvel.enabled = true;

            //ParticleSystem.MinMaxCurve xCurve = newvel.x;
            //xCurve.curveMin = deathVel.x;

            //add velocity based on velocity on death
            newvel.x = new ParticleSystem.MinMaxCurve(-100, 100 + deathVel.x); //= deathVel.x;
            newvel.y = new ParticleSystem.MinMaxCurve(-100, 100 + deathVel.y); //deathVel.y;
            newvel.z = new ParticleSystem.MinMaxCurve(-100, 100 + deathVel.z); //deathVel.z;

            ParticleSystem.CollisionModule newCol = particleSystem.collision;
            newCol.enabled = true;

            destroyed = true;
        }
    }

    public void PullBackRacer()
    {
        if(!pullback)
        {
            ParticleSystem.MainModule newMain = particleSystem.main;
            newMain.gravityModifier = 0f;

            ParticleSystem.VelocityOverLifetimeModule newvel = particleSystem.velocityOverLifetime;
            newvel.enabled = false;

            ParticleSystem.CollisionModule newCol = particleSystem.collision;
            newCol.enabled = false;

            //particleSystem.GetParticles(blocks);

            //not destroyed, start pulling back
            pullback = true;
            destroyed = false;

            //set timer of placing particles
            returnTimer = returnRate;

            //set all blocks to centre position
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i].position = new Vector3(0, 0, 0);
                blocks[i].rotation3D = startingRot[i];
            }

            particleSystem.SetParticles(blocks, size);

            //reset current number lerp is on
            currentReturner = 0;

            //randomise the order the particles are in
            for(int i = 0; i < blocks.Length; i++)
            {
                int randomNum = Random.Range(0, blocks.Length);
                ParticleSystem.Particle temp = blocks[i];
                //swap two random
                blocks[i] = blocks[randomNum];
                blocks[randomNum] = temp;

                Vector3 tempPos = startingPos[i];
                startingPos[i] = startingPos[randomNum];
                startingPos[randomNum] = tempPos;
            }
        }
    }

    //move the particles to the correct place
    void MoveParticles()
    {
        //place particles in system
        for (int i = 0; i < blocks.Length; i++)
        {
            //spacing of 45 between each
            blocks[i].position = txtPositions[i] * 25;
            //store in start pos
            startingPos[i] = blocks[i].position;
            startingRot[i] = blocks[i].rotation3D;

            //set block colour
            Color c = txtColours[i];
            blocks[i].startColor = c;

        }
    }

    void LerpBack()
    {
        //do X many at a time
        for(int i = 0; i < returnChunkAmount; i++)
        {
            if(currentReturner < blocks.Length)
            {
                blocks[currentReturner].velocity = new Vector3(0, 0, 0);
                blocks[currentReturner].position = startingPos[currentReturner]; //Vector3.Lerp(blocks[currentReturner].position, startingPos[currentReturner], 0.05f);

                currentReturner++;     
            }
        }

        particleSystem.SetParticles(blocks, size);

        //for (int i = 0; i < blocks.Length; i++)
        //{
        //    blocks[i].velocity = new Vector3(0, 0, 0);
        //    blocks[i].position = Vector3.Lerp(blocks[i].position, startingPos[i], 0.05f);

        //    float x = blocks[i].position.x;
        //    float y = blocks[i].position.y;
        //    float z = blocks[i].position.z;

        //    if (Mathf.Approximately(blocks[i].position.x, startingPos[i].x))
        //        x = startingPos[i].x;

        //    if (Mathf.Approximately(blocks[i].position.y, startingPos[i].y))
        //        y = startingPos[i].y;

        //    if (Mathf.Approximately(blocks[i].position.z, startingPos[i].z))
        //        z = startingPos[i].z;

        //    blocks[i].position = new Vector3(x, y, z);
        //}


    }
}

