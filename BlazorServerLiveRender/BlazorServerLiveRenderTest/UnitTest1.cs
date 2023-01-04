using BlazorServerLiveRender.Services;

namespace BlazorServerLiveRenderTest
{
    public class UnitTest1
    {
        /// <summary>
        /// ���Ա������ʱdll�Ƿ�����ȷ��ȡType��Ϣ
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