using UnityEngine;

namespace RandomEncounters
{
    public abstract class Encounter
    {
        public abstract string Name { get; }
        public abstract int Weight { get; }

        public virtual bool IsAvailable() => true;
                
        public abstract void Trigger(MonoBehaviour host);
    }
}
