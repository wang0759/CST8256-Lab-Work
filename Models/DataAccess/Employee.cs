using System;
using System.Collections.Generic;

namespace Lab8.Models.DataAccess
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeeRole = new HashSet<EmployeeRole>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public virtual ICollection<EmployeeRole> EmployeeRole { get; set; }
    }
}
