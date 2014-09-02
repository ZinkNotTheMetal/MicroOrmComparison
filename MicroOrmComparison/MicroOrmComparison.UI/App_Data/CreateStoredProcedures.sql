CREATE PROCEDURE GetEmployeeById
    @Id int 
AS
    SET NOCOUNT ON;
    SELECT Id, FirstName, LastName, DepartmentId, Email
    FROM Employees
    WHERE Id = @Id
GO

CREATE PROCEDURE GetAllEmployees
AS
	SELECT Id, FirstName, LastName, DepartmentId, Email
    FROM Employees
GO

CREATE PROCEDURE InsertNewEmployee
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Email nvarchar(255),
	@DepartmentId int = null 
AS
	SET NOCOUNT ON;
	INSERT INTO Employees (FirstName, LastName, Email, DepartmentId) VALUES (@FirstName, @LastName, @Email, @DepartmentId);
                               SELECT CAST(SCOPE_IDENTITY() as int)
GO

CREATE PROCEDURE UpdateEmployeeInfo
	@Id int,
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Email nvarchar(255),
	@DepartmentId int = null
AS
	SET NOCOUNT ON;
	UPDATE Employees SET FirstName = @FirstName, 
						 LastName = @LastName, 
						 Email = @Email, 
						 DepartmentId = @DepartmentId 
		   WHERE Id = @Id;
GO

CREATE PROCEDURE InsertAddressForEmployee
	@EmployeeId int,
	@StreetAddress nvarchar(255),
	@City nvarchar(100),
	@StateId int,
	@ZipCode nvarchar(10)
AS
	SET NOCOUNT ON;
	INSERT INTO Addresses (EmployeeId, StreetAddress, City, StateId, ZipCode) 
		           VALUES (@EmployeeId, @StreetAddress, @City, @StateId, @ZipCode);
GO

CREATE PROCEDURE UpdateAddress
	@Id int,
	@StreetAddress nvarchar(255),
	@City nvarchar(100),
	@StateId int,
	@ZipCode nvarchar(10)
AS
	SET NOCOUNT ON;
	UPDATE Addresses SET StreetAddress = @StreetAddress,
						 City = @City,
						 StateId = @StateId,
						 ZipCode = @ZipCode
					WHERE Id = @Id
GO

CREATE PROCEDURE AddAssignedRole
	@EmployeeId int,
	@RoleId int
AS
	SET NOCOUNT ON;
	INSERT INTO AssignedRoles (EmployeeId, RoleId)
			VALUES (@EmployeeId, @RoleId)
GO

CREATE PROCEDURE RemoveAssignedRole
	@EmployeeId int,
	@RoleId int
AS
	SET NOCOUNT ON;
	DELETE FROM AssignedRoles WHERE EmployeeId = @EmployeeId AND RoleId = @RoleId;
GO

CREATE PROCEDURE RemoveEmployeeById
	@EmployeeId int
AS
	SET NOCOUNT ON;
	DELETE FROM Addresses WHERE EmployeeId = @EmployeeId;
	DELETE FROM AssignedRoles WHERE EmployeeId = @EmployeeId;
	DELETE FROM Employees WHERE Id = @EmployeeId;
GO

CREATE PROCEDURE GetAddressesByEmployeeId
	@EmployeeId int
AS
	SET NOCOUNT ON;
	SELECT Id, StreetAddress, EmployeeId, City, StateId, ZipCode FROM Addresses WHERE EmployeeId = @EmployeeId
GO

CREATE PROCEDURE GetAssignedRolesByEmployeeId
	@EmployeeId int
AS
	SET NOCOUNT ON;
	SELECT [Role].Id, [Role].Name FROM AssignedRoles,[Role]
                                  WHERE [Role].Id = AssignedRoles.RoleId 
								  AND AssignedRoles.EmployeeId = @EmployeeId;
GO