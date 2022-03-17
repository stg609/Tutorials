# 适用于 Powershell 5.1
# 先安装 Mdbc 用于通过 ps 来操作 mongo，目前仅支持 ps 5.1 , refer to https://github.com/nightroman/Mdbc
# Install-Module Mdbc

# 设置 cookie，因为获取时区的接口根据 cookie 来进行本地化
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$cookie = New-Object System.Net.Cookie
$cookie.Name = ".AspNetCore.Culture"
$cookie.Value = "c%3Dzh-Hans%7Cuic%3Dzh-Hans"
$cookie.Domain = "test.powertradepro.com"
$session.Cookies.Add($cookie)

# 获取所有国家信息
$resp = Invoke-RestMethod https://api.test.powertradepro.com/basicdata/api/app/country/fullList  -WebSession $session 
$countries = $resp.items

# 获取所有时区信息（通过cookie来返回不同的语言）
$resp = Invoke-RestMethod https://api.test.powertradepro.com/basicdata/api/app/windowsTimeZone -WebSession $session 
$timezones = $resp.items

Import-Module Mdbc

# 连接 mongo 查询客户表
Connect-Mdbc "mongodb://admin:bestadmin@192.168.123.154:27017/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false" "39f7827aa8f52d7642edb3de6b1a91e3" "t60dd104b2f797db9f43914b0" #测试
#Connect-Mdbc "mongodb://rwuser:!passWORD123@121.36.203.45:8635/?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false" "m4000000" "t60dfbc2fff7103915bf77ccc" #生产
$data = Get-MdbcData -Project "{customerName:1,customerCode:1,Country_code:1,Country_displayname:1,originId:1}" #@{_id= "61dfef40271df0a7fb2edcb0"};

# 加载 mysql driver
[void][System.Reflection.Assembly]::LoadWithPartialName("MySql.Data")
$connectionString = "server=192.168.123.154;port=30007;uid=root;pwd=bestadmin;database=mail;charset=utf8" #测试
#$connectionString = "server=192.168.123.49;port=13306;uid=root;pwd=!passWORD;database=mail_prd;charset=utf8" #生产

$connection = New-Object MySql.Data.MySqlClient.MySqlConnection($connectionString)
$connection.Open()

$total = 0;
foreach($itm in $data){
    $customerName =  $itm.customerName;
    $customerCode =  $itm.customerCode;
    $ccode = $itm.Country_code;
    $cname = $itm.Country_displayname;

    # 找到该客户的国家信息
    $country = [System.Linq.Enumerable]::FirstOrDefault($countries,[Func[object,bool]]{ param($x) $x.code -eq $itm.Country_code});
    if($null -ne $country){
        # 找到该国家的时区信息
        $timezone = [System.Linq.Enumerable]::FirstOrDefault($timezones,[Func[object,bool]]{ param($x) $x.code -eq $country.timeZone});
        # Write-host $timezone

        if($null -ne $timezone)
        {
            # 更新该客户的时区信息
            Update-MdbcData @{_id=$itm._id} @{'$set'=@{Timezone_code=$timezone.code;Timezone_offset=$timezone.offset;Timezone_displayname=$timezone.displayName}}

            # 更新 mysql 中的数据
            $sql = "update customer set timezone = '"+ $timezone.displayName +"' where origin_id = '"+ $itm.originId +"' "
            $command = New-Object MySql.Data.MySqlClient.MySqlCommand -ArgumentList $sql,$connection
            $command.ExecuteNonQuery();

            $total += 1;
        }
        else
        {
            Write-Warning "该客户($customerName,$customerCode)的国家($ccode,  $cname)对应的时区未找到！"
        }
    }
    else
    {
        Write-Warning "该客户($customerName,$customerCode)的国家($ccode,  $cname)未找到！"
    }
}

# 关闭 mysql 连接
$connection.Close()

Write-Output "共更新 $total 条！"