using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace CreateUserFeatureUnitTest
{
    public class Tests
    {
        private ChromeDriver driver;

        [SetUp]
        public void Setup()
        {
            DeleteUsers();
            SetupWebDriver();
            driver.Navigate().GoToUrl("http://54.251.184.170:3030/");
        }

        [Test]
        public void Test01_ValidFieldTest()
        {
            GetElement("//button[text()='Sign Up']").Click();

            Assert.AreEqual(GetElement("//div[@id='inputNameMessage']").Text, "Valid Full name is required.");
            Assert.AreEqual(GetElement("//div[@id='inputEmailMessage']").Text, "Valid email is required.");
            Assert.AreEqual(GetElement("//div[@id='inputPasswordMessage']").Text, "Valid Password is required.");
            Assert.AreEqual(GetElement("//div[@id='inputConfirmPasswordMessage']").Text, "Valid Confirm Password is required.");

            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test02_FullNameUniqueTest()
        {
            SignUpUser("aa bb", "aabb@gmail.com", "123123", "123123");
            GetElement("//button[text()='Sign Up']").Click();
            Thread.Sleep(2000);

            GetElement("//input[@value='New User']").Click();
            Thread.Sleep(2000);

            SignUpUser("aa bb", "aabb@gmail.com", "123123", "123123");
            GetElement("//button[text()='Sign Up']").Click();
            Thread.Sleep(2000);

            Assert.AreEqual(GetElement("//div[@id='inputNameMessage']").Text, "Full name must be unique.");
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test03_EmailUniqueTest()
        {
            SignUpUser("aa bb", "aabb@gmail.com", "123123", "123123");
            GetElement("//button[text()='Sign Up']").Click();
            Thread.Sleep(2000);

            GetElement("//input[@value='New User']").Click();
            Thread.Sleep(2000);

            SignUpUser("aa bb", "aabb@gmail.com", "123123", "123123");
            GetElement("//button[text()='Sign Up']").Click();
            Thread.Sleep(2000);

            Assert.AreEqual(GetElement("//div[@id='inputEmailMessage']").Text, "Email must be unique.");
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test04_PasswordCharacterLimitTest()
        {
            SignUpUser("aa bb", "aabb@gmail.com", "123", "123");
            GetElement("//button[text()='Sign Up']").Click();

            Assert.AreEqual(GetElement("//div[@id='inputPasswordMessage']").Text, "Valid Password is required.");
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test05_PasswordMatchTest()
        {
            SignUpUser("aa bb", "aabb@gmail.com", "1234567", "1234568");
            GetElement("//button[text()='Sign Up']").Click();

            Assert.AreEqual(GetElement("//div[@id='inputConfirmPasswordMessage']").Text, "Password should match.");
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test06_SignUpTest()
        {
            bool pass = false;

            SignUpUser("aa bb", "aabb@gmail.com", "123123", "123123");
            GetElement("//button[text()='Sign Up']").Click();

            JObject jObject = GetUsers();
            JToken user = jObject.First.First.First;

            while (user != null)
            {
                string name = user.Value<string>("name");
                string email = user.Value<string>("email");
                string pw = user.Value<string>("password");

                if (name.Equals("aa bb") && email.Equals("aabb@gmail.com") & pw.Equals("123123"))
                {
                    pass = true;
                    break;
                }

                user = user.Next;
            }

            Assert.IsTrue(pass);
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test07_NavigateToUsersTest()
        {
            SignUpUser("aa bb", "aabb@gmail.com", "123123", "123123");
            GetElement("//button[text()='Sign Up']").Click();
            Thread.Sleep(2000);

            GetElement("//input[@value='New User']").Click();
            Thread.Sleep(2000);

            GetElement("//button[text()='All Users']").Click();

            Assert.AreEqual(driver.Url, "http://54.251.184.170:3030/users.html");
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test08_NavigateToNewUserTest()
        {
            GetElement("//button[text()='All Users']").Click();
            Thread.Sleep(2000);

            GetElement("//input[@value='New User']").Click();
            Thread.Sleep(2000);

            Assert.AreEqual(driver.Url, "http://54.251.184.170:3030/?");
            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test09_AllUsersTest()
        {
            for (int i = 1; i <= 3; i++)
            {
                SignUpUser("a" + i);
                GetElement("//button[text()='Sign Up']").Click();
                Thread.Sleep(2000);

                GetElement("//input[@value='New User']").Click();
                Thread.Sleep(2000);
            }

            GetElement("//button[text()='All Users']").Click();
            Thread.Sleep(2000);

            for (int j = 1; j <= 3; j++)
            {
                Assert.AreEqual(GetElement("//table[@id='usersTable']/thead/tr[" + (j + 1) + "]/td[1]").Text, "a" + j);
                Assert.AreEqual(GetElement("//table[@id='usersTable']/thead/tr[" + (j + 1) + "]/td[2]").Text, "a" + j + "@gmail.com");
                Assert.AreEqual(GetElement("//table[@id='usersTable']/thead/tr[" + (j + 1) + "]/td[3]").Text, "123123");
            }

            driver.Close();
            driver.Dispose();
        }

        [Test]
        public void Test10_DeleteUsersTest()
        {
            for (int i = 1; i <= 3; i++)
            {
                SignUpUser("a" + i);
                GetElement("//button[text()='Sign Up']").Click();
                Thread.Sleep(2000);

                GetElement("//input[@value='New User']").Click();
                Thread.Sleep(2000);
            }

            DeleteUsers();

            GetElement("//button[text()='All Users']").Click();
            Thread.Sleep(2000);

            Assert.AreEqual(driver.FindElements(By.XPath("//table[@id='usersTable']/thead/tr")).Count, 1); // used 1 because the table header is counted

            driver.Close();
            driver.Dispose();
        }

        /// <summary>
        /// Setup the Chrome Web Driver
        /// </summary>
        public void SetupWebDriver()
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            driver = new ChromeDriver(driverService);
            driver.Manage().Timeouts().PageLoad = new TimeSpan(0, 2, 0);
            driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 30);
        }

        /// <summary>
        /// Sign up a user from the credentials provided
        /// </summary>
        /// <param name="fname">The full name</param>
        private void SignUpUser(string fname)
        {
            SignUpUser(fname, fname + "@gmail.com", "123123", "123123");
        }

        /// <summary>
        /// Sign up a user from the credentials provided
        /// </summary>
        /// <param name="fname">The full name</param>
        /// <param name="email">The email address</param>
        /// <param name="pw">The password</param>
        /// <param name="cpw">The confirm password</param>
        private void SignUpUser(string fname, string email, string pw, string cpw)
        {
            GetElement("//input[@id='inputName']").SendKeys(fname);
            GetElement("//input[@id='inputEmail']").SendKeys(email);
            GetElement("//input[@id='inputPassword']").SendKeys(pw);
            GetElement("//input[@id='inputConfirmPassword']").SendKeys(cpw);
        }

        /// <summary>
        /// Gets all the users registered
        /// </summary>
        /// <returns>Returns all the users as a JSON object</returns>
        private JObject GetUsers()
        {
            string result = "";
            string urlAddress = "http://54.251.184.170:3030/api/users";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.ContentType = "application/json";
            request.Method = "GET";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                result = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }

            result = "{\"users\":" + result + "}";

            return JObject.Parse(result);
        }

        /// <summary>
        /// Deletes all the users registered
        /// </summary>
        private void DeleteUsers()
        {
            string urlAddress = "http://54.251.184.170:3030/api/users";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.ContentType = "application/json";
            request.Method = "DELETE";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// Gets the element to manipulate based on xpath.
        /// </summary>
        /// <param name="xpath">The xpath of an element</param>
        /// <returns>An element to manipulate</returns>
        private IWebElement GetElement(string xpath)
        {
            IWebElement we = null;

            try
            {
                we = driver.FindElement(By.XPath(xpath));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return we;
        }
    }
}
 