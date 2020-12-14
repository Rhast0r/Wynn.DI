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
            var kernel = Container.Create();

            kernel.Install();
        }

        [Fact]
        public void EmptyKernelBeingValidatedDoesNotThrow()
        {
            var kernel = Container.Create();

            kernel.Validate();
            kernel.Install();
        }

        [Fact]
        public void ResolvingClassWithConstructionArgumentsThrows()
        {
            var kernel = Container.Create();
            kernel.Bind<ConstructionArgumentNeeded>().ToNew().AsCached().OnInstall();
            Assert.Throws<MissingMethodException>(() => kernel.Install());
        }

        [Fact]
        public void InstallTriggersOnInstallResolution()
        {
            ResolutionAware.WasTriggered = false;
            var kernel = Container.Create();
            kernel.Bind<ResolutionAware>().ToNew().AsCached().OnInstall();
            kernel.Install();

            Assert.True(ResolutionAware.WasTriggered);
        }

        [Fact]
        public void InstallDoesNotTriggerOtherResolutions()
        {
            ResolutionAware.WasTriggered = false;

            var kernel = Container.Create();
            kernel.Bind<ResolutionAware>().ToNew().AsCached().OnRequest();
            kernel.Install();

            Assert.False(ResolutionAware.WasTriggered);
        }

        [Fact]
        public void DependencylessClassResolves()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsCached().OnRequest();

            kernel.Install();

            Assert.NotNull(kernel.Get<EmptyClass>());
        }

        [Fact]
        public void DirectCircularDependencyThrows()
        {
            var kernel = Container.Create();
            kernel.Bind<DirectCircularDependency_A_To_B>().ToNew().AsCached().OnRequest();
            kernel.Bind<DirectCircularDependency_B_To_A>().ToNew().AsCached().OnRequest();

            var exception = Assert.Throws<InvalidOperationException>(() => kernel.Validate());
            Assert.True(exception.Message == "circular dependency");
        }

        [Fact]
        public void IndirectCircularDependencyThrows()
        {
            var kernel = Container.Create();
            kernel.Bind<IndirectCircularDependency_A_To_B>().ToNew().AsCached().OnRequest();
            kernel.Bind<IndirectCircularDependency_B_To_C>().ToNew().AsCached().OnRequest();
            kernel.Bind<IndirectCircularDependency_C_To_A>().ToNew().AsCached().OnRequest();

            var exception = Assert.Throws<InvalidOperationException>(() => kernel.Validate());
            Assert.True(exception.Message == "circular dependency");
        }

        [Fact]
        public void AsCachedToConstantReturnsSameObject()
        {
            var empty = new EmptyClass();

            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToConstant(empty).AsCached().OnInstall();

            kernel.Install();

            Assert.Equal(empty, kernel.Get<EmptyClass>());
        }

        [Fact]
        public void AsCachedToNewReturnsSameObject()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsCached().OnInstall();

            kernel.Install();

            var sameA = kernel.Get<EmptyClass>();

            Assert.Equal(kernel.Get<EmptyClass>(), kernel.Get<EmptyClass>());
        }

        [Fact]
        public void AsTransientReturnsDifferentObjectsFromResolve()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();

            kernel.Install();
            Assert.NotEqual(kernel.Get<EmptyClass>(), kernel.Get<EmptyClass>());
        }

        [Fact]
        public void AsTransientReturnsDifferentObjectsFromCreate()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();

            kernel.Install();

            var factory = kernel.Get<IFactory<EmptyClass>>();
            Assert.NotEqual(factory.Create(), factory.Create());
        }

        [Fact]
        public void AsCachedReturnsSameObjects()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsCached().OnRequest();

            kernel.Install();
            Assert.Equal(kernel.Get<EmptyClass>(), kernel.Get<EmptyClass>());
        }

        [Fact]
        public void InitializeToConstantIsCalledOnInstall()
        {
            var eventCounter = new InjectEventCounter();
            var initializationAware = new InitializationAware();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            kernel.Bind<InitializationAware>().ToConstant(initializationAware).AsCached().OnInstall();

            Assert.Equal(0, eventCounter.Initialize);

            kernel.Install();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToConstantIsCalledOnRequest()
        {
            var eventCounter = new InjectEventCounter();
            var initializationAware = new InitializationAware();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            kernel.Bind<InitializationAware>().ToConstant(initializationAware).AsCached().OnRequest();

            Assert.Equal(0, eventCounter.Initialize);

            kernel.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware2 = kernel.Get<InitializationAware>();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewIsCalledOnInstall()
        {
            var eventCounter = new InjectEventCounter();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            kernel.Bind<InitializationAware>().ToNew().AsCached().OnInstall();

            Assert.Equal(0, eventCounter.Initialize);

            kernel.Install();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewIsCalledOnRequest()
        {
            var eventCounter = new InjectEventCounter();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            kernel.Bind<InitializationAware>().ToNew().AsCached().OnRequest();

            Assert.Equal(0, eventCounter.Initialize);

            kernel.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware = kernel.Get<InitializationAware>();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewAsTransientIsCalledOnExplicitCreate()
        {
            var eventCounter = new InjectEventCounter();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            kernel.Bind<InitializationAware>().ToNew().AsTransient().OnRequest();

            kernel.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var factory = kernel.Get<IFactory<InitializationAware>>();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware = factory.Create();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeToNewAsTransientIsCalledOnImplicitCreate()
        {
            var eventCounter = new InjectEventCounter();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();
            kernel.Bind<InitializationAware>().ToNew().AsTransient().OnRequest();

            kernel.Install();

            Assert.Equal(0, eventCounter.Initialize);

            var initializationAware = kernel.Get<InitializationAware>();

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void InitializeIsCalledOnInject()
        {
            var eventCounter = new InjectEventCounter();

            var kernel = Container.Create();
            kernel.Bind<InjectEventCounter>().ToConstant(eventCounter).AsCached().OnInstall();

            kernel.Install();

            var initializationAware = new InitializationAware();

            Assert.Equal(0, eventCounter.Initialize);

            kernel.Inject(initializationAware);

            Assert.Equal(1, eventCounter.Initialize);
        }

        [Fact]
        public void GetAsCachedResolves()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsCached().OnInstall();
            kernel.Install();

            var empty = kernel.Get<EmptyClass>();
        }

        [Fact]
        public void GetAsTransientResolves()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();
            kernel.Install();

            var empty = kernel.Get<EmptyClass>();
        }

        [Fact]
        public void GetAsTransientResolveFactoryType()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsTransient().OnRequest();
            kernel.Install();

            var emptyFactory = kernel.Get<IFactory<EmptyClass>>();
        }

        [Fact]
        public void InjectFillsField()
        {
            var kernel = Container.Create();
            kernel.Bind<EmptyClass>().ToNew().AsCached().OnRequest();
            kernel.Install();

            var unknownClass = new UnknownClass();
            kernel.Inject(unknownClass);

            Assert.True(unknownClass.Empty != null);
        }

        [Fact]
        public void InjectThrowsOnUnknownDependency()
        {
            var kernel = Container.Create();
            kernel.Install();

            var unknownClass = new UnknownClass();

            var exception = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => kernel.Inject(unknownClass));
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