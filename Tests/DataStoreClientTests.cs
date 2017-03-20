using System;
using System.Collections.Generic;
using Google.Cloud.Datastore.V1;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace GoogleCloudJObjectHelper.Tests
{

    [TestFixture]
    public class DataStoreClientTests
    {
        public static DateTime NowUtc = DateTime.UtcNow;

        [Test]
        public void TestSimpleJsonToEntityConverter()
        {
            var simple = new SimpleJson();
            var obj = JObject.FromObject(simple);
            var entity = obj.ConvertToEntity();
            AssertSimpleJson(entity);
        }

        private static void AssertSimpleJson(Entity entity)
        {
            Assert.True(entity.Properties.Count == 5);
            Assert.True(entity.Properties["Name"].StringValue == "David");
            Assert.True(entity.Properties["Age"].IntegerValue == 30);
            Assert.True(entity.Properties["IsAccepted"].BooleanValue);
            Assert.True(entity.Properties["LastChecked"].TimestampValue.Equals(Timestamp.FromDateTime(NowUtc)));
            Assert.True(entity.Properties["Price"].DoubleValue == 65.54);
        }


        [Test]
        public void TestComplicatedJsonToEntityConverter()
        {
            var Complicated = new ComplicatedJson();
            var obj = JObject.FromObject(Complicated);
            var entity = obj.ConvertToEntity();
            AssetComplicated(entity);
        }

        private static void AssetComplicated(Entity entity)
        {
            Assert.True(entity.Properties.Count == 7);
            AssertSimpleJson((Entity) entity.Properties["simple1"]);
            AssertSimpleJson((Entity) entity.Properties["simple2"]);
        }


        [Test]
        public void TestComplicatedJsonWithSimpleArrayValues()
        {
            var ComplicatedWithArray = new ComplicatedJsonWithSimpleArray();
            var obj = JObject.FromObject(ComplicatedWithArray);
            var entity = obj.ConvertToEntity();

            Assert.True(entity.Properties.Count == 9);
            Assert.True(entity.Properties["simpleString"].ArrayValue.Values[0].StringValue == "a");
            Assert.True(entity.Properties["simpleString"].ArrayValue.Values[1].StringValue == "b");
            Assert.True(entity.Properties["simpleString"].ArrayValue.Values[2].StringValue == "c");
            Assert.True(entity.Properties["SimpleDateTimes"].ArrayValue.Values[0].TimestampValue.Equals(Timestamp.FromDateTime(NowUtc)));
        }





        [Test]
        public void TestComplicatedJsonWithArrayToEntityConverter()
        {
            var complicatedWithArray = new ComplicatedJsonWithComplexArray();
            var obj = JObject.FromObject(complicatedWithArray);
            var entity = obj.ConvertToEntity();


            Assert.True(entity.Properties.Count == 12);
            AssertSimpleJson((Entity)entity.Properties["simple11"].ArrayValue.Values[0]);
            AssertSimpleJson((Entity)entity.Properties["simple11"].ArrayValue.Values[1]);
            AssetComplicated((Entity)entity.Properties["simple12"].ArrayValue.Values[0]);
            AssetComplicated((Entity)entity.Properties["simple12"].ArrayValue.Values[1]);
            AssetComplicated(entity.Properties["lst"].ArrayValue.Values[0].ArrayValue.Values[0].ArrayValue.Values[0].EntityValue);
        }



    }






    public class SimpleJson
    {
        public string Name { get; set; } = "David";
        public int Age { get; set; } = 30;
        public bool IsAccepted { get; set; } = true;
        public DateTime LastChecked { get; set; } = DataStoreClientTests.NowUtc;
        public Double Price { get; set; } = 65.54;
    }


    public class ComplicatedJson : SimpleJson
    {
        public SimpleJson simple1 { get; set; } = new SimpleJson();
        public SimpleJson simple2 { get; set; } = new SimpleJson();


    }

    public class ComplicatedJsonWithSimpleArray : ComplicatedJson
    {
        public List<string> simpleString = new List<string> { "a","b","c"};
        public List<DateTime> SimpleDateTimes = new List<DateTime> { DataStoreClientTests.NowUtc };



    }

    public class ComplicatedJsonWithComplexArray : ComplicatedJsonWithSimpleArray
    {
        public List<SimpleJson> simple11 { get; set; } = new List<SimpleJson> { new SimpleJson(), new SimpleJson() };
        public List<ComplicatedJson> simple12 { get; set; } = new List<ComplicatedJson> { new ComplicatedJson(), new ComplicatedJson() };


        public List<List<List<ComplicatedJson>>> lst = new List<List<List<ComplicatedJson>>> {  new List<List<ComplicatedJson>> { new List<ComplicatedJson> { new ComplicatedJson() } } };

    }
}
