# Wynn.DI

**Wynn.DI** is a lightweight Dependency Injection-Framework helping you to accelerate the development of your application whilst still being easy to maintain and test.

## Goals
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

## Overview

### Implementation Type 
 | Method | Definition | Implications |
 | --- | --- | --- |
 | `.Bind(...)` <br> `.Bind<T>()` | Sets service type to passed type |  |

### Service type and creation method 

 | Method | Definition | Implications |
 | --- | --- | --- |
 | `.ToNew()` | Sets implementation type to previously passed service type. Object will be created using the constructor without arguments |  |
 | `.ToNew(...)` <br> `.ToNew<T>()`  | Sets implementation type to passed type. Object will be created using the constructor without arguments |  |
 | `.ToConstant(...)`  | Sets implementation type to type of passed object. | Cannot use `.AsTransient()` anymore |
 
### Scope
 | Method | Definition | Implications |
 | --- | --- | --- |
 | `.AsCached()` | A single object of implemententation type will be resolved which will be returned whenever requested | Cannot use `.OnInstall()` anymore |
 | `.AsTransient()` | A new instance of implemententation type is created and returned whenever requested. In addition a binding for IFactory<TServiceType> will be created which can be used to create objects of implementation type |  |
	
### Resolution
 | Method | Definition | Implications |
 | --- | --- | --- |
 | `.OnInstall()` | The binding will we resolved as soon as the container is installed |  |
 | `.OnRequest()` | The binding will we resolved when explicitly requested with `.Get(...)` or `.Get<T>()` or implicitly when another resolved binding has a dependency on service type |  |
 
## Examples

```csharp
public interface IFoo { }

public class Foo : IFoo { }

public class Bar : IInitialize
{ 
    [Inject]
    private readonly IFoo _foo = null; 
    public IFoo Foo => _foo; 
    [Inject]
    private readonly Baz _baz = null; 
    public Baz Baz => _baz; 
    
    void IInitialize.Initialize()
    {
        System.Diagnostics.Debug.WriteLine("Bar was initialized"); 
    }
}

public class Baz 
{
    [Inject]
    private readonly IFoo _foo = null; 
    public IFoo Foo => _foo; 
}

// create container
var container = Container.Create();

// create our bindings
container.Bind<IFoo>().ToNew<Foo>().AsCached().OnInstall();
container.Bind<Bar>().ToNew().AsCached().OnRequest();
container.Bind<Baz>().ToNew().AsTransient().OnRequest();
  
// finish our bindings. Bindings marked as "OnInstall" are immediatly resolved"
var resolver = container.Install(); 

// Get objects of certain types
var foo = resolver.Get<IFoo>();

// Bar implements IInitialize, hence when it is resolved the Initialize()-method is called
var bar = resolver.Get<Bar>(); 
// "Bar was initialized" is being printed

// As Baz is marked "AsTransient" a new instance is returned whenever requested
var baz1 = resolver.Get<Baz>(); 
var baz2 = resolver.Get<Baz>(); 
var baz3 = bar.Baz; // Bar has requested an object of type Baz as field

// As Baz is marked "AsTransient" a IFactory<Baz> is also bound and can be used to create instances of Baz
var bazFactory = resolver.Get<IFactory<Baz>(); 
var baz4 = bazFactory.Create(); 
```
