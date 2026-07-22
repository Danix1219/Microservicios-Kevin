using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Configuration
{
    public static class PostgresConnectionString
    {
        public static string Normalize(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
                || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
            {
                return connectionString;
            }

            var separatorIndex = uri.UserInfo.IndexOf(':');
            if (separatorIndex < 0)
            {
                throw new FormatException("La URL de PostgreSQL debe incluir usuario y contraseña.");
            }

            var query = ParseQuery(uri.Query);
            var builder = new DbConnectionStringBuilder
            {
                ["Host"] = uri.Host,
                ["Port"] = uri.Port > 0 ? uri.Port : 5432,
                ["Database"] = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/')),
                ["Username"] = Uri.UnescapeDataString(uri.UserInfo[..separatorIndex]),
                ["Password"] = Uri.UnescapeDataString(uri.UserInfo[(separatorIndex + 1)..]),
            };

            if (query.TryGetValue("sslmode", out var sslMode))
            {
                builder["SSL Mode"] = sslMode;
            }

            if (query.TryGetValue("channel_binding", out var channelBinding))
            {
                builder["Channel Binding"] = channelBinding;
            }

            return builder.ConnectionString;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            return query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .Where(part => part.Length == 2)
                .ToDictionary(
                    part => Uri.UnescapeDataString(part[0]),
                    part => Uri.UnescapeDataString(part[1]),
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
