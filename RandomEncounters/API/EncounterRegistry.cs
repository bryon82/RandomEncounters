using System.Collections.Generic;
using System.Linq;

namespace RandomEncounters.API
{
    public static class EncounterRegistry
    {
        internal static readonly List<Encounter> RegisteredEncounters = new List<Encounter>();

        /// <summary>
        /// Registers an encounter to the registry. This should be called in the mod's entry point.
        /// </summary>
        /// <param name="enc">The encounter to register.</param>
        public static void RegisterEncounter(Encounter enc) => RegisteredEncounters.Add(enc);

        internal static IEnumerable<Encounter> GetAvailable() => RegisteredEncounters.Where(e => e.IsAvailable);

        internal static Encounter GetByName(string name) => RegisteredEncounters.FirstOrDefault(e => e.Name == name);

        internal static Encounter GetActive() => RegisteredEncounters.FirstOrDefault(e => e.IsActive);
    }
}
