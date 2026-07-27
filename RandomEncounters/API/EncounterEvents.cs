using System;

namespace RandomEncounters.API
{
    public class EncounterEvents
    {
        /// <summary>
        /// Fired when an encounter is triggered, before its Run() method is invoked.
        /// This event allows subscribers to perform actions or modifications before the encounter starts.
        /// </summary>
        public static event Action<Encounter> EncounterTriggered;

        /// <summary>
        /// Fired after the encounter's coroutine/action has finished.
        /// </summary>
        public static event Action<Encounter> EncounterCompleted;

        /// <summary>
        /// Fired when an encounter is skipped, either due to player choice or other conditions.
        /// </summary>
        public static event Action EncounterSkipped;

        internal static void RaiseEncounterTriggered(Encounter enc) => EncounterTriggered?.Invoke(enc);

        /// <summary>
        /// Fires the EncounterCompleted event, indicating that the encounter has finished its execution.
        /// </summary>
        /// <param name="enc">The encounter that has completed.</param>
        public static void RaiseEncounterCompleted(Encounter enc) => EncounterCompleted?.Invoke(enc);

        internal static void RaiseEncounterSkipped() => EncounterSkipped?.Invoke();
    }
}
