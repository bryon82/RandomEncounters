using System;
using System.Collections;
using UnityEngine;

namespace RandomEncounters
{
    public abstract class Encounter
    {
        public abstract string Name { get; }
        public abstract int Weight { get; }
        public float TimeRemaining { get; set; }
        public bool IsActive { get; internal set; }
        public Coroutine Runner(IEnumerator enumerator) => EncounterGenerator.Instance.StartCoroutine(enumerator);

        public virtual bool IsAvailable() => true;
                
        public abstract void Trigger();
    }
}
