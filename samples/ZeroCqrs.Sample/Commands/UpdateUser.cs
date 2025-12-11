namespace ZeroCqrs.Sample.Commands;

public sealed record UpdateUser(
    Guid Id,
    string Prenom,
    string Nom,
    DateTime DateNaissance,
    string Pays,
    string Ville) : IZeroCommand;
