using System;
using System.IO;

namespace Epicentr.IntegrationTests.PathCreators
{
	internal static class WorkerPathCreator
	{
		public static readonly string ApiWorkersPath = @"/api/workers/";
		public static string MakeGetPath(Guid workerId) => Path.Combine(ApiWorkersPath, workerId.ToString());
		public static string MakeGetAllPath() => ApiWorkersPath;
		public static string MakePostPath() => ApiWorkersPath;
		public static string MakePutPath(Guid workerId) => Path.Combine(ApiWorkersPath, workerId.ToString());
		public static string MakeDeletePath(Guid workerId) => Path.Combine(ApiWorkersPath, workerId.ToString());
	}
}
