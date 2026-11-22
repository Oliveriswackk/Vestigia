
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ObservatorioInteraction : MonoBehaviour
{
    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    [Header("Configuración de puerta")]
    [SerializeField] private bool isCorrectDoor = false;

    [Header("Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueHint;

    [Header("Transición")]
    [SerializeField] private string nextScene = "Escena4.1_InteriorObservatorio";

    private bool playerIsNear;
    private bool transitioning;
    private bool dialogueActive;

    private PlayerMovement playerMovement;
    private CameraRotation cameraRotation;

    private void Start()
    {
        Debug.Log($"{gameObject.name}: Script iniciado. Puerta correcta: {isCorrectDoor}");

        if (interactHint != null)
            interactHint.SetActive(false);
        else
            Debug.LogError($"{gameObject.name}: InteractHint no está asignado.");
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerIsNear || transitioning || dialogueActive)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{gameObject.name}: Trigger detectó {other.name}");

        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            Debug.Log($"{gameObject.name}: El objeto detectado no tiene PlayerMovement.");
            return;
        }

        Debug.Log($"{gameObject.name}: ¡Jugador reconocido!");

        playerMovement = movement;
        cameraRotation = other.GetComponentInParent<CameraRotation>();

        playerIsNear = true;

        if (interactHint != null)
            interactHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null)
            return;

        Debug.Log($"{gameObject.name}: Jugador salió del trigger.");

        playerIsNear = false;

        if (interactHint != null && !dialogueActive)
            interactHint.SetActive(false);
    }

    private void TryInteract()
    {
        Debug.Log($"{gameObject.name}: Interacción. Puerta correcta: {isCorrectDoor}");

        if (!isCorrectDoor)
        {
            StartCoroutine(ShowIncorrectDoorMessage());
            return;
        }

        if (SceneTransition.Instance == null)
        {
            Debug.LogError("ObservatorioInteraction: No se encontró SceneTransition.");
            return;
        }

        transitioning = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        SceneTransition.Instance.ChangeScene(nextScene);
    }

    private IEnumerator ShowIncorrectDoorMessage()
    {
        dialogueActive = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = "La llave no es para esta puerta.";

        if (continueHint != null)
            continueHint.gameObject.SetActive(true);

        yield return null;

        while (Keyboard.current == null ||
               !Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.gameObject.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (cameraRotation != null)
            cameraRotation.SetInputEnabled(true);

        dialogueActive = false;

        if (playerIsNear && interactHint != null)
            interactHint.SetActive(true);
    }
}
