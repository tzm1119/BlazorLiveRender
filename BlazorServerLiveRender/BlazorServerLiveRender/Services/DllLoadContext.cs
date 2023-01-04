using System.Reflection;
using System.Runtime.Loader;

namespace BlazorServerLiveRender.Services
{

    public class DllLoadContext : AssemblyLoadContext
    {

        public DllLoadContext() : base(isCollectible: true)
        {
           
        }

        protected override Assembly Load(AssemblyName name)
        {
            return null!;
        }
    }
}
