using UnityEngine;

// El borde tiene colision fisica; un sensor interior independiente registra la entrada.
[RequireComponent(typeof(Rigidbody2D), typeof(EdgeCollider2D))]
public class Cup : MonoBehaviour
{
    public string targetId = "A";
    public int points = 1;
    public CupSensor sensor;
    Rigidbody2D body;
    Vector2 initialPosition;
    float initialRotation;
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        initialPosition = body.position;
        initialRotation = body.rotation;
    }
    public void ResetCup()
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0;
        body.position = initialPosition;
        body.rotation = initialRotation;
        body.WakeUp();
    }
    public bool Hit(Ball ball) { return GameManager.Instance != null && GameManager.Instance.RegisterHit(this, ball); }
}
