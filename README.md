# RandomEncounters

Generates encounters while you are out at sea. You must be roughly 10 nautical miles away from land to get a chance at an encounter.

## Features

These are the encounters that currently are a part of this mod. There is a configurable chance for an encounter, if an encounter is triggered, a type of encounter is selected at random. Each type has a weighted value in the selection process:
* Partially full cargo and ship wreckage from an unfortunate vessel. Low chance for this encounter. <details><summary>click to view weighted value</summary>15</details>
* An eerily dense fog with almost no wind. Very low chance for this encounter. <details><summary>click to view weighted value</summary>10</details>
* Generates whales from the fantastic mod [SeaLifeMod](https://github.com/BryanP-JP19/SailwindSeaLifeMod) by Discord user BryanPhillips, if it is installed, at a more random cadence. Medium chance for this encounter. <details><summary>click to view weighted value</summary>25</details>
* An intense storm that blows in fast. Very low chance for this encounter. <details><summary>click to view weighted value</summary>5</details>
* A fishing bonanza, heralded by seagulls circling your boat, where fish are fast to hook on your fishing line. Compatible with IdleFishing mod. Low chance for this encounter. <details><summary>click to view weighted value</summary>10</details>
<br>
These encounters will persist. When you save the game in the middle of an encounter, quit the game, and then reload that save, that encounter will be restored. For timed encounters, that means you will still have the same time remaining for the encounter. For the whales encounter, that means the same number of whales that were active will be respawned. For flotsam encounters, that means all of the items except the wreck will still be there; the wreck turns into a piece of firewood.

### Configurable

* If this mod controls the SeaLifeMod mod
* Enable/disable: flotsam encounter, dense fog encounter, intense storm encounter, fishing bonanza encounter
* Minimum amount of time between chance rolls for an encounter. There is a configurable time range added to this. Encounter chances happen at some point in this time range.
* The percent chance an encounter will occur.
* The amount of time the dense fog, intense storm, and fishing bonanza encounters lasts

### Other Mod Authors

This mod includes an API if you want to use RandomEncounters to spawn your custom encounter.  
Here are the steps to use the API to add your encounter:
1. Add RandomEncounters as a BepInDependency 
```
[BepInDependency("com.raddude.randomencounters", "2.0.0")]
```

2. Add the RandomEncounters.dll as a reference in your project.

3. Make a class for your encounter which extends the abstract Encounter class. For example:
```
public class KrakenEncounter : Encounter
{
    public override string Name => "Kraken";

    public override int Weight => 2;

    public override bool IsAvailable => GameState.playing && GameState.currentBoat

    public override void Trigger() => Runner(SpawnKraken());
}
```

4. In your mod's Awake, add your encounter to the registry
```
EncounterRegistry.Register(new KrakenEncounter());
```

5. The trigger of the ecounter will be raise an event and, if you add `RaiseEncounterCompleted` at the end of you encounter code, the completion of the encounter will as well. Both of those events as well as an encounters skipped event can be subscribed to with: `EncounterTriggered`, `EncounterCompleted`, and `EncounterSkipped` from the `EncounterEvents` class.

### Requires

* [BepInEx 5.4.23](https://github.com/BepInEx/BepInEx/releases)

### Installation

If updating, remove RandomEncounters folders and/or RandomEncounters.dll files from previous installations.  
<br>
Extract the downloaded zip. Inside the extracted RandomEncounters-\<version\> folder copy the RandomEncounters folder and paste it into the Sailwind/BepInEx/Plugins folder.  

#### Consider supporting me 🤗

<a href='https://www.paypal.com/donate/?business=WKY25BB3TSH6E&no_recurring=0&item_name=Thank+you+for+your+support%21+I%27m+glad+you+are+enjoying+my+mods%21&currency_code=USD' target='_blank'><img src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif" border="0" alt="Donate with PayPal button" />
<a href='https://ko-fi.com/S6S11DDLMC' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi6.png?v=6' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>