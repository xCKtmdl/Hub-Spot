namespace Models
{
   
    public class Partner
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string country { get; set; }
        public List<DateTime> availableDates { get; set; }
    }

    public class Partners
    {
        public List<Partner> partners { get; set; }
    }

    public class Country
    {
        public int attendeeCount { get; set; }
        public List<string> attendees { get; set; }
        public string name { get; set; }
        public string startDate { get; set; }
    }

    public class Countries
    {
        public List<Country> countries { get; set; }
    }


}
