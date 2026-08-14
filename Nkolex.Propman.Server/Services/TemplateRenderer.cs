namespace Nkolex.Propman.Server.Services
{
    public static class TemplateRenderer
    {
        public static string Render(string template, IDictionary<string, string> tokens)
        {
            foreach (var (key, value) in tokens)
            {
                template = template.Replace("{{" + key + "}}", value);
            }
            return template;
        }
    }
}
