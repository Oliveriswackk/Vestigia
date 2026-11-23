
using System.Collections;
using UnityEngine;

public class MaquetaTransition : MonoBehaviour
{
    [Header("Maqueta")]
    [SerializeField] private Transform maqueta;

    [Header("Controles que se pausan")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraRotation cameraRotation;
    [SerializeField] private PlayerCameraFacing playerCameraFacing;

    [Header("Movimiento vertical")]
    [SerializeField] private float distanciaVertical = 30f;
    [SerializeField] private float duracion = 2f;

    private Vector3 posicionInicial;
    private Coroutine movimientoActual;
    private bool maquetaElevada;

    private Rigidbody[] rigidbodies;
    private bool[] gravedadOriginal;
    private bool[] kinematicOriginal;

    private bool movimientoOriginal;
    private bool rotacionOriginal;
    private bool seguimientoOriginal;
    private bool controlesGuardados;

    private void Awake()
    {
        if (maqueta == null)
        {
            Debug.LogError("MaquetaTransition: falta asignar PuebloRoot.");
            enabled = false;
            return;
        }

        posicionInicial = maqueta.position;

        rigidbodies = maqueta.GetComponentsInChildren<Rigidbody>(true);

        gravedadOriginal = new bool[rigidbodies.Length];
        kinematicOriginal = new bool[rigidbodies.Length];

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            gravedadOriginal[i] = rigidbodies[i].useGravity;
            kinematicOriginal[i] = rigidbodies[i].isKinematic;
        }
    }

    [ContextMenu("PRUEBA: Subir maqueta")]
    public void SubirMaqueta()
    {
        if (maquetaElevada)
            return;

        GuardarYBloquear();

        maquetaElevada = true;

        MoverMaqueta(
            posicionInicial + Vector3.up * distanciaVertical
        );
    }

    [ContextMenu("PRUEBA: Bajar maqueta")]
    public void BajarMaqueta()
    {
        if (!maquetaElevada)
            return;

        MoverMaqueta(posicionInicial, RestaurarControles);
        maquetaElevada = false;
    }

    private void GuardarYBloquear()
    {
        if (!controlesGuardados)
        {
            movimientoOriginal = playerMovement != null &&
                                 playerMovement.enabled;

            rotacionOriginal = cameraRotation != null &&
                               cameraRotation.enabled;

            seguimientoOriginal = playerCameraFacing != null &&
                                  playerCameraFacing.enabled;

            controlesGuardados = true;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (playerCameraFacing != null)
            playerCameraFacing.enabled = false;

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] == null)
                continue;

            rigidbodies[i].linearVelocity = Vector3.zero;
            rigidbodies[i].angularVelocity = Vector3.zero;
            rigidbodies[i].useGravity = false;
            rigidbodies[i].isKinematic = true;
        }
    }

    private void RestaurarControles()
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] == null)
                continue;

            rigidbodies[i].isKinematic = kinematicOriginal[i];
            rigidbodies[i].useGravity = gravedadOriginal[i];
        }

        if (playerMovement != null)
            playerMovement.enabled = movimientoOriginal;

        if (cameraRotation != null)
            cameraRotation.enabled = rotacionOriginal;

        if (playerCameraFacing != null)
            playerCameraFacing.enabled = seguimientoOriginal;

        controlesGuardados = false;
    }

    private void MoverMaqueta(
        Vector3 destino,
        System.Action alTerminar = null)
    {
        if (movimientoActual != null)
            StopCoroutine(movimientoActual);

        movimientoActual = StartCoroutine(
            AnimarMovimiento(destino, alTerminar)
        );
    }

    private IEnumerator AnimarMovimiento(
        Vector3 destino,
        System.Action alTerminar)
    {
        Vector3 inicio = maqueta.position;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);
            float suavizado = progreso * progreso *
                              (3f - 2f * progreso);

            maqueta.position = Vector3.Lerp(
                inicio,
                destino,
                suavizado
            );

            yield return null;
        }

        maqueta.position = destino;
        movimientoActual = null;

        alTerminar?.Invoke();
    }
}
