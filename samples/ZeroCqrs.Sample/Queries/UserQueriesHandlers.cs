using ZeroCqrs.Sample.Repository;

namespace ZeroCqrs.Sample.Queries;

public class UserQueriesHandlers(UserRepository repository) :
    IZeroQueryHandler<GetUserById, UserDetails>,
    IZeroQueryHandler<GetUsersCount, int>
{
    public Task<UserDetails> Answer(GetUserById query, CancellationToken ct = default)
    {
        var user = repository.GetById(query.Id);

        if (user is null)
            throw new KeyNotFoundException($"User with Id {query.Id} not found.");

        return Task.FromResult(new UserDetails(
            user.Id,
            user.Prenom,
            user.Nom,
            user.DateNaissance,
            user.Pays,
            user.Ville));
    }

    public Task<int> Answer(GetUsersCount query, CancellationToken ct = default)
    {
        var count = repository.GetAll().Count();

        return Task.FromResult(count);
    }
}
