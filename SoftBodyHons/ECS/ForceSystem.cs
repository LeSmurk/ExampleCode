using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

//apply forces and create velocities from gravity and springs

public class ForceSystem : JobComponentSystem
{
    protected struct ForceGroup
    {
        //look at component group instead of data array
        public ComponentDataArray<Position> objectPos;
        public ComponentDataArray<Velocity> objectVel;
        public ComponentDataArray<Body> objectBody;
        public ComponentDataArray<Neighbours> objectNeigh;
        public ComponentDataArray<FixedNeighbours> objectFixedNeigh;
        public readonly ComponentDataArray<ID> objectID;
        public readonly int Length;
        //SUBTRACTIVE COMPONENT (Removes from the loop if it has stationary component)
        //public SubtractiveComponent<Stationary> subsStationary; //(don't need as stationary objects won't have bodies)
    }

    [Inject] protected ForceGroup forceGroup;
    public struct ForceJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public ComponentDataArray<Body> objectBody;
        [NativeDisableParallelForRestriction]
        public ComponentDataArray<Velocity> objectVel;
        [ReadOnly]
        public ComponentDataArray<Position> objectPos;
        [ReadOnly]
        public ComponentDataArray<Neighbours> objectNeigh;
        [ReadOnly]
        public ComponentDataArray<FixedNeighbours> objectFixedNeigh;
        [ReadOnly]
        public ComponentDataArray<ID> objectID;

        public void Execute(int currentMass)
        {
            //VARIABLES
            float damp = 50; //10, 8  30
            float springConst = 300; //1, 2   300
            float springRest = 0.6f;

            Vector3 currForce = Vector3.zero;// objectBody[currentMass].force;
            //CHANGE THIS FROM 26 AND 48 FOR DIFFERENT AMOUNTS AND TURN DOWN SPRING CONST
            int[] neighbours = new int[48];
            float[] rests = new float[48];

            ////CHANGE BETWEEN FIXED NEIGHBOURS AND DYNAMIC UNCOMMENT
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            neighbours[0] = objectFixedNeigh[currentMass].neighbour0;
            neighbours[1] = objectFixedNeigh[currentMass].neighbour1;
            neighbours[2] = objectFixedNeigh[currentMass].neighbour2;
            neighbours[3] = objectFixedNeigh[currentMass].neighbour3;
            neighbours[4] = objectFixedNeigh[currentMass].neighbour4;
            neighbours[5] = objectFixedNeigh[currentMass].neighbour5;
            neighbours[6] = objectFixedNeigh[currentMass].neighbour6;
            neighbours[7] = objectFixedNeigh[currentMass].neighbour7;
            neighbours[8] = objectFixedNeigh[currentMass].neighbour8;
            neighbours[9] = objectFixedNeigh[currentMass].neighbour9;
            neighbours[10] = objectFixedNeigh[currentMass].neighbour10;
            neighbours[11] = objectFixedNeigh[currentMass].neighbour11;
            neighbours[12] = objectFixedNeigh[currentMass].neighbour12;
            neighbours[13] = objectFixedNeigh[currentMass].neighbour13;
            neighbours[14] = objectFixedNeigh[currentMass].neighbour14;
            neighbours[15] = objectFixedNeigh[currentMass].neighbour15;
            neighbours[16] = objectFixedNeigh[currentMass].neighbour16;
            neighbours[17] = objectFixedNeigh[currentMass].neighbour17;
            neighbours[18] = objectFixedNeigh[currentMass].neighbour18;
            neighbours[19] = objectFixedNeigh[currentMass].neighbour19;
            neighbours[20] = objectFixedNeigh[currentMass].neighbour20;
            neighbours[21] = objectFixedNeigh[currentMass].neighbour21;
            neighbours[22] = objectFixedNeigh[currentMass].neighbour22;
            neighbours[23] = objectFixedNeigh[currentMass].neighbour23;
            neighbours[24] = objectFixedNeigh[currentMass].neighbour24;
            neighbours[25] = objectFixedNeigh[currentMass].neighbour25;
            neighbours[26] = objectFixedNeigh[currentMass].neighbour26;
            neighbours[27] = objectFixedNeigh[currentMass].neighbour27;
            neighbours[28] = objectFixedNeigh[currentMass].neighbour28;
            neighbours[29] = objectFixedNeigh[currentMass].neighbour29;
            neighbours[30] = objectFixedNeigh[currentMass].neighbour30;
            neighbours[31] = objectFixedNeigh[currentMass].neighbour31;
            neighbours[32] = objectFixedNeigh[currentMass].neighbour32;
            neighbours[33] = objectFixedNeigh[currentMass].neighbour33;
            neighbours[34] = objectFixedNeigh[currentMass].neighbour34;
            neighbours[35] = objectFixedNeigh[currentMass].neighbour35;
            neighbours[36] = objectFixedNeigh[currentMass].neighbour36;
            neighbours[37] = objectFixedNeigh[currentMass].neighbour37;
            neighbours[38] = objectFixedNeigh[currentMass].neighbour38;
            neighbours[39] = objectFixedNeigh[currentMass].neighbour39;
            neighbours[40] = objectFixedNeigh[currentMass].neighbour40;
            neighbours[41] = objectFixedNeigh[currentMass].neighbour41;
            neighbours[42] = objectFixedNeigh[currentMass].neighbour42;
            neighbours[43] = objectFixedNeigh[currentMass].neighbour43;
            neighbours[44] = objectFixedNeigh[currentMass].neighbour44;
            neighbours[45] = objectFixedNeigh[currentMass].neighbour45;
            neighbours[46] = objectFixedNeigh[currentMass].neighbour46;
            neighbours[47] = objectFixedNeigh[currentMass].neighbour47;

            rests[0] = objectFixedNeigh[currentMass].rest0;
            rests[1] = objectFixedNeigh[currentMass].rest1;
            rests[2] = objectFixedNeigh[currentMass].rest2;
            rests[3] = objectFixedNeigh[currentMass].rest3;
            rests[4] = objectFixedNeigh[currentMass].rest4;
            rests[5] = objectFixedNeigh[currentMass].rest5;
            rests[6] = objectFixedNeigh[currentMass].rest6;
            rests[7] = objectFixedNeigh[currentMass].rest7;
            rests[8] = objectFixedNeigh[currentMass].rest8;
            rests[9] = objectFixedNeigh[currentMass].rest9;
            rests[10] = objectFixedNeigh[currentMass].rest10;
            rests[11] = objectFixedNeigh[currentMass].rest11;
            rests[12] = objectFixedNeigh[currentMass].rest12;
            rests[13] = objectFixedNeigh[currentMass].rest13;
            rests[14] = objectFixedNeigh[currentMass].rest14;
            rests[15] = objectFixedNeigh[currentMass].rest15;
            rests[16] = objectFixedNeigh[currentMass].rest16;
            rests[17] = objectFixedNeigh[currentMass].rest17;
            rests[18] = objectFixedNeigh[currentMass].rest18;
            rests[19] = objectFixedNeigh[currentMass].rest19;
            rests[20] = objectFixedNeigh[currentMass].rest20;
            rests[21] = objectFixedNeigh[currentMass].rest21;
            rests[22] = objectFixedNeigh[currentMass].rest22;
            rests[23] = objectFixedNeigh[currentMass].rest23;
            rests[24] = objectFixedNeigh[currentMass].rest24;
            rests[25] = objectFixedNeigh[currentMass].rest25;
            rests[26] = objectFixedNeigh[currentMass].rest26;
            rests[27] = objectFixedNeigh[currentMass].rest27;
            rests[28] = objectFixedNeigh[currentMass].rest28;
            rests[29] = objectFixedNeigh[currentMass].rest29;
            rests[30] = objectFixedNeigh[currentMass].rest30;
            rests[31] = objectFixedNeigh[currentMass].rest31;
            rests[32] = objectFixedNeigh[currentMass].rest32;
            rests[33] = objectFixedNeigh[currentMass].rest33;
            rests[34] = objectFixedNeigh[currentMass].rest34;
            rests[35] = objectFixedNeigh[currentMass].rest35;
            rests[36] = objectFixedNeigh[currentMass].rest36;
            rests[37] = objectFixedNeigh[currentMass].rest37;
            rests[38] = objectFixedNeigh[currentMass].rest38;
            rests[39] = objectFixedNeigh[currentMass].rest39;
            rests[40] = objectFixedNeigh[currentMass].rest40;
            rests[41] = objectFixedNeigh[currentMass].rest41;
            rests[42] = objectFixedNeigh[currentMass].rest42;
            rests[43] = objectFixedNeigh[currentMass].rest43;
            rests[44] = objectFixedNeigh[currentMass].rest44;
            rests[45] = objectFixedNeigh[currentMass].rest45;
            rests[46] = objectFixedNeigh[currentMass].rest46;
            rests[47] = objectFixedNeigh[currentMass].rest47;

            //spring to nearest few neighbours
            //normally neighbours.length but this array isn't the correct size
            for (int z = 0; z < objectFixedNeigh[currentMass].totalNeighbours; z++)
            {
                ////set z to a fake num
                int comparison = neighbours[z];
                if (comparison == -1 || objectID[currentMass].parentID != objectID[comparison].parentID)
                    continue;

                //find distance to base spring
                float dist = Vector3.Distance(objectPos[comparison].Value, objectPos[currentMass].Value);

                //direction to base spring
                Vector3 dir = (objectPos[comparison].Value - objectPos[currentMass].Value);
                dir.Normalize();

                //compression
                //
                float compression = dist - rests[z];
                //float compression = dist - springRest;

                //damping coefficient (we want underdamped, so coeff is < 1)
                //c = 2 sqr (k / m)
                //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

                //create relative vel between two mass points
                Vector3 vel = objectVel[currentMass].velocity - objectVel[comparison].velocity;
                float relVel = Vector3.Dot(vel, dir);

                //f = kx (force of spring) - relative velocity between the two masses * dampener
                float force = (springConst * compression) - relVel * damp;

                //push force on mass in correct direction
                //dir * force

                //update locally stored force
                currForce += dir * force;

                //force other point in other dir
                //massPoints[comparison].force -= dir * force;
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //////Update neighbours frequently
            //neighbours[0] = objectNeigh[currentMass].neighbour0;
            //neighbours[1] = objectNeigh[currentMass].neighbour1;
            //neighbours[2] = objectNeigh[currentMass].neighbour2;
            //neighbours[3] = objectNeigh[currentMass].neighbour3;
            //neighbours[4] = objectNeigh[currentMass].neighbour4;
            //neighbours[5] = objectNeigh[currentMass].neighbour5;
            //neighbours[6] = objectNeigh[currentMass].neighbour6;
            //neighbours[7] = objectNeigh[currentMass].neighbour7;
            //neighbours[8] = objectNeigh[currentMass].neighbour8;
            //neighbours[9] = objectNeigh[currentMass].neighbour9;
            //neighbours[10] = objectNeigh[currentMass].neighbour10;
            //neighbours[11] = objectNeigh[currentMass].neighbour11;
            //neighbours[12] = objectNeigh[currentMass].neighbour12;
            //neighbours[13] = objectNeigh[currentMass].neighbour13;
            //neighbours[14] = objectNeigh[currentMass].neighbour14;
            //neighbours[15] = objectNeigh[currentMass].neighbour15;
            //neighbours[16] = objectNeigh[currentMass].neighbour16;
            //neighbours[17] = objectNeigh[currentMass].neighbour17;
            //neighbours[18] = objectNeigh[currentMass].neighbour18;
            //neighbours[19] = objectNeigh[currentMass].neighbour19;
            //neighbours[20] = objectNeigh[currentMass].neighbour20;
            //neighbours[21] = objectNeigh[currentMass].neighbour21;
            //neighbours[22] = objectNeigh[currentMass].neighbour22;
            //neighbours[23] = objectNeigh[currentMass].neighbour23;
            //neighbours[24] = objectNeigh[currentMass].neighbour24;
            //neighbours[25] = objectNeigh[currentMass].neighbour25;
            //neighbours[26] = objectNeigh[currentMass].neighbour26;
            //neighbours[27] = objectNeigh[currentMass].neighbour27;
            //neighbours[28] = objectNeigh[currentMass].neighbour28;
            //neighbours[29] = objectNeigh[currentMass].neighbour29;
            //neighbours[30] = objectNeigh[currentMass].neighbour30;
            //neighbours[31] = objectNeigh[currentMass].neighbour31;
            //neighbours[32] = objectNeigh[currentMass].neighbour32;
            //neighbours[33] = objectNeigh[currentMass].neighbour33;
            //neighbours[34] = objectNeigh[currentMass].neighbour34;
            //neighbours[35] = objectNeigh[currentMass].neighbour35;
            //neighbours[36] = objectNeigh[currentMass].neighbour36;
            //neighbours[37] = objectNeigh[currentMass].neighbour37;
            //neighbours[38] = objectNeigh[currentMass].neighbour38;
            //neighbours[39] = objectNeigh[currentMass].neighbour39;
            //neighbours[40] = objectNeigh[currentMass].neighbour40;
            //neighbours[41] = objectNeigh[currentMass].neighbour41;
            //neighbours[42] = objectNeigh[currentMass].neighbour42;
            //neighbours[43] = objectNeigh[currentMass].neighbour43;
            //neighbours[44] = objectNeigh[currentMass].neighbour44;
            //neighbours[45] = objectNeigh[currentMass].neighbour45;
            //neighbours[46] = objectNeigh[currentMass].neighbour46;
            //neighbours[47] = objectNeigh[currentMass].neighbour47;

            //for (int z = 0; z < neighbours.Length; z++)
            //{
            //    ////set z to a fake num
            //    int comparison = neighbours[z];
            //    if (comparison == -1 || objectID[currentMass].parentID != objectID[comparison].parentID)
            //        continue;

            //    //find distance to base spring
            //    float dist = Vector3.Distance(objectPos[comparison].Value, objectPos[currentMass].Value);

            //    //direction to base spring
            //    Vector3 dir = (objectPos[comparison].Value - objectPos[currentMass].Value);
            //    dir.Normalize();

            //    //compression
            //    //
            //    float compression = dist - springRest;

            //    //damping coefficient (we want underdamped, so coeff is < 1)
            //    //c = 2 sqr (k / m)
            //    //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

            //    //create relative vel between two mass points
            //    Vector3 vel = objectVel[currentMass].velocity - objectVel[comparison].velocity;
            //    float relVel = Vector3.Dot(vel, dir);

            //    //f = kx (force of spring) - relative velocity between the two masses * dampener
            //    float force = (springConst * compression) - relVel * damp;

            //    //push force on mass in correct direction
            //    //dir * force

            //    //update locally stored force
            //    currForce += dir * force;

            //    //force other point in other dir
            //    //massPoints[comparison].force -= dir * force;
            //}

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //stationary blob
            //if (objectID[currentMass].massID < 60) //110
            //{
            //calculate accelleration
            Body newBody = objectBody[currentMass];
                newBody.force = currForce;
                newBody.accel = currForce / newBody.mass;
                //add gravity
                newBody.accel += new float3(0, -1, 0) / 1000f;
                //cal vel
                Velocity newVel = objectVel[currentMass];
                newVel.velocity += newBody.accel;
                objectVel[currentMass] = newVel;

                objectBody[currentMass] = newBody;
            //}

            //create force against the direction of velocity (Kinda makes an air resistance too as not based on touching anything)
            //float3 friction = new Vector3(objectVel[currentMass].velocity.x, objectVel[currentMass].velocity.y, objectVel[currentMass].velocity.z).normalized * -0.1f;
            //newBody.force += friction;

        }

        //public void Execute(int currentMass)
        //{
        //    //VARIABLES
        //    float damp = 8; //8
        //    float springConst = 2; //2
        //    float springRest = 1f;
        //    float maxLength = 2;

        //    Vector3 currForce = new Vector3(0, 0, 0);
        //    int numSprings = 0;

        //    //spring to other mass points next to it in array
        //    for (int z = 0; z < objectPos.Length; z++)
        //    {
        //        //don't compare to its own mass point, and only use viable numbers (also check same parent body id)
        //        if (currentMass != z && objectID[currentMass].parentID == objectID[z].parentID)
        //        {
        //            ////set z to a fake num
        //            int comparison = z;

        //            //find distance to base spring
        //            float dist = Vector3.Distance(objectPos[comparison].Value, objectPos[currentMass].Value);

        //            //ONLY CONNECT SPRING IF CLOSE ENOUGH
        //            if (dist < maxLength)
        //            {
        //                //direction to base spring
        //                Vector3 dir = (objectPos[comparison].Value - objectPos[currentMass].Value);
        //                dir.Normalize();

        //                //compression
        //                //
        //                float compression = dist - springRest;

        //                //damping coefficient (we want underdamped, so coeff is < 1)
        //                //c = 2 sqr (k / m)
        //                //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

        //                //create relative vel between two mass points
        //                Vector3 vel = objectVel[currentMass].velocity - objectVel[comparison].velocity;
        //                float relVel = Vector3.Dot(vel, dir);

        //                //f = kx (force of spring) - relative velocity between the two masses * dampener
        //                float force = (springConst * compression) - relVel * damp;

        //                //push force on mass iin correct direction
        //                //dir * force

        //                //update locally stored force
        //                currForce += dir * force;

        //                numSprings++;

        //                //force other point in other dir
        //                //massPoints[comparison].force -= dir * force;
        //            }
        //        }
        //    }

        //    Vector3 totalForce = objectBody[currentMass].force;
        //    totalForce += (currForce / (numSprings * 1));

        //    //calculate accelleration
        //    Body newBody = objectBody[currentMass];
        //    newBody.accel = currForce / newBody.mass;
        //    //add gravity
        //    newBody.accel += new float3(0, -1, 0) / 1000f;
        //    //cal vel
        //    Velocity newVel = objectVel[currentMass];
        //    newVel.velocity += newBody.accel;
        //    objectVel[currentMass] = newVel;

        //    //create force against the direction of velocity(Kinda makes an air resistance too as not based on touching anything)
        //    float3 friction = new Vector3(objectVel[currentMass].velocity.x, objectVel[currentMass].velocity.y, objectVel[currentMass].velocity.z).normalized * -0.1f;
        //    newBody.force += friction;

        //    //decay force constantly
        //    newBody.force /= 1.5f;

        //    objectBody[currentMass] = newBody;
        //}
    }

    //public struct SpringJob : IJobProcessComponentData<Position, Body>
    //{
    //    public void Execute(ref Position pos, ref Body body)
    //    {
            
    //    }
    //}

    protected override JobHandle OnUpdate(JobHandle handle)
    {
        ForceJob fJob = new ForceJob
        {
            objectBody = forceGroup.objectBody,
            objectVel = forceGroup.objectVel,
            objectPos = forceGroup.objectPos,
            objectNeigh = forceGroup.objectNeigh,
            objectFixedNeigh = forceGroup.objectFixedNeigh,
            objectID = forceGroup.objectID,
        };
       
        return fJob.Schedule(forceGroup.Length, 1, handle);

        //throw new System.NotImplementedException();
    }
    //protected override void OnUpdate()
    //{
    //    for (int i = 0; i < positionGroup.Length; i++)
    //    {
    //        ClosestPoints(i);
    //        //NearestNeighbours(i);
    //    }
    //}

    //void ClosestPoints(int currentMass)
    //{
    //    //VARIABLES
    //    float damp = 10;
    //    float springConst = 1;
    //    float springRest = 0.6f;
    //    float maxLength = 2;

    //    Vector3 currForce = new Vector3(0, 0, 0);
    //    int numSprings = 0;

    //    //spring to other mass points next to it in array
    //    for (int z = 0; z < positionGroup.Length; z++)
    //    {
    //        //don't compare to its own mass point, and only use viable numbers
    //        if (currentMass != z)
    //        {
    //            ////set z to a fake num
    //            int comparison = z;

    //            //find distance to base spring
    //            float dist = Vector3.Distance(positionGroup.objectPos[comparison].Value, positionGroup.objectPos[currentMass].Value);

    //            //ONLY CONNECT SPRING IF CLOSE ENOUGH
    //            if (dist < maxLength)
    //            {
    //                //direction to base spring
    //                Vector3 dir = (positionGroup.objectPos[comparison].Value - positionGroup.objectPos[currentMass].Value);
    //                dir.Normalize();

    //                //compression
    //                //
    //                float compression = dist - springRest;

    //                //damping coefficient (we want underdamped, so coeff is < 1)
    //                //c = 2 sqr (k / m)
    //                //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

    //                //create relative vel between two mass points
    //                Vector3 vel = positionGroup.objectBody[currentMass].velocity - positionGroup.objectBody[comparison].velocity;
    //                float relVel = Vector3.Dot(vel, dir);

    //                //f = kx (force of spring) - relative velocity between the two masses * dampener
    //                float force = (springConst * compression) - relVel * damp;

    //                //push force on mass iin correct direction
    //                //dir * force

    //                //update locally stored force
    //                currForce += dir * force;

    //                numSprings++;

    //                //force other point in other dir
    //                //massPoints[comparison].force -= dir * force;
    //            }
    //        }
    //    }

    //    Vector3 totalForce = positionGroup.objectBody[currentMass].force;
    //    totalForce += (currForce / (numSprings * 1));

    //    //calculate accelleration
    //    Body newBody = positionGroup.objectBody[currentMass];
    //    newBody.accel = currForce / newBody.mass;
    //    //add gravity
    //    newBody.accel += new float3(0, -1, 0) / 1000f;
    //    //cal vel
    //    newBody.velocity += newBody.accel;

    //    //decay force constantly
    //    newBody.force /= 1.5f;

    //    positionGroup.objectBody[currentMass] = newBody;
    //}

    //void NearestNeighbours(int currentMass)
    //{
    //    //VARIABLES
    //    float damp = 10;
    //    float springConst = 1;
    //    float springRest = 0.6f;

    //    Vector3 currForce = positionGroup.objectBody[currentMass].force;
    //    int[] neighbours = GetNearestNeighbours(currentMass);

    //    //spring to nearest few neighbours
    //    for (int z = 0; z < neighbours.Length; z++)
    //    {
    //        ////set z to a fake num
    //        int comparison = neighbours[z];

    //        //find distance to base spring
    //        float dist = Vector3.Distance(positionGroup.objectPos[comparison].Value, positionGroup.objectPos[currentMass].Value);

    //        //direction to base spring
    //        Vector3 dir = (positionGroup.objectPos[comparison].Value - positionGroup.objectPos[currentMass].Value);
    //        dir.Normalize();

    //        //compression
    //        //
    //        float compression = dist - springRest;

    //        //damping coefficient (we want underdamped, so coeff is < 1)
    //        //c = 2 sqr (k / m)
    //        //float dampCoeff = 2 * Mathf.Sqrt(springConst * massPoints[1].mass);

    //        //create relative vel between two mass points
    //        Vector3 vel = positionGroup.objectBody[currentMass].velocity - positionGroup.objectBody[comparison].velocity;
    //        float relVel = Vector3.Dot(vel, dir);

    //        //f = kx (force of spring) - relative velocity between the two masses * dampener
    //        float force = (springConst * compression) - relVel * damp;

    //        //push force on mass iin correct direction
    //        //dir * force

    //        //update locally stored force
    //        currForce += dir * force;

    //        //force other point in other dir
    //        //massPoints[comparison].force -= dir * force;
    //    }

    //    //calculate accelleration
    //    Body newBody = positionGroup.objectBody[currentMass];
    //    newBody.accel = currForce / newBody.mass;
    //    //add gravity
    //    newBody.accel += new float3(0, -1, 0) / 1000f;
    //    //cal vel
    //    newBody.velocity += newBody.accel;

    //    //decay force constantly
    //    newBody.force /= 1.5f;

    //    positionGroup.objectBody[currentMass] = newBody;
    //}

    //int[] GetNearestNeighbours(int house)
    //{
    //    int neighboursNum = 12;

    //    //flat find the nearest points, no exclusivity lock or far point grab
    //    int[] neighbours = new int[neighboursNum];

    //    //store all the distances from the house
    //    float[] allDist = new float[positionGroup.Length];
    //    //don't include itself as neighbour
    //    allDist[house] = float.PositiveInfinity;

    //    for (int i = 0; i < positionGroup.Length; i++)
    //    {
    //        //don't include itself
    //        if (i != house)
    //            allDist[i] = Vector3.Distance(positionGroup.objectPos[house].Value, positionGroup.objectPos[i].Value);
    //    }

    //    for (int x = 0; x < neighbours.Length; x++)
    //    {
    //        //find closest
    //        float closest = Mathf.Min(allDist);

    //        //find out which mass point it is
    //        for (int i = 0; i < allDist.Length; i++)
    //        {
    //            if (closest == allDist[i] && i != house)
    //            {
    //                //set this mass point as a neighbour
    //                neighbours[x] = i;

    //                //remove the distance set (POSSIBLE ERROR HERE?)
    //                allDist[i] = float.PositiveInfinity;

    //                break;
    //            }
    //        }
    //    }

    //    return neighbours;
    //}
}


////move towards the next mass point
//int targetElem = 0;
//if (i + 1 < positionGroup.Length)
//    targetElem = i + 1;

//Vector3 trailPos = (positionGroup.objectPos[i].Value - positionGroup.objectPos[targetElem].Value);
//trailPos.Normalize();
//float3 target = trailPos;
//target = (positionGroup.objectPos[targetElem].Value + (target * 0.5f)) - positionGroup.objectPos[i].Value;


////create average position of all nearest neighbours
//float3 target = new float3(0f, 0f, 0f);
//for (int elem = 0; elem < positionGroup.Length; elem++)
//{
//    //discounts itself
//    if (elem != i)
//    {
//        //use average
//        target += positionGroup.objectPos[elem].Value;
//    }
//}

////create average position
//target /= positionGroup.Length - 1;

////move to that position
//target = target - positionGroup.objectPos[i].Value;
//Vector3 dir = target;
//dir.Normalize();
//target = dir;