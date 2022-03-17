# 演示如何通过自定义的C# 类型来把 mongo 中大写开头的属性名转成 camel case
# 先编译成 dll
Add-Type -Path "xxx\TransformLogHelper.dll"

Import-Module Mdbc
Connect-Mdbc "mongodb://admin:bestadmin@192.168.123.154:27017authSource=admin&readPreference=primary&appname=MongoDB%20Compass&ssl=false" "39f7827aa8f52d7642edb3de6b1a91e3" "t60e5126962771d0967f0f5bb_transformLog"

# 转成 BsonDocument[]
$forms = Get-MdbcData @{_id="610c8aab9f2fb9307da9d041"} -First 1 -As ([MongoDB.Bson.BsonDocument[]])

foreach($itm in $forms)
{
    # 调用自定义方法来反序列化
    [OperationRecordEntity] $rslt2 = [Helper]::Deserialize($itm)
    $rslt2
}