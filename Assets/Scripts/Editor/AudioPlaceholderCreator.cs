#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TheSSand.Editor
{
    public class AudioPlaceholderCreator
    {
        static readonly string BGMPath = "Assets/Resources/Audio/BGM";
        static readonly string SFXPath = "Assets/Resources/Audio/SFX";

        static readonly (string name, float freq, float duration)[] BGMClips =
        {
            ("BGM_Title",       261.63f, 8f),
            ("BGM_Prologue",    293.66f, 8f),
            ("BGM_Ch1_Desert",  329.63f, 8f),
            ("BGM_Ch1_Oasis",   349.23f, 8f),
            ("BGM_Ch1_Boss",    392.00f, 8f),
            ("BGM_Ch2_Desert",  440.00f, 8f),
            ("BGM_Ch2_Oasis",   493.88f, 8f),
            ("BGM_Ch2_Boss",    523.25f, 8f),
            ("BGM_Ch3_Desert",  587.33f, 8f),
            ("BGM_Ch3_Oasis",   659.25f, 8f),
            ("BGM_Ch3_Boss",    698.46f, 8f),
            ("BGM_Ch4_Desert",  783.99f, 8f),
            ("BGM_Ch4_Oasis",   880.00f, 8f),
            ("BGM_Ch4_Boss",    987.77f, 8f),
            ("BGM_Ending",      523.25f, 8f),
            ("BGM_NG",          440.00f, 8f),
        };

        static readonly (string name, float freq, float duration)[] SFXClips =
        {
            ("player_hit",        220f,  0.15f),
            ("player_jump",       440f,  0.10f),
            ("player_land",       180f,  0.08f),
            ("player_die",        150f,  0.50f),
            ("item_pickup",       880f,  0.12f),
            ("checkpoint",        660f,  0.25f),
            ("enemy_hit",         300f,  0.12f),
            ("enemy_die",         200f,  0.30f),
            ("shop_buy",          523f,  0.20f),
            ("shop_sell",         440f,  0.18f),
            ("SFX_BookPageTurn",  600f,  0.10f),
            ("SFX_BookBurn",      250f,  0.40f),
            ("SFX_Confirm",       784f,  0.15f),
            ("SFX_Cancel",        330f,  0.12f),
            ("SFX_Footsteps_Run", 350f,  0.06f),
            ("SFX_SeedPlant",     500f,  0.20f),
            ("boss_hit",          260f,  0.15f),
            ("boss_die",          180f,  0.60f),
            ("boss_attack",       350f,  0.20f),
            ("boss_phase_change", 700f,  0.35f),
            ("ui_click",          1000f, 0.05f),
            ("ui_hover",          1200f, 0.03f),
            ("dialogue_blip",     800f,  0.04f),
            ("save_complete",     600f,  0.30f),
        };

        [MenuItem("The SSand/오디오/플레이스홀더 전체 생성", false, 20)]
        static void CreateAllPlaceholders()
        {
            EnsureFolders();

            int count = 0;
            foreach (var (name, freq, dur) in BGMClips)
            {
                CreateWavFile($"{BGMPath}/{name}.wav", freq, dur, 0.15f);
                count++;
            }
            foreach (var (name, freq, dur) in SFXClips)
            {
                CreateWavFile($"{SFXPath}/{name}.wav", freq, dur, 0.3f);
                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Audio] 플레이스홀더 오디오 {count}개 생성 완료!");
            EditorUtility.DisplayDialog("완료", $"플레이스홀더 오디오 클립 {count}개가 생성되었습니다.\n실제 음원으로 교체해주세요.", "확인");
        }

        [MenuItem("The SSand/오디오/BGM만 생성")]
        static void CreateBGMOnly()
        {
            EnsureFolders();
            foreach (var (name, freq, dur) in BGMClips)
                CreateWavFile($"{BGMPath}/{name}.wav", freq, dur, 0.15f);
            AssetDatabase.Refresh();
            Debug.Log($"[Audio] BGM {BGMClips.Length}개 생성 완료!");
        }

        [MenuItem("The SSand/오디오/SFX만 생성")]
        static void CreateSFXOnly()
        {
            EnsureFolders();
            foreach (var (name, freq, dur) in SFXClips)
                CreateWavFile($"{SFXPath}/{name}.wav", freq, dur, 0.3f);
            AssetDatabase.Refresh();
            Debug.Log($"[Audio] SFX {SFXClips.Length}개 생성 완료!");
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Audio");
            EnsureFolder(BGMPath);
            EnsureFolder(SFXPath);
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        static void CreateWavFile(string assetPath, float frequency, float durationSec, float amplitude)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", assetPath);
            if (File.Exists(fullPath)) return;

            int sampleRate = 22050;
            int channels = 1;
            int bitsPerSample = 16;
            int sampleCount = (int)(sampleRate * durationSec);
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            int dataSize = sampleCount * blockAlign;

            using var stream = new FileStream(fullPath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            writer.Write(new char[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new char[] { 'W', 'A', 'V', 'E' });

            writer.Write(new char[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);

            writer.Write(new char[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f;
                if (t < 0.01f)
                    envelope = t / 0.01f;
                else if (t > durationSec - 0.02f)
                    envelope = (durationSec - t) / 0.02f;

                float sample = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
                short pcm = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(pcm);
            }
        }
    }
}
#endif
