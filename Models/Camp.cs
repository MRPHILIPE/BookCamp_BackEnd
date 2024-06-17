namespace bookcamp.Models
{
    public class Camp
    {
        public int CampId { get; set; } = 0;
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public bool IsBooked { get; set; } = false;
        public string CreatedByEmail { get; set; }
    }
}
