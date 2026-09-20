# Laboratorio 02 — Seeking y Wandering en Python

Implementación en **Python + Pygame** de los tres ejemplos del enunciado:

1. `seeking.py`: el agente persigue el cursor y reduce su velocidad al entrar en el radio de llegada.
2. `wandering.py`: movimiento de patrulla suave mediante un objetivo que cambia sobre un círculo proyectado.
3. `seeking_wandering.py`: un NPC patrulla con Wandering y cambia a Seeking/Arrive cuando el jugador entra en su radio de detección.

La función compartida `movement_core.py` contiene los cálculos vectoriales y evita errores al normalizar vectores de magnitud cero.

## Instalación

Desde esta carpeta:

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
```

## Ejecución

```powershell
python seeking.py
python wandering.py
python seeking_wandering.py
```

Controles:

- **Mouse:** objetivo o jugador.
- **V:** mostrar u ocultar vectores y radios de depuración.
- **M:** agregar otro agente en los ejemplos individuales.
- **Espacio:** pausar los ejemplos individuales.
- **Esc:** cerrar.

## Significado de los colores

- **Verde:** velocidad actual.
- **Rojo:** velocidad deseada.
- **Cian:** objetivo de movimiento o círculo/objetivo de Wandering.
- **Blanco:** radio de detección o radio de llegada.
- **Amarillo:** agente autónomo.
- **Azul:** jugador controlado por el cursor.

## Pruebas y capturas reproducibles

```powershell
python -m pytest -q
python seeking.py --capture resultados\01_seeking.png
python wandering.py --capture resultados\02_wandering.png
python seeking_wandering.py --capture-dir resultados
```

Las capturas usan semillas y pasos de tiempo fijos, por lo que documentan resultados reproducibles.
