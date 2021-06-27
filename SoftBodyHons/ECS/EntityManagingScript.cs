using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Mathematics;

public class EntityManagingScript : MonoBehaviour
{
    public GameObject objectsToEntities;
    public GameObject secondBody;
    EntityManager entityManager;

    //loading in objects
    public GameObject massPrefab;
    private bool loadedBody = false;
    private bool loadedSecondBody = false;
    private int massPointsCount = 0;

    //scene objects that need to be converted
    public GameObject stairs;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private void Start()
    {
        //creates entity manager
        entityManager = World.Active.GetOrCreateManager<EntityManager>();


        //LoadClothExample();
        LoadOneBigExample();
        //LoadOneHugeExample();
        //LoadTwoExample();


        StartLoadScene(1, false);
    }

    //REMEMBER AND UNCOMMENT THE PART IN FORCE SYSTEM
    void LoadClothExample()
    {
        ////cloth shape
        for (float y = -1f; y <= 1f; y += 0.2f)
        {
            for (float z = -1; z <= 0f; z += 0.2f)
            {
                //spawn a new mass point
                Instantiate(massPrefab, objectsToEntities.transform.TransformPoint(new Vector3(5, y, z)), Quaternion.identity, objectsToEntities.transform);
            }
        }

        //////cloth shape
        //for (float y = -1f; y <= 0f; y += 0.2f)
        //{
        //    for (float z = -1f; z <= 0f; z += 0.2f)
        //    {
        //        for (float x = 5; x <= 6; x += 0.2f)
        //        {
        //            //spawn a new mass point
        //            Instantiate(massPrefab, secondBody.transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, secondBody.transform);
        //        }
        //    }
        //}
    }

    void LoadTwoExample()
    {
        //spawn in loads of the base
        //create box shape
        for (float y = -0.5f; y <= 0.2f; y += 0.2f) // -1, 0
        {
            for (float z = -0.5f; z <= 0.2f; z += 0.2f) // -1, 0
            {
                for (float x = -0.5f; x <= 0.2f; x += 0.2f) //5.5, 6.5
                {
                    //spawn a new mass point
                    Instantiate(massPrefab, objectsToEntities.transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, objectsToEntities.transform);

                }
            }
        }

        //second body
        for (float y = 3.5f; y <= 4.2f; y += 0.2f) // -2.5, -1.5
        {
            for (float z = -0.5f; z <= 0.2f; z += 0.2f) // -1, 0.5
            {
                for (float x = -0.5f; x <= 0.2f; x += 0.2f) // 5.5, 6
                {
                    //spawn a new mass point
                    Instantiate(massPrefab, secondBody.transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, secondBody.transform);

                }
            }
        }
    }

    void LoadOneBigExample()
    {
        //spawn in loads of the base
        //create box shape
        for (float y = -1f; y <= 0f; y += 0.2f) // -1, 0
        {
            for (float z = -1f; z <= 0f; z += 0.2f) // -1, 0
            {
                for (float x = 5.5f; x <= 6.5f; x += 0.2f) //5.5, 6.5
                {
                    //spawn a new mass point
                    Instantiate(massPrefab, objectsToEntities.transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, objectsToEntities.transform);

                }
            }
        }
    }

    void LoadOneHugeExample()
    {
        //spawn in loads of the base
        //create box shape
        for (float y = -1f; y <= 0.5f; y += 0.2f) // -1, 0
        {
            for (float z = -1f; z <= 0.5f; z += 0.2f) // -1, 0
            {
                for (float x = 5.5f; x <= 7f; x += 0.2f) //5.5, 6.5
                {
                    //spawn a new mass point
                    Instantiate(massPrefab, objectsToEntities.transform.TransformPoint(new Vector3(x, y, z)), Quaternion.identity, objectsToEntities.transform);

                }
            }
        }
    }

    void StartLoadScene(int sceneIndex, bool unload)
    {
        //load
        if (!unload)
            StartCoroutine(LoadScene(sceneIndex));
        //unload
        else
            StartCoroutine(UnLoadScene(sceneIndex));
    }

    System.Collections.IEnumerator LoadScene(int index)
    {
        //load the entities scene
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        //newScene.allowSceneActivation = false;
        //loadScene = newScene;

        while (!loadScene.isDone)
        {
            yield return null;
        }

        //finished loading scene
        LoadEntities();
    }

    System.Collections.IEnumerator UnLoadScene(int index)
    {
        //unload the entities scene
        AsyncOperation loadScene = SceneManager.UnloadSceneAsync(index);
        //newScene.allowSceneActivation = false;
        //loadScene = newScene;

        while (!loadScene.isDone)
        {
            yield return null;
        }
    }

    void LoadEntities()
    {
        //get the scene
        Scene entityScene = SceneManager.GetSceneByBuildIndex(1);

        //does that load into this world?
        //load into other scene test
        if (entityScene.IsValid())
        {
            //move wanted objects to entities scene
            SceneManager.MoveGameObjectToScene(objectsToEntities, entityScene);

            //get the bounds off all the objects being moved
            List<Bounds> boundsList = new List<Bounds>();
            for(int i = 0; i < objectsToEntities.transform.childCount; i++)
            {
                boundsList.Add(objectsToEntities.transform.GetChild(i).GetComponent<Collider>().bounds);
            }

            //remove the entities from the parent
            objectsToEntities.transform.DetachChildren();

            //move base object back to scene
            SceneManager.MoveGameObjectToScene(objectsToEntities, SceneManager.GetSceneByBuildIndex(0));

            //convert everything in the entities scene to entities
            GameObjectConversionUtility.ConvertScene(entityScene, World.Active);

            //unload the entities scene now
            StartLoadScene(1, true);

            //attach the components we want to the bodies (if this is a body)
            if(!loadedBody)
            {
                loadedBody = true;

                AttachBodyComp(boundsList, 0);
                Debug.Log("first body");

                //load the second body now
                while (secondBody.transform.childCount > 0)
                {
                    secondBody.transform.GetChild(0).parent = objectsToEntities.transform;
                }

                //async load the scene
                StartLoadScene(1, false);
            }
            //static scene objects
            else if(!loadedSecondBody)
            {
                loadedSecondBody = true;

                AttachBodyComp(boundsList, 1);
                Debug.Log("second body");
                
                //load the scene now
                //deparent stairs
                while (stairs.transform.childCount > 0)
                {
                    //Debug.Log(x);
                    stairs.transform.GetChild(0).parent = objectsToEntities.transform;
                }

                //async load the scene
                StartLoadScene(1, false);
            }

            //SCENE IS LOADED LAST TO ENSURE THAT ALL MASS POINTS HAVE CORRECT IDS
            //load scene now
            else
            {
                AttachStaticComp(boundsList);
                Debug.Log("statics");
            }
        }
    }

    void AttachBodyComp(List<Bounds> boundsList, int bodyNum)
    {
        NativeArray<Entity> storedEntities = entityManager.GetAllEntities();

        float3 zeroFloat = new float3(0, 0, 0);

        for (int i = massPointsCount; i < storedEntities.Length; i++)
        {
            //add body
            entityManager.AddComponentData(storedEntities[i], new Body { mass = 1000, force = zeroFloat, accel = zeroFloat });
            //add velocity
            entityManager.AddComponentData(storedEntities[i], new Velocity { velocity = zeroFloat });
            //add collider
            entityManager.AddComponentData(storedEntities[i], new BoxCollider { size = new float3(boundsList[i - massPointsCount].extents * 0.7f) });
            //add IDs
            entityManager.AddComponentData(storedEntities[i], new ID { parentID = bodyNum, massID = i });
            //add fixed neighbours at start
            SetFixedNearestNeighbours(i, storedEntities, boundsList);
            //add neighbours (this makes me FeelsBadMan)
            entityManager.AddComponentData(storedEntities[i], new Neighbours
            {
                neighbour0 = -1,
                neighbour1 = -1,
                neighbour2 = -1,
                neighbour3 = -1,
                neighbour4 = -1,
                neighbour5 = -1,
                neighbour6 = -1,
                neighbour7 = -1,
                neighbour8 = -1,
                neighbour9 = -1,
                neighbour10 = -1,
                neighbour11 = -1,
                neighbour12 = -1,
                neighbour13 = -1,
                neighbour14 = -1,
                neighbour15 = -1,
                neighbour16 = -1,
                neighbour17 = -1,
                neighbour18 = -1,
                neighbour19 = -1,
                neighbour20 = -1,
                neighbour21 = -1,
                neighbour22 = -1,
                neighbour23 = -1,
                neighbour24 = -1,
                neighbour25 = -1,
                neighbour26 = -1,
                neighbour27 = -1,
                neighbour28 = -1,
                neighbour29 = -1,
                neighbour30 = -1,
                neighbour31 = -1,
                neighbour32 = -1,
                neighbour33 = -1,
                neighbour34 = -1,
                neighbour35 = -1,
                neighbour36 = -1,
                neighbour37 = -1,
                neighbour38 = -1,
                neighbour39 = -1,
                neighbour40 = -1,
                neighbour41 = -1,
                neighbour42 = -1,
                neighbour43 = -1,
                neighbour44 = -1,
                neighbour45 = -1,
                neighbour46 = -1,
                neighbour47 = -1,

            });
        }

        //store number of entities in this body
        massPointsCount = storedEntities.Length;
    }

    void AttachStaticComp(List<Bounds> boundsList)
    {
        NativeArray<Entity> storedEntities = entityManager.GetAllEntities();

        float3 zeroFloat = new float3(0, 0, 0);

        for (int i = massPointsCount; i < storedEntities.Length; i++)
        {
            //add colider adn stationary
            //entityManager.AddComponentData(storedEntities[i], new Body { mass = 1000, force = zeroFloat, accel = zeroFloat, velocity = zeroFloat, parentID = i});
            //RenderMesh mesh = entityManager.GetSharedComponentData(storedEntities[i], RenderMesh);
            entityManager.AddComponentData(storedEntities[i], new BoxCollider { size = new float3(boundsList[i - massPointsCount].extents) });
            entityManager.AddComponentData(storedEntities[i], new Stationary { });
            entityManager.AddComponentData(storedEntities[i], new Velocity { velocity = zeroFloat });
            //Slammed all these static objects into the same parent id DOESNT MATTER FOR NOW (mass id being -1 also means collisions aren't calculated)
            entityManager.AddComponentData(storedEntities[i], new ID { parentID = -1, massID = -1 });
        }

        //store number of entities total
        massPointsCount = storedEntities.Length;
    }

    int[] GetNearestNeighbours(int house, NativeArray<Entity> storedEntities, List<Bounds> boundsList)
    {
        //flat find 6 nearest points, no exclusivity lock or far point grab
        int[] neighbours = new int[48];

        //store all the distances from the house
        float[] allDist = new float[storedEntities.Length - massPointsCount];
        //don't include itself as neighbour
        allDist[house] = float.PositiveInfinity;

        for (int i = massPointsCount; i < storedEntities.Length; i++)
        {
            //don't include itself
            if (i != house)
                allDist[i] = Vector3.Distance(boundsList[house - massPointsCount].center, boundsList[i - massPointsCount].center);

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

    void SetFixedNearestNeighbours(int house, NativeArray<Entity> storedEntities, List<Bounds> boundsList)
    {
        //maximum neighbours for any point is 4
        int[] neighbours = new int[48];
        float[] rests = new float[48];
        int totalNeigh = 0;

        ////get the neighbours
        //neighbours = GetNearestNeighbours(house, storedEntities, boundsList);
        ////get the rests
        //for (int i = 0; i < neighbours.Length; i++)
        //{
        //    rests[i] = Vector3.Distance(boundsList[house - massPointsCount].center, boundsList[neighbours[i] - massPointsCount].center);

        //    totalNeigh++;
        //}

        //hybrid between closestpoints and nearest neighbours
        //loop through all mass points
        for (int i = massPointsCount; i < storedEntities.Length; i++)
        {
            //don't use itself
            if (house == i)
                continue;

            float dist = Vector3.Distance(boundsList[house - massPointsCount].center, boundsList[i - massPointsCount].center);
            //store ones close enough (should probs use nearest neighbours here too)
            if (dist <= 0.6f && totalNeigh < neighbours.Length)
            {
                //Debug.Log("dist from " + house + " : " + Vector3.Distance(massPoints[house].position, massPoints[i].position));
                neighbours[totalNeigh] = i;
                rests[totalNeigh] = dist;
                totalNeigh++;
            }
        }

        //add neighbours (this also makes me FeelsBadMan)
        entityManager.AddComponentData(storedEntities[house], new FixedNeighbours
        {
            totalNeighbours = totalNeigh,
            neighbour0 = neighbours[0],
            neighbour1 = neighbours[1],
            neighbour2 = neighbours[2],
            neighbour3 = neighbours[3],
            neighbour4 = neighbours[4],
            neighbour5 = neighbours[5],
            neighbour6 = neighbours[6],
            neighbour7 = neighbours[7],
            neighbour8 = neighbours[8],
            neighbour9 = neighbours[9],
            neighbour10 = neighbours[10],
            neighbour11 = neighbours[11],
            neighbour12 = neighbours[12],
            neighbour13 = neighbours[13],
            neighbour14 = neighbours[14],
            neighbour15 = neighbours[15],
            neighbour16 = neighbours[16],
            neighbour17 = neighbours[17],
            neighbour18 = neighbours[18],
            neighbour19 = neighbours[19],
            neighbour20 = neighbours[20],
            neighbour21 = neighbours[21],
            neighbour22 = neighbours[22],
            neighbour23 = neighbours[23],
            neighbour24 = neighbours[24],
            neighbour25 = neighbours[25],
            neighbour26 = neighbours[26],
            neighbour27 = neighbours[27],
            neighbour28 = neighbours[28],
            neighbour29 = neighbours[29],
            neighbour30 = neighbours[30],
            neighbour31 = neighbours[31],
            neighbour32 = neighbours[32],
            neighbour33 = neighbours[33],
            neighbour34 = neighbours[34],
            neighbour35 = neighbours[35],
            neighbour36 = neighbours[36],
            neighbour37 = neighbours[37],
            neighbour38 = neighbours[38],
            neighbour39 = neighbours[39],
            neighbour40 = neighbours[40],
            neighbour41 = neighbours[41],
            neighbour42 = neighbours[42],
            neighbour43 = neighbours[43],
            neighbour44 = neighbours[44],
            neighbour45 = neighbours[45],
            neighbour46 = neighbours[46],
            neighbour47 = neighbours[47],

            rest0 = rests[0],
            rest1 = rests[1],
            rest2 = rests[2],
            rest3 = rests[3],
            rest4 = rests[4],
            rest5 = rests[5],
            rest6 = rests[6],
            rest7 = rests[7],
            rest8 = rests[8],
            rest9 = rests[9],
            rest10 = rests[10],
            rest11 = rests[11],
            rest12 = rests[12],
            rest13 = rests[13],
            rest14 = rests[14],
            rest15 = rests[15],
            rest16 = rests[16],
            rest17 = rests[17],
            rest18 = rests[18],
            rest19 = rests[19],
            rest20 = rests[20],
            rest21 = rests[21],
            rest22 = rests[22],
            rest23 = rests[23],
            rest24 = rests[24],
            rest25 = rests[25],
            rest26 = rests[26],
            rest27 = rests[27],
            rest28 = rests[28],
            rest29 = rests[29],
            rest30 = rests[30],
            rest31 = rests[31],
            rest32 = rests[32],
            rest33 = rests[33],
            rest34 = rests[34],
            rest35 = rests[35],
            rest36 = rests[36],
            rest37 = rests[37],
            rest38 = rests[38],
            rest39 = rests[39],
            rest40 = rests[40],
            rest41 = rests[41],
            rest42 = rests[42],
            rest43 = rests[43],
            rest44 = rests[44],
            rest45 = rests[45],
            rest46 = rests[46],
            rest47 = rests[47],

        });
    }
}

//Debug.Log(loadScene.progress);

//var entities = new NativeArray<Entity>(1, Allocator.Temp);

//creates Entity's archetype
//EntityArchetype bodyArcheType = entityManager.CreateArchetype(typeof(Position), typeof(RenderMeshComponent));

//Entity body = entityManager.CreateEntity(bodyArcheType);
//entityManager.SetSharedComponentData(body, new RenderMeshComponent{m});
//Entity newEntity = entityManager.CreateEntity();
//GameObjectEntity.CopyAllComponentsToEntity(massObjPrefab, entityManager, newEntity);
//entityManager.Instantiate(massPrefab);

//loadScene.allowSceneActivation = true;
