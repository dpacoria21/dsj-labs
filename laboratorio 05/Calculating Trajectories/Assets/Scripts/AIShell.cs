// Adaptación de AIShell (S0311Resources, Unity Learn) a la API Unity 6.
using UnityEngine;

public class AIShell : MonoBehaviour
{
    public GameObject explosion;
    [HideInInspector] public GameObject target;
    [HideInInspector] public FireShell owner;
    [HideInInspector] public bool lowArc;
    Rigidbody body;

    void Awake() { body = GetComponent<Rigidbody>(); }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == target && owner != null)
        {
            owner.RegisterHit(lowArc);
            Debug.Log("TUTORIAL_TARGET_HIT target=" + collision.gameObject.name);
        }
        if (explosion != null)
        {
            GameObject effect = Instantiate(explosion, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
        Destroy(gameObject);
    }

    void Update()
    {
        if (body.linearVelocity.sqrMagnitude > 0.001f) transform.forward = body.linearVelocity;
    }
}

