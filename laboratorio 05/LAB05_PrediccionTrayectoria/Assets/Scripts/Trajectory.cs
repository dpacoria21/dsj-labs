using UnityEngine;

// Adaptacion del Trajectory.cs de hamza herbou (MIT).
public class Trajectory : MonoBehaviour
{
    [SerializeField] [Min(2)] public int dotsNumber = 45;
    [SerializeField] public GameObject dotsParent;
    [SerializeField] public GameObject dotPrefab;
    [SerializeField] [Min(.001f)] public float dotSpacing = .05f;
    [SerializeField] [Range(.01f, .3f)] public float dotMinScale = .10f;
    [SerializeField] [Range(.3f, 1f)] public float dotMaxScale = .35f;
    public Ball ball;
    public bool stopAtFirstCollision = true;
    Transform[] dotsList;
    readonly RaycastHit2D[] hits = new RaycastHit2D[16];
    public int VisibleDots { get; private set; }

    void Start() { PrepareDots(); Hide(); }
    public void PrepareDots()
    {
        if (dotsList != null) return;
        dotsNumber = Mathf.Max(2, dotsNumber);
        dotSpacing = Mathf.Max(.001f, dotSpacing);
        dotsList = new Transform[dotsNumber];
        for (int i = 0; i < dotsNumber; i++)
        {
            dotsList[i] = Instantiate(dotPrefab, dotsParent.transform).transform;
            dotsList[i].name = "Punto " + (i + 1);
            dotsList[i].localScale = Vector3.one * Mathf.Lerp(dotMaxScale, dotMinScale, (float)i / (dotsNumber - 1));
        }
    }
    // Ecuacion continua de la guia: p(t)=p0+v0*t+0.5*g*t^2.
    public static Vector2 PositionAtTime(Vector2 origin, Vector2 velocity, Vector2 acceleration, float time)
        => origin + velocity * time + .5f * acceleration * time * time;

    // Box2D integra v primero y despues p. En pasos enteros esta correccion
    // suma exactamente g*dt^2*n*(n+1)/2. Entre pasos dibujamos la curva suave.
    public static Vector2 PhysicsPositionAtTime(Vector2 origin, Vector2 velocity, Vector2 acceleration, float time, float fixedStep)
        => PositionAtTime(origin, velocity, acceleration, time) + .5f * acceleration * fixedStep * time;

    public void UpdateDots(Vector3 ballPos, Vector2 forceApplied)
    {
        PrepareDots();
        Vector2 velocity = forceApplied / ball.rb.mass;
        Vector2 acceleration = Physics2D.gravity * ball.rb.gravityScale;
        Vector2 previous = ballPos;
        bool blocked = false;
        VisibleDots = 0;
        float radius = ball.col.radius * Mathf.Abs(ball.transform.lossyScale.x);
        var filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.GetLayerCollisionMask(ball.gameObject.layer));
        filter.useTriggers = false;
        for (int i = 0; i < dotsList.Length; i++)
        {
            float t = (i + 1) * dotSpacing;
            Vector2 next = PhysicsPositionAtTime(ballPos, velocity, acceleration, t, Time.fixedDeltaTime);
            if (!blocked && stopAtFirstCollision)
            {
                Vector2 delta = next - previous;
                int count = Physics2D.CircleCast(previous, radius, delta.normalized, filter, hits, delta.magnitude);
                for (int h = 0; h < count; h++)
                    if (hits[h].collider != ball.col) { blocked = true; break; }
            }
            dotsList[i].gameObject.SetActive(!blocked);
            dotsList[i].position = next;
            if (!blocked) VisibleDots++;
            previous = next;
        }
    }
    public void Show() { dotsParent.SetActive(true); }
    public void Hide() { dotsParent.SetActive(false); }
}
