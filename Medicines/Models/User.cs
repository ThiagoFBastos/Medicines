using Medicines.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Models
{
    [Table("users")]
    public class User: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public required long UserId { get; set; }

        public required string Username { get; set; }

        public DateTimeOffset registeredDate { get; set; } = DateTimeOffset.UtcNow;
    }
}
