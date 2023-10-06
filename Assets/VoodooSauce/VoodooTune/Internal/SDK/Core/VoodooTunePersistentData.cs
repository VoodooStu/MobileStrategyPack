using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Tune.Core
{
	[Flags]
	public enum VoodooTuneDataType
	{
		None    = 0,
		Segment = 1 << 0,
		Cohort  = 1 << 1,
		Sandbox = 1 << 2,
		Version = 1 << 3,
		AppId   = 1 << 4,
		All     = ~(~0 << 5)
	}
	
	public static class VoodooTunePersistentData
	{
		private const string PLAYER_PREFS_DEBUG_VERSION_ID = "vtDebugVersionId";
		private const string PLAYER_PREFS_DEBUG_APP_ID     = "vtDebugAppId";
		private const string PLAYER_PREFS_DEBUG_ABTEST_ID  = "vtDebugAbTestId";
		private const string PLAYER_PREFS_DEBUG_COHORT_ID  = "vtDebugCohortId";
		private const string PLAYER_PREFS_DEBUG_SEGMENT_ID = "vtDebugSegmentsId";
		private const string PLAYER_PREFS_DEBUG_SANDBOX_ID = "vtDebugSandboxId";

		private const string CONFIG_URL_PREFS_KEY = "ConfigPlayerPrefsConfigUrlPrefs";
		private const string CONFIG_PLAYER_PREFS_KEY = "ConfigPlayerPrefsConfigPlayerPrefs";
		
		public const string VOODOO_TUNE_VERSION_KEY = "RemoteConfigVersion";
		public const string VOODOO_TUNE_SERVER_KEY = "RemoteConfigServer";
		
		public static readonly string[] SavedServerDisplayNames = { "Production", "Staging", "Development" };
		public static readonly string[] SavedStatusDisplayNames = { "Live", "Work In Progress"};
		
		private static List<Segment> _savedSegments;
		public static List<Segment> SavedSegments
		{
			get
			{
				if (_savedSegments == null)
				{
					Load();
				}
				
				return _savedSegments;
			}
			private set => _savedSegments = value;
		}

		private static List<ABTest> _savedABTests;
		public static List<ABTest> SavedABTests
		{
			get
			{
				if (_savedABTests == null)
				{
					Load();
				}
				
				return _savedABTests;
			}
			private set => _savedABTests = value;
		}

		private static List<Cohort> _savedCohorts;
		public static List<Cohort> SavedCohorts
		{
			get
			{
				if (_savedCohorts == null)
				{
					Load();
				}
				
				return _savedCohorts;
			}
			private set => _savedCohorts = value;
		}
		
		private static Sandbox _savedSandbox;
		public static Sandbox SavedSandbox
		{
			get
			{
				if (_savedSandbox == null)
				{
					Load();
				}
				
				return _savedSandbox;
			}
			private set => _savedSandbox = value;
		}

		public static string SavedVersionId
		{
			get => PlayerPrefs.GetString(PLAYER_PREFS_DEBUG_VERSION_ID, null);
			set
			{
				PlayerPrefs.SetString(PLAYER_PREFS_DEBUG_VERSION_ID, value);
				PlayerPrefs.Save();
			}
		}

		public static string SavedAppId
		{
			get => PlayerPrefs.GetString(PLAYER_PREFS_DEBUG_APP_ID, null);
			set
			{
				PlayerPrefs.SetString(PLAYER_PREFS_DEBUG_APP_ID, value);
				PlayerPrefs.Save();
			}
		}

		public static Server SavedServer
		{
			get => (Server)PlayerPrefs.GetInt(VOODOO_TUNE_SERVER_KEY, (int)Server.tech);
			set
			{
				PlayerPrefs.SetInt(VOODOO_TUNE_SERVER_KEY, (int)value);
				PlayerPrefs.Save();
			}
		}

		public static Status SavedStatus
		{
			get => (Status)PlayerPrefs.GetInt(VOODOO_TUNE_VERSION_KEY, (int)Status.live);
			set
			{
				PlayerPrefs.SetInt(VOODOO_TUNE_VERSION_KEY, (int)value);
				PlayerPrefs.Save();
			}
		}
		
		public static string SavedConfig
		{
			get => PlayerPrefs.GetString(CONFIG_PLAYER_PREFS_KEY, null);
			set
			{
				PlayerPrefs.SetString(CONFIG_PLAYER_PREFS_KEY, value);
				PlayerPrefs.Save();
			}
		}

		public static string SavedURL
		{
			get => PlayerPrefs.GetString(CONFIG_URL_PREFS_KEY, null);
			set
			{
				PlayerPrefs.SetString(CONFIG_URL_PREFS_KEY, value);
				PlayerPrefs.Save();
			}
		}
		
		public static readonly Dictionary<Status, string> SavedStatusToName = new Dictionary<Status, string>
		{
			{Status.live, "Live"},
			{Status.wip, "Work in progress"},
			{Status.history, "History"},
		};

		public static string SavedStatusName => SavedStatusToName[SavedStatus];

		public static void Load()
		{
			SavedABTests = LoadList<ABTest>(PLAYER_PREFS_DEBUG_ABTEST_ID);
			SavedCohorts = LoadList<Cohort>(PLAYER_PREFS_DEBUG_COHORT_ID);
			SavedSegments = LoadList<Segment>(PLAYER_PREFS_DEBUG_SEGMENT_ID);
			SavedSandbox = Load<Sandbox>(PLAYER_PREFS_DEBUG_SANDBOX_ID);

			if (SavedABTests.Count != SavedCohorts.Count)
			{
				SavedABTests = new List<ABTest>();
				SavedCohorts = new List<Cohort>();
			}
		}

		private static List<T> LoadList<T>(string key)
		{
			string json = PlayerPrefs.GetString(key, null);
			
			if (string.IsNullOrEmpty(json) == false)
			{
#if NEWTONSOFT_JSON
				try
				{
					List<T> value = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(json);
					return value ?? new List<T>();
				}
				catch
				{
					// ignored
				}
#endif
			}
			
			return new List<T>();
		}

		private static T Load<T>(string key)
		{
#if NEWTONSOFT_JSON
			try
			{
				string json = PlayerPrefs.GetString(key, null);
				return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
			}
			catch
			{
				// ignored
			}
#endif
			return default;
		}
		
		public static void Save(List<Cohort> cohorts, List<ABTest> abTests, List<Segment> segments, Sandbox sandbox)
		{
			Save(abTests, cohorts);
			Save(segments);
			Save(sandbox);
		}

		public static void Save(List<ABTest> abTests, List<Cohort> cohorts)
		{
#if NEWTONSOFT_JSON
			string abtestJson = Newtonsoft.Json.JsonConvert.SerializeObject(abTests, VoodooTuneRequest.SerializerSettings);
			string cohortJson = Newtonsoft.Json.JsonConvert.SerializeObject(cohorts, VoodooTuneRequest.SerializerSettings);
			PlayerPrefs.SetString(PLAYER_PREFS_DEBUG_ABTEST_ID, abtestJson);
			PlayerPrefs.SetString(PLAYER_PREFS_DEBUG_COHORT_ID, cohortJson);
			PlayerPrefs.Save();
#endif
			SavedABTests = abTests;
			SavedCohorts = cohorts;
		}

		public static void Save(List<Segment> segments)
		{
#if NEWTONSOFT_JSON
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(segments, VoodooTuneRequest.SerializerSettings);
			PlayerPrefs.SetString(PLAYER_PREFS_DEBUG_SEGMENT_ID, json);
			PlayerPrefs.Save();
#endif
			SavedSegments = segments;
		}

		public static void Save(Sandbox sandbox)
		{
#if NEWTONSOFT_JSON
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(sandbox, VoodooTuneRequest.SerializerSettings);
			PlayerPrefs.SetString(PLAYER_PREFS_DEBUG_SANDBOX_ID, json);
			PlayerPrefs.Save();
#endif
			SavedSandbox = sandbox;
		}

		public static void Clear(VoodooTuneDataType flaggedType = VoodooTuneDataType.All ^ VoodooTuneDataType.AppId)
		{
			var modification = false;
			if ((flaggedType & VoodooTuneDataType.Cohort) != 0)
			{
				PlayerPrefs.DeleteKey(PLAYER_PREFS_DEBUG_ABTEST_ID);
				PlayerPrefs.DeleteKey(PLAYER_PREFS_DEBUG_COHORT_ID);
				modification = true;
				SavedABTests = new List<ABTest>();
				SavedCohorts = new List<Cohort>();
			}
			
			if ((flaggedType & VoodooTuneDataType.Segment) != 0)
			{
				PlayerPrefs.DeleteKey(PLAYER_PREFS_DEBUG_SEGMENT_ID);
				modification = true;
				SavedSegments = new List<Segment>();
			}
			
			if ((flaggedType & VoodooTuneDataType.Sandbox) != 0)
			{
				PlayerPrefs.DeleteKey(PLAYER_PREFS_DEBUG_SANDBOX_ID);
				modification = true;
				SavedSandbox = null;
			}
			
			if ((flaggedType & VoodooTuneDataType.Version) != 0)
			{
				PlayerPrefs.DeleteKey(PLAYER_PREFS_DEBUG_VERSION_ID);
				modification = true;
			}
			
			if ((flaggedType & VoodooTuneDataType.AppId) != 0)
			{
				PlayerPrefs.DeleteKey(PLAYER_PREFS_DEBUG_APP_ID);
				modification = true;
			}

			if (modification)
			{
				PlayerPrefs.Save();
			}
		}
	}
}