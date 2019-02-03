using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactScript : MonoBehaviour
{
    public float maxAliveTime = 10;
    private float timer = 0;
    private bool exploded = false;
    public float colliderEnableTime = 0.6f;
    private float colliderTimer;
    public SphereCollider triggerCollider;
    public ParticleSystem explosionEffect;
    public ParticleSystem smokeEffect;
    public ParticleSystem waveEffect;

    [Header("Wave colour")]
    public Material[] waveMaterials = new Material[8];

    // Use this for initialization
    void Start ()
    {
        timer = maxAliveTime;

        //set wave colour to whatever racer this is
        if (transform.parent != null)
            waveEffect.GetComponent<ParticleSystemRenderer>().material = waveMaterials[int.Parse(transform.root.name[6].ToString())];
	}
	
	// Update is called once per frame
	void Update ()
    {
        timer -= Time.deltaTime;

        if (timer <= 0 && !exploded)
            Explode();

        //enable collider after delay
        if(colliderTimer <= colliderEnableTime && exploded)
        {
            colliderTimer -= Time.deltaTime;

            if(colliderTimer <= 0)
                triggerCollider.enabled = true;
        }
    }

    void Explode()
    {
        exploded = true;

        FMODUnity.RuntimeManager.PlayOneShot("event:/Pickups/Grenade/GrenadePulse", transform.position);
        
        //turn on collider after a delay
        colliderTimer = colliderEnableTime;

        //particles
        explosionEffect.Play();
        smokeEffect.Play();
        waveEffect.Play();


        //destroy this object after 1 secs
        Destroy(gameObject, 2);
    }

    //trigger damage player
    void OnTriggerEnter(Collider other)
    {
        //check if player
        if(other.CompareTag("racer"))
        {
            //take damage hit
            other.GetComponent<RacerScript>().DecreaseHP(100);
        }

        //should just be able to put this here?
        //determine what the bullet hit
        if (other.gameObject.tag == "Destructible")
        {
            //destroy the destructible
            Destroy(other.gameObject);

            //play blocks breaking particles
            GameObject blocksEffect = (GameObject)Instantiate(Resources.Load("Prefabs/Effects/Breaking Rocks Effect"), transform.position, Quaternion.identity);
        }
    }

    //impact
    void OnCollisionEnter(Collision col)
    {
        if (!exploded)
            Explode();
    }
}
