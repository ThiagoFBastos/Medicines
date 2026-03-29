using Medicines.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Medicines.Models
{
    [Table("medicines")]
    public class Medicine: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public int PillsQuantity { get; set; } = 0;

        public required long UserId { get; set; }

        public required DateTimeOffset ScheduledTime { get; set; }

        public DateTimeOffset RegisteredDate { get; set; } = DateTimeOffset.UtcNow;

        public bool IsValid()
        {
            var match = Regex.Match(Name, @"^[A-Za-z\s]+$");

            return !string.IsNullOrEmpty(Name) && PillsQuantity >= 0 && match.Success;
        }
    }
}
