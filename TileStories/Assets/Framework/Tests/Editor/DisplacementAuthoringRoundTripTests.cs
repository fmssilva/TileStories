using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Every Displacement and displacement demo field survives Save All to JSON -> load (JsonUtility, the
    // same serializer the POI Editor and WallConfigLoader use). Walked by reflection, so a new field
    // cannot be forgotten.
    public class DisplacementAuthoringRoundTripTests
    {
        private static object NonDefault(FieldInfo f, object current)
        {
            if (f.FieldType == typeof(bool)) return !(bool)current;
            if (f.FieldType == typeof(int)) return (int)current + 3;
            if (f.FieldType == typeof(float)) return (float)current + 0.25f;
            if (f.FieldType == typeof(string)) return (string)current + "_x";
            throw new AssertionException("no round-trip value for " + f.Name);
        }

        [Test]
        public void EveryDisplacementAndDemoField_SurvivesAJsonRoundTrip()
        {
            var config = new WallConfigData { wall_id = "rt" };
            object[] blocks = { config.displacement_settings, config.displacement_demo };
            foreach (var block in blocks)
                foreach (var f in block.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                    f.SetValue(block, NonDefault(f, f.GetValue(block)));

            var loaded = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(config, true));
            object[] loadedBlocks = { loaded.displacement_settings, loaded.displacement_demo };
            for (int b = 0; b < blocks.Length; b++)
            {
                var fields = blocks[b].GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
                Assert.Greater(fields.Count, 5);
                foreach (var f in fields)
                    Assert.AreEqual(f.GetValue(blocks[b]), f.GetValue(loadedBlocks[b]), blocks[b].GetType().Name + "." + f.Name + " survives the round trip");
            }
        }

        [Test]
        public void AConfigWithoutTheBlocks_GetsTheDefaults_DemoOff()
        {
            var loaded = JsonUtility.FromJson<WallConfigData>("{\"wall_id\":\"old\"}");
            Assert.IsNotNull(loaded.displacement_settings);
            Assert.IsTrue(loaded.displacement_settings.enabled);
            Assert.IsNotNull(loaded.displacement_demo);
            Assert.IsFalse(loaded.displacement_demo.enabled, "a developer-only switch is off unless ticked");
        }
    }
}
