using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EliteMMO.API;

namespace Checkem
{
    public class InstanceFactory
    {
        public IEnumerable<string> GetCharacterNames() => GetInstances().Select(i => i.Player.Name);

        public EliteAPI GetInstance(string characterName) => GetInstances().Single(i => i.Player.Name == characterName);

        private static IEnumerable<EliteAPI> GetInstances() => PolIds().Select(id => new EliteAPI(id));

        private static IEnumerable<int> PolIds() => AshitaIds().Concat(WindowerIds());
        private static IEnumerable<int> AshitaIds() => Process.GetProcessesByName("pol").Select(p => p.Id);
        private static IEnumerable<int> WindowerIds() => Process.GetProcessesByName("xiloader").Select(p => p.Id);
    }
}
