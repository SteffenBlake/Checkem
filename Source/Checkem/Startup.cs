using System;
using System.Collections.Generic;
using System.Linq;
using EliteMMO.API;

namespace Checkem
{
    public class Startup
    {
        public static void Main(params string[] args)
        {
            var format = "normal";
            if (args.Any())
                for (var n = 0; n < args.Length; n += 2)
                {
                    var cmd = args[n].ToLower().Substring(0, 2);
                    switch (cmd)
                    {
                        case "-f":
                            format = args[n + 1].ToLower();
                            break;
                        default:
                            Console.WriteLine("Unrecognized arg '{0}'", args[n]);
                            Close();
                            break;
                    }
                }

            var instanceFactory = new InstanceFactory();

            var names = instanceFactory.GetCharacterNames().ToArray();

            if (!names.Any())
            {
                Console.WriteLine("No instances of pol found, please make sure FFXI is open!");
                Close();
            }

            var characterName = SelectName(names);

            var instance = instanceFactory.GetInstance(characterName);

            // Get all of our trusts
            var trustList = GetTrusts(instance)
                .OrderBy(t => t.Type)
                .ThenBy(t => t.Name)
                .ToArray();

            if (format == "wiki")
                Console.WriteLine("{{Trust Checklist \n| Complete - Color = green \n| Default - Color = black");

            // Output our data
            foreach (var trustGroup in trustList.GroupBy(t => t.Type))
            {
                if (format == "normal")
                    Console.WriteLine("==={0}===", trustGroup.Key);

                foreach (var trust in trustGroup)
                    if (format == "normal")
                        Console.WriteLine("[{0}] {1}", trust.HasSpell ? "√" : " ", trust.Name);
                    else if (format == "wiki")
                        Console.WriteLine("| {0} = {1}", trust.Name, trust.HasSpell ? "Complete" : "");
            }

            if (format == "normal")
            {
                // Aggregate data as well
                var hasTotal = trustList.Count(t => t.HasSpell);
                var total = trustList.Length;

                Console.WriteLine("Total: {0}/{1}", hasTotal, total);
            }
            else if (format == "wiki")
            {
                Console.WriteLine("}}");
            }

            Close();
        }

        private static string SelectName(IReadOnlyList<string> names)
        {
            // Only one character online
            if (names.Count == 1)
                return names.First();

            Console.WriteLine("Please select a character:");

            // Write out menu
            for (var n = 0; n < names.Count; n++)
                Console.WriteLine("[{0}] {1}", n, names[n]);

            var loweredNames = names.Select(n => n.ToLower()).ToArray();

            // Wait for user to enter an valid index or a name
            while (true)
            {
                var line = Console.ReadLine()?.Trim().ToLower();

                // Empty input
                if (string.IsNullOrEmpty(line))
                    continue;

                // Valid index
                if (int.TryParse(line, out var temp) && temp < names.Count)
                    return names[temp];

                // Couldnt find name either
                if (!loweredNames.Contains(line))
                    continue;

                // Valid name

                // Get lowered name index then return it in unlowered form
                var index = Array.IndexOf(loweredNames, line);
                return names[index];
            }
        }

        private static void Close()
        {
            Console.WriteLine("Press Enter to close");
            Console.ReadKey();
            Environment.Exit(-1);
        }

        private static IEnumerable<(bool HasSpell, string Name, string Type)> GetTrusts(EliteAPI instance)
        {
            // Iterate from 0 to 10,000 because I dunno how to get every spell that exists. It seems to be fast this way anyways
            for (uint n = 0; n < 10000; n++)
            {
                // Try and build the spell
                var spell = instance.Resources.GetSpell(n);

                // Is it a Trust?
                if (spell?.MagicType != (int) MagicType.Trust)
                    continue;

                // Load up info on the spell
                var name = spell.Name.FirstOrDefault()?.Trim();

                // Filter away placeholders
                if (string.IsNullOrEmpty(name))
                    continue;

                // Ignore Unity Trusts too
                if (name.Contains("(UC)"))
                    continue;

                var hasTrust = instance.Player.HasSpell(spell.Index);
                var icon = spell.ListIcon2;

                var type =
                    icon <= 1009
                        ? "Tank"
                        : icon <= 1018
                            ? "Melee Fighter"
                            : icon <= 1027
                                ? "Ranged Fighter"
                                : icon <= 1036
                                    ? "Offensive Caster"
                                    : icon <= 1045
                                        ? "Healer"
                                        : icon <= 1054
                                            ? "Support"
                                            : "Other";


                // Ok yeah it is a real trust
                yield return (hasTrust, name, type);
            }
        }
    }
}