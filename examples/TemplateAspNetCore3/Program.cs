using AspNetCore.Mvc.Extensions;
using System.Threading.Tasks;

namespace TemplateAspNetCore3
{
    public class Program : ProgramBase<Startup>
    {
        public async static Task<int> Main(string[] args)
        {
            return await RunApp(args);
        }
    }
}
