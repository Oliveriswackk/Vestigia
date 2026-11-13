
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FuseBoxInteraction : MonoBehaviour
{
    [Header("Estados de la caja")]
    [SerializeField] private GameObject fusiblesCerrados;
    [SerializeField] private GameObject fusiblesAbiertos;

    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraRotation cameraRotation;

    [Header("Interfaz")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueHint;
    [SerializeField] private GameObject interactHint;

    [Header("Sonido de la pista")]
    [SerializeField] private AudioSource hintAudioSource;
    [SerializeField] private AudioClip hintAppearClip;

    [Header("Sonido de la caja")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoApertura;

    [Header("Diálogo")]
    [TextArea(2, 4)]
    [SerializeField] private string dialogueLine =
        "JANETH: No entiendo... El portafusibles está calcinado y, aun así, la luz sigue encendida. Si la señal tiene algo que ver con esto, quizá encuentre alguna pista más arriba.";

    private bool playerInRange;
    private bool inDialogue;
    private bool hasOpened;

    private void Start()
    {
        if (fusiblesCerrados != null)
            fusiblesCerrados.SetActive(true);

        if (fusiblesAbiertos != null)
            fusiblesAbiertos.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SetInteractHint(false);

        if (fusiblesCerrados == null)
            Debug.LogWarning("FuseBox: Falta Fusibles Cerrados", this);

        if (fusiblesAbiertos == null)
            Debug.LogWarning("FuseBox: Falta Fusibles Abiertos", this);

        if (dialoguePanel == null)
            Debug.LogWarning("FuseBox: Falta Dialogue Panel", this);

        if (dialogueText == null)
            Debug.LogWarning("FuseBox: Falta Dialogue Text", this);

        if (playerMovement == null)
            Debug.LogWarning("FuseBox: Falta Player Movement", this);
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        if (inDialogue)
        {
            FinishDialogue();
            return;
        }

        if (playerInRange && !hasOpened)
            OpenFuseBox();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement detectedPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (detectedPlayer == null)
            return;

        playerMovement = detectedPlayer;
        playerInRange = true;

        if (!hasOpened && !inDialogue)
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

    private void OpenFuseBox()
    {
        hasOpened = true;
        inDialogue = true;

        SetInteractHint(false);

        if (fusiblesCerrados != null)
            fusiblesCerrados.SetActive(false);

        if (fusiblesAbiertos != null)
            fusiblesAbiertos.SetActive(true);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = dialogueLine;

        if (continueHint != null)
            continueHint.text = "ESPACIO — CONTINUAR";

        if (audioSource != null && sonidoApertura != null)
            audioSource.PlayOneShot(sonidoApertura);
    }

    private void FinishDialogue()
    {
        inDialogue = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;
    }

    private void SetInteractHint(bool visible)
    {
        if (interactHint == null)
            return;

        bool wasVisible = interactHint.activeSelf;

        interactHint.SetActive(visible);

        if (visible && !wasVisible &&
            hintAudioSource != null && hintAppearClip != null)
        {
            hintAudioSource.PlayOneShot(hintAppearClip);
        }
    }

    private void OnDisable()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = true;
    }
}
