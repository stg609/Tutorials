## 基本概念
### Cmdlet (Command-let, aka. Powershell Command)
每个命令是一对 “动词-名词” ，比如  `Get-Process`，这也是 PowerShell 中 Cmdlet 的命名约定 。PowerShell 已经内置了几百个比较常用的命令，我们可以通过 `Get-Command` 获取所有已经安装的命令。  
Cmdlet 基本都是用 .Net 写的。

#### 常用的动词
* Get — 获取 xxx
* Start — 启动 xxx
* Out — 输出 xxx
* Stop — 停止 xxx
* Set — 设置 xxx
* New — 创建 xxx

### 函数 Function
功能上与 `Cmdlet` 差不多，相较于 `Cmdlet` 通过 .Net 写，`Function` 直接通过 Powershell 脚语言本来实现。

### 别名 Alias

```PowerShell
PS> Get-Alias -Definition Set-Location

CommandType     Name                                               
-----------     ----                                               
Alias           cd -> Set-Location
Alias           chdir -> Set-Location
Alias           sl -> Set-Location
```

### 管道 Pipeline
通过 `|` 把多个 `Cmdlet` 串联起来，前一个 `Cmdlet` 的 `输出` 作为 后一个 `Cmdlet` 的 `输入` 。

如下：

```PowerShell
PS> Get-Date | Get-Member

   TypeName: System.DateTime

Name                 MemberType     Definition
----                 ----------     ----------
Add                  Method         datetime Add(timespan value)
AddDays              Method         datetime AddDays(double value)
AddHours             Method         datetime AddHours(double value)
AddMilliseconds      Method         datetime AddMilliseconds(double value)
AddMinutes           Method         datetime AddMinutes(double value)
AddMonths            Method         datetime AddMonths(int months)
AddSeconds           Method         datetime AddSeconds(double value)
AddTicks             Method         datetime AddTicks(long value)
```
### 脚本 Script
PowerShell 脚本的扩展名，PowerShell 脚本中可以包含一个或多个 Cmdlet 的调用。执行一个脚本和调用一个 Cmdlet 很相似，通过 `路径` 和 `文件名` 来进行调用。

#### 执行策略（Execution Policy）
在 *Windows* 上，是否能够执行 PowerShell 脚本受限于执行策略。默认的执行策略为 `Restricted` (受限的)，阻止所有脚本的执行。

* AllSigned   
不管是本地的脚本还是非本地的脚本（比如来自一个链接），必须要被签名
 
* RemoteSigned  
本地脚本不限制，运行非本地的脚本必须要被签名

* Unrestricted
不受限，对于 非Windows 电脑，从 PowerShell 6 开始就是默认且唯一的策略。会有提示。

* ByPass  
绕过检查，不会有任何提示和警告。

**设置执行策略**
```PowerShell
PS> Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
```

### 会话 Session
运行 PowerShell 时候的环境, 会话可以是本地的，也可以是远程的。  
在执行某些 cmd-let 的时候可以把 session 作为参数进行传递，那么对应的 cmd-let 将会在对应的 session 中运行。  
常用于需要远程操作的场景：比如自动化部署

### 模块 Module
模块是一个包，其中包含 PowerShell 成员，例如 cmdlet、函数、变量、别名等，以一个整体进行分发、安装、加载。

通过 `Get-Module -ListAvailable` 可以获取已经安装的模块：
```PowerShell
> Get-Module -ListAvailable

    Directory: C:\program files\windowsapps\microsoft.powershell_7.1.4.0_x64__8wekyb3d8bbwe\Modules

ModuleType Version    PreRelease Name                                PSEdition ExportedCommands
---------- -------    ---------- ----                                --------- ----------------
Manifest   7.0.0.0               CimCmdlets                          Core      {Get-CimAssociatedInstance, Get-CimClass, Get-CimInstance, Get-CimSession…}
Manifest   1.2.5                 Microsoft.PowerShell.Archive        Desk      {Compress-Archive, Expand-Archive}
...
```

下载并安装安装模块 （*默认情况下，都是从 [PowerShell Gallery](https://www.powershellgallery.com/) 下载*）  
`Install-Module -Name PowerShellGet`