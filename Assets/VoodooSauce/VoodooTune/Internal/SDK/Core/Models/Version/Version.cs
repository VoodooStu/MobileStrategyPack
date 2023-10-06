using System;
using System.Collections.Generic;

namespace Voodoo.Tune.Core
{
	[Serializable]
	public class VersionAPI
	{
		public bool Success { get; protected set; }
		public List<Version> Versions { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public VersionAPI(bool success, List<Version> versions)
		{
			Success = success;
			Versions = versions;
		}
	}
	
	public class Version
	{
		public string Id { get; protected set; }
		public string AppId { get; protected set; }
		public string Name { get; protected set; }
		public int HistorySeq { get; protected set; }
		public bool NewSegmentation { get; protected set; }
		public string S3ConfigPath { get; protected set; }
		public Status Status { get; protected set; }
		public DateTime? PublishedDate { get; protected set; }
		public DateTime? CreatedAt { get; protected set; }

#if NEWTONSOFT_JSON
		[Newtonsoft.Json.JsonConstructor]
#endif
		public Version(string id, string appId, string name, int historySeq, bool newSegmentation, string s3ConfigPath, Status status, DateTime? publishedDate, DateTime? createdAt)
		{
			Id = id;
			AppId = appId;
			Name = name;
			HistorySeq = historySeq;
			NewSegmentation = newSegmentation;
			S3ConfigPath = s3ConfigPath;
			Status = status;
			PublishedDate = publishedDate;
			CreatedAt = createdAt;
		}
	}
}