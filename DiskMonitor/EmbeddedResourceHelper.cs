namespace DiskMonitor
{
    public static class EmbeddedResourceHelper
    {
        // --- GetResourceStream Overloads ---

        public static System.IO.Stream? GetResourceStream(string fileName)
        {
            return GetResourceStream(fileName, System.Reflection.Assembly.GetCallingAssembly());
        }

        public static System.IO.Stream? GetResourceStream(string fileName, System.Reflection.Assembly assembly)
        {
            string? resourceName = GetResourceName(fileName, assembly);

            return resourceName == null
                ? null
                : assembly.GetManifestResourceStream(resourceName);
        }

        public static System.IO.Stream? GetResourceStream(string fileName, System.Type type)
        {
            return GetResourceStream(fileName, type.Assembly);
        }

        // --- GetResourceName Overloads ---

        public static string? GetResourceName(string fileName)
        {
            return GetResourceName(fileName, System.Reflection.Assembly.GetCallingAssembly());
        }

        public static string? GetResourceName(string fileName, System.Reflection.Assembly assembly)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string name in resourceNames)
            {
                if (name.EndsWith(fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }

            return null;
        }

        // --- ReadResourceText Overloads ---

        public static string? ReadResourceText(string fileName)
        {
            return ReadResourceText(fileName, System.Reflection.Assembly.GetCallingAssembly());
        }

        public static string? ReadResourceText(string fileName, System.Reflection.Assembly assembly)
        {
            using System.IO.Stream? stream = GetResourceStream(fileName, assembly);

            if (stream == null)
            {
                return null;
            }

            using System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}