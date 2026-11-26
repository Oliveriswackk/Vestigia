using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class PlayerAmbientMusic : MonoBehaviour
{
    [Header("Pistas ambientales (ej. 4 sonidos completos)")]
    [Tooltip("Arrastra aquí tus pistas de música ambiental. Cada una se reproducirá completa antes de pasar a la siguiente.")]
    [SerializeField] private List<AudioClip> ambientTracks = new List<AudioClip>();

    [Header("Mezcla y ritmo")]
    [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.18f;
    [Tooltip("Pausa breve en segundos entre una pista y la siguiente.")]
    [SerializeField, Min(0f)] private float gapBetweenTracks = 1.5f;
    [Tooltip("Duración del fundido de entrada/salida en segundos (0 para corte limpio).")]
    [SerializeField, Min(0f)] private float fadeDuration = 1.5f;
    [SerializeField] private bool shuffleOrder = false;

    [Header("Control por escena (opcional)")]
    [Tooltip("Si tu Player cambia de escena y quieres silenciar en escenas específicas (ej. cinemáticas), anótalas aquí.")]
    [SerializeField] private List<string> excludedScenes = new List<string>();

    private AudioSource musicSource;
    private Coroutine playlistCoroutine;
    private int currentIndex;
    private bool sceneAllowed = true;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = false; // Importante: false para que avance al siguiente clip al terminar
        musicSource.spatialBlend = 0f; // 2D para que se escuche parejo como música de fondo
        musicSource.volume = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EvaluateScene(SceneManager.GetActiveScene().name);

        if (sceneAllowed && playlistCoroutine == null)
            playlistCoroutine = StartCoroutine(PlayAmbientLoop());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (playlistCoroutine != null)
        {
            StopCoroutine(playlistCoroutine);
            playlistCoroutine = null;
        }

        if (musicSource != null)
            musicSource.Stop();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EvaluateScene(scene.name);
    }

    private void EvaluateScene(string sceneName)
    {
        sceneAllowed = !excludedScenes.Contains(sceneName);

        if (!sceneAllowed && musicSource != null)
        {
            musicSource.volume = 0f;
            musicSource.Pause();
        }
        else if (sceneAllowed && musicSource != null && musicSource.clip != null)
        {
            musicSource.UnPause();
        }
    }

    private IEnumerator PlayAmbientLoop()
    {
        // Espera un frame para que todo en la escena inicialice calmado
        yield return null;

        while (true)
        {
            List<AudioClip> validTracks = GetValidTracks();
            if (validTracks.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (currentIndex >= validTracks.Count)
                currentIndex = 0;

            AudioClip clip = shuffleOrder
                ? validTracks[Random.Range(0, validTracks.Count)]
                : validTracks[currentIndex];

            currentIndex = (currentIndex + 1) % validTracks.Count;

            musicSource.clip = clip;
            musicSource.volume = 0f;

            if (sceneAllowed)
                musicSource.Play();

            float clipDuration = Mathf.Max(0.1f, clip.length);
            float playedTime = 0f;

            // Reproduce el clip completo mientras respeta el volumen ambiental
            while (playedTime < clipDuration)
            {
                float dt = Time.deltaTime;

                if (sceneAllowed)
                {
                    if (!musicSource.isPlaying && playedTime < clipDuration - 0.05f)
                        musicSource.UnPause();

                    float targetVol = ambientVolume;

                    if (fadeDuration > 0f)
                    {
                        float fadeIn = Mathf.Clamp01(playedTime / fadeDuration);
                        float remaining = Mathf.Max(0f, clipDuration - playedTime);
                        float fadeOut = Mathf.Clamp01(remaining / fadeDuration);
                        targetVol *= Mathf.Min(fadeIn, fadeOut);
                    }

                    musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVol, dt * 1.5f);
                    playedTime += dt;
                }
                else
                {
                    musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0f, dt * 2f);
                }

                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = null;

            if (gapBetweenTracks > 0f)
                yield return new WaitForSeconds(gapBetweenTracks);
        }
    }

    private List<AudioClip> GetValidTracks()
    {
        List<AudioClip> list = new List<AudioClip>();
        for (int i = 0; i < ambientTracks.Count; i++)
        {
            if (ambientTracks[i] != null)
                list.Add(ambientTracks[i]);
        }
        return list;
    }
}