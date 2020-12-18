using Wynn.DI;
using System;
using Xunit;

namespace Wynn.DI.Test
{
    public class DITest
    {
        [Fact]
        public void EmptyKernelBeingInstalledDoesNotThrow()
        {
            var container = Container.Create();

            var resolver = container.Install();
        }

        [Fact]
        public void EmptyKernelBeingValidatedDoesNotThrow()
        {
            var container = Container.Create();

            container.Validate();
            var resolver = container.Install();
        }

        [Fact]
        public void ValidateThrowsWhenDependecyIsMissingAsCached()
        {
            var container = Container.Create();

            container.Bind<DependencyMissingClass>().ToNew().AsCached().OnRequest();

            Assert.Throws<InvalidOperationException>(() => container.Validate());

            var resolver = container.Install();
        }

        [Fact]
        public void ValidateThrowsWhenDependecyIsMissingAsTransient()
        {
            var container = Container.Create();

            container.Bind<DependencyMissingClass>().ToNew().AsTransient().OnRequest();

            Assert.Throws<InvalidOperationException>(() => container.Validate());

            var resolver = container.Install();
        }

        [Fact]
        public void ResolvingClassWithConstructionArgumentsThrows()
        {
            var container = Container.Create();
            container.Bind<ConstructionArgumentNeeded>().ToNew().AsCached().OnInstall();
            Assert.Throws<MissingMethodException>(() => container.Install());
        }

        [Fact]
        public void InstallTriggersOnInstallResolution()
        {
            ResolutionAware.WasTriggered = false;
            var container = Container.Create();
            container.Bind<ResolutionAware>().ToNew().AsCached().OnInstall();
            var resolver = container.Install();

            Assert.True(ResolutionAware.WasTriggered);
        }

        [Fact]
        public void InstallDoesNotTriggerOtherResolutions()
        {
            ResolutionAware.WasTriggered = false;

            var container = Container.Create();
            container.Bind<ResolutionAware>().ToNew().AsCached().OnRequest();
            var resolver = container.Install();

            Assert.False(ResolutionAware.WasTriggered);
        }

        [Fact]
        public void DependencylessClassResolves()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsCached().OnRequest();

            var resolver = container.Install();

            Assert.NotNull(resolver.Get<EmptyClass>());
        }

        [Fact]
        public void DirectCircularDependencyThrows()
        {
            var container = Container.Create();
            container.Bind<DirectCircularDependency_A_To_B>().ToNew().AsCached().OnRequest();
            container.Bind<DirectCircularDependency_B_To_A>().ToNew().AsCached().OnRequest();

            var exception = Assert.Throws<InvalidOperationException>(() => container.Validate());
            Assert.True(exception.Message == "circular dependency");
        }

        [Fact]
        public void IndirectCircularDependencyThrows()
        {
            var container = Container.Create();
            container.Bind<IndirectCircularDependency_A_To_B>().ToNew().AsCached().OnRequest();
            container.Bind<IndirectCircularDependency_B_To_C>().ToNew().AsCached().OnRequest();
            container.Bind<IndirectCircularDependency_C_To_A>().ToNew().AsCached().OnRequest();

            var exception = Assert.Throws<InvalidOperationException>(() => container.Validate());
            Assert.True(exception.Message == "circular dependency");
        }

        [Fact]
        public void AsCachedToConstantReturnsSameObject()
        {
            var empty = new EmptyClass();

            var container = Container.Create();
            container.Bind<EmptyClass>().ToConstant(empty).AsCached().OnInstall();

            var resolver = container.Install();

            Assert.Equal(empty, resolver.Get<EmptyClass>());
        }

        [Fact]
        public void AsCachedToNewReturnsSameObject()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsCached().OnInstall();

            var resolver = container.Install();

            var sameA = resolver.Get<EmptyClass>();

            Assert.Equal(resolver.Get<EmptyClass>(), resolver.Get<EmptyClass>());
        }

        [Fact]
        public void AsTransientReturnsDifferentObjectsFromResolve()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();

            var resolver = container.Install();
            Assert.NotEqual(resolver.Get<EmptyClass>(), resolver.Get<EmptyClass>());
        }

        [Fact]
        public void AsTransientReturnsDifferentObjectsFromCreate()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();

            var resolver = container.Install();

            var factory = resolver.Get<IFactory<EmptyClass>>();
            Assert.NotEqual(factory.Create(), factory.Create());
        }

        [Fact]
        public void AsCachedReturnsSameObjects()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsCached().OnRequest();

            var resolver = container.Install();
            Assert.Equal(resolver.Get<EmptyClass>(), resolver.Get<EmptyClass>());
        }

        [Fact]
        public void CreatingAnotherBindingWhilstAnotherIsPendingThrows()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>();
            Assert.Throws<InvalidOperationException>(() => container.Bind<object>());
        }

        [Fact]
        public void InstallingWhilstBindingIsPendingThrows()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>();
            Assert.Throws<InvalidOperationException>(() => container.Install());
        }

        [Fact]
        public void CreatingDuplicateBindingThrows()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsCached().OnInstall();
            Assert.Throws<ArgumentException>(() => container.Bind<EmptyClass>().ToNew().AsCached().OnInstall());
        }

        [Fact]
        public void InitializeToConstantIsCalledOnInstall()
        {
            var eventCounter = new InjectEventCounter();
            var initializationAware = new InitializationAware();

            var container = Container.Create();
            container.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            container.Bind<InitializationAware>().ToConstant(initializationAware).AsCached().OnInstall();

            Assert.Equal(0, eventCounter.Initialize);

            var resolver = container.Install();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToConstantIsCalledOnRequest()
        {
            var eventCounter = new InjectEventCounter();
            var initializationAware = new InitializationAware();

            var container = Container.Create();
            container.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            container.Bind<InitializationAware>().ToConstant(initializationAware).AsCached().OnRequest();

            Assert.Equal(0, eventCounter.Initialize);

            var resolver = container.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware2 = resolver.Get<InitializationAware>();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewIsCalledOnInstall()
        {
            var eventCounter = new InjectEventCounter();

            var container = Container.Create();
            container.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            container.Bind<InitializationAware>().ToNew().AsCached().OnInstall();

            Assert.Equal(0, eventCounter.Initialize);

            var resolver = container.Install();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewIsCalledOnRequest()
        {
            var eventCounter = new InjectEventCounter();

            var container = Container.Create();
            container.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            container.Bind<InitializationAware>().ToNew().AsCached().OnRequest();

            Assert.Equal(0, eventCounter.Initialize);

            var resolver = container.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware = resolver.Get<InitializationAware>();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewAsTransientIsCalledOnExplicitCreate()
        {
            var eventCounter = new InjectEventCounter();

            var container = Container.Create();
            container.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            container.Bind<InitializationAware>().ToNew().AsTransient().OnRequest();

            var resolver = container.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var factory = resolver.Get<IFactory<InitializationAware>>();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware = factory.Create();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewAsTransientIsCalledOnImplicitCreate()
        {
            var eventCounter = new InjectEventCounter();

            var container = Container.Create();
            container.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            container.Bind<InitializationAware>().ToNew().AsTransient().OnRequest();

            var resolver = container.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware = resolver.Get<InitializationAware>();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void GetAsCachedResolves()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsCached().OnInstall();
            var resolver = container.Install();

            var empty = resolver.Get<EmptyClass>();
        }

        [Fact]
        public void GetAsTransientResolves()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();
            var resolver = container.Install();

            var empty = resolver.Get<EmptyClass>();
        }

        [Fact]
        public void GetAsTransientResolveFactoryType()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();
            var resolver = container.Install();

            var emptyFactory = resolver.Get<IFactory<EmptyClass>>();
        }

        [Fact]
        public void InjectFillsField()
        {
            var container = Container.Create();
            container.Bind<EmptyClass>().ToNew().AsCached().OnRequest();
            var resolver = container.Install();

            var unknownClass = new UnknownClass();
            resolver.Inject(unknownClass);

            Assert.True(unknownClass.Empty != null);
        }

        [Fact]
        public void InjectThrowsOnUnknownDependency()
        {
            var container = Container.Create();
            var resolver = container.Install();

            var unknownClass = new UnknownClass();

            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => resolver.Inject(unknownClass));
        }
    }

    public class InjectEventCounter
    {
        public int Initialize { get; set; }
    }

    public class UnknownClass
    {
        [InjectAttribute]
        public readonly EmptyClass Empty;
    }

    public class ResolutionAware
    {
        public static bool WasTriggered { get; set; }

        public ResolutionAware()
        {
            if (WasTriggered)
                throw new InvalidOperationException();

            WasTriggered = true;
        }
    }

    public class InitializationAware : IInitialize
    {
        [Inject]
        public readonly InjectEventCounter EventCounter;

        public void Initialize()
        {
            EventCounter.Initialize++;
        }
    }

    public sealed class EmptyClass
    {
    }

    public sealed class DependencyMissingClass
    {
        [Inject]
        private readonly object _object = null; 
    }

    public sealed class ConstructionArgumentNeeded
    {
        public ConstructionArgumentNeeded(int i)
        {
        }
    }

    public class DirectCircularDependency_A_To_B
    {
        private DirectCircularDependency_A_To_B() { }
        [Inject]
        internal readonly DirectCircularDependency_B_To_A _directCircularDependency_B_To_A = null;
    }

    public class DirectCircularDependency_B_To_A
    {
        private DirectCircularDependency_B_To_A() { }
        [Inject]
        internal readonly DirectCircularDependency_A_To_B _directCircularDependency_A_To_B = null;
    }

    public class IndirectCircularDependency_A_To_B
    {
        private IndirectCircularDependency_A_To_B() { }

        [Inject]
        internal readonly IndirectCircularDependency_B_To_C _indirectCircularDependency_B_To_C = null;
    }

    public class IndirectCircularDependency_B_To_C
    {
        private IndirectCircularDependency_B_To_C() { }

        [Inject]
        internal readonly IndirectCircularDependency_C_To_A _indirectCircularDependency_C_To_A = null;
    }

    public class IndirectCircularDependency_C_To_A
    {
        private IndirectCircularDependency_C_To_A() { }

        [Inject]
        internal readonly IndirectCircularDependency_A_To_B _indirectCircularDependency_A_To_B = null;
    }
}