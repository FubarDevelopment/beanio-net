# ![BeanIO .NET](beanio-logo.png "BeanIO") A .NET port of the [BeanIO](http://beanio.org) java library

# Advantages over IKVM converted java BeanIO

* Also usable in mobile (Windows Phone, Android, iOS) and Windows Store applications
* No need to handle with the java types (like ```ArrayList```).
* No need to provide all the IKVM assemblies.
* Contains support for customer provided scheme handlers to be able to get the imported mappings from every place you need to load it from (Internet, isolated storage, etc.)

# Compatibility

This port is in most parts compatbile with 2.1.0 of BeanIO.

## Differences

### New features

* Exists as PCL for .NET 4.5, Windows 8, WP 8.1, Xamarin.iOS, Xamarin.MonoTouch, and Xamarin.Android.
* Contains support for generics.
* Contains support for customer provided scheme handlers (to be able to provide direct file access).
* New mapping file to enable field validation while marshalling (2015-06)
* Added mapping files to NuGet package
* Support for ```ICollection<>``` interfaces
* New At() function for StreamBuilder

### Changed

* The namespace is now ```BeanIO``` instead of ```org.beanio```.
* Type names must contain the assembly name, because there is no ```ClassLoader``` in .NET.
* The ```date``` type handler uses ```NodaTime.LocalDate```.
* The ```time``` type handler uses ```NodaTime.LocalTime```.
* When you want to use a date with time zone, you have to use the ```datetimeoffset``` type handler.
* There's a ```resource:``` scheme instead of a ```classpath:``` scheme that references an assembly ressource. The assembly name must be specified.
* Use of ressource files (```*.resx```) for error message ressources instead of ```*.properties``` files. 
* There's no ```FieldsAttribute``` which may contain ```FieldAttribute```s. Just use multiple ```UnboundFieldAttribute```s.

### Missing 

* BeanIO.NET exists as PCL and therefore doesn't provide any file access.
* Patterns like ```X###0.0X``` aren't supported by .NET, because there is no ```ParseExact``` for numeric types.
