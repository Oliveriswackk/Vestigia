using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Puente entre la historia y los puzzles del teléfono.
//
// Con una sala activa y la app conectada, el puzzle se juega en el
// teléfono de verdad: se registra el descubrimiento que lo desbloquea,
// después "abrir_<puzzle>" (la app abre la pantalla en ese momento) y se
// espera a que el servidor lo marque como resuelto.
//
// Sin teléfono, o si se desconecta a medias, se simula con una espera para
// que el juego se pueda completar sin la app.
public class MobileBridge : MonoBehaviour
{
    public static MobileBridge Instance { get; private set; }

    [Header("Simulación de Móvil")]
    [SerializeField] private float simulationDelay = 3f;

    public bool IsPuzzleRunning { get; private set; }

    // Verdadero si el próximo puzzle se jugará en la app real.
    public static bool AppDisponible =>
        VestigiaCliente.Instance != null &&
        VestigiaCliente.Instance.TieneSalaActiva &&
        VestigiaCliente.Instance.AppEstaConectada;

    private string puzzleEnApp;
    private bool resueltoEnApp;
    private bool appDesconectada;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        DejarDeEscuchar();
    }

    public void StartPuzzle(string puzzleId, UnityAction onCompleted)
    {
        if (IsPuzzleRunning)
        {
            Debug.LogWarning("Ya hay un puzzle en ejecución.");
            return;
        }

        string puzzle = PuzzleBackend(puzzleId);

        if (puzzle != null && AppDisponible)
            StartCoroutine(PuzzleEnApp(puzzle, onCompleted));
        else
            StartCoroutine(SimulatePuzzle(puzzleId, onCompleted));
    }

    // Nombre del puzzle en el backend para cada id del juego.
    private static string PuzzleBackend(string puzzleId)
    {
        switch (puzzleId)
        {
            case "radio": return "radio";
            case "photo": return "photo";
            case "lookout": return "lookout";
            case "telescopio":
            case "observatory": return "observatory";
            default: return null;
        }
    }

    // Descubrimiento que el juego registra para desbloquear el puzzle.
    // La foto se desbloquea sola con las tres piezas.
    private static string ClaveDesbloqueo(string puzzle)
    {
        switch (puzzle)
        {
            case "radio": return "pescador_dialogo_completo";
            case "lookout": return "lookout_enabled";
            case "observatory": return "observatory_enabled";
            default: return null;
        }
    }

    private IEnumerator PuzzleEnApp(string puzzle, UnityAction onCompleted)
    {
        IsPuzzleRunning = true;
        puzzleEnApp = puzzle;
        resueltoEnApp = false;
        appDesconectada = false;

        VestigiaCliente cliente = VestigiaCliente.Instance;
        cliente.PuzzleActualizado += AlPuzzleActualizado;
        cliente.AppConectada += AlCambiarConexionApp;

        Debug.Log("Móvil: el puzzle " + puzzle + " se juega en la app.");

        // En orden: primero se desbloquea y después se pide abrirlo.
        string desbloqueo = ClaveDesbloqueo(puzzle);

        if (desbloqueo != null)
            yield return cliente.RegistrarDescubrimiento(desbloqueo);

        yield return cliente.RegistrarDescubrimiento("abrir_" + puzzle);

        while (!resueltoEnApp && !appDesconectada)
            yield return null;

        DejarDeEscuchar();

        if (appDesconectada && !resueltoEnApp)
        {
            Debug.LogWarning(
                "Móvil: la app se desconectó; se simula " + puzzle + "."
            );

            yield return new WaitForSeconds(simulationDelay);
        }

        Debug.Log("Móvil: puzzle completado " + puzzle);

        IsPuzzleRunning = false;

        onCompleted?.Invoke();
    }

    private void AlPuzzleActualizado(VestigiaCliente.PuzzleEstado estado)
    {
        if (estado != null &&
            estado.puzzle == puzzleEnApp &&
            estado.estado == "resuelto")
        {
            resueltoEnApp = true;
        }
    }

    private void AlCambiarConexionApp(bool conectada)
    {
        if (!conectada)
            appDesconectada = true;
    }

    private void DejarDeEscuchar()
    {
        if (VestigiaCliente.Instance != null)
        {
            VestigiaCliente.Instance.PuzzleActualizado -= AlPuzzleActualizado;
            VestigiaCliente.Instance.AppConectada -= AlCambiarConexionApp;
        }

        puzzleEnApp = null;
    }

    private IEnumerator SimulatePuzzle(
        string puzzleId,
        UnityAction onCompleted)
    {
        IsPuzzleRunning = true;

        Debug.Log("Móvil: iniciando puzzle " + puzzleId);

        yield return new WaitForSeconds(simulationDelay);

        Debug.Log("Móvil: puzzle completado " + puzzleId);

        IsPuzzleRunning = false;

        onCompleted?.Invoke();
    }
}
