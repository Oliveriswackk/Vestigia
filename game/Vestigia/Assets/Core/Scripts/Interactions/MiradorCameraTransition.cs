
using UnityEngine;
using System.Collections;

public class MiradorCameraTransition : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera cameraTargetObservatorio;
    [SerializeField] private PlayerCameraFacing playerCameraFacing;
    [SerializeField] private LensVisualController lensVisualController;

    [Header("Transición")]
    [SerializeField] private float transitionDuration = 3f;

    public IEnumerator MoveToObservatory()
    {
        // Validar referencias
        if (cameraRig == null ||
            mainCamera == null ||
            cameraTargetObservatorio == null)
        {
            Debug.LogError(
                "MiradorCameraTransition: faltan referencias."
            );
            yield break;
        }

        // Ambas cámaras deben ser ortográficas
        if (!mainCamera.orthographic ||
            !cameraTargetObservatorio.orthographic)
        {
            Debug.LogError(
                "MiradorCameraTransition: ambas cámaras deben ser ortográficas."
            );
            yield break;
        }

        // Evitar que PlayerCameraFacing mueva el rig durante la transición
        if (playerCameraFacing != null)
            playerCameraFacing.enabled = false;

        // Posición y rotación iniciales del rig
        Vector3 startPosition = cameraRig.position;
        Quaternion startRotation = cameraRig.rotation;

        // Tamaño ortográfico inicial
        float startSize = mainCamera.orthographicSize;

        Quaternion targetRigRotation =
            cameraTargetObservatorio.transform.rotation *
            Quaternion.Inverse(mainCamera.transform.localRotation);

        Vector3 targetRigPosition =
            cameraTargetObservatorio.transform.position -
            targetRigRotation * mainCamera.transform.localPosition;

        // Tamaño ortográfico objetivo
        float targetSize =
            cameraTargetObservatorio.orthographicSize;

        float elapsed = 0f;

        Debug.Log("Mirador: iniciando transición de cámara.");

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / transitionDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            // Mover y rotar el rig
            cameraRig.SetPositionAndRotation(
                Vector3.Lerp(startPosition, targetRigPosition, t),
                Quaternion.Slerp(startRotation, targetRigRotation, t)
            );

            // Acercar la cámara de 8.5 a 2
            mainCamera.orthographicSize =
                Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        // Asegurar el encuadre final exacto
        cameraRig.SetPositionAndRotation(
            targetRigPosition,
            targetRigRotation
        );

        mainCamera.orthographicSize = targetSize;

        Debug.Log(
            "Mirador: transición completada. " +
            "Orthographic Size: " + mainCamera.orthographicSize
        );

        if (lensVisualController != null)
            lensVisualController.ActivateLensSequence();
    }
}
