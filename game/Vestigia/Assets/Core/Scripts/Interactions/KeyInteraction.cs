
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyInteraction : MonoBehaviour
{
    [Header("Objetos de la llave")]
    [SerializeField] private GameObject visualLlave;
    [SerializeField] private GameObject efectoLlave;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoCollectMagic;

    private bool playerIsNear;
    private bool isCollected;

    public static bool HasKey { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetKey()
    {
        HasKey = false;
    }

    private void Start()
    {
        if (visualLlave != null)
            visualLlave.SetActive(true);

        if (efectoLlave != null)
            efectoLlave.SetActive(true);

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void Update()
    {
        if (!playerIsNear || isCollected)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CollectKey();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isCollected)
            return;

        playerIsNear = true;

        if (interactHint != null)
        {
            interactHint.SetActive(true);

            if (audioSource != null && sonidoClick != null)
                audioSource.PlayOneShot(sonidoClick);
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

    private void CollectKey()
    {
        isCollected = true;
        HasKey = true;

        if (visualLlave != null)
            visualLlave.SetActive(false);

        if (efectoLlave != null)
            efectoLlave.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);

        if (audioSource != null && sonidoCollectMagic != null)
            audioSource.PlayOneShot(sonidoCollectMagic);

        Debug.Log("Llave recogida. Ahora puedes abrir la reja.");
    }
}
