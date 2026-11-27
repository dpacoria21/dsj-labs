using UnityEngine;

// Anima un Quad con una hoja de sprites (como Sprite2D con hframes/vframes en Godot): cambia el recorte de la
// textura con _MainTex_ST. Los índices de cuadro son los mismos que usa DSJRetro (0 = esquina superior izquierda).
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class SpriteHoja : MonoBehaviour
{
    public int columnas = 6;
    public int filas = 13;
    public int[] cuadros = { 0, 1, 2, 3 };
    public float cuadrosPorSegundo = 6.67f;
    public bool repetir = true;
    [Tooltip("Mira hacia la izquierda (flip_h en Godot).")]
    public bool voltear;

    public int CuadroActual { get; private set; } = -1;

    private Renderer quad;
    private MaterialPropertyBlock bloque;
    private float tiempo;

    private void OnEnable()
    {
        tiempo = 0f;
        Aplicar(cuadros is { Length: > 0 } ? cuadros[0] : 0);
    }

    private void OnValidate() => OnEnable();

    private void Update()
    {
        if (!Application.isPlaying || cuadros == null || cuadros.Length < 2 || cuadrosPorSegundo <= 0f)
            return;
        tiempo += Time.deltaTime;
        int indice = Mathf.FloorToInt(tiempo * cuadrosPorSegundo);
        indice = repetir ? indice % cuadros.Length : Mathf.Min(indice, cuadros.Length - 1);
        Aplicar(cuadros[indice]);
    }

    public void Reproducir(int[] nuevos, float velocidad, bool enBucle)
    {
        cuadros = nuevos;
        cuadrosPorSegundo = velocidad;
        repetir = enBucle;
        tiempo = 0f;
        Aplicar(nuevos[0]);
    }

    public void Mostrar(int cuadro) => Reproducir(new[] { cuadro }, 0f, false);

    private void Aplicar(int cuadro)
    {
        quad ??= GetComponent<Renderer>();
        bloque ??= new MaterialPropertyBlock();
        int columna = cuadro % columnas;
        int fila = cuadro / columnas;
        float ancho = 1f / columnas;
        float alto = 1f / filas;
        // La fila 0 está arriba en la imagen, pero la coordenada V de Unity empieza abajo.
        float v = 1f - (fila + 1) * alto;
        Vector4 recorte = voltear
            ? new Vector4(-ancho, alto, (columna + 1) * ancho, v)
            : new Vector4(ancho, alto, columna * ancho, v);
        quad.GetPropertyBlock(bloque);
        bloque.SetVector("_MainTex_ST", recorte);
        quad.SetPropertyBlock(bloque);
        CuadroActual = cuadro;
    }
}
