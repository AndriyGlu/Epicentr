using System;
using System.IO;

namespace Epicentr.IntegrationTests.PathCreators
{
	internal static class TaskPathCreator
	{
		public static readonly string ApiTasksPath = @"/api/tasks/";
		public static string MakeGetPath(Guid taskId) => Path.Combine(ApiTasksPath, taskId.ToString());
		public static string MakeGetAllPath() => ApiTasksPath;
		public static string MakePostPath() => ApiTasksPath;
		public static string MakePutPath(Guid taskId) => Path.Combine(ApiTasksPath, taskId.ToString());
		public static string MakeDeletePath(Guid taskId) => Path.Combine(ApiTasksPath, taskId.ToString());
	}
}
