namespace RandomEncounters
{
    internal struct EncounterEntry
    {
        public string Name;
        public int Weight;
        public System.Action Trigger;

        internal EncounterEntry(string name, int weight, System.Action trigger)
        {
            Name = name;
            Weight = weight;
            Trigger = trigger;
        }
    }
}
