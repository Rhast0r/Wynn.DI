# Wynn.DI

**Wynn.DI** is a lightweight Dependency Injection-Framework helping you to accelerate the development of your application whilst still being easy to maintain and test.

## Overview
- **Simple** - Easy to use fluent-API can be picked up quickly
- **Safe** - Additional validation features help you to detect errors early before you start shipping
- **Strict** - Enforces code to be written in a certain way improving maintainability and reducing side-effects
- **Specific** - Everything is done explicity leaving no room for imagination

## Features
- Seperation between Service- and Implementation type
- Singleton and non-Singleton support
- Factories
- Safe Post-Creation callbacks
- Validation

## Getting started

### Singleton
```csharp
public interface IFoo
{
}

public class Foo : IFoo
{
}

var container = Container.Create();

container
  .Bind<IFoo>() // Specify the service type
  .ToNew<Foo>() // Specify the implementation type of said service type and the method it is supposed to be constructed
  .AsCached() // Returns a cached instance whenever requested
  .OnInstall(); // Is resolved when the container is installed
  
container.Install(); // Finish the binding process and resolve types which are marked as "OnInstall"
var foo = container.Get<IFoo>(); 
```

### Field Injection and Post-Injection Callback
```csharp
public class Foo
{
  public void DoFooThings()
  {
    System.Diagnostics.Debug.WriteLine("DoFooThings"); 
  }
}

// with IInitialize we register ourselves for callback which are going to be called when: 
//    dependencies have been resolved, injected and initialized 
//    this type has been fully injected
public class Bar : IInitialize 
{
  [Inject]
  private readonly Foo _foo = null; // we explicitly mark the field we want to inject with an attribute and therefor declare it's dependencies
  
  void IInitialize.Initialize()
  {
    // at this stage is safe to communicate with other dependencies
    _foo.DoFooThings(); 
  }
} 

var container = Container.Create();

container.Bind<Foo>().ToNew().AsCached().OnInstall();
container.Bind<Bar>().ToNew().AsCached().OnInstall();

container.Install();
// "DoFooThings" is printed as Bar is resolved on install which triggers the Initialize-Callback
```

### Factories
```csharp
public class Foo
{
}

public class Bar
{
  [Inject]
  private readonly Foo _foo; // A non-singleton version of Foo will be injected
  [Inject]
  private readonly IFactory<Foo> _fooFactory; // A singleton Foo-Factory will be injected allowing you to create more Foo-objects
}

container
  .Bind<Foo>() // Specify the service type
  .ToNew() // Specify the implementation type is equal to the service type and the method it is supposed to be constructed
  .AsTransient() // Return a new instance whenever requested
  .OnRequest(); // Is going to be resolved whenever the service type is requested
  
container.Bind<Bar>().ToNew().AsCached().OnInstall(); 
  
container.Install(); 
  
// As Bar is marked as transient it will only be created when explicity asked for which can be done as such
//    1. By requesting it from the container
var foo = container.Get<Foo>(); 
//    2. By requesting the factory and using the create-Method
var fooFactory = container.Get<IFactory<Foo>>(); 
var foo2 = barFactory.Create(); 
//    3. By a class having a field of said service type (See: "Bar")
```
