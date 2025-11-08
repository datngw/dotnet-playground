namespace HATEOAS.Common;

public sealed record Resource<T>(T Data, List<Link> Links);