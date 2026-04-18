using Medicines.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Commands
{
    public record UpdateCommand(string Medicine, int PillsQuantity, int Hours, int Minutes): Command;
}
