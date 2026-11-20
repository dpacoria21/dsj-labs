# Laboratorio 08 — Flocking en Unity 6

Abre cada carpeta como un proyecto independiente desde Unity Hub con **Unity 6000.3.15f1**. En la ventana Project, abre su escena en `Assets/Scenes` y pulsa **Play**.

| Proyecto | Escena | Controles |
| --- | --- | --- |
| [01_Cardumen](01_Cardumen) | `01_Cardumen.unity` | WASD para desplazar la cámara, Q/E para bajar/subir, botón derecho y ratón para mirar, Shift para acelerar. |
| [02_Enemigos_Boids](02_Enemigos_Boids) | `02_Enemigos_Boids.unity` | Clic izquierdo sobre el suelo para mover al jugador. Mantén Shift izquierdo para detenerlo y orientarlo hacia el cursor. |

En el cardumen, selecciona `Flock` o `Flock (1)` en la jerarquía para ajustar por separado los pesos y los radios de vecindad. Se adaptaron el terreno y el material animado de los peces al pipeline integrado de Unity 6. [Base de GameDevChef](https://github.com/GameDevChef/Flocking/tree/9ad3c2d6b23ec7b8acb575e9346b9f939fb58ece)

En la arena, el prefab `Assets/Game_Src/Prefabs/Enemy.prefab` expone los tres pesos y el radio de detección. Los enemigos combinan las reglas de Boids con el seguimiento del jugador. [Tutorial de Eincode](https://www.youtube.com/watch?v=Srg6GRFspps) · [Commit de referencia](https://github.com/Jerga99/top-down-unity-youtube/tree/c4d57419067f83a59c4e4bb785882af7a26ab3c5)

El [informe del laboratorio](https://docs.google.com/document/d/1XwmQDZFmQC_pnH9OLt60lxhyCuAJzIZI/edit) incluye el desarrollo de las actividades y los ejercicios, el cuestionario y las evidencias de ejecución.

## Verificación

- Cardumen [resultados](Verificacion/01_Cardumen/resultados.txt) y [captura](Verificacion/01_Cardumen/play-mode.jpg).
- Enemigos [resultados](Verificacion/02_Enemigos_Boids/resultados.txt) y [captura](Verificacion/02_Enemigos_Boids/play-mode.jpg).

Las secuencias de ejecución muestran [el cardumen desde la aparición hasta el nado cerca del terreno](Verificacion/01_Cardumen/Secuencia) y [la persecución antes y después de que el jugador cambie de destino](Verificacion/02_Enemigos_Boids/Secuencia).

Cada proyecto incluye la verificación automatizada que genera estos resultados. Con el proyecto cerrado, ejecuta Unity con `-batchmode -force-d3d11 -projectPath "<ruta del proyecto>" -executeMethod Lab08Verification.Run`. Unity entra en Play Mode, ejecuta las pruebas y deja los archivos en `Logs/Lab08`. El código de salida es distinto de cero si falla una comprobación.
