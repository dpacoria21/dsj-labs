# Evidencias reales del proyecto 2D

Generadas con Unity **6000.3.15f1** el **9 de octubre de 2026**, mediante el build Windows del proyecto `LAB05_PrediccionTrayectoria`. El build y los logs completos son archivos locales ignorados por Git. `resultado-editor-2d.txt` registra **29 comprobaciones**; `resultado-runtime-2d.txt`, **34 comprobaciones**. Ambos terminaron con **0 errores**.

Las nueve PNG son renders reales de `Camera.main` a un RenderTexture de 1440 × 1080. Incluyen los textos de puntuación y estado que existen dentro de la escena. **No contienen Inspector, Console ni la interfaz IMGUI del juego**. La entrada de arrastre es automatizada y reproducible: llama los mismos métodos `OnDragStart`, `OnDrag`, `OnDragEnd` que utiliza el ratón. No representa una sesión humana.

| Archivo | Estado observado |
| --- | --- |
| 01_Escena_base.png | Escena inicial: bola, una taza, muros y soporte |
| 02_Prediccion_base.png | Predicción mientras se prepara el tiro hacia A |
| 03_Entrada_objetivo_base.png | Entrada física en A, sin puntuación en la escena base |
| 04_Dos_objetivos.png | Extensión con tazas A y B y puntuación inicial 0 |
| 05_Prediccion_objetivo_A.png | Predicción del primer tiro hacia A |
| 06_Punto_objetivo_A.png | Primer impacto interior real: puntuación 1 |
| 07_Segundo_tiro_A.png | Otro tiro hacia A: puntuación 2 |
| 08_Prediccion_objetivo_B.png | Predicción hacia B por encima de A, puntuación 2 |
| 09_Punto_objetivo_B.png | Entrada física en B: puntuación 3 y arco real de vuelo |

Los puntos claros representan la predicción hasta el primer obstáculo; la línea verde es la estela real del Rigidbody2D. La línea cian de las imágenes de preparación indica el arrastre. Los saltos de recuperación de la bola no se dibujan como vuelos.

`resultados-runtime-2d.csv` mide cuatro vuelos libres de **0.8 s** con masas **1 y 2** y gravityScale **1 y 0.6**, utilizando `Physics2D.Simulate(0.02)` y Box2D real. El error máximo de la predicción ajustada al integrador es **0.0000029 unidades de Unity**. La fórmula continua sin ajuste presenta aproximadamente **0.07848 unidades** de diferencia con gravityScale 1 y **0.04709** con 0.6, por el método numérico de integración. Las columnas de error se interpretan únicamente en las filas `vuelo_libre`; los ceros de las filas de impacto/rebote son campos no medidos, no una comparación de predicción después de colisionar.

El verificador también comprueba entradas reales en A y B, que un duplicado de registro de la misma taza y tiro no sume otra vez, que R conserve la puntuación, que N la borre, y que un rebote en el suelo no puntúe. El rebote real produjo una razón de velocidades verticales de **0.61755** entre pasos consecutivos, compatible con bounciness 0.6 más la gravedad aplicada durante el paso. `eventos-impactos-2d.txt` conserva los mensajes reales de los cuatro impactos válidos.

Para repetir, cierra el proyecto en Unity y ejecuta `tools/Verificar-2D.ps1` desde el laboratorio. El script regenera las dos escenas desde el constructor, compila el build y sustituye estas evidencias con una nueva ejecución; conserva antes cualquier escena personalizada. Para obtener capturas personales de Game con la interfaz del juego, abre una escena, pulsa Play y usa F12.
