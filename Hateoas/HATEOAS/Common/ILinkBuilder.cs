namespace HATEOAS.Common;

public interface ILinkBuilder<T> where T : class
{
  Resource<T> Build(T resource, LinkGenerator linker, HttpContext ctx);
}