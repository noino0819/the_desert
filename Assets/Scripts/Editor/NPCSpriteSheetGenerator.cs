#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TheSSand.Editor
{
    /// <summary>
    /// 아직 필드 스프라이트가 없는 NPC용 플레이스홀더 스프라이트 시트를 생성한다.
    /// 실제 도트 아트로 교체할 때까지 색상 블록으로 대체하며,
    /// 그리드 규격은 SpriteAnimatorSetup의 프레임 매핑과 일치한다.
    /// </summary>
    public static class NPCSpriteSheetGenerator
    {
        struct NPCDef
        {
            public string fileName;
            public int rows, cols, frameSize;
            public Color baseColor;
            public string label;
        }

        static readonly NPCDef[] Defs =
        {
            // 기존 시트가 없는 NPC들 — 2×2 (4프레임: Idle 0,1 / Talking 2,3)
            new() { fileName = "jamjamcraft_npc.png", rows = 2, cols = 2, frameSize = 64,
                     baseColor = new Color(0.85f, 0.55f, 0.2f), label = "JJC" },
            new() { fileName = "sol_npc.png",         rows = 2, cols = 2, frameSize = 64,
                     baseColor = new Color(1f, 0.8f, 0.2f),     label = "SOL" },
            new() { fileName = "luna_npc.png",        rows = 2, cols = 2, frameSize = 64,
                     baseColor = new Color(0.4f, 0.4f, 0.85f),  label = "LUN" },
            new() { fileName = "researcher_npc.png",  rows = 2, cols = 2, frameSize = 64,
                     baseColor = new Color(0.3f, 0.7f, 0.5f),   label = "RES" },
            new() { fileName = "grandfather_npc.png", rows = 2, cols = 2, frameSize = 64,
                     baseColor = new Color(0.6f, 0.5f, 0.35f),  label = "GRA" },
            new() { fileName = "shopkeeper_npc.png",  rows = 2, cols = 2, frameSize = 64,
                     baseColor = new Color(0.9f, 0.65f, 0.4f),  label = "SHP" },
        };

        [MenuItem("The SSand/스프라이트 생성/NPC 플레이스홀더 전체 생성", false, 40)]
        static void GenerateAll()
        {
            string dir = "Assets/Art/Characters";
            EnsureDirectory(dir);

            int created = 0;
            foreach (var def in Defs)
            {
                string path = $"{dir}/{def.fileName}";
                if (File.Exists(path))
                {
                    Debug.Log($"[SpriteGen] 이미 존재 — {path}");
                    continue;
                }

                CreatePlaceholderSheet(path, def);
                created++;
            }

            // villager_npcs 가 없으면 3×4 시트도 생성
            string villagerPath = $"{dir}/villager_npcs.png";
            if (!File.Exists(villagerPath))
            {
                CreateVillagerSheet(villagerPath);
                created++;
            }

            // fairy_sprite 가 2×2이면 2×3으로 확장 (Talk 행 추가)
            // 확장은 기존 시트를 덮어쓰므로 별도 메뉴로만 제공

            AssetDatabase.Refresh();
            Debug.Log($"[SpriteGen] NPC 플레이스홀더 {created}개 생성 완료");
            EditorUtility.DisplayDialog("완료", $"{created}개 NPC 스프라이트 시트 생성.", "확인");
        }

        [MenuItem("The SSand/스프라이트 생성/Fairy 시트 확장 (Talk 추가)", false, 41)]
        static void ExpandFairySheet()
        {
            string dir = "Assets/Art/Characters";
            string path = $"{dir}/fairy_sprite.png";

            int rows = 3, cols = 2, fs = 64;
            var tex = new Texture2D(cols * fs, rows * fs, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null && existing.isReadable)
            {
                int copyH = Mathf.Min(existing.height, rows * fs);
                int copyW = Mathf.Min(existing.width, cols * fs);
                for (int y = 0; y < copyH; y++)
                    for (int x = 0; x < copyW; x++)
                        tex.SetPixel(x, y + (rows * fs - existing.height), existing.GetPixel(x, y));
            }

            Color fairyColor = new Color(0.7f, 0.9f, 1f);
            FillFrame(tex, cols, rows, fs, 4, fairyColor, 0.9f);
            FillFrame(tex, cols, rows, fs, 5, fairyColor, 0.7f);

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("[SpriteGen] fairy_sprite.png → 3×2 확장 (Talk 프레임 추가)");
        }

        static void CreatePlaceholderSheet(string path, NPCDef def)
        {
            int w = def.cols * def.frameSize;
            int h = def.rows * def.frameSize;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            ClearTexture(tex, Color.clear);

            int total = def.rows * def.cols;
            for (int i = 0; i < total; i++)
            {
                float brightness = (i % 2 == 0) ? 1f : 0.8f;
                FillFrame(tex, def.cols, def.rows, def.frameSize, i, def.baseColor, brightness);
            }

            DrawLabel(tex, def.cols, def.rows, def.frameSize, def.label);

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[SpriteGen] 생성 → {path} ({def.rows}×{def.cols})");
        }

        static void CreateVillagerSheet(string path)
        {
            int rows = 3, cols = 4, fs = 64;
            int w = cols * fs, h = rows * fs;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, Color.clear);

            Color[] villagerColors = {
                new(0.75f, 0.55f, 0.65f),
                new(0.55f, 0.7f, 0.55f),
                new(0.55f, 0.6f, 0.8f),
            };

            for (int v = 0; v < 3; v++)
            {
                int baseIdx = v * cols;
                for (int f = 0; f < cols; f++)
                {
                    float brightness = (f % 2 == 0) ? 1f : 0.8f;
                    FillFrame(tex, cols, rows, fs, baseIdx + f, villagerColors[v], brightness);
                }
            }

            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[SpriteGen] 생성 → {path} (3×4, 주민 3명)");
        }

        #region 헬퍼

        static void FillFrame(Texture2D tex, int cols, int rows, int fs, int index,
            Color baseColor, float brightness)
        {
            int col = index % cols;
            int row = index / cols;
            int px = col * fs;
            int py = (rows - 1 - row) * fs;

            Color c = baseColor * brightness;
            c.a = 1f;

            int margin = 4;
            for (int y = py + margin; y < py + fs - margin; y++)
                for (int x = px + margin; x < px + fs - margin; x++)
                    tex.SetPixel(x, y, c);

            Color outline = Color.black;
            outline.a = 0.6f;
            for (int x = px + margin; x < px + fs - margin; x++)
            {
                tex.SetPixel(x, py + margin, outline);
                tex.SetPixel(x, py + fs - margin - 1, outline);
            }
            for (int y = py + margin; y < py + fs - margin; y++)
            {
                tex.SetPixel(px + margin, y, outline);
                tex.SetPixel(px + fs - margin - 1, y, outline);
            }

            // 홀수 프레임은 1px 위로 오프셋하여 미세 움직임 표현
            if (index % 2 == 1)
            {
                Color highlight = Color.white;
                highlight.a = 0.3f;
                for (int x = px + margin + 2; x < px + fs - margin - 2; x++)
                    tex.SetPixel(x, py + fs - margin - 3, highlight);
            }
        }

        static void DrawLabel(Texture2D tex, int cols, int rows, int fs, string label)
        {
            Color labelColor = Color.white;
            labelColor.a = 0.7f;
            int cx = fs / 2 - 4;
            int cy = (rows - 1) * fs + fs / 2;
            for (int x = cx; x < cx + label.Length * 3; x++)
                tex.SetPixel(x, cy, labelColor);
        }

        static void ClearTexture(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
        }

        static void EnsureDirectory(string assetPath)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", assetPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
        }

        #endregion
    }
}
#endif
