using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Ejercicio 2: tanda de penales de DSJRetro sobre el marcador. P1 elige hacia dónde patear con los botones
// Arriba, Centro y Abajo (las mismas direcciones con las que se apunta en el juego) y el arquero de la CPU
// adivina un lado. Si adivina, ataja; si no, es gol.
public class PenalesAR : MonoBehaviour
{
    // Cuadros de soccer-player.png usados por las animaciones de scenes/characters/player.tscn.
    public static readonly int[] Quieto = { 0, 1, 2, 3 };
    public static readonly int[] Patada = { 49, 50, 51 };
    public const int PrepararPatada = 48;
    public const int ArqueroAbierto = 24;
    public const int ArqueroEstirado = 25;

    public const int Arriba = 1;
    public const int Centro = 0;
    public const int Abajo = -1;

    [Header("Escena")]
    public SpriteHoja pateador;
    public SpriteHoja arquero;
    public Transform balon;
    public SpriteHoja balonHoja;
    [Tooltip("Centro de la boca del arco, sobre la línea de gol.")]
    public Transform arco;
    public float anchoArco = 0.064f;

    [Header("Interfaz")]
    public Text marcador;
    public Text mensaje;
    public GameObject cartelGol;

    [Header("Juego")]
    public int semilla = 9;
    public float duracionTiro = 0.5f;

    public int Goles { get; private set; }
    public int Atajadas { get; private set; }
    public bool EnJuego { get; private set; }
    public string UltimoResultado { get; private set; } = "";
    // Solo para la verificación automática: fija hacia dónde se lanza el arquero.
    public int? AdivinanzaForzada { get; set; }

    private Vector3 inicioBalon;
    private Vector3 inicioArquero;
    private System.Random azar;

    private void Awake()
    {
        inicioBalon = balon.localPosition;
        inicioArquero = arquero.transform.localPosition;
        azar = new System.Random(semilla);
        mensaje.text = "Elige dónde patear";
        cartelGol.SetActive(false);
        ActualizarMarcador();
    }

    public void Patear(int direccion)
    {
        if (EnJuego)
            return;
        StartCoroutine(Tiro(Mathf.Clamp(direccion, Abajo, Arriba)));
    }

    public void Reiniciar()
    {
        StopAllCoroutines();
        Goles = Atajadas = 0;
        azar = new System.Random(semilla);
        Colocar();
        mensaje.text = "Elige dónde patear";
        ActualizarMarcador();
    }

    private IEnumerator Tiro(int direccion)
    {
        EnJuego = true;
        mensaje.text = "";
        int adivinanza = AdivinanzaForzada ?? azar.Next(Abajo, Arriba + 1);
        bool atajada = adivinanza == direccion;
        float lado = anchoArco * 0.35f;

        pateador.Mostrar(PrepararPatada);
        yield return new WaitForSeconds(0.25f);
        pateador.Reproducir(Patada, 20f, false);

        Vector3 salida = balon.localPosition;
        Vector3 posicionArquero = inicioArquero + new Vector3(0f, 0f, adivinanza * lado);
        Vector3 destino = atajada
            ? posicionArquero + new Vector3(-0.012f, 0.012f, 0f)
            : arco.localPosition + new Vector3(0.012f, 0.014f, direccion * lado);
        arquero.Mostrar(adivinanza == Centro ? ArqueroAbierto : ArqueroEstirado);

        for (float t = 0f; t < duracionTiro; t += Time.deltaTime)
        {
            float k = t / duracionTiro;
            balon.localPosition = Vector3.Lerp(salida, destino, k) + Vector3.up * (0.02f * Mathf.Sin(Mathf.PI * k));
            arquero.transform.localPosition = Vector3.Lerp(inicioArquero, posicionArquero, Mathf.Clamp01(k * 1.6f));
            balonHoja.Mostrar(Mathf.FloorToInt(t * 20f) % 4);
            yield return null;
        }
        balon.localPosition = destino;
        arquero.transform.localPosition = posicionArquero;

        if (atajada)
        {
            // El balón rebota en el arquero y vuelve hacia la cancha.
            Vector3 rebote = destino + new Vector3(-0.07f, -destino.y + inicioBalon.y, 0f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                balon.localPosition = Vector3.Lerp(destino, rebote, t / 0.3f);
                yield return null;
            }
            balon.localPosition = rebote;
            Atajadas++;
            UltimoResultado = "ATAJADA";
            mensaje.text = "¡Atajada de la CPU!";
        }
        else
        {
            Goles++;
            UltimoResultado = "GOL";
            mensaje.text = "¡Gol de P1!";
            cartelGol.SetActive(true);
        }

        ActualizarMarcador();
        pateador.Reproducir(Quieto, 6.67f, true);
        yield return new WaitForSeconds(1.5f);
        cartelGol.SetActive(false);
        Colocar();
        mensaje.text = "Elige dónde patear";
        EnJuego = false;
    }

    private void Colocar()
    {
        balon.localPosition = inicioBalon;
        balonHoja.Mostrar(0);
        arquero.transform.localPosition = inicioArquero;
        arquero.Reproducir(Quieto, 6.67f, true);
        pateador.Reproducir(Quieto, 6.67f, true);
        cartelGol.SetActive(false);
        EnJuego = false;
    }

    private void ActualizarMarcador() => marcador.text = $"P1  {Goles} - {Atajadas}  CPU";
}
