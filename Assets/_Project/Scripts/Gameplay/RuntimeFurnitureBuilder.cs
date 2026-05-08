using LibraryGame.Services.Inventory;
using UnityEngine;

namespace LibraryGame.Gameplay
{
    /// <summary>
    /// Builds a placeholder 3D mesh for a FurnitureCatalog.Item by combining
    /// primitive cubes (and a sphere/cylinder where it helps). Programmer art.
    /// </summary>
    public static class RuntimeFurnitureBuilder
    {
        public static GameObject Build(FurnitureCatalog.Item item, Transform parent)
        {
            var root = new GameObject($"Item_{item.id}");
            root.transform.SetParent(parent, false);
            var mat = MakeMat(item.color);

            switch (item.shape)
            {
                case FurnitureCatalog.Shape.Box:        BuildBox(root.transform, item, mat); break;
                case FurnitureCatalog.Shape.Shelf:      BuildShelf(root.transform, item, mat); break;
                case FurnitureCatalog.Shape.Chair:      BuildChair(root.transform, item, mat); break;
                case FurnitureCatalog.Shape.Lamp:       BuildLamp(root.transform, item, mat); break;
                case FurnitureCatalog.Shape.Tree:       BuildTree(root.transform, item, mat); break;
                case FurnitureCatalog.Shape.Rug:        BuildRug(root.transform, item, mat); break;
                case FurnitureCatalog.Shape.Painting:   BuildPainting(root.transform, item, mat); break;
            }
            return root;
        }

        private static GameObject Cube(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            return go;
        }

        private static GameObject Sphere(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            return go;
        }

        private static void BuildBox(Transform p, FurnitureCatalog.Item it, Material mat)
            => Cube(p, "Box", new Vector3(0, it.size.y * 0.5f, 0), it.size, mat);

        private static void BuildShelf(Transform p, FurnitureCatalog.Item it, Material mat)
        {
            // Frame
            Cube(p, "Frame", new Vector3(0, it.size.y * 0.5f, 0), it.size, mat);
            // 3 horizontal bands of "books" inset on the front
            var bookMatA = MakeMat(new Color(0.85f, 0.55f, 0.30f));
            var bookMatB = MakeMat(new Color(0.30f, 0.45f, 0.65f));
            for (int i = 0; i < 3; i++)
            {
                float y = 0.4f + i * 0.45f;
                var inset = it.size.z * 0.4f;
                Cube(p, $"BooksA_{i}", new Vector3(-it.size.x * 0.2f, y, inset),
                    new Vector3(it.size.x * 0.4f, 0.30f, 0.10f), bookMatA);
                Cube(p, $"BooksB_{i}", new Vector3(it.size.x * 0.2f, y, inset),
                    new Vector3(it.size.x * 0.4f, 0.30f, 0.10f), bookMatB);
            }
        }

        private static void BuildChair(Transform p, FurnitureCatalog.Item it, Material mat)
        {
            float seatH = 0.45f;
            // Seat
            Cube(p, "Seat", new Vector3(0, seatH, 0), new Vector3(it.size.x, 0.20f, it.size.z), mat);
            // Back
            Cube(p, "Back", new Vector3(0, seatH + 0.45f, -it.size.z * 0.5f + 0.1f),
                new Vector3(it.size.x, 0.9f, 0.15f), mat);
            // Armrests
            Cube(p, "ArmL", new Vector3(-it.size.x * 0.5f + 0.07f, seatH + 0.20f, 0),
                new Vector3(0.15f, 0.20f, it.size.z * 0.9f), mat);
            Cube(p, "ArmR", new Vector3(it.size.x * 0.5f - 0.07f, seatH + 0.20f, 0),
                new Vector3(0.15f, 0.20f, it.size.z * 0.9f), mat);
        }

        private static void BuildLamp(Transform p, FurnitureCatalog.Item it, Material mat)
        {
            var poleMat = MakeMat(new Color(0.20f, 0.20f, 0.22f));
            Cube(p, "Base", new Vector3(0, 0.05f, 0), new Vector3(it.size.x, 0.10f, it.size.z), poleMat);
            Cube(p, "Pole", new Vector3(0, it.size.y * 0.5f, 0), new Vector3(0.05f, it.size.y, 0.05f), poleMat);
            Sphere(p, "Bulb", new Vector3(0, it.size.y, 0), new Vector3(it.size.x, it.size.x, it.size.x), mat);

            // Add a real light inside the bulb (modest range so it doesn't blow out the room)
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(p, false);
            lightGo.transform.localPosition = new Vector3(0, it.size.y, 0);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = it.color;
            light.range = 4.5f;
            light.intensity = 0.9f;
        }

        private static void BuildTree(Transform p, FurnitureCatalog.Item it, Material mat)
        {
            var trunkMat = MakeMat(new Color(0.30f, 0.20f, 0.12f));
            Cube(p, "Pot", new Vector3(0, 0.15f, 0), new Vector3(it.size.x * 0.6f, 0.30f, it.size.z * 0.6f), trunkMat);
            Cube(p, "Trunk", new Vector3(0, 0.55f, 0), new Vector3(0.10f, 0.5f, 0.10f), trunkMat);
            // Foliage stack: three increasing-size cubes (or cones if we had them)
            float baseY = 0.85f;
            float h = it.size.y - baseY;
            int slices = 4;
            for (int i = 0; i < slices; i++)
            {
                float t = i / (float)slices;
                float y = baseY + h * t;
                float w = it.size.x * (1f - t * 0.65f);
                Cube(p, $"Leaf_{i}", new Vector3(0, y, 0), new Vector3(w, h / slices * 1.05f, w), mat);
            }
        }

        private static void BuildRug(Transform p, FurnitureCatalog.Item it, Material mat)
        {
            Cube(p, "Rug", new Vector3(0, it.size.y * 0.5f, 0), it.size, mat);
        }

        private static void BuildPainting(Transform p, FurnitureCatalog.Item it, Material mat)
        {
            var frameMat = MakeMat(new Color(0.30f, 0.20f, 0.12f));
            Cube(p, "Frame", new Vector3(0, it.size.y * 0.5f + 1.2f, 0),
                new Vector3(it.size.x, it.size.y, 0.05f), frameMat);
            Cube(p, "Canvas", new Vector3(0, it.size.y * 0.5f + 1.2f, 0.03f),
                new Vector3(it.size.x * 0.85f, it.size.y * 0.85f, 0.04f), mat);
        }

        private static Material MakeMat(Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            return m;
        }
    }
}
