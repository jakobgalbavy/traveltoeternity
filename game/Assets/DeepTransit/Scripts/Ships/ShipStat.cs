namespace DeepTransit.Ships
{
    public enum ShipStat
    {
        HullIntegrity,       // damage resistance
        PassengerCapacity,   // max colonists
        CargoCapacity,       // max freight units
        LifeSupportQuality,  // morale / health baseline
        MedicalCapacity,     // illness outcome modifier
        NavigationRating,    // reduces hazard event frequency
        EngineEfficiency,    // voyage speed bonus
        SecurityLevel,       // reduces conflict events
        FoodProduction,      // grow bay output per cycle
    }
}
