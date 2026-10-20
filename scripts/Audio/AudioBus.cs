using System.Collections.Generic;
using Godot;

namespace GhettoBeasts.Audio
{
    /// <summary>
    /// Project AudioManager (autoload name: AudioBus).
    /// Master / Music / SFX buses, BGM crossfade, optional Spatial3D helpers.
    /// Prefer licensed/free files under res://assets/audio/; else procedural blips (see OPEN_SOURCE.md).
    /// </summary>
    public partial class AudioBus : Node
    {
        /// <summary>Alias for call sites that prefer the AudioManager name.</summary>
        public static AudioBus? AudioManager => Instance;
        public static AudioBus? Instance { get; private set; }

        private readonly Dictionary<string, AudioStreamPlayer> players = new();
        private AudioStreamPlayer? musicA;
        private AudioStreamPlayer? musicB;
        private bool musicOnA = true;
        private Tween? crossfadeTween;

        private static readonly Dictionary<string, string> FileHints = new()
        {
            ["hit"] = "res://assets/audio/hit.ogg",
            ["summon"] = "res://assets/audio/summon.ogg",
            ["raid"] = "res://assets/audio/raid_siren.ogg",
            ["ui"] = "res://assets/audio/ui_click.ogg",
            ["pack"] = "res://assets/audio/pack_open.ogg",
            ["whoosh"] = "res://assets/audio/whoosh.ogg"
        };

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            EnsureBuses();
            musicA = new AudioStreamPlayer { Name = "MusicA", VolumeDb = -12f, Bus = "Music" };
            musicB = new AudioStreamPlayer { Name = "MusicB", VolumeDb = -80f, Bus = "Music" };
            AddChild(musicA);
            AddChild(musicB);
            TryAssignStream(musicA, "res://assets/audio/bgm_district.ogg");

            foreach (var key in FileHints.Keys)
            {
                var player = new AudioStreamPlayer { Name = $"Sfx_{key}", Bus = "SFX" };
                AddChild(player);
                if (!TryAssignStream(player, FileHints[key]))
                    player.Stream = MakeBlip(key);
                players[key] = player;
            }
        }

        public static void ApplyVolumes(float master, float music, float sfx)
        {
            int masterIdx = AudioServer.GetBusIndex("Master");
            int musicIdx = AudioServer.GetBusIndex("Music");
            int sfxIdx = AudioServer.GetBusIndex("SFX");
            if (masterIdx >= 0) AudioServer.SetBusVolumeDb(masterIdx, LinearToDb(master));
            if (musicIdx >= 0) AudioServer.SetBusVolumeDb(musicIdx, LinearToDb(music));
            if (sfxIdx >= 0) AudioServer.SetBusVolumeDb(sfxIdx, LinearToDb(sfx));
        }

        public static void Play(string key)
        {
            if (Instance is null) return;
            if (!Instance.players.TryGetValue(key, out var player)) return;
            if (player.Stream is null) player.Stream = MakeBlip(key);
            player.PitchScale = 0.92f + (float)GD.Randf() * 0.16f;
            player.Play();
        }

        public static void PlayMusic()
        {
            if (Instance?.musicA?.Stream is null) return;
            if (!Instance.musicA.Playing) Instance.musicA.Play();
        }

        /// <summary>Crossfade to another BGM path (or restart current if missing).</summary>
        public static void CrossfadeMusic(string path, double duration = 1.2)
        {
            if (Instance is null) return;
            var next = Instance.musicOnA ? Instance.musicB : Instance.musicA;
            var prev = Instance.musicOnA ? Instance.musicA : Instance.musicB;
            if (next is null || prev is null) return;

            if (!TryAssignStream(next, path) && next.Stream is null)
                next.Stream = MakeBlip("ui");

            next.VolumeDb = -40f;
            if (!next.Playing) next.Play();
            Instance.crossfadeTween?.Kill();
            Instance.crossfadeTween = Instance.CreateTween().SetParallel();
            Instance.crossfadeTween.TweenProperty(next, "volume_db", -12f, duration);
            Instance.crossfadeTween.TweenProperty(prev, "volume_db", -40f, duration);
            Instance.crossfadeTween.Chain().TweenCallback(Callable.From(() =>
            {
                prev.Stop();
                Instance.musicOnA = !Instance.musicOnA;
            }));
        }

        /// <summary>One-shot spatial SFX parented under a world node.</summary>
        public static AudioStreamPlayer3D PlaySpatial(Node parent, string key, Vector3 position, float unitSize = 8f)
        {
            var player = new AudioStreamPlayer3D
            {
                Bus = "SFX",
                UnitSize = unitSize,
                MaxDistance = 28f,
                AttenuationFilterCutoffHz = 5000f
            };
            parent.AddChild(player);
            player.GlobalPosition = position;
            if (Instance is not null && Instance.players.TryGetValue(key, out var src) && src.Stream is not null)
                player.Stream = src.Stream;
            else
                player.Stream = MakeBlip(key);
            player.Play();
            player.Finished += player.QueueFree;
            return player;
        }

        private static void EnsureBuses()
        {
            EnsureBus("Music");
            EnsureBus("SFX");
        }

        private static void EnsureBus(string name)
        {
            if (AudioServer.GetBusIndex(name) >= 0) return;
            int idx = AudioServer.BusCount;
            AudioServer.AddBus(idx);
            AudioServer.SetBusName(idx, name);
            AudioServer.SetBusSend(idx, "Master");
        }

        private static float LinearToDb(float linear)
        {
            linear = Mathf.Clamp(linear, 0.0001f, 1.5f);
            return Mathf.LinearToDb(linear);
        }

        private static bool TryAssignStream(AudioStreamPlayer player, string path)
        {
            if (!ResourceLoader.Exists(path)) return false;
            var stream = GD.Load<AudioStream>(path);
            if (stream is null) return false;
            player.Stream = stream;
            return true;
        }

        private static AudioStreamWav MakeBlip(string key)
        {
            int sampleRate = 22050;
            float duration = key is "raid" or "whoosh" ? 0.55f : 0.12f;
            int samples = (int)(sampleRate * duration);
            var data = new byte[samples * 2];
            float freq = key switch
            {
                "hit" => 180f,
                "summon" => 520f,
                "raid" => 740f,
                "pack" => 360f,
                "whoosh" => 220f,
                _ => 440f
            };

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - t / duration;
                float wobble = key == "raid" ? Mathf.Sin(t * 18f) * 40f : 0f;
                float noise = key == "whoosh" ? (float)GD.RandRange(-0.2, 0.2) : 0f;
                float sample = (Mathf.Sin((freq + wobble) * Mathf.Tau * t) + noise) * envelope * 0.35f;
                short pcm = (short)Mathf.Clamp((int)(sample * short.MaxValue), short.MinValue, short.MaxValue);
                data[i * 2] = (byte)(pcm & 0xff);
                data[i * 2 + 1] = (byte)((pcm >> 8) & 0xff);
            }

            return new AudioStreamWav
            {
                Data = data,
                MixRate = sampleRate,
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                Stereo = false
            };
        }
    }
}
