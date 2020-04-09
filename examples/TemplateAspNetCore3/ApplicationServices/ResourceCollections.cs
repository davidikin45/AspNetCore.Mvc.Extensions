using AspNetCore.Mvc.Extensions;

namespace TemplateAspNetCore3.ApplicationServices
{
    public class ResourceCollections
    {
        public class Blog
        {
            public class Authors
            {
                public const string CollectionId = "blog.authors";

                public class Operations
                {

                }

                public class Scopes
                {
                    public const string Create = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.Create;
                    public const string Read = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.Read;
                    public const string ReadOwner = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.ReadOwner;
                    public const string Update = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.Update;
                    public const string UpdateOwner = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.UpdateOwner;
                    public const string Delete = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.Delete;
                    public const string DeleteOwner = CollectionId + "." + ResourceCollectionsCore.CRUD.Operations.DeleteOwner;
                }
            }
        }
    }
}
