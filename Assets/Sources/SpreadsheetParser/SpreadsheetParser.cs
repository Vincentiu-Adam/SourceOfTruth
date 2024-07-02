using System;
using System.Reflection;

using Newtonsoft.Json.Linq;

public class SpreadsheetColumnData
{
    public int[] ColumnIndices;
    public string[] ColumnNames;

    public SpreadsheetColumnData(int columnSize)
    {
        ColumnIndices = new int[columnSize];
        ColumnNames = new string[columnSize];
    }
}

public class SpreadsheetParser
{
    public SpreadsheetColumnData CreateColumnDataForType<T>(JArray spreadsheetColumns) where T : SpreadsheetDataScriptableObject
    {
        //create temp data with all the present columns in the spreadsheet (these are not all relevant and can contain empty data), so we can filter which ones we need based on type
        SpreadsheetColumnData tempColumnData = new SpreadsheetColumnData(spreadsheetColumns.Count);

        int columnDataIndex = 0;
        for (int i = 0; i < spreadsheetColumns.Count; i++)
        {
            string columnName = GetValue(spreadsheetColumns[i]);
            if (string.IsNullOrEmpty(columnName))
            {
                continue; //skip empty columns
            }

            //check if column name is among type fields (only use public fields for now)
            Type dataType = typeof(T);
            if (!ContainsField(columnName, dataType))
            {
                continue; //skip if field does not exist
            }

            tempColumnData.ColumnIndices[columnDataIndex] = i;
            tempColumnData.ColumnNames[columnDataIndex++] = columnName;
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

    private string GetValue(JToken userValue)
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
            return value.ToString();
        }

        value = userValue["numberValue"];
        if (value != null)
        {
            return value.ToString();
        }

        value = userValue["boolValue"];
        return value?.ToString();
    }

    //check if dataType contains field specified by fieldName. We want to completely ignore case so do the check with lowercase
    private bool ContainsField(string fieldName, Type dataType)
    {
        //only look for public fields
        foreach (FieldInfo field in dataType.GetFields())
        {
            if (fieldName.ToLower().Equals(field.Name.ToLower()))
            {
                return true;
            }
        }

        return false;
    }
}
