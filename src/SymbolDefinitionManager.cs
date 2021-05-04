using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Appalachia.Utility.Compilation
{
    [InitializeOnLoad]
    public class SymbolDefinitionManager : Editor
    {
        static SymbolDefinitionManager()
        {
            var identifier = "Appalachia";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var defineString =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defines = defineString.Split(';').Select(s => s.Trim()).ToList();

            defines.AddRange(
                assemblies.Select(assembly => assembly.FullName)
                          .Select(fullName => fullName.Split(',')[0])
                          .Select(name => name.ToLowerInvariant().Replace(".", "_").ToUpperInvariant())
                          .Where(name => name.StartsWith($"{identifier.ToUpper()}_"))
            );

            var definesLookup = new HashSet<string>(defines);

            var newDefineString = string.Join(";", definesLookup);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                newDefineString
            );
        }
    }
}