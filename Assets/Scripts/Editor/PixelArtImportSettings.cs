#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace TheSSand.Editor
{
    public class PixelArtImportSettings
    {
        [MenuItem("The SSand/아트 설정/인게임 에셋 → 픽셀아트 Import", false, 20)]
        static void ApplyPixelArtSettings()
        {
            string[] pixelArtFolders = {
                "Assets/Art/Characters",
                "Assets/Art/Bosses",
                "Assets/Art/Projectiles",
                "Assets/Art/Items",
                "Assets/Art/Environment",
                "Assets/Art/Tilesets",
                "Assets/Art/Effects"
            };

            int count = 0;
            foreach (var folder in pixelArtFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (ApplyPixelSettings(path, GetPPU(folder)))
                        count++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[PixelArt] {count}개 에셋에 픽셀아트 Import 설정 적용 완료");
            EditorUtility.DisplayDialog("완료", $"{count}개 텍스처에 픽셀아트 설정을 적용했습니다.", "확인");
        }

        [MenuItem("The SSand/아트 설정/배경·UI → 일러스트 Import", false, 21)]
        static void ApplyIllustrationSettings()
        {
            string[] illustFolders = {
                "Assets/Art/Backgrounds",
                "Assets/Art/UI"
            };

            int count = 0;
            foreach (var folder in illustFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.maxTextureSize = 2048;
                    importer.spritePixelsPerUnit = 100;
                    importer.SaveAndReimport();
                    count++;
                }
            }

            Debug.Log($"[PixelArt] {count}개 배경/UI 에셋에 일러스트 Import 설정 적용 완료");
        }

        [MenuItem("The SSand/아트 설정/초상화 → Import", false, 22)]
        static void ApplyPortraitSettings()
        {
            string folder = "Assets/Resources/Portraits";
            if (!AssetDatabase.IsValidFolder(folder)) return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 1024;
                importer.spritePixelsPerUnit = 100;
                importer.SaveAndReimport();
            }

            Debug.Log($"[PixelArt] 초상화 Import 설정 적용 완료");
        }

        static int GetPPU(string folder)
        {
            if (folder.Contains("Tilesets")) return 16;
            if (folder.Contains("Items")) return 16;
            if (folder.Contains("Effects")) return 16;
            if (folder.Contains("Projectiles")) return 16;
            if (folder.Contains("Characters")) return 32;
            if (folder.Contains("Bosses")) return 64;
            if (folder.Contains("Environment")) return 32;
            return 16;
        }

        static bool ApplyPixelSettings(string path, int ppu)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = ppu;

            var platformSettings = importer.GetDefaultPlatformTextureSettings();
            platformSettings.format = TextureImporterFormat.RGBA32;
            platformSettings.maxTextureSize = 2048;
            importer.SetPlatformTextureSettings(platformSettings);

            importer.SaveAndReimport();
            return true;
        }
    }
}
#endif
