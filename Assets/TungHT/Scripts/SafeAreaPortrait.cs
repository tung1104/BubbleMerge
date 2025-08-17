using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaPortrait : MonoBehaviour
{
    private RectTransform panelSafeArea;

    void Awake()
    {
        panelSafeArea = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Chuyển đổi pixel sang tỉ lệ (anchor)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panelSafeArea.anchorMin = anchorMin;
        panelSafeArea.anchorMax = anchorMax;

        Debug.Log("SafeArea applied (portrait): " + safeArea);
    }
}
