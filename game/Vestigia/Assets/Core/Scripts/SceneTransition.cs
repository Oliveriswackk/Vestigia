
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private AudioSource footstepsSource;
    [SerializeField] private AudioClip footstepsClip;

    [Header("Duraciones")]
    [SerializeField] private float fadeToBlack = 0.5f;
    [SerializeField] private float footstepsDuration = 1.2f;
    [SerializeField] private float fadeFromBlack = 0.7f;

    private bool transitioning;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (overlay == null)
            overlay = GetComponentInChildren<CanvasGroup>(true);

        if (overlay == null)
        {
            Debug.LogError("SceneTransition: No se encontró el CanvasGroup.");
            enabled = false;
            return;
        }

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
        overlay.interactable = false;

        if (footstepsSource != null)
        {
            footstepsSource.playOnAwake = false;
            footstepsSource.loop = false;
            footstepsSource.spatialBlend = 0f;
        }
    }

    // Transición normal: utiliza las pisadas originales.
    public void ChangeScene(string sceneName)
    {
        StartTransition(
            sceneName,
            footstepsClip,
            footstepsDuration
        );
    }

    // Transición personalizada: utiliza el sonido indicado.
    public void ChangeScene(string sceneName, AudioClip transitionClip)
    {
        float duration = transitionClip != null
            ? transitionClip.length
            : 0f;

        StartTransition(
            sceneName,
            transitionClip,
            duration
        );
    }

    private void StartTransition(
        string sceneName,
        AudioClip transitionClip,
        float audioDuration)
    {
        if (transitioning)
            return;

        if (overlay == null)
        {
            Debug.LogError("SceneTransition: No hay overlay asignado.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("No se puede cargar la escena: " + sceneName);
            return;
        }

        StartCoroutine(
            ChangeSceneRoutine(sceneName, transitionClip, audioDuration)
        );
    }

    private IEnumerator ChangeSceneRoutine(
        string sceneName,
        AudioClip transitionClip,
        float audioDuration)
    {
        transitioning = true;
        overlay.blocksRaycasts = true;

        // 1. Bloquear al jugador.
        playerMovement = FindObjectOfType<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        // 2. Fundido hasta negro.
        yield return Fade(0f, 1f, fadeToBlack);

        // 3. Garantizar que el negro se haya renderizado.
        overlay.alpha = 1f;
        yield return new WaitForEndOfFrame();

        // 4. Reproducir el sonido seleccionado.
        if (footstepsSource != null && transitionClip != null)
        {
            footstepsSource.clip = transitionClip;
            footstepsSource.Play();
        }
        else if (transitionClip != null)
        {
            Debug.LogWarning("SceneTransition: No hay AudioSource asignado.");
        }

        // 5. Iniciar la carga de la escena.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("No se pudo iniciar la carga de: " + sceneName);

            StopFootsteps();
            yield return Fade(1f, 0f, fadeFromBlack);
            FinishTransition();
            yield break;
        }

        // 6. Mantener pantalla negra hasta terminar
        // tanto la carga como el audio.
        float elapsed = 0f;

        while (!operation.isDone || elapsed < audioDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 7. Detener el sonido.
        StopFootsteps();

        // 8. Buscar al jugador de la nueva escena.
        playerMovement = FindObjectOfType<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError(
                "SceneTransition: No se encontró PlayerMovement en la nueva escena."
            );
        }
        else
        {
            playerMovement.SetMovementEnabled(false);
        }

        // 9. Esperar un frame manteniendo el negro.
        yield return new WaitForEndOfFrame();

        // 10. Revelar la nueva escena.
        yield return Fade(1f, 0f, fadeFromBlack);

        // 11. Devolver el control.
        FinishTransition();
    }

    private void StopFootsteps()
    {
        if (footstepsSource == null)
            return;

        footstepsSource.Stop();
        footstepsSource.clip = null;
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        overlay.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            overlay.alpha = Mathf.Lerp(
                start,
                end,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        overlay.alpha = end;
    }

    private void FinishTransition()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;

        transitioning = false;
    }
}
