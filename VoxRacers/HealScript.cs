using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealScript : MonoBehaviour
{
    [Header("Flight / Alive")]
    public float minOpenVelocity;
    private bool landed = false;
    private float timeAlive = 0;
    public float maxTimeAlive;
    public BoxCollider cubeCollider;
    private Rigidbody rb;

    [Header("Opening")]
    public BoxCollider healCollider;
    public float openingTime = 0.8f;
    private float openTimer = 0;
    private ParticleSystem particles;
    private ParticleSystem.Particle[] blocks;
    private int currentXPos = 0;
    private int currentYPos = 0;
    private int currentElement;
    private int size;
    public int width;
    public int height;

    [Header("Destroy")]
    //time after which has landed, destroy
    public float destroyTime;
    public float blockFallSpeed;
    public float blocksLeftTime;
    private bool destroyed;
    public bool startLine = false;

    // Use this for initialization
    void Start ()
    {
        rb = GetComponent<Rigidbody>();

        particles = GetComponent<ParticleSystem>();

        blocks = new ParticleSystem.Particle[particles.main.maxParticles];

        //alive time to destroy
        timeAlive = Time.timeSinceLevelLoad;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(landed && !destroyed)
        {
            //move position
            if (currentXPos < width)
                currentXPos++;

            //go upwards
            else
            {
                if(currentYPos < height)
                    currentYPos++;

                //reach max height, go back in
                else if(currentXPos > 0)
                {
                    width = 0;
                    currentXPos--;                  
                }
            }

            //place particles
            if (currentElement + 1 < blocks.Length)
            {
                //move block to area
                blocks[currentElement].position = new Vector3(currentXPos, currentYPos, 0);
                //mirrored
                blocks[currentElement + 1].position = new Vector3(-currentXPos, currentYPos, 0);

                //update current element
                currentElement += 2;
            }

            //move particles
            particles.SetParticles(blocks, size);

            //ping timer to see if collider should be activated
            openTimer += Time.deltaTime;

            if (openTimer >= openingTime)
                healCollider.enabled = true;
        }

        //Call to destroy
        if (Time.timeSinceLevelLoad - timeAlive >= maxTimeAlive && !destroyed)
            DestroyHeal();
    }

    void OnCollisionStay(Collision col)
    {
        //check that the cube has slowed down
        if (rb.velocity.magnitude <= minOpenVelocity && !landed)
            OpenHeal();
    }

    void OnTriggerEnter(Collider other)
    {
        //perform heal on player
        if (other.CompareTag("racer"))
        {
            //heal racer
            other.GetComponent<RacerScript>().IncreaseHP(100);

            DestroyHeal();
        }
    }

    void OpenHeal()
    {
        //freeze position (except y)
        rb.constraints = RigidbodyConstraints.FreezeAll;//RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        //stop collisions
        cubeCollider.enabled = false;

        //set to opened
        landed = true;

        size = particles.GetParticles(blocks);

        //force destroy after fixed time
        timeAlive = Time.timeSinceLevelLoad - maxTimeAlive + destroyTime;
    }

    void DestroyHeal()
    {
        if(!startLine)
        {
            destroyed = true;

            //turn off collider
            healCollider.enabled = false;

            ParticleSystem.MainModule newMain = particles.main;
            newMain.gravityModifier = blockFallSpeed;

            //add velocity over time
            ParticleSystem.VelocityOverLifetimeModule newVel = particles.velocityOverLifetime;
            newVel.enabled = true;

            //turn on collision
            ParticleSystem.CollisionModule newCol = particles.collision;
            newCol.enabled = true;


            //destroy object after a couple seconds
            Destroy(gameObject, blocksLeftTime);
        }
    }
}
