using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Epicentr.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Epicentr.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WorkersController : ControllerBase
	{
		public WorkersController(EpicentrDbContext dbcontext) => _dbcontext = dbcontext;

		// GET: api/<WorkersController>
		[HttpGet]
		[ProducesResponseType(typeof(WorkerGetModel), StatusCodes.Status200OK)]
		public IAsyncEnumerable<WorkerGetModel> Get()
		{
			return _dbcontext.Workers.Select(w => new WorkerGetModel(w.Id, w.FirstName, w.LastName, w.Tasks.Select(t => t.Id))).AsAsyncEnumerable();
		}

		// GET api/<WorkersController>/5
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(WorkerGetModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ValueTask<IActionResult> Get(Guid id)
		{
			var worker = _dbcontext.Workers.Where(w => w.Id == id).Select(w => new WorkerGetModel(w.Id, w.FirstName, w.LastName, w.Tasks.Select(t => t.Id))).FirstOrDefault();
			return ValueTask.FromResult(worker == null ? (IActionResult)NotFound() : Ok(worker));
		}

		// GET api/<WorkersController>/5/tasks
		[HttpGet("{id}/tasks")]
		[ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<IAsyncEnumerator<TaskGetModel>> GetTasks(Guid id)
		{
			var workerTasks = _dbcontext.Workers.Where(w => w.Id == id).Select(w => new { Id = w.Id, TasksIds = w.Tasks.Select(t => t.Id) }).FirstOrDefault();
			return workerTasks == null ? (ActionResult)NotFound() : Ok(workerTasks.TasksIds);
		}

		// POST api/<WorkersController>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(WorkerGetModel), StatusCodes.Status201Created)]
		public async ValueTask<IActionResult> Post([FromBody] WorkerPostModel value)
		{
			if (_dbcontext.Workers.Where(w => w.Id == value.Id).FirstOrDefault() != null)
				return Conflict();

			var worker = new Worker(value.Id, value.FirstName, value.LastName);
			_dbcontext.Workers.Add(worker);
			await _dbcontext.SaveChangesAsync();

			return CreatedAtAction(nameof(Get), new { id = worker.Id }, worker);
		}

		// PUT api/<WorkersController>/5
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async ValueTask<IActionResult> Put(Guid id, [FromBody] WorkerPutModel value)
		{
			var worker = _dbcontext.Workers.Where(w => w.Id == id).FirstOrDefault();
			if (worker == null)
				return NotFound();

			worker.FirstName = value.FirstName;
			worker.LastName = value.LastName;
			foreach (var task in _dbcontext.Tasks.Where(t => value.TasksIds.Contains(t.Id)).AsNoTracking())
				worker.Tasks.Add(task);

			await _dbcontext.SaveChangesAsync();
			return NoContent();
		}

		// DELETE api/<WorkersController>/5
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async ValueTask<IActionResult> Delete(Guid id)
		{
			var worker = _dbcontext.Workers.Where(w => w.Id == id).FirstOrDefault();
			if (worker == null)
				return NotFound();

			_dbcontext.Workers.Remove(worker);
			await _dbcontext.SaveChangesAsync();

			return NoContent();
		}

		private readonly EpicentrDbContext _dbcontext;
	}
}
