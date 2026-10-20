using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Core;

namespace GhettoBeasts.Combat
{
    public enum PrisonTile
    {
        Dirt,
        Rock,
        Tunnel,
        Start,
        Exit,
        Guard,
        Tool,
        Vent
    }

    public enum PrisonOutcome
    {
        Ongoing,
        Escaped,
        Caught,
        Bribed
    }

    public enum DigDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public sealed class PrisonEscapeEngine
    {
        public const int DefaultWidth = 11;
        public const int DefaultHeight = 9;
        public const int BribeDiamondCost = 1;
        public const int MaxNoise = 100;

        private readonly IRandom random;
        private readonly Economy economy;
        private readonly PrisonTile[,] tiles;
        private readonly HashSet<(int X, int Y)> revealed = new();
        private readonly List<(int X, int Y)> guards = new();

        public PrisonEscapeEngine(Economy economy, IRandom? random = null, int? seed = null)
        {
            this.economy = economy ?? throw new ArgumentNullException(nameof(economy));
            this.random = random ?? new SystemRandom(seed);
            Width = DefaultWidth;
            Height = DefaultHeight;
            tiles = new PrisonTile[Width, Height];
            Generate();
        }

        public event Action<string>? Log;
        public event Action? Changed;

        public int Width { get; }
        public int Height { get; }
        public int PlayerX { get; private set; }
        public int PlayerY { get; private set; }
        public int Noise { get; private set; }
        public int Tools { get; private set; }
        public int Moves { get; private set; }
        public PrisonOutcome Outcome { get; private set; } = PrisonOutcome.Ongoing;
        public bool IsOver => Outcome != PrisonOutcome.Ongoing;

        public PrisonTile TileAt(int x, int y) => InBounds(x, y) ? tiles[x, y] : PrisonTile.Rock;

        public bool IsRevealed(int x, int y) => revealed.Contains((x, y));

        public bool TryMove(DigDirection direction)
        {
            if (IsOver) return false;
            var (dx, dy) = Offset(direction);
            int nx = PlayerX + dx;
            int ny = PlayerY + dy;
            if (!InBounds(nx, ny))
            {
                Emit("Beton. Kein Durchkommen.");
                return false;
            }

            var tile = tiles[nx, ny];
            if (tile == PrisonTile.Rock)
            {
                if (Tools <= 0)
                {
                    Emit("Fels. Du brauchst ein Werkzeug.");
                    RaiseNoise(12);
                    PatrolGuards();
                    Changed?.Invoke();
                    return false;
                }

                Tools--;
                tiles[nx, ny] = PrisonTile.Tunnel;
                Emit("Werkzeug knackt den Fels.");
                RaiseNoise(8);
            }

            if (tiles[nx, ny] == PrisonTile.Guard)
            {
                Emit("Ein Wärter packt dich am Kragen!");
                Outcome = PrisonOutcome.Caught;
                Changed?.Invoke();
                return true;
            }

            DigInto(nx, ny);
            Moves++;
            RevealAround(PlayerX, PlayerY);
            RaiseNoise(tile == PrisonTile.Vent ? 4 : 6);
            PatrolGuards();

            if (tiles[PlayerX, PlayerY] == PrisonTile.Exit)
            {
                Outcome = PrisonOutcome.Escaped;
                Emit("FREI! Du kriechst in die Gasse.");
            }

            Changed?.Invoke();
            return true;
        }

        public bool TryBribeGuard()
        {
            if (IsOver) return false;
            if (!economy.TrySpend(Currency.Diamonds, BribeDiamondCost))
            {
                Emit($"Bestechung kostet {BribeDiamondCost} Dirty Diamond.");
                return false;
            }

            Outcome = PrisonOutcome.Bribed;
            Emit("Du schmierst den Wärter mit einem Diamond – Seiteneingang offen.");
            Changed?.Invoke();
            return true;
        }

        public IEnumerable<(int X, int Y)> AllCells()
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                yield return (x, y);
        }

        private void DigInto(int x, int y)
        {
            PlayerX = x;
            PlayerY = y;
            var tile = tiles[x, y];
            if (tile is PrisonTile.Dirt or PrisonTile.Tunnel or PrisonTile.Start)
                tiles[x, y] = PrisonTile.Tunnel;

            if (tile == PrisonTile.Tool)
            {
                Tools++;
                tiles[x, y] = PrisonTile.Tunnel;
                Emit("Rostige Brechstange gefunden (+1 Werkzeug).");
            }
            else if (tile == PrisonTile.Vent)
            {
                Emit("Lüftungsschacht – leiser und schneller.");
                tiles[x, y] = PrisonTile.Tunnel;
            }
        }

        private void RaiseNoise(int amount)
        {
            Noise = Math.Min(MaxNoise, Noise + amount);
            if (Noise < MaxNoise) return;
            Outcome = PrisonOutcome.Caught;
            Emit("Alarm! Die Wärter haben den Tunnel gehört.");
        }

        private void PatrolGuards()
        {
            if (IsOver) return;
            for (int i = 0; i < guards.Count; i++)
            {
                var (gx, gy) = guards[i];
                if (tiles[gx, gy] == PrisonTile.Guard) tiles[gx, gy] = PrisonTile.Dirt;

                var dirs = new[] { DigDirection.Up, DigDirection.Down, DigDirection.Left, DigDirection.Right };
                var dir = dirs[random.Range(0, dirs.Length)];
                var (dx, dy) = Offset(dir);
                int nx = gx + dx;
                int ny = gy + dy;
                if (!InBounds(nx, ny) || tiles[nx, ny] is PrisonTile.Rock or PrisonTile.Exit or PrisonTile.Start)
                {
                    nx = gx;
                    ny = gy;
                }

                if (nx == PlayerX && ny == PlayerY)
                {
                    Outcome = PrisonOutcome.Caught;
                    Emit("Ein patrouillierender Wärter erwischt dich!");
                    guards[i] = (nx, ny);
                    tiles[nx, ny] = PrisonTile.Guard;
                    return;
                }

                tiles[nx, ny] = PrisonTile.Guard;
                guards[i] = (nx, ny);
            }
        }

        private void Generate()
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                tiles[x, y] = PrisonTile.Dirt;

            for (int y = 0; y < Height; y++)
            {
                tiles[0, y] = PrisonTile.Rock;
                tiles[Width - 1, y] = PrisonTile.Rock;
            }
            for (int x = 0; x < Width; x++)
            {
                tiles[x, 0] = PrisonTile.Rock;
                tiles[x, Height - 1] = PrisonTile.Rock;
            }

            PlayerX = 1;
            PlayerY = Height / 2;
            tiles[PlayerX, PlayerY] = PrisonTile.Start;
            tiles[Width - 2, Height / 2] = PrisonTile.Exit;

            Place(PrisonTile.Rock, 14);
            Place(PrisonTile.Tool, 4);
            Place(PrisonTile.Vent, 5);
            PlaceGuards(3);
            RevealAround(PlayerX, PlayerY);
            Emit("Du liegst im Knast-Keller. Grabe dich zur EXIT-Zelle. Werkzeuge knacken Fels.");
        }

        private void Place(PrisonTile tile, int count)
        {
            int placed = 0;
            int guard = 0;
            while (placed < count && guard < 200)
            {
                guard++;
                int x = random.Range(1, Width - 1);
                int y = random.Range(1, Height - 1);
                if (tiles[x, y] != PrisonTile.Dirt) continue;
                if (Math.Abs(x - PlayerX) + Math.Abs(y - PlayerY) < 2) continue;
                tiles[x, y] = tile;
                placed++;
            }
        }

        private void PlaceGuards(int count)
        {
            int placed = 0;
            int guard = 0;
            while (placed < count && guard < 200)
            {
                guard++;
                int x = random.Range(3, Width - 2);
                int y = random.Range(1, Height - 1);
                if (tiles[x, y] != PrisonTile.Dirt) continue;
                tiles[x, y] = PrisonTile.Guard;
                guards.Add((x, y));
                placed++;
            }
        }

        private void RevealAround(int cx, int cy)
        {
            for (int y = cy - 1; y <= cy + 1; y++)
            for (int x = cx - 1; x <= cx + 1; x++)
                if (InBounds(x, y)) revealed.Add((x, y));
        }

        private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        private static (int X, int Y) Offset(DigDirection direction) => direction switch
        {
            DigDirection.Up => (0, -1),
            DigDirection.Down => (0, 1),
            DigDirection.Left => (-1, 0),
            DigDirection.Right => (1, 0),
            _ => (0, 0)
        };

        private void Emit(string message) => Log?.Invoke(message);
    }
}
