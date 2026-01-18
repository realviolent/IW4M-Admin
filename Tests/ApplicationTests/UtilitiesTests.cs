using System.Collections.Generic;
using NUnit.Framework;
using SharedLibraryCore;
using static Data.Models.Client.EFClient;

namespace ApplicationTests
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void TestCapClientNameLengthReachesMax()
        {
            string originalName = "SomeVeryLongName";
            string expectedName = "SomeVeryLong...";
            int maxLength = originalName.Length - 1;

            string cappedName = originalName.CapClientName(maxLength);

            Assert.AreEqual(expectedName, cappedName);
        }

        [Test]
        public void TestCapClientNameRetainsOriginal()
        {
            string originalName = "Short";
            int maxLength = originalName.Length;

            string cappedName = originalName.CapClientName(maxLength);

            Assert.AreEqual(originalName, cappedName);
        }

        [Test]
        public void TestConvertLevelToColorReturnsCorrectColor()
        {
            var existingColors = new Dictionary<Permission, string>(Utilities.PermissionLevelColors);

            try
            {
                Utilities.PermissionLevelColors.Clear();
                Utilities.PermissionLevelColors.Add(Permission.Banned, "Black");
                Utilities.PermissionLevelColors.Add(Permission.Flagged, "White");

                Assert.AreEqual("(Color::Black)Banned", Utilities.ConvertLevelToColor(Permission.Banned, null));
                Assert.AreEqual("(Color::White)Flagged", Utilities.ConvertLevelToColor(Permission.Flagged, null));

                // Check fallback
                Assert.AreEqual("(Color::Pink)Owner", Utilities.ConvertLevelToColor(Permission.Owner, null));
            }

            finally
            {
                Utilities.PermissionLevelColors.Clear();
                foreach (var (key, value) in existingColors)
                {
                    Utilities.PermissionLevelColors.Add(key, value);
                }
            }
        }
    }
}
