using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System;
using System.Threading;

namespace TruckeroApp.Tests;

[TestFixture]
public class StartupFlowTests
{
    // For Appium.WebDriver 7.x
    private AndroidDriver _driver;  // No generic type parameter
    private AppiumOptions _options;

    [SetUp]
    public void Setup()
    {
        _options = new AppiumOptions();
        _options.PlatformName = "Android";
        _options.AddAdditionalAppiumOption("app", "/path/to/your/app.apk"); // Replace with actual path
        _options.AddAdditionalAppiumOption("deviceName", "Android Emulator");
        _options.AddAdditionalAppiumOption("automationName", "UiAutomator2");

        // Initialize without generic type parameter
        _driver = new AndroidDriver(
            new Uri("http://127.0.0.1:4723/wd/hub"),
            _options
        );
    }
}
