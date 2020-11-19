using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using PersonNamespace;

namespace MyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"PersonFromConfig:Name", "value1"},
            }).Build();

            var person = new Person();
            configuration.Bind("CustomFromConfig", person);
            Console.WriteLine("Custom options property using generated code: " + person.Name);
        }
    }
}
namespace PersonNamespace
{
    public class Person
    {
        public string Name { get; set; }
    }
}