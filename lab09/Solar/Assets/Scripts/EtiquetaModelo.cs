using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Muestra en pantalla el nombre del modelo activo dentro del ImageTarget.
[RequireComponent(typeof(Text))]
public class EtiquetaModelo : MonoBehaviour
{
    public Transform imageTarget;

    private static readonly Dictionary<string, string> nombres = new()
    {
        { "Sun", "Sol" }, { "Mercury", "Mercurio" }, { "Venus", "Venus" }, { "Earth", "Tierra" },
        { "Mars", "Marte" }, { "Jupiter", "Júpiter" }, { "Saturn", "Saturno" }, { "Uranus", "Urano" },
        { "Neptune", "Neptuno" },
    };

    private Text texto;
    private Transform ultimo;

    void Awake()
    {
        texto = GetComponent<Text>();
    }

    void Update()
    {
        Transform activo = null;
        if (imageTarget != null)
        {
            foreach (Transform hijo in imageTarget)
            {
                if (hijo.gameObject.activeSelf)
                {
                    activo = hijo;
                    break;
                }
            }
        }

        if (activo == ultimo)
            return;

        ultimo = activo;
        if (activo == null)
            texto.text = "";
        else
            texto.text = nombres.TryGetValue(activo.name, out string nombre) ? nombre : activo.name;
    }
}
