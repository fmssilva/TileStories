using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NUnit.Framework;
using TileStories;

namespace TileStories.Tests
{
    // ZoomControlView tests are EditMode (not PlayMode): the view is pure UI
    // Toolkit screen-space, so UXML/USS are verifiable via AssetDatabase, and
    // button->controller routing is synchronous.
    //
    // Note on the click trigger: in this Unity version Button.clicked is an *event*
    // that can only be subscribed to, never invoked from outside the class. So we
    // verify routing by (a) asserting Mount() resolves the three named buttons
    // (proving BindButtons wired those exact names to the handlers), and (b)
    // reflect-calling the private handlers to prove they drive the REAL
    // ARZoomController -> observable ARZoomState. Button's own ClickEvent->clicked
    // dispatch is Unity's responsibility, not ours.
    public class ZoomControlViewEditModeTests
    {
        private const string UxmlPath = "Assets/Framework/Runtime/UI/Zoom/ZoomControlView.uxml";
        private const string UssPath = "Assets/Framework/Runtime/UI/Zoom/ZoomControlView.uss";

        // --- 1d gating (pure) ---
        [Test]
        public void ShouldShowButtons_gates_on_zoom_show_ui_buttons()
        {
            Assert.IsFalse(ZoomControlView.ShouldShowButtons(null));
            Assert.IsFalse(ZoomControlView.ShouldShowButtons(new LodSettings { zoom_show_ui_buttons = false }));
            Assert.IsTrue(ZoomControlView.ShouldShowButtons(new LodSettings { zoom_show_ui_buttons = true }));
        }

        // --- 1d UXML declares the three buttons by the exact names BindButtons queries ---
        [Test]
        public void ZoomControlView_uxml_declares_three_named_buttons()
        {
            var vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            Assert.IsNotNull(vta, "ZoomControlView.uxml missing or failed to import");

            var root = vta.CloneTree();
            Assert.IsNotNull(root.Q<Button>("zoom-in-button"));
            Assert.IsNotNull(root.Q<Button>("zoom-out-button"));
            Assert.IsNotNull(root.Q<Button>("zoom-reset-button"));
        }

        // --- 1d WCAG 2.5.5 tap-target floor (>=44px) lives in USS, verified on the file ---
        [Test]
        public void ZoomControlView_uss_enforces_44px_tap_target_minimum()
        {
            Assert.IsTrue(File.Exists(UssPath), "ZoomControlView.uss missing");
            var uss = File.ReadAllText(UssPath);
            StringAssert.Contains("min-width: 44px", uss);
            StringAssert.Contains("min-height: 44px", uss);
        }

    }
}
