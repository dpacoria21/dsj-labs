# Laboratorio 02 — Nivel horizontal de colores

Space Invaders convertido en un nivel horizontal con oleadas de enemigos, trayectorias curvas y absorción de ataques. Reutiliza las imágenes y sonidos del laboratorio 01.

## Ejecutar

Desde esta carpeta:

```powershell
python -m pip install -r requirements.txt
python main.py
```

## Controles

| Tecla | Acción |
| --- | --- |
| W / A / S / D o flechas | Moverse en cualquier dirección |
| Espacio | Disparar hacia la derecha; una bala activa |
| C | Cambiar el escudo entre rojo y azul |
| X | Activar el especial con cinco absorciones |
| R | Reiniciar después de GAME OVER |
| Esc | Salir |

## Oleadas y movimiento

La nave comienza a la izquierda, orientada hacia la derecha. El fondo se desplaza hacia la izquierda para acompañar el avance horizontal. La nave mide 40 × 40 píxeles y puede recorrer el área jugable completa; el movimiento diagonal no aumenta su velocidad.

Cada oleada trae seis monstruos desde el borde derecho, separados por 0,65 segundos: un grupo de tres rojos y otro de tres azules. El color inicial alterna por oleada. Los monstruos tienen su propio color, sin escudos.

Las rutas alternan entre **Serpiente**, **Arco** y **Ondas cruzadas**. Todos avanzan hacia la izquierda mientras suben y bajan siguiendo curvas. La velocidad aumenta gradualmente entre oleadas, hasta un límite de 155 píxeles por segundo.

Cuando todos los integrantes han sido destruidos o han salido por la izquierda, hay una pausa de 1,5 segundos y comienza la siguiente oleada. No hay un número final de oleadas. Un enemigo que escapa no causa GAME OVER.

## Combate y escudo

- Los monstruos disparan bolas de su color hacia la izquierda, tomando como referencia la posición del jugador y añadiendo variación diagonal. Las bolas mantienen su rumbo después de salir; no persiguen al jugador. Los enemigos fuera de pantalla no disparan.
- El mismo color que el escudo se absorbe sin daño y aporta una unidad de energía. Se pueden combinar absorciones de ambos colores cambiando el escudo.
- Cinco absorciones cargan el especial. **X** elimina los enemigos presentes en pantalla, suma un punto por enemigo eliminado y limpia las bolas. No elimina ni puntúa enemigos que aún no hayan entrado; la energía vuelve a cero.
- Una bola de color contrario resta una de las tres vidas. Chocar directamente con un monstruo también resta una vida, independientemente de su color, y no carga energía.
- Después de recibir daño hay un segundo de protección. La partida termina al perder las tres vidas. Reiniciar restaura también las oleadas, temporizadores y energía.

## Archivos y verificación

`main.py` mantiene la organización por bloques del ejercicio: inicialización, jugador, enemigos, disparos, escudo, puntaje, funciones y bucle principal. `start_wave()`, `spawn_enemy()` y `update_waves()` organizan las oleadas y sus curvas; `fire_enemy_attack()` calcula la dirección de las bolas.

`recursos.py` carga las imágenes y sonidos de `src/`. Las orientaciones y colores se aplican en memoria, sin modificar los archivos originales.

Ejecuta `python -m unittest -v test_game` para comprobar oleadas, curvas, disparos, escudo, límites y reinicio. Las pruebas usan pantalla y audio virtuales.
