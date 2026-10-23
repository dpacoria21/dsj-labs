# Laboratorio 06 - Waypoints

Dos proyectos independientes para la guía de Desarrollo de Software para Juegos. Preparados y comprobados con **Unity 6000.3.15f1**, usando el renderizador 3D integrado (Built-in).

## Abrir y ejecutar

1. En Unity Hub, utiliza **Add / Add project from disk** y selecciona la carpeta del proyecto, no la carpeta `Assets` ni la carpeta `lab06` completa.
2. Abre la escena indicada en la tabla y pulsa **Play**. Los vehículos empiezan a recorrer el circuito automáticamente.
3. Los ejecutables `Builds/Lab06.exe` se generaron localmente y no se versionan. Si descargas el repositorio, abre la escena y pulsa **Play**, o compila para Windows desde **File > Build Profiles** para crear tu propio ejecutable.

| Proyecto | Escena principal | Contenido |
| --- | --- | --- |
| [01_Actividades_Waypoints](01_Actividades_Waypoints) | `Assets/Scenes/01_Actividades_TankRacing.unity` | Cuatro tanques originales, nueve palmeras ordenadas y seguimiento directo con `FollowWP`. |
| [02_Ejercicios_CircuitoAutos](02_Ejercicios_CircuitoAutos) | `Assets/Scenes/02_Circuito_Autos.unity` | Cuatro autos, circuito de asfalto, boxes, gradas, Rigidbody, colisiones y seguimiento de un tracker. |

El proyecto de ejercicios también incluye `Assets/Scenes/01_Tutorial_CircuitoTracker.unity`: completa el tutorial con los tanques antes de aplicar su solución al circuito de autos. El botón **Cambiar: tutorial / autos** permite alternar entre las dos escenas. Así se conservan **dos proyectos**, aunque el segundo contiene las dos etapas del ejercicio.

## Controles y experimentos

| Control | Acción |
| --- | --- |
| Botones de colores o Tab | Seleccionar el vehículo cuyos parámetros se editan. |
| Deslizadores | Cambiar velocidad, velocidad de giro y umbral de llegada o distancia de anticipación. |
| Espacio | Pausar o continuar. |
| R | Reiniciar posiciones y contadores. Mantiene los parámetros elegidos. |
| C | Alternar cámara general y cámara de seguimiento. |
| V | Mostrar u ocultar el recorrido; en ejercicios también muestra los trackers. |
| B, solo ejercicios | Aplicar un impulso lateral al vehículo seleccionado para observar su recuperación. |

En actividades, prueba velocidad 10, giro 6 y umbral 3. Después reduce el giro a 0.5: el vehículo puede orbitar alrededor de un waypoint. Ese comportamiento permite analizar el efecto de velocidad, giro y precisión mostrado en el tutorial. Aumenta el giro o el umbral para recuperar el recorrido.

En ejercicios, activa V para observar el tracker. Se desplaza exactamente entre los puntos y deja de avanzar cuando alcanza la distancia de anticipación respecto al vehículo. El vehículo tiene un cuerpo dinámico y recibe fuerzas en `FixedUpdate`; un choque puede desviarlo sin cambiar el orden de los waypoints. El contador se identifica como **vueltas del tracker** porque mide el avance del objetivo auxiliar.

## Correspondencia con la guía

| Requisito | Implementación |
| --- | --- |
| Actividades 1-5 | Proyecto 3D y escena TankRacing derivados del paquete oficial S0501Starter; se conserva `Assets/TankRacing.unity` como referencia inicial. |
| Actividad 6 | Nueve instancias `PalmTree_WP_00` a `PalmTree_WP_08` bajo `Circuito de 9 palmeras`. |
| Actividades 7-11 | `Assets/Scripts/FollowWP.cs` asignado a GreenTank y arreglo Waypoints poblado en orden. Los pasos de bloqueo del Inspector son acciones de edición; su resultado ya está guardado. |
| Actividades 12-13 | Movimiento automático y parámetros `speed`, `rotSpeed`, `accuracy`, disponibles en Inspector y en el panel. |
| Actividades 14-15 | Los cuatro tanques tienen el mismo circuito con velocidades diferentes. |
| Ejercicio 1 | Escena del tutorial con Rigidbody, BoxCollider, rotación X/Z restringida y ProgressTracker sin collider. |
| Ejercicio 2 | Escenario nuevo y cuatro prefabs de autos propios en `Assets/Prefabs`, con el mismo sistema de seguimiento. |
| Ejercicio 3 | Proyectos y capturas preparados para documentar la solución; la entrega al aula virtual corresponde al estudiante. |

## Organización y edición

- `Assets/Scripts/FollowWP.cs`: seguimiento directo de palmeras, interpolación de giro y vuelta al primer punto.
- `Assets/Scripts/ProgressTracker.cs`: objetivo independiente que recorre el circuito y limita su separación del vehículo.
- `Assets/Scripts/CircuitVehicle.cs`: conducción física hacia el tracker, reinicio e impulso de prueba.
- `Assets/Scripts/WaypointCircuit.cs`: puntos ordenados, línea del recorrido y gizmos.
- `Assets/Scripts/LabDashboard.cs` y `LabCamera.cs`: controles de experimentación y cámaras.
- `Assets/Editor/Lab06Builder.cs`: generación reproducible de las escenas y del ejecutable.
- `Verificacion/`: resultados y capturas obtenidos durante las pruebas reales.

Puedes mover las palmeras o los puntos y cambiar parámetros directamente en las escenas. El menú **Laboratorio 06 > Regenerar escenas del laboratorio** reemplaza las escenas generadas; guarda tus cambios en una copia si deseas conservarlos antes de usarlo.

## Verificación realizada

Se compilaron los dos ejecutables para Windows y se simularon **240 segundos por escena** con pasos de 0.02 segundos. Las pruebas ejecutan la lógica real y PhysX en el reproductor de Unity.

- Actividades: **15 comprobaciones, 0 errores**. Los cuatro tanques completaron entre 10 y 14 vueltas.
- Ejercicios: **46 comprobaciones, 0 errores**. Se probaron ambas escenas: avance de todos los vehículos, recuperación tras un impulso, contacto físico entre vehículos, límite de anticipación, permanencia sobre el suelo y reinicio.
- Las imágenes de `Verificacion` provienen de `Camera.Render` y muestran las escenas; no incluyen el panel IMGUI ni equivalen a una sesión manual de uso del Inspector.

Para repetir las pruebas desde PowerShell, primero compila para Windows en `Builds/Lab06.exe`. Después, dentro de cada proyecto:

```powershell
& .\Builds\Lab06.exe -batchmode --lab06-verify --lab-output "$PWD\Verificacion" -logFile "$PWD\Verificacion\player.log"
```

## Fuentes y recursos

- Guía del curso: **CONT2 2026B DSJ_LAB06_Waypoints**, sesiones de José Sulla Torres y Roxana Limache.
- [Unity Learn - Waypoints](https://learn.unity.com/tutorial/waypoints).
- [Unity Learn - Following a Circuit & Tracker](https://learn.unity.com/tutorial/following-a-circuit).
- Tanques, palmeras, materiales y escena inicial: [S0501Resources.zip, recurso oficial del tutorial](https://unity-connect-prd.storage.googleapis.com/20220912/d06cc434-c3c5-4246-ad9e-22f78f9dd85e/S0501Resources.zip). Se conservaron sus archivos y metadatos originales.
- Autos, carretera y elementos del autódromo: geometría propia generada para este laboratorio. Los cuatro autos se guardaron como prefabs editables.

La guía utiliza Unity 2019.4. Esta implementación conserva el algoritmo del material y lo adapta a Unity 6; utiliza `Rigidbody.linearVelocity` y fuerzas de aceleración para que el movimiento conviva con el motor de física.
