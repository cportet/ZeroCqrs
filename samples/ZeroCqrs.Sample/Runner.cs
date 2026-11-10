using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZeroCqrs.Sample.Commands;
using ZeroCqrs.Sample.Queries;

namespace ZeroCqrs.Sample;

// A hosted service is a singleton: the buses (and handlers) are scoped,
// so they must be resolved from a scope created for the unit of work.
internal class Runner(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var zeroQueryBus = scope.ServiceProvider.GetRequiredService<IZeroQueryBus>();
        var zeroCommandBus = scope.ServiceProvider.GetRequiredService<IZeroCommandBus>();

        // ------ Récupérer le nombre total d'utilisateurs initiaux
        var getRepoCountQuery = new GetUsersCount();
        var usersTotalCount = await zeroQueryBus.Ask(getRepoCountQuery, ct);
        Console.WriteLine($"Nombre total d'utilisateurs : {usersTotalCount}");

        // ------ Créer un utilisateur
        var createUserCommand = new CreateUser(
            "Bob",
            "Sinclar",
            new DateTime(1995, 10, 11),
            "Paris",
            "France");

        var createResult = await zeroCommandBus
            .Send(createUserCommand, ct);

        Console.Write($"Nouvel utilisateur créé : {createResult.Id}");

        // ------ Récupérer un utilisateur par son Id
        var getUserQuery = new GetUserById(createResult.Id);
        var user = await zeroQueryBus.Ask(getUserQuery, ct);
        Console.WriteLine($" - {user.Prenom} {user.Nom}, né le {user.DateNaissance:d} à {user.Ville}, {user.Pays}");

        // ------ Récupérer le nombre total d'utilisateurs après l'ajout
        var getUsersCountQuery = new GetUsersCount();
        var usersCount = await zeroQueryBus.Ask(getUsersCountQuery, ct);
        Console.WriteLine($"Nombre total d'utilisateurs : {usersCount}");


        // ------ Mettre à jour un utilisateur
        var updateUserCommand = new UpdateUser(
            createResult.Id,
            "Robert",
            "Sinclar",
            new DateTime(1995, 10, 11),
            "Lyon",
            "France");

        await zeroCommandBus.Send(updateUserCommand, ct);
        Console.WriteLine("Utilisateur mis à jour.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

}