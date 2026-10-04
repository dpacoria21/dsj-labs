# Capturas auténticas del editor de Unity

Imágenes de la ventana real de **Unity 6000.3.15f1**. Se obtuvieron en una copia aislada del proyecto publicado en el commit `0f16ceb4e32c70c1c698aa59307f5ec303e615ff`, mediante la API del editor. Los píxeles proceden de `InternalEditorUtility.ReadScreenPixel` sobre los límites reales de la ventana, sin dibujar, reconstruir ni retocar la interfaz.

- `00_base_predator.png`: Scene, jerarquía e Inspector de Predator; Seek, velocidad 4.6 y referencias.
- `01_base_runner.png`: Scene e Inspector de Runner; Manual, velocidad 5 y referencias.
- `A_game_inspector_1.png`: Play Mode pausado, Game e Inspector de Predator en Seek.
- `A_game_inspector_2.png`: mismo estado de A con Inspector de Runner.
- `B_game_inspector_1.png`: Play Mode pausado, Game e Inspector de Predator en Pursuit.

La entrada de movimiento en estas capturas se automatizó; **no son una sesión humana usando WASD**. El propio panel lo indica. Las capturas del editor pertenecen a una ejecución adicional para documentar configuración y comportamiento, y no deben confundirse con los instantes o mediciones de la prueba de ocho segundos del CSV.

Solo se alteró la selección, la vista y el estado temporal de la copia para capturar. No se modificaron los scripts ni las escenas publicados del laboratorio.
