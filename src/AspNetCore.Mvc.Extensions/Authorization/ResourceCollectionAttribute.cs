using System;

namespace AspNetCore.Mvc.Extensions.Authorization
{
    public class ResourceCollectionAttribute : Attribute
    {
        public string CollectionId { get;}
        public ResourceCollectionAttribute(string collectionId)
        {
            CollectionId = collectionId;
        }
    }
}
