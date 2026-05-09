using UnityEngine;

namespace LibraryGame.Gameplay
{
    /// <summary>
    /// Procedural library room with architectural details: wainscoting, crown
    /// molding, window frames, ceiling pendant lights, and PBR-tuned materials.
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

            // ── Materials ────────────────────────────────────────────────────
            var floorMat    = MakeMat(new Color(0.38f, 0.26f, 0.16f), smooth: 0.75f);  // polished dark wood
            var wallMat     = MakeMat(new Color(0.93f, 0.89f, 0.80f), smooth: 0.15f);  // warm cream plaster
            var wainMat     = MakeMat(new Color(0.82f, 0.74f, 0.60f), smooth: 0.30f);  // beige wainscoting
            var trimMat     = MakeMat(new Color(0.96f, 0.95f, 0.90f), smooth: 0.50f);  // white gloss trim
            var ceilingMat  = MakeMat(new Color(0.97f, 0.96f, 0.93f), smooth: 0.10f);
            var windowMat   = MakeMat(new Color(0.55f, 0.72f, 0.88f), smooth: 0.95f, metallic: 0.0f); // glass-ish
            var pendantMat  = MakeMat(new Color(0.15f, 0.12f, 0.10f), smooth: 0.70f, metallic: 0.8f); // dark metal

            // ── Floor ────────────────────────────────────────────────────────
            Box(root, "Floor", new Vector3(0, -WallThickness * 0.5f, 0),
                new Vector3(Width, WallThickness, Depth), floorMat);

            // Plank lines suggestion (thin inset strips across the floor)
            var plankMat = MakeMat(new Color(0.32f, 0.21f, 0.12f), smooth: 0.6f);
            for (int i = -4; i <= 4; i++)
            {
                Box(root, $"Plank_{i}", new Vector3(i * 1.1f, 0.001f, 0),
                    new Vector3(0.04f, 0.001f, Depth), plankMat);
            }

            // ── Ceiling ──────────────────────────────────────────────────────
            Box(root, "Ceiling", new Vector3(0, Height + WallThickness * 0.5f, 0),
                new Vector3(Width, WallThickness, Depth), ceilingMat);

            // Ceiling coffers (grid of shallow recessed frames)
            BuildCeilingCoffers(root, trimMat, ceilingMat);

            // ── Walls ────────────────────────────────────────────────────────
            BuildNorthWall(root, wallMat, wainMat, trimMat);
            BuildSouthWall(root, wallMat, wainMat, trimMat);
            BuildWestWall(root, wallMat, wainMat, trimMat);
            BuildEastWall(root, wallMat, wainMat, trimMat, windowMat);

            // ── Crown molding — top edge of all four walls ───────────────────
            float cx = Width * 0.5f + WallThickness;
            float cz = Depth * 0.5f + WallThickness;
            float molding = 0.08f;
            float mY = Height - molding * 0.5f;
            Box(root, "Crown_N", new Vector3(0, mY, Depth * 0.5f),           new Vector3(cx * 2, molding, molding), trimMat);
            Box(root, "Crown_S", new Vector3(0, mY, -Depth * 0.5f),          new Vector3(cx * 2, molding, molding), trimMat);
            Box(root, "Crown_W", new Vector3(-Width * 0.5f, mY, 0),          new Vector3(molding, molding, cz * 2), trimMat);
            Box(root, "Crown_E", new Vector3(Width * 0.5f, mY, 0),           new Vector3(molding, molding, cz * 2), trimMat);

            // ── Ceiling pendant lights ───────────────────────────────────────
            BuildPendant(root, new Vector3(-2.5f, Height, 0),  pendantMat, new Color(1.0f, 0.88f, 0.65f), 5.5f, 1.2f);
            BuildPendant(root, new Vector3( 2.5f, Height, 0),  pendantMat, new Color(1.0f, 0.88f, 0.65f), 5.5f, 1.2f);
            BuildPendant(root, new Vector3(0,     Height, 3f), pendantMat, new Color(1.0f, 0.90f, 0.70f), 4.5f, 1.0f);
            BuildPendant(root, new Vector3(0,     Height,-3f), pendantMat, new Color(1.0f, 0.90f, 0.70f), 4.5f, 1.0f);

            return root;
        }

        // ── Wall builders ────────────────────────────────────────────────────

        private static void BuildNorthWall(GameObject root, Material wall, Material wain, Material trim)
        {
            float wx = Width + WallThickness * 2f;
            float wainH = 1.0f;
            float wainTop = wainH + 0.04f;

            // Upper plaster
            Box(root, "WallN_Upper",
                new Vector3(0, wainTop + (Height - wainTop) * 0.5f, Depth * 0.5f + WallThickness * 0.5f),
                new Vector3(wx, Height - wainTop, WallThickness), wall);
            // Wainscoting panel
            Box(root, "WallN_Wain",
                new Vector3(0, wainH * 0.5f, Depth * 0.5f + WallThickness * 0.5f),
                new Vector3(wx, wainH, WallThickness), wain);
            // Chair rail
            Box(root, "WallN_Rail",
                new Vector3(0, wainTop, Depth * 0.5f + WallThickness * 0.5f),
                new Vector3(wx, 0.05f, WallThickness + 0.04f), trim);
            // Baseboard
            Box(root, "WallN_Base",
                new Vector3(0, 0.06f, Depth * 0.5f + WallThickness * 0.5f),
                new Vector3(wx, 0.12f, WallThickness + 0.04f), trim);
        }

        private static void BuildSouthWall(GameObject root, Material wall, Material wain, Material trim)
        {
            float wx = Width + WallThickness * 2f;
            float wainH = 1.0f;
            float wainTop = wainH + 0.04f;

            // Doorway opening in center (1.0m wide × 2.2m tall)
            float dw = 1.0f; float dh = 2.2f;

            // Upper span above door
            Box(root, "WallS_AboveDoor",
                new Vector3(0, dh + (Height - dh) * 0.5f, -Depth * 0.5f - WallThickness * 0.5f),
                new Vector3(wx, Height - dh, WallThickness), wall);
            // Left of door
            Box(root, "WallS_Left",
                new Vector3(-(Width * 0.5f + dw * 0.5f + WallThickness) * 0.5f - dw * 0.25f, Height * 0.5f, -Depth * 0.5f - WallThickness * 0.5f),
                new Vector3((Width - dw) * 0.5f, Height, WallThickness), wall);
            // Right of door
            Box(root, "WallS_Right",
                new Vector3( (Width * 0.5f + dw * 0.5f + WallThickness) * 0.5f + dw * 0.25f, Height * 0.5f, -Depth * 0.5f - WallThickness * 0.5f),
                new Vector3((Width - dw) * 0.5f, Height, WallThickness), wall);
            // Door frame trim
            Box(root, "DoorFrame_Left",  new Vector3(-dw * 0.5f - 0.05f, dh * 0.5f, -Depth * 0.5f - WallThickness * 0.5f), new Vector3(0.10f, dh, WallThickness + 0.04f), trim);
            Box(root, "DoorFrame_Right", new Vector3( dw * 0.5f + 0.05f, dh * 0.5f, -Depth * 0.5f - WallThickness * 0.5f), new Vector3(0.10f, dh, WallThickness + 0.04f), trim);
            Box(root, "DoorFrame_Top",   new Vector3(0, dh + 0.05f, -Depth * 0.5f - WallThickness * 0.5f), new Vector3(dw + 0.20f, 0.10f, WallThickness + 0.04f), trim);
            // Baseboard
            Box(root, "WallS_Base", new Vector3(0, 0.06f, -Depth * 0.5f - WallThickness * 0.5f),
                new Vector3(wx, 0.12f, WallThickness + 0.04f), trim);
        }

        private static void BuildWestWall(GameObject root, Material wall, Material wain, Material trim)
        {
            float depth = Depth + WallThickness * 2f;
            float wainH = 1.0f;
            float wainTop = wainH + 0.04f;
            float wx = -Width * 0.5f - WallThickness * 0.5f;

            Box(root, "WallW_Upper",
                new Vector3(wx, wainTop + (Height - wainTop) * 0.5f, 0),
                new Vector3(WallThickness, Height - wainTop, depth), wall);
            Box(root, "WallW_Wain",
                new Vector3(wx, wainH * 0.5f, 0),
                new Vector3(WallThickness, wainH, depth), wain);
            Box(root, "WallW_Rail", new Vector3(wx, wainTop, 0), new Vector3(WallThickness + 0.04f, 0.05f, depth), trim);
            Box(root, "WallW_Base", new Vector3(wx, 0.06f, 0), new Vector3(WallThickness + 0.04f, 0.12f, depth), trim);
        }

        private static void BuildEastWall(GameObject root, Material wall, Material wain, Material trim, Material glass)
        {
            float depth = Depth + WallThickness * 2f;
            float wainH = 1.0f;
            float wainTop = wainH + 0.04f;
            float wx = Width * 0.5f + WallThickness * 0.5f;

            // Window opening: 3m wide × 1.6m tall, bottom at 1.2m
            float winW = 3.0f, winH = 1.6f, winY = 1.2f;
            float sideZ = (Depth - winW) * 0.5f;

            // Wall sections around window
            // Above window
            Box(root, "WallE_Top",
                new Vector3(wx, winY + winH + (Height - winY - winH) * 0.5f, 0),
                new Vector3(WallThickness, Height - winY - winH, depth), wall);
            // Below window (wainscoting height)
            Box(root, "WallE_BotWain",
                new Vector3(wx, wainH * 0.5f, 0),
                new Vector3(WallThickness, wainH, depth), wain);
            // Between wainscot top and window sill
            Box(root, "WallE_BotUpper",
                new Vector3(wx, wainTop + (winY - wainTop) * 0.5f, 0),
                new Vector3(WallThickness, winY - wainTop, depth), wall);
            // Side strips (north and south of window)
            Box(root, "WallE_SideN",
                new Vector3(wx, Height * 0.5f, Depth * 0.5f - sideZ * 0.5f),
                new Vector3(WallThickness, Height, sideZ), wall);
            Box(root, "WallE_SideS",
                new Vector3(wx, Height * 0.5f, -Depth * 0.5f + sideZ * 0.5f),
                new Vector3(WallThickness, Height, sideZ), wall);

            // Window glass pane
            Box(root, "WindowGlass",
                new Vector3(wx, winY + winH * 0.5f, 0),
                new Vector3(0.04f, winH, winW), glass);

            // Window frame
            float fw = 0.06f;
            Box(root, "WinFrame_Top",    new Vector3(wx, winY + winH + fw * 0.5f, 0),         new Vector3(fw, fw, winW + fw * 2), trim);
            Box(root, "WinFrame_Bot",    new Vector3(wx, winY - fw * 0.5f, 0),                new Vector3(fw, fw, winW + fw * 2), trim);
            Box(root, "WinFrame_Left",   new Vector3(wx, winY + winH * 0.5f, winW * 0.5f + fw * 0.5f), new Vector3(fw, winH + fw * 2, fw), trim);
            Box(root, "WinFrame_Right",  new Vector3(wx, winY + winH * 0.5f, -winW * 0.5f - fw * 0.5f), new Vector3(fw, winH + fw * 2, fw), trim);
            // Mullion cross
            Box(root, "WinMullion_V",    new Vector3(wx, winY + winH * 0.5f, 0),              new Vector3(fw * 0.6f, winH, fw * 0.6f), trim);
            Box(root, "WinMullion_H",    new Vector3(wx, winY + winH * 0.5f, 0),              new Vector3(fw * 0.6f, fw * 0.6f, winW), trim);

            // Window sill
            Box(root, "WindowSill",
                new Vector3(wx + 0.1f, winY - 0.03f, 0),
                new Vector3(0.22f, 0.06f, winW + 0.2f), trim);

            // Baseboard / rail
            Box(root, "WallE_Rail", new Vector3(wx, wainTop, 0), new Vector3(WallThickness + 0.04f, 0.05f, depth), trim);
            Box(root, "WallE_Base", new Vector3(wx, 0.06f, 0),   new Vector3(WallThickness + 0.04f, 0.12f, depth), trim);
        }

        private static void BuildCeilingCoffers(GameObject root, Material trim, Material ceiling)
        {
            // 2×3 grid of shallow rectangular coffers
            float[] xs = { -2.5f, 0f, 2.5f };
            float[] zs = { -3.5f, 0f, 3.5f };
            float cw = 1.8f, cd = 2.0f, depth = 0.04f;
            float cy = Height - 0.001f;

            foreach (var x in xs)
            foreach (var z in zs)
            {
                // Outer border strips of each coffer
                Box(root, $"Coffer_{x}_{z}_N", new Vector3(x, cy, z + cd * 0.5f), new Vector3(cw, depth, 0.06f), trim);
                Box(root, $"Coffer_{x}_{z}_S", new Vector3(x, cy, z - cd * 0.5f), new Vector3(cw, depth, 0.06f), trim);
                Box(root, $"Coffer_{x}_{z}_W", new Vector3(x - cw * 0.5f, cy, z), new Vector3(0.06f, depth, cd), trim);
                Box(root, $"Coffer_{x}_{z}_E", new Vector3(x + cw * 0.5f, cy, z), new Vector3(0.06f, depth, cd), trim);
            }
        }

        /// <summary>Ceiling pendant light: cord + shade + actual Point light.</summary>
        private static void BuildPendant(GameObject root, Vector3 ceilPos, Material metal, Color lightColor, float range, float intensity)
        {
            float cordLen = 0.55f;
            float shadeR  = 0.22f;

            var cord = Box(root, "Cord", ceilPos - new Vector3(0, cordLen * 0.5f, 0),
                new Vector3(0.025f, cordLen, 0.025f), metal);

            // Shade (truncated cone approximated by a slightly tapered cube)
            var shade = Box(root, "Shade", ceilPos - new Vector3(0, cordLen + shadeR * 0.4f, 0),
                new Vector3(shadeR * 2f, shadeR * 0.9f, shadeR * 2f), metal);

            // Emissive bulb inside shade
            var bulbMat = MakeMat(lightColor, smooth: 1.0f, emissive: lightColor * 2.5f);
            Box(root, "Bulb", ceilPos - new Vector3(0, cordLen + 0.05f, 0),
                new Vector3(0.10f, 0.10f, 0.10f), bulbMat);

            // Real point light
            var lightGo = new GameObject("PendantLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.position = ceilPos - new Vector3(0, cordLen + 0.12f, 0);
            var l = lightGo.AddComponent<Light>();
            l.type      = LightType.Point;
            l.color     = lightColor;
            l.range     = range;
            l.intensity = intensity;
            l.shadows   = LightShadows.Soft;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static GameObject Box(GameObject root, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            var r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            return go;
        }

        private static Material MakeMat(Color c, float smooth = 0.2f, float metallic = 0f, Color emissive = default)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor"))    m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color"))   m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness"))   m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Metallic"))     m.SetFloat("_Metallic", metallic);
            if (emissive != default && m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emissive);
            }
            return m;
        }
    }
}
