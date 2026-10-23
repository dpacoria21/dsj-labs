# Ejercicios - Circuito de autos

Abre esta carpeta como proyecto con **Unity 6000.3.15f1**.

- **Ejercicio 1:** `Assets/Scenes/01_Tutorial_CircuitoTracker.unity`. Tanques con física y objetivo auxiliar.
- **Ejercicio 2:** `Assets/Scenes/02_Circuito_Autos.unity`. Aplicación del sistema a un autódromo y cuatro modelos de autos propios.

La escena de autos es la primera del ejecutable `Builds/Lab06.exe`, generado localmente y no incluido en GitHub. Para crearlo, compila para Windows desde **File > Build Profiles**; también puedes usar **Play**. El botón **Cambiar: tutorial / autos** alterna las dos escenas.

Pulsa **Play**. Selecciona un vehículo y cambia velocidad, giro o anticipación. **V** muestra la ruta y los trackers; **B** aplica un impulso lateral; **C** cambia la cámara; **R** reinicia y **Espacio** pausa.

Los autos tienen `Rigidbody` y `BoxCollider`. El tracker no tiene collider y limita cuánto se adelanta; el automóvil gira y acelera hacia él. Las vueltas del panel son las del tracker.

Los modelos editables están en `Assets/Prefabs/Auto_1.prefab` a `Auto_4.prefab`. Consulta `../README.md` para detalles, fuentes y repetición de pruebas. `Verificacion/resultado.txt` registra 46 comprobaciones completadas sin errores en ambas escenas.
