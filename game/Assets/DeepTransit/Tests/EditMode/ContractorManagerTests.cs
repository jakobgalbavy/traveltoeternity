using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DeepTransit.Contractors;

namespace DeepTransit.Tests
{
    public class ContractorManagerTests
    {
        GameObject        _go;
        ContractorManager _cm;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ContractorManager");
            _cm = _go.AddComponent<ContractorManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        ContractorInstance AddToRoster(string id, ContractorRole role, bool onMission = false)
        {
            var def = ScriptableObject.CreateInstance<ContractorSO>();
            def.Role = role;
            var c = new ContractorInstance { InstanceId = id, Definition = def, IsOnMission = onMission };
            _cm.Roster.Add(c);
            return c;
        }

        // ── GetById ───────────────────────────────────────────────────────────

        [Test]
        public void GetById_ReturnsCorrectContractor()
        {
            AddToRoster("c1", ContractorRole.Engineer);
            AddToRoster("c2", ContractorRole.Medic);

            var result = _cm.GetById("c2");

            Assert.IsNotNull(result);
            Assert.AreEqual("c2", result.InstanceId);
        }

        [Test]
        public void GetById_UnknownId_ReturnsNull()
        {
            AddToRoster("c1", ContractorRole.Engineer);

            Assert.IsNull(_cm.GetById("unknown"));
        }

        [Test]
        public void GetById_EmptyRoster_ReturnsNull()
        {
            Assert.IsNull(_cm.GetById("c1"));
        }

        // ── GetByRoleFromAssigned ─────────────────────────────────────────────

        [Test]
        public void GetByRoleFromAssigned_FindsMatchingRole()
        {
            AddToRoster("c1", ContractorRole.Engineer, onMission: true);
            AddToRoster("c2", ContractorRole.Medic,    onMission: true);

            var ids = new List<string> { "c1", "c2" };
            var result = _cm.GetByRoleFromAssigned(ContractorRole.Medic, ids);

            Assert.IsNotNull(result);
            Assert.AreEqual("c2", result.InstanceId);
        }

        [Test]
        public void GetByRoleFromAssigned_RoleNotInList_ReturnsNull()
        {
            AddToRoster("c1", ContractorRole.Engineer, onMission: true);

            var ids = new List<string> { "c1" };
            var result = _cm.GetByRoleFromAssigned(ContractorRole.Navigator, ids);

            Assert.IsNull(result);
        }

        [Test]
        public void GetByRoleFromAssigned_NullIdList_ReturnsNull()
        {
            AddToRoster("c1", ContractorRole.Engineer, onMission: true);

            Assert.IsNull(_cm.GetByRoleFromAssigned(ContractorRole.Engineer, null));
        }

        [Test]
        public void GetByRoleFromAssigned_ContractorInRosterButNotInList_ReturnsNull()
        {
            AddToRoster("c1", ContractorRole.Engineer, onMission: true);

            var ids = new List<string> { "c2" }; // c1 not in this mission
            Assert.IsNull(_cm.GetByRoleFromAssigned(ContractorRole.Engineer, ids));
        }

        [Test]
        public void GetByRoleFromAssigned_FindsCorrectOne_WhenMultipleSameRole()
        {
            AddToRoster("c1", ContractorRole.Engineer, onMission: true);
            AddToRoster("c2", ContractorRole.Engineer, onMission: true);

            // Only c2 is assigned to this mission
            var ids = new List<string> { "c2" };
            var result = _cm.GetByRoleFromAssigned(ContractorRole.Engineer, ids);

            Assert.IsNotNull(result);
            Assert.AreEqual("c2", result.InstanceId);
        }
    }
}
