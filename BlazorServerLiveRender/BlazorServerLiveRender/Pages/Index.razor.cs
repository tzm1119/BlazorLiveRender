using BlazorServerLiveRender.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace BlazorServerLiveRender.Pages
{
    public class MyDynamicCom:ComponentBase,IDisposable
    {
        [Parameter]
        [NotNull]
        public Type? Type { get; set; }

        public void Dispose()
        {
            Type = null;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent(1, Type);
            builder.CloseComponent();
        }
    }

    public partial class Index:IDisposable
    {
        private CompileInfo CompileInfo;

        [Inject]
        [NotNull]
        public ICompileHelper? CompileHelper { get; set; }

        private Type DisplayComponentType { get; set; }

        public Index()
        {
            DisplayComponentType = typeof(Counter);
            CompileInfo = new CompileInfo()
            {
                RazorHtml = """
                   <h1 style="background:red">Counter 1</h1>

                 <p role="status">Current count: @currentCount</p>

                 <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

                 @code {
                     private int currentCount = 0;

                     private void IncrementCount()
                     {
                         currentCount++;
                     }
                 }
                 """,
                Css = """
                 h1 {background:red;}
                 """
            };
        }
        private bool isCompiling = false;
        private void RunGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        /// <summary>
        /// 运行编译
        /// </summary>
        private async Task RunCompile()
        {
            try
            {
                isCompiling = true;
                _ = InvokeAsync(StateHasChanged);
                DisplayComponentType = await CompileHelper.GetComponentType(CompileInfo);
                //await CompileHelper.GetComponentType(CompileInfo);
            }
            catch (Exception ex)
            {
                compileError = ex;
            }
            finally
            {
                isCompiling = false;
                _ = InvokeAsync(StateHasChanged);
            }

        }

        public void Dispose()
        {
            DisplayComponentType = null!;
            Console.WriteLine("index组件Dispose");
        }

        private Exception? compileError;
    }
}
