
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

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

    private IEnumerator YaroPopAnimation()
    {
        yaroCanvas.SetActive(true);
        yaroImage.localScale = Vector3.zero;

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

        // Mantener visible.
        yield return new WaitForSeconds(yaroDisplayDuration);

        // Desaparecer.
        elapsed = 0f;
        float hideDuration = 0.2f;

        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / hideDuration);

            yaroImage.localScale =
                Vector3.Lerp(Vector3.one, Vector3.zero, t);

            yield return null;
        }

        yaroImage.localScale = Vector3.zero;
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
