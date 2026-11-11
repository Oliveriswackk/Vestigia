using UnityEngine;
using UnityEngine.InputSystem;

public class KeyInteraction : MonoBehaviour
{
    [Header("Objetos de la llave")]
    [SerializeField] private GameObject visualLlave;
    [SerializeField] private GameObject efectoLlave;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    private bool playerIsNear;
    private bool isCollected;

    public static bool HasKey { get; private set; }

    // Reinicia el estado al iniciar una nueva sesión del juego.
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

        Debug.Log("Llave recogida. Ahora puedes abrir la reja.");
    }
}