# Laboratorio 09 — Realidad Aumentada con Vuforia

Abre [Solar](Solar) desde Unity Hub con **Unity 6000.4.9f1**. Es la versión instalada que incluye el módulo Android. Después abre la escena `Assets/Scenes/SolarAR.unity`.

| Botón | Acción |
| --- | --- |
| **Previo** | Oculta el modelo actual y muestra el anterior (`ChangeARModel(-1)`). Desde el Sol salta al último. |
| **Siguiente** | Muestra el modelo siguiente (`ChangeARModel(1)`). Desde el último vuelve al Sol. |

El `ImageTarget` contiene los nueve prefabs del paquete SolarSystem, con Position Y = 0.08. Al iniciar solo se ve la Tierra, que es el *Start Model* de [`MultiTargetVuforia`](Solar/Assets/Scripts/MultiTargetVuforia.cs). La guía nombra el archivo `MultiTargetsVuforia`, pero Unity exige que coincida con el nombre de la clase.

## Configuración de Android

- La plataforma es Android y la Graphics API solo usa OpenGLES3 (sin Vulkan).
- El Minimum API Level es **Android 10 (API 29)**. La guía pide API 23, pero Vuforia 11.4 bloquea el build con un valor menor a 29. Además, Unity 6.4 no acepta valores inferiores a 25.
- El backend es IL2CPP con ARM64, como exigen Vuforia y Google Play.
- *Active Input Handling* está en **Both**. El simulador de Vuforia usa el Input System y los botones usan el `StandaloneInputModule`.
- Se usa el pipeline integrado porque los materiales del paquete SolarSystem usan `Legacy Shaders/Diffuse`. Así no hace falta el Render Pipeline Converter.

## Ejercicio 1 — Assets animados

Al final del carrusel hay tres assets animados con `Animator` y clips propios, en [Assets/Animados](Solar/Assets/Animados):

| Asset | Animación |
| --- | --- |
| Tierra y Luna | La Luna orbita una vuelta cada 4 s en una órbita inclinada. La Tierra gira sobre su eje. |
| Sistema Orbital | El Sol rodeado de Mercurio, Venus, la Tierra y Marte, que dan una vuelta cada 3, 5, 8 y 12 s. El clip dura 24 s para que todas las órbitas cierren al repetirse. |
| Cohete | Despega, se balancea y aterriza en 4 s, con partículas de propulsión. |

Además, cada planeta rota sobre su propio eje con [`RotacionPropia`](Solar/Assets/Scripts/RotacionPropia.cs). Arriba de la pantalla, [`EtiquetaModelo`](Solar/Assets/Scripts/EtiquetaModelo.cs) muestra el nombre del modelo activo.

## Ejercicio 2 — DSJRetro AR

El proyecto del curso es DSJRetro (carpeta `djs-retro-arcade-game`), un juego de fútbol arcade hecho en Godot 4.7. La escena `Assets/Scenes/DSJRetroAR.unity` lleva el juego al mismo marcador `SolarSystem.jpg`: la cancha, los jugadores de P1 y de la CPU, el balón y el cartel GOAL! aparecen sobre la imagen y se juega una tanda de penales.

| Botón | Acción |
| --- | --- |
| **Arriba / Centro / Abajo** | P1 patea hacia ese lado. El arquero de la CPU elige un lado al azar: si acierta, ataja. |
| **Reiniciar** | Vuelve el marcador a 0 - 0. |
| **Sistema Solar** | Regresa a `SolarAR`. En `SolarAR`, el botón **DSJRetro AR** abre esta escena. |

- [`PenalesAR`](Solar/Assets/DSJRetro/Scripts/PenalesAR.cs): tiro, atajada, marcador y mensajes.
- [`SpriteHoja`](Solar/Assets/DSJRetro/Scripts/SpriteHoja.cs): anima los quads con los cuadros de `soccer-player.png`, con los mismos índices que DSJRetro (reposo 0–3, carrera 6–11, patada 48–51).
- [`MiraCamara`](Solar/Assets/DSJRetro/Scripts/MiraCamara.cs): los jugadores giran sobre el eje vertical para mirar siempre a la cámara.
- [`exportar_sprites_dsjretro.py`](Solar/Herramientas/exportar_sprites_dsjretro.py): copia los sprites del juego a `Assets/DSJRetro/Sprites` con los colores que Godot aplica con `modulate` y con el shader de paleta (P1 fila 1, CPU fila 8). Los sprites originales son del curso de Nicolas Bize, con licencia MIT.

```powershell
python lab09\Solar\Herramientas\exportar_sprites_dsjretro.py ..\djs-retro-arcade-game
```

Las dos escenas están en Build Profiles, así que el APK incluye ambas.

DSJRetro no necesita realidad aumentada: esta escena es un prototipo de cómo se vería el juego sobre un marcador. Como los jugadores son sprites planos que solo giran sobre el eje vertical, se ven aplastados si la cámara mira el marcador muy inclinada.

## Reconstrucción y verificación

El menú **Lab09** del editor repite los pasos de la guía:

- configurar Android;
- crear los assets animados;
- construir la escena con los botones ya enlazados;
- construir la escena DSJRetro AR (ejercicio 2);
- verificar en Play Mode;
- generar el APK.

Con el proyecto cerrado, se puede ejecutar desde la línea de comandos:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.9f1\Editor\Unity.exe" -batchmode -quit -projectPath "lab09\Solar" -executeMethod Lab09Setup.RunAll
```

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.9f1\Editor\Unity.exe" -batchmode -force-d3d11 -projectPath "lab09\Solar" -executeMethod Lab09Verification.Run
```

La verificación comprueba lo siguiente:
- la configuración de Vuforia (ARCamera, Image Target, licencia, ARCore desactivado);
- que el motor se inicie con la licencia;
- el recorrido del carrusel con los botones, incluida la vuelta en ambos extremos;
- las animaciones y la ausencia de errores;
- en DSJRetro AR: el cambio de escena, las animaciones de reposo y patada, un gol, una atajada, el marcador y Reiniciar.

Los resultados están en [resultados.txt](Verificacion/resultados.txt).

En batchmode el simulador de Vuforia no detecta el marcador (`NO_POSE`), así que el **reconocimiento se prueba en el celular**: se comprobó en un Samsung Galaxy A15 con Android 16 ([capturas](Informe/capturas)). El emulador de Android Studio no sirve: la imagen es x86_64 y Vuforia, que solo trae bibliotecas ARM, se congela al iniciar. Vuforia oculta el contenido mientras no detecta la imagen; para las capturas de [Verificacion](Verificacion), la verificación llama a `OnTrackingFound` de `DefaultObserverEventHandler`, el mismo método que usa Vuforia al detectarla.

[`VuforiaDriverPreload`](Solar/Assets/Editor/VuforiaDriverPreload.cs) precarga `UnityDriver.dll` al abrir el proyecto. Vuforia abre esa DLL por nombre y su carpeta no está en la ruta de búsqueda; sin la precarga, el Play Mode falla con `DRIVER_CONFIG_LOAD_ERROR`.

`Lab09Build.BuildApk` genera `Solar/Builds/Solar.apk`. Esa carpeta no se sube al repositorio.

## Vuforia Engine 11.4.4

La escena usa el **ARCamera** de Vuforia (paso 9). El `ImageTarget` es de tipo *From Image* con `SolarSystem.jpg` (pasos 19–20) y mide 1 × 0.715 unidades. Con ese tamaño, cada planeta (0.1) ocupa una décima parte del ancho del marcador. *Include ARCore Library* está desactivado (paso 16). El Play Mode usa el **simulador** de Vuforia, así que se puede probar sin webcam.

El paquete de Vuforia (138 MB) y la licencia **no se suben al repositorio**. El paquete supera el límite de 100 MB de GitHub y la licencia es personal. Para preparar un clon nuevo:

1. Descarga `add-vuforia-package-11-4-4.unitypackage` desde developer.vuforia.com. Extrae `com.ptc.vuforia.engine-11.4.4.tgz` en `Solar/Packages/`; `manifest.json` ya lo referencia.
2. Guarda tu *App License Key* en `Solar/vuforia-license.txt`.
3. Ejecuta **Lab09 > Reconstruir todo**. Copia la licencia a `Assets/Resources/VuforiaConfiguration.asset` y arma la escena.

Sin el paquete de Vuforia, el proyecto sigue compilando. En ese caso la escena usa una `Preview Camera` y un plano `EditorOnly` con la imagen del marcador.
