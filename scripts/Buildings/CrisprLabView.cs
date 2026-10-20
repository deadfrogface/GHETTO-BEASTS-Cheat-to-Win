using System;
using System.Collections.Generic;
using System.Linq;
using GhettoBeasts.Combat;
using GhettoBeasts.Core;
using GhettoBeasts.Data;
using GhettoBeasts.UI;
using GhettoBeasts.Visuals;
using Godot;

namespace GhettoBeasts.Buildings
{
    public partial class CrisprLabView : BuildingShell
    {
        private OptionButton beastPicker = null!;
        private readonly Dictionary<MutationSlot, OptionButton> slotPickers = new();
        private RichTextLabel preview = null!;
        private bool suppressSlotApply;

        protected override string Title => "CRISPR LAB";
        protected override string Subtitle => "Kopf / Gliedmaßen / Haut mutieren. Stabilisatoren senken die Gen-Instabilität.";
        protected override Color Accent => Palette.NeonPurple;

        private static GameManager Game => GameManager.Instance;

        protected override void BuildContent()
        {
            var grid = new GridContainer { Columns = 2 };
            grid.AddThemeConstantOverride("h_separation", 12);
            grid.AddThemeConstantOverride("v_separation", 8);
            Body.AddChild(grid);

            grid.AddChild(UiKit.Label("Bestie", 14, Palette.TextDim));
            beastPicker = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0f, 38f) };
            grid.AddChild(beastPicker);
            foreach (var card in Roster.OwnedCards(Game.Save).Where(c => c.CanMutate))
            {
                beastPicker.AddItem($"{card.Name} [{card.Rarity.DisplayName()}]");
                beastPicker.SetItemMetadata(beastPicker.ItemCount - 1, card.Id);
            }
            if (beastPicker.ItemCount == 0)
            {
                Body.AddChild(UiKit.Label("Keine mutierbaren Bestien im Roster. Öffne Darknet-Packs!", 14, Palette.Danger));
                return;
            }
            beastPicker.ItemSelected += _ => RestoreSlots();
            SelectId(beastPicker, Game.Save.ActiveFighterId);

            foreach (MutationSlot slot in Enum.GetValues<MutationSlot>())
            {
                grid.AddChild(UiKit.Label($"Slot {slot.DisplayName()}", 14, Palette.TextDim));
                var picker = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0f, 38f) };
                picker.AddItem("— leer —");
                picker.SetItemMetadata(0, string.Empty);
                foreach (var gene in ContentCatalog.MutationsForSlot(slot).Where(m => Roster.OwnsMutation(Game.Save, m.Id)))
                {
                    picker.AddItem($"{gene.Name} (+{gene.Instability}%)");
                    picker.SetItemMetadata(picker.ItemCount - 1, gene.Id);
                    picker.SetItemTooltip(picker.ItemCount - 1, gene.Description);
                }
                picker.ItemSelected += _ => ApplySlot(slot, picker);
                slotPickers[slot] = picker;
                grid.AddChild(picker);
            }

            preview = new RichTextLabel { BbcodeEnabled = true, FitContent = true, ScrollActive = false, CustomMinimumSize = new Vector2(0f, 90f) };
            Body.AddChild(preview);

            var actions = new HBoxContainer();
            actions.AddThemeConstantOverride("separation", 10);
            Body.AddChild(actions);

            var stab = UiKit.Button($"Stabilisator ({CrisprLab.StabilizerCostClean} $ Clean)",
                $"−{CrisprLab.StabilizerReduction}% Instabilität, max {CrisprLab.MaxStabilizerStacks} Stacks");
            stab.Pressed += OnStabilizer;
            actions.AddChild(stab);

            var setActive = UiKit.Button("Als Aktiven setzen");
            setActive.Pressed += () =>
            {
                Roster.SetActiveFighter(Game.Save, SelectedBeastId());
                Persist();
                SetStatus($"Aktiver Kämpfer: {SelectedBeastId()}");
            };
            actions.AddChild(setActive);

            RestoreSlots();
        }

        private string SelectedBeastId() =>
            beastPicker.Selected >= 0 ? beastPicker.GetItemMetadata(beastPicker.Selected).AsString() : string.Empty;

        private void ApplySlot(MutationSlot slot, OptionButton picker)
        {
            if (suppressSlotApply) return;
            string geneId = picker.Selected >= 0 ? picker.GetItemMetadata(picker.Selected).AsString() : string.Empty;
            if (!CrisprLab.TryEquip(Game.Save, SelectedBeastId(), slot, string.IsNullOrEmpty(geneId) ? null : geneId, out var error))
            {
                SetStatus(error, true);
                RestoreSlots();
                return;
            }
            Persist();
            RefreshPreview();
            SetStatus($"Slot {slot.DisplayName()} aktualisiert.");
        }

        private void OnStabilizer()
        {
            if (!CrisprLab.TryBuyStabilizer(Game.Save, Game.Economy, SelectedBeastId(), out var error))
            {
                SetStatus(error, true);
                return;
            }
            Persist();
            RefreshPreview();
            SetStatus("Stabilisator injiziert.");
        }

        private void RestoreSlots()
        {
            suppressSlotApply = true;
            string id = SelectedBeastId();
            var equipped = Roster.GetEquippedMutationIds(Game.Save, id).ToHashSet();
            foreach (var (slot, picker) in slotPickers)
            {
                string? match = ContentCatalog.MutationsForSlot(slot).Select(m => m.Id).FirstOrDefault(equipped.Contains);
                SelectId(picker, match ?? string.Empty);
            }
            suppressSlotApply = false;
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            string id = SelectedBeastId();
            if (!ContentCatalog.TryGetCard(id, out var card)) return;
            var mutations = Roster.ResolveMutations(Game.Save, id);
            var combatant = Combatant.Create(Combat.Side.Player, card, mutations);
            var loadout = Roster.GetOrCreateLoadout(Game.Save, id);
            int raw = combatant.Instability;
            int effective = CrisprLab.ApplyStabilizers(raw, loadout.StabilizerStacks);
            string hint = effective >= 100 ? "Kollaps-Risiko!"
                : effective > InstabilityResolver.CriticalThreshold ? "Kritisch (>80%)"
                : effective > InstabilityResolver.ConfusionThreshold ? "Verwirrungs-Risiko (>50%)"
                : "Stabil.";
            preview.Text =
                $"[b]{card.Name}[/b]  HP {combatant.Stats.MaxHp} · ATK {combatant.Stats.Attack} · DEF {combatant.Stats.Defense}\n" +
                $"Instabilität roh {raw}% → effektiv [color=#{UiKit.Hex(effective > 50 ? Palette.Danger : Palette.NeonGreen)}]{effective}%[/color] ({hint})\n" +
                $"Stabilisator-Stacks: {loadout.StabilizerStacks}/{CrisprLab.MaxStabilizerStacks}";
        }

        private static void SelectId(OptionButton picker, string id)
        {
            for (int i = 0; i < picker.ItemCount; i++)
            {
                if (picker.IsItemSeparator(i)) continue;
                if (picker.GetItemMetadata(i).AsString() == id)
                {
                    picker.Select(i);
                    return;
                }
            }
            if (picker.ItemCount > 0) picker.Select(0);
        }
    }
}
