namespace AspNetCore.Mvc.Extensions.Domain
{
    public interface IEntityOwned
    {
        string OwnedBy { get; set; }
    }
}
