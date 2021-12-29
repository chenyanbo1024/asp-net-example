using System.Linq;

using ExpressionTree.Extensions.v2;
using ExpressionTree.Model;
using ExpressionTree.Model.EFCoreContext;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpressionTree.Controllers
{
    [Route("v2/api/[controller]")]
    [ApiController]
    public class ExpressionControllerV2 : ControllerBase
    {
        private readonly CoreContext _context;

        public ExpressionControllerV2(CoreContext context)
        {
            _context = context;
        }


        /// <summary>
        /// 单条件查询
        /// </summary>
        [HttpGet("Test_A")]
        public async Task<IActionResult> Test_A()
        {
            List<Extensions.v2.QueryEntity> list = new()
            {
                new Extensions.v2.QueryEntity
                {
                    Key = "name",
                    Value = "chen",
                    Operator = "Contains"
                }
            };

            var users = await _context.User.Where(list).ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// 多条件查询
        /// </summary>
        [HttpGet("Test_B")]
        public async Task<IActionResult> Test_B()
        {
            List<Extensions.v2.QueryEntity> list = new List<Extensions.v2.QueryEntity>
            {
                new Extensions.v2.QueryEntity
                {
                    Key = "name",
                    Value = "chen",
                    Operator = "Contains"
                },
                new Extensions.v2.QueryEntity
                {
                    Key = "age",
                    Value = "18",
                    Operator = "GreaterEqual",
                    // 注意：这里得填入 "AND",代表两个条件是并且的关系，如果需要查询名称包含 "chen" 或者 年龄大于等于18，则填入 "OR"
                    LogicalOperator = "AND"
                }
            };
            var users = await _context.User.Where(list).ToListAsync();
            var users2 = await _context.User.Where(x => x.Name.Contains("chen") && x.Age >= 18).ToListAsync();


            return Ok(users);
        }

        /// <summary>
        /// 多表查询
        /// </summary>
        [HttpGet("Test_C")]
        public async Task<IActionResult> Test_C()
        {
            List<Extensions.v2.QueryEntity> list = new List<Extensions.v2.QueryEntity>
            {
                new Extensions.v2.QueryEntity
                {
                    Key = "name",
                    Value = "chen",
                    Operator = "Contains"
                },
                new Extensions.v2.QueryEntity
                {
                    Key = "address.Province",
                    Value = "广西省",
                    Operator = "Equals",
                    // 注意：这里得填入 "AND",代表两个条件是并且的关系，如果需要查询名称包含 "chen" 或者 年龄大于等于18，则填入 "OR"
                    LogicalOperator = "AND"
                }
            };
            var users = await _context.User.Where(list).ToListAsync();

            return Ok(users);
        }
    }
}
