
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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

    [Header("Interfaz")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;
    public GameObject interactHint;

    [Header("Sonido del indicador")]
    public AudioSource hintAudioSource;
    public AudioClip hintAppearClip;

    [Header("Escena siguiente")]
    [SerializeField] private string nextScene =
        "Escena3.1_PrimerPiso";

    private string[] lines =
    {
        "VELADOR: ¿Viene por la luz? Lleva días encendiéndose sola...",
        "JANETH: ¿Cuánto tiempo lleva abandonado el faro?",
        "VELADOR: Unos días. Desde entonces no me he atrevido a volver a entrar.",
        "JANETH: ¿Dejó algo dentro?",
        "VELADOR: Mi mochila... La llave del segundo piso sigue ahí.",
        "VELADOR: Y hay una fotografía que dejé dentro de un libro. Se rompió en tres pedazos.",
        "VELADOR: Si la encuentra... ¿podría traérmela? Es importante para mí."
    };

    private int currentLine;
    private bool playerInRange;
    private bool inDialogue;
    private bool hasTalked;
    private bool controlsLocked;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;
    }

    void Update()
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

        if (hasTalked)
            return;

        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartDialogue();
        }
    }

    void SetInteractHint(bool visible)
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

    void StartDialogue()
    {
        inDialogue = true;
        currentLine = 0;

        SetInteractHint(false);

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
            controlsLocked = true;
        }

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (spriteRenderer != null)
            spriteRenderer.sprite = talkingSprite;

        ShowLine();
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            FinishDialogue();
            return;
        }

        ShowLine();
    }

    void ShowLine()
    {
        if (dialogueText != null)
            dialogueText.text = lines[currentLine];

        if (continueHint != null)
            continueHint.text = "ESPACIO — CONTINUAR";
    }

    void FinishDialogue()
    {
        inDialogue = false;
        hasTalked = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;

        SetInteractHint(false);

        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError(
                "VeladorInteraction: No se puede cargar la escena: " +
                nextScene
            );

            RestoreControls();
            return;
        }

        if (SceneTransition.Instance == null)
        {
            Debug.LogError(
                "VeladorInteraction: No se encontró SceneTransition."
            );

            RestoreControls();
            return;
        }

        // La transición controla el bloqueo del jugador.
        // Restauramos la cámara para la siguiente escena.
        if (cameraRotation != null)
            cameraRotation.enabled = true;

        SceneTransition.Instance.ChangeScene(nextScene);
    }

    void RestoreControls()
    {
        if (controlsLocked && playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;

        controlsLocked = false;
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

        if (!inDialogue && !hasTalked)
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

    void OnDisable()
    {
        if (controlsLocked && playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;

        controlsLocked = false;
    }
}
