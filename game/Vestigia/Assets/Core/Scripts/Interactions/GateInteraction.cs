using UnityEngine;
using UnityEngine.InputSystem;

public class GateInteraction : MonoBehaviour
{
    [Header("Estados de la reja")]
    [SerializeField] private GameObject rejaCerrada;
    [SerializeField] private GameObject rejaAbierta;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    private bool playerIsNear;
    private bool isOpen;

    private void Start()
    {
        if (rejaCerrada != null)
            rejaCerrada.SetActive(true);

        if (rejaAbierta != null)
            rejaAbierta.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void Update()
    {
        bool canInteract = playerIsNear &&
                        KeyInteraction.HasKey &&
                        !isOpen;

        if (!canInteract)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OpenGate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerIsNear = true;

        if (KeyInteraction.HasKey && !isOpen && interactHint != null)
        {
            interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerIsNear = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void OpenGate()
    {
        isOpen = true;

        if (rejaCerrada != null)
            rejaCerrada.SetActive(false);

        if (rejaAbierta != null)
            rejaAbierta.SetActive(true);

        if (interactHint != null)
            interactHint.SetActive(false);

        Debug.Log("Reja abierta.");
    }
}