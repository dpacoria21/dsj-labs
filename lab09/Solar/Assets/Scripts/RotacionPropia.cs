using UnityEngine;

// Ejercicio 1: rotación sobre su propio eje para que cada planeta se vea animado sobre el marcador.
public class RotacionPropia : MonoBehaviour
{
    [Tooltip("Grados por segundo alrededor del eje Y local.")]
    public float velocidad = 20f;

    void Update()
    {
        transform.Rotate(0f, velocidad * Time.deltaTime, 0f, Space.Self);
    }
}
