# Space Invaders — ejercicio resuelto por el docente

Implementación de la aplicación base de la sección IV del laboratorio: ventana de 800 × 600, fondo, icono, nave, seis enemigos, disparo único, colisiones, puntaje, música y efectos, y GAME OVER.

## Ejecutar

Requiere Python 3.10 o posterior. Abre una terminal en esta carpeta y ejecuta:

```powershell
python -m pip install -r requirements.txt
python main.py
```

En PyCharm, abre esta carpeta como proyecto, selecciona tu intérprete de Python, instala `requirements.txt` en ese intérprete y ejecuta `main.py`.

## Colocar tus recursos

Guarda las imágenes en `src/imagenes/` con estos nombres exactos:

- `background.jpg`: fondo (800 × 600).
- `ufo.png`: icono (32 × 32).
- `player.png`: jugador (64 × 64).
- `enemy.png`: enemigo (64 × 64).
- `bullet.png`: disparo (32 × 32).

El programa adapta los tamaños y conserva la transparencia de los PNG. Si faltan archivos, muestra figuras provisionales. Para reemplazarlas, basta con colocar tus archivos y reiniciar el juego.

Los audios del enunciado van en `src/sonidos/`: `background.wav`, `laser.wav` y `explosion.wav`. Son opcionales. La fuente `freesansbold.ttf` viene incluida en Pygame.

## Controles

- Flechas izquierda/derecha: mover la nave.
- Espacio: disparar; solo puede haber una bala activa.
- Cerrar la ventana: salir.
- Tras GAME OVER, cierra y vuelve a ejecutar para jugar otra vez.

## Explicación del código base

**Límites del escenario.** El jugador se mantiene entre X = 0 y X = 736, descontando los 64 píxeles de su nave. Los enemigos invierten su dirección en esos extremos y bajan 40 píxeles.

**Movimiento de las naves.** El jugador empieza en (370, 480). Se crean seis enemigos en posiciones aleatorias, con Y entre 50 y 150. El bucle se limita a 60 fotogramas por segundo: las naves avanzan 1 píxel por fotograma y la bala sube 10. Se conserva el desplazamiento del ejemplo; si el equipo no alcanza los 60 FPS, el juego avanzará más despacio.

**Colisiones y explosiones.** Como en la guía, se calcula la distancia euclidiana entre las coordenadas del enemigo y la bala. Si es menor que 27 y hay una bala activa, se reproduce `explosion.wav`, se retira la bala y el enemigo reaparece arriba. El ejemplo utiliza un sonido de explosión, no una animación.

**Puntaje.** Cada impacto suma un punto; el contador verde se muestra en (10, 10). Cuando un enemigo supera Y = 440, termina la partida y aparece GAME OVER.

## Ajustes para que el ejemplo funcione correctamente

Se comparan cadenas con `==` en lugar de `is`, se verifican colisiones únicamente con balas activas, se precargan los sonidos, se resuelven las rutas desde `main.py` y se detiene la lógica al terminar la partida. Se añaden recursos provisionales y tolerancia a la ausencia de audio. Son ajustes de funcionamiento del ejercicio base; la propuesta de mejora queda pendiente en la carpeta del estudiante.

## Organización según el PDF

`main.py` sigue el orden de las páginas 4 a 7: importaciones, inicialización, pantalla, fondo, música, título e icono, jugador, listas de enemigos, bala, puntaje, texto de fin, funciones y bucle principal. Se conservan nombres como `playerX`, `enemyX`, `bullet_state` e `isCollision` para facilitar la comparación con el enunciado.

Dentro del bucle se encuentran los eventos de teclado, límites del jugador, movimiento de enemigos, colisiones, movimiento de la bala y dibujo final. La carga y adaptación de recursos queda en `recursos.py` para que esos detalles no interrumpan la lectura del ejercicio.
