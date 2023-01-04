# 项目说明

今天看到masa的贡献群里在讨论动态编译blazor组件的问题。

我出于兴趣，提出了自己的解决方案。

# 实现原理

1. 用户在blaozr的页面上输入想要编译的razor(html),css等字符串
2. 用户点击编译后，程序将这些字符串保存为razor文件和css文件，文件保存到模板项目中去

3. 使用dotnet build命令，对模板项目进行编译，生成动态组件的dll文件

4. 使用AssemblyLoadContext，对dll进行加载，从dll中取出动态组件的Type对象

5. blazor使用内置的DynamicComponent，将这个动态组件渲染出来

   1. ```
       <DynamicComponent Type="DisplayComponentType"></DynamicComponent>
      ```



# 注意点

下面总结一些实现过程中的问题

1. 动态加载dll必须使用AssemblyLoadContext，而不能直接使用Assembly.LoadFrom直接加载dll
    > 由于每次编译后，都会生成一个同名的 CompileTemplateProject.dll
    > 因此，如果直接使用 Assembly.LoadFrom 加载dll，会出现重复加载同名dll的问题
    > 解决方案是：创建新的 加载上下文 DllLoadContext
    > 具体可参考微软文档
	> https://learn.microsoft.com/zh-cn/dotnet/standard/assembly/unloadability

2. 关于程序集的卸载

   > 程序集的卸载是比较复杂的，需要考虑到外部是否引用已加载的类型，也就是说，只有当外部没有对程序集内部类型的引用时，程序集才能完全卸载。

# 结论

对于群里提出的动态编译blazor组件的需求，我其实是不赞同的。原因如下:

1. **安全性问题**，由于需要动态编译dll，如何保证用户输入的代码中，不包含恶意代码？
2. **资源占用问题**(不是性能问题)，如果很多用户都来动态编译dll，那么文档站点将会动态加载大量dll，造成资源占用过多

## 我的建议

之所以有动态编译blazor组件的需求，是因为组件的属性太多，在线文档可能无法对所有属性进行说明。需要给用户一个在线修改属性，动态看到改变结果的方案。动态编译是一种方案，但这种方案实现难度比较大，坑很多。

我推荐做一个功能强大的**组件属性编辑器**，类似于winform和wpf中PropertyGrid的控件。这样不仅开发出了一个新的非常通用的组件，还能解决现有的动态属性编辑问题。

