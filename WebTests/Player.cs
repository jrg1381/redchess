using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using RedChess.ChessCommon.Enumerations;

namespace RedChess.WebTests
{
    class Player
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public PieceColor Color { get; private set; }
        public IWebDriver Driver { get; private set; }

        public Player(PieceColor color, string username, string password, IWebDriver driver)
        {
            Color = color;
            Driver = driver;
            Username = username;
            Password = password;
        }

        public void Logout()
        {
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        public void AssertGameOver()
        {
            var turnIndicator = Driver.FindElement(By.Id("turnindicator"));
            var messages = Driver.FindElement(By.Id("messages"));

            bool success = WaitHelper.WaitFor(TimeSpan.FromSeconds(5), () => turnIndicator.Text.Contains("GAME OVER"));

            Assert.True(success, "Waited for GAME OVER notification in messages but it never came - " + Color);
            Assert.AreEqual("Checkmate", messages.Text, "Expected checkmate message - " + Color);
        }

        public void Login(string baseUrl)
        {
            Driver.Navigate().GoToUrl(baseUrl);
            var loginField = Driver.FindElement(By.Id("loginName"));
            var passwordField = Driver.FindElement(By.Id("password"));

            loginField.Clear();
            loginField.Click();
            loginField.SendKeys(Username);

            passwordField.Clear();
            passwordField.Click();
            passwordField.SendKeys(Password);

            Thread.Sleep(2000);

            Driver.FindElement(By.CssSelector("input[type=\"submit\"]")).Click();
        }

        public void WaitForTurn()
        {
            var turnIndicator = Driver.FindElement(By.Id("turnindicator"));
            var expectedText = Color == PieceColor.Black ? "Black" : "White";

            bool success = WaitHelper.WaitFor(TimeSpan.FromSeconds(10), () => turnIndicator.Text.Contains(expectedText));
            Assert.True(success, "Waited for my turn but it never arrived");
        }

        public void PerformDragAndDrop(Location start, Location end)
        {
            WaitForTurn();

            var builder = new Actions(Driver);
            var board = Driver.FindElement(By.CssSelector("div.board-b72b1"));
            var startPosition = new Offset(start, Color);
            var endPosition = new Offset(end, Color);

            IAction dragAndDrop = builder.MoveToElement(board, startPosition.X, startPosition.Y).
                ClickAndHold().
                MoveByOffset(endPosition.X - startPosition.X, endPosition.Y - startPosition.Y).
                Release().
                Build();

            dragAndDrop.Perform();
        }
    }
}