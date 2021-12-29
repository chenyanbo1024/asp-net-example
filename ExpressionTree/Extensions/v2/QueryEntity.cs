namespace ExpressionTree.Extensions.v2
{
    public class QueryEntity
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 操作方法，对应OperatorEnum枚举类（Equals、Contains等）
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 逻辑运算符，只支持AND、OR
        /// </summary>
        public string LogicalOperator { get; set; }
    }
}
