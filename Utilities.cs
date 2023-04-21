
using Newtonsoft.Json.Linq;

namespace Worker;

public static class Utilities
{

/*
The first approach directly parses each JSON object from the multiJson string and looks for the specified ID in the "it_bit_it" array of each object. 
This means that the amount of work performed is proportional to the number of JSON objects in the input string. 
If there are a large number of objects, this could become computationally expensive.

The second approach recursively traverses the JSON object tree looking for the specified ID. 
This means that the amount of work performed is proportional to the size of the JSON object, 
but not necessarily the number of objects in the input string. 
Depending on the structure of the JSON objects and the number of nested arrays/objects, 
this approach could also become computationally expensive.

In general, if the input string contains a small number of JSON objects, 
the first approach may be faster since it avoids the overhead of recursively traversing the entire JSON tree. 
However, if the input string contains a large number of JSON objects or the JSON objects are deeply nested, 
the second approach may be faster since it avoids repeatedly parsing each JSON object and instead only traverses the tree once.

It's difficult to give an exact number for what qualifies as a "small" number of JSON objects, 
as it can depend on a variety of factors such as the size of each object and the complexity of the data structure. 
However, as a rough guideline, if you're dealing with less than 10,000 JSON objects, 
the performance difference between the two approaches is likely to be negligible for most use cases.

If you're dealing with larger datasets, it's a good idea to benchmark both approaches on your specific use case to determine which is faster.
*/

    public JObject FindJsonById(string multiJson, string id)
    {
        string[] jsonObjects = multiJson.Trim().Split('\n');

        foreach (string jsonObject in jsonObjects)
        {
            JObject parsed = JObject.Parse(jsonObject);
            JArray items = (JArray)parsed["mappingOutput"]["it_bit_it"];

            foreach (JObject item in items)
            {
                string itemId = item["ID"].ToString();

                if (itemId == id)
                {
                    return parsed;
                }
            }
        }

        // Return null if the ID is not found in any of the JSON objects
        return null;
    }

    public JObject FindJsonById(string multiJson, string id)
    {
        string[] jsonObjects = multiJson.Trim().Split('\n');

        foreach (string jsonObject in jsonObjects)
        {
            JObject parsed = JObject.Parse(jsonObject);
            JObject result = FindJsonById(parsed, id);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private JObject FindJsonById(JObject obj, string id)
    {
        foreach (var property in obj.Properties())
        {
            if (property.Value is JObject)
            {
                JObject result = FindJsonById((JObject)property.Value, id);
                if (result != null)
                {
                    return result;
                }
            }
            else if (property.Value is JArray)
            {
                foreach (var item in property.Value)
                {
                    if (item is JObject)
                    {
                        JObject result = FindJsonById((JObject)item, id);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            else if (property.Name == "ID" && property.Value.ToString() == id)
            {
                return obj;
            }
        }

        return null;
    }


}