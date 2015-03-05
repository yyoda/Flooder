# Flooder
fluent-pligin for windows.

##Supports the following
text file in to fluentd  
iis access log to fluentd  
eventlog to fluentd  

##fluentd settings sample
```
<source>
  type forward
  port 9901
</source>
```

##dependency
MsgPack  
NLog  
System.Reactive.Core  
System.Reactive.Linq  
System.Reactive.Interfaces  
