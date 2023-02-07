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


var result = 1.0f;

for (int i = 1; i <= 10; i++)
{
    var i1 = i;
    new Thread(() => { result += DoStep(i1 * 1000); }).Start();
}


Console.WriteLine(result*4);
