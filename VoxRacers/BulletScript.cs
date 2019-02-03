using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    //what type of projectile
    public string type;

    [Header("Bullet")]
    public float deathTime;

    [Header("Hook End")]
    public GameObject grabbedItem;
    public Rigidbody Player;
    public float pullForce;
    public float dragForce;

	// Use this for initialization
	void Awake ()
    {
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        switch (type)
        {
            case ("hook"):
                HookUpdate();
                break;
        }
    }

    public void InitBullet()
    {
        type = "bullet";
    }

    public void InitHook()
    {
        type = "hook";
    }

    void OnCollisionEnter(Collision collision)
    {
        switch (type)
        {
            case ("bullet"):
                BulletCollision(collision);
                break;

            case ("hook"):
                HookCollision(collision);
                break;

            case (""):
                break;
        }
    }

    void BulletCollision(Collision col)
    {
        //play explosion effect
        GameObject explosion = transform.Find("Explosion particles").gameObject;
        explosion.GetComponent<ParticleSystem>().Play();
        FMODUnity.RuntimeManager.PlayOneShot("event:/Pickups/Bullet/BulletImpact", transform.position);

        //fix the transform position
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        //turn off the bullet's renderer
        GetComponent<MeshRenderer>().enabled = false;

        //determine what the bullet hit
        if (col.collider.gameObject.tag == "Destructible")
        {
            //destroy the destructible
            Destroy(col.collider.gameObject);

            //play blocks breaking particles
            GameObject blocksEffect = (GameObject)Instantiate(Resources.Load("Prefabs/Effects/Breaking Rocks Effect"), transform.position, Quaternion.identity);
        }

        //deeestroy this object after time
        Destroy(gameObject, deathTime);
    }

    void HookCollision(Collision col)
    {
        //detect hitting player
        if(col.collider.gameObject.tag == "racer")
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Pickups/Hook/HookLatch", transform.position);

            //apply pull back
            grabbedItem = col.collider.gameObject;
            //position of thrower
            Player = transform.root.GetComponentInChildren<Rigidbody>();

            //lock this transform to the other transfrom
            //fix this hook to the hit object
            GetComponent<Rigidbody>().isKinematic = true;
            transform.parent = grabbedItem.transform;
            //turn off this collision
            GetComponent<Collider>().enabled = false;
        }
    }

    ////extra trigger hitbox for hook
    //void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject.tag == "racer")
    //    {
    //        FMODUnity.RuntimeManager.PlayOneShot("event:/Pickups/Hook/HookLatch", transform.position);

    //        //apply pull back
    //        grabbedItem = other.gameObject;
    //        //position of thrower
    //        Player = transform.root.GetComponentInChildren<Rigidbody>();

    //        //lock this transform to the other transfrom
    //        //fix this hook to the hit object
    //        GetComponent<Rigidbody>().isKinematic = true;
    //        transform.parent = grabbedItem.transform;
    //        //turn off this collision
    //        GetComponent<Collider>().enabled = false;

    //        //snap this position to the actual position wanted
    //        transform.position = other.transform.position;
    //    }
    //}

    void HookUpdate()
    {
        if(grabbedItem != null)
        {
            //addforce to drag hit back to player
            grabbedItem.GetComponent<Rigidbody>().AddForce((Player.transform.position - grabbedItem.transform.position) * dragForce, ForceMode.Impulse);

            //create position to the side of grabbed item (MIGHT NEED INCREASING OR DECREASING SO ADD FLOAT HERE)
            //Vector3 position = grabbedItem.transform.position + Vector3.up;
            //drag base to here
            Player.AddForce((transform.position - Player.transform.position) * pullForce, ForceMode.Impulse);

        }
    }
}
