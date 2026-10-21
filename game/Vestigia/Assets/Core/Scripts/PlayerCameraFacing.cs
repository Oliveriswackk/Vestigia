using UnityEngine;

public class PlayerCameraFacing : MonoBehaviour
{
    [SerializeField] private float distance = 8f;
    [SerializeField] private float height = 8f;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private Transform cameraRig;

    private Transform cameraTransform;

    void Start()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "PlayerCameraFacing: No se encontró una cámara con el tag MainCamera."
            );

            enabled = false;
            return;
        }

        cameraTransform = mainCamera.transform;

        if (playerVisual == null)
        {
            Debug.LogError(
                "PlayerCameraFacing: No se asignó Player Visual."
            );

            enabled = false;
            return;
        }

        if (cameraRig == null)
        {
            Debug.LogError(
                "PlayerCameraFacing: No se asignó Camera Rig."
            );

            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null || playerVisual == null || cameraRig == null)
            return;

        // El CameraRig sigue al Player.
        Vector3 cameraDirection = -cameraRig.forward;
        cameraDirection.y = 0f;

        if (cameraDirection.sqrMagnitude > 0.001f)
        {
            cameraDirection.Normalize();

            cameraRig.position =
                transform.position
                + cameraDirection * distance
                + Vector3.up * height;
        }

        // El Visual siempre mira hacia la cámara.
        Vector3 lookDirection =
            cameraTransform.position - playerVisual.position;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            playerVisual.rotation =
                Quaternion.LookRotation(lookDirection);
        }
    }
}