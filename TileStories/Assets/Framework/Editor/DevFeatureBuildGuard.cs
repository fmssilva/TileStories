using System.Collections.Generic;

namespace TileStories.Editor
{
    // One developer-only switch that lives in a wall's config.json. Register every such switch here
    // (_5.1_Editor_Tab.md, "Dev-only features and build safety") so a build never ships one by accident.
    public class DevSwitch
    {
        public string Name;                                        // what the developer sees in the window
        public string HowToDisable;                                // where to untick it
        public bool ActiveInReleaseBuild;                          // false = the runtime ignores it outside development builds
        public System.Func<WallConfigData, bool> IsOn;
    }

    // Pure logic: which developer-only switches are ON in a config and would actually take effect in
    // the build being made. No Unity build API here, so it is testable with plain data.
    public static class DevFeatureBuildGuard
    {
        public static readonly DevSwitch[] Registry =
        {
            new DevSwitch
            {
                Name = "Add effects demo grid",
                HowToDisable = "POI Editor > Global Scene > Effects > Test > untick 'Add effects demo grid', then Save All to JSON and Copy to StreamingAssets",
                ActiveInReleaseBuild = false,
                IsOn = c => c.effect_defaults != null && c.effect_defaults.preview != null && c.effect_defaults.preview.enabled,
            },
            new DevSwitch
            {
                Name = "Add outline demo grid",
                HowToDisable = "POI Editor > Global Scene > Outline > Test > untick 'Add outline demo grid', then Save All to JSON and Copy to StreamingAssets",
                ActiveInReleaseBuild = false,
                IsOn = c => c.outline_preview != null && c.outline_preview.enabled,
            },
            new DevSwitch
            {
                Name = "Add LOD demo field",
                HowToDisable = "POI Editor > Global Scene > LOD > Test > untick 'Add LOD demo field', then Save All to JSON and Copy to StreamingAssets",
                ActiveInReleaseBuild = false,
                IsOn = c => c.demo_field != null && c.demo_field.enabled,
            },
            new DevSwitch
            {
                Name = "Add displacement demo",
                HowToDisable = "POI Editor > Global Scene > Displacement > Test > untick 'Add displacement demo', then Save All to JSON and Copy to StreamingAssets",
                ActiveInReleaseBuild = false,
                IsOn = c => c.displacement_demo != null && c.displacement_demo.enabled,
            },
            new DevSwitch
            {
                Name = "Add search demo",
                HowToDisable = "POI Editor > Global Scene > Select, Filter & Search > Test > untick 'Add search demo', then Save All to JSON and Copy to StreamingAssets",
                ActiveInReleaseBuild = false,
                IsOn = c => c.search_demo != null && c.search_demo.enabled,
            },
        };

        // Messages for every switch that is ON and would take effect in this kind of build
        public static List<string> ActiveMessages(WallConfigData config, bool developmentBuild)
        {
            var messages = new List<string>();
            if (config == null) return messages;

            foreach (var sw in Registry)
            {
                if (!sw.IsOn(config)) continue;
                if (!developmentBuild && !sw.ActiveInReleaseBuild) continue;
                messages.Add("'" + sw.Name + "' is ON. The app would show a developer view instead of the real app. To turn it off: " + sw.HowToDisable + ".");
            }
            return messages;
        }
    }
}
