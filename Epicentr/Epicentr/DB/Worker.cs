using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Epicentr.DB
{
	public class Worker
	{
		public Worker(Guid id, string firstName, string lastName)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;
		}

		[Key]
		public Guid Id { get; private set; }

		[Required]
		public bool SoftDeleted { get; set; }

		[Required]
		[MinLength(2)]
		public string FirstName { get; set; }

		[Required]
		[MinLength(2)]
		public string LastName { get; set; }

		public ICollection<DB.Task> Tasks { get; }
	}
}
