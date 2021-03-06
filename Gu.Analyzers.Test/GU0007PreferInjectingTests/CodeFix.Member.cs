﻿namespace Gu.Analyzers.Test.GU0007PreferInjectingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class Member
        {
            private static readonly string BarCode = @"
namespace RoslynSandbox
{
    public class Bar
    {
        public void Baz()
        {
        }

        public void Baz(int i)
        {
        }
    }
}";

            private static readonly string FooBaseCode = @"
namespace RoslynSandbox
{
    public abstract class FooBase
    {
        private readonly Bar bar;

        protected FooBase(Bar bar)
        {
            this.bar = bar;
        }
    }
}";

            private static readonly string LocatorCode = @"
namespace RoslynSandbox
{
    public class ServiceLocator
    {
        public ServiceLocator(Bar bar)
        {
            this.Bar = bar;
            this.BarObject = bar;
        }

        public Bar Bar { get; }

        public object BarObject { get; }
    }
}";

            [Test]
            public void WhenNotInjectingFieldInitialization()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator)
        {
            this.bar = locator.↓Bar;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator, Bar bar)
        {
            this.bar = bar;
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenNotInjectingFieldInitializationUnderscore()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar _bar;

        public Foo(ServiceLocator locator)
        {
            _bar = locator.↓Bar;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar _bar;

        public Foo(ServiceLocator locator, Bar bar)
        {
            _bar = bar;
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenNotInjectingFieldInitializationObject()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly object bar;

        public Foo(ServiceLocator locator)
        {
            this.bar = locator.↓BarObject;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly object bar;

        public Foo(ServiceLocator locator, Bar bar)
        {
            this.bar = bar;
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenNotInjectingFieldInitializationWithNameCollision()
            {
                var enumCode = @"
namespace RoslynSandbox
{
    public enum Meh
    {
        Bar
    }
}";
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator, Meh meh)
        {
            this.bar = locator.↓Bar;
            if (meh == Meh.Bar)
            {
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator, Meh meh, Bar bar)
        {
            this.bar = bar;
            if (meh == Meh.Bar)
            {
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, enumCode, fooCode }, fixedCode);
            }

            [Test]
            public void FieldInitializationAndBaseCall()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator)
            : base(locator.↓Bar)
        {
            this.bar = locator.↓Bar;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator, Bar bar)
            : base(bar)
        {
            this.bar = bar;
        }
    }
}";
                AnalyzerAssert.FixAll<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, FooBaseCode, fooCode }, fixedCode);
            }

            [Test]
            public void FieldInitializationAndBaseCallUnderscoreNames()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar _bar;

        public Foo(ServiceLocator locator)
            : base(locator.↓Bar)
        {
            _bar = locator.↓Bar;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar _bar;

        public Foo(ServiceLocator locator, Bar bar)
            : base(bar)
        {
            _bar = bar;
        }
    }
}";
                AnalyzerAssert.FixAll<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, FooBaseCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingMethodInjectedLocator()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        public Foo()
        {
        }

        public void Meh(ServiceLocator locator)
        {
            locator.↓Bar.Baz();
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;

        public Foo(Bar bar)
        {
            this.bar = bar;
        }

        public void Meh(ServiceLocator locator)
        {
            this.bar.Baz();
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingLocatorInMethod()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly ServiceLocator locator;

        public Foo(ServiceLocator locator)
        {
            this.locator = locator;
        }

        public void Meh()
        {
            this.locator.↓Bar.Baz();
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;
        private readonly ServiceLocator locator;

        public Foo(ServiceLocator locator, Bar bar)
        {
            this.bar = bar;
            this.locator = locator;
        }

        public void Meh()
        {
            this.bar.Baz();
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingLocatorInLamdaClosure()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.Linq;

    public class Foo
    {
        private readonly ServiceLocator locator;

        public Foo(IEnumerable<ServiceLocator> bars, ServiceLocator locator)
        {
            this.locator = bars.First(x => x.Bar == locator.↓Bar);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System.Collections.Generic;
    using System.Linq;

    public class Foo
    {
        private readonly ServiceLocator locator;

        public Foo(IEnumerable<ServiceLocator> bars, ServiceLocator locator, Bar bar)
        {
            this.locator = bars.First(x => x.Bar == bar);
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingLocatorInTwoMethods()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly ServiceLocator locator;

        public Foo(ServiceLocator locator)
        {
            this.locator = locator;
        }

        public void Meh1()
        {
            this.locator.↓Bar.Baz();
        }

        public void Meh2()
        {
            this.locator.↓Bar.Baz(2);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar bar;
        private readonly ServiceLocator locator;

        public Foo(ServiceLocator locator, Bar bar)
        {
            this.bar = bar;
            this.locator = locator;
        }

        public void Meh1()
        {
            this.bar.Baz();
        }

        public void Meh2()
        {
            this.bar.Baz(2);
        }
    }
}";
                AnalyzerAssert.FixAll<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingLocatorInMethodUnderscoreNames()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly ServiceLocator _locator;

        public Foo(ServiceLocator locator)
        {
            _locator = locator;
        }

        public void Meh()
        {
            _locator.↓Bar.Baz();
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly Bar _bar;
        private readonly ServiceLocator _locator;

        public Foo(ServiceLocator locator, Bar bar)
        {
            _bar = bar;
            _locator = locator;
        }

        public void Meh()
        {
            _bar.Baz();
        }
    }
}";
                AnalyzerAssert.CodeFix<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingLocatorInMethodAndBaseCall()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly ServiceLocator locator;

        public Foo(ServiceLocator locator)
            : base(locator.↓Bar)
        {
            this.locator = locator;
        }

        public void Meh()
        {
            this.locator.↓Bar.Baz();
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar bar;
        private readonly ServiceLocator locator;

        public Foo(ServiceLocator locator, Bar bar)
            : base(bar)
        {
            this.bar = bar;
            this.locator = locator;
        }

        public void Meh()
        {
            this.bar.Baz();
        }
    }
}";
                AnalyzerAssert.FixAll<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, FooBaseCode, fooCode }, fixedCode);
            }

            [Test]
            public void WhenUsingLocatorInStaticMethod()
            {
                var fooCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator)
            : base(locator.↓Bar)
        {
            this.bar = locator.↓Bar;
        }

        public static void Meh(ServiceLocator locator)
        {
            locator.Bar.Baz();
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : FooBase
    {
        private readonly Bar bar;

        public Foo(ServiceLocator locator, Bar bar)
            : base(bar)
        {
            this.bar = bar;
        }

        public static void Meh(ServiceLocator locator)
        {
            locator.Bar.Baz();
        }
    }
}";
                AnalyzerAssert.FixAll<GU0007PreferInjecting, InjectCodeFixProvider>(new[] { BarCode, LocatorCode, FooBaseCode, fooCode }, fixedCode);
            }
        }
    }
}