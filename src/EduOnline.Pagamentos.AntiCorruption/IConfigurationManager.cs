using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduOnline.Pagamentos.AntiCorruption;

public interface IConfigurationManager
{
    string GetValue(string node);
}
