dg.Utilities
============

*This repo is archived now.*

* The pieces of code that are still relevant - migrated to [Codenet](https://github.com/danielgindi/Codenet) (Can be found on Nuget).  
* Other stuff is obsolete and there are better and maintained solutions either as packages or as the .NET core.  
* Html encoder is available as `System.Net.WebUtility.EncodeHtml(...)`.
* The streaming csv reader is migrated, but for excel files you should one of the many libs available on Nuget. A good candidate could be [ExcelDataReader](https://www.nuget.org/packages/ExcelDataReader).
* Due to BouncyCastle not implementing XXTEA, and other implementations appending their own data to the input buffer - I had to migrate the XXTEA implementation too.
* Levenstein distance is available in [Standard.Data.StringMetrics](https://www.nuget.org/packages/Standard.Data.StringMetrics/).
