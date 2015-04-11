# Flooder
fluent-pligin for windows.

##supports the following
* text file  
* iis access log  
* eventlog  
* performance counter

send to fluentd.  

##fluentd settings sample
```
<source>
  type forward
  port 9901
</source>
```
##app.config
```App.config
<configuration>
    <!-- カスタマイズされたコンフィグセクションを読み込むのでその設定を行います -->
    <configSections>
        <section name="flooder" type="Flooder.Configuration.Section, Flooder"/>
    </configSections>
    <!-- Flooder用にカスタマイズされたコンフィグセクションです -->
    <flooder>
        <!-- 監視対象をeventセクションで設定します -->
        <event>
            <!-- テキストファイルをリアルタイム監視して検出した変更差分を転送します(省略可) -->
            <fileSystems>
                <!--
                    tag:    送信先でデータを識別するためのタグを指定します(必須)
                    path:   監視したいファイルのパスを指定します(必須)
                    file:   監視したいファイル名を指定します。アスタリスクによるあいまい検索が可能です(必須)
                    parser: 取得したデータを転送前に加工するためのパーサを指定可能です(省略可)
                            省略時はFlooder.Event.Parser.DefaultParser(何もしない)が適用されます
                            Flooder.Event.IPayloadParserを実装したクラスを作成してココに指定することで拡張することができます
                -->
                <add tag="myapp1_log" path="C:\myapp1\log" file="app.log"/>
                <add tag="myapp1_log" path="C:\myapp1\log" file="perf.log"/>
                <add tag="myapp2_log" path="C:\myapp2\log" file="app.*" parser="Flooder.Event.Parser.ApiParser, Flooder"/>
           </fileSystems>
            <!-- IISログを一定間隔で監視して検出した変更差分を転送します(省略可) -->
            <iis>
                <!--
                    tag:      送信先でデータを識別するためのタグを指定します(必須)
                    path:     監視したいファイルのパスを指定します(必須)
                    interval: 監視する間隔を秒で指定可能です。省略時は1秒が設定されます(省略可)
                -->
                <add tag="myapp1_iis" path="C:\LogFiles\myapp\W3SVC31" interval="1"/>
                <add tag="myapp2_iis" path="C:\LogFiles\myapp\W3SVC31" interval="1"/>
            </iis>
            <!-- イベントログをリアルタイム監視して検出した変更差分を転送します(省略可)
                    tag:    送信先でデータを識別するためのタグを指定します(必須)
                    scopes: どのイベントログを監視するかをカンマ区切りで複数指定することができます(省略可)
                            省略時はApplicationとSystemが設定されます
            -->
            <eventLogs tag="myserver_event" scopes="Application,System"/>
            <!-- パフォーマンスカウンタを一定間隔で採取して結果を転送します(省略可)
                    tag:      送信先でデータを識別するためのタグを指定します(必須)
                    interval: 採取する間隔を秒で指定可能です(省略可)
                              省略時は15秒で設定されます
            -->
            <performanceCounters tag="myserver_performance_counter" interval="15">
                <!--
                    categoryName: パフォーマンスカウンタのカテゴリ名を指定します
                    counterName:  パフォーマンスカウンタのカウンタ名を指定します
                    instanceName: インスタンス名を指定します（存在しない場合は省略可能で、又あいまい検索が可能です）
                -->
                <add categoryName="Memory" counterName="% Committed Bytes In Use"/>
                <add categoryName="Memory" counterName="Available MBytes"/>
                <add categoryName="Memory" counterName="Committed Bytes"/>
                <add categoryName="Memory" counterName="Pages/sec"/>
                <add categoryName="Memory" counterName="Pool Nonpaged Bytes"/>
                <add categoryName="Memory" counterName="Cache Faults/sec"/>
                <add categoryName="PhysicalDisk" counterName="% Disk Time" instanceName="_Total"/>
                <add categoryName="PhysicalDisk" counterName="Avg. Disk sec/Read" instanceName="_Total"/>
                <add categoryName="PhysicalDisk" counterName="Avg. Disk sec/Transfer" instanceName="_Total"/>
                <add categoryName="PhysicalDisk" counterName="Avg. Disk sec/Write" instanceName="_Total"/>
                <add categoryName="PhysicalDisk" counterName="Current Disk Queue Length" instanceName="_Total"/>
                <add categoryName="Process" counterName="% Processor Time" instanceName="w3w*"/>
                <add categoryName="Process" counterName="IO Read Bytes/sec" instanceName="w3w*"/>
                <add categoryName="Process" counterName="IO Write Bytes/sec" instanceName="w3w*"/>
                <add categoryName="Process" counterName="Thread Count" instanceName="w3w*"/>
                <add categoryName="Processor" counterName="% Interrupt Time" instanceName="_Total"/>
                <add categoryName="Processor" counterName="% Privileged Time" instanceName="_Total"/>
                <add categoryName="Processor" counterName="% Processor Time" instanceName="_Total"/>
            </performanceCounters>
        </event>
        <!--
            転送先や方法をworkerで設定します
                type: fluentd   複数のfluentdサーバーが設定できます(この場合はどのサーバーへ転送するかを都度抽選します)
                      stdout    標準出力です
        -->
        <worker type="fluentd">
            <add host="192.168.100.1" port="5091"/>
            <add host="192.168.100.2" port="5091"/>
        </worker>
    </flooder>
</configuration>
```

##dependency
MsgPack  
NLog  
System.Reactive.Core  
System.Reactive.Linq  
System.Reactive.Interfaces  
