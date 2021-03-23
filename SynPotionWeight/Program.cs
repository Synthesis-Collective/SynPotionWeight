using System;
using System.IO;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Newtonsoft.Json.Linq;
using SynPotionWeight.Types;

namespace SynPotionWeight
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "SynReweightedPotions.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var weights = JObject.Parse(File.ReadAllText(Path.Combine(state.ExtraSettingsDataPath, "settings.json"))).ToObject<Settings>().WeightMult;
            foreach (var alch in state.LoadOrder.PriorityOrder.OnlyEnabled().Ingestible().WinningOverrides())
            {
                if (alch.Keywords?.Contains(Skyrim.Keyword.VendorItemPotion.FormKey)??false)
                {
                    Console.WriteLine($"Patching {alch.Name}");
                    var nalch = state.PatchMod.Ingestibles.GetOrAddAsOverride(alch);
                    nalch.Weight *= weights;
                }
            }
        }
    }
}
