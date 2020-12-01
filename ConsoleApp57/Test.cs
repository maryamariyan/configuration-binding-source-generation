using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp57
{
    public static class GG
    {
        public static void CanBindIConfigurationSection()
        {
            var dic = new Dictionary<string, string>
            {
                {"Section:Integer", "-2"},
                {"Section:Boolean", "TRUe"},
                {"Section:Nested:Integer", "11"},
                {"Section:Virtual", "Sup"}
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();

            // TODO support for DerivedOptions
            var options = new DerivedOptions();
            //Bind(config, options, o => { });
            //config.GetSection("Section").Bind(options, o => { });
            //config.Bind(options);
            //config.Bind("Section", options);
            //config.Bind("Section", options);

            Console.WriteLine($"Assert.True(options.Boolean) {options.Boolean}");
            Console.WriteLine($"Assert.Equal(-2, childOptions.Integer) {options.Integer}");
            Console.WriteLine($"Assert.Equal(11, childOptions.Nested.Integer) {options.Nested.Integer}");
        }
    }
    public class ComplexOptions
    {

        public ComplexOptions()
        {
            Nested = new NestedOptions();
            Virtual = "complex";
        }

        public NestedOptions Nested { get; set; }
        public int Integer { get; set; }
        public bool Boolean { get; set; }
        public virtual string Virtual { get; set; }
        public object Object { get; set; }

        public string PrivateSetter { get; private set; }
        public string ProtectedSetter { get; protected set; }
        public string InternalSetter { get; internal set; }
        public static string StaticProperty { get; set; }

        private string PrivateProperty { get; set; }
        internal string InternalProperty { get; set; }
        protected string ProtectedProperty { get; set; }

        protected string ProtectedPrivateSet { get; private set; }

        private string PrivateReadOnly { get; }
        internal string InternalReadOnly { get; }
        protected string ProtectedReadOnly { get; }

        public string ReadOnly
        {
            get { return null; }
        }
    }

    public class NestedOptions
    {
        public int Integer { get; set; }
    }

    public class DerivedOptions : ComplexOptions
    {
        public override string Virtual
        {
            get
            {
                return base.Virtual;
            }
            set
            {
                base.Virtual = "Derived:" + value;
            }
        }
    }

    public class NullableOptions
    {
        public bool? MyNullableBool { get; set; }
        public int? MyNullableInt { get; set; }
        public DateTime? MyNullableDateTime { get; set; }
    }

    public class EnumOptions
    {
        public UriKind UriKind { get; set; }
    }

    public class GenericOptions<T>
    {
        public T Value { get; set; }
    }

    public class OptionsWithNesting
    {
        public NestedOptions Nested { get; set; }

        public class NestedOptions
        {
            public int Value { get; set; }
        }
    }

    public class ConfigurationInterfaceOptions
    {
        public IConfigurationSection Section { get; set; }
    }

    public class DerivedOptionsWithIConfigurationSection : DerivedOptions
    {
        public IConfigurationSection DerivedSection { get; set; }
    }

    public struct ValueTypeOptions
    {
        public int MyInt32 { get; set; }
        public string MyString { get; set; }
    }

    public class ByteArrayOptions
    {
        public byte[] MyByteArray { get; set; }
    }
}
