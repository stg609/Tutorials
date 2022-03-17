using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class OperationRecordEntity
{
    public string _id { get; set; }
    [BsonElement("DataId")]
    public string dataId { get; set; }
    [BsonElement("ModifiedTime")]
    public DateTime modifiedTime { get; set; }
    [BsonElement("ModifiedUserId")]
    public string modifiedUserId { get; set; }
    [BsonElement("ModifiedUserName")]
    public string modifiedUserName { get; set; }
    [BsonElement("Avatar")]
    public string avatar { get; set; }
    /// <summary>
    /// 操作类型 1修改 2新增 3删除 4查看
    /// </summary>
    [BsonElement("OperationRecordType")]
    public int operationRecordType { get; set; }
    [BsonElement("DeletedRecord")]
    public List<DeleteRecord> deletedRecord { get; set; } = new List<DeleteRecord>();
    [BsonElement("InsertedRecord")]
    public List<InsertRecord> insertedRecord { get; set; } = new List<InsertRecord>();
    [BsonElement("InsertMainFieldRecord")]
    public List<InsertRecordField> insertMainFieldRecord { get; set; } = new List<InsertRecordField>();
    [BsonElement("ModifiedRecord")]
    public List<FieldTransformInfo> modifiedRecord { get; set; } = new List<FieldTransformInfo>();  
}

public class InsertRecordField
{
    [BsonElement("FieldWholeName")]
    public string fieldWholeName { get; set; }
    [BsonElement("FieldName")]
    public string fieldName { get; set; }
    [BsonElement("Value")]
    public string value { get; set; }
    [BsonElement("FieldDisplayName")]
    public string fieldDisplayName { get; set; }
    [BsonElement("Path")]
    public string path { get; set; }
}

public class FieldTransformInfo
{
    [BsonElement("FieldWholeName")]
    public string fieldWholeName { get; set; }
    [BsonElement("OperRecord")]
    public List<Fields> operRecord { get; set; }
}

public class Fields : UpdateData
{
    [BsonElement("FormWholeName")]
    public string formWholeName { get; set; }
    [BsonElement("ParentFormName")]
    public string parentFormName { get; set; }
    [BsonElement("FieldDisplayName")]
    public string fieldDisplayName { get; set; }
    [BsonElement("OldValue")]
    public string oldValue { get; set; }
    /// <summary>
    /// 1修改 2新增 3删除
    /// </summary>
    [BsonElement("CurrentFormOperationType")]
    public int currentFormOperationType { get; set; }
}

public class UpdateData
{
    [BsonElement("Field")]
    public string field { get; set; }

    [BsonElement("NewValue")]
    public string newValue { get; set; }
    /// <summary>
    /// 子表数据标题
    /// </summary>
    [BsonElement("SubFormDataDisplayName")]
    public string subFormDataDisplayName { get; set; }
    /// <summary>
    /// 子表名
    /// </summary>
    [BsonElement("SubFormName")]
    public string subFormName { get; set; }
    /// <summary>
    /// 子表Id
    /// </summary>
    [BsonElement("SubFormId")]
    public string subFormId { get; set; }
    [BsonElement("Path")]
    public string path { get; set; }

    [BsonElement("FieldWholeName")]
    public string fieldWholeName { get; set; }

    [BsonElement("ComponentName")]
    public string componentName { get; set; }
}

public class InsertRecord
{
    [BsonElement("FormName")]
    public string formName { get; set; }

    [BsonElement("FormWholeName")]
    public string formWholeName { get; set; }

    [BsonElement("FormDisplayName")]
    public string formDisplayName { get; set; }

    [BsonElement("DataDisplayName")]
    public string dataDisplayName { get; set; }

    [BsonElement("Fields")]
    public List<InsertRecordField> fields { get; set; }
}

public class DeleteRecord
{
    [BsonElement("FormName")]
    public string formName { get; set; }

    [BsonElement("FormWholeName")]
    public string formWholeName { get; set; }

    [BsonElement("DataDisplayName")]
    public string dataDisplayName { get; set; }

    [BsonElement("DataId")]
    public string dataId { get; set; }
}


public static class Helper
{
    public static string ToJson(BsonDocument doc)
    {
        return doc.ToJson();
    }

    public static OperationRecordEntity Deserialize(BsonDocument doc)
    {
        return BsonSerializer.Deserialize<OperationRecordEntity>(doc,null);
    }
}
