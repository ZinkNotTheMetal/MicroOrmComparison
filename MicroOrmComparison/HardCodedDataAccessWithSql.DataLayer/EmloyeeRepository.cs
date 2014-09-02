using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using MicroOrmComparison.UI.Interfaces;
using MicroOrmComparison.UI.Models;

namespace HardCodedDataAccessWithSql.DataLayer
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
            const string sqlQuery =
                "Select Id, FirstName, LastName, Email, DepartmentId FROM Employees WHERE Id = @employeeId";

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@employeeId", employeeId);
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
                            Email = (string)dataReader[3],
                            DepartmentId = (int)dataReader[4]
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
            const string sqlQuery = "SELECT Id, FirstName, LastName, Email, DepartmentId FROM Employees";
            var listOfEmployees = new List<Employee>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(sqlQuery, connection);

                try
                {
                    connection.Open();
                    var dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        var tempEmployee = new Employee
                        {
                            Id = (int)dataReader[0],
                            FirstName = (string)dataReader[1],
                            LastName = (string)dataReader[2],
                            Email = (string)dataReader[3],
                            DepartmentId = (int)dataReader[4]
                        };
                        listOfEmployees.Add(tempEmployee);
                    }
                    dataReader.Close();
                    connection.Close();
                }
                catch (SqlException sqlException)
                {
                    return null;
                }
            }
            return listOfEmployees;
        }

        public Employee Add(Employee employee)
        {
            Int32 newEmployeeId = 0;
            const string sql = "INSERT INTO Employees VALUES (@FirstName, @LastName, @Email, @DepartmentId);" +
                               "SELECT CAST(scope_identity() AS int)";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                cmd.Parameters.AddWithValue("@Email", employee.Email);
                cmd.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);
                try
                {
                    conn.Open();
                    newEmployeeId = (Int32)cmd.ExecuteScalar();
                    conn.Close();
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
            const string sql = "UPDATE Employees " +
                               "SET FirstName = @FirstName, " +
                               "     LastName = @LastName, " +
                               "     Email = @Email, " +
                               " DepartmentId = @DepartmentId " +
                               "     WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                cmd.Parameters.AddWithValue("@Email", employee.Email);
                cmd.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);
                cmd.Parameters.AddWithValue("@Id", employee.Id);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
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
            const string sql = "DELETE FROM AssignedRoles WHERE EmployeeId = @Id;" +
                               "DELETE FROM Addresses WHERE EmployeeId = @Id;" +
                               "DELETE FROM Employees WHERE Id = @Id;";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public Employee GetFullEmployeeInfo(int employeeId)
        {
            Employee employee = null;
            const string sqlForEmployee = "SELECT Id, FirstName, LastName, Email, DepartmentId FROM Employees WHERE Id = @EmployeeId;";
            const string sqlForAddress = "SELECT Id, EmployeeId, StreetAddress, City, StateId, ZipCode FROM Addresses WHERE EmployeeId = @EmployeeId;";
            const string sqlForRoles = "SELECT Role.Id, Role.Name FROM AssignedRoles, Role " +
                                       "WHERE Role.Id = AssignedRoles.RoleId AND " +
                                       "AssignedRoles.EmployeeId = @EmployeeId;";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmdForEmployee = new SqlCommand(sqlForEmployee, conn);
                cmdForEmployee.Parameters.AddWithValue("@EmployeeId", employeeId);
                conn.Open();

                var dataReader = cmdForEmployee.ExecuteReader();
                while (dataReader.Read())
                {
                    employee = new Employee
                    {
                        Id = (int)dataReader[0],
                        FirstName = (string)dataReader[1],
                        LastName = (string)dataReader[2],
                        Email = (string)dataReader[3],
                        DepartmentId = (int)dataReader[4]
                    };
                }
                dataReader.Close();

                SqlCommand cmdForAddresses = new SqlCommand(sqlForAddress, conn);
                cmdForAddresses.Parameters.AddWithValue("@EmployeeId", employeeId);

                var addressReader = cmdForAddresses.ExecuteReader();
                if (addressReader.HasRows)
                {
                    while (addressReader.Read())
                    {
                        var tempAddress = new Address
                        {
                            Id = (int)addressReader[0],
                            EmployeeId = (int)addressReader[1],
                            StreetAddress = (string)addressReader[2],
                            City = (string)addressReader[3],
                            StateId = (int)addressReader[4],
                            ZipCode = (string)addressReader[5]
                        };
                        if (employee != null)
                            employee.Addresses.Add(tempAddress);
                    }
                }

                addressReader.Close();

                SqlCommand cmdForRoles = new SqlCommand(sqlForRoles, conn);
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
            const string sql = "INSERT INTO AssignedRoles(RoleId, EmployeeId) VALUES (@RoleId, @EmployeeId)";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
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
            const string sql =
                "INSERT INTO Addresses (EmployeeId, StreetAddress, City, StateId, ZipCode) VALUES (@EmployeeId, @StreetAddress, @City, @StateId, @ZipCode);";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmployeeId", address.EmployeeId);
                cmd.Parameters.AddWithValue("@StreetAddress", address.StreetAddress);
                cmd.Parameters.AddWithValue("@City", address.City);
                cmd.Parameters.AddWithValue("@StateId", address.StateId);
                cmd.Parameters.AddWithValue("@ZipCode", address.ZipCode);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void Update(Address address)
        {
            const string sql = "UPDATE Addresses " +
                                "SET StreetAddress = @StreetAddress, " +
                                "City = @City, " +
                                "StateId = @StateId, " +
                                "ZipCode = @ZipCode " +
                                "WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@StreetAddress", address.StreetAddress);
                cmd.Parameters.AddWithValue("@City", address.City);
                cmd.Parameters.AddWithValue("@StateId", address.StateId);
                cmd.Parameters.AddWithValue("@ZipCode", address.ZipCode);
                cmd.Parameters.AddWithValue("@Id", address.Id);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void RemoveRole(int employeeId, int roleId)
        {
            const string sql = "DELETE FROM AssignedRoles WHERE EmployeeId = @employeeId AND RoleId = @roleId";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@employeeId", employeeId);
                cmd.Parameters.AddWithValue("@roleId", roleId);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
