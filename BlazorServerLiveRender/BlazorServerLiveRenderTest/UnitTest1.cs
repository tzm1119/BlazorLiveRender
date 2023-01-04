using BlazorServerLiveRender.Services;

namespace BlazorServerLiveRenderTest
{
    public class UnitTest1
    {
        /// <summary>
        /// 测试编译的临时dll是否能正确获取Type信息
        /// </summary>
        [Fact]
        public async Task Test1Compile ()
        {
            ICompileHelper helper=new CompileHelper();

            var html = "<h1>Counter 1</h1>";
            var css = "";
            var comType=await helper.GetComponentType(new CompileInfo {  RazorHtml=html, Css=css});

            Assert.NotNull(comType);
        }
    }
}