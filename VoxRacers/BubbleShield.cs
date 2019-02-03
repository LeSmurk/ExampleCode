using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleShield : MonoBehaviour
{
    [Header("Constructing")]
    public float scaleSpeed;
    public float maxScale = 2f;
    private float scaletimer = 0;
    private float startingTimer;

    public int height = 5;
    public int width = 5;
    public int depth = 5;
    public ParticleSystem particles;
    private ParticleSystem.Particle[] blocks;
    private int size;
    private int element = 0;

    public Collider shieldCollider;

    [Header("Destroying")]
    public float aliveTime;
    private bool alive = false;
    public float destroyTime = 1f;

	// Use this for initialization
	void Start ()
    {
        blocks = new ParticleSystem.Particle[particles.main.maxParticles];

        //only spawn shield after particles are actuallly spawned
        startingTimer = 0.1f;

    }

    void SpawnShield()
    {
        alive = true;

        size = particles.GetParticles(blocks);

        //position particles
        for (int y = -height; y <= height; y++)
        {
            for (int z = -depth; z <= depth; z++)
            {
                for (int x = -width; x <= width; x++)
                {
                    //hollow cube (only place at extremities)
                    if (z == -depth || z == depth || y == -height || y == height || x == -width || x == width)
                    {
                        //position particle at position
                        blocks[element].position = new Vector3(x, y, z);

                        //increment array
                        element++;
                    }
                }
            }
        }

        particles.SetParticles(blocks, size);

        //pause shield until we want to destroy it
        particles.Pause();
    }

    // Update is called once per frame
    void Update ()
    {
        //only spawn shield after a small delay
        if(!alive)
        {
            startingTimer -= Time.deltaTime;
            if (startingTimer <= 0)
                SpawnShield();
        }

        if(alive)
        {
            scaletimer += Time.deltaTime;

            //scale up whole transform
            if(scaletimer <= maxScale)
            {
                transform.localScale = new Vector3(transform.localScale.x + scaletimer * scaleSpeed, transform.localScale.y + scaletimer * scaleSpeed, transform.localScale.z + scaletimer * scaleSpeed);
            }

            else
            {
                //stay for a period of time them go pop
                //shimmer?

                if(scaletimer >= aliveTime)
                {
                    particles.Play();

                    //turn on size over lifetime
                    ParticleSystem.SizeOverLifetimeModule newSize = particles.sizeOverLifetime;
                    newSize.enabled = true;

                    //turn on coolour over lifetime
                    ParticleSystem.ColorOverLifetimeModule newCol = particles.colorOverLifetime;
                    newCol.enabled = true;

                    //TURN OFF COLLIDER
                    shieldCollider.enabled = false;

                    Destroy(gameObject, destroyTime);
                }
            }
        }

	}
}
