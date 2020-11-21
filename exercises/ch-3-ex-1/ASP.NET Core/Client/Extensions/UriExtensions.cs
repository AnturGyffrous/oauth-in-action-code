using System;
using System.Linq;
using System.Web;

namespace Client.Extensions
{
    public static class UriExtensions
    {
        public static Uri AddQueryString(this Uri uri, object queryString)
        {
            if (queryString == null)
            {
                return uri;
            }

            var parameters = HttpUtility.ParseQueryString(string.Empty);

            queryString
                .GetType()
                .GetProperties()
                .ToList()
                .ForEach(x => parameters.Add(x.Name, x.GetValue(queryString, null).ToString()));

            var uriBuilder = new UriBuilder(uri) { Query = parameters.ToString() };

            return uriBuilder.Uri;
        }
    }
}