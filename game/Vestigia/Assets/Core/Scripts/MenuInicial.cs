using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

public class MenuInicial : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaSalas = "Salas";
    [SerializeField] private string escenaReanudar = "Escena3.1_PrimerPiso";

    [Header("Ambiente")]
    [SerializeField, Range(30, 300)] private int cantidadLluvia = 180;
    [SerializeField, Range(0.05f, 1.5f)] private float velocidadHaz = 0.3f;
    [Tooltip("Opcional. Si se deja vacío se genera un viento/estática por código.")]
    [SerializeField] private AudioSource audioAmbiente;
    [SerializeField, Range(0f, 1f)] private float volumenAmbiente = 0.35f;

    [Header("Fuentes (opcional)")]
    [Tooltip("Por ejemplo una fuente cursiva tipo Great Vibes.")]
    [SerializeField] private Font fuenteTitulo;
    [SerializeField] private Font fuenteTexto;

    [Header("Textos")]
    [SerializeField] private string etiquetaRegistro = "REGISTRO #04";

    // Paleta
    private readonly Color dorado = new Color(0.90f, 0.76f, 0.35f);
    private readonly Color colorTexto = new Color(0.80f, 0.84f, 0.89f);
    private readonly Color colorApagado = new Color(0.50f, 0.58f, 0.67f);

    // Jerarquía
    private RectTransform raiz;
    private RectTransform faroRoot;
    private RectTransform cieloRoot;
    private RectTransform titan;
    private RectTransform haz1;
    private RectTransform haz2;
    private CanvasGroup grupoAnomalia;
    private Image luzLinterna;
    private Image ventanaGlow;
    private Image reflejo;
    private RawImage grano;
    private RawImage lineas;

    // Interfaz
    private BotonUI infoIniciar;
    private BotonUI infoReanudar;
    private BotonUI infoRadio;
    private Text textoRadio;
    private Image lamparaRadio;

    // Sprites / texturas procedurales
    private Sprite spRadial;
    private Sprite spCirculo;
    private Sprite spHaz;
    private Sprite spAnillo;
    private Sprite spVigneta;
    private Texture2D texGrano;
    private Texture2D texLineas;

    // Estado
    private Font fuente;
    private bool sonidoActivo;
    private bool cargandoEscena;
    private float tiempo;
    private float anguloHaz = -Mathf.PI / 3f;
    private float alphaAnomalia;
    private float temporizadorGrano;
    private float volumenObjetivo;

    // Audio
    private AudioSource sfx;
    private AudioClip clipHover;
    private AudioClip clipClick;

    // Animación
    private const int TENTACULOS = 7;
    private const int SEGMENTOS = 14;
    private RectTransform[][] segmentosTentaculo;
    private readonly Vector2[] puntosTmp = new Vector2[SEGMENTOS + 1];

    private readonly List<Gota> gotas = new List<Gota>();
    private readonly List<Onda> olas = new List<Onda>();

    private class Gota
    {
        public RectTransform rect;
        public float velocidad;
    }

    private class Onda
    {
        public RectTransform rect;
        public Image imagen;
        public float alpha;
        public float fase;
        public float velocidad;
    }

    private class BotonUI
    {
        public Button boton;
        public Image fondo;
        public Image[] bordes;
        public Text etiqueta;
        public Color colorFondo;
        public Color colorFondoHover;
        public Color colorBorde;
        public Color colorEtiqueta;
    }

    // =========================================================
    // CICLO DE VIDA
    // =========================================================

    private void Awake()
    {
        ConfigurarCanvas();
        CrearEventSystem();
        ConstruirMenu();
        PrepararAudio();
    }

    private void Update()
    {
        if (raiz == null)
            return;

        float dt = Time.unscaledDeltaTime;
        tiempo += dt;

        AnimarHaz();
        AnimarAnomalia(dt);
        AnimarLinterna();
        AnimarMar();
        AnimarLluvia(dt);
        AnimarGrano(dt);
        AnimarAudio(dt);
    }

    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    private void ConfigurarCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetComponent<CanvasScaler>();

        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void CrearEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject sistema = new GameObject("EventSystem");
        sistema.AddComponent<EventSystem>();
        sistema.AddComponent<InputSystemUIInputModule>();
    }

    private void CargarFuentes()
    {
        if (fuenteTexto == null)
        {
            try
            {
                fuenteTexto = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (System.Exception)
            {
                fuenteTexto = null;
            }
        }

        if (fuenteTexto == null)
        {
            try
            {
                fuenteTexto = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch (System.Exception)
            {
                fuenteTexto = null;
            }
        }

        if (fuenteTexto == null)
            fuenteTexto = Font.CreateDynamicFontFromOSFont("Arial", 16);

        if (fuenteTitulo == null)
            fuenteTitulo = fuenteTexto;

        fuente = fuenteTexto;
    }

    private void ConstruirMenu()
    {
        CargarFuentes();
        CrearSprites();

        Transform anterior = transform.Find("MenuVestigia");

        if (anterior != null)
            Destroy(anterior.gameObject);

        GameObject contenedor = new GameObject("MenuVestigia", typeof(RectTransform));
        contenedor.transform.SetParent(transform, false);

        raiz = contenedor.GetComponent<RectTransform>();
        Estirar(raiz);

        CrearFondo();
        CrearCielo();
        CrearAnomalia();
        CrearMar();
        CrearFaro();
        CrearLluvia();
        CrearGrano();
        CrearVigneta();
        CrearScanlines();
        CrearInterfaz();
    }

    private void Estirar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void CrearSprites()
    {
        spRadial = CrearSpriteRadial();
        spCirculo = CrearSpriteCirculo();
        spHaz = CrearSpriteHaz();
        spAnillo = CrearSpriteAnillo();
        spVigneta = CrearSpriteVigneta();

        // Grano de película
        texGrano = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Color32[] g = new Color32[128 * 128];

        for (int i = 0; i < g.Length; i++)
        {
            byte v = (byte)Random.Range(0, 256);
            g[i] = new Color32(v, v, v, 255);
        }

        texGrano.SetPixels32(g);
        texGrano.wrapMode = TextureWrapMode.Repeat;
        texGrano.filterMode = FilterMode.Point;
        texGrano.Apply();

        // Scanlines
        texLineas = new Texture2D(1, 4, TextureFormat.RGBA32, false);
        texLineas.SetPixels32(new Color32[]
        {
            new Color32(0, 0, 0, 0),
            new Color32(0, 0, 0, 0),
            new Color32(0, 0, 0, 70),
            new Color32(0, 0, 0, 70)
        });
        texLineas.wrapMode = TextureWrapMode.Repeat;
        texLineas.filterMode = FilterMode.Point;
        texLineas.Apply();
    }

    // =========================================================
    // FONDO, CIELO Y ANOMALÍA CÓSMICA
    // =========================================================

    private void CrearFondo()
    {
        CrearImagen(raiz, "FondoNoche", new Color(0.008f, 0.016f, 0.035f),
            Vector2.zero, Vector2.one);
    }

    private void CrearCielo()
    {
        Image cielo = CrearImagen(raiz, "Cielo", Color.white,
            new Vector2(0f, 0.35f), Vector2.one);

        cielo.sprite = CrearGradienteVertical(
            new Color(0.008f, 0.020f, 0.047f),
            new Color(0.027f, 0.059f, 0.118f),
            new Color(0.043f, 0.086f, 0.165f));
    }

    private void CrearAnomalia()
    {
        GameObject go = new GameObject("AnomaliaCosmica",
            typeof(RectTransform), typeof(CanvasGroup));
        go.transform.SetParent(raiz, false);

        cieloRoot = go.GetComponent<RectTransform>();
        cieloRoot.anchorMin = new Vector2(0.5f, 0.80f);
        cieloRoot.anchorMax = new Vector2(0.5f, 0.80f);
        cieloRoot.sizeDelta = Vector2.zero;
        cieloRoot.anchoredPosition = Vector2.zero;

        grupoAnomalia = go.GetComponent<CanvasGroup>();
        grupoAnomalia.alpha = 0f;
        grupoAnomalia.interactable = false;
        grupoAnomalia.blocksRaycasts = false;

        // Aurora alienígena
        Image verde = Elemento(cieloRoot, "AuroraVerde", spRadial,
            new Color(0.063f, 0.725f, 0.506f, 0.35f),
            Vector2.zero, new Vector2(1800f, 520f));
        verde.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 5.7f);

        Image violeta = Elemento(cieloRoot, "AuroraVioleta", spRadial,
            new Color(0.576f, 0.200f, 0.918f, 0.50f),
            Vector2.zero, new Vector2(1200f, 380f));
        violeta.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 5.7f);

        // Entidad titánica
        GameObject tg = new GameObject("Titan", typeof(RectTransform));
        tg.transform.SetParent(cieloRoot, false);

        titan = tg.GetComponent<RectTransform>();
        titan.anchorMin = new Vector2(0.5f, 0.5f);
        titan.anchorMax = new Vector2(0.5f, 0.5f);
        titan.sizeDelta = Vector2.zero;
        titan.anchoredPosition = new Vector2(0f, 10f);
        titan.localScale = Vector3.one * 1.3f;

        Color oscuro = new Color(0.012f, 0.031f, 0.070f, 1f);

        Elemento(titan, "Cuerpo", spCirculo, oscuro, Vector2.zero, new Vector2(360f, 100f));

        segmentosTentaculo = new RectTransform[TENTACULOS][];

        for (int t = 0; t < TENTACULOS; t++)
        {
            segmentosTentaculo[t] = new RectTransform[SEGMENTOS];

            for (int s = 0; s < SEGMENTOS; s++)
            {
                Image seg = Elemento(titan, "Zarcillo", null, oscuro,
                    Vector2.zero, new Vector2(8f, 10f), new Vector2(0.5f, 0f));

                segmentosTentaculo[t][s] = seg.rectTransform;
            }
        }

        Color ojo = new Color(0.99f, 0.88f, 0.28f, 1f);

        Elemento(titan, "OjoResplandor", spRadial,
            new Color(ojo.r, ojo.g, ojo.b, 0.55f),
            Vector2.zero, new Vector2(70f, 70f));

        Elemento(titan, "OjoNucleo", spCirculo, ojo,
            Vector2.zero, new Vector2(12f, 12f));

        cieloRoot.gameObject.SetActive(false);
    }

    private void AnimarAnomalia(float dt)
    {
        // El haz revela la anomalía cuando apunta hacia arriba.
        float norm = Mathf.Atan2(Mathf.Sin(anguloHaz), Mathf.Cos(anguloHaz));
        bool apuntaArriba = norm < -Mathf.PI * 0.15f && norm > -Mathf.PI * 0.85f;

        float objetivo = 0f;

        if (apuntaArriba)
        {
            float u = (-norm - Mathf.PI * 0.15f) / (Mathf.PI * 0.7f);
            objetivo = Mathf.Sin(u * Mathf.PI) * 0.7f;
        }

        alphaAnomalia = Mathf.Lerp(alphaAnomalia, objetivo, 1f - Mathf.Exp(-3f * dt));

        bool visible = alphaAnomalia > 0.01f;

        if (cieloRoot.gameObject.activeSelf != visible)
            cieloRoot.gameObject.SetActive(visible);

        if (!visible)
            return;

        grupoAnomalia.alpha = Mathf.Clamp01(alphaAnomalia * 1.1f);

        titan.anchoredPosition = new Vector2(Mathf.Sin(tiempo * 0.5f) * 30f, 10f);

        for (int i = -3; i <= 3; i++)
        {
            int t = i + 3;

            Vector2 p0 = new Vector2(i * 45f, -20f);
            Vector2 p1 = new Vector2(i * 45f + Mathf.Sin(tiempo + i) * 40f, -90f);
            Vector2 p2 = new Vector2(i * 45f - Mathf.Cos(tiempo * 0.8f + i) * 50f, -160f);
            Vector2 p3 = new Vector2(i * 45f + Mathf.Sin(tiempo * 1.2f + i) * 30f, -230f);

            for (int s = 0; s <= SEGMENTOS; s++)
                puntosTmp[s] = Bezier(p0, p1, p2, p3, s / (float)SEGMENTOS);

            for (int s = 0; s < SEGMENTOS; s++)
            {
                Vector2 a = puntosTmp[s];
                Vector2 b = puntosTmp[s + 1];
                Vector2 d = b - a;

                RectTransform seg = segmentosTentaculo[t][s];
                float grosor = Mathf.Lerp(9f, 3f, s / (float)SEGMENTOS);

                seg.anchoredPosition = a;
                seg.sizeDelta = new Vector2(grosor, d.magnitude + 1.5f);
                seg.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f);
            }
        }
    }

    private Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float u)
    {
        float v = 1f - u;

        return v * v * v * p0
             + 3f * v * v * u * p1
             + 3f * v * u * u * p2
             + u * u * u * p3;
    }

    // =========================================================
    // MAR
    // =========================================================

    private void CrearMar()
    {
        Image mar = CrearImagen(raiz, "Mar", Color.white,
            Vector2.zero, new Vector2(1f, 0.38f));

        mar.sprite = CrearGradienteVertical(
            new Color(0.024f, 0.055f, 0.102f),
            new Color(0.012f, 0.027f, 0.059f),
            new Color(0.004f, 0.012f, 0.027f));

        CrearImagen(raiz, "LineaHorizonte",
            new Color(0.42f, 0.56f, 0.68f, 0.12f),
            new Vector2(0f, 0.378f), new Vector2(1f, 0.381f));

        reflejo = CrearImagenAnclada(raiz, "ReflejoFaro",
            new Color(1f, 0.82f, 0.35f, 0.14f),
            new Vector2(0.5f, 0.27f), new Vector2(90f, 330f));
        reflejo.sprite = spRadial;

        for (int i = 0; i < 46; i++)
        {
            float alpha = Random.Range(0.03f, 0.10f);

            Image ola = CrearImagenAnclada(raiz, "Ola",
                new Color(0.58f, 0.64f, 0.72f, alpha),
                new Vector2(Random.Range(0.02f, 0.98f), Random.Range(0.02f, 0.36f)),
                new Vector2(Random.Range(30f, 190f), Random.Range(1f, 2.5f)));

            olas.Add(new Onda
            {
                rect = ola.rectTransform,
                imagen = ola,
                alpha = alpha,
                fase = Random.Range(0f, 6.28f),
                velocidad = Random.Range(0.6f, 1.6f)
            });
        }
    }

    private void AnimarMar()
    {
        for (int i = 0; i < olas.Count; i++)
        {
            Onda o = olas[i];

            o.rect.anchoredPosition = new Vector2(
                Mathf.Sin(tiempo * 0.3f + o.fase) * 18f,
                Mathf.Sin(tiempo * 1.5f + o.fase) * 3f);

            Color c = o.imagen.color;
            c.a = o.alpha * (0.55f + 0.45f * Mathf.Sin(tiempo * o.velocidad + o.fase));
            o.imagen.color = c;
        }

        if (reflejo != null)
        {
            Color c = reflejo.color;
            c.a = 0.12f + Mathf.Sin(tiempo * 2.4f) * 0.04f;
            reflejo.color = c;
        }
    }

    // =========================================================
    // FARO
    // =========================================================

    private void CrearFaro()
    {
        GameObject go = new GameObject("Faro", typeof(RectTransform));
        go.transform.SetParent(raiz, false);

        faroRoot = go.GetComponent<RectTransform>();
        faroRoot.anchorMin = new Vector2(0.5f, 0.40f);
        faroRoot.anchorMax = new Vector2(0.5f, 0.40f);
        faroRoot.sizeDelta = Vector2.zero;
        faroRoot.anchoredPosition = Vector2.zero;
        faroRoot.localScale = Vector3.one * 1.4f;

        Vector2 centro = new Vector2(0.5f, 0.5f);

        // Rocas
        Elemento(faroRoot, "Rocas", spCirculo,
            new Color(0.020f, 0.039f, 0.078f), new Vector2(0f, -40f), new Vector2(240f, 60f));

        // Casita
        Elemento(faroRoot, "Casa", null,
            new Color(0.039f, 0.071f, 0.125f), new Vector2(37.5f, -9.5f), new Vector2(45f, 35f));

        Poligono(faroRoot, "TechoCasa", new Color(0.067f, 0.110f, 0.188f),
            new Vector2(37.5f, 17f), new Vector2(55f, 18f), centro,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f));

        ventanaGlow = Elemento(faroRoot, "VentanaResplandor", spRadial,
            new Color(0.92f, 0.70f, 0.03f, 0.35f), new Vector2(34.5f, -5f), new Vector2(55f, 55f));

        Elemento(faroRoot, "VentanaCasa", null,
            new Color(0.92f, 0.70f, 0.03f, 1f), new Vector2(34.5f, -5f), new Vector2(9f, 10f));

        // Torre a franjas
        const int franjas = 5;
        const float baseW = 42f;
        const float topW = 24f;
        const float torreH = 135f;
        float altoFranja = torreH / franjas;

        for (int i = 0; i < franjas; i++)
        {
            float w1 = baseW - (baseW - topW) * (i / (float)franjas);
            float w2 = baseW - (baseW - topW) * ((i + 1) / (float)franjas);
            float inset = (w1 - w2) / (2f * w1);

            Color c = (i % 2 == 0)
                ? new Color(0.345f, 0.110f, 0.090f)
                : new Color(0.510f, 0.570f, 0.660f);

            Poligono(faroRoot, "Franja" + i, c,
                new Vector2(0f, -20f + i * altoFranja),
                new Vector2(w1, altoFranja + 0.5f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f - inset, 1f), new Vector2(inset, 1f));
        }

        // Sombra del lado derecho de la torre
        Poligono(faroRoot, "SombraTorre", new Color(0.02f, 0.04f, 0.07f, 0.40f),
            new Vector2(0f, -20f), new Vector2(21f, torreH), new Vector2(0f, 0f),
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.571f, 1f), new Vector2(0f, 1f));

        // Galería
        Elemento(faroRoot, "Galeria", null,
            new Color(0.07f, 0.11f, 0.19f), new Vector2(0f, 117f), new Vector2(36f, 5f));

        // Resplandor y cristal de la linterna
        luzLinterna = Elemento(faroRoot, "LinternaResplandor", spRadial,
            new Color(1f, 0.85f, 0.40f, 0.50f), new Vector2(0f, 125f), new Vector2(170f, 170f));

        Elemento(faroRoot, "Linterna", null,
            new Color(0.996f, 0.941f, 0.541f), new Vector2(0f, 125f), new Vector2(20f, 20f));

        // Cúpula
        Vector2[] domo = new Vector2[13];

        for (int k = 0; k <= 12; k++)
        {
            float a = Mathf.PI * k / 12f;
            domo[k] = new Vector2(0.5f + 0.5f * Mathf.Cos(a), Mathf.Sin(a));
        }

        Poligono(faroRoot, "Cupula", new Color(0.027f, 0.051f, 0.094f),
            new Vector2(0f, 135f), new Vector2(26f, 13f), new Vector2(0.5f, 0f), domo);

        // Haces de luz
        Color colorHaz = new Color(1f, 0.92f, 0.55f, 0.55f);

        haz1 = Elemento(faroRoot, "Haz1", spHaz, colorHaz,
            new Vector2(0f, 125f), new Vector2(900f, 2000f), new Vector2(0.5f, 0f)).rectTransform;

        haz2 = Elemento(faroRoot, "Haz2", spHaz, colorHaz,
            new Vector2(0f, 125f), new Vector2(900f, 2000f), new Vector2(0.5f, 0f)).rectTransform;

        // Escala de los haces
        haz1.localScale = new Vector3(2.862463f, 0.02416191f, 1f);
        haz2.localScale = new Vector3(2.862463f, 0.02416191f, 1f);
    }

    private void AnimarHaz()
    {
        anguloHaz += velocidadHaz * Time.unscaledDeltaTime;

        float grados = -(anguloHaz + Mathf.PI * 0.5f) * Mathf.Rad2Deg;

        haz1.localRotation = Quaternion.Euler(0f, 0f, grados);
        haz2.localRotation = Quaternion.Euler(0f, 0f, grados + 180f);
    }

    private void AnimarLinterna()
    {
        if (luzLinterna != null)
            luzLinterna.rectTransform.localScale =
                Vector3.one * (0.92f + Mathf.Sin(tiempo * 3f) * 0.08f);

        if (ventanaGlow != null)
        {
            Color c = ventanaGlow.color;
            c.a = 0.25f + Mathf.PerlinNoise(tiempo * 2f, 0.3f) * 0.2f;
            ventanaGlow.color = c;
        }
    }

    // =========================================================
    // LLUVIA Y EFECTOS DE PELÍCULA
    // =========================================================

    private void CrearLluvia()
    {
        for (int i = 0; i < cantidadLluvia; i++)
        {
            Image img = CrearImagenAnclada(raiz, "GotaLluvia",
                new Color(0.80f, 0.84f, 0.88f, Random.Range(0.10f, 0.28f)),
                new Vector2(0.5f, 0.5f),
                new Vector2(Random.Range(1f, 2f), Random.Range(10f, 30f)));

            img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -6.8f);
            img.rectTransform.anchoredPosition = new Vector2(
                Random.Range(-960f, 960f), Random.Range(-540f, 540f));

            gotas.Add(new Gota
            {
                rect = img.rectTransform,
                velocidad = Random.Range(700f, 1300f)
            });
        }
    }

    private void AnimarLluvia(float dt)
    {
        float alto = raiz.rect.height;
        float ancho = raiz.rect.width;

        if (alto <= 0f || ancho <= 0f)
            return;

        for (int i = 0; i < gotas.Count; i++)
        {
            Gota g = gotas[i];
            Vector2 p = g.rect.anchoredPosition;

            p.y -= g.velocidad * dt;
            p.x -= g.velocidad * 0.12f * dt;

            if (p.y < -alto * 0.5f - 40f || p.x < -ancho * 0.5f - 40f)
            {
                p.x = Random.Range(-ancho * 0.5f, ancho * 0.5f + 120f);
                p.y = alto * 0.5f + Random.Range(0f, 60f);
            }

            g.rect.anchoredPosition = p;
        }
    }

    private void CrearGrano()
    {
        grano = CrearRawImagen("GranoPelicula", texGrano, new Color(1f, 1f, 1f, 0.05f));
    }

    private void CrearVigneta()
    {
        Image v = CrearImagen(raiz, "Vineta", Color.white, Vector2.zero, Vector2.one);
        v.sprite = spVigneta;
    }

    private void CrearScanlines()
    {
        lineas = CrearRawImagen("Scanlines", texLineas, new Color(1f, 1f, 1f, 0.6f));
    }

    private void AnimarGrano(float dt)
    {
        Rect r = raiz.rect;

        temporizadorGrano -= dt;

        if (temporizadorGrano <= 0f && grano != null)
        {
            temporizadorGrano = 0.07f;
            grano.uvRect = new Rect(Random.value, Random.value, r.width / 128f, r.height / 128f);
        }

        if (lineas != null)
            lineas.uvRect = new Rect(0f, 0f, 1f, r.height / 4f);
    }

    private RawImage CrearRawImagen(string nombre, Texture2D textura, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(raiz, false);

        Estirar(go.GetComponent<RectTransform>());

        RawImage raw = go.GetComponent<RawImage>();
        raw.texture = textura;
        raw.color = color;
        raw.raycastTarget = false;

        return raw;
    }

    // =========================================================
    // INTERFAZ
    // =========================================================

    private void CrearInterfaz()
    {
        Color apagado = colorApagado;

        // Cabecera izquierda
        CrearTexto(raiz, "V", 26, FontStyle.Bold, dorado,
            new Vector2(0.035f, 0.955f), new Vector2(40f, 40f), TextAnchor.MiddleCenter, fuenteTitulo);

        CrearTexto(raiz, "/", 14, FontStyle.Normal, apagado,
            new Vector2(0.050f, 0.955f), new Vector2(20f, 30f));

        CrearTexto(raiz, Espaciar("VESTIGIA"), 14, FontStyle.Bold, colorTexto,
            new Vector2(0.088f, 0.955f), new Vector2(170f, 30f));

        CrearTexto(raiz, "|", 14, FontStyle.Normal, apagado,
            new Vector2(0.125f, 0.955f), new Vector2(20f, 30f));

        CrearTexto(raiz, "PUERTO MEXICANO     -  47°09'S 126°43'W", 11, FontStyle.Normal, apagado,
            new Vector2(0.215f, 0.955f), new Vector2(380f, 30f));

        // Cabecera derecha
        CrearBotonRadio();

        CrearTexto(raiz, "12 NOV 1940  ·  03:14 AM", 11, FontStyle.Normal, apagado,
            new Vector2(0.925f, 0.955f), new Vector2(230f, 30f));

        CrearTitulo();
        CrearBotones();

        CrearTexto(raiz, "HORROR CÓSMICO ANALÓGICO  ·  CAPÍTULO I", 11, FontStyle.Normal,
            new Color(0.50f, 0.58f, 0.67f, 0.9f),
            new Vector2(0.5f, 0.085f), new Vector2(700f, 28f));

        CrearTexto(raiz, "TECLADO / MOUSE  |  TIEMPO ESTIMADO ~ 15 MIN", 10, FontStyle.Normal,
            new Color(0.43f, 0.50f, 0.60f, 0.9f),
            new Vector2(0.5f, 0.035f), new Vector2(700f, 25f));
    }

    private void CrearTitulo()
    {
        Image glow = CrearImagenAnclada(raiz, "TituloResplandor",
            new Color(0.90f, 0.76f, 0.35f, 0.07f),
            new Vector2(0.5f, 0.735f), new Vector2(1100f, 380f));
        glow.sprite = spRadial;

        Text titulo = CrearTexto(raiz, "Vestigia", 150, FontStyle.Italic, dorado,
            new Vector2(0.5f, 0.735f), new Vector2(1100f, 230f),
            TextAnchor.MiddleCenter, fuenteTitulo);

        Shadow sombra = titulo.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.9f);
        sombra.effectDistance = new Vector2(0f, -8f);

        CrearTexto(raiz, "No todo lo que desaparece deja de estar presente", 15,
            FontStyle.Normal, new Color(0.80f, 0.84f, 0.89f, 0.85f),
            new Vector2(0.5f, 0.650f), new Vector2(1000f, 34f));
    }

    private void CrearBotones()
    {
        infoIniciar = CrearBotonRetro("BotonIniciar", "INICIAR PARTIDA",
            new Vector2(0.5f, 0.220f), new Vector2(440f, 60f), true, null);

        infoReanudar = CrearBotonRetro("BotonReanudar", "REANUDAR PARTIDA",
            new Vector2(0.5f, 0.145f), new Vector2(440f, 60f), false, etiquetaRegistro);

        infoIniciar.boton.onClick.AddListener(
            () => CargarEscena(escenaSalas, "Sintonizando prólogo..."));

        infoReanudar.boton.onClick.AddListener(
            () => CargarEscena(escenaReanudar, "Reanudando frecuencia..."));

        AgregarEventosHover(infoIniciar, 160f);
        AgregarEventosHover(infoReanudar, 160f);
    }

    private BotonUI CrearBotonRetro(
        string nombre,
        string etiqueta,
        Vector2 anclaje,
        Vector2 tam,
        bool iconoPlay,
        string insignia)
    {
        BotonUI b = CrearBotonBase(
            nombre, anclaje, tam,
            new Color(0.03f, 0.055f, 0.10f, 0.92f),
            new Color(dorado.r, dorado.g, dorado.b, 0.22f),
            colorTexto);

        Transform t = b.boton.transform;

        Vector2[] icono = iconoPlay
            ? new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 1f) }
            : new Vector2[] { new Vector2(1f, 0f), new Vector2(0f, 0.5f), new Vector2(1f, 1f) };

        Poligono(t, "Icono", dorado, new Vector2(-190f, 0f), new Vector2(10f, 12f),
            new Vector2(0.5f, 0.5f), icono);

        b.etiqueta = CrearTexto(t, Espaciar(etiqueta), 13, FontStyle.Bold, colorTexto,
            new Vector2(0.5f, 0.5f), new Vector2(300f, tam.y), TextAnchor.MiddleLeft);
        b.etiqueta.rectTransform.anchoredPosition = new Vector2(-15f, 0f);

        if (insignia != null)
        {
            Image fondoInsignia = Elemento(t, "Insignia", null,
                new Color(dorado.r, dorado.g, dorado.b, 0.12f),
                new Vector2(150f, 0f), new Vector2(112f, 20f));

            for (int lado = 0; lado < 4; lado++)
                CrearBorde(fondoInsignia.transform,
                    new Color(dorado.r, dorado.g, dorado.b, 0.30f), lado, 1f);

            CrearTexto(fondoInsignia.transform, insignia, 9, FontStyle.Normal, dorado,
                new Vector2(0.5f, 0.5f), new Vector2(112f, 20f));
        }
        else
        {
            CrearTexto(t, "›", 22, FontStyle.Bold, colorApagado,
                new Vector2(0.5f, 0.5f), new Vector2(30f, tam.y)).rectTransform.anchoredPosition
                = new Vector2(190f, 0f);
        }

        return b;
    }

    private void CrearBotonRadio()
    {
        infoRadio = CrearBotonBase(
            "RadioToggle",
            new Vector2(0.790f, 0.955f), new Vector2(250f, 34f),
            new Color(0.03f, 0.05f, 0.09f, 0.85f),
            new Color(0.40f, 0.47f, 0.58f, 0.30f),
            colorTexto);

        Transform t = infoRadio.boton.transform;

        lamparaRadio = Elemento(t, "Lampara", spCirculo,
            new Color(0.40f, 0.47f, 0.58f, 1f), new Vector2(-100f, 0f), new Vector2(9f, 9f));

        textoRadio = CrearTexto(t, "SEÑAL RADIO: OFF", 12, FontStyle.Normal, colorTexto,
            new Vector2(0.5f, 0.5f), new Vector2(200f, 34f));
        textoRadio.rectTransform.anchoredPosition = new Vector2(10f, 0f);

        infoRadio.etiqueta = textoRadio;
        infoRadio.boton.onClick.AddListener(ToggleRadio);

        AgregarEventosHover(infoRadio, 0f);
    }

    private BotonUI CrearBotonBase(
        string nombre,
        Vector2 anclaje,
        Vector2 tam,
        Color colorFondo,
        Color colorBorde,
        Color colorEtiqueta)
    {
        GameObject go = new GameObject(nombre,
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(raiz, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anclaje;
        rect.anchorMax = anclaje;
        rect.sizeDelta = tam;
        rect.anchoredPosition = Vector2.zero;

        Image fondo = go.GetComponent<Image>();
        fondo.color = colorFondo;

        Button boton = go.GetComponent<Button>();
        boton.targetGraphic = fondo;
        boton.transition = Selectable.Transition.None;

        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        boton.navigation = nav;

        BotonUI info = new BotonUI();
        info.boton = boton;
        info.fondo = fondo;
        info.colorFondo = colorFondo;
        info.colorFondoHover = new Color(
            Mathf.Clamp01(colorFondo.r + 0.02f),
            Mathf.Clamp01(colorFondo.g + 0.03f),
            Mathf.Clamp01(colorFondo.b + 0.05f),
            Mathf.Clamp01(colorFondo.a + 0.04f));
        info.colorBorde = colorBorde;
        info.colorEtiqueta = colorEtiqueta;
        info.bordes = new Image[4];

        for (int lado = 0; lado < 4; lado++)
            info.bordes[lado] = CrearBorde(go.transform, colorBorde, lado, 1.5f);

        return info;
    }

    private void AgregarEventosHover(BotonUI info, float frecuenciaSfx)
    {
        EventTrigger trigger = info.boton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entrar = new EventTrigger.Entry();
        entrar.eventID = EventTriggerType.PointerEnter;
        entrar.callback.AddListener(datos =>
        {
            Resaltar(info, true);

            if (frecuenciaSfx > 0f)
                ReproducirSfx(clipHover);
        });

        EventTrigger.Entry salir = new EventTrigger.Entry();
        salir.eventID = EventTriggerType.PointerExit;
        salir.callback.AddListener(datos => Resaltar(info, false));

        trigger.triggers.Add(entrar);
        trigger.triggers.Add(salir);
    }

    private void Resaltar(BotonUI info, bool encendido)
    {
        if (info == null || !info.boton.interactable)
            return;

        Color borde = encendido
            ? new Color(dorado.r, dorado.g, dorado.b, 0.80f)
            : info.colorBorde;

        for (int i = 0; i < info.bordes.Length; i++)
            info.bordes[i].color = borde;

        info.fondo.color = encendido ? info.colorFondoHover : info.colorFondo;

        if (info.etiqueta != null)
            info.etiqueta.color = encendido
                ? new Color(1f, 0.98f, 0.92f)
                : info.colorEtiqueta;

        info.boton.transform.localScale = Vector3.one * (encendido ? 1.025f : 1f);
    }

    private Image CrearBorde(Transform padre, Color color, int lado, float grosor)
    {
        Image img = CrearImagen(padre, "Borde" + lado, color, Vector2.zero, Vector2.one);
        RectTransform r = img.rectTransform;

        switch (lado)
        {
            case 0:
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(0.5f, 1f);
                r.sizeDelta = new Vector2(0f, grosor);
                break;
            case 1:
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(1f, 0f);
                r.pivot = new Vector2(0.5f, 0f);
                r.sizeDelta = new Vector2(0f, grosor);
                break;
            case 2:
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0f, 0.5f);
                r.sizeDelta = new Vector2(grosor, 0f);
                break;
            default:
                r.anchorMin = new Vector2(1f, 0f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(1f, 0.5f);
                r.sizeDelta = new Vector2(grosor, 0f);
                break;
        }

        r.anchoredPosition = Vector2.zero;
        return img;
    }

    private string Espaciar(string texto)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < texto.Length; i++)
        {
            char c = texto[i];

            if (c == ' ')
            {
                sb.Append("   ");
            }
            else
            {
                sb.Append(c);

                if (i < texto.Length - 1 && texto[i + 1] != ' ')
                    sb.Append(' ');
            }
        }

        return sb.ToString();
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PrepararAudio()
    {
        if (audioAmbiente == null)
        {
            audioAmbiente = gameObject.AddComponent<AudioSource>();
            audioAmbiente.clip = GenerarAmbiente();
        }

        audioAmbiente.playOnAwake = false;
        audioAmbiente.loop = true;
        audioAmbiente.volume = 0f;
        audioAmbiente.Stop();

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.volume = 0.6f;

        clipHover = GenerarClick(160f);
        clipClick = GenerarClick(90f);
    }

    private void AnimarAudio(float dt)
    {
        if (audioAmbiente == null)
            return;

        audioAmbiente.volume = Mathf.MoveTowards(
            audioAmbiente.volume, volumenObjetivo, dt * 0.15f);

        if (!sonidoActivo && audioAmbiente.isPlaying && audioAmbiente.volume <= 0.001f)
            audioAmbiente.Stop();

        if (lamparaRadio != null)
        {
            if (sonidoActivo)
            {
                float p = 0.65f + 0.35f * Mathf.Sin(tiempo * 5f);
                lamparaRadio.color = new Color(dorado.r, dorado.g, dorado.b, p);
            }
            else
            {
                lamparaRadio.color = new Color(0.40f, 0.47f, 0.58f, 1f);
            }
        }
    }

    private void ToggleRadio()
    {
        sonidoActivo = !sonidoActivo;

        volumenObjetivo = sonidoActivo ? volumenAmbiente : 0f;

        if (sonidoActivo && audioAmbiente != null && !audioAmbiente.isPlaying)
            audioAmbiente.Play();

        if (textoRadio != null)
        {
            textoRadio.text = sonidoActivo ? "SEÑAL RADIO: ON" : "SEÑAL RADIO: OFF";
            textoRadio.color = sonidoActivo ? dorado : colorTexto;
        }

        if (infoRadio != null)
            infoRadio.colorEtiqueta = sonidoActivo ? dorado : colorTexto;

        ReproducirSfx(clipClick);
    }

    private void ReproducirSfx(AudioClip clip)
    {
        if (!sonidoActivo || sfx == null || clip == null)
            return;

        sfx.PlayOneShot(clip);
    }

    private AudioClip GenerarClick(float frecuencia)
    {
        const int sr = 44100;
        int n = (int)(sr * 0.13f);
        float[] datos = new float[n];
        float fase = 0f;

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float f = frecuencia * Mathf.Pow(30f / frecuencia, Mathf.Clamp01(t / 0.12f));

            fase += f / sr;

            float triangulo = 2f * Mathf.Abs(2f * (fase - Mathf.Floor(fase + 0.5f))) - 1f;
            float envolvente = 0.3f * Mathf.Pow(0.001f / 0.06f, Mathf.Clamp01(t / 0.12f));

            datos[i] = triangulo * envolvente;
        }

        AudioClip clip = AudioClip.Create("ClickVestigia", n, 1, sr, false);
        clip.SetData(datos, 0);
        return clip;
    }

    private AudioClip GenerarAmbiente()
    {
        const int sr = 44100;
        const int largo = sr * 4;
        const int cruce = sr / 2;
        const int total = largo + cruce;

        System.Random rnd = new System.Random(1978);

        float[] ruido = new float[total];
        float b0 = 0f, b1 = 0f, b2 = 0f, b3 = 0f, b4 = 0f, b5 = 0f, b6 = 0f;

        // Ruido rosa
        for (int i = 0; i < total; i++)
        {
            float blanco = (float)(rnd.NextDouble() * 2.0 - 1.0);

            b0 = 0.99886f * b0 + blanco * 0.0555179f;
            b1 = 0.99332f * b1 + blanco * 0.0750759f;
            b2 = 0.96900f * b2 + blanco * 0.1538520f;
            b3 = 0.86650f * b3 + blanco * 0.3104856f;
            b4 = 0.55000f * b4 + blanco * 0.5329522f;
            b5 = -0.7616f * b5 - blanco * 0.0168980f;

            ruido[i] = (b0 + b1 + b2 + b3 + b4 + b5 + b6 + blanco * 0.5362f) * 0.11f;
            b6 = blanco * 0.115926f;
        }

        // Filtro pasa-banda (viento helado)
        float w0 = 2f * Mathf.PI * 220f / sr;
        float alfa = Mathf.Sin(w0) / (2f * 3f);
        float a0 = 1f + alfa;
        float cb0 = alfa / a0;
        float cb2 = -alfa / a0;
        float ca1 = -2f * Mathf.Cos(w0) / a0;
        float ca2 = (1f - alfa) / a0;

        float[] viento = new float[total];
        float x1 = 0f, x2 = 0f, y1 = 0f, y2 = 0f;
        float maximo = 0.0001f;

        for (int i = 0; i < total; i++)
        {
            float x0 = ruido[i];
            float y0 = cb0 * x0 + cb2 * x2 - ca1 * y1 - ca2 * y2;

            x2 = x1;
            x1 = x0;
            y2 = y1;
            y1 = y0;

            viento[i] = y0;

            if (Mathf.Abs(y0) > maximo)
                maximo = Mathf.Abs(y0);
        }

        float escala = 0.8f / maximo;

        // Bucle sin clics (fundido cruzado) + ráfagas + estática de radio
        float[] datos = new float[largo];

        for (int i = 0; i < largo; i++)
        {
            float v;

            if (i < cruce)
            {
                float k = i / (float)cruce;
                v = viento[i] * k + viento[largo + i] * (1f - k);
            }
            else
            {
                v = viento[i];
            }

            float rafaga = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * i / largo);
            float estatica = (float)(rnd.NextDouble() * 2.0 - 1.0) * 0.06f;

            datos[i] = Mathf.Clamp(v * escala * rafaga + estatica, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("AmbienteVestigia", largo, 1, sr, false);
        clip.SetData(datos, 0);
        return clip;
    }

    // =========================================================
    // ELEMENTOS UI Y GEOMETRÍA
    // =========================================================

    private Image CrearImagen(
        Transform padre,
        string nombre,
        Color color,
        Vector2 minimo,
        Vector2 maximo)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = minimo;
        rect.anchorMax = maximo;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;

        return imagen;
    }

    private Image CrearImagenAnclada(
        Transform padre,
        string nombre,
        Color color,
        Vector2 anclaje,
        Vector2 dimensiones)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = anclaje;
        rect.anchorMax = anclaje;
        rect.sizeDelta = dimensiones;
        rect.anchoredPosition = Vector2.zero;

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;

        return imagen;
    }

    private Image Elemento(
        Transform padre,
        string nombre,
        Sprite sprite,
        Color color,
        Vector2 posicion,
        Vector2 tamano)
    {
        return Elemento(padre, nombre, sprite, color, posicion, tamano, new Vector2(0.5f, 0.5f));
    }

    private Image Elemento(
        Transform padre,
        string nombre,
        Sprite sprite,
        Color color,
        Vector2 posicion,
        Vector2 tamano,
        Vector2 pivot)
    {
        Image imagen = CrearImagenAnclada(padre, nombre, color, new Vector2(0.5f, 0.5f), tamano);

        imagen.sprite = sprite;
        imagen.rectTransform.pivot = pivot;
        imagen.rectTransform.anchoredPosition = posicion;

        return imagen;
    }

    private Image Poligono(
        Transform padre,
        string nombre,
        Color color,
        Vector2 posicion,
        Vector2 tamano,
        Vector2 pivot,
        params Vector2[] puntos)
    {
        Image imagen = Elemento(padre, nombre, CrearSpritePoligono(puntos),
            color, posicion, tamano, pivot);

        imagen.type = Image.Type.Simple;
        imagen.preserveAspect = false;

        return imagen;
    }

    private Text CrearTexto(
        Transform padre,
        string contenido,
        int tamano,
        FontStyle estilo,
        Color color,
        Vector2 anclaje,
        Vector2 dimensiones,
        TextAnchor alineacion = TextAnchor.MiddleCenter,
        Font fuenteUsar = null)
    {
        GameObject objeto = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = anclaje;
        rect.anchorMax = anclaje;
        rect.sizeDelta = dimensiones;
        rect.anchoredPosition = Vector2.zero;

        Text etiqueta = objeto.GetComponent<Text>();
        etiqueta.text = contenido;
        etiqueta.font = fuenteUsar != null ? fuenteUsar : fuente;
        etiqueta.fontSize = tamano;
        etiqueta.fontStyle = estilo;
        etiqueta.color = color;
        etiqueta.alignment = alineacion;
        etiqueta.horizontalOverflow = HorizontalWrapMode.Overflow;
        etiqueta.verticalOverflow = VerticalWrapMode.Overflow;
        etiqueta.raycastTarget = false;

        return etiqueta;
    }

    // =========================================================
    // TEXTURAS PROCEDURALES
    // =========================================================

    private Sprite CrearSprite(Texture2D textura)
    {
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.filterMode = FilterMode.Bilinear;
        textura.Apply();

        return Sprite.Create(
            textura,
            new Rect(0, 0, textura.width, textura.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }

    private Sprite CrearGradienteVertical(Color arriba, Color medio, Color abajo)
    {
        const int ancho = 4;
        const int alto = 256;

        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[ancho * alto];

        for (int y = 0; y < alto; y++)
        {
            float t = y / (float)(alto - 1);

            Color c = t < 0.5f
                ? Color.Lerp(abajo, medio, t * 2f)
                : Color.Lerp(medio, arriba, (t - 0.5f) * 2f);

            for (int x = 0; x < ancho; x++)
                px[y * ancho + x] = c;
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private Sprite CrearSpriteRadial()
    {
        const int res = 128;

        Texture2D textura = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = ((x + 0.5f) / res) * 2f - 1f;
                float dy = ((y + 0.5f) / res) * 2f - 1f;

                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);

                px[y * res + x] = new Color(1f, 1f, 1f, a);
            }
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private Sprite CrearSpriteCirculo()
    {
        const int res = 128;

        Texture2D textura = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = ((x + 0.5f) / res) * 2f - 1f;
                float dy = ((y + 0.5f) / res) * 2f - 1f;

                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((1f - d) * res * 0.5f);

                px[y * res + x] = new Color(1f, 1f, 1f, a);
            }
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private Sprite CrearSpriteAnillo()
    {
        const int res = 128;

        Texture2D textura = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = ((x + 0.5f) / res) * 2f - 1f;
                float dy = ((y + 0.5f) / res) * 2f - 1f;

                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float exterior = Mathf.Clamp01((1f - d) * res * 0.5f);
                float interior = Mathf.Clamp01((d - 0.78f) * res * 0.5f);

                float ang = (Mathf.Atan2(dy, dx) + Mathf.PI) / (2f * Mathf.PI);
                float brillo = Mathf.Lerp(0.12f, 1f, ang);

                px[y * res + x] = new Color(1f, 1f, 1f, exterior * interior * brillo);
            }
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private Sprite CrearSpriteHaz()
    {
        const int ancho = 128;
        const int alto = 256;

        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[ancho * alto];

        for (int y = 0; y < alto; y++)
        {
            float ny = y / (float)(alto - 1);

            // Cono: estrecho en la base, se abre hacia el extremo.
            float limite = Mathf.Lerp(0.01f, 0.5f, ny);

            for (int x = 0; x < ancho; x++)
            {
                float nx = x / (float)(ancho - 1);
                float distancia = Mathf.Abs(nx - 0.5f);

                float borde = 1f - Mathf.SmoothStep(limite * 0.5f, limite, distancia);
                float a = borde * Mathf.Pow(1f - ny, 1.5f);

                px[y * ancho + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            }
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private Sprite CrearSpriteVigneta()
    {
        const int res = 128;

        Texture2D textura = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[res * res];

        float maxDist = Mathf.Sqrt(0.5f);

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = (x + 0.5f) / res - 0.5f;
                float dy = (y + 0.5f) / res - 0.5f;

                float d = Mathf.Sqrt(dx * dx + dy * dy) / maxDist;

                float a;

                if (d < 0.35f)
                    a = 0f;
                else if (d < 0.75f)
                    a = Mathf.Lerp(0f, 0.75f, (d - 0.35f) / 0.40f);
                else
                    a = Mathf.Lerp(0.75f, 0.98f, (d - 0.75f) / 0.25f);

                px[y * res + x] = new Color(0.008f, 0.016f, 0.035f, a);
            }
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private Sprite CrearSpritePoligono(Vector2[] puntos)
    {
        const int res = 64;

        Texture2D textura = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color32[] px = new Color32[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int aciertos = 0;

                for (int sy = 0; sy < 2; sy++)
                {
                    for (int sx = 0; sx < 2; sx++)
                    {
                        Vector2 p = new Vector2(
                            (x + (sx + 0.5f) / 2f) / res,
                            (y + (sy + 0.5f) / 2f) / res);

                        if (PuntoDentroPoligono(p, puntos))
                            aciertos++;
                    }
                }

                px[y * res + x] = new Color32(255, 255, 255, (byte)(aciertos * 255 / 4));
            }
        }

        textura.SetPixels32(px);
        return CrearSprite(textura);
    }

    private bool PuntoDentroPoligono(Vector2 punto, Vector2[] poligono)
    {
        bool dentro = false;
        int j = poligono.Length - 1;

        for (int i = 0; i < poligono.Length; j = i++)
        {
            bool cruza =
                ((poligono[i].y > punto.y) != (poligono[j].y > punto.y)) &&
                (punto.x < (poligono[j].x - poligono[i].x) *
                (punto.y - poligono[i].y) /
                (poligono[j].y - poligono[i].y + 0.00001f) +
                poligono[i].x);

            if (cruza)
                dentro = !dentro;
        }

        return dentro;
    }

    // =========================================================
    // CAMBIO DE ESCENA
    // =========================================================

    private void CargarEscena(string escena, string mensaje)
    {
        if (cargandoEscena)
            return;

        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            Debug.LogError(
                "[VESTIGIA] No se encuentra la escena: " + escena +
                ". Comprueba Build Profiles → Scene List."
            );

            return;
        }

        StartCoroutine(CargarEscenaCoroutine(escena, mensaje));
    }

    private IEnumerator CargarEscenaCoroutine(string escena, string mensaje)
    {
        cargandoEscena = true;

        infoIniciar.boton.interactable = false;
        infoReanudar.boton.interactable = false;
        infoRadio.boton.interactable = false;

        ReproducirSfx(clipClick);

        GameObject cortina = new GameObject("CortinaCarga",
            typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        cortina.transform.SetParent(raiz, false);

        Estirar(cortina.GetComponent<RectTransform>());

        Image fondo = cortina.GetComponent<Image>();
        fondo.color = Color.black;
        fondo.raycastTarget = true;

        CanvasGroup grupo = cortina.GetComponent<CanvasGroup>();
        grupo.alpha = 0f;

        Image anillo = Elemento(cortina.transform, "Spinner", spAnillo,
            new Color(dorado.r, dorado.g, dorado.b, 0.9f),
            new Vector2(0f, 70f), new Vector2(48f, 48f));

        CrearTexto(cortina.transform, mensaje.ToUpper(), 22, FontStyle.Bold, dorado,
            new Vector2(0.5f, 0.5f), new Vector2(900f, 50f));

        CrearTexto(cortina.transform,
            "\"Creíamos que la luz del faro vigilaba la costa. En realidad, señalaba hacia arriba.\"",
            14, FontStyle.Italic, colorApagado,
            new Vector2(0.5f, 0.43f), new Vector2(1000f, 40f));

        float duracion = 1.0f;
        float transcurrido = 0f;

        while (transcurrido < duracion)
        {
            float dt = Time.unscaledDeltaTime;

            transcurrido += dt;
            grupo.alpha = Mathf.Clamp01(transcurrido / duracion);
            anillo.rectTransform.Rotate(0f, 0f, -360f * dt);

            yield return null;
        }

        grupo.alpha = 1f;

        float espera = 0.4f;

        while (espera > 0f)
        {
            float dt = Time.unscaledDeltaTime;

            espera -= dt;
            anillo.rectTransform.Rotate(0f, 0f, -360f * dt);

            yield return null;
        }

        SceneManager.LoadScene(escena);
    }
}