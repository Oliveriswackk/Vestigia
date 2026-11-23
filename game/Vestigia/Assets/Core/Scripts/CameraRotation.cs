
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
    private bool inputEnabled = true;

    void Update()
    {
        if (!inputEnabled || isRotating || Keyboard.current == null)
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

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    private IEnumerator RotateCamera(float angle)
    {
        isRotating = true;

        PlayRotationSound();

        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation =
            startRotation * Quaternion.Euler(0f, angle, 0f);

        yield return RotateTo(startRotation, targetRotation);

        isRotating = false;
    }

    public void FaceVelador()
    {
        if (isRotating)
            StopAllCoroutines();

        StartCoroutine(RotateToVelador());
    }

    private IEnumerator RotateToVelador()
    {
        isRotating = true;

        PlayRotationSound();

        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            -180f,
            transform.eulerAngles.z
        );

        yield return RotateTo(startRotation, targetRotation);

        isRotating = false;
    }

    private IEnumerator RotateTo(
        Quaternion startRotation,
        Quaternion targetRotation)
    {
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
    }

    private void PlayRotationSound()
    {
        if (rotationAudio != null && rotationClip != null)
            rotationAudio.PlayOneShot(rotationClip);
    }
}
