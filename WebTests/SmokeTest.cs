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
        private const int c_Port = 60898;

        private StringBuilder m_VerificationErrors;
        private string m_BaseUrl;
        private IisExpressStarter m_IisStarter;
        private Player m_ClivePlayer, m_JamesPlayer;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            m_IisStarter = new IisExpressStarter(c_Port);
            m_IisStarter.Start(); 
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            m_IisStarter.Stop();
        }

        [SetUp]
        public void SetupTest()
        {
            IWebDriver driverPlayerOne = new ChromeDriver();
            IWebDriver driverPlayerTwo = new ChromeDriver();
            driverPlayerOne.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(8));
            driverPlayerTwo.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(8));
            m_BaseUrl = (new UriBuilder("http", "localhost", c_Port)).ToString();

            WarmTheWebServer(m_BaseUrl);

            m_ClivePlayer = new Player(PieceColor.Black, "player1", "password1", driverPlayerTwo);
            m_JamesPlayer = new Player(PieceColor.White, "player2", "password2", driverPlayerOne);

            m_VerificationErrors = new StringBuilder();
        }
        
        [TearDown]
        public void TeardownTest()
        {
            try
            {
                m_ClivePlayer.Driver.Quit();
                m_JamesPlayer.Driver.Quit();
                m_IisStarter.Stop();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }

            Assert.AreEqual("", m_VerificationErrors.ToString());
        }
        
        [Test]
        public void ScholarsMate()
        {
            m_JamesPlayer.Login(m_BaseUrl);
            m_ClivePlayer.Login(m_BaseUrl);

            m_JamesPlayer.Driver.FindElement(By.LinkText("New game")).Click();
            m_JamesPlayer.Driver.FindElement(By.CssSelector("button[id=\"submitbutton\"]")).Click();

            m_JamesPlayer.WaitForTurn();

            var gameId = m_JamesPlayer.Driver.Url.Split(new[] { '/' }).Last();

            var gameDetailsUri = new Uri(m_BaseUrl + "/Board/Details/" + gameId);
            m_ClivePlayer.Driver.Navigate().GoToUrl(gameDetailsUri);

            m_JamesPlayer.PerformDragAndDrop(Location.E2, Location.E4); // e4
            m_ClivePlayer.PerformDragAndDrop(Location.E7, Location.E5); // e5

            m_JamesPlayer.PerformDragAndDrop(Location.D1, Location.H5); // Qh5
            m_ClivePlayer.PerformDragAndDrop(Location.B8, Location.C6); // Nc6

            m_JamesPlayer.PerformDragAndDrop(Location.F1, Location.C4); // Bc4
            m_ClivePlayer.PerformDragAndDrop(Location.G8, Location.F6); // Nf6

            m_JamesPlayer.OfferDraw();
            m_ClivePlayer.RejectDraw();

            m_JamesPlayer.PerformDragAndDrop(Location.H5, Location.F7); // Qxf7#

            m_JamesPlayer.AssertGameOver();
            m_ClivePlayer.AssertGameOver();
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
