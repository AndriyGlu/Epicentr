using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Epicentr.DB
{
	public class Task
	{
		public Task(Guid id, string description, Priority priority)
		{
			Id = id;
			Description = description;
			Priority = priority;
		}

		[Key]
		public Guid Id { get; private set; }

		[Required]
		public bool SoftDeleted { get; set; }

		[Required]
		[MinLength(1)]
		public string Description { get; set; }

		[Required]
		public Priority Priority { get; set; }

		public ICollection<Worker> Workers { get; } = new List<Worker>();
	}
}
