using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using MicroOrmComparison.UI.Enums;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;

namespace MicroOrmComparison.UI.Controllers
{
    public class EmployeeController : Controller
    {
        private IEmployeeRepository _employeeRepository;

        //factory
        //or switch :)
        public JsonResult EmployeeRepositoryResult(OrmTool ormTool, bool clearCache = false)
        {
            switch (ormTool)
            {
                case OrmTool.Dapper:
                    _employeeRepository = new Dapper.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.Entity:
                    _employeeRepository = new EntityFramework.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.HandrolledSprocs:
                    _employeeRepository = new HardCodedDataAccessWithSprocs.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.HandrolledSql:
                    _employeeRepository = new HardCodedDataAccessWithSql.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.InsightDbWithStoredProcedures:
                    _employeeRepository = new InsightDBSprocFirst.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.InsightDbWithSql:
                    _employeeRepository = new InsightDB.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.OrmLite:
                    _employeeRepository = new OrmLite.DataLayer.EmployeeRepository(clearCache);
                    break;
                case OrmTool.SimpleData:
                    _employeeRepository = new Simple.Data.DataLayer.EmployeeRepository(clearCache);
                    break;
                default:
                    _employeeRepository = null;
                    break;
            }


            var results = new Dictionary<string, string>();
            var getById = GetById();
            var add = Add();
            var getAll = GetAll();
            var update = Update();
            var remove = Remove();
            var getFullInfo = GetFullInfo();

            var sum = getById + getAll + add + update + remove + getFullInfo;
            results.Add("GetById", getById + "ms");
            results.Add("GetAll", getAll + "ms");
            results.Add("Add", add + "ms");
            results.Add("Update", update + "ms");
            results.Add("Remove", remove + "ms");
            results.Add("GetFullInfo", getFullInfo + "ms");
            results.Add("Average", (sum/6) + "ms");

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        private long GetById()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _employeeRepository.GetById(2);

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        private long GetAll()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _employeeRepository.GetAll();

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        private long Add()
        {
            var stopwatch = new Stopwatch();
            var employee = new Employee
            {
                FirstName = "TestAdd",
                LastName = "TestAdd",
                Email = "TestAdd@add.com",
                DepartmentId = 1
            };

            stopwatch.Start();

            _employeeRepository.Add(employee);

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        private long Update()
        {
            var stopwatch = new Stopwatch();
            var employeeToUpdate = _employeeRepository.GetById(5);
            employeeToUpdate.Email = "UpdatedEmail@yahoo.com";
            
            stopwatch.Start();

            _employeeRepository.Update(employeeToUpdate);

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        private long Remove()
        {
            var stopwatch = new Stopwatch();

            var employee = new Employee
            {
                FirstName = "TestAdd",
                LastName = "TestAdd",
                Email = "TestAdd@add.com",
                DepartmentId = 1
            };

            var idToRemove = _employeeRepository.Add(employee).Id;

            stopwatch.Start();

            _employeeRepository.Remove(idToRemove);

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        private long GetFullInfo()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            _employeeRepository.GetFullEmployeeInfo(1);

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }
    }
}