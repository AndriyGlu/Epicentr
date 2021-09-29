using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

using static Epicentr.IntegrationTests.PathCreators.TaskPathCreator;

namespace Epicentr.IntegrationTests
{
	public class TaskIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _webAppFactory;

		public TaskIntegrationTests(WebApplicationFactory<Startup> webAppFactory)
		{
			_webAppFactory = webAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureAppConfiguration((hostBuilderContext, config) => config.AddJsonFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "taskintegrationsettings.json")));
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
		public async Task Post_task_has_correct_response_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();
			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());

			// Act
			var postResponse = await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			// Assert
			postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
		}

		[Fact]
		public async Task Post_task_has_response_with_correct_location_header()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();
			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());

			// Act
			var postResponse = await httpClient.PostAsJsonAsync(MakePostPath(), testTask);
			var resultTask = await httpClient.GetFromJsonAsync<TaskGetModel>(postResponse.Headers.Location);

			// Assert
			resultTask.Should().BeEquivalentTo(new TaskGetModel(testTask.Id, testTask.Description, testTask.Priority, testTask.WorkersIds));
		}

		[Fact]
		public async Task Get_task_by_id_has_correct_response_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			// Act
			var getResponse = await httpClient.GetAsync(MakeGetPath(testTask.Id));

			// Assert
			getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
		}

		[Fact]
		public async Task Get_task_by_nonexists_id_has_correct_response_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			// Act
			var getResponse = await httpClient.GetAsync(MakeGetPath(Guid.NewGuid()));

			// Assert
			getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task Get_task_by_id_returns_correct_task()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			// Act
			var resultTask = await httpClient.GetFromJsonAsync<TaskGetModel>(MakeGetPath(testTask.Id));

			// Assert
			resultTask.Should().BeEquivalentTo(new TaskGetModel(testTask.Id, testTask.Description, testTask.Priority, testTask.WorkersIds));
		}

		[Fact]
		public async Task For_empty_repository_get_all_tasks_returns_empty_tasks_collection()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			// Act
			var resultTasks = await httpClient.GetFromJsonAsync<ICollection<TaskGetModel>>(MakeGetAllPath());

			// Assert
			resultTasks.Should().BeEmpty();
		}

		[Fact]
		public async Task For_nonempty_repository_get_all_tasks_returns_correct_tasks_collection()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask1 = new TaskPostModel(Guid.NewGuid(), "MyDesc1", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask1);

			var testTask2 = new TaskPostModel(Guid.NewGuid(), "MyDesc2", 2, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask2);

			// Act
			var resultTasks = await httpClient.GetFromJsonAsync<IEnumerable<TaskGetModel>>(MakeGetAllPath());

			// Assert
			resultTasks.Should().HaveCount(2);
			resultTasks.Should().ContainEquivalentOf(new TaskGetModel(testTask1.Id, testTask1.Description, testTask1.Priority, testTask1.WorkersIds));
			resultTasks.Should().ContainEquivalentOf(new TaskGetModel(testTask2.Id, testTask2.Description, testTask2.Priority, testTask2.WorkersIds));
		}

		[Fact]
		public async Task Delete_task_by_nonexists_id_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			// Act
			var deleteResponse = await httpClient.DeleteAsync(MakeDeletePath(Guid.NewGuid()));

			// Assert
			deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task Delete_task_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			// Act
			var deleteResponse = await httpClient.DeleteAsync(MakeDeletePath(testTask.Id));

			// Assert
			deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
		}

		[Fact]
		public async Task Update_task_by_nonexists_id_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);
			var updatedTestTask = new TaskPostModel(Guid.NewGuid(), "MyDesc2", 2, Enumerable.Empty<Guid>());

			// Act
			var putResponse = await httpClient.PutAsJsonAsync(MakePutPath(Guid.NewGuid()), updatedTestTask);

			// Assert
			putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task Update_task_by_id_has_response_with_correct_status_code()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);
			var updatedTestTask = new TaskPutModel("MyDesc2", 2, Enumerable.Empty<Guid>());

			// Act
			var putResponse = await httpClient.PutAsJsonAsync(MakePutPath(testTask.Id), updatedTestTask);

			// Assert
			putResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
		}

		[Fact]
		public async Task Get_task_after_task_update_returns_updated_task()
		{
			// Arrange
			var httpClient = _webAppFactory.CreateClient();

			var testTask = new TaskPostModel(Guid.NewGuid(), "MyDesc", 1, Enumerable.Empty<Guid>());
			await httpClient.PostAsJsonAsync(MakePostPath(), testTask);

			var updatedTestTask = new TaskPutModel("MyDesc2", 2, Enumerable.Empty<Guid>());
			await httpClient.PutAsJsonAsync(MakePutPath(testTask.Id), updatedTestTask);

			// Act
			var resultTask = await httpClient.GetFromJsonAsync<TaskGetModel>(MakeGetPath(testTask.Id));

			// Assert
			resultTask.Should().BeEquivalentTo(new TaskGetModel(testTask.Id, updatedTestTask.Description, updatedTestTask.Priority, updatedTestTask.WorkersIds));
		}
	}
}
