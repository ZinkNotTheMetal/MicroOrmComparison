using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;
using OrmLite.DataLayer.DTOs;
using OrmLite.DataLayer.Tests;
using ServiceStack.OrmLite;

namespace OrmLite.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnection _db;

        public EmployeeRepository(bool clearCache = false)
        {
            _db = CreateDbConnection();
            AutoMapperConfigurator.CreateMaps();

            if(clearCache)
                _db.ExecuteSql("CHECKPOINT;DBCC FREEPROCCACHE;DBCC DROPCLEANBUFFERS");
        }

        private static IDbConnection CreateDbConnection()
        {
            var dbFactory =
                new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString, SqlServerDialect.Provider);
            var db = dbFactory.OpenDbConnection();
            return db;
        }

        public IEnumerable<Employee> GetAll()
        {
            return AutoMapper.Mapper.Map<List<EmployeeDto>, List<Employee>>(_db.Select<EmployeeDto>());
        }

        public Employee GetById(int employeeId)
        {
            return AutoMapper.Mapper.Map<EmployeeDto, Employee>(_db.Where<EmployeeDto>(new { Id = employeeId }).SingleOrDefault());
        }

        public Employee Add(Employee employee)
        {
            var employeeForDatabase = AutoMapper.Mapper.Map<Employee, EmployeeDto>(employee);
            try
            {
                _db.InsertOnly(employeeForDatabase,
                    x => x.Insert(emp => new {emp.FirstName, emp.LastName, emp.Email, emp.DepartmentId}));
                employee.Id = (int) _db.LastInsertId();
            }
            catch (SqlException sqlException)
            {
                return null;
            }
            return employee;
        }

        public Employee Update(Employee employee)
        {
            var employeeForDatabase = AutoMapper.Mapper.Map<Employee, EmployeeDto>(employee);
            _db.Update(employeeForDatabase);
            return employee;
        }

        public void Remove(int id)
        {
            var roles = _db.Select<AssignedRolesDto>(ar => ar.EmployeeId == id);
            if (roles != null)
                _db.Delete<AssignedRolesDto>(ar => ar.EmployeeId == id);
            var addresses = _db.Select<AddressDto>(addr => addr.EmployeeId == id);
            if(addresses != null)
                _db.Delete<AddressDto>(addr => addr.EmployeeId == id);
            _db.DeleteById<EmployeeDto>(id);
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            var employee = AutoMapper.Mapper.Map<EmployeeDto, Employee>(_db.SingleById<EmployeeDto>(employeeId));
            var addresses = AutoMapper.Mapper.Map<List<AddressDto>, List<Address>>(_db.Select<AddressDto>(a => a.EmployeeId == employeeId));
            var roles = new List<RoleDto>();

            var result = _db.Select<AssignedRolesDto>(ar => ar.EmployeeId == employeeId);
            foreach (var role in result)
            {
                roles.Add(_db.Select<RoleDto>(r => r.Id == role.RoleId).Single());
            }

            if (employee != null && addresses != null)
            {
                employee.Addresses.AddRange(addresses);
            }
            if (employee != null && roles != null)
            {
                employee.Roles.AddRange(AutoMapper.Mapper.Map<List<RoleDto>, List<Role>>(roles));
            }
            return employee;
        }

        public void Save(Employee employee)
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
        }

        private void Add(int employeeId, int roleId)
        {
            _db.Insert(new AssignedRolesDto
            {
                EmployeeId = employeeId,
                RoleId = roleId
            });
        }

        private void Add(Address address)
        {
            var addressForDatabase = AutoMapper.Mapper.Map<Address, AddressDto>(address);
            _db.Insert(addressForDatabase);
        }

        private void Update(Address address)
        {
            var addressForDatabase = AutoMapper.Mapper.Map<Address, AddressDto>(address);
            _db.Update(addressForDatabase);
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            _db.Delete<AssignedRolesDto>(x => x.EmployeeId == employeeId && x.RoleId == roleId);
        }
    }
}
