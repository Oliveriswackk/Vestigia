
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TelescopeInteraction : MonoBehaviour
{
    [Header("Jugador")]
    public PlayerMovement playerMovement;
    public CameraRotation cameraRotation;

    [Header("Interfaz")]
    public GameObject interactHint;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;
    public GameObject processPanel;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip voiceClip;

    [Header("Sonido del indicador")]
    public AudioSource hintAudioSource;
    public AudioClip hintAppearClip;

    private readonly string[] lines =
    {
        "JANETH: Las coordenadas coinciden con las de mis registros anteriores...",
        "JANETH: Pero todavía no consigo determinar qué estoy observando.",
        "JANETH: Una última comprobación..."
    };

    private int currentLine;
    private bool playerTouching;
    private bool inDialogue;
    private bool inProcess;
    private bool hasTalked;

    private void Start()
    {
        Debug.Log("TELESCOPE: Script iniciado.");

        if (interactHint != null)
            interactHint.SetActive(false);
        else
            Debug.LogError("TELESCOPE: Falta asignar InteractHint.");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogError("TELESCOPE: Falta asignar Dialogue Panel.");

        if (processPanel != null)
            processPanel.SetActive(false);
        else
            Debug.LogError("TELESCOPE: Falta asignar Process Panel.");
    }

    private void Update()
    {
        if (inProcess || hasTalked)
            return;

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        Debug.Log("TELESCOPE: Se detectó ESPACIO.");

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
        Debug.Log("TELESCOPE: Entró en la zona: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        playerTouching = true;

        if (!inDialogue && !hasTalked)
            SetInteractHint(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("TELESCOPE: Jugador salió de la zona.");

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

        Debug.Log("TELESCOPE: Indicador " +
                  (visible ? "activado." : "desactivado."));
    }

    private void StartDialogue()
    {
        Debug.Log("TELESCOPE: Iniciando diálogo.");

        inDialogue = true;
        currentLine = 0;

        SetInteractHint(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);
        else
            Debug.LogError("TELESCOPE: PlayerMovement no asignado.");

        if (cameraRotation != null)
            cameraRotation.enabled = false;
        else
            Debug.LogError("TELESCOPE: CameraRotation no asignado.");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            ShowProcess();
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

        if (audioSource != null && voiceClip != null)
            audioSource.PlayOneShot(voiceClip);

        Debug.Log("TELESCOPE: Mostrando línea " +
                  (currentLine + 1) + " de " + lines.Length);
    }

    private void ShowProcess()
    {
        Debug.Log("TELESCOPE: Iniciando proceso.");

        inDialogue = false;
        inProcess = true;
        hasTalked = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (processPanel != null)
            processPanel.SetActive(true);

        // Iniciar la simulación del acertijo.
        if (MobileBridge.Instance != null)
        {
            MobileBridge.Instance.StartPuzzle(
                "telescopio",
                OnPuzzleCompleted
            );
        }
        else
        {
            Debug.LogError(
                "TELESCOPE: No se encontró una instancia de MobileBridge."
            );
        }
    }

    private void OnPuzzleCompleted()
    {
        Debug.Log("TELESCOPE: Puzzle completado. Regresando al juego.");

        inProcess = false;

        if (processPanel != null)
            processPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;
    }

    private void OnDisable()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;
    }
}
