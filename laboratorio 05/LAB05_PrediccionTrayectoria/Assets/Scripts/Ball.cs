using System.Collections;
using UnityEngine;

// Adaptacion del Ball.cs de hamza herbou (MIT), API de Unity 6.
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CircleCollider2D col;
    public Vector3 pos => transform.position;
    public bool InFlight { get; private set; }
    public int ShotId { get; private set; }
    public Vector2 SpawnPosition { get; private set; }
    public Vector2 InitialVelocity { get; private set; }
    public Vector2 ShotOrigin { get; private set; }
    public int CollisionCount { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        SpawnPosition = rb.position;
    }
    public void Push(Vector2 force)
    {
        // AddForce en modo Impulse: cambio de velocidad = impulso / masa.
        ShotId++;
        CollisionCount = 0;
        ShotOrigin = rb.position;
        InitialVelocity = force / rb.mass;
        InFlight = true;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    public void ActivateRb() { rb.bodyType = RigidbodyType2D.Dynamic; rb.WakeUp(); }
    public void DesactivateRb()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        InFlight = false;
    }
    public void ResetBall()
    {
        DesactivateRb();
        rb.position = SpawnPosition;
        transform.position = SpawnPosition;
        rb.rotation = 0;
        Physics2D.SyncTransforms();
        var trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
            StartCoroutine(ResumeTrail(trail));
        }
    }
    IEnumerator ResumeTrail(TrailRenderer trail)
    {
        // El salto de recuperacion no forma parte de la trayectoria fisica.
        yield return new WaitForEndOfFrame();
        trail.Clear();
        trail.emitting = true;
    }
    void OnCollisionEnter2D(Collision2D collision) { CollisionCount++; }
}
