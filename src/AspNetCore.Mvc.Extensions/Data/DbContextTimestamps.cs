using AspNetCore.Mvc.Extensions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCore.Mvc.Extensions.Data
{
    public class DbContextTimestamps
    {
        public static void AddTimestamps(IEnumerable<object> addedObjects, IEnumerable<object> modifiedObjects)
        {
            var added = addedObjects.Where(x => x is IEntityAuditable);
            var modified = modifiedObjects.Where(x => x is IEntityAuditable);

            foreach (var entity in added)
            {

                ((IEntityAuditable)entity).CreatedOn = DateTime.UtcNow;

                ((IEntityAuditable)entity).UpdatedOn = DateTime.UtcNow;
            }

            foreach (var entity in modified)
            {

                ((IEntityAuditable)entity).UpdatedOn = DateTime.UtcNow;
            }
        }
    }
}
