using UnityEngine;
using TheSSand.Core;
using TheSSand.Card;

namespace TheSSand.Boss
{
    /// <summary>
    /// 2회차 보스 변이체 — 기존 보스에 부착하여 NG+ 강화 패턴 적용
    /// isNewGame2일 때 자동으로 보스 파라미터를 변형시킨다.
    /// </summary>
    public class NGBossModifier : MonoBehaviour
    {
        [Header("공통 NG 배율")]
        [SerializeField] float hpMultiplier = 1.5f;
        [SerializeField] float speedMultiplier = 1.3f;
        [SerializeField] float damageMultiplier = 1.2f;

        [Header("비주얼")]
        [SerializeField] SpriteRenderer bossSprite;
        [SerializeField] Color mutantTint = new(0.7f, 0.3f, 1f, 1f);
        [SerializeField] GameObject mutantParticleEffect;
        [SerializeField] Material mutantMaterial;

        [Header("하마 변이체 (Ch.1)")]
        [SerializeField] bool isHippo;
        [SerializeField] float hippoNGFireRateMultiplier = 0.6f;

        [Header("독수리 변이체 (Ch.2)")]
        [SerializeField] bool isEagle;
        [SerializeField] float eagleNGScrollMultiplier = 1.4f;
        [SerializeField] int eagleNGExtraFeathers = 3;

        [Header("늑대 변이체 (Ch.3)")]
        [SerializeField] bool isWolf;
        [SerializeField] float wolfNGBPMMultiplier = 1.3f;
        [SerializeField] bool wolfNGFakeBeats = true;

        [Header("거북이 변이체 (Ch.4)")]
        [SerializeField] bool isTurtle;
        [SerializeField] int turtleNGPoisonPerTurn = 2;
        [SerializeField] int turtleNGExtraShield = 3;

        void Awake()
        {
            if (GameManager.Instance == null || !GameManager.Instance.CurrentSave.isNewGame2)
            {
                enabled = false;
                return;
            }

            ApplyNGModifications();
        }

        void ApplyNGModifications()
        {
            ApplyVisuals();

            if (isHippo) ModifyHippo();
            if (isEagle) ModifyEagle();
            if (isWolf) ModifyWolf();
            if (isTurtle) ModifyTurtle();
        }

        void ApplyVisuals()
        {
            if (bossSprite != null)
            {
                bossSprite.color = mutantTint;
                if (mutantMaterial != null)
                    bossSprite.material = mutantMaterial;
            }

            if (mutantParticleEffect != null)
                mutantParticleEffect.SetActive(true);
        }

        void ModifyHippo()
        {
            var hippo = GetComponent<BossHippo>();
            if (hippo == null) return;

            var hpField = typeof(BossBase).GetField("maxHP",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                int baseHP = (int)hpField.GetValue(hippo);
                hpField.SetValue(hippo, Mathf.RoundToInt(baseHP * hpMultiplier));
            }
        }

        void ModifyEagle()
        {
            var eagle = GetComponent<BossEagle>();
            if (eagle == null) return;

            var hpField = typeof(BossBase).GetField("maxHP",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                int baseHP = (int)hpField.GetValue(eagle);
                hpField.SetValue(eagle, Mathf.RoundToInt(baseHP * hpMultiplier));
            }
        }

        void ModifyWolf()
        {
            var wolf = GetComponent<BossWolf>();
            if (wolf == null) return;

            var hpField = typeof(BossBase).GetField("maxHP",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                int baseHP = (int)hpField.GetValue(wolf);
                hpField.SetValue(wolf, Mathf.RoundToInt(baseHP * hpMultiplier));
            }
        }

        void ModifyTurtle()
        {
            var battleMgr = GetComponent<CardBattleManager>();
            if (battleMgr == null) return;

            var bossHPField = typeof(CardBattleManager).GetField("bossMaxHP",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (bossHPField != null)
            {
                int baseHP = (int)bossHPField.GetValue(battleMgr);
                bossHPField.SetValue(battleMgr, Mathf.RoundToInt(baseHP * hpMultiplier));
            }

            var shieldField = typeof(CardBattleManager).GetField("bossShieldPerTurn",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (shieldField != null)
            {
                int baseShield = (int)shieldField.GetValue(battleMgr);
                shieldField.SetValue(battleMgr, baseShield + turtleNGExtraShield);
            }
        }
    }
}
