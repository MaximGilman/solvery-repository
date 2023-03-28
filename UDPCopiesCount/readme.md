# Проект для подсчета количества инстансов приложения в локальной сети.

### Каждое приложение необходимо запускать отдельно.
### В папке `publish` представлены сборки win х64 и х32 

#### Каждый узел отправляет запрос в локальную сеть по `IpAdress.Broadcast` и слушает по `IpAdress.Any`
Пример конфигурационного файла:

```json
{
  "Port": 8001
}
```


## !Важно
файл NLog.Config не хранится в гите, а прилетает из нугетов, поэтому нужно либо запускать уже собранные приложения из `publish`, либо вставить в  проекте в файл `NLog.Config следующее содержимое

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      xsi:schemaLocation="http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd"
      autoReload="true"
      throwExceptions="true"
      internalLogLevel="Off" internalLogFile="c:\temp\nlog-internal.log">

  <targets>
    <target name="logConsole" xsi:type="ColoredConsole"  useDefaultRowHighlightingRules="false"
            layout="${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${message}">
      <highlight-row condition="level == LogLevel.Debug" foregroundColor="DarkGray"/>
      <highlight-row condition="level == LogLevel.Info" foregroundColor="White"/>
      <highlight-row condition="level == LogLevel.Error" foregroundColor="Red" />
    </target>

  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="logConsole"/>

  </rules>
</nlog>
```