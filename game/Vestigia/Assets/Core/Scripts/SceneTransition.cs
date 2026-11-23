
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

    private string pendingSpawnPoint;
    private bool keepPlayerLocked;

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
            Debug.LogError("SceneTransition: No se encontró CanvasGroup.");
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

    // Transición normal.
    public void ChangeScene(string sceneName)
    {
        StartTransition(sceneName, footstepsClip, footstepsDuration, null, false);
    }

    // Transición con punto de aparición.
    public void ChangeScene(string sceneName, string spawnPoint)
    {
        StartTransition(sceneName, footstepsClip, footstepsDuration, spawnPoint, false);
    }

    // Transición con punto de aparición y jugador bloqueado.
    public void ChangeScene(string sceneName, string spawnPoint, bool lockPlayer)
    {
        StartTransition(sceneName, footstepsClip, footstepsDuration, spawnPoint, lockPlayer);
    }

    // Transición con sonido personalizado.
    public void ChangeScene(string sceneName, AudioClip transitionClip)
    {
        float duration = transitionClip != null ? transitionClip.length : 0f;
        StartTransition(sceneName, transitionClip, duration, null, false);
    }

    private void StartTransition(
        string sceneName,
        AudioClip transitionClip,
        float audioDuration,
        string spawnPoint,
        bool lockPlayer)
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

        pendingSpawnPoint = spawnPoint;
        keepPlayerLocked = lockPlayer;

        StartCoroutine(ChangeSceneRoutine(sceneName, transitionClip, audioDuration));
    }

    private IEnumerator ChangeSceneRoutine(
        string sceneName,
        AudioClip transitionClip,
        float audioDuration)
    {
        transitioning = true;
        overlay.blocksRaycasts = true;

        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        yield return Fade(0f, 1f, fadeToBlack);

        overlay.alpha = 1f;
        yield return new WaitForEndOfFrame();

        if (footstepsSource != null && transitionClip != null)
        {
            footstepsSource.clip = transitionClip;
            footstepsSource.Play();
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError("No se pudo cargar: " + sceneName);
            StopFootsteps();
            yield return Fade(1f, 0f, fadeFromBlack);
            FinishTransition(false);
            yield break;
        }

        float elapsed = 0f;

        while (!operation.isDone || elapsed < audioDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        StopFootsteps();

        playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("SceneTransition: No se encontró PlayerMovement.");
        }
        else
        {
            playerMovement.SetMovementEnabled(false);

            if (!string.IsNullOrEmpty(pendingSpawnPoint))
            {
                GameObject spawn = GameObject.Find(pendingSpawnPoint);

                if (spawn == null)
                {
                    Debug.LogError(
                        "SceneTransition: No se encontró el punto de aparición: " +
                        pendingSpawnPoint
                    );
                }
                else
                {
                    playerMovement.transform.SetPositionAndRotation(
                        spawn.transform.position,
                        spawn.transform.rotation
                    );
                }
            }
        }

        yield return new WaitForEndOfFrame();

        yield return Fade(1f, 0f, fadeFromBlack);

        FinishTransition(!keepPlayerLocked);
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

    private void FinishTransition(bool unlockPlayer)
    {
        if (unlockPlayer && playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;

        transitioning = false;
    }
}
