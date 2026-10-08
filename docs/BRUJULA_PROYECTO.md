# VESTIGIA
## Brújula narrativa y guion de escenas
### Versión de trabajo — canon actualizado

> **Objetivo de este documento:** servir como brújula común para narrativa, diseño de niveles, puzzles, dirección visual, sonido y desarrollo técnico.  
> No pretende cerrar diálogos definitivos; define qué debe ocurrir, qué debe entender el jugador y por qué cada escena conecta con la siguiente.

---

# 0. Visión general

**Vestigia** es una aventura breve de misterio y suspenso, sin combate ni enemigos, centrada en exploración, investigación, perspectiva y resolución de puzzles.

La experiencia principal ocurre en **PC**, pero utiliza un **teléfono como segundo dispositivo integrado a la experiencia**. El teléfono funciona como la libreta/herramienta de investigación de Janeth y, en el clímax, como un visor físico del cielo mediante el giroscopio.

La partida completa está pensada para durar aproximadamente **15 minutos**.

## Idea central

El jugador reconstruye una investigación a partir de pequeños vestigios: una radio, una fotografía, coordenadas, objetos y anomalías. Al final comprende que la investigación que acaba de vivir forma parte de una historia mucho más personal.

La palabra **Vestigia** remite a huellas, rastros o vestigios: aquello que permanece de alguien o de algo que ya no está.

---

# 1. Pilares del proyecto

## 1.1 Misterio antes que terror

El juego debe generar curiosidad, incertidumbre y suspenso.

No hay monstruos, persecuciones, jumpscares ni combate. La tensión surge de:

- dispositivos que reaccionan sin explicación;
- interferencias;
- luces que se encienden donde no deberían;
- señales que parecen imposibles;
- información incompleta;
- una primera interpretación equivocada;
- la sensación de que los escenarios conservan huellas del pasado.

## 1.2 Perspectiva como mecánica

La cámara se presenta con una composición isométrica y puede girarse en intervalos de **90°**.

El giro de cámara no es únicamente estético. Debe servir para:

- descubrir personas u objetos ocultos;
- revelar accesos;
- encontrar pistas;
- cambiar el punto de referencia del escenario;
- dirigir la atención del jugador sin usar flechas o marcadores excesivos.

Controles base:

- **WASD:** movimiento.
- **Q / E:** rotar cámara 90°.
- **Espacio:** interactuar.
- **Esc:** pausa.

## 1.3 Experiencia multidispositivo

PC y teléfono forman parte de una misma experiencia.

El teléfono representa principalmente la **libreta/herramienta de investigación de Janeth**.

Funciones previstas:

- recibir pistas;
- almacenar información;
- mostrar los códigos/coordenadas obtenidos;
- participar en puzzles;
- reaccionar de forma sincronizada con la PC;
- utilizar el giroscopio durante el puzzle del Observatorio.

La intención es que el jugador sienta que está manipulando una herramienta del mundo del juego y no simplemente consultando una segunda pantalla.

## 1.4 Dirección visual

La estética combina:

- escenarios principalmente **3D**;
- composición tipo **diorama / maqueta artesanal**;
- personajes y elementos interactuables con apariencia de **papel o cartón recortado**;
- texturas imperfectas, bordes visibles, dobleces y materiales físicos;
- iluminación cinematográfica y atmosférica.

Los elementos 2D/papel deben destacar ligeramente sobre el entorno 3D para comunicar qué pertenece al plano narrativo o interactuable.

### Regla de Janeth

**El rostro de Janeth nunca se muestra.**

Debe aparecer:

- de espaldas;
- de tres cuartos sin revelar el rostro;
- parcialmente oculta cuando sea necesario.

Esta regla ayuda a conservar la identidad visual del personaje y protege la revelación final.

## 1.5 Sonido

El audio es una parte narrativa importante de la experiencia.

Elementos recurrentes:

- lluvia;
- viento;
- costa;
- tormenta;
- estática de radio;
- interferencias;
- pulsos eléctricos;
- zumbidos;
- sonidos de mecanismos;
- silencio controlado en momentos de descubrimiento.

La anomalía debe sentirse antes de comprenderse.

---

# 2. Estructura completa

| Escena | Tipo | Espacio | Función |
|---|---|---|---|
| 1 | Cinemática | Casa | Presentar el marco narrativo |
| 2 | Jugable | Pueblo | Descubrir la primera anomalía y obtener la primera coordenada |
| 3 | Jugable | Faro | Obtener la segunda coordenada y utilizar el mirador de la cima para investigar el falso origen |
| 4 | Jugable | Observatorio | Unir las coordenadas y localizar el magnetar |
| 5 | Cinemática | Casa / maqueta | Revelar la conexión entre Janeth, el padre y la niña |

---

# 3. Cadena principal de pistas

La progresión narrativa debe sentirse como una sola investigación:

**Radio**  
→ aparece un código extraño  
→ Janeth lo registra  
→ el código resulta ser una coordenada.

**Faro**  
→ fotografía dividida en tres fragmentos  
→ un fragmento en cada nivel  
→ fotografía reconstruida  
→ en el reverso aparece otra coordenada.

**Mirador de la cima del faro**  
→ el lente está sucio  
→ el jugador lo limpia con los dedos en el teléfono  
→ ajusta la vista con dos perillas independientes, una para X y otra para Y  
→ los valores X e Y se muestran para depuración durante el prototipo.

**Observatorio**  
→ ambas coordenadas se interpretan juntas  
→ forman una posición astronómica completa  
→ el jugador utiliza el teléfono y su giroscopio para apuntar físicamente  
→ el telescopio de la PC se alinea con el movimiento  
→ se localiza la anomalía  
→ **magnetar**.

Ninguna de las dos coordenadas debe resolver por sí sola el misterio.

---

# 4. ESCENA 1 — APERTURA
## La tormenta, el padre y la niña

**Tipo:** Cinemática  
**Duración objetivo:** ~1 minuto  
**Lugar:** Casa familiar, de noche  
**Clima:** Tormenta / apagón  
**Personajes:** Padre, niña  
**Jugabilidad:** Ninguna

---

## 4.1 ¿Qué pasa?

Una tormenta provoca un apagón en una casa.

El padre busca una fuente de luz —una lámpara o linterna— mientras su hija permanece con él.

Para tranquilizarla o entretenerla durante el apagón, el padre saca figuras u objetos artesanales y comienza a contarle una historia.

La historia habla de una científica llamada **Janeth**.

La cámara se aproxima a las figuras de papel o a una pequeña maqueta.

La casa desaparece visualmente y la maqueta se transforma en el mundo jugable.

Entramos al pueblo.

---

## 4.2 ¿Por qué pasa?

Esta escena crea el marco narrativo sin explicar todavía su importancia.

El jugador debe creer inicialmente que el padre simplemente está inventando o narrando una historia para su hija.

No se debe explicar:

- quién es realmente Janeth;
- qué relación tiene con ellos;
- si la historia ocurrió;
- por qué el padre conoce tantos detalles.

---

## 4.3 ¿Qué queremos que experimente el jugador?

- Calidez doméstica.
- Contraste entre seguridad y tormenta.
- Curiosidad.
- Una sensación de cuento artesanal.
- La impresión de entrar físicamente en una historia construida con papel.

La apertura no debe sentirse triste de manera evidente. El componente melancólico debe adquirir significado **retroactivamente** al terminar el juego.

---

## 4.4 Alebrije — foreshadowing

El **alebrije** puede aparecer desde esta escena.

Su posición exacta todavía está por confirmarse:

- dentro de la caja de objetos;
- entre las figuras;
- en una repisa;
- discretamente en el fondo.

### Regla

En la introducción **no debe destacarse demasiado**.

La intención es que:

- un jugador casual quizá no lo recuerde;
- un jugador atento pueda reconocerlo al final.

El alebrije **no es actualmente una herramienta de puzzle**.

Su función prevista es narrativa y visual.

---

## 4.5 ¿Qué cambia después de esta escena?

El jugador pasa del mundo real al mundo narrado.

La estética de las figuras de papel de la casa justifica visualmente:

- el diseño de Janeth;
- los NPC de papel;
- ciertos objetos interactuables;
- la apariencia artesanal del juego.

---

## 4.6 Transición

El padre comienza la historia.

La cámara se acerca a una figura o maqueta.

La iluminación de la casa se transforma gradualmente en la iluminación nocturna del pueblo.

La figura que representa a Janeth pasa de ser un objeto físico a convertirse en el personaje jugable.

---

# 5. ESCENA 2 — PUEBLO
## La radio y la primera coordenada

**Tipo:** Jugable  
**Duración objetivo:** ~4 minutos  
**Lugar:** Pueblo costero mexicano  
**Momento:** Noche  
**Clima:** Lluvia / tormenta aproximándose  
**Personajes:** Janeth, pescador  
**Puzzle principal:** Radio  
**Resultado:** Primera coordenada

---

## 5.1 Propósito narrativo

Presentar:

- a Janeth;
- los controles;
- la rotación de cámara;
- el teléfono/libreta;
- la anomalía;
- el primer código;
- la primera hipótesis del jugador.

Janeth llega al pueblo como investigadora. Es nueva en el lugar y está ahí debido a las interferencias y comportamientos anómalos detectados en la zona.

---

## 5.2 Escenario

El pueblo debe sentirse compacto, no como un mapa abierto.

Elementos posibles:

- puerto;
- camino principal;
- kiosco o pequeño centro;
- construcciones costeras;
- zona donde se encuentra el pescador;
- salida o transición hacia el faro.

La intención es representar únicamente los espacios necesarios para la experiencia, como pequeñas maquetas conectadas.

---

## 5.3 Primer uso importante de la perspectiva

Al llegar, Janeth no necesariamente ve de inmediato al pescador.

El jugador debe rotar la cámara para:

- descubrirlo;
- acercarse;
- iniciar la conversación.

Esto enseña desde temprano que **cambiar la perspectiva revela información**.

---

## 5.4 El pescador

El pescador es el primer testigo directo de las anomalías.

Tiene una radio que comenzó a comportarse de forma extraña.

No debe convertirse en un NPC que simplemente diga:

> “Ve al faro.”

La dirección hacia el faro debe surgir de manera más orgánica.

Durante el encuentro:

- la radio presenta interferencias;
- aparecen patrones o señales;
- el faro, visible a la distancia, comienza a reaccionar de una forma imposible o inusual.

El pescador puede comentar que el comportamiento del faro es extraño, especialmente porque llevaba mucho tiempo sin funcionar con normalidad.

Janeth y el jugador forman la primera hipótesis:

**quizá el faro está relacionado con el origen del fenómeno.**

---

# 6. PUZZLE 1 — RADIO
## Primera coordenada

El jugador interactúa con la radio.

La radio reproduce:

- estática;
- interferencias;
- señales repetitivas;
- fragmentos que parecen formar un patrón.

El teléfono/libreta participa en la resolución.

Durante el puzzle, la PC puede reaccionar mediante:

- parpadeos;
- interferencia visual;
- cambios de intensidad de luz;
- pequeños pulsos sincronizados.

---

## 6.1 Resultado

El jugador obtiene un **código**.

En este momento puede presentarse inicialmente como una secuencia extraña.

La libreta registra el resultado.

Más adelante se comprende que este código representa **una de las coordenadas necesarias para determinar una posición astronómica**.

### Importante

La coordenada de la radio por sí sola **no debe revelar el magnetar**.

Es sólo una mitad del dato necesario.

---

## 6.2 ¿Qué debe entender el jugador?

Al terminar la escena:

- algo está afectando las comunicaciones;
- el fenómeno puede relacionarse con el faro;
- el código es importante;
- el teléfono no es decorativo: forma parte de la investigación;
- la anomalía parece responder en más de un dispositivo.

---

## 6.3 Conexión con la siguiente escena

Durante o después del puzzle:

- el faro reacciona;
- Janeth observa el fenómeno;
- el pescador confirma que su comportamiento no es normal.

Janeth decide investigarlo.

Transición a la maqueta/escena del Faro.

---

# 7. ESCENA 3 — FARO
## El falso origen, la fotografía y el mirador

**Tipo:** Jugable  
**Duración objetivo:** ~4–5 minutos  
**Lugar:** Faro y alrededores  
**Personajes:** Janeth, vigilante del faro  
**Estructura:** 3 niveles  
**Puzzles:** reconstrucción de fotografía y mirador de dos fases en la cima  
**Resultado:** Segunda coordenada

---

## 7.1 Propósito narrativo

El faro funciona como **falso culpable**.

El jugador llega esperando encontrar ahí el origen de las anomalías.

Sin embargo, conforme asciende descubre que:

- el faro no está produciendo el fenómeno;
- el faro también está reaccionando a él;
- la verdadera fuente se encuentra en otra parte.

---

## 7.2 El vigilante

Janeth encuentra al vigilante del faro.

### El vigilante NO:

- proporciona un código;
- conoce la solución del puzzle;
- abre el camino mediante una contraseña;
- entrega el alebrije como herramienta del Observatorio.

### El vigilante SÍ:

le explica que dentro del faro existe una fotografía incompleta que quiere recuperar/reconstruir.

La fotografía tiene valor personal para él.

> El sujeto exacto de la fotografía puede mantenerse según la versión narrativa definitiva; en el documento académico actual se relaciona con su esposa, pero esto puede confirmarse posteriormente.

Su petición le da al jugador un objetivo secundario que termina convirtiéndose en una pista principal.

---

# 8. FARO — NIVEL 1

## 8.1 Objetivo

Explorar el primer nivel y encontrar el **Fragmento 1/3** de la fotografía.

Al mismo tiempo, Janeth comienza a observar que las instalaciones del faro no explican lo que vio desde el pueblo.

---

## 8.2 Uso de perspectiva

Debe existir al menos un elemento que sólo pueda descubrirse al girar la cámara:

- el fragmento;
- un acceso;
- un mecanismo;
- una evidencia ambiental.

---

## 8.3 Descubrimiento narrativo

Janeth registra observaciones en su libreta.

El faro parece estar recibiendo o reaccionando ante algo.

La primera hipótesis comienza a debilitarse.

---

# 9. FARO — NIVEL 2

## 9.1 Objetivo

Encontrar el **Fragmento 2/3**.

Este nivel puede contener más elementos del antiguo funcionamiento del faro:

- paneles;
- maquinaria;
- mecanismos;
- lámparas;
- instalaciones deterioradas.

---

## 9.2 Función narrativa

Aquí debe ser cada vez más evidente que:

- el sistema no está funcionando de manera autónoma;
- algo externo lo está afectando;
- Janeth no está “reparando el faro”; está utilizándolo como evidencia.

---

# 10. FARO — NIVEL 3

## 10.1 Objetivo

Encontrar el **Fragmento 3/3**.

Al reunir los tres fragmentos se habilita la reconstrucción de la fotografía.

---

# 11. PUZZLE 2 — FOTOGRAFÍA
## Segunda coordenada

Los tres fragmentos encontrados —uno por cada nivel del faro— forman una fotografía.

El jugador debe reconstruirla.

La interacción puede ocurrir principalmente en el teléfono/libreta.

Una vez completada:

1. se muestra la fotografía reconstruida;
2. el jugador puede observarla;
3. se revela su reverso;
4. detrás aparece un segundo código;
5. la libreta registra el código.

El nuevo código corresponde a **la segunda coordenada**.

---

## 11.1 Relación con la radio

Janeth posee ahora:

- Coordenada A — obtenida de la radio.
- Coordenada B — obtenida detrás de la fotografía.

Todavía puede no estar completamente claro qué representan juntas.

---

## 11.2 PUZZLE 3 — MIRADOR DEL FARO

**Confirmado por el usuario:** en la cima del faro hay un mirador cuyo lente está sucio. El puzzle tiene dos fases consecutivas y se manipula desde el teléfono.

### Fase 1 — Limpiar el lente

El jugador desliza los dedos sobre el lente para retirar la suciedad y recuperar la visibilidad.

### Fase 2 — Ajustar la vista

Después de limpiar el lente, el jugador utiliza dos perillas:

- una controla el eje X;
- la otra controla el eje Y.

Los valores actuales de ambos ejes se muestran en pantalla como información de depuración para las pruebas. No se ha definido su presencia en la interfaz final ni su relación con las coordenadas de la radio y la fotografía.

**Pendiente:** objetivo visual de la búsqueda, umbral de limpieza, rangos y tolerancia de los ejes, y condición de resolución. Los valores que use el prototipo deben identificarse como provisionales.

Este puzzle ocurre después de la fotografía y antes del Observatorio. El Observatorio pasa a ser el Puzzle 4 y conserva la interacción física mediante el giroscopio.

---

## 11.3 Revelación del faro

Al alcanzar la parte superior o un punto de observación:

Janeth confirma que el faro **no es la fuente**.

Es otro sistema afectado por la anomalía.

Desde este punto puede observarse el **Observatorio**.

La lógica de Janeth cambia:

> Si el origen no está aquí, necesito observar el fenómeno desde un instrumento diseñado para mirar más lejos.

---

## 11.4 ¿Qué debe entender el jugador?

- Su primera hipótesis estaba incompleta.
- Las dos coordenadas están relacionadas.
- El fenómeno es externo al faro.
- El Observatorio es el siguiente lugar lógico de investigación.

---

## 11.5 Transición

Cambio de maqueta:

**Faro → Observatorio**

Puede utilizarse:

- fundido;
- desplazamiento de cámara;
- transición que simule cambiar de una maqueta a otra.

---

# 12. ESCENA 4 — OBSERVATORIO
## Las coordenadas y el magnetar

**Tipo:** Jugable  
**Duración objetivo:** ~3–4 minutos  
**Lugar:** Observatorio  
**Puzzle principal:** localizar la anomalía mediante coordenadas + telescopio + teléfono  
**Resultado:** descubrimiento del magnetar

---

## 12.1 Propósito narrativo

Esta escena une todo lo aprendido.

Por primera vez, las pistas de los escenarios anteriores dejan de ser objetos separados y forman una sola respuesta.

---

## 12.2 Escenario

Elementos principales:

- cúpula;
- telescopio;
- mecanismo de apertura;
- controles o paneles;
- mesa de trabajo;
- espacio suficiente para que la orientación del telescopio sea visualmente clara.

---

# 13. Interpretación de las coordenadas

Janeth relaciona los datos de la radio y la fotografía.

Descubre que:

- ambas secuencias corresponden a coordenadas;
- cada una proporciona una parte necesaria;
- juntas forman una **posición astronómica completa**.

### Formato exacto pendiente

Por definir técnicamente:

- sistema de coordenadas concreto;
- representación en UI;
- valores definitivos;
- qué parte aporta radio y qué parte aporta fotografía.

La intención narrativa sí está confirmada:

**ninguna pista basta por sí sola; las dos son necesarias para localizar el fenómeno.**

---

# 14. PUZZLE 4 — OBSERVATORIO
## El teléfono como visor físico

Este es el clímax mecánico de Vestigia.

El teléfono deja de funcionar únicamente como libreta y se convierte en una herramienta física.

---

## 14.1 Paso 1 — Preparar el telescopio

El jugador interactúa con el Observatorio.

Puede ser necesario:

- abrir la compuerta de la cúpula;
- activar el telescopio;
- introducir o seleccionar las coordenadas obtenidas.

La complejidad exacta de esta primera fase debe mantenerse breve para no romper el ritmo.

---

## 14.2 Paso 2 — Utilizar las coordenadas

El jugador consulta en el teléfono:

- coordenada proveniente de la radio;
- coordenada proveniente de la fotografía.

Las utiliza para delimitar la región del cielo que debe investigar.

---

## 14.3 Paso 3 — Giroscopio

El teléfono funciona como **visor astronómico físico**.

El jugador debe:

1. sostener el teléfono;
2. moverlo físicamente;
3. apuntarlo hacia distintas direcciones;
4. buscar la posición indicada por las coordenadas.

El movimiento del teléfono se obtiene mediante el **giroscopio**.

---

## 14.4 Sincronización PC + teléfono

Mientras el jugador mueve el teléfono:

- el telescopio de la PC rota o se alinea en tiempo real;
- la imagen del juego reacciona a la orientación;
- ambos dispositivos se sienten como partes del mismo instrumento.

La PC representa el telescopio físico del observatorio.

El teléfono representa el visor/control que el jugador sostiene en sus manos.

---

## 14.5 Integración astronómica — Stellarium

Se contempla integrar **Stellarium / datos de Stellarium** para apoyar la representación astronómica de la búsqueda.

Objetivos posibles:

- disponer de una bóveda celeste coherente;
- traducir las coordenadas a una posición del cielo;
- orientar la visualización;
- reforzar la sensación de utilizar una herramienta astronómica real.

### Estado

**Intención técnica confirmada para exploración**, pero la forma exacta de integración debe validarse durante implementación.

La narrativa no debe depender de una implementación específica de Stellarium. Si la integración cambia, la lógica del puzzle sigue siendo:

**coordenadas + orientación física + telescopio = localización de la anomalía.**

---

# 15. Descubrimiento — Magnetar

Cuando el teléfono y el telescopio alcanzan la orientación correcta:

- la señal se estabiliza;
- la interferencia cambia;
- el telescopio enfoca;
- aparece la fuente del fenómeno.

Janeth identifica un **magnetar**.

El magnetar es la anomalía responsable de los fenómenos observados durante el juego.

Su influencia explica dentro del universo de Vestigia:

- interferencias;
- alteraciones de radio;
- comportamiento extraño de dispositivos;
- pulsos;
- fenómenos electromagnéticos;
- la reacción anómala del faro.

La representación tiene libertad estilizada: se busca una base astronómica reconocible, pero Vestigia puede exagerar sus efectos para servir al misterio.

---

## 15.1 Posible tratamiento de las voces/interferencias

Una idea compatible con el concepto —si se conserva— es que algunas interferencias parezcan voces o transmisiones antiguas.

No serían fantasmas.

Serían señales alteradas o devueltas por el fenómeno asociado al magnetar.

**Estado:** recurso narrativo opcional; debe confirmarse antes de tratarlo como canon.

---

## 15.2 ¿Qué debe sentir el jugador?

- satisfacción por conectar información de escenarios anteriores;
- comprensión repentina;
- sensación de descubrimiento científico;
- asombro;
- un breve alivio al descubrir que el fenómeno no era sobrenatural;
- preparación emocional para el verdadero giro del juego.

---

# 16. ESCENA 5 — DESENLACE
## Los vestigios de Janeth

**Tipo:** Cinemática  
**Duración objetivo:** ~1–2 minutos  
**Lugar:** Regreso a la casa de la apertura  
**Personajes:** Padre, niña  
**Revelación:** Janeth era la madre de la niña

---

## 16.1 Transición

Tras descubrir el magnetar, el juego comienza a abandonar el mundo de Janeth.

La cámara puede:

- alejarse del telescopio;
- salir del observatorio;
- subir hacia el cielo;
- transformar nuevamente el escenario en una maqueta;
- regresar a las manos del padre o a la mesa de la casa.

La historia que el jugador creía estar viviendo directamente vuelve a revelarse como una narración construida con figuras.

---

## 16.2 Giro

Ahora se hace evidente que:

**Janeth era la madre de la niña.**

El padre no estaba contando una historia cualquiera.

Estaba preservando para su hija la historia de su madre.

---

## 16.3 Janeth falleció

Canon interno:

**Janeth está muerta.**

Sin embargo:

- no se explica la causa;
- no se muestra su muerte;
- no existe una escena de fallecimiento;
- no debe afirmarse mediante exposición directa si no es necesario;
- su muerte no tiene por qué estar relacionada con el magnetar.

El jugador debe **inferirlo**.

Se puede comunicar mediante:

- la forma en que habla el padre;
- silencios;
- pasado verbal;
- la reacción de la niña;
- objetos conservados;
- la ausencia de Janeth;
- composición visual;
- pequeños detalles ambientales.

La intención es que la comprensión llegue de forma emocional, no como un dato enciclopédico.

---

# 17. El alebrije en el desenlace

El alebrije reaparece.

Esta vez se le da **un poco más de presencia visual** que en la introducción.

No es necesario decir:

> “Este alebrije pertenecía a Janeth.”

La conexión debe surgir por reconocimiento.

El jugador atento puede pensar:

> “Ese objeto ya estaba al principio.”

Y después:

> “Entonces la historia de Janeth estaba conectada con esta familia desde el inicio.”

El alebrije funciona como:

- foreshadowing;
- vestigio material;
- puente entre las dos capas narrativas;
- recompensa para la observación.

---

## 17.1 Ubicación exacta pendiente

Debe decidirse dónde aparece en la Escena 1:

- dentro de la caja;
- junto a las figuras;
- al fondo de la habitación;
- en una repisa.

Y cómo reaparece en la Escena 5.

La regla es:

**primero discreto, después reconocible.**

---

# 18. Significado del final

El juego tiene dos revelaciones diferentes.

## Revelación de investigación

La anomalía no provenía del faro.

Era un **magnetar** localizado mediante las coordenadas reunidas durante el recorrido.

## Revelación emocional

La científica cuya historia acaba de reconstruir el jugador era la madre de la niña.

Ya falleció.

El padre conserva su memoria a través de:

- historias;
- figuras;
- objetos;
- fotografías;
- pequeños rastros.

Esos rastros son los **vestigia**.

---

# 19. Arco del jugador

## Al inicio

> “Un padre está contando un cuento durante un apagón.”

## Pueblo

> “Hay una anomalía. Quizá el faro tiene algo que ver.”

## Faro

> “El faro no la causa. También está reaccionando. ¿Qué significan estos datos?”

## Observatorio

> “Los códigos eran coordenadas. Todo apunta hacia algo en el cielo.”

## Descubrimiento

> “Es un magnetar.”

## Final

> “Janeth era real dentro de esta historia. Era la madre de la niña. Ya no está.”

## Última lectura

> “Todo el juego consistió en reconstruir los rastros que dejó.”

---

# 20. Objetos narrativos y de puzzle

| Objeto | Escena | Función |
|---|---|---|
| Figuras de papel | Apertura / final | Enmarcan la historia y justifican el estilo visual |
| Lámpara / linterna | Apertura | Introduce el apagón, la luz y la intimidad |
| Alebrije | Apertura / final | Foreshadowing y conexión narrativa; no es puzzle |
| Radio | Pueblo | Puzzle 1; produce la primera coordenada |
| Teléfono / libreta | Todo el juego | Registro, sincronización y herramienta de puzzles |
| Fragmento de foto 1 | Faro nivel 1 | Parte del Puzzle 2 |
| Fragmento de foto 2 | Faro nivel 2 | Parte del Puzzle 2 |
| Fragmento de foto 3 | Faro nivel 3 | Parte del Puzzle 2 |
| Fotografía reconstruida | Faro | Revela la segunda coordenada en el reverso |
| Mirador con lente sucio | Cima del faro | Puzzle 3; limpiar el lente y ajustar X e Y con dos perillas |
| Telescopio | Observatorio | Instrumento principal del Puzzle 4 |
| Giroscopio del teléfono | Observatorio | Permite apuntar físicamente hacia la anomalía |
| Magnetar | Observatorio | Verdadera fuente de las anomalías |

---

# 21. Estado de personajes

## Janeth

- Protagonista jugable.
- Científica / investigadora.
- Madre de la niña.
- Fallecida en el presente de la apertura/desenlace.
- Causa de muerte deliberadamente desconocida.
- Rostro nunca visible.
- Racional; investiga antes de aceptar explicaciones sobrenaturales.

## Padre

- Personaje de apertura y cierre.
- Narrador de la historia de Janeth.
- Conserva su memoria para la hija.
- No jugable.

## Niña

- Hija de Janeth.
- Escucha la historia durante el apagón.
- No jugable.
- Su relación con Janeth se revela al final.

## Pescador

- Primer testigo de la anomalía.
- Presenta la radio.
- Ayuda a contextualizar el comportamiento extraño del pueblo y del faro.
- No debe resolver el misterio por Janeth.

## Vigilante del faro

- Vive o trabaja cerca del faro.
- Le pide a Janeth recuperar/reconstruir una fotografía.
- No entrega ningún código.
- No resuelve el acceso mediante una contraseña.
- Su relación exacta con la persona de la fotografía puede terminar de confirmarse.

---

# 22. Reglas narrativas

1. **No explicar antes de tiempo.**
2. El jugador debe descubrir relaciones, no recibirlas completas por diálogo.
3. El pescador y el vigilante aportan contexto, no soluciones.
4. El faro debe parecer importante antes de revelarse como falso origen.
5. Los códigos deben parecer misteriosos antes de comprenderse como coordenadas.
6. El teléfono debe tener una función real en la experiencia.
7. El magnetar resuelve el misterio científico, pero no el misterio emocional.
8. La muerte de Janeth se entiende por contexto; no se explica su causa.
9. El alebrije nunca debe convertirse en una exposición obvia.
10. La revelación de que Janeth es la madre debe ocurrir hasta el desenlace.
11. El rostro de Janeth nunca se muestra.
12. La historia debe poder completarse aproximadamente en 15 minutos.

---

# 23. Preguntas que todavía deben cerrarse

Estas decisiones **no están completamente definidas** y deben tratarse como pendientes:

### Faro

- ¿Qué impide exactamente pasar del nivel 1 al 2?
- ¿Qué impide pasar del nivel 2 al 3?
- ¿Los tres niveles se recorren únicamente por exploración o cada uno tendrá un micro-reto?
- ¿Qué muestra concretamente la fotografía?
- ¿La fotografía es definitivamente de la esposa del vigilante?

### Mirador del faro

- ¿Qué debe encontrar el jugador al ajustar la vista?
- ¿Cuánta superficie del lente debe quedar limpia para pasar a la segunda fase?
- ¿Qué rangos y tolerancia tendrán los ejes X e Y?
- ¿Cómo se confirma que la vista está correctamente alineada?
- ¿Los valores X e Y permanecerán visibles fuera del modo de depuración?

### Radio

- ¿Cuál es la interacción exacta del Puzzle 1?
- ¿Cómo se transforma el patrón de radio en una coordenada sin que parezca arbitrario?

### Coordenadas

- ¿Qué sistema astronómico se utilizará?
- ¿Cómo se representarán en la libreta?
- ¿Qué dato entrega la radio y cuál la fotografía?
- ¿Los valores serán reales, ficticios o adaptados?

### Observatorio

- ¿Cómo se abrirá la compuerta?
- ¿El jugador introduce manualmente las coordenadas o la libreta las transfiere?
- ¿Qué tipo de feedback tendrá el teléfono al acercarse a la orientación correcta?
- ¿Habrá vibración/hápticos?
- ¿Cómo se sincronizará técnicamente la orientación con Unity?
- ¿Cómo se integrará exactamente Stellarium?

### Magnetar

- Diseño visual definitivo.
- Intensidad de sus efectos.
- Qué tan científicamente realista será la explicación.
- Si se conservará la idea de transmisiones/voces antiguas dentro de la interferencia.

### Alebrije

- Diseño.
- Ubicación exacta en la introducción.
- Ubicación exacta en el desenlace.
- Relación concreta con Janeth.

### Desenlace

- Diálogo exacto del padre.
- Qué detalle hace que el jugador intuya definitivamente que Janeth falleció.
- Último plano antes de créditos/menú.

---

# 24. Prioridad para el MVP

## Esencial

- Movimiento de Janeth.
- Rotación de cámara 90°.
- Sistema de interacción.
- Diálogos básicos.
- Pueblo.
- Faro de tres niveles.
- Observatorio.
- Puzzle de radio.
- Tres fragmentos de fotografía.
- Puzzle de reconstrucción de fotografía.
- Mirador de la cima del faro: limpieza táctil del lente y ajuste con perillas X e Y.
- Dos coordenadas.
- Teléfono sincronizado con PC.
- Uso del giroscopio.
- Alineación del telescopio.
- Descubrimiento del magnetar.
- Cinemática de apertura.
- Cinemática final.
- Revelación de Janeth como madre.

## Implementación a validar

- Integración directa con Stellarium.
- Hápticos avanzados.
- Efectos complejos de postprocesado.
- Voces/transmisiones antiguas.
- Animaciones elaboradas de las cinemáticas.

---

# 25. Resumen en una frase por escena

### Escena 1 — Apertura
Un padre y su hija atraviesan un apagón durante una tormenta; él utiliza figuras de papel para comenzar a contarle la historia de Janeth.

### Escena 2 — Pueblo
Janeth investiga una radio alterada por una anomalía, obtiene un código que en realidad es una coordenada y sospecha del faro al verlo reaccionar.

### Escena 3 — Faro
Janeth recorre tres niveles, reconstruye la fotografía y obtiene una segunda coordenada; en la cima limpia el lente del mirador y ajusta la vista con dos perillas antes de continuar hacia el Observatorio.

### Escena 4 — Observatorio
Las dos coordenadas forman una posición astronómica; el jugador apunta físicamente el teléfono usando el giroscopio mientras el telescopio se alinea en la PC hasta localizar un magnetar.

### Escena 5 — Desenlace
La historia regresa a la casa y se revela que Janeth era la madre fallecida de la niña; el alebrije y otros objetos convierten todo lo jugado en los vestigios que quedan de ella.

---

# 26. Brújula final

Cuando exista una duda de diseño, preguntarse:

1. **¿Qué pasa?**
2. **¿Por qué pasa?**
3. **¿Qué queremos que el jugador entienda o experimente?**
4. **¿Qué hace el jugador?**
5. **¿Cómo se vuelve jugable?**
6. **¿Qué cambia después de esta escena?**
7. **¿Cómo conecta con la siguiente?**

Si una escena, puzzle, objeto o diálogo no ayuda a responder alguna de estas preguntas o no fortalece la investigación, la experiencia multidispositivo o la relación emocional final, debe simplificarse o eliminarse.

---

## Estado del documento

Esta versión incorpora los acuerdos actuales sobre:

- faro de **tres niveles**;
- **un fragmento de fotografía por nivel**;
- código de radio y código de fotografía convertidos en **coordenadas**;
- coordenadas utilizadas conjuntamente en el Observatorio;
- mirador de la cima del faro con dos fases: limpiar el lente con los dedos y ajustar X e Y con dos perillas;
- valores X e Y visibles para depuración en el prototipo del mirador;
- **teléfono apuntado físicamente mediante giroscopio**;
- sincronización del teléfono con el telescopio de PC;
- intención de explorar **Stellarium**;
- **magnetar** como anomalía;
- vigilante sin conocimiento del código;
- alebrije como elemento narrativo/foreshadowing;
- Janeth como madre de la niña;
- Janeth **fallecida**, con causa desconocida para el jugador.
