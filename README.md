# Vestigia

Repositorio principal del proyecto integrador **Vestigia**.

Vestigia es un MVP de videojuego de aventura y acertijos con enfoque en perspectiva, acompañado de una PWA y una aplicación móvil complementaria. Los componentes del proyecto se desarrollan de manera independiente y se integran mediante servicios compartidos y sincronización de información.

## Estructura del proyecto

La estructura raíz establece una organización común para los diferentes componentes del proyecto:

```text
vestigia/
│
├── assets/
├── game/
├── web/
├── mobile/
├── backend/
├── docs/
├── .gitignore
└── README.md
```

### `assets/`

Recursos generales y compartidos del proyecto que pueden ser utilizados por más de un componente, como personajes, referencias visuales, elementos gráficos, audio u otros recursos comunes.

### `game/`

Desarrollo del videojuego principal realizado en Unity y C#.

### `web/`

Desarrollo de la PWA que complementa la experiencia del videojuego.

### `mobile/`

Desarrollo de la aplicación móvil complementaria al videojuego.

### `backend/`

Servicios, configuraciones y componentes relacionados con la comunicación, almacenamiento y sincronización de información entre los diferentes componentes del proyecto.

### `docs/`

Documentación relacionada con la planeación, diseño, arquitectura, integración, pruebas y desarrollo del proyecto.

---

## Organización del trabajo

La estructura de las carpetas principales se mantiene como la **columna vertebral del repositorio**.

Cada área puede organizar internamente sus archivos, código y recursos de acuerdo con las necesidades de su componente, procurando mantener su trabajo dentro de la carpeta correspondiente.

La estructura interna de cada componente puede modificarse conforme avance el desarrollo y se definan las tecnologías, herramientas y responsabilidades del equipo.

La estructura raíz no se considera inmutable; cualquier cambio necesario deberá responder a las necesidades reales del proyecto y mantenerse consistente con la organización general.

## Desarrollo

El proyecto se desarrolla mediante iteraciones cortas, permitiendo ajustar funcionalidades, herramientas y responsabilidades conforme avance la implementación.

Los diferentes componentes pueden desarrollarse de forma paralela y posteriormente integrarse mediante los servicios compartidos definidos para el proyecto.

## Tecnologías

Las tecnologías contempladas inicialmente son:

* **Videojuego:** Unity + C#
* **PWA:** React + Vite
* **Aplicación móvil:** Flutter o React Native
* **Servicios compartidos:** Firebase
* **Control de versiones:** Git + GitHub
* **Diseño:** Figma

Las tecnologías marcadas como preliminares podrán modificarse durante el desarrollo de acuerdo con las necesidades del proyecto.

## Estado

**En desarrollo — MVP**

La estructura y documentación del repositorio se encuentran en construcción y podrán evolucionar junto con el proyecto.
