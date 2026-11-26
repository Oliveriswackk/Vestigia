using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VeladorInteraction : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;
    public PlayerMovement playerMovement;
    public CameraRotation cameraRotation;

    [Header("Figura del velador")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite talkingSprite;

    [Header("Interfaz de diálogo")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;
    public GameObject interactHint;

    [Header("Sonido del indicador")]
    public AudioSource hintAudioSource;
    public AudioClip hintAppearClip;

    [Header("Entrega de fotografía")]
    [SerializeField] private AudioSource interactionAudioSource;
    [SerializeField] private AudioClip photoDeliveryClip;

    [Header("Regalo de Yaro")]
    [SerializeField] private GameObject yaroCanvas;
    [SerializeField] private RectTransform yaroImage;
    [SerializeField] private float yaroDisplayDuration = 2.5f;

    [Header("Aura Misteriosa (Horror Cósmico / Suspenso)")]
    [SerializeField] private Color yaroAuraColor = new Color(0.82f, 0.68f, 0.26f, 0.28f);
    [SerializeField] private Color yaroEerieAccentColor = new Color(0.36f, 0.22f, 0.58f, 0.22f);
    [SerializeField, Range(6, 28)] private int yaroSparkCount = 16;

    [Header("Escenas")]
    [SerializeField] private string firstScene = "Escena3.1_PrimerPiso";
    [SerializeField] private string villageScene = "Escenario2_Pueblo";
    [SerializeField] private string villageSpawnPoint = "Spawn_Bosque";

    private readonly string[] firstVisitLines =
    {
        "VELADOR: ¿Viene por la luz? Lleva días encendiéndose sola...",
        "JANETH: ¿Cuánto tiempo lleva abandonado el faro?",
        "VELADOR: Unos días. Desde entonces no me he atrevido a volver a entrar.",
        "JANETH: ¿Dejó algo dentro?",
        "VELADOR: Mi mochila... La llave del segundo piso sigue ahí.",
        "VELADOR: Y hay una fotografía que dejé dentro de un libro. Se rompió en tres pedazos.",
        "VELADOR: Si la encuentra... ¿podría traérmela? Es importante para mí."
    };

    private readonly string[] returnLines =
    {
        "VELADOR: ¡Vaya! Ya empezaba a pensar que no iba a volver.",
        "JANETH: Encontré lo que dejó dentro. También... esto.",
        "VELADOR: Mi fotografía... Creí que se había perdido para siempre.",
        "VELADOR: No sabe cuánto se lo agradezco. Tiene muchos años conmigo.",
        "VELADOR: Espere, quisiera darle algo.",
        "VELADOR: Mi mujer las hace. Dice que cada una tiene algo especial y que sirven para espantar las malas cosas.",
        "JANETH: ¿Las malas cosas?",
        "VELADOR: Ja, ja. Ya sabe cómo son esas creencias. Yo no sé si funcionen, pero desde que tengo una, no me ha pasado nada malo.",
        "VELADOR: Quédese con ella. Después de lo que hizo por mí, es lo menos que puedo darle.",
        "JANETH: Muchas gracias. La cuidaré.",
        "VELADOR: Cuídese usted también, señorita. Y no se quede demasiado tiempo por aquí.",
        "JANETH: Lo intentaré. Hasta luego."
    };

    private int currentLine;

    private bool playerInRange;
    private bool inDialogue;
    private bool hasTalked;
    private bool isReturnVisit;

    private Coroutine yaroCoroutine;
    private string[] activeLines;

    // Elementos generados por código para el aura misteriosa
    private RectTransform auraRoot;
    private RectTransform haloUmbral;
    private CanvasGroup auraGroup;
    private Sprite softRadialSprite;
    private readonly List<RectTransform> sparks = new List<RectTransform>();
    private readonly List<Image> sparkImages = new List<Image>();
    private readonly List<Vector2> sparkBasePos = new List<Vector2>();
    private readonly List<float> sparkPhases = new List<float>();
    private readonly List<float> sparkSpeeds = new List<float>();

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);

        if (yaroCanvas != null)
            yaroCanvas.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            player = playerMovement.transform;

        if (cameraRotation == null)
            cameraRotation = FindFirstObjectByType<CameraRotation>();

        bool observatoryFound =
            GameProgress.Instance != null &&
            GameProgress.Instance.obs_encontrado;

        bool yaroAlreadyDelivered =
            GameProgress.Instance != null &&
            GameProgress.Instance.regalo_yaro_entregado;

        isReturnVisit = observatoryFound && !yaroAlreadyDelivered;

        if (yaroAlreadyDelivered)
        {
            hasTalked = true;
            Debug.Log("Velador: Yaro ya fue entregado. No se repetirá el diálogo.");
        }
        else if (isReturnVisit)
        {
            hasTalked = true;

            Debug.Log("Velador: regreso del observatorio. Iniciando diálogo.");

            StartCoroutine(StartReturnDialogue());
        }
    }

    private IEnumerator StartReturnDialogue()
    {
        yield return null;

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            player = playerMovement.transform;

        if (cameraRotation == null)
            cameraRotation = FindFirstObjectByType<CameraRotation>();

        StartDialogue(true);
    }

    private void Update()
    {
        if (inDialogue)
        {
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                NextLine();
            }

            return;
        }

        if (hasTalked || isReturnVisit)
            return;

        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartDialogue(false);
        }
    }

    private void SetInteractHint(bool visible)
    {
        if (interactHint == null)
            return;

        bool wasVisible = interactHint.activeSelf;

        if (visible == wasVisible)
            return;

        interactHint.SetActive(visible);

        if (visible &&
            hintAudioSource != null &&
            hintAppearClip != null)
        {
            hintAudioSource.PlayOneShot(hintAppearClip);
        }
    }

    private void StartDialogue(bool returning)
    {
        inDialogue = true;
        currentLine = 0;

        activeLines = returning ? returnLines : firstVisitLines;

        SetInteractHint(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        // Bloquear Q/E y girar la cámara hacia el velador.
        if (cameraRotation != null)
        {
            cameraRotation.SetInputEnabled(false);
            cameraRotation.FaceVelador();
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.sprite = talkingSprite;

        ShowLine();
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= activeLines.Length)
        {
            FinishDialogue();
            return;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        if (dialogueText != null)
            dialogueText.text = activeLines[currentLine];

        if (continueHint != null)
            continueHint.text = "ESPACIO — CONTINUAR";

        if (!isReturnVisit)
            return;

        // Entrega de fotografía: únicamente sonido.
        if (currentLine == 2)
        {
            if (interactionAudioSource != null &&
                photoDeliveryClip != null)
            {
                interactionAudioSource.PlayOneShot(photoDeliveryClip);
            }
        }

        // Entrega de Yaro.
        if (currentLine == 8)
        {
            ShowYaro();
        }
    }

    private void ShowYaro()
    {
        if (yaroCanvas == null || yaroImage == null)
        {
            Debug.LogWarning(
                "VeladorInteraction: falta asignar YaroCanvas o YaroImage."
            );

            return;
        }

        if (yaroCoroutine != null)
            StopCoroutine(yaroCoroutine);

        yaroCoroutine = StartCoroutine(YaroPopAnimation());
    }

    private void EnsureYaroMysteryEffect()
    {
        if (auraRoot != null || yaroCanvas == null || yaroImage == null)
            return;

        if (softRadialSprite == null)
            softRadialSprite = CreateSoftRadialSprite(128);

        GameObject rootObj = new GameObject(
            "YaroCosmicAura",
            typeof(RectTransform),
            typeof(CanvasGroup)
        );

        auraRoot = rootObj.GetComponent<RectTransform>();
        auraRoot.SetParent(yaroImage.parent, false);
        auraRoot.anchorMin = yaroImage.anchorMin;
        auraRoot.anchorMax = yaroImage.anchorMax;
        auraRoot.pivot = yaroImage.pivot;
        auraRoot.anchoredPosition = yaroImage.anchoredPosition;
        auraRoot.sizeDelta = yaroImage.sizeDelta;

        // Colocar detrás de yaroImage para no obstruir la figura.
        int yaroIndex = yaroImage.GetSiblingIndex();
        auraRoot.SetSiblingIndex(Mathf.Max(0, yaroIndex));
        yaroImage.SetSiblingIndex(yaroIndex + 1);

        auraGroup = rootObj.GetComponent<CanvasGroup>();
        auraGroup.alpha = 0f;
        auraGroup.interactable = false;
        auraGroup.blocksRaycasts = false;

        float baseSize = Mathf.Max(yaroImage.rect.width, yaroImage.rect.height);
        if (baseSize <= 1f)
            baseSize = Mathf.Max(yaroImage.sizeDelta.x, yaroImage.sizeDelta.y);
        if (baseSize <= 1f)
            baseSize = 220f;

        // Resplandor ámbar apagado detrás del objeto (como luz de faro en niebla)
        GameObject glowObj = new GameObject("EerieGlow", typeof(RectTransform), typeof(Image));
        RectTransform glowRect = glowObj.GetComponent<RectTransform>();
        glowRect.SetParent(auraRoot, false);
        glowRect.anchoredPosition = Vector2.zero;
        glowRect.sizeDelta = Vector2.one * (baseSize * 2.15f);

        Image glowImg = glowObj.GetComponent<Image>();
        glowImg.sprite = softRadialSprite;
        glowImg.color = new Color(yaroAuraColor.r, yaroAuraColor.g, yaroAuraColor.b, 0.24f);
        glowImg.raycastTarget = false;

        // Halo cósmico frío/violáceo elíptico alrededor
        GameObject haloObj = new GameObject("CosmicHalo", typeof(RectTransform), typeof(Image));
        haloUmbral = haloObj.GetComponent<RectTransform>();
        haloUmbral.SetParent(auraRoot, false);
        haloUmbral.anchoredPosition = Vector2.zero;
        haloUmbral.sizeDelta = new Vector2(baseSize * 1.75f, baseSize * 1.35f);

        Image haloImg = haloObj.GetComponent<Image>();
        haloImg.sprite = softRadialSprite;
        haloImg.color = new Color(yaroEerieAccentColor.r, yaroEerieAccentColor.g, yaroEerieAccentColor.b, 0.22f);
        haloImg.raycastTarget = false;

        // Motas/chispas erráticas alrededor del objeto
        for (int i = 0; i < yaroSparkCount; i++)
        {
            GameObject spObj = new GameObject("EerieMote_" + i, typeof(RectTransform), typeof(Image));
            RectTransform spRect = spObj.GetComponent<RectTransform>();
            spRect.SetParent(auraRoot, false);

            float angle = (Mathf.PI * 2f * i) / Mathf.Max(1, yaroSparkCount) + Random.Range(-0.22f, 0.22f);
            float radius = baseSize * Random.Range(0.48f, 0.92f);
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            spRect.anchoredPosition = pos;
            float moteSize = Random.Range(3.5f, 8f);
            spRect.sizeDelta = Vector2.one * moteSize;

            Image spImg = spObj.GetComponent<Image>();
            spImg.sprite = softRadialSprite;

            bool violetMote = (i % 4 == 0);
            spImg.color = violetMote
                ? new Color(0.62f, 0.42f, 0.92f, Random.Range(0.25f, 0.60f))
                : new Color(0.95f, 0.84f, 0.45f, Random.Range(0.25f, 0.65f));
            spImg.raycastTarget = false;

            sparks.Add(spRect);
            sparkImages.Add(spImg);
            sparkBasePos.Add(pos);
            sparkPhases.Add(Random.Range(0f, Mathf.PI * 2f));
            sparkSpeeds.Add(Random.Range(0.9f, 2.2f));
        }
    }

    private Sprite CreateSoftRadialSprite(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = ((x + 0.5f) / resolution) * 2f - 1f;
                float dy = ((y + 0.5f) / resolution) * 2f - 1f;
                float dist = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                float alpha = Mathf.Pow(1f - dist, 2.5f);

                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels32(pixels);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100f);
    }

    private IEnumerator YaroPopAnimation()
    {
        yaroCanvas.SetActive(true);
        yaroImage.localScale = Vector3.zero;
        yaroImage.localRotation = Quaternion.identity;

        EnsureYaroMysteryEffect();
        if (auraGroup != null)
            auraGroup.alpha = 0f;

        // Crecer.
        float elapsed = 0f;
        float growDuration = 0.22f;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / growDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            yaroImage.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one * 1.15f, t);

            yield return null;
        }

        // Ajustar tamaño.
        elapsed = 0f;
        float settleDuration = 0.12f;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / settleDuration);

            yaroImage.localScale =
                Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t);

            yield return null;
        }

        yaroImage.localScale = Vector3.one;

        // Mantener visible con aura y motas inquietantes alrededor.
        float holdElapsed = 0f;
        float fxTime = 0f;

        while (holdElapsed < yaroDisplayDuration)
        {
            float dt = Time.deltaTime;
            holdElapsed += dt;
            fxTime += dt;

            // Leve temblor/deriva en el objeto para reforzar lo desconocido
            float tilt = Mathf.Sin(fxTime * 2.4f) * 1.8f;
            yaroImage.localRotation = Quaternion.Euler(0f, 0f, tilt);

            if (auraRoot != null && auraGroup != null)
            {
                auraGroup.alpha = Mathf.Clamp01(holdElapsed / 0.25f);

                float flicker = 0.94f + 0.06f * Mathf.Sin(fxTime * 2.7f) + 0.02f * Mathf.Cos(fxTime * 7.3f);
                auraRoot.localScale = Vector3.one * flicker;

                if (haloUmbral != null)
                    haloUmbral.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(fxTime * 0.8f) * 14f);

                for (int i = 0; i < sparks.Count; i++)
                {
                    float phase = sparkPhases[i];
                    float speed = sparkSpeeds[i];

                    Vector2 drift = new Vector2(
                        Mathf.Cos(fxTime * speed + phase) * 9f,
                        Mathf.Sin(fxTime * (speed * 1.25f) + phase) * 11f + (fxTime * 4f) % 10f - 5f
                    );

                    sparks[i].anchoredPosition = sparkBasePos[i] + drift;

                    if (i < sparkImages.Count && sparkImages[i] != null)
                    {
                        Color c = sparkImages[i].color;
                        float baseAlpha = 0.35f + 0.35f * Mathf.Sin(fxTime * (speed * 2.2f) + phase);
                        c.a = Mathf.Clamp01(baseAlpha);
                        sparkImages[i].color = c;
                    }
                }
            }

            yield return null;
        }

        // Desaparecer.
        elapsed = 0f;
        float hideDuration = 0.2f;

        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / hideDuration);

            yaroImage.localScale =
                Vector3.Lerp(Vector3.one, Vector3.zero, t);

            if (auraGroup != null)
                auraGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        yaroImage.localScale = Vector3.zero;
        yaroImage.localRotation = Quaternion.identity;
        if (auraGroup != null)
            auraGroup.alpha = 0f;
        yaroCanvas.SetActive(false);

        yaroCoroutine = null;
    }

    private void FinishDialogue()
    {
        inDialogue = false;
        hasTalked = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;

        SetInteractHint(false);

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(true);

        if (SceneTransition.Instance == null)
        {
            Debug.LogError(
                "VeladorInteraction: No se encontró SceneTransition."
            );

            RestoreControls();
            return;
        }

        if (isReturnVisit)
        {
            if (!Application.CanStreamedLevelBeLoaded(villageScene))
            {
                Debug.LogError(
                    "VeladorInteraction: No se puede cargar la escena: " +
                    villageScene
                );

                RestoreControls();
                return;
            }

            if (GameProgress.Instance == null)
            {
                Debug.LogError(
                    "VeladorInteraction: No existe GameProgress.Instance."
                );

                RestoreControls();
                return;
            }

            // Registrar la entrega para que no se repita.
            GameProgress.Instance.SetRegaloYaroEntregado();

            Debug.Log(
                "Velador: diálogo terminado. Regresando al pueblo."
            );

            SceneTransition.Instance.ChangeScene(
                villageScene,
                villageSpawnPoint
            );
        }
        else
        {
            if (!Application.CanStreamedLevelBeLoaded(firstScene))
            {
                Debug.LogError(
                    "VeladorInteraction: No se puede cargar la escena: " +
                    firstScene
                );

                RestoreControls();
                return;
            }

            SceneTransition.Instance.ChangeScene(firstScene);
        }
    }

    private void RestoreControls()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(true);

        inDialogue = false;
        hasTalked = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement detectedPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (detectedPlayer == null)
            return;

        player = detectedPlayer.transform;
        playerMovement = detectedPlayer;

        playerInRange = true;

        if (!inDialogue && !hasTalked && !isReturnVisit)
            SetInteractHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement detectedPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (detectedPlayer == null ||
            detectedPlayer != playerMovement)
            return;

        playerInRange = false;
        SetInteractHint(false);
    }

    private void OnDisable()
    {
        if (inDialogue && playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(true);
    }
}