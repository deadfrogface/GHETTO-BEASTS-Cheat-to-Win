using System.Collections.Generic;
using Godot;

namespace GhettoBeasts.Audio
{
    /// <summary>
    /// Project AudioManager (autoload name: AudioBus).
    /// Master / Music / SFX buses, BGM crossfade, optional Spatial3D helpers.
    /// Prefer licensed/free files under res://assets/audio/; else procedural loops/blips.
    /// </summary>
    public partial class AudioBus : Node
    {
        /// <summary>Alias for call sites that prefer the AudioManager name.</summary>
        public static AudioBus? AudioManager => Instance;
        public static AudioBus? Instance { get; private set; }

        private readonly Dictionary<string, AudioStreamPlayer> players = new();
        private readonly List<AudioStreamPlayer3D> ambients = new();
        private AudioStreamPlayer? musicA;
        private AudioStreamPlayer? musicB;
        private bool musicOnA = true;
        private Tween? crossfadeTween;
        private string currentMusicKey = "district";

        private static readonly Dictionary<string, string> FileHints = new()
        {
            ["hit"] = "res://assets/audio/hit.ogg",
            ["hit_bass"] = "res://assets/audio/hit_bass.ogg",
            ["hit_crack"] = "res://assets/audio/hit_crack.ogg",
            ["summon"] = "res://assets/audio/summon.ogg",
            ["raid"] = "res://assets/audio/raid_siren.ogg",
            ["ui"] = "res://assets/audio/ui_click.ogg",
            ["pack"] = "res://assets/audio/pack_open.ogg",
            ["whoosh"] = "res://assets/audio/whoosh.ogg",
            ["ambient_alley"] = "res://assets/audio/ambient_alley.ogg",
            ["ambient_neon"] = "res://assets/audio/ambient_neon.ogg"
        };

        private static readonly Dictionary<string, string> MusicFiles = new()
        {
            ["district"] = "res://assets/audio/bgm_district.ogg",
            ["combat"] = "res://assets/audio/bgm_combat.ogg",
            ["prison"] = "res://assets/audio/bgm_prison.ogg"
        };

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            EnsureBuses();
            musicA = new AudioStreamPlayer { Name = "MusicA", VolumeDb = -12f, Bus = "Music" };
            musicB = new AudioStreamPlayer { Name = "MusicB", VolumeDb = -80f, Bus = "Music" };
            AddChild(musicA);
            AddChild(musicB);
            AssignMusic(musicA, "district");

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
            if (!Instance.players.TryGetValue(key, out var player))
            {
                // Lazy stub for layered keys not registered at boot.
                var stub = new AudioStreamPlayer { Name = $"Sfx_{key}", Bus = "SFX", Stream = MakeBlip(key) };
                Instance.AddChild(stub);
                Instance.players[key] = stub;
                player = stub;
            }
            if (player.Stream is null) player.Stream = MakeBlip(key);
            player.PitchScale = 0.92f + (float)GD.Randf() * 0.16f;
            player.Play();
        }

        /// <summary>Layered attack SFX: whoosh wind-up + impact (+ optional heavy bass/crack).</summary>
        public static void PlayAttackLayered(bool critical = false, bool heavy = false)
        {
            Play("whoosh");
            Play("hit");
            if (critical || heavy) Play("hit_crack");
            if (heavy) Play("hit_bass");
        }

        /// <summary>Explore → battle BGM helper (crossfade into combat pad).</summary>
        public static void CrossfadeExploreToBattle(double duration = 0.7) =>
            CrossfadeMusic("combat", duration);

        public static void CrossfadeBattleToExplore(double duration = 1.0) =>
            CrossfadeMusic("district", duration);

        public static void PlayMusic(string key = "district")
        {
            if (Instance is null) return;
            Instance.currentMusicKey = key;
            var active = Instance.musicOnA ? Instance.musicA : Instance.musicB;
            if (active is null) return;
            AssignMusic(active, key);
            active.VolumeDb = -12f;
            if (!active.Playing) active.Play();
        }

        /// <summary>Crossfade to another BGM key or explicit path.</summary>
        public static void CrossfadeMusic(string keyOrPath, double duration = 1.2)
        {
            if (Instance is null) return;
            var next = Instance.musicOnA ? Instance.musicB : Instance.musicA;
            var prev = Instance.musicOnA ? Instance.musicA : Instance.musicB;
            if (next is null || prev is null) return;

            string key = keyOrPath;
            if (keyOrPath.StartsWith("res://"))
            {
                if (!TryAssignStream(next, keyOrPath))
                    next.Stream = MakeLoop("district");
            }
            else
            {
                AssignMusic(next, key);
                Instance.currentMusicKey = key;
            }

            next.VolumeDb = -40f;
            if (!next.Playing) next.Play();
            Instance.crossfadeTween?.Kill();
            Instance.crossfadeTween = Instance.CreateTween().SetParallel();
            Instance.crossfadeTween.TweenProperty(next, "volume_db", -12f, duration);
            Instance.crossfadeTween.TweenProperty(prev, "volume_db", -40f, duration);
            Instance.crossfadeTween.Chain().TweenCallback(Callable.From(() =>
            {
                prev.Stop();
                Instance!.musicOnA = !Instance.musicOnA;
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

        /// <summary>Looping <see cref="AudioStreamPlayer3D"/> ambient bed (alley / neon stubs).</summary>
        public static AudioStreamPlayer3D AttachAmbient(Node parent, string key, Vector3 position, float unitSize = 14f)
        {
            var player = new AudioStreamPlayer3D
            {
                Name = $"Ambient_{key}",
                Bus = "SFX",
                UnitSize = unitSize,
                MaxDistance = 40f,
                VolumeDb = -18f,
                AttenuationFilterCutoffHz = 3500f
            };
            parent.AddChild(player);
            player.GlobalPosition = position;
            if (Instance is not null && Instance.players.TryGetValue(key, out var src) && src.Stream is not null)
                player.Stream = src.Stream;
            else
                player.Stream = MakeLoop(key.Contains("neon") ? "district" : "district");
            // Soft procedural bed when no dedicated ambient file.
            if (player.Stream is AudioStreamWav wav)
            {
                wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
            }
            player.Play();
            Instance?.ambients.Add(player);
            return player;
        }

        public static void ClearAmbients()
        {
            if (Instance is null) return;
            foreach (var a in Instance.ambients)
            {
                if (GodotObject.IsInstanceValid(a)) a.QueueFree();
            }
            Instance.ambients.Clear();
        }

        private static void AssignMusic(AudioStreamPlayer player, string key)
        {
            if (MusicFiles.TryGetValue(key, out var path) && TryAssignStream(player, path))
                return;
            player.Stream = MakeLoop(key);
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
                "hit_bass" => 90f,
                "hit_crack" => 920f,
                "summon" => 520f,
                "raid" => 740f,
                "pack" => 360f,
                "whoosh" => 220f,
                "ambient_alley" => 140f,
                "ambient_neon" => 410f,
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

        /// <summary>Procedural looping pad for district / combat / prison when OGG missing.</summary>
        private static AudioStreamWav MakeLoop(string key)
        {
            int sampleRate = 22050;
            float duration = 4.0f;
            int samples = (int)(sampleRate * duration);
            var data = new byte[samples * 2];
            float root = key switch
            {
                "combat" => 110f,
                "prison" => 82f,
                _ => 98f
            };
            float[] harmonics = key switch
            {
                "combat" => new[] { 1f, 1.5f, 2f, 3f },
                "prison" => new[] { 1f, 1.25f, 2.5f },
                _ => new[] { 1f, 1.5f, 2f }
            };

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float sample = 0f;
                foreach (float h in harmonics)
                {
                    float amp = 0.12f / h;
                    sample += Mathf.Sin(root * h * Mathf.Tau * t) * amp;
                }
                // Soft pulse / bass drum feel
                float pulse = Mathf.Sin(t * Mathf.Tau * (key == "combat" ? 2.2f : 1.4f));
                sample += pulse * 0.04f * Mathf.Sin(root * 0.5f * Mathf.Tau * t);
                // Crossfade ends for seamless loop
                float fade = 1f;
                float edge = 0.08f;
                if (t < edge) fade = t / edge;
                else if (t > duration - edge) fade = (duration - t) / edge;
                sample *= fade * 0.55f;
                short pcm = (short)Mathf.Clamp((int)(sample * short.MaxValue), short.MinValue, short.MaxValue);
                data[i * 2] = (byte)(pcm & 0xff);
                data[i * 2 + 1] = (byte)((pcm >> 8) & 0xff);
            }

            return new AudioStreamWav
            {
                Data = data,
                MixRate = sampleRate,
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                Stereo = false,
                LoopMode = AudioStreamWav.LoopModeEnum.Forward,
                LoopBegin = 0,
                LoopEnd = samples
            };
        }
    }
}
