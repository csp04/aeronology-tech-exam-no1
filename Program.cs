using System;

namespace aeronology_tech_exam_no1
{
    class Program
    {
        static void Main(string[] args)
        {
            var classDefinition = @"public class Flight
{
    public string Carrier { get; set; }
    public List<Route> Routes { get; set; }
    public int FlightNumber { get; set; }
    public long? AgentId { get; set; }
    public class Route
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Suburb { get; set; }
        public int FlightDuration { get; set; }
    }
}";

            var typeScriptClasses = CSharpTypeScript.ConvertCSharpObjectToTypeScript(classDefinition);

            foreach(var tsClass in typeScriptClasses)
            {
                Console.WriteLine(tsClass);
            }

        }
}
}
