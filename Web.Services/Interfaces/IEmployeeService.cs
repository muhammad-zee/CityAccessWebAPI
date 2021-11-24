//<<<<<<< Updated upstream
//﻿namespace Web.Services.Interfaces
//=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services
//>>>>>>> Stashed changes
{
    public interface IEmployeeService
    {
        string CreateEmployee();

        string UpdateEmployee();

        string GetAllEmployee();
    }
}
