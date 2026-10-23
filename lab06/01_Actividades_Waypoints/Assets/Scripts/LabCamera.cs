using UnityEngine;

public class LabCamera : MonoBehaviour
{
    public Transform target;
    public bool follow;
    Vector3 overviewPosition; Quaternion overviewRotation;
    void Awake() { overviewPosition = transform.position; overviewRotation = transform.rotation; }
    public void Toggle() { follow = !follow; if (!follow) transform.SetPositionAndRotation(overviewPosition, overviewRotation); }
    void LateUpdate()
    {
        if (!follow || target == null) return;
        Vector3 wanted = target.position - target.forward * 13 + Vector3.up * 8;
        transform.position = Vector3.Lerp(transform.position, wanted, 1 - Mathf.Exp(-5 * Time.unscaledDeltaTime));
        transform.rotation = Quaternion.LookRotation(target.position + target.forward * 5 + Vector3.up - transform.position);
    }
}
