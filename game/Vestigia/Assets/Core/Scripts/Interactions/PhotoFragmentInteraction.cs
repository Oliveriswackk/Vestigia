
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PhotoFragmentInteraction : MonoBehaviour
{
    // En falso: las piezas solo se registran al encontrarlas (la libreta
    // del teléfono las pega una a una).
    private const bool DEVELOP = false;
    private static bool developProgressPrepared;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetDevelopFlag()
    {
        developProgressPrepared = false;
    }

    [Header("Identificación")]
    [SerializeField, Range(1, 3)]
    private int planta = 1;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    [Header("Diálogo de Janeth")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueHint;

    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines;

    [Header("Diálogo del mirador")]
    [TextArea(2, 4)]
    [SerializeField] private string[] miradorDialogueLines;

    [SerializeField] private MiradorCameraTransition miradorCameraTransition;

    private bool showingMiradorDialogue;

    [Header("Pantalla de espera")]
    [SerializeField] private GameObject waitingCanvas;

    [Header("Controles del jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraRotation cameraRotation;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoCollectMagic;

    private bool playerIsNear;
    private bool collected;
    private bool sequenceRunning;
    private bool puzzleCompleted;

    private bool cameraWasEnabled;
    private int currentDialogueLine;

    public bool IsCollected => collected;

    private void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.SetActive(false);

        if (waitingCanvas != null)
            waitingCanvas.SetActive(false);

        PrepareDevelopmentProgress();
    }

    private void PrepareDevelopmentProgress()
    {
        if (!DEVELOP || developProgressPrepared || planta != 3)
            return;

        if (GameProgress.Instance == null ||
            StoryManager.Instance == null)
        {
            Debug.LogError("DEVELOP: Faltan sistemas de progreso.");
            return;
        }

        developProgressPrepared = true;

        if (GameProgress.Instance.photo_quantity > 0)
        {
            Debug.Log("DEVELOP: Ya existe progreso. No se modificará.");
            return;
        }

        StoryManager.Instance.AddPhotoFragment(1);
        StoryManager.Instance.AddPhotoFragment(2);

        Debug.Log("DEVELOP: Fragmentos 1 y 2 registrados.");
    }

    private void Update()
    {
        if (sequenceRunning)
        {
            HandleDialogueInput();
            return;
        }

        if (!playerIsNear || collected)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CollectFragment();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null || collected || sequenceRunning)
            return;

        playerIsNear = true;

        if (interactHint != null)
            interactHint.SetActive(true);

        if (audioSource != null && sonidoClick != null)
            audioSource.PlayOneShot(sonidoClick);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        playerIsNear = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void CollectFragment()
    {
        if (collected)
            return;

        if (StoryManager.Instance == null ||
            GameProgress.Instance == null)
        {
            Debug.LogError("No se encontraron los sistemas de progreso.");
            return;
        }

        int quantityBefore =
            GameProgress.Instance.photo_quantity;

        collected = true;
        playerIsNear = false;

        if (interactHint != null)
            interactHint.SetActive(false);

        StoryManager.Instance.AddPhotoFragment(planta);

        int quantityAfter =
            GameProgress.Instance.photo_quantity;

        bool isLastFragment =
            quantityBefore < 3 && quantityAfter == 3;

        Debug.Log("Fragmento " + planta + " recogido.");

        HideFragment();

        // El sonido se reproduce sin retrasar la secuencia.
        if (audioSource != null && sonidoCollectMagic != null)
            audioSource.PlayOneShot(sonidoCollectMagic);

        // El diálogo comienza inmediatamente.
        if (isLastFragment)
            StartPhotoSequence();
    }

    private void HideFragment()
    {
        SpriteRenderer[] sprites =
            GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sprite in sprites)
            sprite.enabled = false;

        Collider[] colliders =
            GetComponents<Collider>();

        foreach (Collider fragmentCollider in colliders)
            fragmentCollider.enabled = false;
    }

    private void StartPhotoSequence()
    {
        if (sequenceRunning)
            return;

        sequenceRunning = true;

        LockControls();

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No hay líneas de diálogo configuradas.");
            StartCoroutine(ShowWaitingScreen());
            return;
        }

        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("Faltan referencias del diálogo.");
            StartCoroutine(ShowWaitingScreen());
            return;
        }

        currentDialogueLine = 0;

        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[currentDialogueLine];

        if (continueHint != null)
        {
            continueHint.SetActive(true);
            continueHint.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("No se asignó Continue Hint.");
        }

        Debug.Log("Historia: comienza el diálogo de Janeth.");
    }

    private void HandleDialogueInput()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        if (Keyboard.current == null ||
            !Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        currentDialogueLine++;

        string[] activeLines = showingMiradorDialogue
            ? miradorDialogueLines
            : dialogueLines;

        if (currentDialogueLine >= activeLines.Length)
        {
            if (showingMiradorDialogue)
            {
                StartCoroutine(TransitionToMirador());
            }
            else
            {
                StartCoroutine(ShowWaitingScreen());
            }

            return;
        }

        dialogueText.text = activeLines[currentDialogueLine];
    }

    private void StartMiradorDialogue()
    {
        showingMiradorDialogue = true;
        currentDialogueLine = 0;

        if (miradorDialogueLines == null ||
            miradorDialogueLines.Length == 0)
        {
            Debug.LogError("No hay diálogo del mirador configurado.");
            StartCoroutine(TransitionToMirador());
            return;
        }

        dialoguePanel.SetActive(true);
        dialogueText.text = miradorDialogueLines[0];

        if (continueHint != null)
        {
            continueHint.SetActive(true);
            continueHint.transform.SetAsLastSibling();
        }

        Debug.Log("Mirador: comienza el diálogo.");
    }

    private IEnumerator TransitionToMirador()
    {
        dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.SetActive(false);

        if (miradorCameraTransition == null)
        {
            Debug.LogError("No se asignó MiradorCameraTransition.");
            yield break;
        }

        Debug.Log("Mirador: comienza la transición de cámara.");

        yield return StartCoroutine(
            miradorCameraTransition.MoveToObservatory()
        );

        Debug.Log("Mirador: transición terminada. Listo para iniciar la observación.");
    }

    private IEnumerator ShowWaitingScreen()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.SetActive(false);

        if (waitingCanvas == null)
        {
            Debug.LogError(
                "No se asignó Canvas_PuzzleEspera en el Inspector."
            );

            RestoreControls();
            sequenceRunning = false;
            yield break;
        }

        waitingCanvas.SetActive(true);

        Canvas.ForceUpdateCanvases();

        Debug.Log(
            "Canvas de espera activado: " + waitingCanvas.name
        );

        if (MobileBridge.Instance == null)
        {
            Debug.LogError("No se encontró MobileBridge.");

            RestoreControls();
            sequenceRunning = false;
            yield break;
        }

        if (MobileBridge.Instance.IsPuzzleRunning)
        {
            Debug.LogWarning("Ya hay otro puzzle en ejecución.");

            waitingCanvas.SetActive(false);
            RestoreControls();
            sequenceRunning = false;
            yield break;
        }

        puzzleCompleted = false;

        MobileBridge.Instance.StartPuzzle(
            "photo",
            OnPhotoPuzzleCompleted
        );

        while (!puzzleCompleted)
            yield return null;

        waitingCanvas.SetActive(false);

        Debug.Log("Fotografía: puzzle completado.");

        StartMiradorDialogue();
    }

    private void OnPhotoPuzzleCompleted()
    {
        puzzleCompleted = true;
    }

    private void LockControls()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (cameraRotation != null)
        {
            cameraWasEnabled = cameraRotation.enabled;
            cameraRotation.enabled = false;
        }
    }

    private void RestoreControls()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.enabled = cameraWasEnabled;
    }

    private void OnDisable()
    {
        RestoreControls();
    }
}
