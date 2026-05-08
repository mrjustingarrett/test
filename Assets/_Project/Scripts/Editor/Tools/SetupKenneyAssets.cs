#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LibraryGame.Editor.Tools
{
    /// <summary>
    /// One-click tool: copies Kenney furniture-kit FBX files and colormap texture
    /// from Downloaded Assets into Assets/Resources/Furniture so they can be loaded
    /// at runtime with Resources.Load.
    /// </summary>
    public static class SetupKenneyAssets
    {
        private const string SrcBase = "Assets/_Project/Art/Downloaded Assets/kenney_furniture-kit/Models/FBX format";
        private const string DstDir  = "Assets/Resources/Furniture";

        private static readonly string[] Models =
        {
            "bookcaseClosed",
            "bookcaseClosedDoors",
            "bookcaseClosedWide",
            "bookcaseOpen",
            "bookcaseOpenLow",
            "books",
            "chair",
            "chairCushion",
            "desk",
            "lampRoundFloor",
            "lampSquareFloor",
            "lampWall",
            "loungeChair",
            "loungeChairRelax",
            "plantSmall1",
            "plantSmall2",
            "plantSmall3",
            "pottedPlant",
            "rugRectangle",
            "rugRound",
            "rugSquare",
            "sideTable",
            "tableCoffee",
        };

        [MenuItem("LibraryGame/Setup Kenney Assets")]
        public static void Run()
        {
            if (!Directory.Exists(SrcBase))
            {
                EditorUtility.DisplayDialog("Setup Kenney Assets",
                    $"Source folder not found:\n{SrcBase}\n\nMake sure you have placed the kenney_furniture-kit pack at:\nAssets/_Project/Art/Downloaded Assets/",
                    "OK");
                return;
            }

            Directory.CreateDirectory(DstDir);

            // Copy texture subfolder so Unity auto-assigns materials
            CopyTextures();

            int copied = 0, skipped = 0;
            foreach (var name in Models)
            {
                var src = $"{SrcBase}/{name}.fbx";
                var dst = $"{DstDir}/{name}.fbx";

                if (!File.Exists(src))
                {
                    Debug.LogWarning($"[SetupKenneyAssets] Not found: {src}");
                    skipped++;
                    continue;
                }

                if (File.Exists(dst))
                {
                    skipped++;
                    continue;
                }

                // AssetDatabase.CopyAsset preserves import settings
                if (AssetDatabase.CopyAsset(src, dst))
                    copied++;
                else
                    Debug.LogWarning($"[SetupKenneyAssets] CopyAsset failed: {src}");
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Setup Kenney Assets",
                $"Done!\n• Copied: {copied}\n• Already present / skipped: {skipped}\n\nPress Play to see real 3D furniture in your library.",
                "OK");
        }

        private static void CopyTextures()
        {
            var srcTex = $"{SrcBase}/Textures";
            var dstTex = $"{DstDir}/Textures";
            Directory.CreateDirectory(dstTex);

            if (!Directory.Exists(srcTex))
            {
                // Some Kenney packs store the colormap next to the FBX files
                foreach (var f in Directory.GetFiles(SrcBase, "*.png"))
                {
                    var dst = Path.Combine(dstTex, Path.GetFileName(f));
                    if (!File.Exists(dst)) File.Copy(f, dst);
                }
                return;
            }

            foreach (var f in Directory.GetFiles(srcTex, "*.png"))
            {
                var dst = Path.Combine(dstTex, Path.GetFileName(f));
                if (!File.Exists(dst)) File.Copy(f, dst);
            }
        }
    }
}
#endif
