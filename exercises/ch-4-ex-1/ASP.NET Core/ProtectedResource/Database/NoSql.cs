using System.Collections;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace ProtectedResource.Database
{
    public class NoSql : INoSql
    {
        private readonly string _path;

        public NoSql(string path)
        {
            _path = path;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IDictionary<string, object>> GetEnumerator()
        {
            if (!File.Exists(_path))
            {
                yield break;
            }

            using (var db = new StreamReader(_path))
            {
                string line;
                while ((line = db.ReadLine()) != null)
                {
                    yield return JsonConvert.DeserializeObject<Dictionary<string, object>>(line);
                }
            }
        }
    }
}