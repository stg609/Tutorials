# 演示如何通过自定义的C# 类型来把 mongo 中大写开头的属性名转成 camel case

# 先编译成 dll
Add-Type -path "C:\Users\admin\.nuget\packages\mongodb.bson\2.13.2\lib\netstandard2.1\MongoDB.Bson.dll"
Add-Type -Path "E:\DangQu.PT.LogicalArrangement\ClassLibrary1\bin\Release\net5.0\TransformLogHelper.dll"

#Install-Module Newtonsoft.Json
Import-Module Mdbc
Import-Module Newtonsoft.Json
Connect-Mdbc "mongodb://admin:bestadmin@192.168.123.154:27017?authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false" "39f7827aa8f52d7642edb3de6b1a91e3" "t60e5126962771d0967f0f5bb_transformLog"

$elasticUri = 'http://192.168.123.154:9200/test4/_doc'
$username = "admin"
$password = "bestadmin"
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username,$password)))

# 转成 BsonDocument[]
$forms = Get-MdbcData  {_id: "610c8aab9f2fb9307da9d041"} -As ([MongoDB.Bson.BsonDocument]) -First 10

foreach($itm in $forms)
{
    # 调用自定义方法来反序列化
    [OperationRecordEntity] $rslt2 = [Helper]::Deserialize($itm)
    $json = [Newtonsoft.Json.JsonConvert]::SerializeObject($rslt2)

    Invoke-Restmethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} -uri $elasticUri -ContentType 'application/json;charset=utf-8' -method POST -Body $json
}