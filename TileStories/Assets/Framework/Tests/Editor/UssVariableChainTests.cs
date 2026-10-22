using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Pins the USS variable behaviour the card's token tiers stand on (_3.3 R4, measured in P0 S1 on 6000.3): a token may point at
    // another token (--a: var(--b)), two and three levels deep, with a var(--x, fallback); a sheet reads tokens an ANCESTOR
    // defines in another sheet; and a primitive overridden on a descendant reaches every token built on it below that descendant
    // (USS substitutes at the element that USES the value -- unlike CSS, which computes a custom property where it is declared).
    // An engine upgrade that changes any of this turns this fixture red before a single themed card looks wrong.
    // Real panel: a utility EditorWindow (public API only), closed in TearDown.
    public class UssVariableChainTests
    {
        private const string FixtureFolder = "Assets/Framework/Tests/Editor/UssVariableChain/";
        private const int MaxFramesToResolve = 120;

        // The fixture colours, as written in ChainTokens.uss / ChainConsumers.uss
        private static readonly Color32 Primitive = new Color32(10, 120, 200, 255);
        private static readonly Color32 Fallback = new Color32(30, 60, 90, 255);
        private static readonly Color32 Override = new Color32(220, 40, 70, 255);

        private EditorWindow _window;
        private VisualElement _root, _readsComponent, _readsFallback, _overrideHost, _readsUnderOverride;

        // Build tokens-on-an-ancestor (sheet A on the window root) and consumers in ANOTHER sheet (sheet B on the chain root)
        [UnitySetUp]
        public IEnumerator OpenPanelWithTheChain()
        {
            var tokens = AssetDatabase.LoadAssetAtPath<StyleSheet>(FixtureFolder + "ChainTokens.uss");
            var consumers = AssetDatabase.LoadAssetAtPath<StyleSheet>(FixtureFolder + "ChainConsumers.uss");
            Assert.IsNotNull(tokens, "fixture sheet ChainTokens.uss missing -- SETUP is wrong, not the engine");
            Assert.IsNotNull(consumers, "fixture sheet ChainConsumers.uss missing -- SETUP is wrong, not the engine");

            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.titleContent = new GUIContent("UssVariableChainTests");
            _window.ShowUtility();
            _window.position = new Rect(120, 120, 240, 160);

            _window.rootVisualElement.styleSheets.Add(tokens);
            _root = Element("chain-root");
            _root.styleSheets.Add(consumers);
            _readsComponent = Element("chain-reads-component");
            _readsFallback = Element("chain-reads-fallback");
            _overrideHost = Element("chain-override");
            _readsUnderOverride = Element("chain-reads-component");
            _overrideHost.Add(_readsUnderOverride);
            _root.Add(_readsComponent);
            _root.Add(_readsFallback);
            _root.Add(_overrideHost);
            _window.rootVisualElement.Add(_root);

            // wait on the panel's own signal, not on seconds: the root's custom style resolved + a consumer painted
            bool resolved = false;
            _root.RegisterCallback<CustomStyleResolvedEvent>(_ => resolved = true);
            for (int frame = 0; frame < MaxFramesToResolve && !(resolved && _readsComponent.resolvedStyle.backgroundColor.a > 0f); frame++)
            {
                _window.Repaint();
                yield return null;
            }
            Assert.IsTrue(resolved, $"the chain root's custom style never resolved within {MaxFramesToResolve} editor frames -- SETUP (panel) is wrong");
        }

        // Close the window for real (Close, not DestroyImmediate) and prove none is left (40-testing 4.2.3)
        [TearDown]
        public void CloseThePanel()
        {
            if (_window != null) _window.Close();
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
                Assert.AreNotEqual("UssVariableChainTests", w.titleContent.text, "the test window was left open");
        }

        [Test]
        public void ATokenPointingAtAToken_ResolvesTwoLevelsDeep()
        {
            Assert.AreEqual(Primitive, Custom(_root, "--chain-role"), "--chain-role: var(--chain-primitive)");
        }

        [Test]
        public void ATokenPointingAtAToken_ResolvesThreeLevelsDeep()
        {
            Assert.AreEqual(Primitive, Custom(_root, "--chain-component"), "--chain-component: var(--chain-role) -> var(--chain-primitive)");
        }

        [Test]
        public void AFallback_IsUsedOnlyWhenTheNameIsUndefined()
        {
            Assert.AreEqual(Fallback, Custom(_root, "--chain-fallback-missing"), "var(--never-defined, fallback) takes the fallback");
            Assert.AreEqual(Primitive, Custom(_root, "--chain-fallback-defined"), "var(--defined, fallback) takes the defined value");
            Assert.AreEqual(Fallback, (Color32)_readsFallback.resolvedStyle.backgroundColor, "a property reading the fallback token paints the fallback");
        }

        [Test]
        public void ASheetReadsTheChainAnAncestorDefinesInAnotherSheet()
        {
            Assert.AreEqual(Primitive, (Color32)_readsComponent.resolvedStyle.backgroundColor,
                "ChainConsumers.uss: background-color: var(--chain-component), the chain defined by ChainTokens.uss on the ancestor .chain-root");
        }

        [Test]
        public void APrimitiveOverriddenOnADescendant_ReachesTheTokensBuiltOnItBelowIt()
        {
            Assert.AreEqual(Override, Custom(_overrideHost, "--chain-primitive"), "precondition: the descendant declares its own primitive");
            Assert.AreEqual(Override, (Color32)_readsUnderOverride.resolvedStyle.backgroundColor,
                "var(--chain-component) under .chain-override must follow the overridden primitive (USS substitutes at the using element)");
            Assert.AreEqual(Primitive, (Color32)_readsComponent.resolvedStyle.backgroundColor, "the sibling outside the override keeps the root's primitive");
        }

        [Test]
        public void CustomStyle_HoldsOnlyTheTokensDeclaredOnThatElement()
        {
            // why it matters: C# reading a token (CardTokenColours) must ask the element that DECLARES it (the themed root), never a child
            Assert.IsFalse(_readsComponent.customStyle.TryGetValue(new CustomStyleProperty<Color>("--chain-component"), out _),
                "a child's customStyle does not expose an inherited token");
            Assert.AreEqual(Primitive, Custom(_root, "--chain-component"), "the declaring element does");
        }

        // An element carrying one USS class
        private static VisualElement Element(string ussClass)
        {
            var e = new VisualElement();
            e.AddToClassList(ussClass);
            e.style.height = 8;   // something to paint; a size, not a look
            return e;
        }

        // A colour token as the element's own custom style resolves it (fails if it does not resolve at all)
        private static Color32 Custom(VisualElement element, string token)
        {
            Assert.IsTrue(element.customStyle.TryGetValue(new CustomStyleProperty<Color>(token), out Color value), $"{token} did not resolve on {string.Join(".", element.GetClasses())}");
            return value;
        }
    }
}
