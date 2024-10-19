using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = System.Object;

public class CollisionManager : MonoBehaviour
{

    int collisionMask;

    SkinnedMeshRenderer rend;
    bool invincible = false;
    [SerializeField]
    float blinkRate = 0.2f, blinkDuration = 2f;

    //get a reference to our player robot
    GameObject player;
    CollisionSphere[] collisionSpheres;

    Vector3[] slideSpheres; //position of collision spheres during slide
    Animator playerAnim;
    int slideCurveParameter; 

        // Start is called before the first frame update
    void Start()
    {
        collisionMask = GetLayerMask((int)Layer.Obstacle);

        player = GameObject.Find("Robot");
        rend = player.GetComponentInChildren<SkinnedMeshRenderer>();
        playerAnim = player.GetComponent<Animator>();
        slideCurveParameter = Animator.StringToHash("SlideCurve");

        //import all sphere colliders from player into a local variable
        SphereCollider[] colliders = player.GetComponents<SphereCollider>();

        //math the amount of collision spheres to the sphere colliders made in the scene
        collisionSpheres = new CollisionSphere[colliders.Length];

        //create new collision spheres for each sphere collider made in scene
        for (int i = 0; i < collisionSpheres.Length; i++)
        {
            collisionSpheres[i] = new CollisionSphere(colliders[i].center, colliders[i].radius);
        }

        //collisionSpheres is the array of collisionSpheres objects
        //the second parameter is how the array is sorted
        //since the collisionSpheres struct also implements its own way to compare itself
        //It is used twice here
        //array.sort needs what it needs to sort and how it sorts it
        Array.Sort(collisionSpheres, new CollisionSphereCompare());

        slideSpheres = new Vector3[collisionSpheres.Length];
        slideSpheres[0] = new Vector3(0f, 0.2f, 0.75f);
        slideSpheres[1] = new Vector3(0f, 0.25f, 0.25f);
        slideSpheres[2] = new Vector3(0f, 0.55f, -0.15f);
        slideSpheres[3] = new Vector3(0.4f, 0.7f, -0.28f);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //store a local list of everthing that has collided with our player
        List<Collider> Collisions = new List<Collider>();

        for (int i = 0; i < collisionSpheres.Length; i++)
        {
            //Get the vector direction of the collision sphere so it moves to slide position
            Vector3 slideDisplacement = slideSpheres[i] - collisionSpheres[i].offset;

            slideDisplacement *= playerAnim.GetFloat(slideCurveParameter);

            //apply to slide displacement to the collisionSphere's position
            Vector3 offset = collisionSpheres[i].offset + slideDisplacement;
            
            //if our CollisionSpheres overlaps any obstacle found on the collisionMask layer in Unity
            //physics.overlapSPhere returns an array of colliders that overlap a position and size on a given layer
            Collider[] allCollisions = Physics.OverlapSphere(player.transform.position + offset,
                collisionSpheres[i].radius, collisionMask);  
            foreach (Collider c in allCollisions)
            {
                Collisions.Add(c); 
            }
        }
        if (Collisions.Count > 0)
        {
            Debug.Log("Collided with gameobject: {collisions[0].name} -- WHOOPS");
            ObstacleCollision();
        }
    }
    public void ObstacleCollision()
    {
        if (invincible == false)
        {
            invincible = true;
            StartCoroutine(BlinkPlayer());
        }
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < collisionSpheres.Length; i++)
        {
            //Get the vector direction of the collision sphere so it moves to slide position
            Vector3 slideDisplacement = slideSpheres[i] - collisionSpheres[i].offset;

            slideDisplacement *= playerAnim.GetFloat(slideCurveParameter);

            //apply to slide displacement to the collisionSphere's position
            Vector3 offset = collisionSpheres[i].offset + slideDisplacement;

            //if our CollisionSpheres overlaps any obstacle found on the collisionMask layer in Unity
            //physics.overlapSPhere returns an array of colliders that overlap a position and size on a given layer
            Collider[] allCollisions = Physics.OverlapSphere(player.transform.position + offset,
                collisionSpheres[i].radius, collisionMask);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(player.transform.position + offset, collisionSpheres[i].radius);
        }
    }

    IEnumerator BlinkPlayer()
    {
        float startTime = Time.time; // he furst  time the coroutine start
        while(invincible){
            rend.enabled = !rend.enabled;
            if (Time.time >= startTime + blinkDuration)
            {
                invincible = false;
                rend.enabled = true;
            }
            yield return new WaitForSeconds(blinkRate);
        }
    }

    //get the layer at a certain index using Left Bit Shifting
    int GetLayerMask(params int[] indices)
    {
        //by defult, no layer is turned on
        int layerMask = 0;

        //for every integer inside indices, add to the mask
        for (int i = 0; i < indices.Length; i++)
        {
            layerMask |= 1 << indices[i];
        }

        //shift the value of the bit by index amount of times
        return layerMask;
    }

    public struct CollisionSphere
    {
        //locate or origin of the sphere
        public Vector3 offset;
        //size of sphere
        public float radius;

        public CollisionSphere(Vector3 offset, float radius)
        {
            this.offset = offset;
            this.radius = radius;
        }

        public static bool operator >(CollisionSphere lhs, CollisionSphere rhs)
        {
            return lhs.offset.y > rhs.offset.y;
        }

        public static bool operator <(CollisionSphere lhs, CollisionSphere rhs)
        {
            return lhs.offset.y < rhs.offset.y;
        }
    }
    public struct CollisionSphereCompare : IComparer
    {
        public int Compare(Object x, Object y)
        {
            //check to see if both objects are collision spheres before comparing them
            if (!(x is CollisionSphere) || !(y is CollisionSphere))
            {
                Debug.Log(Environment.StackTrace); //how did we get to this error
                //the error exception message that will appear with this error
                throw new Exception("Can not compare collision spheres to non collision spheres");
            }
            //convert object into collision spheres for comparisons 
            CollisionSphere lhs = (CollisionSphere)x;
            CollisionSphere rhs = (CollisionSphere)y;

            if (lhs > rhs)
            {
                return 1; //positive one means the value is greater in the sort
            }
            else if (lhs < rhs)
            {
                return -1; //negetive one means the value is less in the sort
            }
            else
            {
                return 0; //0 means the balue is the same in the sort
            }
        }
    }
}
