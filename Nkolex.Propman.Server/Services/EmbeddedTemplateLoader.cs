using System.Reflection;
using System.Text;

namespace Nkolex.Propman.Server.Services
{
    public class EmbeddedTemplateLoader
    {
        private readonly Assembly _assembly;
        private readonly string _resourceRootNamespace;

        public EmbeddedTemplateLoader(string resourceRootNamespace)
        {
            _assembly = Assembly.GetExecutingAssembly();
            _resourceRootNamespace = resourceRootNamespace;
        }

        public string Load(string fileName)
        {
            var resourceName = $"{_resourceRootNamespace}.{fileName}";
            using var stream = _assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException(
                    $"Embedded template not found: {resourceName}. " +
                    "Check the .csproj <EmbeddedResource> entry and the resource root namespace.");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
