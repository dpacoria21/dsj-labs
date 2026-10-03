# Laboratorio 04 - Pursuit y Evasion

Proyecto completo para **Unity 6000.3.15f1 (Unity 6)**, con ocho escenas, scripts C#, materiales, configuración y documentación. Sigue la sesión 4 de DSJ: Predator rojo y Runner azul sobre una arena 3D cenital.

## Clonar y abrir

```powershell
git clone https://github.com/dpacoria21/dsj-labs.git
```

Si ya tienes el repositorio, ejecuta `git pull` desde tu copia, conservando tus cambios locales.

1. En Unity Hub, elige **Add / Add project from disk**.
2. Selecciona **dsj-labs/laboratorio 04/LAB04_PursuitEvasion**. Esta carpeta contiene `Assets`, `Packages` y `ProjectSettings`; no selecciones la raíz del repositorio.
3. Abre con **6000.3.15f1**, versión instalada y utilizada para las pruebas. La guía pide Unity 6 sin fijar un parche.
4. Abre `Assets/Scenes/01_A_Seek_Manual.unity` y pulsa **Play**.
5. Haz clic en **Game** para que reciba el teclado. Usa **WASD**.

No necesitas los tres ZIP/unitypackage del aula para abrir esta recreación. Se construyó con primitivas de Unity y los algoritmos descritos en la guía; esos paquetes no estaban adjuntos y no se han importado.

## Escenas

Todas están incluidas en Build Profiles. El panel del juego permite cambiar de escena.

| Escena en Assets/Scenes | Predator | Runner | Uso |
| --- | --- | --- | --- |
| 01_A_Seek_Manual | Seek | Manual | Escena base y experimento A |
| 02_B_Pursuit_Manual | Pursuit | Manual | B; solo cambia el modo del Predator respecto a A |
| 03_C_Pursuit_Evasion | Pursuit | Evasion | C automático, factor 0.12 y máximo 1.5 s |
| 04_C_Reactiva | Pursuit | Evasion | C: factor 0.03, máximo 0.25 s |
| 05_C_Anticipada | Pursuit | Evasion | C: factor 0.35, máximo 3 s |
| 06_E1_Seek_RunnerRapido | Seek | Manual | V.1: Runner 5.5, Predator 5.0 |
| 07_E1_Pursuit_RunnerRapido | Pursuit | Manual | V.1: mismas velocidades para comparar |
| 08_E2_Captura | Pursuit | Manual | V.2: distancia estrictamente menor que 0.8 |

Los valores base de la guía ya hacen más rápido al Runner (5 frente a 4.6). El ejercicio V.1 **modifica ambos valores a 5.5 y 5.0** para cumplir la consigna, manteniendo al Runner un 10% más rápido.

## Controles y colores

- **WASD:** mover Runner en modo Manual; las diagonales se normalizan.
- **R:** reiniciar la escena y la condición de captura.
- **F12:** guardar una captura real de Game en `LAB04_PursuitEvasion/Capturas`.
- Botones del panel: abrir cada variante. Detén Play antes de guardar cambios permanentes en el Inspector.
- Runner azul, Predator rojo, esfera de predicción del Runner cian, esfera de predicción del Predator magenta.
- Línea amarilla: hacia dónde se dirige Predator. Cian y magenta: extrapolaciones de ambos agentes. Las estelas muestran el recorrido reciente.

## Configuración base

Arena Plane escala (3,1,3); Runner (6,0.5,0); Predator (-6,0.5,0); cámara (0,20,0), rotación (90,0,0), ortográfica. Marcadores esfera escala 0.25 y collider desactivado. Se asignaron todas las referencias en las escenas.

| Parámetro | Runner | Predator |
| --- | --- | --- |
| maxSpeed | 5 | 4.6 |
| maxAcceleration | 10 | 9 |
| predictionFactor | 0.12 | 0.12 |
| maxPrediction | 1.5 | 1.5 |
| arenaLimit | 13 | 13 |

Se usa **Input Manager (Old)** porque la guía lee `UnityEngine.Input.GetKey`. La escena ya está configurada; no hace falta instalar Input System.

## Código y decisiones

- `EvaderController.cs`: WASD / Evasion, velocidad pública de solo lectura, predicción y límites.
- `PursuerAgent.cs`: Seek / Pursuit y steering con aceleración limitada.
- `CaptureDetector.cs`: condición `distance < 0.8f` y mensaje exacto **Runner captured**, una sola vez por ejecución. No detiene ni modifica el movimiento; R permite repetir.
- `PredictionVisuals.cs`: líneas en Game/build además de los `Debug.DrawLine` del algoritmo.
- `LabSession.cs`: panel, navegación y F12.
- `LabRuntimeVerification.cs`: pruebas opcionales activadas únicamente con `--lab-verify`; las escenas normales leen WASD.
- `Assets/Editor`: generación explícita de escenas, verificaciones y build.

La lógica de la guía conserva `desiredVelocity - Velocity`, limitación de aceleración y velocidad, integración con `deltaTime`, orientación y clamp de posición. Se extrajo `Step` para comprobar propiedades de movimiento, sin cambiar la integración. No hay Rigidbody ni respuesta física entre cápsulas: la captura usa distancia.

**Límite del ejemplo:** el clamp conserva la velocidad interna aunque el agente esté contra el borde. Por ello, una predicción puede situarse fuera de la arena y un evasor puede quedar atrapado. Es una simplificación de la guía, no una garantía de intercepción óptima ni de evasión exitosa.

## Evidencias e informe

- [Guía para ejecutar y obtener capturas](docs/GUIA_DE_CAPTURAS.md).
- [Cuestionario explicado y plantilla del informe](docs/INFORME.md).
- [Resultados y alcance de las pruebas reales](evidencias/README.md).

Las imágenes de `evidencias` son renders reales de la cámara de Unity durante una prueba automatizada. No incluyen el panel, Inspector o Console, ni representan una sesión humana con WASD. Sus parámetros y resultados están en el CSV. Las instrucciones explican cómo obtener las capturas completas en tu PC.

## Repetir las verificaciones

Consulta [tools/Verificar.ps1](tools/Verificar.ps1). Con Unity cerrado para este proyecto:

```powershell
powershell -ExecutionPolicy Bypass -File ".\tools\Verificar.ps1" -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe"
```

El script recompila y **regenera las ocho escenas del laboratorio** desde su constructor; úsalo antes de personalizarlas o guarda tus cambios en otra escena. El build, cachés, capturas personales y logs quedan fuera de Git. En el editor también puedes usar **Laboratorio 04 > Verificar configuración y algoritmos**; esa verificación abre escenas, por lo que debes guardar antes cualquier cambio propio.

## Fuentes

Implementación basada en `CONT1 2026B DSJ_LAB04_Pursuit_Evasion_Unity.pdf`, sesión 4, José Sulla Torres y Roxana Limache, UNSA, 2026B. El PDF del docente no se redistribuye en este repositorio.

Documentación consultada: [Time.deltaTime](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Time-deltaTime.html), [Debug.DrawLine](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Debug.DrawLine.html), [argumentos del editor](https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html).
