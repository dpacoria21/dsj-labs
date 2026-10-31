using UnityEngine;
using Pathfinding;

public class EnemyMovement : MonoBehaviour
{
    public AIPath aiPath;
    void Update()
    {
        Vector2 direction = aiPath.desiredVelocity;
        if (direction.sqrMagnitude > .001f) transform.right = direction;
    }
}
