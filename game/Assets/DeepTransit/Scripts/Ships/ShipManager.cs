using System.Collections.Generic;
using UnityEngine;
using DeepTransit.Core;

namespace DeepTransit.Ships
{
    public class ShipManager : MonoBehaviour
    {
        public List<ShipInstance> Ships { get; private set; } = new();

        void OnEnable()  => TimeManager.OnGameMinuteTick += Tick;
        void OnDisable() => TimeManager.OnGameMinuteTick -= Tick;

        void Tick(long gameMinute)
        {
            foreach (var ship in Ships)
                ship.TickUpgrades(gameMinute);
        }

        public ShipInstance CreateShip(string shipName, ShipBlueprintSO blueprint)
        {
            var ship = new ShipInstance { Name = shipName, Blueprint = blueprint };
            ship.InitSlots();
            Ships.Add(ship);
            return ship;
        }
    }
}
