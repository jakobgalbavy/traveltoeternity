using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepTransit.Destinations
{
    public class StarMapManager : MonoBehaviour
    {
        public StarMapSO StarMap;

        private readonly HashSet<string> _unlocked = new();

        public void UnlockDestination(string id) => _unlocked.Add(id);

        public bool IsUnlocked(string id) => _unlocked.Contains(id);

        public List<DestinationSO> GetAvailable(float reputation)
        {
            var result = new List<DestinationSO>();
            if (StarMap?.AllDestinations == null) return result;
            foreach (var d in StarMap.AllDestinations)
                if (_unlocked.Contains(d.Id) && d.ReputationRequired <= reputation)
                    result.Add(d);
            return result;
        }

        public DestinationSO GetById(string id)
        {
            if (StarMap?.AllDestinations == null) return null;
            foreach (var d in StarMap.AllDestinations)
                if (d.Id == id) return d;
            return null;
        }

        // Called after a mission arrives to apply first-completion unlocks.
        public void OnMissionArrived(DestinationSO destination)
        {
            if (destination?.UnlocksOnFirstCompletion == null) return;
            foreach (var d in destination.UnlocksOnFirstCompletion)
                _unlocked.Add(d.Id);
        }

        public void LoadUnlocked(List<string> ids)
        {
            _unlocked.Clear();
            foreach (var id in ids) _unlocked.Add(id);
        }

        public List<string> SaveUnlocked() => new(_unlocked);
    }
}
