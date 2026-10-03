# Verificación ejecutada en Unity

Fecha UTC: **2026-10-03 23:42:50**. Editor/build Windows x64: **Unity 6000.3.15f1**. El script `../tools/Verificar.ps1` terminó con código 0.

- Compilación C# y build Windows completados.
- **58 comprobaciones** de configuración y propiedades del movimiento aprobadas: referencias, modos, velocidades, marcadores sin collider, posiciones, límites, predicción, T=0, dirección de evasión y frontera estricta de captura.
- **8 escenas ejecutadas**, cada una durante 8 segundos simulados a deltaTime 1/60, sin errores del proyecto registrados.
- V.2 emitió exactamente una vez **Runner captured**.
- **17 imágenes reales** de la cámara: una base, ocho a los 3 segundos (`_t03`) y ocho al final. No se han dibujado ni retocado trayectorias o agentes fuera de Unity.

## Método y alcance

La prueba corre el ejecutable de Unity y actualiza los mismos componentes del proyecto. En escenas Manual inyecta una dirección circular normalizada (`-sin(0.65*t), 0, cos(0.65*t)`), idéntica para A/B y para las dos variantes V.1. Restablece las posiciones y velocidades antes de cada prueba. C usa Evasion autónoma. En V.2 Runner permanece quieto para garantizar que se pueda comprobar el registro.

Las PNG son renders directos de la cámara durante esa ejecución, a 1200 x 1200. Se usó una RenderTexture porque las capturas del framebuffer de la ventana oculta resultaban negras. Se amplía temporalmente el viewport de cámara para usar todo el cuadrado: las imágenes **no incluyen** el panel IMGUI, Inspector ni Console. Esto no cambia posiciones, parámetros o algoritmos.

La prueba no contiene pulsaciones humanas de WASD. Los resultados verifican una trayectoria concreta; no demuestran que Pursuit sea mejor para cualquier recorrido. Los dos agentes se actualizan por fotograma como en la guía; las cifras pueden variar ligeramente entre entornos por el orden de actualización. No se realizaron capturas manuales del editor. Obtén las que pide tu informe siguiendo [GUIA_DE_CAPTURAS.md](../docs/GUIA_DE_CAPTURAS.md).

## Resultados medidos

| Caso | Distancia mínima (u) | Distancia final (u) | Adelanto medio de Predator (u) |
| --- | ---: | ---: | ---: |
| A Seek | 3.9689 | 7.1077 | 0.0000 |
| B Pursuit | 1.7627 | 5.5532 | 2.6317 |
| C base | 0.0000 | 0.0000 | 2.5874 |
| C reactiva | 0.0000 | 0.0000 | 0.5051 |
| C anticipada | 0.0076 | 2.2767 | 4.8892 |
| V.1 Seek, 5.5 / 5.0 | 4.3497 | 7.8727 | 0.0000 |
| V.1 Pursuit, 5.5 / 5.0 | 1.9879 | 6.0995 | 3.0609 |
| V.2 captura | 0.0073 | 0.0073 | 0.0000 |

El adelanto se calcula como magnitud de velocidad de Runner por T de Predator y se promedia en los fotogramas muestreados. Las distancias son muestras de la ejecución, no mínimos continuos analíticos. Un 0 en la columna `capturas` del CSV en A/C/V.1 significa que **no tienen detector de captura**; no significa que nunca se hayan acercado a menos de 0.8.

En este recorrido Pursuit redujo la distancia mínima frente a Seek, también con Runner un 10% más rápido. En C base/reactiva ambos agentes terminaron superpuestos en el límite: el clamp de la guía puede atrapar al evasor. En C anticipada se separaron de nuevo al final, pero se acercaron a menos de 0.8 antes; por eso no sería correcto concluir que escapó sin ser alcanzado.

## Archivos de respaldo

- `verificacion-configuracion.txt`: 58 comprobaciones aprobadas.
- `resultados-runtime.csv`: mediciones por escena.
- `resultado-runtime.txt`: versión, fecha y número de errores.
- `extracto-logs.txt`: líneas literales relevantes de compilación y ejecución, incluido el mensaje de captura.
- `00_Escena_base.png`: disposición inicial.
- PNG de cada escena y variantes `_t03`: estado a 3 y 8 segundos. En C base/reactiva al final las cápsulas se superponen; usa también `_t03` para ver ambos agentes y las predicciones.

Los logs completos y el ejecutable están en el equipo de preparación bajo `LAB04_PursuitEvasion/Logs` y `Builds/Windows`; no se suben a Git. Se regeneran con el script. No hay evidencias fabricadas de Console o Inspector.
