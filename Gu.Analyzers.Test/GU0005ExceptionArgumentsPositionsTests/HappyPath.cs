namespace Gu.Analyzers.Test.GU0005ExceptionArgumentsPositionsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly GU0005ExceptionArgumentsPositions Analyzer = new GU0005ExceptionArgumentsPositions();

        [Test]
        public void ArgumentExceptionWithMessageAndNameof()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(object o)
        {
            throw new ArgumentException(""message"", nameof(o));
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ArgumentNullExceptionWithMessageAndNameof()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(object o)
        {
            throw new ArgumentNullException(nameof(o), ""message"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ArgumentOutOfRangeExceptionWithMessageAndNameof()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(object o)
        {
            throw new ArgumentOutOfRangeException(nameof(o), ""message"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}