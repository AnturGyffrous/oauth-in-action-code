using System.Collections.Generic;

namespace ProtectedResource.Database
{
    public interface INoSql : IEnumerable<IDictionary<string, object>>
    {
    }
}