using UnityEngine;
using UnityEngine.UI;

public class ParallelogramSegment : MaskableGraphic
{
    public float width  = 28f;
    public float height = 18f;
    public float skew   = 6f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 bl = new Vector2(0,             0);
        Vector2 br = new Vector2(width,          0);
        Vector2 tr = new Vector2(width + skew,   height);
        Vector2 tl = new Vector2(skew,            height);

        Color32 c = color;
        vh.AddVert(new Vector3(bl.x, bl.y, 0), c, Vector2.zero);  // 0
        vh.AddVert(new Vector3(br.x, br.y, 0), c, Vector2.zero);  // 1
        vh.AddVert(new Vector3(tr.x, tr.y, 0), c, Vector2.zero);  // 2
        vh.AddVert(new Vector3(tl.x, tl.y, 0), c, Vector2.zero);  // 3

        vh.AddTriangle(0, 3, 2);
        vh.AddTriangle(0, 2, 1);
    }
}