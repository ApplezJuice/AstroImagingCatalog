using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroImagingCatalog
{
    class TestDatabase : IFITInformation
    {
        public IEnumerable<FITInformation> GetAllInformationInCatalog()
        {
            throw new NotImplementedException();
        }

        public FITInformation GetInformationByID(int id)
        {
            throw new NotImplementedException();
        }
    }
}
