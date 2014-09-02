using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;

namespace Simple.Data.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly dynamic _db;

        public EmployeeRepository(bool clearCache=false)
        {
            _db = Database.OpenNamedConnection("EmployeeDb");

            if (clearCache)
            {
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString))
                {
                    var command = new SqlCommand("CHECKPOINT;DBCC FREEPROCCACHE;DBCC DROPCLEANBUFFERS", connection);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
        }

        public Employee GetById(int employeeId)
        {
            Employee employee = _db.Employees.FindById(employeeId);
            if (employee == null)
            {
                return null;
            }
            return employee;
        }

        public IEnumerable<Employee> GetAll()
        {
            List<Employee> employees = _db.Employees.All();
            return employees;
        }

        public Employee Add(Employee employee)
        {
            var tx = _db.BeginTransaction(); // Internal IDbConnection opened by this call
            try
            {
                Employee addedContact = _db.Employees.Insert(employee);
                employee.Id = addedContact.Id;

                tx.Commit(); // Internal IDbConnection closed by this call...
            }
            catch
            {
                tx.Rollback();
                return null;
            }
            
            return employee;
        }

        public Employee Update(Employee employee)
        {
            _db.Employees.Update(employee);
            return employee;
        }

        public void Remove(int id)
        {
            Employee employeeToRemove = _db.Employees.Get(id);
            if (employeeToRemove.Addresses != null)
            {
                _db.Addresses.DeleteByEmployeeId(id);
            }
            if (employeeToRemove.Roles != null)
            {
                _db.AssignedRoles.DeleteByEmployeeId(id);
            }
            _db.Employees.DeleteById(id);
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            Employee employee = _db.Employees.WithAddresses().Get(employeeId);
            if (employee == null)
                return null;
            List<Role> assignedRoles = _db.Role.WithAssignedRoles()
                                               .Where(_db.AssignedRoles.EmployeeId == employeeId)
                                               .Where(_db.AssignedRoles.RoleId == _db.Role.Id)
                                               .Select(_db.Role.Id, _db.Role.Name);
                                                        

            if (assignedRoles.Count > 0)
            {
                employee.Roles.AddRange(assignedRoles);
            }
            return employee;
        }

        public void Save(Employee employee)
        {
            if (employee.Id == 0)
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
                if (address.Id == 0)
                {
                    AddAddress(address);
                }
                else
                {
                    UpdateAddress(address);
                }
            }
            if (employee.Roles != null)
            {
                foreach (var role in employee.Roles)
                {
                    AddRole(employee.Id, role.Id);
                }
            }
        }

        private void AddAddress(dynamic address)
        {
            _db.Addresses.Insert(address);
        }

        private void UpdateAddress(dynamic address)
        {
            _db.Addresses.Update(address);
        }

        private void AddRole(int employeeId, int roleId)
        {
            var existingRole = _db.AssignedRoles.FindByEmployeeIdAndRoleId(employeeId, roleId);
            if(existingRole == null)
                _db.AssignedRoles.Insert(EmployeeId: employeeId, RoleId: roleId);
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            _db.AssignedRoles.DeleteByEmployeeIdAndRoleId(employeeId, roleId);
        }
    }
}
