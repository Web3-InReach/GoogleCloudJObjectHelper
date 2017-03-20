using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Datastore.V1;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using Type = System.Type;
using Value = Google.Cloud.Datastore.V1.Value;

namespace GoogleCloudJObjectHelper
{
    public static class JObjectGoogleUtils
    {

        private static readonly Dictionary<Type,Func<dynamic,Value>> FactoryTypeToGoogle = new Dictionary<Type, Func<dynamic, Value>>
        {
            {typeof(double), x=>  new Value { DoubleValue = (double)x} },
            {typeof(int), x=> new Value { IntegerValue = (long)x} },
            {typeof(long), x=> new Value { IntegerValue = (long)x} },
            {typeof(string), x=> new Value { StringValue = (string)x} },
            {typeof(DateTime), x=> new Value { TimestampValue = Timestamp.FromDateTime((DateTime)x)} },
            {typeof(bool), x=> new Value { BooleanValue = (bool)x} },
            {typeof(NullValue), x=> new Value { NullValue  = NullValue.NullValue} },
            {typeof(Entity), x=> new Value { EntityValue  = (Entity)x} }
        };


        private static readonly Dictionary<Type, Func<IEnumerable, Value>> FactoryArrayTypeToGoogle = new Dictionary<Type, Func<IEnumerable, Value>>
        {
            {typeof(double), x=>  new Value { ArrayValue = ConvertToArray<double>(x) } },
            {typeof(int), x=> new Value { ArrayValue = ConvertToArray<long>(x)} },
            {typeof(long), x=> new Value { ArrayValue = ConvertToArray<long>(x)} },
            {typeof(string), x=> new Value { ArrayValue = ConvertToArray<string>(x)} },
            {typeof(DateTime), x=> new Value { ArrayValue = ConvertToArray<DateTime>(x)}},
            {typeof(bool), x=> new Value { ArrayValue = ConvertToArray<bool>(x)} },
            {typeof(Entity), x=> new Value { ArrayValue  = ConvertToArray<Entity>(x)} },
            {typeof(Value), x=> new Value { ArrayValue  = ConvertToArray<Value>(x)} },
            {typeof(IEnumerable), x=> new Value { ArrayValue  = ConvertToArray<ArrayValue>(x)} }


        };


        public static Entity ConvertToEntity(this JObject jobject)
        {
            var entityToReturn = new Entity();
            ConvertEntityRecursion(jobject, entityToReturn);
            return entityToReturn;
        }


        private static void ConvertEntityRecursion(this JObject jobject, Entity entity)
        {

            jobject.Properties().ToList().ForEach(x =>
            {
                if (x.Value.Type == JTokenType.Object)
                {
                    AddInnerJObject(entity, x);
                }
                else if (x.Value.Type == JTokenType.Array)
                {
                    IList<dynamic> listEntities = new List<dynamic>();
                    BuildJArray(x, listEntities);
                    if (listEntities.First() is Entity)
                        entity.Properties.Add(x.Name, listEntities.Cast<Entity>().ToArray());
                    else
                        entity.Properties.Add(x.Name, listEntities.Cast<Value>().ToArray());

                }
                else
                {
                    
                    entity.Properties.Add(x.Name, ConvertToGoogleValue(((JValue)x.Value).Value));
                }
            });
        }

        private static void BuildJArray(JToken x,ICollection<dynamic> listEntities)
        {



            var tmpValue = x.Value<dynamic>();
            JArray arr;

            if (tmpValue is JProperty)
                arr = (JArray)((JProperty)tmpValue).Value;
            else
                arr = x.Value<JArray>();
            arr.ToList().ForEach(r =>
            {
                if (r.Type == JTokenType.Object)
                {
                    AddJObjectToArray(listEntities, r);
                }
                else if (r.Type == JTokenType.Array)
                {
                    AddArrayToArray(listEntities, r);
                }
                else
                {
                    listEntities.Add(ConvertToGoogleValue(r.Value<JValue>().Value));
                }
            });
        }

        private static void AddArrayToArray(ICollection<dynamic> listEntities, JToken r)
        {
            IList<dynamic> innerlistEntities = new List<dynamic>();
            BuildJArray(r, innerlistEntities);
            listEntities.Add(ConvertToGoogleArrayValue(innerlistEntities.ToArray()));
        }

        private static void AddJObjectToArray(ICollection<dynamic> listEntities, JToken r)
        {
            var entityToAdd = new Entity();
            ConvertEntityRecursion(r.Value<JObject>(), entityToAdd);
            listEntities.Add(entityToAdd);
        }

        private static void AddInnerJObject(Entity entity, JProperty x)
        {
            var entityToAdd = new Entity();
            ConvertEntityRecursion((JObject) x.Value, entityToAdd);
            entity.Properties.Add(x.Name, entityToAdd);
        }


        private static Value ConvertToGoogleValue(dynamic value)
        {
            return FactoryTypeToGoogle[value.GetType()](value);
        }


        private static Value ConvertToGoogleArrayValue(IEnumerable<dynamic> value)
        {
            return FactoryArrayTypeToGoogle[value.First().GetType()](value);
        }

        private static T[] ConvertToArray<T>(IEnumerable x)
        {
            return x.OfType<T>().ToArray();
        }
    }
}
