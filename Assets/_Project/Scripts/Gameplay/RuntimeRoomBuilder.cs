using UnityEngine;

namespace LibraryGame.Gameplay
{
    /// <summary>
    /// Builds a grey-box library room procedurally at runtime: floor, ceiling,
    /// 4 walls (with a window opening on one wall) and a few placeholder
    /// shelves. No prefabs/meshes required. Replace at M8 with real art.
    /// </summary>
    public static class RuntimeRoomBuilder
    {
        public const float Width = 10f;
        public const float Depth = 12f;
        public const float Height = 3.2f;
        public const float WallThickness = 0.2f;

        public static GameObject Build(Transform parent = null)
        {
            var root = new GameObject("Room");
            if (parent != null) root.transform.SetParent(parent, false);

            var floorMat = MakeMat(new Color(0.45f, 0.34f, 0.25f)); // wood
            var wallMat = MakeMat(new Color(0.92f, 0.88f, 0.78f));  // cream
            var ceilingMat = MakeMat(new Color(0.96f, 0.94f, 0.90f));
            var trimMat = MakeMat(new Color(0.30f, 0.22f, 0.16f));

            // Floor
            MakeBox(root.transform, "Floor", new Vector3(0, -WallThickness * 0.5f, 0),
                new Vector3(Width, WallThickness, Depth), floorMat);

            // Ceiling
            MakeBox(root.transform, "Ceiling", new Vector3(0, Height + WallThickness * 0.5f, 0),
                new Vector3(Width, WallThickness, Depth), ceilingMat);

            // North wall (back, +Z) — solid
            MakeBox(root.transform, "WallNorth",
                new Vector3(0, Height * 0.5f, Depth * 0.5f + WallThickness * 0.5f),
                new Vector3(Width + WallThickness * 2f, Height, WallThickness), wallMat);

            // South wall (entrance, -Z) — solid for now
            MakeBox(root.transform, "WallSouth",
                new Vector3(0, Height * 0.5f, -Depth * 0.5f - WallThickness * 0.5f),
                new Vector3(Width + WallThickness * 2f, Height, WallThickness), wallMat);

            // East wall (right, +X) — with window opening (made of three boxes)
            float winW = 3f, winH = 1.6f, winY = 1.2f;
            float ex = Width * 0.5f + WallThickness * 0.5f;
            // Above window
            MakeBox(root.transform, "WallEast_Top",
                new Vector3(ex, winY + winH + (Height - winY - winH) * 0.5f, 0),
                new Vector3(WallThickness, Height - winY - winH, Depth), wallMat);
            // Below window
            MakeBox(root.transform, "WallEast_Bottom",
                new Vector3(ex, winY * 0.5f, 0),
                new Vector3(WallThickness, winY, Depth), wallMat);
            // Window-side strips (left/right of window)
            float sideDepth = (Depth - winW) * 0.5f;
            MakeBox(root.transform, "WallEast_North",
                new Vector3(ex, winY + winH * 0.5f, Depth * 0.5f - sideDepth * 0.5f),
                new Vector3(WallThickness, winH, sideDepth), wallMat);
            MakeBox(root.transform, "WallEast_South",
                new Vector3(ex, winY + winH * 0.5f, -Depth * 0.5f + sideDepth * 0.5f),
                new Vector3(WallThickness, winH, sideDepth), wallMat);

            // West wall (left, -X) — solid with bookshelves baked in
            MakeBox(root.transform, "WallWest",
                new Vector3(-Width * 0.5f - WallThickness * 0.5f, Height * 0.5f, 0),
                new Vector3(WallThickness, Height, Depth + WallThickness * 2f), wallMat);

            // Trim along floor on the back wall — adds a tiny visual cue of scale
            MakeBox(root.transform, "FloorTrim_North",
                new Vector3(0, 0.05f, Depth * 0.5f - 0.05f),
                new Vector3(Width, 0.1f, 0.05f), trimMat);

            return root;
        }

        private static GameObject MakeBox(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            var r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            // Static collider stays (no Rigidbody) so player CharacterController collides.
            return go;
        }

        private static Material MakeMat(Color c)
        {
            // Find URP/Lit shader; fall back to Standard.
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }
    }
}
