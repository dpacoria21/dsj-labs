using System.Collections.Generic;
using UnityEngine;

public class FlockUnit : MonoBehaviour
{
    private const float ObstacleClearance = 0.3f;

    [SerializeField] private float FOVAngle;
    [SerializeField] private float smoothDamp;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Vector3[] directionsToCheckWhenAvoidingObstacles;

    private readonly List<FlockUnit> cohesionNeighbours = new List<FlockUnit>();
    private readonly List<FlockUnit> avoidanceNeighbours = new List<FlockUnit>();
    private readonly List<FlockUnit> aligementNeighbours = new List<FlockUnit>();
    private Flock assignedFlock;
    private Vector3 currentVelocity;
    private Vector3 currentObstacleAvoidanceVector;
    private float speed;

    public Transform myTransform { get; private set; }

    private void Awake()
    {
        myTransform = transform;
    }

    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }

    public void InitializeSpeed(float speed)
    {
        this.speed = speed;
    }

    public void MoveUnit()
    {
        FindNeighbours();
        CalculateSpeed();

        var cohesionVector = CalculateCohesionVector() * assignedFlock.cohesionWeight;
        var avoidanceVector = CalculateAvoidanceVector() * assignedFlock.avoidanceWeight;
        var aligementVector = CalculateAligementVector() * assignedFlock.aligementWeight;
        var boundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;
        var obstacleVector = CalculateObstacleVector() * assignedFlock.obstacleWeight;

        Vector3 desired = cohesionVector + avoidanceVector + aligementVector + boundsVector + obstacleVector;
        if (desired.sqrMagnitude < 0.0001f)
            desired = myTransform.forward;

        Vector3 heading = Vector3.SmoothDamp(myTransform.forward, desired.normalized,
            ref currentVelocity, smoothDamp).normalized;
        if (heading.sqrMagnitude < 0.0001f)
            heading = desired.normalized;

        float step = speed * Time.deltaTime;
        if (Physics.SphereCast(myTransform.position, ObstacleClearance, heading, out _, step, obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            heading = desired.normalized;
            if (Physics.SphereCast(myTransform.position, ObstacleClearance, heading, out _, step, obstacleMask,
                    QueryTriggerInteraction.Ignore))
                step = 0;
        }

        myTransform.forward = heading;
        myTransform.position += heading * step;
    }

    private void FindNeighbours()
    {
        cohesionNeighbours.Clear();
        avoidanceNeighbours.Clear();
        aligementNeighbours.Clear();
        var allUnits = assignedFlock.allUnits;
        for (int i = 0; i < allUnits.Length; i++)
        {
            var currentUnit = allUnits[i];
            if (currentUnit != this)
            {
                float currentNeighbourDistanceSqr = Vector3.SqrMagnitude(currentUnit.myTransform.position - myTransform.position);
                if(currentNeighbourDistanceSqr <= assignedFlock.cohesionDistance * assignedFlock.cohesionDistance)
                {
                    cohesionNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= assignedFlock.avoidanceDistance * assignedFlock.avoidanceDistance)
                {
                    avoidanceNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= assignedFlock.aligementDistance * assignedFlock.aligementDistance)
                {
                    aligementNeighbours.Add(currentUnit);
                }
            }
        }
    }

    private void CalculateSpeed()
    {
        if (cohesionNeighbours.Count == 0)
            return;
        speed = 0;
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            speed += cohesionNeighbours[i].speed;
        }

        speed /= cohesionNeighbours.Count;
        speed = Mathf.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);
    }

    private Vector3 CalculateCohesionVector()
    {
        var cohesionVector = Vector3.zero;
        if (cohesionNeighbours.Count == 0)
            return Vector3.zero;
        int neighboursInFOV = 0;
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            if (IsInFOV(cohesionNeighbours[i].myTransform.position))
            {
                neighboursInFOV++;
                cohesionVector += cohesionNeighbours[i].myTransform.position;
            }
        }

        if (neighboursInFOV == 0)
            return Vector3.zero;

        cohesionVector /= neighboursInFOV;
        cohesionVector -= myTransform.position;
        cohesionVector = cohesionVector.normalized;
        return cohesionVector;
    }

    private Vector3 CalculateAligementVector()
    {
        var aligementVector = myTransform.forward;
        if (aligementNeighbours.Count == 0)
            return myTransform.forward;
        for (int i = 0; i < aligementNeighbours.Count; i++)
        {
            if (IsInFOV(aligementNeighbours[i].myTransform.position))
            {
                aligementVector += aligementNeighbours[i].myTransform.forward;
            }
        }

        aligementVector = aligementVector.normalized;
        return aligementVector;
    }

    private Vector3 CalculateAvoidanceVector()
    {
        var avoidanceVector = Vector3.zero;
        if (avoidanceNeighbours.Count == 0)
            return Vector3.zero;
        for (int i = 0; i < avoidanceNeighbours.Count; i++)
        {
            if (IsInFOV(avoidanceNeighbours[i].myTransform.position))
            {
                avoidanceVector += (myTransform.position - avoidanceNeighbours[i].myTransform.position);
            }
        }

        avoidanceVector = avoidanceVector.normalized;
        return avoidanceVector;
    }

    private Vector3 CalculateBoundsVector()
    {
        var offsetToCenter = assignedFlock.transform.position - myTransform.position;
        bool isOutsideCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9f);
        return isOutsideCenter ? offsetToCenter.normalized : Vector3.zero;
    }

    public bool IsAvoidingObstacle => currentObstacleAvoidanceVector != Vector3.zero;

    private Vector3 CalculateObstacleVector()
    {
        Vector3 position = myTransform.position;
        float distance = assignedFlock.obstacleDistance;
        if (!Physics.Raycast(position, myTransform.forward, out RaycastHit forwardHit, distance,
                obstacleMask, QueryTriggerInteraction.Ignore))
        {
            currentObstacleAvoidanceVector = Vector3.zero;
            return Vector3.zero;
        }

        if (IsAvoidingObstacle && !Physics.Raycast(position, currentObstacleAvoidanceVector,
                distance, obstacleMask, QueryTriggerInteraction.Ignore))
            return currentObstacleAvoidanceVector;

        // Favor the first clear direction. If boxed in, steer along the hit normal.
        foreach (Vector3 localDirection in directionsToCheckWhenAvoidingObstacles)
        {
            Vector3 direction = myTransform.TransformDirection(localDirection.normalized);
            if (!Physics.Raycast(position, direction, distance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                currentObstacleAvoidanceVector = direction;
                return direction;
            }
        }

        currentObstacleAvoidanceVector = forwardHit.normal;
        return currentObstacleAvoidanceVector;
    }

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(myTransform.forward, position - myTransform.position) <= FOVAngle;
    }
}
