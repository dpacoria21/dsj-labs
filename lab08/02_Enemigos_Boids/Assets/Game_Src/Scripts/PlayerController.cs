using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_RotationSpeed = 10f;
    [SerializeField] private LayerMask m_FloorMask;

    private CharacterController controller;
    private Animator animator;
    private Vector3? destination;
    private float movementBlend;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void SetDestination(Vector3 point)
    {
        destination = point;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            destination = null;
            if (TryGetCursorPoint(out Vector3 aim))
                RotateTo(aim - transform.position);
        }
        else if (Input.GetMouseButton(0) && TryGetCursorPoint(out Vector3 point))
        {
            SetDestination(point);
        }

        Vector3 step = Vector3.zero;
        if (destination.HasValue)
        {
            Vector3 offset = (destination.Value - transform.position).WithNewY(0);
            if (offset.sqrMagnitude > 0.0025f)
            {
                step = Vector3.ClampMagnitude(offset, m_Speed * Time.deltaTime);
                RotateTo(offset);
            }
            else
            {
                destination = null;
            }
        }

        controller.Move(step);
        movementBlend = Mathf.MoveTowards(movementBlend, step == Vector3.zero ? 0 : 1,
            m_Speed * Time.deltaTime);
        animator.SetFloat("Speed", movementBlend);
    }

    private void RotateTo(Vector3 offset)
    {
        Vector3 heading = offset.WithNewY(0);
        if (heading.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(heading),
                1 - Mathf.Exp(-m_RotationSpeed * Time.deltaTime));
        }
    }

    private bool TryGetCursorPoint(out Vector3 point)
    {
        Camera camera = Camera.main;
        if (camera != null && Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition),
                out RaycastHit hit, 100, m_FloorMask))
        {
            point = hit.point;
            return true;
        }

        point = default;
        return false;
    }
}
