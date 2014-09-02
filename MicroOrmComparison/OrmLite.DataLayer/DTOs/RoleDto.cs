namespace OrmLite.DataLayer.DTOs
{
    [ServiceStack.DataAnnotations.Alias("Role")]
    internal class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
