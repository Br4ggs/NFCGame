using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class AbilityDataConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        //AbilityData data = (AbilityData)value;

        //writer.WriteStartObject();
        //writer.WritePropertyName("name");
        //serializer.Serialize(writer, data.name);

        throw new System.NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        AbilityData data = new AbilityData();

        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName)
                break;

            string propertyName = (string)reader.Value;
            if (!reader.Read())
                continue;

            switch (propertyName)
            {
                case "name":
                    {
                        data.name = serializer.Deserialize<string>(reader);
                    }
                    break;

                case "description":
                    {
                        data.description = serializer.Deserialize<string>(reader);
                    }
                    break;
                case "damage":
                    {
                        data.damage = serializer.Deserialize<int>(reader);
                    }
                    break;
                case "canDamageMultiple":
                    {
                        data.canDamageMultiple = serializer.Deserialize<bool>(reader);
                    }
                    break;
                case "heals":
                    {
                        data.heals = serializer.Deserialize<int>(reader);
                    }
                    break;
                case "pointCost":
                    {
                        data.pointCost = serializer.Deserialize<int>(reader);
                    }
                    break;
            }
        }

        return data;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(AbilityData);
    }
}
