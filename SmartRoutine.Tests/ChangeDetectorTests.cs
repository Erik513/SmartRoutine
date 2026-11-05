using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Logic.Services;
using System;
using System.Collections.Generic;

namespace SmartRoutine.Tests
{
    [TestClass]
    public class ChangeDetectorTests
    {
        private class Item
        {
            public string Name { get; set; }
            public int Count { get; set; }
            public DateTime? When { get; set; }
            public List<Item> Children { get; set; }
        }

        [TestMethod]
        public void SameValues_AreEqual()
        {
            var a = new Item { Name = "x", Count = 1, Children = new List<Item> { new Item { Name = "k" } } };
            var b = new Item { Name = "x", Count = 1, Children = new List<Item> { new Item { Name = "k" } } };

            Assert.IsTrue(ChangeDetector.AreEqual(a, b));
            Assert.IsFalse(ChangeDetector.HasChanges(a, b));
        }

        [TestMethod]
        public void DifferentProperty_IsChange()
        {
            Assert.IsTrue(ChangeDetector.HasChanges(new Item { Name = "x" }, new Item { Name = "y" }));
            Assert.IsTrue(ChangeDetector.HasChanges(new Item { Count = 1 }, new Item { Count = 2 }));
        }

        [TestMethod]
        public void IgnoredProperty_IsNotCompared()
        {
            var a = new Item { Name = "x", When = new DateTime(2020, 1, 1) };
            var b = new Item { Name = "x", When = new DateTime(2024, 1, 1) };

            Assert.IsTrue(ChangeDetector.HasChanges(a, b));
            Assert.IsFalse(ChangeDetector.HasChanges(a, b, "When"));
        }

        [TestMethod]
        public void NullAndNonNull_AreDifferent_TwoNullsAreEqual()
        {
            Assert.IsTrue(ChangeDetector.HasChanges(new Item(), null));
            Assert.IsTrue(ChangeDetector.HasChanges<Item>(null, new Item()));
            Assert.IsFalse(ChangeDetector.HasChanges<Item>(null, null));
        }

        [TestMethod]
        public void NullVersusEmptyString_IsChange()
        {
            Assert.IsTrue(ChangeDetector.HasChanges(new Item { Name = null }, new Item { Name = "" }));
        }

        [TestMethod]
        public void ListsDifferInLengthOrContent_AreChanges()
        {
            var one = new Item { Children = new List<Item> { new Item { Name = "a" } } };
            var two = new Item { Children = new List<Item> { new Item { Name = "a" }, new Item { Name = "b" } } };
            var other = new Item { Children = new List<Item> { new Item { Name = "z" } } };

            Assert.IsTrue(ChangeDetector.HasChanges(one, two));
            Assert.IsTrue(ChangeDetector.HasChanges(one, other));
        }

        [TestMethod]
        public void ListOrder_Matters()
        {
            var ab = new Item { Children = new List<Item> { new Item { Name = "a" }, new Item { Name = "b" } } };
            var ba = new Item { Children = new List<Item> { new Item { Name = "b" }, new Item { Name = "a" } } };

            Assert.IsTrue(ChangeDetector.HasChanges(ab, ba));
        }

        [TestMethod]
        public void IgnoredPropertyIsAlsoIgnoredInsideLists()
        {
            var a = new Item { Children = new List<Item> { new Item { Name = "a", Count = 1 } } };
            var b = new Item { Children = new List<Item> { new Item { Name = "a", Count = 2 } } };

            Assert.IsTrue(ChangeDetector.HasChanges(a, b));
            Assert.IsFalse(ChangeDetector.HasChanges(a, b, "Count"));
        }
    }
}
