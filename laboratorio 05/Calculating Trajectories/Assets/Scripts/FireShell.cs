// Adaptación de FireShell del paquete oficial S0311Resources (Unity Learn).
// Se mantiene la solución de los dos ángulos de tiro; se corrige el origen del
// disparo y la persecución durante recarga. El ZIP original se conserva intacto.
using UnityEngine;

public class FireShell : MonoBehaviour
{
    public GameObject bullet;
    public GameObject turret;
    public GameObject enemy;
    public Transform turretBase;
    public float speed = 15f;
    public float rotationSpeed = 5f;
    public float moveSpeed = 1f;
    public float shotInterval = 0.8f;
    public bool lowArc;
    public bool automaticFire = true;
    public bool HasSolution { get; private set; }
    public float Elevation { get; private set; }
    public Vector3 LaunchVelocity { get; private set; }
    public int Shots { get; private set; }
    public int TargetHits { get; private set; }
    public int HighArcHits { get; private set; }
    public int LowArcHits { get; private set; }
    float delay = 0.8f;

    public Vector3 TargetPosition => enemy.GetComponent<Collider>().bounds.center;

    public static bool TryCalculateAngle(float x, float y, float speed, float gravity,
        bool low, out float angle)
    {
        angle = 0f;
        if (x <= 0.001f || speed <= 0f || gravity <= 0f) return false;
        float squaredSpeed = speed * speed;
        float discriminant = squaredSpeed * squaredSpeed -
            gravity * (gravity * x * x + 2f * y * squaredSpeed);
        if (discriminant < 0f) return false;
        float root = Mathf.Sqrt(discriminant);
        angle = Mathf.Atan2(squaredSpeed + (low ? -root : root), gravity * x) * Mathf.Rad2Deg;
        return true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) lowArc = !lowArc;
        delay -= Time.deltaTime;
        Vector3 direction = TargetPosition - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

        HasSolution = AimAtTarget();
        if (!HasSolution)
        {
            // El tutorial aproxima el tanque cuando el blanco está fuera del alcance.
            transform.Translate(Vector3.forward * (Time.deltaTime * moveSpeed));
            return;
        }
        // Recargar no debe activar accidentalmente la persecución.
        if (automaticFire && delay <= 0f && Vector3.Angle(transform.forward, direction) < 2f)
        {
            Fire();
            delay = shotInterval;
        }
    }

    public bool AimAtTarget()
    {
        float angle = 0f;
        // El muzzle se desplaza cuando la torreta gira. Recalcular evita el
        // desplazamiento fijo de un metro usado como aproximación en el original.
        for (int i = 0; i < 12; i++)
        {
            Vector3 delta = TargetPosition - turret.transform.position;
            float horizontal = new Vector2(delta.x, delta.z).magnitude;
            if (!TryCalculateAngle(horizontal, delta.y, speed, -Physics.gravity.y, lowArc, out angle))
                return false;
            turretBase.localEulerAngles = new Vector3(-angle, 0f, 0f);
        }
        Elevation = angle;
        LaunchVelocity = speed * turretBase.forward;
        return true;
    }

    public void Fire()
    {
        GameObject shell = Instantiate(bullet, turret.transform.position, turret.transform.rotation);
        Rigidbody body = shell.GetComponent<Rigidbody>();
        body.linearVelocity = LaunchVelocity;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        AIShell projectile = shell.GetComponent<AIShell>();
        projectile.owner = this;
        projectile.lowArc = lowArc;
        projectile.target = enemy;
        Collider shellCollider = shell.GetComponent<Collider>();
        foreach (Collider ownCollider in GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(shellCollider, ownCollider);
        Shots++;
    }

    public void RegisterHit(bool low) { TargetHits++; if (low) LowArcHits++; else HighArcHits++; }
}

