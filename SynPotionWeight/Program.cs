using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Newtonsoft.Json.Linq;
using SynPotionWeight.Types;
using System.IO;
using System;

namespace SynPotionWeight
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                new UserPreferences() {
                    ActionsForEmptyArgs = new RunDefaultPatcher
                    {
                        IdentifyingModKey = "SynPotionWeight.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var weights = JObject.Parse(File.ReadAllText(Path.Combine(state.ExtraSettingsDataPath, "settings.json"))).ToObject<Settings>()?.WeightMult??0.2f;
            foreach(var alch in state.LoadOrder.PriorityOrder.OnlyEnabled().Ingestible().WinningOverrides()) {
                if((alch.Keywords?.Contains(Skyrim.Keyword.VendorItemPotion)??false) || (alch.Keywords?.Contains(Skyrim.Keyword.VendorItemPoison)??false)) {
                    Console.WriteLine($"Patching {alch.Name}");
                    var nalch = state.PatchMod.Ingestibles.GetOrAddAsOverride(alch);
                    nalch.Weight *= weights;
                }
            }
        }
    }
}
