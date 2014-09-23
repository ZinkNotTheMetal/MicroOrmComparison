using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;
using TransactionScope = System.Transactions.TransactionScope;

namespace Dapper.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnection _db;

        public EmployeeRepository()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString);
        } 

        public Employee GetById(int employeeId)
        {
            return _db.Query<Employee>("SELECT * FROM Employees WHERE Id = @Id", new { @Id = employeeId }).SingleOrDefault();
        }

        public IEnumerable<Employee> GetAll()
        {
            return _db.Query<Employee>("SELECT * FROM Employees").ToList();
        }

        public void Remove(int id)
        {
            const string sql = "SELECT * FROM Employees WHERE Id = @Id; " +
                //What Addresses does the Employee Have
                   "SELECT * FROM Addresses WHERE EmployeeId = @Id; " +
                //What Roles does the Employee Have
                   "SELECT AssignedRoles.RoleId, [Role].Name FROM AssignedRoles " +
                            "JOIN [Role] " +
                            "ON AssignedRoles.RoleId = [Role].Id " +
                            "WHERE AssignedRoles.EmployeeId = @Id";

            using (var multipleResults = _db.QueryMultiple(sql, new { @Id = id }))
            {
                var employee = multipleResults.Read<Employee>().SingleOrDefault();
                var addresses = multipleResults.Read<Address>().ToList();
                var roles = multipleResults.Read<Role>().ToList();

                if (employee != null && addresses != null)
                {
                    foreach (var address in addresses)
                    {
                        _db.Execute("DELETE FROM Addresses Where Id = @Id", new {address.Id});
                    }
                }

                if (employee != null && roles != null)
                {
                    foreach (var role in roles)
                    {
                        _db.Execute("DELETE FROM AssignedRoles WHERE EmployeeId = @Id", new {employee.Id});
                    }
                }
            }

            _db.Execute("DELETE FROM Employees WHERE Id = @Id", new { id });
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            const string sql = "SELECT * FROM Employees WHERE Id = @Id; " +
                               //What Addresses does the Employee Have
                               "SELECT * FROM Addresses WHERE EmployeeId = @Id; " +
                               //What Roles does the Employee Have
                               "SELECT Role.Id, Role.Name FROM AssignedRoles, Role " +
                                        "WHERE Role.Id = AssignedRoles.RoleId AND " +
                                        "AssignedRoles.EmployeeId = @Id;";

            using (var multipleResults = _db.QueryMultiple(sql, new {@Id = employeeId}))
            {
                var employee = multipleResults.Read<Employee>().SingleOrDefault();
                var addresses = multipleResults.Read<Address>().ToList();
                var roles = multipleResults.Read<Role>().ToList();

                if (employee != null && addresses != null)
                {
                    employee.Addresses.AddRange(addresses);
                }

                if (employee != null && roles != null)
                {
                    employee.Roles.AddRange(roles);
                }

                return employee;
            }
        }

        public void Save(Employee employee)
        {
            using (var transactionScope = new TransactionScope())
            {
                if (employee.IsNew)
                {
                    Add(employee);
                }
                else
                {
                    Update(employee);
                }

                foreach (var address in employee.Addresses)
                {
                    address.EmployeeId = employee.Id;
                    if (address.IsNew)
                    {
                        Add(address);
                    }
                    else
                    {
                        Update(address);
                    }
                }
                foreach (var roleId in employee.Roles.Select(x=>x.Id).Distinct())
                {
                    Add(employee.Id, roleId);
                }
                transactionScope.Complete();
            }
        }

        public Employee Update(Employee employee)
        {
            const string sql = "UPDATE Employees " +
                               "SET FirstName = @FirstName, " +
                               "     LastName = @LastName, " +
                               "     Email = @Email, " +
                               " DepartmentId = @DepartmentId " +
                               "     WHERE Id = @Id";
            _db.Execute(sql, employee);
            return employee;
        }

        public Employee Add(Employee employee)
        {
            const string sql = "INSERT INTO Employees (FirstName, LastName, Email, DepartmentId) VALUES (@FirstName, @LastName, @Email, @DepartmentId);" +
                               "SELECT CAST(SCOPE_IDENTITY() as int)";
            try
            {
                var id = _db.Query<int>(sql, employee).Single();
                employee.Id = id;
            }
            catch (SqlException sqlException)
            {
                return null;
            }
            return employee;
        }

        private void Add(Address address)
        {
            const string sql =
                "INSERT INTO Addresses (EmployeeId, StreetAddress, City, StateId, ZipCode) VALUES (@EmployeeId, @StreetAddress, @City, @StateId, @ZipCode);";
            _db.Execute(sql, address);
        }

        private void Update(Address address)
        {
            _db.Execute("UPDATE Addresses " +
                        "SET StreetAddress = @StreetAddress, " +
                        "City = @City, " +
                        "StateId = @StateId, " +
                        "ZipCode = @ZipCode " +
                        "WHERE Id = @Id", address);
        }

        private void Add(int employeeId, int roleId)
        {
            const string sql = "INSERT INTO AssignedRoles (RoleId, EmployeeId) VALUES (@RoleId, @EmployeeId)";
            _db.Execute(sql, new {@EmployeeId = employeeId, @RoleId = roleId});
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            const string sql = "DELETE FROM AssignedRoles WHERE EmployeeId = @EmployeeId AND RoleId = @RoleId";
            _db.Execute(sql, new {@EmployeeId = employeeId, @RoleId = roleId});
        }
    }
}
