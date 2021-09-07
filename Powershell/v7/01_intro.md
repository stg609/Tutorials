# Intro
## What is Shell
* 中文翻译：壳，物体的坚硬外皮
* 简单来说，是一种用于接受外部输入的命令，并交由操作系统执行的一种软件。在过去，这是 Unix 系统上提供的唯一种用户交互方式，且主要为命令行交互方式（CLI），e.g. `bash`
* 大多数 Windows 用户主要以 GUI 操作为主


## What is Powershell?
* Cross-platform command-line shell
* An Object-oriented automation engine and scripting language
* Configuration management framework
* Built on .NET Framework and .Net Core

## Usecase
* 活动目录（AD）管理 (e.g. 创建1000+个新用户)
* 数据库管理 (e.g. SQL Server, Azure SQL Database)
* EventLog 监控
* 部署 / CICD
* 创建 IIS Web 应用程序
* 可以管理几乎所有的微软产品（Exchange, System Center)

## Histories
| 名称  | 时间| 备注 |
| ---  | ---| --- |
| Monad (aka. Microsoft Shell)| 2005 |
|Windows Powershell v1|2006| 对 Monad 的改名，基于 .Net Framework, 集成于 Win XP、Win Server 2003, Win Vista|
|Windows Powershell v2.0 |2009.8| 增加更多的命令，集成于 Win7、Win Server 2008 R2|
|Windows Powershell v3.0 |2012| 集成于 Win8、Win Server 2012|
|Windows Powershell v4.0 |2012| 集成于 Win8.1、Win Server 2012 R2|
|Windows Powershell v5.0 |2016| |
|Windows Powershell v5.1 |2016| 集成于  Win10、Win Server 2016|
|Powershell Core 6|2016.8| 基于 .Net Core 3，开源、跨平台 |
|Powershell 7|2020.3| 类似 .Net 5 的命名，兼容大部分 Windows Powershell 5.1 的模块，开源、跨平台|
|**Powershell 7.1**|2020.11| 基于 .Net 5，支持基于 SSH 远程连接，支持 Docker
|Powershell 7.2.0-preivew.9 | 2021.8 | 

# Setup And Installation
## Win 10
默认自带 Windows Powershell v5.1
### 安装最新版
* 下载最新安装包 https://github.com/PowerShell/PowerShell/releases/tag/v7.1.4
* 通过微软应用商店
## Linux
### Centos 7
```
# Register the Microsoft RedHat repository
curl https://packages.microsoft.com/config/rhel/7/prod.repo | sudo tee /etc/yum.repos.d/microsoft.repo

# Install PowerShell
sudo yum install -y powershell
```
### Ubuntu 16.04
```
# Update the list of packages
sudo apt-get update
# Install pre-requisite packages.
sudo apt-get install -y wget apt-transport-https software-properties-common
# Download the Microsoft repository GPG keys
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
# Register the Microsoft repository GPG keys
sudo dpkg -i packages-microsoft-prod.deb
# Update the list of packages after we added packages.microsoft.com
sudo apt-get update
# Install PowerShell
sudo apt-get install -y powershell
```
## Docker
镜像：`mcr.microsoft.com/powershell`

举例:  
`> docker run -it mcr.microsoft.com/powershell`

# Compatibility Aliases

|            cmd.exe command            | UNIX command | PowerShell cmdlet |             PowerShell alias              |
| ------------------------------------- | ------------ | ----------------- | ----------------------------------------- |
| **cd**, **chdir**                     | **cd**       | `Set-Location`    | `sl`, `cd`, `chdir`                       |
| **cls**                               | **clear**    | `Clear-Host`      | `cls` `clear`                             |
| **copy**                              | **cp**       | `Copy-Item`       | `cpi`, `cp`, `copy`                       |
| **del**, **erase**, **rd**, **rmdir** | **rm**       | `Remove-Item`     | `ri`, `del`, `erase`, `rd`, `rm`, `rmdir` |
| **dir**                               | **ls**       | `Get-ChildItem`   | `gci`, `dir`, `ls`                        |
| **echo**                              | **echo**     | `Write-Output`    | `write` `echo`                            |
| **md**                                | **mkdir**    | `New-Item`        | `ni`                                      |
| **move**                              | **mv**       | `Move-Item`       | `mi`, `move`, `mi`                        |
| **popd**                              | **popd**     | `Pop-Location`    | `popd`                                    |
|                                       | **pwd**      | `Get-Location`    | `gl`, `pwd`                               |
| **pushd**                             | **pushd**    | `Push-Location`   | `pushd`                                   |
| **ren**                               | **mv**       | `Rename-Item`     | `rni`, `ren`                              |
| **type**                              | **cat**      | `Get-Content`     | `gc`, `cat`, `type`                       |

* 摘自 https://docs.microsoft.com/en-us/powershell/scripting/learn/compatibility-aliases?view=powershell-7.1

# Have a Try
## 查看版本
```
> $PSVersionTable

Name                           Value
----                           -----
PSVersion                      7.1.4
PSEdition                      Core
GitCommitId                    7.1.4
OS                             Microsoft Windows 10.0.18363
Platform                       Win32NT
PSCompatibleVersions           {1.0, 2.0, 3.0, 4.0…}
PSRemotingProtocolVersion      2.3
SerializationVersion           1.1.0.1
WSManStackVersion              3.0
```
## 命令举例
* 切换目录  
`> cd 'C:\Program Files'`  
`> Set-Location C:\Software`
* 获取所有 powershell 命令  
`> Get-Command`
* 获取帮助  
`> Get-Help Get-Command`
* 获取当前日期和时间  
`> Get-Date`
* 获取所有进程   
`> Get-Process`
* 关机  
`> Stop-Computer`
* 重启  
`> Restart-Computer`
* 检查路径是否存在  
`> Test-Path C:\test.txt`
### 文件操作
* 获取文件内容  
`> Get-Content -Path .\test.txt `
### 网络操作
* 访问网址  
`> Inovke-WebRequest https://www.baidu.com`

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

```
> Get-Alias -Definition Set-Location

CommandType     Name                                               
-----------     ----                                               
Alias           cd -> Set-Location
Alias           chdir -> Set-Location
Alias           sl -> Set-Location
```

### Module
模块是一个包，其中包含 PowerShell 成员，例如 cmdlet、函数、变量、别名等，以一个整体进行分发、安装、加载。

通过 `Get-Module -ListAvailable` 来获取已经安装的模块：
```
> Get-Module -ListAvailable

    Directory: C:\program files\windowsapps\microsoft.powershell_7.1.4.0_x64__8wekyb3d8bbwe\Modules

ModuleType Version    PreRelease Name                                PSEdition ExportedCommands
---------- -------    ---------- ----                                --------- ----------------
Manifest   7.0.0.0               CimCmdlets                          Core      {Get-CimAssociatedInstance, Get-CimClass, Get-CimInstance, Get-CimSession…}
Manifest   1.2.5                 Microsoft.PowerShell.Archive        Desk      {Compress-Archive, Expand-Archive}
...
```

下载并安装安装模块  
`Install-Module -Name PowerShellGet`

默认情况下，都是从 [PowerShell Gallery](https://www.powershellgallery.com/) 下载

### Pipeline (|)
通过 `|` 把多个 `Cmdlet` 串联起来，前一个 `Cmdlet` 的 `输出` 作为 后一个 `Cmdlet` 的 `输入` 。

如下：

```
> Get-Date | Get-Member

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
### Script
PowerShell 脚本的扩展名，PowerShell 脚本中可以包含一个或多个 Cmdlet。

#### 执行脚本
执行一个脚本和调用一个 Cmdlet 很相似，通过 `路径` 和 `文件名` 来进行调用。

#### 执行策略（Execution Policy）
在 Windows 上，是否能够执行 PowerShell 脚本，受限于执行策略。这是 PowerShell 的一种安全策略，表示在什么情况下允许执行 PowerShell 脚本。默认的执行策略为 `Restricted` (受限的)，阻止所有脚本的执行。

* AllSigned   
不管是本地的脚本还是非本地的脚本（比如来自一个链接），必须要被签名
 
* RemoteSigned  
本地脚本不限制，运行非本地的脚本必须要被签名

* ByPass  
没有限制

**设置执行策略**
```
> Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
```


## IDE
### PowerShell ISE (Integrated Scripting Environment)
### VSCode

# TODO
Ctrl+C  
基本元素  
Session  
管道  
单引号双引号  
默认的 .Net 名称空间  
基本语法 （弱类型、Scope、忽略大小写、注释、比较运算符、循环、异常 try...catch...、Escape） 
CSV
远程


# Demo
自动化脚本  



# References
* [PowerShell Documentation](https://docs.microsoft.com/en-us/powershell/)
* [Powershell Wikipedia](https://en.wikipedia.org/wiki/PowerShell#cite_note-17)