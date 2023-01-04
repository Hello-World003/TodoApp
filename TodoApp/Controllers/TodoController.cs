using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodoController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public TodoController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.Items.ToListAsync();
            return Ok(items);
        }


        [HttpPost]
        public async Task<IActionResult> AddItem(ItemData itemData)
        {
            if (ModelState.IsValid)
            {
                await _context.Items.AddAsync(itemData);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetItem", new { itemData.Id }, itemData);
            }

            return new JsonResult("Something Went Wrong") { StatusCode = 500 };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem([FromRoute]int id)
        {
            var result = await _context.Items.FirstOrDefaultAsync(x=>x.Id==id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPut("id")]
        public async Task<IActionResult> Update([FromRoute]int id,[FromBody] ItemData itemData)
        {
            if (id != itemData.Id)
            {
                return BadRequest();
            }

            var result = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            result.Title = itemData.Title;
            result.Description = itemData.Description;
            result.Done = itemData.Done;

            await _context.SaveChangesAsync();
            
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var result = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (result == null)
            {
                return NotFound();
            }

             _context.Items.Remove(result);
             await _context.SaveChangesAsync();

             return Ok($"Remove {result}");
        }
    }
}