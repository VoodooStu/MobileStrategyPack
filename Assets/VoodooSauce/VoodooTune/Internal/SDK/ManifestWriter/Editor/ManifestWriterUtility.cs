#if UNITY_EDITOR && !NEWTONSOFT_JSON
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Voodoo.Tune.ManifestWriter
{
	public static class ManifestWriterUtility
	{
		private const string NewtonsoftSymbol   = "NEWTONSOFT_JSON";
		private const string VoodooTuneVersion  = "1.4.0";
		private const string PlayerPrefKey      = "ManifestWriterKey";
		private const string SuccessLog         = "Successfully edited the manifest.json file to add {0}";
		private const string DialogTitle        = "Package Manager dependencies";
		private const string PackageName        = "com.unity.nuget.newtonsoft-json";
		private const string PackageDisplayName = "Newtonsoft Json";

		private static readonly System.Version ToDownloadVersion = new System.Version("3.0.2");

		private static readonly string DialogMessage = $"VoodooTune requires \"{PackageDisplayName}\" to be installed." +
		                                               $"\nIt will automatically be downloaded in version \"{ToDownloadVersion}\"";

		[MenuItem("Voodoo/VoodooTune/Enable Newtonsoft verifier")]
		private static void RemoveKey()
		{
			PlayerPrefs.DeleteKey(PlayerPrefKey);
		}

		[InitializeOnLoadMethod]
		private static void EditManifest()
		{
			if (PlayerPrefs.GetString(PlayerPrefKey, "") == VoodooTuneVersion)
			{
				return;
			}

			AddPackageToManifest(PackageName);
		}

		private static async void AddPackageToManifest(string packageName)
		{
			int download = EditorUtility.DisplayDialogComplex(DialogTitle, DialogMessage, "Continue", "Cancel", "Don't ask again");

			if (download == 0)
			{
				string identifier = string.Join("@", packageName, ToDownloadVersion);
				PackageInfo package = await AddPackageAsync(identifier);

				if (package == null)
				{
					return;
				}

				Debug.Log(string.Format(SuccessLog, package.displayName));
#if !UNITY_2019_4_OR_NEWER
				BuildTargetGroup[] buildGroup = { BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone };
				foreach (BuildTargetGroup group in buildGroup)
				{
					string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
					if (definesString.Contains(NewtonsoftSymbol))
					{
						return;
					}
            
					List<string> allDefines = definesString.Split(';').ToList();
					allDefines.Add(NewtonsoftSymbol);
					PlayerSettings.SetScriptingDefineSymbolsForGroup(@group, string.Join(";", allDefines.ToArray()));
				}
#endif
			}
			else if (download == 2)
			{
				PlayerPrefs.SetString(PlayerPrefKey, VoodooTuneVersion);
			}
		}

		private static async Task<PackageInfo> AddPackageAsync(string identifier)
		{
			AddRequest addRequest = Client.Add(identifier);
			while (addRequest.IsCompleted == false)
			{
				await Task.Yield();
			}

			if (addRequest.Result == null)
			{
				Debug.LogError(addRequest.Error.message);
			}

			return addRequest.Result;
		}
	}
}
#endif