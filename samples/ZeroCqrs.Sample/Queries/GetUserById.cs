namespace ZeroCqrs.Sample.Queries;

public sealed record GetUserById(Guid Id) : IZeroQuery<UserDetails>;

public sealed record UserDetails(
    Guid Id,
    string Prenom,
    string Nom,
    DateTime DateNaissance,
    string Pays,
    string Ville);



