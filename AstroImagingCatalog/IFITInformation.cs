using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroImagingCatalog
{
    interface IFITInformation
    {
        FITInformation GetInformationByID(int id);
        IEnumerable<FITInformation> GetAllInformationInCatalog();
    }
}
