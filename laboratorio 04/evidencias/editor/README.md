# Capturas auténticas del editor de Unity

Imágenes de la ventana real de **Unity 6000.3.15f1**. Se obtuvieron en una copia aislada del proyecto publicado en el commit `0f16ceb4e32c70c1c698aa59307f5ec303e615ff`, mediante la API del editor. Los píxeles proceden de `InternalEditorUtility.ReadScreenPixel` sobre los límites reales de la ventana, sin dibujar, reconstruir ni retocar la interfaz.

## Conjunto completo y pies de figura

Son **20 PNG de 1918 x 980**, sin recortar ni componer. Las capturas base se hicieron el 3 de octubre de 2026 a las 23:52 UTC; la serie de Play Mode terminó el 4 de octubre a las 00:02:47 UTC. Console muestra la hora local de Windows, distinta de las marcas UTC del registro.

| Archivo | Escena / selección / pie fiel |
| --- | --- |
| [00_base_predator.png](00_base_predator.png) | `01_A_Seek_Manual`, Predator: Scene, jerarquía e Inspector nativo; Seek, velocidad 4.6 y referencias configuradas. Sin Play. |
| [01_base_runner.png](01_base_runner.png) | `01_A_Seek_Manual`, Runner: Scene e Inspector nativo; Manual, velocidad 5 y referencias configuradas. Sin Play. |
| [A_game_inspector_1.png](A_game_inspector_1.png) | `01_A_Seek_Manual`, Predator: Game y PursuerAgent en Seek, durante Play Mode pausado para la captura. |
| [A_game_inspector_2.png](A_game_inspector_2.png) | Misma escena y estado de A, Runner: EvaderController en Manual. |
| [B_game_inspector_1.png](B_game_inspector_1.png) | `02_B_Pursuit_Manual`, Predator: Game y PursuerAgent en Pursuit; factor 0.12 y máximo 1.5. |
| [B_game_inspector_2.png](B_game_inspector_2.png) | Misma escena y estado de B, Runner: Manual y velocidad 5. |
| [C_base_game_inspector_1.png](C_base_game_inspector_1.png) | `03_C_Pursuit_Evasion`, Runner: Evasion automática, factor 0.12, máximo 1.5. Se ve su posición en el borde X=13. |
| [C_base_game_inspector_2.png](C_base_game_inspector_2.png) | Misma escena y estado de C base, Predator: Pursuit con factor 0.12 y máximo 1.5. |
| [C_reactiva_game_inspector_1.png](C_reactiva_game_inspector_1.png) | `04_C_Reactiva`, Runner: Evasion, factor 0.03 y máximo 0.25. |
| [C_reactiva_game_inspector_2.png](C_reactiva_game_inspector_2.png) | Misma escena y estado, Predator: Pursuit con factor 0.03 y máximo 0.25. |
| [C_anticipada_game_inspector_1.png](C_anticipada_game_inspector_1.png) | `05_C_Anticipada`, Runner: Evasion, factor 0.35 y máximo 3. |
| [C_anticipada_game_inspector_2.png](C_anticipada_game_inspector_2.png) | Misma escena y estado, Predator: Pursuit con factor 0.35 y máximo 3. |
| [V1_Seek_game_inspector_1.png](V1_Seek_game_inspector_1.png) | `06_E1_Seek_RunnerRapido`, Predator: Seek y velocidad máxima modificada a 5. |
| [V1_Seek_game_inspector_2.png](V1_Seek_game_inspector_2.png) | Misma escena y estado, Runner: velocidad máxima modificada a 5.5. |
| [V1_Pursuit_game_inspector_1.png](V1_Pursuit_game_inspector_1.png) | `07_E1_Pursuit_RunnerRapido`, Predator: Pursuit y velocidad máxima 5. |
| [V1_Pursuit_game_inspector_2.png](V1_Pursuit_game_inspector_2.png) | Misma escena y estado, Runner: velocidad máxima 5.5, un 10% mayor. |
| [V2_Captura_game_inspector_1.png](V2_Captura_game_inspector_1.png) | `08_E2_Captura`, Predator: Game muestra captura registrada, Inspector de PursuerAgent. |
| [V2_Captura_game_inspector_2.png](V2_Captura_game_inspector_2.png) | Misma escena y estado, Inspector de Runner en Manual. Runner estuvo inmóvil en esta prueba. |
| [V2_Console_Runner_captured.png](V2_Console_Runner_captured.png) | `08_E2_Captura`, Laboratorio: Console nativa con una entrada **Runner captured**, contador de cero advertencias y errores, y componente CaptureDetector con sus referencias asignadas. |
| [V2_Codigo_Console.png](V2_Codigo_Console.png) | `08_E2_Captura`, activo **CaptureDetector.cs**: vista nativa MonoScript del código `distance < 0.8f`, control de registro único y `Debug.Log`, junto con Console. Los campos None superiores son referencias predeterminadas del activo, no las referencias de la instancia de la escena; estas se ven asignadas en la imagen anterior. |

La entrada de movimiento en estas capturas se automatizó; **no son una sesión humana usando WASD**. El propio panel lo indica. Las capturas del editor pertenecen a una ejecución adicional para documentar configuración y comportamiento, y no deben confundirse con los instantes o mediciones de la prueba de ocho segundos del CSV.

Solo se alteró la selección, la vista y el estado temporal de la copia para capturar. No se modificaron los scripts ni las escenas publicados del laboratorio.

## Procedencia y verificación

La automatización abre las escenas con `EditorSceneManager.OpenScene`, selecciona objetos con `Selection.activeGameObject`, utiliza las ventanas nativas `SceneView`, `UnityEditor.GameView`, `UnityEditor.InspectorWindow` y el menú nativo de Console. Tras iniciar Play Mode, usa la entrada de prueba del proyecto, pausa la ejecución para conservar el estado y toma los píxeles de la ventana completa obtenida por `EditorGUIUtility.GetMainWindowPosition`.

El panel dentro de Game es la interfaz didáctica del proyecto. **El Inspector y Console exteriores son los reales de Unity**, no controles auxiliares que reproduzcan su aspecto. La captura no procede de una RenderTexture ni de una reconstrucción HTML.

Se compararon hashes SHA-256 de los seis scripts de ejecución entre el proyecto publicado y la copia: todos coinciden. El registro [capturas-editor.txt](capturas-editor.txt) identifica fecha UTC, escena y objeto seleccionado para cada imagen de la serie. [series-status.txt](series-status.txt) confirma su terminación; [sha256.txt](sha256.txt) permite verificar la integridad de las 20 PNG.
