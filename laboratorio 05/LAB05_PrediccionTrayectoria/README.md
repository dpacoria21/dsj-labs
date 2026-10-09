# LAB05 — Predicción de trayectoria con Rigidbody2D

Abre esta carpeta con Unity Hub usando **6000.3.15f1**. Selecciona `Assets/Scenes/02_Dos_Objetivos_Puntuacion.unity` y pulsa Play. Haz clic sobre Ball, arrastra en el sentido contrario al lanzamiento y suelta el botón izquierdo. Los puntos representan la predicción; la estela verde muestra el movimiento físico real.

- `01_Prediccion_Base`: ejercicio de una taza de la guía.
- `02_Dos_Objetivos_Puntuacion`: segunda taza y puntuación de las entradas válidas.
- **R** recupera la bola y las tazas, conservando puntos; **N** borra puntos e historial.
- **1/2** cambia de escena; **F12** guarda una captura personal de Game en `Capturas`.

La física usa gravedad `(0, -9.81)`, paso 0.02 s, Ball masa 1 y damping 0. Las tazas tienen masa 1.5, damping 0.5, cuerpo Dynamic y borde abierto por arriba. El material de rebote tiene fricción 0.5 y bounciness 0.6, conforme a la guía. La puntuación aumenta una vez por taza y tiro al entrar en su sensor interior; los roces exteriores no suman.

La predicción utiliza `velocidad = impulso / masa`, gravedad escalada por el Rigidbody y la corrección del integrador discreto. Termina en el primer obstáculo y no calcula los rebotes posteriores ni el movimiento futuro de las tazas.

Los assets y scripts proceden del repositorio [herbou/Tuto_DrawTrajectory](https://github.com/herbou/Tuto_DrawTrajectory), commit `9348e63d9d3c088568735257941fcc9d8751d309`, licencia MIT. `Assets/UpstreamSource` conserva los cuatro scripts originales y la escena fuente binaria, junto con la licencia y la procedencia. Las adaptaciones ejecutables están en `Assets/Scripts`; los sprites fuente se conservan.

Consulta [README del laboratorio](../README.md), [informe](../docs/INFORME.md) y [evidencias verificadas](../evidencias/README.md). El constructor y los verificadores están en `Assets/Editor`. `../tools/Verificar-2D.ps1` regenera las dos escenas, compila Windows y ejecuta las comprobaciones físicas; conserva antes cualquier escena personalizada.
