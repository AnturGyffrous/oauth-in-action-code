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

            var uriBuilder = new UriBuilder(uri) { Query = queryString.AsQueryString() };

            return uriBuilder.Uri;
        }

        public static string AsQueryString(this object queryString)
        {
            if (queryString == null)
            {
                return null;
            }

            var parameters = HttpUtility.ParseQueryString(string.Empty);

            queryString
                .GetType()
                .GetProperties()
                .ToList()
                .ForEach(x => parameters.Add(x.Name, x.GetValue(queryString, null).ToString()));

            return parameters.ToString();
        }
    }
}