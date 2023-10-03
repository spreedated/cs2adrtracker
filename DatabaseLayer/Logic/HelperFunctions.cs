using System.IO;

namespace DatabaseLayer.Logic
{
    internal static class HelperFunctions
    {
        public static string LoadEmbeddedSql(string filename)
        {
            if (!filename.EndsWith("sql"))
            {
                filename = $"{filename}.sql";
            }

            using (Stream s = typeof(HelperFunctions).Assembly.GetManifestResourceStream($"{typeof(HelperFunctions).Assembly.GetName().Name}.Sql.{filename}"))
            {
                if (s == null)
                {
                    return null;
                }
                using (StreamReader sr = new(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
