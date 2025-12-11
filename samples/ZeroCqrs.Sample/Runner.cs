using Microsoft.Extensions.Hosting;
using ZeroCqrs.Sample.Commands;
using ZeroCqrs.Sample.Queries;

namespace ZeroCqrs.Sample;

internal class Runner(IZeroQueryBus zeroQueryBus, IZeroCommandBus zeroCommandBus) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
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