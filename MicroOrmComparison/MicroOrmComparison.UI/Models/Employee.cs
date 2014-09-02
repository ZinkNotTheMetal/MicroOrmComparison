using System.Collections.Generic;

namespace MicroOrmComparison.UI.Models
{
    public class Employee
    {
        public Employee()
        {
            Addresses = new List<Address>();
            Roles = new List<Role>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int DepartmentId { get; set; }

        public List<Address> Addresses { get; private set; }

        public List<Role> Roles { get; private set; } 

        public bool IsNew
        {
            get { return Id == default(int); }
        }
    }
}