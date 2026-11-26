
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LensVisualController : MonoBehaviour
{
    [Header("Interfaz de la lente")]
    [SerializeField] private Image overlayLenteSucio;

    [Header("Simulación de prueba")]
    [SerializeField] private bool simulateForTesting = true;
    [SerializeField] private float cleaningDuration = 3f;
    [SerializeField] private float focusingDuration = 3f;

    [Header("Efectos visuales")]
    [SerializeField] private float dirtyAlpha = 0.35f;
    [SerializeField] private float transitionSpeed = 2f;

    [Header("Frase de descubrimiento")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private string finalPhrase = "Ahí está...";
    [SerializeField] private float finalPhraseDuration = 2.5f;

    [Header("Sonido de descubrimiento")]
    [SerializeField] private AudioSource discoveryAudioSource;
    [SerializeField] private AudioClip discoverySound;

    [Header("Transición de escena")]
    [SerializeField] private string nextScene = "Escena3_Faro";
    [SerializeField] private string returnSpawnPoint = "Spawn_Velador";

    private static readonly int BlurIntensity =
        Shader.PropertyToID("_VestigiaBlurIntensity");

    private float currentDirtyAlpha;
    private float targetDirtyAlpha;

    private float currentBlur;
    private float targetBlur;

    private bool sequenceActive;
    private bool finalSequenceStarted;

    private Coroutine simulationCoroutine;

    private void Awake()
    {
        Shader.SetGlobalFloat(BlurIntensity, 0f);

        currentDirtyAlpha = 0f;
        targetDirtyAlpha = 0f;

        currentBlur = 0f;
        targetBlur = 0f;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        UpdateDirtyOverlay();
    }

    public void ActivateLensSequence()
    {
        if (sequenceActive)
            return;

        sequenceActive = true;

        targetDirtyAlpha = dirtyAlpha;
        targetBlur = 1f;

        Shader.SetGlobalFloat(BlurIntensity, currentBlur);

        Debug.Log("Mirador: lente sucia y desenfocada.");

        // Con el teléfono conectado, el mirador se juega en la app: al
        // resolverse allá, aquí se limpia la lente y se enfoca.
        if (MobileBridge.Instance != null && MobileBridge.AppDisponible)
        {
            MobileBridge.Instance.StartPuzzle(
                "lookout",
                () => simulationCoroutine = StartCoroutine(FinishFromApp())
            );
        }
        else if (simulateForTesting)
        {
            simulationCoroutine = StartCoroutine(SimulatePuzzle());
        }
    }

    private IEnumerator SimulatePuzzle()
    {
        yield return new WaitForSeconds(cleaningDuration);

        CompleteLensCleaning();

        yield return new WaitForSeconds(focusingDuration);

        CompleteObservatoryFound();

        simulationCoroutine = null;
    }

    private IEnumerator FinishFromApp()
    {
        CompleteLensCleaning();

        yield return new WaitForSeconds(focusingDuration);

        CompleteObservatoryFound();

        simulationCoroutine = null;
    }

    public void CompleteLensCleaning()
    {
        targetDirtyAlpha = 0f;

        Debug.Log("Mirador: lente limpia. Permanece el desenfoque.");
    }

    public void CompleteObservatoryFound()
    {
        targetDirtyAlpha = 0f;
        targetBlur = 0f;

        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.SetObservatorioEncontrado();
        }
        else
        {
            Debug.LogError(
                "LensVisualController: No existe GameProgress.Instance."
            );
        }

        Debug.Log("Mirador: observatorio enfocado y registrado.");
    }

    private void Update()
    {
        if (!sequenceActive)
            return;

        currentDirtyAlpha = Mathf.MoveTowards(
            currentDirtyAlpha,
            targetDirtyAlpha,
            transitionSpeed * Time.deltaTime
        );

        currentBlur = Mathf.MoveTowards(
            currentBlur,
            targetBlur,
            transitionSpeed * Time.deltaTime
        );

        UpdateDirtyOverlay();

        Shader.SetGlobalFloat(BlurIntensity, currentBlur);

        if (targetBlur <= 0f &&
            currentBlur <= 0.001f &&
            !finalSequenceStarted)
        {
            finalSequenceStarted = true;

            StartCoroutine(FinalDiscoverySequence());
        }
    }

    private void UpdateDirtyOverlay()
    {
        if (overlayLenteSucio == null)
            return;

        Color color = overlayLenteSucio.color;
        color.a = currentDirtyAlpha;

        overlayLenteSucio.color = color;
    }

    private IEnumerator FinalDiscoverySequence()
    {
        Debug.Log("Mirador: iniciando descubrimiento final.");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = finalPhrase;

        if (discoveryAudioSource != null && discoverySound != null)
            discoveryAudioSource.PlayOneShot(discoverySound);

        yield return new WaitForSeconds(finalPhraseDuration);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SceneTransition transition = SceneTransition.Instance;

        if (transition == null)
        {
            SceneTransition[] transitions =
                FindObjectsByType<SceneTransition>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            if (transitions.Length > 0)
            {
                transition = transitions[0];

                if (!transition.gameObject.activeInHierarchy)
                    transition.gameObject.SetActive(true);

                if (!transition.enabled)
                    transition.enabled = true;

                yield return null;
            }
        }

        if (SceneTransition.Instance != null)
            transition = SceneTransition.Instance;

        if (transition == null)
        {
            Debug.LogError(
                "Mirador: no existe ningún SceneTransition."
            );

            yield break;
        }

        Debug.Log(
            "Mirador: regresando al faro en " + returnSpawnPoint
        );

        transition.ChangeScene(
            nextScene,
            returnSpawnPoint,
            true
        );
    }

    private void OnDisable()
    {
        Shader.SetGlobalFloat(BlurIntensity, 0f);
    }
}
