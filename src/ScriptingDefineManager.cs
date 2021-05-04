using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Appalachia.Utility.Compilation
{
    [InitializeOnLoad]
    internal class ScriptingDefineManager : Editor
    {



        static ScriptingDefineManager()
        {
            if (!ScriptingDefineSettings.Enabled)
            {
                if (ScriptingDefineSettings.WarnIfDisabled)
                {
                    Debug.LogWarning("Scripting Define Manager is disabled.");
                }

                return;
            }

            Execute();
        }

        public static void Execute()
        {
            var defines = GetDefines();

            AddDefinesUsingFilter(defines);
            SaveNewDefines(defines);
        }

        public static void ResetDefines()
        {
            var targetGroup = ScriptingDefineSettings.BuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            if (string.IsNullOrWhiteSpace(defines))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Empty);
            }
            else
            {
                ScriptingDefineSettings.BackupDefines = defines;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Empty);
            }
        }

        public static void RestoreDefines()
        {
            var backup = ScriptingDefineSettings.BackupDefines;

            ScriptingDefineSettings.BackupDefines = string.Empty;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(ScriptingDefineSettings.BuildTargetGroup, backup);
        }

        public static string GetDefinesUnformatted()
        {
            var defineString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(ScriptingDefineSettings.BuildTargetGroup);

            return defineString;
        }

        public static List<string> GetDefines()
        {
            var defineString = GetDefinesUnformatted();
            var defines = defineString.Split(';').Select(s => s.Trim()).ToList();

            return defines;
        }

        private static void AddDefinesUsingFilter(List<string> defines)
        {
            var doFilter = ScriptingDefineSettings.Filtered;
            var filter = ScriptingDefineSettings.FilterValue;
            var excludeTests = ScriptingDefineSettings.ExcludeTests;
            var filterStart = $"{filter.ToUpper()}_";
            var filterMid = $"_{filter.ToUpper()}_";
            var filterEnd = $"_{filter.ToUpper()}";

            var assemblies = GetCandidateAssemblies();

            defines.AddRange(
                assemblies.Select(FormatAssemblyNameAsDefine)
                          .Where(
                               name => !excludeTests ||
                                       !(name.Contains("TEST_") || name.Contains("TESTS_") || name.Contains("_TEST"))
                           )
                          .Where(
                               name => !doFilter ||
                                       name.StartsWith(filterStart) ||
                                       name.Contains(filterMid) ||
                                       name.EndsWith(filterEnd)
                           )
            );
        }

        public static Assembly[] GetCandidateAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies;
        }

        public static string FormatAssemblyNameAsDefine(Assembly assembly)
        {
            return assembly.FullName.Split(',')[0].ToLowerInvariant().Replace(".", "_").ToUpperInvariant();
        }

        public static void SaveNewDefines(List<string> defines)
        {
            var definesLookup = new HashSet<string>(defines);

            var newDefineString = string.Join(";", definesLookup);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(ScriptingDefineSettings.BuildTargetGroup, newDefineString);
        }
    }
}