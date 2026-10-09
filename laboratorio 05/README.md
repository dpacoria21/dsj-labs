# Laboratorio 05 - Predicción de Trayectoria

Ejercicio de **Ball y Cup con Rigidbody2D**, desarrollado a partir del repositorio que indica la guía, y tutorial complementario **Calculating Trajectories** de Unity Learn. Los dos proyectos completos utilizan **Unity 6000.3.15f1** y siguen la organización de los laboratorios anteriores.

## Abrir el ejercicio principal

1. En Unity Hub selecciona **Add project from disk**.
2. Abre `laboratorio 05/LAB05_PrediccionTrayectoria` con Unity **6000.3.15f1**.
3. Abre `Assets/Scenes/02_Dos_Objetivos_Puntuacion.unity`.
4. Pulsa **Play** y haz clic en **Game** para que reciba la entrada.
5. Haz clic sobre la bola, arrastra en sentido contrario al lanzamiento y suelta el botón izquierdo.

La carpeta elegida en Hub debe contener `Assets`, `Packages` y `ProjectSettings`. Cada proyecto se abre por separado.

## Escenas y controles

| Escena del proyecto 2D | Contenido |
| --- | --- |
| `01_Prediccion_Base.unity` | Una taza, sprites originales y trayectoria previa al lanzamiento. |
| `02_Dos_Objetivos_Puntuacion.unity` | Dos tazas, sensores de llegada y marcador de puntos. |

- **Ratón izquierdo:** comenzar el arrastre sobre Ball y soltar para lanzar.
- **R:** recuperar la bola y recolocar las tazas; conserva la puntuación.
- **N:** iniciar otra partida y borrar puntuación e historial.
- **1 / 2:** cambiar entre las escenas.
- **F12:** guardar una captura de Game en `Capturas` dentro del proyecto.

Cada taza concede **un punto cuando Ball entra en su sensor interior**. La misma taza no repite puntos en el mismo tiro. Un disparo posterior sí puede volver a puntuar.

## Física y predicción

Ball tiene `Rigidbody2D`, `CircleCollider2D`, masa 1 y damping 0. Cada taza tiene `Rigidbody2D` Dynamic, masa 1.5, damping 0.5 y un `EdgeCollider2D` abierto por arriba. Los muros utilizan `BoxCollider2D`.

El material `bounciness` conserva los valores de la guía: **fricción 0.5** y **rebote 0.6**. La gravedad 2D es `(0, -9.81)` y el paso físico es 0.02 s. Las tazas bloquean la rotación para mantener su boca estable.

La ecuación continua es `p(t) = p0 + v0*t + 0.5*g*t*t`, donde `v0 = impulso / masa`. La vista previa incorpora el término `0.5*g*fixedDeltaTime*t` para aproximar la integración discreta del motor. Se muestran hasta 45 puntos separados por 0.05 s y se interrumpe el arco cuando un `CircleCast` detecta el primer obstáculo. La estela verde muestra el recorrido real.

La predicción presupone gravedad constante y damping cero. No calcula rebotes ni el movimiento futuro de las tazas. Entre los pasos físicos la representación es una curva suave; las comprobaciones de precisión usan posiciones del Rigidbody en pasos completos.

## Tutorial complementario

Abre por separado `laboratorio 05/Calculating Trajectories`. Utiliza la escena `Assets/Tanks.unity` y los recursos oficiales **S0311Resources** del tutorial de Unity Learn. Es un ejemplo 3D de tanques que calculan arcos de disparo. Consulta [TUTORIAL.md](docs/TUTORIAL.md) para controles, adaptación y verificaciones.

## Documentación y evidencias

- [Informe con el ejercicio resuelto, los propuestos 1 a 3 y el cuestionario sin responder](docs/INFORME.md).
- [Documento de Drive actualizado](https://docs.google.com/document/d/1ntOIu8F2rpGrAeNx3JvPr9e5l4G8FWw-/edit).
- [Evidencias del proyecto principal](evidencias/README.md).
- [Tutorial y resultados complementarios](docs/TUTORIAL.md).

Las evidencias incluidas se generan en ejecuciones automatizadas reales de Unity. El informe distingue esas imágenes de una captura manual del editor. Los builds, cachés y logs quedan fuera de Git.

## Fuentes

La guía **CONT2 2026B DSJ_LAB05_ENUNCIADO_Prediction.pdf**, de José Sulla Torres y Roxana Limache, proporciona actividades y ejercicios. El PDF no se redistribuye.

- [herbou/Tuto_DrawTrajectory](https://github.com/herbou/Tuto_DrawTrajectory), commit `9348e63d9d3c088568735257941fcc9d8751d309`, licencia MIT conservada.
- [Calculating Trajectories de Unity Learn](https://learn.unity.com/tutorial/calculating-trajectories), paquete oficial S0311Resources.
- [Rigidbody2D.AddForce](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody2D.AddForce.html), documentación de Unity 6.3.
