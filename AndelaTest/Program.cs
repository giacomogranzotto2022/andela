using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static TicketsConsole.Program;

namespace TicketsConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var events = new List<Event>{
                new Event(1, "Phantom of the Opera", "New York", new DateTime(2023,12,23)),
                new Event(2, "Metallica", "Los Angeles", new DateTime(2023,12,02)),
                new Event(3, "Metallica", "New York", new DateTime(2023,12,06)),
                new Event(4, "Metallica", "Boston", new DateTime(2023,10,23)),
                new Event(5, "LadyGaGa", "New York", new DateTime(2023,09,20)),
                new Event(6, "LadyGaGa", "Boston", new DateTime(2023,08,01)),
                new Event(7, "LadyGaGa", "Chicago", new DateTime(2023,07,04)),
                new Event(8, "LadyGaGa", "San Francisco", new DateTime(2023,07,07)),
                new Event(9, "LadyGaGa", "Washington", new DateTime(2023,05,22)),
                new Event(10, "Metallica", "Chicago", new DateTime(2023,01,01)),
                new Event(11, "Phantom of the Opera", "San Francisco", new DateTime(2023,07,04)),
                new Event(12, "Phantom of the Opera", "Chicago", new DateTime(2024,05,15))
            };

            var customer = new Customer()
            {
                Id = 1,
                Name = "John",
                City = "New York",
                BirthDate = new DateTime(1995, 05, 10)
            };

            var marketingEngine = new MarketingEngine(events);
            //marketingEngine.SendCustomerSameCityEventNotifications(customer);
            //marketingEngine.SendCustomerCloseToBirthdayEventNotifications(customer);
            //marketingEngine.SendCustomerClosetCitiesEventNotifications(customer);

            foreach (var item in marketingEngine.SortEventsByField("Date"))
            {
                Console.WriteLine($"{item.Name} - {item.City} - {item.Date}");
            }

        }

        public class Event
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
            public DateTime Date { get; set; }

            public Event(int id, string name, string city, DateTime date)
            {
                this.Id = id;
                this.Name = name;
                this.City = city;
                this.Date = date;
            }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
            public DateTime BirthDate { get; set; }
        }
        public record City(string Name, int X, int Y);

        public static readonly IDictionary<string, City> Cities = new Dictionary<string, City>()
            {
                { "New York", new City("New York", 3572, 1455) },
                { "Los Angeles", new City("Los Angeles", 462, 975) },
                { "San Francisco", new City("San Francisco", 183, 1233) },
                { "Boston", new City("Boston", 3778, 1566) },
                { "Chicago", new City("Chicago", 2608, 1525) },
                { "Washington", new City("Washington", 3358, 1320) },
            };

        public class MarketingEngine
        {
            private Dictionary<string, int> DistanceBetweenCitiesCache = new Dictionary<string, int>();

            private readonly IEnumerable<Event> _events;
            public MarketingEngine(IEnumerable<Event> events)
            {
                _events = events;
            }

            public void SendCustomerClosetCitiesEventNotifications(Customer customer, int maxCities = 5)
            {
                var events = _events.OrderBy(x => CalculateDistanceBetweenCities(Cities[x.City], Cities[customer.City])).Take(maxCities);

                foreach (var e in events)
                {
                    SendCustomerNotifications(customer, e);
                }
            }

            public void SendCustomerSameCityEventNotifications(Customer customer)
            {
                var events = _events.Where(x => x.City.Equals(customer.City, StringComparison.CurrentCultureIgnoreCase));
                foreach (var e in events)
                {
                    SendCustomerNotifications(customer, e);
                }
            }

            public void SendCustomerCloseToBirthdayEventNotifications(Customer customer, int daysFar = 30)
            {
                //Should we consider just events that are close from Today AND close to the next birthDay?
                //I'm just considering close to the next birthday assuming in a real scenario this method would be triggered by a job with some filter on it

                var nextCustomerBirthday = new DateTime(DateTime.Now.Year, customer.BirthDate.Month, customer.BirthDate.Day);
                if (nextCustomerBirthday < DateTime.Today) //The birthday gone so we take the next one
                {
                    nextCustomerBirthday = nextCustomerBirthday.AddYears(1);
                }

                var events = _events.Where(x => x.Date.Subtract(nextCustomerBirthday).TotalDays < daysFar);

                foreach (var e in events)
                {
                    SendCustomerNotifications(customer, e);
                }
            }

            private void SendCustomerNotifications(Customer customer, Event e)
            {
                Console.WriteLine($"{customer.Name} from {customer.City} event {e.Name} at {e.Date}");
            }

            private int CalculateDistanceBetweenCities(City cityA, City cityB)
            {
                //If this method was calling an external API to calculate the distance I would use a library to handle the possible error codes like Polly https://github.com/App-vNext/Polly
                //With that we could set a retry policy (which codes would retry, how many times, how long wait between the retries, etc...)

                var cityNames = new List<string> { cityA.Name, cityB.Name };
                cityNames.Sort();

                var citiesDistanceKey = string.Join("-", cityNames);
                if (DistanceBetweenCitiesCache.ContainsKey(citiesDistanceKey))
                {
                    return DistanceBetweenCitiesCache[citiesDistanceKey];
                }

                var distance = Math.Abs(cityA.X - cityB.X) + Math.Abs(cityA.Y - cityB.Y);

                DistanceBetweenCitiesCache.Add(citiesDistanceKey, distance);

                return distance;
            }

            public IEnumerable<Event> SortEventsByField(string fieldName, bool ascendingResults = true)
            {
                var prop = TypeDescriptor.GetProperties(typeof(Event)).Find(fieldName, true);
                if (prop == null)
                {
                    throw new ArgumentException(nameof(fieldName));
                }

                if (ascendingResults)
                {
                    return _events.OrderBy(x => prop.GetValue(x));
                }

                return _events.OrderByDescending(x => prop.GetValue(x));
            }
        }
    }
}

