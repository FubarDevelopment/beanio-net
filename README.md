# ![BeanIO .NET](beanio-logo.png "BeanIO") A .NET port of the [BeanIO](http://beanio.org) java library

[![Build-Status](https://build.fubar-dev.de/guestAuth/app/rest/builds/buildType:%28id:BeanIONet_ReleaseBuild%29/statusIcon)](https://build.fubar-dev.com/viewType.html?buildTypeId=BeanIONet_ReleaseBuild&guest=1)

# Latest changes

## 4.1.0

* Supports a new experimental property accessor factory `asm` which uses compiled expression trees instead of reflection
* Update of all dependencies

## 4.0.0

* Now supports .NET Core 1.0 (through .NET Standard 1.1)
* BREAKING CHANGE: Removed JSON support, because [Newtonsoft.Json](http://www.newtonsoft.com/json) is available for all platforms and provides its own (de-)serialization mechanisms
* BREAKING CHANGE: Support for .NET Standard 1.1, .NET 4.5, and C++/CLI (.NET 4.5); Visual Studio 2015 Update 3 with NuGet 3.5.0 beta 2 or latest Xamarin Studio (maybe even from the beta channel) are required.

## 3.1.3

* Support for adding derived types to collections

## 3.1.2

* Fixed `parseDefault==false` when serializing non-null value

# Advantages over IKVM converted java BeanIO

* Also usable in mobile (Windows Phone, Android, iOS) and Windows Store applications
* No need to handle with the java types (like `ArrayList`).
* No need to provide all the IKVM assemblies.
* Contains support for customer provided scheme handlers to be able to get the imported mappings from every place you need to load it from (Internet, isolated storage, etc.)

# Compatibility

This port is in most parts compatbile with 2.1.0 of BeanIO, except the JSON functionality, as [Newtonsoft.Json](http://www.newtonsoft.com/json) already supports very flexible (de-)serialization.

## Differences

### New features

* Supports .NET Standard 1.1 (.NET Core 1.0, .NET Framework 4.5+, Windows 8+, Windows Phone 8.1+, Mono 4.4+, Xamarin.iOS, Xamarin.MonoTouch, and Xamarin.Android).
* Contains support for generics.
* Contains support for customer provided scheme handlers (to be able to provide direct file access).
* New mapping file to enable field validation while marshalling (2015-06)
* Added mapping files to NuGet package
* Support for `ICollection<>` interfaces
* New `At()` function for StreamBuilder
* Added the ability to use templates from the `RecordBuilder` and `SegmentBuilder`
* New `parseDefault` configuration to allow default values that don't match the unterlying fields type (e.g. `00000000` as `LocalDate` with format `yyyyMMdd`)

### Changed

* The namespace is now `BeanIO` instead of `org.beanio`.
* Type names must contain the assembly name, because there is no `ClassLoader` in .NET.
* The `date` type handler uses `NodaTime.LocalDate`.
* The `time` type handler uses `NodaTime.LocalTime`.
* When you want to use a date with time zone, you have to use the `datetimeoffset` type handler.
* There's a `resource:` scheme instead of a `classpath:` scheme that references an assembly ressource. The assembly name must be specified.
* Use of ressource files (`*.resx`) for error message ressources instead of `*.properties` files. 
* There's no `FieldsAttribute` which may contain `FieldAttribute`s. Just use multiple `UnboundFieldAttribute`s.

### Missing 

* BeanIO.NET exists as portable (.NET Standard 1.1) library and therefore doesn't provide any file access.
* Patterns like `X###0.0X` aren't supported by .NET, because there is no `ParseExact` for numeric types.
* JSON isn't a supported file format, because [Newtonsoft.Json](http://www.newtonsoft.com/json) already supports very flexible (de-)serialization.
