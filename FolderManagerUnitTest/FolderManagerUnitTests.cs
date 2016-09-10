using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderManager;
using System.Linq;

namespace FolderManagerUnitTest
{
    [TestClass]
    public class FolderManagerUnitTests
    {
        [TestMethod]
        public void VerifyIfSortedSubDirsAreReturedForAPath()
        {
            MainWindow win = new MainWindow();
            var path = @"K:\Videos\PS\CLR Fundamentals\managed-code";
            var sortedPath = win.SortedSubDirectories(path);

            for (int i = 0; i < sortedPath.Length; i++)
            {
                Assert.AreEqual(i.ToString(), sortedPath[i].Split('\\').Last());
            }
        }
    }
}
