using UnityEngine;

public class Move : MonoBehaviour {

    public GameObject goal;
    Vector3 direction;
    public float speed = 5.0f;

    void Start() {
        //direction = goal.transform.position - this.transform.position;
        
        // this.transform.Translate(direction);
    }

    void LateUpdate() {

        direction = goal.transform.position - this.transform.position;
        this.transform.LookAt(goal.transform.position);

        if (direction.magnitude > 2.0f ) {
            Vector3 velocity = direction.normalized * speed * Time.deltaTime;
            this.transform.position = this.transform.position + velocity;
        }
    }
}
