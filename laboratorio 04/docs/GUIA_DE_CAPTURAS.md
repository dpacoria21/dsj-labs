# Capturas reales para el documento

Guarda cada imagen desde Unity durante tu ejecución. No presentes las pruebas automáticas incluidas como si hubieras hecho ese recorrido con WASD.

## Antes de comenzar

1. Abre el proyecto y la escena A. Usa una ventana Game con proporción 16:10 (por ejemplo, 1440 x 900).
2. Muestra **Hierarchy**, **Inspector**, **Scene/Game** y **Console** (Window > General > Console).
3. En Scene activa **Gizmos** para ver las líneas de Debug.DrawLine. Game también incluye líneas mediante LineRenderer.
4. Para capturar el editor completo con el Inspector o Console, usa la herramienta de capturas de tu sistema. **F12 solo captura Game**.
5. Conserva los archivos con nombres como `A_inspector_predator.png`, `A_recorrido.png`, etc.

## Escena base

- Sin Play: captura la jerarquía, arena y los agentes en sus posiciones iniciales.
- Selecciona Runner: registra EvaderController, modo Manual, valores y referencias.
- Selecciona Predator: registra PursuerAgent, modo Seek, valores y referencias.
- Selecciona ambas esferas: comprueba escala 0.25 y collider desactivado.
- Comprueba cámara cenital y Plane escala (3,1,3).

## Experimento A

Abre `01_A_Seek_Manual`. Con Play activo, mueve Runner con WASD en curvas amplias. Captura escena/Game y configuración del Inspector. Anota duración y recorrido aproximado.

Observa si Predator apunta a la posición actual, si sigue la estela azul y qué sucede al girar. Evita hacer toda la comparación contra un borde, donde el clamp domina el resultado.

## Experimento B

Detén Play y abre `02_B_Pursuit_Manual`. Los parámetros de movimiento son los de A; solo cambia Seek a Pursuit. Repite aproximadamente la misma trayectoria y duración.

Captura Inspector de Predator y un instante con Runner en movimiento, de modo que se aprecie la esfera cian adelantada. Compara el punto objetivo, recorrido y acercamiento. No basta con una captura del Runner quieto: su velocidad cero hace coincidir las posiciones actual y predicha.

## Experimento C

Abre `03_C_Pursuit_Evasion` y ejecuta sin WASD. Captura Inspector de ambos agentes, la esfera magenta y las trayectorias.

Repite desde las posiciones iniciales con `04_C_Reactiva` y `05_C_Anticipada`. Cada variante cambia **ambos agentes** a los valores documentados. Para aislar únicamente la evasión, usa C base y cambia solo predictionFactor / maxPrediction de Runner en el Inspector, dejando Predator en 0.12 / 1.5.

Registra las parejas de parámetros, tiempo de observación, cercanía al borde y cambios de trayectoria. Un T grande no implica siempre un resultado mejor.

## V.1 - Runner ligeramente más rápido

Compara `06_E1_Seek_RunnerRapido` y `07_E1_Pursuit_RunnerRapido`, repitiendo un recorrido semejante. Captura los maxSpeed del Inspector (5.5 y 5.0) y el comportamiento.

Incluye la modificación propia del constructor o la configuración serializada de ambas escenas junto con los scripts de movimiento. Explica por qué una ruta de intercepción puede reducir distancia aunque Predator sea más lento, sin afirmar que siempre alcanzará al Runner.

## V.2 - Captura

1. Abre `08_E2_Captura`, muestra Console y limpia mensajes anteriores.
2. Pulsa Play y no muevas Runner; Predator se acercará desde (-6,0.5,0).
3. Cuando la distancia baje de 0.8, Console mostrará exactamente **Runner captured**.
4. Captura Console y Game juntos, y otra imagen de `CaptureDetector.cs` que muestre la condición estricta y el registro.
5. Espera: el mensaje no se repite. R reinicia y permite una nueva captura.
6. Documenta que la simulación continúa; la condición es una notificación, no un bloqueo del juego.

## Lista de entrega sugerida

- [ ] Escena base y configuraciones/referencias del Inspector.
- [ ] A: modo, recorrido y observación.
- [ ] B: modo, predicción y comparación con A.
- [ ] C: automático y comparación entre intervalos de predicción.
- [ ] V.1: velocidades modificadas, código/configuración propios y análisis.
- [ ] V.2: código de captura y Console real.
- [ ] Cinco respuestas del cuestionario.
- [ ] Conclusiones basadas en tus observaciones, distinguiendo teoría y prueba.
- [ ] URL del repositorio y commit utilizado.

El repositorio no realiza entregas por correo ni publica el informe ante el docente.
