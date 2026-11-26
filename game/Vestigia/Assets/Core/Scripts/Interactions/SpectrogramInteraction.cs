using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SpectrogramInteraction : MonoBehaviour
{
    [Header("Jugador")]
    public PlayerMovement playerMovement;
    public CameraRotation cameraRotation;

    [Header("Telescopio")]
    public TelescopeInteraction telescopeInteraction;

    [Header("Interfaz")]
    public GameObject interactHint;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;

    [Header("Sonido del indicador")]
    public AudioSource hintAudioSource;
    public AudioClip hintAppearClip;

    [Header("Diálogo")]
    private readonly string[] lines =
    {
        "JANETH: No consigo identificar qué representa este registro.",
        "JANETH: La señal quedó registrada, pero todavía no sé de dónde proviene.",
        "JANETH: Tal vez pueda encontrar alguna correspondencia observando desde el telescopio."
    };

    private int currentLine;
    private bool playerTouching;
    private bool inDialogue;
    private bool hasInteracted;

    private void Start()
    {
        Debug.Log("SPECTROGRAM: Script iniciado.");

        if (interactHint != null)
            interactHint.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (cameraRotation == null)
            cameraRotation = FindFirstObjectByType<CameraRotation>();

        if (telescopeInteraction == null)
            telescopeInteraction = FindFirstObjectByType<TelescopeInteraction>();

        if (telescopeInteraction == null)
            Debug.LogError("SPECTROGRAM: No se encontró TelescopeInteraction.");
    }

    private void Update()
    {
        if (hasInteracted || Keyboard.current == null)
            return;

        if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        if (inDialogue)
        {
            NextLine();
            return;
        }

        if (playerTouching)
            StartDialogue();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerTouching = true;

        if (!hasInteracted)
            SetInteractHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerTouching = false;
        SetInteractHint(false);
    }

    private void SetInteractHint(bool visible)
    {
        if (interactHint == null)
            return;

        bool wasVisible = interactHint.activeSelf;
        interactHint.SetActive(visible);

        if (visible && !wasVisible &&
            hintAudioSource != null &&
            hintAppearClip != null)
        {
            hintAudioSource.PlayOneShot(hintAppearClip);
        }
    }

    private void StartDialogue()
    {
        Debug.Log("SPECTROGRAM: Iniciando interacción.");

        inDialogue = true;
        currentLine = 0;

        SetInteractHint(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            FinishInteraction();
            return;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        if (dialogueText != null)
            dialogueText.text = lines[currentLine];

        if (continueHint != null)
            continueHint.text = "ESPACIO — CONTINUAR";
    }

    private void FinishInteraction()
    {
        Debug.Log("SPECTROGRAM: Interacción terminada. Desbloqueando telescopio.");

        inDialogue = false;
        hasInteracted = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (telescopeInteraction != null)
            telescopeInteraction.UnlockTelescope();

        RestoreControls();
    }

    private void RestoreControls()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;
    }

    private void OnDisable()
    {
        if (inDialogue)
            RestoreControls();
    }
}