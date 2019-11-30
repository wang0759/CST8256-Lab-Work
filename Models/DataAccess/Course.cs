using System;
using System.Collections.Generic;

namespace Lab8.Models.DataAccess
{
    public partial class Course
    {
        public Course()
        {
            AcademicRecord = new HashSet<AcademicRecord>();
            Registration = new HashSet<Registration>();
        }

        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? HoursPerWeek { get; set; }
        public decimal? FeeBase { get; set; }

        public virtual ICollection<AcademicRecord> AcademicRecord { get; set; }
        public virtual ICollection<Registration> Registration { get; set; }
    }
}
