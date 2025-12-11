using Bogus;

namespace ZeroCqrs.Sample.Repository;

public record User(
    Guid Id,
    string Prenom,
    string Nom,
    DateTime DateNaissance,
    string Pays,
    string Ville)
{
    public int Age => DateTime.Today.Year - DateNaissance.Year;
}

public class UserRepository(int count = 10000)
{
    private readonly List<User> _users = GenerateUsers(count);

    private static List<User> GenerateUsers(int count)
    {
        var faker = new Faker<User>("fr")
            .CustomInstantiator(f => new User(
                Guid.NewGuid(),
                f.Person.FirstName,
                f.Person.LastName,
                f.Person.DateOfBirth,
                f.Address.Country(),
                f.Address.City())
            );

        return faker.Generate(count);
    }

    public IReadOnlyList<User> GetAll() => _users.AsReadOnly();

    public User? GetById(Guid id) => _users.FirstOrDefault(x => x.Id == id);

    public void Add(User newUser)
    {
        _users.Add(newUser);
    }

    public void Update(User updatedUser)
    {
        var user = _users.FirstOrDefault(x => x.Id == updatedUser.Id);
        if (user != null)
        {
            var newUser = user with
            {
                Prenom = updatedUser.Prenom,
                Nom = updatedUser.Nom,
                DateNaissance = updatedUser.DateNaissance,
                Pays = updatedUser.Pays,
                Ville = updatedUser.Ville
            };

            var index = _users.IndexOf(user);
            _users[index] = newUser;
        }
    }
}