SELECT 
    'ALTER TABLE ' +  OBJECT_SCHEMA_NAME(parent_object_id) +
    '.[' + OBJECT_NAME(parent_object_id) + 
    '] DROP CONSTRAINT ' + name
FROM sys.foreign_keys

ALTER TABLE dbo.[Employees] DROP CONSTRAINT fk_Department_Id
ALTER TABLE dbo.[AssignedRoles] DROP CONSTRAINT fk_Employee_Id
ALTER TABLE dbo.[AssignedRoles] DROP CONSTRAINT fk_Role_Id
ALTER TABLE dbo.[Addresses] DROP CONSTRAINT fk_Employee_Id_Address
ALTER TABLE dbo.[Addresses] DROP CONSTRAINT fk_State_Id

drop table Employees
drop table [Role]
drop table Departments
drop table AssignedRoles
drop table States
drop table Addresses