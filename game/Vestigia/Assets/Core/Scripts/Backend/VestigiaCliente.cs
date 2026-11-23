
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class VestigiaCliente : MonoBehaviour
{
    public static VestigiaCliente Instance { get; private set; }

    [Header("Backend")]
    [SerializeField]
    private string backendUrl =
        "https://vestigia-backend-production-745d.up.railway.app";

    [Header("Prueba temporal")]
    [SerializeField]
    private bool mostrarInterfazPrueba = false;

    [Serializable]
    public class Pueblo
    {
        public string codigo;
        public bool libre;
    }

    [Serializable]
    public class SalaCreada
    {
        public string codigo;
        public string estado;
        public string token;
    }

    [Serializable]
    public class Datos
    {
        public bool sintonizada;
        public bool iniciado;
        public bool completada;
        public string tipo;
        public string valor;
    }

    [Serializable]
    public class PuzzleEstado
    {
        public string puzzle;
        public string estado;
        public Datos datos;
    }

    [Serializable]
    public class Descubrimiento
    {
        public string clave;
        public Datos datos;
        public string origen;
        public string creado_en;
    }

    [Serializable]
    public class Dispositivos
    {
        public bool juego;
        public bool app;
    }

    [Serializable]
    public class EstadoSala
    {
        public string codigo;
        public string estado;
        public string escena_actual;
        public PuzzleEstado[] puzzles;
        public Descubrimiento[] descubrimientos;
        public Dispositivos dispositivos;
        public string creada_en;
        public string expira_en;
    }

    [Serializable]
    private class ListaPueblos
    {
        public Pueblo[] items;
    }

    [Serializable]
    private class ErrorApi
    {
        public int status;
        public string codigo;
        public string message;
    }

    [Serializable]
    private class PayloadWs
    {
        public string rol;
        public string sala;
        public string puzzle;
        public string estado;
        public Datos datos;
        public string clave;
        public string origen;
        public string codigo;
        public string message;
    }

    [Serializable]
    private class MensajeWs
    {
        public string type;
        public PayloadWs payload;
    }

    [Serializable]
    private class CodigoRequest
    {
        public string codigo;
    }

    [Serializable]
    private class EscenaRequest
    {
        public string escena;
    }

    [Serializable]
    private class DescubrimientoRequest
    {
        public string clave;
    }

    [Serializable]
    private class AuthPayload
    {
        public string token;
    }

    [Serializable]
    private class AuthRequest
    {
        public string type = "auth";
        public AuthPayload payload;
    }

    // =========================================================
    // URL Y ESTADO PÚBLICO
    // =========================================================

    private string Base
    {
        get
        {
            string url = backendUrl.Trim().TrimEnd('/');

            if (url.StartsWith("http://") ||
                url.StartsWith("https://"))
                return url;

            return "https://" + url;
        }
    }

    private string WsUrl
    {
        get
        {
            if (Base.StartsWith("https://"))
                return "wss://" + Base.Substring(8) + "/ws";

            return "ws://" + Base.Substring(7) + "/ws";
        }
    }

    public string Codigo =>
        PlayerPrefs.GetString("vestigia_codigo", "");

    private string Token =>
        PlayerPrefs.GetString("vestigia_token", "");

    public bool AppEstaConectada { get; private set; }
    public bool WebSocketAutenticado { get; private set; }

    public bool TieneSalaActiva => salaCreada;
    public bool EstaCreandoSala => creandoSala;
    public bool CancelandoSala => cancelandoSala;
    public bool BackendDisponible => backendDisponible;
    public bool CargandoPueblos => cargandoPueblos;

    public string Mensaje => mensaje;
    public string EstadoConexion => estadoConexion;

    public Pueblo[] PueblosDisponibles => pueblosDisponibles;

    public bool MostrarInterfazPrueba
    {
        get => mostrarInterfazPrueba;
        set => mostrarInterfazPrueba = value;
    }

    // =========================================================
    // EVENTOS
    // =========================================================

    public event Action<EstadoSala> EstadoRecibido;
    public event Action<bool> AppConectada;
    public event Action<PuzzleEstado> PuzzleActualizado;
    public event Action<Descubrimiento> DescubrimientoNuevo;
    public event Action SalaTerminada;
    public event Action<Pueblo[]> PueblosActualizados;

    // =========================================================
    // ESTADO INTERNO
    // =========================================================

    private Pueblo[] pueblosDisponibles;
    private int puebloSeleccionado;

    private bool backendDisponible;
    private bool cargandoPueblos;
    private bool creandoSala;
    private bool salaCreada;
    private bool cancelandoSala;

    private string mensaje = "Conectando al backend...";
    private string estadoConexion = "Desconectado";

    private Vector2 scrollPueblos;

    private readonly ConcurrentQueue<Action> accionesPendientes =
        new ConcurrentQueue<Action>();

    private ClientWebSocket socket;
    private CancellationTokenSource cancelar;

    private bool reconectar;
    private int intentos;

    // =========================================================
    // CICLO DE VIDA
    // =========================================================

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

    private void Start()
    {
        if (!string.IsNullOrEmpty(Codigo) &&
            !string.IsNullOrEmpty(Token))
        {
            salaCreada = true;
            mensaje = "Sesión anterior encontrada: " + Codigo;

            ConectarWebSocket();
        }
        else
        {
            StartCoroutine(ComprobarBackend());
        }
    }

    private void Update()
    {
        while (accionesPendientes.TryDequeue(out Action accion))
        {
            accion?.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            DetenerWebSocket();
            Instance = null;
        }
    }

    // =========================================================
    // PETICIONES HTTP
    // =========================================================

    private IEnumerator Enviar(
        string metodo,
        string ruta,
        string cuerpoJson,
        bool conToken,
        Action<long, string> respuesta)
    {
        using (UnityWebRequest request =
               new UnityWebRequest(Base + ruta, metodo))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            if (cuerpoJson != null)
            {
                byte[] body = Encoding.UTF8.GetBytes(cuerpoJson);

                request.uploadHandler = new UploadHandlerRaw(body);

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json"
                );
            }

            if (conToken)
            {
                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + Token
                );
            }

            request.timeout = 20;

            yield return request.SendWebRequest();

            if (request.result ==
                    UnityWebRequest.Result.ConnectionError ||
                request.result ==
                    UnityWebRequest.Result.DataProcessingError)
            {
                respuesta?.Invoke(0, null);
                yield break;
            }

            respuesta?.Invoke(
                request.responseCode,
                request.downloadHandler.text
            );
        }
    }

    private IEnumerator ComprobarBackend()
    {
        mensaje = "Comprobando backend...";

        yield return Enviar(
            "GET",
            "/actuator/health",
            null,
            false,
            (status, cuerpo) =>
            {
                if (status == 200)
                {
                    backendDisponible = true;

                    mensaje =
                        "Backend conectado. Consultando pueblos...";

                    Debug.Log("[VESTIGIA] Backend conectado.");
                }
                else
                {
                    backendDisponible = false;
                    mensaje = "No se pudo conectar con el backend.";

                    Debug.LogError(
                        "[VESTIGIA] Error de conexión HTTP: " +
                        status
                    );
                }
            }
        );

        if (backendDisponible)
            yield return StartCoroutine(PedirPueblos());
    }

    // =========================================================
    // PUEBLOS
    // =========================================================

    public IEnumerator PedirPueblos(
        Action<Pueblo[]> alTerminar = null)
    {
        if (cargandoPueblos)
            yield break;

        cargandoPueblos = true;
        mensaje = "Consultando pueblos...";

        yield return Enviar(
            "GET",
            "/api/v1/pueblos?disponibles=true",
            null,
            false,
            (status, cuerpo) =>
            {
                cargandoPueblos = false;

                if (status == 200)
                {
                    try
                    {
                        string json = "{\"items\":" + cuerpo + "}";

                        ListaPueblos respuesta =
                            JsonUtility.FromJson<ListaPueblos>(json);

                        if (respuesta == null ||
                            respuesta.items == null)
                        {
                            mensaje = "Lista de pueblos inválida.";
                            alTerminar?.Invoke(null);
                            return;
                        }

                        pueblosDisponibles = respuesta.items;
                        puebloSeleccionado = 0;

                        mensaje =
                            "Pueblos disponibles: " +
                            pueblosDisponibles.Length;

                        Debug.Log(
                            "[VESTIGIA] Pueblos disponibles: " +
                            pueblosDisponibles.Length
                        );

                        PueblosActualizados?.Invoke(
                            pueblosDisponibles
                        );

                        alTerminar?.Invoke(pueblosDisponibles);
                    }
                    catch (Exception e)
                    {
                        mensaje = "Error procesando pueblos.";

                        Debug.LogError(
                            "[VESTIGIA] " + e.Message
                        );

                        alTerminar?.Invoke(null);
                    }
                }
                else
                {
                    mensaje = status == 0
                        ? "Sin conexión con el servidor."
                        : "Error al consultar pueblos: " + status;

                    Debug.LogError("[VESTIGIA] " + mensaje);

                    alTerminar?.Invoke(null);
                }
            }
        );
    }

    public void ActualizarPueblos()
    {
        if (!cargandoPueblos)
            StartCoroutine(PedirPueblos());
    }

    // =========================================================
    // CREACIÓN DE SALA
    // =========================================================

    public void CrearSala(string codigo)
    {
        if (!backendDisponible ||
            creandoSala ||
            salaCreada ||
            cancelandoSala)
            return;

        if (string.IsNullOrWhiteSpace(codigo))
            return;

        StartCoroutine(CrearSalaCoroutine(codigo));
    }

    private IEnumerator CrearSalaCoroutine(string codigo)
    {
        creandoSala = true;
        mensaje = "Creando sala...";

        string json = JsonUtility.ToJson(
            new CodigoRequest { codigo = codigo }
        );

        yield return Enviar(
            "POST",
            "/api/v1/salas",
            json,
            false,
            (status, cuerpo) =>
            {
                if (status == 201)
                {
                    try
                    {
                        SalaCreada sala =
                            JsonUtility.FromJson<SalaCreada>(cuerpo);

                        if (sala == null ||
                            string.IsNullOrEmpty(sala.token) ||
                            string.IsNullOrEmpty(sala.codigo))
                        {
                            mensaje = "Respuesta de sala inválida.";

                            Debug.LogError(
                                "[VESTIGIA] " + mensaje
                            );

                            return;
                        }

                        PlayerPrefs.SetString(
                            "vestigia_codigo",
                            sala.codigo
                        );

                        PlayerPrefs.SetString(
                            "vestigia_token",
                            sala.token
                        );

                        PlayerPrefs.Save();

                        salaCreada = true;
                        AppEstaConectada = false;

                        mensaje = "Sala creada: " + sala.codigo;

                        Debug.Log(
                            "[VESTIGIA] Sala creada: " + sala.codigo
                        );

                        ConectarWebSocket();
                    }
                    catch (Exception e)
                    {
                        mensaje = "Error procesando la sala.";

                        Debug.LogError(
                            "[VESTIGIA] " + e.Message
                        );
                    }
                }
                else if (status == 409)
                {
                    mensaje = "Ese pueblo ya está ocupado.";

                    Debug.LogWarning("[VESTIGIA] Pueblo ocupado.");

                    StartCoroutine(PedirPueblos());
                }
                else
                {
                    ManejarError(status, cuerpo);
                }
            }
        );

        creandoSala = false;
    }

    // =========================================================
    // ESTADO DE SALA
    // =========================================================

    public IEnumerator PedirEstado()
    {
        if (string.IsNullOrEmpty(Codigo) ||
            string.IsNullOrEmpty(Token))
            yield break;

        yield return Enviar(
            "GET",
            "/api/v1/salas/" + Codigo + "/estado",
            null,
            true,
            (status, cuerpo) =>
            {
                if (status == 200)
                {
                    EstadoSala estado =
                        JsonUtility.FromJson<EstadoSala>(cuerpo);

                    if (estado != null)
                    {
                        AppEstaConectada =
                            estado.dispositivos != null &&
                            estado.dispositivos.app;

                        AppConectada?.Invoke(AppEstaConectada);
                        EstadoRecibido?.Invoke(estado);

                        Debug.Log(
                            "[VESTIGIA] Estado de sala actualizado."
                        );
                    }
                }
                else
                {
                    ManejarError(status, cuerpo);
                }
            }
        );
    }

    // =========================================================
    // CAMBIOS DE ESCENA
    // =========================================================

    public IEnumerator CambiarEscena(
        string escena,
        Action<bool> alTerminar = null)
    {
        if (string.IsNullOrEmpty(Codigo))
        {
            alTerminar?.Invoke(false);
            yield break;
        }

        string json = JsonUtility.ToJson(
            new EscenaRequest { escena = escena }
        );

        yield return Enviar(
            "PUT",
            "/api/v1/salas/" + Codigo + "/escena",
            json,
            true,
            (status, cuerpo) =>
            {
                bool correcto = status >= 200 && status < 300;

                if (!correcto)
                    ManejarError(status, cuerpo);

                alTerminar?.Invoke(correcto);
            }
        );
    }

    // =========================================================
    // DESCUBRIMIENTOS
    // =========================================================

    public IEnumerator RegistrarDescubrimiento(string clave)
    {
        if (string.IsNullOrEmpty(Codigo))
            yield break;

        string json = JsonUtility.ToJson(
            new DescubrimientoRequest { clave = clave }
        );

        yield return Enviar(
            "POST",
            "/api/v1/salas/" + Codigo + "/descubrimientos",
            json,
            true,
            SoloErrores
        );
    }

    // =========================================================
    // CANCELAR / CERRAR SALA
    // =========================================================

    public void CancelarSala()
    {
        if (!salaCreada || cancelandoSala)
            return;

        StartCoroutine(CerrarSala());
    }

    public IEnumerator CerrarSala()
    {
        if (string.IsNullOrEmpty(Codigo) ||
            string.IsNullOrEmpty(Token) ||
            cancelandoSala)
            yield break;

        cancelandoSala = true;
        mensaje = "Cancelando sala...";

        DetenerWebSocket();

        long resultadoHTTP = 0;
        string respuestaHTTP = null;

        yield return Enviar(
            "DELETE",
            "/api/v1/salas/" + Codigo,
            null,
            true,
            (status, cuerpo) =>
            {
                resultadoHTTP = status;
                respuestaHTTP = cuerpo;
            }
        );

        if (resultadoHTTP == 204 ||
            resultadoHTTP == 404 ||
            resultadoHTTP == 401)
        {
            Debug.Log("[VESTIGIA] Sala cancelada.");

            Terminar();
            yield break;
        }

        cancelandoSala = false;

        mensaje = resultadoHTTP == 0
            ? "No se pudo conectar para cancelar la sala."
            : "No se pudo cancelar la sala. HTTP: " + resultadoHTTP;

        Debug.LogWarning("[VESTIGIA] " + mensaje);

        if (!string.IsNullOrEmpty(Token))
            ConectarWebSocket();
    }

    private void SoloErrores(long status, string cuerpo)
    {
        if (status < 200 || status >= 300)
            ManejarError(status, cuerpo);
    }

    private void ManejarError(long status, string cuerpo)
    {
        if (status == 0)
        {
            mensaje = "Sin conexión con el servidor.";

            Debug.LogWarning("[VESTIGIA] Sin conexión.");
            return;
        }

        ErrorApi error = null;

        if (!string.IsNullOrEmpty(cuerpo))
        {
            try
            {
                error = JsonUtility.FromJson<ErrorApi>(cuerpo);
            }
            catch
            {
                // Respuesta no compatible con ErrorApi.
            }
        }

        if (status == 401)
        {
            Debug.LogWarning(
                "[VESTIGIA] Sesión inválida o expirada."
            );

            Terminar();
            return;
        }

        if (status == 403)
        {
            Debug.LogError(
                "[VESTIGIA] Error de permisos: " +
                (error != null ? error.message : cuerpo)
            );

            return;
        }

        if (status == 409 &&
            error != null &&
            error.codigo == "puzzle_bloqueado")
        {
            Debug.LogWarning(
                "[VESTIGIA] Puzzle bloqueado: " +
                error.message
            );

            return;
        }

        Debug.LogError(
            "[VESTIGIA] HTTP " + status + ": " +
            (error != null ? error.message : cuerpo)
        );
    }

    private void Terminar()
    {
        DetenerWebSocket();

        PlayerPrefs.DeleteKey("vestigia_codigo");
        PlayerPrefs.DeleteKey("vestigia_token");
        PlayerPrefs.Save();

        salaCreada = false;
        creandoSala = false;
        cancelandoSala = false;

        AppEstaConectada = false;
        WebSocketAutenticado = false;

        mensaje = "Sala terminada.";

        SalaTerminada?.Invoke();
    }

    // =========================================================
    // WEBSOCKET
    // =========================================================

    public async void ConectarWebSocket()
    {
        if (string.IsNullOrEmpty(Token))
        {
            Debug.LogError(
                "[VESTIGIA] No existe token para conectar."
            );

            return;
        }

        DetenerWebSocket();

        reconectar = true;
        intentos = 0;

        CancellationTokenSource miCancelacion =
            new CancellationTokenSource();

        cancelar = miCancelacion;

        while (reconectar &&
               !miCancelacion.IsCancellationRequested)
        {
            int codigoCierre = -1;

            ClientWebSocket nuevoSocket = new ClientWebSocket();

            socket = nuevoSocket;
            WebSocketAutenticado = false;

            try
            {
                estadoConexion = "Conectando...";

                Debug.Log(
                    "[VESTIGIA] Conectando WebSocket: " + WsUrl
                );

                await nuevoSocket.ConnectAsync(
                    new Uri(WsUrl),
                    miCancelacion.Token
                );

                string auth = JsonUtility.ToJson(
                    new AuthRequest
                    {
                        payload = new AuthPayload
                        {
                            token = Token
                        }
                    }
                );

                byte[] datos = Encoding.UTF8.GetBytes(auth);

                await nuevoSocket.SendAsync(
                    new ArraySegment<byte>(datos),
                    WebSocketMessageType.Text,
                    true,
                    miCancelacion.Token
                );

                estadoConexion = "Autenticando...";

                codigoCierre = await LeerMensajes(
                    nuevoSocket,
                    miCancelacion.Token
                );
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                string error = e.Message;

                accionesPendientes.Enqueue(() =>
                {
                    Debug.LogWarning(
                        "[VESTIGIA] WebSocket: " + error
                    );
                });
            }
            finally
            {
                nuevoSocket.Dispose();

                if (socket == nuevoSocket)
                    socket = null;
            }

            WebSocketAutenticado = false;

            if (!reconectar ||
                miCancelacion.IsCancellationRequested)
                break;

            if (codigoCierre == 1000 ||
                codigoCierre == 4001 ||
                codigoCierre == 4009 ||
                codigoCierre == 4000 ||
                codigoCierre == 1009)
            {
                if (codigoCierre == 4001)
                {
                    accionesPendientes.Enqueue(Terminar);
                }

                reconectar = false;
                break;
            }

            int segundos = intentos < 5
                ? (int)Math.Pow(2, intentos)
                : 30;

            segundos = Math.Min(segundos, 30);

            intentos++;

            estadoConexion =
                "Reconectando en " + segundos + " s...";

            try
            {
                await Task.Delay(
                    segundos * 1000,
                    miCancelacion.Token
                );
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        if (cancelar == miCancelacion)
            cancelar = null;

        miCancelacion.Dispose();
    }

    private async Task<int> LeerMensajes(
        ClientWebSocket wsActual,
        CancellationToken token)
    {
        byte[] buffer = new byte[8192];

        using (MemoryStream mensaje = new MemoryStream())
        {
            while (wsActual.State == WebSocketState.Open)
            {
                WebSocketReceiveResult resultado =
                    await wsActual.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        token
                    );

                if (resultado.MessageType ==
                    WebSocketMessageType.Close)
                {
                    return (int)(
                        wsActual.CloseStatus ??
                        WebSocketCloseStatus.Empty
                    );
                }

                mensaje.Write(buffer, 0, resultado.Count);

                if (mensaje.Length > 16384)
                    return 1009;

                if (resultado.EndOfMessage)
                {
                    string json = Encoding.UTF8.GetString(
                        mensaje.ToArray()
                    );

                    mensaje.SetLength(0);

                    accionesPendientes.Enqueue(
                        () => AlRecibir(json)
                    );
                }
            }
        }

        return -1;
    }

    private void AlRecibir(string json)
    {
        MensajeWs mensajeWs;

        try
        {
            mensajeWs =
                JsonUtility.FromJson<MensajeWs>(json);
        }
        catch
        {
            Debug.LogWarning(
                "[VESTIGIA] Mensaje WebSocket inválido."
            );

            return;
        }

        if (mensajeWs == null)
            return;

        PayloadWs p = mensajeWs.payload;

        switch (mensajeWs.type)
        {
            case "auth.ok":
                WebSocketAutenticado = true;
                intentos = 0;
                estadoConexion = "Conectado";

                Debug.Log(
                    "[VESTIGIA] WebSocket autenticado."
                );

                StartCoroutine(PedirEstado());
                break;

            case "dispositivo.conectado":
                if (p != null && p.rol == "app")
                {
                    AppEstaConectada = true;
                    AppConectada?.Invoke(true);

                    Debug.Log(
                        "[VESTIGIA] Teléfono conectado."
                    );
                }
                break;

            case "dispositivo.desconectado":
                if (p != null && p.rol == "app")
                {
                    AppEstaConectada = false;
                    AppConectada?.Invoke(false);

                    Debug.Log(
                        "[VESTIGIA] Teléfono desconectado."
                    );
                }
                break;

            case "puzzle.actualizado":
                if (p != null)
                {
                    PuzzleActualizado?.Invoke(
                        new PuzzleEstado
                        {
                            puzzle = p.puzzle,
                            estado = p.estado,
                            datos = p.datos
                        }
                    );

                    Debug.Log(
                        "[VESTIGIA] Puzzle " +
                        p.puzzle + ": " + p.estado
                    );
                }
                break;

            case "descubrimiento.nuevo":
                if (p != null)
                {
                    DescubrimientoNuevo?.Invoke(
                        new Descubrimiento
                        {
                            clave = p.clave,
                            datos = p.datos,
                            origen = p.origen
                        }
                    );

                    Debug.Log(
                        "[VESTIGIA] Descubrimiento: " +
                        p.clave
                    );
                }
                break;

            case "sala.cerrada":
                Debug.Log("[VESTIGIA] Sala cerrada.");
                Terminar();
                break;

            case "error":
                if (p != null)
                {
                    Debug.LogWarning(
                        "[VESTIGIA] WebSocket " +
                        p.codigo + ": " + p.message
                    );
                }
                break;
        }
    }

    public void DetenerWebSocket()
    {
        reconectar = false;

        if (cancelar != null)
        {
            cancelar.Cancel();
            cancelar = null;
        }

        WebSocketAutenticado = false;
        estadoConexion = "Desconectado";
    }

    // =========================================================
    // INTERFAZ TEMPORAL DE PRUEBA
    // =========================================================

    private void OnGUI()
    {
        if (!Application.isPlaying || !mostrarInterfazPrueba)
            return;

        GUILayout.BeginArea(new Rect(20, 20, 380, 600));

        GUILayout.Label("VESTIGIA - MENU TEMPORAL");
        GUILayout.Space(8);

        GUILayout.Label(mensaje);
        GUILayout.Label("WebSocket: " + estadoConexion);

        if (cargandoPueblos)
        {
            GUILayout.Label("Cargando pueblos...");
        }
        else if (!salaCreada && pueblosDisponibles != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Selecciona un pueblo:");

            scrollPueblos = GUILayout.BeginScrollView(
                scrollPueblos,
                GUILayout.Height(250)
            );

            for (int i = 0; i < pueblosDisponibles.Length; i++)
            {
                string codigo = pueblosDisponibles[i].codigo;

                if (GUILayout.Toggle(
                    puebloSeleccionado == i,
                    codigo,
                    "Button"))
                {
                    puebloSeleccionado = i;
                }
            }

            GUILayout.EndScrollView();

            GUI.enabled =
                !creandoSala &&
                pueblosDisponibles.Length > 0;

            if (GUILayout.Button("CREAR SALA"))
            {
                CrearSala(
                    pueblosDisponibles[puebloSeleccionado].codigo
                );
            }

            GUI.enabled = true;

            if (GUILayout.Button("ACTUALIZAR PUEBLOS"))
                ActualizarPueblos();
        }

        if (salaCreada)
        {
            GUILayout.Space(10);
            GUILayout.Label("Código de sala: " + Codigo);

            GUILayout.Label(
                AppEstaConectada
                    ? "Teléfono conectado"
                    : "Esperando teléfono..."
            );

            GUI.enabled = !cancelandoSala;

            if (GUILayout.Button(
                cancelandoSala ? "CANCELANDO..." : "CANCELAR SALA"))
            {
                CancelarSala();
            }

            GUI.enabled = true;
        }

        GUILayout.EndArea();
    }
}
