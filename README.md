# ZeroCqrs — Le CQRS léger

**Zéro dépendance externe (seulement les abstractions DI de .NET) - Ask / Send - Rien de plus**

## Installation

Ajouter le package [cportetdev.ZeroCqrs](https://www.nuget.org/packages/cportetdev.ZeroCqrs)

```bash
dotnet add package cportetdev.ZeroCqrs
```

Déclarer le service (dans `Program.cs` ou `Startup.cs`)

```csharp
// Tout en un : scanne l'assembly qui contient le type indiqué
builder.Services.AddZeroCqrs(typeof(Program));

// Ou séparé (pour un contrôle plus fin)
builder.Services.AddZeroCqrsQueries(typeof(Program));
builder.Services.AddZeroCqrsCommands(typeof(Program));
```

Sans argument, `AddZeroCqrs()` scanne l'assembly d'entrée de l'application : pratique, mais à éviter
si vos handlers sont dans une autre assembly (bibliothèque, projet de tests…).

Les bus et les handlers sont enregistrés en **scoped**. Dans un singleton (par exemple un
`BackgroundService` ou un `IHostedService`), créez un scope :

```csharp
public class Worker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var queryBus = scope.ServiceProvider.GetRequiredService<IZeroQueryBus>();
        // ...
    }
}
```

## Requête (Query<T,R>) - Je récupère des données

Cas d'usage : Demander une requête et obtenir un résultat.

### Exemple d'implémentation du Handler

```csharp
// Récupération d’un utilisateur par son ID
public record GetUserQuery(int Id) : IZeroQuery<UserDto>;

public class GetUserQueryHandler(AppDbContext db) : IZeroQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Answer(GetUserQuery query, CancellationToken ct = default)
    {
        return await db.Users
            .Where(u => u.Id == query.Id)
            .Select(u => new UserDto(u.Name, u.Email))
            .FirstAsync(ct);
    }
}
```

### Exemple d’utilisation

```csharp
public class ServiceUtilisateur(IZeroQueryBus queryBus)
{
    public async Task RecupererUtilisateur()
    {
        int userId = 42;
        var query = new GetUserQuery(userId);

        var user = await queryBus.Ask(query);

        Console.WriteLine($"Utilisateur : {user.Name}, Email : {user.Email}");
    }
}
```

## Commande (Command<T>) - J’exécute une action

Cas d'usage : Exécuter une commande sans résultat attendu.

### Exemple d'implémentation du Handler

```csharp
// Suppression d’un utilisateur
public record DeleteUserCommand(int Id) : IZeroCommand;

public class DeleteUserCommandHandler(AppDbContext db) : IZeroCommandHandler<DeleteUserCommand>
{
    public async Task Execute(DeleteUserCommand command, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync(new object[] { command.Id }, ct);
        if (user != null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync(ct);
        }
    }
}
```

### Exemple d’utilisation

```csharp
public class ServiceUtilisateur(IZeroCommandBus commandBus)
{
    public async Task SupprimerUtilisateur()
    {
        int userId = 42;
        var command = new DeleteUserCommand(userId);

        await commandBus.Send(command);
    }
}
```

## Commande avec résultat (Command<T,R>) - J’exécute une action et je reçois un résultat

Cas d'usage : Exécuter une commande et obtenir un résultat.

### Exemple d'implémentation du Handler

```csharp
// On créé un utilisateur et on retourne son ID

public record CreateUserCommand(string Name, string Email) : IZeroCommand<int>;

public class CreateUserCommandHandler(AppDbContext db) : IZeroCommandHandler<CreateUserCommand, int>
{
    public async Task<int> Execute(CreateUserCommand command, CancellationToken ct = default)
    {
        var user = new User { Name = command.Name, Email = command.Email };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user.Id; // l’ID créé
    }
}
```

### Exemple d’utilisation

```csharp
public class ServiceUtilisateur(IZeroCommandBus commandBus)
{
    public async Task CreerUtilisateur()
    {
        string nom = "Alice";
        string email = "alice@example.com";

        var command = new CreateUserCommand(nom, email);

        int newId = await commandBus.Send(command);

        Console.WriteLine($"Nouvel utilisateur créé avec l'ID : {newId}");
    }
}
```