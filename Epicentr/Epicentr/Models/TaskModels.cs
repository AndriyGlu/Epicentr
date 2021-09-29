using System;
using System.Collections.Generic;

namespace Epicentr
{
	public record TaskGetModel(Guid Id, string Description, int Priority, IEnumerable<Guid> WorkersIds);

	public record TaskPostModel(Guid Id, string Description, int Priority, IEnumerable<Guid> WorkersIds);

	public record TaskPutModel(string Description, int Priority, IEnumerable<Guid> WorkersIds);
}
