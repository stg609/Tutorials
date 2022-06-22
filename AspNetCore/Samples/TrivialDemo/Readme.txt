调试 nuget 包 （VS2022）
1. 禁用 “启用仅我的代码”，
2. 启用 源链接 （内部的子选项可以不勾）
3. 在 调试-符号 页签中，启用 Microsoft 符号服务器（如果不想调试 .Net 的源码，可以不勾选），Nuget.org 符号服务器

调试自己的nuget包
1. 生成 symbol nuget 包： https://docs.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg