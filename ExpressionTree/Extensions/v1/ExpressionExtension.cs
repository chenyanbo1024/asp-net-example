using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTree.Extensions.v1
{
    /// <summary>
    /// 表达式扩展
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public static class ExpressionExtension<T> where T : class, new()
    {
        /// <summary>
        /// 表达式动态拼接
        /// </summary>
        public static Expression<Func<T, bool>> ExpressionSplice(List<QueryEntity> entities)
        {
            if (entities.Count < 1)
            {
                return ex => true;
            }
            var expression_first = CreateExpressionDelegate(entities[0]);
            foreach (var entity in entities.Skip(1))
            {
                var expression = CreateExpressionDelegate(entity);
                InvocationExpression invocation = Expression.Invoke(expression_first, expression.Parameters.Cast<Expression>());
                BinaryExpression binary;
                // 逻辑运算符判断
                if (entity.LogicalOperator.ToUpper().Equals("OR"))
                {
                    binary = Expression.Or(expression.Body, invocation);
                }
                else
                {
                    binary = Expression.And(expression.Body, invocation);
                }
                expression_first = Expression.Lambda<Func<T, bool>>(binary, expression.Parameters);
            }
            return expression_first;
        }

        /// <summary>
        /// 创建 Expression<TDelegate>
        /// </summary>
        private static Expression<Func<T, bool>> CreateExpressionDelegate(QueryEntity entity)
        {
            ParameterExpression param = Expression.Parameter(typeof(T));

            Expression key = param;
            var entityKey = entity.Key.Trim();
            // 包含'.'，说明是父表的字段
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

            Expression value = Expression.Constant(ParseType(entity));
            Expression body = CreateExpression(key, value, entity.Operator);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return lambda;
        }

        /// <summary>
        /// 属性类型转换
        /// </summary>
        /// <param name="entity">查询实体</param>
        /// <returns></returns>
        private static object ParseType(QueryEntity entity)
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
                OperatorEnum.Equals => Expression.Equal(left, Expression.Convert(value, left.Type)),
                OperatorEnum.NotEqual => Expression.NotEqual(left, Expression.Convert(value, left.Type)),
                OperatorEnum.Contains => Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), value),
                OperatorEnum.StartsWith => Expression.Call(left, typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), value),
                OperatorEnum.EndsWith => Expression.Call(left, typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), value),
                OperatorEnum.Greater => Expression.GreaterThan(left, Expression.Convert(value, left.Type)),
                OperatorEnum.GreaterEqual => Expression.GreaterThanOrEqual(left, Expression.Convert(value, left.Type)),
                OperatorEnum.Less => Expression.LessThan(left, Expression.Convert(value, left.Type)),
                OperatorEnum.LessEqual => Expression.LessThanOrEqual(left, Expression.Convert(value, left.Type)),
                _ => Expression.Equal(left, Expression.Convert(value, left.Type)),
            };
        }
    }

}
