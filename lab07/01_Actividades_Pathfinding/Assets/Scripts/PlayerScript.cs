using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class PlayerScript : MonoBehaviour
{
    public float Speed = 5f;
    public Transform graphics;
    [System.NonSerialized] public bool automated;
    [System.NonSerialized] public Vector2 testInput;
    Rigidbody2D body;
    Vector2 input;
    void Awake() { body = GetComponent<Rigidbody2D>(); }
    void Update()
    {
        input = automated ? testInput : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);
        if (graphics != null && input.sqrMagnitude > .001f) graphics.right = input;
    }
    void FixedUpdate() { body.linearVelocity = input * Speed; }
}
