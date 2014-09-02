namespace OrmLite.DataLayer.DTOs
{
    [ServiceStack.DataAnnotations.Alias("Addresses")]
    internal class AddressDto
    {
        [ServiceStack.DataAnnotations.AutoIncrement]
        [ServiceStack.DataAnnotations.PrimaryKey]
        public int Id { get; set; }
        [ServiceStack.DataAnnotations.ForeignKey(typeof(EmployeeDto), OnDelete = "SET DEFAULT")]
        public int EmployeeId { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public string ZipCode { get; set; }

        [ServiceStack.DataAnnotations.Ignore]
        public bool IsNew
        {
            get { return Id == default(int); }
        }
    }
}
