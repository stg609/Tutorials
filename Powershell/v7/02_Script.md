## Common
1. 不区分大小写  
`Get-Command` 和 `gEt-coMmaNd` 是一样的  
`$Foo` 和 `$fOo` 是一样的
1. 变量
   * 以 `$` 开头，如 `$foo`
   * 变量名可以包含空格或者特殊字符 （应尽量避免），如 `${a var} = 1`
   * 不需要声明，直接通过 变量赋值 的形式来创建一个变量
   * 类型非常宽松 (loosely typed)，允许变量为任意类型，类似 Javascript
     ```PowerShell
     PS> $foo = 1
     PS> $foo = "string"
     PS> $foo = Get-Process
     ```
   * 变量类型实际是由 .Net 类型系统决定。要查看一个变量的类型， 可以使用 `Get-Member -InputObject $value`
   * 可以使用类型属性来确保变量只能包含特定的类型  
     如果尝试将其他类型赋值给该变量，则 PowerShell 会尝试进行类型转换，如果无法转换，则赋值失败。   
     ```powershell
     [int]$number = 8
     $number = "12345" # 转成数字类型

     [datetime] $dates = "09/12/91" #转成 DateTime 类型
     ```


   * 自动变量   
      变量的值是由 PowerShell 自动设置的变量，比如用户信息、运行时信息等。这类变量应该仅通过只读的方式使用。
      *  $PID   
         当前执行 PowerShell 会话的进程标识符
      *  $PsVersionTable  
         执行当前会话的 PowerShell 版本信息
      *  $Pwd
         当前目录的完整路径
      *  $Error  
         包含最近一次的错误
      *  $IsCoreCLR   
         表示当前的 PowerShell 会话是不是基于 .Net Core 的运行时
      *  $IsLinux | $IsMacOS | $IsWindows   
         是否是某个 OS
      *  $_ 或者 $PsItem   
         管道传递的数组对象中的某个一个 Item   
         `Get-Command | ForEach-Object { $_.Name }`
      *  $null  
         **当 $null 参与判断的时候，把 $null 放在左边**
         ```PowerShell
         if($null -eq $value)
         {
             # Do Something
         }
         ```
         当 `$value` 直接参与检查时，如下: 表示 *如果 `$value` 不是 `$null` 或 `0` 或    `$false`  或 空字符串，则 Do Something*
         ```PowerShell
         if（$value)
         {
            # Do Something
         }
         ```
         如果是字符串，也可以使用 .Net 字符串的静态方法 `IsNullOrEmpty` 及    `IsNullOrWhiteSpace`  
         ```PowerShell
         if(-not [string]::IsNullOrWhiteSpace($value))
         {
            # Do Something
         }
         ```
         foreach 会自动忽略 $null
         ```PowerShell
         foreach ( $node in $null )
         {
             #skipped
         }
         ```
   

      * $true

   * 变量作用域
      * 全局 Global
      * 脚本 Script
1. 引号
   *  双引号 字符串   
      是一个可扩展的字符串
      * 在双引号中的变量（e.g. $Foo) 会被变量值替换，如：  
         ```powershell
         PS> $Foo = 5
         PS> "The value of $Foo is $Foo"
         ```
         输出：
         ```
         The value of 5 is 5
         ```
      * 在双引号中的表达式会被计算，如：  
         ```powershell
         PS> "The value of $(2+3) is 5"
         ```
         输出：
         ```
         The value of 5 is 5
         ```
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
   ```PowerShell
   PS> Get-XXX 参数1 参数2
   PS> Get-XXX -参数名 值
   PS> Get-XXX -参数名:值
   ```
   * 参数类型  

不显示指定参数名的时候，需要根据参数定义的顺序（多个参数通过 **空格** 分隔）进行提供   

类型定义   
括号
