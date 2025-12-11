using ZeroCqrs.Sample.Repository;

namespace ZeroCqrs.Sample.Commands;

public class UserCommandsHandlers(UserRepository repository) :
    ICommandHandler<CreateUser, CreateUserResponse>,
    IZeroCommandHandler<UpdateUser>
{
    public Task<CreateUserResponse> Execute(CreateUser command, CancellationToken ct = default)
    {
        var newId = Guid.NewGuid();

        var user = new User(newId,
            command.Prenom,
            command.Nom,
            command.DateNaissance,
            command.Pays,
            command.Ville);

        repository.Add(user);

        return Task.FromResult(new CreateUserResponse(newId));
    }

    public Task Execute(UpdateUser command, CancellationToken ct = default)
    {
        repository.Update(new User(
            command.Id,
            command.Prenom,
            command.Nom,
            command.DateNaissance,
            command.Pays,
            command.Ville));

        return Task.CompletedTask;
    }
}