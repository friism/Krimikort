using EB.Crime.Map.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EB.Crime.MapTests
{
	[TestClass]
	public class ViewUtilTests
	{
		[TestMethod]
		public void Base26TestOne()
		{
			Assert.AreEqual(ViewUtil.FromBase26(ViewUtil.ToBase26(1)), 1);
		}

		[TestMethod]
		public void Base26Test26()
		{
			Assert.AreEqual(ViewUtil.FromBase26(ViewUtil.ToBase26(26)), 26);
		}

		[TestMethod]
		public void Base26Test25()
		{
			Assert.AreEqual(ViewUtil.FromBase26(ViewUtil.ToBase26(25)), 25);
		}

		[TestMethod]
		public void Base26Test27()
		{
			Assert.AreEqual(ViewUtil.FromBase26(ViewUtil.ToBase26(27)), 27);
		}

		[TestMethod]
		public void Base26Test27text()
		{
			Assert.AreEqual(ViewUtil.ToBase26(27), "aa");
		}

		[TestMethod]
		public void FromBase26Test27text()
		{
			Assert.AreEqual(ViewUtil.FromBase26("aa"), 27);
		}

		[TestMethod]
		public void FromBase26Test1text()
		{
			Assert.AreEqual(ViewUtil.FromBase26("a"), 1);
		}

		[TestMethod]
		public void FromBase26Test100text()
		{
			Assert.AreEqual(ViewUtil.FromBase26("cv"), 100);
		}

		[TestMethod]
		public void Base26Test1text()
		{
			Assert.AreEqual(ViewUtil.ToBase26(1), "a");
		}

		[TestMethod]
		public void Base26Test100text()
		{
			Assert.AreEqual(ViewUtil.ToBase26(100), "cv");
		}

		[TestMethod]
		public void Base26Test1000000()
		{
			Assert.AreEqual(ViewUtil.FromBase26(ViewUtil.ToBase26(1000000)), 1000000);
		}

		[TestMethod]
		public void Base26Test1000000tob26()
		{
			Assert.AreEqual(ViewUtil.ToBase26(1000000), "bdwgn");
		}

		[TestMethod]
		public void Base26Test199030tob26()
		{
			Assert.AreEqual(ViewUtil.ToBase26(199030), "khjz");
		}

		[TestMethod]
		public void Base26Test199030()
		{
			Assert.AreEqual(ViewUtil.FromBase26(ViewUtil.ToBase26(199030)), 199030);
		}
	}
}
