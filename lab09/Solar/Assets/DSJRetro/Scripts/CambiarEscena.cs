using UnityEngine;
using UnityEngine.SceneManagement;

// Botones que pasan del sistema solar a DSJRetro AR y viceversa. Las dos escenas están en Build Profiles.
public class CambiarEscena : MonoBehaviour
{
    public void Cargar(string escena) => SceneManager.LoadScene(escena);
}
