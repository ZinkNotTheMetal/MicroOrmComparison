using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Insight.Database;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;

namespace InsightDB.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnection _db;

        public EmployeeRepository(bool clearCache = false)
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString);
        }


        public Employee GetById(int employeeId)
        {
            return _db.SingleSql<Employee>("Select * FROM Employees Where Id = @Id", new { @Id = employeeId });
        }

        public IEnumerable<Employee> GetAll()
        {
            return _db.QuerySql<Employee>("Select * FROM Employees");
        }

        public Employee Add(Employee employee)
        {
            const string sql = "INSERT INTO Employees (FirstName, LastName, Email, DepartmentId) VALUES (@FirstName, @LastName, @Email, @DepartmentId);" +
                               "SELECT CAST(SCOPE_IDENTITY() as int)";
            try
            {
                employee.Id = _db.QuerySql<int>(sql, employee).Single();
            }
            catch (SqlException slException)
            {
                return null;
            }
            return employee;
        }

        public Employee Update(Employee employee)
        {
            const string sql = "UPDATE Employees " +
                               "SET FirstName = @FirstName, " +
                               "     LastName = @LastName, " +
                               "        Email = @Email, " +
                               " DepartmentId = @DepartmentId " +
                               "     WHERE Id = @Id";
            _db.ExecuteSql(sql, employee);
            return employee;
        }

        private void Add(Address address)
        {
            const string sql =
                "INSERT INTO Addresses (EmployeeId, StreetAddress, City, StateId, ZipCode) VALUES (@EmployeeId, @StreetAddress, @City, @StateId, @ZipCode);";
            _db.ExecuteSql(sql, address);
        }

        private void Update(Address address)
        {
            _db.ExecuteSql("UPDATE Addresses " +
                        "SET StreetAddress = @StreetAddress, " +
                        "City = @City, " +
                        "StateId = @StateId, " +
                        "ZipCode = @ZipCode " +
                        "WHERE Id = @Id", address);
        }

        private void Add(int employeeId, int roleId)
        {
            const string sql = "INSERT INTO AssignedRoles (RoleId, EmployeeId) VALUES (@RoleId, @EmployeeId)";
            _db.ExecuteSql(sql, new {@EmployeeId = employeeId, @RoleId = roleId});
        }

        public void Remove(int id)
        {
            const string sql = "SELECT * FROM Employees WHERE Id = @Id;" +
                               "SELECT * FROM Addresses WHERE EmployeeId = @Id;" +
                               "SELECT [Role].Id, [Role].Name FROM AssignedRoles, [Role] " +
                               "WHERE [Role].Id = AssignedRoles.RoleId AND " +
                               "AssignedRoles.EmployeeId = @Id;";
            var results = _db.QueryResultsSql<Employee, Address, Role>(sql, new { @Id = id });
            
            if (results.Set2 != null)
            {
                _db.ExecuteSql("DELETE FROM Addresses WHERE EmployeeId = @Id", new {id});
            }
            if (results.Set3 != null)
            {
                _db.ExecuteSql("DELETE FROM AssignedRoles WHERE EmployeeId = @Id", new {id});
            }
            _db.ExecuteSql("DELETE FROM Employees WHERE Id = @Id", new {id});
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            const string sql = "SELECT * FROM Employees WHERE Id = @Id;" +
                               "SELECT * FROM Addresses WHERE EmployeeId = @Id;" +
                               "SELECT Role.Id, Role.Name FROM AssignedRoles, Role " +
                               "WHERE Role.Id = AssignedRoles.RoleId AND " +
                               "AssignedRoles.EmployeeId = @Id;";
            var results = _db.QueryResultsSql<Employee, Address, Role>(sql, new {@Id = employeeId});
            var employee = results.Set1.FirstOrDefault();
            if (employee == null)
            {
                return null;
            }

            employee.Addresses.AddRange(results.Set2);
            employee.Roles.AddRange(results.Set3);

            return employee;
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
                foreach (var roleId in employee.Roles.Select(x => x.Id).Distinct())
                {
                    Add(employee.Id, roleId);
                }
                transactionScope.Complete();
            }
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            const string sql = "DELETE FROM AssignedRoles WHERE EmployeeId = @EmployeeId AND RoleId = @RoleId";
            _db.ExecuteSql(sql, new { employeeId, roleId });
        }
    }
}
