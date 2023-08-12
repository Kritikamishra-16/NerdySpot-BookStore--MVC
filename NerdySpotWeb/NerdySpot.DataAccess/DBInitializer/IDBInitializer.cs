using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdySpot.DataAccess.DBInitializer
{
    public interface IDBInitializer
    {
        //this methd will be responsible for creating ADMIN users and ROLES in ur website
        void Initialize();
    }
}
