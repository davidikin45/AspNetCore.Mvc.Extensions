namespace AspNetCore.Mvc.Extensions.Domain
{
    public interface IEntityConcurrencyAware
    {
        byte[] RowVersion { get; set; }
    }
}
