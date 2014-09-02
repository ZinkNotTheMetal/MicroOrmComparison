namespace OrmLite.DataLayer.DTOs
{
    [ServiceStack.DataAnnotations.Alias("AssignedRoles")]
    internal class AssignedRolesDto
    {
        [ServiceStack.DataAnnotations.ForeignKey(typeof(EmployeeDto), OnDelete = "CASCADE", Type = typeof(EmployeeDto))]
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
    }
}
