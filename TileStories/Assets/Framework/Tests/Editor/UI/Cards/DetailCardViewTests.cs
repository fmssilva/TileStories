using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for DetailCardView: selection -> show+label, re-select update,
    // X / external clear -> hide. No UIDocument needed because the view keeps
    // selection state independent of its (null-guarded) UI label. (spec §14)
    public class DetailCardViewTests
    {
        private GameObject _go;
        private DetailCardView _view;
        private WallConfigData _config;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("DetailCardTest");
            _view = _go.AddComponent<DetailCardView>();
            _config = new WallConfigData
            {
                pois = new List<POIData>
                {
                    new POIData { id = "poi_1", name = "Cathedral", category = "religious" },
                    new POIData { id = "poi_2", name = "Town Hall", category = "civic" },
                }
            };
            _view.Initialize(_config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void ShowOnSelect_SetsVisibleAndLabel()
        {
            SelectionEventBus.RaiseMarkerSelected("poi_1");
            Assert.IsTrue(_view.IsVisibleState());
            Assert.AreEqual("Cathedral", _view.GetLabelText());
        }

        [Test]
        public void Reselect_UpdatesLabel()
        {
            SelectionEventBus.RaiseMarkerSelected("poi_1");
            SelectionEventBus.RaiseMarkerSelected("poi_2");
            Assert.IsTrue(_view.IsVisibleState());
            Assert.AreEqual("Town Hall", _view.GetLabelText());
        }

        [Test]
        public void Close_HidesViaClearEvent()
        {
            SelectionEventBus.RaiseMarkerSelected("poi_1");
            Assert.IsTrue(_view.IsVisibleState());

            // X button raises the shared clear, which the card reacts to by hiding.
            _view.Close();

            Assert.IsFalse(_view.IsVisibleState());
        }

        [Test]
        public void ExternalClear_Hides()
        {
            SelectionEventBus.RaiseMarkerSelected("poi_1");
            SelectionEventBus.RaiseSelectionCleared();
            Assert.IsFalse(_view.IsVisibleState());
        }

        [Test]
        public void Select_UnknownId_DoesNotShow()
        {
            SelectionEventBus.RaiseMarkerSelected("does_not_exist");
            Assert.IsFalse(_view.IsVisibleState());
            Assert.AreEqual(string.Empty, _view.GetLabelText());
        }
    }
}
