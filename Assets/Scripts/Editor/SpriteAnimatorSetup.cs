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

        #region 메뉴 — 전체 생성

        [MenuItem("The SSand/애니메이션 설정/전체 생성", false, 30)]
        static void CreateAll()
        {
            EnsureFolder(AnimRoot);

            SliceAndAnimate_Hero();
            SliceAndAnimate_CaptainBoy();
            SliceAndAnimate_Fairy();
            SliceAndAnimate_Villagers();
            SliceAndAnimate_Jamjamcraft();
            SliceAndAnimate_Sol();
            SliceAndAnimate_Luna();
            SliceAndAnimate_Researcher();
            SliceAndAnimate_Grandfather();
            SliceAndAnimate_ShopKeeper();
            SliceAndAnimate_BossHippo();
            SliceAndAnimate_BossEagle();
            SliceAndAnimate_BossWolf();
            SliceAndAnimate_BossTurtle();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Animator] 전체 애니메이션 생성 완료!");
            EditorUtility.DisplayDialog("완료", "14개 캐릭터/NPC/보스 AnimatorController 생성 완료.", "확인");
        }

        #endregion

        #region Hero — 5×4 시트 (Idle, Walk, Run, Jump, Hit, Fall, Dash, WallSlide, Death)

        [MenuItem("The SSand/애니메이션 설정/Hero")]
        static void SliceAndAnimate_Hero()
        {
            // hero_spritesheet.png 레이아웃 (5행 × 4열 = 20프레임)
            // Row 0: [0] Idle_0  [1] Idle_1  [2] Hit_0   [3] Hit_1
            // Row 1: [4] Walk_0  [5] Walk_1  [6] Walk_2  [7] Walk_3
            // Row 2: [8] Run_0   [9] Run_1  [10] Run_2  [11] Run_3
            // Row 3: [12] Jump_0 [13] Jump_1 [14] Dash_0 [15] WallSlide_0
            // Row 4: [16] Fall_0 [17] Fall_1 [18] Death_0 [19] Death_1
            string texPath = "Assets/Art/Characters/hero_spritesheet.png";
            var sprites = SliceSpriteSheet(texPath, 5, 4, out _, out _);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/Hero";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/Hero_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsHit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDashing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsWallSliding", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsDead", AnimatorControllerParameterType.Trigger);

            var stateMap = new Dictionary<string, (int[] frames, int fps, bool loop)> {
                { "Idle",      (new[]{ 0, 1 },             4,  true)  },
                { "Walk",      (new[]{ 4, 5, 6, 7 },       8,  true)  },
                { "Run",       (new[]{ 8, 9, 10, 11 },     8,  true)  },
                { "Jump",      (new[]{ 12, 13 },            8,  false) },
                { "Fall",      (new[]{ 16, 17 },            8,  true)  },
                { "Hit",       (new[]{ 2, 3 },              6,  false) },
                { "Dash",      (new[]{ 14 },                8,  false) },
                { "WallSlide", (new[]{ 15 },                8,  true)  },
                { "Death",     (new[]{ 18, 19 },            4,  false) },
            };

            var states = new Dictionary<string, AnimatorState>();
            foreach (var kv in stateMap)
            {
                var clip = CreateClip(kv.Key, sprites, kv.Value.frames, kv.Value.fps, folder, kv.Value.loop);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                states[kv.Key] = state;
            }

            rootSM.defaultState = states["Idle"];

            // Idle → Walk (속도 > 0.1)
            AddTransition(states["Idle"], states["Walk"],
                ("Speed", AnimatorConditionMode.Greater, 0.1f));
            // Walk → Idle (속도 ≤ 0.1)
            AddTransition(states["Walk"], states["Idle"],
                ("Speed", AnimatorConditionMode.Less, 0.1f));
            // Walk → Run (속도 > 5)
            AddTransition(states["Walk"], states["Run"],
                ("Speed", AnimatorConditionMode.Greater, 5f));
            // Run → Walk (속도 ≤ 5)
            AddTransition(states["Run"], states["Walk"],
                ("Speed", AnimatorConditionMode.Less, 5f));

            // 지면 → Jump (공중 + 상승)
            AddTransition(states["Idle"], states["Jump"],
                ("IsGrounded", AnimatorConditionMode.IfNot, 0),
                ("VelocityY", AnimatorConditionMode.Greater, 0.1f));
            AddTransition(states["Walk"], states["Jump"],
                ("IsGrounded", AnimatorConditionMode.IfNot, 0),
                ("VelocityY", AnimatorConditionMode.Greater, 0.1f));
            AddTransition(states["Run"], states["Jump"],
                ("IsGrounded", AnimatorConditionMode.IfNot, 0),
                ("VelocityY", AnimatorConditionMode.Greater, 0.1f));

            // Jump → Fall (하강)
            AddTransition(states["Jump"], states["Fall"],
                ("VelocityY", AnimatorConditionMode.Less, -0.1f));
            // Fall → Idle (착지)
            AddTransition(states["Fall"], states["Idle"],
                ("IsGrounded", AnimatorConditionMode.If, 0));

            // Dash
            AddTransition(states["Idle"], states["Dash"],
                ("IsDashing", AnimatorConditionMode.If, 0));
            AddTransition(states["Walk"], states["Dash"],
                ("IsDashing", AnimatorConditionMode.If, 0));
            AddTransition(states["Run"], states["Dash"],
                ("IsDashing", AnimatorConditionMode.If, 0));
            AddTransition(states["Dash"], states["Idle"],
                ("IsDashing", AnimatorConditionMode.IfNot, 0));

            // WallSlide
            AddTransition(states["Fall"], states["WallSlide"],
                ("IsWallSliding", AnimatorConditionMode.If, 0));
            AddTransition(states["WallSlide"], states["Fall"],
                ("IsWallSliding", AnimatorConditionMode.IfNot, 0));

            // Any → Hit (트리거)
            var toHit = rootSM.AddAnyStateTransition(states["Hit"]);
            toHit.AddCondition(AnimatorConditionMode.If, 0, "IsHit");
            toHit.hasExitTime = false;
            toHit.duration = 0;
            // Hit → Idle (exitTime)
            var hitToIdle = states["Hit"].AddTransition(states["Idle"]);
            hitToIdle.hasExitTime = true;
            hitToIdle.exitTime = 1f;
            hitToIdle.duration = 0;

            // Any → Death (트리거)
            var toDeath = rootSM.AddAnyStateTransition(states["Death"]);
            toDeath.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
            toDeath.hasExitTime = false;
            toDeath.duration = 0;

            Debug.Log("[Animator] Hero AnimatorController 생성 (9개 상태)");
        }

        #endregion

        #region CaptainBoy — 2×2 시트

        [MenuItem("The SSand/애니메이션 설정/CaptainBoy")]
        static void SliceAndAnimate_CaptainBoy()
        {
            string texPath = "Assets/Art/Characters/captain_boy_npc.png";
            var sprites = SliceSpriteSheet(texPath, 2, 2, out _, out _);
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

            AddTransition(idleState, talkState,
                ("IsTalking", AnimatorConditionMode.If, 0));
            AddTransition(talkState, idleState,
                ("IsTalking", AnimatorConditionMode.IfNot, 0));

            Debug.Log("[Animator] CaptainBoy AnimatorController 생성");
        }

        #endregion

        #region Fairy — 3×2 시트 (Float, Sparkle, Talk)

        [MenuItem("The SSand/애니메이션 설정/Fairy")]
        static void SliceAndAnimate_Fairy()
        {
            // fairy_sprite.png 레이아웃 (3행 × 2열 = 6프레임)
            // Row 0: [0] Float_0  [1] Float_1
            // Row 1: [2] Sparkle_0 [3] Sparkle_1
            // Row 2: [4] Talk_0   [5] Talk_1
            string texPath = "Assets/Art/Characters/fairy_sprite.png";
            var sprites = SliceSpriteSheet(texPath, 3, 2, out _, out _);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/Fairy";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/Fairy_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("IsSparkle", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Talk", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);

            var floatClip = CreateClip("Float", sprites, new[]{ 0, 1 }, 6, folder);
            var sparkleClip = CreateClip("Sparkle", sprites, new[]{ 2, 3 }, 6, folder);
            var talkClip = CreateClip("Talk", sprites, new[]{ 4, 5 }, 6, folder);

            var floatState = rootSM.AddState("Float");
            floatState.motion = floatClip;
            var sparkleState = rootSM.AddState("Sparkle");
            sparkleState.motion = sparkleClip;
            var talkState = rootSM.AddState("Talk");
            talkState.motion = talkClip;

            rootSM.defaultState = floatState;

            AddTransition(floatState, sparkleState,
                ("IsSparkle", AnimatorConditionMode.If, 0));
            AddTransition(sparkleState, floatState,
                ("IsSparkle", AnimatorConditionMode.IfNot, 0));

            // Talk 트리거 (SavePoint에서 SetTrigger("Talk") 호출)
            var toTalk = rootSM.AddAnyStateTransition(talkState);
            toTalk.AddCondition(AnimatorConditionMode.If, 0, "Talk");
            toTalk.hasExitTime = false;
            toTalk.duration = 0;

            // IsTalking Bool로도 진입 가능 (NPCInteractable 연동)
            AddTransition(floatState, talkState,
                ("IsTalking", AnimatorConditionMode.If, 0));
            AddTransition(talkState, floatState,
                ("IsTalking", AnimatorConditionMode.IfNot, 0));

            // Talk 트리거 후 자동 복귀
            var talkToFloat = talkState.AddTransition(floatState);
            talkToFloat.hasExitTime = true;
            talkToFloat.exitTime = 1f;
            talkToFloat.duration = 0;

            Debug.Log("[Animator] Fairy AnimatorController 생성 (3개 상태)");
        }

        #endregion

        #region BossHippo — 4×4 시트 (Idle, MudAttack, WaterAttack, Stunned, Hit, Defeated)

        [MenuItem("The SSand/애니메이션 설정/BossHippo")]
        static void SliceAndAnimate_BossHippo()
        {
            // boss_hippo.png 레이아웃 (4행 × 4열 = 16프레임)
            // Row 0: [0] Idle_0  [1] Idle_1  [2] MudAtk_0  [3] MudAtk_1
            // Row 1: [4] MudAtk_2 [5] WaterAtk_0 [6] WaterAtk_1 [7] WaterAtk_2
            // Row 2: [8] Stunned_0 [9] Stunned_1 [10] Hit_0 [11] Hit_1
            // Row 3: [12] Defeated_0 [13] Defeated_1 [14] Defeated_2 [15] (빈칸)
            string texPath = "Assets/Art/Bosses/boss_hippo.png";
            var sprites = SliceSpriteSheet(texPath, 4, 4, out _, out _);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/BossHippo";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/BossHippo_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);
            controller.AddParameter("IsStunned", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsHit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDefeated", AnimatorControllerParameterType.Bool);

            var stateMap = new Dictionary<string, (int[] frames, int fps, bool loop)> {
                { "Idle",        (new[]{ 0, 1 },          3,  true)  },
                { "MudAttack",   (new[]{ 2, 3, 4 },       6,  false) },
                { "WaterAttack", (new[]{ 5, 6, 7 },       6,  false) },
                { "Stunned",     (new[]{ 8, 9 },          3,  true)  },
                { "Hit",         (new[]{ 10, 11 },        6,  false) },
                { "Defeated",    (new[]{ 12, 13, 14 },    4,  false) },
            };

            var states = new Dictionary<string, AnimatorState>();
            foreach (var kv in stateMap)
            {
                var clip = CreateClip(kv.Key, sprites, kv.Value.frames, kv.Value.fps, folder, kv.Value.loop);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                states[kv.Key] = state;
            }

            rootSM.defaultState = states["Idle"];

            // Idle → Attack (AttackType 1=Mud, 2=Water)
            AddTransition(states["Idle"], states["MudAttack"],
                ("AttackType", AnimatorConditionMode.Equals, 1));
            AddTransition(states["Idle"], states["WaterAttack"],
                ("AttackType", AnimatorConditionMode.Equals, 2));
            // Attack → Idle (exitTime)
            var mudToIdle = states["MudAttack"].AddTransition(states["Idle"]);
            mudToIdle.hasExitTime = true; mudToIdle.exitTime = 1f; mudToIdle.duration = 0;
            var waterToIdle = states["WaterAttack"].AddTransition(states["Idle"]);
            waterToIdle.hasExitTime = true; waterToIdle.exitTime = 1f; waterToIdle.duration = 0;

            // Stunned
            AddTransition(states["Idle"], states["Stunned"],
                ("IsStunned", AnimatorConditionMode.If, 0));
            AddTransition(states["Stunned"], states["Idle"],
                ("IsStunned", AnimatorConditionMode.IfNot, 0));

            // Any → Hit (트리거)
            var toHit = rootSM.AddAnyStateTransition(states["Hit"]);
            toHit.AddCondition(AnimatorConditionMode.If, 0, "IsHit");
            toHit.hasExitTime = false; toHit.duration = 0;
            var hitToIdle = states["Hit"].AddTransition(states["Idle"]);
            hitToIdle.hasExitTime = true; hitToIdle.exitTime = 1f; hitToIdle.duration = 0;

            // Defeated
            AddTransition(states["Idle"], states["Defeated"],
                ("IsDefeated", AnimatorConditionMode.If, 0));
            AddTransition(states["Stunned"], states["Defeated"],
                ("IsDefeated", AnimatorConditionMode.If, 0));

            Debug.Log("[Animator] BossHippo AnimatorController 생성 (6개 상태)");
        }

        #endregion

        #region BossEagle — 4×4 시트 (Hover, DiveWarning, Dive, FeatherAttack, WindAttack, Hit, Defeated)

        [MenuItem("The SSand/애니메이션 설정/BossEagle")]
        static void SliceAndAnimate_BossEagle()
        {
            // boss_eagle.png 레이아웃 (4행 × 4열 = 16프레임)
            // Row 0: [0] Hover_0 [1] Hover_1 [2] Hover_2 [3] DiveWarning_0
            // Row 1: [4] Dive_0  [5] Dive_1  [6] Dive_2  [7] DiveReturn_0
            // Row 2: [8] FeatherAtk_0 [9] FeatherAtk_1 [10] WindAtk_0 [11] WindAtk_1
            // Row 3: [12] Hit_0 [13] Hit_1  [14] Defeated_0 [15] Defeated_1
            string texPath = "Assets/Art/Bosses/boss_eagle.png";
            var sprites = SliceSpriteSheet(texPath, 4, 4, out _, out _);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/BossEagle";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/BossEagle_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("IsDiving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);
            controller.AddParameter("IsHit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDefeated", AnimatorControllerParameterType.Bool);

            var stateMap = new Dictionary<string, (int[] frames, int fps, bool loop)> {
                { "Hover",         (new[]{ 0, 1, 2 },     6, true)  },
                { "DiveWarning",   (new[]{ 3 },           6, false) },
                { "Dive",          (new[]{ 4, 5, 6 },     10, false) },
                { "DiveReturn",    (new[]{ 7 },           6, false) },
                { "FeatherAttack", (new[]{ 8, 9 },        8, false) },
                { "WindAttack",    (new[]{ 10, 11 },      6, false) },
                { "Hit",           (new[]{ 12, 13 },      6, false) },
                { "Defeated",      (new[]{ 14, 15 },      4, false) },
            };

            var states = new Dictionary<string, AnimatorState>();
            foreach (var kv in stateMap)
            {
                var clip = CreateClip(kv.Key, sprites, kv.Value.frames, kv.Value.fps, folder, kv.Value.loop);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                states[kv.Key] = state;
            }

            rootSM.defaultState = states["Hover"];

            // Hover → DiveWarning → Dive → DiveReturn → Hover
            AddTransition(states["Hover"], states["DiveWarning"],
                ("IsDiving", AnimatorConditionMode.If, 0));
            var warnToDive = states["DiveWarning"].AddTransition(states["Dive"]);
            warnToDive.hasExitTime = true; warnToDive.exitTime = 1f; warnToDive.duration = 0;
            var diveToReturn = states["Dive"].AddTransition(states["DiveReturn"]);
            diveToReturn.hasExitTime = true; diveToReturn.exitTime = 1f; diveToReturn.duration = 0;
            AddTransition(states["DiveReturn"], states["Hover"],
                ("IsDiving", AnimatorConditionMode.IfNot, 0));

            // Hover → FeatherAttack (AttackType=1), WindAttack (AttackType=2)
            AddTransition(states["Hover"], states["FeatherAttack"],
                ("AttackType", AnimatorConditionMode.Equals, 1));
            AddTransition(states["Hover"], states["WindAttack"],
                ("AttackType", AnimatorConditionMode.Equals, 2));
            var featherToHover = states["FeatherAttack"].AddTransition(states["Hover"]);
            featherToHover.hasExitTime = true; featherToHover.exitTime = 1f; featherToHover.duration = 0;
            var windToHover = states["WindAttack"].AddTransition(states["Hover"]);
            windToHover.hasExitTime = true; windToHover.exitTime = 1f; windToHover.duration = 0;

            // Any → Hit
            var toHit = rootSM.AddAnyStateTransition(states["Hit"]);
            toHit.AddCondition(AnimatorConditionMode.If, 0, "IsHit");
            toHit.hasExitTime = false; toHit.duration = 0;
            var hitToHover = states["Hit"].AddTransition(states["Hover"]);
            hitToHover.hasExitTime = true; hitToHover.exitTime = 1f; hitToHover.duration = 0;

            // Any → Defeated
            var toDefeated = rootSM.AddAnyStateTransition(states["Defeated"]);
            toDefeated.AddCondition(AnimatorConditionMode.If, 0, "IsDefeated");
            toDefeated.hasExitTime = false; toDefeated.duration = 0;

            Debug.Log("[Animator] BossEagle AnimatorController 생성 (8개 상태)");
        }

        #endregion

        #region BossWolf — 4×4 시트 (Idle, ClawSlash, Bite, Howl, Spin, Hit, Defeated)

        [MenuItem("The SSand/애니메이션 설정/BossWolf")]
        static void SliceAndAnimate_BossWolf()
        {
            // boss_wolf.png 레이아웃 (4행 × 4열 = 16프레임)
            // Row 0: [0] Idle_0  [1] Idle_1  [2] ClawSlash_0 [3] ClawSlash_1
            // Row 1: [4] ClawSlash_2 [5] Bite_0 [6] Bite_1 [7] Bite_2
            // Row 2: [8] Howl_0  [9] Howl_1  [10] Spin_0  [11] Spin_1
            // Row 3: [12] Hit_0  [13] Hit_1  [14] Defeated_0 [15] Defeated_1
            string texPath = "Assets/Art/Bosses/boss_wolf.png";
            var sprites = SliceSpriteSheet(texPath, 4, 4, out _, out _);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/BossWolf";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/BossWolf_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);
            controller.AddParameter("IsSpinning", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsHit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDefeated", AnimatorControllerParameterType.Bool);

            var stateMap = new Dictionary<string, (int[] frames, int fps, bool loop)> {
                { "Idle",      (new[]{ 0, 1 },          4,  true)  },
                { "ClawSlash", (new[]{ 2, 3, 4 },       8,  false) },
                { "Bite",      (new[]{ 5, 6, 7 },       8,  false) },
                { "Howl",      (new[]{ 8, 9 },          4,  false) },
                { "Spin",      (new[]{ 10, 11 },        10, true)  },
                { "Hit",       (new[]{ 12, 13 },        6,  false) },
                { "Defeated",  (new[]{ 14, 15 },        4,  false) },
            };

            var states = new Dictionary<string, AnimatorState>();
            foreach (var kv in stateMap)
            {
                var clip = CreateClip(kv.Key, sprites, kv.Value.frames, kv.Value.fps, folder, kv.Value.loop);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                states[kv.Key] = state;
            }

            rootSM.defaultState = states["Idle"];

            // Idle → ClawSlash (AttackType=1), Bite (AttackType=2), Howl (AttackType=3)
            AddTransition(states["Idle"], states["ClawSlash"],
                ("AttackType", AnimatorConditionMode.Equals, 1));
            AddTransition(states["Idle"], states["Bite"],
                ("AttackType", AnimatorConditionMode.Equals, 2));
            AddTransition(states["Idle"], states["Howl"],
                ("AttackType", AnimatorConditionMode.Equals, 3));

            var clawToIdle = states["ClawSlash"].AddTransition(states["Idle"]);
            clawToIdle.hasExitTime = true; clawToIdle.exitTime = 1f; clawToIdle.duration = 0;
            var biteToIdle = states["Bite"].AddTransition(states["Idle"]);
            biteToIdle.hasExitTime = true; biteToIdle.exitTime = 1f; biteToIdle.duration = 0;
            var howlToIdle = states["Howl"].AddTransition(states["Idle"]);
            howlToIdle.hasExitTime = true; howlToIdle.exitTime = 1f; howlToIdle.duration = 0;

            // Spin (Phase 3 원형 회전)
            AddTransition(states["Idle"], states["Spin"],
                ("IsSpinning", AnimatorConditionMode.If, 0));
            AddTransition(states["Spin"], states["Idle"],
                ("IsSpinning", AnimatorConditionMode.IfNot, 0));

            // Any → Hit
            var toHit = rootSM.AddAnyStateTransition(states["Hit"]);
            toHit.AddCondition(AnimatorConditionMode.If, 0, "IsHit");
            toHit.hasExitTime = false; toHit.duration = 0;
            var hitToIdle = states["Hit"].AddTransition(states["Idle"]);
            hitToIdle.hasExitTime = true; hitToIdle.exitTime = 1f; hitToIdle.duration = 0;

            // Any → Defeated
            var toDefeated = rootSM.AddAnyStateTransition(states["Defeated"]);
            toDefeated.AddCondition(AnimatorConditionMode.If, 0, "IsDefeated");
            toDefeated.hasExitTime = false; toDefeated.duration = 0;

            Debug.Log("[Animator] BossWolf AnimatorController 생성 (7개 상태)");
        }

        #endregion

        #region BossTurtle — 4×4 시트 (Idle, Attack, Shielded, ShieldBreak, Hit, Defeated)

        [MenuItem("The SSand/애니메이션 설정/BossTurtle")]
        static void SliceAndAnimate_BossTurtle()
        {
            // boss_turtle.png 레이아웃 (4행 × 4열 = 16프레임)
            // Row 0: [0] Idle_0 [1] Idle_1 [2] Attack_0 [3] Attack_1
            // Row 1: [4] Attack_2 [5] Shielded_0 [6] Shielded_1 [7] ShieldBreak_0
            // Row 2: [8] ShieldBreak_1 [9] Hit_0  [10] Hit_1  [11] (빈칸)
            // Row 3: [12] Defeated_0 [13] Defeated_1 [14] Defeated_2 [15] (빈칸)
            string texPath = "Assets/Art/Bosses/boss_turtle.png";
            var sprites = SliceSpriteSheet(texPath, 4, 4, out _, out _);
            if (sprites == null) return;

            string folder = $"{AnimRoot}/BossTurtle";
            EnsureFolder(folder);

            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{folder}/BossTurtle_AC.controller");
            var rootSM = controller.layers[0].stateMachine;

            controller.AddParameter("Phase", AnimatorControllerParameterType.Int);
            controller.AddParameter("HPRatio", AnimatorControllerParameterType.Float);
            controller.AddParameter("Shielded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsHit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsDefeated", AnimatorControllerParameterType.Bool);

            var stateMap = new Dictionary<string, (int[] frames, int fps, bool loop)> {
                { "Idle",         (new[]{ 0, 1 },          4,  true)  },
                { "Attack",       (new[]{ 2, 3, 4 },       6,  false) },
                { "Shielded",     (new[]{ 5, 6 },          3,  true)  },
                { "ShieldBreak",  (new[]{ 7, 8 },          6,  false) },
                { "Hit",          (new[]{ 9, 10 },         6,  false) },
                { "Defeated",     (new[]{ 12, 13, 14 },    4,  false) },
            };

            var states = new Dictionary<string, AnimatorState>();
            foreach (var kv in stateMap)
            {
                var clip = CreateClip(kv.Key, sprites, kv.Value.frames, kv.Value.fps, folder, kv.Value.loop);
                var state = rootSM.AddState(kv.Key);
                state.motion = clip;
                states[kv.Key] = state;
            }

            rootSM.defaultState = states["Idle"];

            // Idle → Attack
            AddTransition(states["Idle"], states["Attack"],
                ("IsAttacking", AnimatorConditionMode.If, 0));
            var atkToIdle = states["Attack"].AddTransition(states["Idle"]);
            atkToIdle.hasExitTime = true; atkToIdle.exitTime = 1f; atkToIdle.duration = 0;

            // Idle ↔ Shielded
            AddTransition(states["Idle"], states["Shielded"],
                ("Shielded", AnimatorConditionMode.If, 0));
            AddTransition(states["Shielded"], states["ShieldBreak"],
                ("Shielded", AnimatorConditionMode.IfNot, 0));
            var breakToIdle = states["ShieldBreak"].AddTransition(states["Idle"]);
            breakToIdle.hasExitTime = true; breakToIdle.exitTime = 1f; breakToIdle.duration = 0;

            // Any → Hit
            var toHit = rootSM.AddAnyStateTransition(states["Hit"]);
            toHit.AddCondition(AnimatorConditionMode.If, 0, "IsHit");
            toHit.hasExitTime = false; toHit.duration = 0;
            var hitToIdle = states["Hit"].AddTransition(states["Idle"]);
            hitToIdle.hasExitTime = true; hitToIdle.exitTime = 1f; hitToIdle.duration = 0;

            // Any → Defeated
            var toDefeated = rootSM.AddAnyStateTransition(states["Defeated"]);
            toDefeated.AddCondition(AnimatorConditionMode.If, 0, "IsDefeated");
            toDefeated.hasExitTime = false; toDefeated.duration = 0;

            Debug.Log("[Animator] BossTurtle AnimatorController 생성 (6개 상태)");
        }

        #endregion

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

        static AnimationClip CreateClip(string clipName, Sprite[] allSprites, int[] frameIndices, int fps, string folder, bool loop = true)
        {
            var clip = new AnimationClip();
            clip.frameRate = fps;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
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

        #region 트랜지션 헬퍼

        static void AddTransition(AnimatorState from, AnimatorState to,
            params (string param, AnimatorConditionMode mode, float threshold)[] conditions)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0;

            foreach (var (param, mode, threshold) in conditions)
                transition.AddCondition(mode, threshold, param);
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
