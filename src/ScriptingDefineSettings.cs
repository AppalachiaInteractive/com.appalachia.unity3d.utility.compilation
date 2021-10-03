using UnityEditor;

namespace Appalachia.CI.Compilation
{
    internal static class ScriptingDefineSettings
    {
        private static readonly string _key_prefix = typeof(ScriptingDefineManager).Namespace;
        private static readonly string _key_warnIfDisabled = $"{_key_prefix}.WarnIfDisabled";
        private static readonly string _key_enabled = $"{_key_prefix}.Enabled";
        private static readonly string _key_filtered = $"{_key_prefix}.Filtered";
        private static readonly string _key_filterValue = $"{_key_prefix}.FilterValue";
        private static readonly string _key_excludeTests = $"{_key_prefix}.ExcludeTests";
        private static readonly string _key_backupDefines = $"{_key_prefix}.BackupDefines.{0}";
        private const string _defaultFilter = "Appalachia";

        public static bool WarnIfDisabled
        {
            get => EditorPrefs.GetBool(_key_warnIfDisabled, true);
            set => EditorPrefs.SetBool(_key_warnIfDisabled, value);
        }

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(_key_enabled, false);
            set => EditorPrefs.SetBool(_key_enabled, value);
        }

        public static bool Filtered
        {
            get => EditorPrefs.GetBool(_key_filtered, false);
            set => EditorPrefs.SetBool(_key_filtered, value);
        }

        public static string FilterValue
        {
            get => EditorPrefs.GetString(_key_filterValue, _defaultFilter);
            set => EditorPrefs.SetString(_key_filterValue, value);
        }

        public static bool ExcludeTests
        {
            get => EditorPrefs.GetBool(_key_excludeTests, false);
            set => EditorPrefs.SetBool(_key_excludeTests, value);
        }

        public static BuildTargetGroup BuildTargetGroup
        {
            get => EditorUserBuildSettings.selectedBuildTargetGroup;
            set => EditorUserBuildSettings.selectedBuildTargetGroup = value;
        }

        public static string BackupDefines
        {
            get
            {
                Initialize();
                return EditorPrefs.GetString(
                    string.Format(_key_backupDefines, BuildTargetGroup.ToString()),
                    _defaultFilter
                );
            }
            set => EditorPrefs.SetString(string.Format(_key_backupDefines, BuildTargetGroup.ToString()), value);
        }

        public static void Initialize()
        {
            if (!EditorPrefs.HasKey(_key_warnIfDisabled))
            {
                EditorPrefs.SetBool(_key_warnIfDisabled, true);
            }

            if (!EditorPrefs.HasKey(_key_enabled))
            {
                EditorPrefs.SetBool(_key_enabled, false);
            }

            if (!EditorPrefs.HasKey(_key_filtered))
            {
                EditorPrefs.SetBool(_key_filtered, true);
            }

            if (!EditorPrefs.HasKey(_key_filterValue))
            {
                EditorPrefs.SetString(_key_filterValue, _defaultFilter);
            }

            if (!EditorPrefs.HasKey(_key_excludeTests))
            {
                EditorPrefs.SetBool(_key_excludeTests, true);
            }

            if (!EditorPrefs.HasKey(_key_backupDefines))
            {
                EditorPrefs.SetString(_key_backupDefines, string.Empty);
            }
        }
    }
}
