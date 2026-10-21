
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private float transitionDuration = 0.7f;

    private bool transitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (overlay == null)
            overlay = GetComponentInChildren<CanvasGroup>();

        if (overlay != null)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
        }
    }

    public void ChangeScene(string sceneName)
    {
        if (!transitioning)
            StartCoroutine(ChangeSceneRoutine(sceneName));
    }

    private IEnumerator ChangeSceneRoutine(string sceneName)
    {
        transitioning = true;
        overlay.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return Fade(1f, 0f);

        overlay.blocksRaycasts = false;
        transitioning = false;
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;
        overlay.alpha = start;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Lerp(
                start, end,
                Mathf.Clamp01(elapsed / transitionDuration)
            );

            yield return null;
        }

        overlay.alpha = end;
    }
}
