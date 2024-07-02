using System;
using System.Reflection;

using Newtonsoft.Json.Linq;

[AttributeUsage(AttributeTargets.Field)]
public class SpreadsheetNameAttribute : Attribute
{
    public string Name;

    public SpreadsheetNameAttribute(string name)
    {
        Name = name.ToLower();
    }
}

public class SpreadsheetColumnData
{
    public int[] ColumnIndices;
    public string[] ColumnNames;

    public int Length => ColumnIndices.Length;

    public SpreadsheetColumnData(int columnSize)
    {
        ColumnIndices = new int[columnSize];
        ColumnNames = new string[columnSize];
    }
}

public class SpreadsheetParser
{
    public static SpreadsheetColumnData CreateColumnDataForType<T>(JArray spreadsheetColumns) where T : SpreadsheetDataScriptableObject
    {
        //create temp data with all the present columns in the spreadsheet (these are not all relevant and can contain empty data), so we can filter which ones we need based on type
        SpreadsheetColumnData tempColumnData = new SpreadsheetColumnData(spreadsheetColumns.Count);

        int columnDataIndex = 0;
        for (int i = 0; i < spreadsheetColumns.Count; i++)
        {
            object columnName = GetValue(spreadsheetColumns[i]);
            if (columnName == null)
            {
                continue; //skip empty columns
            }

            //check if column name is among type fields (only use public fields for now)
            Type dataType = typeof(T);
            if (!ContainsField(columnName.ToString(), dataType, out _))
            {
                continue; //skip if field does not exist
            }

            tempColumnData.ColumnIndices[columnDataIndex] = i;
            tempColumnData.ColumnNames[columnDataIndex++] = columnName.ToString();
        }

        //create new column data with only relevant size and init the data
        SpreadsheetColumnData columnData = new SpreadsheetColumnData(columnDataIndex);
        for (int i = 0; i < columnDataIndex; i++)
        {
            columnData.ColumnIndices[i] = tempColumnData.ColumnIndices[i];
            columnData.ColumnNames[i] = tempColumnData.ColumnNames[i];
        }

        return columnData;
    }

    //fetch the data based on the column data from the spreadsheet and set it to the data container
    public static void SetDataForTypeFromColumn<T>(JArray spreadsheetColumns, SpreadsheetColumnData columnData, ref T dataContainer) where T : SpreadsheetDataScriptableObject
    {
        Type dataType = typeof(T);
        for (int i = 0; i < columnData.Length; i++)
        {
            int columnIndex = columnData.ColumnIndices[i];
            string columnName = columnData.ColumnNames[i];

            //set the value to the field 
            if (ContainsField(columnName, dataType, out FieldInfo field))
            {
                JToken userValue = spreadsheetColumns[columnIndex];
                object columnValue = field.FieldType.IsEnum ? GetEnumValue(userValue, field.FieldType) : GetValue(userValue);

                field.SetValue(dataContainer, columnValue);
            }
        }
    }

    private static object GetValue(JToken userValue)
    {
        userValue = userValue["userEnteredValue"];
        if (userValue == null)
        {
            return null;
        }

        //fetch one of different values from the sheet ["numberValue", "stringValue", "boolValue"]
        JToken value = userValue["stringValue"];
        if (value != null)
        {
            return value.Value<string>();
        }

        value = userValue["numberValue"];
        if (value != null)
        {
            return value.Value<float>();
        }

        value = userValue["boolValue"];
        return value?.Value<bool>();
    }

    private static object GetEnumValue(JToken userValue, Type enumType)
    {
        userValue = userValue["userEnteredValue"];
        if (userValue == null)
        {
            return null;
        }

        //enums will always be strings in the spreadsheet (otherwise wtf are you using numbers for enums? :/)
        JToken value = userValue["stringValue"];
        if (value == null)
        {
            return null;
        }

        string lowerCaseValue = value.Value<string>().ToLower();

        Array enumValues = Enum.GetValues(enumType);
        foreach (object enumValue in enumValues)
        {
            string lowerCaseEnumValue = enumValue.ToString().ToLower();
            if (lowerCaseValue.Equals(lowerCaseEnumValue))
            {
                return enumValue;
            }
        }

        return null;
    }

    //check if dataType contains field specified by fieldName. We want to completely ignore case so do the check with lowercase
    private static bool ContainsField(string fieldName, Type dataType, out FieldInfo foundField)
    {
        foundField = null;

        //only look for public fields
        foreach (FieldInfo field in dataType.GetFields())
        {
            string publicFieldName = field.Name.ToLower();

            //if we have a custom spreadsheet name attribute use that instead
            var fieldNameAttribute = field.GetCustomAttribute<SpreadsheetNameAttribute>();
            if (fieldNameAttribute != null)
            {
                publicFieldName = fieldNameAttribute.Name;
            }

            if (fieldName.ToLower().Equals(publicFieldName))
            {
                foundField = field;
                return true;
            }
        }

        return false;
    }
}
