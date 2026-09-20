# NPC Movement — fútbol con Seeking

Proyecto básico para Godot 4 que representa dos personajes como círculos de colores y una pelota como un punto más pequeño. La simulación es automática: el personaje que no tiene la pelota usa el algoritmo **Seeking** para perseguirla y quitársela al rival.

## Funcionamiento

- El círculo azul ataca hacia la portería derecha.
- El círculo rojo ataca hacia la portería izquierda.
- El poseedor avanza hacia la portería contraria.
- El rival calcula un vector hacia la pelota y aplica `velocidad deseada - velocidad actual`.
- Cuando el perseguidor llega a la distancia de robo, cambia la posesión.
- Después de un robo existe una pausa breve para evitar cambios instantáneos repetidos.
- Si un personaje llega a la portería rival, suma un punto y comienza una nueva jugada.

## Ejecutar

1. Abrir Godot 4.
2. Elegir **Importar**.
3. Seleccionar el archivo `project.godot` de esta carpeta.
4. Presionar **F6** o **F5** para iniciar.

Controles opcionales:

- `Espacio`: pausar o continuar.
- `R`: reiniciar el marcador y las posiciones.

No se utilizan imágenes ni recursos externos; la cancha, los personajes y la pelota se dibujan mediante código.
