using System;
using UnityEngine;

namespace TheSSand.Quest
{
    public enum QuestState
    {
        Inactive,
        Active,
        Completed,
        Failed
    }

    [Serializable]
    public class QuestData
    {
        public string questId;
        public string questName;
        public string description;
        public QuestState state;

        public int progress;
        public int goal;

        /// <summary>
        /// 완료 시 자동으로 세트되는 GameManager 플래그 이름 배열.
        /// </summary>
        public string[] flagsOnComplete;

        public Vector2 mapMarkerPosition;

        public bool IsCountable => goal > 0;

        public float ProgressNormalized =>
            goal > 0 ? Mathf.Clamp01((float)progress / goal) : 0f;

        public bool IsGoalReached => goal > 0 && progress >= goal;

        public void Advance(int amount = 1)
        {
            progress = Mathf.Min(progress + amount, goal);
        }
    }
}
