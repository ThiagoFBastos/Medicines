using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicines.Interfaces
{
    public interface ICommandExtraction
    {
        Command Extract(string text);
    }
}
