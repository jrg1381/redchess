using System;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebTests
{
    [TestFixture]
    public class SmokeTest
    {
        private const int Port = 60898;

        private StringBuilder m_verificationErrors;
        private string m_baseUrl;
        private StartIisExpress m_iisStarter;
        private Player m_clivePlayer, m_jamesPlayer;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            m_iisStarter = new StartIisExpress(Port);
            m_iisStarter.Start(); 
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            m_iisStarter.Stop();
        }

        [SetUp]
        public void SetupTest()
        {
            IWebDriver driverPlayerOne = new ChromeDriver();
            IWebDriver driverPlayerTwo = new ChromeDriver();
            driverPlayerOne.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(8));
            driverPlayerTwo.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(8));
            m_baseUrl = (new UriBuilder("http", "localhost", Port)).ToString();

            WarmTheWebServer(m_baseUrl);

            m_clivePlayer = new Player(PieceColor.Black, "clivetong", "grandmaster", driverPlayerTwo);
            m_jamesPlayer = new Player(PieceColor.White, "james", "doomlord", driverPlayerOne);

            m_verificationErrors = new StringBuilder();
        }
        
        [TearDown]
        public void TeardownTest()
        {
            try
            {
                m_clivePlayer.Driver.Quit();
                m_jamesPlayer.Driver.Quit();
                m_iisStarter.Stop();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }

            Assert.AreEqual("", m_verificationErrors.ToString());
        }
        
        [Test]
        public void ScholarsMate()
        {
            m_jamesPlayer.Login(m_baseUrl);
            m_clivePlayer.Login(m_baseUrl);

            m_jamesPlayer.Driver.FindElement(By.LinkText("New game")).Click();
            m_jamesPlayer.Driver.FindElement(By.CssSelector("button[id=\"submitbutton\"]")).Click();

            m_jamesPlayer.WaitForTurn();

            var gameId = m_jamesPlayer.Driver.Url.Split(new[] { '/' }).Last();

            var gameDetailsUri = new Uri(m_baseUrl + "/Board/Details/" + gameId);
            m_clivePlayer.Driver.Navigate().GoToUrl(gameDetailsUri);

            m_jamesPlayer.PerformDragAndDrop(Location.E2, Location.E4); // e4
            m_clivePlayer.PerformDragAndDrop(Location.E7, Location.E5); // e5

            m_jamesPlayer.PerformDragAndDrop(Location.D1, Location.H5); // Qh5
            m_clivePlayer.PerformDragAndDrop(Location.B8, Location.C6); // Nc6

            m_jamesPlayer.PerformDragAndDrop(Location.F1, Location.C4); // Bc4
            m_clivePlayer.PerformDragAndDrop(Location.G8, Location.F6); // Nf6

            m_jamesPlayer.PerformDragAndDrop(Location.H5, Location.F7); // Qxf7#

            m_jamesPlayer.AssertGameOver();
            m_clivePlayer.AssertGameOver();
        }

        private static void WarmTheWebServer(string uri)
        {
            using (var client = new WebClient())
            {
                string frontPage = client.DownloadString(uri);
                Console.WriteLine(frontPage.Substring(0,128) + "...");
            }
        }
    }
}
