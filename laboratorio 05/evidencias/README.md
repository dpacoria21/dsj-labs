# Evidencias del laboratorio 05

La carpeta [2d](2d/README.md) contiene nueve renders reales de Unity **6000.3.15f1**, los resultados de **29 comprobaciones del editor** y **34 comprobaciones en ejecución**, el CSV de vuelo/impactos y el registro de las entradas válidas a los objetivos. Las dos verificaciones terminaron con **0 errores**.

Las imágenes del proyecto principal proceden de la cámara de Unity, renderizada a un RenderTexture de **1440 × 1080**. Los marcadores de puntuación y tiro pertenecen a la escena. No son capturas del Inspector o Console, ni incluyen el panel IMGUI. La entrada se automatiza mediante los mismos métodos de arrastre que utiliza el juego; la simulación física es la real de Rigidbody2D/Box2D.

Para el informe, las imágenes más útiles son:

- [Predicción en la escena base](2d/02_Prediccion_base.png).
- [Entrada en el objetivo base](2d/03_Entrada_objetivo_base.png).
- [Predicción hacia A con dos tazas](2d/05_Prediccion_objetivo_A.png).
- [Primer punto al entrar en A](2d/06_Punto_objetivo_A.png).
- [Predicción hacia B](2d/08_Prediccion_objetivo_B.png).
- [Tercer punto al entrar en B](2d/09_Punto_objetivo_B.png).

El error máximo medido en los cuatro vuelos libres es **0.0000029 unidades de Unity**. Las condiciones combinan masas 1/2 y gravityScale 1/0.6 durante 0.8 s. La puntuación comprobada en la extensión avanza **1 → 2 → 3** al disparar hacia **A → A → B**. Los registros también verifican duplicados del mismo tiro, recuperación R, nueva partida N y rebote en el suelo. Consulta [resultados-runtime-2d.csv](2d/resultados-runtime-2d.csv) y [resultado-runtime-2d.txt](2d/resultado-runtime-2d.txt) para el alcance exacto.

La carpeta [tutorial](tutorial/) reúne los renders y resultados del proyecto complementario **Calculating Trajectories**, desarrollado a partir de los recursos oficiales de Unity Learn. Sus controles, ejecución, adaptación y resultados se explican en [TUTORIAL.md](../docs/TUTORIAL.md).

Para repetir el proyecto principal, ejecuta [Verificar-2D.ps1](../tools/Verificar-2D.ps1) con Unity cerrado para ese proyecto. El script regenera ambas escenas y las evidencias; conserva antes tus cambios en otra escena. Para producir capturas personales de Game con el panel del juego, abre una escena, pulsa Play y utiliza F12.
