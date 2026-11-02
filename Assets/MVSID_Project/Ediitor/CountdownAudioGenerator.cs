
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class CountdownAudioGenerator
{
    const int SampleRate = 44100;

    [MenuItem("Tools/VR Tutorial/Generate Countdown Sounds")]
    public static void Generate()
    {
        string folder = "Assets/MSVID_Project/Audio";
        Directory.CreateDirectory(folder);

        // Short, soft electronic tick.
        float[] tick = GenerateTick(0.12f);

        // Gentle two-tone completion chime.
        float[] chime = GenerateChime(0.85f);

        SaveWav(folder + "/CountdownTick.wav", tick);
        SaveWav(folder + "/CountdownComplete.wav", chime);

        AssetDatabase.Refresh();

        Debug.Log("Countdown audio generated successfully!");
    }

    static float[] GenerateTick(float duration)
    {
        int count = Mathf.RoundToInt(duration * SampleRate);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;

            float envelope = Mathf.Exp(-45f * t);
            float attack = Mathf.Clamp01(t / 0.003f);

            float tone =
                Mathf.Sin(2f * Mathf.PI * 900f * t);

            samples[i] = tone * envelope * attack * 0.22f;
        }

        return samples;
    }

    static float[] GenerateChime(float duration)
    {
        int count = Mathf.RoundToInt(duration * SampleRate);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;

            float first = Mathf.Sin(
                2f * Mathf.PI * 523.25f * t
            );

            float second = Mathf.Sin(
                2f * Mathf.PI * 659.25f * t
            );

            float firstEnvelope = Mathf.Exp(-4f * t);

            float secondStart = Mathf.Max(0f, t - 0.16f);
            float secondEnvelope = t >= 0.16f
                ? Mathf.Exp(-5f * secondStart)
                : 0f;

            float attack = Mathf.Clamp01(t / 0.01f);

            samples[i] =
                (first * firstEnvelope * 0.16f +
                 second * secondEnvelope * 0.18f)
                * attack;
        }

        return samples;
    }

    static void SaveWav(string path, float[] samples)
    {
        using (BinaryWriter writer =
            new BinaryWriter(File.Create(path)))
        {
            int dataLength = samples.Length * 2;

            writer.Write(
                System.Text.Encoding.ASCII.GetBytes("RIFF")
            );
            writer.Write(36 + dataLength);
            writer.Write(
                System.Text.Encoding.ASCII.GetBytes("WAVE")
            );

            writer.Write(
                System.Text.Encoding.ASCII.GetBytes("fmt ")
            );
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);

            writer.Write(
                System.Text.Encoding.ASCII.GetBytes("data")
            );
            writer.Write(dataLength);

            foreach (float sample in samples)
            {
                short value = (short)(
                    Mathf.Clamp(sample, -1f, 1f) *
                    short.MaxValue
                );

                writer.Write(value);
            }
        }
    }
}
#endif
