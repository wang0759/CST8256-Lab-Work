using System;
using System.Collections.Generic;

namespace Lab8.Models.DataAccess
{
    public partial class Student
    {
        public Student()
        {
            AcademicRecord = new HashSet<AcademicRecord>();
            Registration = new HashSet<Registration>();
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AcademicRecord> AcademicRecord { get; set; }
        public virtual ICollection<Registration> Registration { get; set; }
    }
}
