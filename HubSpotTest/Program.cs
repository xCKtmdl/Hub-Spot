using Newtonsoft.Json;
using RestSharp;


/*
 * 
 * Program to do interesting conference scheduler test for HubSpot
 * 
 * runs and builds in Visual Studio 2022
 * Targets .NET 6.0 LTS runtime
 * 
 * Nuget dependencies:
 * 
 * RestSharp for doing the http GET and POST requests for communicating
 * with HubSpot test API.
 * 
 * And of course Newtonsoft Json for making json <-> POCO
 * serialization/deserialization easy.
 */

namespace HubSpotTest
{
    public class MyClass
    {
        private static string MY_API_KEY = String.Empty;


        private static string firstApiUrl = "https://candidate.hubteam.com";

        public static void Main()
        {
            /* Comment/uncomment code for toggling back and forth */
            /* between unit testing from file vs. actually hitting */
            /* the webservice. */

            //var jsonResponse = Task.Run(() => HitFirstAPI(firstApiUrl, MY_API_KEY));
            //jsonResponse.Wait();
            //string json = JsonConvert.SerializeObject(jsonResponse.Result);

            Models.Partners jsonResponse = new Models.Partners();
            using (StreamReader r = new StreamReader(@"\Hubspot-GET.json"))
            {
                string json1 = r.ReadToEnd();
                jsonResponse = JsonConvert.DeserializeObject<Models.Partners>(json1);
            }

            HashSet<string> countryList = new HashSet<string>();
            Models.Countries countriesPOST = new Models.Countries();

            countriesPOST.countries = new List<Models.Country>();


            // Build list of countries
            foreach (var x in jsonResponse.partners)
            {
                countryList.Add(x.country);
            }

            // Get attendees list for the conference in each country
            foreach (var country in countryList)
            {
                Models.Country countryInvite = new Models.Country();
                countryInvite= GetAvailableDates(country, jsonResponse.partners.Where(e => e.country.Equals(country)).ToList());
               
                countriesPOST.countries.Add(countryInvite);
            }

            string json = JsonConvert.SerializeObject(countriesPOST, Formatting.Indented);
            // var jsonResponse2 = Task.Run(() => HitSecondAPI(firstApiUrl, MY_API_KEY,countriesPOST));
            // jsonResponse2.Wait();
        }

        /*
         * GetAvailableDates
         * 
         * Takes a country name as a string and list of partners.
         * 
         * Returns a populated Country model with the relevant information
         * such as start date, partners who are able to attend, etc.
         */
        public static Models.Country GetAvailableDates(string country, List<Models.Partner> CountryPartners)
        {
            Models.Country retCountry = new Models.Country();
            retCountry.name= country;
            retCountry.attendeeCount = 0;
            retCountry.startDate = String.Empty;
            retCountry.attendees = new List<string>();
            
            DateTime ChosenDate;
            var AttendingPartners = new List<Models.Partner>();
            var WorkingDates = new HashSet<DateTime>();
            var PotentialDates = new Dictionary<Models.Partner, List<DateTime>>
            {
            };
            
            foreach (var cp in CountryPartners)
            {
                PotentialDates[cp] = MatchingDatesPartner(cp.availableDates,ref WorkingDates);
            }
            

            WorkingDates = WorkingDates.OrderBy(x => x.Date).ToHashSet();
            var mostAttending = 0;
            var AttendingDict = new Dictionary<DateTime, List<Models.Partner>>();

            // Iterate through sorted dates and calculate which partners are able to attend based on availability.
            foreach (var date in WorkingDates)
            {
                AttendingDict[date] = new List<Models.Partner>();
                var attending = 0;
                foreach (var partner in PotentialDates)
                {
                    if (partner.Value.Count() > 0)
                    {
                        foreach(var PartnerDate in partner.Value)
                        {
                            if (PartnerDate == date)
                            {
                                AttendingDict[date].Add(partner.Key);
                                attending=attending+1;
                            }
                        }
                    }
                }
                if (attending > mostAttending)
                {
                    mostAttending = attending;
                    ChosenDate = date;
                    AttendingPartners = AttendingDict[date];
                    foreach (var ap in AttendingPartners)
                    {
                        retCountry.attendees.Add(ap.email);
                    }
                    retCountry.startDate = String.Format("{0:yyyy-MM-dd}", ChosenDate);
                }

            }
            

            retCountry.attendees = retCountry.attendees.ToHashSet().ToList();

            // remove attending partners if it turns out they
            // don't have availability for calculated start date

            foreach (var attendee in retCountry.attendees)
            {
                var partner = CountryPartners.Where(x => x.email.Equals(attendee)).FirstOrDefault();

                DateTime dtStartDate = new DateTime();
                DateTime.TryParse(retCountry.startDate, out dtStartDate);
                if (!partner.availableDates.Contains(dtStartDate))
                {
                    retCountry.attendees = retCountry.attendees.Where(x => !x.Equals(partner.email)).ToList();
                }
            }


            retCountry.attendeeCount = retCountry.attendees.Count();
            if (retCountry.attendeeCount == 0)
            {
                retCountry.startDate = null;
            }

            return retCountry;
        }


        /*
         * MatchingDatesPartner
         * 
         * Takes a reference to a hash set of dates to work with that can be updated, 
         * and a list of the partner/customer/exhibitor's availability.
         * 
         * Returns dates that will be used in a partner dictionary of consecutive dates to potentially schedule
         * the conference.
         */
        public static List<DateTime> MatchingDatesPartner(List<DateTime> availableDates,ref HashSet<DateTime> WorkingDates)
        {
            var PotentialDates = new List<DateTime>();
            var avlDatesArry = availableDates.OrderBy(x => x.Date).ToArray();
            for (int i=0;i<avlDatesArry.Length-1;i++)
            {
                if (avlDatesArry[i].Subtract(avlDatesArry[i + 1]).TotalDays.Equals(-1))
                {
                    PotentialDates.Add(avlDatesArry[i]);
                    WorkingDates.Add(avlDatesArry[i]);
                }
            }
            return PotentialDates;
        }

        // Method to do the GET
        public static async Task<Models.Partners> HitFirstAPI(string base_url, string MY_API_KEY)
        {
            Models.Partners partners = new Models.Partners();
            var client = new RestClient(base_url);

            CancellationToken cancellationToken = new CancellationToken();

            var request = new RestRequest("/candidateTest/v3/problem/dataset", Method.Get);
            request.AddQueryParameter("userKey", MY_API_KEY);
            
            partners = await client.GetAsync<Models.Partners>(request, cancellationToken);
            return partners;
        }

        // Method to POST the answer
        public static async Task<ResponseStatus> HitSecondAPI(string base_url, string MY_API_KEY,Models.Countries countries)
        {
            var client = new RestClient(base_url);

            CancellationToken cancellationToken = new CancellationToken();

            var request = new RestRequest("/candidateTest/v3/problem/result", Method.Post);
            request.AddQueryParameter("userKey", MY_API_KEY);
            request.AddJsonBody(countries);

            var response = await client.GetAsync(request, cancellationToken);
            return response.ResponseStatus;
        }


    }


}




