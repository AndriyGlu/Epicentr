using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Epicentr.DB;
using Microsoft.EntityFrameworkCore;

namespace Epicentr.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TasksController : ControllerBase
	{
		public TasksController(EpicentrDbContext dbcontext) => _dbcontext = dbcontext;

		// GET: api/<TasksController>
		[HttpGet]
		[ProducesResponseType(typeof(TaskGetModel), StatusCodes.Status200OK)]
		public async IAsyncEnumerable<TaskGetModel> Get()
		{
			await foreach (var task in _dbcontext.Tasks.AsAsyncEnumerable())
				yield return new TaskGetModel(task.Id, task.Description, (int)task.Priority, Enumerable.Empty<Guid>());
		}

		// GET api/<TasksController>/5
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(TaskGetModel), StatusCodes.Status200OK)]
		public ValueTask<IActionResult> Get(Guid id)
		{
			var task = _dbcontext.Tasks.Where(t => t.Id == id).FirstOrDefault();
			return ValueTask.FromResult(task != null ? Ok(new TaskGetModel(task.Id, task.Description, (int)task.Priority, Enumerable.Empty<Guid>())) : (IActionResult)NotFound());
		}

		// GET api/<TasksController>/5/workers
		[HttpGet("{id}/workers")]
		[ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IAsyncEnumerator<WorkerGetModel>> GetWorkers(Guid id)
		{
			var taskWorkers = _dbcontext.Tasks.Where(t => t.Id == id).Select(t => new { Id = t.Id, WorkersIds = t.Workers.Select(w => w.Id) }).FirstOrDefault();
			return taskWorkers == null ? (ActionResult)NotFound() : Ok(taskWorkers.WorkersIds);
		}

		// POST api/<TasksController>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(TaskGetModel), StatusCodes.Status201Created)]
		public async ValueTask<IActionResult> Post([FromBody] TaskPostModel value)
		{
			if (_dbcontext.Tasks.Where(t => t.Id == value.Id).FirstOrDefault() != null)
				return Conflict();

			var task = new DB.Task(value.Id, value.Description, (Priority)value.Priority);
			_dbcontext.Tasks.Add(task);
			await _dbcontext.SaveChangesAsync();

			var taskGetModel = new TaskGetModel(task.Id, task.Description, (int)task.Priority, Enumerable.Empty<Guid>());
			return CreatedAtAction(nameof(Get), new { id = taskGetModel.Id }, taskGetModel);
		}

		// PATCH api/<TasksController>/{id}/workers
		//[HttpPatch("{id}/workers")]
		//public ValueTask<IActionResult> Patch(Guid id, [FromBody]  workersIds)
		//{
		//	throw new NotImplementedException();
		//}

		// PUT api/<TasksController>/5
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async ValueTask<IActionResult> Put(Guid id, [FromBody] TaskPutModel value)
		{
			var task = _dbcontext.Tasks.Where(t => t.Id == id).FirstOrDefault();
			if (task == null)
				return NotFound();

			task.Priority = (Priority)value.Priority;
			task.Description = value.Description;
			foreach (var worker in _dbcontext.Workers.AsNoTracking().Where(w => value.WorkersIds.Contains(w.Id)))
				task.Workers.Add(worker);

			await _dbcontext.SaveChangesAsync();
			return NoContent();
		}

		// DELETE api/<TasksController>/5
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async ValueTask<IActionResult> Delete(Guid id)
		{
			var task = _dbcontext.Tasks.Where(t => t.Id == id).FirstOrDefault();
			if (task == null)
				return NotFound();

			_dbcontext.Tasks.Remove(task);
			await _dbcontext.SaveChangesAsync();

			return NoContent();
		}

		private readonly EpicentrDbContext _dbcontext;
	}
}
