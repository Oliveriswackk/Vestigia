
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PhotoFragmentInteraction : MonoBehaviour
{
    [Header("Identificación")]
    [SerializeField, Range(1, 3)]
    private int planta = 1;

    [Header("Interacción")]
    [SerializeField] private GameObject interactHint;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoCollectMagic;

    private bool playerIsNear;
    private bool collected;

    // La escalera puede consultar si ESTE fragmento fue recogido.
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
            CollectFragment();
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

    private void CollectFragment()
    {
        if (collected)
            return;

        collected = true;
        playerIsNear = false;

        Debug.Log("Fragmento " + planta + " recogido.");

        if (interactHint != null)
            interactHint.SetActive(false);

        // Ocultar el fragmento inmediatamente.
        SpriteRenderer[] sprites =
            GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sprite in sprites)
            sprite.enabled = false;

        // Desactivar sus colliders.
        Collider[] colliders =
            GetComponents<Collider>();

        foreach (Collider fragmentCollider in colliders)
            fragmentCollider.enabled = false;

        // Reproducir el sonido de recolección.
        if (audioSource != null && sonidoCollectMagic != null)
            audioSource.PlayOneShot(sonidoCollectMagic);

        StartCoroutine(DesactivarDespuesDelSonido());
    }

    private IEnumerator DesactivarDespuesDelSonido()
    {
        if (sonidoCollectMagic != null)
        {
            yield return new WaitForSecondsRealtime(
                sonidoCollectMagic.length
            );
        }

        gameObject.SetActive(false);
    }
}
