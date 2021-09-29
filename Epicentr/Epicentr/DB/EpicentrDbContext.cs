using Microsoft.EntityFrameworkCore;

namespace Epicentr.DB
{
	public class EpicentrDbContext : DbContext
	{
		public EpicentrDbContext() { }
		public EpicentrDbContext(DbContextOptions<EpicentrDbContext> options) : base(options) { }

		public DbSet<Worker> Workers { get; set; }
		public DbSet<DB.Task> Tasks { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Worker
			modelBuilder.Entity<Worker>().HasQueryFilter(wm => !wm.SoftDeleted);

			// Task
			modelBuilder.Entity<DB.Task>().HasQueryFilter(tm => !tm.SoftDeleted);
		}
	}
}
