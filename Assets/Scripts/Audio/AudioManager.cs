using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minesweeper.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfxSource;
        private AudioSource _tickSource;

        private float _volume = 0.8f;
        private bool _isMuted = false;
        private bool _timerTickEnabled = false;

        private Dictionary<SoundType, AudioClip> _clips = new Dictionary<SoundType, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _tickSource = gameObject.AddComponent<AudioSource>();
            _tickSource.playOnAwake = false;

            GenerateAudioClips();
            ApplyAudioSettings();
        }

        public void InitializeSettings(float volume, bool isMuted, bool timerTick)
        {
            _volume = Mathf.Clamp01(volume);
            _isMuted = isMuted;
            _timerTickEnabled = timerTick;
            ApplyAudioSettings();
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
        }

        public void SetMute(bool isMuted)
        {
            _isMuted = isMuted;
            ApplyAudioSettings();
        }

        public void SetTimerTickEnabled(bool enabled)
        {
            _timerTickEnabled = enabled;
        }

        public float Volume => _volume;
        public bool IsMuted => _isMuted;
        public bool TimerTickEnabled => _timerTickEnabled;

        private void ApplyAudioSettings()
        {
            float activeVolume = _isMuted ? 0f : _volume;
            if (_sfxSource != null) _sfxSource.volume = activeVolume;
            if (_tickSource != null) _tickSource.volume = activeVolume * 0.4f;
        }

        public void Play(SoundType type)
        {
            if (_isMuted || _volume <= 0f) return;

            if (_clips.TryGetValue(type, out var clip) && clip != null)
            {
                _sfxSource.PlayOneShot(clip, _volume);
            }
        }

        public void PlayTimerTick()
        {
            if (_isMuted || !_timerTickEnabled || _volume <= 0f) return;

            if (_clips.TryGetValue(SoundType.TimerTick, out var clip) && clip != null)
            {
                _tickSource.PlayOneShot(clip, _volume * 0.35f);
            }
        }

        private void GenerateAudioClips()
        {
            _clips[SoundType.Click] = CreateClickClip();
            _clips[SoundType.Cascade] = CreateCascadeClip();
            _clips[SoundType.Flag] = CreateFlagClip();
            _clips[SoundType.Unflag] = CreateUnflagClip();
            _clips[SoundType.Explosion] = CreateExplosionClip();
            _clips[SoundType.Victory] = CreateVictoryClip();
            _clips[SoundType.TimerTick] = CreateTimerTickClip();
        }

        private AudioClip CreateClickClip()
        {
            int sampleRate = 44100;
            float duration = 0.05f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 90f);
                float wave = Mathf.Sin(2f * Mathf.PI * 1800f * t) * 0.7f +
                             Mathf.Sin(2f * Mathf.PI * 900f * t) * 0.3f;
                data[i] = wave * envelope;
            }

            var clip = AudioClip.Create("Click", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateFlagClip()
        {
            int sampleRate = 44100;
            float duration = 0.08f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 50f);
                float wave = Mathf.Sin(2f * Mathf.PI * 2400f * t) * 0.6f +
                             Mathf.Sin(2f * Mathf.PI * 3600f * t) * 0.4f;
                data[i] = wave * envelope;
            }

            var clip = AudioClip.Create("Flag", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateUnflagClip()
        {
            int sampleRate = 44100;
            float duration = 0.07f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 60f);
                float wave = Mathf.Sin(2f * Mathf.PI * 1400f * t) * 0.7f +
                             Mathf.Sin(2f * Mathf.PI * 700f * t) * 0.3f;
                data[i] = wave * envelope;
            }

            var clip = AudioClip.Create("Unflag", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateCascadeClip()
        {
            int sampleRate = 44100;
            float duration = 0.12f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(600f, 1500f, t / duration);
                float envelope = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
            }

            var clip = AudioClip.Create("Cascade", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateExplosionClip()
        {
            int sampleRate = 44100;
            float duration = 0.8f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];
            System.Random rnd = new System.Random(42);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 6f);
                float subBass = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 35f, t / duration) * t);
                float noise = ((float)rnd.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 12f);
                data[i] = Mathf.Clamp(subBass * 0.6f + noise * 0.4f, -1f, 1f) * envelope;
            }

            var clip = AudioClip.Create("Explosion", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateVictoryClip()
        {
            int sampleRate = 44100;
            float duration = 1.2f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            // Cyberpunk major fanfare notes: C5 (523Hz), E5 (659Hz), G5 (784Hz), C6 (1046Hz)
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };
            float noteDuration = 0.25f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Min((int)(t / noteDuration), notes.Length - 1);
                float noteT = t - (noteIndex * noteDuration);
                float freq = notes[noteIndex];

                float env = (noteIndex == notes.Length - 1) ?
                            Mathf.Exp(-noteT * 3f) :
                            Mathf.Exp(-noteT * 7f);

                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f +
                             Mathf.Sin(2f * Mathf.PI * (freq * 2f) * t) * 0.25f;

                data[i] = wave * env * 0.5f;
            }

            var clip = AudioClip.Create("Victory", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateTimerTickClip()
        {
            int sampleRate = 44100;
            float duration = 0.02f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 200f);
                data[i] = Mathf.Sin(2f * Mathf.PI * 1200f * t) * envelope * 0.3f;
            }

            var clip = AudioClip.Create("Tick", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
