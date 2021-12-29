using ExpressionTree.Extensions;
using ExpressionTree.Model;
using ExpressionTree.Model.EFCoreContext;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpressionTree.Controllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class ExpressionControllerV1: ControllerBase
    {
        private readonly CoreContext _context;

        public ExpressionControllerV1(CoreContext context)
        {
            _context = context;
        }


        /// <summary>
        /// 单条件查询
        /// </summary>
        [HttpGet("Test_A")]
        public async Task<IActionResult> Test_A()
        {
            List<Extensions.v1.QueryEntity> list = new()
            {
                new Extensions.v1.QueryEntity
                {
                    Key = "name",
                    Value = "chen",
                    Operator = "Contains"
                }
            };
            var expression = Extensions.v1.ExpressionExtension<User>.ExpressionSplice(list);
            // expression = Param_0 => Param_0.Name.Contains("chen") 

            var users = await _context.User.Where(expression).ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// 多条件查询
        /// </summary>
        [HttpGet("Test_B")]
        public async Task<IActionResult> Test_B()
        {
            List<Extensions.v1.QueryEntity> list = new List<Extensions.v1.QueryEntity>
            {
                new Extensions.v1.QueryEntity
                {
                    Key = "name",
                    Value = "chen",
                    Operator = "Contains"
                },
                new Extensions.v1.QueryEntity
                {
                    Key = "age",
                    Value = "18",
                    Operator = "GreaterEqual",
                    // 注意：这里得填入 "AND",代表两个条件是并且的关系，如果需要查询名称包含 "chen" 或者 年龄大于等于18，则填入 "OR"
                    LogicalOperator = "AND"
                }
            };
            var expression = Extensions.v1.ExpressionExtension<User>.ExpressionSplice(list);
            // expression = Param_0 => ((Param_0.Status >= Convert(1, Int32)) And Invoke(Param_1 => Param_1.OpenId.Contains("9JJdFTVt6oimCgdbW61sk"), Param_0))

            var users = await _context.User.Where(expression).ToListAsync();
            var users2 = await _context.User.Where(x => x.Name.Contains("chen") && x.Age >= 18).ToListAsync();


            return Ok(users);
        }

        /// <summary>
        /// 多表查询
        /// </summary>
        [HttpGet("Test_C")]
        public async Task<IActionResult> Test_C()
        {
            List<Extensions.v1.QueryEntity> list = new List<Extensions.v1.QueryEntity>
            {
                new Extensions.v1.QueryEntity
                {
                    Key = "name",
                    Value = "chen",
                    Operator = "Contains"
                },
                new Extensions.v1.QueryEntity
                {
                    Key = "address.Province",
                    Value = "广西省",
                    Operator = "Equals",
                    // 注意：这里得填入 "AND",代表两个条件是并且的关系，如果需要查询名称包含 "chen" 或者 年龄大于等于18，则填入 "OR"
                    LogicalOperator = "AND"
                }
            };
            var expression = Extensions.v1.ExpressionExtension<User>.ExpressionSplice(list);
            // expression = {Param_0 => ((Param_0.Address.Province == Convert("广东省", String)) And Invoke(Param_1 => Param_1.Name.Contains("chen"), Param_0))}

            var users = await _context.User.Where(expression).ToListAsync();

            return Ok(users);
        }
    }
}
