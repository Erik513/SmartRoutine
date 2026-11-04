using ErikwnkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRoutine.Tests
{
    // The safeguards of the translation tables: nothing is missing in one language, the placeholders
    // match, and every key used in the code exists.
    [TestClass]
    public class LocalizationTests
    {
        private static readonly Regex Placeholder = new Regex(@"\{(\d+)[^}]*\}");

        private static IReadOnlyDictionary<string, string> German => Loc.Table(AppLanguage.German);
        private static IReadOnlyDictionary<string, string> English => Loc.Table(AppLanguage.English);

        [TestMethod]
        public void EveryKeyIsInBothLanguages()
        {
            var onlyGerman = German.Keys.Except(English.Keys).ToList();
            var onlyEnglish = English.Keys.Except(German.Keys).ToList();

            Assert.AreEqual(0, onlyGerman.Count, "Only in German: " + string.Join(", ", onlyGerman));
            Assert.AreEqual(0, onlyEnglish.Count, "Only in English: " + string.Join(", ", onlyEnglish));
        }

        [TestMethod]
        public void NoTextIsEmpty()
        {
            foreach (var table in new[] { German, English })
            {
                var empty = table.Where(p => string.IsNullOrWhiteSpace(p.Value)).Select(p => p.Key).ToList();

                Assert.AreEqual(0, empty.Count, "Empty texts: " + string.Join(", ", empty));
            }
        }

        [TestMethod]
        public void BothTextsOfAKeyHaveTheSamePlaceholders()
        {
            foreach (string key in German.Keys.Intersect(English.Keys))
            {
                Assert.AreEqual(
                    PlaceholdersOf(German[key]),
                    PlaceholdersOf(English[key]),
                    "Placeholders differ: " + key);
            }
        }

        [TestMethod]
        public void EveryKeyUsedInTheCodeExists()
        {
            string sourceFolder = FindSourceFolder();
            var usedKeys = new Dictionary<string, string>();
            var call = new Regex(@"Loc\.(?:S|F)\(\s*""([A-Za-z0-9_.]+)""");

            foreach (string file in Directory.EnumerateFiles(sourceFolder, "*.cs", SearchOption.AllDirectories))
            {
                char separator = Path.DirectorySeparatorChar;

                if (file.Contains(separator + "obj" + separator) || file.Contains(separator + "bin" + separator))
                    continue;

                // Comments (the examples in the documentation of Loc) are not code.
                string code = string.Join("\n", File.ReadAllLines(file).Where(line => !line.TrimStart().StartsWith("//")));

                foreach (Match match in call.Matches(code))
                    usedKeys[match.Groups[1].Value] = Path.GetFileName(file);
            }

            var missing = usedKeys
                .Where(p => !German.ContainsKey(p.Key) || !English.ContainsKey(p.Key))
                .Select(p => p.Key + " (" + p.Value + ")")
                .ToList();

            Assert.AreEqual(0, missing.Count, "Keys used in the code but missing in a table: " + string.Join(", ", missing));
        }

        [TestMethod]
        public void UnknownKey_GivesTheKey_NullGivesEmptyText()
        {
            Assert.AreEqual("Loc.Test.NoSuchKey", Loc.S("Loc.Test.NoSuchKey"));
            Assert.AreEqual("", Loc.S(null));
        }

        private static string PlaceholdersOf(string text)
        {
            return string.Join(",", Placeholder.Matches(text).Cast<Match>().Select(m => m.Groups[1].Value).Distinct().OrderBy(v => v));
        }

        private static string FindSourceFolder()
        {
            string folder = AppContext.BaseDirectory;

            while (folder != null)
            {
                if (File.Exists(Path.Combine(folder, "SmartRoutine", "SmartRoutine.csproj")))
                    return Path.Combine(folder, "SmartRoutine");

                folder = Path.GetDirectoryName(folder);
            }

            Assert.Fail("The source folder of SmartRoutine was not found.");

            return null;
        }
    }

    // The tests that compare texts run in German, the language the program starts in.
    [TestClass]
    public static class TestLanguage
    {
        [AssemblyInitialize]
        public static void UseGerman(TestContext context)
        {
            ErikwnkWFUI.UIStyles.Language = ErikwnkWFUI.Styles.UILanguage.German;
        }
    }
}
