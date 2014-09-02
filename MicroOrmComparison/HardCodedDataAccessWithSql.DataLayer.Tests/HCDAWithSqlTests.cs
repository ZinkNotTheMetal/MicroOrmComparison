using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper.DataLayer;
using FluentAssertions;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;
using NUnit.Framework;

namespace HardCodedDataAccessWithSql.DataLayer.Tests
{
    [TestFixture]
    public class EmployeeRepositoryTests
    {
        private IEmployeeRepository _employeeRepository;

        [Test]
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

        [Test]
        public void ShouldReturnCorrectEmployeeFromId()
        {
            //Act
            var result = _employeeRepository.GetById(2);
            //Assert
            result.FirstName.Should().Be("Bill");
            result.LastName.Should().Be("Burr");
            result.DepartmentId.Should().Be(2);
            result.Email.Should().Be("bill.burr@hilarious.com");
        }

        [Test]
        public void GetAllEmployeeInfoShouldReturnNull()
        {
            //Act
            var result = _employeeRepository.GetFullEmployeeInfo(int.MaxValue);
            //Arrange
            result.Should().BeNull();
        }

        [Test]
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

        [Test]
        public void ShouldModifyEmployeeToDatabase()
        {
            var testUserId = InsertSimpleUserToBeModified();

            var testContact = _employeeRepository.GetById(testUserId);
            testContact.LastName = "Jones";

            //Act
            _employeeRepository.Update(testContact);

            //Assert
            _employeeRepository.GetById(testUserId).LastName.Should().Be("Jones");

            RemoveUserToBeModified(testUserId);
        }

        [Test]
        public void ShouldRemoveSimpleEmployeeFromDatabase()
        {
            var testUserId = InsertSimpleUserToBeModified();

            //Act
            _employeeRepository.Remove(testUserId);
            //Assert
            var tempRepository = new EmployeeRepository();
            var result = tempRepository.GetById(testUserId);
            result.Should().BeNull();
        }

        [Test]
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

        [Test]
        public void ShouldReturnFullContactFromDatabase()
        {
            //Act
            var result = _employeeRepository.GetFullEmployeeInfo(2);
            //Assert
            result.FirstName.Should().Be("Bill");
            result.LastName.Should().Be("Burr");
            result.DepartmentId.Should().Be(2);
            result.Email.Should().Be("bill.burr@hilarious.com");
            result.Addresses.Count.Should().Be(1);
            result.Addresses.FirstOrDefault().Should().NotBeNull();
            result.Addresses.First().City.Should().Be("Los Angeles");
            result.Addresses.First().StateId.Should().Be(5);
            result.Addresses.First().ZipCode.Should().Be("90020");
            result.Addresses.First().StreetAddress.Should().Be("123 Main Street");
            result.Roles.Should().NotBeNull();
            result.Roles.First().Id.Should().Be(3);
        }

        [Test]
        public void ShouldModifyAddressAsWellAsContactUsingSaveToDatabase()
        {
            //Arrange
            int testUserId = InsertUserToBeModified();

            var testContact = _employeeRepository.GetFullEmployeeInfo(testUserId);
            testContact.LastName = "Jones";
            testContact.Addresses.First().StreetAddress = "345 Main Street";

            //Act
            _employeeRepository.Save(testContact);

            //Assert
            _employeeRepository.GetFullEmployeeInfo(testUserId).LastName.Should().Be("Jones");
            _employeeRepository.GetFullEmployeeInfo(testUserId).Addresses[0].StreetAddress.Should().Be("345 Main Street");

            //Cleanup
            RemoveUserToBeModified(testUserId);
        }

        [Test]
        public void ShouldRemoveTestUserFromDatabase()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            //Act
            _employeeRepository.Remove(testUserId);
            //Assert
            _employeeRepository.GetById(testUserId).Should().BeNull();
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        [Test]
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
            result.Should().BeNull();
        }

        [Test]
        public void ShouldAddRolesToUser()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            //Act
            var employee = _employeeRepository.GetFullEmployeeInfo(testUserId);
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });
            _employeeRepository.Save(employee);
            //Assert
            var result = _employeeRepository.GetFullEmployeeInfo(testUserId);
            result.Roles.First().Id.Should().Be(3);
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        [Test]
        public void ShouldRemoveRolesFromUser()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            var employee = _employeeRepository.GetFullEmployeeInfo(testUserId);
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });
            _employeeRepository.Save(employee);
            //Make Sure Role was added properly
            _employeeRepository.GetFullEmployeeInfo(testUserId).Roles.First().Id.Should().Be(3);

            //Act
            _employeeRepository.RemoveRole(testUserId, 3);

            //Assert
            _employeeRepository.GetFullEmployeeInfo(testUserId).Roles.Should().BeEmpty();
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        [Test]
        public void ShouldNotInsertMultipleRolesIntoDatabase()
        {
            //Arrange
            var testUserId = InsertUserToBeModified();
            //Act
            var employee = _employeeRepository.GetFullEmployeeInfo(testUserId);
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });
            //test add the same role twice
            employee.Roles.Add(new Role
            {
                Id = 3,
                Name = "Supervisor"
            });

            _employeeRepository.Save(employee);
            //Assert
            var result = _employeeRepository.GetFullEmployeeInfo(testUserId);
            result.Roles.Count().Should().Be(1);
            //Cleanup
            _employeeRepository.Remove(testUserId);
        }

        #region Private Methods

        private int InsertSimpleUserToBeModified()
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
            employee.Id.Should().NotBe(0, "Identity should be assigned by the database");
            employee.Addresses.Count().Should().NotBe(0);
            employee.Addresses[0].StreetAddress.Should().Be("1 Infinite Loop");

            return employee.Id;
        }

        #endregion

        [SetUp]
        public void Setup()
        {
            _employeeRepository = new EmployeeRepository();
        }
    }

}
