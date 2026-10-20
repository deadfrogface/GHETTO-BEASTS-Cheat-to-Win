using System.Collections.Generic;
using Godot;

namespace GhettoBeasts.Audio
{
    /// <summary>
    /// Lightweight SFX hooks. Prefer imported AudioStream files under res://assets/audio/;
    /// when missing, synthesize short procedural blips so gameplay never blocks.
    /// </summary>
    public partial class AudioBus : Node
    {
        public static AudioBus? Instance { get; private set; }

        private readonly Dictionary<string, AudioStreamPlayer> players = new();
        private AudioStreamPlayer? music;

        private static readonly Dictionary<string, string> FileHints = new()
        {
            ["hit"] = "res://assets/audio/hit.ogg",
            ["summon"] = "res://assets/audio/summon.ogg",
            ["raid"] = "res://assets/audio/raid_siren.ogg",
            ["ui"] = "res://assets/audio/ui_click.ogg",
            ["pack"] = "res://assets/audio/pack_open.ogg"
        };

        public override void _EnterTree() => Instance = this;

        public override void _Ready()
        {
            music = new AudioStreamPlayer { Name = "Music", VolumeDb = -12f, Bus = "Master" };
            AddChild(music);
            // Optional BGM placeholder — silent if file missing
            TryAssignStream(music, "res://assets/audio/bgm_district.ogg");

            foreach (var key in FileHints.Keys)
            {
                var player = new AudioStreamPlayer { Name = $"Sfx_{key}", Bus = "Master" };
                AddChild(player);
                if (!TryAssignStream(player, FileHints[key]))
                    player.Stream = MakeBlip(key);
                players[key] = player;
            }
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
            if (Instance?.music?.Stream is null) return;
            if (!Instance.music.Playing) Instance.music.Play();
        }

        private static bool TryAssignStream(AudioStreamPlayer player, string path)
        {
            if (!ResourceLoader.Exists(path)) return false;
            var stream = GD.Load<AudioStream>(path);
            if (stream is null) return false;
            player.Stream = stream;
            return true;
        }

        /// <summary>Tiny generated WAV so missing asset files never break F5.</summary>
        private static AudioStreamWav MakeBlip(string key)
        {
            int sampleRate = 22050;
            float duration = key == "raid" ? 0.55f : 0.12f;
            int samples = (int)(sampleRate * duration);
            var data = new byte[samples * 2];
            float freq = key switch
            {
                "hit" => 180f,
                "summon" => 520f,
                "raid" => 740f,
                "pack" => 360f,
                _ => 440f
            };

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - t / duration;
                float wobble = key == "raid" ? Mathf.Sin(t * 18f) * 40f : 0f;
                float sample = Mathf.Sin((freq + wobble) * Mathf.Tau * t) * envelope * 0.35f;
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
