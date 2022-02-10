# Your working directory
$workingPath = 'E:\Github\Tutorials\Powershell\v7\Demo\Selenium'

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

# 启动浏览器
$ChromeDriver.Navigate().GoToURL('https://app.dev.powertradepro.com/')

# 账户
$ChromeDriver.FindElementByXPath('//*[@id="account"]').SendKeys('2853023689@qq.com')

# 密码
$ChromeDriver.FindElementByXPath('//*[@id="password"]').SendKeys('Passw0rd1')

# 登录
$ChromeDriver.FindElementByXPath('//*[@id="login-form"]/div[6]/button').Click()

# 选择应用
$ChromeDriver.Manage().Timeouts().ImplicitWait = New-TimeSpan -Seconds 5
$ChromeDriver.FindElementByXPath('/html/body/div[1]/div/section/div[1]/aside/div/div[2]/div/div[1]/ul[2]/li[4]/a').Click()

# 选择表单
$ChromeDriver.FindElementByXPath('/html/body/div[1]/div/div/section/div[1]/div[1]/div[2]/div/div/div/div[2]/div[3]/div/div/div/div/span[3]/span[2]/span/span[1]').Click()

# 表单记录详情
$ChromeDriver.FindElementByXPath('/html/body/div[1]/div/div/section/div[2]/main/div/div/div/div/div/div/div/div/div[1]/div/div[1]/div/div/div/div[1]/button/span[2]').Click()

# 填写
[string]$dateStr = "$(Get-Date) test"
$inputEle = $ChromeDriver.FindElementByXPath("/html/body/div[1]/div/div/section/div[2]/main/div/div/div/div/div/div/div/div/div[4]/div/div[2]/div/div/div/div[2]/div/div/div[2]/div/div/div/div/div/form/div/div/div/div/div/div/div/div/div[2]/div/div/div/div/input")
$ChromeDriver.executeScript("arguments[0].value='$dateStr'",$inputEle)

# 提交
$ChromeDriver.Manage().Timeouts().ImplicitWait = New-TimeSpan -Seconds 5
$ChromeDriver.FindElementByXPath("/html/body/div[1]/div/div/section/div[2]/main/div/div/div/div/div/div/div/div/div[4]/div/div[2]/div/div/div/div[2]/div/div/div[1]/div/div[2]/div[2]/div").Click()