一、调试 nuget 包 （VS2022）
1. 禁用 “启用仅我的代码”，
2. 启用 源链接 （内部的子选项可以不勾）
3. 在 调试-符号 页签中，启用 Microsoft 符号服务器（如果不想调试 .Net 的源码，可以不勾选），Nuget.org 符号服务器

二、调试自己的 nuget 包 （github)
1. 生成 symbol nuget 包： https://docs.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg
   下面是我用的方式：
   a. 在 DebugNugetPkgDemoPkgForTrivialDemo.csproj 中增加
		<IncludeSymbols>true</IncludeSymbols>
		<SymbolPackageFormat>snupkg</SymbolPackageFormat>
   b. 因为我用的是 github 作用源码服务器，所以再加上
        <PackageReference Include="Microsoft.SourceLink.GitHub" Version="1.1.1" PrivateAssets="All" />

如果这个 nuget 包是给其它人用，也就是说他们可能没有我们这个项目的源码，那么要先执行下述的步骤2，3。
否则直接4（如果直接本地，则不需要 source link, pdb 可以直接指向源码，我个人猜测）
2. 在打包前先提交一次源码到 github (必须要在打包前提交，因为打包的时候会包含 git commit 的信息，用于 source link 找到对应的源码)
3. 打包，并把生成的 nupkg, snupkg 上传到 nuget.org

4. 在 TrivialDemo 项目中安装该 pkg
5. 为了模拟是在其它人电脑上用，把本地 DebugNugetPkgDemoPkgForTrivialDemo SomeClass.cs 文件删了。
6. 按照 “一、调试 nuget 包”方式调试。


注意，目前 source link 暂时不支持指向私有的 git 仓库
https://github.com/dotnet/sourcelink/issues/281