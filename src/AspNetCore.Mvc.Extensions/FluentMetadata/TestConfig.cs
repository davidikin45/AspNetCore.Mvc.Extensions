namespace AspNetCore.Mvc.Extensions.FluentMetadata
{
    public class PersonConfig : ModelMetadataConfiguration<Person>
    {
        public PersonConfig()
        {
            Configure(p => p.Name).Required();
            Configure<string>("Name").Required();
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }
}
