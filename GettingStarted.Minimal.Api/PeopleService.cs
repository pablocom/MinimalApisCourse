namespace GettingStarted.Minimal.Api;

public record Person(string FullName);

public class PeopleService
{
    private readonly List<Person> _people = new()
    {
        new Person("John Mayer"),
        new Person("Anthony Kiedis"),
        new Person("Steve Lucather")
    };

    public IEnumerable<Person> Search(string searchTerm)
        => _people.Where(x => x.FullName.Contains(searchTerm)).ToArray();
}
