using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace TestCommon
{
    public enum WebBrowserTypeEnum { InternetExplorer, Chrome, FireFox };

    public class WebItemClass
    {
        public IWebElement WebElement;
        
        public WebItemClass(IWebElement WebElementParam)
        {
            WebElement = WebElementParam;
        }

        public String GetTagName()
        {
            return WebElement.TagName;
        }

        public String GetAttribute(String AttributeParam)
        {
            return WebElement.GetAttribute(AttributeParam);
        }

        public Boolean IsSelected()
        {
            return WebElement.Selected;
        }

        public String Text()
        {
            return WebElement.Text;
        }

        public WebItemClass GetParent()
        {
            return new WebItemClass(WebElement.FindElement(By.XPath("..")));
        }

        public List<WebItemClass> GetChildren()
        {
            List<WebItemClass> DataReturn = new List<WebItemClass>();
            ReadOnlyCollection<IWebElement> SrchCollection = WebElement.FindElements(By.XPath("/*"));

            foreach (IWebElement ElementEnum in SrchCollection)
            {
                WebItemClass WebItemEnum = new WebItemClass(ElementEnum);
                DataReturn.Add(WebItemEnum);
            }

            return DataReturn;
        }

        public List<WebItemClass> Find(String XPath)
        {
            List<WebItemClass> DataReturn = new List<WebItemClass>();
            ReadOnlyCollection<IWebElement> SrchCollection = WebElement.FindElements(By.XPath(XPath));

            foreach (IWebElement ElementEnum in SrchCollection)
            {
                WebItemClass WebItemEnum = new WebItemClass(ElementEnum);
                DataReturn.Add(WebItemEnum);
            }

            return DataReturn;
        }

        public void Click()
        {
            WebElement.Click();
        }
    }

    public class WebTestClass
    {
        WebBrowserTypeEnum WebBrowserType = WebBrowserTypeEnum.InternetExplorer;
        IWebDriver WebDriver;
        String UrlBase;

        public WebTestClass(WebBrowserTypeEnum WebBrowserTypeParam)
        {
            WebBrowserType = WebBrowserTypeParam;
        }

        public void Start()
        {
            if (WebBrowserType == WebBrowserTypeEnum.InternetExplorer)
                WebDriver = new InternetExplorerDriver();
            else if (WebBrowserType == WebBrowserTypeEnum.FireFox)
                WebDriver = new FirefoxDriver();
            else if (WebBrowserType == WebBrowserTypeEnum.Chrome)
            {
                ChromeOptions BrowserOptions = new ChromeOptions();

                BrowserOptions.AddArguments("--disable-extensions");

                WebDriver = new ChromeDriver(BrowserOptions);
            }
        }

        public void Stop()
        {
            WebDriver.Close();
            WebDriver.Quit();
        }

        public void SetUrlBase(String UrlBaseParam)
        {
            UrlBase = UrlBaseParam;
        }

        public void GoToUrl(String UrlParam = null)
        {
            String FullUrl = null;

            if (UrlBase == null)
            {
                FullUrl = UrlParam;
            }
            else
            {
                if (UrlBase.Contains("http") || UrlBase.Contains("file"))
                    FullUrl = UrlBase + UrlParam;
                else
                    FullUrl = "http://" + UrlBase + UrlParam;
            }

            WebDriver.Navigate().GoToUrl(FullUrl);
        }

        public void GoToUrlFull(String UrlParam)
        {
            WebDriver.Navigate().GoToUrl(UrlParam);
        }

        public void EnterText(
            String XPath,
            String TextParam)
        {
            WaitForElementPresent(XPath);

            DateTime StopTime = DateTime.Now.AddSeconds(15);

            while (DateTime.Now < StopTime)
            {
                try
                {
                    IWebElement SrchElement = WebDriver.FindElement(By.XPath(XPath));

                    SrchElement.Clear();

                    if (TextParam != null)
                        SrchElement.SendKeys(TextParam);
                }
                catch
                {
                    continue;
                }

                break;
            }
        }

        public void WaitForElementPresent(String XPathParam, Int32 WaitTimeSeconds = 15)
        {
            //WebDriverWait DrvWait = new WebDriverWait(WebDriver, new TimeSpan(0, 0, WaitTimeSeconds));

            //try
            //{
            //    DrvWait.Until(ExpectedConditions.ElementIsVisible(By.XPath(XPathParam)));
            //}
            //catch (Exception Err)
            //{
            //    throw new Exception("WaitByXPath <" + XPathParam + ">[" + Err.Message + "]");
            //}

            DateTime StopTime = DateTime.Now.AddSeconds(WaitTimeSeconds);

            while (DateTime.Now < StopTime)
            {
                ReadOnlyCollection<IWebElement> SrchCollection = WebDriver.FindElements(By.XPath(XPathParam));

                if (SrchCollection.Count == 0)
                    continue;
                else
                    return;
            }

            throw new Exception("WaitByXPath <" + XPathParam + ">[Timed Out]");
        }

        public void Click(String XPath)
        {
            DateTime StopTime = DateTime.Now.AddSeconds(15);

            while (DateTime.Now < StopTime)
            {
                try
                {
                    WaitForElementPresent(XPath);
                    IWebElement SrchElement = WebDriver.FindElement(By.XPath(XPath));

                    if(SrchElement.Enabled == false)
                        continue;

                    SrchElement.Click();
                    return;
                }
                catch
                {
                }
            }

            throw new Exception(
                String.Format("Unable to click on element <{0}>",
                    XPath));
        }

        public void ClickHover(String XPath)
        {
            DateTime StopTime = DateTime.Now.AddSeconds(15);

            while(DateTime.Now < StopTime)
            {
                try
                {
                    WaitForElementPresent(XPath);
                    IWebElement SrchElement = WebDriver.FindElement(By.XPath(XPath));

                    if(SrchElement.Enabled == false)
                        continue;

                    Actions ActionItem = new Actions(WebDriver);

                    ActionItem.Click(SrchElement).MoveToElement(SrchElement).Build().Perform();

                    return;
                }
                catch
                {
                }
            }

            throw new Exception(
                String.Format("Unable to click and hover on element <{0}>",
                    XPath));
        }

        public List<WebItemClass> Find(String XPath)
        {
            WaitForElementPresent(XPath);

            DateTime StopTime = DateTime.Now.AddSeconds(15);

            List<WebItemClass> DataReturn = new List<WebItemClass>();

            while (DateTime.Now < StopTime)
            {
                ReadOnlyCollection<IWebElement> SrchCollection = WebDriver.FindElements(By.XPath(XPath));

                try
                {
                    foreach (IWebElement ElementEnum in SrchCollection)
                    {
                        WebItemClass WebItemEnum = new WebItemClass(ElementEnum);

                        DataReturn.Add(WebItemEnum);
                    }
                }
                catch (StaleElementReferenceException)
                {
                    DataReturn.Clear();
                    continue;
                }

                break;
            }

            return DataReturn;
        }

        public List<WebItemClass> FindByText(String XPath, String SrchText)
        {
            DateTime StopTime = DateTime.Now.AddSeconds(30);

            List<WebItemClass> DataReturn = new List<WebItemClass>();

            while (DateTime.Now < StopTime)
            {
                ReadOnlyCollection<IWebElement> SrchCollection = WebDriver.FindElements(By.XPath(XPath));

                try
                {
                    foreach (IWebElement ElementEnum in SrchCollection)
                    {
                        String linkText = ElementEnum.Text;
                        /// TODO: Need weight logic

                        if (ElementEnum.Text.Contains(SrchText))
                        {
                            WebItemClass WebItemEnum = new WebItemClass(ElementEnum);
                            DataReturn.Add(WebItemEnum);
                        }
                    }
                }
                catch
                {
                    DataReturn.Clear();
                    continue;
                }

                if (DataReturn.Count == 0)
                    continue;

                break;
            }

            return DataReturn;
        }
    }
}
