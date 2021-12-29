using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTree.Extensions.v2
{
    /// <summary>
    /// 表达式扩展
    /// </summary>
    public static class ExpressionExtension
    {
        /// <summary>
        /// 扩展：表达式动态拼接
        /// </summary>
        public static IQueryable<T> Where<T>(this IQueryable<T> queryable, List<QueryEntity> entities)
        {
            if (entities.Count < 1)
            {
                return queryable;
            }
            ParameterExpression param = Expression.Parameter(typeof(T));

            var expression = CreateExpressionDelegate<T>(param, entities[0]);
            foreach (var entity in entities.Skip(1))
            {
                var tempExpression = CreateExpressionDelegate<T>(param, entity);
                expression = AddOrExpression(expression, tempExpression, entity.LogicalOperator);
            }
            var lambda = Expression.Lambda<Func<T, bool>>(expression, param);

            return queryable.Where(lambda);
        }

        /// <summary>
        /// 创建 Expression<TDelegate>
        /// </summary>
        private static Expression CreateExpressionDelegate<T>(ParameterExpression param,QueryEntity entity)
        {
            Expression key = param;
            var entityKey = entity.Key.Trim();
            // 包含'.'，说明是子类的字段
            if (entityKey.Contains('.'))
            {
                var tableNameAndField = entityKey.Split('.');
                key = Expression.Property(key, tableNameAndField[0].ToString());
                key = Expression.Property(key, tableNameAndField[1].ToString());
            }
            else
            {
                key = Expression.Property(key, entityKey);
            }

            Expression value = Expression.Constant(ParseType<T>(entity));
            return CreateExpression(key, value, entity.Operator);
        }

        /// <summary>
        /// 创建 Expression
        /// </summary>
        private static Expression CreateExpression(Expression left, Expression value, string entityOperator)
        {
            if (!Enum.TryParse(entityOperator, true, out OperatorEnum operatorEnum))
            {
                throw new ArgumentException("操作方法不存在,请检查operator的值");
            }

            return operatorEnum switch
            {
                OperatorEnum.Equals => Expression.Equal(left, value),
                OperatorEnum.NotEqual => Expression.NotEqual(left, value),
                OperatorEnum.Contains => Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), value),
                OperatorEnum.StartsWith => Expression.Call(left, typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), value),
                OperatorEnum.EndsWith => Expression.Call(left, typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), value),
                OperatorEnum.Greater => Expression.GreaterThan(left, value),
                OperatorEnum.GreaterEqual => Expression.GreaterThanOrEqual(left, value),
                OperatorEnum.Less => Expression.LessThan(left, value),
                OperatorEnum.LessEqual => Expression.LessThanOrEqual(left, value),
                _ => Expression.Equal(left, value),
            };
        }

        /// <summary>
        /// 条件操作（AND 或者 OR）
        /// </summary>
        /// <param name="left">左节点</param>
        /// <param name="right">右节点</param>
        /// <param name="logicalOperator">逻辑运算符，只支持AND、OR</param>
        /// <returns></returns>
        private static Expression AddOrExpression(Expression left,Expression right, string logicalOperator)
        {
            if ("OR".Equals(logicalOperator, StringComparison.OrdinalIgnoreCase))
            {
                return Expression.OrElse(left, right);
            }
            else
            {
                return Expression.AndAlso(left, right);
            }
        }

        /// <summary>
        /// 属性类型转换
        /// </summary>
        /// <param name="entity">查询实体</param>
        /// <returns></returns>
        private static object ParseType<T>(QueryEntity entity)
        {
            try
            {
                PropertyInfo property;
                // 包含'.'，说明是子类的字段
                if (entity.Key.Contains('.'))
                {
                    var tableNameAndField = entity.Key.Split('.');

                    property = typeof(T).GetProperty(tableNameAndField[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    property = property.PropertyType.GetProperty(tableNameAndField[1], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                }
                else
                {
                    property = typeof(T).GetProperty(entity.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                }

                return Convert.ChangeType(entity.Value, property.PropertyType);
            }
            catch (Exception)
            {
                throw new ArgumentException("字段类型转换失败：字段名错误或值类型不正确");
            }
        }
    }
}
