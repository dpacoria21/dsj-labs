using UnityEngine;

// Los jugadores son sprites planos: giran sobre el eje vertical del marcador para quedar siempre de frente a la
// cámara del celular, aunque el usuario rodee el marcador.
[ExecuteAlways]
public class MiraCamara : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera camara = Camera.main;
        if (camara == null)
            return;
        Vector3 arriba = transform.parent != null ? transform.parent.up : Vector3.up;
        Vector3 haciaAtras = Vector3.ProjectOnPlane(transform.position - camara.transform.position, arriba);
        if (haciaAtras.sqrMagnitude > 1e-8f)
            transform.rotation = Quaternion.LookRotation(haciaAtras, arriba);
    }
}
