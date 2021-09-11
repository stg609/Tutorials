## Common
1. 不区分大小写  
`Get-Command`, `gEt-coMmaNd` 是一样的
1. 变量
   * 以 `$` 开头，如 `$foo`
   * 弱类型
     ```
     PS> $foo = 1
     PS> $foo = "string"
     PS> $foo = Get-Process
     ```
1. 引号
1. 操作符   
   不支持 `==`, `<=`, `>=`, `!=`，`>`，`<` 等比较操作符 
   | 常规 | PowerShell | 示例|
   | --- | ---| --- |
   |==|-eq| 
   |!=|-ne|
   |>|-gt|
   |>=|-ge|
   |<|-lt|
   |<=|-le|
   |!| ! 或者 -not| -not ($true) |

2. 参数 （Parameter)  
参数跟在命令的名称之后，格式如下：
   ```
   PS> Get-XXX 参数1 参数2
   PS> Get-XXX -参数名 值
   PS> Get-XXX -参数名:值
   ```
   * 参数类型  

不显示指定参数名的时候，需要根据参数定义的顺序（多个参数通过 **空格** 分隔）进行提供   

类型定义
* 布尔
`$true`
`$false`
