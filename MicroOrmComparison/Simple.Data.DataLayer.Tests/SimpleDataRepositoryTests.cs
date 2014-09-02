using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using MicroOrmComparison.UI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Data.DataLayer.Tests
{
    [TestClass]
    [DeploymentItem("Simple.Data.Ado.dll")]
    [DeploymentItem("Simple.Data.SqlServer.dll")]
    public class SimpleDataRepositoryTests
    {
        private EmployeeRepository _employeeRepository;

        [TestMethod]
        public void ShouldReturnCorrectCountFromDatabase()
        {
            const string sql = "SELECT COUNT(*) FROM Employees";
            var expectedResult = 0;

            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString);
            conn.Open();

            using (var command = new SqlCommand(sql, conn))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    expectedResult = reader.GetInt32(0);
                }
            }

            conn.Close();
            //Act
            var result = _employeeRepository.GetAll();
            //Assert
            result.Count().Should().Be(expectedResult);
        }

        [TestMethod]
        public void ShouldReturnCorrectEmployeeFromId()
        {
            //Act
            var result = _employeeRepository.GetById(2);
            //Assert
            Assert.AreEqual("Bill", result.FirstName);
            Assert.AreEqual("Burr", result.LastName);
            Assert.AreEqual(2, result.DepartmentId);
            Assert.AreEqual("bill.burr@hilarious.com", result.Email);
        }

        [TestMethod]
        public void GetAllEmployeeInfoShouldReturnNull()
        {
            //Act
            var result = _employeeRepository.GetFullEmployeeInfo(int.MaxValue);
            //Arrange
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ShouldInsertEmployeeIntoDatabase()
        {
            //Arrange
            var employee = new Employee
            {
                FirstName = "Test",
                LastName = "Dummy",
                Email = "wh@tisanemail.com",
                DepartmentId = 5,
            };

            //Act
            _employeeRepository.Add(employee);

            //Assert
            employee.Id.Should().NotBe(0, "Identity should be assigned by the database");
        }

        [TestMethod]
        public void ShouldModifyEmployeeToDatabase()
        {
            var testUserId = InsertSimpleUserToBeModified();

            var testContact = _employeeRepository.GetById(testUserId);
            testContact.LastName = "Jones";

            //Act
            _employeeRepository.Update(testContact);

            //Assert
            Assert.AreEqual("Jones", testContact.LastName);

            RemoveUserToBeModified(testUserId);
        }

        [TestMethod]
        public void ShouldRemoveSimpleEmployeeFromDatabase()
        {
            var testUserId = InsertSimpleUserToBeModified();

            //Act
            _employeeRepository.Remove(testUserId);
            //Assert
            var tempRepository = new EmployeeRepository();
            var result = tempRepository.GetById(testUserId);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ShouldInsertAddressAsWellAsContactUsingSaveToDatabase()
        {
            var employee = new Employee
            {
                FirstName = "Quality",
                LastName = "Assurance",
                Email = "quality@assurance.com",
                DepartmentId = 5,
            };

            var address = new Address
            {
                StreetAddress = "1 Infinite Loop",
                StateId = 5,
                City = "Cupertino",
                ZipCode = "95014"
            };
            employee.Addresses.Add(address);

            //Act
            _employeeRepository.Save(employee);

            //Assert
            employee.Id.Should().NotBe(0, "Identity should be assigned by the database");
            employee.Addresses.Count().Should().NotBe(0);
            employee.Addresses[0].StreetAddress.Should().Be("1 Infinite Loop");
        }

        [TestMethod]
        public void ShouldReturnFullContactFromDatabase()
        {
            //Act
            var result = _employeeRepository.GetFullEmployeeInfo(2);
            //Assert
            Assert.AreEqual("Bill", result.FirstName);
            Assert.AreEqual("Burr", result.LastName);
            Assert.AreEqual(2, result.DepartmentId);
            Assert.AreEqual("bill.burr@hilarious.com", result.Email);
            Assert.IsNotNull(result.Addresses);
            Assert.AreEqual(1, result.Addresses.Count);
            Assert.AreEqual("Los Angeles", result.Addresses.First().City);
            Assert.AreEqual(5, result.Addresses.First().StateId);
            Assert.AreEqual("90020", result.Addresses.First().ZipCode);
            Assert.AreEqual("123 Main Street", result.Addresses.First().StreetAddress);
            Assert.IsNotNull(result.Roles);
            Assert.AreEqual(3, result.Roles.First().Id);
        }

        [TestMethod]
        public void ShouldModifyAddressAsWellAsContactUsingSaveToDatabase()
        {
            //Arrange
            int testUserId = InsertUserToBeModified();

            Employee testContact = _employeeRepository.GetFullEmployeeInfo(testUserId);
            testContact.LastName = "Jones";
            testContact.Addresses.First().StreetAddress = "345 Main Street";

            //Act
            _employeeRepository.Save(testContact);

            //Assert
            Assert.AreEqual("Jones", _employeeRepository.GetFullEmployeeInfo(testUserId).LastName);
            Assert.AreEqual("345 Main Street",
                _employeeRepository.GetFullEmployeeInfo(testUserId).Addresses[0].StreetAddress);
            //Cleanup
            RemoveUserToBeModified(testUserId);
        }

        [TestMethod]
        public void ShouldRemoveTestUserFromDatabase()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            //Act
            _employeeRepository.Remove(testUserId);
            //Assert
            Assert.IsNull(_employeeRepository.GetById(testUserId));
        }

        [TestMethod]
        public void ShouldReturnNullIfInsertFails()
        {
            //Arrange
            var employee = new Employee
            {
                Id = 2,
                FirstName = "Fail"
            };
            //Act
            var result = _employeeRepository.Add(employee);
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ShouldAddRolesToUser()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            //Act
            Employee employee = _employeeRepository.GetFullEmployeeInfo(testUserId);
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });
            _employeeRepository.Save(employee);
            //Assert
            var result = _employeeRepository.GetFullEmployeeInfo(testUserId);
            Assert.AreEqual(3, result.Roles.First().Id);
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        [TestMethod]
        public void ShouldRemoveRolesFromUser()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();

            Employee employee = _employeeRepository.GetFullEmployeeInfo(testUserId);
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });
            _employeeRepository.Save(employee);

            //Make Sure Role was added properly
            Assert.AreEqual(3, _employeeRepository.GetFullEmployeeInfo(testUserId).Roles.First().Id);

            //Act
            _employeeRepository.RemoveRole(testUserId, 3);

            //Assert
            Assert.IsNull(_employeeRepository.GetFullEmployeeInfo(testUserId).Roles.FirstOrDefault());
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        [TestMethod]
        public void ShouldNotInsertMultipleRolesIntoDatabase()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            //Act
            Employee employee = _employeeRepository.GetFullEmployeeInfo(testUserId);
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });
            //Tests add the same role twice
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });

            _employeeRepository.Save(employee);
            //Assert
            var result = _employeeRepository.GetFullEmployeeInfo(testUserId);
            Assert.AreEqual(1, result.Roles.Count);
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        #region Private Methods

        private int InsertSimpleUserToBeModified()
        {
            //Arrange
            var employee = new Employee
            {
                FirstName = "TestMethod",
                LastName = "Dummy",
                Email = "wh@tisanemail.com",
                DepartmentId = 5,
            };

            //Act
            _employeeRepository.Add(employee);

            //Assert
            employee.Id.Should().NotBe(0, "Identity should be assigned by the database");
            return employee.Id;
        }

        private void RemoveUserToBeModified(int id)
        {
            _employeeRepository.Remove(id);
        }

        private int InsertUserToBeModified()
        {
            var employee = new Employee
            {
                FirstName = "Modify",
                LastName = "Test",
                Email = "make@assurance.com",
                DepartmentId = 5,
            };

            var address = new Address
            {
                StreetAddress = "1 Infinite Loop",
                StateId = 5,
                City = "Cupertino",
                ZipCode = "95014"
            };
            employee.Addresses.Add(address);

            //Act
            _employeeRepository.Save(employee);

            //Assert
            Assert.AreNotEqual(0, employee.Id);
            Assert.AreNotEqual(0, employee.Addresses.Count());
            Assert.AreEqual("1 Infinite Loop", employee.Addresses[0].StreetAddress);

            return employee.Id;
        }

        #endregion

        [TestInitialize]
        public void Setup()
        {
            _employeeRepository = new EmployeeRepository();
        }
    }

}
