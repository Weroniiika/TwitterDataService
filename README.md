# TwitterDataService

What is it?
-----------

This is a project created as a mean to become more familiar with .NET and some concepts of Windows Comunication Foundation services. It is an example of a simple service that provides api to obtain, save and present tweets from Twitter Public Stream. 
TwitterDataService api consists of four methods of which one allows to request tweets from Twitter with filtering word 
and three other allow to retrieve data from database and get some overview of tweets. Unfortunately Twitter has in place some strict rules that limit how many requests can be made on behalf of one app during 15 minutes, 1 hour etc. Therefore application will never be suitable for wide and extensive use. To see how the service works together with a client go here: http://datafromtwitter.com. 

Technology
----------

Application uses some of popular .NET 4.5 solutions, such as: EntityFramework (code first application), LINQ, WCF web services and Tasks for thread handling. Parsing of tweets is handled with use of Newtonsoft.Json library and c# dynamic objects. Logging is implemented with Nlog and some tests where created in NUnit framework.

How to try project?
-------------------

If you want to download a project and try it out localy you need a service host and a client that would communicate with service. 
You can find them on my github account: [ServiceHost](https://github.com/Weroniiika/TwitterDataServiceHost) and [TwitterMonitor](https://github.com/Weroniiika/TwitterMonitor) (client). Project is of course well suited to be hosted on IIS. Some configuration changes in project will be are required depending on your environment, so you should have some minimun familiarity with .NET. In particular database conection strings would have to be updated and more importantly Authorisation.cs file requires token and consumer key values to be provided. To obtain them Twitter App  must be created. To do that go here: https://apps.twitter.com/  and for Twitter development info and documentation go here: https://dev.twitter.com/.

Contact
-------

If you have any questions or suggestions regarding the project I am happy to hear them. Write here: wer.adamiec@gmail.com
