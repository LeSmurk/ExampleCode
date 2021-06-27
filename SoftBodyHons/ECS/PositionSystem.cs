using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

//position system checks the velocities of bodies and moves them (checking for colisions)

public class PositionSystem : JobComponentSystem
{
    //protected struct MovingCollider
    //{
    //    //look at component group instead of data array
    //    public ComponentDataArray<Position> objectPos;
    //    public ComponentDataArray<BoxCollider> colliderInfo;
    //    public ComponentDataArray<Body> objectBody;
    //    public readonly int Length;
    //}

    //protected struct StationaryCollider
    //{
    //    public ComponentDataArray<Position> objectPos;
    //    public ComponentDataArray<BoxCollider> colliderInfo;
    //    public ComponentDataArray<Stationary> colliderStationary;
    //    public readonly int Length;
    //}

    //[Inject] protected MovingCollider movingGroup;
    //[Inject] protected StationaryCollider stationaryGroup;

    //[BurstCompile]
    //struct MovingCollisionsJob : IJobParallelFor
    //{

    //    //public ComponentDataArray<Body> objectBody;
    //    //[ReadOnly]
    //    //public ComponentDataArray<Position> objectPos;
    //    //[ReadOnly]
    //    //public ComponentDataArray<BoxCollider> colliderInfo;
    //    //[NativeDisableParallelForRestriction]

    //    float3 CompareCollisions(float3 pos1, float3 pos2, BoxCollider col1, BoxCollider col2)
    //    {
    //        float3 collisionNormal = new float3(-100, -100, -100);

    //        float3 max1 = AABBMax(pos1, col1);
    //        float3 min1 = AABBMin(pos1, col1);
    //        float3 max2 = AABBMax(pos2, col2);
    //        float3 min2 = AABBMin(pos2, col2);

    //        bool isColliding = false;

    //        if (max1.x >= min2.x && min1.x < max2.x)
    //        {

    //            if (max1.y >= min2.y && min1.y < max2.y)
    //            {
    //                if (max1.z >= min2.z && min1.z < max2.z)
    //                {
    //                    //COLLIDING
    //                    isColliding = true;
    //                }
    //            }
    //        }

    //        if (isColliding)
    //        {
    //            //create normal vector based on point and position of ball
    //            float3 colPoint = ClosestPoint(max2, min2, pos1);
    //            Vector3 dir = pos1 - colPoint;
    //            dir = dir.normalized;

    //            collisionNormal = dir;
    //        }

    //        return collisionNormal;
    //    }

    //    float3 ClosestPoint(float3 Max, float3 Min, float3 pos)
    //    {
    //        float3 point;

    //        if (pos.x > Max.x)
    //            point.x = Max.x;
    //        else if (pos.x < Min.x)
    //            point.x = Min.x;
    //        else
    //            point.x = pos.x;
    //        //yclose
    //        if (pos.y > Max.y)
    //            point.y = Max.y;
    //        else if (pos.y < Min.y)
    //            point.y = Min.y;
    //        else
    //            point.y = pos.y;
    //        //zclose
    //        if (pos.z > Max.z)
    //            point.z = Max.z;
    //        else if (pos.z < Min.z)
    //            point.z = Min.z;
    //        else
    //            point.z = pos.z;

    //        return point;
    //    }

    //    float3 AABBMax(float3 pos, BoxCollider col)
    //    {
    //        return new float3(pos.x + col.size.x, pos.y + col.size.y, pos.z + col.size.z);
    //    }

    //    float3 AABBMin(float3 pos, BoxCollider col)
    //    {
    //        return new float3(pos.x - col.size.x, pos.y - col.size.y, pos.z - col.size.z);
    //    }

    //    public void Execute(int checkElem)
    //    {
    //        List<float3> collidingNormals = new List<float3>();
    //        Body tempBody = objectBody[checkElem];

    //        //Check for collisions
    //        for (int i = 0; i < objectPos.Length; i++)
    //        {
    //            //don't compare things that belong to the same object (stored in body id)
    //            if (objectBody[checkElem].parentID != objectBody[i].parentID)
    //            {
    //                //Collision detected
    //                float3 newCol = CompareCollisions(objectPos[checkElem].Value, objectPos[i].Value, colliderInfo[checkElem], colliderInfo[i]);
    //                if (newCol.x != -100)
    //                    collidingNormals.Add(newCol);
    //            }
    //        }

    //        //if colliding with any normals, remove the component that moves in that direction
    //        for (int x = 0; x < collidingNormals.Count; x++)
    //        {
    //            float dotProd = Vector3.Dot(objectBody[checkElem].velocity, collidingNormals[x]);

    //            //only remove in direction we want
    //            if (dotProd < 0)
    //                tempBody.velocity -= collidingNormals[x] * dotProd;

    //        }

    //        //set new velocity of body with components removed
    //        objectBody[checkElem] = tempBody;
    //    }
    //}

    protected struct ColliderGroup
    {
        public ComponentDataArray<Velocity> colVel;
        public readonly ComponentDataArray<Position> colPos;
        public readonly ComponentDataArray<BoxCollider> colInfo;
        public readonly ComponentDataArray<ID> colID;
        public readonly int Length;
    }

    protected struct PositionGroup
    {
        [NativeDisableParallelForRestriction]
        public ComponentDataArray<Position> objPos;
        [ReadOnly]
        public ComponentDataArray<Velocity> objVel;
        //public SubtractiveComponent<Stationary> objStationary;
        public readonly int Length;
    }

    public struct CollisionJob : IJobParallelFor
    {
        public ComponentDataArray<Velocity> colliderVel;
        [ReadOnly]
        public ComponentDataArray<Position> colliderPos;
        [ReadOnly]
        public ComponentDataArray<BoxCollider> colliderInfo;
        [ReadOnly]
        public ComponentDataArray<ID> colliderID;

        float3 CompareCollisions(float3 pos1, float3 pos2, BoxCollider col1, BoxCollider col2)
        {
            float3 collisionNormal = new float3(-100, -100, -100);

            float3 max1 = AABBMax(pos1, col1);
            float3 min1 = AABBMin(pos1, col1);
            float3 max2 = AABBMax(pos2, col2);
            float3 min2 = AABBMin(pos2, col2);

            bool isColliding = false;

            if (max1.x >= min2.x && min1.x < max2.x)
            {

                if (max1.y >= min2.y && min1.y < max2.y)
                {
                    if (max1.z >= min2.z && min1.z < max2.z)
                    {
                        //COLLIDING
                        isColliding = true;
                    }
                }
            }

            if (isColliding)
            {
                //create normal vector based on point and position of ball
                float3 colPoint = ClosestPoint(max2, min2, pos1);
                Vector3 dir = pos1 - colPoint;
                dir = dir.normalized;

                collisionNormal = dir;
            }

            return collisionNormal;
        }

        float3 ClosestPoint(float3 Max, float3 Min, float3 pos)
        {
            float3 point;

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

        float3 AABBMax(float3 pos, BoxCollider col)
        {
            return new float3(pos.x + col.size.x, pos.y + col.size.y, pos.z + col.size.z);
        }

        float3 AABBMin(float3 pos, BoxCollider col)
        {
            return new float3(pos.x - col.size.x, pos.y - col.size.y, pos.z - col.size.z);
        }

        public void Execute(int checkElem)
        {
            //only execute this if an actual mass point
            if (colliderID[checkElem].massID == -1)
                return;

            List<float3> collidingNormals = new List<float3>();
            //List<float3> collidingVels = new List<float3>();
            Velocity tempVel = colliderVel[checkElem];

            //compare all colliders to all others
            for (int i = 0; i < colliderPos.Length; i++)
            {
                //don't compare things that belong to the same object (stored in body id)
                if (colliderID[checkElem].parentID != colliderID[i].parentID)
                {
                    //Collision detected
                    float3 newCol = CompareCollisions(colliderPos[checkElem].Value, colliderPos[i].Value, colliderInfo[checkElem], colliderInfo[i]);
                    if (newCol.x != -100)
                    {
                        collidingNormals.Add(newCol);
                        //collidingVels.Add(colliderVel[i].velocity);
                    }
                }
            }

            //if colliding with any normals, remove the component that moves in that direction
            for (int x = 0; x < collidingNormals.Count; x++)
            {
                //add velocity of colliding body
                //tempVel.velocity += collidingVels[x];

                float dotProd = Vector3.Dot(colliderVel[checkElem].velocity, collidingNormals[x]);

                //only remove in direction we want
                if (dotProd < 0)
                    tempVel.velocity -= collidingNormals[x] * dotProd;

            }

            //set new velocity of body with components removed
            colliderVel[checkElem] = tempVel;
        }
    }

    struct PositionJob : IJobParallelFor
    {
        public ComponentDataArray<Position> objectPos;
        [ReadOnly]
        public ComponentDataArray<Velocity> objectVel;

        public void Execute(int index)
        {
            Position newPos = objectPos[index];

            //remove portions of velocities if collision detected
            //CheckCollisions(index);

            //move to position
            newPos.Value += objectVel[index].velocity;

            //newPos.Value += target * Time.deltaTime;
            objectPos[index] = newPos;
        }

    }

    [Inject] protected ColliderGroup colliderGroup;
    [Inject] protected PositionGroup positionGroup;

    
    protected override JobHandle OnUpdate(JobHandle handle)
    {
        CollisionJob fJob = new CollisionJob
        {
            colliderVel = colliderGroup.colVel,
            colliderPos = colliderGroup.colPos,
            colliderInfo = colliderGroup.colInfo,
            colliderID = colliderGroup.colID,
        };

        handle = fJob.Schedule(colliderGroup.Length, 1, handle);

        PositionJob posJob = new PositionJob
        {
            objectPos = positionGroup.objPos,
            objectVel = positionGroup.objVel,
        };
        
        return posJob.Schedule(positionGroup.Length, 1, handle);
    }
    

    //protected override void OnUpdate()
    //{
    //    //PositionJob pJob = new PositionJob
    //    //{
    //    //    movingGroup = movingGp,
    //    //    stationaryGroup = stationaryGp,
    //    //};

    //    ////loop through moving objects, changing positions accordingly
    //    //for (int i = 0; i < movingGroup.Length; i++)
    //    //{
    //    //    Position newPos = movingGroup.objectPos[i];

    //    //    //remove portions of velocities if collision detected
    //    //    CheckCollisions(i);

    //    //    //move to position
    //    //    newPos.Value += movingGroup.objectBody[i].velocity;

    //    //    //newPos.Value += target * Time.deltaTime;
    //    //    movingGroup.objectPos[i] = newPos;
    //    //}

    //    //return pJob.Schedule(movingGp.Length, 1, handle);

    //    //loop through moving objects, changing positions accordingly
    //    for (int i = 0; i < movingGroup.Length; i++)
    //    {
    //        Position newPos = movingGroup.objectPos[i];

    //        //remove portions of velocities if collision detected
    //        CheckCollisions(i);

    //        //move to position
    //        newPos.Value += movingGroup.objectBody[i].velocity;

    //        //newPos.Value += target * Time.deltaTime;
    //        movingGroup.objectPos[i] = newPos;
    //    }
    //}

    //void CheckCollisions(int checkElem)
    //{
    //    List<float3> collidingNormals = new List<float3>();
    //    Body tempBody = movingGroup.objectBody[checkElem];

    //    //compare moving to moving
    //    for (int i = 0; i < movingGroup.Length; i++)
    //    {
    //        //don't compare things that belong to the same object (stored in body id)
    //        if (movingGroup.objectBody[checkElem].parentID != movingGroup.objectBody[i].parentID)
    //        {
    //            Debug.Log("Comparing Moving (SHOULDNT)");
    //            //Collision detected
    //            float3 newCol = CompareCollisions(movingGroup.objectPos[checkElem].Value, movingGroup.objectPos[i].Value, movingGroup.colliderInfo[checkElem], movingGroup.colliderInfo[i]);
    //            if (newCol.x != -100)
    //                collidingNormals.Add(newCol);
    //        }
    //    }

    //    //compare moving to stationary
    //    for (int i = 0; i < stationaryGroup.Length; i++)
    //    {
    //        //Collision detected
    //        float3 newCol = CompareCollisions(movingGroup.objectPos[checkElem].Value, stationaryGroup.objectPos[i].Value, movingGroup.colliderInfo[checkElem], stationaryGroup.colliderInfo[i]);
    //        if (newCol.x != -100)
    //            collidingNormals.Add(newCol);
    //    }

    //    //if colliding with any normals, remove the component that moves in that direction
    //    for (int x = 0; x < collidingNormals.Count; x++)
    //    {
    //        float dotProd = Vector3.Dot(movingGroup.objectBody[checkElem].velocity, collidingNormals[x]);

    //        //only remove in direction we want
    //        if (dotProd < 0)
    //            tempBody.velocity -= collidingNormals[x] * dotProd;

    //    }

    //    //set new velocity of body with components removed
    //    movingGroup.objectBody[checkElem] = tempBody;
    //}

    //float3 CompareCollisions(float3 pos1, float3 pos2, BoxCollider col1, BoxCollider col2)
    //{
    //    float3 collisionNormal = new float3(-100, -100, -100);

    //    float3 max1 = AABBMax(pos1, col1);
    //    float3 min1 = AABBMin(pos1, col1);
    //    float3 max2 = AABBMax(pos2, col2);
    //    float3 min2 = AABBMin(pos2, col2);

    //    bool isColliding = false;

    //    if (max1.x >= min2.x && min1.x < max2.x)
    //    {

    //        if (max1.y >= min2.y && min1.y < max2.y)
    //        {
    //            if (max1.z >= min2.z && min1.z < max2.z)
    //            {
    //                //COLLIDING
    //                isColliding = true;
    //            }
    //        }
    //    }

    //    if (isColliding)
    //    {
    //        //create normal vector based on point and position of ball
    //        float3 colPoint = ClosestPoint(max2, min2, pos1);
    //        Vector3 dir = pos1 - colPoint;
    //        dir = dir.normalized;

    //        collisionNormal = dir;
    //    }

    //    return collisionNormal;
    //}

    //float3 ClosestPoint(float3 Max, float3 Min, float3 pos)
    //{
    //    float3 point;

    //    if (pos.x > Max.x)
    //        point.x = Max.x;
    //    else if (pos.x < Min.x)
    //        point.x = Min.x;
    //    else
    //        point.x = pos.x;
    //    //yclose
    //    if (pos.y > Max.y)
    //        point.y = Max.y;
    //    else if (pos.y < Min.y)
    //        point.y = Min.y;
    //    else
    //        point.y = pos.y;
    //    //zclose
    //    if (pos.z > Max.z)
    //        point.z = Max.z;
    //    else if (pos.z < Min.z)
    //        point.z = Min.z;
    //    else
    //        point.z = pos.z;

    //    return point;
    //}

    //float3 AABBMax(float3 pos, BoxCollider col)
    //{
    //    return new float3(pos.x + col.size.x, pos.y + col.size.y, pos.z + col.size.z);
    //}

    //float3 AABBMin(float3 pos, BoxCollider col)
    //{
    //    return new float3(pos.x - col.size.x, pos.y - col.size.y, pos.z - col.size.z);
    //}

}


