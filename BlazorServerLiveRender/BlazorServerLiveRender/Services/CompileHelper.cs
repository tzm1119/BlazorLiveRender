using CliWrap;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

namespace BlazorServerLiveRender.Services
{
    public class CompileInfo
    {
        [Required]
        public string RazorHtml { get; set; } = "";
        public string Css { get; set; } = "";
    }
    public interface ICompileHelper:IDisposable
    {
        Task<Type> GetComponentType(CompileInfo info);
    }
    public class CompileHelper : ICompileHelper
    {
        public CompileHelper()
        {
            // 防止编译错误时，错误信息中的中文出现乱码
            Console.OutputEncoding = Encoding.UTF8;

            if (!Directory.Exists(TemplateProjectDirectoryPath))
            {
                throw new Exception($"{TemplateProjectDirectoryPath}文件夹不存在，请修改此处的绝对路径为模板项目的文件夹路径。");
            }

            timer = new Timer(CheckWeakRefAlive, null, 1000, 3000);
        }

        private Timer timer;

        private void CheckWeakRefAlive(object? state)
        {
            var removeCount = weakList.RemoveAll(t => !t.IsAlive);
            if (removeCount > 0)
            {
                Console.WriteLine($"本次移除了{removeCount}的上下文弱引用,总数为:{weakList.Count}");
            }
            else
            {
                Console.WriteLine($"未移除任何上下文弱引用，总数为:{weakList.Count}");
            }

        }

        /// <summary>
        /// 这个路径需要考虑 是单元测试，
        /// 还是真实项目，所以这里使用了绝对路径
        /// </summary>
        private string TemplateProjectDirectoryPath
            = "D:\\Devops\\LiveRender\\BlazorServerLiveRender\\CompileTemplateProject";

        private List<WeakReference> weakList = new List<WeakReference>();
        /// <summary>
        /// 临时生成的dll文件，在此文件夹下保存
        /// </summary>
        private string TempDllDirName = "TempComponentDll\\Com";

        private static int TempDir_Count = 1;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public async Task<Type> GetComponentType(CompileInfo info)
        {
            // 生成新的临时文件夹名称
            var tempDllDirName = $"{this.TempDllDirName}_{TempDir_Count++}";
            // 创建临时文件夹
            Directory.CreateDirectory(tempDllDirName);

            string razorText = info.RazorHtml;
            string css = info.Css;
            // 随机生成一个组件文件的名字,转为大小，否则编译报错
            // Component 'cchgk5e4_4yy' starts with a lowercase character.
            // Component names cannot start with a lowercase character. 
            var comName = "DynamicCom";
            var razorFilePath = Path.Combine(TemplateProjectDirectoryPath, $"{comName}.razor");
            var cssFilePath = Path.Combine(TemplateProjectDirectoryPath, $"{comName}.css");
            File.WriteAllText(razorFilePath, razorText);
            File.WriteAllText(cssFilePath, css);


            var stdErrBuffer = new StringBuilder();

            var result = await Cli.Wrap("dotnet")
                .WithWorkingDirectory(TemplateProjectDirectoryPath)
                .WithArguments($"build -c Release -o {tempDllDirName}")
                .WithValidation(CommandResultValidation.None)
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();

            // 不管编译是否成功，删除生成的临时razor和css文件
            File.Delete(razorFilePath);
            File.Delete(cssFilePath);

            // 编译失败
            if (result.ExitCode != 0)
            {
                throw new Exception(stdErrBuffer.ToString());
            }

            var dllName = "CompileTemplateProject.dll";
            var dllFilePath = Path.Combine(TemplateProjectDirectoryPath, tempDllDirName, dllName);
            Console.WriteLine($"临时dll路径为{dllFilePath}");

            // 由于每次编译后，都会生成一个同名的 CompileTemplateProject.dll
            // 因此，如果直接使用 Assembly.LoadFrom 加载dll，会出现重复加载同名dll的问题
            // 解决方案是：创建新的 加载上下文 DllLoadContext
            // 具体可参考微软文档
            // https://learn.microsoft.com/zh-cn/dotnet/standard/assembly/unloadability
            var alc = new DllLoadContext();

            Assembly assembly = alc.LoadFromAssemblyPath(dllFilePath);

            var alcWeakRef = new WeakReference(alc, trackResurrection: true);
            weakList.Add(alcWeakRef);

            var comType = assembly.GetType($"CompileTemplateProject.{comName}");

            // 卸载上下文
            alc.Unload();
            if (comType != null)
            {
                return comType;
            }
            else
            {
                throw new Exception($"在dll中，未找到组件{comName}的Type，dll路径为{dllFilePath}");
            }

        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
