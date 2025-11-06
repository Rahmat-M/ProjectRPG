using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;
    public Vector3 offset = new Vector3(1f, 0.5f, 0f); // Geser ke kanan dan atas
    Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Tambahkan offset ke posisi target
        Vector3 targetPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z // Tetap gunakan z kamera
        );

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
