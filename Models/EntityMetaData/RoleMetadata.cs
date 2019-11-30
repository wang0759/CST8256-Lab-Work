using System.ComponentModel.DataAnnotations;

namespace Lab8.Models.DataAccess
{
    internal class RoleMetadata
    {
        [Display(Name = "Job Title")]
        public string Role1 { get; set; }
    }
}