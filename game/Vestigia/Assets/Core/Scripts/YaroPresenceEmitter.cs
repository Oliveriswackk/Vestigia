
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class YaroPresenceEmitter : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraRotation cameraRotation;

    [Header("Interfaz de interacción")]
    [SerializeField] private GameObject interactHint;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueHint;

    [Header("Sonido del indicador")]
    [SerializeField] private AudioSource hintAudioSource;
    [SerializeField] private AudioClip hintAppearClip;

    [Header("Presencia ambiental de Yaro")]
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float interval = 6f;

    [Header("Posición de sonidos ambientales")]
    [SerializeField] private float minRadius = 8f;
    [SerializeField] private float maxRadius = 25f;
    [SerializeField] private float heightVariation = 0f;

    [Header("Volumen ambiental")]
    [SerializeField] private float minVolume = 0.35f;
    [SerializeField] private float maxVolume = 0.85f;

    [Header("Distancia de audición")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 35f;

    [Header("Interacción con Yaro")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField] private float interactionVolume = 1f;

    [Header("Diálogo de Janeth")]
    [SerializeField] private string dialogueLine =
        "JANETH: No... por ahí no. Hay algo entre esos árboles. Será mejor que encuentre otra forma de entrar.";

    private bool playerInRange;
    private bool dialogueActive;
    private bool hasInteracted;

    private void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.gameObject.SetActive(false);

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (cameraRotation == null)
            cameraRotation = FindFirstObjectByType<CameraRotation>();

        StartCoroutine(PlayAmbientSounds());
    }

    private void Update()
    {
        if (dialogueActive)
        {
            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                FinishDialogue();
            }

            return;
        }

        if (hasInteracted || !playerInRange)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartDialogue();
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

    private void StartDialogue()
    {
        dialogueActive = true;

        SetInteractHint(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(false);

        if (interactionSound != null)
        {
            PlaySpatialSound(
                interactionSound,
                transform.position,
                interactionVolume
            );
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = dialogueLine;

        if (continueHint != null)
        {
            continueHint.text = "ESPACIO — CONTINUAR";
            continueHint.gameObject.SetActive(true);
        }
    }

    private void FinishDialogue()
    {
        dialogueActive = false;
        hasInteracted = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.gameObject.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(true);

        SetInteractHint(false);
    }

    private IEnumerator PlayAmbientSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (ambientSounds == null ||
                ambientSounds.Length == 0)
                continue;

            AudioClip randomSound =
                ambientSounds[
                    Random.Range(0, ambientSounds.Length)
                ];

            Vector2 direction =
                Random.insideUnitCircle.normalized;

            float radius =
                Random.Range(minRadius, maxRadius);

            Vector3 randomPosition = transform.position +
                new Vector3(
                    direction.x * radius,
                    Random.Range(-heightVariation, heightVariation),
                    direction.y * radius
                );

            float randomVolume =
                Random.Range(minVolume, maxVolume);

            PlaySpatialSound(
                randomSound,
                randomPosition,
                randomVolume
            );
        }
    }

    private void PlaySpatialSound(
        AudioClip clip,
        Vector3 position,
        float volume)
    {
        if (clip == null)
            return;

        GameObject soundObject =
            new GameObject("Yaro_SpatialSound");

        soundObject.transform.position = position;

        AudioSource source =
            soundObject.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.dopplerLevel = 0f;
        source.playOnAwake = false;

        source.Play();

        Destroy(soundObject, clip.length + 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement detectedPlayer =
            other.GetComponentInParent<PlayerMovement>();

        if (detectedPlayer == null || hasInteracted)
            return;

        playerMovement = detectedPlayer;
        playerInRange = true;

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

        if (!dialogueActive)
            SetInteractHint(false);
    }

    private void OnDisable()
    {
        if (dialogueActive)
        {
            if (playerMovement != null)
                playerMovement.SetMovementEnabled(true);

            if (cameraRotation != null)
                cameraRotation.SetInputEnabled(true);
        }
    }
}
