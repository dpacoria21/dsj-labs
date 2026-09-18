using UnityEngine;

public class ZombieFollower : MonoBehaviour {
    public GameObject goal;
    Vector3 direction;
    public float speed = 5.0f;

    void Start() {

    }

    void LateUpdate() {

        Vector3 targetPosXZ = new Vector3(goal.transform.position.x, this.transform.position.y, goal.transform.position.z);

        direction = targetPosXZ - this.transform.position;

        if (Vector3.Angle(direction, this.transform.forward) < 20.0f) {


            if (direction.sqrMagnitude > 4.0f && direction.sqrMagnitude < 100.0f) {

                this.transform.LookAt(targetPosXZ);
                Vector3 velocity = direction.normalized * speed * Time.deltaTime;
                this.transform.position = this.transform.position + velocity;
            }
        }
    }
}
