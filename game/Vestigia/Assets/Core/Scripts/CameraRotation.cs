
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraRotation : MonoBehaviour
{
    [Header("Giro de cámara")]
    [SerializeField] private float rotationDuration = 0.4f;

    [Header("Sonido del giro")]
    [SerializeField] private AudioSource rotationAudio;
    [SerializeField] private AudioClip rotationClip;

    private bool isRotating = false;

    void Update()
    {
        if (isRotating || Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            StartCoroutine(RotateCamera(-90f));
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(RotateCamera(90f));
        }
    }

    private IEnumerator RotateCamera(float angle)
    {
        isRotating = true;

        // Reproducir el sonido al comenzar el giro.
        if (rotationAudio != null && rotationClip != null)
        {
            rotationAudio.PlayOneShot(rotationClip);
        }

        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation =
            startRotation * Quaternion.Euler(0f, angle, 0f);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / rotationDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }
}
