using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squid.Database
{
    public class UserJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var twd = value as Users.User;
            if (twd == null)
                return;

            JToken t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                var o = (JObject)t;
                
                //Store original token in a temporary var
                var ns = o.Property("NotificationSettings");

                if (ns != null)
                {
                    //Remove original from the JObject
                    o.Remove("NotificationSettings");
                    //Add a new 'NotificationSettings' property 'stringified'
                    o.Add("NotificationSettings", ns.Value.ToString());
                }
                
                //Write away!
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(Users.User))
                return null;

            //Load our object
            JObject jObject = JObject.Load(reader);

            var dictionary = new Dictionary<string, Users.NotificationSetting>();

            if (jObject.Property("NotificationSettings") != null)
            {
                //Get the NotificationSettings token into a temp var
                var nsToken = jObject.Property("NotificationSettings").Value;
                //Remove it so it's not deserialized by Json.NET
                jObject.Remove("NotificationSettings");

                //Get the dictionary ourselves and deserialize
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, Users.NotificationSetting>>(nsToken.ToString());
            }
                
            //The output
            var output = new Users.User();
            //Deserialize all the normal properties
            if (serializer == null)
                serializer = new JsonSerializer();

            serializer.Populate(jObject.CreateReader(), output);

            //Add our dictionary
            output.NotificationSettings = dictionary;

            //return
            return output;
        }

        public override bool CanConvert(Type objectType)
        {
            //Only can convert if it's of the right type
            return objectType == typeof(Users.User);
        }
    }
}
