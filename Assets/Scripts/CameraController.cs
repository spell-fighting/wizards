using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Smoothing = 5f; // The speed with which the camera will be following.
    public Vector3 Offset; // The initial Offset from the target.
    public Vector3 Rotation; // The initial Offset from the target.

    private Transform _target; // The position that that camera will be following.

    private void Start()
    {    
        _target = transform.parent;
    }

    private void FixedUpdate()
    {
        // Create a position the camera is aiming for based on the Offset from the target.
        var targetCamPos = _target.position + _target.transform.rotation * Offset;

        // Smoothly interpolate between the camera's current position and it's target position.
        transform.position = Vector3.Lerp(transform.position, targetCamPos, Smoothing * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation,
            _target.transform.rotation * Quaternion.Euler(Rotation), Smoothing * Time.deltaTime);
    }
}