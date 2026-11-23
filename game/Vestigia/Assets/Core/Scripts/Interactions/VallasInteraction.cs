
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class VallasInteraction : MonoBehaviour
{
    [Header("Jugador")]
    public PlayerMovement playerMovement;
    public CameraRotation cameraRotation;

    [Header("Estados de las vallas")]
    public GameObject cerrado;
    public GameObject abierto;

    [Header("DialogueCanvas")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;

    [Header("Sonidos de interacción")]
    public AudioSource audioSource;
    public AudioClip sonidoYaro;
    public AudioClip sonidoApertura;

    [Header("Transición")]
    [SerializeField] private string nextScene = "Escena4_Observatorio";

    [Header("Diálogos")]
    [SerializeField] private string lockedLine =
        "No... No creo que deba ir por ahí todavía.";

    [SerializeField] private string unlockedLine =
        "La llave... Debo tenerla en mi mochila.";

    private bool playerTouching;
    private bool inDialogue;
    private bool opening;
    private bool isOpen;
    private bool interactionTriggered;

    private void Start()
    {
        Debug.Log("VALLAS: Script iniciado.");

        if (cerrado != null)
            cerrado.SetActive(true);

        if (abierto != null)
            abierto.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogError("VALLAS: Falta asignar DialoguePanel.");

        playerMovement = playerMovement != null
            ? playerMovement
            : FindFirstObjectByType<PlayerMovement>();

        cameraRotation = cameraRotation != null
            ? cameraRotation
            : FindFirstObjectByType<CameraRotation>();
    }

    private void Update()
    {
        if (!inDialogue || opening || Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            FinishDialogue();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("VALLAS: Jugador entró en la zona.");

        playerTouching = true;

        if (interactionTriggered || isOpen || opening)
            return;

        interactionTriggered = true;

        StartDialogue();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerTouching = false;

        Debug.Log("VALLAS: Jugador salió de la zona.");
    }

    private void StartDialogue()
    {
        Debug.Log("VALLAS: Iniciando interacción automática.");

        bool observatoryFound =
            GameProgress.Instance != null &&
            GameProgress.Instance.obs_encontrado;

        inDialogue = true;

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);
        else
            Debug.LogError("VALLAS: PlayerMovement no asignado.");

        if (cameraRotation != null)
            cameraRotation.enabled = false;
        else
            Debug.LogError("VALLAS: CameraRotation no asignado.");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
        {
            dialogueText.text = observatoryFound
                ? unlockedLine
                : lockedLine;
        }

        if (continueHint != null)
            continueHint.text = "ESPACIO — CONTINUAR";

        if (!observatoryFound &&
            audioSource != null &&
            sonidoYaro != null)
        {
            audioSource.PlayOneShot(sonidoYaro);
        }
    }

    private void FinishDialogue()
    {
        Debug.Log("VALLAS: Diálogo terminado.");

        inDialogue = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        RestoreControls();

        bool observatoryFound =
            GameProgress.Instance != null &&
            GameProgress.Instance.obs_encontrado;

        if (observatoryFound)
        {
            StartCoroutine(OpenVallas());
        }
        else
        {
            // Permitir que vuelva a activarse si sale y entra otra vez.
            interactionTriggered = false;
        }
    }

    private IEnumerator OpenVallas()
    {
        opening = true;

        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError(
                "VALLAS: No se puede cargar la escena: " + nextScene
            );

            opening = false;
            interactionTriggered = false;
            yield break;
        }

        Debug.Log("VALLAS: Abriendo acceso al observatorio.");

        // Sonido y cambio visual simultáneos.
        if (audioSource != null && sonidoApertura != null)
            audioSource.PlayOneShot(sonidoApertura);

        if (cerrado != null)
            cerrado.SetActive(false);

        if (abierto != null)
            abierto.SetActive(true);

        isOpen = true;

        if (audioSource != null && sonidoApertura != null)
        {
            float pitch = Mathf.Max(
                Mathf.Abs(audioSource.pitch), 0.01f
            );

            yield return new WaitForSeconds(
                sonidoApertura.length / pitch
            );
        }

        if (SceneTransition.Instance == null)
        {
            Debug.LogError(
                "VALLAS: No se encontró SceneTransition."
            );

            yield break;
        }

        SceneTransition.Instance.ChangeScene(nextScene);
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
