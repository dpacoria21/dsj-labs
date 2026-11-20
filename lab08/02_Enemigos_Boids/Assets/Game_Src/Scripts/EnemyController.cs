using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float m_Speed = 4f;
    [SerializeField] private float m_RotationSpeed = 8f;
    [SerializeField] private float m_StopDistance = 1.5f;

    [Header("Boids")]
    [SerializeField, Min(0)] private float m_DetectionDistance = 3f;
    [SerializeField, Min(0)] private float m_SeparationWeight = 1.5f;
    [SerializeField, Min(0)] private float m_AlignmentWeight = 0.4f;
    [SerializeField, Min(0)] private float m_CohesionWeight = 0.5f;

    private Animator animator;
    private Transform target;
    private int enemyMask;
    private float movementBlend;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            target = player.transform;
        enemyMask = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        if (target == null)
            return;

        Vector3 toTarget = (target.position - transform.position).WithNewY(0);
        Vector3 direction = Vector3.zero;
        if (toTarget.sqrMagnitude > m_StopDistance * m_StopDistance)
        {
            Vector3 steering = toTarget.normalized + CalculateFlocking();
            direction = steering.normalized;
            transform.position += direction * m_Speed * Time.deltaTime;
        }

        movementBlend = Mathf.MoveTowards(movementBlend, direction == Vector3.zero ? 0 : 1,
            m_Speed * Time.deltaTime);
        animator.SetFloat("Speed", movementBlend);

        Vector3 facing = direction == Vector3.zero ? toTarget : direction;
        if (facing.sqrMagnitude > 0.0001f)
        {
            Quaternion rotation = Quaternion.LookRotation(facing);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation,
                1 - Mathf.Exp(-m_RotationSpeed * Time.deltaTime));
        }
    }

    private Vector3 CalculateFlocking()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 center = Vector3.zero;
        int neighbors = 0;

        Collider[] nearby = Physics.OverlapSphere(transform.position, m_DetectionDistance,
            enemyMask, QueryTriggerInteraction.Collide);
        foreach (Collider collider in nearby)
        {
            // The tutorial prefab has one capsule collider on the same root as its controller.
            if (!collider.TryGetComponent(out EnemyController other) || other == this)
                continue;

            Vector3 offset = (other.transform.position - transform.position).WithNewY(0);
            float distance = offset.magnitude;
            if (distance > 0.001f)
                separation -= offset / (distance * distance);

            alignment += other.transform.forward.WithNewY(0);
            center += other.transform.position;
            neighbors++;
        }

        if (neighbors == 0)
            return Vector3.zero;

        Vector3 cohesion = (center / neighbors - transform.position).WithNewY(0).normalized;
        return separation * m_SeparationWeight
            + alignment.normalized * m_AlignmentWeight
            + cohesion * m_CohesionWeight;
    }
}
