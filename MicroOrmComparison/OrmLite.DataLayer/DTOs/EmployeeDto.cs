using System.Collections.Generic;

namespace OrmLite.DataLayer.DTOs
{
    [ServiceStack.DataAnnotations.Alias("Employees")]
    internal class EmployeeDto
    {
        [ServiceStack.DataAnnotations.PrimaryKey]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int DepartmentId { get; set; }

        [ServiceStack.DataAnnotations.Ignore]
        public List<AddressDto> Addresses { get; set; }

        [ServiceStack.DataAnnotations.Ignore]
        public List<RoleDto> Roles { get; set; } 
    }
}
