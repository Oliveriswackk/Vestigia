
using UnityEngine;
using UnityEngine.InputSystem;

public class BookInteraction : MonoBehaviour
{
    [Header("Estados del libro")]
    [SerializeField] private GameObject bookClose;
    [SerializeField] private GameObject bookOpen;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    private bool playerIsNear;
    private bool isOpen;

    private void Start()
    {
        if (bookClose != null)
            bookClose.SetActive(true);

        if (bookOpen != null)
            bookOpen.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void Update()
    {
        if (!playerIsNear || isOpen)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OpenBook();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerIsNear = true;

        if (!isOpen && interactHint != null)
            interactHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerIsNear = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void OpenBook()
    {
        isOpen = true;

        if (bookClose != null)
            bookClose.SetActive(false);

        if (bookOpen != null)
            bookOpen.SetActive(true);

        if (interactHint != null)
            interactHint.SetActive(false);
    }
}
