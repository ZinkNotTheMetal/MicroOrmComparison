CREATE TABLE [States] (
	Id int identity (1, 1) not null primary key,
	Name varchar(50) not null,
	Abbreviation varchar(2) not null
)
GO

CREATE TABLE Departments (
	Id int identity (1, 1) not null primary key,
	Name varchar(50),
)
GO

CREATE TABLE [Role] (
	Id int identity (1, 1) not null primary key,
	Name varchar(50)
)
GO

CREATE TABLE Employees (
	Id int identity (1, 1) not null primary key,
	FirstName varchar(50),
	LastName varchar(50),
	Email varchar(255),
	DepartmentId int constraint fk_Department_Id foreign key references Departments(Id)
)

GO

CREATE TABLE AssignedRoles (
	Id int identity (1, 1) not null primary key,
	EmployeeId int not null constraint fk_Employee_Id foreign key references Employees(Id),
	RoleId int not null constraint fk_Role_Id foreign key references [Role](Id),
)
GO

CREATE TABLE [Addresses] (
	Id int identity (1, 1) not null primary key,
	EmployeeId int not null,
	StreetAddress varchar(255),
	City varchar(55),
	StateId int not null,
	ZipCode varchar(10),
	CONSTRAINT fk_Employee_Id_Address foreign key (EmployeeId) REFERENCES [Employees](Id),
	CONSTRAINT fk_State_Id foreign key (StateId) REFERENCES [States](Id)
)
GO