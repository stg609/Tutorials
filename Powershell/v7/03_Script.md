1. 不区分大小写  
`Get-Command` 和 `gEt-coMmaNd` 是一样的  
`$Foo` 和 `$fOo` 是一样的
1. 结尾的 “分号” 可有可无，但尽量统一
1. 变量
   * 以 `$` 开头，如 `$foo`
   * 变量名可以包含空格或者特殊字符 （，但应尽量避免），如 `${a var} = 1`
   * 不需要先声明，直接通过 变量赋值 的形式会自动创建一个变量
   * 类型非常宽松 (loosely typed)，允许变量为任意类型，类似 Javascript
     ```PowerShell
     $foo = 1
     $foo = "string"
     $foo = Get-Process
     ```
   * 变量类型实际是由 .Net 类型系统决定。要查看一个变量的类型， 可以使用 `Get-Member -InputObject $value`
   * 可以使用类型属性来确保变量只能包含特定的类型  
     如果尝试将其他类型赋值给该变量，则 PowerShell 会尝试进行类型转换，如果无法转换，则赋值失败。   
     ```powershell
     [int]$number = 8
     $number = "12345" # 转成数字类型

     [datetime] $dates = "09/12/91" #转成 DateTime 类型
     ```
   * 作用域
      * 范围
        * 脚本 Script  
        作用范围仅在当前脚本，运行结束变量就被释放了
        * 全局 Global  
        运行不同脚本时，都能访问该变量
      * 继承   
        子作用域可以看见父级的变量，但是不能修改（除非显示声明父级的作用域）。  
        *注意，子作用域中创建一个同名的变量，并不会覆盖父级的变量，仅仅只是隐藏了。*
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
   | 常规 | PowerShell | 示例|说明|
   | --- | ---| --- |---|
   |==|-eq| 
   |!=|-ne|
   |>|-gt|
   |>=|-ge|
   |<|-lt|
   |<=|-le|
   |!| ! 或者 -not| -not ($true) ||
   | 无|::|[datetime]::Now|调用静态成员|
 

1. 类型定义 
    * string
    * char
    * byte
    * int32、int
    * long
    * bool
    * decimal
    * single、float
    * guid
    * regex
    * datetime
    * timespan
    * Array
    * Hashtable
    * PsObject
    * xml
1. 括号
    * 小括号 "`()`"  
      * (...)
        表示优先级
        ```powershell
         (get-process -name win*).name
        ```
      * @(...)
        用于创建数组
        ```powershell
        $arr = @("a","b")
        ```
      * $(...)
        子表达式
        ```powershell
        "2 + 3 = $(2+3)"
        ```

    * 大括号 "`{}`"
      * @{...}   
        创建一个 hashtable
        ```powershell
        $hashtable = @{Name="PowerShell";Title="Sharing";}
        ```

    * 中括号 "`[]`"
      * 访问数组
        ```powershell
        # 返回第一个元素
        $arr[0]

        # 返回倒数第二个元素
        $arr[-2]

        # 返回 0 到 3 个元素
        $arr[0...3]
        ```
      * 访问 Hashtable
        ```powershell
        $hashtable = @{Name="PowerShell";Title="Sharing";}

        # 返回 PowerShell
        $hastable["Name"]
        ```
      * 类型声明
        ```powershell
        [string]$stringVar = "this is string"
        ```
      * 类型转换
        ```powershell
        [datetime]"2021/09/16"
        ```
      * .Net 静态类
        ```powershell
        # 返回 1
        [System.Math]::Abs(-1)

        [math]::Abs(-1)
        ```

1. 参数 （Parameter)  
参数跟在命令的名称之后，格式如下：
   ```PowerShell
   PS> Get-XXX 参数1 参数2
   PS> Get-XXX -参数名 值
   PS> Get-XXX -参数名:值
   ```

    不显示指定参数名的时候，需要根据参数定义的顺序（多个参数通过 **空格** 分隔）进行提供  

1. 使用 .Net 类库
    ```powershell
    Add-Type -Path "xxx.dll"
    [ClassLibrary1.Class1]::HelloWorld()
    ```
1. 循环
   * ForEach 关键字
   ```powershell
   $letterArray = "a","b","c","d"
   foreach ($letter in $letterArray)
   {
      Write-Host $letter
   }
   ```

   * ForEach-Object cmdlet
   ```powershell
   1..5 | ForEach-Object {$_}
   ```