using Epicentr.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Epicentr
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Epicentr", Version = "v1" });
			});

			services.AddDbContext<EpicentrDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString("SqlServerDbConnection"));
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			EnsureDatabase(app);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Epicentr v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private void EnsureDatabase(IApplicationBuilder app)
		{
			using (var scope = app.ApplicationServices.CreateScope())
			{
				var dbcontext = scope.ServiceProvider.GetService<EpicentrDbContext>();
				dbcontext.Database.EnsureCreated();
			}
		}
	}
}
