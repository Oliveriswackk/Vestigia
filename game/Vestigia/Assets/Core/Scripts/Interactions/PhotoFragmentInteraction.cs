
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoFragmentInteraction : MonoBehaviour
{
    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    private bool playerIsNear;
    private bool collected;

    private void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void Update()
    {
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
        if (!other.CompareTag("Player"))
            return;

        playerIsNear = true;

        if (!collected && interactHint != null)
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

    private void CollectFragment()
    {
        collected = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        if (GameProgress.Instance != null)
            GameProgress.Instance.photo_quantity++;

        gameObject.SetActive(false);
    }
}
