using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

using static Epicentr.IntegrationTests.PathCreators.WorkerPathCreator;

namespace Epicentr.IntegrationTests
{
	public class WorkerIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _webAppFactory;

		public WorkerIntegrationTests(WebApplicationFactory<Startup> webAppFactory)
		{
			_webAppFactory = webAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureAppConfiguration((hostBuilderContext, config) => config.AddJsonFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "workerintegrationsettings.json")));
			});

			var scopeFactory = _webAppFactory.Services.GetService<IServiceScopeFactory>();
			using (var scope = scopeFactory.CreateScope())
			{
				var context = scope.ServiceProvider.GetService<DB.EpicentrDbContext>();
				context.Database.EnsureDeleted();
				context.Database.EnsureCreated();
			}
		}

		[Fact]
		public async Task Post_worker_has_correct_response_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();
			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());

			// Act
			var postResponse = await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			// Assert
			postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
		}

		[Fact]
		public async Task Post_worker_has_response_with_correct_location_header()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();
			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());

			// Act
			var postResponse = await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);
			var resultWorker = await httpClient.GetFromJsonAsync<WorkerGetModel>(postResponse.Headers.Location);

			// Assert
			resultWorker.Should().BeEquivalentTo(new WorkerGetModel(testWorker.Id, testWorker.FirstName, testWorker.LastName, testWorker.TasksIds));
		}

		[Fact]
		public async Task Get_worker_by_id_has_correct_response_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			// Act
			var getResponse = await httpClient.GetAsync(MakeGetPath(testWorker.Id));

			// Assert
			getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
		}

		[Fact]
		public async Task Get_worker_by_nonexists_id_has_correct_response_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			// Act
			var getResponse = await httpClient.GetAsync(MakeGetPath(Guid.NewGuid()));

			// Assert
			getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task Get_worker_by_id_returns_correct_task()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			// Act
			var resultWorker = await httpClient.GetFromJsonAsync<WorkerGetModel>(MakeGetPath(testWorker.Id));

			// Assert
			resultWorker.Should().BeEquivalentTo(new WorkerGetModel(testWorker.Id, testWorker.FirstName, testWorker.LastName, testWorker.TasksIds));
		}

		[Fact]
		public async Task For_empty_repository_get_all_workers_returns_empty_workers_collection()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			// Act
			var resultWorkers = await httpClient.GetFromJsonAsync<ICollection<WorkerGetModel>>(MakeGetAllPath());

			// Assert
			resultWorkers.Should().BeEmpty();
		}

		[Fact]
		public async Task For_nonempty_repository_get_all_workers_returns_correct_workers_collection()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker1 = new WorkerPostModel(Guid.NewGuid(), "FName1", "LName1", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker1);

			var testWorker2 = new WorkerPostModel(Guid.NewGuid(), "FName2", "LName2", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker2);

			// Act
			var resultWorkers = await httpClient.GetFromJsonAsync<IEnumerable<WorkerGetModel>>(MakeGetAllPath());

			// Assert
			resultWorkers.Should().HaveCount(2);
			resultWorkers.Should().ContainEquivalentOf(new WorkerGetModel(testWorker1.Id, testWorker1.FirstName, testWorker1.LastName, testWorker1.TasksIds));
			resultWorkers.Should().ContainEquivalentOf(new WorkerGetModel(testWorker2.Id, testWorker2.FirstName, testWorker2.LastName, testWorker2.TasksIds));
		}

		[Fact]
		public async Task Delete_worker_by_nonexists_id_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			// Act
			var deleteResponse = await httpClient.DeleteAsync(MakeDeletePath(Guid.NewGuid()));

			// Assert
			deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task Delete_worker_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			// Act
			var deleteResponse = await httpClient.DeleteAsync(MakeDeletePath(testWorker.Id));

			// Assert
			deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
		}

		[Fact]
		public async Task Update_worker_by_nonexists_id_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);
			var updatedTestWorker = new WorkerPutModel("FName2", "LName2", Enumerable.Empty<Guid>());

			// Act
			var putResponse = await httpClient.PutAsJsonAsync(MakePutPath(Guid.NewGuid()), updatedTestWorker);

			// Assert
			putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task Update_worker_by_id_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);
			var updatedTestWorker = new WorkerPutModel("FName2", "LName2", Enumerable.Empty<Guid>());

			// Act
			var putResponse = await httpClient.PutAsJsonAsync(MakePutPath(testWorker.Id), updatedTestWorker);

			// Assert
			putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
		}

		[Fact]
		public async Task Get_worker_after_worker_update_returns_updated_worker()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testWorker = new WorkerPostModel(Guid.NewGuid(), "FName", "LName", Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testWorker);

			var updatedTestWorker = new WorkerPutModel("FName2", "LName2", Enumerable.Empty<Guid>());
			await httpClient.PutAsJsonAsync(MakePutPath(testWorker.Id), updatedTestWorker);

			// Act
			var resultWorker = await httpClient.GetFromJsonAsync<WorkerGetModel>(MakeGetPath(testWorker.Id));

			// Assert
			resultWorker.Should().BeEquivalentTo(new WorkerGetModel(testWorker.Id, updatedTestWorker.FirstName, updatedTestWorker.LastName, updatedTestWorker.TasksIds));
		}
	}
}
