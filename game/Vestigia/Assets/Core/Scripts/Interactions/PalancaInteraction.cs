
using UnityEngine;
using UnityEngine.InputSystem;

public class PalancaInteraction : MonoBehaviour
{
    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;
    [SerializeField] private GameObject objetoVisual;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoCollectMagic;

    private bool playerIsNear;
    private bool collected;

    // La escalera puede consultar si ESTA palanca fue recogida.
    public bool IsCollected => collected;

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
            CollectLever();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null || collected)
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

    private void CollectLever()
    {
        if (collected)
            return;

        collected = true;
        playerIsNear = false;

        Debug.Log("Palanca recogida.");

        if (interactHint != null)
            interactHint.SetActive(false);

        // Ocultar visual inmediatamente.
        if (objetoVisual != null)
        {
            objetoVisual.SetActive(false);
        }
        else
        {
            Transform visual = transform.Find("default");

            if (visual == null)
                visual = transform.Find("Default");

            if (visual != null)
                visual.gameObject.SetActive(false);
        }

        if (audioSource != null && sonidoCollectMagic != null)
            audioSource.PlayOneShot(sonidoCollectMagic);
    }
}
