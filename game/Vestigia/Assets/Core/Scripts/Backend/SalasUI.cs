using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

// =============================================================
// INTERFAZ PRINCIPAL
// =============================================================

public class SalasUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private VestigiaCliente cliente;

    [Header("Escena")]
    [SerializeField] private string escenaIntroduccion = "Scene_01";

    // =========================================================
    // PALETA DE COLORES
    // =========================================================

    [Header("Paleta de colores")]

    // Fondo azul abismal.
    private readonly Color colorFondo =
        new Color(0.027f, 0.075f, 0.129f, 1f);

    // Superficies azul petróleo.
    private readonly Color colorTarjeta =
        new Color(0.063f, 0.157f, 0.239f, 0.97f);

    // Dorado champán.
    private readonly Color colorDorado =
        new Color(0.820f, 0.678f, 0.439f, 1f);

    // Dorado secundario.
    private readonly Color colorOroSecundario =
        new Color(0.663f, 0.518f, 0.310f, 1f);

    // Texto principal.
    private readonly Color colorTexto =
        new Color(0.816f, 0.894f, 0.914f, 1f);

    // Texto secundario.
    private readonly Color colorTextoSecundario =
        new Color(0.490f, 0.655f, 0.733f, 1f);

    // =========================================================
    // TARJETAS
    // =========================================================

    [Header("Tarjetas")]

    [SerializeField] private int columnas = 4;

    [SerializeField] private float anchoMaximoTarjeta = 340f;

    [SerializeField] private float anchoMinimoTarjeta = 220f;

    [SerializeField] private float altoTarjeta = 270f;

    [SerializeField] private float separacionTarjetas = 22f;

    // =========================================================
    // AMBIENTE
    // =========================================================

    [Header("Ambiente")]
    [SerializeField] private int cantidadGotas = 65;

    private RectTransform raiz;
    private RectTransform contenidoPueblos;
    private RectTransform viewportLista;

    private GridLayoutGroup gridPueblos;

    private Text textoEstado;
    private Text textoCodigo;
    private Text textoBotonPrincipal;

    private Button botonPrincipal;
    private Button botonCancelar;
    private Button botonActualizar;

    private int puebloSeleccionado = -1;

    private bool partidaIniciada;
    private bool cargandoPartida;

    private int operacionPartida;

    private float ultimoAnchoLista = -1f;

    private VestigiaCliente.Pueblo[] pueblos;

    private Font fuente;

    private GameObject objetoLista;

    // =========================================================
    // FRASES DE LOS PUEBLOS
    // =========================================================

    // Cada posición corresponde al índice del pueblo.
    // Se incluyen 101 frases para evitar repeticiones
    // dentro de la lista actual.

    private static readonly string[] frasesPueblos =
    {
        "Hay lugares que nunca terminan de despedirse.",
        "El silencio también guarda recuerdos.",
        "Algunas respuestas esperan bajo la superficie.",
        "Donde termina el camino, comienza el misterio.",
        "No todas las estrellas señalan el camino.",
        "Hay historias que sobreviven al tiempo.",
        "A veces, perderse es la única forma de encontrar algo.",
        "El horizonte siempre esconde una pregunta.",
        "Cada sombra conserva algo de luz.",
        "El pasado nunca está tan lejos como parece.",

        "Hay secretos que solo revela la marea.",
        "Incluso la noche tiene algo que contar.",
        "Algunos lugares recuerdan por nosotros.",
        "Lo desconocido también puede sentirse familiar.",
        "Las huellas permanecen aunque nadie las busque.",
        "No todo lo que duerme está en silencio.",
        "Hay destinos escritos en lugares inesperados.",
        "El tiempo no siempre se lleva lo que amamos.",
        "A veces, una señal cambia toda una vida.",
        "Cada rincón esconde una historia sin terminar.",

        "La distancia no siempre significa ausencia.",
        "Hay ecos que atraviesan generaciones.",
        "Lo que desaparece no siempre deja de existir.",
        "Incluso las ruinas conservan esperanza.",
        "El mar conoce nombres que nadie recuerda.",
        "Hay caminos que solo aparecen al anochecer.",
        "Toda luz proyecta una sombra.",
        "Quizá el misterio siempre estuvo frente a nosotros.",
        "Algunas noches parecen demasiado antiguas.",
        "El silencio puede ser una forma de memoria.",

        "Hay estrellas que parecen estar esperando.",
        "No todo lo perdido quiere ser encontrado.",
        "Cada lugar tiene su propia manera de recordar.",
        "A veces, el destino comienza con una pequeña señal.",
        "Lo imposible también deja rastros.",
        "Hay preguntas que el cielo nunca responde.",
        "Incluso la oscuridad tiene profundidad.",
        "El tiempo se detiene en ciertos recuerdos.",
        "Algunas despedidas nunca llegan a pronunciarse.",
        "Toda historia comienza en algún lugar.",

        "Los recuerdos también tienen un hogar.",
        "Hay secretos enterrados bajo la calma.",
        "No todo lo extraño viene de lejos.",
        "La noche revela lo que el día oculta.",
        "Algunos lugares parecen reconocernos.",
        "El universo guarda silencios inmensos.",
        "Una pequeña luz puede desafiar la oscuridad.",
        "Hay señales que no deberían existir.",
        "El pasado siempre encuentra una manera de volver.",
        "Quizá nunca estuvimos realmente solos.",

        "Hay historias escondidas entre las estrellas.",
        "Cada viaje deja algo que no puede llevarse.",
        "El misterio comienza cuando dejamos de mirar.",
        "No todas las respuestas necesitan palabras.",
        "La memoria es un mapa incompleto.",
        "Algunas cosas solo existen cuando alguien las recuerda.",
        "El mar nunca devuelve exactamente lo que recibe.",
        "Hay lugares donde el tiempo parece respirar.",
        "Una ausencia también puede llenar un espacio.",
        "El horizonte no es necesariamente el final.",

        "Lo desconocido puede tener un rostro familiar.",
        "Hay luces que sobreviven a las tormentas.",
        "Cada sombra tiene una historia detrás.",
        "A veces, el silencio es la última advertencia.",
        "El cielo también puede guardar cicatrices.",
        "Hay recuerdos que no pertenecen a nadie.",
        "No todo lo que brilla viene de las estrellas.",
        "El mundo es más grande que nuestros mapas.",
        "Algunas puertas solo se abren una vez.",
        "Lo que buscamos puede estar buscándonos también.",

        "Hay noches que cambian para siempre un lugar.",
        "La distancia entre dos recuerdos puede ser infinita.",
        "Incluso el vacío parece tener una voz.",
        "Toda señal proviene de algún lugar.",
        "El tiempo guarda secretos bajo su superficie.",
        "Hay caminos que nacen de una pérdida.",
        "A veces, recordar es volver a viajar.",
        "Las estrellas también conocen la soledad.",
        "Un lugar puede cambiar sin dejar de ser el mismo.",
        "No todas las despedidas son un final.",

        "Hay misterios que sobreviven a sus testigos.",
        "La calma nunca cuenta toda la historia.",
        "El universo no siempre está tan lejos.",
        "Algunas huellas conducen hacia lo imposible.",
        "La noche tiene sus propias constelaciones.",
        "Lo que callamos también forma parte de nosotros.",
        "Hay lugares que parecen fuera del tiempo.",
        "Una señal puede ser el comienzo de todo.",
        "El recuerdo de alguien puede iluminar un camino.",
        "A veces, el misterio es lo único que permanece.",

        "Incluso el silencio puede dejar una marca.",
        "Hay secretos que esperan bajo cielos tranquilos.",
        "Cada estrella es una historia que sigue viajando.",
        "No todo lo que se pierde desaparece.",
        "Algunos lugares son imposibles de olvidar.",
        "El pasado puede esconderse en la distancia.",
        "Hay preguntas que merecen ser perseguidas.",
        "La oscuridad también puede revelar belleza.",
        "Cada encuentro deja una pequeña señal.",
        "Quizá el final solo sea otra forma de comenzar.",

        "Hay algo eterno en los lugares que recordamos."
    };

    // =========================================================
    // CICLO DE VIDA
    // =========================================================

    private void Awake()
    {
        cliente = cliente != null
            ? cliente
            : VestigiaCliente.Instance;

        ConfigurarCanvas();
        CrearEventSystem();
        ConstruirInterfaz();
    }

    private void OnEnable()
    {
        BuscarCliente();

        if (cliente != null)
            SuscribirEventos();
    }

    private void Start()
    {
        BuscarCliente();

        if (cliente == null)
        {
            textoEstado.text =
                "No se encontró el cliente del backend.";

            return;
        }

        SuscribirEventos();

        Canvas.ForceUpdateCanvases();
        ActualizarDisenoLista();

        if (cliente.TieneSalaActiva)
        {
            MostrarEspera();

            if (cliente.AppEstaConectada)
            {
                textoEstado.text = "CONECTADO";
                PrepararPartida();
            }
        }
        else if (cliente.PueblosDisponibles != null)
        {
            ConstruirLista(cliente.PueblosDisponibles);
        }
        else
        {
            textoEstado.text = "Consultando pueblos...";
            cliente.ActualizarPueblos();
        }
    }

    private void Update()
    {
        if (cliente == null)
            BuscarCliente();

        if (cliente == null)
            return;

        ActualizarDisenoLista();
        ActualizarBotonPrincipal();

        if (cliente.TieneSalaActiva)
        {
            MostrarEspera();

            botonCancelar.gameObject.SetActive(true);

            botonCancelar.interactable =
                !cliente.CancelandoSala;

            if (!cliente.AppEstaConectada &&
                !cliente.CancelandoSala)
            {
                textoEstado.text =
                    "ESPERANDO A LA APLICACIÓN";

                textoCodigo.text =
                    "Código: " + cliente.Codigo;
            }
        }
        else if (cliente.EstaCreandoSala)
        {
            objetoLista.SetActive(false);
            botonCancelar.gameObject.SetActive(false);

            textoEstado.text =
                "PREPARANDO TU HISTORIA...";

            textoCodigo.text = "";
        }
        else
        {
            objetoLista.SetActive(true);

            botonCancelar.gameObject.SetActive(false);
            botonActualizar.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        DesuscribirEventos();
    }

    // =========================================================
    // CANVAS
    // =========================================================

    private void ConfigurarCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            return;

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;

        CanvasScaler scaler =
            GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920, 1080);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100;
    }

    // =========================================================
    // REFERENCIAS Y EVENTOS
    // =========================================================

    private void BuscarCliente()
    {
        if (cliente == null)
            cliente = VestigiaCliente.Instance;
    }

    private void SuscribirEventos()
    {
        if (cliente == null)
            return;

        cliente.PueblosActualizados -= ConstruirLista;
        cliente.PueblosActualizados += ConstruirLista;

        cliente.AppConectada -= ActualizarConexion;
        cliente.AppConectada += ActualizarConexion;

        cliente.SalaTerminada -= AlTerminarSala;
        cliente.SalaTerminada += AlTerminarSala;
    }

    private void DesuscribirEventos()
    {
        if (cliente == null)
            return;

        cliente.PueblosActualizados -= ConstruirLista;
        cliente.AppConectada -= ActualizarConexion;
        cliente.SalaTerminada -= AlTerminarSala;
    }

    // =========================================================
    // EVENT SYSTEM
    // =========================================================

    private void CrearEventSystem()
    {
        if (EventSystem.current != null)
            return;

        new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule)
        );
    }

    // =========================================================
    // CONSTRUCCIÓN DE INTERFAZ
    // =========================================================

    private void ConstruirInterfaz()
    {
        fuente = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf"
        );

        if (fuente == null)
        {
            fuente = Resources.GetBuiltinResource<Font>(
                "Arial.ttf"
            );
        }

        Transform anterior = transform.Find("VestigiaUI");

        if (anterior != null)
            Destroy(anterior.gameObject);

        GameObject contenedor = new GameObject(
            "VestigiaUI",
            typeof(RectTransform)
        );

        contenedor.transform.SetParent(transform, false);

        raiz = contenedor.GetComponent<RectTransform>();

        raiz.anchorMin = Vector2.zero;
        raiz.anchorMax = Vector2.one;
        raiz.offsetMin = Vector2.zero;
        raiz.offsetMax = Vector2.zero;

        Image fondo = contenedor.AddComponent<Image>();
        fondo.color = colorFondo;

        CrearVinetas();
        CrearLluvia();

        CrearTitulo();
        CrearEstado();

        CrearBotonMenu();

        CrearLista();

        CrearBotonPrincipal();
        CrearBotonCancelar();
        CrearBotonActualizar();

        CrearPie();
    }

    // =========================================================
    // FONDO Y VIÑETA
    // =========================================================

    private void CrearVinetas()
    {
        GameObject objeto = new GameObject(
            "Vinetas",
            typeof(RectTransform),
            typeof(Image)
        );

        objeto.transform.SetParent(raiz, false);

        RectTransform rect =
            objeto.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image imagen = objeto.GetComponent<Image>();

        imagen.sprite = CrearSpriteVinetas();
        imagen.color = Color.white;
        imagen.raycastTarget = false;
    }

    private Sprite CrearSpriteVinetas()
    {
        int resolucion = 256;

        Texture2D textura = new Texture2D(
            resolucion,
            resolucion,
            TextureFormat.RGBA32,
            false
        );

        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < resolucion; y++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                float px = (float)x / resolucion;
                float py = (float)y / resolucion;

                float dx = Mathf.Abs(px - 0.5f) * 2f;
                float dy = Mathf.Abs(py - 0.5f) * 2f;

                float distancia = Mathf.Max(dx, dy);

                float alpha = Mathf.SmoothStep(
                    0.02f,
                    1f,
                    distancia
                ) * 0.72f;

                textura.SetPixel(
                    x,
                    y,
                    new Color(
                        0.002f,
                        0.006f,
                        0.015f,
                        alpha
                    )
                );
            }
        }

        textura.Apply();

        return Sprite.Create(
            textura,
            new Rect(0, 0, resolucion, resolucion),
            new Vector2(0.5f, 0.5f)
        );
    }

    // =========================================================
    // LLUVIA
    // =========================================================

    private void CrearLluvia()
    {
        GameObject objeto = new GameObject(
            "Lluvia",
            typeof(RectTransform)
        );

        objeto.transform.SetParent(raiz, false);

        RectTransform rect =
            objeto.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        objeto.AddComponent<VestigiaLluviaUI>()
            .Inicializar(rect, cantidadGotas);
    }

    // =========================================================
    // TÍTULO
    // =========================================================

    private void CrearTitulo()
    {
        Text titulo = CrearTexto(
            raiz,
            "¿Dónde ocurre tu historia?",
            42,
            FontStyle.Normal,
            colorDorado,
            new Vector2(0.5f, 0.91f),
            new Vector2(0.88f, 75)
        );

        titulo.name = "TituloPrincipal";

        Text subtitulo = CrearTexto(
            raiz,
            "Cada lugar guarda algo que merece ser descubierto.",
            18,
            FontStyle.Normal,
            colorTextoSecundario,
            new Vector2(0.5f, 0.85f),
            new Vector2(0.88f, 38)
        );

        subtitulo.name = "Subtitulo";
    }

    // =========================================================
    // ESTADO
    // =========================================================

    private void CrearEstado()
    {
        textoEstado = CrearTexto(
            raiz,
            "Consultando pueblos...",
            16,
            FontStyle.Normal,
            colorDorado,
            new Vector2(0.5f, 0.785f),
            new Vector2(0.88f, 32)
        );

        textoCodigo = CrearTexto(
            raiz,
            "",
            17,
            FontStyle.Normal,
            colorTexto,
            new Vector2(0.5f, 0.745f),
            new Vector2(0.88f, 32)
        );
    }

    // =========================================================
    // LISTA DE PUEBLOS
    // =========================================================

    private void CrearLista()
    {
        objetoLista = new GameObject(
            "ListaPueblos",
            typeof(RectTransform),
            typeof(ScrollRect),
            typeof(Image)
        );

        objetoLista.transform.SetParent(raiz, false);

        RectTransform scrollRect =
            objetoLista.GetComponent<RectTransform>();

        scrollRect.anchorMin = new Vector2(0.07f, 0.19f);
        scrollRect.anchorMax = new Vector2(0.93f, 0.72f);

        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        Image fondoLista =
            objetoLista.GetComponent<Image>();

        fondoLista.color = new Color(0, 0, 0, 0);

        // Viewport.

        GameObject viewport = new GameObject(
            "Viewport",
            typeof(RectTransform),
            typeof(Image),
            typeof(Mask)
        );

        viewport.transform.SetParent(
            objetoLista.transform,
            false
        );

        viewportLista =
            viewport.GetComponent<RectTransform>();

        viewportLista.anchorMin = Vector2.zero;
        viewportLista.anchorMax = Vector2.one;

        viewportLista.offsetMin = Vector2.zero;
        viewportLista.offsetMax = Vector2.zero;

        Image imagenViewport =
            viewport.GetComponent<Image>();

        imagenViewport.color = Color.white;

        Mask mascara = viewport.GetComponent<Mask>();
        mascara.showMaskGraphic = false;

        // Barra de desplazamiento.

        Scrollbar barraVertical =
            CrearBarraVertical(objetoLista.transform);

        // Contenido.

        GameObject contenido = new GameObject(
            "Contenido",
            typeof(RectTransform),
            typeof(GridLayoutGroup),
            typeof(ContentSizeFitter)
        );

        contenido.transform.SetParent(
            viewport.transform,
            false
        );

        contenidoPueblos =
            contenido.GetComponent<RectTransform>();

        contenidoPueblos.anchorMin =
            new Vector2(0, 1);

        contenidoPueblos.anchorMax =
            new Vector2(1, 1);

        contenidoPueblos.pivot =
            new Vector2(0.5f, 1);

        contenidoPueblos.anchoredPosition =
            Vector2.zero;

        contenidoPueblos.sizeDelta =
            Vector2.zero;

        gridPueblos =
            contenido.GetComponent<GridLayoutGroup>();

        gridPueblos.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        gridPueblos.constraintCount = columnas;

        gridPueblos.spacing = new Vector2(
            separacionTarjetas,
            separacionTarjetas
        );

        gridPueblos.padding =
            new RectOffset(12, 12, 12, 12);

        gridPueblos.childAlignment =
            TextAnchor.UpperCenter;

        ContentSizeFitter fitter =
            contenido.GetComponent<ContentSizeFitter>();

        fitter.horizontalFit =
            ContentSizeFitter.FitMode.Unconstrained;

        fitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        // Configuración del ScrollRect.

        ScrollRect scroll =
            objetoLista.GetComponent<ScrollRect>();

        scroll.viewport = viewportLista;
        scroll.content = contenidoPueblos;

        scroll.horizontal = false;
        scroll.vertical = true;

        scroll.verticalScrollbar = barraVertical;

        scroll.verticalScrollbarVisibility =
            ScrollRect.ScrollbarVisibility
                .AutoHideAndExpandViewport;

        scroll.verticalScrollbarSpacing = -3f;

        scroll.movementType =
            ScrollRect.MovementType.Clamped;

        scroll.scrollSensitivity = 35;

        ActualizarDisenoLista();
    }

    // =========================================================
    // BARRA DE DESPLAZAMIENTO
    // =========================================================

    private Scrollbar CrearBarraVertical(Transform padre)
    {
        GameObject objeto = new GameObject(
            "ScrollbarVertical",
            typeof(RectTransform),
            typeof(Image),
            typeof(Scrollbar)
        );

        objeto.transform.SetParent(padre, false);

        RectTransform rect =
            objeto.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);

        rect.pivot = new Vector2(1f, 0.5f);

        rect.sizeDelta = new Vector2(14f, 0f);
        rect.anchoredPosition = new Vector2(-3f, 0f);

        Image fondo = objeto.GetComponent<Image>();

        fondo.color = new Color(
            0.20f,
            0.32f,
            0.39f,
            0.30f
        );

        Scrollbar barra =
            objeto.GetComponent<Scrollbar>();

        barra.direction =
            Scrollbar.Direction.BottomToTop;

        barra.targetGraphic = fondo;

        // Handle.

        GameObject handle = new GameObject(
            "Handle",
            typeof(RectTransform),
            typeof(Image)
        );

        handle.transform.SetParent(
            objeto.transform,
            false
        );

        RectTransform handleRect =
            handle.GetComponent<RectTransform>();

        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;

        handleRect.offsetMin = new Vector2(2f, 2f);
        handleRect.offsetMax = new Vector2(-2f, -2f);

        Image imagenHandle =
            handle.GetComponent<Image>();

        imagenHandle.color = new Color(
            colorDorado.r,
            colorDorado.g,
            colorDorado.b,
            0.85f
        );

        barra.handleRect = handleRect;

        return barra;
    }

    // =========================================================
    // DISEÑO RESPONSIVO
    // =========================================================

    private void ActualizarDisenoLista()
    {
        if (viewportLista == null || gridPueblos == null)
            return;

        float anchoDisponible = viewportLista.rect.width;

        if (anchoDisponible <= 0)
            return;

        if (Mathf.Abs(anchoDisponible - ultimoAnchoLista) < 1f)
            return;

        ultimoAnchoLista = anchoDisponible;

        float paddingHorizontal =
            gridPueblos.padding.left +
            gridPueblos.padding.right;

        int maxColumnas = Mathf.Clamp(columnas, 3, 4);

        int columnasActuales = Mathf.Clamp(
            Mathf.FloorToInt(
                (anchoDisponible -
                paddingHorizontal +
                separacionTarjetas) /
                (anchoMinimoTarjeta +
                separacionTarjetas)
            ),
            3,
            maxColumnas
        );

        gridPueblos.constraintCount = columnasActuales;

        float separacionTotal =
            separacionTarjetas * (columnasActuales - 1);

        float anchoTarjeta =
            (anchoDisponible -
            paddingHorizontal -
            separacionTotal) / columnasActuales;

        anchoTarjeta = Mathf.Clamp(
            anchoTarjeta,
            anchoMinimoTarjeta,
            anchoMaximoTarjeta
        );

        gridPueblos.cellSize = new Vector2(
            anchoTarjeta,
            altoTarjeta
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            contenidoPueblos
        );
    }

    // =========================================================
    // OBTENER FRASE DEL PUEBLO
    // =========================================================

    private string ObtenerFrasePueblo(int indice)
    {
        if (frasesPueblos == null ||
            frasesPueblos.Length == 0)
        {
            return "Cada lugar guarda una historia.";
        }

        return frasesPueblos[
            indice % frasesPueblos.Length
        ];
    }

    // =========================================================
    // TARJETAS
    // =========================================================

    private void CrearTarjetaPueblo(
        VestigiaCliente.Pueblo pueblo,
        int indice)
    {
        GameObject tarjeta = new GameObject(
            "Pueblo_" + pueblo.codigo,
            typeof(RectTransform)
        );

        tarjeta.transform.SetParent(
            contenidoPueblos,
            false
        );

        // Superficie.

        GameObject superficie = new GameObject(
            "Superficie",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        superficie.transform.SetParent(
            tarjeta.transform,
            false
        );

        RectTransform superficieRect =
            superficie.GetComponent<RectTransform>();

        superficieRect.anchorMin = Vector2.zero;
        superficieRect.anchorMax = Vector2.one;

        superficieRect.offsetMin = Vector2.zero;
        superficieRect.offsetMax = Vector2.zero;

        Image imagen =
            superficie.GetComponent<Image>();

        imagen.color = colorTarjeta;

        Button boton =
            superficie.GetComponent<Button>();

        boton.targetGraphic = imagen;

        ColorBlock colores = boton.colors;

        colores.normalColor = colorTarjeta;

        colores.highlightedColor = new Color(
            0.105f, 0.235f, 0.325f, 1f
        );

        colores.pressedColor = new Color(
            0.145f, 0.290f, 0.380f, 1f
        );

        colores.selectedColor = new Color(
            0.105f, 0.235f, 0.325f, 1f
        );

        colores.disabledColor = colorTarjeta;

        boton.colors = colores;

        // Halo dorado.

        GameObject haloObjeto = new GameObject(
            "HaloDorado",
            typeof(RectTransform),
            typeof(Image)
        );

        haloObjeto.transform.SetParent(
            superficie.transform,
            false
        );

        RectTransform haloRect =
            haloObjeto.GetComponent<RectTransform>();

        haloRect.anchorMin = Vector2.zero;
        haloRect.anchorMax = Vector2.one;

        haloRect.offsetMin = new Vector2(-5, -5);
        haloRect.offsetMax = new Vector2(5, 5);

        Image halo =
            haloObjeto.GetComponent<Image>();

        halo.sprite = CrearSpriteHalo();

        halo.color = new Color(
            colorDorado.r,
            colorDorado.g,
            colorDorado.b,
            0f
        );

        halo.raycastTarget = false;

        // Línea decorativa superior.

        GameObject linea = new GameObject(
            "LineaDorada",
            typeof(RectTransform),
            typeof(Image)
        );

        linea.transform.SetParent(
            superficie.transform,
            false
        );

        RectTransform lineaRect =
            linea.GetComponent<RectTransform>();

        lineaRect.anchorMin = new Vector2(0.10f, 0.91f);
        lineaRect.anchorMax = new Vector2(0.90f, 0.918f);

        lineaRect.offsetMin = Vector2.zero;
        lineaRect.offsetMax = Vector2.zero;

        Image imagenLinea =
            linea.GetComponent<Image>();

        imagenLinea.color = new Color(
            colorDorado.r,
            colorDorado.g,
            colorDorado.b,
            0.65f
        );

        imagenLinea.raycastTarget = false;

        // Número del lugar.

        CrearTexto(
            superficie.transform,
            (indice + 1).ToString("00"),
            14,
            FontStyle.Normal,
            colorOroSecundario,
            new Vector2(0.5f, 0.79f),
            new Vector2(0.85f, 28)
        );

        // Nombre del pueblo.

        Text nombre = CrearTexto(
            superficie.transform,
            pueblo.codigo,
            23,
            FontStyle.Bold,
            colorTexto,
            new Vector2(0.5f, 0.55f),
            new Vector2(0.88f, 65)
        );

        nombre.name = "NombrePueblo";

        // Frase individual.

        Text frase = CrearTexto(
            superficie.transform,
            ObtenerFrasePueblo(indice),
            14,
            FontStyle.Normal,
            colorTextoSecundario,
            new Vector2(0.5f, 0.25f),
            new Vector2(0.82f, 65)
        );

        frase.name = "FrasePueblo";

        // Efecto de hover y selección.

        VestigiaTarjetaHover efecto =
            superficie.AddComponent<VestigiaTarjetaHover>();

        efecto.Configurar(halo);

        boton.onClick.AddListener(
            () => SeleccionarPueblo(indice)
        );
    }

    // =========================================================
    // SPRITE DEL HALO
    // =========================================================

    private Sprite CrearSpriteHalo()
    {
        int resolucion = 128;

        Texture2D textura = new Texture2D(
            resolucion,
            resolucion,
            TextureFormat.RGBA32,
            false
        );

        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < resolucion; y++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                float px = (float)x / (resolucion - 1);
                float py = (float)y / (resolucion - 1);

                float distanciaBorde = Mathf.Min(
                    Mathf.Min(px, 1f - px),
                    Mathf.Min(py, 1f - py)
                );

                float alpha = 1f - Mathf.Clamp01(
                    distanciaBorde * 9f
                );

                alpha = Mathf.Pow(alpha, 2.5f) * 0.65f;

                textura.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    )
                );
            }
        }

        textura.Apply();

        return Sprite.Create(
            textura,
            new Rect(0, 0, resolucion, resolucion),
            new Vector2(0.5f, 0.5f)
        );
    }

    // =========================================================
    // SELECCIÓN DE PUEBLO
    // =========================================================

    private void SeleccionarPueblo(int indice)
    {
        if (cliente == null ||
            cliente.TieneSalaActiva ||
            pueblos == null ||
            indice < 0 ||
            indice >= pueblos.Length)
            return;

        puebloSeleccionado = indice;

        for (int i = 0;
             i < contenidoPueblos.childCount;
             i++)
        {
            Transform tarjeta =
                contenidoPueblos.GetChild(i);

            Transform superficie =
                tarjeta.Find("Superficie");

            if (superficie == null)
                continue;

            VestigiaTarjetaHover efecto =
                superficie.GetComponent<VestigiaTarjetaHover>();

            if (efecto != null)
                efecto.SetSeleccionada(i == indice);
        }

        textoEstado.text =
            "Pueblo seleccionado: " +
            pueblos[indice].codigo;

        ActualizarBotonPrincipal();
    }

    // =========================================================
    // CONSTRUCCIÓN DE LA LISTA
    // =========================================================

    private void ConstruirLista(
        VestigiaCliente.Pueblo[] lista)
    {
        pueblos = lista;
        puebloSeleccionado = -1;

        if (contenidoPueblos == null)
            return;

        foreach (Transform hijo in contenidoPueblos)
            Destroy(hijo.gameObject);

        if (lista == null || lista.Length == 0)
        {
            textoEstado.text =
                "No hay pueblos disponibles.";

            ActualizarBotonPrincipal();
            return;
        }

        for (int i = 0; i < lista.Length; i++)
        {
            CrearTarjetaPueblo(lista[i], i);
        }

        textoEstado.text =
            lista.Length + " pueblos disponibles";

        textoCodigo.text = "";

        Canvas.ForceUpdateCanvases();

        ultimoAnchoLista = -1f;

        ActualizarDisenoLista();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            contenidoPueblos
        );

        ActualizarBotonPrincipal();
    }

    // =========================================================
    // BOTONES
    // =========================================================

    private void CrearBotonPrincipal()
    {
        botonPrincipal = CrearBoton(
            raiz,
            "BotonIniciarHistoria",
            "INICIAR HISTORIA",
            new Vector2(0.5f, 0.115f),
            new Vector2(310, 58),
            colorTarjeta,
            colorDorado
        );

        textoBotonPrincipal =
            botonPrincipal.GetComponentInChildren<Text>();

        botonPrincipal.onClick.AddListener(
            EjecutarAccionPrincipal
        );
    }

    private void CrearBotonCancelar()
    {
        botonCancelar = CrearBoton(
            raiz,
            "BotonCancelar",
            "CANCELAR BÚSQUEDA",
            new Vector2(0.5f, 0.115f),
            new Vector2(260, 52),
            new Color(0.12f, 0.19f, 0.25f, 1f),
            colorTexto
        );

        botonCancelar.onClick.AddListener(CancelarSala);

        botonCancelar.gameObject.SetActive(false);
    }

    private void CrearBotonActualizar()
    {
        botonActualizar = CrearBoton(
            raiz,
            "BotonActualizar",
            "ACTUALIZAR PUEBLOS",
            new Vector2(0.5f, 0.045f),
            new Vector2(210, 32),
            new Color(0.025f, 0.045f, 0.075f, 0.8f),
            colorTextoSecundario
        );

        Text texto =
            botonActualizar.GetComponentInChildren<Text>();

        texto.fontSize = 12;

        botonActualizar.onClick.AddListener(() =>
        {
            if (cliente == null ||
                cliente.TieneSalaActiva ||
                cliente.EstaCreandoSala)
                return;

            textoEstado.text =
                "Actualizando pueblos...";

            cliente.ActualizarPueblos();
        });
    }

    private Button CrearBoton(
        Transform padre,
        string nombre,
        string contenido,
        Vector2 posicion,
        Vector2 dimensiones,
        Color color,
        Color colorTextoBoton)
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        objeto.transform.SetParent(padre, false);

        RectTransform rect =
            objeto.GetComponent<RectTransform>();

        rect.anchorMin = posicion;
        rect.anchorMax = posicion;

        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = dimensiones;

        Image imagen =
            objeto.GetComponent<Image>();

        imagen.color = color;

        Button boton =
            objeto.GetComponent<Button>();

        boton.targetGraphic = imagen;

        ColorBlock colores = boton.colors;

        colores.normalColor = color;

        colores.highlightedColor =
            Color.Lerp(color, colorDorado, 0.12f);

        colores.pressedColor =
            Color.Lerp(color, Color.black, 0.2f);

        colores.selectedColor =
            Color.Lerp(color, colorDorado, 0.08f);

        colores.disabledColor =
            new Color(0.3f, 0.3f, 0.3f, 0.5f);

        boton.colors = colores;

        CrearTexto(
            objeto.transform,
            contenido,
            15,
            FontStyle.Bold,
            colorTextoBoton,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.9f, dimensiones.y)
        );

        return boton;
    }

    // =========================================================
    // BOTÓN VOLVER AL MENÚ
    // =========================================================

    private void CrearBotonMenu()
    {
        Button boton = CrearBoton(
            raiz,
            "BotonVolverMenu",
            "← VOLVER AL MENÚ",
            new Vector2(0.12f, 0.94f),
            new Vector2(210, 42),
            new Color(0.025f, 0.052f, 0.075f, 0.85f),
            colorTextoSecundario
        );

        Text texto = boton.GetComponentInChildren<Text>();
        texto.fontSize = 13;

        boton.onClick.AddListener(VolverAlMenu);
    }

    private void VolverAlMenu()
    {
        if (cliente != null &&
            (cliente.TieneSalaActiva ||
            cliente.EstaCreandoSala ||
            cliente.CancelandoSala))
        {
            textoEstado.text =
                "Primero debes cancelar o cerrar la sala.";

            return;
        }

        SceneManager.LoadScene("Menu_Inicio");
    }

    // =========================================================
    // PIE
    // =========================================================

    private void CrearPie()
    {
        CrearTexto(
            raiz,
            "V E S T I G I A",
            11,
            FontStyle.Normal,
            new Color(
                colorDorado.r,
                colorDorado.g,
                colorDorado.b,
                0.65f
            ),
            new Vector2(0.5f, 0.012f),
            new Vector2(0.8f, 20)
        );
    }

    // =========================================================
    // TEXTOS
    // =========================================================

    private Text CrearTexto(
        Transform padre,
        string contenido,
        int tamano,
        FontStyle estilo,
        Color color,
        Vector2 posicion,
        Vector2 dimensiones)
    {
        GameObject objeto = new GameObject(
            "Texto",
            typeof(RectTransform),
            typeof(Text)
        );

        objeto.transform.SetParent(padre, false);

        RectTransform rect =
            objeto.GetComponent<RectTransform>();

        if (dimensiones.x > 0f &&
            dimensiones.x <= 1f)
        {
            float anchoRelativo = dimensiones.x;

            rect.anchorMin = new Vector2(
                posicion.x - anchoRelativo / 2f,
                posicion.y
            );

            rect.anchorMax = new Vector2(
                posicion.x + anchoRelativo / 2f,
                posicion.y
            );

            rect.sizeDelta = new Vector2(
                0f,
                dimensiones.y
            );
        }
        else
        {
            rect.anchorMin = posicion;
            rect.anchorMax = posicion;

            rect.sizeDelta = dimensiones;
        }

        rect.anchoredPosition = Vector2.zero;

        Text texto =
            objeto.GetComponent<Text>();

        texto.text = contenido;
        texto.font = fuente;
        texto.fontSize = tamano;
        texto.fontStyle = estilo;
        texto.color = color;

        texto.alignment =
            TextAnchor.MiddleCenter;

        texto.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        texto.verticalOverflow =
            VerticalWrapMode.Overflow;

        texto.raycastTarget = false;

        return texto;
    }

    // =========================================================
    // ESTADOS VISUALES
    // =========================================================

    private void MostrarEspera()
    {
        if (objetoLista != null)
            objetoLista.SetActive(false);

        botonActualizar.gameObject.SetActive(false);

        textoEstado.text =
            "ESPERANDO A LA APLICACIÓN";

        textoCodigo.text =
            "Código: " + cliente.Codigo;
    }

    // =========================================================
    // ACCIÓN PRINCIPAL
    // =========================================================

    private void EjecutarAccionPrincipal()
    {
        if (cliente == null)
            return;

        if (cliente.TieneSalaActiva)
        {
            if (cliente.AppEstaConectada)
                PrepararPartida();

            return;
        }

        if (cliente.EstaCreandoSala ||
            cliente.CancelandoSala)
            return;

        if (pueblos == null ||
            puebloSeleccionado < 0 ||
            puebloSeleccionado >= pueblos.Length)
        {
            textoEstado.text =
                "Selecciona un pueblo primero.";

            return;
        }

        cliente.CrearSala(
            pueblos[puebloSeleccionado].codigo
        );

        ActualizarBotonPrincipal();
    }

    // =========================================================
    // BOTÓN PRINCIPAL
    // =========================================================

    private void ActualizarBotonPrincipal()
    {
        if (cliente == null ||
            botonPrincipal == null ||
            textoBotonPrincipal == null)
            return;

        if (partidaIniciada || cargandoPartida)
        {
            textoBotonPrincipal.text =
                "INICIANDO...";

            botonPrincipal.interactable = false;
            return;
        }

        if (cliente.CancelandoSala)
        {
            textoBotonPrincipal.text =
                "CANCELANDO...";

            botonPrincipal.interactable = false;
            return;
        }

        if (cliente.EstaCreandoSala)
        {
            textoBotonPrincipal.text =
                "PREPARANDO...";

            botonPrincipal.interactable = false;
            return;
        }

        if (cliente.TieneSalaActiva)
        {
            textoBotonPrincipal.text =
                "ESPERANDO...";

            botonPrincipal.interactable = false;
            return;
        }

        textoBotonPrincipal.text =
            "INICIAR HISTORIA";

        botonPrincipal.interactable =
            cliente.BackendDisponible &&
            pueblos != null &&
            pueblos.Length > 0 &&
            puebloSeleccionado >= 0;
    }

    // =========================================================
    // CONEXIÓN DE LA APLICACIÓN
    // =========================================================

    private void ActualizarConexion(bool conectada)
    {
        if (cliente == null)
            return;

        if (conectada && cliente.TieneSalaActiva)
        {
            textoEstado.text = "CONECTADO";

            textoCodigo.text =
                "Aplicación vinculada correctamente.";

            Debug.Log(
                "[VESTIGIA] Aplicación conectada. " +
                "Iniciando historia."
            );

            ActualizarBotonPrincipal();

            PrepararPartida();
        }
        else if (!conectada && cliente.TieneSalaActiva)
        {
            textoEstado.text =
                "La aplicación se desconectó.";

            textoCodigo.text =
                "Código: " + cliente.Codigo;

            ActualizarBotonPrincipal();
        }
    }

    // =========================================================
    // INICIO DE PARTIDA
    // =========================================================

    private void PrepararPartida()
    {
        if (cliente == null ||
            !cliente.TieneSalaActiva ||
            !cliente.AppEstaConectada ||
            partidaIniciada ||
            cargandoPartida)
            return;

        cargandoPartida = true;

        int estaOperacion = ++operacionPartida;

        textoEstado.text = "CONECTADO";

        textoCodigo.text =
            "Preparando historia...";

        ActualizarBotonPrincipal();

        StartCoroutine(
            PrepararPartidaCoroutine(estaOperacion)
        );
    }

    private System.Collections.IEnumerator PrepararPartidaCoroutine(
        int estaOperacion)
    {
        bool correcto = false;

        yield return cliente.CambiarEscena(
            "introduccion",
            resultado =>
            {
                correcto = resultado;
            }
        );

        if (estaOperacion != operacionPartida ||
            !cliente.TieneSalaActiva)
        {
            cargandoPartida = false;
            yield break;
        }

        cargandoPartida = false;

        if (!correcto)
        {
            textoEstado.text =
                "No se pudo preparar la historia.";

            textoCodigo.text =
                "Intenta nuevamente o cancela la sala.";

            ActualizarBotonPrincipal();

            yield break;
        }

        partidaIniciada = true;

        Debug.Log(
            "[VESTIGIA] Iniciando cinemática: " +
            escenaIntroduccion
        );

        SceneManager.LoadScene(escenaIntroduccion);
    }

    // =========================================================
    // CANCELACIÓN
    // =========================================================

    private void CancelarSala()
    {
        if (cliente == null ||
            !cliente.TieneSalaActiva ||
            cliente.CancelandoSala)
            return;

        operacionPartida++;

        cargandoPartida = false;
        partidaIniciada = false;

        textoEstado.text =
            "Cancelando búsqueda...";

        textoCodigo.text = "";

        botonCancelar.interactable = false;

        ActualizarBotonPrincipal();

        cliente.CancelarSala();
    }

    // =========================================================
    // SALA TERMINADA
    // =========================================================

    private void AlTerminarSala()
    {
        operacionPartida++;

        cargandoPartida = false;
        partidaIniciada = false;

        textoCodigo.text = "";

        textoEstado.text =
            "Sala cerrada. Elige otro pueblo.";

        botonCancelar.gameObject.SetActive(false);
        botonCancelar.interactable = true;

        objetoLista.SetActive(true);

        botonActualizar.gameObject.SetActive(true);

        if (cliente != null)
            cliente.ActualizarPueblos();

        ActualizarBotonPrincipal();
    }
}

// =============================================================
// EFECTO DE HOVER Y SELECCIÓN DE TARJETAS
// =============================================================

public class VestigiaTarjetaHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private Image halo;

    private bool encima;
    private bool seleccionada;

    private float intensidadActual;

    private Vector3 escalaOriginal;

    public void Configurar(Image imagenHalo)
    {
        halo = imagenHalo;
        escalaOriginal = transform.localScale;
    }

    private void Update()
    {
        float intensidadObjetivo = seleccionada
            ? 0.55f
            : encima ? 0.42f : 0f;

        intensidadActual = Mathf.Lerp(
            intensidadActual,
            intensidadObjetivo,
            Time.unscaledDeltaTime * 7f
        );

        if (halo != null)
        {
            Color color = halo.color;
            color.a = intensidadActual;
            halo.color = color;
        }

        float escalaObjetivo = seleccionada
            ? 1.025f
            : encima ? 1.025f : 1f;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            escalaOriginal * escalaObjetivo,
            Time.unscaledDeltaTime * 8f
        );
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        encima = true;
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        encima = false;
    }

    public void SetSeleccionada(bool valor)
    {
        seleccionada = valor;
    }
}

// =============================================================
// LLUVIA PROCEDURAL
// =============================================================

public class VestigiaLluviaUI : MonoBehaviour
{
    private class Gota
    {
        public RectTransform rect;
        public float velocidad;
    }

    private readonly List<Gota> gotas =
        new List<Gota>();

    private RectTransform contenedor;

    public void Inicializar(
        RectTransform padre,
        int cantidad)
    {
        contenedor = padre;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject objeto = new GameObject(
                "Gota",
                typeof(RectTransform),
                typeof(Image)
            );

            objeto.transform.SetParent(
                contenedor,
                false
            );

            RectTransform rect =
                objeto.GetComponent<RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            float largo = Random.Range(9f, 24f);

            rect.sizeDelta =
                new Vector2(1f, largo);

            Image imagen =
                objeto.GetComponent<Image>();

            // Lluvia azul fría, no dorada.
            imagen.color = new Color(
                0.65f,
                0.75f,
                0.9f,
                Random.Range(0.035f, 0.11f)
            );

            imagen.raycastTarget = false;

            Gota gota = new Gota();

            gota.rect = rect;

            gota.velocidad =
                Random.Range(65f, 125f);

            gotas.Add(gota);

            ReiniciarGota(gota);
        }
    }

    private void Update()
    {
        if (contenedor == null)
            return;

        float ancho = contenedor.rect.width;
        float alto = contenedor.rect.height;

        if (ancho <= 0 || alto <= 0)
            return;

        foreach (Gota gota in gotas)
        {
            Vector2 posicion =
                gota.rect.anchoredPosition;

            posicion.y -=
                gota.velocidad *
                Time.unscaledDeltaTime;

            if (posicion.y < -alto / 2f - 20f)
            {
                ReiniciarGota(gota);
            }
            else
            {
                gota.rect.anchoredPosition =
                    posicion;
            }
        }
    }

    private void ReiniciarGota(Gota gota)
    {
        if (contenedor == null)
            return;

        float ancho = contenedor.rect.width;
        float alto = contenedor.rect.height;

        gota.rect.anchoredPosition =
            new Vector2(
                Random.Range(-ancho / 2f, ancho / 2f),
                Random.Range(-alto / 2f, alto / 2f)
            );
    }
}