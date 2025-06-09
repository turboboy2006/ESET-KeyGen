using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI; // For WebDriverWait if needed later
using System;
using System.Linq;
using System.Threading.Tasks; // For Task.Delay

// Note: Using System.Text.RegularExpressions is not in the provided snippet, but good for URL normalization if more complex rules are needed.

namespace RankTracker.Core.Services
{
    public class SeleniumRankCheckerService : ISeleniumRankChecker, IDisposable
    {
        private IWebDriver _driver;
        private readonly string _chromeDriverPath;

        public SeleniumRankCheckerService(string chromeDriverPath = null)
        {
            // If chromeDriverPath is null, ChromeDriver will look in the executing assembly's directory and then in PATH.
            _chromeDriverPath = chromeDriverPath;
        }

        private void InitializeDriver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
            chromeOptions.AddArgument("--lang=en-US"); // Request English results
            chromeOptions.AddUserProfilePreference("intl.accept_languages", "en-US");


            if (!string.IsNullOrEmpty(_chromeDriverPath))
            {
                _driver = new ChromeDriver(_chromeDriverPath, chromeOptions);
            }
            else
            {
                _driver = new ChromeDriver(chromeOptions);
            }
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        }

        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            url = url.ToLowerInvariant().Trim();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                // Prefer HTTPS, but be aware if the site doesn't support it.
                // For matching, it's often safer to strip protocol or normalize to one.
                url = "https://" + url;
            }
            url = url.Replace("www.", "");
            url = url.TrimEnd('/');
            return url;
        }

        public async Task<(int? Rank, string Notes)> GetRankAsync(string targetUrl, string keyword, int maxPagesToCheck = 10)
        {
            if (_driver == null)
            {
                InitializeDriver();
            }

            string normalizedTargetUrl = NormalizeUrl(targetUrl);
            if (string.IsNullOrWhiteSpace(normalizedTargetUrl))
            {
                 Dispose(); // Dispose driver if URL is invalid and it was just initialized
                return (null, "Invalid target URL provided.");
            }

            try
            {
                // Using num=100 is aggressive and might get flagged. Default (10) is safer to start.
                _driver.Navigate().GoToUrl("https://www.google.com/search?q=" + Uri.EscapeDataString(keyword) + "&num=10");

                // Cookie Consent Handling - Highly Volatile Selectors
                try
                {
                    // Try multiple common selectors for consent buttons. Order by likelihood or specificity.
                    IWebElement consentButton = null;
                    var buttonSelectors = new[] {
                        "//button[.//div[contains(text(),'Accept all')]]",
                        "//button[.//div[contains(text(),'Reject all')]]", // Often good to click reject if available
                        "//button[.//div[contains(text(),'Agree')]]",
                        "//div[@role='dialog']//form//button[contains(., 'Accept') or contains(., 'Agree')]", // More specific form button
                        "//div[contains(text(), 'Before you continue to Google Search')]//button[contains(., 'Accept all')]" // Example of a more complex one
                    };

                    foreach(var selector in buttonSelectors) {
                        try {
                            consentButton = _driver.FindElements(By.XPath(selector)).FirstOrDefault(b => b.Displayed && b.Enabled);
                            if (consentButton != null) break;
                        } catch (NoSuchElementException) {} // Ignore if a particular selector fails
                    }

                    if (consentButton != null)
                    {
                        consentButton.Click();
                        await Task.Delay(TimeSpan.FromSeconds(3)); // Increased delay
                    }
                }
                catch (Exception) { /* Consent handling failed, log and continue if possible */ }


                int currentRank = 0;
                for (int pageNum = 1; pageNum <= maxPagesToCheck; pageNum++)
                {
                    // Search result selectors - also volatile. Prioritize by stability.
                    // 'div.g' is general. 'div.tF2Cxc', 'div.MjjYud', 'div.hlcw0c', 'div.Gx5Zad' are more specific containers.
                    // It's best to inspect current Google SERP structure.
                    var searchResultContainers = _driver.FindElements(By.CssSelector("div.g")); // Broadest
                    if (!searchResultContainers.Any()) searchResultContainers = _driver.FindElements(By.CssSelector("div.MjjYud")); // Common modern one
                    if (!searchResultContainers.Any()) searchResultContainers = _driver.FindElements(By.CssSelector("div.tF2Cxc")); // Older common one

                    if (!searchResultContainers.Any() && pageNum == 1) {
                        // Check for CAPTCHA or unusual page structure
                        if (_driver.PageSource.Contains("Our systems have detected unusual traffic")) {
                            return (null, "CAPTCHA or unusual traffic detected.");
                        }
                         return (null, "No search results found on the first page. Possible page structure change or CAPTCHA.");
                    }


                    foreach (var container in searchResultContainers)
                    {
                        try
                        {
                            var linkElement = container.FindElement(By.CssSelector("a[href]")); // Get the first 'a' tag with an href
                            string rawUrl = linkElement.GetAttribute("href");

                            if (!string.IsNullOrWhiteSpace(rawUrl))
                            {
                                // Filter out non-http/https links (e.g., related searches, images) if they get caught by selector
                                if (!rawUrl.StartsWith("http://") && !rawUrl.StartsWith("https://")) continue;

                                currentRank++; // Increment only for valid search result links
                                string normalizedFoundUrl = NormalizeUrl(rawUrl);

                                if (normalizedFoundUrl.Contains(normalizedTargetUrl))
                                {
                                    return (currentRank, $"Found on page {pageNum}, overall position {currentRank}.");
                                }
                            }
                        }
                        catch (NoSuchElementException) { /* This container wasn't a standard result type */ }
                    }

                    if (pageNum < maxPagesToCheck)
                    {
                        try
                        {
                            var nextPageLink = _driver.FindElement(By.CssSelector("a#pnnext"));
                            if (nextPageLink != null && nextPageLink.Displayed && nextPageLink.Enabled) {
                                nextPageLink.Click();
                                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(3, 7))); // Longer, randomized delay
                            } else {
                                 return (null, $"Not found. Reached end of results or 'Next' button not interactable after {pageNum} pages.");
                            }
                        }
                        catch (NoSuchElementException)
                        {
                            return (null, $"Not found. No 'Next' page link after {pageNum} pages.");
                        }
                         catch (ElementNotInteractableException)
                        {
                             return (null, $"Not found. 'Next' page link not interactable after {pageNum} pages.");
                        }
                    }
                }
                return (null, $"Not found within {maxPagesToCheck} pages.");
            }
            catch (WebDriverException ex)
            {
                return (null, $"WebDriver error: {ex.Message.Split('\n').FirstOrDefault()}"); // Shorter error
            }
            catch (Exception ex)
            {
                return (null, $"Unexpected error: {ex.Message.Split('\n').FirstOrDefault()}");
            }
            finally
            {
                // Consider if driver should be disposed here or managed by DI lifecycle
                // If service is Scoped, DI will call Dispose. If Singleton, manual or dedicated management is needed.
            }
        }

        public void Dispose()
        {
            _driver?.Quit(); // Closes all browser windows and safely ends the session
            _driver?.Dispose(); // Releases the resources used by the ChromeDriverService
            _driver = null; // Ensure it's null so InitializeDriver is called next time if service is reused (not typical for Scoped)
        }
    }
}
