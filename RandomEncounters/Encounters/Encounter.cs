using System.Collections;
using UnityEngine;

namespace RandomEncounters
{
    public abstract class Encounter
    {
        /// <summary>
        /// The name of the encounter, used for logging and display purposes.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Used to determine the likelihood of this encounter being selected among others.
        /// </summary>
        public abstract int Weight { get; }

        /// <summary>
        /// The time remaining for the encounter to be active, in seconds. This property is set internally
        /// and should not be modified directly.
        /// </summary>
        public float TimeRemaining { get; internal set; }

        /// <summary>
        /// Indicates whether the encounter is currently active. This property is set internally and
        /// should not be modified directly.
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// Indicates whether the encounter is available to be triggered.
        /// </summary>
        public abstract bool IsAvailable { get; }

        /// <summary>
        /// Runs a coroutine using the EncounterGenerator's instance. This is a helper method to start
        /// coroutines related to the encounter.
        /// </summary>
        /// <param name="enumerator">The enumerator for the coroutine to start.</param>
        /// <returns>The started coroutine.</returns>
        public Coroutine Runner(IEnumerator enumerator) => EncounterGenerator.Instance.StartCoroutine(enumerator);

        /// <summary>
        /// Triggers the encounter. This method should contain the logic to initiate the encounter.
        /// </summary>
        public abstract void Trigger();
    }
}
