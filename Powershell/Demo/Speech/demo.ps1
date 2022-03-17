Add-Type -AssemblyName System.Speech

$voice = New-Object System.Speech.Synthesis.SpeechSynthesizer


$voice.SpeakAsync("各位老板好！考考我的小学数学题吧") | Out-Null

try {
    $math = Read-Host "随便写个公式"

    $rslt = Invoke-Expression $math
    $voice.SpeakAsync("$math=$rslt")

    $voice.SpeakAsync("谢谢老板，欢迎下次光临！") | Out-Null
}
catch {
    $voice.Speak("这是什么鬼！玩我呢"); 
}