using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fillImage;

    // Ini field yang dipakai EnemyHealth
    public Vector3 offset = new Vector3(0, 1f, 0);

    private Transform target;
    private Camera cam;

    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (cam == null)
            cam = Camera.main;

        // Posisi dunia -> screen space
        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        transform.position = screenPos;
    }

    public void SetHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
