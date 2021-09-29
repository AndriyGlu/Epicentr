using System;
using System.Collections.Generic;

namespace Epicentr
{
	public record WorkerGetModel(Guid Id, string FirstName, string LastName, IEnumerable<Guid> TasksIds);

	public record WorkerPostModel(Guid Id, string FirstName, string LastName, IEnumerable<Guid> TasksIds);

	public record WorkerPutModel(string FirstName, string LastName, IEnumerable<Guid> TasksIds);
}
