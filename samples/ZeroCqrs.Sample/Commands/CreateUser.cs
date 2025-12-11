namespace ZeroCqrs.Sample.Commands;

public sealed record CreateUser(
    string Prenom,
    string Nom,
    DateTime DateNaissance,
    string Ville,
    string Pays) : IZeroCommand<CreateUserResponse>;

public sealed record CreateUserResponse(Guid Id);