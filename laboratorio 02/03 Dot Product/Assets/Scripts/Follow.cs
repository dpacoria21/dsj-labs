using UnityEngine;

public class Follow : MonoBehaviour {

    public GameObject goal;
    Vector3 direction;
    public float speed = 5.0f;

    void Start() {

    }

    void LateUpdate() {

        direction = goal.transform.position - this.transform.position;

        if (Vector3.Angle(direction, this.transform.forward) < 20.0f) {

            this.transform.LookAt(goal.transform.position);

            if (direction.sqrMagnitude > 4.0f) {

                Vector3 velocity = direction.normalized * speed * Time.deltaTime;
                this.transform.position = this.transform.position + velocity;
            }
        }
    }
}
