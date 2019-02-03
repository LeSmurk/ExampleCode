using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    [Header("Flight / Alive")]
    public float minOpenVelocity;
    private bool landed = false;
    private float timeAlive = 0;
    public float maxTimeAlive;
    public BoxCollider cubeCollider;
    private Rigidbody rb;

    [Header("Opening")]
    private float timer = 0;
    public BoxCollider shieldCollider;
    private ParticleSystem particles;
    //block particles
    //IF YOU WANT TO CHANGE SIZE, LOOK AT PARTICLE EMITTER MAX PARTICLES AND EMISSION
    private ParticleSystem.Particle[] blocks;
    private int currentPosX = 1;
    private int currentPosY = 1;
    private int currentElement = 0;
    private int blockArraySize = 0;
    public float blockSpawnRate;

    [Header("Destroy")]
    public int maxHealth;
    private int totalHealth;
    public int healthLoss;
    private bool destroyed = false;
    //time after which has landed, destroy
    public float destroyTime;
    public float blockFallSpeed;
    //time for which particles will stay before whole object is destroyed
    public float blocksLeftTime;


	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();

        particles = GetComponent<ParticleSystem>();

        //alive time to destroy
        timeAlive = Time.timeSinceLevelLoad;

        //init HP
        totalHealth = maxHealth;

        blocks = new ParticleSystem.Particle[particles.main.maxParticles];

	}
	
	// Update is called once per frame
	void Update ()
    {
        //spawn blocks if landed
        if(landed && !destroyed)
        {
            timer += Time.deltaTime;

            //spawn blocks in row at a certain rate
            if (timer >= blockSpawnRate)
            {
                //reset timer
                timer = 0;

                //loop move blocks
                for (int y = 0; y <= currentPosY; y++)
                {
                    for (int x = currentPosX; x >= (currentPosX - y); x--)
                    {
                        //dont spawn one at any position lower than the current x, unless it is top row
                        if ((x < currentPosX && (y - currentPosX) < 0) || x == 0)
                            break;

                        //Move particles
                        if (currentElement + 1 < blocks.Length)
                        {

                            //move block to area
                            blocks[currentElement].position = new Vector3(x, y, 0);
                            //mirrored
                            blocks[currentElement + 1].position = new Vector3(-x, y, 0);

                            //update current element
                            currentElement += 2;

                        }
                    }
                }

                //spawn Centre particle
                if (currentElement < blocks.Length)
                {
                    //spawn centre one
                    blocks[currentElement].position = new Vector3(0, currentPosY, 0);

                    //update current element
                    currentElement++;
                    //set particles array
                    particles.SetParticles(blocks, blockArraySize);

                    //update position
                    currentPosX++;
                    currentPosY++;
                }
            }
        }

        //too little hp
        if (totalHealth <= 0 && !destroyed)
            DestroyShield();

        //Call to destroy
        if (Time.timeSinceLevelLoad - timeAlive >= maxTimeAlive && !destroyed)
            DestroyShield();
	}

    void OnCollisionStay(Collision col)
    {
        //check that the cube has slowed down
        if (rb.velocity.magnitude <= minOpenVelocity && !landed)
            OpenShield();
    }

    void OpenShield()
    {
        //freeze position (except y)
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        //stop collisions
        cubeCollider.enabled = false;

        //set to opened
        landed = true;

        //init timer to start spawning
        timer = blockSpawnRate;

        //force destroy after fixed time
        timeAlive = Time.timeSinceLevelLoad - maxTimeAlive + destroyTime;

        //store particle array
        blockArraySize = particles.GetParticles(blocks);

        //FMODStuff
        FMODUnity.RuntimeManager.PlayOneShot("event:/Pickups/Shield/ShieldImpact", transform.position);

    }

    void OnDisable()
    {

    }

    void DestroyShield()
    {
        Debug.Log("destroy");
        destroyed = true;

        //turn off collider
        shieldCollider.enabled = false;

        FMODUnity.RuntimeManager.PlayOneShot("event:/Pickups/Shield/ShieldClose", transform.position);

        particles.Play();

        //prevent more spawning
        ParticleSystem.EmissionModule newEmis = particles.emission;
        newEmis.enabled = false;

        //add velocity over time
        ParticleSystem.VelocityOverLifetimeModule newVel = particles.velocityOverLifetime;
        newVel.enabled = true;

        ////rotate
        //ParticleSystem.RotationBySpeedModule newRot = particles.rotationBySpeed;
        //newRot.enabled = true;

        //turn on gravity for particles
        ParticleSystem.MainModule newMain = particles.main;
        newMain.gravityModifier = blockFallSpeed;
        
        //set simulation speed back to normal
        newMain.simulationSpeed = 1;

        //turn on collision
        ParticleSystem.CollisionModule newCol = particles.collision;
        newCol.enabled = true;


        //destroy object after a couple seconds
        Destroy(gameObject, blocksLeftTime);
    }

    //trigger collisions for players and projectiles
    void OnCollisionEnter(Collision col)
    {
        if(col.collider.CompareTag("Projectile"))
        {
            //play sound

            //shield lose some hp
            totalHealth -= healthLoss;

            //only update the particles if not destroyed
            if(totalHealth >= healthLoss && !destroyed)
            {
                //ParticleSystem.Particle[] parts = new ParticleSystem.Particle[particles.main.maxParticles];
                //int totalNum = particles.GetParticles(parts);


                //change the colour of particles in crack
                for (int i = totalHealth; i < blockArraySize; i += (maxHealth))
                {
                    blocks[i].startColor = new Color(0, 0, 0, 0);
                }

                //set the particles
                particles.SetParticles(blocks, blockArraySize);
            }        

        }
    }
}
