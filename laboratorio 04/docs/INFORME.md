# Informe de apoyo - laboratorio 04

Este archivo explica la teoría y ofrece una estructura para redactar el documento. Las observaciones automáticas verificadas están en ../evidencias/README.md. Completa tus datos y capturas manuales; no se han inventado resultados de una sesión humana.

## Datos de tu ejecución

- Nombre y grupo: completar.
- Fecha: completar.
- Unity: 6000.3.15f1, o indicar la versión efectivamente utilizada.
- Commit del proyecto: ejecutar `git rev-parse HEAD`.
- Equipo y duración por prueba: completar.

## Implementación

El Runner usa un vector de entrada normalizado o huye de la posición futura del Predator. Predator aplica Seek al punto actual o Pursuit al punto extrapolado. Ambos calculan una velocidad deseada, restan su velocidad actual, limitan el steering y lo integran con deltaTime.

La predicción se calcula cada fotograma:

```text
T = min(maxPrediction, distancia * predictionFactor)
posicion_futura = posicion_actual + velocidad_actual * T
```

La estimación supone velocidad constante durante ese horizonte. Si cambia la dirección, la siguiente actualización corrige la predicción; un horizonte largo puede causar sobreanticipación.

V.1 modifica maxSpeed a 5.5 / 5.0 y compara Seek con Pursuit. V.2 registra la captura una vez por ejecución cuando la distancia es estrictamente menor que 0.8.

## Registro manual

| Prueba | Valores | Capturas propias | Lo observado |
| --- | --- | --- | --- |
| A | Seek, Manual, 5 / 4.6 | Añadir | Completar |
| B | Pursuit, Manual, 5 / 4.6 | Añadir | Completar |
| C base | factor 0.12, maxT 1.5 | Añadir | Completar |
| C reactiva | factor 0.03, maxT 0.25 | Añadir | Completar |
| C anticipada | factor 0.35, maxT 3 | Añadir | Completar |
| V.1 | Runner 5.5, Predator 5.0 | Añadir | Completar |
| V.2 | distancia < 0.8 | Añadir Console | Completar |

## Cuestionario explicado

**1. ¿Cuál es la diferencia conceptual entre Seek y Pursuit?**

Seek genera movimiento hacia el punto objetivo actual. Pursuit estima dónde estará un objetivo móvil y aplica Seek a esa posición futura. Seek puede seguir por detrás; Pursuit puede dirigirse hacia una posible intercepción.

**2. ¿Por qué Pursuit necesita conocer o estimar la velocidad del objetivo?**

La velocidad aporta dirección y rapidez. Al multiplicarla por T se obtiene el desplazamiento esperado del objetivo durante el horizonte de predicción. Con solo la posición actual no se puede estimar ese desplazamiento mediante este modelo.

**3. ¿Qué representa el tiempo de predicción T?**

Es cuántos segundos hacia el futuro se extrapola la posición. Aquí depende de la distancia y tiene un máximo: min(maxPrediction, distancia * predictionFactor). No es deltaTime: deltaTime integra un fotograma; T define el horizonte que se intenta anticipar. Si la distancia se mide en unidades y T en segundos, predictionFactor representa segundos por unidad.

**4. ¿Qué ocurriría si T = 0?**

Velocidad por cero es cero: la posición futura coincide con la actual. Pursuit equivale a Seek y Evasion equivale a Flee, con los mismos parámetros y estado.

**5. ¿Cuál es la diferencia entre Flee y Evasion?**

Flee huye de una posición actual. Evasion estima la posición futura del perseguidor usando su velocidad y huye de ese punto. Ambos desean alejarse, pero Evasion incorpora predicción.

## Conclusiones: cómo redactarlas

Relaciona cada afirmación con una captura y con los parámetros usados. Puedes explicar estos principios teóricos, y después indicar si aparecieron en tu ejecución:

- La predicción puede ayudar a cortar una trayectoria, incluso si el perseguidor tiene menor velocidad máxima.
- No garantiza captura: depende de velocidad, aceleración, giros, estado inicial y límites.
- Un T pequeño favorece respuesta a cambios recientes; un T grande anticipa más, pero puede apuntar demasiado lejos.
- Contra el borde, el clamp de posición puede atrapar al evasor y conservar una velocidad interna que no corresponde a su desplazamiento real. Se debe separar ese efecto de la calidad de la predicción.
- Registrar una captura una sola vez permite una evidencia clara en Console.

No concluyas que una variante es siempre superior a partir de una única trayectoria.
