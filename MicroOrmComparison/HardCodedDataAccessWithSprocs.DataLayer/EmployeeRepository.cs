using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;

namespace HardCodedDataAccessWithSprocs.DataLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        readonly string _connectionString = ConfigurationManager.ConnectionStrings["EmployeeDB"].ConnectionString;

        public EmployeeRepository(bool clearCache = false)
        {
            if (clearCache)
            {
                using (var connection = new SqlConnection(_connectionString))
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
            Employee employee = null;
            const string storedProcedureName = "GetEmployeeById";

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", employeeId);
                try
                {
                    connection.Open();
                    var dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        employee = new Employee
                        {
                            Id = (int)dataReader[0],
                            FirstName = (string)dataReader[1],
                            LastName = (string)dataReader[2],
                            DepartmentId = (int)dataReader[3],
                            Email = (string)dataReader[4]
                        };
                    }
                    dataReader.Close();
                    connection.Close();
                }
                catch (SqlException sqlException)
                {
                    return null;
                }
            }
            return employee;
        }

        public IEnumerable<Employee> GetAll()
        {
            const string storedProcedureName = "GetAllEmployees";
            var listOfEmployees = new List<Employee>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                try
                {
                    connection.Open();
                    var dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        var tempEmployee = new Employee
                        {
                            Id = (int) dataReader[0],
                            FirstName = (string) dataReader[1],
                            LastName = (string) dataReader[2],
                            DepartmentId = (int) dataReader[3],
                            Email = (string) dataReader[4]
                        };
                        listOfEmployees.Add(tempEmployee);
                    }
                    dataReader.Close();
                    connection.Close();
                }
                catch(SqlException sqlException)
                {
                    return null;
                }
            }
            return listOfEmployees;
        }

        public Employee Add(Employee employee)
        {
            Int32 newEmployeeId = 0;
            const string storedProcedureName = "InsertNewEmployee";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);
                try
                {
                    connection.Open();
                    newEmployeeId = (Int32)command.ExecuteScalar();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            employee.Id = (int)newEmployeeId;

            return employee;
        }

        public Employee Update(Employee employee)
        {
            const string storedProcedureName = "UpdateEmployeeInfo";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);
                command.Parameters.AddWithValue("@Id", employee.Id);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return employee;
        }

        public void Remove(int id)
        {
            const string storedProcedureName = "RemoveEmployeeById";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmployeeId", id);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            Employee employee = null;
            const string sprocNameForEmployeeInfo = "GetEmployeeById";
            const string sprocNameForAddresses = "GetAddressesByEmployeeId";
            const string sqlForRoles = "GetAssignedRolesByEmployeeId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmdForEmployee = new SqlCommand(sprocNameForEmployeeInfo, conn);
                cmdForEmployee.CommandType = CommandType.StoredProcedure;
                cmdForEmployee.Parameters.AddWithValue("@Id", employeeId);
                conn.Open();

                var dataReader = cmdForEmployee.ExecuteReader();
                while (dataReader.Read())
                {
                    employee = new Employee
                    {
                        Id = (int)dataReader[0],
                        FirstName = (string)dataReader[1],
                        LastName = (string)dataReader[2],
                        DepartmentId = (int)dataReader[3],
                        Email = (string)dataReader[4]
                    };
                }
                dataReader.Close();

                SqlCommand cmdForAddresses = new SqlCommand(sprocNameForAddresses, conn);
                cmdForAddresses.CommandType = CommandType.StoredProcedure;
                cmdForAddresses.Parameters.AddWithValue("@EmployeeId", employeeId);

                var addressReader = cmdForAddresses.ExecuteReader();
                if (addressReader.HasRows)
                {
                    while (addressReader.Read())
                    {
                        //Id, StreetAddress, EmployeeId, City, StateId, ZipCode
                        var tempAddress = new Address
                        {
                            Id = (int) addressReader[0],
                            StreetAddress = (string) addressReader[1],
                            EmployeeId = (int) addressReader[2],
                            City = (string) addressReader[3],
                            StateId = (int) addressReader[4],
                            ZipCode = (string) addressReader[5]
                        };
                        if (employee != null)
                            employee.Addresses.Add(tempAddress);
                    }
                }

                addressReader.Close();

                SqlCommand cmdForRoles = new SqlCommand(sqlForRoles, conn);
                cmdForRoles.CommandType = CommandType.StoredProcedure;
                cmdForRoles.Parameters.AddWithValue("@EmployeeId", employeeId);
                var assignedRolesReader = cmdForRoles.ExecuteReader();

                if (assignedRolesReader.HasRows)
                {
                    while (assignedRolesReader.Read())
                    {
                        var tempRole = new Role
                        {
                            Id = (int)assignedRolesReader[0],
                            Name = (string)assignedRolesReader[1]
                        };
                        if (employee != null)
                            employee.Roles.Add(tempRole);
                    }
                }
                assignedRolesReader.Close();

                conn.Close();
            }
            return employee;
        }

        private void Add(int employeeId, int roleId)
        {
            const string storedProcedureName = "AddAssignedRole";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@EmployeeId", employeeId);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
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
                foreach (var roleId in employee.Roles.Select(x => x.Id).Distinct())
                {
                    Add(employee.Id, roleId);
                }
                transactionScope.Complete();
            }
        }

        private void Add(Address address)
        {
            const string storedProcedureName = "InsertAddressForEmployee";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@EmployeeId", address.EmployeeId);
                command.Parameters.AddWithValue("@StreetAddress", address.StreetAddress);
                command.Parameters.AddWithValue("@City", address.City);
                command.Parameters.AddWithValue("@StateId", address.StateId);
                command.Parameters.AddWithValue("@ZipCode", address.ZipCode);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void Update(Address address)
        {
            const string storedProcedureName = "UpdateAddress";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StreetAddress", address.StreetAddress);
                command.Parameters.AddWithValue("@City", address.City);
                command.Parameters.AddWithValue("@StateId", address.StateId);
                command.Parameters.AddWithValue("@ZipCode", address.ZipCode);
                command.Parameters.AddWithValue("@Id", address.Id);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            const string storedProcedureName = "RemoveAssignedRole";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@employeeId", employeeId);
                command.Parameters.AddWithValue("@roleId", roleId);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
