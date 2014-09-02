using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Insight.Database;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;

namespace InsightDBSprocFirst.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        readonly IDbConnection _db = new SqlConnection(ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString);

        public EmployeeRepository(bool clearCache = false)
        {
            if (clearCache)
                _db.ExecuteSql("CHECKPOINT;DBCC FREEPROCCACHE;DBCC DROPCLEANBUFFERS");
        }

        public Employee GetById(int employeeId)
        {
            return _db.Single<Employee>("GetEmployeeById", new {@Id = employeeId});
        }

        public IEnumerable<Employee> GetAll()
        {
            return _db.Query<Employee>("GetAllEmployees");
        }

        public Employee Add(Employee employee)
        {
            try
            {
                employee.Id = _db.Query<int>("InsertNewEmployee", employee).Single();
            }
            catch (SqlException slException)
            {
                return null;
            }
            return employee;
        }

        public Employee Update(Employee employee)
        {
            _db.Execute("UpdateEmployeeInfo", employee);
            return employee;
        }

        public void Remove(int id)
        {
            _db.Execute("RemoveEmployeeById", new {@EmployeeId = id});
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            var employee = _db.Single<Employee>("GetEmployeeById", new { @Id = employeeId });
            var addresses = _db.Query<Address>("GetAddressesByEmployeeId", new { @EmployeeId = employeeId });
            var assignedRoles = _db.Query<Role>("GetAssignedRolesByEmployeeId", new { @EmployeeId = employeeId });

            if (employee == null)
            {
                return null;
            }

            employee.Addresses.AddRange(addresses);
            employee.Roles.AddRange(assignedRoles);

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

        private void Add(Address address)
        {
            _db.Execute("InsertAddressForEmployee", address);
        }

        private void Update(Address address)
        {
            _db.Execute("UpdateAddress", address);
        }

        private void Add(int employeeId, int roleId)
        {
            _db.Execute("AddAssignedRole", new {@EmployeeId = employeeId, @RoleId = roleId});
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            _db.Execute("RemoveAssignedRole", new {@EmployeeId = employeeId, @RoleId = roleId});
        }
    }
}
