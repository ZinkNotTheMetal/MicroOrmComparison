using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using MicroOrmComparison.UI.Interfaces;

namespace EntityFramework.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public EmployeeRepository(bool clearCache = false)
        {
            Automapper.AutoMapperConfigurator.CreateMaps();

            if (clearCache)
            {
                using (var db = new EmployeeDb())
                {
                    db.Database.ExecuteSqlCommand("CHECKPOINT;DBCC FREEPROCCACHE;DBCC DROPCLEANBUFFERS");
                }
            }
        }

        public MicroOrmComparison.UI.Models.Employee GetById(int employeeId)
        {
            Employee employee = null;
            using (var db = new EmployeeDb())
            {
                db.Configuration.LazyLoadingEnabled = false;
                employee = db.Employees.FirstOrDefault(x => x.Id == employeeId);
            }
            return AutoMapper.Mapper.Map<Employee, MicroOrmComparison.UI.Models.Employee>(employee);
        }

        public IEnumerable<MicroOrmComparison.UI.Models.Employee> GetAll()
        {
            List<Employee> listOfEmployees;
            using (var db = new EmployeeDb())
            {
                db.Configuration.LazyLoadingEnabled = false;
                listOfEmployees = db.Employees.ToList();
            }
            return AutoMapper.Mapper.Map<List<Employee>, List<MicroOrmComparison.UI.Models.Employee>>(listOfEmployees);
        }

        public MicroOrmComparison.UI.Models.Employee Add(MicroOrmComparison.UI.Models.Employee employee)
        {
            var employeeToInsert = AutoMapper.Mapper.Map<MicroOrmComparison.UI.Models.Employee, Employee>(employee);
            using (var db = new EmployeeDb())
            {
                db.Employees.AddOrUpdate(employeeToInsert);
                if (employeeToInsert.Addresses != null)
                {
                    foreach (var address in employeeToInsert.Addresses)
                    {
                        db.Addresses.AddOrUpdate(address);
                    }
                }
                if (employeeToInsert.AssignedRoles != null)
                {
                    foreach (var roleId in employeeToInsert.AssignedRoles.Select(x => x.Id).Distinct())
                    {
                        var roleToInsert = new AssignedRole
                        {
                            EmployeeId = employeeToInsert.Id,
                            RoleId = roleId
                        };
                        db.AssignedRoles.Add(roleToInsert);
                    }
                }
                db.SaveChanges();
                employee.Id = employeeToInsert.Id;
            }
            return employee;
        }

        public MicroOrmComparison.UI.Models.Employee Update(MicroOrmComparison.UI.Models.Employee employee)
        {
            return Add(employee);
        }

        public void Remove(int id)
        {
            using (var db = new EmployeeDb())
            {
                var employeeToRemove = db.Employees.First(x => x.Id == id);
                if (employeeToRemove.Addresses != null)
                {
                    db.Addresses.RemoveRange(employeeToRemove.Addresses);
                }
                if (employeeToRemove.AssignedRoles != null)
                {
                    db.AssignedRoles.RemoveRange(employeeToRemove.AssignedRoles);
                }
                db.Employees.Remove(employeeToRemove);
                db.SaveChanges();
            }
        }

        public MicroOrmComparison.UI.Models.Employee GetFullEmployeeInfo(int employeeId)
        {
            Employee employeeToShow = null;

            using (var db = new EmployeeDb())
            {
                db.Configuration.LazyLoadingEnabled = false;
                employeeToShow = db.Employees.FirstOrDefault(x => x.Id == employeeId);
                if (employeeToShow != null)
                {
                    employeeToShow.Addresses = db.Addresses.Where(x => x.EmployeeId == employeeId).ToList();
                    employeeToShow.AssignedRoles = db.AssignedRoles.Where(x => x.EmployeeId == employeeId).ToList();
                }
            }

            return AutoMapper.Mapper.Map<Employee, MicroOrmComparison.UI.Models.Employee>(employeeToShow);

        }

        public void Save(MicroOrmComparison.UI.Models.Employee employee)
        {
            Add(employee);
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            using (var db = new EmployeeDb())
            {
                db.AssignedRoles.Remove(db.AssignedRoles.FirstOrDefault(ar => ar.RoleId == roleId && ar.EmployeeId == employeeId));                
                db.SaveChanges();
            }
        }
    }
}
