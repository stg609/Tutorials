# Your working directory
$workingPath = 'C:\Projects\Github\Tutorials\Powershell\v7\Demo\Selenium'

# Add the working directory to the environment path.
# This is required for the ChromeDriver to work.
if (($env:Path -split ';') -notcontains $workingPath) {
    $env:Path += ";$workingPath"
}

# Import Selenium to PowerShell using the Add-Type cmdlet.
Add-Type -Path "$($workingPath)\WebDriver.dll"

# create object of ChromeOption class
$options = New-Object OpenQA.Selenium.Chrome.ChromeOptions
$options.AddArgument("--disable-notifications")

# Create a new ChromeDriver Object instance.
$ChromeDriver = New-Object OpenQA.Selenium.Chrome.ChromeDriver $options

# Launch a browser and go to URL
$ChromeDriver.Navigate().GoToURL('https://app.dev.powertradepro.com/')

# Enter the username in the Username box
$ChromeDriver.FindElementByXPath('//*[@id="account"]').SendKeys('2853023689@qq.com')

# Enter the password in the Password box
$ChromeDriver.FindElementByXPath('//*[@id="password"]').SendKeys('Passw0rd1')

# Click on the Login button
$ChromeDriver.FindElementByXPath('//*[@id="login-form"]/div[6]/button').Click()

$ChromeDriver.Manage().Timeouts().ImplicitWait = New-TimeSpan -Seconds 5
$ChromeDriver.FindElementByXPath('/html/body/div[1]/div/section/div[1]/aside/div/div[2]/div/div/ul[2]/li[1]/a').Click()

$ChromeDriver.FindElementByXPath('/html/body/div[1]/div/div/section/div[1]/div[1]/div[2]/div/div/div/div[2]/div[3]/div/div/div/div/span[3]/span[2]/span/span[1]').Click()

$ChromeDriver.FindElementByXPath('/html/body/div[1]/div/div/section/div[2]/main/div/div/div/div/div/div/div/div/div[1]/div/div[1]/div/div/div/div[1]/button/span[2]').Click()

[string]$dateStr = "$(Get-Date) test"
$inputEle = $ChromeDriver.FindElementByXPath("/html/body/div[1]/div/div/section/div[2]/main/div/div/div/div/div/div/div/div/div[4]/div/div[2]/div/div/div/div[2]/div/div/div[2]/div/div/div/div/div/form/div/div/div/div/div/div/div/div/div[2]/div/div/div/div/input")
$ChromeDriver.executeScript("arguments[0].value='$dateStr'",$inputEle)

$ChromeDriver.Manage().Timeouts().ImplicitWait = New-TimeSpan -Seconds 5
$ChromeDriver.FindElementByXPath("/html/body/div[1]/div/div/section/div[2]/main/div/div/div/div/div/div/div/div/div[4]/div/div[2]/div/div/div/div[2]/div/div/div[1]/div/div[2]/div[2]/div").Click()



Get-Process |Where-Object {$_.CPU -gt 80} | Select-Object -First 1