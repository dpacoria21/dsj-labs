using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script de la guía (paso 31). Va en el ImageTarget y recorre sus hijos como un carrusel:
// solo uno está activo y ChangeARModel(-1 / 1) pasa al anterior o al siguiente, dando la vuelta en los extremos.
// La guía llama al archivo "MultiTargetsVuforia", pero Unity exige que el archivo y la clase tengan el mismo nombre.
public class MultiTargetVuforia : MonoBehaviour
{
    [SerializeField] private GameObject startModel;
    private int modelsCount;
    private int indexCurrentModel;

    // Start is called before the first frame update
    void Start()
    {
        modelsCount = transform.childCount;
        indexCurrentModel = startModel.transform.GetSiblingIndex();
    }

    public void ChangeARModel(int index)
    {
        transform.GetChild(indexCurrentModel).gameObject.SetActive(false);
        int newIndex = indexCurrentModel + index;
        if (newIndex < 0)
        {
            newIndex = modelsCount - 1;
        }
        else if (newIndex > modelsCount - 1)
        {
            newIndex = 0;
        }
        GameObject newModel = transform.GetChild(newIndex).gameObject;
        newModel.SetActive(true);
        indexCurrentModel = newModel.transform.GetSiblingIndex();
    }
}
