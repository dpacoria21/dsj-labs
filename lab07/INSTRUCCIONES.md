# Laboratorio 07: Pathfinding - análisis e instrucciones

## Alcance de este archivo

Esta guía explica qué solicita el PDF `CONT2 2026B DSJ_LAB07_Pathfinding.pdf` y propone un orden para resolverlo. Fue preparada inicialmente como planificación. Las actividades 1 a 17 y los ejercicios 1 y 2 ya se implementaron en `01_Actividades_Pathfinding`; consulta `README.md` para abrir sus escenas y revisar los resultados. El informe de Google contiene el ejercicio 3, las explicaciones y capturas, el cuestionario, siete conclusiones y referencias y citas en formato IEEE.

El archivo se identifica como LAB07, aunque su portada dice «Sesión 6: Pathfinding». Se usa `lab07` para mantener la identificación del archivo y distinguirlo del laboratorio previo de waypoints.

## 1. Qué debes realizar según el documento

| Parte | Requisito | Páginas |
| --- | --- | --- |
| Actividades | Crear un juego 2D en Unity con un jugador controlable, paredes y un enemigo que encuentre una ruta hacia el jugador mediante A*. | 2-9, pasos 1-17 |
| Ejercicio 1 | Añadir más paredes como obstáculos y otro enemigo a la aplicación desarrollada. | 10 |
| Ejercicio 2 | Implementar el ejemplo «The A* Algorithm» de Unity Learn enlazado en la guía. | 10 |
| Ejercicio 3 | Elaborar un informe de la solución a los ejercicios. | 10 |
| Cuestionario | Responder cómo funciona A* y qué componentes apoyan el pathfinding en videojuegos. | 10 |

El resultado de actividades y del primer ejercicio debe permitir que al menos dos enemigos persigan al jugador y rodeen los obstáculos. El ejemplo de Unity Learn es un requisito adicional: no queda cubierto solo por duplicar al enemigo.

## 2. Actividades: instrucciones de implementación

1. Crear un proyecto **2D en Unity**. Diseñar el escenario tomando como referencia la imagen de la página 2: perímetro rectangular y paredes interiores que obliguen a buscar recorridos alternativos.
2. Descargar e importar el recurso del personaje indicado en la guía. Añadir `Player` y el sprite `Enemy` a la escena.
3. Crear y asignar `PlayerScript` al jugador. El código de la página 3 usa las cuatro flechas, una velocidad pública inicial de `5f` y desplazamiento con `Time.deltaTime`.
4. Configurar las paredes con colliders para representar los obstáculos. Las capturas muestran componentes de física 2D.
5. Descargar e importar la versión gratuita de **A* Pathfinding Project**, de Aron Granberg, indicada en la página 5.
6. Crear un GameObject vacío llamado `PathFinding` y añadir el componente que la guía denomina `PathFinder`.
7. Crear un **Grid Graph**, configurarlo para un escenario 2D y activar **Use 2D Physics**. Ajustar su tamaño, posición y resolución para cubrir el escenario.
8. Crear la capa `Environment` para las paredes; la guía indica `User Layer 8`. Asignarla a todas las paredes y seleccionarla en **Obstacle Layer Mask**.
9. Pulsar **Scan** y comprobar que las paredes se detecten como zonas no transitables.
10. Añadir al enemigo **AI Path (2D, 3D)** y **AI Destination Setter**. Asignar `Player` al campo **Target** de este último.
11. Ejecutar y observar la ruta hacia el jugador. Comprobar que el enemigo rodee las paredes.
12. Añadir `EnemyMovement` al objeto hijo `Graphics`. Según el código de la página 9, debe usar `Pathfinding.AIPath`, consultar `aiPath.desiredVelocity` y orientar `transform.right` hacia esa dirección. Asignar en el Inspector la referencia al `AIPath` del enemigo.
13. Ejecutar otra vez y comprobar el movimiento y la orientación del sprite.

### Precauciones de implementación recomendadas

Estas son recomendaciones técnicas; no son requisitos adicionales del PDF:

- Registrar la versión de Unity y del paquete utilizado. Las capturas muestran Unity 2020.1.0f1; al implementar, verificar los nombres y ajustes equivalentes en la versión instalada.
- Mantener el proyecto 2D en el plano XY y configurar el movimiento del agente de acuerdo con ese plano.
- Comprobar las dependencias del agente, como `Seeker`, según el paquete utilizado; no asumir que todos los componentes se agregaron automáticamente.
- Si se conserva `Input.GetKey`, comprobar la compatibilidad de la configuración de entrada del proyecto.
- Revisar las colisiones del jugador: mover `transform.position` como en la captura no garantiza por sí solo que el jugador respete las paredes. Si se adapta a movimiento físico, explicar el cambio en el informe.
- Evitar conflictos entre la rotación automática del agente y la orientación de `Graphics`. No aplicar una dirección nula al orientar el sprite.
- El marco teórico menciona **AI Navigation/NavMesh**, pero las actividades utilizan **A* Pathfinding Project con Grid Graph**. Mantener esta distinción al explicar la solución.

## 3. Ejercicios

### Ejercicio 1: ampliar el juego

1. Conservar el funcionamiento de la escena base.
2. Añadir otras paredes y asignarles colliders y la capa `Environment`.
3. Actualizar el grafo con **Scan** después de cambiar los obstáculos.
4. Añadir un segundo enemigo con los componentes y referencias necesarios; ambos deben tener al jugador como destino.
5. Comprobar que ambos persigan al jugador y encuentren recorridos alrededor de las nuevas paredes.

No se especifica cuántas paredes adicionales añadir ni se exige que los enemigos eviten colisionar entre sí.

### Ejercicio 2: ejemplo de Unity Learn

1. Abrir el enlace asociado a «The A* Algorithm» que contiene el PDF.
2. Revisar sus instrucciones, recursos y resultado esperado antes de implementarlo.
3. Implementar el ejemplo y documentar su funcionamiento por separado del juego de persecución.

El PDF solo aporta el enlace: no reproduce los pasos del tutorial. Su contenido externo no se ha consultado en este análisis. Por ello, no se atribuyen al tutorial requisitos que no aparecen en la guía.

### Ejercicio 3: informe

La guía exige un informe de la solución, pero no fija plantilla, extensión ni formato. Se recomienda incluir:

1. Datos del estudiante, curso y laboratorio.
2. Objetivo y herramientas/versiones utilizadas.
3. Resumen de la escena base y configuración del pathfinding.
4. Solución del ejercicio 1: nuevas paredes, segundo enemigo y pruebas.
5. Solución del ejercicio 2: desarrollo del tutorial y resultado obtenido.
6. Capturas reales que permitan verificar las soluciones.
7. Respuestas al cuestionario, conclusiones y fuentes.

## 4. Cuestionario que debes responder

1. **¿Cómo funciona el algoritmo de pathfinding A*?** Explicar nodos, vecinos, nodos pendientes y explorados, costos `g(n)`, heurística `h(n)`, prioridad `f(n) = g(n) + h(n)` y reconstrucción de la ruta. Indicar qué ocurre cuando no hay camino y bajo qué condiciones se obtiene una ruta óptima.
2. **¿Qué componentes existen para apoyar la implementación de pathfinding en videojuegos?** Describir el papel de grafos o mallas, detección de obstáculos y agentes de navegación. Relacionarlo con los componentes usados en el laboratorio y distinguirlos de AI Navigation/NavMesh.

Estas indicaciones orientan la redacción; las dos preguntas son las que aparecen en la página 10.

## 5. Organización sugerida

La guía no exige proyectos independientes ni esta estructura. Para facilitar la revisión se propone:

```text
lab07/
  INSTRUCCIONES.md
  ProyectoUnity/          # Escenas separadas para base, ampliación y tutorial
  informe/               # Informe y respuestas
  evidencias/            # Capturas de resultados reales
  README.md              # Cómo abrir, ejecutar y usar los controles
```

Conservar una escena base permite demostrar las actividades sin perderlas al desarrollar los ejercicios. Evitar modificar los laboratorios anteriores.

## 6. Lista de comprobación

- [ ] Proyecto 2D y escenario basado en la referencia.
- [ ] Jugador controlable con las flechas.
- [ ] Paredes con colliders y capa `Environment`.
- [ ] Grid Graph 2D configurado y escaneado.
- [ ] Enemigo que persigue al jugador mediante rutas alrededor de obstáculos.
- [ ] `EnemyMovement` en `Graphics`, con su referencia asignada y orientación funcional.
- [ ] Ejercicio 1: paredes adicionales y al menos dos enemigos en total.
- [ ] Ejercicio 2: ejemplo de Unity Learn implementado.
- [ ] Informe de la solución a los ejercicios.
- [ ] Las dos preguntas del cuestionario respondidas.
- [ ] Recomendado: evidencias de ejecución, versiones y pasos para abrir el proyecto.

El PDF no especifica fecha de entrega, plataforma de envío, nombre del archivo final, formato del informe ni obligación de generar un ejecutable. No tratar esos puntos como exigencias del documento.

## 7. Recursos y referencias del PDF

- Fuente analizada: `C:/Users/diego/Downloads/CONT2 2026B DSJ_LAB07_Pathfinding.pdf`, 10 páginas; incluye texto, capturas y código en imágenes.
- [Recurso del personaje en Google Drive](https://drive.google.com/file/d/1Epzoo14fYCxe6VsyiHPyt3BMpFhn3_p-/view).
- [A* Pathfinding Project: descarga](https://arongranberg.com/astar/download).
- [Enlace de Unity Learn asociado a «The A* Algorithm» en el PDF](https://learn.unity.com/course/artificial-intelligence-for-beginners/unit/a-36369ng?version=6.0).
- [Video de referencia incluido en el PDF](https://www.youtube.com/watch?v=63zWZ2rJZ68&ab_channel=TheGameGuy).
- Millington, I., & Funge, J. (2019). *Artificial intelligence for games*. Third Ed. CRC Press. Referencia transcrita de la guía.
- Unity Technologies. *AI Navigation package (com.unity.ai.navigation)*. Referencia incluida en la guía.

Los enlaces se extrajeron del PDF; no se ha verificado su disponibilidad ni su contenido actual.

