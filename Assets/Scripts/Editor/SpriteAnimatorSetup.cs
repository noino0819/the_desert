#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace TheSSand.Editor
{
    public class SpriteAnimatorSetup
    {
        static readonly string AnimRoot = "Assets/Animations";

        [MenuItem("The SSand/애니메이션 설정/전체 생성", false, 30)]
        static void CreateAll()
        {
            EnsureFolder(AnimRoot);
            EnsureFolder($"{AnimRoot}/Hero");
            EnsureFolder($"{AnimRoot}/CaptainBoy");
            EnsureFolder($"{AnimRoot}/Fairy");
            EnsureFolder($"{AnimRoot}/BossHippo");

            SliceAndAnimate_Hero();
            SliceAndAnimate_CaptainBoy();
            SliceAndAnimate_Fairy();
            SliceAndAnimate_BossHippo();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Animator] 전체 애니메이션 생성 완료!");
            EditorUtility.DisplayDialog("완료", "4개 캐릭터 AnimatorController 생성 완료.", "확인");
        }

        [MenuItem("The SSand/애니메이션 설정/Hero")]
        static void SliceAndAnimate_Hero()
        {
            string texPath = "Assets/Art/Characters/hero_spritesheet.png";
            int cols = 4, frameW = 0, frameH = 0;

            var sprites = SliceSpriteSheet(texPath, 5, cols, out frameW, out frameH);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/Hero";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/Hero_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            var stateMap = new Dictionary<string, int[]> {
                { "Idle",  new[]{ 0, 1 } },
                { "Walk",  new[]{ 4, 5, 6, 7 } },
                { "Run",   new[]{ 8, 9, 10, 11 } },
                { "Jump",  new[]{ 12, 13 } },
                { "Fall",  new[]{ 16, 17 } },
            };

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);

            AnimatorState idleState = null;
            foreach (var kv in stateMap)
            {
                var clip = CreateClip(kv.Key, sprites, kv.Value, kv.Key == "Idle" ? 4 : 8, folder);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                if (kv.Key == "Idle") idleState = state;
            }

            if (idleState != null)
                rootSM.defaultState = idleState;

            Debug.Log("[Animator] Hero AnimatorController 생성");
        }

        [MenuItem("The SSand/애니메이션 설정/CaptainBoy")]
        static void SliceAndAnimate_CaptainBoy()
        {
            string texPath = "Assets/Art/Characters/captain_boy_npc.png";
            int frameW, frameH;
            var sprites = SliceSpriteSheet(texPath, 2, 2, out frameW, out frameH);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/CaptainBoy";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/CaptainBoy_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);

            var idleClip = CreateClip("Idle", sprites, new[]{ 0, 1 }, 4, folder);
            var talkClip = CreateClip("Talking", sprites, new[]{ 2, 3 }, 4, folder);

            var idleState = rootSM.AddState("Idle");
            idleState.motion = idleClip;
            var talkState = rootSM.AddState("Talking");
            talkState.motion = talkClip;

            rootSM.defaultState = idleState;

            var toTalk = idleState.AddTransition(talkState);
            toTalk.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
            toTalk.hasExitTime = false;
            toTalk.duration = 0;

            var toIdle = talkState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsTalking");
            toIdle.hasExitTime = false;
            toIdle.duration = 0;

            Debug.Log("[Animator] CaptainBoy AnimatorController 생성");
        }

        [MenuItem("The SSand/애니메이션 설정/Fairy")]
        static void SliceAndAnimate_Fairy()
        {
            string texPath = "Assets/Art/Characters/fairy_sprite.png";
            int frameW, frameH;
            var sprites = SliceSpriteSheet(texPath, 2, 2, out frameW, out frameH);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/Fairy";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/Fairy_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("IsSparkle", AnimatorControllerParameterType.Bool);

            var floatClip = CreateClip("Float", sprites, new[]{ 0, 1 }, 6, folder);
            var sparkleClip = CreateClip("Sparkle", sprites, new[]{ 2, 3 }, 6, folder);

            var floatState = rootSM.AddState("Float");
            floatState.motion = floatClip;
            var sparkleState = rootSM.AddState("Sparkle");
            sparkleState.motion = sparkleClip;

            rootSM.defaultState = floatState;

            var toSparkle = floatState.AddTransition(sparkleState);
            toSparkle.AddCondition(AnimatorConditionMode.If, 0, "IsSparkle");
            toSparkle.hasExitTime = false;
            toSparkle.duration = 0;

            var toFloat = sparkleState.AddTransition(floatState);
            toFloat.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSparkle");
            toFloat.hasExitTime = false;
            toFloat.duration = 0;

            Debug.Log("[Animator] Fairy AnimatorController 생성");
        }

        [MenuItem("The SSand/애니메이션 설정/BossHippo")]
        static void SliceAndAnimate_BossHippo()
        {
            string texPath = "Assets/Art/Bosses/boss_hippo.png";
            int frameW, frameH;
            var sprites = SliceSpriteSheet(texPath, 4, 4, out frameW, out frameH);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/BossHippo";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/BossHippo_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);
            controller.AddParameter("IsStunned", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDefeated", AnimatorControllerParameterType.Bool);

            var stateMap = new Dictionary<string, int[]> {
                { "Idle",        new[]{ 0, 1 } },
                { "MudAttack",   new[]{ 2, 3, 4 } },
                { "WaterAttack", new[]{ 5, 6, 7 } },
                { "Stunned",     new[]{ 8, 9 } },
                { "Defeated",    new[]{ 10, 11, 12 } },
            };

            AnimatorState idleState = null;
            foreach (var kv in stateMap)
            {
                int fps = kv.Key == "Idle" ? 3 : 6;
                var clip = CreateClip(kv.Key, sprites, kv.Value, fps, folder);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                if (kv.Key == "Idle") idleState = state;
            }

            if (idleState != null)
                rootSM.defaultState = idleState;

            Debug.Log("[Animator] BossHippo AnimatorController 생성");
        }

        #region 스프라이트 슬라이싱

        static Sprite[] SliceSpriteSheet(string texPath, int rows, int cols, out int frameW, out int frameH)
        {
            frameW = 0;
            frameH = 0;

            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[Animator] 텍스처 없음: {texPath}");
                return null;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
            {
                importer.SaveAndReimport();
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            }
            if (tex == null) return null;

            frameW = tex.width / cols;
            frameH = tex.height / rows;

            var spriteMetaData = new List<SpriteMetaData>();
            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var meta = new SpriteMetaData
                    {
                        name = $"frame_{index}",
                        rect = new Rect(c * frameW, (rows - 1 - r) * frameH, frameW, frameH),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                    spriteMetaData.Add(meta);
                    index++;
                }
            }

            importer.spritesheet = spriteMetaData.ToArray();
            importer.SaveAndReimport();

            var allAssets = AssetDatabase.LoadAllAssetsAtPath(texPath);
            var sprites = new List<Sprite>();
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sp)
                    sprites.Add(sp);
            }

            sprites.Sort((a, b) => {
                int idxA = GetFrameIndex(a.name);
                int idxB = GetFrameIndex(b.name);
                return idxA.CompareTo(idxB);
            });

            return sprites.ToArray();
        }

        static int GetFrameIndex(string name)
        {
            int underscoreIdx = name.LastIndexOf('_');
            if (underscoreIdx >= 0 && int.TryParse(name.Substring(underscoreIdx + 1), out int idx))
                return idx;
            return 0;
        }

        #endregion

        #region AnimationClip 생성

        static AnimationClip CreateClip(string clipName, Sprite[] allSprites, int[] frameIndices, int fps, string folder)
        {
            var clip = new AnimationClip();
            clip.frameRate = fps;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var keyframes = new ObjectReferenceKeyframe[frameIndices.Length];
            for (int i = 0; i < frameIndices.Length; i++)
            {
                int idx = frameIndices[i];
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = (float)i / fps,
                    value = idx < allSprites.Length ? allSprites[idx] : null
                };
            }

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            string clipPath = $"{folder}/{clipName}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);

            return clip;
        }

        #endregion

        #region 유틸

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folderName);
        }

        #endregion
    }
}
#endif
