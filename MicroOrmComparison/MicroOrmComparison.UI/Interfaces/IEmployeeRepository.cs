using System.Collections.Generic;
using MicroOrmComparison.UI.Models;

namespace MicroOrmComparison.UI.Interfaces
{
    public interface IEmployeeRepository
    {
        //Returns an Employee by their Employee Id
        Employee GetById(int employeeId);
        //Returns all employees in the database
        IEnumerable<Employee> GetAll();
        //Add
        Employee Add(Employee employee);
        //Update
        Employee Update(Employee employee);
        //Remove
        void Remove(int id);
        //Get Full Employee
        Employee GetFullEmployeeInfo(int employeeId);
        //Save
        void Save(Employee employee);
        //Remove an Employee from an Assigned Role
        void RemoveRole(int employeeId, int roleId);
    }
}
