
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FishermanInteraction : MonoBehaviour
{
    [Header("Jugador")]
    public Transform player;
    public float interactionDistance = 2.5f;
    public PlayerMovement playerMovement;
    public CameraRotation cameraRotation;

    [Header("Figura del pescador")]
    public Transform visual;
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite talkingSprite;
    public float idleAngle = 15f;

    [Header("Interfaz")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text continueHint;
    public GameObject processPanel;
    public GameObject interactHint;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip voiceClip;
    public AudioClip staticClip;

    [Header("Sonido del indicador")]
    public AudioSource hintAudioSource;
    public AudioClip hintAppearClip;

    [Header("Duracion")]
    public float processDuration = 3f;

    private string[] lines =
    {
        "PESCADOR: Que bonito dia para pescar, si no fuera por la radio",
        "CIENTÍFICA: ¿Qué le ocurre?",
        "PESCADOR: No sé, de repente hace interferencia y no me deja escuchar a Frank Sinatra",
        "CIENTÍFICA: ¿Me la permite?",
        "PESCADOR: Claro, pero ya te digo yo que parece estar rota"
    };

    private int currentLine;
    private bool inDialogue;
    private bool inProcess;
    private bool hasTalked;
    private bool controlsLocked;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (processPanel != null)
            processPanel.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);

        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;
    }

    void Update()
    {
        if (player == null || visual == null)
            return;

        if (inProcess)
            return;

        if (inDialogue)
        {
            FacePlayer(0f);

            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                NextLine();
            }

            return;
        }

        FacePlayer(idleAngle);

        if (hasTalked)
        {
            SetInteractHint(false);
            return;
        }

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        bool playerIsNear = distance <= interactionDistance;

        SetInteractHint(playerIsNear);

        if (playerIsNear &&
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

        if (visible && !wasVisible &&
            hintAudioSource != null &&
            hintAppearClip != null)
        {
            hintAudioSource.PlayOneShot(hintAppearClip);
        }
    }

    void FacePlayer(float angleOffset)
    {
        Vector3 direction = player.position - visual.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        visual.rotation =
            targetRotation * Quaternion.Euler(0f, angleOffset, 0f);
    }

    void StartDialogue()
    {
        inDialogue = true;
        currentLine = 0;

        SetInteractHint(false);

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
            StartCoroutine(ShowProcess());
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

        if (audioSource != null && voiceClip != null)
            audioSource.PlayOneShot(voiceClip);
    }

    IEnumerator ShowProcess()
    {
        inDialogue = false;
        inProcess = true;
        hasTalked = true;

        SetInteractHint(false);

        controlsLocked = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.sprite = idleSprite;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (processPanel != null)
            processPanel.SetActive(true);

        // Reproducir la estática en bucle.
        if (audioSource != null && staticClip != null)
        {
            audioSource.Stop();
            audioSource.clip = staticClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Esperar la respuesta simulada de Móvil.
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.StartRadioPuzzle();
        }
        else
        {
            Debug.LogError(
                "FishermanInteraction: no se encontró StoryManager."
            );

            // Respaldo para evitar dejar al jugador bloqueado
            // si falta el sistema de historia.
            yield return new WaitForSeconds(processDuration);
        }

        // Esperar a que el puzzle termine.
        while (MobileBridge.Instance != null &&
               MobileBridge.Instance.IsPuzzleRunning)
        {
            yield return null;
        }

        // Si el puzzle no pudo iniciarse, evitar quedarse bloqueado.
        if (GameProgress.Instance == null ||
            !GameProgress.Instance.radio_sintonizada)
        {
            Debug.LogWarning(
                "La radio no se completó. Se cerrará la secuencia."
            );
        }

        // Detener la estática.
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }

        if (processPanel != null)
            processPanel.SetActive(false);

        // Restaurar controles.
        if (controlsLocked)
        {
            if (playerMovement != null)
                playerMovement.enabled = true;

            if (cameraRotation != null)
                cameraRotation.enabled = true;

            controlsLocked = false;
        }

        inProcess = false;
    }

    void OnDisable()
    {
        if (controlsLocked)
        {
            if (playerMovement != null)
                playerMovement.enabled = true;

            if (cameraRotation != null)
                cameraRotation.enabled = true;

            controlsLocked = false;
        }
    }
}
