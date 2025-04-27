using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[AddComponentMenu("UI/Effects/CornerDarkness")]
public class CornerDarkness : BaseMeshEffect
{
    public float radius = 0.5f;  // Od jakiej odległości zaczyna się ciemnienie (0-1)
    public float intensity = 1f; // Jak mocne przyciemnienie (0-1)

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        var rect = GetComponent<RectTransform>().rect;
        var center = rect.center;

        UIVertex vertex = new UIVertex();

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // Przelicz pozycję względem środka (normalizowane 0-1)
            Vector2 normalized = new Vector2(
                (vertex.position.x - rect.x) / rect.width,
                (vertex.position.y - rect.y) / rect.height
            );

            // Odległość od środka
            float distToCenter = Vector2.Distance(normalized, new Vector2(0.5f, 0.5f));

            // Wylicz ciemność
            float t = Mathf.InverseLerp(radius, 0.707f, distToCenter); // 0.707 = sqrt(0.5) = max dystans w rogu
            float darkness = Mathf.Clamp01(t) * intensity;

            // Przekształcenie koloru
            Color originalColor = vertex.color;
            Color darkenedColor = Color.Lerp(originalColor, Color.black, darkness);
            darkenedColor.a *= (1f - darkness);

            vertex.color = darkenedColor; // Color typ pasuje
            vh.SetUIVertex(vertex, i);
        }
    }
}
