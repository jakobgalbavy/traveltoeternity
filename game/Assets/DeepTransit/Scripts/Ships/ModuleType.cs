namespace DeepTransit.Ships
{
    public enum ModuleType
    {
        Hull,             // → HullIntegrity
        Engine,           // → EngineEfficiency
        LifeSupport,      // → LifeSupportQuality
        GrowBay,          // → FoodProduction
        MedicalBay,       // → MedicalCapacity
        PassengerBerths,  // → PassengerCapacity
        CargoHold,        // → CargoCapacity
        Navigation,       // → NavigationRating
        Security,         // → SecurityLevel
        CryoPods,         // → PassengerCapacity (premium; also cuts LifeSupport drain)
    }
}
