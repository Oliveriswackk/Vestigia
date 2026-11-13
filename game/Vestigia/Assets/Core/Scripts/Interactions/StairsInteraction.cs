
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StairsInteraction : MonoBehaviour
{
    [Header("Objetos necesarios de ESTE piso")]
    [SerializeField] private PhotoFragmentInteraction fragmentoNecesario;
    [SerializeField] private PalancaInteraction palancaNecesaria;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    [Header("Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject continueHint;

    [Header("Destino")]
    [SerializeField] private string nextScene = "Escena3.3_TercerPiso";

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoSubir;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoClick;

    private bool playerIsNear;
    private bool showingDialogue;
    private bool transitioning;

    private PlayerMovement nearbyPlayer;

    private List<string> dialogueMessages = new List<string>();
    private int currentDialogueIndex;

    private HashSet<Collider> playerCollidersInside =
        new HashSet<Collider>();

    private void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.SetActive(false);
    }

    private void Update()
    {
        if (transitioning)
            return;

        if (Keyboard.current == null ||
            !Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        if (showingDialogue)
        {
            NextDialogue();
            return;
        }

        if (playerIsNear)
            TryClimb();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null)
            return;

        if (!playerCollidersInside.Add(other))
            return;

        nearbyPlayer = player;

        if (playerIsNear)
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

        playerCollidersInside.Remove(other);

        if (playerCollidersInside.Count > 0)
            return;

        playerIsNear = false;
        nearbyPlayer = null;

        if (interactHint != null)
            interactHint.SetActive(false);

        if (showingDialogue)
            CloseDialogue();
    }

    private void TryClimb()
    {
        if (fragmentoNecesario == null || palancaNecesaria == null)
        {
            Debug.LogError(
                "Escaleras: Asigna el fragmento y la palanca necesarios."
            );
            return;
        }

        bool tieneFragmento = fragmentoNecesario.IsCollected;
        bool tienePalanca = palancaNecesaria.IsCollected;

        dialogueMessages.Clear();

        if (!tieneFragmento)
        {
            dialogueMessages.Add(
                "Necesito encontrar el fragmento de esta planta..."
            );
        }

        if (!tienePalanca)
        {
            dialogueMessages.Add(
                "La compuerta está cerrada; necesito encontrar la palanca."
            );
        }

        if (dialogueMessages.Count > 0)
        {
            currentDialogueIndex = 0;
            ShowDialogue();
            return;
        }

        if (SceneTransition.Instance == null)
        {
            Debug.LogError("No se encontró SceneTransition.");
            return;
        }

        transitioning = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        SceneTransition.Instance.ChangeScene(
            nextScene,
            sonidoSubir
        );
    }

    private void ShowDialogue()
    {
        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError(
                "Asigna DialoguePanel y DialogueText en el Inspector."
            );
            return;
        }

        showingDialogue = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        dialogueText.text = dialogueMessages[currentDialogueIndex];

        dialoguePanel.SetActive(true);

        if (continueHint != null)
            continueHint.SetActive(true);

        if (nearbyPlayer != null)
            nearbyPlayer.SetMovementEnabled(false);
    }

    private void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogueMessages.Count)
        {
            dialogueText.text = dialogueMessages[currentDialogueIndex];
        }
        else
        {
            CloseDialogue();
        }
    }

    private void CloseDialogue()
    {
        showingDialogue = false;

        dialogueMessages.Clear();
        currentDialogueIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueHint != null)
            continueHint.SetActive(false);

        if (nearbyPlayer != null)
            nearbyPlayer.SetMovementEnabled(true);

        if (playerIsNear && interactHint != null)
            interactHint.SetActive(true);
    }
}
