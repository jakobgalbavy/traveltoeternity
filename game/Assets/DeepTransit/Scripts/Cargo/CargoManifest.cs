using System;
using UnityEngine;
using DeepTransit.Ships;

namespace DeepTransit.Cargo
{
    [Serializable]
    public class CargoManifest
    {
        public int PassengerCount;
        public int PackageCount;

        public bool Validate(ShipInstance ship, out string error)
        {
            error = null;
            int passCap = Mathf.FloorToInt(ship.GetStat(ShipStat.PassengerCapacity));
            int cargoCap = Mathf.FloorToInt(ship.GetStat(ShipStat.CargoCapacity));

            if (PassengerCount > passCap)
            {
                error = $"Passenger capacity exceeded ({PassengerCount}/{passCap})";
                return false;
            }
            if (PackageCount > cargoCap)
            {
                error = $"Cargo capacity exceeded ({PackageCount}/{cargoCap})";
                return false;
            }
            return true;
        }

        public bool IsEmpty => PassengerCount == 0 && PackageCount == 0;
    }
}
