using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Webao.Test.Dto;

namespace Webao.Test
{
    [TestClass]
    public class AccessObjectTest
    {
        static readonly WebaoArtist artistWebao = (WebaoArtist) WebaoBuilder.Build(typeof(WebaoArtist), new HttpRequest());
        static readonly WebaoTrack trackWebao = (WebaoTrack) WebaoBuilder.Build(typeof(WebaoTrack), new HttpRequest());


        [TestMethod]
        public void TestWebaoArtistGetInfo()
        {
            Artist artist = artistWebao.GetInfo("muse");
            Assert.AreEqual("Muse", artist.Name);
            Assert.AreEqual("fd857293-5ab8-40de-b29e-55a69d4e4d0f", artist.Mbid);
            Assert.AreEqual("https://www.last.fm/music/Muse", artist.Url);
            Assert.AreNotEqual(0, artist.Stats.Listeners);
            Assert.AreNotEqual(0, artist.Stats.Playcount);
        }

        [TestMethod]
        public void TestWebaoArtistSearch()
        {
            List<Artist> artists = artistWebao.Search("black");
            Assert.AreEqual("Black Sabbath", artists[1].Name);
            Assert.AreEqual("Black Eyed Peas", artists[2].Name);
        }

        [TestMethod]
        public void TestWebaoTrackGeoGetTopTracks()
        {
            List<Track> tracks = trackWebao.GeoGetTopTracks("australia");
            Assert.AreEqual("The Less I Know the Better", tracks[0].Name);
            Assert.AreEqual("Mr. Brightside", tracks[1].Name);
            Assert.AreEqual("The Killers", tracks[1].Artist.Name);
        }
    }
}
