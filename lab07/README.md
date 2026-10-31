# Laboratorio 07 - Pathfinding 2D

Implementación de las **actividades 1 a 17 y los ejercicios 1 y 2** de la guía. Proyecto preparado con **Unity 6000.3.15f1**, renderizador integrado y **A* Pathfinding Project Free 4.2.17** original.

El **ejercicio 3** corresponde al [informe del laboratorio en Google Docs](https://docs.google.com/document/d/1ep0wA9GwCp9leEoG7QhGuMETSopAP2Lj/edit), que incluye las capturas, el cuestionario, siete conclusiones y referencias y citas IEEE. Las tres escenas pertenecen al mismo proyecto de Unity.

## Abrir y ejecutar

1. En Unity Hub, selecciona **Add project from disk** y abre `01_Actividades_Pathfinding`, dentro de `lab07`. No selecciones la carpeta `Assets`.
2. Abre `Assets/Scenes/01_Actividades_Pathfinding.unity`.
3. Pulsa **Play**. El enemigo comienza a perseguir al jugador automáticamente y la línea verde muestra la ruta calculada.

También puedes ejecutar `01_Actividades_Pathfinding/Builds/Lab07.exe`, generado localmente. `Builds` y `Library` están excluidos del repositorio; para regenerar el ejecutable utiliza **File > Build Profiles > Windows**.

| Control | Acción |
| --- | --- |
| Flechas o WASD | Mover al jugador con marcador azul. |
| V | Mostrar u ocultar la ruta. |
| R | Reiniciar las posiciones del jugador y el enemigo. |
| Espacio | Pausar o continuar. |

El enemigo tiene un marcador naranja. Los personajes usan el sprite original enlazado por la guía. El movimiento se desarrolla en XY, con cámara ortográfica y física 2D sin gravedad.

## Correspondencia con los 17 pasos

| Paso | Resultado implementado |
| --- | --- |
| 1 | Proyecto en modo 2D y escena rectangular con cuatro paredes exteriores y cuatro interiores, siguiendo la distribución de referencia. |
| 2 | Personaje descargado del enlace de Google Drive e importado como sprite en `Assets/Sprites/Player.png`. |
| 3 | `PlayerScript` asignado a Player; velocidad inicial 5 y movimiento con las flechas. |
| 4 | Objeto Enemy con el sprite del personaje. |
| 5 | Las ocho paredes llevan `BoxCollider2D`. |
| 6 | Paquete gratuito original A* Pathfinding Project descargado del sitio de Aron Granberg. |
| 7 | Paquete desempaquetado en `Assets/AstarPathfindingProject`, con sus scripts, metadatos y recursos. |
| 8 | GameObject `PathFinding` con el componente original `AstarPath`, que el Inspector del paquete presenta como Pathfinder. |
| 9 | Grid Graph en XY, modo 2D y `Use 2D Physics` activado. |
| 10 | Capa 8 `Environment` asignada a las paredes y seleccionada en Obstacle Layer Mask. |
| 11 | Grafo escaneado y guardado con nodos; se vuelve a escanear al iniciar. |
| 12 | Enemy incluye `Seeker` y `AIPath` para calcular y recorrer la ruta. |
| 13 | `AIDestinationSetter` de Enemy apunta al Transform de Player. |
| 14 | La ejecución muestra el camino mediante una línea verde, además de los gizmos del paquete en el Editor. |
| 15 | Script `EnemyMovement` agregado al hijo `Enemy/Graphics`. |
| 16 | Orientación mediante `AIPath.desiredVelocity` y `transform.right`, con referencia asignada. |
| 17 | Escena ejecutable y compilación para Windows. Los resultados de verificación se guardan en `Verificacion`. |

## Configuración y adaptaciones

- Grafo de 98 × 58 nodos, tamaño de nodo 0.25. El test de obstáculos usa círculos con diámetro 3.2 nodos, equivalente a 0.8 unidades, para reservar espacio para el personaje.
- Se desactiva el corte de esquinas para evitar atravesar las aristas de las paredes.
- El enemigo utiliza `AIPath` del paquete original, no una sustitución del algoritmo. Su velocidad es 2.8 y solicita rutas periódicamente hacia la posición actual del jugador.
- La anticipación de seguimiento es 0.3 y la aceleración es 25: estos valores permiten negociar las esquinas de este mapa sin apuntar demasiado lejos al otro lado de una pared.
- `PlayerScript` conserva los controles y la velocidad de la guía, pero aplica la velocidad a `Rigidbody2D` en `FixedUpdate`, para respetar los colliders. Se limita la magnitud de entrada para evitar mayor velocidad en diagonal.
- `EnemyMovement` conserva la lógica de orientación del PDF y evita orientar el sprite cuando la velocidad deseada es cero. La rotación automática del agente está desactivada para que la orientación la controle el hijo `Graphics`.
- El panel, los marcadores de color y la cuadrícula visual facilitan observar la actividad; no cambian la distribución de obstáculos ni el cálculo de A*.

## Edición y regeneración

Puedes editar la escena directamente. Después de mover paredes, selecciona `PathFinding` y pulsa **Scan** en su Inspector. El menú **Laboratorio 07 > Regenerar escena de actividades** reconstruye la escena; guarda una copia antes de usarlo si realizaste cambios propios.

Archivos principales:

- `Assets/Scripts/PlayerScript.cs`: entrada y movimiento físico del jugador.
- `Assets/Scripts/EnemyMovement.cs`: dirección del sprite enemigo.
- `Assets/Scripts/Lab07Dashboard.cs`: controles de observación y dibujo de la ruta recibida del Seeker.
- `Assets/Editor/Lab07Builder.cs`: creación reproducible del escenario y compilación.
- `Assets/Scripts/Lab07Verification.cs`: comprobaciones opcionales en el reproductor de Unity, activadas únicamente con `--lab07-verify`.

Para repetir las comprobaciones, desde la carpeta del proyecto y después de compilar:

```powershell
& .\Builds\Lab07.exe -batchmode --lab07-verify --lab-output "$PWD\Verificacion" -logFile "$PWD\Verificacion\player.log"
```

Las imágenes de esa verificación proceden de `Camera.Render` en el juego real. Muestran el escenario y la ruta; no capturan el panel IMGUI ni son capturas de acciones intermedias de importación en el Editor.

### Resultado de la verificación

La compilación para Windows terminó correctamente. Se ejecutaron **12 comprobaciones con 0 fallos** en el reproductor real: grafo y máscara de obstáculos, referencias del agente, llegada a cuatro destinos, recálculo de rutas, respeto de paredes por el enemigo y colisión del jugador con el perímetro. El enemigo recorrió 50.49 unidades durante estas pruebas, sin solapamientos ni cruces de paredes. El detalle está en `01_Actividades_Pathfinding/Verificacion/resultados.json`.

También se abrió la escena y se comprobó visualmente su ejecución con Play dentro de Unity. El paquete antiguo puede mostrar un aviso de fallo al consultar actualizaciones; la compilación y la navegación funcionan sin esa consulta.

## Recursos originales

- Guía: `CONT2 2026B DSJ_LAB07_Pathfinding.pdf`, páginas 2 a 9.
- [Sprite indicado por la guía](https://drive.google.com/file/d/1Epzoo14fYCxe6VsyiHPyt3BMpFhn3_p-/view).
- [A* Pathfinding Project Free](https://arongranberg.com/astar/download), versión 4.2.17.
- `Recursos/Player_original.png`: copia del recurso descargado. El archivo `.unitypackage` descargado se conserva localmente en `Recursos` y está excluido por el `.gitignore` del repositorio.

Se conservan los avisos y recursos originales del paquete importado. El ejercicio 2 de Unity Learn también está implementado; véase la sección correspondiente.

## Ejercicio 1: más obstáculos y otro Enemy

Abre `Assets/Scenes/02_Ejercicio_1_DosEnemigos.unity` y pulsa Play, o ejecuta `Builds/Ejercicio1/Lab07_Ejercicio1.exe`. La escena de las actividades se conserva por separado.

- Se añadieron cuatro paredes (`wall_Extra_1` a `wall_Extra_4`), para un total de doce. Todas tienen `BoxCollider2D` y capa `Environment`. Se escaneó y guardó nuevamente el grafo.
- `Enemy_2` parte de (9, 4), conserva `Seeker`, `AIPath`, `AIDestinationSetter` con destino Player y su referencia propia en `EnemyMovement`.
- El enemigo original tiene marcador naranja y ruta verde; el segundo tiene marcador y ruta violeta. Ambos persiguen al jugador. R reinicia las posiciones de los tres personajes; V alterna las dos rutas.
- No se añade evasión entre enemigos: pueden coincidir al alcanzar al jugador. Los obstáculos de este ejercicio son las paredes.

La compilación Windows terminó correctamente. La prueba real del ejecutable pasó **27 comprobaciones con 0 fallos**, incluidas seis llegadas (dos enemigos por tres destinos), bloqueo de nodos en las doce paredes y 0 solapamientos o cruces con las paredes. Los enemigos recorrieron 41.21 y 39.36 unidades y recalcularon once rutas cada uno durante el recorrido medido. El resultado está en `01_Actividades_Pathfinding/Verificacion/Ejercicio1/resultados.json`; las cuatro capturas del informe están en `Verificacion/Ejercicio1/Informe`.

Para regenerar esta escena y su ejecutable, usa **Laboratorio 07 > Crear ejercicio 1 y compilar**. Reconstruye el ejercicio desde la escena de actividades, por lo que conviene guardar una copia si has realizado modificaciones propias en el ejercicio.

```powershell
& .\Builds\Ejercicio1\Lab07_Ejercicio1.exe -batchmode -nographics --lab07-exercise-verify --lab-output "$PWD\Verificacion\Ejercicio1" -logFile "$PWD\ejercicio1-test.log"
```

## Ejercicio 2: Unity Learn, The A* Algorithm

Abre `Assets/Scenes/03_Ejercicio_2_UnityLearn_AStar.unity` y pulsa Play. Adaptación 2D del tutorial oficial y de su código S0407SolutionU6, consultado como referencia y conservado en `Recursos/UnityLearnEjercicio2`. La cuadrícula se dibuja con IMGUI; no utiliza AIPath para resolver A*.

P elige inicio/meta aleatorios libres; C expande un nodo; Espacio alterna avance automático; M muestra la ruta final. Se visualizan abiertos, cerrados, G/H/F y padres. Se actualizan nodos abiertos solo con un coste menor y se trata el caso sin camino. Se usan cuatro vecinos, coste unitario y heurística euclídea. El mapa tiene 17 × 13 celdas.

`Lab07LearnAStar.cs` contiene la búsqueda y visualización. **Laboratorio 07 > Crear ejercicio 2 Unity Learn** regenera la escena y comprueba 100 pares aleatorios contra BFS y un caso sin ruta: **101 pruebas correctas**. Resultado en `Verificacion/Ejercicio2/resultados.txt`. Capturas reales del Editor en `Verificacion/Ejercicio2/Informe`.

Fuente: https://learn.unity.com/course/artificial-intelligence-for-beginners/unit/a-36369ng/tutorial/debugging-and-tweaking-a?version=6.0

