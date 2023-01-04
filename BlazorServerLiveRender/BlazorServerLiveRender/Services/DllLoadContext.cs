using System.Reflection;
using System.Runtime.Loader;

namespace BlazorServerLiveRender.Services
{

    public class DllLoadContext : AssemblyLoadContext
    {

        public DllLoadContext() : base(isCollectible: true)
        {
            this.Unloading += DllLoadContext_Unloading;
        }

        private void DllLoadContext_Unloading(AssemblyLoadContext obj)
        {
            Console.WriteLine($"正在卸载DllLoadContext");
        }

        protected override Assembly Load(AssemblyName name)
        {
            return null!;
        }


    }
}
