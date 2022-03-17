# 适用于 Powershell 5.1
# 用于迁移原先表单设计中变更记录的开关到新的表单设置中
# 1. 原先显示变更记录，那么迁移到表单设置中存储及显示都打开
# 2. 原先关闭变更记录，那么迁移到表单设置中存储及显示都关闭

# 先安装 Mdbc 用于通过 ps 来操作 mongo，目前仅支持 ps 5.1 , refer to https://github.com/nightroman/Mdbc
# Install-Module Mdbc

$psqlBin = "C:\Program Files\pgAdmin 4\v5\runtime\psql.exe"

Import-Module Mdbc

# 1. 获取所有租户的连接字符串信息
$psqlConnStr = "postgresql://postgres:bestadmin@192.168.123.154:5433/Tenancy_Module" # dev 环境

# -c 表示执行命令 -t 表示只返回数据行，不返回列名
$tenants = & $psqlBin  -t -c 'SELECT ""ConnectStrings""->>''AppConnectString'' FROM ""Tenants"";' $psqlConnStr;


# 2. 遍历所有租户的 connectstrings
foreach ($tenant in $tenants) {
    if([string]::IsNullOrWhiteSpace($tenant))
    {
        continue;
    }
    $tenantConn = $($tenant -split "_")[0].Trim()

    # 2.1 连接 mongo 查询客户表
    Connect-Mdbc "mongodb://admin:bestadmin@192.168.123.154:27018/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false" $tenantConn "Form"

    # 2.2 获取该 tenant 下的 Form 中 IsToShowDataOperationRecordBox 是 true 的
    $forms =  Get-MdbcData @{ "CurrentFormDesigner.Layout.IsToShowDataOperationRecordBox"= $true; "IsDelete"=$false;} -Project "{AppId:1,_id:1,Name:1}";
    Connect-Mdbc "mongodb://admin:bestadmin@192.168.123.154:27018/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false" $tenantConn "FormSetting"
    foreach($form in $forms)
    {
        # 2.3 修改 FormSetting 中 IsChangeRecordStore 及 IsChangeRecordShow
        Write-Output "$tenantConn,$($form.Name),$($form.AppId),$($form._id)"
        Update-MdbcData @{FormId=$($form._id)} @{'$set' = @{ "ChangeRecordHook.Hook.IsChangeRecordShow" = $true; "ChangeRecordHook.Hook.IsChangeRecordStore" = $true } }
        break;
    }
    break;
}