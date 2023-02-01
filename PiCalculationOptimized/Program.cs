using System.Diagnostics;

static float DoStep(int maxValue)
{
    float piValue = 0;
    int currentValue = maxValue - 1000;
    while (currentValue++ < maxValue)
    {
        piValue += (float) (1.0 / (currentValue * 4.0 + 1.0));
        piValue -= (float)(1.0 / (currentValue * 4.0 - 1.0));
    }

    return piValue;
}

Stopwatch watch = new Stopwatch();

watch.Start();
var result = 1.0f;

new Thread(() => { result += DoStep(1_000); }).Start();
new Thread(() => { result += DoStep(2_000); }).Start();
new Thread(() => { result += DoStep(3_000); }).Start();
new Thread(() => { result += DoStep(4_000); }).Start();
new Thread(() => { result += DoStep(5_000); }).Start();
new Thread(() => { result += DoStep(6_000); }).Start();
new Thread(() => { result += DoStep(7_000); }).Start();
new Thread(() => { result += DoStep(8_000); }).Start();
new Thread(() => { result += DoStep(9_000); }).Start();
new Thread(() => { result += DoStep(10_000);}).Start();

watch.Stop();

Console.WriteLine(result*4);
Console.WriteLine(watch.Elapsed);