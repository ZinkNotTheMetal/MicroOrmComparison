namespace MicroOrmComparison.UI.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public string ZipCode { get; set; }

        public bool IsNew
        {
            get { return Id == default(int); }
        }
    }
}