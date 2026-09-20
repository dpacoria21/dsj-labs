# Laboratorio 02 — The Mathematics of AI

Los cuatro ejemplos se prepararon con **Unity 6000.3.15f1 (Unity 6.3 LTS)** a partir de los paquetes `Solution` oficiales de Unity Learn. Los ZIP originales no incluían un proyecto completo ni `ProjectVersion.txt`; por eso se crearon proyectos independientes con la instalación local. Los paquetes fuente se conservaron dentro de cada carpeta y no se modificó la lógica educativa de los scripts del tutorial.

## 1. Cartesian Coordinates

- **Escena principal:** `Assets/Scenes/SampleScene.unity`.
- **Qué demuestra:** posición de un punto/objeto en el espacio cartesiano de Unity.
- **Controles:** ninguno; es una demostración visual estática.
- **Cómo ejecutarlo:** abrir la escena y pulsar **Play**, o ejecutar `Builds/Windows/01 Cartesian Coordinates.exe`.
- **Cambios:** importación de `S0201Solution`, escena añadida a Build Settings y build de Windows generado.

## 2. Vectors

- **Escena principal:** `Assets/Moving.unity`.
- **Qué demuestra:** dirección, normalización, magnitud y movimiento de agentes hacia un objetivo.
- **Controles:** **W/S** o **↑/↓** para avanzar/retroceder; **A/D** o **←/→** para girar.
- **Cómo ejecutarlo:** abrir la escena y pulsar **Play**, o ejecutar `Builds/Windows/02 Vectors.exe`.
- **Cambios:** importación de `PigsStarter` y `S0204Solution`; se restauraron los prefabs Pig/Villager y se retiraron tres componentes URP faltantes que no intervienen en el ejercicio. Escena añadida a Build Settings y build generado.

## 3. Dot Product

- **Escena principal:** `Assets/Scenes/Follow.unity`.
- **Qué demuestra:** uso del ángulo/producto punto para limitar el campo de visión y hacer que un agente siga un objetivo solo cuando está delante.
- **Controles:** ninguno; observar el comportamiento automático de los personajes.
- **Cómo ejecutarlo:** abrir la escena y pulsar **Play**, o ejecutar `Builds/Windows/03 Dot Product.exe`.
- **Cambios:** importación de `PigsStarter` y `S0206Solution`; se restauraron los prefabs Pumpkin/Pilgrim/Pig y se retiraron tres componentes URP faltantes ajenos al ejercicio. Escena añadida a Build Settings y build generado.

## 4. Virtual Pet Challenges

- **Escena principal:** `Assets/PetZombie/PetZombie.unity`.
- **Qué demuestra:** un zombi mascota que detecta y sigue al jugador según ángulo y distancia.
- **Controles:** **WASD** o flechas para moverse, **ratón** para mirar, **Espacio** para saltar y **Shift izquierdo** para correr.
- **Cómo ejecutarlo:** abrir la escena y pulsar **Play**, o ejecutar `Builds/Windows/04 Virtual Pet Challenges.exe`.
- **Cambios:** importación de `ZombieStarter` y `S0207Solution`; agregado Input System 1.17.0 y activado como sistema de entrada; se retiraron dos componentes URP faltantes; se conectó `ZombieFollower` al objeto Zombie y su referencia `goal` al Player. Escena añadida a Build Settings y build generado.

## Verificación

Cada proyecto compiló, entró realmente en Play Mode durante una prueba automatizada de 5 segundos con **0 errores del proyecto**, y su ejecutable de Windows se abrió y permaneció funcionando durante la prueba. Los archivos de automatización dentro de `Assets/Editor` solo configuran/verifican el proyecto y no se incluyen en el comportamiento educativo del build.

## 5. Algoritmos de Movimiento en Python

- **Carpeta:** `05 Algoritmos de Movimiento Python`.
- **Ejemplos:** Seeking con Arrive, Wandering continuo y combinación Wandering/Seeking por radio de detección.
- **Tecnología:** Python 3.12 y Pygame 2.6.1.
- **Verificación:** 6 pruebas automáticas y 4 capturas reproducibles en `resultados`.
- **Ejecución:** consultar el `README.md` de la carpeta para instalar dependencias y ejecutar cada ejemplo.

## 6. NPC Movement en Godot

- **Carpeta:** `npc-movement`.
- **Escena principal:** `main.tscn`.
- **Qué demuestra:** dos NPC de fútbol disputan automáticamente una pelota usando el algoritmo Seeking.
- **Representación:** círculos azul y rojo para los personajes y un círculo amarillo más pequeño para la pelota.
- **Comportamiento:** quien no posee la pelota la persigue, la roba al acercarse y pasa a atacar la portería contraria.
- **Controles opcionales:** **Espacio** para pausar y **R** para reiniciar.
- **Ejecución:** importar `npc-movement/project.godot` con Godot 4 y presionar **F5**.
