# ZeroCqrs — Le CQRS léger

**Zéro dépendance - Ask / Execute - Rien de plus**

## Installation

Ajouter le package
*(Pas encore disponible sur NuGet, utiliser le projet localement)*

```bash
dotnet add package ZeroCqrs
```

Déclarer le service (dans `Program.cs` ou `Startup.cs`)

```csharp
// Tout en un
builder.Services.AddZeroCqrs();

// Ou séparé (pour un contrôle plus fin)
builder.Services.AddZeroCqrsQueries(typeof(Program));
builder.Services.AddZeroCqrsCommands(typeof(Program));
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

        await commandBus.Execute(command);
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

        int newId = await commandBus.Execute(command);

        Console.WriteLine($"Nouvel utilisateur créé avec l'ID : {newId}");
    }
}
```