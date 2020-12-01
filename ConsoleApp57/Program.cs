using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using PersonNamespace;

namespace ConsoleApp57
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"PersonFromConfig:Name", "value1"},
                {"PersonFromConfig:Age", "123"},
            }).Build();

            var noPrefixConfiguration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"Name", "value1"},
                {"Age", "123"},
            }).Build();

            var withOverload1 = new Person();
            configuration.Bind("PersonFromConfig", withOverload1);
            Console.WriteLine($"person.Name: {withOverload1.Name}, person.Age: {withOverload1.Age}");

            var withOverload2 = new Person();
            noPrefixConfiguration.Bind(withOverload2, o => { });
            Console.WriteLine($"person.Name: {withOverload2.Name}, person.Age: {withOverload2.Age}");

            var withOverload3 = new Person();
            noPrefixConfiguration.Bind(withOverload3);
            Console.WriteLine($"person.Name: {withOverload3.Name}, person.Age: {withOverload3.Age}");
        }
    }
}

namespace PersonNamespace
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}