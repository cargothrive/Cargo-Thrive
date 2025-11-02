using CargoThrive.Core.Models;
using CargoThrive.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoThrive.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;

        public TestController(ITestService testService)
        {
            _testService = testService;
        }
      
        // 获取所有租户
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestModel>>> GetTests()
        {
            var tests = await _testService.GetAllTestsAsync();
            return Ok(tests);
        }

        // 根据 ID 获取租户
        [HttpGet("{id}")]
        public async Task<ActionResult<TestModel>> GetTest(int id)
        {
            var tenant = await _testService.GetTestByIdAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }
            return Ok(tenant);
        }

        // 创建新租户
        [HttpPost]
        public async Task<ActionResult<TestModel>> CreateTest(TestModel tenant)
        {
            var newTest = await _testService.CreateTestAsync(tenant);
            return CreatedAtAction(nameof(GetTest), new { id = newTest.TenantId }, newTest);
        }

        // 更新租户
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTest(int id, TestModel test)
        {
            if (id != test.TenantId)
            {
                return BadRequest();
            }

            var updatedTest = await _testService.UpdateTestAsync(test);
            return Ok(updatedTest);
        }

        // 删除租户
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var success = await _testService.DeleteTestAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
