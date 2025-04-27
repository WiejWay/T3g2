using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient2 : BaseMeshEffect
{
    public Color topColor = Color.white;
    public Color bottomColor = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        UIVertex vertex = new UIVertex();
        int count = vh.currentVertCount;
        if (count == 0)
            return;

        float bottomY = float.MaxValue;
        float topY = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float y = vertex.position.y;
            if (y > topY)
                topY = y;
            if (y < bottomY)
                bottomY = y;
        }

        float height = topY - bottomY;
        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            Color color = Color.Lerp(bottomColor, topColor, (vertex.position.y - bottomY) / height);
            vertex.color *= color;
            vh.SetUIVertex(vertex, i);
        }
    }
}
