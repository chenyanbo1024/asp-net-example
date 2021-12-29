using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpressionTree.Model
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("address_id")]
        public int? AddressId { get; set; }

        public Address Address { get; set; }
    }
}
