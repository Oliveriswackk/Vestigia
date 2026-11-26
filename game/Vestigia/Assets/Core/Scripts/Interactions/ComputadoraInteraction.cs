
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ComputadoraInteraction : MonoBehaviour
{
    [Header("Jugador")]
    public PlayerMovement playerMovement;
    public CameraRotation cameraRotation;

    [Header("Elementos de la computadora")]
    public GameObject pantallaEncendida;
    public GameObject espectrograma;

    [Header("Indicador de interacción")]
    public GameObject interactHint;

    [Header("DialogueCanvas")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;

    [Header("Sonido de impresión")]
    public AudioSource audioSource;
    public AudioClip sonidoImpresion;

    [Header("Configuración de impresión")]
    [SerializeField] private float duracionImpresion = 2f;

    [Header("Diálogos")]
    [SerializeField] private string[] dialogueLines =
    {
        "Vaya... Hay una señal registrada en la última observación.",
        "No recuerdo haber visto algo parecido.",
        "Parece que la máquina alcanzó a registrar algo. Será mejor revisar lo que imprimió en la otra mesa."
    };

    private bool playerTouching;
    private bool interactionTriggered;
    private bool inDialogue;

    private void Start()
    {
        Debug.Log("COMPUTADORA: Script iniciado.");

        if (pantallaEncendida != null)
            pantallaEncendida.SetActive(false);

        if (espectrograma != null)
            espectrograma.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);

        playerMovement = playerMovement != null
            ? playerMovement
            : FindFirstObjectByType<PlayerMovement>();

        cameraRotation = cameraRotation != null
            ? cameraRotation
            : FindFirstObjectByType<CameraRotation>();
    }

    private void Update()
    {
        if (inDialogue)
        {
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                continueDialogue = true;
            }

            return;
        }

        if (!playerTouching ||
            interactionTriggered ||
            Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            StartInteraction();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerTouching = true;

        Debug.Log("COMPUTADORA: Jugador cerca.");

        if (!interactionTriggered && interactHint != null)
            interactHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerTouching = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private bool continueDialogue;

    private void StartInteraction()
    {
        Debug.Log("COMPUTADORA: Iniciando interacción.");

        interactionTriggered = true;
        inDialogue = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (pantallaEncendida != null)
            pantallaEncendida.SetActive(true);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartCoroutine(ComputerSequence());
    }

    private IEnumerator ComputerSequence()
    {
        // Primer diálogo.
        yield return ShowDialogue(dialogueLines[0]);

        // Segundo diálogo.
        yield return ShowDialogue(dialogueLines[1]);

        // Comienza la impresión.
        Debug.Log("COMPUTADORA: Imprimiendo registro.");

        if (audioSource != null && sonidoImpresion != null)
        {
            audioSource.Stop();
            audioSource.clip = sonidoImpresion;
            audioSource.Play();
        }

        // Aparece el espectrograma.
        if (espectrograma != null)
            espectrograma.SetActive(true);

        // Sonido limitado a dos segundos.
        yield return new WaitForSeconds(duracionImpresion);

        if (audioSource != null)
            audioSource.Stop();

        // Último diálogo.
        yield return ShowDialogue(dialogueLines[2]);

        FinishInteraction();
    }

    private IEnumerator ShowDialogue(string line)
    {
        if (dialogueText != null)
            dialogueText.text = line;

        if (continueHint != null)
            continueHint.text = "ESPACIO — CONTINUAR";

        continueDialogue = false;

        // Esperar una nueva pulsación.
        while (!continueDialogue)
            yield return null;
    }

    private void FinishInteraction()
    {
        Debug.Log("COMPUTADORA: Interacción terminada.");

        inDialogue = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

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
        if (audioSource != null)
            audioSource.Stop();

        if (inDialogue)
            RestoreControls();
    }
}
