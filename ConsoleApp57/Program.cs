using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using CustomOptionsNamespace;

namespace MyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"CustomFromConfig:CustomProperty", "value1"},
            }).Build();

            var co = new CustomOptions();
            configuration.Bind<CustomOptions>("CustomFromConfig", co);
            Console.WriteLine("Custom options property using generated code: " + co.CustomProperty);
        }
    }
}
namespace CustomOptionsNamespace
{
    public class CustomOptions
    {
        public string CustomProperty { get; set; }
    }
}